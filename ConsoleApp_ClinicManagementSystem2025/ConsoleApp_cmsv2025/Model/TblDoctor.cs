using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblDoctor
    {
        //DoctorId INT PRIMARY KEY IDENTITY(1,1),
        public int DoctorId { get; set; }
        //UserId INT UNIQUE FOREIGN KEY REFERENCES TblUser(UserId),
        public int UserId { get; set; }
        //ConsultationFee DECIMAL(10,2),
        public decimal ConsultationFee { get; set; }
        //CreatedDate DATETIME DEFAULT GETDATE(),
        public DateTime CreatedDate { get; set; }
        //IsActive BIT DEFAULT 1
        public bool IsActive { get; set; }
        
        // Additional properties from JOINs
        public string DoctorName { get; set; } = string.Empty; // From TblUser.FullName
        public string SpecializationNames { get; set; } = string.Empty; // From aggregated specializations
    }
}