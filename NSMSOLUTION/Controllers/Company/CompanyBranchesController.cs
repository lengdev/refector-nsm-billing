using MVC;
using System.Collections.Generic;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Company
{
    [Authorize(Roles = "Add Company Branches, Edit Company Branches, Delete Company Branches")]
    public class CompanyBranchesController : Controller
    {
        // GET: CompanyBranches
        public string Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Company Branches"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Company Branches"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Company Branches"))
                html.AllowDelete = true;

            html.AddButton = "Add Company Branches";
            html.AddURL = "AddDepartment/CreateDepartment";
            html.TableName = "TblCompanyBranches";
            html.PrimaryField = "BranchID";
            html.OrderBy = "BranchID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Branch Name",
                SQL = "select BranchID, BranchName from dbo.TblCompanyBranches where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by BranchName",
                SelectValue = "BranchID",
                DisplayField = "BranchName",
                RadioValue = EncryAndDecry.Encryp("BranchID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Branch Name", FieldName = "BranchName" },
                new GridField() { Header = "Action", FieldName = "BranchID", TextAlign = "Center",  IsModifyField = true }
            };
            return html.DrawHTMLSearchForm();
        }
    }
}