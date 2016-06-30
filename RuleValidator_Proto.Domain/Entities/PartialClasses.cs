using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuleValidator_Proto.Domain.Entities.Metadata;

///Bind data annotations to their respective entities
namespace RuleValidator_Proto.Domain.Entities
{
    [MetadataType(typeof(Metadata.ExportedItem))]
    public partial class ExportedItem { }     //An entity from entity framework

    [MetadataType(typeof(InvoiceMetadata))]
    public partial class sp_GetInvHedDetWithAdditionTransaction_Result { }

    [MetadataType(typeof(Metadata.File))]
    public partial class File { }
}
