using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblUser
    {
        //UserId INT PRIMARY KEY IDENTITY(1,1),
        public int UserId { get; set; }
        //FullName NVARCHAR(150) NOT NULL,
        public string FullName { get; set; } = string.Empty;
        //Gender NVARCHAR(10),
        public string? Gender { get; set; }
        //DateOfBirth DATE,
        public DateTime? DateOfBirth { get; set; }
        //Email NVARCHAR(150) UNIQUE,
        public string? Email { get; set; }
        //MobileNumber NVARCHAR(15),
        public string? MobileNumber { get; set; }
        //Address NVARCHAR(300),
        public string? Address { get; set; }
        //Place NVARCHAR(300),
        public string? Place { get; set; }
        //BloodGroup NVARCHAR(5),
        public string? BloodGroup { get; set; }
        //UserName NVARCHAR(50) UNIQUE,
        public string? UserName { get; set; }
        //Password NVARCHAR(200),
        public string? Password { get; set; }
        //RoleId INT FOREIGN KEY REFERENCES TblRole(RoleId),
        public int RoleId { get; set; }
        //JoiningDate DATE,
        public DateTime? JoiningDate { get; set; }
        //CreatedDate DATETIME DEFAULT GETDATE(),
        public DateTime CreatedDate { get; set; }
        //IsActive BIT DEFAULT 1
        public bool IsActive { get; set; }
    }
}