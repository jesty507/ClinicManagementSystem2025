using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblMedicine
    {
        //MedicineId INT PRIMARY KEY IDENTITY(1,1),
        public int MedicineId { get; set; }
        //MedicineCategoryId INT FOREIGN KEY REFERENCES TblMedicineCategory(MedicineCategoryId),
        public int MedicineCategoryId { get; set; }
        //MedicineName NVARCHAR(150) NOT NULL,
        public string MedicineName { get; set; } = string.Empty;
        //ManufactureDate DATE,
        public DateTime? ManufactureDate { get; set; }
        //ExpiryDate DATE,
        public DateTime? ExpiryDate { get; set; }
        //UnitAvailable INT,
        public int UnitAvailable { get; set; }
        //Price DECIMAL(10,2),
        public decimal Price { get; set; }
    }
}
