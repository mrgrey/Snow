using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cleancode.Snow;
using Cleancode.Snow.Misc;

namespace SnowTests
{
    /// <summary>
    /// Тестовый класс для Cleancode.Misc.Helper
    /// </summary>
    [TestClass]
    public class MiscHelperTest
    {
        public MiscHelperTest()
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
        public void TestHelperJoinNotEmptyStrings()
        {
            var strings = new string[]{"", "", "first", "second", "third", "", "", "fifth", "", ""};
            var result = Helper.JoinNotEmptyStrings(strings);

            Assert.AreEqual<string>("first, second, third, fifth", result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestHelperJoinNotEmptyStrinsWithNullParam()
        {
            var result = Helper.JoinNotEmptyStrings(null);
        }

        [TestMethod]
        public void TestHelperGetMaskByLength()
        {
            byte maskLength = 5;
            var result = Helper.GetMaskByLength(maskLength);

            Assert.AreEqual<ushort>(0x1F, result);
        }

        [TestMethod]
        public void TestHelperGetMaskByLength16Bits()
        {
            byte maskLength = 16;
            ushort result = Helper.GetMaskByLength(maskLength);

            Assert.AreEqual<ushort>(0xFFFF, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestHelperGetMaskByLength17Bits()
        {
            byte maskLength = 17;
            int result = Helper.GetMaskByLength(maskLength);            
        }

        [TestMethod]
        public void TestHelperParseHexNumberWithPrefix()
        {
            var hexNumber = "0x013fF";
            var result = Helper.ParseHexNumber(hexNumber);

            Assert.AreEqual<int>(0x013fF,result);
        }

        [TestMethod]
        public void TestHelperParseHexNumberWithoutPrefix()
        {
            var hexNumber = "26AF";
            var result = Helper.ParseHexNumber(hexNumber);

            Assert.AreEqual<int>(0x26AF, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestHelperParseHexNumberWrong()
        {
            var hexNumber = "0xaw26AF";
            var result = Helper.ParseHexNumber(hexNumber);
        }

        [TestMethod]
        public void TestHelperFormatHexInt()
        {
            uint hexNumber = 0x34abcd;
            var result = Helper.FormatHexInt(hexNumber, 8);

            Assert.AreEqual<string>("0034ABCD", result);
        }

        [TestMethod]
        public void TestHelperGetDumpByArray()
        {
            byte[] source = new byte[] { 0x1, 0x2, 0x3};
            byte[] result = Helper.GetDumpByArray(source);

            CollectionAssert.AreEqual(source, result);
        }
    }
}
