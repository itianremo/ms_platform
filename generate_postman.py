import os
import re
import json
import uuid
import datetime

# Configuration
ROOT_DIR = r"d:\Worx\_MyProjz\ms_platform"
OUTPUT_FILE = r"d:\Worx\_MyProjz\ms_platform\docs\postman\FitIT_All_Endpoints.postman_collection.json"

# Gateway Mappings (Service Project Name -> Gateway Route Prefix)
SERVICE_PREFIXES = {
    "Auth.API": "auth",
    "Users.API": "users",
    "Apps.API": "apps",
    "Notifications.API": "notifications",
    "Media.API": "media",
    "Chat.API": "chat",
    "Payments.API": "payments",
    "Audit.API": "audit",
    "Search.API": "search",
    "Scheduler.API": "scheduler",
    "Geo.API": "geo",
    "Recommendation.API": "recommendation",
    "Gateway.API": ""
}

# Regex Models
REGEX_NAMESPACE = re.compile(r"namespace\s+([\w\.]+)")
REGEX_CLASS = re.compile(r"public\s+class\s+(\w+)\s*:\s*ControllerBase")
REGEX_CONTROLLER_ROUTE = re.compile(r'\[Route\("(.+?)"\)\]')
REGEX_METHOD = re.compile(r'\[Http(Get|Post|Put|Delete|Patch|Head|Options)\(?(".*?")?\)\]')
REGEX_PUBLIC_METHOD_WITH_ARGS = re.compile(r'public\s+(async\s+)?Task<.*?>\s+(\w+)\s*\((.*?)\)')
REGEX_FROM_BODY = re.compile(r'\[FromBody\]\s+([\w\<\>\.]+)\s+(\w+)')

# Record Parsing
REGEX_RECORD_PRIMARY = re.compile(r"public\s+record\s+(\w+)\s*\((.*?)\)", re.DOTALL)
REGEX_CLASS_PROP = re.compile(r"public\s+([\w\?<>]+)\s+(\w+)\s*\{\s*get;\s*set;\s*\}")

TYPE_FILE_CACHE = {}

def generate_uuid():
    return str(uuid.uuid4())

def find_file_for_type(type_name):
    # Strip generics for file search e.g. List<T> -> List
    clean_name = type_name.split("<")[0]
    
    # Handle fully qualified names e.g. Apps.Commands.CreateApp -> CreateApp
    if "." in clean_name:
        clean_name = clean_name.split(".")[-1]
    
    if clean_name in TYPE_FILE_CACHE:
        return TYPE_FILE_CACHE[clean_name]

        if f"{clean_name}.cs" in files:
            full_path = os.path.join(root, f"{clean_name}.cs")
            TYPE_FILE_CACHE[clean_name] = full_path
            return full_path
    
    TYPE_FILE_CACHE[clean_name] = None
    return None

def parse_type_properties(type_name):
    file_path = find_file_for_type(type_name)
    if not file_path:
        return {}
        
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception:
        return {}

    props = {}

    # Check for Record Primary Constructor
    record_match = REGEX_RECORD_PRIMARY.search(content)
    if record_match:
        # Args string can be multi-line
        args_str = record_match.group(2)
        args_str = " ".join(args_str.split()) # Flatten
        
        # Robust split by comma respecting < >
        args = []
        current_arg = ""
        depth = 0
        for char in args_str:
            if char == '<': depth += 1
            if char == '>': depth -= 1
            if char == ',' and depth == 0:
                args.append(current_arg.strip())
                current_arg = ""
            else:
                current_arg += char
        if current_arg:
            args.append(current_arg.strip())
            
        for arg in args:
            # Handle default value "="
            # e.g. "string? Bio = null" -> "string? Bio"
            decl = arg.split('=')[0].strip()
            
            parts = decl.split()
            if len(parts) >= 2:
                # Last part is name, second to last is type
                p_name = parts[-1]
                p_type = parts[-2]
                props[p_name] = p_type
        
        return props

    # Check for Class Properties
    matches = REGEX_CLASS_PROP.findall(content)
    for p_type, p_name in matches:
        props[p_name] = p_type

    return props

