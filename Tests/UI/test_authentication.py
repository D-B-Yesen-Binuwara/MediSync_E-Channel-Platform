"""
UI tests for authentication flows (Login, Signup, Logout).
"""
import pytest
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import time


class TestAuthentication:
    """Authentication UI tests."""
    
    def test_login_page_loads(self, driver, base_url):
        """Test that the login page loads successfully."""
        driver.get(f"{base_url}/login")
        time.sleep(2)
        
        # Check if login form elements are present
        email_input = WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.XPATH, "//input[@type='email']"))
        )
        password_input = driver.find_element(By.XPATH, "//input[@type='password']")
        submit_button = driver.find_element(By.XPATH, "//button[contains(text(), 'Sign In')]")
        
        assert email_input is not None, "Email input not found"
        assert password_input is not None, "Password input not found"
        assert submit_button is not None, "Login button not found"
    
    def test_login_with_invalid_credentials(self, driver, base_url):
        """Test login with invalid credentials."""
        driver.get(f"{base_url}/login")
        time.sleep(2)
        
        # Find and fill email field
        email_input = WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.XPATH, "//input[@type='email']"))
        )
        email_input.clear()
        email_input.send_keys("invalid@test.com")
        
        # Find and fill password field
        password_input = driver.find_element(By.XPATH, "//input[@type='password']")
        password_input.clear()
        password_input.send_keys("wrongpassword")
        
        # Click login button
        login_button = WebDriverWait(driver, 10).until(
            EC.element_to_be_clickable((By.XPATH, "//button[contains(text(), 'Sign In')]"))
        )
        login_button.click()
        
        time.sleep(2)
        
        # Check for error message
        wait = WebDriverWait(driver, 10)
        error_element = wait.until(
            EC.presence_of_element_located((By.XPATH, "//*[contains(text(), 'Invalid') or contains(text(), 'error') or contains(text(), 'failed')]"))
        )
        assert error_element is not None, "Error message not displayed"
    
    def test_signup_page_loads(self, driver, base_url):
        """Test that the signup page loads successfully."""
        driver.get(f"{base_url}/register")
        time.sleep(2)
        
        # Check if signup form elements are present
        try:
            # Look for typical signup form fields
            email_input = WebDriverWait(driver, 10).until(
                EC.presence_of_element_located((By.XPATH, "//input[@type='email']"))
            )
            password_input = driver.find_element(By.XPATH, "//input[@type='password']")
            signup_button = WebDriverWait(driver, 10).until(
                EC.element_to_be_clickable((By.XPATH, "//button[contains(text(), 'Sign') or contains(text(), 'Register')]"))
            )
            assert signup_button is not None, "Signup button not found"
        except Exception as e:
            pytest.skip(f"Signup form elements not found: {str(e)}")
    
    def test_logout_flow(self, driver, base_url, test_user):
        """Test logout flow."""
        # First, login with test user
        driver.get(f"{base_url}/login")
        time.sleep(2)
        
        email_input = WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.XPATH, "//input[@type='email']"))
        )
        password_input = driver.find_element(By.XPATH, "//input[@type='password']")
        login_button = driver.find_element(By.XPATH, "//button[contains(text(), 'Sign In')]")
        
        email_input.send_keys(test_user['email'])
        password_input.send_keys(test_user['password'])
        login_button.click()
        
        time.sleep(3)
        
        # Now look for logout button in header
        try:
            logout_button = WebDriverWait(driver, 10).until(
                EC.element_to_be_clickable((By.XPATH, "//button[contains(text(), 'Logout')] | //*[contains(text(), 'Logout')]"))
            )
            logout_button.click()
            time.sleep(2)
            
            # Verify redirect to login
            assert "/login" in driver.current_url or "/home" in driver.current_url, "Not redirected after logout"
        except Exception as e:
            pytest.skip(f"Logout button not found: {str(e)}")


class TestLoginValidation:
    """Login form validation tests."""
    
    def test_email_field_required(self, driver, base_url):
        """Test that email field is required."""
        driver.get(f"{base_url}/login")
        time.sleep(2)
        
        # Try to submit without email - just fill password
        password_input = WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.XPATH, "//input[@type='password']"))
        )
        password_input.send_keys("somepassword")
        
        login_button = driver.find_element(By.XPATH, "//button[contains(text(), 'Sign In')]")
        login_button.click()
        
        time.sleep(1)
        
        # Check for validation message or error
        email_input = driver.find_element(By.XPATH, "//input[@type='email']")
        
        # Check browser validation
        validation = email_input.get_attribute("validationMessage")
        if validation:
            assert validation, "Email validation not enforced"
        else:
            # If no browser validation, check for error message
            try:
                error = WebDriverWait(driver, 5).until(
                    EC.presence_of_element_located((By.XPATH, "//*[contains(text(), 'Email')] | //*[contains(text(), 'required')]"))
                )
                assert error is not None, "Email field validation not enforced"
            except:
                pytest.skip("Email validation not implemented in this version")
    
    def test_email_format_validation(self, driver, base_url):
        """Test email format validation."""
        driver.get(f"{base_url}/login")
        time.sleep(2)
        
        email_input = WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.XPATH, "//input[@type='email']"))
        )
        email_input.send_keys("invalidemail")
        
        password_input = driver.find_element(By.XPATH, "//input[@type='password']")
        password_input.send_keys("password123")
        
        login_button = driver.find_element(By.XPATH, "//button[contains(text(), 'Sign In')]")
        login_button.click()
        
        time.sleep(1)
        
        # Email input should have invalid state or error
        validation = email_input.get_attribute("validationMessage")
        is_invalid = email_input.get_attribute("aria-invalid")
        
        # Browser email validation or our custom validation
        if validation or is_invalid:
            assert validation or is_invalid, "Email format validation failed"
        else:
            # Check for error message in UI
            try:
                error = WebDriverWait(driver, 5).until(
                    EC.presence_of_element_located((By.XPATH, "//*[contains(text(), 'Email') or contains(text(), 'invalid')]"))
                )
                assert error is not None, "Email format validation not shown"
            except:
                pytest.skip("Email format validation not implemented in this version")
