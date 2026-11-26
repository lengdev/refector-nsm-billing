using MVC;
using NSMSOLUTION.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Report
{
    [Authorize]
    public class CustomerController : Controller
    {
        // GET: Customer
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Show Customer Connection")]
        public ActionResult Connection()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Customers Connection",
                DialogBoxWidth = "Auto",
                DialogBoxHeight = 600
            };
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Activate Date:",
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                ComboBox = true,
                Label = "Activate Status:",
                DBField = "ConnectivityStatus",
                SQL_CBO = "select StatusID as ConnectivityStatus, Status from dbo.TblStatus where StatusID in (40,41,42,43)",
                DisplayText = "Status",
                Value = "ConnectivityStatus",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType from dbo.TblCustomerType ct where ct.SystemID = " + html.SystemID + " and ct.WarehouseID=" + html.WarehouseID,
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ClassName",
                SQL_CBO = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.ClassName <> 'NSM no VAT' and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtVLan",
                Label = "VLanName:",
                DBField = "VLanName",
                SQL_CBO = "select ac.VLanName from dbo.TblActivateCustomer ac where ac.ConnectionType = 'Static' and ac.VLanName is not null",
                DisplayText = "VLanName",
                Value = "VLanName",
                ComboBox = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtPPPoE",
                Label = "PPPoE:",
                DBField = "PPPoEUserName",
                SQL_CBO = "select ac.PPPoEUserName from dbo.TblActivateCustomer ac where ac.ConnectionType = 'PPPoE' and ac.PPPoEUserName is not null",
                DisplayText = "PPPoEUserName",
                Value = "PPPoEUserName",
                ComboBox = true
            });

            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtConnectionType",
                Label = "Connection Type:",
                DBField = "ConnectionType",
                SQL_CBO = "select distinct ac.ConnectionType from dbo.TblActivateCustomer ac",
                DisplayText = "ConnectionType",
                Value = "ConnectionType",
                ComboBox = true
            });

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CustomerName") + "," +
                EncryAndDecry.Encryp("SID") + "," +
                EncryAndDecry.Encryp("ConnectionID"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select top 10 c.CustomerName from dbo.TblCustomer c where lower(c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " union all select top 10 c.CustomerNameLatin as CustomerName from dbo.TblCustomer c where lower(c.CustomerNameLatin) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CustomerName") + "," +
                EncryAndDecry.Encryp("SID") + "," +
                EncryAndDecry.Encryp("ConnectionID"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "SID",
                SQL = "select distinct top 10 ac.[SID] from dbo.TblActivateCustomer ac where lower(ac.SID) like @SearchText",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CustomerName") + "," +
                EncryAndDecry.Encryp("SID") + "," +
                EncryAndDecry.Encryp("ConnectionID"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Connection ID",
                SQL = "select distinct top 10 ac.ConnectionID from dbo.TblActivateCustomer ac where lower(ac.SID) like @SearchText",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CustomerName") + "," +
                EncryAndDecry.Encryp("SID") + "," +
                EncryAndDecry.Encryp("ConnectionID"),
                ISAutoComplete = true,
            });
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage=true },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage=true },
                new GridField() { Header = "Invoice No.", FieldName = "InvoiceNo", OnClickFunction="PrintInvoice(#=CustomerID#,'#=InvoiceNo#','1','10','',false,this);" },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "ConnectionID", FieldName = "ConnectionID"},
                new GridField() { Header = "Room #", FieldName = "ATTName"},
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", ClassName = "text-center"},
                new GridField() { Header = "Start Date", FieldName = "StartDate", FormatDateTime = "dd-MMM-yyyy" },
                new GridField() { Header = "End Date", FieldName = "EndDate", FormatDateTime = "dd-MMM-yyyy" },
                new GridField() { Header = "Activate Status",
                    FieldName = "ActivateStatus",
                    ClassName = "text-center",
                    DisplayBadge = true,
                    BadgeClassMap = new Dictionary<string, string> {
                        { "Actived", "badge-success" },
                        { "Locked", "badge-danger" },
                        { "Terminate", "badge-danger" },
                        { "Inactive", "badge-warning" },
                    }
                },
                new GridField() { Header = "Activate Date", FieldName = "ActivateDate", FormatDateTime = "dd-MMM-yyyy", ClassName = "text-center"},
                new GridField() { Header = "Activate By", FieldName = "ActivateBy" },
                new GridField() { Header = "id/usernmae", FieldName = "id" },
                new GridField() { Header = "VLan Name/PPPoE", FieldName = "VLanPPPoE" },
                new GridField() { Header = "Connection Type", FieldName = "ConnectionType", ClassName = "text-center" },
                new GridField() { Header = "Moved Status", FieldName = "MovedStatus" },
                new GridField() { Header = "Phone", FieldName = "CustomerMobile" },
                new GridField() { Header = "Block", FieldName = "CustomerType" },
                new GridField() { Header = "Class Name", FieldName = "ClassName" },
                new GridField() { Header = "Register By", FieldName = "RegisterBy"},
                new GridField() { Header = "Package Name", FieldName = "PackageName" },
                new GridField() { Header = "Install Address", FieldName = "InstallAddress" },
            };
            html.GH.GridTable2 = new GridField[]
            {
                new GridField() { Header = "router_name", FieldName = "router_name", ClassName = "text-center" },
                new GridField() { Header = "id/usernmae", FieldName = "id", ClassName = "text-center text-danger"},
                new GridField() { Header = "vlan/pppoe", FieldName = "name", ClassName = "text-center"},
                new GridField() { Header = "disabled", FieldName = "disabled", ClassName = "text-center"},
                new GridField() { Header = "ip_address", FieldName = "ip_address", },
                new GridField() { Header = "port", FieldName = "port", },
                new GridField() { Header = "comment", FieldName = "comment",  },
            };

            var kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_SEARCH_CUSTOMER_CONNECTION"
            };

            var service = new MikrotikService();
            var routersTable = html.LoadData("SELECT * FROM dbo.TblRouter");

            var routerData = new List<object>();

            foreach (DataRow router in routersTable.Tables[0].Rows)
            {
                int routerId = Convert.ToInt32(router["RouterID"]);

                List<object> routerLogs;
                try
                {
                    routerLogs = service.GetVlans(routerId);
                    routerLogs.AddRange(service.GetPPPoEUsers(routerId));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning($"GetVlans failed for router {routerId}: {ex.Message}");
                    continue;
                }

                foreach (var log in routerLogs)
                {
                    var d = log.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(log));
                    bool isUser = d.ContainsKey("password");

                    routerData.Add(new
                    {
                        id = isUser ? d["name"] : d["id"],
                        name = d.ContainsKey("name") ? d["name"] : "",
                        password = isUser ? (d["password"] ?? "") : "",
                        comment = d.ContainsKey("comment") ? d["comment"] : "",
                        disabled = d.ContainsKey("disabled") ? d["disabled"] : "",
                        ip_address = router["IPAddress"],
                        port = router["Port"],
                        router_name = router["RouterName"]
                    });
                }
            }

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(
                1, "", true,
                html.GH.GridTable.ToList().ToJsonString(),
                kendoGridSetting.ToJsonString(),
                html.GH.GridTable2.ToList().ToJsonString(),
                LinkField: "id",
                JsonData: routerData.ToJsonString());
            return View();
        }

        public ActionResult Connections()
        {

            HTML html = new HTML
            {
                DefaultSearch = false,
                //if (html.HasPermission("Add Customer"))
                //	html.AllowAdd = true;
                //if (html.HasPermission("Edit Customer"))
                //	html.AllowEdit = true;
                //if (html.HasPermission("Delete Customer"))
                //	html.AllowDelete = true;
                //html.AddButton = "New Customer";
                //html.AddURL = "AddCustomer/CreateCustomer";
                //html.DialogBoxWidth = "Auto";
                //html.DialogBoxHeight = 600;
                ReportTitle = "List Customers Connection"
            };
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            //html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            //{
            //    DefaultValueDate = false,
            //    DateRangePicker = true,
            //    LabelFromDate = "Register Date:",
            //    DBField = "c.RegisterDate",
            //    FromDateID = "RegisterDate"
            //});
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Start Date:",
                //DBField = "bi.StartDate"
            });
            //string sqlp = "select distinct top 10 (pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM) PackageName from dbo.TblCustomerBillingInvoice bi ";
            //sqlp += "inner join dbo.TblPackageDetail pd on bi.PackageDetailID = pd.PackageDetailID ";
            //sqlp += "where lower((pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM)) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID;
            string sqlp = "select (case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end) PackageName ";
            sqlp += "from dbo.TblCustomerBillingInvoice bi ";
            sqlp += "inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID ";
            sqlp += "inner join dbo.TblPackages p on p.PackageID = bi.PackageID ";
            sqlp += "where bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%' group by (case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end) order by (case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end)";
            html.HtmlAddControls.Add(
                new HtmlAddControls()
                {
                    Hide = true,
                    ID = "txtFreeDuration",
                    ComboBox = true,
                    Label = "FreeDuration:",
                    DBField = "FreeDuration",
                    Value = "FreeDuration",
                    DisplayText = "FreeDuration",
                    SQL_CBO = @"select bi.FreeDuration
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblPackages p on p.PackageID = bi.PackageID
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
where bi.SystemID = " + html.SystemID + @" and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + @"%'
group by bi.FreeDuration
order by bi.FreeDuration"
                });
            HtmlAddControls htmlAddControl = new HtmlAddControls()
            {
                ID = "txtStatus",
                ComboBox = true,
                Label = "Activate Status:",
                //DBField = "bi.ConnectivityStatus",
                DBField = "ConnectivityStatus",
                Value = "ConnectivityStatus",
                DisplayText = "Status",
                SQL_CBO = "select distinct bi.ConnectivityStatus, s.[Status] from dbo.TblActivateCustomer bi inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus where bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%' union all select '43' ConnectivityStatus, 'Inactive' union all select '-1' ConnectivityStatus, 'Expired' [Status]"
            };
            htmlAddControls.Add(htmlAddControl);
            //			htmlAddControl = new HtmlAddControls()
            //			{
            //				ID = "txtPackageName",
            //				Label = "Package Name:",
            //				//DBField = "pd.PackageName",
            //				DBField = "PackageName",
            //				Hide = true,
            ////				SQL_CBO = @"select 
            ////(p.PackageName +' - ' + convert(varchar, pd.Bandwidth) + ' Mpbs - ' + bi.UM) PackageName
            ////from dbo.TblCustomerBillingInvoice bi
            ////inner join dbo.TblPackages p on p.PackageID = bi.PackageID
            ////inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
            ////inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
            ////where bi.SystemID = " + html.SystemID + @" and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + @"%'
            ////group by (p.PackageName +' - ' + convert(varchar, pd.Bandwidth) + ' Mpbs - ' + bi.UM), bi.UM
            ////order by (p.PackageName +' - ' + convert(varchar, pd.Bandwidth) + ' Mpbs - ' + bi.UM), bi.UM",
            ////				ComboBox = true,
            ////				Value = "PackageName",
            ////				DisplayText = "PackageName",
            //			};
            //htmlAddControls.Add(htmlAddControl);
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerType",
                Label = "Block:",
                DBField = "CustomerTypeID",
                SQL_CBO = @"select distinct ct.CustomerTypeID, ct.CustomerType from dbo.TblCustomerType ct
