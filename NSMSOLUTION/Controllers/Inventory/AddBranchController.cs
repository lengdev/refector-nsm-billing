using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers
{
    [Authorize]
    public class AddBranchController : Controller
    {
        // GET: AddBranch
        [Authorize(Roles = "Add Branch, Edit Branch, Delete Branch")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Branch"
            };
            if (html.HasPermission("Add Branch"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Branch"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Branch"))
                html.AllowDelete = true;

            html.AddButton = "Add Branch";
            html.AddURL = "AddBranch/CreateBranch";
            html.TableName = "TblBranch";
            html.PrimaryField = "BranchID";
            html.OrderBy = "BranchID desc";
            html.DialogBoxWidth = "1000";
            html.DialogBoxHeight = 500;

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Branch Name",
                SQL = "select BranchID, BranchName from dbo.TblBranch where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by BranchName",
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
                new GridField() { Header = "Description", FieldName = "BranchDescription" },
                new GridField() { Header = "Address", FieldName = "Address" , Width = "100%"},
                //new GridField() { Header = "Default", FieldName = "IsDefaultBranch" },
                new GridField() { Header = "Action", FieldName = "BranchID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Branch, Edit Branch")]
        public ActionResult CreateBranch(string PrimaryFieldValue, string Type = "")
        {
            HTML html = new HTML();
            if (Type == "NewItem")
            {
                html.BtnSaveChangeDialog = "SaveChange2";
            }
            html.FormID = "CreateBranch";
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.FieldIDFocus = "TxtBranchName";
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblBranch",
                    FormTitle="Create Branch",
                    GetIDENTITYByFiled="BranchID",
                    PrimaryFieldName="BranchID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Branch Name:",
                            ID = "TxtBranchName",
                            DBField = "BranchName",
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
                            Label = "Contact Name:",
                            ID = "TxtContactNameBranch",
                            DBField = "ContactName",
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            Required= true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Contact Phone:",
                            ID = "TxtContactPhoneBranch",
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
                            Label = "Branch Description:",
                            ID = "TxtBranchDescriptionBranch",
                            DBField = "BranchDescription",
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
                            ID = "TxtAddressBranch",
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
                            Label="Commune:", ID="TxtCommuneBranch", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Commune", SQL="select DistrictID, Commune from dbo.TblCommune where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            isTypeahead = true, HiddenField ="DistrictID",
                            SQLCallBack="select td.District, p.ProvinceCity from dbo.TblDistrict td inner join dbo.TblCommune c on c.DistrictID = td.DistrictID inner join dbo.TblProvinceCity p on p.ProvinceCityID = td.ProvinceCityID where td.DistrictID = @DistrictID and td.SystemID = " + html.SystemID + " and td.WarehouseID = " + html.WarehouseID,
                            CallBackToControlID="TxtDistrictBranch, TxtProvinceCityBranch",
                            AddValueToTblNameIfNotExistingData="TblCommune", AddValueToTblNameIfNotExistingDataFieldName="Commune",

                        },
                        new HtmlTextBox()
                        {
                            Label="District:", ID="TxtDistrictBranch",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="District",
                            AddValueToTblNameIfNotExistingData="TblDistrict", AddValueToTblNameIfNotExistingDataFieldName="District",
                            SetIDENTITYToTblName="TblCommune", GetIDENTITYFromFieldName="DistrictID",
                            SETIDENTITYFromControlID="TxtCommuneBranch", SETIDENTITYFieldName="Commune"
                        },
                        new HtmlTextBox()
                        {
                            Label="Province/City:", ID="TxtProvinceCityBranch",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ProvinceCity",
                            AddValueToTblNameIfNotExistingData="TblProvinceCity", AddValueToTblNameIfNotExistingDataFieldName="ProvinceCity",
                            SetIDENTITYToTblName="TblDistrict", GetIDENTITYFromFieldName="ProvinceCityID",
                            SETIDENTITYFromControlID="TxtDistrictBranch", SETIDENTITYFieldName="District"
                        }
                    }
            }
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
            //HTML html = new HTML();
            //html.FormID = "CreateBranch";
            //html.FieldIDFocus = "TxtBranchName";
            //html.TableName = "TblBranch";
            //html.PrimaryField = "BranchID";
            //html.PrimaryFieldValue = PrimaryFieldValue;
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Branch Name:",
            //    ID = "TxtBranchName",
            //    DBField = "BranchName",
            //    Required = true,
            //    ISUniqueFiled = true,
            //    CheckExsitingWithField = "WarehouseID",
            //    CheckExsitingWithDefaultValue = html.WarehouseID
            //});
            //html.HTMLControls.Add(new HtmlTextarea()
            //{
            //    Label = "Branch Description:",
            //    ID = "TxtBranchDescription",
            //    DBField = "BranchDescription",
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
            ////html.HTMLControls.Add(new HtmlCheckBox()
            ////{
            ////    Label = "Is Default Branch:",
            ////    ID = "TxtIsDefaultBranch",
            ////    DBField = "IsDefaultBranch",
            ////    ISCheckBox = true,
            ////});
            //ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            //return View();
        }
    }
}