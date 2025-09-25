using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblPatient
    {
        //PatientId INT PRIMARY KEY IDENTITY(1,1),
        public int PatientId { get; set; }
        //UserId INT UNIQUE FOREIGN KEY REFERENCES TblUser(UserId),
        public int UserId { get; set; }
        //PatientName NVARCHAR(150),
        public string? PatientName { get; set; }
        //DateOfBirth DATE,
        public DateTime? DateOfBirth { get; set; }
        //Gender NVARCHAR(10),
        public string? Gender { get; set; }
        //BloodGroup NVARCHAR(3),
        public string? BloodGroup { get; set; }
        //Address NVARCHAR(200),
        public string? Address { get; set; }
        //MobileNumber NVARCHAR(15),
        public string? MobileNumber { get; set; }
        //MembershipId INT FOREIGN KEY REFERENCES TblMembership(MembershipId),
        public int MembershipId { get; set; }
        //CreatedDate DATETIME DEFAULT GETDATE(),
        public DateTime CreatedDate { get; set; }
        //IsActive BIT DEFAULT 1
        public bool IsActive { get; set; }
    }
}