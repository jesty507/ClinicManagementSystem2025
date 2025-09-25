using ConsoleApp_cmsv2025.Model;
using ConsoleApp_cmsv2025.Utility;
using System;

namespace ConsoleApp_cmsv2025
{
    public class ValidationDemo
    {
        public static void RunValidationTests()
        {
            Console.WriteLine("=== CLINIC MANAGEMENT SYSTEM VALIDATION DEMO ===\n");

            // Test Mobile Number Validation
            TestMobileNumberValidation();

            // Test Date and Time Validation
            TestDateTimeValidation();

            // Test Model Validation
            TestModelValidation();

            Console.WriteLine("\n=== VALIDATION DEMO COMPLETED ===");
        }

        private static void TestMobileNumberValidation()
        {
            Console.WriteLine("--- MOBILE NUMBER VALIDATION TESTS ---");

            string[] testMobileNumbers = {
                "9876543210",      // Valid - 10 digits starting with 9
                "+919876543210",   // Valid - with +91 prefix
                "09876543210",     // Valid - with 0 prefix
                "1234567890",      // Invalid - starts with 1
                "987654321",       // Invalid - only 9 digits
                "98765432100",     // Invalid - 11 digits
                "abc987654321",    // Invalid - contains letters
                "987-654-3210",    // Valid - with formatting (will be cleaned)
                "+91 98765 43210", // Valid - with spaces (will be cleaned)
                ""                 // Invalid - empty
            };

            foreach (string mobile in testMobileNumbers)
            {
                bool isValid = CustomValidation.IsValidMobileNumber(mobile);
                string formatted = CustomValidation.FormatMobileNumber(mobile);
                Console.WriteLine($"Mobile: '{mobile}' -> Valid: {isValid} | Formatted: '{formatted}'");
            }

            Console.WriteLine();
        }

        private static void TestDateTimeValidation()
        {
            Console.WriteLine("--- DATE AND TIME VALIDATION TESTS ---");

            DateTime now = DateTime.Now;
            DateTime yesterday = now.AddDays(-1);
            DateTime tomorrow = now.AddDays(1);
            DateTime nextHour = now.AddHours(1);
            DateTime pastHour = now.AddHours(-1);

            // Test Birth Date Validation
            Console.WriteLine("Birth Date Validation:");
            Console.WriteLine($"Yesterday ({yesterday:yyyy-MM-dd}): {CustomValidation.IsValidBirthDate(yesterday)}");
            Console.WriteLine($"Today ({now.Date:yyyy-MM-dd}): {CustomValidation.IsValidBirthDate(now.Date)}");
            Console.WriteLine($"Tomorrow ({tomorrow.Date:yyyy-MM-dd}): {CustomValidation.IsValidBirthDate(tomorrow.Date)}");

            // Test Appointment DateTime Validation
            Console.WriteLine("\nAppointment DateTime Validation:");
            Console.WriteLine($"Past Hour ({pastHour:yyyy-MM-dd HH:mm}): {CustomValidation.IsValidAppointmentDateTime(pastHour)}");
            Console.WriteLine($"Now ({now:yyyy-MM-dd HH:mm}): {CustomValidation.IsValidAppointmentDateTime(now)}");
            Console.WriteLine($"Next Hour ({nextHour:yyyy-MM-dd HH:mm}): {CustomValidation.IsValidAppointmentDateTime(nextHour)}");

            // Test Future DateTime Validation
            Console.WriteLine("\nFuture DateTime Validation:");
            Console.WriteLine($"Past Hour ({pastHour:yyyy-MM-dd HH:mm}): {CustomValidation.IsValidFutureDateTime(pastHour)}");
            Console.WriteLine($"Now ({now:yyyy-MM-dd HH:mm}): {CustomValidation.IsValidFutureDateTime(now)}");
            Console.WriteLine($"Next Hour ({nextHour:yyyy-MM-dd HH:mm}): {CustomValidation.IsValidFutureDateTime(nextHour)}");

            Console.WriteLine();
        }

