# Clinic Management System - Validation Guide

## Overview
This document outlines all the validation rules implemented in the Clinic Management System. The validation system ensures data integrity and provides user-friendly error messages.

## Validation Categories

### 1. Mobile Number Validation
**Purpose**: Validates Indian mobile number format

**Rules**:
- Must be 10 digits starting with 6, 7, 8, or 9
- Can include country code (+91) or leading zero (0)
- Spaces, hyphens, and parentheses are automatically cleaned
- Examples of valid formats:
  - `9876543210` (10 digits)
  - `+919876543210` (with +91)
  - `09876543210` (with 0)
  - `+91 98765 43210` (with formatting)

**Methods**:
- `IsValidMobileNumber(string mobileNumber)`
- `FormatMobileNumber(string mobileNumber)` - Returns formatted number

### 2. Date and Time Validation

#### Birth Date Validation
**Purpose**: Ensures birth dates are realistic and not in the future

**Rules**:
- Cannot be in the future
- Cannot be more than 150 years ago
- Optional field (null is valid)

**Method**: `IsValidBirthDate(DateTime? birthDate)`

#### Joining Date Validation
**Purpose**: Ensures joining dates are not in the future

**Rules**:
- Cannot be in the future
- Optional field (null is valid)

**Method**: `IsValidJoiningDate(DateTime? joiningDate)`

#### Appointment DateTime Validation
**Purpose**: Ensures appointment dates and times are not in the past

**Rules**:
- Must be in the future (after current date and time)
- Critical for preventing past appointments

**Methods**:
- `IsValidAppointmentDateTime(DateTime appointmentDateTime)`
- `IsValidAppointmentDate(DateTime appointmentDate)` (date only)
- `IsValidFutureDateTime(DateTime dateTime)` (general future validation)

#### Consultation DateTime Validation
**Purpose**: Ensures consultation dates and times are not in the past

**Rules**:
- Must be in the future (after current date and time)

**Method**: `IsValidConsultationDateTime(DateTime consultationDateTime)`

### 3. Email Validation
**Purpose**: Validates email format

**Rules**:
- Must contain @ symbol
- Must have valid domain structure
- Cannot be empty

**Method**: `IsValidEmail(string email)`

### 4. Name Validation
**Purpose**: Validates person names

**Rules**:
- Must be 2-150 characters long
- Can contain letters, spaces, hyphens, apostrophes, and dots
- Cannot be empty
- Cannot contain numbers or special characters

**Method**: `IsValidName(string name)`

### 5. Blood Group Validation
**Purpose**: Validates blood group format

**Rules**:
- Must be one of: A+, A-, B+, B-, AB+, AB-, O+, O-
- Case insensitive
- Optional field (empty/null is valid)

**Method**: `IsValidBloodGroup(string bloodGroup)`

### 6. Gender Validation
**Purpose**: Validates gender field

**Rules**:
- Must be one of: Male, Female, Other, M, F, O
- Case insensitive
- Optional field (empty/null is valid)

**Method**: `IsValidGender(string gender)`

### 7. Username Validation
**Purpose**: Validates usernames for login

**Rules**:
- Must contain only alphanumeric characters, underscores, and dots
- Maximum 30 characters
- Cannot be empty

**Method**: `IsValidUsername(string userName)`

### 8. Password Validation
**Purpose**: Validates password strength

**Rules**:
- Must contain at least one uppercase letter
- Must contain at least one lowercase letter
- Must contain at least one digit
- Must contain at least one special character
- Cannot be empty

**Method**: `isValidPassword(string password)`

### 9. Address Validation
**Purpose**: Validates address fields

**Rules**:
- Must be 5-300 characters long
- Optional field (empty/null is valid)

**Method**: `IsValidAddress(string address)`

### 10. Numeric Validation

#### Positive Integer Validation
**Purpose**: Validates ID fields and counts

**Rules**:
- Must be greater than 0
- Must be an integer

**Methods**:
- `IsValidPositiveInteger(int value)`
- `IsValidNonNegativeInteger(int value)` (allows 0)

#### Decimal Validation
**Purpose**: Validates fees and monetary amounts

**Rules**:
- Must be non-negative (≥ 0)
- Can be decimal

**Methods**:
- `IsValidPositiveDecimal(decimal value)`
- `IsValidPositiveDecimal(decimal value, decimal minimumValue)`

### 11. Text Length Validation
**Purpose**: Validates text field lengths

**Rules**:
- Must be within specified minimum and maximum lengths
- Empty text is valid only if minimum length is 0

**Method**: `IsValidTextLength(string text, int minLength, int maxLength)`

### 12. Address Validation
**Purpose**: Validates address fields

**Rules**:
- Optional field (empty/null is valid)
- Accepts any length address
- No format restrictions

**Method**: `IsValidAddress(string address)`

### 13. MRN Validation
**Purpose**: Validates Medical Record Number format

