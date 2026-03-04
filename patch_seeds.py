import json
import re

postman_path = r"d:\Worx\_MyProjz\ms_platform\docs\postman\Gateway_Collection.postman_collection.json"
readme_path = r"d:\Worx\_MyProjz\ms_platform\Gateway\README.md"

wissler_app_id = "00000000-0000-0000-0000-000000000012"
vis1_id = "00000000-0000-0000-0000-000000000001"
vis2_id = "00000000-0000-0000-0000-000000000002"

# 1. Update Postman Collection
with open(postman_path, 'r', encoding='utf-8') as f:
    text = f.read()

# Replace any par_app_id3 variables or values
text = text.replace('{{par_app_id3}}', wissler_app_id)
text = text.replace('{{user_id}}', vis1_id)
text = text.replace('userId={{user_id}}', f'userId={vis1_id}')
text = text.replace('reporterId=demo_value', f'reporterId={vis1_id}')
text = text.replace('reportedId=demo_value', f'reportedId={vis2_id}')
text = text.replace('appId=demo_value', f'appId={wissler_app_id}')
text = text.replace('days=demo_value', 'days=7') 

with open(postman_path, 'w', encoding='utf-8') as f:
    f.write(text)

# 2. Update README.md
with open(readme_path, 'r', encoding='utf-8') as f:
    text = f.read()

text = text.replace('{{par_app_id3}}', wissler_app_id)
text = text.replace('{{user_id}}', vis1_id)
text = text.replace('userId={{user_id}}', f'userId={vis1_id}')
text = text.replace('reporterId=demo_value', f'reporterId={vis1_id}')
text = text.replace('reportedId=demo_value', f'reportedId={vis2_id}')
text = text.replace('appId=demo_value', f'appId={wissler_app_id}')
text = text.replace('days=demo_value', 'days=7') 

with open(readme_path, 'w', encoding='utf-8') as f:
    f.write(text)

print("Values successfully replaced in Postman and README.")
