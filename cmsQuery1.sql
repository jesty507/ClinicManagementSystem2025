
-- TblRole==============================================================================================
CREATE TABLE TblRole (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(100) NOT NULL UNIQUE, -- e.g., Doctor, Receptionist
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);
-- TblUser==============================================================================================
CREATE TABLE TblUser (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(150) NOT NULL,
    Gender NVARCHAR(10),
    DateOfBirth DATE,
    Email NVARCHAR(150) UNIQUE,
    MobileNumber NVARCHAR(15),
    Address NVARCHAR(300),
	Place NVARCHAR(300),
    BloodGroup NVARCHAR(5),
    UserName NVARCHAR(50) UNIQUE,
    Password NVARCHAR(200),
    RoleId INT FOREIGN KEY REFERENCES TblRole(RoleId),
    JoiningDate DATE,
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);


-- TblDoctor==============================================================================================
CREATE TABLE TblDoctor (
    DoctorId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT UNIQUE FOREIGN KEY REFERENCES TblUser(UserId),
    ConsultationFee DECIMAL(10,2),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);

CREATE TABLE TblSpecialization (
    SpecializationId INT IDENTITY(1,1) PRIMARY KEY,
    SpecializationName NVARCHAR(100) NOT NULL
);

CREATE TABLE TblDoctorSpecialization (
    DoctorSpecializationId INT IDENTITY(1,1) PRIMARY KEY,
    DoctorId INT NOT NULL FOREIGN KEY REFERENCES TblDoctor(DoctorId),
    SpecializationId INT NOT NULL FOREIGN KEY REFERENCES TblSpecialization(SpecializationId)
);

-- ==========================================
-- 6. Membership Table
-- ==========================================
CREATE TABLE TblMembership (
    MembershipId INT PRIMARY KEY IDENTITY(1,1),
    MembershipType NVARCHAR(50),
	CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);
SELECT * FROM TblMembership;


-- TblPatient==============================================================================================
CREATE TABLE TblPatient (
    PatientId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT UNIQUE FOREIGN KEY REFERENCES TblUser(UserId),
    PatientName NVARCHAR(150),
	DateOfBirth DATE,
	Gender NVARCHAR(10),
	BloodGroup NVARCHAR(3),
    Address NVARCHAR(200),
    MobileNumber NVARCHAR(15),
	MembershipId INT FOREIGN KEY REFERENCES TblMembership(MembershipId),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);


-- TblAppointment==============================================================================================
CREATE TABLE TblAppointment (
    AppointmentId INT PRIMARY KEY IDENTITY(1,1),
    PatientId INT FOREIGN KEY REFERENCES TblPatient(PatientId),
    DoctorId INT FOREIGN KEY REFERENCES TblDoctor(DoctorId),
	UserId INT FOREIGN KEY REFERENCES TblUser(UserId),
    AppointmentDate DATETIME NOT NULL,
    TokenNumber INT,
    ConsultationStatus NVARCHAR(50) DEFAULT 'Pending',
    CreatedBy INT FOREIGN KEY REFERENCES TblUser(UserId),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);

-- ================= CONSULTATION =================
CREATE TABLE TblConsultation (
    ConsultationId INT PRIMARY KEY IDENTITY(1,1),
    Symptoms NVARCHAR(MAX),
    Diagnosis NVARCHAR(MAX),
    Notes NVARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE(),
    AppointmentId INT FOREIGN KEY REFERENCES TblAppointment(AppointmentId),
    IsActive BIT DEFAULT 1,
    PatientId INT FOREIGN KEY REFERENCES TblPatient(PatientId)
);


--  TblLabCategory==============================================================================================
CREATE TABLE TblLabTestCategory (
    LabTesCategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) UNIQUE,
);

--  TblLabTest==============================================================================================
CREATE TABLE TblLabTest (
    LabTestId INT PRIMARY KEY IDENTITY(1,1),
    LabTestCategoryId INT FOREIGN KEY REFERENCES TblLabTestCategory(LabTesCategoryId),
    TestName NVARCHAR(100) NOT NULL,
    Amount DECIMAL(10,2),
    ReferenceMinRange NVARCHAR(50),
    ReferenceMaxRange NVARCHAR(50),
    SampleRequired NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1

);

-- TblLabTestPrescription==============================================================================================
CREATE TABLE TblLabTestPrescription (
    PrescriptionId INT PRIMARY KEY IDENTITY(1,1),
    AppointmentId INT FOREIGN KEY REFERENCES TblAppointment(AppointmentId),
    DoctorId INT FOREIGN KEY REFERENCES TblDoctor(DoctorId),
    PatientId INT FOREIGN KEY REFERENCES TblPatient(PatientId),
	LabTestId INT FOREIGN KEY REFERENCES TblLabTest(LabTestId),
    Notes NVARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- TblMedicineCategory==============================================================================================
CREATE TABLE TblMedicineCategory (
    MedicineCategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) UNIQUE,
);

-- TblMedicine==============================================================================================
CREATE TABLE TblMedicine (
    MedicineId INT PRIMARY KEY IDENTITY(1,1),
    MedicineCategoryId INT FOREIGN KEY REFERENCES TblMedicineCategory(MedicineCategoryId),
    MedicineName NVARCHAR(150) NOT NULL,
    ManufactureDate DATE,
	ExpiryDate DATE,
	UnitAvailable INT,
    Price DECIMAL(10,2),

);

-- TblPrescriptionMedicine==============================================================================================
CREATE TABLE TblPrescriptionMedicine (
    PrescriptionMedicineId INT PRIMARY KEY IDENTITY(1,1),
    MedicineId INT FOREIGN KEY REFERENCES TblMedicine(MedicineId),
    Dosage NVARCHAR(50),
    Duration NVARCHAR(50),
    Instructions NVARCHAR(200),
	AppointmentId INT FOREIGN KEY REFERENCES TblAppointment(AppointmentId),
);

--  TblConsultationBill==============================================================================================
CREATE TABLE TblConsultationBill (
    BillId INT PRIMARY KEY IDENTITY(1,1),
    AppointmentId INT FOREIGN KEY REFERENCES TblAppointment(AppointmentId),
	PatientId INT FOREIGN KEY REFERENCES TblPatient(PatientId),
    ConsultationFee DECIMAL(10,2),
    LabCharges DECIMAL(10,2),
    MedicineCharges DECIMAL(10,2),
    TotalAmount AS (ConsultationFee + LabCharges + MedicineCharges) PERSISTED,
    PaymentStatus NVARCHAR(50) DEFAULT 'Unpaid',
    PaymentDate DATETIME NULL,
    CreatedDate DATETIME DEFAULT GETDATE()
);

