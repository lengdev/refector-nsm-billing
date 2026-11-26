using MVC;
using Newtonsoft.Json;
using NSMSOLUTION.Library;
using NSMSOLUTION.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utilities;
using static NSMSOLUTION.Library.Library;

namespace NSMSOLUTION.Controllers.Expenses
{
    public class TaxsController : Controller
    {
        private readonly HttpClient client = new HttpClient();
        private static List<SeleniumWebDriverSession> seleniumWebDriverSessions = new List<SeleniumWebDriverSession>();
        private string BaseAddress = "https://192.168.200.145:7174/";
        //"https://localhost:7174/";
        //"https://192.168.69.83:4434/";
        // GET: Taxs
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult UserLogin()
        {
            HTML html = new HTML
            {
                //if (html.HasPermission("Add Department"))
                AllowAdd = true,
                //if (html.HasPermission("Edit Department"))
                AllowEdit = true,
                //if (html.HasPermission("Delete Department"))
                AllowDelete = true,

                AddButton = "Add User Login",
                AddURL = "Taxs/CreateUser",
                TableName = "TblEfillingUserLogin",
                PrimaryField = "Id",
                OrderBy = "Id desc"
            };

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "User Name",
                SQL = "select Id, UserName from dbo.TblEfillingUserLogin where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by UserName",
                SelectValue = "Id",
                DisplayField = "UserName",
                RadioValue = EncryAndDecry.Encryp("Id"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "UserName", FieldName = "UserName" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate", Format="dd-MM-yyyy" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Action", FieldName = "Id", TextAlign = "Center",  IsModifyField = true }
            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public string CreateUser(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateUser",
                FieldIDFocus = "TxtUserName",
                TableName = "TblEfillingUserLogin",
                PrimaryField = "Id",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "User Name:",
                ID = "TxtUserName",
                DBField = "UserName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Password:",
                ID = "TxtPassword",
                DBField = "Password",
                Required = true,
            });

            return html.DrawHTMLForm();
        }

