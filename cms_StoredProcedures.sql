CREATE OR ALTER PROCEDURE sp_AuthenticateUserByRoleId
    @UserName NVARCHAR(100),
    @Password NVARCHAR(100),
    @RoleId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @RoleId = RoleId
    FROM TblUser
    WHERE UserName = @UserName
      AND Password = @Password
      AND IsActive = 1;
END

-- =================================================

CREATE OR ALTER PROCEDURE sp_SearchPatientByMRN
    @PatientId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.PatientId,
        p.UserId,
        p.PatientName,
        p.DateOfBirth,
        p.Gender,
        p.BloodGroup,
        p.Address,
        p.MobileNumber,
        p.MembershipId,
        p.CreatedDate,
        p.IsActive
    FROM TblPatient p
    WHERE p.PatientId = @PatientId AND p.IsActive = 1
END

-- ====================================================

CREATE OR ALTER PROCEDURE sp_SearchPatientByMobile
    @MobileNumber NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.PatientId,
        p.UserId,
        p.PatientName,
        p.DateOfBirth,
        p.Gender,
        p.BloodGroup,
        p.Address,
        p.MobileNumber,
        p.MembershipId,
        p.CreatedDate,
        p.IsActive
    FROM TblPatient p
    WHERE p.MobileNumber = @MobileNumber AND p.IsActive = 1
END

-- ======================================================

CREATE OR ALTER PROCEDURE sp_GetDoctorAppointmentsForToday
    @DoctorId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        AppointmentId,
        AppointmentDate,
        TokenNumber,
        ConsultationStatus,
        PatientId,
        DoctorId,
        UserId,
        CreatedDate,
        IsActive
    FROM TblAppointment
    WHERE DoctorId = @DoctorId
      AND CAST(AppointmentDate AS DATE) = CAST(GETDATE() AS DATE)
      AND IsActive = 1
    ORDER BY TokenNumber;
END

-- ===================================================================

CREATE OR ALTER PROCEDURE sp_GetDoctorIdByUserName
    @Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT d.DoctorId
    FROM TblDoctor d
    INNER JOIN TblUser u ON d.UserId = u.UserId  -- Fixed: was TblUsers
    WHERE u.UserName = @Username
      AND d.IsActive = 1
      AND u.IsActive = 1;
END

-- =====================================================================
ALTER TABLE TblPatient ALTER COLUMN UserId INT NULL;
ALTER TABLE TblPatient DROP CONSTRAINT UQ__TblPatie__1788CC4DAFBEB0D5;


CREATE OR ALTER PROCEDURE sp_CreateNewPatient
    @UserId INT = NULL,  -- Make it optional
    @PatientName NVARCHAR(150) = NULL,
    @DateOfBirth DATE = NULL,
    @Gender NVARCHAR(10) = NULL,
    @BloodGroup NVARCHAR(3) = NULL,
    @Address NVARCHAR(200) = NULL,
    @MobileNumber NVARCHAR(15) = NULL,
    @MembershipId INT,
    @PatientId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Create the patient record with NULL UserId
    INSERT INTO TblPatient (
        UserId, PatientName, DateOfBirth, Gender, BloodGroup, 
        Address, MobileNumber, MembershipId, CreatedDate, IsActive
    )
    VALUES (
        @UserId, @PatientName, @DateOfBirth, @Gender, @BloodGroup,
        @Address, @MobileNumber, @MembershipId, GETDATE(), 1
    );
    
    SET @PatientId = SCOPE_IDENTITY();
END
-- ==================================================================================================


CREATE OR ALTER PROCEDURE sp_CheckMobileNumberExists
    @MobileNumber NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE 
        WHEN EXISTS (SELECT 1 FROM TblPatient WHERE MobileNumber = @MobileNumber AND IsActive = 1)
        THEN 1 
        ELSE 0 
    END AS ExistsFlag;
END
-- ======================================================================================================


CREATE OR ALTER PROCEDURE sp_CheckMRNNumberExists
    @PatientId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CASE 
        WHEN EXISTS (SELECT 1 FROM TblPatient WHERE PatientId = @PatientId AND IsActive = 1)
        THEN 1 
        ELSE 0 
    END AS ExistsFlag;
END
-- ===========================================================================================



