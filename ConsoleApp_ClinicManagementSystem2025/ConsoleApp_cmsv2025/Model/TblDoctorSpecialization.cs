using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblDoctorSpecialization
    {
        //DoctorSpecializationId INT IDENTITY(1,1) PRIMARY KEY,
        public int DoctorSpecializationId { get; set; }
        //DoctorId INT NOT NULL FOREIGN KEY REFERENCES TblDoctor(DoctorId),
        public int DoctorId { get; set; }
        //SpecializationId INT NOT NULL FOREIGN KEY REFERENCES TblSpecialization(SpecializationId)
        public int SpecializationId { get; set; }
    }
}