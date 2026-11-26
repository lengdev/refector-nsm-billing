using MVC;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TL;
using Utilities;

namespace NSMSOLUTION.Controllers.Telegrams
{
    //[Authorize]
    public class TelegramsController : Controller
    {
        // GET: Telegram
        static WTelegram.Client client;
        static string SessionPath = "";
        //public TelegramsController()
        //{
        //    HTML html = new HTML();
        //    SessionPath = html.httpContext.Server.MapPath("~/Uploads/TelegramSession");
        //    SessionPath = Path.Combine(SessionPath, "0965565118.session");
        //    client = new Client(Config);
        //}
        public async Task<ActionResult> Login()
        {
            try
            {
                HTML html = new HTML();
                SessionPath = html.httpContext.Server.MapPath("~/Uploads/TelegramSession");
                SessionPath = Path.Combine(SessionPath, "0965565118.session");
                FileInfo fileInfo = new FileInfo(SessionPath);
                if (!fileInfo.Exists)
                {
                    System.Diagnostics.Debug.WriteLine("File Not Found");
                }
                client = new WTelegram.Client(Config);
                System.Diagnostics.Debug.WriteLine("Starting LoginUserIfNeeded...");
                // Attempt to login
                var user = await client.LoginUserIfNeeded();

                // Log successful login
                System.Diagnostics.Debug.WriteLine($"Logged in as: {user.first_name} {user.last_name}");

                // Save user session
                Session["TelegramUser"] = user;

                return RedirectToAction("Welcome");
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Login failed: {ex.Message}");

                // Handle the error
                ViewBag.Error = ex.Message;
                return View("HandleLogin");
            }
        }


        [HttpPost]
        public async Task<ActionResult> HandleLogin(string input)
        {
            // Set the input received from the user as the next input for the login process
            // (e.g., phone code, password, etc.)
            Session["TelegramInput"] = input;

            // Retry login
            return await Login();
        }

        public ActionResult Welcome()
        {
            var user = Session["TelegramUser"] as User;
            return user == null ? RedirectToAction("Login") : (ActionResult)View(user);
        }

        // This method provides the necessary inputs when prompted by WTelegramClient
        static string Config(string what)
        {
            switch (what)
            {
                case "session_pathname": return SessionPath;// "D:\\NSM PROJECT\\NSM_ISP_BILLING\\NSMSOLUTION\\Uploads\\TelegramSession\\0965565118.session";
                case "api_id": return "27294688";
                case "api_hash": return "708a468fb6811978a5ad88088a1a31ea";
                case "phone_number": return "+85561588777";
                //case "verification_code": Console.Write("Code: "); return Console.ReadLine();
                //case "first_name": return "John";      // if sign-up is required
                //case "last_name": return "Doe";        // if sign-up is required
                case "password": return "Acc@Team@NSM";     // if user has enabled 2FA
                default: return null;                  // let WTelegramClient decide the default config
            }
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        client.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}


        [Authorize(Roles = "Add Message, Edit Message, Delete Message")]

        public ActionResult TelegramMessage()
        {
            HTML html = new HTML();
            //html.ReportTitle = "List Employee";
            if (html.HasPermission("Add Message"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Message"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Message"))
                html.AllowDelete = true;

            html.AddButton = "Add Message";
            html.AddURL = "Telegrams/AddTelegramMessage";
            html.TableName = "TblTelegramMessage";
            html.PrimaryField = "TelegramID";
            html.OrderBy = "TelegramID desc";
            html.DialogBoxWidth = "700";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Title",
                SQL = "select TelegramID, Title from dbo.TblTelegramMessage where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by Title",
                SelectValue = "TelegramID",
                DisplayField = "Title",
                RadioValue = EncryAndDecry.Encryp("TelegramID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Title", FieldName = "Title", Width="500px", ClassName="truncate-cell" },
                new GridField() { Header = "Message", FieldName = "Message", Width="500px", ClassName="truncate-cell" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "CreateDate", FieldName = "CreateDate", FormatDateTime = "dd-MM-yyyy" },
                new GridField() { Header = "Action", FieldName = "TelegramID", TextAlign = "Center",  IsModifyField = true }
            };

            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Message, Edit Message")]
        public ActionResult AddTelegramMessage(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "Title",
                FieldIDFocus = "TxtTitle",
                TableName = "TblTelegramMessage",
                PrimaryField = "TelegramID",
                PrimaryFieldValue = PrimaryFieldValue
            };

            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Title:",
                ID = "TxtTitle",
                DBField = "Title",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Message:",
                ID = "TxtMessage",
                DBField = "Message",
                Required = true,
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        [Authorize(Roles = "Send Message")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Employee"
            };
            if (html.HasPermission("Send Message"))
                html.AllowAdd = true;

