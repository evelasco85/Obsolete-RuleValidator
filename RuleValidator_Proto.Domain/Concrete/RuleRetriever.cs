using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RuleValidator_Proto.Domain.Entities;
using RuleValidator_Proto.Integration.Utility;

namespace RuleValidator_Proto.Domain.Concrete
{
    public class NoRuleFound : RuleList
    {
        //Returns if no rule is found
        //Special case pattern
    }

    public interface IRuleRetriever
    {
        IRuleRetriever SetCriteria<TOut>(Expression<Func<RuleList, TOut>> ruleListField, string fieldValueToCompare);
        RuleList GetRule();
        IQueryable<RuleList> GetRules();
        IRuleRetriever ClearQueryConditions(string uniqueRecordId = "");
        IRuleRetriever SetEntity(ExportedItem exportedItem);
        string EvaluateEUCountryCode(string countryCode);
        string GetCountryToCompare(string group, string countryCode, Expression<Func<RuleList, string>> ruleListField);
    }

    public class RuleRetriever : IRuleRetriever
    {
        IList<RuleList> _localCopyOfRuleList;
        string _uniqueRecordId;
        string _euCountryCodes;
        
        
        IDynamicFilterBuilder<RuleList> _filterBuilder;
        public RuleRetriever(IQueryable<RuleList> rules, string euCountryCodes)
        {
            this._localCopyOfRuleList = rules.Select(x => x).ToList();        //Forcefully create a local copy so that all processing are placed on POCO as opposed with db query
            this._euCountryCodes = euCountryCodes;
        }

        public IRuleRetriever ClearQueryConditions(string uniqueRecordId = "")
        {
            this._filterBuilder = new DynamicFilterBuilder<RuleList>(this._localCopyOfRuleList.AsQueryable());
            this._uniqueRecordId = uniqueRecordId;

            return this;
        }

        public string EvaluateEUCountryCode(string countryCode)
        {
            IList<string> euCountryCodes = new List<string>(this._euCountryCodes.ToLower().Split(','));

            string evaluatedCountry = countryCode;

            if ((!string.IsNullOrEmpty(countryCode)) && (euCountryCodes.Any(euCountry => euCountry.Trim() == countryCode.Trim().ToLower())))
                evaluatedCountry = "eu";

            return evaluatedCountry;
        }

        public string GetCountryToCompare(string group, string countryCode, Expression<Func<RuleList, string>> ruleListField)
        {
            string eu_Code = "eu";
            string countryCodeToUse = countryCode.ToLower();
            bool isEuropianUnionCountry = this.EvaluateEUCountryCode(countryCodeToUse) == eu_Code;

            if ((string.IsNullOrEmpty(group)) || (string.IsNullOrEmpty(countryCode)))
                return string.Empty;

            if (!isEuropianUnionCountry)
                return countryCodeToUse;

            //Construct expression to verify if specific country exists in rule list table
            Expression propertyBody = (ruleListField.Body);
            Expression<Func<RuleList, bool>> specificCountryCondition = Expression
                    .Lambda<Func<RuleList, bool>>(
                        Expression.Equal(
                            Expression.Call(propertyBody, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes)),
                            Expression.Constant(countryCodeToUse, propertyBody.Type)),
                        ruleListField.Parameters
                        );
            /****************************************************************************/

            IEnumerable<RuleList> rules = this._localCopyOfRuleList
                .AsQueryable()
                .Where(filter => filter.Group == group)
                .Where(specificCountryCondition);

            if (rules.Any())     //Use specific country(if exists) for comparison
                return countryCodeToUse;
            else
                return eu_Code;     //No specific country found from the rule list, hence, use 'EU' country instead

        }

        public IRuleRetriever SetEntity(ExportedItem exportedItem)
        {
            string group = exportedItem.File.group_customer;
            string receivingCountry = GetCountryToCompare(group, exportedItem.Receiving_Country_Code, rule => rule.Receiving_Country_Code);
            string shippingCountry = GetCountryToCompare(group, exportedItem.Shipping_Country_Code, rule => rule.Shipping_Country_Code);
            string recordId = exportedItem.Id.ToString();

            this.ClearQueryConditions(recordId)
                .SetCriteria((field) => field.Group, group)
                .SetCriteria((field) => field.Receiving_Country_Code, receivingCountry)
                .SetCriteria((field) => field.Shipping_Country_Code, shippingCountry)
                .SetCriteria((field) => field.Shipping_Company, exportedItem.Shipping_Company_Name)
                .SetCriteria((field) => field.Receving_Company, exportedItem.Receiving_Company_Name)
                .SetCriteria((field) => field.Shipping_PostalCode, exportedItem.Shipping_Postal_Code)
                .SetCriteria((field) => field.Receiving_PostalCode, exportedItem.Receiving_Postal_Code)
                ;
                

            return this;
        }

        public IRuleRetriever SetCriteria<TOut>(Expression<Func<RuleList, TOut>> ruleListField, string fieldValueToCompare)
        {
            string propertyNameToCompare = ((MemberExpression)ruleListField.Body).Member.Name;
            IList<Expression<Func<RuleList, bool>>> ruleEvaluators = new List<Expression<Func<RuleList, bool>>>();

            IRuleExpressionValidationTemplate validationTemplate = new RuleExpressionValidationTemplate(propertyNameToCompare, fieldValueToCompare) { DebugId = this._uniqueRecordId };
            Expression<Func<RuleList, bool>> ruleEvaluator = validationTemplate.GetConstructedRegExValidationWhereQuery(this._filterBuilder.QueryableType);
            this._filterBuilder = this._filterBuilder.AddCustomizedFilterToCurrentQuery(ruleEvaluator.Body);

            return this;
        }

        public IQueryable<RuleList> GetRules()
        {
            IQueryable<RuleList> resultQuery = this._filterBuilder
                .GetQuery()
                .OrderBy(sort => sort.Number);

            return resultQuery;
        }

        public RuleList GetRule()
        {
            IQueryable<RuleList> resultQuery = this.GetRules();
            RuleList foundRule = new NoRuleFound();

            if ((resultQuery != null) && (resultQuery.Any()))
                foundRule = resultQuery.First();

            return foundRule;
        }
    }
}
