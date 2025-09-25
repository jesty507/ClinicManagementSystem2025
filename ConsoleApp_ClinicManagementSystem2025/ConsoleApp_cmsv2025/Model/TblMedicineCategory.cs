using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblMedicineCategory
    {
        //MedicineCategoryId INT PRIMARY KEY IDENTITY(1,1),
        public int MedicineCategoryId { get; set; }
        //CategoryName NVARCHAR(100) UNIQUE,
        public string CategoryName { get; set; } = string.Empty;
    }
}
