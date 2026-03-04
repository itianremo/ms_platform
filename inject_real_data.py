import os
import re
import json

collection_path = r"d:\Worx\_MyProjz\ms_platform\docs\postman\Gateway_Collection.postman_collection.json"

def get_realistic_value(key, val_type):
    k_lower = key.lower()
    
    # Specific fields based on naming conventions
    if 'email' in k_lower:
        return "admin@ump.com"
    if 'password' in k_lower:
        return "SecureP@ssw0rd!"
    if 'phone' in k_lower:
        return "+15551234567"
    if 'firstname' in k_lower or 'displayname' in k_lower:
        return "John Doe"
    if 'lastname' in k_lower:
        return "Doe"
    if 'url' in k_lower:
        return "https://example.com/image.jpg"
    if 'description' in k_lower or 'bio' in k_lower:
        return "This is a realistic description generated for testing purposes."
    if 'color' in k_lower or 'theme' in k_lower:
        return "#FFFFFF"
    if 'ipaddress' in k_lower:
        return "192.168.1.1"
    if 'useragent' in k_lower:
        return "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
    if 'role' in k_lower:
        return "Admin"
    if 'currency' in k_lower:
        return "USD"
    if 'country' in k_lower:
        return "US"
    if 'status' in k_lower:
        return 1
    if 'latitude' in k_lower:
        return 34.0522
    if 'longitude' in k_lower:
        return -118.2437
    if 'json' in k_lower:
        return "{}"
    if 'date' in k_lower:
        return "2026-03-02T12:00:00Z"
    
    # Defaults by type
    if isinstance(val_type, bool):
        return True
    if isinstance(val_type, int):
        return 100
    if isinstance(val_type, float):
        return 99.99
    if isinstance(val_type, str):
        if re.match(r'^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$', val_type):
            return val_type # keep existing Guids like userIds or appIds
        return f"Sample payload for {key}"
        
    return val_type

def process_raw_body(body):
    if not isinstance(body, dict): return
    if body.get('mode') == 'raw':
        raw_str = body.get('raw', '')
        if raw_str:
            try:
                payload = json.loads(raw_str)
                if isinstance(payload, dict):
                    for k, v in payload.items():
                        # Only replace if it matches the generated boilerplate
                        if isinstance(v, str) and ("sample_string_for_" in v or "user@example.com" in v or "SecureP@ssw0rd!" in v):
                            payload[k] = get_realistic_value(k, v)
                body['raw'] = json.dumps(payload, indent=4)
            except json.JSONDecodeError:
                pass

def process_item(item):
    if 'item' in item:
        for sub_item in item['item']:
            process_item(sub_item)
    
    if 'request' in item:
        body = item['request'].get('body', {})
        process_raw_body(body)

    if 'response' in item and isinstance(item['response'], list):
        for resp in item['response']:
            orig_req = resp.get('originalRequest', {})
            orig_body = orig_req.get('body', {})
            process_raw_body(orig_body)

with open(collection_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

process_item(data)

with open(collection_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)

print("Realistic data injected into Postman collection.")

# 2. Update README.md
readme_path = r"d:\Worx\_MyProjz\ms_platform\Gateway\README.md"
with open(readme_path, 'r', encoding='utf-8') as f:
    readme_content = f.read()

# Replace all "sample_string_for_X" in the markdown
def markdown_replacer(match):
    full_string = match.group(0)
    key_name = match.group(1)
    new_val = get_realistic_value(key_name, "")
    # If new_val is a string, return quoted, else return raw
    if isinstance(new_val, str):
        return f'"{new_val}"'
    elif isinstance(new_val, bool):
        return str(new_val).lower()
    else:
        return str(new_val)

readme_content = re.sub(r'"sample_string_for_([a-zA-Z0-9_]+)"', markdown_replacer, readme_content)
readme_content = readme_content.replace('"user@example.com"', '"admin@ump.com"')
readme_content = readme_content.replace('"SecureP@ssw0rd!"', '"StrongPass123!"')

with open(readme_path, 'w', encoding='utf-8') as f:
    f.write(readme_content)

print("README.md updated with realistic constraints.")
