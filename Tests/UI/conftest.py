"""
Pytest configuration and fixtures for Selenium UI tests.
"""
import pytest
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.chrome.service import Service
import time


@pytest.fixture(scope="function")
def driver():
    """
    Create and return a Chrome WebDriver instance.
    Automatically cleanup after test.
    """
    chrome_options = Options()
    # Uncomment the line below to run headless (without UI)
    # chrome_options.add_argument("--headless")
    chrome_options.add_argument("--disable-blink-features=AutomationControlled")
    chrome_options.add_argument("user-agent=Mozilla/5.0")
    
    driver = webdriver.Chrome(options=chrome_options)
    driver.implicitly_wait(10)
    
    yield driver
    
    driver.quit()


@pytest.fixture
def base_url():
    """Base URL for the frontend application."""
    return "http://localhost:5173"


@pytest.fixture
def wait(driver):
    """WebDriverWait instance for explicit waits."""
    return WebDriverWait(driver, 15)


@pytest.fixture
def test_user():
    """Test user credentials."""
    return {
        "email": "test@example.com",
        "password": "TestPassword123!",
        "firstName": "Test",
        "lastName": "User"
    }


def wait_for_element(driver, locator, timeout=10):
    """Helper function to wait for element to be visible."""
    wait = WebDriverWait(driver, timeout)
    return wait.until(EC.visibility_of_element_located(locator))


def wait_for_element_clickable(driver, locator, timeout=10):
    """Helper function to wait for element to be clickable."""
    wait = WebDriverWait(driver, timeout)
    return wait.until(EC.element_to_be_clickable(locator))


def login_user(driver, base_url, test_user):
    """
    Helper function to log in a user before accessing protected routes.
    
    Args:
        driver: Selenium WebDriver instance
        base_url: Base URL of the application
        test_user: Dictionary with 'email' and 'password' keys
    
    Returns:
        True if login successful, False otherwise
    """
    try:
        driver.get(f"{base_url}/login")
        time.sleep(2)
        
        # Find and fill email field
        email_input = WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.XPATH, "//input[@type='email']"))
        )
        password_input = driver.find_element(By.XPATH, "//input[@type='password']")
        
        email_input.clear()
        email_input.send_keys(test_user['email'])
        password_input.clear()
        password_input.send_keys(test_user['password'])
        
        # Click login button
        login_button = driver.find_element(By.XPATH, "//button[contains(text(), 'Sign In')]")
        login_button.click()
        
        # Wait for redirect (look for patient dashboard or account page)
        time.sleep(3)
        return True
    except Exception as e:
        print(f"Login failed: {str(e)}")
        return False
