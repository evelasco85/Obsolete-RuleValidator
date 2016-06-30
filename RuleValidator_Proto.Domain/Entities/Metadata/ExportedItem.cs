using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using RuleValidator_Proto.Domain.Entities.CustomAttributes;

namespace RuleValidator_Proto.Domain.Entities.Metadata
{
    public class ExportedItem
    {
        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "Please enter a positive price")]
        public Nullable<decimal> Calculated_Price { get; set; }

        [Required(ErrorMessage="This should not be empty", AllowEmptyStrings=false)]
        public string Shipping_Company_Name { get; set; }

        [DataType(DataType.DateTime)]
        [UTCDateTimeKind]
        public Nullable<System.DateTime> Date_Registered { get; set; }
    }
}
