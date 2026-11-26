using MVC;
using NewInventory.Controllers;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers
{
    [Authorize]
    public class PrintController : Controller
    {
        // GET Print
        public PrintController()
        {
        }

        public async Task<string> Index()
        {
            HTML html = new HTML();
            int PageSize = int.Parse(base.Request.Form["PageSize"]);
            int Page = int.Parse(base.Request.Form["Page"]);
            string Title = base.Request.Form["Title"];
            string GridData = base.Request.Form["GridData"];
            string ModifyField = base.Request.Form["ModifyField"];
            string FieldWithValue = base.Request.Form["FieldWithValue"];
            string EditTitle = base.Request.Form["EditTitle"];
            string EditURL = base.Request.Form["EditURL"];
            //string str = Request.Form["DialogBoxWidth"].ToString();
            string DialogBoxWidth = Request.Form["DialogBoxWidth"].ToString();// == "" ? 0 : int.Parse(Request.Form["DialogBoxWidth"]));
            int DialogBoxHeight = int.Parse(base.Request.Form["DialogBoxHeight"]);
            int.Parse(base.Request.Form["RowSeleted"]);
            int FooterSummaryDataTable = int.Parse(base.Request.Form["FooterSummaryDataTable"]);
            string FormID = base.Request.Form["FormID"];
            string LinkSummaryField = base.Request.Form["LinkSummaryField"];
            string LinkSummaryColumn = base.Request.Form["LinkSummaryColumn"];
            bool PrintHeader = bool.Parse(base.Request.Form["PrintHeader"]);
            bool PrintFooter = bool.Parse(base.Request.Form["PrintFooter"]);
            bool GridUseConnection2 = bool.Parse(base.Request.Form["GridUseConnection2"].ToString());
            bool HideOnPrintTotalFooter = bool.Parse(base.Request.Form["HideOnPrintTotalFooter"].ToString());
            bool ISPrintOut = bool.Parse(base.Request.Form["ISPrintOut"].ToString());
            bool ISSendEmail = bool.Parse(base.Request.Form["SendEmail"].ToString());
            bool DrawTotalAllHeader = bool.Parse(base.Request.Form["DrawTotalAllHeader"].ToString());
            int DrawTotalAllHeaderDataTableNumber = int.Parse(base.Request.Form["DrawTotalAllHeaderDataTableNumber"]);
            int SendEmailType = int.Parse(base.Request.Form["SendEmailType"]);
            string EmailSubject = base.Request.Form["EmailSubject"].ToString();
            string GridProcedureName = base.Request.Form["GridProcedureName"].ToString();
            string ReportTitle = base.Request.Form["ReportTitle"].ToString();

            string LoadGridFunction = "";
            bool DrawTotalHeaderTbl = false;
            int dataTableNumber = 0;
            string TableClassName = "table table-bordered table-head-fixed";
            string TblID = "";
            bool ShowPaging = false;
            bool NotShowWithNoRecord = false;
            string SQL = Request.Form["SQL"].ToString();
            string GridHeight = "";
            string OrderBy = "";
            string TableName = "";
            string PrimaryField = "";
            bool AllowAdd = false;
            bool AllowEdit = false;
            bool AllowDelete = false;
            bool IsUseHavingCondition = false;
            string UserID = Request.Form["UserID"].ToString();
            string SystemID = Request.Form["SystemID"].ToString();
            string WarehouseID = Request.Form["WarehouseID"].ToString();
            bool IsHaveWhereCondition = false;
            bool isfixedTableHeader = false;
            bool ShowPrintOnTopTable = bool.Parse(Request.Form["ShowPrintOnTopTable"].ToString());
            string rnd = Request.Form["rnd"].ToString();
            string Status = Request.Form["Status"].ToString();
            DoActionDataController d = new DoActionDataController();
            string Result = await d.LoadTableGrid(PageSize, Page, GridData, ModifyField, FieldWithValue, EditTitle, EditURL, DialogBoxWidth.ToString(), DialogBoxHeight, FormID, false, false, 0, 0, GridUseConnection2, PrintHeader, FooterSummaryDataTable, LinkSummaryField, LinkSummaryColumn, PrintFooter, HideOnPrintTotalFooter, ISPrintOut, DrawTotalAllHeader.ToString(), DrawTotalAllHeaderDataTableNumber.ToString(), GridProcedureName, LoadGridFunction, DrawTotalHeaderTbl, dataTableNumber, TableClassName, TblID, ShowPaging, NotShowWithNoRecord, SQL, GridHeight, OrderBy, TableName, PrimaryField, AllowAdd, AllowEdit, AllowDelete, IsUseHavingCondition, UserID, SystemID, WarehouseID, IsHaveWhereCondition, isfixedTableHeader, ShowPrintOnTopTable, rnd);

            if (Status == "ViewPDF" || Status == "Download")
            {
                string FilePath = HttpContext.Server.MapPath("~/Templates/ReportTemplate.html");
                StreamReader str = new StreamReader(FilePath);
                string Text = str.ReadToEnd();
                str.Close();
                Text = Text.Replace("{Title}", Title);
                //Text = Text.Replace("/", html.GetLocalIP());
                Text = Text.Replace("{ReportTitle}", ReportTitle);
                Result = Text.Replace("{BodyMessage}", Result);

                var data = await html.HTMLToPDF(Result, "Uploads/Reports", ReportTitle == "" ? Title : ReportTitle);
                Result = data.OutPutUrlFile;
            }

            if (ISSendEmail)
                Result = SendEmail(Result, EmailSubject, SendEmailType);
            return Result;
        }

        private string SendEmail(string body, string EmailSubject, int SendEmailType)
        {
            string Resutl = "";
            try
            {
                //using (var smtpClient = new SmtpClient())
                //{
                //    var mailAddressTo = new MailAddress("sothea8989@gmail.com");
                //    var mailAddressFrom = new MailAddress("sothea8989@hotmail.com");
                //    using (var mailMessage = new MailMessage(mailAddressFrom, mailAddressTo))
                //    {
                //        smtpClient.Host = "smtp.live.com";
                //        smtpClient.Port = 587;
                //        smtpClient.EnableSsl = true;
                //        smtpClient.Credentials = new NetworkCredential("sothea8989@hotmail.com", "Thea8989");
                //        mailMessage.Subject = "test";
                //        smtpClient.Send(mailMessage);
                //    }
                //}

                MailMessage objeto_mail = new MailMessage();
                SmtpClient client = new SmtpClient
                {
                    Port = 587,
                    Host = "smtp.live.com",
                    //client.Timeout = 90000;
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    //client.UseDefaultCredentials = false;
                    Credentials = new NetworkCredential("sothea8989@hotmail.com", "Thea8989")
                };
                string system = "Milk";//;ConfigurationManager.AppSettings["System"];
                objeto_mail.From = new MailAddress("sothea8989@hotmail.com", "Inventory System: " + system);

                string sqlemail = "select Name, EmailAddress from dbo.EmailAlert where EmailAlertTypeID = " + SendEmailType;
                HTML html = new HTML();
                DataSet dse = html.LoadData(sqlemail);
                string addresses = "";
                string names = "";
                if (dse.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dse.Tables[0].Rows)
                    {
                        if (addresses == "")
                        {
                            names = dr["Name"].ToString();
                            addresses = dr["EmailAddress"].ToString();
                        }
                        else
                        {
                            names += ";" + dr["Name"].ToString();
                            addresses += ";" + dr["EmailAddress"].ToString();
                        }
                    }
                    string[] name = names.Split(';');
                    int j = 0;
                    foreach (var address in addresses.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        objeto_mail.To.Add(new MailAddress(address, name[j].ToString()));
                        j++;
                    }
                    objeto_mail.IsBodyHtml = true;
                    //objeto_mail.SubjectEncoding = System.Text.Encoding.UTF8;
                    //objeto_mail.BodyEncoding = System.Text.Encoding.UTF8;
                    objeto_mail.Body = body;
                    objeto_mail.Subject = EmailSubject;
                    client.Send(objeto_mail);
                    Resutl = "Sent Successfully";
                }
            }
            catch (Exception ex)
            {
                Resutl = "Sent Faild. " + ex.Message;
            }
            return Resutl;
        }
        public async Task<ActionResult> PrintExcelFile()
        {
            if (base.Request.Form["PageSize"] != null)
            {
                int PageSize = int.Parse(base.Request.Form["PageSize"]);
                int Page = int.Parse(base.Request.Form["Page"]);
                string GridData = base.Request.Form["GridData"];
                string ModifyField = base.Request.Form["ModifyField"];
                string FieldWithValue = base.Request.Form["FieldWithValue"];
                string EditTitle = base.Request.Form["EditTitle"];
                string EditURL = base.Request.Form["EditURL"];
                string DialogBoxWidth = Request.Form["DialogBoxWidth"];
                int DialogBoxHeight = int.Parse(base.Request.Form["DialogBoxHeight"]);
                int.Parse(base.Request.Form["RowSeleted"]);
                string FormID = base.Request.Form["FormID"];
                bool GridUseConnection2 = bool.Parse(base.Request.Form["GridUseConnection2"].ToString());
                DoActionDataController d = new DoActionDataController();
                string Result = await d.LoadTableGrid(PageSize, Page, GridData, ModifyField, FieldWithValue, EditTitle, EditURL, DialogBoxWidth.ToString(), DialogBoxHeight, FormID, false, false, 0, 0, GridUseConnection2, false, 0, "", "", false, false, false);
                base.Session["TblData"] = Result;
            }
            return base.View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<string> ConvertToPDF(string HTMLData, string FileName, string Title = "", bool ReturnUrlImg = false, decimal Scale = 1)
        {
            HTML html = new HTML();
            string FilePath = HttpContext.Server.MapPath("~/Templates/HtmlTemplate.html");
            StreamReader str = new StreamReader(FilePath);
            string Text = str.ReadToEnd();
            str.Close();
            Text = Text.Replace("{Title}", Title == "" ? "Print Data" : Title);
            //Text = Text.Replace("/", html.GetLocalIP());
            HTMLData = Text.Replace("{BodyMessage}", HTMLData);
            string pageSize = Scale != 1 ? "A3" : "";
            var data = await html.HTMLToPDF(HTMLData, "Uploads/Reports", FileName, ReturnUrlImg, true, pageSize, Scale, true);
            if (!ReturnUrlImg)
            {
                //System.IO.File.WriteAllText(@"C:\Data\1.html", HTMLData);
                return data.OutPutUrlFile;
            }
            else
            {
                return Json(new { OutPutFile = data.OutPutFile, OutPutUrlFile = data.OutPutUrlFile, OutPutUrlImg = data.OutPutUrlImg }, JsonRequestBehavior.AllowGet).ToJsonString();
            }
        }
    }
}