using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConsoleApp_cmsv2025.Model;

namespace ConsoleApp_cmsv2025.Utility
{
    public class CustomValidation
    {

        #region 1 - Username Validation
        //Username should not be empty
        //It contain only alphanumeric characters and underscores and dot
        public static bool IsValidUsername(string userName)
        {
            return !string.IsNullOrWhiteSpace(userName) 
                && userName.Length <= 30
                && Regex.IsMatch(userName, @"^[a-zA-Z0-9._]+$");
        }
        #endregion



        #region 2 - Password Validation
        //Password should have atleast 4 characters
        //including atleast one uppercase,one lowercase,one digit and one special character
        public static bool isValidPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password)
                && Regex.IsMatch(password, @"[A-Z]+") //atleast one uppercase
                && Regex.IsMatch(password, @"[a-z]+") //atleast one lowercase
                && Regex.IsMatch(password, @"[0-9]+") //atleast one digit
                && Regex.IsMatch(password, @"[\W_]+"); //atleast one special character
        }
        #endregion



        #region * sysmbol for password
        public static string ReadPassword()
        {
            string password = string.Empty;
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                //Each keystroke from user replace with * symbol
                //Add it to passwordstring until the user press Enter key
                if (key.Key != ConsoleKey.Backspace 
                    && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace 
                    && password.Length > 0)
                {
                    password = password.Substring(0, (password.Length - 1));
                    Console.Write("\b \b");
                }
            }while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return password;
        }
        #endregion

        #region 3 - Mobile Number Validation
        //Mobile number should be valid Indian mobile number format
        //Should start with +91 or 0 and be 10-12 digits
        public static bool IsValidMobileNumber(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                return false;

            // Remove any spaces, hyphens, or parentheses
            string cleanNumber = Regex.Replace(mobileNumber, @"[\s\-\(\)]", "");
            
            // Indian mobile number patterns:
            // +91 followed by 10 digits starting with 6,7,8,9
            // 0 followed by 10 digits starting with 6,7,8,9
            // 10 digits starting with 6,7,8,9
            string pattern = @"^(\+91|0)?[6-9]\d{9}$";
            
            return Regex.IsMatch(cleanNumber, pattern);
        }

        //Get formatted mobile number
        public static string FormatMobileNumber(string mobileNumber)
        {
            if (!IsValidMobileNumber(mobileNumber))
                return mobileNumber;

            string cleanNumber = Regex.Replace(mobileNumber, @"[\s\-\(\)]", "");
            
            if (cleanNumber.StartsWith("+91"))
                cleanNumber = cleanNumber.Substring(3);
            else if (cleanNumber.StartsWith("0"))
                cleanNumber = cleanNumber.Substring(1);

            return $"+91 {cleanNumber.Substring(0, 5)} {cleanNumber.Substring(5)}";
        }
        #endregion

        #region 4 - Email Validation
        //Email should be in valid format
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }
        #endregion

        #region 5 - Date Validation
        //Date should not be in the future for birth dates and joining dates
        public static bool IsValidBirthDate(DateTime? birthDate)
        {
            if (!birthDate.HasValue)
                return true; // Optional field

            DateTime today = DateTime.Today;
            return birthDate.Value <= today && birthDate.Value >= today.AddYears(-150); // Reasonable age range
        }

        //Date should not be in the future for joining dates
        public static bool IsValidJoiningDate(DateTime? joiningDate)
        {
            if (!joiningDate.HasValue)
                return true; // Optional field

            DateTime today = DateTime.Today;
            return joiningDate.Value <= today;
        }

        //Date should not be before current date for appointments
        public static bool IsValidAppointmentDate(DateTime appointmentDate)
        {
            DateTime today = DateTime.Today;
            return appointmentDate >= today;
        }
        #endregion

        #region 6 - DateTime Validation (Date + Time)
        //DateTime should not be before current date and time
        public static bool IsValidFutureDateTime(DateTime dateTime)
        {
            DateTime now = DateTime.Now;
            return dateTime >= now;
        }

        //Appointment date and time should not be in the past
        public static bool IsValidAppointmentDateTime(DateTime appointmentDateTime)
        {
            DateTime now = DateTime.Now;
            return appointmentDateTime >= now;
        }

        //Consultation date and time should not be in the past
        public static bool IsValidConsultationDateTime(DateTime consultationDateTime)
        {
            DateTime now = DateTime.Now;
            return consultationDateTime >= now;
        }
        #endregion

        #region 7 - Name Validation
        //Name should contain only letters, spaces, and common name characters
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Allow letters, spaces, hyphens, apostrophes, and dots
            string pattern = @"^[a-zA-Z\s\-'\.]+$";
            return Regex.IsMatch(name, pattern) && name.Length >= 2 && name.Length <= 150;
        }
        #endregion

        #region 8 - Blood Group Validation
        //Blood group should be valid blood type
        public static bool IsValidBloodGroup(string bloodGroup)
        {
            if (string.IsNullOrWhiteSpace(bloodGroup))
                return true; // Optional field

            string[] validBloodGroups = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            return validBloodGroups.Contains(bloodGroup.ToUpper());
        }
        #endregion

        #region 9 - Gender Validation
        //Gender should be valid
        public static bool IsValidGender(string gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                return true; // Optional field

            string[] validGenders = { "Male", "Female", "Other", "M", "F", "O" };
            return validGenders.Contains(gender, StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region 10 - Decimal Validation
        //Positive decimal validation for fees and amounts
        public static bool IsValidPositiveDecimal(decimal value)
        {
            return value >= 0;
        }

        //Positive decimal validation for fees and amounts with minimum value
        public static bool IsValidPositiveDecimal(decimal value, decimal minimumValue)
        {
            return value >= minimumValue;
        }
        #endregion

        #region 11 - Integer Validation
        //Positive integer validation for IDs and counts
        public static bool IsValidPositiveInteger(int value)
        {
            return value > 0;
        }

        //Non-negative integer validation
        public static bool IsValidNonNegativeInteger(int value)
        {
            return value >= 0;
        }
        #endregion

        #region 12 - Text Length Validation
        //Validate text length within specified range
        public static bool IsValidTextLength(string text, int minLength, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return minLength == 0;

            return text.Length >= minLength && text.Length <= maxLength;
        }
        #endregion

        #region 13 - Address Validation
        //Basic address validation - allows any length
        public static bool IsValidAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return true; // Optional field

            return true; // Accept any non-empty address
        }
        #endregion

        #region 15 - MRN Validation
        //MRN should be in format MRN000123
        public static bool IsValidMRN(string mrn)
        {
            if (string.IsNullOrWhiteSpace(mrn))
                return false;

            // Check if MRN starts with "MRN000" (case insensitive)
            if (!mrn.StartsWith("MRN000", StringComparison.OrdinalIgnoreCase))
                return false;

            // Get the number part after "MRN000"
            string numberPart = mrn.Substring(6);
            
            // Check if the number part is a valid positive integer
            return int.TryParse(numberPart, out int patientId) && patientId > 0;
        }

        //Format MRN with proper prefix
        public static string FormatMRN(int patientId)
        {
            return $"MRN000{patientId}";
        }

        //Extract patient ID from MRN format
        public static int ExtractPatientIdFromMRN(string mrn)
        {
            if (!IsValidMRN(mrn))
                return 0;

            string numberPart = mrn.Substring(6);
            return int.TryParse(numberPart, out int patientId) ? patientId : 0;
        }
        #endregion

        #region 14 - Comprehensive Model Validation
        //Validate all fields in TblUser model
        public static string ValidateUserModel(TblUser user)
        {
            var errors = new List<string>();

            if (!IsValidName(user.FullName))
                errors.Add("Full Name is required and should contain only letters, spaces, hyphens, apostrophes, and dots (2-150 characters)");

            if (!IsValidEmail(user.Email))
                errors.Add("Invalid email format");

            if (!IsValidMobileNumber(user.MobileNumber))
                errors.Add("Invalid mobile number format");

            if (!IsValidGender(user.Gender))
                errors.Add("Invalid gender. Valid options: Male, Female, Other");

            if (!IsValidBirthDate(user.DateOfBirth))
                errors.Add("Invalid birth date. Date cannot be in the future or more than 150 years ago");

            if (!IsValidJoiningDate(user.JoiningDate))
                errors.Add("Invalid joining date. Date cannot be in the future");

            if (!IsValidBloodGroup(user.BloodGroup))
                errors.Add("Invalid blood group. Valid options: A+, A-, B+, B-, AB+, AB-, O+, O-");

            if (!IsValidAddress(user.Address))
                errors.Add("Address should be 5-300 characters long");

            if (!IsValidUsername(user.UserName))
                errors.Add("Username should contain only alphanumeric characters, underscores, and dots (max 30 characters)");

            if (!isValidPassword(user.Password))
                errors.Add("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");

            if (!IsValidPositiveInteger(user.RoleId))
                errors.Add("Role ID must be a positive integer");

            return errors.Count > 0 ? string.Join("; ", errors) : string.Empty;
        }

        //Validate all fields in TblPatient model
        public static string ValidatePatientModel(TblPatient patient)
        {
            var errors = new List<string>();

            // UserId is optional for patients (can be 0 or null)
            if (patient.UserId < 0)
                errors.Add("User ID cannot be negative");

            if (!IsValidName(patient.PatientName))
                errors.Add("Patient Name is required and should contain only letters, spaces, hyphens, apostrophes, and dots (2-150 characters)");

            if (!IsValidBirthDate(patient.DateOfBirth))
                errors.Add("Invalid birth date. Date cannot be in the future or more than 150 years ago");

            if (!IsValidGender(patient.Gender))
                errors.Add("Invalid gender. Valid options: Male, Female, Other");

            if (!IsValidBloodGroup(patient.BloodGroup))
                errors.Add("Invalid blood group. Valid options: A+, A-, B+, B-, AB+, AB-, O+, O-");

            if (!IsValidAddress(patient.Address))
                errors.Add("Address should be 5-300 characters long");

            if (!IsValidMobileNumber(patient.MobileNumber))
                errors.Add("Invalid mobile number format");

            if (!IsValidPositiveInteger(patient.MembershipId))
                errors.Add("Membership ID must be a positive integer");

            return errors.Count > 0 ? string.Join("; ", errors) : string.Empty;
        }

        //Validate all fields in TblAppointment model
        public static string ValidateAppointmentModel(TblAppointment appointment)
        {
            var errors = new List<string>();

            if (!IsValidPositiveInteger(appointment.PatientId))
                errors.Add("Patient ID must be a positive integer");

            if (!IsValidPositiveInteger(appointment.DoctorId))
                errors.Add("Doctor ID must be a positive integer");

            if (!IsValidPositiveInteger(appointment.UserId))
                errors.Add("User ID must be a positive integer");

            if (!IsValidAppointmentDateTime(appointment.AppointmentDate))
                errors.Add("Appointment date and time cannot be in the past");

            if (!IsValidNonNegativeInteger(appointment.TokenNumber))
                errors.Add("Token number must be a non-negative integer");

            if (!IsValidTextLength(appointment.ConsultationStatus, 1, 50))
                errors.Add("Consultation status is required (max 50 characters)");

            string[] validStatuses = { "Pending", "Completed", "Cancelled", "No Show" };
            if (!validStatuses.Contains(appointment.ConsultationStatus))
                errors.Add("Invalid consultation status. Valid options: Pending, Completed, Cancelled, No Show");

            if (!IsValidPositiveInteger(appointment.CreatedAt))
                errors.Add("Created At must be a positive integer");

            return errors.Count > 0 ? string.Join("; ", errors) : string.Empty;
        }

        //Validate all fields in TblDoctor model
        public static string ValidateDoctorModel(TblDoctor doctor)
        {
            var errors = new List<string>();

            if (!IsValidPositiveInteger(doctor.UserId))
                errors.Add("User ID must be a positive integer");

            if (!IsValidPositiveDecimal(doctor.ConsultationFee, 0))
                errors.Add("Consultation fee must be a non-negative decimal value");

            return errors.Count > 0 ? string.Join("; ", errors) : string.Empty;
        }

        //Validate all fields in TblConsultation model
        public static string ValidateConsultationModel(TblConsultation consultation)
        {
            var errors = new List<string>();

            if (!IsValidPositiveInteger(consultation.AppointmentId))
                errors.Add("Appointment ID must be a positive integer");

            if (!IsValidPositiveInteger(consultation.PatientId))
                errors.Add("Patient ID must be a positive integer");

            if (!IsValidTextLength(consultation.Symptoms, 0, 4000))
                errors.Add("Symptoms cannot exceed 4000 characters");

            if (!IsValidTextLength(consultation.Diagnosis, 0, 4000))
                errors.Add("Diagnosis cannot exceed 4000 characters");

            if (!IsValidTextLength(consultation.Notes, 0, 4000))
                errors.Add("Notes cannot exceed 4000 characters");

            return errors.Count > 0 ? string.Join("; ", errors) : string.Empty;
        }
        #endregion
    }
}
