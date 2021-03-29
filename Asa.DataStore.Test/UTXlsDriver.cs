using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Asa.DataStore;
using System.Diagnostics;
using System.Collections.Generic;

namespace Asa.DataStore.Test
{
    [TestClass]
    public class UTXlsDriver
    {
        const string XlsUri = @"C:\Users\Entrevista\Documents\Visual Studio 2015\Projects\MapsTest\MapsTest\Data\ds.xls";

        [TestMethod]
        public void Connect()
        {
            var driver = new XlsDriver();
            Assert.IsNotNull(driver);
            var connection = driver.Connect(XlsUri);
            connection.Open();
            Assert.AreEqual(connection.State, System.Data.ConnectionState.Open);
            connection.Close();
            Assert.AreEqual(connection.State, System.Data.ConnectionState.Closed);
        }

        [TestMethod]
        [ExpectedException(typeof(XlsDriver.XlsDriverException))]
        public void RunWithoutOpen()
        {
            var driver = new XlsDriver();
            Assert.IsNotNull(driver);
            var connection = driver.Connect(XlsUri);
            driver.ListTables(connection);
        }

        [TestMethod]
        public void ListTables()
        {
            var driver = new XlsDriver();
            
            using(var connection = driver.Connect(XlsUri)){
                connection.Open();
                var dt = driver.ListTables(connection);
                Assert.AreEqual(dt.Length, 3);
                connection.Close();
            }
            
        }

        [TestMethod]
        public void Select()
        {
            var driver = new XlsDriver();

            using (var connection = driver.Connect(XlsUri))
            {
                connection.Open();
                var dt = driver.Select(connection, "SELECT * FROM [Categories$]");
                Assert.IsNotNull(dt);
                Assert.IsTrue(dt.Rows.Count > 0);
                connection.Close();
            }
        }

        [TestMethod]
        public void ListData()
        {
            var driver = new XlsDriver();

            using (var connection = driver.Connect(XlsUri))
            {
                connection.Open();
                var dt = driver.ListData(connection, "POIs");
                Assert.IsNotNull(dt);
                Assert.IsTrue(dt.Rows.Count > 0);
                connection.Close();
            }
        }


        [TestMethod]
        public void InsertData()
        {
            var driver = new XlsDriver();

            using (var connection = driver.Connect(XlsUri))
            {
                connection.Open();
                driver.InsertData(connection, "Categories", new List<string> { "Id", "Value" }, new List<string> { "T", "Test"});
                
                connection.Close();
            }
        }
    }

    
}
