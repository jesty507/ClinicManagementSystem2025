using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblConsultation
    {
        //ConsultationId INT PRIMARY KEY IDENTITY(1,1),
        public int ConsultationId { get; set; }
        //Symptoms NVARCHAR(MAX),
        public string? Symptoms { get; set; }
        //Diagnosis NVARCHAR(MAX),
        public string? Diagnosis { get; set; }
        //Notes NVARCHAR(MAX),
        public string? Notes { get; set; }
        //CreatedDate DATETIME DEFAULT GETDATE(),
        public DateTime CreatedDate { get; set; }
        //AppointmentId INT FOREIGN KEY REFERENCES TblAppointment(AppointmentId),
        public int AppointmentId { get; set; }
        //IsActive BIT DEFAULT 1,
        public bool IsActive { get; set; }
        //PatientId INT FOREIGN KEY REFERENCES TblPatient(PatientId)
        public int PatientId { get; set; }
    }
}
