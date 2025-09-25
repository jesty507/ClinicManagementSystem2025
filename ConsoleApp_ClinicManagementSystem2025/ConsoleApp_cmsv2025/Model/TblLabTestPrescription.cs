using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblLabTestPrescription
    {
        //PrescriptionId INT PRIMARY KEY IDENTITY(1,1),
        public int PrescriptionId { get; set; }
        //AppointmentId INT FOREIGN KEY REFERENCES TblAppointment(AppointmentId),
        public int AppointmentId { get; set; }
        //DoctorId INT FOREIGN KEY REFERENCES TblDoctor(DoctorId),
        public int DoctorId { get; set; }
        //PatientId INT FOREIGN KEY REFERENCES TblPatient(PatientId),
        public int PatientId { get; set; }
        //LabTestId INT FOREIGN KEY REFERENCES TblLabTest(LabTestId),
        public int LabTestId { get; set; }
        //Notes NVARCHAR(MAX),
        public string? Notes { get; set; }
        //CreatedDate DATETIME DEFAULT GETDATE()
        public DateTime CreatedDate { get; set; }
    }
}
