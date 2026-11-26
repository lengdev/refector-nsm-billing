using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddPackagesController : Controller
    {
        // GET: AddPackages
        [Authorize(Roles = "Add Package, Edit Package")]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Packages"
            };
            if (html.HasPermission("Add Package"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Package"))
                html.AllowEdit = true;

            html.AddButton = "Add Package";
            html.AddURL = "AddPackages/CreatePackage";
            html.TableName = "TblPackages";
            html.PrimaryField = "PackageID";
            html.DialogBoxWidth = "Auto";
            html.DialogBoxHeight = 500;
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtPackageStatus",
                Label = "Package Status:",
                DBField = "PackageStatus",
                SQL_CBO = "select distinct PackageStatus from dbo.TblPackages order by PackageStatus desc",
                DisplayText = "PackageStatus",
                Value = "PackageStatus",
                ComboBox = true
            });

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Package Name",
                SQL = "select PackageName from dbo.TblPackages where lower(PackageName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by PackageID desc",
                RadioValue = EncryAndDecry.Encryp("PackageName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            string SQL = @"select row_number() over(partition by p.PackageID order by p.PackageID desc, pd.Bandwidth) as [No], p.PackageID,
p.PackageCode, s.ServiceName, p.PackageName, p.PackageStatus, concat(pd.Bandwidth, ' ', 'Mbps') as Bandwidth, concat(pd.Duration,' ', pd.UM) as Duration, ISNULL(pd.Cost, 0) Cost, pd.Price, pd.Feature, pd.Status
from dbo.TblPackages p
inner join dbo.TblPackageDetail pd on p.PackageID = pd.PackageID
inner join dbo.TblServices s on s.ServiceID = p.ServiceID
where (@PackageName = '' or p.PackageName = @PackageName)
and (@PackageStatus = '' or p.PackageStatus = @PackageStatus)";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() {
                    Header = "Package Name",
                    FieldName = "PackageName",
                    IsGroupByField = true,
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string,string> {
                        { "Enable", "badge-primary" },
                        { "Disable", "badge-danger" }
                    },
                    BadgeSourceField = "PackageStatus"
                },
                //new GridField() { Header = "Service Name", FieldName = "ServiceName" },
                new GridField() { Header = "Bandwidth", FieldName = "Bandwidth", TextAlign="center"},
                new GridField() { Header = "Duration", FieldName = "Duration", TextAlign="center"},
                new GridField() { Header = "Cost", FieldName = "Cost", TextAlign="center", Format = "#,#0.00", ClassName = "text-right" },
                new GridField() { Header = "Price", FieldName = "Price", TextAlign="center", Format = "#,#0.00", ClassName = "text-right" },
                new GridField() { Header = "Feature", FieldName = "Feature" },
                new GridField() {
                    Header = "Status",
                    FieldName = "Status",
                    ClassName = "text-center",
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string,string> {
                        { "Enable", "badge-primary" },
                        { "Disable", "badge-danger" }
                    }
                },
                new GridField() { Header = "Action", FieldName = "PackageID", TextAlign = "Center", IsModifyField = true},
            };
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                IsRawSql = true,
                SQLQuery = SQL,
                OrderBy = "PackageID desc"
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Package, Edit Package")]
        public ActionResult CreatePackage(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreatePackage",
                FieldIDFocus = "TxtServiceName",
                TableName = "TblPackages",
                PrimaryField = "PackageID",
                PrimaryFieldValue = PrimaryFieldValue,
                NeedsValidationForm = false,
                SaveChangeFunction = "SaveChangeAddPackage()",
                UseTable = true,
                TableColumn = 2
            };
            string[] editdata = new string[20];
            editdata[9] = "True";
            editdata[10] = "Prepaid";
            ViewBag.PackageData = "";
            ViewBag.PrimaryValue = "0";
            if (PrimaryFieldValue != "0")
            {
                string sql = "select p.ServiceID, p.PackageTypeID, p.PackageName, p.SpecialNote,";
                sql += "p.TermsConditions, p.Remark, p.ExpenseAccountID, p.IncomeAccountID, p.PackageCode, p.IsDoubleSpeedAtNight, p.PackageType, pd.ARAccountID, p.PackageStatus ";
                sql += "from dbo.TblPackages p ";
                sql += "inner join dbo.TblPackageDetail pd on pd.PackageID = p.PackageID ";
                sql += "where p.PackageID = " + PrimaryFieldValue + " and pd.SystemID= " + html.SystemID + " and pd.WarehouseID=" + html.WarehouseID + " and pd.DomainID like '%" + html.DomainID + "%' ";
                DataTable dt = html.GetDataForEdit(sql);
                editdata[0] = dt.Rows[0]["ServiceID"].ToString();
                editdata[1] = dt.Rows[0]["PackageTypeID"].ToString();
                editdata[2] = html.UnescapeXml(dt.Rows[0]["PackageName"].ToString());
                editdata[3] = html.UnescapeXml(dt.Rows[0]["SpecialNote"].ToString());
                editdata[4] = html.UnescapeXml(dt.Rows[0]["TermsConditions"].ToString());
                editdata[5] = html.UnescapeXml(dt.Rows[0]["Remark"].ToString());
                editdata[6] = html.UnescapeXml(dt.Rows[0]["ExpenseAccountID"].ToString());
                editdata[7] = html.UnescapeXml(dt.Rows[0]["IncomeAccountID"].ToString());
                editdata[8] = html.UnescapeXml(dt.Rows[0]["PackageCode"].ToString());
                editdata[9] = html.UnescapeXml(dt.Rows[0]["IsDoubleSpeedAtNight"].ToString());
                editdata[10] = html.UnescapeXml(dt.Rows[0]["PackageType"].ToString());
                editdata[11] = html.UnescapeXml(dt.Rows[0]["ARAccountID"].ToString());
                editdata[12] = html.UnescapeXml(dt.Rows[0]["PackageStatus"].ToString());
                sql = "select Bandwidth, PackageDetailID, Duration, UM, isnull(Cost,0)Cost, isnull(Price,0)Price, DowSpeed, UpSpeed, DepositFee, FreeDuration, Feature, IsDoubleSpeedAtNight, Status ";
                sql += "from dbo.TblPackageDetail where PackageID = " + PrimaryFieldValue + " and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
                ViewBag.PackageData = html.JsonData(sql).ToString();
                ViewBag.PrimaryValue = PrimaryFieldValue;
            }
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Service Name:",
                ID = "TxtServiceName",
                //DBField = "ServiceID",
                SQL_CBO = "select ServiceID, ServiceName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by ServiceName",
                Value = "ServiceID",
                DisplayText = "ServiceName",
                //ISCallBackToCbo = true,
                //SQLCallBack = "select PackageTypeID, PackageTypeName from dbo.TblPackageType where ServiceID = @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by PackageTypeName",
                //CallBackDisplayText = "PackageTypeName",
                //CallBackDisplayValue = "PackageTypeID",
                //CallBackToControlID = "TxtPackageTypeName",
                CboValue = editdata[0]
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Connection Type:",
                ID = "TxtPackageTypeName",
                CboValue = editdata[1],
                Value = "PackageTypeID",
                DBField = "PackageTypeID",
                SQL_CBO = "select PackageTypeID, PackageTypeName from dbo.TblPackageType where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by PackageTypeName",
                DisplayText = "PackageTypeName",
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Package Type:",
                ID = "TxtPackageType",
                CboValue = editdata[10],
                Value = "PackageType",
                DBField = "PackageType",
                SQL_CBO = "select 'Prepaid' PackageType union all select 'Postpaid' PackageType",
                DisplayText = "PackageType",
            });


            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Package Group:",
            //    ID = "TxtPackageGroup",
            //    SQL_CBO = "select PackageGroupID, PackageGroupName from dbo.TblPackageGroup where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by PackageGroupName",
            //    Value = "PackageGroupID",
            //    DisplayText = "PackageGroupName",
            //});

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Name:",
                ID = "TxtPackageName",
                DBField = "PackageName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID,
                Value = editdata[2]
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Package Code:",
            //    ID = "TxtPackageCode",
            //    DBField = "PackageCode",
            //    Required = true,
            //    ISUniqueField = true,
            //    CheckExistingWithField = "WarehouseID",
            //    CheckExistingWithDefaultValue = html.WarehouseID,
            //    Value = editdata[2],
            //    Hide = true
            //    //SQLPreFix = "declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = Prefix, @Module = Module from dbo.TblPrefix where Module = 'Package Code' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID,
            //    //CallBackToControlID = "TxtPackageCode, PrefixPackageCode",
            //    //UpdatePrefixModule = "Package Code",
            //    //Prefix = "PrefixPackageCode",
            //    //SetPrefixToDbFieldName = "PackageCode",
            //    //IsReadOnly = true,
            //    //PrimaryFieldValue = PrimaryFieldValue
            //});           

            string ExpenseAccount = @"select distinct coa.ChartOfAccountID ExpenseAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Expense' AND NOT (
      coa.AccountName LIKE '%Discount%') and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Expense Account:",
                ID = "TxtExpenseAccountID",
                CboValue = editdata[6],
                SQL_CBO = ExpenseAccount,
                DBField = "ExpenseAccountID",
                Value = "ExpenseAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 350,
                Required = true,
            });

            string InputVATAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Asset' and coa.AccountName LIKE '%VAT%' and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Input VAT Account:",
                ID = "TxtInputVATAccount",
                CboValue = editdata[7],
                SQL_CBO = InputVATAccount,
                DBField = "InputVATAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });

            string ARAccounts = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Asset' and coa.AccountName LIKE '%Receivable%' and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "AR Account:",
                ID = "Txt_ARAccount",
                CboValue = editdata[11],
                SQL_CBO = ARAccounts,
                DBField = "ARAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });

            string IncomeAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Revenue'  and not coa.AccountName LIKE '%Discount%' and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Income Account:",
                ID = "TxtIncomeAccount",
                CboValue = editdata[7],
                SQL_CBO = IncomeAccount,
                DBField = "IncomeAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });


            string DeferredRevenueAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Liability' and coa.AccountName LIKE '%Deferred%' and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Deferred Revenue Account:",
                ID = "TxtDeferredRevenueAccount",
                CboValue = editdata[7],
                SQL_CBO = DeferredRevenueAccount,
                DBField = "DeferredRevenueAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                //Required = true,
            });

            string OutputVATAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Liability' AND coa.AccountName LIKE '%VAT%' and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Output VAT Account:",
                ID = "TxtOutputVATAccount",
                CboValue = editdata[7],
                SQL_CBO = OutputVATAccount,
                DBField = "OutputVATAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });

            string SalesDiscountAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Revenue' and coa.AccountName LIKE '%Discount%' and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Sales Discount Account:",
                ID = "Txt_SalesDiscountAccount",
                CboValue = editdata[11],
                SQL_CBO = SalesDiscountAccount,
                DBField = "SalesDiscountAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });

            string PurchaseDiscountAccount = @"select distinct coa.ChartOfAccountID, cat.AccountCategory, ct.AccountTypes, coa.AccountNo, coa.AccountName
