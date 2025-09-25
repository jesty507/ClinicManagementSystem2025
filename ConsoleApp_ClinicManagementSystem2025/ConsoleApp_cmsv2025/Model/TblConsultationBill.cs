using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblConsultationBill
    {
        //BillId INT PRIMARY KEY IDENTITY(1,1),
        public int BillId { get; set; }
        //AppointmentId INT FOREIGN KEY REFERENCES TblAppointment(AppointmentId),
        public int AppointmentId { get; set; }
        //PatientId INT FOREIGN KEY REFERENCES TblPatient(PatientId),
        public int PatientId { get; set; }
        //ConsultationFee DECIMAL(10,2),
        public decimal ConsultationFee { get; set; }
        //LabCharges DECIMAL(10,2),
        public decimal LabCharges { get; set; }
        //MedicineCharges DECIMAL(10,2),
        public decimal MedicineCharges { get; set; }
        //TotalAmount AS (ConsultationFee + LabCharges + MedicineCharges) PERSISTED,
        public decimal TotalAmount { get; set; }
        //PaymentStatus NVARCHAR(50) DEFAULT 'Unpaid',
        public string PaymentStatus { get; set; } = "Unpaid";
        //PaymentDate DATETIME NULL,
        public DateTime? PaymentDate { get; set; }
        //CreatedDate DATETIME DEFAULT GETDATE()
        public DateTime CreatedDate { get; set; }
        
        // Additional properties for display
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
    }
}
