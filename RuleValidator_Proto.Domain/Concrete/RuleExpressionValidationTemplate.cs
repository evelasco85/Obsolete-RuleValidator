using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RuleValidator_Proto.Integration.Utility;
using RuleValidator_Proto.Domain.Entities;

namespace RuleValidator_Proto.Domain.Concrete
{
    public interface IRuleExpressionValidationTemplate
    {
        Expression<Func<RuleList, bool>> GetConstructedRegExValidationWhereQuery(ParameterExpression whereParameter);
        string DebugId { get; set; }
    }

    /// <summary>
    /// Creates an validation object per entity record
    /// </summary>
    public class RuleExpressionValidationTemplate : IRuleExpressionValidationTemplate
    {
        public string DebugId
        {
            get { return this._uniqueDebugId; }
            set { this._uniqueDebugId = value; }
        }

        string _propertyNameToCompare;
        string _fieldValueToCompare;
        string _uniqueDebugId;

        /// <summary>
        /// Constructor for rule validation using Regular Expression
        /// </summary>
        /// <param name="propertyNameToCompare">Field name of the entity where dynamic condition is applied</param>
        /// <param name="fieldValueToCompare">Value to compare against dynamic regular expression</param>
        public RuleExpressionValidationTemplate(string propertyNameToCompare, string fieldValueToCompare)
        {
            this._propertyNameToCompare = propertyNameToCompare;
            this._fieldValueToCompare = fieldValueToCompare;
        }

        /// <summary>
        /// Returns newly created validation expression for where query
        /// </summary>
        /// <param name="whereParameter">Parameter associated to where query</param>
        /// <returns>Returns newly created validation</returns>
        public Expression<Func<RuleList, bool>> GetConstructedRegExValidationWhereQuery(ParameterExpression whereParameter)
        {
            string valueToCompare = this._fieldValueToCompare;
            string propertyName = this._propertyNameToCompare;

            Func<RuleList, bool> funcValidateRegEx = 
                            rule =>
                            {
                                string input = valueToCompare;
                                string dbWildcardString = "*";
                                string regexWildcardString = ".*";
                                bool isMatched = false;

                                string currentMemberValue = ExpressionHelper.GetInstance().GetMemberExpressionValue<RuleList, string>(rule, propertyName);

                                if (string.IsNullOrEmpty(currentMemberValue))
                                    return isMatched;

                                //case-insensitive comparison
                                currentMemberValue = currentMemberValue.ToLower().Trim();
                                input = (!string.IsNullOrEmpty(input)) ? input.ToLower() : string.Empty;
                                /////////////////////////////

                                string pattern = currentMemberValue.Replace(dbWildcardString, regexWildcardString);

                                if (!string.IsNullOrEmpty(currentMemberValue))
                                {
                                    isMatched = Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
                                }

#if false       //Enable only if intension is for debugging
      LogRuleResult(propertyName, currentMemberValue, pattern, isMatched, input, rule);                          
#endif
                                return isMatched;
                            };



            /*Replace parameter with the one currently use*/
            ParameterExpression newParam = whereParameter;
            Expression newExprBody = Expression.Invoke(
                (Expression<Func<RuleList, bool>>)(rule => funcValidateRegEx(rule)),
                newParam);
            Expression<Func<RuleList, bool>> lambdaExpr = Expression.Lambda<Func<RuleList, bool>>(newExprBody, newParam);
            /**********************************************/

            return lambdaExpr;
        }

        public void LogRuleResult(string propertyName, string currentMemberValue, string pattern, bool isMatched, string input, RuleList rule)
        {
            string debugOutput = string.Empty;
            //if(isMatched)
            debugOutput = string.Format(
            "[ID: {5}]\t" +
            "[RuleID: {6}]\t" +
            "[Matched: {4}]\t" +
            "[RuleField: {0}]\t" +
            "[RuleValue: {1}]\t" +
            "[RulePattern: {2}]\t" +
            "[InputValue: {3}]\t",
            propertyName,
            currentMemberValue,
            pattern,
            input,
            (isMatched) ? "Y" : "N",
            this._uniqueDebugId,
            rule.Id);

            Debug.WriteLine(debugOutput);
        }
    }
}