CREATE OR ALTER PROCEDURE sp_CreateAppointment
    @AppointmentDate DATETIME,
    @TokenNumber INT,
    @ConsultationStatus NVARCHAR(50),
    @PatientId INT,
    @DoctorId INT,
    @UserId INT,
    @CreatedBy INT,
    @AppointmentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO TblAppointment (
        PatientId, DoctorId, UserId, AppointmentDate, TokenNumber, 
        ConsultationStatus, CreatedBy, CreatedDate, IsActive
    )
    VALUES (
        @PatientId, @DoctorId, @UserId, @AppointmentDate, @TokenNumber,
        @ConsultationStatus, @CreatedBy, GETDATE(), 1
    );

    SET @AppointmentId = SCOPE_IDENTITY();
END
-- ---------------------------------------------------------------------------



CREATE OR ALTER PROCEDURE sp_GetAllDoctors
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        d.DoctorId,
        u.FullName AS DoctorName,
        d.ConsultationFee,
        d.UserId,
        d.CreatedDate,
        d.IsActive,
        STRING_AGG(ISNULL(s.SpecializationName, ''), ', ') AS SpecializationNames
    FROM TblDoctor d
    INNER JOIN TblUser u ON d.UserId = u.UserId
    LEFT JOIN TblDoctorSpecialization ds ON d.DoctorId = ds.DoctorId
    LEFT JOIN TblSpecialization s ON ds.SpecializationId = s.SpecializationId
    WHERE d.IsActive = 1 AND u.IsActive = 1
    GROUP BY d.DoctorId, u.FullName, d.ConsultationFee, d.UserId, d.CreatedDate, d.IsActive
    ORDER BY u.FullName;
END
-- ==========================================================================================

CREATE OR ALTER PROCEDURE sp_GetNextTokenNumber
    @DoctorId INT,
    @AppointmentDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ISNULL(MAX(TokenNumber), 0) AS NextTokenNumber
    FROM TblAppointment
    WHERE DoctorId = @DoctorId
      AND CAST(AppointmentDate AS DATE) = @AppointmentDate
      AND IsActive = 1;
END
-- ============================================================

CREATE OR ALTER PROCEDURE sp_GetAllMedicines
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        m.MedicineId,
        m.MedicineCategoryId,
        m.MedicineName,
        m.ManufactureDate,
        m.ExpiryDate,
        m.UnitAvailable,
        m.Price
    FROM TblMedicine m
    WHERE m.UnitAvailable > 0
    ORDER BY m.MedicineName;
END
-- ==================================================================

--  Get All Medicine Categories
CREATE OR ALTER PROCEDURE sp_GetAllMedicineCategories
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        mc.MedicineCategoryId,
        mc.CategoryName
    FROM TblMedicineCategory mc
    ORDER BY mc.CategoryName;
END
-- ========================================================================

--  Create Prescription Medicine
CREATE OR ALTER PROCEDURE sp_CreatePrescriptionMedicine
    @MedicineId INT,
    @Dosage NVARCHAR(50) = NULL,
    @Duration NVARCHAR(50) = NULL,
    @Instructions NVARCHAR(200) = NULL,
    @AppointmentId INT,
    @PrescriptionMedicineId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO TblPrescriptionMedicine (
        MedicineId, Dosage, Duration, Instructions, AppointmentId
    )
    VALUES (
        @MedicineId, @Dosage, @Duration, @Instructions, @AppointmentId
    );
    
    SET @PrescriptionMedicineId = SCOPE_IDENTITY();
END
-- ==============================================================================

--  Get Prescriptions by Appointment ID
CREATE OR ALTER PROCEDURE sp_GetPrescriptionsByAppointmentId
    @AppointmentId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        pm.PrescriptionMedicineId,
        pm.MedicineId,
        pm.Dosage,
        pm.Duration,
        pm.Instructions,
        pm.AppointmentId
    FROM TblPrescriptionMedicine pm
    WHERE pm.AppointmentId = @AppointmentId
    ORDER BY pm.PrescriptionMedicineId;
END
-- ======================================================================================