        private static void TestModelValidation()
        {
            Console.WriteLine("--- MODEL VALIDATION TESTS ---");

            // Test Valid Patient
            Console.WriteLine("Testing Valid Patient:");
            var validPatient = new TblPatient
            {
                UserId = 1,
                PatientName = "John Doe",
                DateOfBirth = new DateTime(1990, 5, 15),
                Gender = "Male",
                BloodGroup = "O+",
                Address = "123 Main Street, City, State",
                MobileNumber = "9876543210",
                MembershipId = 1,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            string patientValidation = CustomValidation.ValidatePatientModel(validPatient);
            Console.WriteLine($"Valid Patient Errors: {(string.IsNullOrEmpty(patientValidation) ? "None" : patientValidation)}");

            // Test Invalid Patient
            Console.WriteLine("\nTesting Invalid Patient:");
            var invalidPatient = new TblPatient
            {
                UserId = 0, // Invalid - should be positive
                PatientName = "J", // Invalid - too short
                DateOfBirth = DateTime.Now.AddDays(1), // Invalid - future date
                Gender = "Invalid", // Invalid gender
                BloodGroup = "X+", // Invalid blood group
                Address = "Hi", // Invalid - too short
                MobileNumber = "123", // Invalid mobile number
                MembershipId = 0, // Invalid - should be positive
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            string invalidPatientValidation = CustomValidation.ValidatePatientModel(invalidPatient);
            Console.WriteLine($"Invalid Patient Errors: {invalidPatientValidation}");

            // Test Valid Appointment
            Console.WriteLine("\nTesting Valid Appointment:");
            var validAppointment = new TblAppointment
            {
                PatientId = 1,
                DoctorId = 1,
                UserId = 1,
                AppointmentDate = DateTime.Now.AddHours(2), // Future appointment
                TokenNumber = 5,
                ConsultationStatus = "Pending",
                CreatedAt = 1,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            string appointmentValidation = CustomValidation.ValidateAppointmentModel(validAppointment);
            Console.WriteLine($"Valid Appointment Errors: {(string.IsNullOrEmpty(appointmentValidation) ? "None" : appointmentValidation)}");

            // Test Invalid Appointment
            Console.WriteLine("\nTesting Invalid Appointment:");
            var invalidAppointment = new TblAppointment
            {
                PatientId = 0, // Invalid - should be positive
                DoctorId = 0, // Invalid - should be positive
                UserId = 0, // Invalid - should be positive
                AppointmentDate = DateTime.Now.AddHours(-1), // Invalid - past appointment
                TokenNumber = -1, // Invalid - negative token
                ConsultationStatus = "Invalid Status", // Invalid status
                CreatedAt = 0, // Invalid - should be positive
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            string invalidAppointmentValidation = CustomValidation.ValidateAppointmentModel(invalidAppointment);
            Console.WriteLine($"Invalid Appointment Errors: {invalidAppointmentValidation}");

            Console.WriteLine();
        }

        // Additional validation tests for specific fields
        public static void TestFieldValidations()
        {
            Console.WriteLine("--- INDIVIDUAL FIELD VALIDATION TESTS ---");

            // Email Validation
            Console.WriteLine("Email Validation:");
            string[] testEmails = {
                "user@example.com",
                "test.email+tag@domain.co.uk",
                "invalid-email",
                "user@",
                "@domain.com",
                ""
            };

            foreach (string email in testEmails)
            {
                Console.WriteLine($"Email: '{email}' -> Valid: {CustomValidation.IsValidEmail(email)}");
            }

            // Name Validation
            Console.WriteLine("\nName Validation:");
            string[] testNames = {
                "John Doe",
                "Mary-Jane O'Connor",
                "Dr. Smith",
                "A", // Too short
                "John123", // Contains numbers
                "John@Doe", // Contains special characters
                ""
            };

            foreach (string name in testNames)
            {
                Console.WriteLine($"Name: '{name}' -> Valid: {CustomValidation.IsValidName(name)}");
            }

            // Blood Group Validation
            Console.WriteLine("\nBlood Group Validation:");
            string[] testBloodGroups = {
                "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-",
                "a+", "A+", "INVALID", "X+", ""
            };

            foreach (string bloodGroup in testBloodGroups)
            {
                Console.WriteLine($"Blood Group: '{bloodGroup}' -> Valid: {CustomValidation.IsValidBloodGroup(bloodGroup)}");
            }

            // Gender Validation
            Console.WriteLine("\nGender Validation:");
            string[] testGenders = {
                "Male", "Female", "Other", "M", "F", "O",
                "male", "MALE", "Invalid", "Unknown", ""
            };

            foreach (string gender in testGenders)
            {
                Console.WriteLine($"Gender: '{gender}' -> Valid: {CustomValidation.IsValidGender(gender)}");
            }

            Console.WriteLine();
        }
    }
}
