using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class ReportChangePackageController : Controller
    {
        public ActionResult Index()
        {
            HTML html = new HTML();
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoice Date:",
                DateTimePickerWithRanges = true,
            });

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 CustomerName from dbo.TblCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " union all select top 10 CustomerNameLatin from dbo.TblCustomer where lower(CustomerNameLatin) like @SearchText and SystemID = " + html.SystemID + "",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
            });

            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerGroup",
                Label = "Customer Group:",
                DBField = "CustomerGroup",
                SQL_CBO = "select distinct cg.CustomerGroup from dbo.TblCustomerGroup cg inner join dbo.TblCustomer c on c.CustomerGroupID = cg.CustomerGroupID inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID where ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID + " and ci.DomainID like '%" + html.DomainID + "%' order by cg.CustomerGroup",
                DisplayText = "CustomerGroup",
                Value = "CustomerGroup",
                ComboBox = true
            });

            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ClassName",
                SQL_CBO = "select distinct ci.ClassName from dbo.TblCustomerInvoice ci where ci.IsChangePlan = 1 and ci.SystemID = " + html.SystemID + " and ci.WarehouseID = " + html.WarehouseID + " and ci.DomainID like '%" + html.DomainID + "%' order by ci.ClassName",
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });


            html.GH.GridTable = new GridField[]
               {
                new GridField() { Header = "No", FieldName = "No", TextAlign = "right", },
                new GridField() { Header = "Customer Code", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage=true},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", ClassName="text-uppercase", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage=true},
                //new GridField() { Header = "Identification", FieldName = "CustomerIdentification" },
                //new GridField() { Header = "Customer Group", FieldName = "CustomerGroup", ClassName="text-center" },
                //new GridField() { Header = "Class", FieldName = "ClassName", ClassName="text-center" },
                new GridField() { Header = "Block", FieldName = "CustomerType", ClassName="text-center"},

                new GridField() { Header = "Old Invoice No", FieldName = "OldInvoiceNo", ClassName="text-center" },
                new GridField() { Header = "Old Date", FieldName = "OldInvoiceDate", ClassName="text-center" },
                new GridField() { Header = "Old Package", FieldName = "OldPackageName" },
                new GridField() { Header = "Old Feature", FieldName = "OldFeature",},
                //new GridField() { Header = "Old Package Type", FieldName = "OldPackageType", ClassName="text-center" },
                //new GridField() { Header = "Old Package Code", FieldName = "OldPackageCode" },
                //new GridField() { Header = "Old Bandwidth", FieldName = "OldBandwidth", ClassName="text-center" },
                //new GridField() { Header = "Old Amount", FieldName = "OldAmount", ClassName="text-right" },

                new GridField() { Header = "New Invoice No", FieldName = "NewInvoiceNo", ClassName="text-center" },
                new GridField() { Header = "New Date", FieldName = "NewInvoiceDate", ClassName="text-center" },
                new GridField() { Header = "New Package", FieldName = "NewPackageName" },
                new GridField() { Header = "New Feature", FieldName = "NewFeature" },
                //new GridField() { Header = "New Package Type", FieldName = "NewPackageType", ClassName="text-center" },
                //new GridField() { Header = "New Package Code", FieldName = "NewPackageCode" },
                //new GridField() { Header = "New Bandwidth", FieldName = "NewBandwidth", ClassName="text-center" },
                //new GridField() { Header = "New Amount", FieldName = "NewAmount", ClassName="text-right" },

                new GridField() { Header = "Changed By", FieldName = "ChangedBy", ClassName="text-center" },
                               //new GridField() { Header = "Remark", FieldName = "Comment" },
                };

            html.ShowPaging = false;

            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                //filterable = false,
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_REPORT_CUSTOMER_PACKAGE_CHANGE",
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
    }
}