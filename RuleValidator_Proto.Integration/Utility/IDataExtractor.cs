using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleValidator_Proto.Integration.Utility
{
    public interface IDataExtractor
    {
        int GetColumnOrdinal(string columnName);
        string GetColumnData(int rowIndex, int ordinalNumber);
        string GetColumnData(int rowIndex, string columnName);

        int HeaderRowIndex { get; }
        int GetRecordCount();
    }
}
