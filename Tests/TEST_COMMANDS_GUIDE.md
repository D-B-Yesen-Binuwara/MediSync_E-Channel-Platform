# Test Commands Guide

## Available Test Commands

### 1. User Validation Tests
```bash
dotnet test Tests --filter "UserValidationTests" --verbosity detailed
```

Scope: Tests all user profile update operations and validations
Test Cases:
- Change Password (valid case)
- Change Password (wrong current password)
- Change Password (password mismatch)
- Change Password (too short password)
- Change Name (valid case)
- Change Name (empty name)
- Change Contact Number (valid case)
- Change Contact Number (empty number)
- Change Email (valid case)
- Change Email (empty email)

Known Issues Found:
- Empty name validation is missing (allows empty names)
- Empty phone validation is missing (allows empty phone numbers)

Limitations:
- Uses in-memory database (not real database)
- Does not test duplicate email scenarios
- Does not test invalid email format validation

### 2. Real Credential Tests
```bash
dotnet test Tests --filter "RealCredentialTests" --verbosity detailed
```

Scope: Tests authentication system compatibility and password hashing
Test Cases:
- BCrypt vs PBKDF2 compatibility test
- Password strength validation scenarios
- Real user credentials from database
- JWT token generation

Critical Bug Found:
- BCrypt hash from database cannot be verified with PBKDF2 system
- Existing users cannot login due to hash incompatibility

Limitations:
- Uses mock HTTP client
- Does not test actual database connection
- Limited to password scenarios from test data

### 3. User Service Integration Tests
```bash
dotnet test Tests --filter "UserServiceIntegrationTests" --verbosity detailed
```

Scope: Tests UserService business logic operations
Test Cases:
- Get user profile (valid user)
- Get user profile (invalid user)
- Update user profile (valid data)
- Update profile (empty email validation)
- Change password (valid passwords)
- Change password (password mismatch)
- Change password (short password)
- Change password (wrong current password)
- Delete user account (valid user)
- Delete user account (invalid user)

Limitations:
- Uses in-memory database
- Does not test concurrent user operations
- Limited error scenario coverage

### 4. User Controller Endpoint Tests
```bash
dotnet test Tests --filter "UserControllerEndpointTests" --verbosity detailed
```

Scope: Tests HTTP endpoints and API responses
Test Cases:
- Unauthorized access to protected endpoints
- User registration workflow
- User login with real credentials
- Full user workflow (register, login, update profile)

Known Issues:
- May fail due to BCrypt/PBKDF2 compatibility issue
- Requires running web application factory

Limitations:
- Uses test web application factory
- Does not test actual deployed API
- Limited to basic HTTP response validation

### 5. All Tests Combined
```bash
dotnet test Tests --verbosity detailed
```

Scope: Runs all available tests in the project
Coverage:
- All validation tests
- All credential tests
- All service integration tests
- All endpoint tests

Expected Results:
- Total tests: Approximately 25+ tests
- Known failures: 2-3 tests due to validation bugs
- Critical issues: BCrypt/PBKDF2 incompatibility

### 6. Quick Test Summary
```bash
dotnet test Tests
```

Scope: Runs all tests with minimal output
Output: Only shows pass/fail summary without detailed logs
Use Case: Quick validation of overall system health

## Test Results and Logging

Test Logs Location: Tests/Resources/test-execution-log.txt
Console Output: Real-time during test execution
Test Reports: Displayed in terminal with pass/fail counts

## Known System Issues Discovered

1. Critical Authentication Bug
   - BCrypt passwords in database cannot be verified
   - PBKDF2 system incompatible with existing user passwords
   - Priority: CRITICAL - Existing users cannot login

2. Validation Bugs
   - Empty name validation missing in UpdateProfile
   - Empty phone validation missing in UpdateProfile
   - Priority: MEDIUM - Data integrity issues

3. Missing Test Coverage
   - Email format validation not tested
   - Duplicate email scenarios not covered
   - Concurrent operations not tested
   - Database constraint violations not tested

## Recommendations

1. Fix BCrypt/PBKDF2 compatibility issue immediately
2. Add missing validation for empty name and phone
3. Expand test coverage for edge cases
4. Add integration tests with real database
5. Add performance tests for user operations