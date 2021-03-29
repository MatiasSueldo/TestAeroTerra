using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Asa.DataStore
{
    public class XlsDriver
    {
        public OleDbConnection Connect(string fileName)
        {
            return new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + "; Jet OLEDB:Engine Type=5;Extended Properties=\"Excel 8.0;\"");
            // return new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=Excel 12.0;");
        }


        public DataTable ListData(OleDbConnection conn, string sheetName)
        {            
            return this.Select(conn, "select * from [" + sheetName + "$]");            
        }

        public string[] ListTables(OleDbConnection conn)
        {
            if (conn.State != ConnectionState.Open) throw new XlsDriverException("The connection is not opened");
            DataTable tables = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            if(tables != null)
            {
                return tables.Select().Select((x) => x["TABLE_NAME"] as string).ToArray();
            }

            return new string[] { };
        }

        public DataTable Select(OleDbConnection conn, string sql)
        {
            if (conn.State != ConnectionState.Open) throw new XlsDriverException("The connection is not opened");
            DataTable sheetData = new DataTable();

            // retrieve the data using data adapter
            OleDbDataAdapter sheetAdapter = new OleDbDataAdapter(sql, conn);
            sheetAdapter.Fill(sheetData);


            return sheetData;
        }

        public void InsertData(OleDbConnection connection, string table, List<string> columnNames, List<string> theValues)
        {
            OleDbCommand command = null;
            
            string columns = "";
            string values = "";

            try
            {
                for (int index = 0; index < columnNames.Count; index++)
                {

                    columns += (index == 0) ? "[" + Regex.Replace(columnNames[index], @"\t|\n|\r", "\"") + "]" : ", [" + Regex.Replace(columnNames[index], @"\t|\n|\r", "\"") + "]";
                    values += (index == 0) ? "'" + Regex.Replace(theValues[index], @"\t|\n|\r", "\"") + "'" : ", '" + Regex.Replace(theValues[index], @"\t|\n|\r", "") + "'";

                }

                using (command = connection.CreateCommand())
                {

                    command.CommandText = string.Format("Insert into [{2}$] ({0}) values({1})", columns, values, table);

                    command.ExecuteNonQuery();


                }
            }
            catch (Exception ex)
            {
                throw new XlsDriverException("Error on insert.", ex);
            }
        }
        public void UpdateData(OleDbConnection connection, string table, IDictionary<string,object> entity)
        {
            OleDbCommand command = null;
            try
            {
                var entity2 = entity.ToDictionary(k => k.Key, k => ((string[])k.Value)[0].ToString());
                using (command = connection.CreateCommand())
                {

                    command.CommandText = string.Format("UPDATE [{0}$] SET  Name={2},Address={3},Phone={4},Category={5}, [X (LON)]= {6}, [Y (LAT)] ={7}   where Id='{1}'", table, entity2["Key"], entity2["Name"], entity2["Address"], entity2["Phone"], entity2["Category"], entity2["XLon"], entity2["YLat"]);

                    command.ExecuteNonQuery();


                }
            }
            catch
            {
                using (command = connection.CreateCommand())
                {

                    command.CommandText = string.Format("UPDATE [{0}$] SET  Name='{2}',Address='{3}',Phone='{4}',Category='{5}',[X (LON)] ='{6}',[Y (LAT)] = '{7}'   where Id='{1}'", table, entity["Id"], entity["Name"], entity["Address"], entity["Phone"], entity["Category"], entity["XLon"], entity["YLat"]);

                    command.ExecuteNonQuery();


                }
            }

            
            //this.Select(connection, "DELETE * FROM [" + table + "$] WHERE Id='" + Id + "$'");
        }
        //A pesar de realizarlo con Interop lo mantuve dentro del XLSDriver para mantener el orden y que sea mas facil de mantener
        public void DeleteData(string id)
        {
            Excel.Application excel = new Excel.Application();
            Excel.Workbook workbook = excel.Workbooks.Open(AppDomain.CurrentDomain.BaseDirectory + @"\bin\Data\ds.xls");
            Excel._Worksheet workbooksheet = workbook.Sheets[2];
            Excel.Range Rango = workbooksheet.UsedRange;

            foreach (Excel.Range s in Rango)
            {
                if (Convert.ToString(s.Value2) == id)
                {
                    s.EntireRow.Delete(Excel.XlDeleteShiftDirection.xlShiftUp);
                }
            }
            workbook.Save();
            workbook.Close(0);
            excel.Quit();
        }
        public class XlsDriverException : Exception
        {
            public XlsDriverException(string message) : base(message)
            {
            }
            public XlsDriverException(string message, Exception innerException) : base(message, innerException) 
            {         
            }
        }

    }
}
