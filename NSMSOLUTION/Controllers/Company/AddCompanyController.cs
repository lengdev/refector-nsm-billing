using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddCompanyController : Controller
    {
        // GET: AddCompany
        [Authorize(Roles = "Add Company, Edit Company, Delete Company")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Company",
                DialogBoxWidth = "Auto"
            };
            if (html.HasPermission("Add Company"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Company"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Company"))
                html.AllowDelete = true;

            html.AddButton = "Add Company";
            html.AddURL = "AddCompany/CreateCompany";
            html.TableName = "TblCompany";
            html.PrimaryField = "CompanyID";
            //html.AddURL = "AddCompany/CreateUploadQr";  
            //html.AddURL = "Upload/CreateUploadQr";
            html.ShowTblTitle = true;
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Company Name",
                SQL = "select CompanyID, CompanyName from dbo.TblCompany where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CompanyName",
                SelectValue = "CompanyID",
                DisplayField = "CompanyName",
                RadioValue = EncryAndDecry.Encryp("CompanyID"),
                DefaultCheckBok = true
            };

            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            string sql = @"select row_number() Over(order by CompanyID desc) as [No], * from dbo.TblCompany
where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "";

            var ds = html.LoadData(sql);
            string json = JsonConvert.SerializeObject(ds.Tables[0]);
            ViewBag.CompanyJson = json;
            //ViewBag.PrimaryField = ds.Tables[0].Rows[0]["CompanyID"];

            return View();
        }
        //[Authorize(Roles = "Add Company")]
        //public ActionResult CreateUploadQr()
        //{

        //    HTML html = new HTML();
        //    html.UseTable = true;

        //    //html.ReportTitle = "List Company";
        //    html.FormID = "CreateQrPayment";
        //    //html.FieldIDFocus = "TxtInvoiceTemplateName";
        //    html.TableName = "TblQrPayment";
        //    html.PrimaryField = "QrPayment";
        //    //html.PrimaryFieldValue = PrimaryFieldValue;
        //    html.HTMLControls.Add(new HtmlTextBox()
        //    {
        //        Label = "Company Phone:",
        //        ID = "TxtQrName",
        //        DBField = "QrName"
        //    });

        //    ViewBag.DrawHTMLForm = html.DrawHTMLForm();
        //    return View("CreateQrPayment");
        //}
        [Authorize(Roles = "Add Company, Edit Company")]
        public ActionResult CreateCompany(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                UseTable = true,
                TableColumn = 2,
                FormID = "CreateCompany",
                FieldIDFocus = "TxtInvoiceTemplateName",
                TableName = "TblCompany",
                PrimaryField = "CompanyID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Invoice Template Name:",
                ID = "TxtInvoiceTemplateName",
                DBField = "InvoiceTemplateName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Invoice Tamplate Name:",
            //    ID = "CboInvoiecTamplate",
            //    DBField = "InvoiceTmpID",
            //    SQL_CBO = "select InvoiceTmpID, InvoiceTmpName from dbo.TblInvoiceTmp where SystemID ="+ html.SystemID + " and WarehouseID = "+ html.WarehouseID,
            //    Required = true,
            //    DisplayText= "InvoiceTmpName",
            //    Value= "InvoiceTmpID"
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Company Name:",
                ID = "TxtCompanyName",
                DBField = "CompanyName",
                Required = true,
                //ISUniqueFiled = true,
                //CheckExsitingWithField = "WarehouseID",
                //CheckExsitingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Company Name KH:",
                ID = "TxtCompanyNameKH",
                DBField = "CompanyNameKH"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Company Address:",
                ID = "TxtCompanyAddress",
                DBField = "CompanyAddress",
                Required = true
            });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Company Address KH:",
                ID = "TxtAddressKH",
                DBField = "AddressKH",
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Commune:",
            //    ID = "TxtCommune",
            //    DBField = "Commune",
            //    SQL="select Commune from dbo.TblCommune where SystemID ="+html.SystemID+" and WarehouseID = " + html.WarehouseID + " order by Commune",
            //    Required = true,
            //    isTypeahead = true
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "District:",
            //    ID = "TxtDistrict",
            //    DBField = "District",
            //    Required = true,
            //    isTypeahead = true,
            //    SQL= "select District from dbo.TblDistrict where SystemID =" + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by District"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Province/City:",
            //    ID = "TxtProvinceCity",
            //    DBField = "ProvinceCity",
            //    Required = true,
            //    isTypeahead = true,
            //    SQL= "select ProvinceCity from dbo.TblProvinceCity where SystemID =" + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by ProvinceCity"
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Company Phone:",
                ID = "TxtTelephone",
                DBField = "Phone"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Phone KH:",
                ID = "TxtMobile",
                DBField = "PhoneKH"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Customer Service PhonNo:",
                ID = "TxtCustomerServicePhonNo",
                DBField = "CustomerServicePhonNo"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Accounting Phon No:",
                ID = "TxtAccountingPhonNo",
                DBField = "AccountingPhonNo"
            });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Email:",
                ID = "TxtEmail",
                DBField = "Email",
                Type = "email"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Website:",
                ID = "TxtWebsite",
                DBField = "Website"
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Facebook:",
            //    ID = "TxtFacebookLink",
            //    DBField = "Facebook",
            //    Type="url"
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "VAT Number:",
                ID = "TxtVATNumber",
                DBField = "VATNumber"
            });

            //html.HTMLControls.Add(new HtmlCheckBox()
            //{
            //    ISSwitchCheck = true,
            //    Label = "Use For Invoice With VAT:",
            //    ID = "Txtuserforwithvat",
            //    DBField = "UseWithInvoiceVAT",
            //    //Type="checkbox"
            //});            
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Term & Condition VAT:",
                ID = "TxtTermCondition",
                DBField = "TermConditionVAT"
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Term & Condition Non VAT:",
                ID = "TxtTermConditionNonVAT",
                DBField = "TermConditionNonVAT"
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Quote Term & Condition VAT:",
                ID = "QuoteTxtTermConditionVAT",
                DBField = "QuoteTermConditionVAT"
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Quote Term & Condition Non VAT:",
                ID = "QuoteTxtTermConditionNonVAT",
                DBField = "QuoteTermConditionNonVAT"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Prefix VAT:",
                ID = "PrefixVAT",
                DBField = "PrefixVAT",
                ExLabel = "Ex: Any Prefix {yyyy} = Year, {dd} = Day, {MM} = Month, {000000} = 000001"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Prefix Non VAT:",
                ID = "PrefixNonVAT",
                DBField = "PrefixNonVAT",
                ExLabel = "Ex: Any Prefix {yyyy} = Year, {dd} = Day, {MM} = Month, {000000} = 000001"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Prefix Proforma Invoice VAT:",
                ID = "PrefixProformaInvoiceVAT",
                DBField = "PrefixProformaInvoiceVAT",
                ExLabel = "Ex: Any Prefix {yyyy} = Year, {dd} = Day, {MM} = Month, {000000} = 000001"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Prefix Proforma Invoice Non VAT:",
                ID = "PrefixProformaInvoiceNonVAT",
                DBField = "PrefixProformaInvoiceNonVAT",
                ExLabel = "Ex: Any Prefix {yyyy} = Year, {dd} = Day, {MM} = Month, {000000} = 000001"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Prefix Quotation Invoice VAT:",
                ID = "PrefixQuotationInvoiceVAT",
                DBField = "PrefixQuotationInvoiceVAT",
                ExLabel = "Ex: Any Prefix {yyyy} = Year, {dd} = Day, {MM} = Month, {000000} = 000001"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Prefix Quotation Invoice Non VAT:",
                ID = "PrefixQuotationInvoiceNonVAT",
                DBField = "PrefixQuotationInvoiceNonVAT",
                ExLabel = "Ex: Any Prefix {yyyy} = Year, {dd} = Day, {MM} = Month, {000000} = 000001"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Prefix Receipt VAT:",
                ID = "PrefixReceiptVAT",
                DBField = "PrefixReceiptVAT",
                ExLabel = "Ex: Any Prefix {yyyy} = Year, {dd} = Day, {MM} = Month, {000000} = 000001"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Prefix Receipt Non VAT:",
                ID = "PrefixReceiptNonVAT",
                DBField = "PrefixReceiptNonVAT",
                ExLabel = "Ex: Any Prefix {yyyy} = Year, {dd} = Day, {MM} = Month, {000000} = 000001"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Prefix Credit Note:",
                ID = "PrefixCreditNote",
                DBField = "PrefixCreditNote",
                ExLabel = "Ex: Any Prefix {yyyy} = Year, {dd} = Day, {MM} = Month, {000000} = 000001"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Order Number:",
                ID = "TxtOrderNumber",
                DBField = "OrderNumber",
                Required = true,
                Value = "1",
                ISNumeric = true,
                ISInteger = true
            });
            html.HTMLControls.Add(new HtmlCheckBox()
            {
                ISSwitchCheck = true,
                Label = "IS Default:",
                ID = "TxtISDefault",
                DBField = "ISDefault",
                //Type = "checkbox"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Prefix ItemRequest No:",
                ID = "PrefixItemRequestNo",
                DBField = "PrefixItemRequestNo",
                ExLabel = "Ex: Any Prefix {yyyy} = Year, {dd} = Day, {MM} = Month, {000000} = 000001"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Prefix Request Approval Item Out:",
                ID = "PrefixRequestApprovalItemOut",
                DBField = "PrefixRequestApprovalItemOut",
                ExLabel = "Ex: Any Prefix {yyyy} = Year, {dd} = Day, {MM} = Month, {000000} = 000001"
            });
            html.HTMLControls.Add(new HtmlFile()
            {
                Label = "Logo:",
                ID = "Logo",
                DBField = "Logo",
                DestinationFile = "CompanyLogo",
                AcceptImageOnly = true,
                ISFile = true
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        [Authorize(Roles = "Add Invoice Template, Edit Invoice Template, Delete Invoice Template")]
        public ActionResult InvoiceTemplate()
        {
            HTML html = new HTML();
            string sql = @"select row_number() over(order by it.InvoiceTmpID desc) as [No], it.InvoiceTmpID, 
it.InvoiceTemplateName, c.CompanyName, it.TypeOfInvoice, it.Type
from [dbo].[TblInvoiceTemplate] it
inner join dbo.TblCompany c on c.CompanyID = it.CompanyID where it.SystemID = " + html.SystemID + " and it.WarehouseID = " + html.WarehouseID;

            html.GH.SQL = sql;
            html.ReportTitle = "List Invoice Template";
            html.DialogBoxWidth = "750";
            html.DialogBoxHeight = 400;
            if (html.HasPermission("Add Invoice Template"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Invoice Template"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Invoice Template"))
                html.AllowDelete = true;

            html.AddButton = "Add Invoice Template";
            html.AddURL = "AddCompany/AddInvoiceTemplate";
            html.TableName = "TblInvoiceTemplate";
            html.PrimaryField = "InvoiceTmpID";
            html.OrderBy = "InvoiceTmpID desc";
            html.ShowTblTitle = true;
            html.IsHaveWhereCondition = true;
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Template Name",
                SQL = @"select it.InvoiceTmpID, it.InvoiceTemplateName
from dbo.TblInvoiceTemplate it inner join dbo.TblCompany c on c.CompanyID = it.CompanyID 
where it.SystemID = " + html.SystemID + " and it.WarehouseID = " + html.WarehouseID + " order by InvoiceTemplateName",
                SelectValue = "InvoiceTmpID",
                DisplayField = "InvoiceTemplateName",
                RadioValue = EncryAndDecry.Encryp("it.InvoiceTmpID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Invoice Template Name", FieldName = "InvoiceTemplateName" },
                new GridField() { Header = "Type Of Invoice", FieldName = "TypeOfInvoice" },
                new GridField() { Header = "Type", FieldName = "Type" },
                new GridField() { Header = "Company Name", FieldName = "CompanyName", Style="white-space: nowrap; text-overflow:ellipsis; overflow: hidden; max-width:300px;" },
                new GridField() { Header = "Action", FieldName = "InvoiceTmpID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [HttpPost]
        public JsonResult ActiveQrBank()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();

            string qrID = Request["QrID"];
            string newStatus = Request["Status"]; // 1 for active, 0 for inactive

            string sql = @"
            UPDATE TblQrPayment 
            SET Status = @Status
            WHERE QrID = @QrID;

            SELECT QrID, CompanyID, QrName, QrImagePath, Status
            FROM TblQrPayment 
            WHERE QrID = @QrID;
        ";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "QrID", qrID },
                { "Status", newStatus }
            };

            sqlResult = html.SaveChangeData(sql, false, para);

            // Simple response - just success/failure
            return Json(new
            {
                success = sqlResult.Result == ResultType.Successful,
                message = sqlResult.Message
            });
        }
        [HttpGet]

        public JsonResult GetQrBanksByCompany(string companyId)
        {
            try
            {
                HTML html = new HTML();

                string sql = @"SELECT QrID, CompanyID, QrName, QrImagePath, Status
           FROM TblQrPayment 
           WHERE CompanyID = " + companyId + @"
           AND CompanyID IN (
               SELECT CompanyID FROM TblCompany 
               WHERE SystemID = " + html.SystemID + @" 
               AND WarehouseID = " + html.WarehouseID + @"
           )
           ORDER BY QrID";

                var ds = html.LoadData(sql);
                var qrBanks = new List<object>();

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (System.Data.DataRow row in ds.Tables[0].Rows)
                    {
                        qrBanks.Add(new
                        {
                            QrID = row["QrID"],
                            CompanyID = row["CompanyID"],
                            QrName = row["QrName"],
                            QrImagePath = row["QrImagePath"],
                            Status = row["Status"]
                        });
                    }
                }

                return Json(qrBanks, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SaveQrCompany()
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();

            // 1️⃣ Upload file (same pattern as profile picture)

            string uploadFolder = "Uploads/Qrbank";
            string fullPath = Server.MapPath("~/" + uploadFolder);


            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        Result = ResultType.Failed,
                        Message = "Failed to create upload directory: " + ex.Message
                    });
                }
            }

            sqlResult = html.UploadFile(uploadFolder);

            // Upload file
            string qrImagePath;
            if (sqlResult.Result == ResultType.Successful)
                qrImagePath = sqlResult.Value;
            else
                return Json(new { Result = sqlResult.Result, Message = sqlResult.Message });

            // 2️⃣ Build SQL with parameters
            string sql = @"
                            INSERT INTO TblQrPayment
                            (CompanyID, QrName, QrImagePath, Status)
                            VALUES
                            (@CompanyID, @QrName, @QrImagePath, @Status)
                        ";

            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "CompanyID", Request["CompanyID"] },
                { "QrName", Request["QrName"] },
                { "QrImagePath", qrImagePath },
                { "Status", Request["Status"] }
            };

            // 3️⃣ Execute insert
            sqlResult = html.SaveChangeData(sql, false, para);

            return Json(new
            {
                success = sqlResult.Result == ResultType.Successful,
                message = sqlResult.Message
            });
        }

        [HttpGet]
        public JsonResult GetCompanies()
        {
            try
            {
                HTML html = new HTML();

                string sql = @"SELECT CompanyID, CompanyName 
                       FROM TblCompany 
                       WHERE SystemID = " + html.SystemID + @" 
                       AND WarehouseID = " + html.WarehouseID + @"
                       ORDER BY CompanyName";

                var ds = html.LoadData(sql);
                var companies = new List<object>();

                foreach (System.Data.DataRow row in ds.Tables[0].Rows)
                {
                    companies.Add(new
                    {
                        CompanyID = row["CompanyID"],
                        CompanyName = row["CompanyName"]
                    });
                }

                return Json(companies, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize(Roles = "Add Invoice Template, Edit Invoice Template")]
        public ActionResult AddInvoiceTemplate(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreateCompany",
                FieldIDFocus = "TxtInvoiceTemplateName",
                TableName = "TblInvoiceTemplate",
                PrimaryField = "InvoiceTmpID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Invoice Template Name:",
                ID = "TxtInvoiceTemplateName",
                DBField = "InvoiceTemplateName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID,
            });
            string sql = @"select 'None VAT' TypeOfInvoice union select 'VAT' TypeOfInvoice union select 'CM' TypeOfInvoice";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Type Of Invoice:",
                ID = "TxtTypeOfInvoice",
                DBField = "TypeOfInvoice",
                Required = true,
                SQL_CBO = sql,
                DisplayText = "TypeOfInvoice",
                Value = "TypeOfInvoice"
            });

            sql = @"select 'Official Invoice' [Type] union select 'Sale Order Invoice' [Type] union select 'Quotation Invoice' [Type]";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Type:",
                ID = "TxtType",
                DBField = "Type",
                Required = true,
                SQL_CBO = sql,
                DisplayText = "Type",
                Value = "Type"
            });

            sql = "select CompanyID, CompanyName from dbo.TblCompany where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and ISDefault = 1 order by CompanyName";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Company Name:",
                ID = "TxtCompanyName",
                DBField = "CompanyID",
                Required = true,
                SQL_CBO = sql,
                DisplayText = "CompanyName",
                Value = "CompanyID"
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}