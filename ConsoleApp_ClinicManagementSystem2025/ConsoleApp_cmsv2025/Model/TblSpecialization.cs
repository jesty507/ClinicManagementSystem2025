using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblSpecialization
    {
        //SpecializationId INT IDENTITY(1,1) PRIMARY KEY,
        public int SpecializationId { get; set; }
        //SpecializationName NVARCHAR(100) NOT NULL
        public string SpecializationName { get; set; } = string.Empty;
    }
}