inner join dbo.TblCustomer c on c.CustomerTypeID = ct.CustomerTypeID
inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
where ct.[Status] = 'Enable' and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + @"%'
order by ct.CustomerType",
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            htmlAddControl = new HtmlAddControls()
            {
                ID = "txtCustomerName",
                Label = "Customer Name:",
                //DBField = "c.CustomerCode",
                DBField = "CustomerName",
                SQLAutoComplete = "select distinct top 10 c.CustomerName from dbo.TblCustomerBillingInvoice bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where lower(c.CustomerName) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID=" + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'",
                ISAutoComplete = true
            };
            htmlAddControls.Add(htmlAddControl);
            htmlAddControl = new HtmlAddControls()
            {
                ID = "txtVLanName",
                Label = "VLan Name:",
                DBField = "VLanName",
                SQLAutoComplete = "select top 10 ac.VLanName\r\nfrom dbo.TblActivateCustomer ac\r\nwhere ac.VLanName is not null and lower(ac.VLanName) like @SearchText and ac.SystemID = " + html.SystemID + " and ac.WarehouseID=" + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + "%'",
                ISAutoComplete = true
            };
            htmlAddControls.Add(htmlAddControl);
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'",
                //RadioValue = EncryAndDecry.Encryp("(c.CustomerName + ' [' + c.CustomerCode + ']')") + "," + 
                //EncryAndDecry.Encryp("REPLACE(bf.CompanyContactMobileNumber, ' ','')") + "," +
                //EncryAndDecry.Encryp("bi.[SID]") + "," +
                //EncryAndDecry.Encryp("(bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity)"),
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CompanyContactMobileNumber") + "," +
                EncryAndDecry.Encryp("SID") + "," +
                EncryAndDecry.Encryp("ConnectionID") + "," +
                EncryAndDecry.Encryp("InstallAddress"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Telephone",
                SQL = "select distinct top 10 REPLACE(bi.CompanyContactMobileNumber, ' ','') CompanyContactMobileNumber from dbo.TblCustomerBillInformation bi where REPLACE(bi.CompanyContactMobileNumber, ' ','') like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID=" + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'",
                //RadioValue = EncryAndDecry.Encryp("(c.CustomerName + ' [' + c.CustomerCode + ']')") + "," + 
                //EncryAndDecry.Encryp("REPLACE(bf.CompanyContactMobileNumber, ' ','')") + "," +
                //EncryAndDecry.Encryp("bi.[SID]") + "," +
                //EncryAndDecry.Encryp("(bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity)"),
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CompanyContactMobileNumber") + "," +
                EncryAndDecry.Encryp("SID") + "," +
                EncryAndDecry.Encryp("ConnectionID") + "," +
                EncryAndDecry.Encryp("InstallAddress"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "SID",
                SQL = "select distinct top 10 bi.[SID] from dbo.TblCustomerBillingInvoice bi where lower([SID]) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID=" + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'",
                //RadioValue = EncryAndDecry.Encryp("(c.CustomerName + ' [' + c.CustomerCode + ']')") + "," + 
                //EncryAndDecry.Encryp("REPLACE(bf.CompanyContactMobileNumber, ' ','')") + "," +
                //EncryAndDecry.Encryp("bi.[SID]") + "," +
                //EncryAndDecry.Encryp("(bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity)"),
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CompanyContactMobileNumber") + "," +
                EncryAndDecry.Encryp("SID") + "," +
                EncryAndDecry.Encryp("ConnectionID") + "," +
                EncryAndDecry.Encryp("InstallAddress"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Connection ID",
                SQL = "select distinct top 10 bi.ConnectionID from dbo.TblCustomerBillingInvoice bi where lower(ConnectionID) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID=" + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'",
                //RadioValue = EncryAndDecry.Encryp("(c.CustomerName + ' [' + c.CustomerCode + ']')") + "," + 
                //EncryAndDecry.Encryp("REPLACE(bf.CompanyContactMobileNumber, ' ','')") + "," +
                //EncryAndDecry.Encryp("bi.[SID]") + "," +
                //EncryAndDecry.Encryp("(bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity)"),
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CompanyContactMobileNumber") + "," +
                EncryAndDecry.Encryp("SID") + "," +
                EncryAndDecry.Encryp("ConnectionID") + "," +
                EncryAndDecry.Encryp("InstallAddress"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Install Address",
                SQL = "select distinct top 10 (bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity) InstallAddress from dbo.TblCustomerBillInformation bf where lower(bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity) like @SearchText and bf.SystemID = " + html.SystemID + " and bf.WarehouseID=" + html.WarehouseID + " and bf.DomainID like '%" + html.DomainID + "%'",
                //RadioValue = EncryAndDecry.Encryp("(c.CustomerName + ' [' + c.CustomerCode + ']')") + "," + 
                //EncryAndDecry.Encryp("REPLACE(bf.CompanyContactMobileNumber, ' ','')") + "," +
                //EncryAndDecry.Encryp("bi.[SID]") + "," +
                //EncryAndDecry.Encryp("(bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity)"),
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," +
                EncryAndDecry.Encryp("CompanyContactMobileNumber") + "," +
                EncryAndDecry.Encryp("SID") + "," +
                EncryAndDecry.Encryp("ConnectionID") + "," +
                EncryAndDecry.Encryp("InstallAddress"),
                ISAutoComplete = true,
            });
            //string sql = @" select ROW_NUMBER() OVER(ORDER BY c.CustomerID desc, bi.ConnectionID desc) AS [No], '<a href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');"">' + c.CustomerCode + '</a>' CustomerCode, 
            //c.CustomerName, isnull(bi.[SID], '<span class=""text-red"">Pending</span>') [SID],
            //(bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity) InstallAddress,
            //bi.ConnectionID, '<a href=""javascript:void(0);"" onclick=""ShowDialogBox(''Activation Customer'', ''Auto'', ''500'', ''ActivateCustomer/Activate'', { CID: ''' + c.CustomerCode + ''', CustomerID:'+convert(varchar, bi.CustomerID)+', CustomerName:'''+(c.CustomerName + ' [' + c.CustomerCode + ']')+''', ServiceID: '+convert(varchar, bi.ServiceID) +', ConnectionTypeID:'+convert(varchar, p.PackageTypeID)+',PackageDetailID:' + convert(varchar, bi.PackageDetailID) + ', BillingInvoiceID:'+convert(varchar, bi.BillingInvoiceID)+', IsOnCustProfile:false, ActivateCustomerID: '+convert(varchar, isnull(bi.ActivateCustomerID,0))+', RouterID:'+convert(varchar, isnull(bi.RouterID,0)) + '});"">' + s.[Status] + '</a>' [Status], (case when bi.invTmp = 1 then bi.ServiceName + ' - ' + bi.PackageName else bi.ServiceName end) PackageName,
            //format(c.RegisterDate, 'dd-MM-yyyy') RegisterDate, format(bi.StartDate, 'dd-MM-yyyy') StartDate, 
            //format(bi.EndDate, 'dd-MM-yyyy') EndDate, (c.CustomerName + ' [' + c.CustomerCode + ']') CustName, c.RegisterBy,
            //'<span style=""color:red"">' + convert(varchar, (select count(_bi.ConnectionID) from dbo.TblCustomerBillingInvoice _bi where _bi.CustomerID = bi.CustomerID)) + '</span>' CountConnection
            //from dbo.TblCustomer c
            //inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
            //inner join dbo.TblCustomerBillInformation bf on bf.CustomerBillInformationID = bi.CustomerBillInformationID
            //inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus
            //inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
            //inner join dbo.TblPackages p on p.PackageID = bi.PackageID";

            //html.GH.SQL = sql;
            html.GH.GridProcedureName = "SP_SEARCH_CUSTOMER_CONNECTION";
            //html.OrderBy = "[No]";
            html.isShowLoadingGrid = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Block", FieldName = "CustomerType", IsGroupByField = true, GroupLabel="Block: ", Colspan=7 },
                new GridField() { Header = "CID", FieldName = "CustomerCode", IsGroupByField = true, GroupLabel="CID: ", Colspan=7 },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", IsGroupByField = true, GroupLabel="Name: ", Colspan=7 },
                new GridField() { Header = "Phone", FieldName = "CustomerPhone", IsGroupByField = true, Colspan=7 },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", IsGroupByField = true, GroupLabel="Reg.Date: ", Colspan=7 },
                new GridField() { Header = "Register By", FieldName = "RegisterBy", IsGroupByField = true, GroupLabel="Reg.By: ", Colspan=7 },
                new GridField() { Header = "Count Connection", FieldName = "CountConnection", IsGroupByField = true, GroupLabel="Connection: ", Colspan=7 },
                new GridField() { Header = "Conn.ID", FieldName = "ConnectionID", IsHiddenField = true },
                new GridField() { Header = "Service ID", FieldName = "SID" },
                //new GridField() { Header = "Status", FieldName = "Status", ClassName="text-red", TextAlign="center" },
                new GridField() { Header = "Package Name", FieldName = "PackageName" },
                //new GridField() { Header = "Free", FieldName = "FreeDuration", TextAlign="right" },
                new GridField() { Header = "Date", FieldName = "Date" },
                //new GridField() { Header = "End Date", FieldName = "EndDate", ClassName="text-red" },
                //new GridField() { Header = "Remain Day", FieldName = "RemainDay", ClassName="text-red" },
                new GridField() { Header = "Install Address", FieldName = "InstallAddress" }
            };
            //html.DefaultSearch = false;

            //html.DrawTotalAllHeader = true;
            //html.DrawTotalAllHeaderDataTableNumber = 2;

            html.GridEventClick = "ShowCustomerInfo(this)";
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        [Authorize(Roles = "Show Customer Invoice")]
        public ActionResult Invoice(string Type = "", string StatusID = "All")
        {
            HTML html = new HTML
            {
                ReportTitle = "List Customers Invoice"
            };
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Invoice Date:",
                //DBField = "InvoiceDate",
                //FromDateID = "InvoiceDateDate"
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                ComboBox = true,
                Label = "Status:",
                DBField = "StatusID",
                Value = "StatusID",
                DisplayText = "Status",
                SQL_CBO = "select distinct cb.StatusID, s.[Status] from dbo.TblCustomerBalance cb inner join dbo.TblStatus s on s.StatusID = cb.StatusID where cb.StatusID not in (12, 13,20) and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%' union select 15 StatusID, 'PartiallyPaid' [Status] union select 16 StatusID, 'Over Due Payment' [Status]",
                CboValue = StatusID.ToString(),
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtType",
                Label = "Type:",
                DBField = "Type",
                Value = Type,
                ISAutoComplete = true,
                Hide = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoiceNo",
                Label = "InvoiceNo:",
                DBField = "InvoiceNo",
                SQLAutoComplete = "select distinct top 10 cb.InvoiceNo from dbo.TblCustomerBalance cb where lower(cb.InvoiceNo) like @SearchText and StatusID not in (12, 20) and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%' order by cb.InvoiceNo",
                ISAutoComplete = true
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInvoiceBy",
                Label = "Invoice By:",
                DBField = "CreateBy",
                SQL_CBO = "select distinct lower(CreateBy) CreateBy from dbo.TblCustomerBalance where isnull(CreateBy,'') <> '' and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by lower(CreateBy)",
                DisplayText = "CreateBy",
                Value = "CreateBy",
                ComboBox = true,
            });
            htmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtRepresentative",
                Label = "Representative:",
                DBField = "Representative",
                SQLAutoComplete = "select distinct top 10 e.FullName from dbo.TblCustomerBalance cb inner join dbo.TblLeads e on e.LeadID = cb.EmployeeID where lower(e.FullName) like @SearchText and cb.SystemID = " + html.SystemID + " and cb.WarehouseID=" + html.WarehouseID + " and cb.DomainID like '%" + html.DomainID + "%' order by e.FullName",
                ISAutoComplete = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CompanyContactMobileNumber"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select distinct top 10 (c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName from dbo.TblCustomerBillingInvoice bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where lower((c.CustomerName + ' [' + c.CustomerCode + ']')) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID=" + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CompanyContactMobileNumber"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Telephone",
                SQL = "select distinct top 10 REPLACE(bi.CompanyContactMobileNumber, ' ','') CompanyContactMobileNumber from dbo.TblCustomerBillInformation bi where REPLACE(bi.CompanyContactMobileNumber, ' ','') like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID=" + html.WarehouseID + " and bi.DomainID like '%" + html.DomainID + "%'",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CompanyContactMobileNumber"),
                ISAutoComplete = true,
            });
            html.GridEventClick = "ShowPreviewCustInv(this)";
            //string sql = "select ROW_NUMBER() OVER(ORDER BY cb.CustomerBalanceID desc, cb.InvoiceDate) as [No], c.CustomerID, '<a href=\"javascript:void(0);\" onclick=\"OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, c.CustomerID) + ''');\">' + c.CustomerCode + '</a>' CID, ";
            //sql += "c.CustomerName, ";
            //sql += "('<a class=\"text-dark\" name=\"APreviewInv\" href=\"javascript:void(0);\" onclick=\"PrintInvoice('+convert(varchar, c.CustomerID)+', '''+ cb.InvoiceNo +''','+ convert(varchar, cb.IsIncludedVAT) +','+ convert(varchar, cb.StatusID) +','''',false,this);\">' + cb.InvoiceNo + '</a>') InvoiceNo, ";
            //sql += "cb.InvoiceDate, cb.PaymentDueDate, cb.StartDate, cb.EndDate, cb.DiscountP, cb.DiscountUSD, cb.VAT, cb.GrandTotal, ";
            //sql += "cb.Paid, cb.Unpaid, case when cb.StatusID = 10 then '<span class=\"badge bg-primary\">' + s.[Status] + '</span>' else '<span class=\"badge bg-danger\">' + s.[Status] + '</span>' end [Status], e.Salutation + e.FullName FullName, "; // + ' - Customer: ' + convert(varchar, (select count(_cb.CustomerID) from dbo.TblCustomerBillInformation _cb where _cb.CustomerID= cb.CustomerID and _cb.SystemID = cb.SystemID and _cb.WarehouseID = cb.WarehouseID)) EmployeeName
            //sql +="(c.CustomerName + ' [' + c.CustomerCode + ']') CustName, bf.CompanyContactMobileNumber Phone, ";
            //sql += "'<a href=\"javascript:void(0);\" onclick=\"NewInvoice('+convert(varchar, cb.CustomerID)+');\">New Invoice</a>' NewInvoice ";
            //sql += "from dbo.TblCustomerBalance cb ";
            //sql += "inner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID ";
            //sql += "inner join dbo.TblStatus s on s.StatusID = cb.StatusID ";
            //sql += "inner join dbo.TblLeads e on e.LeadID = cb.EmployeeID ";
            //sql += "inner join dbo.TblCustomerBillInformation bf on bf.CustomerID = cb.CustomerID and bf.InvoiceNo = cb.InvoiceNo ";
            //sql += "where cb.StatusID not in (12, 20) and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID;
            //if (Type == "OfficialInvoice")
            //{
            //    sql += " and cb.[Type]='Official Invoice'";
            //}
            //sql += "; select 0 [No], count(distinct cb.CustomerID) Customer, ";
            //sql += "sum(cb.Paid) Paid, sum(cb.Unpaid) Unpaid ";
            //sql += "from dbo.TblCustomerBalance cb ";
            //sql += "inner join dbo.TblCustomer c on c.CustomerID = cb.CustomerID ";
            //sql += "inner join dbo.TblStatus s on s.StatusID = cb.StatusID ";
            //sql += "inner join dbo.TblLeads e on e.LeadID = cb.EmployeeID ";
            //sql += "inner join dbo.TblCustomerBillInformation bf on bf.CustomerID = cb.CustomerID and bf.InvoiceNo = cb.InvoiceNo ";
            //sql += "where cb.StatusID not in (12, 20) and cb.SystemID = " + html.SystemID + " and cb.WarehouseID = " + html.WarehouseID;

            //html.IsHaveWhereCondition = true;
            //if (Type == "OfficialInvoice")
            //{
            //    sql += " and cb.[Type]='Official Invoice'";
            //}

            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 2;
            html.GH.GridProcedureName = "SP_SHOW_CUSTOMER_INVOICE";
            //html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.isShowLoadingGrid = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CID", IsGroupByField = true, GroupLabel = "CID: ", Colspan = 14 },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", IsGroupByField = true, GroupLabel = "Cust.Name: ", Colspan = 14 },
                new GridField() { Header = "Phone", FieldName = "Phone", IsGroupByField = true, GroupLabel = "Phone: ", Colspan = 14 },
                new GridField() { Header = "Representative", FieldName = "FullName", IsGroupByField= true, GroupLabel="Representative: ", Colspan = 14 },
                new GridField() { Header = "NewInvoice", FieldName = "NewInvoice", IsGroupByField= true, Colspan = 14 },
                new GridField() { Header = "RecordPayment", FieldName = "RecordPayment", IsGroupByField= true, Colspan = 14 },
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo" },
                new GridField() { Header = "Date", FieldName = "InvoiceDate", Format="dd-MM-yyyy", TextAlign="center" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", Format="dd-MM-yyyy", TextAlign="center" },
                new GridField() { Header = "Start Date", FieldName = "StartDate", Format="dd-MM-yyyy", TextAlign="center" },
                new GridField() { Header = "End Date", FieldName = "EndDate", Format="dd-MM-yyyy", TextAlign="center", ClassName="text-red" },
                new GridField() { Header = "Dis.%", FieldName = "DiscountP", TextAlign="right" },
                new GridField() { Header = "Dis.$", FieldName = "DiscountUSD", TextAlign="right" },
                new GridField() { Header = "VAT", FieldName = "VAT", TextAlign="right" },
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", TextAlign="right" },
                new GridField() { Header = "Paid", FieldName = "Paid", TextAlign="right", ClassName="text-primary" },
                new GridField() { Header = "Unpaid", FieldName = "Unpaid", TextAlign="right", ClassName="text-red" },
                new GridField() { Header = "Invoice By", FieldName = "CreateBy", TextAlign="center", ClassName="font-weight-bold" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center",
                    DataTableNumber = 3,
                    DisplayFields = "GrandTotal, Paid, Unpaid",
                    LinkField = "CustomerID",
                    Colspan = 14}

            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        [Authorize(Roles = "Show Customer Payment")]
        public ActionResult Payment()
        {
            return View();
        }

        [HttpGet]
        public JsonResult LoadPayment(string StartDate, string EndDate, string StatusID = "")
        {
            HTML html = new HTML
            {
                TblID = "tblCustPayment",
                //html.TableIDToExcel = "tblCustPayment";
                ReportTitle = "List Customers Payment"
            };
            string sql = @"select 
            sum(InvoiceAmount) InvoiceAmount,
            sum(PaidAmount) PaidAmount,
            ((sum(InvoiceAmount) - sum(PaidAmount)) -sum(PartiallyPaid)) UnpaidAmount,
            sum(PartiallyPaid) PartiallyPaid,
            sum(AdvancePaymentAmount) AdvancePaymentAmount,
            sum(InvoiceCancelledAmount) InvoiceCancelledAmount,
            count(distinct CustomerID) Customer
            from dbo.TblCustomerAccount where format(InvoiceDate,'yyyy/MM/dd') between '" + StartDate + "' and '" + EndDate + "' and Type='Official Invoice' and SystemID = @SystemID and WarehouseID = @WarehouseID and DomainID like '%'+@DomainID+'%' and StatusID in (10, 11)";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            DataSet ds = html.LoadData(sql, false, para);

            html.GridEventClick = "Show_Preview_CustInv(this)";
            html.DrawTotalAllHeader = true;
            html.DrawTotalAllHeaderDataTableNumber = 2;
            html.GH.GridProcedureName = "SP_SHOW_CUSTOMER_INVOICE";

            html.GH.GridPara = new GridPara[] {
                new GridPara{ ParaName="FromDate", Value=StartDate},
                new GridPara{ ParaName="ToDate", Value=EndDate},
                new GridPara{ ParaName="Type", Value="OfficialInvoice"},
                new GridPara{ ParaName="SystemID", Value=html.SystemID},
                new GridPara{ ParaName="WarehouseID", Value=html.WarehouseID},
                new GridPara{ ParaName="StatusID", Value=StatusID},
            };
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.isShowLoadingGrid = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CID", IsGroupByField = true, GroupLabel = "CID: ", Colspan = 13 },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", IsGroupByField = true, GroupLabel = "Cust.Name: ", Colspan = 13 },
                new GridField() { Header = "Phone", FieldName = "Phone", IsGroupByField = true, GroupLabel = "Phone: ", Colspan = 13 },
                new GridField() { Header = "Representative", FieldName = "FullName", IsGroupByField= true, GroupLabel="Representative: ", Colspan = 13 },
                new GridField() { Header = "NewInvoice", FieldName = "NewInvoice", IsGroupByField= true, Colspan = 13 },
                new GridField() { Header = "Invoice No", FieldName = "InvoiceNo" },
                new GridField() { Header = "Date", FieldName = "InvoiceDate", Format="dd-MM-yyyy", TextAlign="center" },
                new GridField() { Header = "Due Date", FieldName = "PaymentDueDate", Format="dd-MM-yyyy", TextAlign="center" },
                new GridField() { Header = "Start Date", FieldName = "StartDate", Format="dd-MM-yyyy", TextAlign="center", ClassName="text-primary" },
                new GridField() { Header = "End Date", FieldName = "EndDate", Format="dd-MM-yyyy", TextAlign="center", ClassName="text-red" },
                new GridField() { Header = "Dis.%", FieldName = "DiscountP", TextAlign="right" },
                new GridField() { Header = "Dis.$", FieldName = "DiscountUSD", TextAlign="right" },
                new GridField() { Header = "VAT", FieldName = "VAT", TextAlign="right" },
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", TextAlign="right" },
                new GridField() { Header = "Paid", FieldName = "Paid", TextAlign="right", ClassName="text-primary" },
                new GridField() { Header = "Unpaid", FieldName = "Unpaid", TextAlign="right", ClassName="text-red" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center", ClassName="text-info",
                    DataTableNumber = 3,
                    DisplayFields = "GrandTotal, Paid, Unpaid",
                    LinkField = "CustomerID",
                    Colspan = 13 }

            };
            string GData = html.DrawHTMLSearchForm();

            return Json(new { Data = html.JsonConvertDataTable(ds.Tables[0]), GData = GData }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ConnectionType()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Customers Connection Type"
            };
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Start Date:",
                DBField = "ac.[Date]"
            });
            string sql = @"select distinct s.StatusID, s.[Status]
            from dbo.TblActivateCustomer ac
            inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
            where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + "%'";
            HtmlAddControls htmlAddControl = new HtmlAddControls()
            {
                ID = "txtConnectionType",
                ComboBox = true,
                Label = "Connection Type:",
                DBField = "ac.ConnectionType",
                Value = "ConnectionType",
                DisplayText = "ConnectionType",
                SQL_CBO = "select distinct ConnectionType from dbo.TblActivateCustomer where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'"
            };
            htmlAddControls.Add(htmlAddControl);
            htmlAddControl = new HtmlAddControls()
            {
                ID = "txtStatus",
                ComboBox = true,
                Label = "Activate Status:",
                DBField = "ac.ConnectivityStatus",
                Value = "StatusID",
                DisplayText = "Status",
                SQL_CBO = sql
            };
            htmlAddControls.Add(htmlAddControl);
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCustomerTypeID",
                Label = "Block:",
                DBField = "c.CustomerTypeID",
                SQL_CBO = "select distinct ct.CustomerTypeID, ct.CustomerType\r\nfrom dbo.TblCustomer c inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID inner join dbo.TblCustomerInvoice ci on ci.CustomerID = c.CustomerID where ci.SystemID = " + html.SystemID + " and ci.WarehouseID=" + html.WarehouseID + " and ci.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "CustomerType",
                Value = "CustomerTypeID",
                ComboBox = true
            });
            //htmlAddControl = new HtmlAddControls()
            //{
            //	ID = "txtPackageName",
            //	Label = "Package Name:",
            //	DBField = "PackageName",
            //	Hide = true
            //	//SQL_CBO = "select distinct ac.PackageDetailID, " +
            //	//"pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + bi.UM PackageName " +
            //	//"from dbo.TblActivateCustomer ac " +
            //	//"inner join dbo.TblCustomerBillingInvoice bi on bi.PackageDetailID = ac.PackageDetailID " +
            //	//"inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID " +
            //	//"where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID + " and ac.DomainID like '%"+html.DomainID+"%'",
            //	//ComboBox = true,
            //	//Value = "PackageName",
            //	//DisplayText = "PackageName",
            //};
            //htmlAddControls.Add(htmlAddControl);
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ci.ClassNameID",
                SQL_CBO = "select distinct cc.ClassID ClassNameID, cc.ClassName from dbo.TblClass cc where cc.ClassID in (5, 6) and cc.SystemID = " + html.SystemID + " and cc.WarehouseID=" + html.WarehouseID + " and cc.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "ClassName",
                Value = "ClassNameID",
                ComboBox = true
            });
            htmlAddControl = new HtmlAddControls()
            {
                ID = "txtCustomerName",
                Label = "Customer Name:",
                DBField = "c.CustomerName",
                SQLAutoComplete = "select distinct top 10 c.CustomerName " +
                "from dbo.TblActivateCustomer ac inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID " +
                "where lower(c.CustomerName) like @SearchText and ac.SystemID = " + html.SystemID + " and ac.WarehouseID=" + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + "%'",
                ISAutoComplete = true
            };
            htmlAddControls.Add(htmlAddControl);
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select distinct top 10 c.CustomerCode from dbo.TblActivateCustomer ac " +
                "inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID " +
                "where lower(c.CustomerCode) like @SearchText and ac.SystemID = " + html.SystemID + " and ac.WarehouseID=" + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + "%'",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," +
                EncryAndDecry.Encryp("ac.SID") + "," +
                EncryAndDecry.Encryp("ac.ConnectionID"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "SID",
                SQL = "select distinct top 10 ac.[SID] from dbo.TblActivateCustomer ac where lower(ac.[SID]) like @SearchText and ac.SystemID = " + html.SystemID + " and ac.WarehouseID=" + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + "%'",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," +
                EncryAndDecry.Encryp("ac.SID") + "," +
                EncryAndDecry.Encryp("ac.ConnectionID"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Connection ID",
                SQL = "select distinct top 10 ac.ConnectionID from dbo.TblActivateCustomer ac where lower(ac.ConnectionID) like @SearchText and ac.SystemID = " + html.SystemID + " and ac.WarehouseID=" + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + "%'",
                RadioValue = EncryAndDecry.Encryp("c.CustomerCode") + "," +
                EncryAndDecry.Encryp("ac.SID") + "," +
                EncryAndDecry.Encryp("ac.ConnectionID"),
                ISAutoComplete = true,
            });
            html.GridEventClick = "onGridClickShowCustProfile(this);";
            //html.GH.GridProcedureName = "SP_SEARCH_CUSTOMER_CONNECTION_TYPE";
            sql = @"select row_number() over(order by ct.CustomerType + ' - ' + ct.DistrictCode, ac.ConnectionType) as [No],
