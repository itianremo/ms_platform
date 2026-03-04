import json
import re

# 1. Update Postman Collection
collection_path = r"d:\Worx\_MyProjz\ms_platform\docs\postman\Gateway_Collection.postman_collection.json"

with open(collection_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

def update_postman_auth(item):
    if 'item' in item:
        for sub_item in item['item']:
            update_postman_auth(sub_item)
    elif 'request' in item:
        req = item['request']
        url = req.get('url', {})
        if isinstance(url, dict):
            raw_url = url.get('raw', '')
            if '/auth/api/Analytics/app-user-stats' in raw_url:
                # Update Description
                desc = req.get('description', '')
                req['description'] = desc.replace(
                    "Publicly accessible endpoint (No authentication required).",
                    "Requires 'DashboardRead' policy."
                )
                
                # Add Auth Block if missing
                if 'auth' not in req:
                    req['auth'] = {
                        "type": "bearer",
                        "bearer": [
                            {
                                "key": "token",
                                "value": "{{jwt_token}}",
                                "type": "string"
                            }
                        ]
                    }

        # Also update response originalRequest
        if 'response' in item and isinstance(item['response'], list):
            for resp in item['response']:
                orig_req = resp.get('originalRequest', {})
                orig_url = orig_req.get('url', {})
                if isinstance(orig_url, dict):
                    orig_raw = orig_url.get('raw', '')
                    if '/auth/api/Analytics/app-user-stats' in orig_raw:
                        desc = orig_req.get('description', '')
                        orig_req['description'] = desc.replace(
                            "Publicly accessible endpoint (No authentication required).",
                            "Requires 'DashboardRead' policy."
                        )

update_postman_auth(data)

with open(collection_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)

print("Postman collection updated with Analytics auth.")

# 2. Update README.md
readme_path = r"d:\Worx\_MyProjz\ms_platform\Gateway\README.md"
with open(readme_path, 'r', encoding='utf-8') as f:
    readme_content = f.read()

# Use regex to find the specific section for app-user-stats and replace its authorization context
pattern = r"(\*\*Get App User Stats\*\*\s*This endpoint executes the `GetAppUserStats` operation\.\s*\*\*Authorization Context:\*\*\s*)Publicly accessible endpoint \(No authentication required\)\."
replacement = r"\1Requires 'DashboardRead' policy."
readme_content = re.sub(pattern, replacement, readme_content)

with open(readme_path, 'w', encoding='utf-8') as f:
    f.write(readme_content)

print("README.md updated with Analytics auth.")
