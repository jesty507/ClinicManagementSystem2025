using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblPrescriptionMedicine
    {
        //PrescriptionMedicineId INT PRIMARY KEY IDENTITY(1,1),
        public int PrescriptionMedicineId { get; set; }
        //MedicineId INT FOREIGN KEY REFERENCES TblMedicine(MedicineId),
        public int MedicineId { get; set; }
        //Dosage NVARCHAR(50),
        public string? Dosage { get; set; }
        //Duration NVARCHAR(50),
        public string? Duration { get; set; }
        //Instructions NVARCHAR(200),
        public string? Instructions { get; set; }
        //AppointmentId INT FOREIGN KEY REFERENCES TblAppointment(AppointmentId),
        public int AppointmentId { get; set; }
    }
}
