# MediSync UI Selenium Tests

Python Selenium test suite for the MediSync E-Channel Platform frontend application.

## Overview

This test suite contains automated UI tests for the MediSync platform covering:
- **Authentication**: Login, signup, logout, and validation
- **Doctor Search**: Searching, filtering, sorting doctors
- **Appointment Booking**: Full booking workflow from selection to confirmation
- **User Profile**: Profile viewing, editing, password changes
- **Appointment Management**: Viewing history, canceling appointments

## Project Structure

```
tests/UI/
├── conftest.py                 # Pytest configuration and fixtures
├── test_authentication.py       # Login/signup/logout tests
├── test_doctor_search.py        # Doctor search and filtering tests
├── test_appointment_booking.py  # Appointment booking workflow tests
├── test_user_profile.py         # User profile management tests
├── requirements.txt             # Python dependencies
└── README.md                    # This file
```

## Prerequisites

- Python 3.8+
- Google Chrome browser installed
- ChromeDriver (automatically downloaded by webdriver-manager)
- pip package manager

## Installation

1. **Navigate to the UI tests directory:**
   ```bash
   cd tests/UI
   ```

2. **Create a virtual environment (optional but recommended):**
   ```bash
   python -m venv venv
   source venv/bin/activate  # On Windows: venv\Scripts\activate
   ```

3. **Install dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

## Configuration

### Environment Variables

Create a `.env` file in the `tests/UI/` directory if you need custom configurations:

```
BASE_URL=http://localhost:5173
TEST_USER_EMAIL=test@example.com
TEST_USER_PASSWORD=TestPassword123!
```

### Browser Options

Edit `conftest.py` to configure browser behavior:

```python
# Uncomment to run headless (without UI)
chrome_options.add_argument("--headless")

# Add more options as needed
chrome_options.add_argument("--window-size=1920,1080")
```

## Running Tests

### Run All Tests
```bash
pytest
```

### Run Specific Test File
```bash
pytest test_authentication.py
```

### Run Specific Test Class
```bash
pytest test_doctor_search.py::TestDoctorSearch
```

### Run Specific Test
```bash
pytest test_doctor_search.py::TestDoctorSearch::test_doctor_search_page_loads
```

### Run with Verbose Output
```bash
pytest -v
```

### Run with HTML Report
```bash
pytest --html=report.html --self-contained-html
```

### Run in Headless Mode
```bash
pytest --headless
```

### Run with Timeout (useful for CI/CD)
```bash
pytest --timeout=300
```

## Test Markers

You can run tests by category using markers (if configured):

```bash
# Run only critical tests
pytest -m critical

# Run everything except slow tests
pytest -m "not slow"
```

## Test Structure

Each test file follows this structure:

```python
import pytest
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC


class TestFeatureName:
    """Feature description."""
    
    def test_specific_scenario(self, driver, base_url):
        """Test description."""
        driver.get(f"{base_url}/path")
        time.sleep(2)
        
        # Interact with elements
        element = driver.find_element(By.XPATH, "//xpath")
        element.click()
        
        # Assert results
        assert condition, "Error message"
```

## Key Fixtures (from conftest.py)

- `driver`: Chrome WebDriver instance with implicit waits
- `base_url`: Base URL for the frontend (default: http://localhost:5173)
- `wait`: WebDriverWait instance for explicit waits
- `test_user`: Dictionary with test user credentials

## Helpers

Useful helper functions are available in `conftest.py`:

```python
wait_for_element(driver, locator, timeout=10)
wait_for_element_clickable(driver, locator, timeout=10)
```

## Troubleshooting

### Common Issues

1. **"Element not found" errors**
   - Check if the base_url is correct
   - Verify the frontend application is running
   - Update XPath selectors if UI has changed

2. **"Connection refused" errors**
   - Ensure frontend is running on http://localhost:5173
   - Check if port 5173 is available

3. **Timeout errors**
   - Increase timeout values in conftest.py
   - Check if backend API is responding
   - Verify database connection

4. **WebDriver issues**
   - Ensure Chrome browser is installed
   - Check Chrome version compatibility
   - Update webdriver-manager: `pip install --upgrade webdriver-manager`

## Best Practices

1. **Use explicit waits** instead of `time.sleep()` when possible
2. **Use meaningful test names** that describe what is being tested
3. **Add docstrings** to explain test purpose
4. **Use pytest.skip()** for tests that depend on specific conditions
5. **Keep tests isolated** - each test should be independent
6. **Use fixtures** from conftest.py for common setup/teardown

## CI/CD Integration

### GitHub Actions Example
```yaml
- name: Run UI Tests
  run: |
    cd tests/UI
    pip install -r requirements.txt
    pytest --timeout=300
```

### GitLab CI Example
```yaml
ui_tests:
  script:
    - cd tests/UI
    - pip install -r requirements.txt
    - pytest --timeout=300
```

## Performance Tips

1. **Run tests in parallel** (requires pytest-xdist):
   ```bash
   pip install pytest-xdist
   pytest -n auto
   ```

2. **Enable headless mode** for faster execution:
   - Uncomment in conftest.py or use CLI flag

3. **Use smaller waits** for responsive elements:
   ```python
   wait = WebDriverWait(driver, 5)  # Shorter timeout for faster elements
   ```

## Maintenance

### Updating Selectors

If UI changes break tests, update XPath expressions in the relevant test file:

```python
# Old
driver.find_element(By.XPATH, "//button[@class='old-class']")

# New
driver.find_element(By.XPATH, "//button[@class='new-class']")
```

### Adding New Tests

1. Create a new test method in appropriate test class
2. Follow naming convention: `test_<what_is_being_tested>`
3. Add descriptive docstring
4. Use existing fixtures and helpers
5. Run the test to verify it works

## Contributing

When adding new tests:
1. Ensure tests are independent
2. Use descriptive names
3. Handle exceptions gracefully
4. Add comments for complex selectors
5. Test both happy path and error cases

## License

Same as main MediSync project.

## Support

For issues or questions:
1. Check the troubleshooting section
2. Review test output and browser logs
3. Verify application is running correctly
4. Check that selectors match current UI
