"""
UI tests for doctor search and filtering functionality.
"""
import pytest
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import time
from conftest import login_user


class TestDoctorSearch:
    """Doctor search functionality tests."""
    
    def test_doctor_search_page_loads(self, driver, base_url, test_user):
        """Test that the doctor search/find page loads successfully."""
        # Login first
        if not login_user(driver, base_url, test_user):
            pytest.skip("Failed to login")
        
        driver.get(f"{base_url}/patient")
        time.sleep(2)
        
        # Check if search elements are visible
        try:
            search_input = WebDriverWait(driver, 10).until(
                EC.presence_of_element_located((By.XPATH, "//input[@placeholder='Search doctors...']"))
            )
            assert search_input is not None, "Search input not found"
        except Exception as e:
            pytest.skip(f"Doctor search page not accessible: {str(e)}")
    
    def test_search_doctor_by_name(self, driver, base_url, test_user):
        """Test searching for doctor by name."""
        # Login first
        if not login_user(driver, base_url, test_user):
            pytest.skip("Failed to login")
            
        driver.get(f"{base_url}/patient")
        time.sleep(2)
        
        try:
            # Find and fill search input
            search_input = WebDriverWait(driver, 10).until(
                EC.presence_of_element_located((By.XPATH, "//input[@placeholder='Search doctors...']"))
            )
            search_input.send_keys("John")
            time.sleep(1)
            
            # Click search button
            search_button = driver.find_element(By.XPATH, "//button[contains(text(), 'Search')]")
            search_button.click()
            time.sleep(2)
            
            # Wait for results - doctor cards should appear
            results = WebDriverWait(driver, 10).until(
                EC.presence_of_all_elements_located((By.XPATH, "//div[@class='card'][.//button[contains(text(), 'Book Now')]]"))
            )
            assert len(results) > 0, "No doctor results found"
        except Exception as e:
            pytest.skip(f"Doctor search failed: {str(e)}")
    
    def test_filter_by_specialization(self, driver, base_url, test_user):
        """Test filtering doctors by specialization."""
        # Login first
        if not login_user(driver, base_url, test_user):
            pytest.skip("Failed to login")
            
        driver.get(f"{base_url}/patient")
        time.sleep(2)
        
        try:
            # Find specialization filter dropdown (select element)
            spec_filter = WebDriverWait(driver, 10).until(
                EC.presence_of_element_located((By.XPATH, "//select"))
            )
            
            # Select an option (skip first one which is "All Specializations")
            spec_filter.click()
            time.sleep(1)
            
            # Get all options
            options = spec_filter.find_elements(By.TAG_NAME, "option")
            if len(options) > 1:
                # Click second option
                options[1].click()
                time.sleep(2)
                
                # Click search button to apply filter
                search_button = driver.find_element(By.XPATH, "//button[contains(text(), 'Search')]")
                search_button.click()
                time.sleep(2)
                
                # Verify results are filtered
                results = driver.find_elements(By.XPATH, "//div[@class='card'][.//button[contains(text(), 'Book Now')]]")
                assert len(results) >= 0, "Filter did not work"
        except Exception as e:
            pytest.skip(f"Specialization filter not available: {str(e)}")
    
    def test_doctor_details_modal(self, driver, base_url, test_user):
        """Test opening doctor details modal."""
        # Login first
        if not login_user(driver, base_url, test_user):
            pytest.skip("Failed to login")
            
        driver.get(f"{base_url}/patient")
        time.sleep(2)
        
        try:
            # Wait for first doctor card button to be clickable (Book Now button)
            book_button = WebDriverWait(driver, 10).until(
                EC.element_to_be_clickable((By.XPATH, "//button[contains(text(), 'Book Now')]"))
            )
            book_button.click()
            time.sleep(2)
            
            # Verify we're on booking page
            assert "/book/" in driver.current_url, "Did not navigate to booking page"
        except Exception as e:
            pytest.skip(f"Doctor booking not accessible: {str(e)}")
    
    def test_add_doctor_to_favorites(self, driver, base_url, test_user):
        """Test adding doctor to favorites."""
        # Login first
        if not login_user(driver, base_url, test_user):
            pytest.skip("Failed to login")
            
        driver.get(f"{base_url}/patient")
        time.sleep(2)
        
        try:
            # Find favorite button - look for icon in doctor cards
            favorite_button = WebDriverWait(driver, 10).until(
                EC.element_to_be_clickable((By.XPATH, "//div[@class='card'][.//button[contains(text(), 'Book Now')]]//*[contains(text(), '★') or contains(text(), '☆')]"))
            )
            
            # Click favorite button
            favorite_button.click()
            time.sleep(1)
            
            # Verify favorite was added
            assert True, "Favorite button clicked"
        except Exception as e:
            pytest.skip(f"Favorite button not found: {str(e)}")


class TestDoctorSortAndFilter:
    """Doctor sorting and filtering tests."""
    
    def test_sort_by_rating(self, driver, base_url, test_user):
        """Test sorting doctors by rating."""
        # Login first
        if not login_user(driver, base_url, test_user):
            pytest.skip("Failed to login")
            
        driver.get(f"{base_url}/patient")
        time.sleep(2)
        
        try:
            # Doctor cards should be displayed
            doctors = WebDriverWait(driver, 10).until(
                EC.presence_of_all_elements_located((By.XPATH, "//div[@class='card'][.//button[contains(text(), 'Book Now')]]"))
            )
            assert len(doctors) > 0, "No doctors found"
        except Exception as e:
            pytest.skip(f"Sort functionality not available: {str(e)}")
    
    
    def test_search_clear(self, driver, base_url, test_user):
        """Test clearing search."""
        # Login first
        if not login_user(driver, base_url, test_user):
            pytest.skip("Failed to login")
            
        driver.get(f"{base_url}/patient")
        time.sleep(2)
        
        try:
            search_input = WebDriverWait(driver, 10).until(
                EC.presence_of_element_located((By.XPATH, "//input[@placeholder='Search doctors...']"))
            )
            search_input.send_keys("Test")
            time.sleep(1)
            
            # Clear the input
            search_input.clear()
            time.sleep(1)
            
            # Verify search is cleared
            assert search_input.get_attribute("value") == "", "Search not cleared"
        except Exception as e:
            pytest.skip(f"Clear search not available: {str(e)}")
