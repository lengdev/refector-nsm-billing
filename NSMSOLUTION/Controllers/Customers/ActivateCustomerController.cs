using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using tik4net;
using Utilities;

namespace NSMSOLUTION.Controllers
{
    [Authorize]
    public class ActivateCustomerController : Controller
    {
        //ActivateCustomer
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Activate Customer"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Activate Customer"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Activate Customer"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.IsUseHavingCondition = true;
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Register Date:",
                DBField = "c.RegisterDate"
            });
            //string sqlp = "select distinct top 10 (pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM) PackageName from dbo.TblCustomerBillingInvoice bi ";
            //sqlp += "inner join dbo.TblPackageDetail pd on bi.PackageDetailID = pd.PackageDetailID ";
            //sqlp += "where lower((pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM)) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID;
            string sqlp = "select distinct top 10 pd.PackageName from dbo.TblCustomerBillingInvoice bi ";
            sqlp += "inner join dbo.TblPackageDetail pd on bi.PackageDetailID = pd.PackageDetailID ";
            sqlp += "where lower(pd.PackageName) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID;
            HtmlAddControls htmlAddControl = new HtmlAddControls()
            {
                ID = "txtStatus",
                ComboBox = true,
                Label = "Activate Status:",
                DBField = "bi.ConnectivityStatus",
                Value = "ConnectivityStatus",
                DisplayText = "Status",
                SQL_CBO = "select distinct bi.ConnectivityStatus, s.[Status] from dbo.TblCustomerBillingInvoice bi inner join dbo.TblStatus s on s.StatusID = bi.ConnectivityStatus where bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID
            };
            htmlAddControls.Add(htmlAddControl);
            htmlAddControl = new HtmlAddControls()
            {
                ID = "txtPackageName",
                Label = "Package Name:",
                DBField = "bi.PackageDetailID",
                SQL_CBO = "select distinct ac.PackageDetailID, " +
                "pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + bi.UM PackageName " +
                "from dbo.TblActivateCustomer ac " +
                "inner join dbo.TblCustomerBillingInvoice bi on bi.PackageDetailID = ac.PackageDetailID " +
                "inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ac.PackageDetailID " +
                "where ac.SystemID = " + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID,
                ComboBox = true,
                Value = "PackageDetailID",
                DisplayText = "PackageName",
            };
            htmlAddControls.Add(htmlAddControl);
            htmlAddControl = new HtmlAddControls()
            {
                ID = "txtCID",
                Label = "CID:",
                DBField = "c.CustomerCode",
                SQLAutoComplete = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerCode",
                ISAutoComplete = true
            };
            htmlAddControls.Add(htmlAddControl);
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                SQL = "select distinct top 10 (c.CustomerName + ' [' + c.CustomerCode + ']') CustomerName from dbo.TblCustomerBillingInvoice bi inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID where lower((c.CustomerName + ' [' + c.CustomerCode + ']')) like @SearchText and bi.SystemID = " + html.SystemID + " and bi.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("(c.CustomerName + ' [' + c.CustomerCode + ']')"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            string sql = "select ROW_NUMBER() OVER(ORDER BY s.ServiceName, (p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM + ' Free ' + convert(varchar, pd.FreeDuration) + ' ' + pd.UM), bi.CustomerID desc, bi.InvoiceDate desc) AS[No], bi.CustomerID, ";
            sql += "'<a href=\"javascript:void(0);\" onclick=\"OpenNewTap(''#!/CustomerProfile?ID='+ convert(varchar, bi.CustomerID) +''');\">' + (c.CustomerName + ' [' + c.CustomerCode + ']') + '</a>' CustomerName, ";
            sql += "(case when c.RegisterDate > DATEADD(DAY,-1,GETDATE()) then '<span style=\"color:red\">New Register</span>' else format(c.RegisterDate, 'dd/MM/yyyy') end) RegisterDate, s.ServiceName, (p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM + ' Free ' + convert(varchar, pd.FreeDuration) + ' ' + pd.UM) ";
            sql += "+ ' Customer: <span style=\"color:red\">' + convert(varchar, (select count(bi_.CustomerID) from dbo.TblPackageDetail tp_ inner join dbo.TblCustomerBillingInvoice bi_ on bi_.PackageDetailID = tp_.PackageDetailID where tp_.PackageDetailID = bi.PackageDetailID )) +'</span>' PackageName, ";
            sql += "pd.FreeDuration, bi.InvoiceDate, bi.EndDate, bi.Amount, st.[Status], c.RegisterBy ";
            sql += ", (case when format(bi.EndDate, 'yyyyMMdd') >= format(GETDATE(),'yyyyMMdd') then case when bi.ConnectivityStatus = 43 then '<a href=\"javascript:void(0);\" onclick=\"ShowDialogBox(''Activation Customer'', ''Auto'', ''500'', ''" + html.ServerRoot + "ActivateCustomer/Activate'', { CID: ''' + c.CustomerCode + ''', CustomerID:'+convert(varchar, bi.CustomerID)+', CustomerName:'''+(c.CustomerName + ' [' + c.CustomerCode + ']')+''', ServiceID: '+convert(varchar, bi.ServiceID) +', ConnectionTypeID:'+convert(varchar, p.PackageTypeID)+',PackageDetailID:' + convert(varchar, bi.PackageDetailID) + ', BillingInvoiceID:'+convert(varchar, bi.BillingInvoiceID)+', IsOnCustProfile:false, ActivateCustomerID: '+convert(varchar, bi.ActivateCustomerID)+'});\">Activate Now</a>' else '<span style=\"color:red;\" class=\"\"><b>' + sb.[Status] + '</span>' end else '<span style=\"color:red;\"><b>Expired</b></span>' end) ActivateNow ";
            sql += ",(p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM + ' Free ' + convert(varchar, pd.FreeDuration) + ' ' + pd.UM) PackageNames ";
            sql += "from dbo.TblCustomerBillingInvoice bi ";
            sql += "inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID ";
            sql += "inner join dbo.TblServices s on s.ServiceID = bi.ServiceID ";
            sql += "inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID ";
            sql += "inner join dbo.TblPackages p on p.PackageID = bi.PackageID ";
            sql += "inner join dbo.TblStatus st on st.StatusID = bi.StatusID ";
            sql += "inner join dbo.TblStatus sb on sb.StatusID = bi.ConnectivityStatus ";
            sql += "where bi.StatusID <> 20 and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID;
            sql += "group by bi.PackageDetailID, pd.PackageName, bi.CustomerID, (c.CustomerName + ' [' + c.CustomerCode + ']'), c.CustomerCode, c.RegisterDate, s.ServiceName, (p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM + ' Free ' + convert(varchar, pd.FreeDuration) + ' ' + pd.UM), pd.FreeDuration, bi.InvoiceDate, bi.EndDate, bi.Amount, sb.[Status], st.[Status], c.RegisterBy, p.PackageTypeID, bi.ServiceID, bi.ConnectivityStatus, bi.ConnectivityStatusName, bi.BillingInvoiceID, bi.ActivateCustomerID";
            html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerName", FieldName = "CustomerName" },
                new GridField() { Header = "RegisterDate", FieldName = "RegisterDate", TextAlign="center" },
                new GridField() { Header = "RegisterBy", FieldName = "RegisterBy" },
                new GridField() { Header = "ServiceName", FieldName = "ServiceName", IsGroupByField = true, Colspan=9 },
                new GridField() { Header = "PackageName", FieldName = "PackageName", IsGroupByField = true, Colspan=9 },
                new GridField() { Header = "InvoiceDate", FieldName = "InvoiceDate", Format="dd/MM/yyyy", TextAlign="center" },
                new GridField() { Header = "EndDate", FieldName = "EndDate", Format="dd/MM/yyyy", TextAlign="center", Style="color:red;" },
                new GridField() { Header = "Amount", FieldName = "Amount", Format="#,#.00", TextAlign = "right" },
                new GridField() { Header = "Status", FieldName = "Status", TextAlign="center", Style="color:red;" },
                new GridField() { Header = "Activate Now", FieldName = "ActivateNow", TextAlign="center" }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }

        [Authorize(Roles = "Add Activate Customer, Edit Activate Customer, Lock And Reactive Customer Service")]
        public ActionResult Activate(object[] PrimaryFieldValue)
        {
            ViewBag.DataInfo = PrimaryFieldValue[0];
            //Allow Create PPPoE User
            bool roleManager = HttpContext.GetOwinContext().Request.User.IsInRole("Allow Create PPPoE User");
            ViewBag.AllowCreatePPPoEUser = roleManager;
            Dictionary<string, string> rateDict = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(PrimaryFieldValue[0].ToString());
            string BillingInvoiceID = rateDict["BillingInvoiceID"].ToString();
            ViewBag.ConnectionName = "";
            if (rateDict.ContainsKey("ConnectionName"))
                ViewBag.ConnectionName = rateDict["ConnectionName"].ToString();
            string ActivateCustomerID = rateDict["ActivateCustomerID"].ToString() == "" ? "0" : rateDict["ActivateCustomerID"].ToString();
            string InvoiceNo = rateDict["InvoiceNo"].ToString();
            HTML html = new HTML
            {
                FormID = "ActivateCustomerForm",
                FieldIDFocus = "ProfileName",
                TableName = "TblActivateCustomer",
                PrimaryField = "ActivateCustomerID",
                UseTable = true,
                TableColumn = 2,
                DrawBtnSaveChangeFrmV2 = false
            };
            //html.SaveChangeFunction = "SaveChangeCustActivation()";
            string sql = @"select distinct isnull(bi.IsAlreadyApplyProfile,0) IsAlreadyApplyProfile, isnull(bi.IsChangePlan,0) IsChangePlan, bi.CustomerID, c.CustomerCode + '-' + case when isnull(c.CustomerName, '') <> '' then c.CustomerName else c.CustomerNameLatin end CID, c.CustomerCode, c.CustomerName, s.ServiceName, pt.PackageTypeName ConnectionType,
(pd.PackageName + ' - ' + convert(varchar, pd.Bandwidth) +' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM) PackageName,
bi.UM + case when bi.FreeDuration <> 0 then ' Free Duration: ' + convert(varchar, bi.FreeDuration) else '' end Duration, bi.FreeDuration, convert(varchar, pd.Bandwidth) + ' Mbps - Down: ' + convert(varchar, pd.DowSpeed) + ' Mbps - Up: ' +  convert(varchar, pd.UpSpeed) + ' Mbps' Bandwidth, format(bi.StartDate, 'dd-MM-yyyy') StartDate, 
format(bi.EndDate, 'dd-MM-yyyy') EndDate, format(bi.NextInvoiceDate, 'dd-MM-yyyy') NextInvoiceDate,
format(c.RegisterDate, 'dd-MM-yyyy') RegisterDate, c.RegisterBy, bi.InvoiceNo, bi.ConnectionID,
(i.InstallAddress + case when i.InstallCommune <> '' then ', ' + i.InstallCommune else '' end + 
case when i.InstallDistrict <> '' then ', ' + i.InstallDistrict else '' end +  
case when i.InstallProvinceCity <> '' then ', ' + i.InstallProvinceCity else '' end) InstallLocation,
i.CompanyContactMobileNumber PhoneNumber, i.GoogleMap_FullAddress, i.GoogleMap_Latitude, i.GoogleMap_Longitude, 
isnull(ac.[SID], 0) [SID], ac.IPAddress, ct.CustomerType CustomerBlock,
ac.PPPoEUserName, ac.PPPoEPassword, isnull(bi.ActivateCustomerID, 0) ActivateCustomerID, ac.RouterID, ac.ConnectionType PPPoEConnectionType, 
ac.ConnectivityStatus, ac.ActivateBy, ac.ConnectionID, 
format(ac.ActivateDate, 'dd-MM-yyyy HH:mm:ss tt') ActivateDate, ac.VLanID,
ac.LockedBy, format(ac.LockedDate, 'dd-MM-yyyy HH:mm:ss tt') LockedDate,
ac.DeviceName, ac.WifiIP, ac.WifiName, ac.WifiKey, ac.WifiLoginName, WifiLoginPassword, 
bi.TicketNumber, isnull(ac.ConnectionName, '') ConnectionName, pd.Bandwidth Speed
from dbo.TblCustomerBillingInvoice bi
inner join dbo.TblCustomer c on c.CustomerID = bi.CustomerID
inner join dbo.TblServices s on s.ServiceID = bi.ServiceID            
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
inner join dbo.TblPackages p on p.PackageID = pd.PackageID
inner join dbo.TblPackageType pt on pt.PackageTypeID = p.PackageTypeID
inner join dbo.TblCustomerBillInformation i on i.CustomerID = bi.CustomerID and i.InvoiceNo = bi.InvoiceNo
inner join dbo.TblCustomerType ct on ct.CustomerTypeID = c.CustomerTypeID
";
            if (ActivateCustomerID == "0")
                sql += @" left join dbo.TblActivateCustomer ac on ac.ActivateCustomerID = bi.ActivateCustomerID";
            else
                sql += " inner join dbo.TblActivateCustomer ac on ac.InvoiceNo = bi.InvoiceNo and ac.CustomerID = bi.CustomerID";
            //sql += @" where bi.InvoiceNo = '" + InvoiceNo + "' and  case when " + ActivateCustomerID + " = 0 then bi.BillingInvoiceID else ac.ActivateCustomerID end = " + (int.Parse(ActivateCustomerID) == 0 ? BillingInvoiceID : ActivateCustomerID);
            sql += @" where case when " + ActivateCustomerID + " = 0 then bi.BillingInvoiceID else ac.ActivateCustomerID end = " + (int.Parse(ActivateCustomerID) == 0 ? BillingInvoiceID : ActivateCustomerID);
            DataSet ds = html.LoadData(sql);
            ViewBag.CustData = html.JsonConvertDataTable(ds.Tables[0]);
            dynamic CustDatas = new JavaScriptSerializer().DeserializeObject(ViewBag.CustData);
            //InvoiceNo = CustDatas[0]["InvoiceNo"].ToString();
            DataRow dr = ds.Tables[0].Rows[0];
            string ConnectionID = dr["ConnectionID"].ToString();
            ViewBag.RecommentCID = dr["CustomerBlock"].ToString() + "-" + dr["CustomerCode"].ToString();
            Dictionary<string, string> para = new Dictionary<string, string>{
                { "GoogleMapFullAddress", dr["GoogleMap_FullAddress"].ToString() },
                { "GoogleMapLatitude", dr["GoogleMap_Latitude"].ToString() },
                { "GoogleMapLongitude", dr["GoogleMap_Longitude"].ToString() }
            };
            ViewBag.GoogleMap = new JavaScriptSerializer().Serialize(para);
            string CustomerID = dr["CustomerID"].ToString();
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Block:",
                ID = "CustomerBlock",
                DBField = "CustomerBlock",
                Required = true,
                IsReadOnly = true,
                Value = dr["CustomerBlock"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "CID:",
                ID = "CustomerCID",
                DBField = "CustomerCode",
                Required = true,
                IsReadOnly = true,
                Value = dr["CID"].ToString()
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Customer Name:",
            //    ID = "CustomerName",
            //    DBField = "CustomerName",
            //    Required = true,
            //    IsReadOnly = true,
            //    Value = dr["CustomerName"].ToString()
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Connection ID:",
                ID = "ConnectionID",
                DBField = "ConnectionID",
                Required = true,
                IsReadOnly = true,
                Value = dr["ConnectionID"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Service Name:",
                ID = "ServiceName",
                DBField = "ServiceName",
                Required = true,
                IsReadOnly = true,
                Value = dr["ServiceName"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Connection Type:",
                ID = "ConnectionName",
                DBField = "ConnectionName",
                Required = true,
                IsReadOnly = true,
                Value = dr["ConnectionType"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Name:",
                ID = "PackageName",
                DBField = "PackageName",
                Required = true,
                IsReadOnly = true,
                Value = dr["PackageName"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Duration:",
                ID = "Duration",
                DBField = "Duration",
                Required = true,
                IsReadOnly = true,
                Value = dr["Duration"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Bandwidth:",
                ID = "Bandwidth",
                DBField = "Bandwidth",
                Required = true,
                IsReadOnly = true,
                Value = dr["Bandwidth"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Start Date:",
                ID = "StartDate",
                DBField = "StartDate",
                Required = true,
                IsReadOnly = true,
                Value = dr["StartDate"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "End Date:",
                ID = "EndDate",
                DBField = "EndDate",
                Required = true,
                IsReadOnly = true,
                Value = dr["EndDate"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Next Invoice Date:",
                ID = "NextInvoiceDate",
                DBField = "NextInvoiceDate",
                Required = true,
                IsReadOnly = true,
                Value = dr["NextInvoiceDate"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Register Date:",
                ID = "RegisterDate",
                DBField = "RegisterDate",
                Required = true,
                IsReadOnly = true,
                Value = dr["RegisterDate"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Register By:",
                ID = "RegisterBy",
                DBField = "RegisterBy",
                //Required = true,
                IsReadOnly = true,
                Value = dr["RegisterBy"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Invoice No:",
                ID = "TxtInvoiceNo",
                DBField = "InvoiceNo",
                Required = true,
                IsReadOnly = true,
                Value = dr["InvoiceNo"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Address:",
                ID = "TxtAddress",
                DBField = "InstallLocation",
                Required = true,
                IsReadOnly = true,
                Value = dr["InstallLocation"].ToString()
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Phone Number:",
                ID = "TxtPhoneNumber",
                DBField = "PhoneNumber",
                Required = true,
                IsReadOnly = true,
                Value = dr["PhoneNumber"].ToString()
            });

            sql = @"select isnull(ID, '') ID, ActivateCustomerID, ConnectionType, PPPoEUserName, PPPoEPassword, IPAddress, 
GetwayIP, SubnetIP, DNS1, DNS2, WifiIP, WifiName, WifiKey, WifiLoginName, WifiLoginPassword, RouterID,
ConnectivityStatus, VLanID, VLanName, VLanIPAddress, DeviceName CustDeviceName, format(ActivateDate, 'dd/MM/yyyy') ActivateDate, isnull(RenewBy, '') RenewBy, isnull(ActualProfile,'') ActualProfile, isnull(UserGroup, '') UserGroup
from dbo.TblActivateCustomer
where ActivateCustomerID = @ActivateCustomerID";
            para.Add("ActivateCustomerID", ActivateCustomerID);
            ds = html.LoadData(sql, false, para);
            ViewBag.ActiveCustomerData = html.JsonConvertDataTable(ds.Tables[0]).ToJsonString();
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();

            sql = @"Select REPLACE(ci.ItemDescription, s.ServiceName+ ' - ', '') ItemDescription, ci.ConnectionID, s.ServiceName from TblCustomerInvoice ci
inner join dbo.TblServices s on s.ServiceID = ci.ServiceID
where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo and (CHARINDEX('CON:',ItemDescription) <> 0
or CHARINDEX('CON: ',ItemDescription) <> 0 or CHARINDEX(' CON: ',ItemDescription) <> 0)";
            para = new Dictionary<string, string>
            {
                {"CustomerID", CustomerID.ToString() },
                {"InvoiceNo", InvoiceNo.ToString() }
            };
            string[] delimiterChars = { "CON:" };
            ds = html.LoadData(sql, false, para);
            List<object> list = new List<object>();
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        string ServiceName = row["ItemDescription"].ToString().Replace(";", string.Empty).Trim();
                        string[] Connection = ServiceName.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                        list.Add(new { Connection = row["ConnectionID"].ToString(), Data = Connection.ToList().Skip(1).ToList() });
                    }
                }
            }
            ViewBag.ConnectionLists = list.ToJsonString();

            sql = "select ConnectionName from dbo.TblActivateCustomer where CustomerID = @CustomerID and InvoiceNo = @InvoiceNo";
            para = new Dictionary<string, string>
            {
                {"CustomerID", CustomerID.ToString() },
                {"InvoiceNo", InvoiceNo.ToString() }
            };
            ViewBag.ConnectionListsActivated = html.JsonData(sql, false, para);

            sql = "select RouterID, RouterName from dbo.TblRouter where isnull(StatusID, 0) <> 4 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            ViewBag.RouterName = html.LoadDataToCBO("Cbo_RouterName", sql, "RouterID", "RouterName");

            //sql = @"select s.ItemID, i.Barcode, i.ItemName, i.SerialNumber, i.ItemGroup, case when i.Barcode <> '' then i.Barcode + ' - ' else '' end + i.ItemName DeviceName
            //from dbo.TblStockOut s
            //inner join dbo.TblItem i on i.ItemID = s.ItemID
            //inner join dbo.TblItemGroup ig on ig.ItemGroupID = i.ItemGroupID
            //where s.CustomerID = @CustomerID and s.ConnectionID = @ConnectionID";
            //para.Clear();
            //para.Add("CustomerID", CustomerID);
            //para.Add("ConnectionID", dr["ConnectionID"].ToString());
            //ViewBag.Equipment = html.JsonData(sql, false, para);

            sql = "select StatusID, [Status] from dbo.TblRouterDeviceStatus where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            ViewBag.RouterDeviceStatus = "";// html.LoadDataToCBO("Cbo_RouterDeviceStatus", sql, "StatusID", "Status", "", false, false, true, false, false, "", false, false, true);
            html = new HTML
            {
                BtnSaveChangeDialog = "SaveChange",
                //html.DrawBtnSaveChangeFrmV2 = false;
                SaveChangeFunction = "SaveChangeCustActivation()",
                NeedsValidationForm = false,
                PrimaryFieldValue = ActivateCustomerID
            };
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    SQLEdit="select di.CustomerDeviceInfoID, di.CustomerID, di.ActivateCustomerID, di.InvoiceNo, di.ConnectionID, di.[SID],\r\ndn.DeviceNameID, dn.DeviceName, di.PONPort, di.OLTID, dsn.SplitterNameID, dsn.SplitterName, di.[Port],\r\ndi.[Length], di.LossDB, di.GoogleMap, di.WID, uw.WarehouseName Warehouse, di.CustomerNotes, di.InstallBy, di.CompleteInstallDate\r\nfrom dbo.TblCustomerDeviceInfo di\r\ninner join dbo.TblDeviceName dn on dn.DeviceNameID = di.DeviceNameID\r\ninner join dbo.TblDeviceSplitterName dsn on dsn.SplitterNameID = di.SplitterNameID\r\nleft join dbo.TblUserWarehouse uw on uw.WID = di.WID where di.ActivateCustomerID="+ActivateCustomerID,
                    FormTitle ="Device Information",
                    TableName="TblCustomerDeviceInfo",
                    GetIDENTITYByFiled="ActivateCustomerID",
                    PrimaryFieldName="ActivateCustomerID",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="CustomerNotes", Value="Customer_Notes", GetValueFromTextAreaName = true
                        },
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Device Name:", ID="DeviceName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            isTypeahead=true,
                            SQL="select DeviceName from TblDeviceName where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by DeviceName",
                            //HiddenFeild="OLTNameID",
                            DBField = "DeviceName", Required = true
                        },
                        new HtmlTextBox()
                        {
                            Label="PON/Port:", ID="PON",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "PONPort", ISNumeric=true, ISInteger=true,
                            Required = true
                        },
                        new HtmlTextBox()
                        {
                            Label="ONT ID:", ID="OLTID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "OLTID"
                        },
                        new HtmlTextBox()
                        {
                            Label="Splitter Name:", ID="SplitterName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "SplitterName",
                            isTypeahead=true,Required = true,
                            SQL="select SplitterName from TblDeviceSplitterName where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by SplitterName",
                        },
                        new HtmlTextBox()
                        {
                            Label="Port:", ID="Port",Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "Port", ISNumeric = true, ISInteger=true,
                        },
                    }
                },
                new FormData()
                {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Length:", ID="Length",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "Length",
                        },
                        new HtmlTextBox()
                        {
                            Label="Loss DB:", ID="LossDB",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "LossDB",
                        },
                        new HtmlTextBox()
                        {
                            Label="Google Map:", ID="GoogleMap",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "GoogleMap",
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Warehouse:", ID="Warehouse",
                            Class="input2 CustInfo", IsCustomForm = true,
                            isTypeahead=true,
                            SQL="select WID, WarehouseName Warehouse from dbo.TblUserWarehouse where SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+" order by WarehouseName",
                            HiddenField="WID",
                            DBField = "Warehouse",
                        },
                        new HtmlTextBox()
                        {
                            Label="Install By:", ID="InstallBy",
                            Class="input2 CustInfo", IsCustomForm = true,
                            ISAutoComplete=true,
                            SQL_AutoComplete="select distinct top 10 FullName from dbo.TblUserLogin where SystemID = "+html.SystemID+" and lower(FullName) like @SearchText and UserRoleID = 23 order by FullName",
                            Required = true,
                            DBField="InstallBy"
                        },
                        new HtmlTextBox()
                        {
                            Label="Installed Date:", ID="CompleteInstallDate",
                            Class="input2 CustInfo", IsCustomForm = true,
                            SingleDatePicker=true,
                            TimePicker=true,
                            Required = true,
                            DBField="CompleteInstallDate"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ShowFooter = true,
                            FooterColspan = 3,
                            ShowTermsConditions = false,
                            ShowCustomerNotes = true,
                            ShowBalance=false,
                            ShowDeposit=false,
                            ShowTotal=false,
                            GetIDENTITYByFiled="ActivateCustomerID",
                            PrimaryFieldName="ActivateCustomerID",
                            TableName="TblCustomerDeviceInfoDetail",
                            SQL=@"select ci.ConnectionID, cd.ItemID, cd.ItemSerialDetailID, cd.ItemName, cd.UM, cd.Type, cd.QTY, cd.SerialNo, cd.MACAddress, cd.Note
from dbo.TblCustomerDeviceInfo ci 
inner join dbo.TblCustomerDeviceInfoDetail cd on cd.CustomerDeviceInfoID = ci.CustomerDeviceInfoID where ci.ActivateCustomerID = "+html.PrimaryFieldValue+" and ci.InvoiceNo='"+dr["InvoiceNo"].ToString()+"'",
                            HtmlTblID="TableItemInformation",
                            InputName="ItemName",
                            TRName="TRNo_ItemInfon",
                            TDName="TDNo",
                            ScriptCallBackFunction="CallSetAutoComplete_ItemName",
                            dbField=new DBField[]
                            {
                                new DBField()
                                {
                                    DBFieldName="InstallBy", Value="InstallBy", SetValue="$(\"#InstallBy\").val()"
                                },
                                new DBField()
                                {
                                    DBFieldName="CompleteInstallDate", Value="CompleteInstallDate", SetValue="$(\"#CompleteInstallDate\").val()"
                                }
                            },
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="No."
                                },
                                new TblTHead()
                                {
                                     Label="Item Name",Class="col-4"
                                },
                                new TblTHead()
                                {
                                     Label="UM", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Type", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="QTY"
                                },
                                new TblTHead()
                                {
                                     Label="Connection",
                                },
                                new TblTHead()
                                {
                                     Label="Serial No", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="MAC Address",
                                },
                                new TblTHead()
                                {
                                     Label="Note",
                                     CloseTR="</tr>"
                                },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TRNo_ItemInfon\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TDNo"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ItemName",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ItemName", DivClass="wrap-input3",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo_ItemInfon', 'TDNo', this,'','ItemName','TotalAll');",
                                            placeholder="Enter Item Name",
                                            ISAutoComplete = true,
                                            HiddenField="ItemID, ItemSerialDetailID", HiddenFieldValue = ",",
                                            SQL_AutoComplete=@"SELECT DISTINCT i.ItemID, i.ItemName + CASE WHEN ISNULL(i.ItemCode, '') <> '' THEN ' - ' + i.ItemCode ELSE '' END + CASE WHEN ISNULL(srd.SerialNumber, '') <> '' THEN ' - ' + srd.SerialNumber ELSE '' END AS ItemName, iv.SellingPrice AS SalePrice, iv.UM,srd.SerialNumber, sn.StockName, sd.LocationID, uw.WarehouseName, ISNULL(srd.ItemSerialDetailID, 0) AS ItemSerialDetailID,SUM(CASE WHEN ISNULL(srd.SerialNumber, '') IN ('', 'N/A') THEN ISNULL(sd.QTY, 0) - ISNULL(sd.QTYOUT, 0) WHEN i.ItemID = sd.ItemID AND sd.SerialNumber = srd.SerialNumber THEN ISNULL(sd.QTY, 0) - ISNULL(sd.QTYOUT, 0) WHEN srd.StockInID = sd.StockInID AND sd.Type IN ('Stock In', 'Opening Stock') THEN 1 ELSE 0 END ) AS QTY FROM dbo.TblItem i LEFT JOIN dbo.TblItemSerialDetail srd ON srd.ItemID = i.ItemID INNER JOIN dbo.TblItemVariant iv ON iv.ItemID = i.ItemID LEFT JOIN dbo.TblStockInDetail sd ON i.ItemID = sd.ItemID AND (ISNULL(sd.SerialNumber, '') = '' OR sd.SerialNumber = srd.SerialNumber) LEFT JOIN dbo.TblUserWarehouse uw ON uw.WID = sd.LocationID INNER JOIN dbo.TblStockName sn on sn.StockNameID = i.StockNameID WHERE LOWER(i.ItemName + CASE WHEN ISNULL(i.ItemCode, '') <> '' THEN ' - ' + ISNULL(i.ItemCode, '') ELSE '' END + CASE WHEN ISNULL(srd.SerialNumber, '') <> '' THEN ' - ' + ISNULL(srd.SerialNumber, '') ELSE '' END) LIKE @SearchText AND sd.LocationID = @SearchText2 AND i.SystemID = "+html.SystemID+" AND i.WarehouseID = "+html.WarehouseID+" AND (ISNULL(srd.SerialNumber, '') = '' OR EXISTS (SELECT 1 FROM dbo.TblStockInDetail sid WHERE sid.StockInID = srd.StockInID AND sid.Type IN ('Stock In', 'Opening Stock'))) GROUP BY i.ItemID, i.ItemName + CASE WHEN ISNULL(i.ItemCode, '') <> '' THEN ' - ' + i.ItemCode ELSE '' END + CASE WHEN ISNULL(srd.SerialNumber, '') <> '' THEN ' - ' + srd.SerialNumber ELSE '' END, uw.WID, srd.ItemSerialDetailID, srd.SerialNumber, iv.SellingPrice, iv.UM, sn.StockName, sd.LocationID, uw.WarehouseName HAVING SUM(CASE WHEN ISNULL(srd.SerialNumber, '') IN ('', 'N/A') THEN ISNULL(sd.QTY, 0) - ISNULL(sd.QTYOUT, 0) WHEN i.ItemID = sd.ItemID AND sd.SerialNumber = srd.SerialNumber THEN ISNULL(sd.QTY, 0) - ISNULL(sd.QTYOUT, 0) WHEN srd.StockInID = sd.StockInID AND sd.Type IN ('Stock In', 'Opening Stock') THEN 1 ELSE 0 END) > 0;",
                                            DisplayOnFields="UM, SerialNumber, StockName, LocationID,WarehouseName, CostPrice, ItemSerialDetailID",
                                            CboValue="Warehouse_WID",
                                            isApplyTypeaheadOnCallBack=true,
                                            CallBackToControlID="UM",
                                            FieldName = "ItemID",
                                            CallBackDisplayText="UM",
                                            SQLTypeaheadCallBack = "select distinct * from(select ItemID, UM, SerialNumber, format(CostPrice,'#0.00') CostPrice from dbo.TblItem where ItemID = @ItemID union all select ItemID, UM, '' SerialNumber, '0' CostPrice from dbo.TblUMItem where UM <> '' and ItemID = @ItemID) as a order by ItemID",
                                            DisplayOnFieldsTypeahead="UM, SerialNumber, CostPrice",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="UM",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="UM",
                                            DivClass="wrap-input3",
                                            placeholder="Enter UM",
                                            HiddenField="WarehouseName,LocationID,CostPrice",
                                            HiddenFieldValue="0,0,CostPrice"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="CustomerDeviceInfoDetailID",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="CustomerDeviceInfoDetailID",
                                            DivClass="wrap-input3",
                                            placeholder="Enter CustomerDeviceInfoDetailID",
                                            Hide=true, Value = "0"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="StockName",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Type",
                                            DivClass="wrap-input3",
                                            placeholder="Enter Type"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="QTY", DivClass="wrap-input3 text-center",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="QTY", Value = "1", IsAllowInputOnlyNumber =true,
                                            ISNumeric= true, ISInteger= true,
                                            //HiddenFeild="StockInDate", 
                                            placeholder="Enter QTY",
                                            //OnKeyupFunction="TotalAll();",Digit="0.000"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Connection", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="Connection", placeholder="Enter Connection",
                                            isTypeahead = true,
                                            Value = ConnectionID,
                                            SQL="select distinct ConnectionID from dbo.TblCustomerBillingInvoice where CustomerID = "+ CustomerID
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="SerialNumber", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="SerialNo", placeholder="Enter Serial No"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="MACAddress", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="MACAddress", placeholder="Enter MAC Address"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Remark", DivClass="wrap-input3",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="Note", placeholder="Enter Remark"
                                        },
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                }
            };
            ViewBag.DrawHTMLFormDeviceInformation = html.DrawHTMLFormV1();


            return View();
        }

        public JsonResult LoadRouterDeviceStatus()
        {
            HTML html = new HTML();
            string sql = "select StatusID, [Status] from dbo.TblRouterDeviceStatus where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            string data = html.LoadDataToCBO("Cbo_RouterDeviceStatus", sql, "StatusID", "Status", "", false, false, true, false, false, "", false, false, true);
            return Json(new { Data = data }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserOnPPPeE(int RouterID)
        {
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            sqlResult = MK.ListsUserPPPoE(RouterID.ToString());
            return sqlResult.DataRowRouter != null
                ? Json(sqlResult.DataRowRouter.ToList(), JsonRequestBehavior.AllowGet)
                : Json(string.Empty, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetPPPeEProfile(int RouterID)
        {
            string userGroupsHtml = "";

            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = MK.GetProfileName("", 0, "", "", false, RouterID);

            IEnumerable<ITikReSentence> userGroups = sqlResult.Parameters.UserGroups;

            if (userGroups != null)
            {
                var enumerator = userGroups.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    string cbo = "<select id=\"CboUserGroups\" name=\"CboUserGroups\">";

                    foreach (ITikReSentence data in userGroups)
                    {
                        cbo += "<option data-id=\"" +
                               data.Words[".id"] +
                               "\" value=\"" +
                               data.Words["name"] +
                               "\">" +
                               data.Words["name"] +
                               "</option>";
                    }

                    cbo += "</select>";
                    userGroupsHtml = cbo;
                }
            }

            return Json(
                new
                {
                    ProfileName = sqlResult.Value,
                    UserGroups = userGroupsHtml
                },
                JsonRequestBehavior.AllowGet
            );
        }


        public ActionResult LoadCustEquipment(int ActivateCustomerID, int CustomerID, string ConnectionID)
        {
            HTML html = new HTML();
            //string sql = "select EquipmentID, ActivateCustomerID, CustomerID, ItemName, MACAddress, ";
            //sql += "SerialNo, ItemType, NSM, Customer, Barcode, [Status], ItemID, Reason ";
            //sql += "from dbo.TblCustomerEquipment where ActivateCustomerID = @ActivateCustomerID";
            //--and s.ItemID not in (select ItemID from dbo.TblCustomerEquipment where CustomerID = @CustomerID and ActivateCustomerID = @ActivateCustomerID)
            string sql = @"select s.ItemID, i.Barcode, i.ItemName, i.SerialNumber SerialNo, i.MACAddress,
            case when i.Barcode <> '' then i.Barcode + ' - ' else '' end + i.ItemName DeviceName,
            0 EquipmentID, 0 ActivateCustomerID, s.CustomerID, '' NSM, '' Customer,
            '' [Status], 'New Installtion' Reason, 2 OrderBy, '' Remark
            from dbo.TblStockOut s
            inner join dbo.TblItem i on i.ItemID = s.ItemID
            inner join dbo.TblItemGroup ig on ig.ItemGroupID = i.ItemGroupID
            where isnull(s.IsReturn,0) = 0 and s.CustomerID = @CustomerID and s.ConnectionID = @ConnectionID
            and isnull(s.IsUsed,0) = 0
            union all
            select ItemID, Barcode, ItemName,SerialNo, MACAddress, '' DeviceName,
            EquipmentID, ActivateCustomerID, CustomerID, NSM, Customer, [Status], Reason, 1 OrderBy, Remark
            from dbo.TblCustomerEquipment 
            where CustomerID = @CustomerID and ActivateCustomerID = @ActivateCustomerID Order By OrderBy";
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { "ActivateCustomerID", ActivateCustomerID.ToString() },
                { "CustomerID", CustomerID.ToString() },
                { "ConnectionID", ConnectionID.ToString() }
            };
            string Data = html.JsonData(sql, false, Parameters);
            //sql = "select ActivateCustomerID, ConnectionType, PPPoEUserName, PPPoEPassword, IPAddress, ";
            //sql += "GetwayIP, SubnetIP, DNS1, DNS2, WifiIP, WifiName, WifiKey, WifiLoginName, WifiLoginPassword, ActivateBy, format(ActivateDate, 'dd-MM-yyyy HH:mm:ss') ActivateDate, LockedBy, format(LockedDate, 'dd-MM-yyyy HH:mm:ss') LockedDate, BillingInvoiceID, ConnectivityStatus, VLanID, VLanName, VLanIPAddress,IPAddress ";
            //sql += "from dbo.TblActivateCustomer where ActivateCustomerID = @ActivateCustomerID";
            //DataSet ds = html.LoadData(sql, false, Parameters);
            //bool ISAlreadyActivate = false;
            //string ActivateCustomerData = "";
            //if (ds.Tables.Count > 0)
            //{
            //    if (ds.Tables[0].Rows.Count > 0)
            //    {
            //        ISAlreadyActivate = true;
            //        ActivateCustomerData = JsonConvert.SerializeObject(ds.Tables[0]);
            //    }
            //}
            return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> SaveChangeActivate(object[] CheckPPPoEUserName, object[] DeviceInformation, object[] DeviceInformationDetail, object[] ActivateCustomerData, object[] ApplyCons)
        {
            DataSet ds = new DataSet();
            dynamic DeviceInfo = new JavaScriptSerializer().DeserializeObject(DeviceInformation[0].ToString());
            dynamic DeviceInfoDetail = new JavaScriptSerializer().DeserializeObject(DeviceInformationDetail[0].ToString());
            dynamic ActivateCustomer = new JavaScriptSerializer().DeserializeObject(ActivateCustomerData[0].ToString());
            dynamic ApplyCon = new JavaScriptSerializer().DeserializeObject(ApplyCons[0].ToString());
            Dictionary<string, string> para = new Dictionary<string, string>();
            string ActCustomerData = "";
            bool AllowSaveChange = false;
            string Comment = "", SID = "", CustID = "", CID = "";
            Dictionary<string, object> obj = new Dictionary<string, object>();
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            SqlResult rst = new SqlResult();
            Mikrotik MK = new Mikrotik();
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            string ActivateDate = "";
            int ActivateCustomerID = 0;
            bool IsGenerateConnection = false;
            string getActivateCustomerID = "0";
            string ConnectionID = "";
            string FirstName;
            string LastName;
            string Phone;
            string Email;
            string Location;
            string sql;
            int RouterID;
            int CustomerID;
            string InvoiceNo;
            if (Enumerable.Count(ActivateCustomer) > 0)
            {
                ActivateDate = DateTime.Parse(ActivateCustomer[0]["ActivateDate"].ToString()).ToString("yyyy/MM/dd") + " " + DateTime.Parse(html.GetCurrentDate()).ToString("hh:mm:ss tt");
                dynamic CheckPPPoEUser = new JavaScriptSerializer().DeserializeObject(CheckPPPoEUserName[0].ToString());
                bool ISCheckPPPoEUserName = bool.Parse(CheckPPPoEUser["CheckPPPoEUserName"].ToString());
                CustomerID = int.Parse(ActivateCustomer[0]["CustomerID"].ToString());
                InvoiceNo = ActivateCustomer[0]["InvoiceNo"].ToString();

                string ConnectionType = ActivateCustomer[0]["ConnectionType"];
                string RouterName = ActivateCustomer[0]["RouterName"].ToString();
                ConnectionID = ActivateCustomer[0]["ConnectionID"].ToString();
                string ReActivate = ActivateCustomer[0]["ISReActivate"].ToString();
                string UserPPPoEOnRadiusID = ActivateCustomer[0]["UserPPPoEOnRadiusID"].ToString();
                string UserPPPoEOnRadius = ActivateCustomer[0]["UserPPPoEOnRadius"].ToString();
                string ManualPPPoEProfile = ActivateCustomer[0]["ManualPPPoEProfile"].ToString();
                string UserGroup = ActivateCustomer[0]["UserGroup"].ToString();
                bool ISReActivate = bool.Parse(ReActivate);
                string VLanID = "";
                ActivateCustomerID = int.Parse(ActivateCustomer[0]["ActivateCustomerID"].ToString());
                ActivateCustomer[0]["SystemID"] = html.SystemID;
                ActivateCustomer[0]["WarehouseID"] = html.WarehouseID;
                string ProfileName = "", IPPOOL = "";

                if (ConnectionType != "Manual")
                {
                    RouterID = int.Parse(CheckPPPoEUser["RouterID"].ToString());
                    MK.SetConnection(RouterID);
                    if (ISCheckPPPoEUserName)
                    {
                        ActivateCustomerID = int.Parse(CheckPPPoEUser["ActivateCustomerID"].ToString());
                        string oldPPPoEUserName = CheckPPPoEUser["oldPPPoEUserName"].ToString();
                        string oldPPPoEPassword = CheckPPPoEUser["oldPPPoEPassword"].ToString();
                        string PPPoEUserName = CheckPPPoEUser["PPPoEUserName"].ToString();
                        string PPPoEPassword = CheckPPPoEUser["PPPoEPassword"].ToString();
                        if (oldPPPoEUserName != PPPoEUserName)
                            AllowSaveChange = true;
                        if (oldPPPoEPassword != PPPoEPassword)
                            AllowSaveChange = true;
                        if (AllowSaveChange)
                        {
                            sqlResult = MK.UpdatePPPoEUser(oldPPPoEUserName, PPPoEUserName, PPPoEPassword);
                            if (sqlResult.Result == ResultType.Successful)
                            {
                                sql = "update dbo.TblActivateCustomer set PPPoEUserName = @PPPoEUserName, PPPoEPassword = @PPPoEPassword where ConnectionID = @ConnectionID and CustomerID = @CustomerID;";
                                sql += "update dbo.TblCustomerBillingInvoice set PPPoEUserName = @PPPoEUserName, PPPoEPassword = @PPPoEPassword where ConnectionID = @ConnectionID and CustomerID = @CustomerID";
                                para = new Dictionary<string, string>
                                {
                                    { "PPPoEUserName", PPPoEUserName },
                                    { "PPPoEPassword", PPPoEPassword },
                                    { "ConnectionID", ConnectionID.ToString() },
                                    { "CustomerID", CustomerID.ToString() }
                                };
                                sqlResult = html.SaveChangeData(sql, false, para);
                            }
                            if (sqlResult.Result == ResultType.Failed)
                            {
                                return Json(new { Result = 1, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
                if ((ActivateCustomerID == 0 || ISReActivate) && (ConnectionType != "Manual" || ISReActivate))
                {
                    if ((ConnectionType == "PPPoE" && ManualPPPoEProfile == "") || ISReActivate)
                    {
                        sql = @"select ci.BillingInvoiceID, ci.ServiceID, ci.PackageDetailID, pd.IsDoubleSpeedAtNight, pd.Bandwidth, 
pd.DowSpeed, pd.UpSpeed, ci.Duration, isnull(ci.FreeDuration,0) FreeDuration, pd.Price,
ci.StartDate, ci.EndDate, DATEDIFF(Dy,ci.StartDate, ci.EndDate) Validity, ci.UM, pd.ConnectionTypeID
from dbo.TblCustomerBillingInvoice ci
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = ci.PackageDetailID
where ci.CustomerID = @CustomerID and ci.ConnectionID = @ConnectionID and ci.SystemID = @SystemID and ci.WarehouseID = @WarehouseID";
                        para = new Dictionary<string, string> {
                            { "CustomerID",CustomerID.ToString() },
                            { "ConnectionID",ConnectionID },
                            { "SystemID",html.SystemID },
                            { "WarehouseID",html.WarehouseID },
                        };
                        ds = html.LoadData(sql, false, para);
                        if (ds.Tables.Count > 0)
                        {
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                DataRow dr = ds.Tables[0].Rows[0];
                                string ServiceID = dr["ServiceID"].ToString();
                                string PackageDetailID = dr["PackageDetailID"].ToString();
                                string Validity = dr["Validity"].ToString() + "d";
                                string DowSpeed = dr["DowSpeed"].ToString();
                                string UpSpeed = dr["UpSpeed"].ToString();
                                string Duration = dr["Duration"].ToString();
                                string UM = dr["UM"].ToString();
                                string FreeDuration = dr["FreeDuration"].ToString();
                                string Price = dr["Price"].ToString();
                                if (ConnectionType == "PPPoE")
                                {
                                    //                                    sql = @"select PackageGroupName, Bandwidth, FromTime1, FromTime2, ToTime1, ToTime2, ToTime3, ToTime4, IsDoubleSpeedAtNight, IPPOOL
                                    //from dbo.TblPackageGroup 
                                    //where Bandwidth = @Bandwidth and IsDoubleSpeedAtNight = @IsDoubleSpeedAtNight and RouterID = @RouterID and SystemID = @SystemID and WarehouseID = @WarehouseID";
                                    //                                    para = new Dictionary<string, string> {
                                    //                                        { "Bandwidth", dr["Bandwidth"].ToString()},
                                    //                                        { "IsDoubleSpeedAtNight",(dr["IsDoubleSpeedAtNight"].ToString()==""?"1":dr["IsDoubleSpeedAtNight"].ToString()) },
                                    //                                        { "RouterID",RouterID.ToString() },
                                    //                                        { "SystemID",html.SystemID },
                                    //                                        { "WarehouseID",html.WarehouseID },
                                    //                                    };
                                    //                                    ds = html.LoadData(sql, false, para);
                                    //if (ds.Tables.Count > 0)
                                    //{
                                    //    if (ds.Tables[0].Rows.Count > 0)
                                    //    {
                                    //        if (ConnectionType == "PPPoE")
                                    //        {
                                    //            dr = ds.Tables[0].Rows[0];
                                    //            Bandwidth = dr["Bandwidth"].ToString();
                                    //            ProfileName = dr["PackageGroupName"].ToString() + " - " + UM + (int.Parse(FreeDuration) == 0 ? "" : " - Free: " + html.Pluralize("Month", int.Parse(FreeDuration), "s"));
                                    //            LimitationName = ProfileName;
                                    //            IPPOOL = dr["IPPOOL"].ToString();
                                    //            FromTime1 = dr["FromTime1"].ToString();
                                    //            FromTime2 = dr["FromTime2"].ToString();
                                    //            ToTime1 = dr["ToTime1"].ToString();
                                    //            ToTime2 = dr["ToTime2"].ToString();
                                    //            ToTime3 = dr["ToTime3"].ToString();
                                    //            ToTime4 = dr["ToTime4"].ToString();
                                    //            IsDoubleSpeedAtNight = bool.Parse(dr["IsDoubleSpeedAtNight"].ToString());
                                    //            SqlResult result = new SqlResult();
                                    //            MK.CreateLimitationName(LimitationName, (int.Parse(DowSpeed.ToString()) + 5).ToString(), (int.Parse(UpSpeed) + 5).ToString(), IPPOOL, Prefix);
                                    //            MK.CreateProfileName(ProfileName, Validity, Price);
                                    //            if (IsDoubleSpeedAtNight)
                                    //            {
                                    //                int dDownload = int.Parse(DowSpeed.ToString()) * 2;
                                    //                int dUpload = int.Parse(UpSpeed.ToString()) * 2;
                                    //                LimitationName_DoubleSpeed = ProfileName + "-Double: " + dDownload + Prefix;
                                    //                MK.CreateLimitationName(LimitationName_DoubleSpeed, dDownload.ToString(), dUpload.ToString(), IPPOOL, Prefix);
                                    //                MK.ApplyProfileLimitation(ProfileName, LimitationName, FromTime1, FromTime2);
                                    //                MK.ApplyProfileLimitation(ProfileName, LimitationName_DoubleSpeed, ToTime1, ToTime2);
                                    //                LimitationName_DoubleSpeed = ProfileName + "-Double-ST0: " + dDownload + Prefix;
                                    //                MK.CreateLimitationName(LimitationName_DoubleSpeed, dDownload.ToString(), dUpload.ToString(), IPPOOL, Prefix);
                                    //                result = MK.ApplyProfileLimitation(ProfileName, LimitationName_DoubleSpeed, ToTime3, ToTime4);
                                    //            }
                                    //            else
                                    //            {
                                    //                result = MK.ApplyProfileLimitation(ProfileName, LimitationName, "0s", "23h59m59s");
                                    //                IsDoubleSpeedAtNight = false;
                                    //            }
                                    //            if (result.Result == ResultType.Successful)
                                    //            {
                                    //                Dictionary<string, string> parameters = new Dictionary<string, string>();
                                    //                sql = "insert into dbo.TblRediusProfile (RouterID, Name, UserName, ServiceID, ConnectionTypeID, ";
                                    //                sql += "PackageDetailID, Duration, FreeDuration, DownloadSpeed, UploadSpeed, Price, ProfileName, LimitationName, ";
                                    //                sql += "IPPOOL, Validity, IsDoubleSpeedAtNight, LimitationDoubleSpeedName, FromTime1, FromTime2,";
                                    //                sql += "ToTime1, ToTime2, ToTime3, ToTime4, SystemID, WarehouseID, CreateDate, CreateBy) ";
                                    //                sql += "values (@RouterID, @Name, @UserName, @ServiceID, @ConnectionTypeID, ";
                                    //                sql += "@PackageDetailID, @Duration, @FreeDuration, @DownloadSpeed, @UploadSpeed, ";
                                    //                sql += "@Price, @ProfileName, @LimitationName, @IPPOOL, @Validity, @IsDoubleSpeedAtNight, ";
                                    //                sql += "@LimitationDoubleSpeedName, @FromTime1, @FromTime2, @ToTime1, @ToTime2, @ToTime3, ";
                                    //                sql += "@ToTime4, @SystemID, @WarehouseID,@CreateDate, @CreateBy);SELECT @@IDENTITY AS 'Identity';";
                                    //                //if (RediusProfileID != "0")
                                    //                //{
                                    //                //    sql = "update dbo.TblRediusProfile set RouterID = @RouterID, Name = @Name, UserName = @UserName, ServiceID = @ServiceID, ConnectionTypeID = @ConnectionTypeID, ";
                                    //                //    sql += "PackageDetailID = @PackageDetailID, Duration = @Duration, FreeDuration = @FreeDuration, DownloadSpeed = @DownloadSpeed, UploadSpeed = @UploadSpeed, Price = @Price, ProfileName = @ProfileName, LimitationName = @LimitationName, ";
                                    //                //    sql += "IPPOOL = @IPPOOL, Validity = @Validity, IsDoubleSpeedAtNight = @IsDoubleSpeedAtNight, LimitationDoubleSpeedName = @LimitationDoubleSpeedName, FromTime1 = @FromTime1, FromTime2 = @FromTime2,";
                                    //                //    sql += "ToTime1 = @ToTime1, ToTime2 = @ToTime2, ToTime3 = @ToTime3, ToTime4 = @ToTime4, SystemID = @SystemID, WarehouseID = @WarehouseID ";
                                    //                //    sql += "where RediusProfileID = @RediusProfileID;select @RediusProfileID";
                                    //                //    parameters.Add("RediusProfileID", RediusProfileID);
                                    //                //}
                                    //                parameters.Add("RouterID", RouterID.ToString());
                                    //                parameters.Add("Name", "Radius Mikrotik Profile");
                                    //                parameters.Add("UserName", html.UserFullName);
                                    //                parameters.Add("ServiceID", ServiceID);
                                    //                parameters.Add("ConnectionTypeID", ConnectionTypeID);
                                    //                parameters.Add("PackageDetailID", PackageDetailID);
                                    //                parameters.Add("Duration", Duration);
                                    //                parameters.Add("FreeDuration", FreeDuration);
                                    //                parameters.Add("DownloadSpeed", DowSpeed);
                                    //                parameters.Add("UploadSpeed", UpSpeed);
                                    //                parameters.Add("Price", Price);
                                    //                parameters.Add("ProfileName", ProfileName);
                                    //                parameters.Add("LimitationName", LimitationName);
                                    //                parameters.Add("IPPOOL", IPPOOL);
                                    //                parameters.Add("Validity", Validity);
                                    //                parameters.Add("IsDoubleSpeedAtNight", IsDoubleSpeedAtNight.ToString());
                                    //                parameters.Add("LimitationDoubleSpeedName", LimitationName_DoubleSpeed);
                                    //                parameters.Add("FromTime1", FromTime1);
                                    //                parameters.Add("FromTime2", FromTime2);
                                    //                parameters.Add("ToTime1", ToTime1);
                                    //                parameters.Add("ToTime2", ToTime2);
                                    //                parameters.Add("ToTime3", ToTime3);
                                    //                parameters.Add("ToTime4", ToTime4);
                                    //                parameters.Add("SystemID", html.SystemID);
                                    //                parameters.Add("WarehouseID", html.WarehouseID);
                                    //                parameters.Add("CreateDate", ActivateDate);
                                    //                parameters.Add("CreateBy", html.UserFullName);

                                    //                result = html.SaveChangeData(sql, false, parameters, true);
                                    //                parameters.Clear();
                                    //                sql = "update dbo.TblPackageDetail set IsApplyRediusProfile = 1, RediusProfileID = @RediusProfileID where PackageDetailID = @PackageDetailID";
                                    //                parameters.Add("RediusProfileID", result.Value);
                                    //                parameters.Add("PackageDetailID", PackageDetailID);
                                    //                result = html.SaveChangeData(sql, false, parameters);
                                    //            }
                                    //        }
                                    //    }
                                    //    else
                                    //    {
                                    //        MK.CloseMKConnection();
                                    //        return Json(new { Result = 1, Message = "Activate failed due to package name does not existing in Router Name: <b>" + RouterName + "</b>. <br>Please contact administrator." }, JsonRequestBehavior.AllowGet);
                                    //    }
                                    //}
                                }
                            }
                        }
                        if (ActivateCustomerData[0].ToString().Contains("VLanID"))
                        {
                            VLanID = ActivateCustomer[0]["VLanID"];
                        }
                        if (ConnectionType == "PPPoE")
                        {
                            if (ISReActivate)
                            {
                                string sql_str = "select VLanID from dbo.TblActivateCustomer where ActivateCustomerID=" + ActivateCustomerID;
                                DataSet ds_data = html.LoadData(sql_str);
                                if (ds_data.Tables.Count > 0)
                                {
                                    if (ds_data.Tables[0].Rows.Count > 0)
                                    {
                                        string vlan_ID = ds_data.Tables[0].Rows[0]["VLanID"].ToString();
                                        if (vlan_ID != "")
                                        {
                                            sqlResult = MK.DisableVlanID(vlan_ID, "Change Connection Type to PPPoE.");
                                        }
                                    }
                                }
                            }
                            if (ISReActivate)
                            {
                                AllowSaveChange = true;
                            }
                            else
                            {
                                string PPPoEProfileName = ProfileName;// ActivateCustomer[0]["PPPoEProfileName"];
                                sqlResult = MK.CheckIsExisitingProfileName(PPPoEProfileName);
                                if (sqlResult.Value != "" && sqlResult.Result == ResultType.Successful)
                                {
                                    AllowSaveChange = true;
                                }
                                else
                                {
                                    MK.CloseMKConnection();
                                    return Json(new { Result = sqlResult.Result, Message = sqlResult.Message });
                                }
                            }
                        }
                        else if (ConnectionType == "Static")
                        {
                            if (ISReActivate)
                            {
                                string PPPoEUserName = ActivateCustomer[0]["PPPoEUserName"];
                                //MK.RemoveUser(PPPoEUserName);
                                sqlResult = MK.LockUser(PPPoEUserName, "Change to Static");
                                Comment = "Change Connection Type to Static.";
                            }
                            if (ActivateCustomerID != 0)
                                sqlResult = MK.EnableVlanID(VLanID, Comment);
                            else
                                sqlResult.Result = ResultType.Successful;

                            if (sqlResult.Result == ResultType.Successful)
                            {
                                AllowSaveChange = true;
                            }
                            else
                            {
                                MK.CloseMKConnection();
                                return Json(new { Result = sqlResult.Result, Message = sqlResult.Message });
                            }
                        }
                        else if (ConnectionType == "Manual")
                        {
                            if (ISReActivate)
                            {
                                string sql_str = "select VLanID from dbo.TblActivateCustomer where ActivateCustomerID=" + ActivateCustomerID;
                                DataSet ds_data = html.LoadData(sql_str);
                                if (ds_data.Tables.Count > 0)
                                {
                                    if (ds_data.Tables[0].Rows.Count > 0)
                                    {
                                        string vlan_ID = ds_data.Tables[0].Rows[0]["VLanID"].ToString();
                                        if (vlan_ID != "")
                                        {
                                            MK.DisableVlanID(vlan_ID, "Change Connection Type to Manual.");
                                        }
                                    }
                                }
                                string PPPoEUserName = ActivateCustomer[0]["PPPoEUserName"];
                                //MK.RemoveUser(PPPoEUserName);
                                MK.LockUser(PPPoEUserName, "Change to Manual");
                                AllowSaveChange = true;
                                MK.CloseMKConnection();
                            }
                        }
                    }
                    else
                    {
                        ProfileName = ManualPPPoEProfile;
                        AllowSaveChange = true;
                    }

                    if (AllowSaveChange)
                    {
                        ActivateCustomer[0]["ActivateBy"] = html.UserFullName;
                        ActivateCustomer[0]["ActivateDate"] = ActivateDate;
                        ActivateCustomer[0]["ConnectivityStatus"] = 40;
                        ActivateCustomer[0]["ConnectivityStatusName"] = "Active";
                        ActivateCustomer[0]["PPPoEUserName"] = CheckPPPoEUser["PPPoEUserName"].ToString();
                        ActivateCustomer[0]["PPPoEPassword"] = CheckPPPoEUser["PPPoEPassword"].ToString();
                        string PPPoEIPAddress = ActivateCustomer[0]["PPPoEIPAddress"];
                        DeviceInfo[0]["CreateBy"] = html.UserFullName;
                        DeviceInfo[0]["CreateDate"] = ActivateDate;
                        DeviceInfo[0]["SystemID"] = html.SystemID;
                        DeviceInfo[0]["WarehouseID"] = html.WarehouseID;
                        obj.Add("DeviceInfo", DeviceInfo);
                        obj.Add("DeviceInfoDetail", DeviceInfoDetail);
                        ActivateCustomer[0]["PPPoEProfileName"] = ProfileName;
                        obj.Add("ActivateCustomer", ActivateCustomer);
                        para.Clear();
                        para.Add("Data", "N'" + html.XMLDataObj(obj) + "'");
                        rst = html.SaveChangeData("SP_INSERT_ACTIVATE_CUSTOMER", true, para, true, true);

                        sql = @"select distinct TypeOfInvoice from dbo.TblCustomerInvoice where CustomerID = " + CustomerID + " and InvoiceNo='" + InvoiceNo + "'";
                        string TypeOfInvoice = html.LoadData(sql).Tables[0].Rows[0]["TypeOfInvoice"].ToString();
                        getActivateCustomerID = "0";

                        if (rst.Result == ResultType.Successful)
                        {
                            DataTable dt = rst.Data.Tables[0];
                            DataRow dr = dt.Rows[0];
                            getActivateCustomerID = dr["ActivateCustomerID"].ToString();
                            SID = dr["SID"].ToString();
                            CustID = dr["CustomerID"].ToString();
                            CID = dr["ConnectionID"].ToString();
                            ActCustomerData = html.JsonConvertDataTable(dt);
                            string ChangeConnectionTypeComment = ActivateCustomerID == 0
                ? CID + "; " + SID + " Is Activated By " + html.UserFullName + "; " + DateTime.Parse(ActivateDate).ToString("dd/MMM/yyyy hh:mm:ss tt")
                : "Modify: " + CID + "; " + SID + " By " + html.UserFullName + "; " + DateTime.Parse(ActivateDate).ToString("dd/MMM/yyyy hh:mm:ss tt");
                            if (ConnectionType == "PPPoE")
                            {
                                ConnectionID = dr["ConnectionID"].ToString();
                                string PPPoEUserName = dr["PPPoEUserName"].ToString();
                                string PPPoEPassword = dr["PPPoEPassword"].ToString();
                                string PPPoEProfileName = dr["PPPoEProfileName"].ToString();
                                FirstName = dr["FirstName"].ToString();
                                LastName = dr["LastName"].ToString();
                                Phone = dr["Phone"].ToString();
                                Email = dr["Email"].ToString();
                                Location = dr["Location"].ToString();
                                Comment = ChangeConnectionTypeComment;//dr["Comment"].ToString() +

                                if (UserPPPoEOnRadiusID != "")
                                {
                                    sql = "update dbo.TblActivateCustomer set PPPoEUserName = @PPPoEUserName, PPPoEPassword = @PPPoEPassword where ActivateCustomerID = @ActivateCustomerID;";
                                    sql += "update dbo.TblCustomerBillingInvoice set PPPoEUserName = @PPPoEUserName, PPPoEPassword = @PPPoEPassword where ActivateCustomerID = @ActivateCustomerID";
                                    para = new Dictionary<string, string>
                                    {
                                        { "PPPoEUserName", UserPPPoEOnRadius },
                                        { "PPPoEPassword", UserPPPoEOnRadiusID},
                                        { "ActivateCustomerID", getActivateCustomerID }
                                    };
                                    html.SaveChangeData(sql, false, para);
                                    sqlResult = MK.SetPPPoEUserComment(UserPPPoEOnRadius, Comment);
                                }
                                else
                                {
                                    if (ManualPPPoEProfile != "")
                                    {
                                        PPPoEProfileName = ManualPPPoEProfile;
                                        MK.ClearProfileName(PPPoEUserName);
                                        if (UserGroup != "")
                                            IPPOOL = UserGroup;

                                    }
                                    sqlResult = MK.CreateAndUpdateUser(ConnectionID, PPPoEUserName, PPPoEPassword, PPPoEProfileName, FirstName, LastName, Phone, Email, Location, Comment, int.Parse(getActivateCustomerID), IPPOOL, PPPoEIPAddress);
                                    if (sqlResult.Result == ResultType.Successful)
                                    {
                                        IEnumerable<ITikReSentence> row = sqlResult.DataRowRouter;
                                        if (row != null)
                                        {
                                            if (row.Count() > 0)
                                            {

                                                string ID = row.ToList()[0].Words[".id"];
                                                string ActualProfile = UserGroup == "" ? row.ToList()[0].Words["actual-profile"] : PPPoEProfileName;
                                                sql = @"update dbo.TblActivateCustomer set ID = '" + ID + "', ActualProfile = '" + ActualProfile + "' where ActivateCustomerID = " + getActivateCustomerID;
                                                sqlResult = html.SaveChangeData(sql, false, false);
                                            }
                                        }
                                    }
                                }
                                if (sqlResult.Result == ResultType.Failed)
                                {
                                    sql = @"delete from dbo.TblActivateCustomer where ConnectionID = @ConnectionID and SystemID = @SystemID and WarehouseID = @WarehouseID;
update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 43, ActivateCustomerID = 0 where ConnectionID = @ConnectionID and SystemID = @SystemID and WarehouseID = @WarehouseID;
update dbo.TblCustomerInvoice set ConnectivityStatus = 43,  ActivateCustomerID = 0 where ConnectionID = @ConnectionID and SystemID = @SystemID and WarehouseID = @WarehouseID;
update TblCustomerBillInformation set ConnectivityStatus = 43, ActivateCustomerID = 0 where ConnectionID = @ConnectionID and SystemID = @SystemID and WarehouseID = @WarehouseID";
                                    para.Clear();
                                    para.Add("ConnectionID", ConnectionID);
                                    para.Add("SystemID", html.SystemID);
                                    para.Add("WarehouseID", html.WarehouseID);
                                    html.SaveChangeData(sql, false, para);
                                    return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = ActCustomerData });
                                }
                            }
                            else if (ActivateCustomerID == 0)
                            {
                                sqlResult = MK.EnableVlanID(VLanID, ChangeConnectionTypeComment);
                            }
                        }
                    }
                    MK.CloseMKConnection();
                }
                else
                {
                    if (ConnectionType == "Manual" && ActivateCustomerID == 0)
                    {
                        ActivateCustomer[0]["ActivateBy"] = html.UserFullName;
                        ActivateCustomer[0]["ActivateDate"] = ActivateDate;
                        ActivateCustomer[0]["ConnectivityStatus"] = 40;
                        ActivateCustomer[0]["ConnectivityStatusName"] = "Active";
                        DeviceInfo[0]["CreateBy"] = html.UserFullName;
                        DeviceInfo[0]["CreateDate"] = ActivateDate;
                        DeviceInfo[0]["SystemID"] = html.SystemID;
                        DeviceInfo[0]["WarehouseID"] = html.WarehouseID;
                        obj.Add("DeviceInfo", DeviceInfo);
                        obj.Add("DeviceInfoDetail", DeviceInfoDetail);
                        ActivateCustomer[0]["PPPoEProfileName"] = ProfileName;
                        obj.Add("ActivateCustomer", ActivateCustomer);
                    }
                    else
                    {
                        DeviceInfo[0]["CreateBy"] = html.UserFullName;
                        DeviceInfo[0]["CreateDate"] = ActivateDate;
                        DeviceInfo[0]["SystemID"] = html.SystemID;
                        DeviceInfo[0]["WarehouseID"] = html.WarehouseID;
                        obj.Add("DeviceInfo", DeviceInfo);
                        obj.Add("DeviceInfoDetail", DeviceInfoDetail);
                        ActivateCustomer[0]["ActivateBy"] = html.UserFullName;
                        ActivateCustomer[0]["ActivateDate"] = ActivateDate;
                        obj.Add("ActivateCustomer", ActivateCustomer);
                    }
                    para.Clear();
                    para.Add("Data", html.XMLDataObj(obj));
                    rst = html.SaveChangeData("SP_INSERT_ACTIVATE_CUSTOMER", true, para, true, true);
                    if (rst.Result == ResultType.Successful)
                    {
                        DataTable dt = rst.Data.Tables[0];
                        DataRow dr = dt.Rows[0];
                        SID = dr["SID"].ToString();
                        CustID = dr["CustomerID"].ToString();
                        CID = dr["ConnectionID"].ToString();
                        ActCustomerData = html.JsonConvertDataTable(dt);
                    }
                }
            }
            else if (Enumerable.Count(ApplyCon) > 0)
            {
                IsGenerateConnection = true;
                DeviceInfo[0]["CreateBy"] = html.UserFullName;
                DeviceInfo[0]["SystemID"] = html.SystemID;
                DeviceInfo[0]["WarehouseID"] = html.WarehouseID;
                sql = @"DECLARE @InvoiceDate Datetime, @FreeDuration int, @BillingInvoiceID int, @ServiceID int, @StartDate datetime, @EndDate datetime, @Duration int,
@NewConnectionID nvarchar(50), @SID nvarchar(50), @ServiceCode nvarchar(50), @ConnectionTypeCode nvarchar(50), @PackageCode  nvarchar(50), @PackageDetailID int
select	@ServiceID = ServiceID, @PackageDetailID = PackageDetailID, @StartDate = @ActivateDate,
@EndDate = DATEADD(d, DATEDIFF(d, StartDate, EndDate), @ActivateDate), @Duration = Duration, @BillingInvoiceID = BillingInvoiceID,
@InvoiceDate = InvoiceDate, @FreeDuration = isnull(FreeDuration,0)
from dbo.TblCustomerBillingInvoice where ConnectionID = @ConnectionID and InvoiceNo = @InvoiceNo and SystemID = @SystemID and WarehouseID = @WarehouseID
select distinct @ServiceCode = s.ServiceCode, @ConnectionTypeCode = pt.ConnectionTypeCode, @PackageCode = p.PackageCode
from dbo.TblPackages p
inner join dbo.TblPackageType pt on pt.PackageTypeID = p.PackageTypeID
inner join dbo.TblServices s on s.ServiceID = p.ServiceID
inner join dbo.TblPackageDetail pd on pd.PackageID = p.PackageID
where pd.PackageDetailID = @PackageDetailID
declare @SetSPrefix nvarchar(50)
set @SetSPrefix = @ConnectionTypeCode + @PackageCode + '-00';
exec SP_GET_PREFIX_NUMBER @ServiceCode, @SystemID, @WarehouseID, @SID OUT, 'Service ID', @SetSPrefix
declare @SCode nvarchar(50) = 'CON' + @ServiceCode
exec SP_GET_PREFIX_NUMBER @SCode, @SystemID, @WarehouseID, @NewConnectionID OUT, 'Connection','-00'

update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 40,
ConnectivityStatusName = 'Active', 
ActivateDate = @ActivateDate, EndServiceDate = @EndDate,
[ExpireDate] = @EndDate, [SID] = @SID, PPPoEUserName = @PPPoEUserName, PPPoEPassword = @PPPoEPassword, 
RouterID = @RouterID, ConnectionType = @ConnectionType, StartDate = @StartDate, EndDate = @EndDate,
NextInvoiceDate = DATEADD(MONTH, (isnull(@Duration,0)+@FreeDuration), @StartDate)
where ConnectionID = @ConnectionOfConnectionID and BillingInvoiceID = @BillingInvoiceID;

update dbo.TblCustomerInvoice set ConnectivityStatus=40,
ConnectivityStatusName = 'Active', StartDate = @StartDate, EndDate = @EndDate, 
[SID] = @SID, NextInvoiceDate = DATEADD(MONTH, (isnull(@Duration,0)+@FreeDuration), @StartDate),
PPPoEUserName = @PPPoEUserName
where InvoiceNo = @InvoiceNo and CustomerID = @CustomerID and ConnectionID = @ConnectionOfConnectionID

select @PPPoEUserName PPPoEUserName, @PPPoEPassword PPPoEPassword, 
@ServiceID ServiceID, @PackageDetailID PackageDetailID, @SID [SID], 
@NewConnectionID NewConnectionID, @StartDate StartDate, @EndDate EndDate, @Duration Duration

";
                object[] objs = new object[] { };
                var objList = objs.ToList();
                for (int i = 0; i < Enumerable.Count(ApplyCon); i++)
                {
                    ActivateDate = DateTime.Parse(ApplyCon[i]["ActivateDate"].ToString()).ToString("yyyy/MM/dd") + " " + DateTime.Parse(html.GetCurrentDate()).ToString("hh:mm:ss tt");
                    InvoiceNo = ApplyCon[i]["InvoiceNo"];
                    CustomerID = int.Parse(ApplyCon[i]["CustomerID"]);
                    para = new Dictionary<string, string> {
                        {"PPPoEPassword", ActivateDate },
                        {"PPPoEUserName", ApplyCon[i]["Connection"] },
                        {"ActivateDate", ActivateDate },
                        {"ConnectionID", ApplyCon[i]["ConnectionID"] },
                        {"InvoiceNo", ApplyCon[i]["InvoiceNo"] },
                        {"ConnectionOfConnectionID", ApplyCon[i]["ConnectionID"] },
                        {"RouterID", ApplyCon[i]["RouterID"] },
                        {"CustomerID", ApplyCon[i]["CustomerID"] },
                        {"ConnectionType", ApplyCon[i]["ConnectionType"] },
                        {"SystemID", html.SystemID },
                        {"WarehouseID", html.WarehouseID },
                    };

                    sqlResult = html.SaveChangeData(sql, false, para, true, true);
                    ds = sqlResult.Data;
                    ApplyCon[i]["ActivateDate"] = ActivateDate;
                    ApplyCon[i]["Duration"] = ds.Tables[0].Rows[0]["Duration"].ToString();
                    ApplyCon[i]["EndDate"] = ds.Tables[0].Rows[0]["EndDate"].ToString();
                    ApplyCon[i]["StartDate"] = ds.Tables[0].Rows[0]["StartDate"].ToString();
                    ApplyCon[i]["PPPoEPassword"] = ds.Tables[0].Rows[0]["PPPoEPassword"].ToString();
                    ApplyCon[i]["PPPoEUserName"] = ds.Tables[0].Rows[0]["PPPoEUserName"].ToString();
                    ApplyCon[i]["ServiceID"] = ds.Tables[0].Rows[0]["ServiceID"].ToString();
                    ApplyCon[i]["PackageDetailID"] = ds.Tables[0].Rows[0]["PackageDetailID"].ToString();
                    ApplyCon[i]["SID"] = ds.Tables[0].Rows[0]["SID"].ToString();
                    ApplyCon[i]["NewConnectionID"] = ds.Tables[0].Rows[0]["NewConnectionID"].ToString();
                    ApplyCon[i]["SystemID"] = html.SystemID;
                    ApplyCon[i]["WarehouseID"] = html.WarehouseID;
                    ApplyCon[i]["ActivateCustomerID"] = ActivateCustomerID;
                    ApplyCon[i]["ActivateBy"] = html.UserFullName;
                    ApplyCon[i]["ConnectivityStatus"] = 40;
                    ApplyCon[i]["ConnectivityStatusName"] = "Active";
                    string com = "Service ID: " + ds.Tables[0].Rows[0]["SID"].ToString() + "; Connection ID: " + ds.Tables[0].Rows[0]["NewConnectionID"].ToString() + "; Connection Name: " + ApplyCon[i]["Connection"] + "; VLan Name:" + ApplyCon[i]["VLanName"];
                    ApplyCon[i]["Comment"] = com;
                    objList.Add(ApplyCon[i]);
                }
                obj = new Dictionary<string, object> {
                    { "DeviceInfo", DeviceInfo },
                    { "DeviceInfoDetail", DeviceInfoDetail },
                    { "ActivateCustomer", objList }
                };
                para = new Dictionary<string, string> {
                    { "Data", "N'" + html.XMLDataObj(obj) + "'" },
                    { "GenerateConnectonID", "1" }
                };
                rst = html.SaveChangeData("SP_INSERT_ACTIVATE_CUSTOMER", true, para, true, true);
                if (rst.Result == ResultType.Successful)
                {
                    DataRow drs = rst.Data.Tables[0].Rows[0];
                    string Comments = @"Dear Team, <br><br>Now Customer Name: <b>" + drs["LastName"]
                            + @"</b> is <b>ACTIVATED</b> on date: <b>" + DateTime.Parse(ActivateDate).ToString("dd-MMM-yyyyy HH:mm:ss tt")
                            + @"</b> By: <b>" + html.UserFullName
                            + @"</b>.<br><br>Package Name: <b>" + drs["PPPoEProfileName"] + "</b><br><br>Start Date: <b>" + DateTime.Parse(drs["StartDate"].ToString()).ToString("dd-MMM-yyyy") + "</b><br>Expired Date: <b>" + DateTime.Parse(drs["EndDate"].ToString()).ToString("dd-MMM-yyyy") + "</b><br><br>";
                    int i = 0;
                    string TicketNumber = drs["TicketNumber"].ToString();

                    foreach (DataRow dr in rst.Data.Tables[0].Rows)
                    {
                        i++;
                        RouterID = int.Parse(dr["RouterID"].ToString());
                        MK.SetConnection(RouterID);
                        string VLanID = dr["VLanID"].ToString();
                        sqlResult = MK.EnableVlanID(VLanID, dr["Comment"].ToString());
                        Comments += i + "." + " <b>Service ID:" + dr["SID"] + "; Connection ID: " + dr["ConnectionID"] + "; Connection Name: " + dr["PPPoEUserName"] + "; VLan Name:" + dr["VLanName"] + "</b><br>";
                    }
                    Comments += @"<br><br>Successful Activate Connection: " + i + " Connections.";
                    Comments += @"<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO GENERATE TICKET. " + TicketNumber;

                    sql = @"update dbo.TblActivateCustomer set TicketNumber = @TicketNumber where SID =@SID and SystemID = @SystemID and WarehouseID = @WarehouseID;
insert into dbo.TblSupportOpenTicketComment (SupportProjectID, StatusID, [Status], PriorityID, [Priority], OpenTicketID, FromUserLoginID, FromUserName,
ReplyFromUserLoginID, ReplyFromUserName, Comment, Date, CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus)
select SupportProjectID, 4 StatusID, 'Feedback' [Status], 1 PriorityID, 'Normal' [Priority], 
OpenTicketID, UserLoginID FromUserLoginID, CreateBy FromUserName, " + html.UserID + @" ReplyFromUserLoginID, '" + html.UserFullName + @"' ReplyFromUserName, 
N'" + Comments + "' Comment, '" + ActivateDate + @"' [Date], '" + html.UserFullName + @"' CreateBy, '" + ActivateDate + @"'CreateDate, " + html.SystemID + " SystemID, " + html.WarehouseID + @" WarehouseID, 
'Change Status to Feedback by " + html.UserFullName + @"' DescriptionChangeStatus
from dbo.TblSupportOpenTicket
where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID;
update dbo.TblSupportOpenTicket set SID = @SID, StatusID = 4, Status= 'Feedback', PriorityID = 1, Priority = 'Normal' where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID;";
                    para.Clear();
                    para.Add("SID", "0");
                    para.Add("TicketNumber", TicketNumber);
                    para.Add("SystemID", html.SystemID);
                    para.Add("WarehouseID", html.WarehouseID);
                    sqlResult = html.SaveChangeData(sql, false, para);
                    notificationTelegram.NotificationTelegramBotToTopic(Comments.Replace("<br>", "\n"), -1903890245, 7).Wait();//1903890245_7 = for technical new install
                    notificationTelegram.NotificationTelegramBotToTopic(Comments.Replace("<br>", "\n"), -1545132903, 2).Wait();//635916546 = send finance group new install
                    notificationTelegram.NotificationTelegramBot(Comments.Replace("<br>", "\n"), -954558193).Wait();//-954558193=NSM ISP NOTIFICATION FOR SALE
                }
            }
            if (!IsGenerateConnection)
            {
                //for notificationTelegram
                if (rst.Result == ResultType.Successful)
                {
                    CustomerID = int.Parse(CustID);
                    sql = "select top 1 cb.GrandTotal Amount, s.ServiceName, c.CustomerCode FirstName, COALESCE(NULLIF(ISNULL(c.CustomerName, ''), ''), c.CustomerNameLatin) LastName, bi.CompanyEmail Email, bi.CompanyContactMobileNumber Phone, bi.InstallProvinceCity [Location], 'Activation by: " + html.UserFullName + "; Activate Date: " + DateTime.Parse(ActivateDate).ToString("dd-MM-yyyy HH:mm:ss tt") + "' Comment ";
                    sql += ", b.PDFFileName, b.DestinationFolder, case when b.invTmp = 1 then b.ServiceName + ' - ' + b.PackageName else b.ServiceName end + case when b.FreeDuration <> 0 then ' <br>Free Duration ' + convert(varchar, b.FreeDuration) + ' Month(s)' else '' end PackageName, ";
                    sql += "format(b.StartDate, 'dd/MMM/yyyy') StartDate, format(b.EndDate, 'dd/MMM/yyyy') EndDate, b.TicketNumber, c.CustomerNationality, b.InvoiceNo, b.InvoiceDate, b.IsIncludedVAT, b.StatusID ";
                    sql += "from dbo.TblCustomer c ";
                    sql += "inner join dbo.TblCustomerBillInformation bi on bi.CustomerID = c.CustomerID ";
                    sql += "inner join dbo.TblCustomerBillingInvoice b on b.CustomerID = bi.CustomerID and bi.StatusID not in (12,13,14) ";//--and b.CustomerBillInformationID = bi.CustomerBillInformationID
                    sql += "inner join dbo.TblServices s on s.ServiceID = b.ServiceID ";
                    sql += "inner join dbo.TblCustomerBalance cb on cb.CustomerID = c.CustomerID and cb.InvoiceNo = b.InvoiceNo ";
                    sql += "where b.[SID] = '" + SID + "' and c.CustomerID = " + CustID + " and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID;
                    ds = html.LoadData(sql);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string StatusID = ds.Tables[0].Rows[0]["StatusID"].ToString();
                        string IsIncludedVAT = ds.Tables[0].Rows[0]["IsIncludedVAT"].ToString();
                        string InvoiceDate = ds.Tables[0].Rows[0]["InvoiceDate"].ToString();
                        InvoiceNo = ds.Tables[0].Rows[0]["InvoiceNo"].ToString();
                        string CustomerNationality = ds.Tables[0].Rows[0]["CustomerNationality"].ToString();
                        string ServiceName = ds.Tables[0].Rows[0]["ServiceName"].ToString();
                        FirstName = ds.Tables[0].Rows[0]["FirstName"].ToString();
                        LastName = ds.Tables[0].Rows[0]["LastName"].ToString();
                        Email = ds.Tables[0].Rows[0]["Email"].ToString();
                        Phone = ds.Tables[0].Rows[0]["Phone"].ToString();
                        Location = ds.Tables[0].Rows[0]["Location"].ToString();
                        string PDFFileName = ds.Tables[0].Rows[0]["PDFFileName"].ToString();
                        string DestinationFolder = ds.Tables[0].Rows[0]["DestinationFolder"].ToString();
                        string PackageName = ds.Tables[0].Rows[0]["PackageName"].ToString();
                        string StartDate = ds.Tables[0].Rows[0]["StartDate"].ToString();
                        string EndDate = ds.Tables[0].Rows[0]["EndDate"].ToString();
                        string Amount = ds.Tables[0].Rows[0]["Amount"].ToString();
                        Comment = ds.Tables[0].Rows[0]["Comment"].ToString();
                        string TicketNumber = ds.Tables[0].Rows[0]["TicketNumber"].ToString();
                        if (ActivateCustomerID == 0)
                        {
                            sql = "select * from dbo.TblActivateCustomer where [SID] = N'" + SID + "'";
                            ds = html.LoadData(sql);
                            DataRow dr = ds.Tables[0].Rows[0];
                            ActivateCustomerID = int.Parse(dr["ActivateCustomerID"].ToString());
                            string ConType = dr["ConnectionType"].ToString();
                            string PPPoEUserName = dr["PPPoEUserName"].ToString();
                            string PPPoEPassword = dr["PPPoEPassword"].ToString();
                            string VLanName = dr["VLanName"].ToString();
                            string VLanIPAddress = dr["VLanIPAddress"].ToString();
                            string CustomerIPAddress = dr["IPAddress"].ToString();
                            string GetwayIP = dr["GetwayIP"].ToString();
                            ActivateDate = dr["ActivateDate"].ToString();
                            string Comments = @"Dear Team, <br><br>Now Customer Name: <b>" + LastName
                                + @"</b> is <b>ACTIVATED</b> on date: <b>" + DateTime.Parse(ActivateDate).ToString("dd-MMM-yyyyy HH:mm:ss tt")
                                + @"</b> By: <b>" + html.UserFullName
                                + @"</b>.<br><br>Package Name: <b>" + PackageName + "</b><br><br>" + "Service ID: <b>" + SID + "</b><br>Connection ID: <b>" + CID + "</b><br><b>CID: " + FirstName + "</b><br>Start Date: <b>" + StartDate + "</b><br>Expired Date: <b>" + EndDate + "</b>";

                            //if (ConType == "PPPoE")
                            //{
                            //    Comments += @"<br><br>Connection Type: " + ConType + @"<br>PPPoE User Name: " + PPPoEUserName + @"<br>PPPoE Password: " + PPPoEPassword;
                            //}
                            //else if (ConType == "Static")
                            //{
                            //    Comments += @"<br><br>Connection Type: " + ConType + @"<br>VLan Name: " + VLanName + @"<br>VLan IP Address: " + VLanIPAddress + @"<br>Customer IP Address: " + CustomerIPAddress + @"<br>Getway IP: " + GetwayIP;
                            //}
                            Comments += @"<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO GENERATE TICKET.";

                            //                    sql = @"update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 40, [SID] = @SID,
                            //ActivateCustomerID = @ActivateCustomerID
                            //where ConnectionID = @ConnectionID and CustomerID = @CustomerID;
                            //update dbo.TblCustomerInvoice set ConnectivityStatus = 40 where ConnectionID = @ConnectionID and CustomerID = @CustomerID";
                            //                    para.Clear();
                            //                    para.Add("ActivateCustomerID", ActivateCustomerID.ToString());
                            //                    para.Add("SID", SID);
                            //                    para.Add("ConnectionID", ConnectionID);
                            //                    para.Add("CustomerID", CustID);
                            //                    html.SaveChangeData(sql, false, para);

                            sql = @"update dbo.TblActivateCustomer set TicketNumber = @TicketNumber where SID =@SID and SystemID = @SystemID and WarehouseID = @WarehouseID;
                insert into dbo.TblSupportOpenTicketComment (SupportProjectID, StatusID, [Status], PriorityID, [Priority], OpenTicketID, FromUserLoginID, FromUserName,
                ReplyFromUserLoginID, ReplyFromUserName, Comment, Date, CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus)
                select SupportProjectID, 4 StatusID, 'Feedback' [Status], 1 PriorityID, 'Normal' [Priority], 
                OpenTicketID, UserLoginID FromUserLoginID, CreateBy FromUserName, " + html.UserID + @" ReplyFromUserLoginID, '" + html.UserFullName + @"' ReplyFromUserName, 
                N'" + Comments.Replace("'", "''").Trim() + "' Comment, '" + ActivateDate + @"' [Date], '" + html.UserFullName + @"' CreateBy, '" + ActivateDate + @"'CreateDate, " + html.SystemID + " SystemID, " + html.WarehouseID + @" WarehouseID, 
                'Change Status to Feedback by " + html.UserFullName + @"' DescriptionChangeStatus
                from dbo.TblSupportOpenTicket
                where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID;
                update dbo.TblSupportOpenTicket set SID = @SID, StatusID = 4, Status= 'Feedback', PriorityID = 1, Priority = 'Normal' where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID";
                            para.Clear();
                            para.Add("SID", SID);
                            para.Add("TicketNumber", TicketNumber);
                            para.Add("SystemID", html.SystemID);
                            para.Add("WarehouseID", html.WarehouseID);
                            sqlResult = html.SaveChangeData(sql, false, para);

                            int isvat = IsIncludedVAT == "False" ? 0 : 1;
                            sqlResult = await html.InvoiceToPDFandUpload(LastName, FirstName, int.Parse(CustID), InvoiceNo, InvoiceDate, isvat, int.Parse(StatusID), int.Parse(html.SystemID.ToString()), int.Parse(html.WarehouseID.ToString()), false, "", "/ISP_Invoice", true);
                            //await notificationTelegram.NotificationTelegramBot((Comments + " " + TicketNumber).Replace("<br>", "\n"));//809264553 = for test
                            await notificationTelegram.NotificationTelegramBotToTopic((Comments + " " + TicketNumber).Replace("<br>", "\n"), -1903890245, 7);//1903890245_7 = for technical new install
                            await notificationTelegram.NotificationTelegramBotToTopic("", -1545132903, 2, null, 0, "", sqlResult.OutPutFile, (Comments + " " + TicketNumber).Replace("<br>", "\n"));//635916546 = send finance group new install
                            await notificationTelegram.NotificationTelegramBot((Comments + " " + TicketNumber).Replace("<br>", "\n"), -954558193);//-954558193=NSM ISP NOTIFICATION FOR SALE
                                                                                                                                                  //notification to customer
                            sql = "select IsEnable from dbo.TblTelegramNotification where StatusID = 40 and IsEnable = 1 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
                            ds = html.LoadData(sql);
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                //string msg = "Dear " + LastName + " [" + FirstName + "],\n\n";
                                //msg += "Greeting from NSM SOLUTION CO., LTD and thanks for using our services.\n\n";
                                //msg += "We would like to confirm your service is activated on date: <b>" + DateTime.Parse(html.GetCurrentDate()).ToString("dd-MMM-yyyyy HH:mm:ss tt")+ "</b>\n\n";
                                //msg += "Your current service: <b>" + ServiceName+"</b>";
                                //msg += "\nPackage Name: <b>" + PackageName+ "</b>";
                                //msg += "\n\nService ID: <b>" + SID + "</b>";
                                //msg += "\nConnection ID: <b>" + CID + "</b>";
                                //msg += "\nStart Date: <b>" + StartDate+ "</b>";
                                //msg += "\nExpired Date: <b>" + EndDate + "</b>";
                                //msg += "\n\nAmount USD: <b>" + Amount + "</b>";
                                //msg += "\nPhone Number: " + Phone;
                                //msg += "\n\nThank you and best regards,\n\n";
                                //msg += "NSM SOLUTION AND MAINTENANCE CO.,LTD\n\n";
                                //msg += "#1159No1, Street Number 2, Phum Prek Tanu, Sangkat Chak Angre Leu, Khan Mean Chey, Phnom Penh.\n\n";
                                //msg += "Customer Service: 086 588 777, 078 588 777\n";
                                //msg += "Website: nsmisp.com.kh\n";
                                string msg;
                                if (CustomerNationality.ToString().ToLower() == "khmer" || CustomerNationality == "ខ្មែរ")
                                {
                                    msg = "«សេចក្តីជូនដំណឹងអំពី អ្នកបានភ្ជាប់កញ្ចប់សេវាកម្មអ៊ិនធឺណិតអិនអេសអិម»\n\n";
                                    msg += "ឈ្មោះអតិថិជន: " + LastName + "\n";
                                    msg += "លេខសំគាល់អតិថិជន: " + FirstName + "\n";
                                    msg += "ឈ្មោះកញ្ចប់សេវាកម្មៈ <b>" + PackageName.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n") + "</b>\n\n";
                                    msg += "ថ្ងៃចាប់ផ្តើមៈ <b>" + StartDate + "</b>\n";
                                    msg += "ថ្ងៃផុតសុពលភាពៈ <b>" + EndDate + "</b>\n\n";
                                    //msg += "ទឹកប្រាក់ៈ USD <b>" + Amount + "</b>\n\n";
                                    msg += "សម្រាប់ពត៌មានបន្ថែមសូមទាក់ទង\nផ្នែកបម្រើអតិថិជន: +85586588777, +85578588777\n";
                                    msg += "ផ្នែកបច្ចេកទេសៈ +85561876777\n";
                                    //msg += "គេហទំព័រ: www.nsmisp.com.kh\n";
                                    msg += "\nសូមអរគុណ!";
                                }
                                else
                                {
                                    msg = "«Welcome to NSM ISP! Now your connection is activated.»\n\n";
                                    msg += "Customer Name: " + LastName + "\n";
                                    msg += "CID: " + FirstName + "\n";
                                    msg += "Package Name: <b>" + PackageName.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n") + "</b>\n\n";
                                    msg += "Start Date: <b>" + StartDate + "</b>\n";
                                    msg += "End Date: <b>" + EndDate + "</b>\n\n";
                                    msg += "Contact Information\nCustomer Services: +85586588777, +85578588777\n";
                                    msg += "Technical: +85561876777\n";
                                    //msg += "Website: www.nsmisp.com.kh\n";
                                    msg += "\nThank you!";
                                }
                                //msg += "NSM SOLUTION AND MAINTENANCE CO.,LTD\n\n";
                                //msg += "#1159No1, Street Number 2, Phum Prek Tanu, Sangkat Chak Angre Leu, Khan Mean Chey, Phnom Penh.\n\n";

                                //Phone = Phone.Trim().Replace(" ", string.Empty);
                                //var regex = new Regex(Regex.Escape("0"));
                                //Phone = regex.Replace(Phone, "+855", 1);
                                SqlResult result = new SqlResult();

                                sql = @"select ct.TelegramNotificationID, ct.CustomerPhone, ct.FirstName, ct.LastName, ct.CustomerID
from dbo.TblCustomerTelegram ct
where isnull(ct.TelegramNotificationID,'') <> ''
and 1 = (select [Data] from dbo.Split(ct.TelegramNotificationID, ',') where [Data] = 1)
and ct.CustomerID = " + CustomerID + " and ct.IsUsingTelegram = 'Yes'";
                                ds = html.LoadData(sql);
                                if (ds.Tables.Count > 0)
                                {
                                    if (ds.Tables[0].Rows.Count > 0)
                                    {
                                        foreach (DataRow drs in ds.Tables[0].Rows)
                                        {
                                            Phone = drs["CustomerPhone"].ToString();
                                            FirstName = drs["FirstName"].ToString();
                                            LastName = drs["LastName"].ToString();
                                            CustID = drs["CustomerID"].ToString();
                                            result = await notificationTelegram.NotificationToCustomer(Phone, FirstName, LastName, int.Parse(CustID), msg);
                                            string TelegramNotificationID = "1";
                                            sql = @"insert into dbo.TblCustomerTelegramMessage (DateSend, SendBy, CustomerID, ConnectionID, 
ActivateCustomerID, TelegramNotificationID, MessgeID, MessageText, MessageStatus, CustomerPhone)
values ('" + html.GetCurrentDate() + "','System', " + CustomerID + ",'" + ConnectionID + "'," + getActivateCustomerID + "," + TelegramNotificationID + ",'" + result.Value + "'," +
"N'" + msg + "', '" + result.Message + "', '" + Phone + "')";
                                            result = html.SaveChangeData(sql, false, false);
                                        }
                                    }
                                }
                            }
                            //end notification to customer
                        }
                    }
                }
            }
            
            //Update serial number on stock in detail
            SaveChangeSerialNumber(ActivateCustomerID, ConnectionID);

            MK.CloseMKConnection();
            return Json(new { Result = rst.Result, Message = rst.Message, Data = ActCustomerData });
        }

        public async Task<ActionResult> LockCustomerService(int ActivateCustomerID, int ConnectivityStatus, string Comment, object[] CustEquipmentData = null, object[] ActivateCustomerData = null)
        {
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            Comment = html.HtmlDecode(Comment);
            string sql = "select ac.RouterID, ac.PPPoEUserName, ac.ConnectionType, ac.VLanID, ac.ConnectivityStatus, ac.PPPoEProfileName, ac.[SID], ac.ConnectionID, c.CustomerCode, c.CustomerName from dbo.TblActivateCustomer ac ";
            sql += "inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID ";
            sql += "where ac.ActivateCustomerID = @ActivateCustomerID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ActivateCustomerID", ActivateCustomerID.ToString() }
            };
            DataSet ds = html.LoadData(sql, false, para);

            string RouterID = ds.Tables[0].Rows[0]["RouterID"].ToString();
            string PPPoEUserName = ds.Tables[0].Rows[0]["PPPoEUserName"].ToString();
            string ConnectionType = ds.Tables[0].Rows[0]["ConnectionType"].ToString();
            string VLanID = ds.Tables[0].Rows[0]["VLanID"].ToString();
            string Connectivity_Status = ds.Tables[0].Rows[0]["ConnectivityStatus"].ToString();
            string PPPoEProfileName = ds.Tables[0].Rows[0]["PPPoEProfileName"].ToString();
            string SID = ds.Tables[0].Rows[0]["SID"].ToString();
            string ConnectionID = ds.Tables[0].Rows[0]["ConnectionID"].ToString();
            string CustomerCode = ds.Tables[0].Rows[0]["CustomerCode"].ToString();
            string CustomerName = ds.Tables[0].Rows[0]["CustomerName"].ToString();
            Mikrotik mk = new Mikrotik();
            string Message = "", UserComment = "";

            mk.SetConnection(int.Parse(RouterID));
            if (ConnectionType == "PPPoE")
            {
                if (ConnectivityStatus == 41)
                {
                    UserComment = Comment;
                    Comment = Comment + "; Locked by:" + html.UserFullName + "; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                    sqlResult = mk.LockUser(PPPoEUserName, Comment);
                    if (sqlResult.Result == ResultType.Successful)
                    {
                        //CustomerProfileController customerProfileController = new CustomerProfileController();
                        //await customerProfileController.GetUserONUInfo(PPPoEUserName, 0, false, false, true);
                        //await html.GetUserONUInfo(PPPoEUserName, 0, false, false, true);
                    }
                }
                else if (ConnectivityStatus == 42)
                {
                    UserComment = Comment;
                    Comment = Comment + ";Terminated by:" + html.UserFullName + "; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                    sqlResult = mk.LockUser(PPPoEUserName, Comment);
                }
                else if (ConnectivityStatus == 40)
                {
                    UserComment = Comment;
                    Comment = Comment + "; Reactivate by:" + html.UserFullName + "; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                    if (Connectivity_Status == "50")
                    {
                        mk.ClearProfileName(PPPoEUserName);
                        sqlResult = mk.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
                    }
                    else
                    {
                        sqlResult = mk.ReActivateUser(PPPoEUserName, Comment);
                    }
                    if (sqlResult.Result == ResultType.Successful)
                    {
                        //CustomerProfileController customerProfileController = new CustomerProfileController();
                        //await customerProfileController.GetUserONUInfo(PPPoEUserName, 0, false, false, true);
                        await html.GetUserONUInfo(PPPoEUserName, 0, false, false, true);
                    }
                }
            }
            else if (ConnectionType == "Static")
            {
                if (ConnectivityStatus == 41)
                {
                    UserComment = Comment;
                    Comment = Comment + "; Locked by:" + html.UserFullName + "; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                    sqlResult = mk.DisableVlanID(VLanID, Comment);
                }
                else if (ConnectivityStatus == 42)
                {
                    UserComment = Comment;
                    Comment = Comment + "; Terminated by:" + html.UserFullName + "; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                    sqlResult = mk.DisableVlanID(VLanID, Comment);
                }
                else if (ConnectivityStatus == 40)
                {
                    UserComment = Comment;
                    Comment = Comment + "; Reactivate by:" + html.UserFullName + "; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                    sqlResult = mk.EnableVlanID(VLanID, Comment);

                }
            }
            else if (ConnectionType == "Manual")
            {
                if (ConnectivityStatus == 41)
                {
                    UserComment = Comment;
                    Comment = Comment + "; Locked by:" + html.UserFullName + "; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                }
                else if (ConnectivityStatus == 42)
                {
                    UserComment = Comment;
                    Comment = Comment + "; Terminated by:" + html.UserFullName + "; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                }
                else if (ConnectivityStatus == 40)
                {
                    UserComment = Comment;
                    Comment = Comment + "; Reactivate by:" + html.UserFullName + "; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                }
                sqlResult.Result = ResultType.Successful;
            }
            if (sqlResult.Result == ResultType.Successful)
            {
                Message = "This Connection ID: <b>" + ConnectionID + " [" + CustomerCode + " - " + CustomerName + "]</b> has been locked by <b>" + html.UserFullName + "</b>; Locked Date: <b>" + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + "</b>";
                if (ConnectivityStatus == 40)
                    Message = "This Connection ID: <b>" + ConnectionID + " [" + CustomerCode + " - " + CustomerName + "]</b> has been reactivated by <b>" + html.UserFullName + "</b>; Reactivate Date: <b>" + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + "</b>";
                if (ConnectivityStatus == 42)
                    Message = "This Connection ID: <b>" + ConnectionID + " [" + CustomerCode + " - " + CustomerName + "]</b> has been terminate by <b>" + html.UserFullName + "</b>; Terminate Date: <b>" + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + "</b>";
                if (ConnectivityStatus == 43)
                    Message = "This Connection ID: <b>" + ConnectionID + " [" + CustomerCode + " - " + CustomerName + "]</b> has been inactivate by <b>" + html.UserFullName + "</b>; Inactivate Date: <b>" + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt") + "</b>";

                //sql = "update dbo.TblCustomerBillingInvoice set ConnectivityStatus = " + ConnectivityStatus + ", ConnectivityStatusName= '" + Message + "' ";
                //sql += "where ActivateCustomerID = @ActivateCustomerID;";
                //sql += "update dbo.TblActivateCustomer set [Date]='" + html.GetCurrentDate() + @"', ConnectivityStatus = " + ConnectivityStatus + ", ConnectivityStatusName= '" + Message + "' ";
                //if (ConnectivityStatus == 40)
                //    sql += ", ReactivateBy='" + html.UserFullName + "', ReactivateDate='" + easternTimeNow.ToString() + "'";
                //else if (ConnectivityStatus == 41)
                //    sql += ", LockedBy='" + html.UserFullName + "', LockedDate='" + easternTimeNow.ToString() + "'";
                //else if (ConnectivityStatus == 42)
                //    sql += ", LockedBy='" + html.UserFullName + "', LockedDate='" + easternTimeNow.ToString() + "'";
                //sql += " where ActivateCustomerID = @ActivateCustomerID;";
                //sql += "select InvoiceNo from TblActivateCustomer where ActivateCustomerID = @ActivateCustomerID";
                //sql += ";";
                //sql += "insert into dbo.TblHistoryActivateCustomer (ConnectionType, CustomerID, ServiceID, ";
                //sql += "PackageDetailID, ActivateDate, ActivateBy, Comment, ";
                //sql += "StartDate, EndDate, Duration, ConnectivityStatus, ";
                //sql += "SystemID, WarehouseID, ActivateCustomerID, LockedDate, LockedBy, ConnectionID, [Date]) ";
                //sql += "select ConnectionType, CustomerID, ServiceID, ";
                //sql += "PackageDetailID, ActivateDate, '" + html.UserFullName + "' ActivateBy, N'" + Comment + "', ";
                //sql += "StartDate, EndDate, Duration, ConnectivityStatus, ";
                //sql += "SystemID, WarehouseID, ActivateCustomerID, LockedDate, LockedBy, ConnectionID, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' ";
                //sql += "from dbo.TblActivateCustomer where ActivateCustomerID = @ActivateCustomerID";
                //sqlResult = html.SaveChangeData(sql, false, para, true);

                // Use parameterized queries where possible
                var sqlsb = new StringBuilder();

                // 1. Update TblCustomerBillingInvoice
                sqlsb.Append("UPDATE dbo.TblCustomerBillingInvoice SET ");
                sqlsb.Append("ConnectivityStatus = @ConnectivityStatus, ");
                sqlsb.Append("ConnectivityStatusName = @Message ");
                sqlsb.Append("WHERE ActivateCustomerID = @ActivateCustomerID; ");

                // 2. Update TblActivateCustomer
                sqlsb.Append("UPDATE dbo.TblActivateCustomer SET ");
                sqlsb.Append("[Date] = @CurrentDate, ");
                sqlsb.Append("ConnectivityStatus = @ConnectivityStatus, ");
                sqlsb.Append("ConnectivityStatusName = @Message ");

                if (ConnectivityStatus == 40)
                {
                    sqlsb.Append(", ReactivateBy = @UserFullName, ReactivateDate = @Now ");
                }
                else if (ConnectivityStatus == 41 || ConnectivityStatus == 42)
                {
                    sqlsb.Append(", LockedBy = @UserFullName, LockedDate = @Now ");
                }

                sqlsb.Append("WHERE ActivateCustomerID = @ActivateCustomerID; ");

                // 3. Select InvoiceNo
                sqlsb.Append("SELECT InvoiceNo FROM TblActivateCustomer WHERE ActivateCustomerID = @ActivateCustomerID; ");

                // 4. Insert into history
                sqlsb.Append(@"INSERT INTO dbo.TblHistoryActivateCustomer (
    ConnectionType, CustomerID, ServiceID, PackageDetailID, 
    ActivateDate, ActivateBy, Comment, StartDate, EndDate, Duration, 
    ConnectivityStatus, SystemID, WarehouseID, ActivateCustomerID, 
    LockedDate, LockedBy, ConnectionID, [Date])
    SELECT 
        ConnectionType, CustomerID, ServiceID, PackageDetailID, 
        ActivateDate, @UserFullName, @Comment, 
        StartDate, EndDate, Duration, 
        ConnectivityStatus, SystemID, WarehouseID, ActivateCustomerID, 
        LockedDate, LockedBy, ConnectionID, @NowFormatted 
    FROM dbo.TblActivateCustomer 
    WHERE ActivateCustomerID = @ActivateCustomerID;");
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "ConnectivityStatus", ConnectivityStatus.ToString() },
                    { "Message", Message },
                    { "ActivateCustomerID", ActivateCustomerID.ToString() },
                    { "UserFullName", html.UserFullName },
                    { "CurrentDate", html.GetCurrentDate() },
                    { "Now", easternTimeNow.ToString() },
                    { "NowFormatted", easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") },
                    { "Comment", Comment }
                };

                // Execute
                sqlResult = html.SaveChangeData(sqlsb.ToString(), false, parameters, true);
                // Prepare the SQL query with parameters
                sql = @"UPDATE dbo.TblCustomerInvoice 
SET ConnectivityStatus = @ConnectivityStatus, 
    ConnectivityStatusName = @Message 
WHERE InvoiceNo = @InvoiceNo AND ConnectionID = @ConnectionID;
";

                // Prepare parameters
                para.Clear();
                para.Add("InvoiceNo", sqlResult.Value);
                para.Add("ConnectivityStatus", ConnectivityStatus.ToString());
                para.Add("Message", Message);
                para.Add("ConnectionID", ConnectionID);

                // Execute query
                sqlResult = html.SaveChangeData(sql, false, para);


                //sql = "update dbo.TblCustomerInvoice set ConnectivityStatus = " + ConnectivityStatus + ", ConnectivityStatusName= '" + Message + "' ";
                //sql += "where InvoiceNo = @InvoiceNo and ConnectionID='" + ConnectionID + "';";
                //para.Clear();
                //para.Add("InvoiceNo", sqlResult.Value);
                //sqlResult = html.SaveChangeData(sql, false, para);
            }

            sql = "select * from dbo.TblActivateCustomer where ActivateCustomerID = " + ActivateCustomerID;
            ds = html.LoadData(sql);
            DataRow dr = ds.Tables[0].Rows[0];
            string ConType = dr["ConnectionType"].ToString();
            //string PPPoEUserName = dr["PPPoEUserName"].ToString();
            string PPPoEPassword = dr["PPPoEPassword"].ToString();
            string VLanName = dr["VLanName"].ToString();
            string VLanIPAddress = dr["VLanIPAddress"].ToString();
            string CustomerIPAddress = dr["IPAddress"].ToString();
            string GetwayIP = dr["GetwayIP"].ToString();
            string TicketNumber = dr["TicketNumber"].ToString();
            string Comments = @"Dear Team, <br><br>" + Message + ".";
            Comments += @"<br><br>User Comment: " + UserComment;
            Comments += @"<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO GENERATE TICKET.";
            sql = @"
INSERT INTO dbo.TblSupportOpenTicketComment (
    SupportProjectID, StatusID, Status, PriorityID, Priority, 
    OpenTicketID, FromUserLoginID, FromUserName, 
    ReplyFromUserLoginID, ReplyFromUserName, Comment, [Date], 
    CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus
)
SELECT 
    SupportProjectID, StatusID, [Status], PriorityID, [Priority], 
    OpenTicketID, UserLoginID AS FromUserLoginID, CreateBy AS FromUserName, 
    @ReplyFromUserLoginID AS ReplyFromUserLoginID, 
    @ReplyFromUserName AS ReplyFromUserName, 
    @Comment AS Comment, 
    @Now AS [Date], 
    @ReplyFromUserName AS CreateBy, 
    @Now AS CreateDate, 
    @SystemID AS SystemID, 
    @WarehouseID AS WarehouseID, 
    '' AS DescriptionChangeStatus
FROM dbo.TblSupportOpenTicket
WHERE TicketNumber = @TicketNumber 
    AND SystemID = @SystemID 
    AND WarehouseID = @WarehouseID;
";

            // Build parameter list
            para.Clear();
            para.Add("TicketNumber", TicketNumber);
            para.Add("SystemID", html.SystemID);
            para.Add("WarehouseID", html.WarehouseID);
            para.Add("ReplyFromUserLoginID", html.UserID);
            para.Add("ReplyFromUserName", html.UserFullName);
            para.Add("Comment", Comments);
            para.Add("Now", html.GetCurrentDate());

            // Execute
            sqlResult = html.SaveChangeData(sql, false, para);

            //sql = @"insert into dbo.TblSupportOpenTicketComment (SupportProjectID, StatusID,
            //    Status, PriorityID, Priority, OpenTicketID, FromUserLoginID, FromUserName,
            //    ReplyFromUserLoginID, ReplyFromUserName, Comment, Date, CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus)
            //    select SupportProjectID, StatusID, [Status], PriorityID, [Priority], 
            //    OpenTicketID, UserLoginID FromUserLoginID, CreateBy FromUserName, " + html.UserID + @" ReplyFromUserLoginID, '" + html.UserFullName + @"' ReplyFromUserName, 
            //    N'" + Comments + "' Comment, '" + html.GetCurrentDate() + @"' [Date], '" + html.UserFullName + @"' CreateBy, '" + html.GetCurrentDate() + @"'CreateDate, " + html.SystemID + " SystemID, " + html.WarehouseID + @" WarehouseID, 
            //    '' DescriptionChangeStatus
            //    from dbo.TblSupportOpenTicket
            //    where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID;";
            //para.Clear();
            //para.Add("TicketNumber", TicketNumber);
            //para.Add("SystemID", html.SystemID);
            //para.Add("WarehouseID", html.WarehouseID);
            //sqlResult = html.SaveChangeData(sql, false, para);
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            await notificationTelegram.NotificationTelegramBotToTopic((Comments + " " + TicketNumber).Replace("<br>", "\n"), -1903890245, 4); ///-1903890245_4 for technical support locked connection
			await notificationTelegram.NotificationTelegramBotToTopic((Comments + " " + TicketNumber).Replace("<br>", "\n"), -1545132903, 4);//635916546 = test send finance group locked connection
                                                                                                                                             //if (CustEquipmentData != null)
                                                                                                                                             //{
                                                                                                                                             //    Dictionary<string, object> obj = new Dictionary<string, object>();
                                                                                                                                             //    dynamic Equipment = new JavaScriptSerializer().DeserializeObject(CustEquipmentData[0].ToString());
                                                                                                                                             //    dynamic ActivateCustomer = new JavaScriptSerializer().DeserializeObject(ActivateCustomerData[0].ToString());
                                                                                                                                             //    //int RouterID = int.Parse(ActivateCustomer[0]["RouterID"].ToString());
                                                                                                                                             //    //string ConnectionType = ActivateCustomer[0]["ConnectionType"];
                                                                                                                                             //    string ReActivate = ActivateCustomer[0]["ISReActivate"].ToString();
                                                                                                                                             //    //bool ISReActivate = bool.Parse(ReActivate);
                                                                                                                                             //    //string VLanID = "";
                                                                                                                                             //    //string ChangeConnectionTypeComment = "";
                                                                                                                                             //    //int ActivateCustomerID = int.Parse(ActivateCustomer[0]["ActivateCustomerID"].ToString());
                                                                                                                                             //    ActivateCustomer[0]["SystemID"] = html.SystemID;
                                                                                                                                             //    ActivateCustomer[0]["WarehouseID"] = html.WarehouseID;
                                                                                                                                             //    ActivateCustomer[0]["ActivateBy"] = html.UserFullName;
                                                                                                                                             //    ActivateCustomer[0]["ActivateDate"] = html.GetCurrentDate();
                                                                                                                                             //    ActivateCustomer[0]["ConnectivityStatus"] = 40;
                                                                                                                                             //    ActivateCustomer[0]["ConnectivityStatusName"] = "Active";
                                                                                                                                             //    obj.Add("Equipment", Equipment);
                                                                                                                                             //    obj.Add("ActivateCustomer", ActivateCustomer);
                                                                                                                                             //    para.Clear();
                                                                                                                                             //    para.Add("Data", html.XMLDataObj(obj));
                                                                                                                                             //    sqlResult = html.SaveChangeData("SP_INSERT_ACTIVATE_CUSTOMER", true, para, true, true);
                                                                                                                                             //}
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ApplyChangePlanNow(string InvoiceNo, string ProfileName = "")
        {
            HTML html = new HTML();
            SqlResult sqlresult = new SqlResult();
            DateTime timeNow = DateTime.Now;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime easternTimeNow = TimeZoneInfo.ConvertTime(timeNow, TimeZoneInfo.Local, easternZone);
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            string sql = "select b.CustomerID, b.BillingInvoiceID, b.InvoiceType, b.PPPoEUserName, b.ConnectivityStatus, b.StartDate, b.EndDate, b.Duration, isnull(b.IsAlreadyApplyProfile, 0) IsAlreadyApplyProfile, b.ActivateCustomerID, b.IsChangePlan, case when b.invTmp = 1 then b.PackageName else REPLACE(b.ServiceName, s.ServiceName+' - ', '') end PackageName  ";
            sql += "from dbo.TblCustomerBillingInvoice b ";
            sql += "inner join dbo.TblServices s on s.ServiceID = b.ServiceID ";
            sql += "where b.InvoiceNo = N'" + InvoiceNo + "' and b.SystemID = " + html.SystemID + " and b.WarehouseID = " + html.WarehouseID;
            DataSet ds = html.LoadData(sql);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string CustomerID = dr["CustomerID"].ToString();
                    string InvoiceType = dr["InvoiceType"].ToString();
                    string BillingInvoiceID = dr["BillingInvoiceID"].ToString();
                    string StartDate = dr["StartDate"].ToString();
                    string EndDate = dr["EndDate"].ToString();
                    string Duration = dr["Duration"].ToString();
                    string ActivateCustomerID = dr["ActivateCustomerID"].ToString();
                    string IsAlreadyApplyProfile = dr["IsAlreadyApplyProfile"].ToString();
                    string IsChangePlan = dr["IsChangePlan"].ToString();
                    string PackageName = dr["PackageName"].ToString();
                    sql = "select RouterID, PPPoEUserName, PPPoEPassword, PPPoEProfileName, ConnectivityStatus, ConnectionType, VLanID, VLanName, format(RenewDate, 'yyyy-MM-dd') RenewDate, [SID], ActivateDate from dbo.TblActivateCustomer where ";
                    sql += "ActivateCustomerID =" + ActivateCustomerID;
                    ds = html.LoadData(sql);
                    DateTime SDate = DateTime.Parse(StartDate);
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string ServiceID = ds.Tables[0].Rows[0]["SID"].ToString();
                        string ActivateDate = ds.Tables[0].Rows[0]["ActivateDate"].ToString();
                        string RouterID = ds.Tables[0].Rows[0]["RouterID"].ToString();
                        string VLanID = ds.Tables[0].Rows[0]["VLanID"].ToString();
                        string VLanName = ds.Tables[0].Rows[0]["VLanName"].ToString();
                        string ConnectionType = ds.Tables[0].Rows[0]["ConnectionType"].ToString();
                        string ConnectivityStatus = ds.Tables[0].Rows[0]["ConnectivityStatus"].ToString();
                        string PPPoEProfileName = ds.Tables[0].Rows[0]["PPPoEProfileName"].ToString();
                        string RenewDate = ds.Tables[0].Rows[0]["RenewDate"].ToString();
                        string Comment = "";
                        string PPPoEUserName = ds.Tables[0].Rows[0]["PPPoEUserName"].ToString();
                        string PPPoEPassword = ds.Tables[0].Rows[0]["PPPoEPassword"].ToString();
                        Mikrotik MK = new Mikrotik();
                        MK.SetConnection(int.Parse(RouterID));
                        if (ConnectionType == "PPPoE")
                        {
                            if (IsChangePlan == "True" && IsAlreadyApplyProfile == "False")
                            {
                                //if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                //{
                                Comment = "Customer Change Plan From: <b>" + PPPoEProfileName + "</b> TO <b>" + PackageName + "</b>; Apply Changed by: <b>" + html.UserFullName + "</b>; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                                PPPoEProfileName = PackageName;
                                //MK.ClearProfileName(PPPoEUserName);
                                if (ProfileName != "")
                                    PPPoEProfileName = ProfileName;

                                sqlresult = MK.AppendProfileToUser(PPPoEUserName, PPPoEProfileName, Comment);
                                if (sqlresult.Result == ResultType.Successful)
                                {
                                    //if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                                    //{
                                    sql = "update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 40, ConnectionType = '" + ConnectionType + "', RenewBy = '" + html.UserFullName + "', RenewDate = '" + html.GetCurrentDate() + "', [ExpireDate] = '" + EndDate + "',";
                                    sql += "RouterID=" + RouterID + ", [SID] = '" + ServiceID + "', PPPoEPassword='" + PPPoEPassword + "', ActivateDate ='" + ActivateDate + "',ConnectivityStatusName = 'Active', IsAlreadyApplyProfile = 1 where InvoiceNo = '" + InvoiceNo + "'";
                                    sql += ";update dbo.TblCustomerInvoice set ConnectivityStatus = 40, ConnectivityStatusName = 'Active', RenewBy = '" + html.UserFullName + "', RenewDate = '" + html.GetCurrentDate() + "' ";
                                    sql += "where InvoiceNo = '" + InvoiceNo + "' and SystemID=" + html.SystemID + " and WarehouseID=" + html.WarehouseID + ";";
                                    sql += "update dbo.TblActivateCustomer set IsApplyChangePlan = 1, BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                                    sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', StartDate = '" + StartDate + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', RenewBy='" + html.UserFullName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                                    sql += ";";
                                    sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, BillingInvoiceID, ConnectionType, ";
                                    sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                                    sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                                    sql += "select ac.ActivateCustomerID, ac.BillingInvoiceID, ac.ConnectionType, ac.CustomerID, ";
                                    sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, '" + html.UserFullName + "' ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' LockedDate, ";
                                    sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                                    sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' from dbo.TblActivateCustomer ac ";
                                    sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                                    sqlresult = html.SaveChangeData(sql, false, false);

                                    sql = "select * from dbo.TblActivateCustomer where ActivateCustomerID = " + ActivateCustomerID;
                                    ds = html.LoadData(sql);
                                    DataRow drdata = ds.Tables[0].Rows[0];
                                    string TicketNumber = drdata["TicketNumber"].ToString();
                                    string Comments = @"Dear Team, <br><br>" + Comment + ".";

                                    Comments += @"<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO GENERATE TICKET.";

                                    sql = @"insert into dbo.TblSupportOpenTicketComment (SupportProjectID, StatusID,
                                            Status, PriorityID, Priority, OpenTicketID, FromUserLoginID, FromUserName,
                                            ReplyFromUserLoginID, ReplyFromUserName, Comment, Date, CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus)
                                            select SupportProjectID, StatusID, [Status], PriorityID, [Priority], 
                                            OpenTicketID, UserLoginID FromUserLoginID, CreateBy FromUserName, " + html.UserID + @" ReplyFromUserLoginID, '" + html.UserFullName + @"' ReplyFromUserName, 
                                            N'" + Comments + "' Comment, '" + html.GetCurrentDate() + @"' [Date], '" + html.UserFullName + @"' CreateBy, '" + html.GetCurrentDate() + @"'CreateDate, " + html.SystemID + " SystemID, " + html.WarehouseID + @" WarehouseID, 
                                            '' DescriptionChangeStatus
                                            from dbo.TblSupportOpenTicket
                                            where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID;";
                                    Dictionary<string, string> para = new Dictionary<string, string>
                                    {
                                        { "TicketNumber", TicketNumber },
                                        { "SystemID", html.SystemID },
                                        { "WarehouseID", html.WarehouseID }
                                    };
                                    html.SaveChangeData(sql, false, para);
                                    await notificationTelegram.NotificationTelegramBotToTopic((Comments + " " + TicketNumber).Replace("<br>", "\n"), -1903890245, 13);
                                    await notificationTelegram.NotificationTelegramBotToTopic((Comments + " " + TicketNumber).Replace("<br>", "\n"), -1545132903, 8);//635916546 = test send finance group
                                                                                                                                                                     //}
                                }
                                //}
                            }
                        }
                        else if (IsChangePlan == "True" && IsAlreadyApplyProfile == "False")
                        {
                            //if (easternTimeNow.ToString("yyyyMMdd") == SDate.ToString("yyyyMMdd"))
                            //{
                            Comment = "Customer Change Plan From: <b>" + PPPoEProfileName + "</b> TO <b>" + PackageName + "</b>; Apply Changed by: <b>" + html.UserFullName + "</b>; " + easternTimeNow.ToString("dd-MM-yyyy HH:mm:ss tt");
                            sql = "update dbo.TblCustomerBillingInvoice set ConnectivityStatus = 40, ConnectionType = '" + ConnectionType + "', RenewBy = '" + html.UserFullName + "', RenewDate = '" + html.GetCurrentDate() + "', [ExpireDate] = '" + EndDate + "',";
                            sql += "RouterID=" + RouterID + ", [SID] = '" + ServiceID + "', PPPoEPassword='" + PPPoEPassword + "', ActivateDate ='" + ActivateDate + "',ConnectivityStatusName = 'Active', IsAlreadyApplyProfile = 1 where InvoiceNo = '" + InvoiceNo + "'";
                            sql += ";update dbo.TblCustomerInvoice set ConnectivityStatus = 40, ConnectivityStatusName = 'Active', RenewBy = '" + html.UserFullName + "', RenewDate = '" + html.GetCurrentDate() + "' ";
                            sql += "where InvoiceNo = '" + InvoiceNo + "' and SystemID=" + html.SystemID + " and WarehouseID=" + html.WarehouseID + ";";
                            sql += "update dbo.TblActivateCustomer set IsApplyChangePlan = 1, BillingInvoiceID = " + BillingInvoiceID + ", ConnectivityStatus = 40, ConnectivityStatusName='" + Comment + "' ";
                            sql += ", PPPoEProfileName = '" + PPPoEProfileName + "', StartDate = '" + StartDate + "', EndDate='" + EndDate + "', Duration=" + Duration + ", RenewDate='" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', RenewBy='" + html.UserFullName + "' where ActivateCustomerID = '" + ActivateCustomerID + "'";
                            sql += ";";
                            sql += "insert into dbo.TblHistoryActivateCustomer (ActivateCustomerID, BillingInvoiceID, ConnectionType, ";
                            sql += "CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment, LockedDate, LockedBy, ";
                            sql += "StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, RenewDate, [Date]) ";
                            sql += "select ac.ActivateCustomerID, ac.BillingInvoiceID, ac.ConnectionType, ac.CustomerID, ";
                            sql += "ac.ServiceID, ac.PackageDetailID, ac.ActivateDate, '" + html.UserFullName + "' ActivateBy, N'" + Comment + "' Comment, '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' LockedDate, ";
                            sql += "'System' LockedBy, ac.StartDate, ac.EndDate, ac.Duration, 40 ConnectivityStatus, ac.SystemID, ac.WarehouseID ";
                            sql += ", '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "', '" + easternTimeNow.ToString("yyyy-MM-dd HH:mm:ss tt") + "' from dbo.TblActivateCustomer ac ";
                            sql += "where ac.ActivateCustomerID = " + ActivateCustomerID;
                            sqlresult = html.SaveChangeData(sql, false, false);

                            sql = "select * from dbo.TblActivateCustomer where ActivateCustomerID = " + ActivateCustomerID;
                            ds = html.LoadData(sql);
                            DataRow drdata = ds.Tables[0].Rows[0];
                            string TicketNumber = drdata["TicketNumber"].ToString();
                            string Comments = @"Dear Team, <br><br>" + Comment + ".";

                            Comments += @"<br><br>Thank you and best regards,<br><br>NSM ISP BILLING SYSTEM AUTO GENERATE TICKET.";

                            sql = @"insert into dbo.TblSupportOpenTicketComment (SupportProjectID, StatusID,
                                            Status, PriorityID, Priority, OpenTicketID, FromUserLoginID, FromUserName,
                                            ReplyFromUserLoginID, ReplyFromUserName, Comment, Date, CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus)
                                            select SupportProjectID, StatusID, [Status], PriorityID, [Priority], 
                                            OpenTicketID, UserLoginID FromUserLoginID, CreateBy FromUserName, " + html.UserID + @" ReplyFromUserLoginID, '" + html.UserFullName + @"' ReplyFromUserName, 
                                            N'" + Comments + "' Comment, '" + html.GetCurrentDate() + @"' [Date], '" + html.UserFullName + @"' CreateBy, '" + html.GetCurrentDate() + @"'CreateDate, " + html.SystemID + " SystemID, " + html.WarehouseID + @" WarehouseID, 
                                            '' DescriptionChangeStatus
                                            from dbo.TblSupportOpenTicket
                                            where TicketNumber = @TicketNumber and SystemID = @SystemID and WarehouseID = @WarehouseID;";
                            Dictionary<string, string> para = new Dictionary<string, string>
                            {
                                { "TicketNumber", TicketNumber },
                                { "SystemID", html.SystemID },
                                { "WarehouseID", html.WarehouseID }
                            };
                            html.SaveChangeData(sql, false, para);
                            await notificationTelegram.NotificationTelegramBotToTopic((Comments + " " + TicketNumber).Replace("<br>", "\n"), -1903890245, 13); // technical renew
                            await notificationTelegram.NotificationTelegramBotToTopic((Comments + " " + TicketNumber).Replace("<br>", "\n"), -1545132903, 8);//635916546 = test send finance group renew
                                                                                                                                                             //}
                        }
                    }
                }
            }
            return Json(new
            {
                Message = sqlresult.Message,
                Result = sqlresult.Result
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadVlanName(string ActivateCustomerID, int RouterID)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            StringBuilder sb = new StringBuilder();
            Mikrotik MK = new Mikrotik();
            DataTable dtVlan = null;
            MK.SetConnection(RouterID);
            sqlResult = MK.GetVLanInfo();
            if (sqlResult.Result == ResultType.Successful)
            {
                sb.AppendLine("<select id=\"VLanID\" name=\"VLanID\" class=\"form-control select2\" style=\"width: 100%;\">");
                //string sql = "select VLanID [ID], VLanName [Name] from dbo.TblActivateCustomer where ConnectionType = 'Static' and ActivateCustomerID <> " + ActivateCustomerID + " and ConnectivityStatus = 40";
                string sql = $"select VLanID [ID], VLanName [Name] from dbo.TblActivateCustomer where ConnectionType = 'Static' and RouterID = {RouterID} and ConnectivityStatus <> 40";
                DataSet ds = html.LoadData(sql);
                dtVlan = sqlResult.DataTable;
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var diffVlans = ds.Tables[0].AsEnumerable()
                        .Where(r => !dtVlan.AsEnumerable()
                            .Any(d => d["ID"].ToString() == r["ID"].ToString()));

                    foreach (var row in diffVlans)
                    {
                        dtVlan.ImportRow(row);
                    }
                }
                sb.AppendLine("<option value=\"\"></option>");
                foreach (DataRow dr in dtVlan.Rows)
                {
                    sb.AppendLine("<option value=\"" + dr["ID"] + "\">" + dr["Name"] + "</option>");
                }
                sb.AppendLine("<option value=\"1\">Other</option>");
                sb.AppendLine("</select>");
                MK.CloseMKConnection();
            }
            var a = html.JsonConvertDataTable(dtVlan);
            return Json(new { Data = sb.ToString(), VLanData = a }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadVlanIPAddress(string VlanName, int RouterID)
        {
            SqlResult sqlResult = new SqlResult();
            Mikrotik MK = new Mikrotik();
            MK.SetConnection(RouterID);
            sqlResult = MK.GetVlanAddressInfo(VlanName);
            if (sqlResult.Result == ResultType.Successful)
                MK.CloseMKConnection();
            return Json(new { AddressIP = sqlResult.DictionaryList.Keys, Network = sqlResult.DictionaryList.Values }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckIPIsExisiting(string oldIP, string NewIP, int RouterID)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult
            {
                Result = ResultType.Successful
            };
            DataSet ds = null;
            if (oldIP != NewIP)
            {
                string sql = @"select ac.IPAddress, c.CustomerCode from dbo.TblActivateCustomer ac
inner join dbo.TblCustomer c on c.CustomerID = ac.CustomerID
where ac.IPAddress ='" + NewIP + "' and ac.RouterID=" + RouterID + " and ac.ConnectivityStatus = 40 and ac.SystemID=" + html.SystemID + " and ac.WarehouseID = " + html.WarehouseID;
                ds = html.LoadData(sql);
            }
            if (ds != null)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    sqlResult.Result = ResultType.Failed;
                    sqlResult.Message = "IP Address " + ds.Tables[0].Rows[0]["IPAddress"] + " is already for CID: " + ds.Tables[0].Rows[0]["CustomerCode"] + " please change.";
                }
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PPPoEUser(int ActivateCustomerID, int RouterID)
        {
            HTML html = new HTML
            {
                FormID = "frmPPPoEUser",
                SaveChangeFunction = "SaveChangePPPoEUser()"
            };
            string sql = @"select ActivateCustomerID, ConnectionType, PPPoEUserName, PPPoEPassword, [SID]
	        from dbo.TblActivateCustomer where ActivateCustomerID = @ActivateCustomerID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ActivateCustomerID", ActivateCustomerID.ToString() }
            };
            DataSet ds = html.LoadData(sql, false, para);
            DataRow dr = ds.Tables[0].Rows[0];
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "SID:",
                ID = "SID",
                DBField = "SID",
                Required = true,
                Value = dr["SID"].ToString(),
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "User Name:",
                ID = "PPPoEUserName",
                DBField = "PPPoEUserName",
                Required = true,
                Value = dr["PPPoEUserName"].ToString(),
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Password:",
                ID = "PPPoEPassword",
                DBField = "PPPoEPassword",
                Required = true,
                Value = dr["PPPoEPassword"].ToString(),
                IsReadOnly = true
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            ViewBag.RouterID = RouterID.ToString();
            ViewBag.ActivateCustomerID = ActivateCustomerID;
            return View();
        }
        public ActionResult SaveChangePPPoEUser(object[] PrimaryFieldValue)
        {
            bool AllowSaveChange = false;
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            Mikrotik MK = new Mikrotik();
            Dictionary<string, string> data = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(PrimaryFieldValue[0].ToString());
            string ActivateCustomerID = data["ActivateCustomerID"];
            string oldPPPoEUserName = data["oldPPPoEUserName"];
            string oldPPPoEPassword = data["oldPPPoEPassword"];
            string PPPoEUserName = data["PPPoEUserName"];
            string PPPoEPassword = data["PPPoEPassword"];
            if (oldPPPoEUserName != PPPoEUserName)
                AllowSaveChange = true;
            if (oldPPPoEPassword != PPPoEPassword)
                AllowSaveChange = true;
            if (AllowSaveChange)
            {
                int RouterID = int.Parse(data["RouterID"].ToString());
                MK.SetConnection(RouterID);
                sqlResult = MK.UpdatePPPoEUser(oldPPPoEUserName, PPPoEUserName, PPPoEPassword);
                if (sqlResult.Result == ResultType.Successful)
                {
                    string sql = "update dbo.TblActivateCustomer set PPPoEUserName = @PPPoEUserName, PPPoEPassword = @PPPoEPassword where ActivateCustomerID = @ActivateCustomerID;";
                    sql += "update dbo.TblCustomerBillingInvoice set PPPoEUserName = @PPPoEUserName, PPPoEPassword = @PPPoEPassword where ActivateCustomerID = @ActivateCustomerID";
                    Dictionary<string, string> para = new Dictionary<string, string>
                    {
                        { "PPPoEUserName", PPPoEUserName },
                        { "PPPoEPassword", PPPoEPassword },
                        { "ActivateCustomerID", ActivateCustomerID }
                    };
                    sqlResult = html.SaveChangeData(sql, false, para);
                }
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message });
        }
        public void SaveChangeSerialNumber(int ActivateCustomerID, string ConnectionID)
        {
            HTML html = new HTML();
            DataSet ds = new DataSet();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ActivateCustomerID", ActivateCustomerID.ToString() }
            };
            string sql = @"if(not exists(select top 1 * 
 from dbo.TblStockOut so
 inner join dbo.TblStockInDetail sd on sd.StockOutID = so.StockOutID
 where so.ActivateCustomerID = @ActivateCustomerID and so.ConnectionID = '" + ConnectionID + @"'))
 begin
    insert into dbo.TblStockInDetail(ActivateCustomerID, StockOutID, ItemID, ItemName, ItemDescription, [Type], StockOutDate, CustomerID, InvoiceNumber, LocationID, StockOutType, SystemID, WarehouseID, UomID, UM, UnitPrice, QTYOUT)
    select so.ActivateCustomerID, so.StockOutID, so.ItemID, i.ItemName, i.ItemDescription, 'Stock Out' [Type], '" + html.GetCurrentDate() + @"' StockOutDate, so.CustomerID, so.InvoiceNumber, so.LocationID, so.StockOutType, so.SystemID, so.WarehouseID, so.UomID, so.UM, so.UnitPrice, so.QTY QTYOUT
    from dbo.TblStockOut so
    inner join dbo.TblItem i on i.ItemID = so.ItemID
    where so.ActivateCustomerID = @ActivateCustomerID and so.ConnectionID = '" + ConnectionID + @"'
end
";
            html.SaveChangeData(sql, false, para);

            sql = @"select ac.ActivateCustomerID, cd.ItemID, cd.ItemName,cd.ItemSerialDetailID, cd.SerialNo, um.UomID, cd.UM, cd.QTY, cd.LocationID
from dbo.TblActivateCustomer ac
inner join dbo.TblCustomerDeviceInfoDetail cd on cd.ActivateCustomerID = ac.ActivateCustomerID
inner join dbo.TblItemVariant um on um.UM = cd.UM and um.ItemID = cd.ItemID 
where ac.ActivateCustomerID = @ActivateCustomerID";

            ds = html.LoadData(sql, false, para);
            if (ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int ItemID = int.Parse(dr["ItemID"].ToString());
                    int SerialNumberID = 0;
                    if (dr["ItemSerialDetailID"].ToString() != "")
                        SerialNumberID = int.Parse(dr["ItemSerialDetailID"].ToString());
                    int WID = int.Parse(dr["LocationID"].ToString());
                    string SerialNumber = dr["SerialNo"].ToString();
                    int UomID = int.Parse(dr["UomID"].ToString());
                    double qty = double.Parse(dr["QTY"].ToString());

                    var data = html.GetUnitCal(ItemID, UomID, qty, 0);


                    if (!string.IsNullOrEmpty(SerialNumber))
                    {
                        para.Clear();
                        para.Add("ItemID", ItemID.ToString());
                        para.Add("WID", WID.ToString());
                        para.Add("ActivateCustomerID", ActivateCustomerID.ToString());

                        sql = @"select sd.StockInDetailID, sd.ActivateCustomerID, sd.LocationID, sd.ItemID, sd.SerialNumberID, sd.SerialNumber
from dbo.TblStockInDetail sd
where sd.ActivateCustomerID = @ActivateCustomerID and sd.ItemID = @ItemID and sd.LocationID = @WID";

                        DataSet dsSerial = html.LoadData(sql, false, para);
                        if (dsSerial.Tables.Count > 0)
                        {
                            if (dsSerial.Tables[0].Rows.Count > 0)
                            {
                                var rows = dsSerial.Tables[0].Rows;
                                foreach (DataRow row in rows)
                                {
                                    sql = @"update dbo.TblStockInDetail set SerialNumberID = @SerialNumberID, SerialNumber = @SerialNumber, QtyCalOut = @QtyCalOut, UomID = @UomID where StockInDetailID = @StockInDetailID;
									update dbo.TblItemSerialDetail set IsAvailable = 0, TransactionType = 'Stock Out' where ItemID = @ItemID and SerialNumber = @SerialNumber";
                                    para.Clear();
                                    para.Add("ItemID", row["ItemID"].ToString());
                                    para.Add("SerialNumberID", SerialNumberID.ToString());
                                    para.Add("SerialNumber", SerialNumber);
                                    para.Add("StockInDetailID", row["StockInDetailID"].ToString());
                                    para.Add("UomID", UomID.ToString());
                                    para.Add("QtyCalOut", data["QtyCal"].ToString());
                                    html.SaveChangeData(sql, false, para);
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}