import re

readme_path = r"d:\Worx\_MyProjz\ms_platform\Gateway\README.md"

with open(readme_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
is_auth_endpoint = False
in_json_body = False

for line in lines:
    if line.startswith('#### '):
        is_auth_endpoint = '/auth/api/' in line
        new_lines.append(line)
        continue
    
    if line.strip() == '```json':
        in_json_body = True
        new_lines.append(line)
        continue
    
    if line.strip() == '```':
        in_json_body = False
        new_lines.append(line)
        continue
    
    if is_auth_endpoint and in_json_body:
        # If line contains "AppId":
        if '"AppId"' in line or '"appId"' in line:
            # We skip this line
            continue
            
    # Quick cleanup if the previous line ended with a comma and the next line is a closing brace
    # and we just skipped the actual last element.
    # Actually, a simpler way is just to fix up dangling commas before '}' over the whole string later
    new_lines.append(line)

content = "".join(new_lines)
# Fix dangling commas
content = re.sub(r',\s*\}', '\n  }', content)

with open(readme_path, 'w', encoding='utf-8') as f:
    f.write(content)

print("README payloads cleaned.")
