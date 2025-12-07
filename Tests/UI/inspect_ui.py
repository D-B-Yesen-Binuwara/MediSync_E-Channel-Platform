#!/usr/bin/env python3
"""
UI Inspector Tool - Helps identify correct selectors for your frontend.
Run this to inspect actual HTML elements and their selectors.
"""

from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.options import Options
import time
import json


def inspect_page(url, element_descriptions):
    """
    Inspect a page and find elements matching descriptions.
    
    Args:
        url: Full URL to inspect (e.g., http://localhost:5173/login)
        element_descriptions: List of dicts with 'name' and 'xpath_pattern' keys
    """
    print(f"\n🔍 Inspecting: {url}")
    print("=" * 80)
    
    chrome_options = Options()
    # Uncomment to see the browser
    # chrome_options.add_argument("--headless")
    chrome_options.add_argument("--disable-blink-features=AutomationControlled")
    
    driver = webdriver.Chrome(options=chrome_options)
    driver.get(url)
    time.sleep(2)
    
    results = {}
    
    for desc in element_descriptions:
        name = desc.get('name', 'Unknown')
        xpath = desc.get('xpath')
        css = desc.get('css')
        tag = desc.get('tag')
        
        print(f"\n📍 Looking for: {name}")
        print("-" * 80)
        
        found = False
        element_info = {
            'name': name,
            'found': False,
            'elements': []
        }
        
        # Try XPath
        if xpath:
            try:
                elements = driver.find_elements(By.XPATH, xpath)
                if elements:
                    print(f"  ✅ Found {len(elements)} element(s) with XPath: {xpath}")
                    for i, el in enumerate(elements[:3]):  # Show first 3
                        info = {
                            'method': 'XPATH',
                            'selector': xpath,
                            'tag': el.tag_name,
                            'id': el.get_attribute('id'),
                            'class': el.get_attribute('class'),
                            'name': el.get_attribute('name'),
                            'type': el.get_attribute('type'),
                            'placeholder': el.get_attribute('placeholder'),
                            'text': el.text[:50] if el.text else '',
                        }
                        print(f"    Element {i+1}: {json.dumps(info, indent=6)}")
                        element_info['elements'].append(info)
                    found = True
                    element_info['found'] = True
            except Exception as e:
                print(f"  ❌ XPath not found: {xpath}")
        
        # Try CSS selector
        if css and not found:
            try:
                elements = driver.find_elements(By.CSS_SELECTOR, css)
                if elements:
                    print(f"  ✅ Found {len(elements)} element(s) with CSS: {css}")
                    for i, el in enumerate(elements[:3]):
                        info = {
                            'method': 'CSS_SELECTOR',
                            'selector': css,
                            'tag': el.tag_name,
                            'id': el.get_attribute('id'),
                            'class': el.get_attribute('class'),
                            'name': el.get_attribute('name'),
                            'type': el.get_attribute('type'),
                            'placeholder': el.get_attribute('placeholder'),
                            'text': el.text[:50] if el.text else '',
                        }
                        print(f"    Element {i+1}: {json.dumps(info, indent=6)}")
                        element_info['elements'].append(info)
                    found = True
                    element_info['found'] = True
            except Exception as e:
                print(f"  ❌ CSS selector not found: {css}")
        
        # Try by tag
        if tag and not found:
            try:
                elements = driver.find_elements(By.TAG_NAME, tag)
                if elements:
                    print(f"  ✅ Found {len(elements)} <{tag}> element(s)")
                    for i, el in enumerate(elements[:3]):
                        info = {
                            'method': 'TAG_NAME',
                            'tag': el.tag_name,
                            'id': el.get_attribute('id'),
                            'class': el.get_attribute('class'),
                            'name': el.get_attribute('name'),
                            'type': el.get_attribute('type'),
                            'placeholder': el.get_attribute('placeholder'),
                            'text': el.text[:50] if el.text else '',
                        }
                        print(f"    Element {i+1}: {json.dumps(info, indent=6)}")
                        element_info['elements'].append(info)
                    found = True
                    element_info['found'] = True
            except Exception as e:
                print(f"  ❌ Tag search failed: {tag}")
        
        if not found:
            print(f"  ❌ Element '{name}' not found")
        
        results[name] = element_info
    
    driver.quit()
    print("\n" + "=" * 80)
    return results


def main():
    """Run inspections on all major pages."""
    
    # Define what elements to look for on each page
    pages = [
        {
            'url': 'http://localhost:5173/login',
            'name': 'Login Page',
            'elements': [
                {'name': 'Email Input', 'xpath': "//input[@type='email']", 'css': "input[type='email']"},
                {'name': 'Password Input', 'xpath': "//input[@type='password']", 'css': "input[type='password']"},
                {'name': 'Login Button', 'xpath': "//button[contains(text(), 'Login')]"},
                {'name': 'Error Message', 'xpath': "//*[contains(text(), 'Invalid') or contains(text(), 'error')]"},
            ]
        },
        {
            'url': 'http://localhost:5173/signup',
            'name': 'Signup Page',
            'elements': [
                {'name': 'First Name Input', 'css': "input[placeholder='First Name'], input[name='firstName']"},
                {'name': 'Email Input', 'css': "input[type='email']"},
                {'name': 'Password Input', 'css': "input[type='password']"},
                {'name': 'Signup Button', 'xpath': "//button[contains(text(), 'Sign')] | //button[contains(text(), 'Register')]"},
            ]
        },
        {
            'url': 'http://localhost:5173/patient',
            'name': 'Patient Dashboard',
            'elements': [
                {'name': 'Search Input', 'xpath': "//input[@placeholder='Search doctors'] | //input[@type='search']"},
                {'name': 'Doctor Card', 'xpath': "//div[@class='doctor-card'] | //div[@class='doctor-item']"},
                {'name': 'Filter Button', 'xpath': "//button[contains(text(), 'Filter')] | //button[contains(text(), 'Specialization')]"},
                {'name': 'Book Button', 'xpath': "//button[contains(text(), 'Book')]"},
            ]
        },
        {
            'url': 'http://localhost:5173/patient/profile',
            'name': 'User Profile',
            'elements': [
                {'name': 'Edit Button', 'xpath': "//button[contains(text(), 'Edit')]"},
                {'name': 'First Name Field', 'css': "input[name='firstName']"},
                {'name': 'Email Field', 'css': "input[type='email']"},
                {'name': 'Save Button', 'xpath': "//button[contains(text(), 'Save')]"},
            ]
        },
    ]
    
    print("\n" + "=" * 80)
    print("🔎 MediSync UI Inspector Tool")
    print("=" * 80)
    print("\nThis tool helps identify correct selectors for your Selenium tests.")
    print("It will inspect each page and show actual element selectors.\n")
    
    for page in pages:
        try:
            results = inspect_page(page['url'], page['elements'])
            
            # Count found elements
            found_count = sum(1 for r in results.values() if r['found'])
            total_count = len(results)
            
            print(f"\n📊 {page['name']}: {found_count}/{total_count} elements found")
            
        except Exception as e:
            print(f"\n❌ Error inspecting {page['url']}: {str(e)}")
            print("   Make sure the frontend is running at http://localhost:5173")


if __name__ == '__main__':
    main()