def generate_sample_value(prop_type, prop_name):
    prop_type = prop_type.lower().replace("?", "")
    prop_name_lower = prop_name.lower()

    if "guid" in prop_type:
        if "app" in prop_name_lower:
            return "33333333-3333-3333-3333-333333333330" # Demo App
        if "user" in prop_name_lower:
            return "00000000-0000-0000-0000-000000000000"
        return str(uuid.uuid4())
    
    if "int" in prop_type or "long" in prop_type:
        return 0
    
    if "bool" in prop_type:
        return True
    
    if "datetime" in prop_type:
        return datetime.datetime.now().isoformat()
    
    if "string" in prop_type:
        if "email" in prop_name_lower:
            return "test@example.com"
        if "password" in prop_name_lower:
            return "Password123!"
        if "phone" in prop_name_lower:
            return "+1234567890"
        if "url" in prop_name_lower:
            return "https://example.com"
        if "json" in prop_name_lower:
            return "{}"
        return f"sample {prop_name}"
    
    if "decimal" in prop_type or "double" in prop_type:
        return 10.99

    if "dictionary" in prop_type:
        return {}

    if "list" in prop_type or "IEnumerable" in prop_type or "[]" in prop_type:
        return []
        
    return None

def generate_body_for_type(type_name):
    # Basic recursion protection
    props = parse_type_properties(type_name)
    if not props:
        return {"error": f"Could not parse type {type_name}"}
    
    json_obj = {}
    for p_name, p_type in props.items():
        val = generate_sample_value(p_type, p_name)
        if val is None:
            # Maybe nested type?
            # recursive call (careful about depth)
            # For now, just return type string
            val = f"Type: {p_type}" 
        json_obj[p_name] = val
        
    return json_obj

