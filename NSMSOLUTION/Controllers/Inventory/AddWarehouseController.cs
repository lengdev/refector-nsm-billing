using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers
{
    [Authorize]
    public class AddWarehouseController : Controller
    {
        // GET: AddWarehouse
        [Authorize(Roles = "Add Warehouse, Edit Warehouse, Delete Warehouse")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Warehouse"
            };
            if (html.HasPermission("Add Warehouse"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Warehouse"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Warehouse"))
                html.AllowDelete = true;

            html.AddButton = "Add Warehouse";
            html.AddURL = "AddWarehouse/CreateWarehouse";
            html.TableName = "TblUserWarehouse";
            html.PrimaryField = "WID";
            html.OrderBy = "WID desc";
            html.DialogBoxWidth = "1000";
            html.DialogBoxHeight = 500;

            string sql = @"select ROW_NUMBER() OVER(ORDER BY tw.WID desc) as [No], tw.WID, tw.WarehouseName,
            tw.WarehouseDescription, tw.[Address], b.BranchName
            from dbo.TblUserWarehouse tw
            inner join dbo.TblBranch b on b.BranchID = tw.BranchID
            where tw.SystemID = " + html.SystemID + " and tw.WarehouseID = " + html.WarehouseID;
            html.OrderBy = "b.BranchName, tw.WID desc";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Warehouse Name",
                SQL = "select WID, WarehouseName from dbo.TblUserWarehouse where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by WarehouseName",
                SelectValue = "WID",
                DisplayField = "WarehouseName",
                RadioValue = EncryAndDecry.Encryp("WID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Branch Name", FieldName = "BranchName", IsGroupByField = true, Colspan=5 },
                new GridField() { Header = "Warehouse Name", FieldName = "WarehouseName" },
                new GridField() { Header = "Description", FieldName = "WarehouseDescription" },
                new GridField() { Header = "Address", FieldName = "Address",Width = "100%" },
                //new GridField() { Header = "Default", FieldName = "IsDefaultBranch" },
                new GridField() { Header = "Action", FieldName = "WID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Warehouse, Edit Warehouse")]
        public ActionResult CreateWarehouse(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "NewItem")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
            }
            html.FormID = "CreateWarehouse";
            //html.FieldIDFocus = "TxtWarehouse";
            //html.TableName = "TblUserWarehouse";
            //html.PrimaryField = "WID";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblUserWarehouse",
                    FormTitle="Create Warehouse",
                    GetIDENTITYByFiled="WID",
                    PrimaryFieldName="WID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Branch Name", ID="TxtBranchID", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="BranchName", SQL="select BranchID, BranchName from dbo.TblBranch where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            isTypeahead = true, HiddenField ="BranchID",
                            AddButtonFunction="AddNewBranch();", TypeAheadAddNewFunctionName="AddNewBranch()"
                        },
                        new HtmlTextBox()
                        {
                            Label = "Warehouse Name",
                            ID = "Txt_WarehouseWarehouseName",
                            DBField = "WarehouseName",
                            Required = true,
                            ISUniqueField = true,
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Contact Name",
                            ID = "Txt_WarehouseContactName",
                            DBField = "ContactName",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            Required= true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Contact Phone",
                            ID = "Txt_WarehouseContactPhone",
                            DBField = "ContactPhone",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            Required= true,
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Warehouse Description",
                            ID = "TxtWarehouseDescription",
                            DBField = "WarehouseDescription",
                            Class="input2 CustInfo",
                            IsCustomForm = true
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Address:",
                            ID = "TxtWarehouseAddress",
                            DBField = "Address",
                            Required= true,
                            Class="input2 CustInfo",
                            IsCustomForm = true
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Commune", ID="TxtWarehouseCommune", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Commune", SQL="select DistrictID, Commune from dbo.TblCommune where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            isTypeahead = true, HiddenField ="DistrictID",
                            SQLCallBack="select td.District, p.ProvinceCity from dbo.TblDistrict td inner join dbo.TblCommune c on c.DistrictID = td.DistrictID inner join dbo.TblProvinceCity p on p.ProvinceCityID = td.ProvinceCityID where td.DistrictID = @DistrictID and td.SystemID = " + html.SystemID + " and td.WarehouseID = " + html.WarehouseID,
                            CallBackToControlID="TxtWarehouseDistrict, TxtWarehouseProvinceCity",
                            AddValueToTblNameIfNotExistingData="TblCommune", AddValueToTblNameIfNotExistingDataFieldName="Commune",

                        },
                        new HtmlTextBox()
                        {
                            Label="District", ID="TxtWarehouseDistrict",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="District",
                            AddValueToTblNameIfNotExistingData="TblDistrict", AddValueToTblNameIfNotExistingDataFieldName="District",
                            SetIDENTITYToTblName="TblCommune", GetIDENTITYFromFieldName="DistrictID",
                            SETIDENTITYFromControlID="TxtWarehouseCommune", SETIDENTITYFieldName="Commune"
                        },
                        new HtmlTextBox()
                        {
                            Label="Province/City", ID="TxtWarehouseProvinceCity",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ProvinceCity",
                            AddValueToTblNameIfNotExistingData="TblProvinceCity", AddValueToTblNameIfNotExistingDataFieldName="ProvinceCity",
                            SetIDENTITYToTblName="TblDistrict", GetIDENTITYFromFieldName="ProvinceCityID",
                            SETIDENTITYFromControlID="TxtWarehouseDistrict", SETIDENTITYFieldName="District"
                        }
                    }
            }
            };


            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Branch Name:",
            //    ID = "TxtBranchID",
            //    DBField = "BranchID",
            //    SQL_CBO = "select BranchID, BranchName from dbo.TblBranch where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
            //    DisplayText = "BranchName",
            //    Value = "BranchID"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Warehouse Name:",
            //    ID = "TxtWarehouseName",
            //    DBField = "WarehouseName",
            //    Required = true,
            //    ISUniqueFiled = true,
            //    CheckExsitingWithField = "WarehouseID",
            //    CheckExsitingWithDefaultValue = html.WarehouseID
            //});
            //html.HTMLControls.Add(new HtmlTextarea()
            //{
            //    Label = "Warehouse Description:",
            //    ID = "TxtWarehouseDescription",
            //    DBField = "WarehouseDescription",
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Address:",
            //    ID = "TxtAddress",
            //    DBField = "Address",
            //    Required= true,
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "DistrictID:",
            //    ID = "TxtDistrictID",
            //    Hide = true
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Commune:",
            //    ID = "TxtCommune",
            //    DBField = "Commune",
            //    SQL_AutoComplete = "select distinct top 10 DistrictID, Commune from dbo.TblCommune where lower(Commune) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
            //    ISAutoComplete = true,
            //    ISCallBack = true,
            //    SQLCallBack = "select td.District, p.ProvinceCity from dbo.TblDistrict td inner join dbo.TblCommune c on c.DistrictID = td.DistrictID inner join dbo.TblProvinceCity p on p.ProvinceCityID = td.ProvinceCityID where lower(c.Commune) = @SearchText and td.DistrictID = @Search_Text2 and td.SystemID = " + html.SystemID + " and td.WarehouseID = " + html.WarehouseID,
            //    CallBackToControlID = "TxtDistrict",
            //    FieldCondition = "DistrictID",
            //    CallBackWithControllValueID = "TxtDistrictID",
            //    DisplayOnFields = "TxtDistrict, TxtProvinceCity",
            //    HiddenFeild = "TxtDistrictID"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "District:",
            //    ID = "TxtDistrict",
            //    DBField = "District",
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Province/City:",
            //    ID = "TxtProvinceCity",
            //    DBField = "ProvinceCity",
            //});
            //ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
    }
}