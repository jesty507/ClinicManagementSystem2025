using ConsoleApp_cmsv2025.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Repository
{
    public class UserRepository : IUserRepository
    {
        //ConnectionString - Get from app.config
        string winConnString = ConfigurationManager.ConnectionStrings["CsWinSql"].ConnectionString;

        #region 1- Implement login functionality with database
        public async Task<int> AuthenticateUserByRoleIdAsync(string username, string password)
        {
            //declare variable for roleId
            int roleId = 0;

            try
            {
                //SqlConnection -- connectionString
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();

                    SqlCommand command = new SqlCommand("sp_AuthenticateUserByRoleId", conn);

                    //command type
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    //input parameters
                    command.Parameters.AddWithValue("@UserName", username);
                    command.Parameters.AddWithValue("@Password", password);

                    //output parameters
                    SqlParameter roleIdParameter = new SqlParameter("@RoleId", System.Data.SqlDbType.Int)
                    {
                        Direction = System.Data.ParameterDirection.Output
                    };
                    command.Parameters.Add(roleIdParameter);

                    await command.ExecuteNonQueryAsync();

                    //DBNull check
                    if (roleIdParameter.Value != DBNull.Value)
                    {
                        roleId = Convert.ToInt32(roleIdParameter.Value);
                    }

                    return roleId;
                }

                //Sqlcommand -- SqlScript/storedprocedure, connection
                //Output parameter
                //Check if output parameter is DBnull before conversion
            }
            catch (SqlException es)
            {
                Console.WriteLine(" An SqlException has occurred: " + es.Message);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" An error has occurred: " + ex.Message);
                throw;
            }
        }

        #endregion

        #region 2- Search patient by MRN
        public async Task<TblPatient?> SearchPatientByMRNAsync(int patientId)
        {
            TblPatient? patient = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_SearchPatientByMRN", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PatientId", patientId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            patient = MapPatient(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SearchPatientByMRN: " + ex.Message);
            }

            return patient;
        }
        #endregion


        #region 3- Search patient by Mobile
        public async Task<TblPatient?> SearchPatientByMobileAsync(string mobileNumber)
        {
            TblPatient? patient = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_SearchPatientByMobile", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MobileNumber", mobileNumber);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            patient = MapPatient(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SearchPatientByMobile: " + ex.Message);
            }

            return patient;
        }
        #endregion

        #region 4- Get doctor's appointments (today)
        public async Task<List<TblAppointment>> GetDoctorAppointmentsForTodayAsync(int doctorId)
        {
            var appointments = new List<TblAppointment>();

            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetDoctorAppointmentsForToday", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DoctorId", doctorId);  

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Helper local functions for safe reads by column name
                        bool HasColumn(string name)
                        {
                            try { return reader.GetOrdinal(name) >= 0; } catch { return false; }
                        }
                        int ReadInt(string name)
                        {
                            if (!HasColumn(name)) return 0;
                            var value = reader[name];
                            if (value == DBNull.Value || value == null) return 0;
                            if (value is int i) return i;
                            if (value is long l) return (int)l;
                            if (value is decimal d) return (int)d;
                            if (value is string s && int.TryParse(s, out var si)) return si;
                            return 0;
                        }
                        DateTime ReadDateTime(string name)
                        {
                            if (!HasColumn(name)) return DateTime.MinValue;
                            var value = reader[name];
                            if (value == DBNull.Value || value == null) return DateTime.MinValue;
                            if (value is DateTime dt) return dt;
                            if (value is string s && DateTime.TryParse(s, out var parsed)) return parsed;
                            return DateTime.MinValue;
                        }
                        bool ReadBool(string name)
                        {
                            if (!HasColumn(name)) return false;
                            var value = reader[name];
                            if (value == DBNull.Value || value == null) return false;
                            if (value is bool b) return b;
                            if (value is int i) return i != 0;
                            if (value is string s && bool.TryParse(s, out var sb)) return sb;
                            return false;
                        }
                        string ReadString(string name)
                        {
                            if (!HasColumn(name)) return string.Empty;
                            var value = reader[name];
                            if (value == DBNull.Value || value == null) return string.Empty;
                            return Convert.ToString(value) ?? string.Empty;
                        }

                        while (await reader.ReadAsync())
                        {
                            // Attempt to read by names; fall back to indices if needed
                            var appointment = new TblAppointment
                            {
                                AppointmentId = HasColumn("AppointmentId") ? ReadInt("AppointmentId") : (reader.FieldCount > 0 && !reader.IsDBNull(0) ? Convert.ToInt32(reader.GetValue(0)) : 0),
                                AppointmentDate = HasColumn("AppointmentDate") ? ReadDateTime("AppointmentDate") : (reader.FieldCount > 1 && !reader.IsDBNull(1) ? Convert.ToDateTime(reader.GetValue(1)) : DateTime.MinValue),
                                TokenNumber = HasColumn("TokenNumber") ? ReadInt("TokenNumber") : (reader.FieldCount > 2 && !reader.IsDBNull(2) ? Convert.ToInt32(reader.GetValue(2)) : 0),
                                ConsultationStatus = HasColumn("ConsultationStatus") ? ReadString("ConsultationStatus") : (reader.FieldCount > 3 && !reader.IsDBNull(3) ? Convert.ToString(reader.GetValue(3)) ?? string.Empty : string.Empty),
                                PatientId = HasColumn("PatientId") ? ReadInt("PatientId") : (reader.FieldCount > 4 && !reader.IsDBNull(4) ? Convert.ToInt32(reader.GetValue(4)) : 0),
                                DoctorId = HasColumn("DoctorId") ? ReadInt("DoctorId") : (reader.FieldCount > 5 && !reader.IsDBNull(5) ? Convert.ToInt32(reader.GetValue(5)) : 0),
                                UserId = HasColumn("UserId") ? ReadInt("UserId") : (reader.FieldCount > 6 && !reader.IsDBNull(6) ? Convert.ToInt32(reader.GetValue(6)) : 0),
                                CreatedDate = HasColumn("CreatedDate") ? ReadDateTime("CreatedDate") : (reader.FieldCount > 7 && !reader.IsDBNull(7) ? Convert.ToDateTime(reader.GetValue(7)) : DateTime.MinValue),
                                IsActive = HasColumn("IsActive") ? ReadBool("IsActive") : (reader.FieldCount > 8 && !reader.IsDBNull(8) && Convert.ToInt32(reader.GetValue(8)) != 0)
                            };

                            appointments.Add(appointment);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetDoctorAppointments: " + ex.Message);
            }

            return appointments;
        }
        #endregion

        #region 5- Get doctor's appointments (specific date)
        public async Task<List<TblAppointment>> GetDoctorAppointmentsForDateAsync(int doctorId, DateTime appointmentDate)
        {
            var appointments = new List<TblAppointment>();

            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetDoctorAppointmentsForDate", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                    cmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate.Date);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Helper local functions for safe reads by column name
                        bool HasColumn(string name)
                        {
                            try { return reader.GetOrdinal(name) >= 0; } catch { return false; }
                        }
                        int ReadInt(string name)
                        {
                            if (!HasColumn(name)) return 0;
                            var value = reader[name];
                            if (value == DBNull.Value || value == null) return 0;
                            if (value is int i) return i;
                            if (value is long l) return (int)l;
                            if (value is decimal d) return (int)d;
                            if (value is string s && int.TryParse(s, out var si)) return si;
                            return 0;
                        }
                        DateTime ReadDateTime(string name)
                        {
                            if (!HasColumn(name)) return DateTime.MinValue;
                            var value = reader[name];
                            if (value == DBNull.Value || value == null) return DateTime.MinValue;
                            if (value is DateTime dt) return dt;
                            if (value is string s && DateTime.TryParse(s, out var parsed)) return parsed;
                            return DateTime.MinValue;
                        }
                        bool ReadBool(string name)
                        {
                            if (!HasColumn(name)) return false;
                            var value = reader[name];
                            if (value == DBNull.Value || value == null) return false;
                            if (value is bool b) return b;
                            if (value is int i) return i != 0;
                            if (value is string s && bool.TryParse(s, out var sb)) return sb;
                            return false;
                        }
                        string ReadString(string name)
                        {
                            if (!HasColumn(name)) return string.Empty;
                            var value = reader[name];
                            if (value == DBNull.Value || value == null) return string.Empty;
                            return Convert.ToString(value) ?? string.Empty;
                        }

                        while (await reader.ReadAsync())
                        {
                            // Attempt to read by names; fall back to indices if needed
                            var appointment = new TblAppointment
                            {
                                AppointmentId = HasColumn("AppointmentId") ? ReadInt("AppointmentId") : (reader.FieldCount > 0 && !reader.IsDBNull(0) ? Convert.ToInt32(reader.GetValue(0)) : 0),
                                AppointmentDate = HasColumn("AppointmentDate") ? ReadDateTime("AppointmentDate") : (reader.FieldCount > 1 && !reader.IsDBNull(1) ? Convert.ToDateTime(reader.GetValue(1)) : DateTime.MinValue),
                                TokenNumber = HasColumn("TokenNumber") ? ReadInt("TokenNumber") : (reader.FieldCount > 2 && !reader.IsDBNull(2) ? Convert.ToInt32(reader.GetValue(2)) : 0),
                                ConsultationStatus = HasColumn("ConsultationStatus") ? ReadString("ConsultationStatus") : (reader.FieldCount > 3 && !reader.IsDBNull(3) ? Convert.ToString(reader.GetValue(3)) ?? string.Empty : string.Empty),
                                PatientId = HasColumn("PatientId") ? ReadInt("PatientId") : (reader.FieldCount > 4 && !reader.IsDBNull(4) ? Convert.ToInt32(reader.GetValue(4)) : 0),
                                DoctorId = HasColumn("DoctorId") ? ReadInt("DoctorId") : (reader.FieldCount > 5 && !reader.IsDBNull(5) ? Convert.ToInt32(reader.GetValue(5)) : 0),
                                UserId = HasColumn("UserId") ? ReadInt("UserId") : (reader.FieldCount > 6 && !reader.IsDBNull(6) ? Convert.ToInt32(reader.GetValue(6)) : 0),
                                CreatedDate = HasColumn("CreatedDate") ? ReadDateTime("CreatedDate") : (reader.FieldCount > 7 && !reader.IsDBNull(7) ? Convert.ToDateTime(reader.GetValue(7)) : DateTime.MinValue),
                                IsActive = HasColumn("IsActive") ? ReadBool("IsActive") : (reader.FieldCount > 8 && !reader.IsDBNull(8) && Convert.ToInt32(reader.GetValue(8)) != 0)
                            };

                            appointments.Add(appointment);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetDoctorAppointmentsForDate: " + ex.Message);
            }

            return appointments;
        }
        #endregion

        #region Helper - Map Patient
        private TblPatient MapPatient(SqlDataReader reader)
        {
            // Helper functions for safe reading
            int ReadInt(string columnName)
            {
                try
                {
                    if (reader.IsDBNull(columnName)) return 0;
                    return reader.GetInt32(columnName);
                }
                catch
                {
                    return 0;
                }
            }

            string? ReadString(string columnName)
            {
                try
                {
                    if (reader.IsDBNull(columnName)) return null;
                    return reader.GetString(columnName);
                }
                catch
                {
                    return null;
                }
            }

            DateTime? ReadDateTime(string columnName)
            {
                try
                {
                    if (reader.IsDBNull(columnName)) return null;
                    return reader.GetDateTime(columnName);
                }
                catch
                {
                    return null;
                }
            }

            bool ReadBool(string columnName)
            {
                try
                {
                    if (reader.IsDBNull(columnName)) return false;
                    return reader.GetBoolean(columnName);
                }
                catch
                {
                    return false;
                }
            }

            return new TblPatient
            {
                PatientId = ReadInt("PatientId"),
                UserId = ReadInt("UserId"),
                PatientName = ReadString("PatientName"),
                DateOfBirth = ReadDateTime("DateOfBirth"),
                Gender = ReadString("Gender"),
                BloodGroup = ReadString("BloodGroup"),
                Address = ReadString("Address"),
                MobileNumber = ReadString("MobileNumber"),
                MembershipId = ReadInt("MembershipId"),
                CreatedDate = ReadDateTime("CreatedDate") ?? DateTime.MinValue,
                IsActive = ReadBool("IsActive")
            };
        }

        #endregion
        #region 2- Get DoctorId from Username
        public async Task<int> GetDoctorIdByUserNameAsync(string username)
        {
            int doctorId = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetDoctorIdByUserName", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", username);

                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null) doctorId = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetDoctorIdByUserName: " + ex.Message);
            }

            return doctorId;
        }
        #endregion

        // ... existing code ...

        #region 6- Check if Mobile Number Exists
        public async Task<bool> CheckMobileNumberExistsAsync(string mobileNumber)
        {
            bool exists = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_CheckMobileNumberExists", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MobileNumber", mobileNumber);

                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        exists = Convert.ToBoolean(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CheckMobileNumberExists: " + ex.Message);
            }

            return exists;
        }
        #endregion

        // ... existing code ...

        #region 7- Check if MRN Number Exists
        public async Task<bool> CheckMRNNumberExistsAsync(int patientId)
        {
            bool exists = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_CheckMRNnumberExists", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PatientId", patientId);

                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        exists = Convert.ToBoolean(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CheckMRNNumberExists: " + ex.Message);
            }

            return exists;
        }
        #endregion

        #region 8- Create New Patient
        public async Task<int> CreateNewPatientAsync(TblPatient patient)
        {
            int newPatientId = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_CreateNewPatient", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    cmd.Parameters.AddWithValue("@UserId", patient.UserId == 0 ? (object)DBNull.Value : patient.UserId);
                    cmd.Parameters.AddWithValue("@PatientName", patient.PatientName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Gender", patient.Gender ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BloodGroup", patient.BloodGroup ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", patient.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MobileNumber", patient.MobileNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MembershipId", patient.MembershipId);
                    
                    SqlParameter patientIdParameter = new SqlParameter("@PatientId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(patientIdParameter);

                    await cmd.ExecuteNonQueryAsync();

                    if (patientIdParameter.Value != DBNull.Value)
                    {
                        newPatientId = Convert.ToInt32(patientIdParameter.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateNewPatient: " + ex.Message);
                throw;
            }

            return newPatientId;
        }
        #endregion

        #region 9- Create Appointment
        public async Task<int> CreateAppointmentAsync(TblAppointment appointment)
        {
            int newAppointmentId = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_CreateAppointment", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    cmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
                    cmd.Parameters.AddWithValue("@TokenNumber", appointment.TokenNumber);
                    cmd.Parameters.AddWithValue("@ConsultationStatus", appointment.ConsultationStatus);
                    cmd.Parameters.AddWithValue("@PatientId", appointment.PatientId);
                    cmd.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
                    cmd.Parameters.AddWithValue("@UserId", appointment.UserId);
                    cmd.Parameters.AddWithValue("@CreatedBy", appointment.CreatedAt);
                    
                    SqlParameter appointmentIdParameter = new SqlParameter("@AppointmentId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(appointmentIdParameter);

                    await cmd.ExecuteNonQueryAsync();

                    if (appointmentIdParameter.Value != DBNull.Value)
                    {
                        newAppointmentId = Convert.ToInt32(appointmentIdParameter.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateAppointment: " + ex.Message);
                throw;
            }

            return newAppointmentId;
        }
        #endregion

        #region 10- Get All Doctors
        public async Task<List<TblDoctor>> GetAllDoctorsAsync()
        {
            var doctors = new List<TblDoctor>();
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetAllDoctors", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int doctorId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            string doctorName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            decimal consultationFee = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2);
                            int userId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                            DateTime createdDate = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4);
                            bool isActive = reader.IsDBNull(5) ? false : reader.GetBoolean(5);
                            string specializationNames = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);

                            doctors.Add(new TblDoctor
                            {
                                DoctorId = doctorId,
                                DoctorName = doctorName,
                                ConsultationFee = consultationFee,
                                UserId = userId,
                                CreatedDate = createdDate,
                                IsActive = isActive,
                                SpecializationNames = specializationNames
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllDoctors: " + ex.Message);
            }

            return doctors;
        }
        #endregion

        #region 11- Get Next Token Number
        public async Task<int> GetNextTokenNumberAsync(int doctorId, DateTime appointmentDate)
        {
            int nextTokenNumber = 1;
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetNextTokenNumber", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DoctorId", doctorId);
                    cmd.Parameters.AddWithValue("@AppointmentDate", appointmentDate.Date);

                    var result = await cmd.ExecuteScalarAsync();
                    if (result != null)
                    {
                        nextTokenNumber = Convert.ToInt32(result) + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetNextTokenNumber: " + ex.Message);
            }

            return nextTokenNumber;
        }
        #endregion

        #region 12- Get All Medicines
        public async Task<List<TblMedicine>> GetAllMedicinesAsync()
        {
            var medicines = new List<TblMedicine>();
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetAllMedicines", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            medicines.Add(new TblMedicine
                            {
                                MedicineId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                MedicineCategoryId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                MedicineName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                ManufactureDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                                ExpiryDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                                UnitAvailable = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                Price = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllMedicines: " + ex.Message);
            }

            return medicines;
        }
        #endregion

        #region 13- Get All Medicine Categories
        public async Task<List<TblMedicineCategory>> GetAllMedicineCategoriesAsync()
        {
            var categories = new List<TblMedicineCategory>();
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetAllMedicineCategories", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new TblMedicineCategory
                            {
                                MedicineCategoryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                CategoryName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllMedicineCategories: " + ex.Message);
            }

            return categories;
        }
        #endregion

        #region 14- Create Prescription Medicine
        public async Task<int> CreatePrescriptionMedicineAsync(TblPrescriptionMedicine prescription)
        {
            int prescriptionId = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_CreatePrescriptionMedicine", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    cmd.Parameters.AddWithValue("@MedicineId", prescription.MedicineId);
                    cmd.Parameters.AddWithValue("@Dosage", prescription.Dosage ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Duration", prescription.Duration ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Instructions", prescription.Instructions ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppointmentId", prescription.AppointmentId);
                    
                    SqlParameter prescriptionIdParameter = new SqlParameter("@PrescriptionMedicineId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(prescriptionIdParameter);

                    await cmd.ExecuteNonQueryAsync();

                    if (prescriptionIdParameter.Value != DBNull.Value)
                    {
                        prescriptionId = Convert.ToInt32(prescriptionIdParameter.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreatePrescriptionMedicine: " + ex.Message);
                throw;
            }

            return prescriptionId;
        }
        #endregion

        #region 15- Get Prescriptions by Appointment ID
        public async Task<List<TblPrescriptionMedicine>> GetPrescriptionsByAppointmentIdAsync(int appointmentId)
        {
            var prescriptions = new List<TblPrescriptionMedicine>();
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetPrescriptionsByAppointmentId", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            prescriptions.Add(new TblPrescriptionMedicine
                            {
                                PrescriptionMedicineId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                MedicineId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                Dosage = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Duration = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Instructions = reader.IsDBNull(4) ? null : reader.GetString(4),
                                AppointmentId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetPrescriptionsByAppointmentId: " + ex.Message);
            }

            return prescriptions;
        }
        #endregion

        #region 16- Create Consultation
        public async Task<int> CreateConsultationAsync(TblConsultation consultation)
        {
            int consultationId = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_CreateConsultation", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    cmd.Parameters.AddWithValue("@Symptoms", consultation.Symptoms ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Diagnosis", consultation.Diagnosis ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", consultation.Notes ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AppointmentId", consultation.AppointmentId);
                    cmd.Parameters.AddWithValue("@PatientId", consultation.PatientId);
                    
                    SqlParameter consultationIdParameter = new SqlParameter("@ConsultationId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(consultationIdParameter);

                    await cmd.ExecuteNonQueryAsync();

                    if (consultationIdParameter.Value != DBNull.Value)
                    {
                        consultationId = Convert.ToInt32(consultationIdParameter.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateConsultation: " + ex.Message);
                throw;
            }

            return consultationId;
        }
        #endregion

        #region 17- Get Consultation by Appointment ID
        public async Task<TblConsultation?> GetConsultationByAppointmentIdAsync(int appointmentId)
        {
            TblConsultation? consultation = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetConsultationByAppointmentId", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            consultation = new TblConsultation
                            {
                                ConsultationId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                Symptoms = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Diagnosis = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Notes = reader.IsDBNull(3) ? null : reader.GetString(3),
                                CreatedDate = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                                AppointmentId = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                IsActive = reader.IsDBNull(6) ? false : reader.GetBoolean(6),
                                PatientId = reader.IsDBNull(7) ? 0 : reader.GetInt32(7)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetConsultationByAppointmentId: " + ex.Message);
            }

            return consultation;
        }
        #endregion

        #region 18- Update Appointment Status
        public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string status)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("UPDATE TblAppointment SET ConsultationStatus = @Status WHERE AppointmentId = @AppointmentId", conn);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);
                    
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateAppointmentStatus: " + ex.Message);
                return false;
            }
        }
        #endregion

        #region 19- Get All Patients
        public async Task<List<TblPatient>> GetAllPatientsAsync()
        {
            var patients = new List<TblPatient>();
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("SELECT * FROM TblPatient WHERE IsActive = 1 ORDER BY PatientId", conn);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var patient = MapPatient(reader);
                            if (patient != null)
                            {
                                patients.Add(patient);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllPatients: " + ex.Message);
            }

            return patients;
        }
        #endregion

        #region 20- Get All Lab Tests
        public async Task<List<TblLabTest>> GetAllLabTestsAsync()
        {
            var labTests = new List<TblLabTest>();
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetAllLabTests", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            labTests.Add(new TblLabTest
                            {
                                LabTestId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                LabTestCategoryId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                TestName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                Amount = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                                ReferenceMinRange = reader.IsDBNull(4) ? null : reader.GetString(4),
                                ReferenceMaxRange = reader.IsDBNull(5) ? null : reader.GetString(5),
                                SampleRequired = reader.IsDBNull(6) ? null : reader.GetString(6),
                                CreatedDate = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7),
                                IsActive = reader.IsDBNull(8) ? false : reader.GetBoolean(8)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllLabTests: " + ex.Message);
            }

            return labTests;
        }
        #endregion

        #region 19- Get All Lab Test Categories
        public async Task<List<TblLabTestCategory>> GetAllLabTestCategoriesAsync()
        {
            var categories = new List<TblLabTestCategory>();
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetAllLabTestCategories", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new TblLabTestCategory
                            {
                                LabTesCategoryId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                CategoryName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllLabTestCategories: " + ex.Message);
            }

            return categories;
        }
        #endregion

        #region 20- Create Lab Test Prescription
        public async Task<int> CreateLabTestPrescriptionAsync(TblLabTestPrescription prescription)
        {
            int prescriptionId = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_CreateLabTestPrescription", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    cmd.Parameters.AddWithValue("@AppointmentId", prescription.AppointmentId);
                    cmd.Parameters.AddWithValue("@DoctorId", prescription.DoctorId);
                    cmd.Parameters.AddWithValue("@PatientId", prescription.PatientId);
                    cmd.Parameters.AddWithValue("@LabTestId", prescription.LabTestId);
                    cmd.Parameters.AddWithValue("@Notes", prescription.Notes ?? (object)DBNull.Value);
                    
                    SqlParameter prescriptionIdParameter = new SqlParameter("@PrescriptionId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(prescriptionIdParameter);

                    await cmd.ExecuteNonQueryAsync();

                    if (prescriptionIdParameter.Value != DBNull.Value)
                    {
                        prescriptionId = Convert.ToInt32(prescriptionIdParameter.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateLabTestPrescription: " + ex.Message);
                throw;
            }

            return prescriptionId;
        }
        #endregion

        #region 21- Get Lab Test Prescriptions by Appointment ID
        public async Task<List<TblLabTestPrescription>> GetLabTestPrescriptionsByAppointmentIdAsync(int appointmentId)
        {
            var prescriptions = new List<TblLabTestPrescription>();
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetLabTestPrescriptionsByAppointmentId", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            prescriptions.Add(new TblLabTestPrescription
                            {
                                PrescriptionId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                AppointmentId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                DoctorId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                PatientId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                LabTestId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
                                CreatedDate = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetLabTestPrescriptionsByAppointmentId: " + ex.Message);
            }

            return prescriptions;
        }
        #endregion

        #region 22- Create Consultation Bill
        public async Task<int> CreateConsultationBillAsync(TblConsultationBill bill)
        {
            int billId = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_CreateConsultationBill", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    
                    cmd.Parameters.AddWithValue("@AppointmentId", bill.AppointmentId);
                    cmd.Parameters.AddWithValue("@PatientId", bill.PatientId);
                    cmd.Parameters.AddWithValue("@ConsultationFee", bill.ConsultationFee);
                    cmd.Parameters.AddWithValue("@LabCharges", bill.LabCharges);
                    cmd.Parameters.AddWithValue("@MedicineCharges", bill.MedicineCharges);
                    cmd.Parameters.AddWithValue("@PaymentStatus", bill.PaymentStatus);
                    
                    SqlParameter billIdParameter = new SqlParameter("@BillId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(billIdParameter);

                    await cmd.ExecuteNonQueryAsync();

                    if (billIdParameter.Value != DBNull.Value)
                    {
                        billId = Convert.ToInt32(billIdParameter.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateConsultationBill: " + ex.Message);
                throw;
            }

            return billId;
        }
        #endregion

        #region 23- Update Consultation Bill
        public async Task<bool> UpdateConsultationBillAsync(TblConsultationBill bill)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand(@"
                        UPDATE TblConsultationBill 
                        SET LabCharges = @LabCharges, 
                            MedicineCharges = @MedicineCharges, 
                            TotalAmount = @TotalAmount,
                            PaymentStatus = @PaymentStatus,
                            BillDate = @BillDate,
                            UpdatedDate = @UpdatedDate
                        WHERE BillId = @BillId", conn);

                    cmd.Parameters.AddWithValue("@BillId", bill.BillId);
                    cmd.Parameters.AddWithValue("@LabCharges", bill.LabCharges);
                    cmd.Parameters.AddWithValue("@MedicineCharges", bill.MedicineCharges);
                    cmd.Parameters.AddWithValue("@TotalAmount", bill.TotalAmount);
                    cmd.Parameters.AddWithValue("@PaymentStatus", bill.PaymentStatus);
                    cmd.Parameters.AddWithValue("@BillDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateConsultationBill: " + ex.Message);
                return false;
            }
        }
        #endregion

        #region 24- Get Consultation Bill by Appointment ID
        public async Task<TblConsultationBill?> GetConsultationBillByAppointmentIdAsync(int appointmentId)
        {
            TblConsultationBill? bill = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetConsultationBillByAppointmentId", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AppointmentId", appointmentId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            bill = new TblConsultationBill
                            {
                                BillId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                AppointmentId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                PatientId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                ConsultationFee = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                                LabCharges = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                                MedicineCharges = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                                TotalAmount = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                                PaymentStatus = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                                PaymentDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                                CreatedDate = reader.IsDBNull(9) ? DateTime.MinValue : reader.GetDateTime(9),
                                PatientName = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                                DoctorName = reader.IsDBNull(11) ? string.Empty : reader.GetString(11)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetConsultationBillByAppointmentId: " + ex.Message);
            }

            return bill;
        }
        #endregion

        #region 25- Get All Bills
        public async Task<List<TblConsultationBill>> GetAllBillsAsync()
        {
            var bills = new List<TblConsultationBill>();
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetAllBills", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            bills.Add(new TblConsultationBill
                            {
                                BillId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                AppointmentId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                PatientId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                ConsultationFee = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                                LabCharges = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                                MedicineCharges = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                                TotalAmount = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                                PaymentStatus = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                                PaymentDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                                CreatedDate = reader.IsDBNull(9) ? DateTime.MinValue : reader.GetDateTime(9),
                                PatientName = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                                DoctorName = reader.IsDBNull(11) ? string.Empty : reader.GetString(11)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllBills: " + ex.Message);
            }

            return bills;
        }
        #endregion

        #region 26- Get Bills by Date
        public async Task<List<TblConsultationBill>> GetBillsByDateAsync(DateTime date)
        {
            var bills = new List<TblConsultationBill>();
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetBillsByDate", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Date", date.Date);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            bills.Add(new TblConsultationBill
                            {
                                BillId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                AppointmentId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                PatientId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                ConsultationFee = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                                LabCharges = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                                MedicineCharges = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5),
                                TotalAmount = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                                PaymentStatus = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                                PaymentDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                                CreatedDate = reader.IsDBNull(9) ? DateTime.MinValue : reader.GetDateTime(9),
                                PatientName = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                                DoctorName = reader.IsDBNull(11) ? string.Empty : reader.GetString(11)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetBillsByDate: " + ex.Message);
            }

            return bills;
        }
        #endregion

        #region 26- Get All Appointments
        public async Task<List<TblAppointment>> GetAllAppointmentsAsync()
        {
            var appointments = new List<TblAppointment>();
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetAllAppointments", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            appointments.Add(new TblAppointment
                            {
                                AppointmentId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                PatientId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                DoctorId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                UserId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                AppointmentDate = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                                TokenNumber = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                ConsultationStatus = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                CreatedAt = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                CreatedDate = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8),
                                IsActive = reader.IsDBNull(9) ? false : reader.GetBoolean(9)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllAppointments: " + ex.Message);
            }

            return appointments;
        }
        #endregion

        #region 27- Get Appointments by Date
        public async Task<List<TblAppointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            var appointments = new List<TblAppointment>();
            try
            {
                using (SqlConnection conn = new SqlConnection(winConnString))
                {
                    await conn.OpenAsync();
                    SqlCommand cmd = new SqlCommand("sp_GetAppointmentsByDate", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Date", date.Date);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            appointments.Add(new TblAppointment
                            {
                                AppointmentId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                PatientId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                DoctorId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                                UserId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                AppointmentDate = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                                TokenNumber = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                ConsultationStatus = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                CreatedAt = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                CreatedDate = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8),
                                IsActive = reader.IsDBNull(9) ? false : reader.GetBoolean(9)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAppointmentsByDate: " + ex.Message);
            }

            return appointments;
        }
        #endregion

    }
}
