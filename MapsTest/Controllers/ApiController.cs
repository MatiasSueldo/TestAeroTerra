using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Asa.DataStore;

namespace Asa.MapApi.Controllers
{
    public class ApiController : Controller
    {
        // GET: Api
        public ActionResult Index()
        {


            return View();
        }

        public ActionResult Categories()
        {
            // sheetName: Categorias
            //Corregí el error ortográfico de Residencial, ya que estaba escrita con C.
            switch (Request.HttpMethod)
            {
                case "GET":
                    return Json(new { categories = _ListCategories() }, JsonRequestBehavior.AllowGet);

            }

            Response.StatusCode = 400;
            return Json(new { error = "Method not suported." }, JsonRequestBehavior.AllowGet);
        }

        //Tuve que cambiar el tipo que recibía para validar el form ya que <string,object> automaticamente me transformaba el value del dictionary en un array.
        public ActionResult ValidateForm(string id, Dictionary<string, string> entity)
        {

            if (Request.HttpMethod == "POST")
            {
                List<Object> Validation = RunValidations(entity);
                // entity <-- inbound
                if ((bool)Validation[0])
                {
                    return Json(new { entity = entity, isValid = true, errors = new string[] { "0" } }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { entity = entity, isValid = false, errors = new string[] { Validation[1].ToString() } }, JsonRequestBehavior.AllowGet);
                }

            }

            Response.StatusCode = 400;
            return Json(new { error = "Method not suported." }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult POIs(string id, Dictionary<string, object> entity)
        {

            switch (Request.HttpMethod)
            {
                case "GET":
                    //Lo comento para ahorrar lineas de codigo innecesario, en caso de necesitarlo lo descomentaré
                    //var _POIs = new List<Dictionary<string, object>>();
                    //this._ListPOIS();

                    return Json(new { pois = _ListPOIS() }, JsonRequestBehavior.AllowGet);
                case "POST":
                    //Add funciona. Tuve que añadir una logica para corregir los nombres de las columnas on the fly(No se si se me permite cambiar el excel)
                    _AddPOI(entity);
                    Response.StatusCode = 200;
                    return Json(new { statusCode = 200 });
                    
                    break;
                case "PUT":
                    //UPDATE funciona correctamente. lo que no hice fue implementar la interfaz por temas de tiempo, probè el endpoint a traves de Postman.
                    _UpdatePOI(entity);
                    return Json(new { statusCode = 200 });
                    break;
                case "DELETE":
                    //Delete funciona correctamente tambien, no di a tiempo de realizar la interfaz. Se puede probar a traves del endpoint.
                    _DeletePOI(id);
                    break;
            }

            Response.StatusCode = 400;
            return Json(new { error = "Method not suported." }, JsonRequestBehavior.AllowGet);
        }

        //Como _ListPOIS y _ListCategories utilizan los mismos metodos, para no reescribir el codigo completamente yo lo reutilizaría.
        //Para respetar MVC comentaré la funcion ListPoiCat(Que recibiría una string con el nombre del sheet y podría hacer lo que hacen las 2 funciones juntas). 
        //private List<Object> _ListPoiCat(string sheetName)
        //{
        //    var driver = new XlsDriver();
        //    var conn = driver.Connect(AppDomain.CurrentDomain.BaseDirectory + @"\bin\Data\ds.xls");
        //    conn.Open();
        //    var dt = driver.ListData(conn, sheetName);

        //    var data = new List<Object>();
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        IDictionary<string, object> props = new Dictionary<string, object>();
        //        foreach (DataColumn col in dt.Columns)
        //        {
        //            props.Add(col.ColumnName.Replace(" ", "").Replace("(", "").Replace(")", ""), row[col.Ordinal]);
        //        }

        //        data.Add(props);
        //    }
        //    conn.Close();
        //    return data;

        //}

        private List<Object> _ListPOIS()
        {
            var driver = new XlsDriver();
            var conn = driver.Connect(AppDomain.CurrentDomain.BaseDirectory + @"\bin\Data\ds.xls");
            conn.Open();
            var dt = driver.ListData(conn, "POIs");


            var data = new List<Object>();
            foreach (DataRow row in dt.Rows)
            {
                IDictionary<string, object> props = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    props.Add(col.ColumnName.Replace(" ", "").Replace("(", "").Replace(")", ""), row[col.Ordinal]);
                }

                data.Add(props);
            }
            conn.Close();
            return data;
        }
        private List<Object> _ListCategories()
        {
            var driver = new XlsDriver();
            var conn = driver.Connect(AppDomain.CurrentDomain.BaseDirectory + @"\bin\Data\ds.xls");
            conn.Open();
            var dt = driver.ListData(conn, "Categories");

            var data = new List<Object>();
            foreach (DataRow row in dt.Rows)
            {
                IDictionary<string, object> props = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    props.Add(col.ColumnName.Replace(" ", "").Replace("(", "").Replace(")", ""), row[col.Ordinal]);
                }

                data.Add(props);
            }
            conn.Close();
            //Devuelve la lista de categorias ordenada alfabéticamente.
            return data.OrderBy(
                x => {
                    IDictionary<string, object> sarasa = (IDictionary<string, object>)x;
                    return sarasa["Value"];
                }).ToList();

        }
        private bool _AddPOI(Dictionary<string, object> entity)
        {
            var driver = new XlsDriver();
            var conn = driver.Connect(AppDomain.CurrentDomain.BaseDirectory + @"\bin\Data\ds.xls");
            List<string> columnnames = new List<string>();
            List<string> values = new List<string>();

            try
            {
                var entity2 = entity.ToDictionary(k => k.Key, k => ((string[])k.Value)[0].ToString());
                foreach (var item in entity2)
                {
                    if (item.Key == "XLon" || item.Key == "YLat")
                        columnnames.Add(item.Key.Insert(1, " (").Insert(6, ")"));
                    else
                        columnnames.Add(item.Key);
                    values.Add(item.Value.ToString());
                }
            }
            catch
            {
                foreach (var item in entity)
                {
                    if (item.Key == "XLon" || item.Key == "YLat")
                        columnnames.Add(item.Key.Insert(1, " (").Insert(6, ")"));
                    else
                        columnnames.Add(item.Key);
                    values.Add(item.Value.ToString());
                }
            }
            
            
            conn.Open();
           driver.InsertData(conn, "POIs",columnnames, values);

            conn.Close();
            return true;

        }

