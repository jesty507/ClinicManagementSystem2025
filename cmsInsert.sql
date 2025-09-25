-- 1. Roles (Only Doctor and Receptionist)
INSERT INTO TblRole (RoleName) VALUES
('Doctor'), ('Receptionist');

-- 2. Users (Only Doctor and Receptionist)
INSERT INTO TblUser (FullName, Gender, DateOfBirth, Email, MobileNumber, Address, Place, BloodGroup, UserName, Password, RoleId, JoiningDate)
VALUES
('Dr. Anoop Ravi', 'Male', '1980-05-12', 'john.smith@clinic.com', '9876543210', '123 Main St', 'CityA', 'O+', 'dranoop', 'Anoop@123', 1, '2020-01-10'),
('Dr. Rosmi Jose', 'Female', '1988-05-22', 'rosmi.jose@clinic.com', '6238917217', 'Rosmi Villa', 'Kochi', 'B+', 'drrosmi', 'Rosmi@123', 1, '2022-05-16'),
('Dr. Samuel Thomas', 'Male', '1982-12-08', 'samuel.thomas@clinic.com', '8848051710', 'Thomas Cottage', 'Kollam', 'B+', 'drsam', 'Sam@123', 1, '2015-05-16'),
('Jeny Williams', 'Female', '1995-07-20', 'alice.johnson@clinic.com', '9876543211', '456 Park Ave', 'CityB', 'A+', 'jenyrecep', 'Jeny@123', 2, '2021-03-15');
select * from TblUser;

-- 3. Doctor (Only Dr. John Smith)
INSERT INTO TblDoctor (UserId, ConsultationFee)
VALUES
(1, 500.00),
(4,350.00),
(5,600.00);
select * from TblDoctor;


-- 4. Specializations
INSERT INTO TblSpecialization (SpecializationName) VALUES
('Cardiology'), ('Dermatology'), ('Pediatrics'), ('Orthopedics');
select * from TblSpecialization;

-- 5. DoctorSpecialization
INSERT INTO TblDoctorSpecialization (DoctorId, SpecializationId)
VALUES
(1, 1),  -- Dr. John Smith - Cardiology
(1, 3),  -- Dr. John Smith - Pediatrics
(2,2),	 -- Dr. Rosmi - Dermatology
(3,4);	 -- Dr. Thomas - Orthopdics

-- 6. Membership
INSERT INTO TblMembership (MembershipType) VALUES
('Gold'), ('Silver'), ('Platinum');

-- 7. Sample Patient (for testing)
INSERT INTO TblPatient (UserId, PatientName, DateOfBirth, Gender, BloodGroup, Address, MobileNumber, MembershipId)
VALUES
(2, 'Test Patient', '1990-01-01', 'Female', 'A+', '123 Test St', '9876543214', 1);
select * from tblpatient;
delete from TblPatient where PatientId = 9;

-- 8. Sample Appointment
INSERT INTO TblAppointment (PatientId, DoctorId, UserId, AppointmentDate, TokenNumber, CreatedBy)
VALUES
(1, 1, 2, '2025-01-22 10:30:00', 1, 2);

select * from TblAppointment;
delete from TblAppointment where PatientId = 1;

-- 9. Sample Consultation
INSERT INTO TblConsultation (Symptoms, Diagnosis, Notes, AppointmentId, PatientId)
VALUES
('Fever, cough', 'Viral Infection', 'Rest and hydration recommended', 1, 1);

select * from TblConsultation;
delete from TblConsultation where PatientId = 1;

-- 10. Lab Test Category
INSERT INTO TblLabTestCategory (CategoryName) VALUES
('Blood Test'), ('X-Ray');

-- 11. Lab Test
INSERT INTO TblLabTest (LabTestCategoryId, TestName, Amount, ReferenceMinRange, ReferenceMaxRange, SampleRequired)
VALUES
(1, 'Complete Blood Count', 300.00, '4.0', '10.0', 'Blood Sample'),
(2, 'Chest X-Ray', 700.00, NULL, NULL, 'X-Ray');

-- 12. Lab Test Prescription
INSERT INTO TblLabTestPrescription (AppointmentId, DoctorId, PatientId, LabTestId, Notes)
VALUES
(1, 1, 1, 1, 'Check blood count for infection');
select * from TblLabTestPrescription
delete from TblLabTestPrescription where PatientId = 1;


-- 13. Medicine Category
INSERT INTO TblMedicineCategory (CategoryName) VALUES
('Antibiotics'), ('Painkillers'), ('Vitamin'), ('Gastro_resistant');

-- 14. Medicine
INSERT INTO TblMedicine (MedicineCategoryId, MedicineName, ManufactureDate, ExpiryDate, UnitAvailable, Price)
VALUES
(1, 'Amoxicillin 500mg', '2024-01-01', '2026-01-01', 100, 25.00),
(2, 'Paracetamol 500mg', '2024-02-01', '2027-02-01', 200, 10.00),
(3, 'B-Complex 20mg', '2025-01-02','2027-02-15',200, 15.00),
(4, 'Pantop 20mg', '2025-01-02','2027-02-15',300, 12.00);

-- 15. Prescription Medicine
INSERT INTO TblPrescriptionMedicine (MedicineId, Dosage, Duration, Instructions, AppointmentId)
VALUES
(1, '500mg', '5 days', 'Take after meals', 1),
(2, '500mg', '3 days', 'Take with water', 1);
select * from TblPrescriptionMedicine


-- 16. Consultation Bill
INSERT INTO TblConsultationBill (AppointmentId, PatientId, ConsultationFee, LabCharges, MedicineCharges, PaymentStatus, PaymentDate)
VALUES
(1, 1, 500.00, 300.00, 60.00, 'Paid', '2025-01-22');
select * from TblConsultationBill
delete from TblConsultationBill where PatientId = 1;