CREATE OR ALTER PROCEDURE sp_GetDoctorAppointmentsForDate
    @DoctorId INT,
    @AppointmentDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.AppointmentId,
        a.AppointmentDate,
        a.TokenNumber,
        a.ConsultationStatus,
        a.PatientId,
        a.DoctorId,
        a.UserId,
        a.CreatedDate,
        a.IsActive
    FROM TblAppointment a
    WHERE a.DoctorId = @DoctorId
        AND CAST(a.AppointmentDate AS DATE) = @AppointmentDate
        AND a.IsActive = 1
    ORDER BY a.TokenNumber, a.AppointmentDate;
END
-- ==========================================================================================

-- Create Consultation
CREATE OR ALTER PROCEDURE sp_CreateConsultation
    @Symptoms NVARCHAR(MAX) = NULL,
    @Diagnosis NVARCHAR(MAX) = NULL,
    @Notes NVARCHAR(MAX) = NULL,
    @AppointmentId INT,
    @PatientId INT,
    @ConsultationId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO TblConsultation (
        Symptoms, Diagnosis, Notes, AppointmentId, PatientId, CreatedDate, IsActive
    )
    VALUES (
        @Symptoms, @Diagnosis, @Notes, @AppointmentId, @PatientId, GETDATE(), 1
    );
    
    SET @ConsultationId = SCOPE_IDENTITY();
END
-- ==========================================================================================

-- Get Consultation by Appointment ID
CREATE OR ALTER PROCEDURE sp_GetConsultationByAppointmentId
    @AppointmentId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.ConsultationId,
        c.Symptoms,
        c.Diagnosis,
        c.Notes,
        c.CreatedDate,
        c.AppointmentId,
        c.IsActive,
        c.PatientId
    FROM TblConsultation c
    WHERE c.AppointmentId = @AppointmentId
        AND c.IsActive = 1;
END
-- ==========================================================================================

-- Get All Lab Tests
CREATE OR ALTER PROCEDURE sp_GetAllLabTests
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lt.LabTestId,
        lt.LabTestCategoryId,
        lt.TestName,
        lt.Amount,
        lt.ReferenceMinRange,
        lt.ReferenceMaxRange,
        lt.SampleRequired,
        lt.CreatedDate,
        lt.IsActive
    FROM TblLabTest lt
    WHERE lt.IsActive = 1
    ORDER BY lt.TestName;
END
-- ==========================================================================================

-- Get All Lab Test Categories
CREATE OR ALTER PROCEDURE sp_GetAllLabTestCategories
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ltc.LabTesCategoryId,
        ltc.CategoryName
    FROM TblLabTestCategory ltc
    ORDER BY ltc.CategoryName;
END
-- ==========================================================================================

-- Create Lab Test Prescription
CREATE OR ALTER PROCEDURE sp_CreateLabTestPrescription
    @AppointmentId INT,
    @DoctorId INT,
    @PatientId INT,
    @LabTestId INT,
    @Notes NVARCHAR(MAX) = NULL,
    @PrescriptionId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO TblLabTestPrescription (
        AppointmentId, DoctorId, PatientId, LabTestId, Notes, CreatedDate
    )
    VALUES (
        @AppointmentId, @DoctorId, @PatientId, @LabTestId, @Notes, GETDATE()
    );
    
    SET @PrescriptionId = SCOPE_IDENTITY();
END

-- ===================================================================================

-- Get Lab Test Prescriptions by Appointment ID
CREATE OR ALTER PROCEDURE sp_GetLabTestPrescriptionsByAppointmentId
    @AppointmentId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ltp.PrescriptionId,
        ltp.AppointmentId,
        ltp.DoctorId,
        ltp.PatientId,
        ltp.LabTestId,
        ltp.Notes,
        ltp.CreatedDate
    FROM TblLabTestPrescription ltp
    WHERE ltp.AppointmentId = @AppointmentId
    ORDER BY ltp.PrescriptionId;
END

-- ==========================================================================================

-- Create Consultation Bill
CREATE OR ALTER PROCEDURE sp_CreateConsultationBill
    @AppointmentId INT,
    @PatientId INT,
    @ConsultationFee DECIMAL(10,2),
    @LabCharges DECIMAL(10,2),
    @MedicineCharges DECIMAL(10,2),
    @PaymentStatus NVARCHAR(50),
    @BillId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO TblConsultationBill (
        AppointmentId, 
        PatientId, 
        ConsultationFee, 
        LabCharges, 
        MedicineCharges, 
        PaymentStatus, 
        CreatedDate
    )
    VALUES (
        @AppointmentId, 
        @PatientId, 
        @ConsultationFee, 
        @LabCharges, 
        @MedicineCharges, 
        @PaymentStatus, 
        GETDATE()
    );
    
    SET @BillId = SCOPE_IDENTITY();
