using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuleValidator_Proto.Domain.Entities;

namespace RuleValidator_Proto.Tests.Domain.Concrete
{
      public class TableData
    {
        string[] _csvData;
        IDictionary<string, int> _columns;

        public TableData()
        {
            this._csvData = GetRuleListData();
            this._columns = new Dictionary<string, int>();

            this._columns = _csvData
                .Take(1)
                .Select(line => line.Split(','))
                .Select(
                    columns => columns
                                    .Select((column, index) =>
                                            new { index, column }
                                        ).ToDictionary(a => a.column, a => a.index)
                       ).FirstOrDefault();

        }

        public string[] GetRuleListData()
        {
            string[] csvData = CSVData.RuleList.ToString().Split(System.Environment.NewLine.ToArray());

            csvData = csvData.Where(csvLine => !string.IsNullOrEmpty(csvLine)).ToArray();

            return csvData;
        }

        public int GetColumnOrdinal(string columnName)
        {
            int ordinal = -1;

            this._columns.TryGetValue(columnName, out ordinal);

            return ordinal;
        }

        public string GetColumnData(int csvLineIndex, string columnName)
        {
            string[] csvData = this._csvData;
            string[] columnValue = csvData[csvLineIndex + 1].Split(',');        //csvLineIndex 0 always contain headers

            return columnValue[this.GetColumnOrdinal(columnName)];
        }

    }

      public class TableDataRetriever
      {
          public IDbSet<RuleList> GetRuleListData()
          {
              TableData data = new TableData();
              IList<RuleList> listData;

              listData = data.GetRuleListData()
                  .Skip(1)    //Skip Header File
                  .Select((line, index) => new
                  RuleList
                  {
                      Id = int.Parse(data.GetColumnData(index, "id")),
                      Group = data.GetColumnData(index, "group_customer"),

                      Receiving_Country_Code = data.GetColumnData(index, "Receiving_Country_Code"),
                      Shipping_Country_Code = data.GetColumnData(index, "Shipping_Country_Code"),

                      Shipping_Company = data.GetColumnData(index, "Shipping_Company"),
                      Receving_Company = data.GetColumnData(index, "Receving_Company"),

                      Shipping_PostalCode = data.GetColumnData(index, "Shipping_PostalCode"),
                      Receiving_PostalCode = data.GetColumnData(index, "Receiving_PostalCode"),
                  }
                  ).ToList();

              Mock<IDbSet<RuleList>> mockDbSet = Mock<IDbSet<RuleList>>
                              .Get<IDbSet<RuleList>>(this.GetMockDBSet<RuleList>(listData));

              mockDbSet.Setup(setup => setup.Find(It.IsAny<object[]>()))
                  .Returns((object[] keys) =>
                      mockDbSet.Object.Where(filter => filter.Id == int.Parse(keys[0].ToString())).FirstOrDefault());

              return mockDbSet.Object;
          }

          public IDbSet<T> GetMockDBSet<T>(IList<T> listData = null, Action<T> otherCallback = null) where T : class
          {
              Mock<IDbSet<T>> mockQuery = new Mock<IDbSet<T>>();
              IList<T> list = listData ?? new List<T>();
              IQueryable<T> listQuery = list.AsQueryable();
              ObservableCollection<T> observables = new ObservableCollection<T>(listQuery);

              mockQuery.Setup(setup => setup.Add(It.IsAny<T>())).Callback
                  ((T input) =>
                  {
                      list.Add(input);
                      observables.Add(input);

                      if (otherCallback != null)
                          otherCallback(input);
                  }
                      );

              mockQuery.Setup(setup => setup.Local).Returns(observables);

              mockQuery.As<IQueryable>().Setup(setup => setup.GetEnumerator()).Returns(listQuery.GetEnumerator());
              mockQuery.As<IQueryable>().Setup(setup => setup.Provider).Returns(listQuery.Provider);
              mockQuery.As<IQueryable>().Setup(setup => setup.ElementType).Returns(listQuery.ElementType);
              mockQuery.As<IQueryable>().Setup(setup => setup.Expression).Returns(listQuery.Expression);

              return mockQuery.Object;
          }

      }
}
