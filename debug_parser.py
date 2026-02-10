
import re
import os

REGEX_RECORD_PRIMARY = re.compile(r"public\s+record\s+(\w+)\s*\((.*?)\)", re.DOTALL)

def parse_file(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    record_match = REGEX_RECORD_PRIMARY.search(content)
    if record_match:
        # Args string can be multi-line
        args_str = record_match.group(2)
        print(f"RAW ARGS_STR: {args_str}")
        
        args_str_flat = " ".join(args_str.split()) # Flatten
        print(f"FLAT ARGS_STR: {args_str_flat}")
        
        # Robust split by comma respecting < >
        args = []
        current_arg = ""
        depth = 0
        for char in args_str_flat:
            if char == '<': depth += 1
            if char == '>': depth -= 1
            if char == ',' and depth == 0:
                args.append(current_arg.strip())
                current_arg = ""
            else:
                current_arg += char
        if current_arg:
            args.append(current_arg.strip())
            
        print(f"SPLIT ARGS: {args}")

        props = {}
        for arg in args:
            # Handle default value "="
            # e.g. "string? Bio = null" -> "string? Bio"
            parts_split = arg.split('=')
            decl = parts_split[0].strip()
            
            print(f"ARG: '{arg}' -> DECL: '{decl}'")
            
            parts = decl.split()
            if len(parts) >= 2:
                # Last part is name, second to last is type
                p_name = parts[-1]
                p_type = parts[-2]
                print(f"  FOUND: Name='{p_name}', Type='{p_type}'")
                props[p_name] = p_type
            else:
                 print(f"  IGNORED (len<2): {parts}")
        
        return props

file_path = r"d:\Worx\_MyProjz\ms_platform\Auth\Auth.Application\Features\Auth\Commands\RegisterUser\RegisterUserCommand.cs"
print(parse_file(file_path))
