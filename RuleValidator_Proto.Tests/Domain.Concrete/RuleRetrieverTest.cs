using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using RuleValidator_Proto.Domain.Concrete;
using RuleValidator_Proto.Domain.Entities;

namespace RuleValidator_Proto.Tests.Domain.Concrete
{
    [TestClass]
    public class RuleRetrieverTest
    {
        [TestMethod]
        public void TestRetrieveMatchedRule_FailedResult()
        {
            TableDataRetriever dataRetriever = new TableDataRetriever();
            string euCountryCodes = "AT, BE, DE, GB, NL, DK, FI, IT";
            IRuleRetriever ruleRetriever = new RuleRetriever(dataRetriever.GetRuleListData(), euCountryCodes);

            RuleList actualRule = ruleRetriever
                .ClearQueryConditions()
                .SetCriteria((x) => x.Group, "John Doe Company")
                .GetRule();

            Assert.IsInstanceOfType(actualRule, typeof(NoRuleFound));
        }

        [TestMethod]
        public void TestRetrieveMatchedRule_CompleteQuery()
        {
            TableDataRetriever dataRetriever = new TableDataRetriever();
            string group_customer = "LTSHoldings";
            string receivingCountry = "DK";
            string shippingCountry = "SE";
            string shippingCompany = "Beijer Electronics Products AB";
            string receivingCompany = "KJ-Elektro";
            string shippingPostalCode = "21124";
            string receivingPostalCode = "4800";

            string euCountryCodes = "AT,BE,BG,HR,CY,CZ,DK,EE,FI,FR,DE,GR,HU,IE,IT,LV,LT,LU,MT,NL,PL,PT,RO,SK,SI,ES,SE,UK, GB";
            IRuleRetriever ruleRetriever = new RuleRetriever(dataRetriever.GetRuleListData(), euCountryCodes);
            RuleList actualRuleList = ruleRetriever
                .ClearQueryConditions()
                .SetCriteria((x) => x.Group, group_customer)
                .SetCriteria((x) => x.Receiving_Country_Code, receivingCountry)
                .SetCriteria((x) => x.Shipping_Country_Code, shippingCountry)
                .SetCriteria((x) => x.Shipping_Company, shippingCompany)
                .SetCriteria((x) => x.Receving_Company, receivingCompany)
                .SetCriteria((x) => x.Shipping_PostalCode, shippingPostalCode)
                .SetCriteria((x) => x.Receiving_PostalCode, receivingPostalCode)
                .GetRule();

            Assert.AreEqual(1020, actualRuleList.Id);
        }

        [TestMethod]
        public void TestRetrieveMatchedRule_CompareAgainstSpecificEUCountry()
        {
            TableDataRetriever dataRetriever = new TableDataRetriever();
            string group_customer = "LTSHoldings";
            string shippingCountry = "FI";
            string receivingCountry = "FI";
            string shippingCompany = "Beijer Electronics Products AB";
            string receivingCompany = "KJ-Elektro";
            string shippingPostalCode = "21124";
            string receivingPostalCode = "4800";

            string euCountryCodes = "AT,BE,BG,HR,CY,CZ,DK,EE,FI,FR,DE,GR,HU,IE,IT,LV,LT,LU,MT,NL,PL,PT,RO,SK,SI,ES,SE,UK, GB";
            IRuleRetriever ruleRetriever = new RuleRetriever(dataRetriever.GetRuleListData(), euCountryCodes);
            RuleList actualRuleList = ruleRetriever
                .ClearQueryConditions()
                .SetCriteria((x) => x.Group, group_customer)
                .SetCriteria((x) => x.Receiving_Country_Code, ruleRetriever.GetCountryToCompare(group_customer, receivingCountry, rule => rule.Receiving_Country_Code))
                .SetCriteria((x) => x.Shipping_Country_Code, ruleRetriever.GetCountryToCompare(group_customer, shippingCountry, rule => rule.Shipping_Country_Code))
                .SetCriteria((x) => x.Shipping_Company, shippingCompany)
                .SetCriteria((x) => x.Receving_Company, receivingCompany)
                .SetCriteria((x) => x.Shipping_PostalCode, shippingPostalCode)
                .SetCriteria((x) => x.Receiving_PostalCode, receivingPostalCode)
                .GetRule();

            Assert.AreEqual(1022, actualRuleList.Id);
            Assert.AreEqual("FI", actualRuleList.Shipping_Country_Code);        //Should match with 'FI' country instead of 'EU' due to specific definition of rule for specific country
        }

        [TestMethod]
        public void TestRetrieveMatchedRule_CompareAgainstNonSpecific_EUCountry()
        {
            TableDataRetriever dataRetriever = new TableDataRetriever();
            string group_customer = "LTSHoldings";
            string shippingCountry = "DE";
            string receivingCountry = "SE";
            string shippingCompany = "LTS Holdings";
            string receivingCompany = "LTS";
            string shippingPostalCode = "4800";
            string receivingPostalCode = "21124";

            string euCountryCodes = "AT,BE,BG,HR,CY,CZ,DK,EE,FI,FR,DE,GR,HU,IE,IT,LV,LT,LU,MT,NL,PL,PT,RO,SK,SI,ES,SE,UK, GB";
            IRuleRetriever ruleRetriever = new RuleRetriever(dataRetriever.GetRuleListData(), euCountryCodes);
            RuleList actualRuleList = ruleRetriever
                .ClearQueryConditions()
                .SetCriteria((x) => x.Group, group_customer)
                .SetCriteria((x) => x.Receiving_Country_Code, ruleRetriever.GetCountryToCompare(group_customer, receivingCountry, rule => rule.Receiving_Country_Code))
                .SetCriteria((x) => x.Shipping_Country_Code, ruleRetriever.GetCountryToCompare(group_customer, shippingCountry, rule => rule.Shipping_Country_Code))
                .SetCriteria((x) => x.Shipping_Company, shippingCompany)
                .SetCriteria((x) => x.Receving_Company, receivingCompany)
                .SetCriteria((x) => x.Shipping_PostalCode, shippingPostalCode)
                .SetCriteria((x) => x.Receiving_PostalCode, receivingPostalCode)
                .GetRule();

            Assert.AreEqual(5, actualRuleList.Id);
            Assert.AreEqual("EU", actualRuleList.Shipping_Country_Code);        //Should match with 'EU' country since no rule definition for specific country
        }
    }
}