            html.AddButton = "Send Message";
            html.AddURL = "Telegrams/SendMessage";
            html.OrderBy = "ct.DateSend desc";
            html.DialogBoxWidth = "800";

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "ct.DateSend"
            });

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 c.CustomerCode CID from dbo.TblCustomerTelegramMessage ct inner join dbo.TblCustomer c on c.CustomerID = ct.CustomerID where lower(c.CustomerCode) like @SearchText",
                //SelectValue = "LeadID",
                //DisplayField = "FullName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.SQL = @"select ROW_NUMBER() over(order by ct.DateSend desc) as [No], 
DateSend, SendBy, c.CustomerCode CID, c.CustomerName, CustomerPhone, MessageText, MessageStatus
from dbo.TblCustomerTelegramMessage ct
inner join dbo.TblCustomer c on c.CustomerID = ct.CustomerID";

            html.IsHaveWhereCondition = true;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "DateSend", FormatDateTime="dd-MM-yyyy" },
                new GridField() { Header = "Send By", FieldName = "SendBy" },
                new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Phone", FieldName = "CustomerPhone" },
                new GridField() { Header = "Message", FieldName = "MessageText", Width="200px", ClassName="truncate-cell" },
                new GridField() { Header = "Status", FieldName = "MessageStatus" },
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Send Message")]
        public ActionResult SendMessage()
        {
            HTML html = new HTML();
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Title:",
                ID = "TxtTitle",
                DBField = "Title",
                Required = true,
                SQL_CBO = @"select TelegramID, Title from dbo.TblTelegramMessage
where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "order by Title",
                DisplayText = "Title",
                Value = "TelegramID",
                SQLCallBack = @"select Title + '&#13;&#10;&#13;&#10;&#13;&#10;' + [Message] [Message] from dbo.TblTelegramMessage where TelegramID = @SearchText",
                DisplayOnFields = "TxtMessage",
                //CallBackToControlID= "TxtMessage",
                IsCallBack = true,
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Message:",
                ID = "TxtMessage",
                DBField = "Message",
                Required = true,
                Row = 5
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Block:",
                ID = "TxtCustomerType",
                DBField = "Block",
                Required = true,
                //SQL_CBO = "select LeadGroupID, GroupName from dbo.TblLeadsGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by GroupName",
                SQL_CBO = @"select distinct ct.CustomerTypeID, ct.CustomerType [Block]  from dbo.TblCustomerBillInformation cb
inner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayText = "Block",
                Value = "CustomerTypeID",
                Multiple = true
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "CID:",
                ID = "TxtCustomerCID",
                DBField = "CustomerCode",
                Required = true,
                SQL_CBO = @"select distinct CustomerID, CustomerCode from dbo.TblCustomer c
where isnull(c.CustomerCode, '') <> '' and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayText = "CustomerCode",
                Value = "CustomerID",
                Multiple = true
            });
            html.SaveChangeFunction = "SentMsg()";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        public JsonResult GetBlockDetail(string block, string customerId)
        {
            HTML html = new HTML();
            string sql = @"select * from
(select distinct cg.CustomerGroup, ct.CustomerType, c.CustomerCode, COALESCE(NULLIF(c.CustomerName, ''), c.CustomerNameLatin) AS CustomerName, c.CustomerMobile Phone, c.CustomerID from dbo.TblCustomerBillInformation cb
inner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblCustomerGroup cg on cg.CustomerGroupID = c.CustomerGroupID
inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID and ac.ConnectivityStatus=40
where c.SystemID = " + html.SystemID + " and c.WarehouseID =" + html.WarehouseID;

            // Block and CID are independent filters, combined with AND so a
            // selected block can be narrowed further by a typed-in CID (and
            // vice versa) rather than one silently overriding the other.
            if (!string.IsNullOrEmpty(block))
            {
                sql += @" and ct.CustomerTypeID in (" + block + ")";    
            }
            if (!string.IsNullOrEmpty(customerId))
            {
                sql += @" and c.CustomerID in (" + customerId + ")";
            }
            sql += @") as a order by a.CustomerGroup, a.CustomerType, a.CustomerName";

            List<BlockDetailRow> rows;
            try
            {
                using (var ctx = new DBContext())
                {
                    rows = ctx.Database.SqlQuery<BlockDetailRow>(sql).ToList();
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(rows, JsonRequestBehavior.AllowGet);
        }

        public class BlockDetailRow
        {
            public int No { get; set; }
            public string CustomerGroup { get; set; }
            public string CustomerType { get; set; }
            public string CustomerCode { get; set; }
            public string CustomerName { get; set; }
            public string Phone { get; set; }
            public int CustomerID { get; set; }
        }

        public async Task<ActionResult> CheckUserNumberIsTelegram(string phone)
        {
            string output = Regex.Replace(phone, @"\s+", "");
            if (output == "." || output == "#") output = "";
            SqlResult sqlresult = new SqlResult();
            if (output != "")
            {
                NotificationTelegram notificationTelegram = new NotificationTelegram();
                sqlresult = await notificationTelegram.CheckIsCustomerTelegram(phone);
            }

            return Json(new { Data = output, Result = sqlresult.Result, IsUsingTelegram = sqlresult.Value }, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> SendMsgTelegram(dynamic phone, string Msg)
        {
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            SessionPath = html.httpContext.Server.MapPath("~/Uploads/TelegramSession");
            SessionPath = Path.Combine(SessionPath, "85561588777_a9f2b5db4a474539a135d91cbd535626.session");
            try
            {
                using (client = new WTelegram.Client(Config))
                {
                    sqlresult.Result = ResultType.Successful;
                    var userinfo = await client.LoginUserIfNeeded();
                    //var contact = await client.Contacts_GetContacts();
                    //int contactCount = contact.users.Count;
                    NotificationTelegram notificationTelegram = new NotificationTelegram();

                    Msg = html.HtmlDecode(Msg);
                    dynamic DataInfos = new JavaScriptSerializer().DeserializeObject(phone[0].ToString());
                    foreach (dynamic dataInfo in DataInfos)
                    {
                        var phoneNumber = dataInfo["phone"];
                        var name = dataInfo["name"];
                        var cid = dataInfo["cid"];
                        var CustomerID = dataInfo["CustomerID"];
                        string output = Regex.Replace(phoneNumber, @"\s+", "");
                        if (output == "." || output == "#") output = "";
                        if (output != "")
                        {
                            phoneNumber = output;
                            var regex = new Regex(Regex.Escape("0"));
                            phoneNumber = regex.Replace(phoneNumber, "855", 1);
                            if (IsValidPhoneNumber(phoneNumber))
                            {
                                // Check if the phone number is connected to a Telegram account
                                //dynamic contacts;
                                //if (cid != "" || name != "")
                                var contacts = new Contacts_ImportedContacts();
                                contacts = cid != "" || name != ""
                                    ? await client.Contacts_ImportContacts(new[] { new InputPhoneContact { phone = phoneNumber, first_name = cid, last_name = name } })
                                    : await client.Contacts_ImportContacts(new[] { new InputPhoneContact { phone = phoneNumber } });
                                //else
                                //if (cid == "" || name == "")
                                //  contacts = await client.Contacts_ImportContacts(new[] { new InputPhoneContact { phone = phoneNumber } });
                                Thread.Sleep(1000);
                                if (contacts.users.Count > 0)
                                {
                                    //var contact = contacts.users.ToList();
                                    var user = contacts.users.Values.FirstOrDefault();
                                    //Console.WriteLine($"Phone number {phoneNumber} is connected to Telegram user {user.username} (id {user.id})");

                                    var inputPeer = new InputPeerUser(user.id, user.access_hash); //new TLInputPeerUser { UserId = int.Parse(user.ID.ToString()), AccessHash = user.access_hash };
                                                                                                  // Send a message to the user
                                    long randomId = new Random().Next();
                                    await client.Messages_SendMessage(inputPeer, Msg, randomId);

                                    string sql = @"insert into dbo.TblCustomerTelegramMessage (DateSend, SendBy, CustomerID, CustomerPhone, MessageText,MessageStatus)
values (@DateSend, @SendBy, @CustomerID, @CustomerPhone, @MessageText, @MessageStatus)";
                                    Dictionary<string, string> para = new Dictionary<string, string> {
                                        { "DateSend", html.GetCurrentDate() },
                                        { "SendBy", html.UserFullName },
                                        { "CustomerID", CustomerID },
                                        { "CustomerPhone",phoneNumber },
                                        { "MessageText",Msg },
                                        { "MessageStatus","Message sent successfully!" }
                                    };
                                    sqlresult = html.SaveChangeData(sql, false, para);
                                    Thread.Sleep(2000);
                                    sqlresult.Message = "Message sent successfully!";
                                    //Console.WriteLine("Message sent successfully!");
                                }
                                else
                                {
                                    //Console.WriteLine($"Phone number {phoneNumber} is not connected to any Telegram account.");
                                    string sql = @"insert into dbo.TblCustomerTelegramMessage (DateSend, SendBy, CustomerID, CustomerPhone, MessageText,MessageStatus)
values (@DateSend, @SendBy, @CustomerID, @CustomerPhone, @MessageText, @MessageStatus)";
                                    Dictionary<string, string> para = new Dictionary<string, string> {
                                        { "DateSend", html.GetCurrentDate() },
                                        { "SendBy", html.UserFullName },
                                        { "CustomerID", CustomerID },
                                        { "CustomerPhone",phoneNumber },
                                        { "MessageText",Msg },
                                        { "MessageStatus",$"Send Fail. Phone number {phoneNumber} is not connected to any Telegram account." }
                                    };
                                    html.SaveChangeData(sql, false, para);
                                }
                            }
                            else
                            {
                                string sql = @"insert into dbo.TblCustomerTelegramMessage (DateSend, SendBy, CustomerID, CustomerPhone, MessageText,MessageStatus)
values (@DateSend, @SendBy, @CustomerID, @CustomerPhone, @MessageText, @MessageStatus)";
                                Dictionary<string, string> para = new Dictionary<string, string> {
                                        { "DateSend", html.GetCurrentDate() },
                                        { "SendBy", html.UserFullName },
                                        { "CustomerID", CustomerID },
                                        { "CustomerPhone",phoneNumber },
                                        { "MessageText",Msg },
                                        { "MessageStatus",$"Send Fail. Phone number {phoneNumber} is not connected to any Telegram account." }
                                    };
                                html.SaveChangeData(sql, false, para);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sqlresult.Message = ex.Message;
                sqlresult.Result = ResultType.Failed;
                Console.WriteLine(ex.Message);
            }
            return Json(new { Data = "", Result = sqlresult.Result, IsUsingTelegram = sqlresult.Value, Message = sqlresult.Message }, JsonRequestBehavior.AllowGet);
        }
        public bool IsValidPhoneNumber(string phoneNumber)
        {
            // Regular expression to check if the phone number contains only digits
            string pattern = @"^\d+$";
            return Regex.IsMatch(phoneNumber, pattern);
        }
    }
}