ac.CustomerID, ct.CustomerType + ' - ' + ct.DistrictCode [Block], c.CustomerCode,
case when  isnull((STUFF((select distinct ', ' + ci.CustomerCode from dbo.TblCustomerInvoice ci where ci.CustomerID = c.CustomerID and c.CustomerCode <> ci.CustomerCode and ci.[Type] = 'Official Invoice'
 FOR XML PATH('')), 1, 1, '')), '') <> '' then '<span class=""text-danger"">'+ (STUFF((select distinct ', ' + ci.CustomerCode from dbo.TblCustomerInvoice ci where ci.CustomerID = c.CustomerID and c.CustomerCode <> ci.CustomerCode
 FOR XML PATH('')), 1, 1, '')) + '</span> Moved To CID: ' + c.CustomerCode else '' end CustomerOldCID,
case when isnull(c.CustomerName, '') <> '' then c.CustomerName else c.CustomerNameLatin end CustomerName, 
ac.ConnectionType,
format(ac.ActivateDate, 'dd-MMM-yyyy') ActivateDate, format(ac.StartDate, 'dd-MMM-yyyy') StartDate, format(ac.EndDate, 'dd-MMM-yyyy') EndDate, 
pd.PackageName + ' - ' + convert(varchar, ac.Bandwidth) + ' Mbps' PackageName,
case when DATEDIFF(d, getdate(), ac.EndDate) >=0 then  convert(varchar, DATEDIFF(d, getdate(), ac.EndDate)) + ' days' else 'Expired' end RemainDay,
case when s.[Status] = 'Actived' then '<span class=""""badge badge-primary"""">' + s.[Status] + '</span>' else '<span class=""""badge badge-danger"""">' + s.[Status] + '</span>' end [Status], 
ac.[SID], ac.ConnectionID, c.RegisterDate, case when ac.ConnectionName is null then ac.PPPoEUserName else ac.ConnectionName end ConnectionName, cl.ClassName, ci.ClassNameID
from dbo.TblActivateCustomer ac
inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
inner join dbo.TblStatus s on s.StatusID = ac.ConnectivityStatus
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID
inner join dbo.TblCustomerBillingInvoice ci on ci.CustomerID = ac.CustomerID and ci.StatusID not in (12,13,14,20)
left join dbo.TblClass cl on cl.ClassID = ci.ClassNameID
where ac.SystemID = " + html.SystemID + " and ac.WarehouseID= " + html.WarehouseID + " and ac.DomainID like '%" + html.DomainID + @"%'

