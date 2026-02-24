import os
import re

ROOT_DIR = r"d:\Worx\_MyProjz\ms_platform"

def process_file(file_path):
    with open(file_path, "r", encoding="utf-8") as f:
        content = f.read()

    # Regex to match:
    # if (app.Environment.IsDevelopment())
    # {
    #     app.UseSwagger();
    # }
    
    # We want to replace it with just app.UseSwagger();
    
    pattern = re.compile(r"if\s*\(\s*app\.Environment\.IsDevelopment\(\)\s*\)\s*\{\s*app\.UseSwagger\(\);\s*\}", re.MULTILINE | re.DOTALL)
    
    new_content = pattern.sub("app.UseSwagger();", content)
    
    if new_content != content:
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(new_content)
        print(f"Updated {file_path}")

for root, dirs, files in os.walk(ROOT_DIR):
    for name in files:
        if name == "Program.cs" and ".API" in root:
            process_file(os.path.join(root, name))
