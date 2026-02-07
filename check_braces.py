
import re

def check_braces(filename):
    with open(filename, 'r') as f:
        lines = f.readlines()

    stack = []
    
    # Regex to capture braces but ignore simple strings (not perfect for JSX but helps)
    # Actually, for JSX, braces inside tags are balanced.
    # Let's just do a naive count and print indentation level.
    
    balance = 0
    for i, line in enumerate(lines):
        # Remove comments //
        # clean_line = re.sub(r'//.*', '', line) 
        # But URLs have // so be careful.
        # Just simple char counting
        
        for char in line:
            if char == '{':
                balance += 1
                stack.append(i + 1)
            elif char == '}':
                balance -= 1
                if stack:
                    stack.pop()
        
        # Print status every 50 lines or if balance is negative
        if i == 279:
             print(f"Balance at line 280: {balance}")
        if i == 573:
             print(f"Balance at line 574: {balance}")
        
        if balance < 0:
            print(f"Negative balance at line {i+1}: {line.strip()}")
            return
            
    print(f"Final Balance: {balance}")
    if len(stack) > 0:
        print(f"Unclosed braces starting at lines: {stack[-3:]} (showing last 3)")

check_braces(r"d:\Worx\_MyProjz\ms_platform\Frontend\GlobalAdmin\src\pages\UserDetailsPage.tsx")
