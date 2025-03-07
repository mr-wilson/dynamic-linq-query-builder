﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Castle.DynamicLinqQueryBuilder.Tests.Helpers;
using NUnit.Framework;

namespace Castle.DynamicLinqQueryBuilder.Tests.Rules
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class QueryBuilderFilterRuleTests
    {
        IQueryable<Tests.ExpressionTreeBuilderTestClass> StartingQuery;
        IQueryable<Tests.ExpressionTreeBuilderTestClass> StartingDateQuery;

        [SetUp]
        public void Setup()
        {
            StartingQuery = Tests.GetExpressionTreeData().AsQueryable();
            StartingDateQuery = Tests.GetDateExpressionTreeData().AsQueryable();
        }        

        #region Expression Tree Builder        

        [Test]
        public void DateHandling()
        {
            QueryBuilder.ParseDatesAsUtc = true;
            var contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModified",
                        Id = "LastModified",
                        Input = "NA",
                        Operator = "equal",
                        Type = "date",
                        Value = new[] { "2/23/2016" }
                    }
                }
            };
            var queryable = StartingDateQuery.BuildQuery<Tests.ExpressionTreeBuilderTestClass>(contentIdFilter);
            var contentIdFilteredList = queryable.ToList();
            Assert.IsTrue(contentIdFilteredList != null);
            Assert.IsTrue(contentIdFilteredList.Count == 1);

            contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModified",
                        Id = "LastModified",
                        Input = "NA",
                        Operator = "equal",
                        Type = "date",
                        Value = new[] { "" }
                    }
                }
            };
            ExceptionAssert.Throws<Exception>(() =>
            {
                var contentIdFilteredListNull1 = StartingQuery.BuildQuery(contentIdFilter).ToList();
            });


            contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "DateList",
                        Id = "DateList",
                        Input = "NA",
                        Operator = "in",
                        Type = "date",
                        Value = new[] { "2/23/2016" }
                    }
                }
            };
            queryable = StartingDateQuery.BuildQuery(contentIdFilter);
            var contentIdFilteredList2 = queryable.ToList();
            Assert.IsTrue(contentIdFilteredList2 != null);
            Assert.IsTrue(contentIdFilteredList2.Count == 1);

            contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "DateList",
                        Id = "DateList",
                        Input = "NA",
                        Operator = "between",
                        Type = "date",
                        Value = new[] { "" }
                    }
                }
            };
            ExceptionAssert.Throws<Exception>(() =>
            {
                var contentIdFilteredListNull2 = StartingDateQuery.BuildQuery(contentIdFilter).ToList();

            });

            QueryBuilder.ParseDatesAsUtc = false;
        }

        [Test]
        public void InClause()
        {
            //expect two entries to match for an integer comparison
            var contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "in",
                        Type = "integer",
                        Value = new[] { "1", "2" }
                    }
                }
            };
            var contentIdFilteredList = StartingQuery.BuildQuery(contentIdFilter).ToList();
            Assert.IsTrue(contentIdFilteredList != null);
            Assert.IsTrue(contentIdFilteredList.Count == 3);
            Assert.IsTrue(contentIdFilteredList.All(p => (new List<int>() { 1, 2 }).Contains(p.ContentTypeId)));

            //expect failure when non-numeric value is encountered in integer comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                contentIdFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(contentIdFilter).ToList();

            });

            //single value test
            contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "in",
                        Type = "integer",
                        Value = new[] { "1" }
                    }
                }
            };
            contentIdFilteredList = StartingQuery.BuildQuery(contentIdFilter).ToList();
            Assert.IsTrue(contentIdFilteredList != null);
            Assert.IsTrue(contentIdFilteredList.Count == 2);
            Assert.IsTrue(contentIdFilteredList.All(p => (new List<int>() { 1 }).Contains(p.ContentTypeId)));

            //expect two entries to match for an integer comparison
            var nullableContentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeId",
                        Id = "NullableContentTypeId",
                        Input = "NA",
                        Operator = "in",
                        Type = "integer",
                        Value = new[] { "1", "2" }
                    }
                }
            };
            var nullableContentIdFilteredList = StartingQuery.BuildQuery(nullableContentIdFilter).ToList();
            Assert.IsTrue(nullableContentIdFilteredList != null);
            Assert.IsTrue(nullableContentIdFilteredList.Count == 2);
            Assert.IsTrue(
                nullableContentIdFilteredList.All(p => (new List<int>() { 1, 2 }).Contains(p.NullableContentTypeId.Value)));

            //expect 3 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "in",
                        Type = "string",
                        Value = new[] { "there is something interesting about this text", "there is something interesting about this text2" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 3);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter.ToLower())
                    .All(p => p == "there is something interesting about this text"));

            //expect 3 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilterCaps = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "in",
                        Type = "string",
                        Value = new[] { "THERE is something interesting about this text", "there is something interesting about this text2" }
                    }
                }
            };
            var longerTextToFilterListCaps = StartingQuery.BuildQuery(longerTextToFilterFilterCaps).ToList();
            Assert.IsTrue(longerTextToFilterListCaps != null);
            Assert.IsTrue(longerTextToFilterListCaps.Count == 3);
            Assert.IsTrue(
                longerTextToFilterListCaps.Select(p => p.LongerTextToFilter.ToLower())
                    .All(p => p == "there is something interesting about this text"));


            //expect 4 entries to match for a Date comparison
            var lastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModified",
                        Id = "LastModified",
                        Input = "NA",
                        Operator = "in",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.ToString(), DateTime.UtcNow.Date.ToString() }
                    }
                }
            };
            var lastModifiedFilterList = StartingQuery.BuildQuery(lastModifiedFilter).ToList();
            Assert.IsTrue(lastModifiedFilterList != null);
            Assert.IsTrue(lastModifiedFilterList.Count == 4);
            Assert.IsTrue(
                lastModifiedFilterList.Select(p => p.LastModified)
                    .All(p => p == DateTime.UtcNow.Date));

            //expect failure when an invalid date is encountered in date comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                lastModifiedFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(lastModifiedFilter).ToList();

            });

            //expect 3 entries to match for a possibly empty Date comparison
            var nullableLastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModifiedIfPresent",
                        Id = "LastModifiedIfPresent",
                        Input = "NA",
                        Operator = "in",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.ToString(), DateTime.UtcNow.Date.AddDays(1).ToString() }
                    }
                }
            };
            var nullableLastModifiedFilterList = StartingQuery.BuildQuery(nullableLastModifiedFilter).ToList();
            Assert.IsTrue(nullableLastModifiedFilterList != null);
            Assert.IsTrue(nullableLastModifiedFilterList.Count == 3);
            Assert.IsTrue(
                nullableLastModifiedFilterList.Select(p => p.LastModifiedIfPresent)
                    .All(p => p == DateTime.UtcNow.Date));


            //expect 2 entries to match for a double field
            var statValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StatValue",
                        Id = "StatValue",
                        Input = "NA",
                        Operator = "in",
                        Type = "double",
                        Value = new[] { "1.11", "1.12" }
                    }
                }
            };
            var statValueFilterList = StartingQuery.BuildQuery(statValueFilter).ToList();
            Assert.IsTrue(statValueFilterList != null);
            Assert.IsTrue(statValueFilterList.Count == 3);
            Assert.IsTrue(
                statValueFilterList.Select(p => p.StatValue)
                    .All(p => (new List<double>() { 1.11, 1.12 }).Contains(p)));

            //expect failure when an invalid double is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                statValueFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(statValueFilter).ToList();

            });

            //expect 2 entries to match for a nullable double field
            var nullableStatValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "PossiblyEmptyStatValue",
                        Id = "PossiblyEmptyStatValue",
                        Input = "NA",
                        Operator = "in",
                        Type = "double",
                        Value = new[] {"1.112", "1.113" }
                    }
                }
            };
            var nullableStatFilterList = StartingQuery.BuildQuery(nullableStatValueFilter).ToList();
            Assert.IsTrue(nullableStatFilterList != null);
            Assert.IsTrue(nullableStatFilterList.Count == 2);
            Assert.IsTrue(
                nullableStatFilterList.Select(p => p.PossiblyEmptyStatValue)
                    .All(p => p == 1.112));

            //expect 2 entries to match for a List<DateTime> field
            var dateListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "DateList",
                        Id = "DateList",
                        Input = "NA",
                        Operator = "in",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(-2).ToString() }
                    }
                }
            };
            var dateListFilterList = StartingQuery.ToList().BuildQuery(dateListFilter).ToList();
            Assert.IsTrue(dateListFilterList != null);
            Assert.IsTrue(dateListFilterList.Count == 3);
            Assert.IsTrue(dateListFilterList.All(p => p.DateList.Contains(DateTime.UtcNow.Date.AddDays(-2))));

            //expect failure when an invalid date is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                dateListFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(dateListFilter).ToList();

            });

            //expect 2 entries to match for a List<string> field
            var strListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StrList",
                        Id = "StrList",
                        Input = "NA",
                        Operator = "in",
                        Type = "string",
                        Value = new[] { "Str2" }
                    }
                }
            };
            var strListFilterList = StartingQuery.AsEnumerable().BuildQuery(strListFilter).ToList();
            Assert.IsTrue(strListFilterList != null);
            Assert.IsTrue(strListFilterList.Count == 3);
            Assert.IsTrue(strListFilterList.All(p => p.StrList.Contains("Str2")));

            //expect 2 entries to match for a List<int> field
            var intListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "IntList",
                        Id = "IntList",
                        Input = "NA",
                        Operator = "in",
                        Type = "integer",
                        Value = new[] {"1", "3"}
                    }
                }
            };
            var intListFilterList = StartingQuery.BuildQuery(intListFilter).ToList();
            Assert.IsTrue(intListFilterList != null);
            Assert.IsTrue(intListFilterList.Count == 3);
            Assert.IsTrue(intListFilterList.All(p => p.IntList.Contains(1) || p.IntList.Contains(3)));

            //expect failure when an invalid double is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                intListFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(intListFilter).ToList();

            });

            //expect 2 entries to match for a nullable nullable int field
            var nullableIntListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "IntNullList",
                        Id = "IntNullList",
                        Input = "NA",
                        Operator = "in",
                        Type = "integer",
                        Value = new[] { "5" }
                    }
                }
            };
            var nullableIntListList = StartingQuery.BuildQuery(nullableIntListFilter).ToList();
            Assert.IsTrue(nullableIntListList != null);
            Assert.IsTrue(nullableIntListList.Count == 3);
            Assert.IsTrue(nullableIntListList.All(p => p.IntNullList.Contains(5)));

            var multipleWithBlankRule = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StrList",
                        Id = "StrList",
                        Input = "NA",
                        Operator = "in",
                        Type = "string",
                        Value = new[] {"", "Str2" }
                    }
                }
            };
            var multipleWithBlankList = StartingQuery.BuildQuery(multipleWithBlankRule).ToList();
            Assert.IsTrue(multipleWithBlankList != null);
            Assert.IsTrue(multipleWithBlankList.Count == 4);
            Assert.IsTrue(multipleWithBlankList.All(p => p.StrList.Contains("") || p.StrList.Contains("Str2")));

            var firstGuid = StartingQuery.First().ContentTypeGuid.ToString();
            //expect one entry to match for a Guid Comparison
            var contentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeGuid",
                        Id = "ContentTypeGuid",
                        Input = "NA",
                        Operator = "in",
                        Type = "string",
                        Value = new[] { firstGuid, firstGuid }
                    }
                }
            };
            var contentGuidFilteredList = StartingQuery.BuildQuery(contentGuidFilter).ToList();
            Assert.IsTrue(contentGuidFilteredList != null);
            Assert.IsTrue(contentGuidFilteredList.Count == 1);

            firstGuid = StartingQuery.First().ContentTypeGuid.ToString();
            //expect one entry to match for a Guid Comparison
            contentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeGuid",
                        Id = "ContentTypeGuid",
                        Input = "NA",
                        Operator = "in",
                        Type = "guid",
                        Value = new[] { firstGuid, firstGuid }
                    }
                }
            };
            contentGuidFilteredList = StartingQuery.BuildQuery(contentGuidFilter).ToList();
            Assert.IsTrue(contentGuidFilteredList != null);
            Assert.IsTrue(contentGuidFilteredList.Count == 1);

            //expect one entry to match for a Guid Comparison against a null nullable Id
            var nullableContentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeGuid",
                        Id = "NullableContentTypeGuid",
                        Input = "NA",
                        Operator = "in",
                        Type = "string",
                        Value = new[] { firstGuid.ToLower() }
                    }
                }
            };
            var nullableContentGuidFilteredList = StartingQuery.BuildQuery(nullableContentGuidFilter).ToList();
            Assert.IsTrue(nullableContentGuidFilteredList != null);
            Assert.IsTrue(nullableContentGuidFilteredList.Count == 1);

            //expect one entry to match for a Guid Comparison against a null nullable Id
            nullableContentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeGuid",
                        Id = "NullableContentTypeGuid",
                        Input = "NA",
                        Operator = "in",
                        Type = "guid",
                        Value = new[] { firstGuid.ToUpper() }
                    }
                }
            };
            nullableContentGuidFilteredList = StartingQuery.BuildQuery(nullableContentGuidFilter).ToList();
            Assert.IsTrue(nullableContentGuidFilteredList != null);
            Assert.IsTrue(nullableContentGuidFilteredList.Count == 1);
        }

        [Test]
        public void NotInClause()
        {
            //expect two entries to match for an integer comparison
            var contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "not_in",
                        Type = "integer",
                        Value = new[] { "1", "2" }
                    }
                }
            };
            var contentIdFilteredList = StartingQuery.BuildQuery(contentIdFilter).ToList();
            Assert.IsTrue(contentIdFilteredList != null);
            Assert.IsTrue(contentIdFilteredList.Count == 1);
            Assert.IsTrue(contentIdFilteredList.All(p => (new List<int>() { 3 }).Contains(p.ContentTypeId)));

            //expect failure when non-numeric value is encountered in integer comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                contentIdFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(contentIdFilter).ToList();

            });

            //expect two entries to match for an integer comparison
            var nullableContentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeId",
                        Id = "NullableContentTypeId",
                        Input = "NA",
                        Operator = "not_in",
                        Type = "integer",
                        Value = new[] { "1", "2" }
                    }
                }
            };
            var nullableContentIdFilteredList =
                StartingQuery.BuildQuery(nullableContentIdFilter).ToList();
            Assert.IsTrue(nullableContentIdFilteredList != null);
            Assert.IsTrue(nullableContentIdFilteredList.Count == 2);
            Assert.IsTrue(
                nullableContentIdFilteredList.All(p => !(new List<int>() { 1, 2 }).Contains(p.NullableContentTypeId.GetValueOrDefault())));




            //expect 3 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "not_in",
                        Type = "string",
                        Value = new[] { "there is something interesting about this text" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 1);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter)
                    .All(p => p == null || p != "there is something interesting about this text"));


            //expect 4 entries to match for a Date comparison
            var lastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModified",
                        Id = "LastModified",
                        Input = "NA",
                        Operator = "not_in",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.ToString(), DateTime.UtcNow.Date.ToString() }
                    }
                }
            };
            var lastModifiedFilterList = StartingQuery.BuildQuery(lastModifiedFilter).ToList();
            Assert.IsTrue(lastModifiedFilterList != null);
            Assert.IsTrue(lastModifiedFilterList.Count == 0);
            Assert.IsTrue(
                lastModifiedFilterList.Select(p => p.LastModified)
                    .All(p => p == DateTime.UtcNow.Date));

            //expect failure when an invalid date is encountered in date comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                lastModifiedFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(lastModifiedFilter).ToList();

            });

            //expect 3 entries to match for a possibly empty Date comparison
            var nullableLastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModifiedIfPresent",
                        Id = "LastModifiedIfPresent",
                        Input = "NA",
                        Operator = "not_in",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.ToString(), DateTime.UtcNow.Date.AddDays(1).ToString() }
                    }
                }
            };
            var nullableLastModifiedFilterList = StartingQuery.BuildQuery(nullableLastModifiedFilter).ToList();
            Assert.IsTrue(nullableLastModifiedFilterList != null);
            Assert.IsTrue(nullableLastModifiedFilterList.Count == 1);
            Assert.IsTrue(
                nullableLastModifiedFilterList.Select(p => p.LastModifiedIfPresent)
                    .All(p => p != DateTime.UtcNow.Date));


            //expect 2 entries to match for a double field
            var statValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StatValue",
                        Id = "StatValue",
                        Input = "NA",
                        Operator = "not_in",
                        Type = "double",
                        Value = new[] { "1.11", "1.12" }
                    }
                }
            };
            var statValueFilterList = StartingQuery.BuildQuery(statValueFilter).ToList();
            Assert.IsTrue(statValueFilterList != null);
            Assert.IsTrue(statValueFilterList.Count == 1);
            Assert.IsTrue(
                statValueFilterList.Select(p => p.StatValue)
                    .All(p => !(new List<double>() { 1.11, 1.12 }).Contains(p)));

            //expect failure when an invalid double is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                statValueFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(statValueFilter).ToList();

            });

            //expect 2 entries to match for a nullable double field
            var nullableStatValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "PossiblyEmptyStatValue",
                        Id = "PossiblyEmptyStatValue",
                        Input = "NA",
                        Operator = "not_in",
                        Type = "double",
                        Value = new[] { "1.112", "1.113" }
                    }
                }
            };
            var nullableStatFilterList = StartingQuery.BuildQuery(nullableStatValueFilter).ToList();
            Assert.IsTrue(nullableStatFilterList != null);
            Assert.IsTrue(nullableStatFilterList.Count == 2);
            Assert.IsTrue(
                nullableStatFilterList.Select(p => p.PossiblyEmptyStatValue)
                    .All(p => p != 1.112));

            //expect 2 entries to match for a List<DateTime> field
            var dateListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "DateList",
                        Id = "DateList",
                        Input = "NA",
                        Operator = "not_in",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(-2).ToString() }
                    }
                }
            };
            var dateListFilterList = StartingQuery.BuildQuery(dateListFilter).ToList();
            Assert.IsTrue(dateListFilterList != null);
            Assert.IsTrue(dateListFilterList.Count == 1);
            Assert.IsTrue(dateListFilterList.All(p => !p.DateList.Contains(DateTime.UtcNow.Date.AddDays(-2))));

            //expect failure when an invalid date is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                dateListFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(dateListFilter).ToList();

            });

            //expect 2 entries to match for a List<string> field
            var strListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StrList",
                        Id = "StrList",
                        Input = "NA",
                        Operator = "not_in",
                        Type = "string",
                        Value = new[] { "Str2" }
                    }
                }
            };
            var strListFilterList = StartingQuery.BuildQuery(strListFilter).ToList();
            Assert.IsTrue(strListFilterList != null);
            Assert.IsTrue(strListFilterList.Count == 1);
            Assert.IsTrue(strListFilterList.All(p => !p.StrList.Contains("Str2")));


            //expect 2 entries to match for a List<int> field
            var intListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "IntList",
                        Id = "IntList",
                        Input = "NA",
                        Operator = "not_in",
                        Type = "integer",
                        Value = new[] { "1" ,"3" }
                    }
                }
            };
            var intListFilterList = StartingQuery.BuildQuery(intListFilter).ToList();
            Assert.IsTrue(intListFilterList != null);
            Assert.IsTrue(intListFilterList.Count == 1);
            Assert.IsTrue(intListFilterList.All(p => !p.IntList.Contains(1) && !p.IntList.Contains(3)));

            //expect failure when an invalid double is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                intListFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(intListFilter).ToList();

            });

            //expect 2 entries to match for a nullable nullable int field
            var nullableIntListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "IntNullList",
                        Id = "IntNullList",
                        Input = "NA",
                        Operator = "not_in",
                        Type = "integer",
                        Value = new[] { "5" }
                    }
                }
            };
            var nullableIntListList = StartingQuery.BuildQuery(nullableIntListFilter).ToList();
            Assert.IsTrue(nullableIntListList != null);
            Assert.IsTrue(nullableIntListList.Count == 1);
            Assert.IsTrue(
                nullableIntListList.All(p => !p.IntNullList.Contains(5)));

            //expect 2 entries to match for a nullable double field
            var nullableWrappedStatValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "PossiblyEmptyStatValue",
                        Id = "PossiblyEmptyStatValue",
                        Input = "NA",
                        Operator = "not_in",
                        Type = "double",
                        Value = new[] {"1.112", "1.113" }
                    }
                }
            };
            var nullableWrappedStatFilterList = StartingQuery.BuildQuery(nullableWrappedStatValueFilter).ToList();
            Assert.IsTrue(nullableWrappedStatFilterList != null);
            Assert.IsTrue(nullableWrappedStatFilterList.Count == 2);
            Assert.IsTrue(
                nullableWrappedStatFilterList.Select(p => p.PossiblyEmptyStatValue)
                    .All(p => p != 1.112));
        }

        [Test]
        public void IsNullClause()
        {
            //expect 1 entries to match for a case-insensitive string comparison (nullable type)
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "is_null",
                        Type = "string",
                        Value = new[] { "" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 1);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter)
                    .All(p => p == null));


            //expect 0 entries to match for a non-nullable type
            var contentTypeIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "is_null",
                        Type = "integer",
                        Value = new[] { "" }
                    }
                }
            };
            var contentTypeIdFilterList = StartingQuery.BuildQuery(contentTypeIdFilter).ToList();
            Assert.IsTrue(contentTypeIdFilterList != null);
            Assert.IsTrue(contentTypeIdFilterList.Count == 0);
            Assert.IsTrue(
                contentTypeIdFilterList.Select(p => p.ContentTypeId)
                    .All(p => p == 0));

        }

        [Test]
        public void IsNotNullClause()
        {
            //expect 3 entries to match for a case-insensitive string comparison (nullable type)
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "is_not_null",
                        Type = "string",
                        Value = new[] { "" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 3);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter)
                    .All(p => p != null));


            //expect 0 entries to match for a non-nullable type
            var contentTypeIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "is_not_null",
                        Type = "integer",
                        Value = new[] { "" }
                    }
                }
            };
            var contentTypeIdFilterList = StartingQuery.BuildQuery(contentTypeIdFilter).ToList();
            Assert.IsTrue(contentTypeIdFilterList != null);
            Assert.IsTrue(contentTypeIdFilterList.Count == 4);
            Assert.IsTrue(
                contentTypeIdFilterList.Select(p => p.ContentTypeId)
                    .All(p => p != 0));

        }

        [Test]
        public void IsEmptyClause()
        {
            //expect 3 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "is_empty",
                        Type = "string",
                        Value = new[] { "" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 0);


            //expect 2 entries to match for a List<DateTime> field
            var dateListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "DateList",
                        Id = "DateList",
                        Input = "NA",
                        Operator = "is_empty",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(-2).ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var dateListFilterList = StartingQuery.BuildQuery(dateListFilter).ToList();
            Assert.IsTrue(dateListFilterList != null);
            Assert.IsTrue(dateListFilterList.Count == 0);
            //Assert.IsTrue(dateListFilterList.All(p => !p.DateList.Contains(DateTime.UtcNow.Date.AddDays(-2))));



            //expect 2 entries to match for a List<string> field
            var strListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StrList",
                        Id = "StrList",
                        Input = "NA",
                        Operator = "is_empty",
                        Type = "string",
                        Value = new[] { "Str2" }
                    }
                }
            };
            var strListFilterList = StartingQuery.BuildQuery(strListFilter).ToList();
            Assert.IsTrue(strListFilterList != null);
            Assert.IsTrue(strListFilterList.Count == 0);
            //Assert.IsTrue(strListFilterList.All(p => !p.StrList.Contains("Str2")));


            //expect 2 entries to match for a List<int> field
            var intListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "IntList",
                        Id = "IntList",
                        Input = "NA",
                        Operator = "is_empty",
                        Type = "integer",
                        Value = new[] { "1", "3" }
                    }
                }
            };
            var intListFilterList = StartingQuery.BuildQuery(intListFilter).ToList();
            Assert.IsTrue(intListFilterList != null);
            Assert.IsTrue(intListFilterList.Count == 0);



        }

        [Test]
        public void IsNotEmptyClause()
        {
            //expect 3 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "is_not_empty",
                        Type = "string",
                        Value = new[] { "" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 4);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter)
                    .All(p => p == null || p.Length > 0));


            //expect 2 entries to match for a List<DateTime> field
            var dateListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "DateList",
                        Id = "DateList",
                        Input = "NA",
                        Operator = "is_not_empty",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(-2).ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var dateListFilterList = StartingQuery.BuildQuery(dateListFilter).ToList();
            Assert.IsTrue(dateListFilterList != null);
            Assert.IsTrue(dateListFilterList.Count == 4);
            //Assert.IsTrue(dateListFilterList.All(p => !p.DateList.Contains(DateTime.UtcNow.Date.AddDays(-2))));



            //expect 2 entries to match for a List<string> field
            var strListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StrList",
                        Id = "StrList",
                        Input = "NA",
                        Operator = "is_not_empty",
                        Type = "string",
                        Value = new[] { "Str2" }
                    }
                }
            };
            var strListFilterList = StartingQuery.BuildQuery(strListFilter).ToList();
            Assert.IsTrue(strListFilterList != null);
            Assert.IsTrue(strListFilterList.Count == 4);
            //Assert.IsTrue(strListFilterList.All(p => !p.StrList.Contains("Str2")));


            //expect 2 entries to match for a List<int> field
            var intListFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "IntList",
                        Id = "IntList",
                        Input = "NA",
                        Operator = "is_not_empty",
                        Type = "integer",
                        Value = new[] {"1", "3" }
                    }
                }
            };
            var intListFilterList = StartingQuery.BuildQuery(intListFilter).ToList();
            Assert.IsTrue(intListFilterList != null);
            Assert.IsTrue(intListFilterList.Count == 4);

        }

        [Test]
        public void ContainsClause()
        {
            //expect 3 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "contains",
                        Type = "string",
                        Value = new[] { "something interesting" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 3);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter.ToLower())
                    .All(p => p.Contains("something interesting")));

            //expect at least one entry to match for a Guid Comparison
            var contentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeGuid",
                        Id = "ContentTypeGuid",
                        Input = "NA",
                        Operator = "contains",
                        Type = "string",
                        Value = new[] { StartingQuery.First().ContentTypeGuid.ToString().Substring(0,5) }
                    }
                }
            };
            var contentGuidFilteredList = StartingQuery.BuildQuery(contentGuidFilter).ToList();
            Assert.IsTrue(contentGuidFilteredList != null);
            Assert.IsTrue(contentGuidFilteredList.Count >= 1);

            //expect at least one entry to match for a Guid Comparison
            contentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeGuid",
                        Id = "ContentTypeGuid",
                        Input = "NA",
                        Operator = "contains",
                        Type = "guid",
                        Value = new[] { StartingQuery.First().ContentTypeGuid.ToString().Substring(0,5) }
                    }
                }
            };
            contentGuidFilteredList = StartingQuery.BuildQuery(contentGuidFilter).ToList();
            Assert.IsTrue(contentGuidFilteredList != null);
            Assert.IsTrue(contentGuidFilteredList.Count >= 1);

            //expect one match for a Guid Comparison against a null nullable Id
            var nullableContentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeGuid",
                        Id = "NullableContentTypeGuid",
                        Input = "NA",
                        Operator = "contains",
                        Type = "string",
                        Value = new[] { StartingQuery.First().ContentTypeGuid.ToString().Substring(0, 5) }
                    }
                }
            };
            var nullableContentGuidFilteredList = StartingQuery.BuildQuery(nullableContentGuidFilter).ToList();
            Assert.IsTrue(nullableContentGuidFilteredList != null);
            Assert.IsTrue(nullableContentGuidFilteredList.Count == 1);

            //expect one match for a Guid Comparison against a null nullable Id
            nullableContentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeGuid",
                        Id = "NullableContentTypeGuid",
                        Input = "NA",
                        Operator = "contains",
                        Type = "guid",
                        Value = new[] { StartingQuery.First().ContentTypeGuid.ToString().Substring(0, 5) }
                    }
                }
            };
            nullableContentGuidFilteredList = StartingQuery.BuildQuery(nullableContentGuidFilter).ToList();
            Assert.IsTrue(nullableContentGuidFilteredList != null);
            Assert.IsTrue(nullableContentGuidFilteredList.Count == 1);
        }

        [Test]
        public void NotContainsClause()
        {
            //expect 3 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "not_contains",
                        Type = "string",
                        Value = new[] { "something interesting" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 1);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter)
                    .All(p => p == null));

        }

        [Test]
        public void NotEndsWithClause()
        {
            //expect 3 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "not_ends_with",
                        Type = "string",
                        Value = new[] { "about this text" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 1);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter)
                    .All(p => p == null));

        }

        [Test]
        public void EndsWithClause()
        {
            //expect 3 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "ends_with",
                        Type = "string",
                        Value = new[] { "about this text" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 3);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter.ToLower())
                    .All(p => p.EndsWith("about this text")));

            var fristGuid = StartingQuery.First().ContentTypeGuid.ToString();
            //expect at least one entry to match for a Guid Comparison
            var contentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeGuid",
                        Id = "ContentTypeGuid",
                        Input = "NA",
                        Operator = "ends_with",
                        Type = "string",
                        Value = new[] { fristGuid.Substring(fristGuid.Length - 5) }
                    }
                }
            };
            var contentGuidFilteredList = StartingQuery.BuildQuery(contentGuidFilter).ToList();
            Assert.IsTrue(contentGuidFilteredList != null);
            Assert.IsTrue(contentGuidFilteredList.Count >= 1);

            fristGuid = StartingQuery.First().ContentTypeGuid.ToString();
            //expect at least one entry to match for a Guid Comparison
            contentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeGuid",
                        Id = "ContentTypeGuid",
                        Input = "NA",
                        Operator = "ends_with",
                        Type = "guid",
                        Value = new[] { fristGuid.Substring(fristGuid.Length - 5) }
                    }
                }
            };
            contentGuidFilteredList = StartingQuery.BuildQuery(contentGuidFilter).ToList();
            Assert.IsTrue(contentGuidFilteredList != null);
            Assert.IsTrue(contentGuidFilteredList.Count >= 1);

            //expect one match for a Guid Comparison against a null nullable Id
            var nullableContentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeGuid",
                        Id = "NullableContentTypeGuid",
                        Input = "NA",
                        Operator = "ends_with",
                        Type = "string",
                        Value = new[] { fristGuid.Substring(fristGuid.Length - 5) }
                    }
                }
            };
            var nullableContentGuidFilteredList = StartingQuery.BuildQuery(nullableContentGuidFilter).ToList();
            Assert.IsTrue(nullableContentGuidFilteredList != null);
            Assert.IsTrue(nullableContentGuidFilteredList.Count == 1);

            //expect one match for a Guid Comparison against a null nullable Id
            nullableContentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeGuid",
                        Id = "NullableContentTypeGuid",
                        Input = "NA",
                        Operator = "ends_with",
                        Type = "guid",
                        Value = new[] { fristGuid.Substring(fristGuid.Length - 5) }
                    }
                }
            };
            nullableContentGuidFilteredList = StartingQuery.BuildQuery(nullableContentGuidFilter).ToList();
            Assert.IsTrue(nullableContentGuidFilteredList != null);
            Assert.IsTrue(nullableContentGuidFilteredList.Count == 1);
        }

        [Test]
        public void NotBeginsWithClause()
        {
            //expect 1 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "not_begins_with",
                        Type = "string",
                        Value = new[] { "there is something" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 1);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter)
                    .All(p => p == null));

        }

        [Test]
        public void BeginsWithClause()
        {
            //expect 3 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "begins_with",
                        Type = "string",
                        Value = new[] { "there is something" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 3);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter.ToLower())
                    .All(p => p.StartsWith("there is something")));

            var firstGuid = StartingQuery.First().ContentTypeGuid.ToString();
            //expect at least one entry to match for a Guid Comparison
            var contentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeGuid",
                        Id = "ContentTypeGuid",
                        Input = "NA",
                        Operator = "begins_with",
                        Type = "string",
                        Value = new[] { firstGuid.Substring(0,5) }
                    }
                }
            };
            var contentGuidFilteredList = StartingQuery.BuildQuery(contentGuidFilter).ToList();
            Assert.IsTrue(contentGuidFilteredList != null);
            Assert.IsTrue(contentGuidFilteredList.Count >= 1);

            firstGuid = StartingQuery.First().ContentTypeGuid.ToString();
            //expect at least one entry to match for a Guid Comparison
            contentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeGuid",
                        Id = "ContentTypeGuid",
                        Input = "NA",
                        Operator = "begins_with",
                        Type = "guid",
                        Value = new[] { firstGuid.Substring(0,5) }
                    }
                }
            };
            contentGuidFilteredList = StartingQuery.BuildQuery(contentGuidFilter).ToList();
            Assert.IsTrue(contentGuidFilteredList != null);
            Assert.IsTrue(contentGuidFilteredList.Count >= 1);

            //expect one match for a Guid Comparison against a null nullable Id
            var nullableContentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeGuid",
                        Id = "NullableContentTypeGuid",
                        Input = "NA",
                        Operator = "begins_with",
                        Type = "string",
                        Value = new[] { firstGuid.Substring(0,5) }
                    }
                }
            };
            var nullableContentGuidFilteredList = StartingQuery.BuildQuery(nullableContentGuidFilter).ToList();
            Assert.IsTrue(nullableContentGuidFilteredList != null);
            Assert.IsTrue(nullableContentGuidFilteredList.Count == 1);

            //expect one match for a Guid Comparison against a null nullable Id
            nullableContentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeGuid",
                        Id = "NullableContentTypeGuid",
                        Input = "NA",
                        Operator = "begins_with",
                        Type = "guid",
                        Value = new[] { firstGuid.Substring(0,5) }
                    }
                }
            };
            nullableContentGuidFilteredList = StartingQuery.BuildQuery(nullableContentGuidFilter).ToList();
            Assert.IsTrue(nullableContentGuidFilteredList != null);
            Assert.IsTrue(nullableContentGuidFilteredList.Count == 1);
        }

        [Test]
        public void EqualsClause()
        {

            //expect two entries to match for an integer comparison
            var contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "equal",
                        Type = "integer",
                        Value = new[] { "1" }
                    }
                }
            };
            var contentIdFilteredList = StartingQuery.BuildQuery(contentIdFilter).ToList();
            Assert.IsTrue(contentIdFilteredList != null);
            Assert.IsTrue(contentIdFilteredList.Count == 2);
            Assert.IsTrue(contentIdFilteredList.All(p => p.ContentTypeId == 1));

            //expect failure when non-numeric value is encountered in integer comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                contentIdFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(contentIdFilter).ToList();

            });

            //expect two entries to match for an integer comparison
            var nullableContentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeId",
                        Id = "NullableContentTypeId",
                        Input = "NA",
                        Operator = "equal",
                        Type = "integer",
                        Value = new[] { "1" }
                    }
                }
            };
            var nullableContentIdFilteredList =
                StartingQuery.BuildQuery(nullableContentIdFilter).ToList();
            Assert.IsTrue(nullableContentIdFilteredList != null);
            Assert.IsTrue(nullableContentIdFilteredList.Count == 1);
            Assert.IsTrue(nullableContentIdFilteredList.All(p => p.NullableContentTypeId == 1));

            //expect one entry to match for a Guid Comparison
            var contentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeGuid",
                        Id = "ContentTypeGuid",
                        Input = "NA",
                        Operator = "equal",
                        Type = "string",
                        Value = new[] { StartingQuery.First().ContentTypeGuid.ToString() }
                    }
                }
            };
            var contentGuidFilteredList = StartingQuery.BuildQuery(contentGuidFilter).ToList();
            Assert.IsTrue(contentGuidFilteredList != null);
            Assert.IsTrue(contentGuidFilteredList.Count == 1);

            //expect one entry to match for a Guid Comparison
            contentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeGuid",
                        Id = "ContentTypeGuid",
                        Input = "NA",
                        Operator = "equal",
                        Type = "string",
                        Value = new[] { StartingQuery.First().ContentTypeGuid.ToString() }
                    }
                }
            };
            contentGuidFilteredList = StartingQuery.BuildQuery(contentGuidFilter).ToList();
            Assert.IsTrue(contentGuidFilteredList != null);
            Assert.IsTrue(contentGuidFilteredList.Count == 1);

            //expect one match for a Guid Comparison against a null nullable Id
            var nullableContentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeGuid",
                        Id = "NullableContentTypeGuid",
                        Input = "NA",
                        Operator = "equal",
                        Type = "string",
                        Value = new[] { StartingQuery.First().ContentTypeGuid.ToString() }
                    }
                }
            };
            var nullableContentGuidFilteredList = StartingQuery.BuildQuery(nullableContentGuidFilter).ToList();
            Assert.IsTrue(nullableContentGuidFilteredList != null);
            Assert.IsTrue(nullableContentGuidFilteredList.Count == 1);

            //expect one match for a Guid Comparison against a null nullable Id
            nullableContentGuidFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeGuid",
                        Id = "NullableContentTypeGuid",
                        Input = "NA",
                        Operator = "equal",
                        Type = "string",
                        Value = new[] { StartingQuery.First().ContentTypeGuid.ToString() }
                    }
                }
            };
            nullableContentGuidFilteredList = StartingQuery.BuildQuery(nullableContentGuidFilter).ToList();
            Assert.IsTrue(nullableContentGuidFilteredList != null);
            Assert.IsTrue(nullableContentGuidFilteredList.Count == 1);

            //expect 3 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "equal",
                        Type = "string",
                        Value = new[] { "there is something interesting about this text" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 3);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter.ToLower())
                    .All(p => p == "there is something interesting about this text"));


            //expect 4 entries to match for a Date comparison
            var lastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModified",
                        Id = "LastModified",
                        Input = "NA",
                        Operator = "equal",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var lastModifiedFilterList = StartingQuery.BuildQuery(lastModifiedFilter).ToList();
            Assert.IsTrue(lastModifiedFilterList != null);
            Assert.IsTrue(lastModifiedFilterList.Count == 4);
            Assert.IsTrue(
                lastModifiedFilterList.Select(p => p.LastModified)
                    .All(p => p == DateTime.UtcNow.Date));

            //expect failure when an invalid date is encountered in date comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                lastModifiedFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(lastModifiedFilter).ToList();

            });

            //expect 3 entries to match for a possibly empty Date comparison
            var nullableLastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModifiedIfPresent",
                        Id = "LastModifiedIfPresent",
                        Input = "NA",
                        Operator = "equal",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var nullableLastModifiedFilterList = StartingQuery.BuildQuery(nullableLastModifiedFilter).ToList();
            Assert.IsTrue(nullableLastModifiedFilterList != null);
            Assert.IsTrue(nullableLastModifiedFilterList.Count == 3);
            Assert.IsTrue(
                nullableLastModifiedFilterList.Select(p => p.LastModifiedIfPresent)
                    .All(p => p == DateTime.UtcNow.Date));







            //expect 3 entries to match for a boolean field
            var isSelectedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "IsSelected",
                        Id = "IsSelected",
                        Input = "NA",
                        Operator = "equal",
                        Type = "boolean",
                        Value = new[] { "true" }
                    }
                }
            };
            var isSelectedFilterList = StartingQuery.BuildQuery(isSelectedFilter).ToList();
            Assert.IsTrue(isSelectedFilterList != null);
            Assert.IsTrue(isSelectedFilterList.Count == 3);
            Assert.IsTrue(
                isSelectedFilterList.Select(p => p.IsSelected)
                    .All(p => p == true));

            //expect failure when an invalid bool is encountered in bool comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                isSelectedFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(isSelectedFilter).ToList();

            });

            //expect 2 entries to match for a nullable boolean field
            var nullableIsSelectedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "IsPossiblyNotSetBool",
                        Id = "IsPossiblyNotSetBool",
                        Input = "NA",
                        Operator = "equal",
                        Type = "boolean",
                        Value = new[] { "true" }
                    }
                }
            };
            var nullableIsSelectedFilterList = StartingQuery.BuildQuery(nullableIsSelectedFilter).ToList();
            Assert.IsTrue(nullableIsSelectedFilterList != null);
            Assert.IsTrue(nullableIsSelectedFilterList.Count == 2);
            Assert.IsTrue(
                nullableIsSelectedFilterList.Select(p => p.IsPossiblyNotSetBool)
                    .All(p => p == true));


            //expect 2 entries to match for a double field
            var statValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StatValue",
                        Id = "StatValue",
                        Input = "NA",
                        Operator = "equal",
                        Type = "double",
                        Value = new[] { "1.11" }
                    }
                }
            };
            var statValueFilterList = StartingQuery.BuildQuery(statValueFilter).ToList();
            Assert.IsTrue(statValueFilterList != null);
            Assert.IsTrue(statValueFilterList.Count == 2);
            Assert.IsTrue(
                statValueFilterList.Select(p => p.StatValue)
                    .All(p => p == 1.11));

            //expect failure when an invalid double is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                statValueFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(statValueFilter).ToList();

            });

            //expect 2 entries to match for a nullable boolean field
            var nullableStatValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "PossiblyEmptyStatValue",
                        Id = "PossiblyEmptyStatValue",
                        Input = "NA",
                        Operator = "equal",
                        Type = "double",
                        Value = new[] { "1.112" }
                    }
                }
            };
            var nullableStatFilterList = StartingQuery.BuildQuery(nullableStatValueFilter).ToList();
            Assert.IsTrue(nullableStatFilterList != null);
            Assert.IsTrue(nullableStatFilterList.Count == 2);
            Assert.IsTrue(
                nullableStatFilterList.Select(p => p.PossiblyEmptyStatValue)
                    .All(p => p == 1.112));



        }

        [Test]
        public void NotEqualsClause()
        {
            //expect two entries to match for an integer comparison
            var contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "not_equal",
                        Type = "integer",
                        Value = new[] { "1" }
                    }
                }
            };
            var contentIdFilteredList = StartingQuery.BuildQuery(contentIdFilter).ToList();
            Assert.IsTrue(contentIdFilteredList != null);
            Assert.IsTrue(contentIdFilteredList.Count == 2);
            Assert.IsTrue(contentIdFilteredList.All(p => p.ContentTypeId != 1));

            //expect failure when non-numeric value is encountered in integer comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                contentIdFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(contentIdFilter).ToList();

            });

            //expect 3 entries to match for an integer comparison
            var nullableContentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeId",
                        Id = "NullableContentTypeId",
                        Input = "NA",
                        Operator = "not_equal",
                        Type = "integer",
                        Value = new[] { "1" }
                    }
                }
            };
            var nullableContentIdFilteredList =
                StartingQuery.BuildQuery(nullableContentIdFilter).ToList();
            Assert.IsTrue(nullableContentIdFilteredList != null);
            Assert.IsTrue(nullableContentIdFilteredList.Count == 3);
            Assert.IsTrue(nullableContentIdFilteredList.All(p => p.NullableContentTypeId != 1));

            //expect 1 entries to match for a case-insensitive string comparison
            var longerTextToFilterFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LongerTextToFilter",
                        Id = "LongerTextToFilter",
                        Input = "NA",
                        Operator = "not_equal",
                        Type = "string",
                        Value = new[] { "there is something interesting about this text" }
                    }
                }
            };
            var longerTextToFilterList = StartingQuery.BuildQuery(longerTextToFilterFilter).ToList();
            Assert.IsTrue(longerTextToFilterList != null);
            Assert.IsTrue(longerTextToFilterList.Count == 1);
            Assert.IsTrue(
                longerTextToFilterList.Select(p => p.LongerTextToFilter)
                    .All(p => (p == null) || (p.ToLower() != "there is something interesting about this text")));


            //expect 0 entries to match for a Date comparison
            var lastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModified",
                        Id = "LastModified",
                        Input = "NA",
                        Operator = "not_equal",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var lastModifiedFilterList = StartingQuery.BuildQuery(lastModifiedFilter).ToList();
            Assert.IsTrue(lastModifiedFilterList != null);
            Assert.IsTrue(lastModifiedFilterList.Count == 0);
            Assert.IsTrue(
                lastModifiedFilterList.Select(p => p.LastModified)
                    .All(p => p == DateTime.UtcNow.Date));

            //expect failure when an invalid date is encountered in date comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                lastModifiedFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(lastModifiedFilter).ToList();

            });

            //expect 1 entries to match for a possibly empty Date comparison
            var nullableLastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModifiedIfPresent",
                        Id = "LastModifiedIfPresent",
                        Input = "NA",
                        Operator = "not_equal",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var nullableLastModifiedFilterList = StartingQuery.BuildQuery(nullableLastModifiedFilter).ToList();
            Assert.IsTrue(nullableLastModifiedFilterList != null);
            Assert.IsTrue(nullableLastModifiedFilterList.Count == 1);
            Assert.IsTrue(
                nullableLastModifiedFilterList.Select(p => p.LastModifiedIfPresent)
                    .All(p => p != DateTime.UtcNow.Date));


            //expect 1 entries to match for a boolean field
            var isSelectedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "IsSelected",
                        Id = "IsSelected",
                        Input = "NA",
                        Operator = "not_equal",
                        Type = "boolean",
                        Value = new[] { "true" }
                    }
                }
            };
            var isSelectedFilterList = StartingQuery.BuildQuery(isSelectedFilter).ToList();
            Assert.IsTrue(isSelectedFilterList != null);
            Assert.IsTrue(isSelectedFilterList.Count == 1);
            Assert.IsTrue(
                isSelectedFilterList.Select(p => p.IsSelected)
                    .All(p => p != true));

            //expect failure when an invalid bool is encountered in bool comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                isSelectedFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(isSelectedFilter).ToList();

            });

            //expect 2 entries to match for a nullable boolean field
            var nullableIsSelectedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "IsPossiblyNotSetBool",
                        Id = "IsPossiblyNotSetBool",
                        Input = "NA",
                        Operator = "not_equal",
                        Type = "boolean",
                        Value = new[] { "true" }
                    }
                }
            };
            var nullableIsSelectedFilterList = StartingQuery.BuildQuery(nullableIsSelectedFilter).ToList();
            Assert.IsTrue(nullableIsSelectedFilterList != null);
            Assert.IsTrue(nullableIsSelectedFilterList.Count == 2);
            Assert.IsTrue(
                nullableIsSelectedFilterList.Select(p => p.IsPossiblyNotSetBool)
                    .All(p => p != true));


            //expect 2 entries to match for a double field
            var statValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StatValue",
                        Id = "StatValue",
                        Input = "NA",
                        Operator = "not_equal",
                        Type = "double",
                        Value = new[] { "1.11" }
                    }
                }
            };
            var statValueFilterList = StartingQuery.BuildQuery(statValueFilter).ToList();
            Assert.IsTrue(statValueFilterList != null);
            Assert.IsTrue(statValueFilterList.Count == 2);
            Assert.IsTrue(
                statValueFilterList.Select(p => p.StatValue)
                    .All(p => p != 1.11));

            //expect failure when an invalid double is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                statValueFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(statValueFilter).ToList();

            });

            //expect 2 entries to match for a nullable boolean field
            var nullableStatValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "PossiblyEmptyStatValue",
                        Id = "PossiblyEmptyStatValue",
                        Input = "NA",
                        Operator = "not_equal",
                        Type = "double",
                        Value = new[] { "1.112" }
                    }
                }
            };
            var nullableStatFilterList = StartingQuery.BuildQuery(nullableStatValueFilter).ToList();
            Assert.IsTrue(nullableStatFilterList != null);
            Assert.IsTrue(nullableStatFilterList.Count == 2);
            Assert.IsTrue(
                nullableStatFilterList.Select(p => p.PossiblyEmptyStatValue)
                    .All(p => p != 1.112));

        }

        [Test]
        public void BetweenClause()
        {
            //expect 3 entries to match for an integer comparison
            var contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "between",
                        Type = "integer",
                        Value = new[] { "1", "2" }
                    }
                }
            };
            var contentIdFilteredList = StartingQuery.BuildQuery(contentIdFilter).ToList();
            Assert.IsTrue(contentIdFilteredList != null);
            Assert.IsTrue(contentIdFilteredList.Count == 3);
            Assert.IsTrue(contentIdFilteredList.All(p => p.ContentTypeId < 3));

            //expect failure when non-numeric value is encountered in integer comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                contentIdFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(contentIdFilter).ToList();

            });

            //expect 2 entries to match for an integer comparison
            var nullableContentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeId",
                        Id = "NullableContentTypeId",
                        Input = "NA",
                        Operator = "between",
                        Type = "integer",
                        Value = new[] { "1", "2" }
                    }
                }
            };
            var nullableContentIdFilteredList =
                StartingQuery.BuildQuery(nullableContentIdFilter).ToList();
            Assert.IsTrue(nullableContentIdFilteredList != null);
            Assert.IsTrue(nullableContentIdFilteredList.Count == 2);
            Assert.IsTrue(nullableContentIdFilteredList.All(p => p.NullableContentTypeId < 3));

            //expect 4 entries to match for a Date comparison
            var lastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModified",
                        Id = "LastModified",
                        Input = "NA",
                        Operator = "between",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(-2).ToString(), DateTime.UtcNow.Date.ToString() }
                    }
                }
            };
            var lastModifiedFilterList = StartingQuery.BuildQuery(lastModifiedFilter).ToList();
            Assert.IsTrue(lastModifiedFilterList != null);
            Assert.IsTrue(lastModifiedFilterList.Count == 4);
            Assert.IsTrue(
                lastModifiedFilterList.Select(p => p.LastModified)
                    .All(p => (p >= DateTime.UtcNow.Date.AddDays(-2)) && (p <= DateTime.UtcNow.Date)));

            //expect failure when an invalid date is encountered in date comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                lastModifiedFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(lastModifiedFilter).ToList();

            });

            //expect 3 entries to match for a possibly empty Date comparison
            var nullableLastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModifiedIfPresent",
                        Id = "LastModifiedIfPresent",
                        Input = "NA",
                        Operator = "between",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(-2).ToString(), DateTime.UtcNow.Date.ToString() }
                    }
                }
            };
            var nullableLastModifiedFilterList = StartingQuery.BuildQuery(nullableLastModifiedFilter).ToList();
            Assert.IsTrue(nullableLastModifiedFilterList != null);
            Assert.IsTrue(nullableLastModifiedFilterList.Count == 3);
            Assert.IsTrue(
                nullableLastModifiedFilterList.Select(p => p.LastModifiedIfPresent)
                    .All(p => (p >= DateTime.UtcNow.Date.AddDays(-2)) && (p <= DateTime.UtcNow.Date)));


            //expect 3 entries to match for a double field
            var statValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StatValue",
                        Id = "StatValue",
                        Input = "NA",
                        Operator = "between",
                        Type = "double",
                        Value = new[] { "1.0", "1.12" }
                    }
                }
            };
            var statValueFilterList = StartingQuery.BuildQuery(statValueFilter).ToList();
            Assert.IsTrue(statValueFilterList != null);
            Assert.IsTrue(statValueFilterList.Count == 3);
            Assert.IsTrue(
                statValueFilterList.Select(p => p.StatValue)
                    .All(p => (p >= 1.0) && (p <= 1.12)));

            //expect failure when an invalid double is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                statValueFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(statValueFilter).ToList();

            });

            //expect 2 entries to match for a nullable boolean field
            var nullableStatValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "PossiblyEmptyStatValue",
                        Id = "PossiblyEmptyStatValue",
                        Input = "NA",
                        Operator = "between",
                        Type = "double",
                        Value = new[] { "1.112", "1.112" }
                    }
                }
            };
            var nullableStatFilterList = StartingQuery.BuildQuery(nullableStatValueFilter).ToList();
            Assert.IsTrue(nullableStatFilterList != null);
            Assert.IsTrue(nullableStatFilterList.Count == 2);
            Assert.IsTrue(
                nullableStatFilterList.Select(p => p.PossiblyEmptyStatValue)
                    .All(p => p == 1.112));

        }

        [Test]
        public void NotBetweenClause()
        {
            //expect 1 entry to match for an integer comparison
            var contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "not_between",
                        Type = "integer",
                        Value = new[] { "1", "2" }
                    }
                }
            };
            var contentIdFilteredList = StartingQuery.BuildQuery(contentIdFilter).ToList();
            Assert.IsTrue(contentIdFilteredList != null);
            Assert.IsTrue(contentIdFilteredList.Count == 1);
            Assert.IsTrue(contentIdFilteredList.All(p => p.ContentTypeId < 1 || p.ContentTypeId > 2));

            //expect failure when non-numeric value is encountered in integer comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                contentIdFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(contentIdFilter).ToList();

            });

            //expect 2 entries to match for an integer comparison
            var nullableContentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeId",
                        Id = "NullableContentTypeId",
                        Input = "NA",
                        Operator = "not_between",
                        Type = "integer",
                        Value = new[] { "1", "2" }
                    }
                }
            };
            var nullableContentIdFilteredList =
                StartingQuery.BuildQuery(nullableContentIdFilter).ToList();
            Assert.IsTrue(nullableContentIdFilteredList != null);
            Assert.IsTrue(nullableContentIdFilteredList.Count == 2);
            Assert.IsTrue(nullableContentIdFilteredList.All(p => p.NullableContentTypeId < 1 || p.NullableContentTypeId > 2 || p.NullableContentTypeId == null));

            //expect 0 entries to match for a Date comparison
            var lastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModified",
                        Id = "LastModified",
                        Input = "NA",
                        Operator = "not_between",
                        Type = "datetime",
                        Value = new[] {DateTime.UtcNow.Date.AddDays(-2).ToString(), DateTime.UtcNow.Date.ToString() }
                    }
                }
            };
            var lastModifiedFilterList = StartingQuery.BuildQuery(lastModifiedFilter).ToList();
            Assert.IsTrue(lastModifiedFilterList != null);
            Assert.IsTrue(lastModifiedFilterList.Count == 0);
            Assert.IsTrue(
                lastModifiedFilterList.Select(p => p.LastModified)
                    .All(p => (p <= DateTime.UtcNow.Date.AddDays(-2)) && (p >= DateTime.UtcNow.Date)));

            //expect failure when an invalid date is encountered in date comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                lastModifiedFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(lastModifiedFilter).ToList();

            });

            //expect 1 entries to match for a possibly empty Date comparison
            var nullableLastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModifiedIfPresent",
                        Id = "LastModifiedIfPresent",
                        Input = "NA",
                        Operator = "not_between",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(-2).ToString(), DateTime.UtcNow.Date.ToString() }
                    }
                }
            };
            var nullableLastModifiedFilterList = StartingQuery.BuildQuery(nullableLastModifiedFilter).ToList();
            Assert.IsTrue(nullableLastModifiedFilterList != null);
            Assert.IsTrue(nullableLastModifiedFilterList.Count == 1);
            Assert.IsTrue(
                nullableLastModifiedFilterList.Select(p => p.LastModifiedIfPresent)
                    .All(p => (p <= DateTime.UtcNow.Date.AddDays(-2) && p >= DateTime.UtcNow.Date) || p == null));


            //expect 3 entries to match for a double field
            var statValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StatValue",
                        Id = "StatValue",
                        Input = "NA",
                        Operator = "not_between",
                        Type = "double",
                        Value = new[] { "1.0", "1.12" }
                    }
                }
            };
            var statValueFilterList = StartingQuery.BuildQuery(statValueFilter).ToList();
            Assert.IsTrue(statValueFilterList != null);
            Assert.IsTrue(statValueFilterList.Count == 1);
            Assert.IsTrue(
                statValueFilterList.Select(p => p.StatValue)
                    .All(p => (p <= 1.0) || (p >= 1.12)));

            //expect failure when an invalid double is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                statValueFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(statValueFilter).ToList();

            });

            //expect 2 entries to match for a nullable boolean field
            var nullableStatValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "PossiblyEmptyStatValue",
                        Id = "PossiblyEmptyStatValue",
                        Input = "NA",
                        Operator = "not_between",
                        Type = "double",
                        Value = new[] { "1.112", "1.112" }
                    }
                }
            };
            var nullableStatFilterList = StartingQuery.BuildQuery(nullableStatValueFilter).ToList();
            Assert.IsTrue(nullableStatFilterList != null);
            Assert.IsTrue(nullableStatFilterList.Count == 2);
            Assert.IsTrue(
                nullableStatFilterList.Select(p => p.PossiblyEmptyStatValue)
                    .All(p => p != 1.112));

        }

        [Test]
        public void GreaterOrEqualClause()
        {
            //expect 1 entries to match for an integer comparison
            var contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "greater_or_equal",
                        Type = "integer",
                        Value = new[] { "2" }
                    }
                }
            };
            var contentIdFilteredList = StartingQuery.BuildQuery(contentIdFilter).ToList();
            Assert.IsTrue(contentIdFilteredList != null);
            Assert.IsTrue(contentIdFilteredList.Count == 2);
            Assert.IsTrue(contentIdFilteredList.All(p => p.ContentTypeId >= 2));

            //expect failure when non-numeric value is encountered in integer comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                contentIdFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(contentIdFilter).ToList();

            });

            //expect 1 entries to match for an integer comparison
            var nullableContentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeId",
                        Id = "NullableContentTypeId",
                        Input = "NA",
                        Operator = "greater_or_equal",
                        Type = "integer",
                        Value = new[] { "2" }
                    }
                }
            };
            var nullableContentIdFilteredList =
                StartingQuery.BuildQuery(nullableContentIdFilter).ToList();
            Assert.IsTrue(nullableContentIdFilteredList != null);
            Assert.IsTrue(nullableContentIdFilteredList.Count == 2);
            Assert.IsTrue(nullableContentIdFilteredList.All(p => p.NullableContentTypeId >= 2));


            //expect 4 entries to match for a Date comparison
            var lastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModified",
                        Id = "LastModified",
                        Input = "NA",
                        Operator = "greater_or_equal",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(-2).ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var lastModifiedFilterList = StartingQuery.BuildQuery(lastModifiedFilter).ToList();
            Assert.IsTrue(lastModifiedFilterList != null);
            Assert.IsTrue(lastModifiedFilterList.Count == 4);
            Assert.IsTrue(
                lastModifiedFilterList.Select(p => p.LastModified)
                    .All(p => (p >= DateTime.UtcNow.Date.AddDays(-2))));

            //expect failure when an invalid date is encountered in date comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                lastModifiedFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(lastModifiedFilter).ToList();

            });

            //expect 0 entries to match for a possibly empty Date comparison
            var nullableLastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModifiedIfPresent",
                        Id = "LastModifiedIfPresent",
                        Input = "NA",
                        Operator = "greater_or_equal",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(1).ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var nullableLastModifiedFilterList = StartingQuery.BuildQuery(nullableLastModifiedFilter).ToList();
            Assert.IsTrue(nullableLastModifiedFilterList != null);
            Assert.IsTrue(nullableLastModifiedFilterList.Count == 0);
            Assert.IsTrue(
                nullableLastModifiedFilterList.Select(p => p.LastModifiedIfPresent)
                    .All(p => (p >= DateTime.UtcNow.Date.AddDays(1))));


            //expect 4 entries to match for a double field
            var statValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StatValue",
                        Id = "StatValue",
                        Input = "NA",
                        Operator = "greater_or_equal",
                        Type = "double",
                        Value = new[] { "1" }
                    }
                }
            };
            var statValueFilterList = StartingQuery.BuildQuery(statValueFilter).ToList();
            Assert.IsTrue(statValueFilterList != null);
            Assert.IsTrue(statValueFilterList.Count == 4);
            Assert.IsTrue(
                statValueFilterList.Select(p => p.StatValue)
                    .All(p => (p >= 1)));

            //expect failure when an invalid double is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                statValueFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(statValueFilter).ToList();

            });

            //expect 2 entries to match for a nullable boolean field
            var nullableStatValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "PossiblyEmptyStatValue",
                        Id = "PossiblyEmptyStatValue",
                        Input = "NA",
                        Operator = "greater_or_equal",
                        Type = "double",
                        Value = new[] { "1.112" }
                    }
                }
            };
            var nullableStatFilterList = StartingQuery.BuildQuery(nullableStatValueFilter).ToList();
            Assert.IsTrue(nullableStatFilterList != null);
            Assert.IsTrue(nullableStatFilterList.Count == 2);
            Assert.IsTrue(
                nullableStatFilterList.Select(p => p.PossiblyEmptyStatValue)
                    .All(p => p >= 1.112));

        }

        [Test]
        public void GreaterClause()
        {
            //expect 1 entries to match for an integer comparison
            var contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "greater",
                        Type = "integer",
                        Value = new[] { "2" }
                    }
                }
            };
            var contentIdFilteredList = StartingQuery.BuildQuery(contentIdFilter).ToList();
            Assert.IsTrue(contentIdFilteredList != null);
            Assert.IsTrue(contentIdFilteredList.Count == 1);
            Assert.IsTrue(contentIdFilteredList.All(p => p.ContentTypeId > 2));

            //expect failure when non-numeric value is encountered in integer comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                contentIdFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(contentIdFilter).ToList();

            });

            //expect 1 entries to match for an integer comparison
            var nullableContentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeId",
                        Id = "NullableContentTypeId",
                        Input = "NA",
                        Operator = "greater",
                        Type = "integer",
                        Value = new[] { "2" }
                    }
                }
            };
            var nullableContentIdFilteredList =
                StartingQuery.BuildQuery(nullableContentIdFilter).ToList();
            Assert.IsTrue(nullableContentIdFilteredList != null);
            Assert.IsTrue(nullableContentIdFilteredList.Count == 1);
            Assert.IsTrue(nullableContentIdFilteredList.All(p => p.NullableContentTypeId > 2));


            //expect 4 entries to match for a Date comparison
            var lastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModified",
                        Id = "LastModified",
                        Input = "NA",
                        Operator = "greater",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(-2).ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var lastModifiedFilterList = StartingQuery.BuildQuery(lastModifiedFilter).ToList();
            Assert.IsTrue(lastModifiedFilterList != null);
            Assert.IsTrue(lastModifiedFilterList.Count == 4);
            Assert.IsTrue(
                lastModifiedFilterList.Select(p => p.LastModified)
                    .All(p => (p > DateTime.UtcNow.Date.AddDays(-2))));

            //expect failure when an invalid date is encountered in date comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                lastModifiedFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(lastModifiedFilter).ToList();

            });

            //expect 0 entries to match for a possibly empty Date comparison
            var nullableLastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModifiedIfPresent",
                        Id = "LastModifiedIfPresent",
                        Input = "NA",
                        Operator = "greater",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(1).ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var nullableLastModifiedFilterList = StartingQuery.BuildQuery(nullableLastModifiedFilter).ToList();
            Assert.IsTrue(nullableLastModifiedFilterList != null);
            Assert.IsTrue(nullableLastModifiedFilterList.Count == 0);
            Assert.IsTrue(
                nullableLastModifiedFilterList.Select(p => p.LastModifiedIfPresent)
                    .All(p => (p > DateTime.UtcNow.Date.AddDays(1))));


            //expect 4 entries to match for a double field
            var statValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StatValue",
                        Id = "StatValue",
                        Input = "NA",
                        Operator = "greater",
                        Type = "double",
                        Value = new[] { "1" }
                    }
                }
            };
            var statValueFilterList = StartingQuery.BuildQuery(statValueFilter).ToList();
            Assert.IsTrue(statValueFilterList != null);
            Assert.IsTrue(statValueFilterList.Count == 4);
            Assert.IsTrue(
                statValueFilterList.Select(p => p.StatValue)
                    .All(p => (p > 1)));

            //expect failure when an invalid double is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                statValueFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(statValueFilter).ToList();

            });

            //expect 2 entries to match for a nullable boolean field
            var nullableStatValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "PossiblyEmptyStatValue",
                        Id = "PossiblyEmptyStatValue",
                        Input = "NA",
                        Operator = "greater",
                        Type = "double",
                        Value = new[] { "1.112" }
                    }
                }
            };
            var nullableStatFilterList = StartingQuery.BuildQuery(nullableStatValueFilter).ToList();
            Assert.IsTrue(nullableStatFilterList != null);
            Assert.IsTrue(nullableStatFilterList.Count == 0);
            Assert.IsTrue(
                nullableStatFilterList.Select(p => p.PossiblyEmptyStatValue)
                    .All(p => p > 1.112));

        }

        [Test]
        public void LessClause()
        {
            //expect 2 entries to match for an integer comparison
            var contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "less",
                        Type = "integer",
                        Value = new[] { "2" }
                    }
                }
            };
            var contentIdFilteredList = StartingQuery.BuildQuery(contentIdFilter).ToList();
            Assert.IsTrue(contentIdFilteredList != null);
            Assert.IsTrue(contentIdFilteredList.Count == 2);
            Assert.IsTrue(contentIdFilteredList.All(p => p.ContentTypeId < 2));

            //expect failure when non-numeric value is encountered in integer comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                contentIdFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(contentIdFilter).ToList();

            });

            //expect 1 entries to match for an integer comparison
            var nullableContentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeId",
                        Id = "NullableContentTypeId",
                        Input = "NA",
                        Operator = "less",
                        Type = "integer",
                        Value = new[] { "2" }
                    }
                }
            };
            var nullableContentIdFilteredList =
                StartingQuery.BuildQuery(nullableContentIdFilter).ToList();
            Assert.IsTrue(nullableContentIdFilteredList != null);
            Assert.IsTrue(nullableContentIdFilteredList.Count == 1);
            Assert.IsTrue(nullableContentIdFilteredList.All(p => p.NullableContentTypeId < 2));


            //expect 0 entries to match for a Date comparison
            var lastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModified",
                        Id = "LastModified",
                        Input = "NA",
                        Operator = "less",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(-2).ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var lastModifiedFilterList = StartingQuery.BuildQuery(lastModifiedFilter).ToList();
            Assert.IsTrue(lastModifiedFilterList != null);
            Assert.IsTrue(lastModifiedFilterList.Count == 0);
            Assert.IsTrue(
                lastModifiedFilterList.Select(p => p.LastModified)
                    .All(p => (p <= DateTime.UtcNow.Date.AddDays(-2))));

            //expect failure when an invalid date is encountered in date comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                lastModifiedFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(lastModifiedFilter).ToList();

            });

            //expect 3 entries to match for a possibly empty Date comparison
            var nullableLastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModifiedIfPresent",
                        Id = "LastModifiedIfPresent",
                        Input = "NA",
                        Operator = "less",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(1).ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var nullableLastModifiedFilterList = StartingQuery.BuildQuery(nullableLastModifiedFilter).ToList();
            Assert.IsTrue(nullableLastModifiedFilterList != null);
            Assert.IsTrue(nullableLastModifiedFilterList.Count == 3);
            Assert.IsTrue(
                nullableLastModifiedFilterList.Select(p => p.LastModifiedIfPresent)
                    .All(p => (p <= DateTime.UtcNow.Date.AddDays(1))));


            //expect 3 entries to match for a double field
            var statValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StatValue",
                        Id = "StatValue",
                        Input = "NA",
                        Operator = "less",
                        Type = "double",
                        Value = new[] { "1.13" }
                    }
                }
            };
            var statValueFilterList = StartingQuery.BuildQuery(statValueFilter).ToList();
            Assert.IsTrue(statValueFilterList != null);
            Assert.IsTrue(statValueFilterList.Count == 3);
            Assert.IsTrue(
                statValueFilterList.Select(p => p.StatValue)
                    .All(p => (p <= 1.12)));

            //expect failure when an invalid double is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                statValueFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(statValueFilter).ToList();

            });

            //expect 2 entries to match for a nullable boolean field
            var nullableStatValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "PossiblyEmptyStatValue",
                        Id = "PossiblyEmptyStatValue",
                        Input = "NA",
                        Operator = "less",
                        Type = "double",
                        Value = new[] { "1.113" }
                    }
                }
            };
            var nullableStatFilterList = StartingQuery.BuildQuery(nullableStatValueFilter).ToList();
            Assert.IsTrue(nullableStatFilterList != null);
            Assert.IsTrue(nullableStatFilterList.Count == 2);
            Assert.IsTrue(
                nullableStatFilterList.Select(p => p.PossiblyEmptyStatValue)
                    .All(p => p < 1.113));

        }

        [Test]
        public void LessOrEqualClause()
        {
            //expect 3 entries to match for an integer comparison
            var contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "less_or_equal",
                        Type = "integer",
                        Value = new[] { "2" }
                    }
                }
            };
            var contentIdFilteredList = StartingQuery.BuildQuery(contentIdFilter).ToList();
            Assert.IsTrue(contentIdFilteredList != null);
            Assert.IsTrue(contentIdFilteredList.Count == 3);
            Assert.IsTrue(contentIdFilteredList.All(p => p.ContentTypeId <= 2));

            //expect failure when non-numeric value is encountered in integer comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                contentIdFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(contentIdFilter).ToList();

            });

            //expect 2 entries to match for an integer comparison
            var nullableContentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "NullableContentTypeId",
                        Id = "NullableContentTypeId",
                        Input = "NA",
                        Operator = "less_or_equal",
                        Type = "integer",
                        Value = new[] { "2" }
                    }
                }
            };
            var nullableContentIdFilteredList =
                StartingQuery.BuildQuery(nullableContentIdFilter).ToList();
            Assert.IsTrue(nullableContentIdFilteredList != null);
            Assert.IsTrue(nullableContentIdFilteredList.Count == 2);
            Assert.IsTrue(nullableContentIdFilteredList.All(p => p.NullableContentTypeId <= 2));


            //expect 0 entries to match for a Date comparison
            var lastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModified",
                        Id = "LastModified",
                        Input = "NA",
                        Operator = "less_or_equal",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(-2).ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var lastModifiedFilterList = StartingQuery.BuildQuery(lastModifiedFilter).ToList();
            Assert.IsTrue(lastModifiedFilterList != null);
            Assert.IsTrue(lastModifiedFilterList.Count == 0);
            Assert.IsTrue(
                lastModifiedFilterList.Select(p => p.LastModified)
                    .All(p => (p <= DateTime.UtcNow.Date.AddDays(-2))));

            //expect failure when an invalid date is encountered in date comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                lastModifiedFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(lastModifiedFilter).ToList();

            });

            //expect 3 entries to match for a possibly empty Date comparison
            var nullableLastModifiedFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "LastModifiedIfPresent",
                        Id = "LastModifiedIfPresent",
                        Input = "NA",
                        Operator = "less_or_equal",
                        Type = "datetime",
                        Value = new[] { DateTime.UtcNow.Date.AddDays(1).ToString("d", CultureInfo.InvariantCulture) }
                    }
                }
            };
            var nullableLastModifiedFilterList = StartingQuery.BuildQuery(nullableLastModifiedFilter).ToList();
            Assert.IsTrue(nullableLastModifiedFilterList != null);
            Assert.IsTrue(nullableLastModifiedFilterList.Count == 3);
            Assert.IsTrue(
                nullableLastModifiedFilterList.Select(p => p.LastModifiedIfPresent)
                    .All(p => (p <= DateTime.UtcNow.Date.AddDays(1))));


            //expect 3 entries to match for a double field
            var statValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "StatValue",
                        Id = "StatValue",
                        Input = "NA",
                        Operator = "less_or_equal",
                        Type = "double",
                        Value = new[] { "1.13" }
                    }
                }
            };
            var statValueFilterList = StartingQuery.BuildQuery(statValueFilter).ToList();
            Assert.IsTrue(statValueFilterList != null);
            Assert.IsTrue(statValueFilterList.Count == 4);
            Assert.IsTrue(
                statValueFilterList.Select(p => p.StatValue)
                    .All(p => (p <= 1.13)));

            //expect failure when an invalid double is encountered in double comparison
            ExceptionAssert.Throws<Exception>(() =>
            {
                statValueFilter.Rules.First().Value = new[] { "hello" };
                StartingQuery.BuildQuery(statValueFilter).ToList();

            });

            //expect 2 entries to match for a nullable double field
            var nullableStatValueFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "PossiblyEmptyStatValue",
                        Id = "PossiblyEmptyStatValue",
                        Input = "NA",
                        Operator = "less_or_equal",
                        Type = "double",
                        Value = new[] { "1.113" }
                    }
                }
            };
            var nullableStatFilterList = StartingQuery.BuildQuery(nullableStatValueFilter).ToList();
            Assert.IsTrue(nullableStatFilterList != null);
            Assert.IsTrue(nullableStatFilterList.Count == 2);
            Assert.IsTrue(
                nullableStatFilterList.Select(p => p.PossiblyEmptyStatValue)
                    .All(p => p <= 1.113));

        }

        [Test]
        public void FilterWithInvalidParameters()
        {
            //expect 3 entries to match for an integer comparison
            var contentIdFilter = new QueryBuilderFilterRule
            {
                Condition = "and",
                Rules = new List<QueryBuilderFilterRule>
                {
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "less_or_equal",
                        Type = "integer",
                        Value = new[] { "2" }
                    },
                    new QueryBuilderFilterRule
                    {
                        Condition = "and",
                        Field = "ContentTypeId",
                        Id = "ContentTypeId",
                        Input = "NA",
                        Operator = "less_or_equal",
                        Type = "integer",
                        Value = new[] { "2" }
                    }
                }
            };

            StartingQuery.BuildQuery(contentIdFilter).ToList();

            contentIdFilter.Condition = "or";

            StartingQuery.BuildQuery(contentIdFilter).ToList();

            StartingQuery.BuildQuery(null).ToList();

            StartingQuery.BuildQuery(new QueryBuilderFilterRule()).ToList();

            ExceptionAssert.Throws<Exception>(() =>
            {
                contentIdFilter.Rules.First().Type = "NOT_A_TYPE";
                StartingQuery.BuildQuery(contentIdFilter).ToList();

            });

            ExceptionAssert.Throws<Exception>(() =>
            {
                contentIdFilter.Rules.First().Type = "integer";
                contentIdFilter.Rules.First().Operator = "NOT_AN_OPERATOR";
                StartingQuery.BuildQuery(contentIdFilter).ToList();
            });
        }

        private class IndexedClass
        {
            int this[string someIndex]
            {
                get
                {
                    return 2;
                }
            }
        }

        [Test]
        public void IndexedExpression_Test()
        {
            var rule = new QueryBuilderFilterRule
            {
                Condition = "and",
                Field = "ContentTypeId",
                Id = "ContentTypeId",
                Input = "NA",
                Operator = "equal",
                Type = "integer",
                Value = new[] { "2" }
            };

            var result = new List<IndexedClass> { new IndexedClass() }.AsQueryable().BuildQuery(rule,
                new BuildExpressionOptions() { UseIndexedProperty = true, IndexedPropertyName = "Item" });
            Assert.IsTrue(result.Any());

            rule.Value = new[] { "3" };
            result = new[] { new IndexedClass() }.BuildQuery(rule, true, "Item");
            Assert.IsFalse(result.Any());
        }


        private class DictionaryClass
        {
            public DictionaryClass() { Data = new Dictionary<string, DataValue>(); }

            public Dictionary<string, DataValue> Data { get; set; }
            
        }

        private class DataValue
        { 
            public string StringValue { get; set; }
        }

        [Test]
        public void Dictionary_Test()
        {
            var rule = new JsonNetFilterRule()
            {
                Condition = "and",
                Rules = new List<JsonNetFilterRule>() { 
                    new JsonNetFilterRule() {
                        Field = "Data[\"myKey\"].StringValue",
                        Id = "MyKey",
                        Input = "text",
                        Operator = "equal",
                        Type = "string",
                        Value = "BillyBob" 
                    }
                }
            };

            var d1 = new DictionaryClass();
            d1.Data.Add("myKey", new DataValue() { StringValue = "BillyBob" });
            var d2 = new DictionaryClass();
            d2.Data.Add("myKey", new DataValue() { StringValue = "NotBillyBob" });

            var result = new List<DictionaryClass> { d1, d2 }.AsQueryable().BuildQuery(rule,
                new BuildExpressionOptions());

            Assert.IsTrue(result.Any());
            Assert.AreEqual(result.Count(), 1);

            var resultList = result.ToList();
            Assert.AreEqual(resultList[0].Data["myKey"].StringValue, "BillyBob");
        }


        #endregion
    }
}
