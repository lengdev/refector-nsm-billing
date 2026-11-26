using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Leads
{
    public class LeadsController : Controller
    {
        // GET: Leads
        [Authorize(Roles = "Add Employee, Edit Employee, Delete Employee")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Employee",
                DialogBoxWidth = "800"
            };
            if (html.HasPermission("Add Employee"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Employee"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Employee"))
                html.AllowDelete = true;

            html.AddButton = "Add Employee";
            html.AddURL = "Leads/CreateLeads";
            html.TableName = "TblLeads";
            html.PrimaryField = "LeadID";
            html.OrderBy = "LeadID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Name",
                SQL = "select LeadID, FullName from dbo.TblLeads where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by FullName",
                SelectValue = "LeadID",
                DisplayField = "FullName",
                RadioValue = EncryAndDecry.Encryp("l.LeadID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.SQL = @"select ROW_NUMBER() OVER(ORDER BY l.LeadID desc) as [No], l.Salutation,l.LeadCode, l.Status,
            l.FullName, l.Position, l.Gender, l.Phone, l.Email, l.[Address], l.Memo, lg.DepartmentName, l.LeadID
            from dbo.TblLeads l 
            left join dbo.TblDepartment lg on lg.DepartmentID = l.DepartmentID
            where l.SystemID = " + html.SystemID + " and l.WarehouseID = " + html.WarehouseID + " and l.DomainID like '%" + html.DomainID + "%'";

            html.IsHaveWhereCondition = true;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Code", FieldName = "LeadCode" },
                new GridField() { Header = "Salutation", FieldName = "Salutation" },
                new GridField() { Header = "Full Name", FieldName = "FullName" },
                new GridField() { Header = "Gender", FieldName = "Gender" },
                new GridField() { Header = "Department", FieldName = "DepartmentName" },
                new GridField() { Header = "Position", FieldName = "Position" },
                new GridField() { Header = "Phone", FieldName = "Phone" },
                new GridField() { Header = "Status", FieldName = "Status" },
				//new GridField() { Header = "Email", FieldName = "Email" },
				new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "Action", FieldName = "LeadID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Employee, Edit Employee")]
        public ActionResult CreateLeads(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                TableColumn = 2,
                UseTable = true,
                FormID = "CreateLeads",
                FieldIDFocus = "TxtFullName",
                TableName = "TblLeads",
                PrimaryField = "LeadID",
                PrimaryFieldValue = PrimaryFieldValue
            };

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Salutation:",
                ID = "TxtSalutation",
                DBField = "Salutation",
                Required = true,
                isTypeahead = true,
                SQL = "select 'None' Salutation union all select 'Mr.' Salutation union all select 'Miss.' Salutation union all select 'Ms.' Salutation union all select 'Mrs.' Salutation union all select 'Dr.' Salutation union all select 'Prof.' Salutation"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "FullName:",
                ID = "TxtFullName",
                DBField = "FullName",
                Required = true,
                SQL = "https://192.168.69.83:9797/api/GetEmployee?CompanyID=1,https://192.168.69.83:9797/api/GetEmployee?CompanyID=2",
                isTypeahead = true,
                IsGetDataFromAPI = true,
                //HiddenFeild= "PhoneNumber",
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFkZjgwMTJmLTA4MTQtNDYzMS1hNmU5LTUzMjU5Y2IyYWUyOSIsIm5iZiI6MTcxMDQ3MDU0MSwiZXhwIjoxNzQyMDA2NTQxLCJpYXQiOjE3MTA0NzA1NDEsImlzcyI6Imh0dHBzOi8vaHIubnNtc29sdXRpb24uY29tLmtoLyIsImF1ZCI6Imh0dHBzOi8vaHIubnNtc29sdXRpb24uY29tLmtoOjk3OTcvYXBpLyJ9.nWQXSDg9at2qhe-BdN6sXT9XryVvXxDoDBQ7Qhi57EY",
                ReplaceJsonKey = new Dictionary<string, string>
                {
                    { "employeeName", "FullName" },
                    { "employeePhoneNumber", "TxtPhone" },
                    { "employeeID", "TxtLeadCode" },
                    { "positionName", "TxtTitle" }
                },
                DisplayOnFields = "TxtPhone,TxtLeadCode, TxtTitle"
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Department:",
                ID = "TxtLeadGroup",
                DBField = "DepartmentID",
                Required = true,
                //SQL_CBO = "select LeadGroupID, GroupName from dbo.TblLeadsGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by GroupName",
                SQL_CBO = @"select DepartmentID, DepartmentName from dbo.TblDepartment where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by DepartmentName",
                DisplayText = "DepartmentName",
                Value = "DepartmentID",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Code:",
                ID = "TxtLeadCode",
                DBField = "LeadCode",
                Required = true,
                ISUniqueField = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Gender:",
                ID = "TxtGender",
                DBField = "Gender",
                Required = true,
                isTypeahead = true,
                SQL = "select 'Male' Gender union all select 'Female' Gender",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Position:",
                ID = "TxtTitle",
                DBField = "Position",
                Required = true,
            });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Phone:",
                ID = "TxtPhone",
                DBField = "Phone",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Email:",
                ID = "TxtEmail",
                DBField = "Email",
                Type = "email",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Address:",
                ID = "TxtAddress",
                DBField = "Address",
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Status:",
                ID = "TxtStatus",
                DBField = "Status",
                SQL_CBO = "select 'Enable' Status union all select 'Disable' Status",
                DisplayText = "Status",
                Value = "Status"
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo",
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}