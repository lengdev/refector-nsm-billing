using MVC;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddEmployeesController : Controller
    {
        // GET: AddEmployees
        [Authorize(Roles = "Add Employees, Edit Employees, Delete Employees")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Employees"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Employees"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Employees"))
                html.AllowDelete = true;

            html.AddButton = "Add Employee";
            html.AddURL = "AddEmployees/CreateEmployee";
            html.TableName = "TblEmployees";
            html.PrimaryField = "EmployeeID";
            html.OrderBy = "EmployeeName";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Employee Name",
                SQL = "select EmployeeID, EmployeeName from dbo.TblEmployees where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by EmployeeName",
                SelectValue = "EmployeeID",
                DisplayField = "EmployeeName",
                RadioValue = EncryAndDecry.Encryp("EmployeeID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "EmployeeCode", FieldName = "EmployeeCode" },
                new GridField() { Header = "EmployeeName", FieldName = "EmployeeName" },
                new GridField() { Header = "Gender", FieldName = "Gender" },
                new GridField() { Header = "IdentificationNo", FieldName = "IdentificationNo" },
                new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "MobileNumber", FieldName = "MobileNumber" },
                new GridField() { Header = "StartWorkingDate", FieldName = "StartWorkingDate" },
                new GridField() { Header = "Salary", FieldName = "Salary" },
                new GridField() { Header = "Photo", FieldName = "PhotoFileName", ImagePath="Content/Employee Photos" },
                new GridField() { Header = "Action", FieldName = "EmployeeID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Employees, Edit Employees")]
        public ActionResult CreateEmployee(string PrimaryFieldValue)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            HTML html = new HTML
            {
                FormID = "CreateEmployee",
                FieldIDFocus = "TxtEmployeeCode",
                TableName = "TblEmployees",
                PrimaryField = "EmployeeID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Employee Code:",
                ID = "TxtEmployeeCode",
                DBField = "EmployeeCode",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Employee Name:",
                ID = "TxtEmployeeName",
                DBField = "EmployeeName",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Gender:",
                ID = "TxtGender",
                DBField = "Gender",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Identification No:",
                ID = "TxtIdentificationNo",
                DBField = "IdentificationNo",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Address:",
                ID = "TxtAddress",
                DBField = "Address",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Mobile Number:",
                ID = "TxtMobileNumber",
                DBField = "MobileNumber",
                Required = true,
                Type = "tel"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Email:",
                ID = "TxtEmailAddress",
                DBField = "Email",
                Type = "email"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Facebook Name:",
                ID = "TxtFacebookAddress",
                DBField = "Facebook",
                Type = "url"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Start Working Date:",
                ID = "TxtStartWorkingDate",
                DBField = "StartWorkingDate",
                Type = "date"
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Working Name:",
                ID = "TxtWorkingID",
                DBField = "WorkingID",
                SQL_CBO = "select WorkingID, WorkingName from dbo.TblWorkingTime where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by WorkingName",
                Value = "WorkingID",
                DisplayText = "WorkingName"
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Department Name:",
                ID = "TxtDepartmentID",
                DBField = "DepartmentID",
                SQL_CBO = "select DepartmentID, DepartmentName from dbo.TblDepartment where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by DepartmentName",
                Value = "DepartmentID",
                DisplayText = "DepartmentName"
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Position Name:",
                ID = "TxtPositionID",
                DBField = "PositionID",
                SQL_CBO = "select PositionID, PositionName from dbo.TblPosition where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by PositionName",
                Value = "PositionID",
                DisplayText = "PositionName"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Salary:",
                ID = "TxtSalary",
                DBField = "Salary",
                Type = "number"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Register Date:",
                ID = "TxtRegisterDate",
                DBField = "RegisterDate",
                Type = "date"
            });
            html.HTMLControls.Add(new HtmlFile()
            {
                Label = "Photo:",
                ID = "TxtPhotoFileName",
                DBField = "PhotoFileName",
                DestinationFile = "Employee Photos",
                AcceptImageOnly = true,
                ISFile = true
            });

            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}