        private bool _UpdatePOI(Dictionary<string,object> entity)
        {
            var driver = new XlsDriver();
            var conn = driver.Connect(AppDomain.CurrentDomain.BaseDirectory + @"\bin\Data\ds.xls");
            try
            {

           
            
                    conn.Open();
                    driver.UpdateData(conn, "POIs", entity);
                    conn.Close();
                    return true;
            }
            
            catch
            {
                return false;
            }




        }
        private bool _DeletePOI(string Id)
        {
            var driver = new XlsDriver();
            try
            {
                driver.DeleteData(Id);
                return true;

            }
            catch
            {
                return false;
            }
        }

        private List<Object> RunValidations(Dictionary<string, string> entity)
        {
            List<Object> data = new List<object>();

            //primero reviso la logica de longitud en X
            if (double.Parse(entity["XLon"].ToString()) > -180 && double.Parse(entity["XLon"].ToString()) < 180)
            {
                //Segundo reviso la logica de latitud en Y
                if (double.Parse(entity["YLat"].ToString()) > -180 && double.Parse(entity["YLat"].ToString()) < 180)
                {
                    var T = _ListCategories();
                    //por ultimo Confirmo que la Categoría este dentro de las que recibimos desde nuestro Excel.
                    if (T.Where(x =>
                    {
                        IDictionary<string, object> c = (IDictionary<string, object>)x;
                        if (c["Id"].ToString() == entity["Category"].ToString())
                        {
                            return true;
                        }
                        else
                        {
                            return false;

                        }
                    }).Count() > 0)
                    {
                        data.Add(true);
                        return data;
                    }
                    else
                    {
                        data.Add(false);
                        data.Add("La categoría no esta contenida en el dropdown");
                        return data;
                    }
                }
                else
                {
                    data.Add(false);
                    data.Add("la Latitud Y es invalida");
                    return data;
                }

            }
            else
            {
                data.Add(false);
                data.Add("La Longitud X es invalida");
                return data;
            }

        }
    }
}