from TblChartOfAccount coa
inner join dbo.TblAccountTypes ct on ct.AccountTypesID = coa.AccountTypesID
inner join dbo.TblAccountCategories cat on cat.AccountCategoryID = coa.AccountCategoryID
where ct.AccountTypes = 'Expense' and coa.AccountName LIKE '%Discount%' and coa.SystemID = " + html.SystemID + " and coa.WarehouseID = " + html.WarehouseID + " and coa.DomainID like '%" + html.DomainID + "%' order by cat.AccountCategory, ct.AccountTypes";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Purchase Discount Account:",
                ID = "Txt_PurchaseDiscountAccount",
                CboValue = editdata[11],
                SQL_CBO = PurchaseDiscountAccount,
                DBField = "PurchaseDiscountAccountID",
                Value = "ChartOfAccountID",
                DisplayText = "AccountName",
                GroupByField = "AccountTypes",
                IsKendoMultiColumnComboBox = true,
                KendoComboBoxColumn = new List<KendoComboBoxColumns>
                {
                    new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                    new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "300" }
                },
                KendoMultiColumnComboBoxHeight = 300,
                Required = true,
            });

            //string sqlacc = "select ca.ChartOfAccountID, (ca.AccountNo + ' - ' + ca.AccountName + ' (' + ca.AccountType + ')') AccountName ";
            //sqlacc += "from dbo.TblChartOfAccount ca ";
            //sqlacc += "inner join dbo.TblAccountType act on act.AccountTypeID = ca.AccountTypeID ";
            //sqlacc += "where ca.SystemID = " + html.SystemID + " and ca.WarehouseID = " + html.WarehouseID + " and ca.DomainID like '%" + html.DomainID + "%'";
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "COGS:",
            //    ID = "TxtCOGS",
            //    SQL_CBO= sqlacc,
            //    DisplayText= "AccountName",
            //    Value= "ChartOfAccountID",
            //    CboValue= editdata[6],
            //    Infinity = false               
            //});
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Revenue:",
            //    ID = "TxtRevenue",
            //    SQL_CBO = sqlacc,
            //    DisplayText = "AccountName",
            //    Value = "ChartOfAccountID",
            //    CboValue = editdata[7],
            //    Infinity = false,
            //    Hide = true               
            //});
            //html.HTMLControls.Add(new HtmlTextarea()
            //{
            //    Label = "Special  Note:",
            //    ID = "TxtSpecialNote",
            //    Value = editdata[3]
            //    //DBField = "SpecialNote"
            //});
            //html.HTMLControls.Add(new HtmlTextarea()
            //{
            //    Label = "Terms and Conditions:",
            //    ID = "TxtTermsandCondition",
            //    Value = editdata[4]
            //    //DBField = "TermsCondition"
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Status:",
                ID = "TxtPackageStatus",
                DBField = "PackageStatus",
                Required = true,
                isTypeahead = true,
                SQL = "select 'Enable' PackageStatus union all select 'Disable' PackageStatus",
                Value = editdata[12] ?? "Enable"
            });
            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "Remark:",
                ID = "TxtRemark",
                Value = editdata[5]
                //DBField = "Remark"
            });
            html.HTMLControls.Add(new HtmlCheckBox()
            {
                ISCheckBox = true,
                Label = "Is Double Speed At Night:",
                ID = "IsDoubleSpeedAtNight",
                DBField = "IsDoubleSpeedAtNight",
                Checked = true,
                Value = editdata[9],
                LabelValign = "top"
            });



            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Status:",
            //    ID = "TxtStatus",
            //    DBField = "Status",
            //    SQL_CBO = "select 'Enable' [Status]  union select 'Disable' [Status]",
            //    DisplayText = "Status",
            //    Value = "Status"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Package Feature:",
            //    ID = "TxtPackageFeature",
            //    DBField = "PackageFeature",
            //    isTagsInput = true
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Speed Limited (Mbps):",
            //    ID = "TxtSpeedLimited",
            //    DBField = "SpeedLimited",
            //    Type="number"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Upload Speed (Mbps):",
            //    ID = "TxtUploadSpeed",
            //    DBField = "UploadSpeed",
            //    Type = "number"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Download Speed (Mbps):",
            //    ID = "TxtDownloadSpeed",
            //    DBField = "DownloadSpeed",
            //    Type = "number"
            //});
            ////html.HTMLControls.Add(new HtmlTextBox()
            ////{
            ////    Label = "Data Usage (GB):",
            ////    ID = "TxtDataUsage",
            ////    DBField = "DataUsage",
            ////    Type="number"
            ////});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Package Price:",
            //    ID = "TxtPackagePrice",
            //    DBField = "PackagePrice",
            //    Type = "number"
            //});
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Package Duration:",
            //    ID = "TxtPackageDuration",
            //    DBField = "PackageDurationID",
            //    SQL_CBO = "select PackageDurationID, (convert(varchar, PackageDuration) + ' - ' + PackageDurationName) PackageDurationName from dbo.TblPackageDuration where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by PackageDuration",
            //    Value = "PackageDurationID",
            //    DisplayText = "PackageDurationName"
            //});
            //html.RestFields = "TxtPackageDuration, TxtPackagePrice, TxtDownloadSpeed, TxtUploadSpeed, TxtSpeedLimited";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();

            //			string sqla = @"select distinct coa.ChartOfAccountID, ac.AccountCategory, act.AccountTypes, coa.AccountNo, coa.AccountName
            //from dbo.TblAccountTypes act
            //inner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID
            //inner join dbo.TblChartOfAccount coa on coa.AccountType = act.AccountTypesID
            //where ac.AccountCategoryID = 3";
            //			ViewBag.Datatest = html.JsonData(sqla);
            return View();
        }
        public ActionResult SaveChangePackage(object[] Package, object[] PackageDetail)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            dynamic jsonPackage = new JavaScriptSerializer().DeserializeObject(Package[0].ToString());
            dynamic jsonPackageDetail = new JavaScriptSerializer().DeserializeObject(PackageDetail[0].ToString());
            HTML html = new HTML();
            string SystemID = html.SystemID;
            html.SaveChangeAndClose = true;
            string WarehouseID = html.WarehouseID;
            string PackageID = jsonPackage[0]["PackageID"];
            string ServiceNameID = jsonPackage[0]["ServiceNameID"];
            string ConnectionTypeID = jsonPackage[0]["ConnectionTypeID"];
            string PackageCode = HttpUtility.UrlDecode(jsonPackage[0]["PackageCode"]);
            string PackageName = HttpUtility.UrlDecode(jsonPackage[0]["PackageName"]);
            string SpecialNote = HttpUtility.UrlDecode(jsonPackage[0]["SpecialNote"]);
            string TermsandCondition = HttpUtility.UrlDecode(jsonPackage[0]["TermsandCondition"]);
            string Remark = HttpUtility.UrlDecode(jsonPackage[0]["Remark"]);
            string IsDoubleSpeedAtNight = HttpUtility.UrlDecode(jsonPackage[0]["IsDoubleSpeedAtNight"]);
            string ServiceName = HttpUtility.UrlDecode(jsonPackage[0]["ServiceName"]);
            string ExpenseAccountID = HttpUtility.UrlDecode(jsonPackage[0]["ExpenseAccountID"]);
            string IncomeAccountID = HttpUtility.UrlDecode(jsonPackage[0]["IncomeAccountID"]);
            string SalesDiscountAccountID = HttpUtility.UrlDecode(jsonPackage[0]["SalesDiscountAccountID"]);
            string PurchaseDiscountAccountID = HttpUtility.UrlDecode(jsonPackage[0]["PurchaseDiscountAccountID"]);
            string PackageType = HttpUtility.UrlDecode(jsonPackage[0]["PackageType"]);

            string InputVATAccountID = HttpUtility.UrlDecode(jsonPackage[0]["InputVATAccountID"]);
            string DeferredRevenueAccountID = HttpUtility.UrlDecode(jsonPackage[0]["DeferredRevenueAccountID"]);
            string OutputVATAccountID = HttpUtility.UrlDecode(jsonPackage[0]["OutputVATAccountID"]);
            string ARAccountID = HttpUtility.UrlDecode(jsonPackage[0]["ARAccountID"]);
            string PackageStatus = HttpUtility.UrlDecode(jsonPackage[0]["PackageStatus"]);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<Data>");
            sb.AppendLine("<PackageInfo>");
            sb.AppendLine("<PackageID>" + PackageID + "</PackageID>");
            sb.AppendLine("<ServiceID>" + ServiceNameID + "</ServiceID>");
            sb.AppendLine("<PackageTypeID>" + ConnectionTypeID + "</PackageTypeID>");
            sb.AppendLine("<PackageCode>" + html.EscapeXml(HttpUtility.UrlDecode(PackageCode.Trim())) + "</PackageCode>");
            sb.AppendLine("<PackageType>" + html.EscapeXml(HttpUtility.UrlDecode(PackageType.Trim())) + "</PackageType>");
            sb.AppendLine("<PackageName>" + html.EscapeXml(HttpUtility.UrlDecode(PackageName.Trim())) + "</PackageName>");
            sb.AppendLine("<SpecialNote>" + html.EscapeXml(HttpUtility.UrlDecode(SpecialNote.Trim())) + "</SpecialNote>");
            sb.AppendLine("<TermsConditions>" + html.EscapeXml(HttpUtility.UrlDecode(TermsandCondition.Trim())) + "</TermsConditions>");
            sb.AppendLine("<Remark>" + html.EscapeXml(HttpUtility.UrlDecode(Remark.Trim())) + "</Remark>");
            sb.AppendLine("<IsDoubleSpeedAtNight>" + IsDoubleSpeedAtNight + "</IsDoubleSpeedAtNight>");
            sb.AppendLine("<Date>" + easternTimeNow.ToString("yyyy/MM/dd hh:mm:ss") + "</Date>");
            sb.AppendLine("<SystemID>" + SystemID + "</SystemID>");
            sb.AppendLine("<WarehouseID>" + WarehouseID + "</WarehouseID>");
            sb.AppendLine("<DomainID>" + html.DomainID + "</DomainID>");
            sb.AppendLine("<IncomeAccountID>" + IncomeAccountID + "</IncomeAccountID>");
            sb.AppendLine("<ExpenseAccountID>" + ExpenseAccountID + "</ExpenseAccountID>");
            sb.AppendLine("<InputVATAccountID>" + InputVATAccountID + "</InputVATAccountID>");
            sb.AppendLine("<DeferredRevenueAccountID>" + DeferredRevenueAccountID + "</DeferredRevenueAccountID>");
            sb.AppendLine("<OutputVATAccountID>" + OutputVATAccountID + "</OutputVATAccountID>");
            sb.AppendLine("<ARAccountID>" + ARAccountID + "</ARAccountID>");
            sb.AppendLine("<SalesDiscountAccountID>" + SalesDiscountAccountID + "</SalesDiscountAccountID>");
            sb.AppendLine("<PurchaseDiscountAccountID>" + PurchaseDiscountAccountID + "</PurchaseDiscountAccountID>");
            sb.AppendLine("<PackageStatus>" + PackageStatus + "</PackageStatus>");
            sb.AppendLine("</PackageInfo>");

            foreach (dynamic data in jsonPackageDetail)
            {
                sb.AppendLine("<PackageDetail>");
                //sb.AppendLine("<PackageCode>" + html.EscapeXml(HttpUtility.UrlDecode(PackageCode.Trim())) + data["Bandwidth"] + data["Duration"] + "</PackageCode>");
                sb.AppendLine("<PackageCode>" + html.EscapeXml(HttpUtility.UrlDecode(PackageCode.Trim())) + "</PackageCode>");
                foreach (dynamic item in data)
                {
                    string key = item.Key;
                    string value = HttpUtility.UrlDecode(item.Value.ToString().Trim());
                    sb.AppendLine("<" + key + ">" + html.EscapeXml(value) + "</" + key + ">");
                }
                sb.AppendLine("<ConnectionTypeID>" + ConnectionTypeID + "</ConnectionTypeID>");
                sb.AppendLine("<IsDoubleSpeedAtNight>" + IsDoubleSpeedAtNight + "</IsDoubleSpeedAtNight>");
                sb.AppendLine("<ServiceName>" + ServiceName + "</ServiceName>");
                sb.AppendLine("<PackageName>" + PackageName + "</PackageName>");
                sb.AppendLine("<ServiceID>" + ServiceNameID + "</ServiceID>");
                sb.AppendLine("<SystemID>" + SystemID + "</SystemID>");
                sb.AppendLine("<WarehouseID>" + WarehouseID + "</WarehouseID>");
                sb.AppendLine("<DomainID>" + html.DomainID + "</DomainID>");
                sb.AppendLine("</PackageDetail>");
            }

            sb.AppendLine("</Data>");
            Dictionary<string, string> Para = new Dictionary<string, string>()
            {
                { "Data", sb.ToString() }
            };
            SqlResult sqlresult = html.SaveChangeData("SP_INSERT_PACKAGES", true, Para);
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result
            });
        }

        public ActionResult DeletePackage(int PackageID)
        {
            HTML html = new HTML();
            SqlResult result = new SqlResult();
            html.Detele("delete from dbo.TblPackages where PackageID = @PackageID", "PackageID", PackageID.ToString());
            result = html.Detele("delete from dbo.TblPackageDetail where PackageID = @PackageID", "PackageID", PackageID.ToString());
            return Json(new { Resutl = result.Result, Message = result.Message });
        }
    }
}