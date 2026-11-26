using MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers
{
    public class AddPrefixController : Controller
    {
        // GET: AddPrefix
        [Authorize(Roles = "Add Prefix, Edit Prefix, Delete Prefix")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Prefix"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Prefix"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Prefix"))
                html.AllowDelete = true;

            html.DialogBoxHeight = 300;
            html.AddButton = "Add Prefix";
            html.AddURL = "AddPrefix/CreatePrefix";
            html.TableName = "TblPrefix";
            html.PrimaryField = "PrefixID";
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtYear",
                Label = "Year:",
                DisplayText = "PrefixYear",
                SQL_CBO = "select distinct PrefixYear from TblPrefix where isnull(PrefixYear, '') <> '' and SystemID = " + html.SystemID,
                Value = "PrefixYear",
                DBField = "PrefixYear",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Module",
                SQL = "select distinct [Module] from dbo.TblPrefix where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by Module",
                SelectValue = "Module",
                DisplayField = "Module",
                RadioValue = EncryAndDecry.Encryp("Module"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            html.GH.SQL = $"select row_number() over(order by PrefixID desc) as [No], * from dbo.TblPrefix where isnull(Prefix, '') <> '' and SystemID = {html.SystemID}";
            html.IsHaveWhereCondition = true;
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Prefix", FieldName = "Prefix" ,Width = "100%"},
                new GridField() { Header = "Module", FieldName = "Module" },
                new GridField() { Header = "Current", FieldName = "CurrentPrefix" },
                new GridField() { Header = "Prefix Year", FieldName = "PrefixYear", ClassName="text-center" },
                new GridField() { Header = "Memo", FieldName = "Memo" },
                //new GridField() { Header = "Start Number", FieldName = "StartNumber", TextAlign="center" },
                //new GridField() { Header = "FormatMask", FieldName = "FormatMask", TextAlign="center" },
                //new GridField() { Header = "Display", FieldName = "Display", TextAlign="center" },
                new GridField() { Header = "Action", FieldName = "PrefixID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Prefix, Edit Prefix")]
        public ActionResult CreatePrefix(string PrimaryFieldValue, string Type = "")
        {
            ViewBag.Type = Type;
            HTML html = new HTML();
            if (Type == "Customer")
            {
                html.BtnSaveChangeDialog = "#SaveChange2";
                html.SaveChangeCallbackFunction = "callbackSaveChangePrefix();";
            }
            html.FormID = "CreatePrefix";

            html.FieldIDFocus = "TxtPrefixFor";
            html.TableName = "TblPrefix";
            html.PrimaryField = "PrefixID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Module:",
                ID = "TxtPrefixFor",
                DBField = "Module",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Prefix:",
                ID = "TxtPrefix",
                DBField = "Prefix",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "Module",
                CheckExistingWithDefaultValue = html.WarehouseID,
                CheckExistingWithControlName = "TxtPrefixFor",
                //FieldCondition= "PrefixFor",
                ExLabel = "Ex: Any Prefix {yy} = Year, {MM} = Month, {dd} = day, {00000} = Number"
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Memo:",
                ID = "TxtMemo",
                DBField = "Memo",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "PrefixYear:",
                ID = "TxtPrefixYear",
                DBField = "PrefixYear",
                ISNumeric = true,
                ISInteger = true,
                Required = true,
                Hide = true,
                Value = DateTime.Parse(html.GetCurrentDate().ToString()).ToString("yyyy")
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Format Mask:",
            //    ID = "TxtFormatMask",
            //    DBField = "FormatMask",
            //    Required = true,
            //    //IsallowInputOnlyNumber = true
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Display:",
            //    ID = "TxtDisplay",
            //    DBField = "Display",
            //    IsReadOnly = true,
            //});
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}