**Rules**:
- Must start with "MRN000" (case insensitive)
- Followed by a positive integer
- Examples: MRN000123, MRN000001, MRN000999

**Methods**:
- `IsValidMRN(string mrn)` - Validates MRN format
- `FormatMRN(int patientId)` - Formats patient ID as MRN
- `ExtractPatientIdFromMRN(string mrn)` - Extracts patient ID from MRN

## Model-Level Validation

### TblUser Model Validation
**Method**: `ValidateUserModel(TblUser user)`

**Validates**:
- Full Name (required, name format)
- Email (valid format)
- Mobile Number (Indian format)
- Gender (valid options)
- Date of Birth (not future, reasonable age)
- Joining Date (not future)
- Blood Group (valid options)
- Address (length)
- Username (format)
- Password (strength)
- Role ID (positive integer)

### TblPatient Model Validation
**Method**: `ValidatePatientModel(TblPatient patient)`

**Validates**:
- User ID (positive integer)
- Patient Name (required, name format)
- Date of Birth (not future, reasonable age)
- Gender (valid options)
- Blood Group (valid options)
- Address (length)
- Mobile Number (Indian format)
- Membership ID (positive integer)

### TblAppointment Model Validation
**Method**: `ValidateAppointmentModel(TblAppointment appointment)`

**Validates**:
- Patient ID (positive integer)
- Doctor ID (positive integer)
- User ID (positive integer)
- Appointment Date & Time (not in past)
- Token Number (non-negative integer)
- Consultation Status (valid options: Pending, Completed, Cancelled, No Show)
- Created At (positive integer)

### TblDoctor Model Validation
**Method**: `ValidateDoctorModel(TblDoctor doctor)`

**Validates**:
- User ID (positive integer)
- Consultation Fee (non-negative decimal)

### TblConsultation Model Validation
**Method**: `ValidateConsultationModel(TblConsultation consultation)`

**Validates**:
- Appointment ID (positive integer)
- Patient ID (positive integer)
- Symptoms (max 4000 characters)
- Diagnosis (max 4000 characters)
- Notes (max 4000 characters)

## Service Layer Integration

The validation is integrated into the service layer methods:

### UserServiceImpl Methods with Validation:
- `SearchPatientByMobileAsync()` - Validates mobile number format
- `CreateNewPatientAsync()` - Validates entire patient model
- `CheckMobileNumberExistsAsync()` - Validates mobile number format
- `CreateAppointmentAsync()` - Validates entire appointment model
- `CreateConsultationAsync()` - Validates entire consultation model

### Error Handling:
- Validation failures throw `ArgumentException` with descriptive error messages
- Multiple validation errors are combined into a single message
- Error messages are user-friendly and actionable

## Testing and Demo

### Validation Demo
Run the validation demo to see all validations in action:

1. Launch the application
2. Select "2. Run Validation Demo" from the main menu
3. View comprehensive tests for:
   - Mobile number validation
   - Date and time validation
   - Model validation
   - Individual field validation

### Demo Features:
- Tests valid and invalid inputs
- Shows formatted outputs
- Demonstrates error messages
- Covers edge cases

## Best Practices

1. **Always validate before database operations**
2. **Use model-level validation for complex objects**
3. **Provide clear, actionable error messages**
4. **Validate at multiple layers (UI, Service, Repository)**
5. **Handle validation errors gracefully**
6. **Test with edge cases and invalid inputs**

## Usage Examples

### Basic Field Validation
```csharp
// Validate mobile number
if (!CustomValidation.IsValidMobileNumber(mobileNumber))
{
    throw new ArgumentException("Invalid mobile number format");
}

// Validate appointment date
if (!CustomValidation.IsValidAppointmentDateTime(appointmentDate))
{
    throw new ArgumentException("Appointment date cannot be in the past");
}
```

### Model Validation
```csharp
// Validate entire patient model
string validationErrors = CustomValidation.ValidatePatientModel(patient);
if (!string.IsNullOrEmpty(validationErrors))
{
    throw new ArgumentException($"Patient validation failed: {validationErrors}");
}
```

### Service Layer Integration
```csharp
public async Task<int> CreateNewPatientAsync(TblPatient patient)
{
    // Validate patient model
    string validationErrors = CustomValidation.ValidatePatientModel(patient);
    if (!string.IsNullOrEmpty(validationErrors))
    {
        throw new ArgumentException($"Patient validation failed: {validationErrors}");
    }

    return await _userRepository.CreateNewPatientAsync(patient);
}
```

## Error Messages

The validation system provides specific, actionable error messages:

- **Mobile Number**: "Invalid mobile number format. Please enter a valid Indian mobile number."
- **Appointment Date**: "Appointment date and time cannot be in the past"
- **Patient Model**: "Patient validation failed: Patient Name is required and should contain only letters, spaces, hyphens, apostrophes, and dots (2-150 characters); Invalid mobile number format"

This comprehensive validation system ensures data integrity and provides excellent user experience with clear feedback.
