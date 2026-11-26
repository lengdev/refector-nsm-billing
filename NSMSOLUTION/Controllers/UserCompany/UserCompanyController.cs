using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.UserCompany
{
    public class UserCompanyController : Controller
    {
        // GET: UserCompany
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                DialogBoxWidth = "auto",
                DialogBoxHeight = 500,
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,

                ShowPaging = false,
                ShowGridOnly = true,
                ShowSearchButton = false,
                isShowLoadingGrid = false,
                UsingJqueryDataTable = true,

                TableName = "TblUserCompany",
                Caption = "Companies",
                AddButton = "New Company",
                AddURL = "UserCompany/CreateCompany",
                PrimaryField = "UserCompanyID",
                OrderBy = "UserCompanyID desc"
            };

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CompanyName",
                SQL = "select UserCompanyID, CompanyName from dbo.TblUserCompany where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CompanyName",
                SelectValue = "UserCompanyID",
                DisplayField = "CompanyName",
                RadioValue = EncryAndDecry.Encryp("UserCompanyID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Company Code", FieldName = "CompanyCode" },
                new GridField() { Header = "Company Name", FieldName = "CompanyName" },
                new GridField() { Header = "Company KH", FieldName = "CompanyKH" },
                new GridField() { Header = "Phone", FieldName = "Phone" },
                //new GridField() { Header = "Email", FieldName = "Email" },
                //new GridField() { Header = "Website", FieldName = "Website" },
                new GridField() { Header = "Contact Phone", FieldName = "ContactPhone" },
                //new GridField() { Header = "Contact Email", FieldName = "ContactEmail" },                
                new GridField() { Header = "Action", FieldName = "UserCompanyID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        public ActionResult CreateCompany(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                UseTable = true,
                TableColumn = 2,
                FormID = "CreateCompany",
                FieldIDFocus = "TxtCompanyCode",
                TableName = "TblUserCompany",
                PrimaryField = "UserCompanyID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "ForUser:",
                ID = "TxtForUser",
                DBField = "ForUser",
                Required = true,
                Value = "Vendor",
                Hide = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Company Code:",
                ID = "TxtCompanyCode",
                DBField = "CompanyCode",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID,
                //PrefixFor = "Supplier",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Company Name:",
                ID = "TxtCompanyName",
                DBField = "CompanyName",
                Required = true,
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Address:",
                ID = "TxtAddress",
                DBField = "Address",
                Required = true,
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Business Number:",
                ID = "TxtBusinessNumber",
                DBField = "BusinessNumber",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Business Type:",
                ID = "TxtBusinessType",
                DBField = "BusinessType",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "City:",
                ID = "TxtCity",
                DBField = "City",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "State Region:",
                ID = "TxtStateRegion",
                DBField = "StateRegion",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "ZIP/Postal Code:",
                ID = "TxtZIPPostalCode",
                DBField = "ZIPPostalCode",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Country:",
                ID = "TxtCountry",
                DBField = "Country",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Phone:",
                ID = "TxtPhone",
                DBField = "Phone",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Email:",
                ID = "TxtEmail",
                DBField = "Email",
                //ISFloatingLabel = true
            }); html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Website:",
                ID = "TxtWebsite",
                DBField = "Website",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Contact Person:",
                ID = "TxtContactPerson",
                DBField = "ContactPerson",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Contact Person Position:",
                ID = "TxtContactPersonPosition",
                DBField = "ContactPersonPosition",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "ContactPhone:",
                ID = "TxtContactPhone",
                DBField = "ContactPhone",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Contact Email:",
                ID = "TxtContactEmail",
                DBField = "ContactEmail",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Company KH:",
                ID = "TxtCompanyKH",
                DBField = "CompanyKH",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Company Desk Phone:",
                ID = "TxtCompanyDeskPhone",
                DBField = "CompanyDeskPhone",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "VAT IN Number:",
                ID = "TxtVAT",
                DBField = "VAT",
                //ISFloatingLabel = true
            });
            html.HTMLControls.Add(new HtmlFile()
            {
                Label = "Logo:",
                ID = "TxtLog",
                DBField = "Logo",
                //ISFloatingLabel = true
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        public JsonResult GetSystemCompanyInfo()
        {
            HTML html = new HTML();
            string sql = $@"select top 1 * from dbo.TblCompany 
where SystemID = {html.SystemID} and WarehouseID={html.WarehouseID} 
order by CompanyID desc";
            return Json(new { data = html.JsonData(sql, false) }, JsonRequestBehavior.AllowGet);
        }
    }
}