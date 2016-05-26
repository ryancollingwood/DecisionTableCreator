﻿/*
 * [The "BSD license"]
 * Copyright (c) 2016 Peter Hoch
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DecisionTableCreator.TestCases;
using NUnit.Framework;
using UnitTestSupport;

namespace UnitTests2
{
    [TestFixture]
    public class TestCaseCalculation
    {
        [SetUp]
        public void Setup()
        {
            TestSupport.ClearCreatedFiles();
            TestSupport.DiffAction = new InvokeWinMerge();
        }


        [TestCase(2, -1, -1, -1, 2)]
        [TestCase(1, -1, -1, -1, 1)]
        [TestCase(1, 0, -1, -1, 1)]
        [TestCase(1, 0, -1, 0, 0)]
        [TestCase(1, 0, 0, -1, 0)]
        [TestCase(1, 0, 0, 0, 0)]
        [TestCase(5, 0, 0, 0, 4)]
        [TestCase(5, 0, 0, 1, 3)]
        [TestCase(0, 0, 0, 0, 0)]
        public void TestCalculationEnumValues(int count, int defaultIndex, int invalidIndex, int dontCareIndex, int expectedResult)
        {
            var condition = ConditionObject.Create("name", TestCasesRoot.CreateSampleEnum("name", count, defaultIndex, invalidIndex, dontCareIndex));
            int result = TestCasesRoot.CalculateEnumValuesWithoutDontCareAndInvalid(condition);
            Assert.That(result == expectedResult);
        }

        [Test]
        public void TestCalculationConditions()
        {
            TestCasesRoot tcr = new TestCasesRoot();
            int expectedResult = 0;
            int result = tcr.CalculatePossibleCombinations();
            Assert.That(result == expectedResult);

            expectedResult = 4;
            tcr.Conditions.Add(ConditionObject.Create("name", TestCasesRoot.CreateSampleEnum("name", 4, 0, 0, 3))); 
            tcr.Conditions.Add(ConditionObject.Create("name", TestCasesRoot.CreateSampleEnum("name", 4, 0, 0, 3)));
            result = tcr.CalculatePossibleCombinations();
            Assert.That(result == expectedResult);

            // 2 invalids
            var cond = ConditionObject.Create("name", TestCasesRoot.CreateSampleEnum("name", 6, 0, 0, 3));
            cond.EnumValues[5].IsInvalid = true;
            tcr.Conditions.Add(cond);
            expectedResult *= 3;
            result = tcr.CalculatePossibleCombinations();
            Assert.That(result == expectedResult);

            cond = ConditionObject.Create("name", TestCasesRoot.CreateSampleEnum("name", 2, 0, 0, 1));
            tcr.Conditions.Add(cond);
            result = tcr.CalculatePossibleCombinations();
            Assert.That(result == expectedResult);

            cond = ConditionObject.Create("name", TestCasesRoot.CreateSampleEnum("name", 1, 0, 0, 0));
            tcr.Conditions.Add(cond);
            result = tcr.CalculatePossibleCombinations();
            Assert.That(result == expectedResult);

            cond = ConditionObject.Create("name", TestCasesRoot.CreateSampleEnum("name", 0, 0, 0, 0));
            tcr.Conditions.Add(cond);
            result = tcr.CalculatePossibleCombinations();
            Assert.That(result == expectedResult);

            cond = ConditionObject.Create("name", TestCasesRoot.CreateSampleEnum("name", 2, 0, -1, -1));
            tcr.Conditions.Add(cond);
            expectedResult *= 2;
            result = tcr.CalculatePossibleCombinations();
            Assert.That(result == expectedResult);

        }

        [TestCase("Sample.dtc", 8, 8, 100.0)]
        public void CalculateCoverage(string fileName, int expectedCombinations, int expectedUniqueTestCases, double expectedCoverage)
        {
            string testSettingPath = Path.Combine(TestSupport.CreatedFilesDirectory, "TestSetting.txt");

            string source = Path.Combine(TestSupport.TestFilesDirectory, fileName);
            TestCasesRoot tcr = new TestCasesRoot();
            tcr.Load(source);
            Statistics stat = tcr.CalculateStatistics();

            for (int idx = 0; idx < tcr.TestCases.Count; idx++)
            {
                File.AppendAllText(testSettingPath, String.Format("{0}" + Environment.NewLine, tcr.TestCases[idx]));
            }

            Assert.That(stat.Coverage.Equals(expectedCoverage));
            Assert.That(stat.CoveredTestCases == expectedUniqueTestCases);
            Assert.That(stat.PossibleCombinations == expectedCombinations);
        }


        [TestCase(1, 1, 2, 2, true)]
        [TestCase(0, 0, 3, 3, true)]
        [TestCase(2, 2, 0, 0, true)]
        [TestCase(3, 3, 1, 1, true)]
        [TestCase(0, 1, 2, 2, false)]
        [TestCase(0, 2, 1, 1, false)]
        [TestCase(0, 3, 1, 1, false)]
        [TestCase(0, 3, 0, 1, false)]
        [TestCase(0, 3, 0, 2, false)]
        [TestCase(0, 3, 0, 3, false)]
        public void CompareTestCasesTest(int idx1, int idx2, int idx3, int idx4, bool expectedResult)
        {
            var enum1 = TestCasesRoot.CreateSampleEnum("enum1", 4, 0, 0, 3);
            var enum2 = TestCasesRoot.CreateSampleEnum("enum2", 4, 0, 0, 1);
            List<ValueObject> conditions1 = new List<ValueObject>();
            conditions1.Add(new ValueObject(enum1, idx1));
            conditions1.Add(new ValueObject(enum2, idx3));
            List<ValueObject> conditions2 = new List<ValueObject>();
            conditions2.Add(new ValueObject(enum1, idx2));
            conditions2.Add(new ValueObject(enum2, idx4));
            TestCase tc1 = new TestCase("tc1", conditions1, new List<ValueObject>());
            TestCase tc2 = new TestCase("tc2", conditions2, new List<ValueObject>());
            bool result = tc1.TestSettingIsEqual(tc2);
            Assert.That(result == expectedResult);
        }

        [TestCase(1, 1, 1, 1, 1, 1, true, false, 1)]
        [TestCase(1, 1, 1, 1, 1, 2, true, true, 2)]
        [TestCase(1, 1, 1, 1, 2, 1, true, true, 2)]
        [TestCase(1, 1, 1, 2, 1, 1, true, true, 2)]
        [TestCase(2, 2, 2, 1, 1, 1, true, true, 2)]

        [TestCase(1, 1, 1, 1, 1, 5, false, true, 4)]
        public void CompareTestCases2Test(int selected11, int selected12, int selected13, int selected21, int selected22, int selected23, bool unique1, bool unique2, int expectedCoveredCount)
        {
            var enum1 = TestCasesRoot.CreateSampleEnum("enum1", 6, 0, 0, 5);
            var enum2 = TestCasesRoot.CreateSampleEnum("enum2", 6, 0, 0, 5);
            var enum3 = TestCasesRoot.CreateSampleEnum("enum3", 6, 0, 0, 5);
            List<ValueObject> conditions1 = new List<ValueObject>();
            conditions1.Add(new ValueObject(enum1, selected11));
            conditions1.Add(new ValueObject(enum2, selected12));
            conditions1.Add(new ValueObject(enum3, selected13));
            List<ValueObject> conditions2 = new List<ValueObject>();
            conditions2.Add(new ValueObject(enum1, selected21));
            conditions2.Add(new ValueObject(enum2, selected22));
            conditions2.Add(new ValueObject(enum3, selected23));
            TestCase tc1 = new TestCase("tc1", conditions1, new List<ValueObject>());
            TestCase tc2 = new TestCase("tc2", conditions2, new List<ValueObject>());
            List<TestCase> testCases = new List<TestCase>() {tc1,tc2};
            Trace.WriteLine(tc1);
            Trace.WriteLine(tc2);
            //bool isEqual = tc1.TestSettingIsEqual(tc2);
            Assert.That( TestCase.UpdateUniqueness(tc1,tc2) == (unique1 && unique2));
            int count = TestCasesRoot.CalculateNumberOfUniqueCoveredTestCases(testCases);
            Assert.That(tc1.TestCaseIsUnique == unique1);
            Assert.That(tc2.TestCaseIsUnique == unique2);
            Assert.That(count == expectedCoveredCount);
        }




        [TestCase(0, new int[] { 0, 2, 4, 6 }, 4)]
        [TestCase(1, new int[] { 0, 1, 4, 6 }, 3)]
        [TestCase(2, new int[] { 0, 1, 4, 8, 12, 13 }, 4)]
        [TestCase(3, new int[] { 0, 1, 4, 8, 12, 13, 14, 15 }, 4)]
        [TestCase(4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 7)]
        [TestCase(4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 7)]
        public void CalculateUniqueTestCases(int id, int[] values, int expectedResult)
        {
            List<List<ValueObject>> listOfValueLists = new List<List<ValueObject>>();

            var enum1 = TestCasesRoot.CreateSampleEnum("enum1", 4, 0, 0, -1);
            var enum2 = TestCasesRoot.CreateSampleEnum("enum2", 4, 0, 0, 1);
            var enum3 = TestCasesRoot.CreateSampleEnum("enum3", 6, 0, 0, -1);

            //0
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 1), new ValueObject(enum2, 2), new ValueObject(enum3, 2), });
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 1), new ValueObject(enum2, 2), new ValueObject(enum3, 2), });
            //2
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 2), new ValueObject(enum2, 3), new ValueObject(enum3, 3), });
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 2), new ValueObject(enum2, 3), new ValueObject(enum3, 3), });
            //4                                                         
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 3), new ValueObject(enum2, 3), new ValueObject(enum3, 3), });
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 3), new ValueObject(enum2, 3), new ValueObject(enum3, 3), });
            //6
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 3), new ValueObject(enum2, 2), new ValueObject(enum3, 3), });
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 3), new ValueObject(enum2, 2), new ValueObject(enum3, 3), });
            //8
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 3), new ValueObject(enum2, 3), new ValueObject(enum3, 4), });
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 3), new ValueObject(enum2, 3), new ValueObject(enum3, 4), });
            //10
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 1), new ValueObject(enum2, 2), new ValueObject(enum3, 5), });
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 1), new ValueObject(enum2, 2), new ValueObject(enum3, 5), });
            //12
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 2), new ValueObject(enum2, 2), new ValueObject(enum3, 3), });
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 2), new ValueObject(enum2, 2), new ValueObject(enum3, 3), });
            //14 not valid for unique count
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 2), new ValueObject(enum2, 2), new ValueObject(enum3, 0), });
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 2), new ValueObject(enum2, 2), new ValueObject(enum3, 0), });
            //16
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 1), new ValueObject(enum2, 2), new ValueObject(enum3, 2), });
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 1), new ValueObject(enum2, 2), new ValueObject(enum3, 2), });
            //18
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 1), new ValueObject(enum2, 2), new ValueObject(enum3, 2), });
            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 1), new ValueObject(enum2, 2), new ValueObject(enum3, 2), });

            List<TestCase> testCases = new List<TestCase>();
            for (int idx = 0; idx < values.Length; idx++)
            {
                TestCase tc = new TestCase(String.Format("tc {0:d2} {1:d2}", idx, values[idx]), listOfValueLists[values[idx]], new List<ValueObject>());
                testCases.Add(tc);
                Trace.WriteLine(tc);
            }
            int result = TestCasesRoot.CalculateNumberOfUniqueCoveredTestCases(testCases);
            Assert.That(result == expectedResult);
        }


        [TestCase(0, new int[] { 31, 32, 33, 34, 37, 38, 39, 40, 71 }, 16, 16, 100)]
        [TestCase(1, new int[] { 31, 32, 33, 34, 37, 38, 39, 71 }, 16, 15, 15.0/16.0*100.0)]
        [TestCase(2, new int[] { 31, 31, 32, 33, 34, 37, 38, 39, 40, 71 }, 16, 16, 100)]
        [TestCase(3, new int[] { 31, 32, 33, 34, 37, 38, 39, 40, 71, 71 }, 16, 16, 100)]
        [TestCase(4, new int[] { 31, 32, 33, 34, 37, 38, 39, 40, 71, 11 }, 16, 16, 100)]
        [TestCase(5, new int[] { 31, 32, 33, 34, 37, 38, 39, 40, 71, 65 }, 16, 16, 100)]
        [TestCase(6, new int[] { 72 }, 16, 16, 100)]
        public void CalculateUniqueTestCasesWithDontCare(int id, int[] values, int expectedCombinations, int expectedCoveredTestCases, double expectedCovarage)
        {
            string testOutputPath = Path.Combine(TestSupport.CreatedFilesDirectory, "TestOutput.txt");
            string testSettingPath = Path.Combine(TestSupport.CreatedFilesDirectory, "TestSetting.txt");
            TestCasesRoot tcr = new TestCasesRoot();

            List<List<ValueObject>> listOfValueLists = new List<List<ValueObject>>();

            var enum1 = TestCasesRoot.CreateSampleEnum("enum1", 4, 0, 0, 3);
            var enum2 = TestCasesRoot.CreateSampleEnum("enum2", 4, 0, 0, 3);
            var enum3 = TestCasesRoot.CreateSampleEnum("enum3", 6, 0, 0, 5);

            tcr.Conditions.Add(ConditionObject.Create("name", enum1));
            tcr.Conditions.Add(ConditionObject.Create("name", enum2));
            tcr.Conditions.Add(ConditionObject.Create("name", enum3));

            for (int idx1 = 0; idx1 < 3; idx1++)
            {
                for (int idx2 = 0; idx2 < 4; idx2++)
                {
                    for (int idx3 = 0; idx3 < 6; idx3++)
                    {
                        listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, idx1), new ValueObject(enum2, idx2), new ValueObject(enum3, idx3), });
                    }
                }
            }

            listOfValueLists.Add(new List<ValueObject>() { new ValueObject(enum1, 3), new ValueObject(enum2, 3), new ValueObject(enum3, 5), });

            for (int idx = 0; idx < listOfValueLists.Count; idx++)
            {
                List<ValueObject> condList = listOfValueLists[idx];
                File.AppendAllText(testOutputPath, String.Format("{0:d2} {1} {2} {3}"+Environment.NewLine, idx, condList[0].SelectedValue.ToTestString(), condList[1].SelectedValue.ToTestString(), condList[2].SelectedValue.ToTestString()));
            }

            for (int idx = 0; idx < values.Length; idx++)
            {
                TestCase tc = new TestCase(String.Format("tc {0:d2}", idx), listOfValueLists[values[idx]], new List<ValueObject>());
                tcr.TestCases.Add(tc);
                //File.AppendAllText(testSettingPath, String.Format("{0}" + Environment.NewLine, tc));
            }
            Statistics stat = tcr.CalculateStatistics();

            for (int idx = 0; idx < tcr.TestCases.Count; idx++)
            {
                File.AppendAllText(testSettingPath, String.Format("{0}" + Environment.NewLine, tcr.TestCases[idx]));
            }

            Assert.That(stat.CoveredTestCases == expectedCoveredTestCases);
            Assert.That(stat.PossibleCombinations == expectedCombinations);
            Assert.That(stat.Coverage.Equals(expectedCovarage));
        }

    }
}
