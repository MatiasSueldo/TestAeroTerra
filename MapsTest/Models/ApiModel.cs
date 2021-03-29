using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Asa.MapApi.Models
{
    public class ApiModel
    {
        public List<Object> ListPOIS()
        {
            return null;
        }
        public List<Object> ListCategories()
        {
                return null;
        }
        public void InsertPOI(Dictionary<string, object> entity) { }        
        public void UpdatePOI(string id, Dictionary<string, object> entity) { }
        public void DeletePOI(string id) { }

    }
}