END
-- =======================================================================================

-- Get Consultation Bill by Appointment ID
CREATE OR ALTER PROCEDURE sp_GetConsultationBillByAppointmentId
    @AppointmentId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        b.BillId,
        b.AppointmentId,
        b.PatientId,
        b.ConsultationFee,
        b.LabCharges,
        b.MedicineCharges,
        b.TotalAmount,
        b.PaymentStatus,
        b.PaymentDate,
        b.CreatedDate,
        p.PatientName,
        u.FullName AS DoctorName
    FROM TblConsultationBill b
    INNER JOIN TblAppointment a ON b.AppointmentId = a.AppointmentId
    INNER JOIN TblPatient p ON b.PatientId = p.PatientId
    INNER JOIN TblDoctor d ON a.DoctorId = d.DoctorId
    INNER JOIN TblUser u ON d.UserId = u.UserId
    WHERE b.AppointmentId = @AppointmentId
END
-- ======================================================================================

-- Get All Bills
CREATE OR ALTER PROCEDURE sp_GetAllBills
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        b.BillId,
        b.AppointmentId,
        b.PatientId,
        b.ConsultationFee,
        b.LabCharges,
        b.MedicineCharges,
        b.TotalAmount,
        b.PaymentStatus,
        b.PaymentDate,
        b.CreatedDate,
        p.PatientName,
        u.FullName AS DoctorName
    FROM TblConsultationBill b
    INNER JOIN TblAppointment a ON b.AppointmentId = a.AppointmentId
    INNER JOIN TblPatient p ON b.PatientId = p.PatientId
    INNER JOIN TblDoctor d ON a.DoctorId = d.DoctorId
    INNER JOIN TblUser u ON d.UserId = u.UserId
    ORDER BY b.CreatedDate DESC
END
-- =======================================================================================================

-- Get Bills by Date
CREATE OR ALTER PROCEDURE sp_GetBillsByDate
    @Date DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        b.BillId,
        b.AppointmentId,
        b.PatientId,
        b.ConsultationFee,
        b.LabCharges,
        b.MedicineCharges,
        b.TotalAmount,
        b.PaymentStatus,
        b.PaymentDate,
        b.CreatedDate,
        p.PatientName,
        u.FullName AS DoctorName
    FROM TblConsultationBill b
    INNER JOIN TblAppointment a ON b.AppointmentId = a.AppointmentId
    INNER JOIN TblPatient p ON b.PatientId = p.PatientId
    INNER JOIN TblDoctor d ON a.DoctorId = d.DoctorId
    INNER JOIN TblUser u ON d.UserId = u.UserId
    WHERE CAST(b.CreatedDate AS DATE) = @Date
    ORDER BY b.CreatedDate DESC
END
-- =============================================================================================

-- Get All Appointments
CREATE OR ALTER PROCEDURE sp_GetAllAppointments
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.AppointmentId,
        a.PatientId,
        a.DoctorId,
        a.UserId,
        a.AppointmentDate,
        a.TokenNumber,
        a.ConsultationStatus,
        a.CreatedBy,
        a.CreatedDate,
        a.IsActive
    FROM TblAppointment a
    WHERE a.IsActive = 1
    ORDER BY a.AppointmentDate DESC
END
-- =====================================================================================

-- Get Appointments by Date
CREATE OR ALTER PROCEDURE sp_GetAppointmentsByDate
    @Date DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.AppointmentId,
        a.PatientId,
        a.DoctorId,
        a.UserId,
        a.AppointmentDate,
        a.TokenNumber,
        a.ConsultationStatus,
        a.CreatedBy,
        a.CreatedDate,
        a.IsActive
    FROM TblAppointment a
    WHERE CAST(a.AppointmentDate AS DATE) = @Date 
    AND a.IsActive = 1
    ORDER BY a.AppointmentDate ASC
END
-- ====================================================================================

