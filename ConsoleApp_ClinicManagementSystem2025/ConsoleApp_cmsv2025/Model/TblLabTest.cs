using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_cmsv2025.Model
{
    public class TblLabTest
    {
        //LabTestId INT PRIMARY KEY IDENTITY(1,1),
        public int LabTestId { get; set; }
        //LabTestCategoryId INT FOREIGN KEY REFERENCES TblLabTestCategory(LabTesCategoryId),
        public int LabTestCategoryId { get; set; }
        //TestName NVARCHAR(100) NOT NULL,
        public string TestName { get; set; } = string.Empty;
        //Amount DECIMAL(10,2),
        public decimal Amount { get; set; }
        //ReferenceMinRange NVARCHAR(50),
        public string? ReferenceMinRange { get; set; }
        //ReferenceMaxRange NVARCHAR(50),
        public string? ReferenceMaxRange { get; set; }
        //SampleRequired NVARCHAR(100),
        public string? SampleRequired { get; set; }
        //CreatedDate DATETIME DEFAULT GETDATE(),
        public DateTime CreatedDate { get; set; }
        //IsActive BIT DEFAULT 1
        public bool IsActive { get; set; }
    }
}