group by ac.CustomerID, ct.CustomerType, ct.DistrictCode, s.[Status], c.CustomerCode, c.CustomerID, c.CustomerName, c.CustomerNameLatin, cl.ClassName, ci.ClassNameID,
ac.ConnectionType, ac.ActivateDate, ac.StartDate, ac.EndDate, pd.PackageName, ac.Bandwidth, ac.[SID],
ac.ConnectionID, c.RegisterDate, ac.ConnectivityStatus, c.CustomerTypeID, ac.[Date],
case when ac.ConnectionName is null then ac.PPPoEUserName else ac.ConnectionName end";
            html.OrderBy = "[No]";
            html.IsUseHavingCondition = true;
            html.GH.SQL = sql;
            //html.IsHaveWhereCondition = true;
            html.isShowLoadingGrid = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
				//new GridField() { Header = "CustomerID", FieldName = "CustomerID", IsHiddenField = true },
				new GridField() { Header = "Block", FieldName = "Block", IsGroupByField = true, },
                new GridField() { Header = "Connection Type", FieldName = "ConnectionType", IsGroupByField = true, },// GroupLabel="Connection Type: ", Colspan = 11, DisplayOneRow = true },
				//new GridField() { Header = "Package Name", FieldName = "PackageName", IsGroupByField = true, Colspan = 10 },
				new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID" },
                new GridField() { Header = "Status", FieldName = "CustomerOldCID", },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID" },
                new GridField() { Header = "Class Name", FieldName = "ClassName", TextAlign = "center" },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", FormatDateTime = "dd-MMM-yyyy", TextAlign = "center" },
                new GridField() { Header = "Connection Name", FieldName = "ConnectionName" },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Activate Date", FieldName = "ActivateDate", TextAlign="center" },
                new GridField() { Header = "Start Date", FieldName = "StartDate", TextAlign="center" },
                new GridField() { Header = "End Date", FieldName = "EndDate", TextAlign="center" },
                new GridField() { Header = "Package Name", FieldName = "PackageName" },
                new GridField() { Header = "Remain Day", FieldName = "RemainDay", TextAlign="right" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center" }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
    }
}