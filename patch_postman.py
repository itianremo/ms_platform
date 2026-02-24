import json
import os

collection_path = r"d:\Worx\_MyProjz\ms_platform\docs\postman\Gateway_Collection.postman_collection.json"

with open(collection_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

def ensure_header(request, key, value):
    headers = request.get('header', [])
    for h in headers:
        if h.get('key', '').lower() == key.lower():
            h['value'] = value
            return
    headers.append({"key": key, "value": value, "type": "text"})
    request['header'] = headers

def remove_query_param(request, param_name):
    url = request.get('url', {})
    if not isinstance(url, dict): return
    query = url.get('query', [])
    url['query'] = [q for q in query if q.get('key') != param_name]
    
    raw = url.get('raw', '')
    if '?' in raw:
        base, qstr = raw.split('?', 1)
        qparts = qstr.split('&')
        qparts = [p for p in qparts if not p.startswith(param_name + '=')]
        if qparts:
            url['raw'] = f"{base}?{'&'.join(qparts)}"
        else:
            url['raw'] = base

def remove_path_variable(request, var_name):
    url = request.get('url', {})
    if not isinstance(url, dict): return
    
    # Remove from 'variable' array
    variables = url.get('variable', [])
    url['variable'] = [v for v in variables if v.get('key') != var_name]
    
    # Remove from 'path' array
    path = url.get('path', [])
    new_path = [p for p in path if p != f":{var_name}"]
    url['path'] = new_path
    
    # Remove from 'raw'
    raw = url.get('raw', '')
    raw = raw.replace(f"/:{var_name}", "")
    url['raw'] = raw

def process_item(item):
    if 'item' in item:
        for sub_item in item['item']:
            process_item(sub_item)
    elif 'request' in item:
        req = item['request']
        url = req.get('url', {})
        if isinstance(url, dict):
            raw_url = url.get('raw', '')
            
            # Auth endpoints
            if '/auth/api/' in raw_url:
                header_added = False
                
                # Check for query appId
                query = url.get('query', [])
                if any(q.get('key') == 'appId' for q in query):
                    remove_query_param(req, 'appId')
                    ensure_header(req, 'App-Id', '{{appId}}')
                    header_added = True
                
                # Check for path :appId
                variables = url.get('variable', [])
                if any(v.get('key') == 'appId' for v in variables) or '/:appId' in raw_url:
                    remove_path_variable(req, 'appId')
                    ensure_header(req, 'App-Id', '{{appId}}')
                    header_added = True
                    
                # Other endpoints that use AppId in header but maybe didn't strictly have query
                # Add to all Auth requests if User said "whenever we need the appid in the request to be passed.. use the app-id from the header"
                # For safety, only add to the specific ones modified or if body had it.
                body = req.get('body', {}).get('raw', '')
                if body and '"appId"' in body:
                    # They still might use body. We didn't change body properties except AddUserToAppRequest 
                    pass
        
        # We also need to add App-Id headers for AddUserToAppRequest which had it in body.
        # But maybe the python logic is simpler just to ensure it's in the headers for Auth?
        if 'request' in item and isinstance(item['request'].get('url'), dict):
             raw_url = item['request']['url'].get('raw', '')
             if '/auth/api/Auth/users/' in raw_url and '/apps' in raw_url and item['request'].get('method') == 'POST':
                 ensure_header(item['request'], 'App-Id', '{{appId}}')

process_item(data)

# Pass 2: Iterate stringified bodies and strip any raw JSON 'AppId'/'appId' properties natively
def strip_appid_from_body(item):
    if 'item' in item:
        for sub_item in item['item']:
            strip_appid_from_body(sub_item)
            
    # Clean standard request body
    if 'request' in item:
        body = item['request'].get('body', {})
        clean_raw_body(body)

    # Clean example response -> originalRequest bodies
    if 'response' in item and isinstance(item['response'], list):
        for resp in item['response']:
            orig_req = resp.get('originalRequest', {})
            orig_body = orig_req.get('body', {})
            clean_raw_body(orig_body)

def clean_raw_body(body):
    if not isinstance(body, dict): return
    if body.get('mode') == 'raw':
        raw_str = body.get('raw', '')
        # Case insensitive check for efficiency before parsing
        if raw_str and ('"AppId"' in raw_str or '"appId"' in raw_str):
            try:
                payload = json.loads(raw_str)
                changed = False
                if 'AppId' in payload:
                    del payload['AppId']
                    changed = True
                if 'appId' in payload:
                    del payload['appId']
                    changed = True
                if changed:
                    body['raw'] = json.dumps(payload, indent=4)
            except json.JSONDecodeError:
                pass

strip_appid_from_body(data)

with open(collection_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)

print("Postman collection updated successfully.")
