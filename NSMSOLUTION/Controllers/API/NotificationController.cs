using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Http;

namespace NSMSOLUTION.Controllers.API
{
    public class NotificationController : ApiController
    {
        [HttpGet]
        [Route("api/Notification/GetCustomerNearlyExpire")]
        public IHttpActionResult GET()
        {
            try
            {
                HTML html = new HTML();

                var para = new Dictionary<string, string>
                {
                    { "@SystemID", html.SystemID },
                    { "@WarehouseID", html.WarehouseID },
                    { "@DomainID", html.DomainID },
                    { "@Type", "ShowAll" },
                };

                DataSet ds = html.LoadData("SP_SEARCH_CUSTOMER_NEARLY_EXPIRRE", true, para);

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                {
                    return Ok(new { data = new List<dynamic>(), total = 0 });
                }

                var data = JsonConvert.DeserializeObject<List<dynamic>>(JsonConvert.SerializeObject(ds.Tables[0]));

                int total;
                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    total = Convert.ToInt32(ds.Tables[1].Rows[0]["TotalRecord"]);
                }
                else
                {
                    // fall back to counting the data rows themselves if no separate count table
                    total = ds.Tables[0].Rows.Count;
                }

                return Ok(new { data, total });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}