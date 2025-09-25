using ConsoleApp_cmsv2025.Model;
using ConsoleApp_cmsv2025.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Service
{
    public class UserServiceImpl : IUserService
    {
        //declare private variables
        private readonly Repository.IUserRepository _userRepository;
        //DI - Dependency Injection
        public UserServiceImpl(Repository.IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        #region Authenticate User
        public async Task<int> AuthenticateUserByRoleIdAsync(string username, string password)
        {
            return await _userRepository.AuthenticateUserByRoleIdAsync(username, password);
        }
        #endregion

        #region Get Doctor Appointments (Today)
        public async Task<List<TblAppointment>> GetDoctorAppointmentsForTodayAsync(int doctorId)
        {
            return await _userRepository.GetDoctorAppointmentsForTodayAsync(doctorId);
        }
        #endregion

        #region Get Doctor Appointments (Specific Date)
        public async Task<List<TblAppointment>> GetDoctorAppointmentsForDateAsync(int doctorId, DateTime appointmentDate)
        {
            return await _userRepository.GetDoctorAppointmentsForDateAsync(doctorId, appointmentDate);
        }
        #endregion

        #region Get DoctorId by Username
        public async Task<int> GetDoctorIdByUserNameAsync(string username)
        {
            return await _userRepository.GetDoctorIdByUserNameAsync(username);
        }
        #endregion

        #region Search Patient by Mobile
        public async Task<TblPatient?> SearchPatientByMobileAsync(string mobileNumber)
        {
            // Validate mobile number format
            if (!CustomValidation.IsValidMobileNumber(mobileNumber))
            {
                throw new ArgumentException("Invalid mobile number format. Please enter a valid Indian mobile number.");
            }

            return await _userRepository.SearchPatientByMobileAsync(mobileNumber);
        }
        #endregion

        #region Search Patient by MRN
        public async Task<TblPatient?> SearchPatientByMRNAsync(int patientId)
        {
            return await _userRepository.SearchPatientByMRNAsync(patientId);
        }
        #endregion

        #region Create New Patient
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
        #endregion

        #region Check Mobile Number Exists
        public async Task<bool> CheckMobileNumberExistsAsync(string mobileNumber)
        {
            // Validate mobile number format
            if (!CustomValidation.IsValidMobileNumber(mobileNumber))
            {
                throw new ArgumentException("Invalid mobile number format. Please enter a valid Indian mobile number.");
            }

            return await _userRepository.CheckMobileNumberExistsAsync(mobileNumber);
        }
        #endregion

        #region Check MRN Number Exists
        public async Task<bool> CheckMRNNumberExistsAsync(int patientId)
        {
            return await _userRepository.CheckMRNNumberExistsAsync(patientId);
        }
        #endregion

        #region Create Appointment
        public async Task<int> CreateAppointmentAsync(TblAppointment appointment)
        {
            // Validate appointment model
            string validationErrors = CustomValidation.ValidateAppointmentModel(appointment);
            if (!string.IsNullOrEmpty(validationErrors))
            {
                throw new ArgumentException($"Appointment validation failed: {validationErrors}");
            }

            return await _userRepository.CreateAppointmentAsync(appointment);
        }
        #endregion

        #region Get All Doctors
        public async Task<List<TblDoctor>> GetAllDoctorsAsync()
        {
            return await _userRepository.GetAllDoctorsAsync();
        }
        #endregion

        #region Get Next Token Number
        public async Task<int> GetNextTokenNumberAsync(int doctorId, DateTime appointmentDate)
        {
            return await _userRepository.GetNextTokenNumberAsync(doctorId, appointmentDate);
        }
        #endregion

        #region Medicine Prescription Methods
        public async Task<List<TblMedicine>> GetAllMedicinesAsync()
        {
            return await _userRepository.GetAllMedicinesAsync();
        }

        public async Task<List<TblMedicineCategory>> GetAllMedicineCategoriesAsync()
        {
            return await _userRepository.GetAllMedicineCategoriesAsync();
        }

        public async Task<int> CreatePrescriptionMedicineAsync(TblPrescriptionMedicine prescription)
        {
            return await _userRepository.CreatePrescriptionMedicineAsync(prescription);
        }

        public async Task<List<TblPrescriptionMedicine>> GetPrescriptionsByAppointmentIdAsync(int appointmentId)
        {
            return await _userRepository.GetPrescriptionsByAppointmentIdAsync(appointmentId);
        }
        #endregion

        #region Consultation Methods
        public async Task<int> CreateConsultationAsync(TblConsultation consultation)
        {
            // Validate consultation model
            string validationErrors = CustomValidation.ValidateConsultationModel(consultation);
            if (!string.IsNullOrEmpty(validationErrors))
            {
                throw new ArgumentException($"Consultation validation failed: {validationErrors}");
            }

            return await _userRepository.CreateConsultationAsync(consultation);
        }

        public async Task<TblConsultation?> GetConsultationByAppointmentIdAsync(int appointmentId)
        {
            return await _userRepository.GetConsultationByAppointmentIdAsync(appointmentId);
        }

        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status)
        {
            return await _userRepository.UpdateAppointmentStatusAsync(appointmentId, status);
        }

        public async Task<List<TblPatient>> GetAllPatientsAsync()
        {
            return await _userRepository.GetAllPatientsAsync();
        }
        #endregion

        #region Lab Test Methods
        public async Task<List<TblLabTest>> GetAllLabTestsAsync()
        {
            return await _userRepository.GetAllLabTestsAsync();
        }

        public async Task<List<TblLabTestCategory>> GetAllLabTestCategoriesAsync()
        {
            return await _userRepository.GetAllLabTestCategoriesAsync();
        }

        public async Task<int> CreateLabTestPrescriptionAsync(TblLabTestPrescription prescription)
        {
            return await _userRepository.CreateLabTestPrescriptionAsync(prescription);
        }

        public async Task<List<TblLabTestPrescription>> GetLabTestPrescriptionsByAppointmentIdAsync(int appointmentId)
        {
            return await _userRepository.GetLabTestPrescriptionsByAppointmentIdAsync(appointmentId);
        }
        #endregion

        #region Bill Generation Methods
        public async Task<int> CreateConsultationBillAsync(TblConsultationBill bill)
        {
            return await _userRepository.CreateConsultationBillAsync(bill);
        }

        public async Task<bool> UpdateConsultationBillAsync(TblConsultationBill bill)
        {
            return await _userRepository.UpdateConsultationBillAsync(bill);
        }

        public async Task<TblConsultationBill?> GetConsultationBillByAppointmentIdAsync(int appointmentId)
        {
            return await _userRepository.GetConsultationBillByAppointmentIdAsync(appointmentId);
        }

        public async Task<List<TblConsultationBill>> GetAllBillsAsync()
        {
            return await _userRepository.GetAllBillsAsync();
        }

        public async Task<List<TblConsultationBill>> GetBillsByDateAsync(DateTime date)
        {
            return await _userRepository.GetBillsByDateAsync(date);
        }
        #endregion

        #region Appointment Viewing Methods for Receptionist
        public async Task<List<TblAppointment>> GetAllAppointmentsAsync()
        {
            return await _userRepository.GetAllAppointmentsAsync();
        }

        public async Task<List<TblAppointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            return await _userRepository.GetAppointmentsByDateAsync(date);
        }
        #endregion

    }
}