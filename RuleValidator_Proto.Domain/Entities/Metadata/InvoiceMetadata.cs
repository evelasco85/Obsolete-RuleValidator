using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleValidator_Proto.Domain.Entities.Metadata
{
    public class InvoiceMetadata
    {      


        [Display(Name = "Customer")]
        public string Customer_Name { get; set; }

        [Display(Name = "Paying Site")]
        public string Paying_Site { get; set; }

        [Display(Name = "Invoice Template")]
        public string Invoice_Template { get; set; }

        [Display(Name = "Category")]
        public string Category { get; set; }

        [Display(Name = "Currency")]
        public string Currency_Code { get; set; }

        [Display(Name = "Article No.")]
        public string Article_Number { get; set; }

        [Display(Name = "Description")]
        public string Article_Description { get; set; }

        [Display(Name = "No. of Transactions")]
        public Nullable<int> No_Of_Transactions { get; set; }


        [Display(Name = "Price")]
        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Display(Name = "VAT")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public Nullable<decimal> VAT { get; set; }

        [Display(Name = "Net")]
        [DisplayFormat(DataFormatString = "{0:N4}", ApplyFormatInEditMode = true)]
        public decimal Net { get; set; }

        [Display(Name = "File Id")]
        public int File_Id { get; set; }
    }
}
