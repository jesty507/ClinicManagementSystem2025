using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblRole
    {
        //RoleId INT PRIMARY KEY IDENTITY(1,1),
        public int RoleId { get; set; }
        //RoleName NVARCHAR(100) NOT NULL UNIQUE,
        public string RoleName { get; set; } = string.Empty;
        //IsActive BIT DEFAULT 1,
        public bool IsActive { get; set; }
        //CreatedDate DATETIME DEFAULT GETDATE()
        public DateTime CreatedDate { get; set; }
    }
}