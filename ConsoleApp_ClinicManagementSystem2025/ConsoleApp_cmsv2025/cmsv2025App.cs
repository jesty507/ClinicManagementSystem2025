using ConsoleApp_cmsv2025.Model;
using ConsoleApp_cmsv2025.Repository;
using ConsoleApp_cmsv2025.Service;
using ConsoleApp_cmsv2025.Utility;

namespace ConsoleApp_cmsv2025
{
    internal class cmsv2025App
    {
        // 🔹 Make userService available everywhere
        private static IUserService userService = null!;

        static async Task Main(string[] args)
        {
            Console.Title = "Clinic Management System";

            // Initialize userService once
            userService = new UserServiceImpl(new UserRepository());

            //LOGIN
            while (true)
            {
                //Main menu
                Console.WriteLine("------------------------------------------------");
                Console.WriteLine(" L   O   G   I   N ");
                Console.WriteLine("------------------------------------------------");
            lbluserName:
                Console.Write("Enter Username: ");
                string? userName = Console.ReadLine();
                //Check validation
                bool isValidUserName = !string.IsNullOrEmpty(userName) && Utility.CustomValidation.IsValidUsername(userName);
                if (!isValidUserName)
                {
                    Console.WriteLine("Invalid Username");
                    goto lbluserName;
                }
            lblPassword:
                Console.Write("Enter Password: ");
                string password = Utility.CustomValidation.ReadPassword();
                bool isValidPassword = Utility.CustomValidation.isValidPassword(password);
                if (!isValidPassword)
                {
                    Console.WriteLine("Invalid Password");
                    goto lblPassword;
                }
                int roleId = await userService.AuthenticateUserByRoleIdAsync(userName!, password);

                if (roleId > 0)
                {
                    switch (roleId)
                    {
                        case 1: // Doctor
                            await ShowDoctorDashboardAsync(userName!);
                            break;

                        case 2: // Receptionist
                            await ShowReceptionistDashboardAsync();
                            break;

                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(" Access denied! Unknown role.");
                            Console.ResetColor();
                            break;
                    }

                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid credentials.");
                    Console.ResetColor();
                }

                Console.WriteLine("\nPress any key to return to login...");
                Console.ReadKey();
            }
        }
        #region Doctor Dashboard
        private static async Task ShowDoctorDashboardAsync(string userName)
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("======================================");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("         DOCTOR DASHBOARD");
                Console.ResetColor();
                Console.WriteLine("======================================");
                Console.WriteLine("1. View Appointments");
                Console.WriteLine("2. View Patient History");
                Console.WriteLine("3. Prescribe Medicines");
                Console.WriteLine("4. Prescribe Lab Tests");
                Console.WriteLine("5. Exit");
                Console.Write("Enter your choice: ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        //  Fetch DoctorId from username
                        int doctorId = await userService.GetDoctorIdByUserNameAsync(userName);

                        if (doctorId == 0)
                        {
                            Console.WriteLine("No doctor record found for this login.");
                        }
                        else
                        {
                            var appointments = await userService.GetDoctorAppointmentsForTodayAsync(doctorId);
                            if (appointments.Count == 0)
                            {
                                Console.WriteLine("No appointments found for today.");
                            }
                            else
                            {
                                Console.WriteLine("\nToday's Appointments:");
                                foreach (var appt in appointments)
                                {
                                    Console.WriteLine($"[{appt.AppointmentId}] {appt.AppointmentDate:t} - Token {appt.TokenNumber} - Patient {appt.PatientId} - Status: {appt.ConsultationStatus}");
                                }
                            }
                        }
                        break;
                    case "2":
                        await AddConsultationNotesAsync(userName);
                        break;
                    case "3":
                        await PrescribeMedicinesAsync(userName);
                        break;
                    case "4":
                        await PrescribeLabTestsAsync(userName);
                        break;
                    case "5":
                        return; // Logout
                    default:
                        Console.WriteLine("Invalid choice. Try again.");
                        break;
                }
            }
        }
        #endregion

        #region Receptionist Dashboard
        private static async Task ShowReceptionistDashboardAsync()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("======================================");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("      RECEPTIONIST DASHBOARD");
                Console.ResetColor();
                Console.WriteLine("======================================");
                Console.WriteLine("1. Search by MRN Number");
                Console.WriteLine("2. Search by Mobile Number");
                Console.WriteLine("3. Add New Patient");
                Console.WriteLine("4. View All Patients");
                Console.WriteLine("5. View all Appointments");
                Console.WriteLine("6. View Consultation Bills");
                Console.WriteLine("7. Exit");
                Console.Write("Enter your choice: ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await HandleMRNSearchAsync();
                        break;
                    case "2":
                        await HandleMobileSearchAsync();
                        break;
                    case "3":
                        await AddNewPatientFromReceptionistAsync();
                        break;
                    case "4":
                        await ViewAllPatientsAsync();
                        break;
                    case "5":
                        await ViewAppointmentsAsync();
                        break;
                    case "6":
                        await GenerateConsultationBillAsync();
                        break;
                    case "7":
                        return; // Logout
                    default:
                        Console.WriteLine("Invalid choice. Try again.");
                        break;
                }
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private static async Task HandleMRNSearchAsync()
        {
            Console.Write("Enter MRN Number (format: MRN000123): ");
            string? mrnInput = Console.ReadLine();
            
            if (string.IsNullOrEmpty(mrnInput))
            {
                Console.WriteLine("Invalid MRN entered.");
                return;
            }

            // Validate MRN format (MRN000123)
            if (!CustomValidation.IsValidMRN(mrnInput))
            {
                Console.WriteLine("Invalid MRN format. Please use format: MRN000123");
                return;
            }

            // Extract patient ID from MRN
            int patientId = CustomValidation.ExtractPatientIdFromMRN(mrnInput);
            
            // First check if MRN exists
            bool mrnExists = await userService.CheckMRNNumberExistsAsync(patientId);
            
            if (mrnExists)
            {
                var patient = await userService.SearchPatientByMRNAsync(patientId);
                if (patient != null)
                {
                    PrintPatient(patient);
                    await OfferAppointmentCreationAsync(patient);
                }
            }
            else
            {
                Console.WriteLine("No patient found with this MRN.");
                Console.Write("Do you want to create a new patient? (y/n): ");
                string? createChoice = Console.ReadLine()?.ToLower();
                if (createChoice == "y" || createChoice == "yes")
                {
                    await CreateNewPatientAsync();
                }
            }
        }

        private static async Task HandleMobileSearchAsync()
        {
            Console.Write("Enter Mobile Number: ");
            string? mobileNumber = Console.ReadLine();
            var patientByMobile = await userService.SearchPatientByMobileAsync(mobileNumber!);
            if (patientByMobile != null)
            {
                PrintPatient(patientByMobile);
                await OfferAppointmentCreationAsync(patientByMobile);
            }
            else
            {
                Console.WriteLine("No patient found with this mobile number.");
                Console.Write("Do you want to create a new patient? (y/n): ");
                string createChoice = Console.ReadLine()?.ToLower() ?? string.Empty;
                if (createChoice == "y" || createChoice == "yes")
                {
                    await CreateNewPatientAsync(mobileNumber!);
                }
            }
        }

        private static async Task CreateNewPatientAsync(string? mobileNumber = null)
        {
            Console.WriteLine("\n=== CREATE NEW PATIENT ===");
            Console.WriteLine("Note: MRN number will be automatically assigned by the system.");
            
            TblPatient newPatient = new TblPatient();
            
            // Get patient name
            Console.Write("Enter Patient Name: ");
            newPatient.PatientName = Console.ReadLine() ?? string.Empty;
            
            // Get date of birth
            DateTime dob;
            do
            {
                Console.Write("Enter Date of Birth (yyyy-mm-dd): ");
                string? dobInput = Console.ReadLine();
                if (!DateTime.TryParse(dobInput, out dob))
                {
                    Console.WriteLine("Invalid date format. Please try again.");
                }
            } while (dob == default(DateTime));
            newPatient.DateOfBirth = dob;
            
            // Get gender
            Console.Write("Enter Gender (M/F): ");
            newPatient.Gender = Console.ReadLine()?.ToUpper() ?? string.Empty;
            
            // Get mobile number (if not provided from search)
            if (string.IsNullOrEmpty(mobileNumber))
            {
                Console.Write("Enter Mobile Number: ");
                mobileNumber = Console.ReadLine() ?? string.Empty;
            }
            else
            {
                Console.WriteLine($"Mobile Number: {mobileNumber} (from search)");
            }
            newPatient.MobileNumber = mobileNumber;
            
            // Check if mobile number already exists
            bool mobileExists = await userService.CheckMobileNumberExistsAsync(mobileNumber);
            if (mobileExists)
            {
                Console.WriteLine(" Warning: A patient with this mobile number already exists!");
                Console.Write("Do you want to continue anyway? (y/n): ");
                string? continueChoice = Console.ReadLine()?.ToLower();
                if (continueChoice != "y" && continueChoice != "yes")
                {
                    Console.WriteLine("Patient creation cancelled.");
                    return;
                }
            }
            
            // Get address (optional)
            Console.Write("Enter Address (optional): ");
            newPatient.Address = Console.ReadLine();
            
            // Get blood group (optional)
            Console.Write("Enter Blood Group (optional): ");
            newPatient.BloodGroup = Console.ReadLine();
            
            // Get membership ID (default to 1 if not specified)
            int membershipId;
            do
            {
                Console.Write("Enter Membership ID (default 1): ");
                string? membershipInput = Console.ReadLine();
                if (string.IsNullOrEmpty(membershipInput))
                {
                    membershipId = 1;
                    break;
                }
                if (int.TryParse(membershipInput, out membershipId))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid Membership ID format. Please enter a valid number.");
                }
            } while (true);
            newPatient.MembershipId = membershipId;
            
            // Set UserId to null since patients don't need user accounts
            newPatient.UserId = 0; // This will be converted to NULL in the stored procedure
            
            // Set created date
            newPatient.CreatedDate = DateTime.Now;
            newPatient.IsActive = true;
            
            try
            {
                int newPatientId = await userService.CreateNewPatientAsync(newPatient);
                Console.WriteLine($"\n Patient created successfully!");
                Console.WriteLine($"New MRN Number: MRN000{newPatientId}");
                
                // Display the created patient
                newPatient.PatientId = newPatientId;
                PrintPatient(newPatient);
                
                // Offer to create appointment for the new patient
                await OfferAppointmentCreationAsync(newPatient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error creating patient: {ex.Message}");
            }
        }

        private static async Task AddNewPatientFromReceptionistAsync()
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("         ADD NEW PATIENT");
            Console.ResetColor();
            Console.WriteLine("======================================");
            Console.WriteLine("Note: MRN number will be automatically assigned by the system.");
            Console.WriteLine("All fields will be validated in real-time.\n");
            
            TblPatient newPatient = new TblPatient();
            
            // Get patient name with real-time validation
            do
            {
                Console.Write("Enter Patient Name: ");
                string? patientName = Console.ReadLine();
                if (string.IsNullOrEmpty(patientName))
                {
                    Console.WriteLine("❌ Patient name is required.");
                    continue;
                }
                if (!CustomValidation.IsValidName(patientName))
                {
                    Console.WriteLine("❌ Patient name should contain only letters, spaces, hyphens, apostrophes, and dots (2-150 characters).");
                    continue;
                }
                newPatient.PatientName = patientName;
                break;
            } while (true);
            
            // Get date of birth with real-time validation
            do
            {
                Console.Write("Enter Date of Birth (yyyy-mm-dd): ");
                string? dobInput = Console.ReadLine();
                if (string.IsNullOrEmpty(dobInput))
                {
                    Console.WriteLine("❌ Date of birth is required.");
                    continue;
                }
                if (!DateTime.TryParse(dobInput, out DateTime dob))
                {
                    Console.WriteLine("❌ Invalid date format. Please use yyyy-mm-dd format.");
                    continue;
                }
                if (!CustomValidation.IsValidBirthDate(dob))
                {
                    Console.WriteLine("❌ Invalid birth date. Date cannot be in the future or more than 150 years ago.");
                    continue;
                }
                newPatient.DateOfBirth = dob;
                break;
            } while (true);
            
            // Get gender with real-time validation
            do
            {
                Console.Write("Enter Gender (M/F): ");
                string? gender = Console.ReadLine()?.ToUpper();
                if (string.IsNullOrEmpty(gender))
                {
                    Console.WriteLine("❌ Gender is required.");
                    continue;
                }
                if (!CustomValidation.IsValidGender(gender))
                {
                    Console.WriteLine("❌ Invalid gender. Valid options: Male, Female, Other, M, F, O");
                    continue;
                }
                newPatient.Gender = gender;
                break;
            } while (true);
            
            // Get mobile number with real-time validation
            do
            {
                Console.Write("Enter Mobile Number: ");
                string? mobileNumber = Console.ReadLine();
                if (string.IsNullOrEmpty(mobileNumber))
                {
                    Console.WriteLine("❌ Mobile number is required.");
                    continue;
                }
                if (!CustomValidation.IsValidMobileNumber(mobileNumber))
                {
                    Console.WriteLine("❌ Invalid mobile number format. Please enter a valid Indian mobile number.");
                    continue;
                }
                
                // Check if mobile number already exists
                bool mobileExists = await userService.CheckMobileNumberExistsAsync(mobileNumber);
                if (mobileExists)
                {
                    Console.WriteLine("⚠️ Warning: A patient with this mobile number already exists!");
                    Console.Write("Do you want to continue anyway? (y/n): ");
                    string? continueChoice = Console.ReadLine()?.ToLower();
                    if (continueChoice != "y" && continueChoice != "yes")
                    {
                        continue;
                    }
                }
                newPatient.MobileNumber = mobileNumber;
                break;
            } while (true);
            
            // Get address with real-time validation (optional)
            do
            {
                Console.Write("Enter Address (optional): ");
                string? address = Console.ReadLine();
                if (!string.IsNullOrEmpty(address) && !CustomValidation.IsValidAddress(address))
                {
                    Console.WriteLine("❌ Invalid address format.");
                    continue;
                }
                newPatient.Address = address;
                break;
            } while (true);
            
            // Get blood group with real-time validation (optional)
            do
            {
                Console.Write("Enter Blood Group (optional): ");
                string? bloodGroup = Console.ReadLine();
                if (!string.IsNullOrEmpty(bloodGroup) && !CustomValidation.IsValidBloodGroup(bloodGroup))
                {
                    Console.WriteLine("❌ Invalid blood group. Valid options: A+, A-, B+, B-, AB+, AB-, O+, O-");
                    continue;
                }
                newPatient.BloodGroup = bloodGroup;
                break;
            } while (true);
            
            // Get membership ID with real-time validation
            do
            {
                Console.Write("Enter Membership ID (default 1): ");
                string? membershipInput = Console.ReadLine();
                int membershipId;
                if (string.IsNullOrEmpty(membershipInput))
                {
                    membershipId = 1;
                }
                else if (!int.TryParse(membershipInput, out membershipId) || membershipId <= 0)
                {
                    Console.WriteLine("❌ Invalid membership ID. Please enter a positive number.");
                    continue;
                }
                newPatient.MembershipId = membershipId;
                break;
            } while (true);
            
            // Set other required fields
            newPatient.UserId = 0; // Patients don't need user accounts
            newPatient.CreatedDate = DateTime.Now;
            newPatient.IsActive = true;
            
            // All validations passed, create the patient
            try
            {
                Console.WriteLine("\n" + new string('=', 50));
                Console.WriteLine("CREATING PATIENT...");
                Console.WriteLine(new string('=', 50));
                
                int newPatientId = await userService.CreateNewPatientAsync(newPatient);
                Console.WriteLine($"\n🎉 Patient created successfully!");
                Console.WriteLine($"New MRN Number: MRN000{newPatientId}");
                
                // Display the created patient
                newPatient.PatientId = newPatientId;
                PrintPatient(newPatient);
                
                // Offer to create appointment for the new patient
                Console.WriteLine("\n" + new string('=', 50));
                Console.Write("Do you want to create an appointment for this patient? (y/n): ");
                string? appointmentChoice = Console.ReadLine()?.ToLower();
                if (appointmentChoice == "y" || appointmentChoice == "yes")
                {
                    await CreateAppointmentForPatientAsync(newPatient);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error creating patient: {ex.Message}");
            }
        }

        private static async Task ViewAllPatientsAsync()
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("         VIEW ALL PATIENTS");
            Console.ResetColor();
            Console.WriteLine("======================================");

            try
            {
                var patients = await userService.GetAllPatientsAsync();
                
                if (patients.Count == 0)
                {
                    Console.WriteLine("No patients found in the system.");
                    return;
                }

                Console.WriteLine($"\nTotal Patients: {patients.Count}");
                Console.WriteLine(new string('=', 120));
                Console.WriteLine($"{"MRN",-10} {"Name",-25} {"DOB",-12} {"Gender",-8} {"Mobile",-15} {"Blood Group",-12} {"Membership",-10}");
                Console.WriteLine(new string('=', 120));

                foreach (var patient in patients)
                {
                    string mrn = $"MRN000{patient.PatientId}";
                    string name = patient.PatientName ?? "N/A";
                    string dob = patient.DateOfBirth?.ToString("yyyy-MM-dd") ?? "N/A";
                    string gender = patient.Gender ?? "N/A";
                    string mobile = patient.MobileNumber ?? "N/A";
                    string bloodGroup = patient.BloodGroup ?? "N/A";
                    string membership = patient.MembershipId.ToString();

                    Console.WriteLine($"{mrn,-10} {name,-25} {dob,-12} {gender,-8} {mobile,-15} {bloodGroup,-12} {membership,-10}");
                }

                Console.WriteLine(new string('=', 120));

                // Option to view detailed patient information
                Console.WriteLine("\n" + new string('-', 60));
                Console.Write("Enter MRN number (without MRN000 prefix) to view detailed information, or press Enter to return: ");
                string? input = Console.ReadLine();
                
                if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int patientId))
                {
                    var selectedPatient = patients.FirstOrDefault(p => p.PatientId == patientId);
                    if (selectedPatient != null)
                    {
                        Console.Clear();
                        Console.WriteLine("======================================");
                        Console.WriteLine("      PATIENT DETAILS");
                        Console.WriteLine("======================================");
                        PrintPatient(selectedPatient);
                        
                        Console.WriteLine("\n" + new string('=', 50));
                        Console.Write("Do you want to create an appointment for this patient? (y/n): ");
                        string? appointmentChoice = Console.ReadLine()?.ToLower();
                        if (appointmentChoice == "y" || appointmentChoice == "yes")
                        {
                            await CreateAppointmentForPatientAsync(selectedPatient);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Patient not found with the specified MRN.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving patients: {ex.Message}");
            }
        }

        private static async Task OfferAppointmentCreationAsync(TblPatient patient)
        {
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("APPOINTMENT CREATION");
            Console.WriteLine(new string('=', 50));
            Console.Write("Do you want to create an appointment for this patient? (y/n): ");
            string? appointmentChoice = Console.ReadLine()?.ToLower();
            
            if (appointmentChoice == "y" || appointmentChoice == "yes")
            {
                await CreateAppointmentForPatientAsync(patient);
            }
        }

        private static async Task CreateAppointmentForPatientAsync(TblPatient patient)
        {
            Console.WriteLine("\n=== CREATE APPOINTMENT ===");
            
            try
            {
                // Get all available doctors
                var doctors = await userService.GetAllDoctorsAsync();
                if (doctors.Count == 0)
                {
                    Console.WriteLine(" No doctors available. Cannot create appointment.");
                    return;
                }

                // Display available doctors
                Console.WriteLine("\nAvailable Doctors:");
                Console.WriteLine(new string('-', 80));
                foreach (var doctor in doctors)
                {
                    Console.WriteLine($"{doctor.DoctorId}. {doctor.DoctorName} - {doctor.SpecializationNames} (Fee: ${doctor.ConsultationFee:F2})");
                }
                Console.WriteLine(new string('-', 80));

                // Get doctor selection
                int selectedDoctorId;
                do
                {
                    Console.Write("Select Doctor ID: ");
                    if (!int.TryParse(Console.ReadLine(), out selectedDoctorId) || 
                        !doctors.Any(d => d.DoctorId == selectedDoctorId))
                    {
                        Console.WriteLine("Invalid doctor ID. Please try again.");
                    }
                } while (!doctors.Any(d => d.DoctorId == selectedDoctorId));

                // Get appointment date and time with validation
                DateTime appointmentDate = DateTime.Now;
                bool validAppointment = false;
                
                while (!validAppointment)
                {
                    Console.Write("Enter Appointment Date (yyyy-mm-dd): ");
                    string? dateInput = Console.ReadLine();
                    if (!DateTime.TryParse(dateInput, out appointmentDate) || appointmentDate < DateTime.Today)
                    {
                        Console.WriteLine("Invalid date. Please enter a valid future date.");
                        continue;
                    }

                    // Get appointment time
                    Console.Write("Enter Appointment Time (HH:mm): ");
                    string? timeInput = Console.ReadLine();
                    if (TimeSpan.TryParse(timeInput, out TimeSpan appointmentTime))
                    {
                        appointmentDate = appointmentDate.Date.Add(appointmentTime);
                    }
                    else
                    {
                        Console.WriteLine("Invalid time format. Please try again.");
                        continue;
                    }

                    // Validate that the appointment date and time is not in the past
                    if (appointmentDate < DateTime.Now)
                    {
                        Console.WriteLine("Appointment date and time cannot be in the past. Please enter a future date and time.");
                        continue;
                    }

                    // Check if time slot is already booked for the same doctor
                    var existingAppointments = await userService.GetDoctorAppointmentsForDateAsync(selectedDoctorId, appointmentDate.Date);
                    bool isSlotBooked = existingAppointments.Any(a => 
                        a.IsActive && 
                        a.ConsultationStatus != "Cancelled" &&
                        a.ConsultationStatus != "No Show" &&
                        Math.Abs((a.AppointmentDate - appointmentDate).TotalMinutes) < 30);

                    if (isSlotBooked)
                    {
                        Console.WriteLine($"\n❌ Time slot {appointmentDate:yyyy-MM-dd HH:mm} is already booked for this doctor.");
                        Console.WriteLine("Please choose a different date and time.");
                        
                        // Show available time slots for the same day
                        Console.WriteLine("\nAvailable time slots for the same day:");
                        var bookedTimes = existingAppointments
                            .Where(a => a.IsActive && a.ConsultationStatus != "Cancelled" && a.ConsultationStatus != "No Show")
                            .Select(a => a.AppointmentDate.TimeOfDay)
                            .OrderBy(t => t)
                            .ToList();
                        
                        // Show booked times
                        if (bookedTimes.Any())
                        {
                            Console.WriteLine("Booked times:");
                            foreach (var time in bookedTimes)
                            {
                                Console.WriteLine($"  - {time:HH:mm}");
                            }
                        }
                        
                        Console.WriteLine("\nPlease try again with a different time.");
                        continue; // Go back to date/time input
                    }

                    validAppointment = true; // Valid appointment date and time entered
                }

                // Get next token number for the doctor on this date
                int tokenNumber = await userService.GetNextTokenNumberAsync(selectedDoctorId, appointmentDate);
                if (tokenNumber < 1 || tokenNumber > 30)
                {
                    Console.WriteLine(" Doctor is fully booked for the selected date (30 tokens max).");
                    return;
                }

                // Automatically set consultation status as "Pending" for newly created appointments
                string consultationStatus = "Pending";
                Console.WriteLine($"\nConsultation Status: {consultationStatus} (automatically set - newly created appointment)");

                // Get UserId (for now, we'll use 1 as default - you might want to get this from the logged-in user)
                int userId;
                do
                {
                    Console.Write("Enter User ID (default 1): ");
                    string? userIdInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(userIdInput))
                    {
                        userId = 1;
                        break;
                    }
                    if (int.TryParse(userIdInput, out userId))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid User ID format. Please enter a valid number.");
                    }
                } while (true);

                // Create appointment object
                TblAppointment newAppointment = new TblAppointment
                {
                    AppointmentDate = appointmentDate,
                    TokenNumber = tokenNumber,
                    ConsultationStatus = consultationStatus,
                    PatientId = patient.PatientId,
                    DoctorId = selectedDoctorId,
                    UserId = userId,
                    CreatedAt = userId, // Set CreatedBy to the same user who is creating the appointment
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                // Create appointment
                int appointmentId = await userService.CreateAppointmentAsync(newAppointment);

                Console.WriteLine($"\n Appointment created successfully!");
                Console.WriteLine($"Appointment ID: A{appointmentId}");
                Console.WriteLine($"Patient: {patient.PatientName} (MRN: MRN000{patient.PatientId})");
                Console.WriteLine($"Doctor ID: {selectedDoctorId}");
                Console.WriteLine($"Date & Time: {appointmentDate:yyyy-MM-dd HH:mm}");
                Console.WriteLine($"Token Number: {tokenNumber}");
                Console.WriteLine($"Status: {consultationStatus}");

                // Get doctor's consultation fee and create consultation bill immediately
                var allDoctors = await userService.GetAllDoctorsAsync();
                var selectedDoctor = allDoctors.FirstOrDefault(d => d.DoctorId == selectedDoctorId);
                if (selectedDoctor != null)
                {
                    decimal consultationFee = selectedDoctor.ConsultationFee;
                    
                    // Create consultation bill
                    var consultationBill = new TblConsultationBill
                    {
                        AppointmentId = appointmentId,
                        PatientId = patient.PatientId,
                        ConsultationFee = consultationFee,
                        LabCharges = 0, // Will be updated later if lab tests are prescribed
                        MedicineCharges = 0, // Will be updated later if medicines are prescribed
                        TotalAmount = consultationFee,
                        PaymentStatus = "Unpaid",
                        CreatedDate = DateTime.Now
                    };

                    try
                    {
                        int billId = await userService.CreateConsultationBillAsync(consultationBill);
                        Console.WriteLine($"\n💰 Consultation Bill Generated:");
                        Console.WriteLine($"Bill ID: B{billId}");
                        Console.WriteLine($"Consultation Fee: ${consultationFee:F2}");
                        Console.WriteLine($"Total Amount: ${consultationFee:F2}");
                        Console.WriteLine($"Payment Status: Unpaid");
                        Console.WriteLine($"Note: Lab and medicine charges will be added after consultation.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Appointment created but bill generation failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error creating appointment: {ex.Message}");
            }
        }

        #region Prescribe Medicines
        private static async Task PrescribeMedicinesAsync(string userName)
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("         PRESCRIBE MEDICINES");
            Console.ResetColor();
            Console.WriteLine("======================================");

            try
            {
                // Get doctor ID
                int doctorId = await userService.GetDoctorIdByUserNameAsync(userName);
                if (doctorId == 0)
                {
                    Console.WriteLine("No doctor record found for this login.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // Get doctor's appointments for today
                var appointments = await userService.GetDoctorAppointmentsForDateAsync(doctorId, DateTime.Today);
                if (appointments.Count == 0)
                {
                    Console.WriteLine("No appointments found for today.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // Display appointments
                Console.WriteLine("\nToday's Appointments:");
                Console.WriteLine(new string('-', 80));
                foreach (var appt in appointments)
                {
                    Console.WriteLine($"[A{appt.AppointmentId}] {appt.AppointmentDate:HH:mm} - Token {appt.TokenNumber} - Patient ID: {appt.PatientId} - Status: {appt.ConsultationStatus}");
                }
                Console.WriteLine(new string('-', 80));

                // Select appointment
                int selectedAppointmentId;
                do
                {
                    Console.Write("Enter Appointment ID to prescribe medicines: ");
                    if (!int.TryParse(Console.ReadLine(), out selectedAppointmentId) || 
                        !appointments.Any(a => a.AppointmentId == selectedAppointmentId))
                    {
                        Console.WriteLine("Invalid appointment ID. Please try again.");
                    }
                } while (!appointments.Any(a => a.AppointmentId == selectedAppointmentId));

                // Get medicines
                var medicines = await userService.GetAllMedicinesAsync();
                if (medicines.Count == 0)
                {
                    Console.WriteLine("No medicines available in the system.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // Display medicines
                Console.WriteLine("\nAvailable Medicines:");
                Console.WriteLine(new string('-', 100));
                foreach (var medicine in medicines)
                {
                    Console.WriteLine($"{medicine.MedicineId}. {medicine.MedicineName} - Price: ${medicine.Price:F2} - Available: {medicine.UnitAvailable}");
                }
                Console.WriteLine(new string('-', 100));

                // Prescribe medicines
                bool continuePrescribing = true;
                while (continuePrescribing)
                {
                    Console.WriteLine("\nPrescribe Medicine:");
                    
                    // Select medicine
                    int selectedMedicineId;
                    do
                    {
                        Console.Write("Enter Medicine ID: ");
                        if (!int.TryParse(Console.ReadLine(), out selectedMedicineId) || 
                            !medicines.Any(m => m.MedicineId == selectedMedicineId))
                        {
                            Console.WriteLine("Invalid medicine ID. Please try again.");
                        }
                    } while (!medicines.Any(m => m.MedicineId == selectedMedicineId));

                    // Get prescription details
                    Console.Write("Enter Dosage (e.g., 1 tablet, 5ml): ");
                    string dosage = Console.ReadLine();

                    Console.Write("Enter Duration (e.g., 7 days, 2 weeks): ");
                    string duration = Console.ReadLine();

                    Console.Write("Enter Instructions (optional): ");
                    string instructions = Console.ReadLine();

                    // Create prescription
                    var prescription = new TblPrescriptionMedicine
                    {
                        MedicineId = selectedMedicineId,
                        AppointmentId = selectedAppointmentId,
                        Dosage = dosage,
                        Duration = duration,
                        Instructions = instructions
                    };

                    try
                    {
                        int prescriptionId = await userService.CreatePrescriptionMedicineAsync(prescription);
                        Console.WriteLine($"\n Medicine prescribed successfully! Prescription ID: {prescriptionId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" Error prescribing medicine: {ex.Message}");
                    }

                    // Ask if doctor wants to prescribe more medicines
                    Console.Write("\nDo you want to prescribe another medicine? (y/n): ");
                    string continueChoice = Console.ReadLine()?.ToLower();
                    continuePrescribing = (continueChoice == "y" || continueChoice == "yes");
                }

                // Display all prescriptions for this appointment
                var prescriptions = await userService.GetPrescriptionsByAppointmentIdAsync(selectedAppointmentId);
                if (prescriptions.Count > 0)
                {
                    Console.WriteLine($"\nAll Prescriptions for Appointment A{selectedAppointmentId}:");
                    Console.WriteLine(new string('-', 100));
                    foreach (var prescription in prescriptions)
                    {
                        var medicine = medicines.FirstOrDefault(m => m.MedicineId == prescription.MedicineId);
                        Console.WriteLine($"- {medicine?.MedicineName ?? "Unknown Medicine"}");
                        Console.WriteLine($"  Dosage: {prescription.Dosage}");
                        Console.WriteLine($"  Duration: {prescription.Duration}");
                        if (!string.IsNullOrEmpty(prescription.Instructions))
                        {
                            Console.WriteLine($"  Instructions: {prescription.Instructions}");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine(new string('-', 100));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error in prescription process: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
        #endregion

        #region Add Consultation Notes
        private static async Task AddConsultationNotesAsync(string userName)
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("         ADD CONSULTATION NOTES");
            Console.ResetColor();
            Console.WriteLine("======================================");

            try
            {
                // Get doctor ID
                int doctorId = await userService.GetDoctorIdByUserNameAsync(userName);
                if (doctorId == 0)
                {
                    Console.WriteLine("No doctor record found for this login.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // Get doctor's appointments for today
                var appointments = await userService.GetDoctorAppointmentsForDateAsync(doctorId, DateTime.Today);
                if (appointments.Count == 0)
                {
                    Console.WriteLine("No appointments found for today.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // Display appointments
                Console.WriteLine("\nToday's Appointments:");
                Console.WriteLine(new string('-', 80));
                foreach (var appt in appointments)
                {
                    Console.WriteLine($"[A{appt.AppointmentId}] {appt.AppointmentDate:HH:mm} - Token {appt.TokenNumber} - Patient ID: {appt.PatientId} - Status: {appt.ConsultationStatus}");
                }
                Console.WriteLine(new string('-', 80));

                // Select appointment
                int selectedAppointmentId;
                do
                {
                    Console.Write("Enter Appointment ID to add consultation notes: ");
                    if (!int.TryParse(Console.ReadLine(), out selectedAppointmentId) || 
                        !appointments.Any(a => a.AppointmentId == selectedAppointmentId))
                    {
                        Console.WriteLine("Invalid appointment ID. Please try again.");
                    }
                } while (!appointments.Any(a => a.AppointmentId == selectedAppointmentId));

                // Check if consultation already exists
                var existingConsultation = await userService.GetConsultationByAppointmentIdAsync(selectedAppointmentId);
                if (existingConsultation != null)
                {
                    Console.WriteLine("\nConsultation notes already exist for this appointment:");
                    Console.WriteLine(new string('-', 60));
                    Console.WriteLine($"Symptoms: {existingConsultation.Symptoms ?? "Not specified"}");
                    Console.WriteLine($"Diagnosis: {existingConsultation.Diagnosis ?? "Not specified"}");
                    Console.WriteLine($"Notes: {existingConsultation.Notes ?? "Not specified"}");
                    Console.WriteLine(new string('-', 60));
                    
                    Console.Write("Do you want to update the consultation notes? (y/n): ");
                    string updateChoice = Console.ReadLine()?.ToLower();
                    if (updateChoice != "y" && updateChoice != "yes")
                    {
                        Console.WriteLine("Consultation notes not updated.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        return;
                    }
                }

                // Get consultation details
                Console.WriteLine("\nEnter Consultation Details:");
                Console.WriteLine(new string('-', 40));
                
                Console.Write("Enter Symptoms: ");
                string symptoms = Console.ReadLine();
                
                Console.Write("Enter Diagnosis: ");
                string diagnosis = Console.ReadLine();
                
                Console.Write("Enter Additional Notes (optional): ");
                string notes = Console.ReadLine();

                // Create consultation
                var consultation = new TblConsultation
                {
                    Symptoms = symptoms,
                    Diagnosis = diagnosis,
                    Notes = notes,
                    AppointmentId = selectedAppointmentId,
                    PatientId = appointments.First(a => a.AppointmentId == selectedAppointmentId).PatientId,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                };

                try
                {
                    int consultationId = await userService.CreateConsultationAsync(consultation);
                    Console.WriteLine($"\n Consultation notes added successfully! Consultation ID: {consultationId}");
                    
                    // Update appointment status to "Completed"
                    bool statusUpdated = await userService.UpdateAppointmentStatusAsync(selectedAppointmentId, "Completed");
                    if (statusUpdated)
                    {
                        Console.WriteLine($" ✅ Appointment status updated to 'Completed'");
                    }
                    else
                    {
                        Console.WriteLine($" ⚠️ Consultation created but status update failed");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Error adding consultation notes: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error in consultation process: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
        #endregion

        #region Prescribe Lab Tests
        private static async Task PrescribeLabTestsAsync(string userName)
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("         PRESCRIBE LAB TESTS");
            Console.ResetColor();
            Console.WriteLine("======================================");

            try
            {
                // Get doctor ID
                int doctorId = await userService.GetDoctorIdByUserNameAsync(userName);
                if (doctorId == 0)
                {
                    Console.WriteLine("No doctor record found for this login.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // Get doctor's appointments for today
                var appointments = await userService.GetDoctorAppointmentsForDateAsync(doctorId, DateTime.Today);
                if (appointments.Count == 0)
                {
                    Console.WriteLine("No appointments found for today.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // Display appointments
                Console.WriteLine("\nToday's Appointments:");
                Console.WriteLine(new string('-', 80));
                foreach (var appt in appointments)
                {
                    Console.WriteLine($"[A{appt.AppointmentId}] {appt.AppointmentDate:HH:mm} - Token {appt.TokenNumber} - Patient ID: {appt.PatientId} - Status: {appt.ConsultationStatus}");
                }
                Console.WriteLine(new string('-', 80));

                // Select appointment
                int selectedAppointmentId;
                do
                {
                    Console.Write("Enter Appointment ID to prescribe lab tests: ");
                    if (!int.TryParse(Console.ReadLine(), out selectedAppointmentId) || 
                        !appointments.Any(a => a.AppointmentId == selectedAppointmentId))
                    {
                        Console.WriteLine("Invalid appointment ID. Please try again.");
                    }
                } while (!appointments.Any(a => a.AppointmentId == selectedAppointmentId));

                // Get lab tests
                var labTests = await userService.GetAllLabTestsAsync();
                if (labTests.Count == 0)
                {
                    Console.WriteLine("No lab tests available in the system.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                // Display lab tests
                Console.WriteLine("\nAvailable Lab Tests:");
                Console.WriteLine(new string('-', 100));
                foreach (var test in labTests)
                {
                    Console.WriteLine($"{test.LabTestId}. {test.TestName} - Price: ${test.Amount:F2} - Sample: {test.SampleRequired ?? "Not specified"}");
                }
                Console.WriteLine(new string('-', 100));

                // Prescribe lab tests
                bool continuePrescribing = true;
                while (continuePrescribing)
                {
                    Console.WriteLine("\nPrescribe Lab Test:");
                    
                    // Select lab test
                    int selectedLabTestId;
                    do
                    {
                        Console.Write("Enter Lab Test ID: ");
                        if (!int.TryParse(Console.ReadLine(), out selectedLabTestId) || 
                            !labTests.Any(t => t.LabTestId == selectedLabTestId))
                        {
                            Console.WriteLine("Invalid lab test ID. Please try again.");
                        }
                    } while (!labTests.Any(t => t.LabTestId == selectedLabTestId));

                    // Get prescription details
                    Console.Write("Enter Notes (optional): ");
                    string notes = Console.ReadLine();

                    // Create lab test prescription
                    var prescription = new TblLabTestPrescription
                    {
                        LabTestId = selectedLabTestId,
                        AppointmentId = selectedAppointmentId,
                        DoctorId = doctorId,
                        PatientId = appointments.First(a => a.AppointmentId == selectedAppointmentId).PatientId,
                        Notes = notes,
                        CreatedDate = DateTime.Now
                    };

                    try
                    {
                        int prescriptionId = await userService.CreateLabTestPrescriptionAsync(prescription);
                        Console.WriteLine($"\n Lab test prescribed successfully! Prescription ID: {prescriptionId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" Error prescribing lab test: {ex.Message}");
                    }

                    // Ask if doctor wants to prescribe more lab tests
                    Console.Write("\nDo you want to prescribe another lab test? (y/n): ");
                    string continueChoice = Console.ReadLine()?.ToLower();
                    continuePrescribing = (continueChoice == "y" || continueChoice == "yes");
                }

                // Display all lab test prescriptions for this appointment
                var prescriptions = await userService.GetLabTestPrescriptionsByAppointmentIdAsync(selectedAppointmentId);
                if (prescriptions.Count > 0)
                {
                    Console.WriteLine($"\nAll Lab Test Prescriptions for Appointment A{selectedAppointmentId}:");
                    Console.WriteLine(new string('-', 100));
                    foreach (var prescription in prescriptions)
                    {
                        var test = labTests.FirstOrDefault(t => t.LabTestId == prescription.LabTestId);
                        Console.WriteLine($"- {test?.TestName ?? "Unknown Test"}");
                        Console.WriteLine($"  Price: ${test?.Amount:F2 ?? 0:F2}");
                        if (!string.IsNullOrEmpty(prescription.Notes))
                        {
                            Console.WriteLine($"  Notes: {prescription.Notes}");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine(new string('-', 100));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error in lab test prescription process: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
        #endregion

        #region View Appointments
        private static async Task ViewAppointmentsAsync()
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("         VIEW APPOINTMENTS");
            Console.ResetColor();
            Console.WriteLine("======================================");

            try
            {
                Console.WriteLine("1. View Today's Appointments");
                Console.WriteLine("2. View Appointments for Specific Date");
                Console.WriteLine("3. View All Appointments");
                Console.Write("Enter your choice (1-3): ");
                string viewChoice = Console.ReadLine();

                List<TblAppointment> appointments = new List<TblAppointment>();

                switch (viewChoice)
                {
                    case "1":
                        appointments = await userService.GetAppointmentsByDateAsync(DateTime.Today);
                        break;
                    case "2":
                        DateTime selectedDate;
                        do
                        {
                            Console.Write("Enter Date (yyyy-mm-dd): ");
                            string dateInput = Console.ReadLine();
                            if (!DateTime.TryParse(dateInput, out selectedDate))
                            {
                                Console.WriteLine("Invalid date format. Please try again.");
                            }
                        } while (selectedDate == DateTime.MinValue);
                        appointments = await userService.GetAppointmentsByDateAsync(selectedDate);
                        break;
                    case "3":
                        appointments = await userService.GetAllAppointmentsAsync();
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        return;
                }

                if (appointments.Count == 0)
                {
                    Console.WriteLine("No appointments found.");
                }
                else
                {
                    Console.WriteLine($"\nAppointments ({appointments.Count} found):");
                    Console.WriteLine(new string('-', 100));
                    Console.WriteLine($"{"ID",-5} {"Date",-12} {"Time",-8} {"Token",-6} {"Patient",-8} {"Doctor",-8} {"Status",-12}");
                    Console.WriteLine(new string('-', 100));
                    
                    foreach (var appt in appointments)
                    {
                        Console.WriteLine($"A{appt.AppointmentId,-4} {appt.AppointmentDate:yyyy-MM-dd,-12} {appt.AppointmentDate:HH:mm,-8} {appt.TokenNumber,-6} {appt.PatientId,-8} {appt.DoctorId,-8} {appt.ConsultationStatus,-12}");
                    }
                    Console.WriteLine(new string('-', 100));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error viewing appointments: {ex.Message}");
            }
        }
        #endregion

        #region View Available Time Slots
        private static async Task ViewAvailableTimeSlotsAsync()
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("      VIEW DOCTOR APPOINTMENTS");
            Console.ResetColor();
            Console.WriteLine("======================================");

            try
            {
                // Get all available doctors
                var doctors = await userService.GetAllDoctorsAsync();
                if (doctors.Count == 0)
                {
                    Console.WriteLine("No doctors available.");
                    return;
                }

                // Display available doctors
                Console.WriteLine("\nAvailable Doctors:");
                Console.WriteLine(new string('-', 80));
                foreach (var doctor in doctors)
                {
                    Console.WriteLine($"{doctor.DoctorId}. {doctor.DoctorName} - {doctor.SpecializationNames}");
                }
                Console.WriteLine(new string('-', 80));

                // Get doctor selection
                int selectedDoctorId;
                do
                {
                    Console.Write("Select Doctor ID: ");
                    if (!int.TryParse(Console.ReadLine(), out selectedDoctorId) || 
                        !doctors.Any(d => d.DoctorId == selectedDoctorId))
                    {
                        Console.WriteLine("Invalid doctor ID. Please try again.");
                    }
                } while (!doctors.Any(d => d.DoctorId == selectedDoctorId));

                // Get appointment date
                DateTime appointmentDate;
                do
                {
                    Console.Write("Enter Date to view appointments (yyyy-mm-dd): ");
                    string dateInput = Console.ReadLine();
                    if (!DateTime.TryParse(dateInput, out appointmentDate))
                    {
                        Console.WriteLine("Invalid date format. Please try again.");
                    }
                } while (appointmentDate == DateTime.MinValue);

                // Show existing appointments
                var existingAppointments = await userService.GetDoctorAppointmentsForDateAsync(selectedDoctorId, appointmentDate);
                if (existingAppointments.Count > 0)
                {
                    Console.WriteLine($"\nAppointments for {doctors.First(d => d.DoctorId == selectedDoctorId).DoctorName}:");
                    Console.WriteLine($"Date: {appointmentDate:yyyy-MM-dd}");
                    Console.WriteLine(new string('-', 80));
                    Console.WriteLine($"{"Time",-8} {"Token",-6} {"Patient ID",-10} {"Status",-12}");
                    Console.WriteLine(new string('-', 80));
                    
                    foreach (var appt in existingAppointments.OrderBy(a => a.AppointmentDate))
                    {
                        Console.WriteLine($"{appt.AppointmentDate:HH:mm,-8} {appt.TokenNumber,-6} {appt.PatientId,-10} {appt.ConsultationStatus,-12}");
                    }
                    Console.WriteLine(new string('-', 80));
                }
                else
                {
                    Console.WriteLine($"No appointments found for {doctors.First(d => d.DoctorId == selectedDoctorId).DoctorName} on {appointmentDate:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error viewing appointments: {ex.Message}");
            }
        }
        #endregion

        #region Generate Consultation Bill
        private static async Task GenerateConsultationBillAsync()
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("      VIEW CONSULTATION BILLS");
            Console.ResetColor();
            Console.WriteLine("======================================");

            try
            {
                // Get all bills
                var allBills = await userService.GetAllBillsAsync();
                
                if (allBills.Count == 0)
                {
                    Console.WriteLine("No consultation bills found.");
                    return;
                }

                // Display all bills
                Console.WriteLine("\nAll Consultation Bills:");
                Console.WriteLine(new string('=', 120));
                Console.WriteLine($"{"Bill ID",-8} {"Appt ID",-8} {"Patient",-25} {"Doctor",-20} {"Consult Fee",-12} {"Lab",-10} {"Medicine",-10} {"Total",-12} {"Status",-12}");
                Console.WriteLine(new string('=', 120));
                
                foreach (var bill in allBills)
                {
                    Console.WriteLine($"B{bill.BillId,-7} A{bill.AppointmentId,-7} {bill.PatientName,-25} {bill.DoctorName,-20} ${bill.ConsultationFee,-11:F2} ${bill.LabCharges,-9:F2} ${bill.MedicineCharges,-9:F2} ${bill.TotalAmount,-11:F2} {bill.PaymentStatus,-12}");
                }
                Console.WriteLine(new string('=', 120));

                // Option to view specific bill details
                Console.WriteLine("\n" + new string('-', 60));
                Console.Write("Enter Bill ID (without 'B' prefix) to view details, or press Enter to return: ");
                string? input = Console.ReadLine();
                
                if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int billId))
                {
                    var selectedBill = allBills.FirstOrDefault(b => b.BillId == billId);
                    if (selectedBill != null)
                    {
                        Console.Clear();
                        Console.WriteLine("======================================");
                        Console.WriteLine("      BILL DETAILS");
                        Console.WriteLine("======================================");
                        Console.WriteLine($"Bill ID: B{selectedBill.BillId}");
                        Console.WriteLine($"Appointment ID: A{selectedBill.AppointmentId}");
                        Console.WriteLine($"Patient: {selectedBill.PatientName}");
                        Console.WriteLine($"Doctor: {selectedBill.DoctorName}");
                        Console.WriteLine($"Bill Date: {selectedBill.CreatedDate:yyyy-MM-dd HH:mm}");
                        Console.WriteLine(new string('-', 50));
                        Console.WriteLine($"Consultation Fee: ${selectedBill.ConsultationFee:F2}");
                        Console.WriteLine($"Lab Charges: ${selectedBill.LabCharges:F2}");
                        Console.WriteLine($"Medicine Charges: ${selectedBill.MedicineCharges:F2}");
                        Console.WriteLine(new string('-', 50));
                        Console.WriteLine($"TOTAL AMOUNT: ${selectedBill.TotalAmount:F2}");
                        Console.WriteLine($"Payment Status: {selectedBill.PaymentStatus}");
                        
                        if (selectedBill.PaymentStatus == "Unpaid")
                        {
                            Console.WriteLine("\n" + new string('=', 50));
                            Console.Write("Mark this bill as Paid? (y/n): ");
                            string? markPaidChoice = Console.ReadLine()?.ToLower();
                            if (markPaidChoice == "y" || markPaidChoice == "yes")
                            {
                                selectedBill.PaymentStatus = "Paid";
                                selectedBill.PaymentDate = DateTime.Now;
                                
                                bool updated = await userService.UpdateConsultationBillAsync(selectedBill);
                                if (updated)
                                {
                                    Console.WriteLine("✅ Bill marked as Paid successfully!");
                                }
                                else
                                {
                                    Console.WriteLine("❌ Failed to update payment status.");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Bill not found with the specified ID.");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error in bill generation process: {ex.Message}");
            }
        }
        #endregion

        private static void PrintPatient(TblPatient patient)
        {
            Console.WriteLine("---------------------------------");
            Console.WriteLine($"MRN: MRN000{patient.PatientId}");
            Console.WriteLine($"Name: {patient.PatientName}");
            Console.WriteLine($"DOB: {patient.DateOfBirth:d}");
            Console.WriteLine($"Gender: {patient.Gender}");
            Console.WriteLine($"Mobile: {patient.MobileNumber}");
            Console.WriteLine($"Address: {patient.Address}");
            Console.WriteLine($"Blood Group: {patient.BloodGroup}");
            Console.WriteLine($"MembershipId: {patient.MembershipId}");
            Console.WriteLine($"Created: {patient.CreatedDate:d}");
            Console.WriteLine("---------------------------------");
        }
        #endregion
    }
}
