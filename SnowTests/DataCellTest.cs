using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cleancode.Snow.Memory;

namespace SnowTests
{
    /// <summary>
    /// Summary description for DataCellTest
    /// </summary>
    [TestClass]
    public class DataCellTest
    {
        public DataCellTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestDataCell()
        {
            DataCell cell = new DataCell(16, null);
            ushort sourceValue = 0xF2A1;
            cell.Data = sourceValue;

            Assert.AreEqual<ushort>(sourceValue, cell.Data);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestDataCellWrongLengthBig()
        {
            DataCell cell = new DataCell(17, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestDataCellWrongLengthSmall()
        {
            DataCell cell = new DataCell(0, null);
        }

        [TestMethod]
        public void TestDataCellDefaultValue()
        {
            ushort sourceValue = 0xF2A1;
            DataCell cell = new DataCell(16, null, 0xF2A1);

            Assert.AreEqual<ushort>(sourceValue, cell.Data);
        }

        [TestMethod]
        public void TestDataCellLengthMask()
        {
            DataCell cell = new DataCell(15, null);
            ushort sourceValue = 0xF2A1;
            cell.Data = sourceValue;

            ushort expectedResult = (ushort)(sourceValue & ~0x8000);

            Assert.AreEqual<ushort>(expectedResult, cell.Data);
        }

        [TestMethod]
        public void TestDataCellSetBitFrom1To0()
        {
            ushort sourceValue = 0xF2A1;
            DataCell cell = new DataCell(16, null, sourceValue);
            
            cell[0] = false;

            Assert.AreEqual<ushort>(0xF2A0, cell.Data);
        }

        [TestMethod]
        public void TestDataCellSetBitFrom0To1()
        {
            ushort sourceValue = 0xF2A1;
            DataCell cell = new DataCell(16, null, sourceValue);

            cell[1] = true;

            Assert.AreEqual<ushort>(0xF2A3, cell.Data);
        }

        [TestMethod]
        public void TestDataCellSetBitToSameValue()
        {
            ushort sourceValue = 0xF2A1;
            DataCell cell = new DataCell(16, null, sourceValue);

            cell[0] = true;
            cell[1] = false;

            Assert.AreEqual<ushort>(0xF2A1, cell.Data);
        }

        [TestMethod]
        public void TestDataCellGetBit()
        {
            DataCell cell = new DataCell(16, null, 0xF2A1);

            Assert.AreEqual<bool>(true, cell[0]);
        }

        [TestMethod]
        public void TestDataCellSetMaskedBit()
        {
            DataCell cell = new DataCell(16, null, 0xF2A1, 0x0001);

            cell[0] = false;
            Assert.AreEqual<ushort>(0xF2A1, cell.Data);
        }

        [TestMethod]
        public void TestDataCellSetNotMaskedBit()
        {
            DataCell cell = new DataCell(16, null, 0xF2A1, 0x0001);

            cell[1] = true;
            Assert.AreEqual<ushort>(0xF2A3, cell.Data);
        }

        [TestMethod]
        public void TestDataCellReadonlyMask()
        {
            DataCell cell = new DataCell(16, null, 0, 0x0FF0);
            ushort sourceValue = 0xF2A1;
            cell.Data = sourceValue;

            ushort expectedResult = sourceValue;

            Assert.AreEqual<ushort>(0xF001, cell.Data);
        }
    }
}
