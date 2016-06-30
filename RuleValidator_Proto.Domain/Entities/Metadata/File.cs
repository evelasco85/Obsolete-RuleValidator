using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using RuleValidator_Proto.Domain.Entities.CustomAttributes;

namespace RuleValidator_Proto.Domain.Entities.Metadata
{
    public class File
    {
        [DataType(DataType.DateTime)]
        [UTCDateTimeKind]
        public Nullable<System.DateTime> uploadedDateTime { get; set; }

        [DataType(DataType.DateTime)]
        [UTCDateTimeKind]
        public Nullable<System.DateTime> ImportedDateTime { get; set; }

        [DataType(DataType.DateTime)]
        [UTCDateTimeKind]
        public Nullable<System.DateTime> ExportedDateTime { get; set; }
    }
}