def parse_controller(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Find Controller Class
    class_match = REGEX_CLASS.search(content)
    if not class_match:
        return None

    controller_name = class_match.group(1).replace("Controller", "")
    
    # Find Base Route
    route_match = REGEX_CONTROLLER_ROUTE.search(content)
    base_route = "api/[controller]"
    if route_match:
        base_route = route_match.group(1)
    
    base_route = base_route.replace("[controller]", controller_name)

    endpoints = []
    
    lines = content.split('\n')
    current_verbs = []
    
    for i, line in enumerate(lines):
        line = line.strip()
        
        # Check for Http Verb
        verb_match = REGEX_METHOD.search(line)
        if verb_match:
            verb = verb_match.group(1).upper()
            route_suffix = verb_match.group(2)
            if route_suffix:
                route_suffix = route_suffix.replace('"', '')
            else:
                route_suffix = ""
            
            # Identify if it's the same method (stacked attributes)
            current_verbs.append((verb, route_suffix))
            continue

        # Check for Method Definition
        method_match = REGEX_PUBLIC_METHOD_WITH_ARGS.search(line)
        if method_match and current_verbs:
            method_name = method_match.group(2)
            args_str = method_match.group(3)
            
            # Parse Args to find [FromBody]
            body_type = None
            if "[FromBody]" in args_str:
                # Super naive regex for [FromBody] Type Name
                # args_str looks like: "[FromBody] RegisterUserCommand command"
                # or "Guid id, [FromBody] UpdateProfileCommand command"
                
                # Split by comma
                args_list = args_str.split(',')
                for arg in args_list:
                    arg = arg.strip()
                    if "[FromBody]" in arg:
                        parts = arg.split()
                        # parts should be ['[FromBody]', 'Type', 'Name']
                        # find index of [FromBody]
                        try:
                            idx = parts.index("[FromBody]")
                            if idx + 1 < len(parts):
                                body_type = parts[idx+1]
                        except:
                            pass
            
            for verb, suffix in current_verbs:
                full_route = f"{base_route}/{suffix}".replace("//", "/").strip("/")
                endpoints.append({
                    "name": method_name,
                    "method": verb,
                    "path": full_route,
                    "body_type": body_type
                })
            
            current_verbs = [] 
    
    return {
        "controller": controller_name,
        "endpoints": endpoints
    }

def create_postman_item(name, method, url_path, prefix, event=None, body=None):
    # Construct Full URL
    # format: {{gateway_url}}/prefix/path
    
    path_segments = url_path.split("/")
    path_segments = [p for p in path_segments if p]
    
    if prefix:
        path_segments.insert(0, prefix)

    full_url = "{{gateway_url}}/" + "/".join(path_segments)
    
    item = {
        "name": f"{name} ({url_path})",
        "request": {
            "method": method,
            "header": [],
            "url": {
                "raw": full_url,
                "host": ["{{gateway_url}}"],
                "path": path_segments
            }
        },
        "response": []
    }

    if body:
        item["request"]["body"] = {
            "mode": "raw",
            "raw": json.dumps(body, indent=4),
            "options": {
                "raw": {
                    "language": "json"
                }
            }
        }
        item["request"]["header"].append({
            "key": "Content-Type",
            "value": "application/json",
            "type": "text"
        })

    if event:
        item["event"] = event

    return item

def main():
    info = {
        "_postman_id": generate_uuid(),
        "name": "FitIT Full Platform (Generated)",
        "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
    }
    
    folders = []

    # Walk directories
    for root, dirs, files in os.walk(ROOT_DIR):
        if "Controllers" not in root:
            continue
            
        # Determine Service Name
        parts = root.split(os.sep)
        service_name = None
        for p in parts:
            if p.endswith(".API"):
                service_name = p
                break
        
        if not service_name or service_name not in SERVICE_PREFIXES:
            continue

        gateway_prefix = SERVICE_PREFIXES[service_name]
        
        service_folder = next((f for f in folders if f["name"] == service_name), None)
        if not service_folder:
            service_folder = {
                "name": service_name,
                "item": []
            }
            folders.append(service_folder)
            
        for file in files:
            if file.endswith("Controller.cs"):
                file_path = os.path.join(root, file)
                result = parse_controller(file_path)
                
                if result and result["endpoints"]:
                    # Create Controller Folder
                    controller_folder = {
                        "name": result["controller"],
                        "item": []
                    }
                    
                    for ep in result["endpoints"]:
                        event = None
                        body = None

                        # Dynamic Body Generation
                        if ep["body_type"]:
                            body = generate_body_for_type(ep["body_type"])

                        # Inject Body for Login
                        if "Login" in ep["name"] and "Auth" in result["controller"]:
                            body = {
                                "email": "test@example.com",
                                "password": "Password123!",
                                "appId": "33333333-3333-3333-3333-333333333330"
                            }
                        # Inject Script for Login (Override)
                        if "Login" in ep["name"] and "Auth" in result["controller"]:
                             event = [{
                                "listen": "test",
                                "script": {
                                    "exec": [
                                        "var jsonData = pm.response.json();",
                                        "if (jsonData.accessToken) {",
                                        "    pm.environment.set('jwt_token', jsonData.accessToken);",
                                        "    console.log('Access Token Set');",
                                        "}",
                                        "if (jsonData.refreshToken) {",
                                        "    pm.environment.set('refresh_token', jsonData.refreshToken);",
                                        "}"
                                    ],
                                    "type": "text/javascript"
                                }
                            }]
                        
                        item = create_postman_item(ep["name"], ep["method"], ep["path"], gateway_prefix, event=event, body=body)
                        controller_folder["item"].append(item)
                    
                    service_folder["item"].append(controller_folder)

    # Compile Final JSON
    collection = {
        "info": info,
        "item": folders
    }
    
    with open(OUTPUT_FILE, 'w') as f:
        json.dump(collection, f, indent=4)
        
    print(f"Collection generated at: {OUTPUT_FILE}")

if __name__ == "__main__":
    main()
