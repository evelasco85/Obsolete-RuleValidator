using Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuleValidator_Proto.Integration.Utility
{

    public class CSVDataExtractor : IDataExtractor
    {
        string[] _csvData;
        IDictionary<string, int> _columns;
        readonly string newLine = System.Environment.NewLine;
        int _startingHeaderRowIndex = 0;

        public int HeaderRowIndex
        {
            get { return _startingHeaderRowIndex; }
        }

        public CSVDataExtractor(StringBuilder csvTextContent)
        {
            this._csvData = csvTextContent.ToString().Split(newLine.ToCharArray()).ToArray();
            this._columns = new Dictionary<string, int>();

            this._columns = _csvData
                .Take(1)
                .Select(line => line.Split(','))
                .Select(
                    columns => columns
                                    .Where(column => !string.IsNullOrEmpty(column))     //Skip empty column name
                                    .Select((column, index) =>
                                            new { index, column }
                                        )
                                        .ToDictionary(a => a.column, a => a.index)
                       ).FirstOrDefault();
        }

        public int GetRecordCount()     //Inclusive of header
        {
            int recordCount = 0;

            if ((this._csvData == null) || (this._csvData.Count() == 1))     //If no value or contains header only
                return recordCount;

            recordCount = this._csvData.Count();

            return recordCount;
        }

        public int GetColumnOrdinal(string columnName)
        {
            int ordinal = -1;

            this._columns.TryGetValue(columnName, out ordinal);

            return ordinal;
        }

        public string GetColumnData(int csvLineIndex, string columnName)
        {
            string columnValue = string.Empty;
            string[] csvData = this._csvData;
            int columnNameOrdinal = this.GetColumnOrdinal(columnName);
            string[] columnValues = csvData[csvLineIndex].Split(',');        //contain headers

            if (columnNameOrdinal < columnValues.Count())
                columnValue = columnValues[columnNameOrdinal];

            return columnValue;
        }


        public string GetColumnData(int csvLineIndex, int ordinalNumber)
        {
            throw new NotImplementedException();
        }
    }

}
