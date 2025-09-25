using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblAppointment
    {
        //AppointmentId INT PRIMARY KEY IDENTITY(1,1),
        public int AppointmentId { get; set; }
        //PatientId INT FOREIGN KEY REFERENCES TblPatient(PatientId),
        public int PatientId { get; set; }
        //DoctorId INT FOREIGN KEY REFERENCES TblDoctor(DoctorId),
        public int DoctorId { get; set; }
        //UserId INT FOREIGN KEY REFERENCES TblUser(UserId),
        public int UserId { get; set; }
        //AppointmentDate DATETIME NOT NULL,
        public DateTime AppointmentDate { get; set; }
        //TokenNumber INT,
        public int TokenNumber { get; set; }
        //ConsultationStatus NVARCHAR(50) DEFAULT 'Pending',
        public string ConsultationStatus { get; set; } = "Pending";
        //CreatedBy INT FOREIGN KEY REFERENCES TblUser(UserId),
        public int CreatedAt { get; set; }
        //CreatedDate DATETIME DEFAULT GETDATE(),
        public DateTime CreatedDate { get; set; }
        //IsActive BIT DEFAULT 1
        public bool IsActive { get; set; }
    }
}