        public ActionResult ExportToEFilling()
        {
            HTML html = new HTML();
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "User Name:",
                ID = "TxtUserName",
                SQL_CBO = @"select Id, UserName  from dbo.TblEfillingUserLogin where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by UserName",
                DisplayText = "UserName",
                Value = "Id",
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();

            html = new HTML();
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Column Template:",
                ID = "TxtColumnTemplateName",
                SQL_CBO = @"SELECT 
    Id, 
    ColumnTemplateName,
    (
        SELECT *
        FROM TblEfillingColumnTemplate AS T2
        WHERE T2.Id = T1.Id
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
    ) AS AllValuesAsJson
FROM TblEfillingColumnTemplate AS T1 where T1.SystemID = " + html.SystemID + " and T1.WarehouseID = " + html.WarehouseID + " order by T1.ColumnTemplateName",
                DisplayText = "ColumnTemplateName",
                Value = "AllValuesAsJson",
            });
            ViewBag.ColTMP = html.DrawHTMLForm();

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> StartSeleniumAsync(string sessionId, int Id)
        {
            SqlResult sqlResult = new SqlResult();

            if (seleniumWebDriverSessions.Count > 0)
            {
                var infos = seleniumWebDriverSessions.Where(a => a.sessionId == sessionId).ToList();
                if (infos.Count > 0)
                {
                    sqlResult.Result = ResultType.Failed;
                    sqlResult.Message = "This User Already Login";
                    var infor = infos.Where(a => a.sessionId == sessionId).ToList().First();
                    return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, GridData = infor.MyCompanyInfo.ToList().ToJsonString() }, JsonRequestBehavior.AllowGet);
                }
            }

            var url = $"{BaseAddress}api/selenium/start";

            string session_Id = Guid.NewGuid().ToString(); // Generate a dynamic session ID

            var data = new
            {
                sessionId = session_Id,
                Id = Id
            };

            // Serialize the data to JSON
            var jsonData = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(data),
                Encoding.UTF8,
                "application/json"
            );
            string MyCompanyInfo = "";
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Ignore SSL certificate validation
                ServicePointManager.ServerCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;

                var response = await client.PostAsync(url, jsonData);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var infos = JsonConvert.DeserializeObject<SeleniumWebDriverSession>(responseString);

                seleniumWebDriverSessions.Add(infos);
                sqlResult.Result = ResultType.Successful;
                sqlResult.Message = responseString;
                var info = seleniumWebDriverSessions.Where(a => a.sessionId == infos.sessionId).ToList().First();
                MyCompanyInfo = info.MyCompanyInfo.ToList().ToJsonString();
            }
            catch (HttpRequestException e)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = e.Message;
            }

            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, sessionId = session_Id, GridData = MyCompanyInfo }, JsonRequestBehavior.AllowGet);
        }

        public string GridCompanyInfo(List<SeleniumWebDriverSession> infos, int Id)
        {
            if (infos.Count > 0)
            {
                var info = infos.Where(a => a.UserID == Id).ToList().First();
                HTML html = new HTML
                {
                    TblID = "tblComInfo"
                };
                html.GH.GridTable = new GridField[]
                {
                new GridField() { Header = "User Name", FieldName = "UserName" },
                new GridField() { Header = "Company Name", FieldName = "CompanyName" },
                new GridField() { Header = "Company Name", FieldName = "CompanyNameEN" },
                new GridField() { Header = "VAT", FieldName = "VatNo" },
                new GridField() { Header = "Tax Type", FieldName = "TaxType" },
                new GridField() { Header = "Location", FieldName = "Location" },
                new GridField() { Header = "Tax Branch", FieldName = "TaxBranchKH" },
                };
                KendoGridSetting kendoGridSetting = new KendoGridSetting
                {
                    KendoGridAutoFitColumn = false,
                    //KendoGridStickyColumn = false,
                    // groupable = false,
                    //filterable = false,
                    //columnMenu = false
                };
                return html.DrawKendoGrid(info.MyCompanyInfo.ToJsonString(), null, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            }
            return "No Record.";
        }

        [HttpPost]
        public async Task<ActionResult> GetMyCompanyInof(string sessionId)
        {
            SqlResult sqlResult = new SqlResult();

            if (seleniumWebDriverSessions.Count > 0)
            {

                var url = $"{BaseAddress}api/selenium/mycompanyinfo";

                var data = new
                {
                    sessionId = sessionId
                };

                // Serialize the data to JSON
                var jsonData = new StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );

                try
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    // Ignore SSL certificate validation
                    ServicePointManager.ServerCertificateValidationCallback =
                        (sender, certificate, chain, sslPolicyErrors) => true;
                    var response = await client.PostAsync(url, jsonData);
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                    var myCompanyInfo = JsonConvert.DeserializeObject<MyCompanyInfo>(responseString);

                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = responseString;

                    return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, myCompanyInfo = myCompanyInfo.ToJsonString() }, JsonRequestBehavior.AllowGet);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                    sqlResult.Result = ResultType.Failed;
                    sqlResult.Message = e.Message;
                }
            }
            else
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = "Please Login First.";
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> SearchNonTaxablePerson(string sessionId, string Keyword)
        {
            SqlResult sqlResult = new SqlResult();
            if (Keyword != null)
            {
                var url = $"{BaseAddress}api/selenium/nontaxablepersonlist";

                var data = new
                {
                    sessionId = sessionId,
                    KEYWORD = Keyword
                };

                // Serialize the data to JSON
                var jsonData = new StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(data),
                    Encoding.UTF8,
                    "application/json"
                );

                try
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    // Ignore SSL certificate validation
                    ServicePointManager.ServerCertificateValidationCallback =
                        (sender, certificate, chain, sslPolicyErrors) => true;
                    var response = await client.PostAsync(url, jsonData);
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                    var myCompanyInfo = JsonConvert.DeserializeObject<dynamic>(responseString);

                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = responseString;

                    return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, myCompanyInfo = myCompanyInfo }, JsonRequestBehavior.AllowGet);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Request error: {e.Message}");
                    sqlResult.Result = ResultType.Failed;
                    sqlResult.Message = e.Message;
                }
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ColumnTemplate()
        {
            HTML html = new HTML
            {
                //if (html.HasPermission("Add Department"))
                AllowAdd = true,
                //if (html.HasPermission("Edit Department"))
                AllowEdit = true,
                //if (html.HasPermission("Delete Department"))
                AllowDelete = true,

                AddButton = "Add Column Template",
                AddURL = "Taxs/SetColumnTemplate",
                TableName = "TblEfillingColumnTemplate",
                PrimaryField = "Id",
                OrderBy = "Id desc"
            };

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "User Name",
                SQL = "select Id, ColumnTemplateName from dbo.TblEfillingColumnTemplate where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by ColumnTemplateName",
                SelectValue = "Id",
                DisplayField = "ColumnTemplateName",
                RadioValue = EncryAndDecry.Encryp("Id"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Template Name", FieldName = "ColumnTemplateName" },
                new GridField() { Header = "កាលបរិច្ឆេទ", FieldName = "InvoiceDate" },
                new GridField() { Header = "លេខវិក្កយបត្រ ឬប្រតិវេទគយ", FieldName = "InvoiceNumber" },
                new GridField() { Header = "តម្លៃសរុបលើវិក្កយបត្រ (៛)", FieldName = "InvoiceAmount" },
                new GridField() { Header = "លេខអត្តសញ្ញាណប័ណ្ណ / TID / លិខិតឆ្លងដែន", FieldName = "TID" },
                new GridField() { Header = "ឈ្មោះ", FieldName = "Name" },
                new GridField() { Header = "លេខអត្តសញ្ញាណសារពើពន្ធ", FieldName = "VAT" },
                new GridField() { Header = "តម្លៃជាប់អាករ", FieldName = "VATPrice" },
                new GridField() { Header = "បរិយាយ", FieldName = "Descript" },
                new GridField() { Header = "Action", FieldName = "Id", TextAlign = "Center",  IsModifyField = true }
            };
            html.DialogBoxWidth = "800";
            html.DialogBoxHeight = 600;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        public string SetColumnTemplate(string PrimaryFieldValue)
        {
            string sql = @"SELECT * 
FROM (
    SELECT 'N/A' AS ID, 'N/A' AS ColName UNION
    SELECT 'S1' AS ID, 'S1' AS ColName UNION
    SELECT 'S2' AS ID, 'S2' AS ColName UNION
    SELECT 'S3' AS ID, 'S3' AS ColName UNION
    SELECT 'S4' AS ID, 'S4' AS ColName UNION
    SELECT 'S5' AS ID, 'S5' AS ColName UNION
    SELECT 'S6' AS ID, 'S6' AS ColName UNION
    SELECT 'S7' AS ID, 'S7' AS ColName UNION
    SELECT 'S8' AS ID, 'S8' AS ColName UNION
    SELECT 'S9' AS ID, 'S9' AS ColName UNION
    SELECT 'S10' AS ID, 'S10' AS ColName UNION
    SELECT 'S11' AS ID, 'S11' AS ColName UNION
    SELECT 'S12' AS ID, 'S12' AS ColName UNION
    SELECT 'S13' AS ID, 'S13' AS ColName UNION
    SELECT 'S14' AS ID, 'S14' AS ColName
) AS a
ORDER BY 
    CASE 
        WHEN a.ColName = 'N/A' THEN 0 
        ELSE CAST(SUBSTRING(a.ColName, 2, LEN(a.ColName)) AS INT) 
    END;
";
            HTML html = new HTML
            {
                TblAlign = "center",
                FormID = "SetColumnTemplate",
                FieldIDFocus = "TxtColumnTemplateName",
                TableName = "TblEfillingColumnTemplate",
                PrimaryField = "Id",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Template Name:",
                ID = "TxtColumnTemplateName",
                DBField = "ColumnTemplateName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "កាលបរិច្ឆេទ:",
                ID = "TxtDate",
                DBField = "InvoiceDate",
                SQL_CBO = sql,
                Value = "ID",
                DisplayText = "ColName",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "លេខវិក្កយបត្រ ឬប្រតិវេទគយ:",
                ID = "TxtInvoiceNumber",
                DBField = "InvoiceNumber",
                SQL_CBO = sql,
                Value = "ID",
                DisplayText = "ColName",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "ឈ្មោះ:",
                ID = "TxtName",
                DBField = "Name",
                SQL_CBO = sql,
                Required = true,
                Value = "ID",
                DisplayText = "ColName",
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "លេខអត្តសញ្ញាណសារពើពន្ធ:",
                ID = "TxtVAT",
                DBField = "VAT",
                SQL_CBO = sql,
                Required = true,
                Value = "ID",
                DisplayText = "ColName",
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "តម្លៃជាប់អាករ:",
                ID = "TxtVATPrice",
                DBField = "VATPrice",
                SQL_CBO = sql,
                Required = true,
                Value = "ID",
                DisplayText = "ColName",
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "បរិយាយ:",
                ID = "TxtDescript",
                DBField = "Descript",
                SQL_CBO = sql,
                Required = true,
                Value = "ID",
                DisplayText = "ColName",
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "តម្លៃសរុបលើវិក្កយបត្រ (៛):",
                ID = "TxtInvoiceAmount",
                DBField = "InvoiceAmount",
                SQL_CBO = sql,
                Required = true,
                Value = "ID",
                DisplayText = "ColName",
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "លេខអត្តសញ្ញាណប័ណ្ណ / TID / លិខិតឆ្លងដែន:",
                ID = "TxtTID",
                DBField = "TID",
                SQL_CBO = sql,
                Required = true,
                Value = "ID",
                DisplayText = "ColName",
            });


            html.SaveChangeAndClose = true;
            return html.DrawHTMLForm();
        }

        [HttpPost]
        public async Task<ActionResult> DoExport(List<InvoiceModel> invoices)
        {
            string EFillingNonTaxPayerStatus = "";
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            if (invoices != null)
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (HttpClient client = new HttpClient(handler))
                    {
                        // Serialize the invoice list to JSON
                        var jsonData = new StringContent(
                            Newtonsoft.Json.JsonConvert.SerializeObject(invoices),
                            Encoding.UTF8,
                            "application/json"
                        );

                        try
                        {
                            var url = $"{BaseAddress}api/selenium/insertbuy";
                            ServicePointManager.Expect100Continue = true;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            // Ignore SSL certificate validation
                            ServicePointManager.ServerCertificateValidationCallback =
                                (sender, certificate, chain, sslPolicyErrors) => true;
                            // Send the POST request
                            var response = await client.PostAsync(url, jsonData);

                            // Check if the response is successful
                            if (response.IsSuccessStatusCode)
                            {
                                try
                                {
                                    var responseString = await response.Content.ReadAsStringAsync();
                                    var resultInsertedInvoice = JsonConvert.DeserializeObject<ResultInsertedInvoice>(responseString);

                                    // Now parse the `Result` string as a List of ResultItem
                                    var resultList = JsonConvert.DeserializeObject<List<ResultItem>>(resultInsertedInvoice.Result);
                                    sqlResult.Result = ResultType.Successful;
                                    sqlResult.Message = responseString; // Assuming response contains a message
                                    string sql = @"insert into dbo.TblEFillingInsertedInvoiceBuying (Type, SheetName, MyComID, ComID, InvoiceDate, InvoiceNumber, InvoiceAmount, Name, VAT, 
TID, VATPrice, Descript, Status, CreateBy, CreateDate, SystemID, WarehouseID, DomainID) values (@Type, @SheetName, @MyComID, @ComID, @InvoiceDate, @InvoiceNumber, @InvoiceAmount, @Name, @VAT, 
@TID, @VATPrice, @Descript, @Status, @CreateBy, @CreateDate, @SystemID, @WarehouseID, @DomainID)";

                                    foreach (var rstInv in resultList)
                                    {
                                        var invoice = invoices.Where(a => a.INV_NO == rstInv.INV_NO && a.COM_ID == rstInv.MyComID).First();

                                        Dictionary<string, string> para = new Dictionary<string, string>
                                        {
                                            { "Type", invoice.TYPE },//1=ការទិញទំនិញ និងសេវា(ធាតុចូល)
                                            { "SheetName", invoice.SheetName },
                                            { "MyComID", rstInv.MyComID },
                                            { "ComID", rstInv.ComID },
                                            { "InvoiceDate", invoice.INV_DATE },
                                            { "InvoiceNumber", invoice.INV_NO },
                                            { "InvoiceAmount", invoice.TOTAL_AMT },
                                            { "Name", invoice.Name },
                                            { "VAT", invoice.VAT },
                                            { "TID", invoice.TID },
                                            { "VATPrice", invoice.VATPrice },
                                            { "Descript", invoice.INV_REMARK },
                                            { "Status", rstInv.Status },
                                            { "CreateBy", html.UserFullName },
                                            { "CreateDate", html.GetCurrentDate() },
                                            { "SystemID", html.SystemID },
                                            { "WarehouseID", html.WarehouseID },
                                            { "DomainID", html.DomainID },
                                        };
                                        sqlResult = html.SaveChangeData(sql, false, para);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    sqlResult.Result = ResultType.Failed;
                                    sqlResult.Message = $"Request error: {ex.Message}";
                                }
                            }
                            else
                            {
                                // Read the response content for the error
                                var jsonString = await response.Content.ReadAsStringAsync();

                                var filteredInvoices = GetFilteredInvoices(jsonString);
                                EFillingNonTaxPayerStatus = filteredInvoices.ToList().ToJsonString();
                                // Output the filtered results
                                foreach (var invoice in filteredInvoices)
                                {
                                    string sql = @"insert into dbo.TblEFillingNonTaxPayerStatus (Phone, InvoiceNo, Date, Message, Status, 
SystemID, WarehouseID, DomainID) values (@Phone, @InvoiceNo, @Date, @Message, @Status, @SystemID, @WarehouseID, @DomainID)";
                                    Dictionary<string, string> para = new Dictionary<string, string> {
                                        { "Phone",invoice.PHONE },
                                        { "InvoiceNo",invoice.NAME_KH },
                                        { "Date",html.GetCurrentDate() },
                                        { "Message", "Not Yet Have in TAX System." },
                                        { "Status", "Failed" },
                                        { "SystemID", html.SystemID },
                                        { "WarehouseID", html.WarehouseID },
                                        { "DomainID", html.DomainID },
                                    };
                                    sqlResult = html.SaveChangeData(sql, false, para);

                                    //Console.WriteLine($"Phone: {invoice.PHONE}, Name: {invoice.NAME_KH}");
                                }

                                sqlResult.Result = ResultType.Failed;
                                sqlResult.Message = EFillingNonTaxPayerStatus == ""
                                    ? $"Request failed with status code: {response.StatusCode}. Server response: {jsonString}"
                                    : "Please add Non Tax Person.";
                            }
                        }
                        catch (HttpRequestException e)
                        {
                            // Handle exceptions that occur during the request
                            sqlResult.Result = ResultType.Failed;
                            sqlResult.Message = $"Request error: {e.Message}";
                        }

                    }
                }
                ;
                // Return the result in JSON format
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, EFillingNonTaxPayerStatus = EFillingNonTaxPayerStatus }, JsonRequestBehavior.AllowGet);
        }
        // Define the Invoice and InvoiceResponse classes
        public class InvoiceResponse
        {
            public string Message { get; set; }
            public string Result { get; set; } // Change to string for direct deserialization
        }

        public class Invoice
        {
            public string PHONE { get; set; } = "";
            public string NAME_KH { get; set; } // Not nullable
        }

        // Example method to deserialize and filter invoices
        public List<Invoice> GetFilteredInvoices(string jsonResponse)
        {
            var invoiceResponse = JsonConvert.DeserializeObject<InvoiceResponse>(jsonResponse);

            // Deserialize the Result field which is a JSON string
            var invoices = JsonConvert.DeserializeObject<List<Invoice>>(invoiceResponse.Result);

            // Filter the invoices to include only non-null PHONE and NAME_KH
            var filteredInvoices = invoices
                .Where(invoice => !string.IsNullOrEmpty(invoice.PHONE) && !string.IsNullOrEmpty(invoice.NAME_KH))
                .ToList();

            return filteredInvoices;
        }
        public ActionResult AddNonTaxablePerson(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                BtnSaveChangeDialog = "btnSaveChangeNonTaxablePerson",
                FormID = "CreateNonTaxablePerson",
                FieldIDFocus = "TxtIdentificationNumber",
                TableName = "TblEFillingAddNonTaxablePerson",
                PrimaryField = "Id",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "លេខសម្គាល់​អត្តសញ្ញាណ:",
                ID = "TxtIdentificationNumber",
                DBField = "IdentificationNumber",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "ឈ្មោះជាភាសាខ្មែរ​ (សូមវាយបញ្ចូលជាអក្សរខ្មែរ):",
                ID = "TxtNameInKhmer",
                DBField = "NameInKhmer",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "ឈ្មោះជាឡាតាំង​:",
                ID = "TxtNameInLatin",
                DBField = "NameInLatin",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "លេខទូរស័ព្ទ:",
                ID = "TxtPhoneNumber",
                DBField = "PhoneNumber",
                Required = true,
            });
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SaveChagneAddNonTaxablePerson(dynamic Data)
        {
            var formData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Data);

            AddNonTaxablePerson person = new AddNonTaxablePerson
            {
                TIN = GetValue(formData, "TxtIdentificationNumber"),
                NAME_KH = GetValue(formData, "TxtNameInKhmer"),
                NAME_EN = GetValue(formData, "TxtNameInLatin"),
                PHONE = GetValue(formData, "TxtPhoneNumber"),
                sessionId = GetValue(formData, "sessionId")
            };

            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();

            var url = $"{BaseAddress}api/selenium/insertnontaxableperson";

            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                using (HttpClient client = new HttpClient(handler))
                {
                    // Serialize the invoice list to JSON
                    var jsonData = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(person),
                        Encoding.UTF8,
                        "application/json"
                    );

                    try
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        // Ignore SSL certificate validation
                        ServicePointManager.ServerCertificateValidationCallback =
                            (sender, certificate, chain, sslPolicyErrors) => true;
                        // Send the POST request
                        var response = await client.PostAsync(url, jsonData);

                        // Check if the response is successful
                        if (response.IsSuccessStatusCode)
                        {
                            try
                            {
                                var jsonResponse = await response.Content.ReadAsStringAsync();
                                // Deserialize the main JSON response
                                var apiResponse = JsonConvert.DeserializeObject<ResponseJson>(jsonResponse);

                                // Deserialize the result string into a Result object
                                var result = JsonConvert.DeserializeObject<ResponseResult>(apiResponse.Result);
                                sqlResult.Result = ResultType.Successful;
                                sqlResult.Message = result.RSLT_MSG; // Assuming response contains a message
                                string sql = @"insert into dbo.TblEFillingAddNonTaxablePerson (IdentificationNumber, NameInKhmer, NameInLatin
, PhoneNumber, Status, CreateBy, CreateDate, SystemID, WarehouseID, DomainID) values (@IdentificationNumber, @NameInKhmer, @NameInLatin
, @PhoneNumber, @Status, @CreateBy, @CreateDate, @SystemID, @WarehouseID, @DomainID)";
                                Dictionary<string, string> para = new Dictionary<string, string>
                                {
                                    { "IdentificationNumber", person.TIN  },
                                    { "NameInKhmer", person.NAME_KH  },
                                    { "NameInLatin", person.NAME_EN  },
                                    { "PhoneNumber", person.PHONE  },
                                    { "Status", result.RSLT_MSG },
                                    { "CreateBy", html.UserFullName },
                                    { "CreateDate", html.GetCurrentDate()  },
                                    { "SystemID", html.SystemID },
                                    { "WarehouseID", html.WarehouseID  },
                                    { "DomainID", html.DomainID  },
                                };
                                if (result.RSLT_MSG == "DUPLICATED TIN")
                                {
                                    sqlResult.Result = ResultType.Failed;
                                    sqlResult.Message = $"Request error: {result.RSLT_MSG}";
                                }
                                else
                                    sqlResult = html.SaveChangeData(sql, false, para);
                            }
                            catch (Exception ex)
                            {
                                sqlResult.Result = ResultType.Failed;
                                sqlResult.Message = $"Request error: {ex.Message}";
                            }
                        }
                        else
                        {
                            // Read the response content for the error
                            var responseContent = await response.Content.ReadAsStringAsync();
                            sqlResult.Result = ResultType.Failed;
                            sqlResult.Message = $"Request failed with status code: {response.StatusCode}. Server response: {responseContent}";
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        // Handle exceptions that occur during the request
                        sqlResult.Result = ResultType.Failed;
                        sqlResult.Message = $"Request error: {e.Message}";
                    }

                }
            }
            ;

            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        // Make sure this method matches the argument types
        private static string GetValue(Dictionary<string, string> fields, string fieldName)
        {
            return fields.TryGetValue(fieldName, out var value) ? value.Trim() : null;
        }

        public class FormField
        {
            public string name { get; set; }
            public string value { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> LogOut(string sessionId)
        {
            SqlResult sqlResult = new SqlResult();
            var url = $"{BaseAddress}api/selenium/quit";

            var data = new
            {
                sessionId = sessionId
            };

            // Serialize the data to JSON
            var jsonData = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(data),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Ignore SSL certificate validation
                ServicePointManager.ServerCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) => true;
                // Send the POST request
                var response = await client.PostAsync(url, jsonData);

                // Check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    var responseString = await response.Content.ReadAsStringAsync();
                    sqlResult.Result = ResultType.Successful;
                    sqlResult.Message = responseString;
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    sqlResult.Result = ResultType.Failed;
                    sqlResult.Message = $"Request failed with status code: {response.StatusCode}. Server response: {responseContent}";
                }
                return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = e.Message;
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ShowPurchasingGoodsandServices(int Type)
        {
            HTML html = new HTML();
            //html.AllowDelete=true;
            string sql = @"SELECT row_number() over(order by ei.Id desc) as [No],
ei.[Id]
      ,ei.[Type]
      ,ei.[SheetName]
      ,ei.[MyComID]
      ,ei.[ComID]
      ,ei.[InvoiceDate]
      ,ei.[InvoiceNumber]
      ,ei.[InvoiceAmount]
      ,ei.[Name]
      ,ei.[VAT]
      ,ei.[TID]
      ,ei.[VATPrice]
      ,ei.[Descript]
      ,ei.[Status]
      ,ei.[CreateBy]
      ,ei.[CreateDate]
      ,ei.[SystemID]
      ,ei.[WarehouseID]
      ,ei.[DomainID]
	  ,ci.CompanyName
  FROM [NSM_BILLING_DB].[dbo].[TblEFillingInsertedInvoiceBuying] ei
  inner join [dbo].[TblEFillingCompanyInfo] ci on ci.ComID = ei.MyComID where Type = " + Type + @"
and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID = " + html.DomainID + @"";

            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                ID = "txtDate_" + Type,
                FromDateID = "txtDate_" + Type,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "CreateDate"
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Invoice Number",
                SQL = "select top 10 InvoiceNumber from dbo.TblEFillingInsertedInvoiceBuying where lower(InvoiceNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by InvoiceNumber",
                RadioValue = EncryAndDecry.Encryp("InvoiceNumber"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
                ID = "txtInv_" + Type,
            });
            html.FormID = "frm_" + Type;
            html.BunSearch = "btnsearch_" + Type;
            html.TableName = "Tblshowefilling_" + Type;
            html.TblID = "Tblshowefilling_" + Type;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "SheetName", FieldName = "SheetName", IsGroupByField = true },
                new GridField() { Header = "Company Name", FieldName = "CompanyName", IsGroupByField = true },
                new GridField() { Header = "Invoice Date", FieldName = "InvoiceDate", FormatDateTime="dd-MM-yyyy" },
                new GridField() { Header = "Invoice Number", FieldName = "InvoiceNumber" },
                new GridField() { Header = "Invoice Amount", FieldName = "InvoiceAmount" },
                new GridField() { Header = "Name", FieldName = "Name" },
                new GridField() { Header = "VAT", FieldName = "VAT" },
                new GridField() { Header = "TID", FieldName = "TID", },
                new GridField() { Header = "Descript", FieldName = "Descript" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "Post By", FieldName = "CreateBy" },
                new GridField() { Header = "Post Date", FieldName = "CreateDate", FormatDateTime="dd-MM-yyyy" },
				//new GridField() { Header = "Action", FieldName = "Id", TextAlign = "Center",  IsModifyField = true, HideEdit= true, }
			};

            html.ShowPaging = false;
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                filterable = false,
            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "loaddata_" + Type, true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }

        [HttpGet]
        public ActionResult GetNonTaxablePerson()
        {
            HTML html = new HTML();
            string sql = "select IdentificationNumber, NameInKhmer, NameInLatin, PhoneNumber, Status, CreateBy, CreateDate from dbo.TblEFillingAddNonTaxablePerson where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            return Json(new { Data = html.JsonData(sql) }, JsonRequestBehavior.AllowGet);
        }

        // Define classes to match the JSON structure
        public class PurchasingGoodsandServicesResponse
        {
            public string Message { get; set; }
            public string Result { get; set; }
        }

        public class PurchasingGoodsandServicesDetail
        {
            public decimal IEPORTVAT_AMT { get; set; }
            public bool STATE { get; set; }
            public int TRANSACTION { get; set; }
            public bool VAT_ZERO { get; set; }
            public string TREASURY_INV_NO { get; set; }
            public decimal TOTAL_AMT { get; set; }
            public decimal NONE_VAT_AMT { get; set; }
            public bool NON_REFUNDABLE { get; set; }
            public string INV_REMARK { get; set; }
            public decimal LOCALVAT_AMT { get; set; }
            public int CATEGORY { get; set; }
            public long STCS_ID { get; set; }
            public string ID { get; set; }
            public decimal STATE_LOCALVAT_AMT { get; set; }
            public long AT_ID { get; set; }
            public long STCM_ID { get; set; }
            public decimal VAT_ZERO_AMT { get; set; }
            public decimal VAT_TREASURY { get; set; }
            public bool IS_ABC { get; set; }
            public decimal PPT_RATE { get; set; }
            public int VAT_TYPE { get; set; }
            public int DC_TYPE { get; set; }
            public decimal STCS_AMT { get; set; }
            public bool IS_PAID { get; set; }
            public string ITEM_ID { get; set; }
            public string INV_NO { get; set; }
            public int SECTOR_TYPE { get; set; }
            public decimal AT_AMT { get; set; }
            public string COM_ID { get; set; }
            public string INV_DATE { get; set; }
            public string FILING_COM_MST_ID { get; set; }
            public decimal STATE_IEPORTVAT_AMT { get; set; }
            public decimal PLT_AMT { get; set; }
            public bool DISABLED { get; set; }
            public string NAME_EN { get; set; }
            public bool DISABLE_EDIT { get; set; }
            public decimal EXVAT_AMT { get; set; }
            public string BANK_ACC { get; set; }
            public bool NONE_VAT { get; set; }
            public string TIN { get; set; }
            public long PLT_ID { get; set; }
            public decimal CREDIT_DEBIT_AMT { get; set; }
            public string REF_ID { get; set; }
            public int IS_TREASURY { get; set; }
            public decimal STCM_AMT { get; set; }
            public int TAXPAYER_TYPE { get; set; }
            public string NAME_KH { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> GetPurchasingGoodsandServicesAsync(string sessionId, int Type, string COM_ID, string MONTH, string YEAR)
        {
            ViewBag.Type = Type;
            ViewBag.Result = "";
            ViewBag.invoiceDetails = "";
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();

            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                using (HttpClient client = new HttpClient(handler))
                {
                    var data = new
                    {
                        sessionId = sessionId,
                        COM_ID = COM_ID,
                        MONTH = MONTH,
                        YEAR = YEAR,
                        TYPE = Type
                    };

                    // Serialize the data to JSON
                    var jsonData = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(data),
                        Encoding.UTF8,
                        "application/json"
                    );

                    try
                    {
                        var url = $"{BaseAddress}api/selenium/purchasesaleListingslist";
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        // Ignore SSL certificate validation
                        ServicePointManager.ServerCertificateValidationCallback =
                            (sender, certificate, chain, sslPolicyErrors) => true;
                        // Send the POST request
                        var response = await client.PostAsync(url, jsonData);

                        // Check if the response is successful
                        if (response.IsSuccessStatusCode)
                        {
                            try
                            {
                                var responseString = await response.Content.ReadAsStringAsync();
                                // Step 1: Deserialize the main JSON object
                                PurchasingGoodsandServicesResponse invoiceResponse = JsonConvert.DeserializeObject<PurchasingGoodsandServicesResponse>(responseString);

                                // Step 2: Deserialize the 'result' string into a list of InvoiceDetail
                                List<PurchasingGoodsandServicesDetail> invoiceDetails = JsonConvert.DeserializeObject<List<PurchasingGoodsandServicesDetail>>(invoiceResponse.Result);
                                ViewBag.invoiceDetails = invoiceDetails.ToList().ToJsonString();
                            }
                            catch (Exception ex)
                            {
                                sqlResult.Result = ResultType.Failed;
                                sqlResult.Message = $"Request error: {ex.Message}";
                            }
                        }
                        else
                        {
                            try
                            {
                                var responseString = await response.Content.ReadAsStringAsync();
                                sqlResult.Result = ResultType.Failed;
                                sqlResult.Message = responseString;
                                ViewBag.Result = responseString;
                            }
                            catch (Exception ex)
                            {
                                sqlResult.Result = ResultType.Failed;
                                sqlResult.Message = $"Request error: {ex.Message}";
                            }
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        // Handle exceptions that occur during the request
                        sqlResult.Result = ResultType.Failed;
                        sqlResult.Message = $"Request error: {e.Message}";
                    }

                }
            }
            ;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> DeletePurchasingGoodsandServicesAsync(string sessionId, string ID = "", string ids = "")
        {
            SqlResult sqlResult = new SqlResult();
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                using (HttpClient client = new HttpClient(handler))
                {
                    var data = new
                    {
                        sessionId = sessionId,
                        COM_ID = ID,
                        ids = ids
                    };

                    // Serialize the data to JSON
                    var jsonData = new StringContent(
                        Newtonsoft.Json.JsonConvert.SerializeObject(data),
                        Encoding.UTF8,
                        "application/json"
                    );

                    try
                    {
                        var url = $"{BaseAddress}api/selenium/deletepurchasesalelist";
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        // Ignore SSL certificate validation
                        ServicePointManager.ServerCertificateValidationCallback =
                            (sender, certificate, chain, sslPolicyErrors) => true;
                        // Send the POST request
                        var response = await client.PostAsync(url, jsonData);

                        // Check if the response is successful
                        if (response.IsSuccessStatusCode)
                        {
                            try
                            {
                                var responseString = await response.Content.ReadAsStringAsync();
                                sqlResult.Result = ResultType.Successful;
                                sqlResult.Message = responseString;
                            }
                            catch (Exception ex)
                            {
                                sqlResult.Result = ResultType.Failed;
                                sqlResult.Message = $"Request error: {ex.Message}";
                            }
                        }
                        else
                        {
                            try
                            {
                                var responseString = await response.Content.ReadAsStringAsync();
                                sqlResult.Result = ResultType.Failed;
                                sqlResult.Message = responseString;
                            }
                            catch (Exception ex)
                            {
                                sqlResult.Result = ResultType.Failed;
                                sqlResult.Message = $"Request error: {ex.Message}";
                            }
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        // Handle exceptions that occur during the request
                        sqlResult.Result = ResultType.Failed;
                        sqlResult.Message = $"Request error: {e.Message}";
                    }

                }
            }
            ;
            return Json(new { Data = sqlResult.Message, Result = sqlResult.Result }, JsonRequestBehavior.AllowGet);
        }
    }
}