import json
import os

collection_path = r"d:\Worx\_MyProjz\ms_platform\docs\postman\Gateway_Collection.postman_collection.json"

with open(collection_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

def process_item(item):
    if 'item' in item:
        for sub_item in item['item']:
            process_item(sub_item)
    elif 'request' in item:
        url = item['request'].get('url', {})
        if isinstance(url, dict):
            raw_url = url.get('raw', '')
            
            # Update responses for Apps endpoints
            if '/apps/api/Apps' in raw_url:
                if 'response' in item and isinstance(item['response'], list):
                    for resp in item['response']:
                        if 'body' in resp:
                            try:
                                body_json = json.loads(resp['body'])
                                if isinstance(body_json, list) and len(body_json) > 0 and 'Name' in body_json[0] and 'BaseUrl' in body_json[0]:
                                    for app in body_json:
                                        app['DefaultCountry'] = 'US'
                                    resp['body'] = json.dumps(body_json, indent=4)
                                elif isinstance(body_json, dict) and 'Name' in body_json and 'BaseUrl' in body_json:
                                    body_json['DefaultCountry'] = 'US'
                                    resp['body'] = json.dumps(body_json, indent=4)
                            except:
                                pass

            # Update Get Packages response
            if '/packages' in raw_url:
                if 'response' in item and isinstance(item['response'], list):
                    for resp in item['response']:
                        if 'body' in resp:
                            # Replace the old array response or "Success" object with the new structured response
                            new_body = {
                                "subscriptions": [
                                    {
                                        "id": "2eed470d-7934-4edc-9bf5-0a1093b88fd2",
                                        "name": "Weekly",
                                        "description": "Premium Access",
                                        "price": 5.0,
                                        "period": 1,
                                        "currency": "USD",
                                        "packageType": 0,
                                        "coinsAmount": 0
                                    }
                                ],
                                "coins": [
                                    {
                                        "id": "3eed470d-7934-4edc-9bf5-0a1093b88fd3",
                                        "name": "50 Coins",
                                        "description": "50 Virtual Coins",
                                        "price": 5.0,
                                        "period": 0,
                                        "currency": "USD",
                                        "packageType": 1,
                                        "coinsAmount": 50
                                    }
                                ]
                            }
                            resp['body'] = json.dumps(new_body, indent=4)

process_item(data)

with open(collection_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)

print("Postman collection updated successfully.")
