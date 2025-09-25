using ConsoleApp_cmsv2025.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Repository
{
    public interface IUserRepository
    {
        //Login Functionality - passing username and passsword
        //giving task for multithreading
        Task<int> AuthenticateUserByRoleIdAsync(string username, string password);
        Task<TblPatient?> SearchPatientByMRNAsync(int patientId);
        Task<TblPatient?> SearchPatientByMobileAsync(string mobileNumber);
        Task<int> GetDoctorIdByUserNameAsync(string username); 
        Task<List<TblAppointment>> GetDoctorAppointmentsForTodayAsync(int doctorId);
        Task<List<TblAppointment>> GetDoctorAppointmentsForDateAsync(int doctorId, DateTime appointmentDate);
        
        // New methods for patient creation
        Task<int> CreateNewPatientAsync(TblPatient patient);
        Task<bool> CheckMobileNumberExistsAsync(string mobileNumber);
        Task<bool> CheckMRNNumberExistsAsync(int patientId);
        
        // Appointment creation methods
        Task<int> CreateAppointmentAsync(TblAppointment appointment);
        Task<List<TblDoctor>> GetAllDoctorsAsync();
        Task<int> GetNextTokenNumberAsync(int doctorId, DateTime appointmentDate);
        
        // Medicine prescription methods
        Task<List<TblMedicine>> GetAllMedicinesAsync();
        Task<List<TblMedicineCategory>> GetAllMedicineCategoriesAsync();
        Task<int> CreatePrescriptionMedicineAsync(TblPrescriptionMedicine prescription);
        Task<List<TblPrescriptionMedicine>> GetPrescriptionsByAppointmentIdAsync(int appointmentId);
        
        // Consultation methods
        Task<int> CreateConsultationAsync(TblConsultation consultation);
        Task<TblConsultation?> GetConsultationByAppointmentIdAsync(int appointmentId);
        Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status);
        Task<List<TblPatient>> GetAllPatientsAsync();
        
        // Lab test methods
        Task<List<TblLabTest>> GetAllLabTestsAsync();
        Task<List<TblLabTestCategory>> GetAllLabTestCategoriesAsync();
        Task<int> CreateLabTestPrescriptionAsync(TblLabTestPrescription prescription);
        Task<List<TblLabTestPrescription>> GetLabTestPrescriptionsByAppointmentIdAsync(int appointmentId);
        
        // Bill generation methods
        Task<int> CreateConsultationBillAsync(TblConsultationBill bill);
        Task<bool> UpdateConsultationBillAsync(TblConsultationBill bill);
        Task<TblConsultationBill?> GetConsultationBillByAppointmentIdAsync(int appointmentId);
        Task<List<TblConsultationBill>> GetAllBillsAsync();
        Task<List<TblConsultationBill>> GetBillsByDateAsync(DateTime date);
        
        // Appointment viewing methods for receptionist
        Task<List<TblAppointment>> GetAllAppointmentsAsync();
        Task<List<TblAppointment>> GetAppointmentsByDateAsync(DateTime date);
        
    }
}