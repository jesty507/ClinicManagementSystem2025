using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblMembership
    {
        //MembershipId INT PRIMARY KEY IDENTITY(1,1),
        public int MembershipId { get; set; }
        //MembershipType NVARCHAR(50),
        public string? MembershipType { get; set; }
        //CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        public DateTime CreatedDate { get; set; }
        //IsActive BIT DEFAULT 1
        public bool IsActive { get; set; }
    }
}