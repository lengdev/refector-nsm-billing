using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Customers
{
    public class SurveyCustomerController : Controller
    {
        // GET: SurveyCustomer
        [Authorize(Roles = "Add Survey Customer, Edit Survey Customer, Delete Survey Customer")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Survey Customer"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Survey Customer"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Survey Customer"))
                html.AllowDelete = true;

            html.AddButton = "Add Customer Survey";
            html.AddURL = "SurveyCustomer/AddSurveyCustomer";
            html.TableName = "TblSurveyCustomer";
            html.PrimaryField = "SurveyCustomerID";
            html.OrderBy = "SurveyCustomerID desc";
            html.DialogBoxWidth = "Auto";
            html.ReportTitle = "Customer Survey List";
            //            string sql = @"select ROW_NUMBER() over(order by sc.SurveyCustomerID) as [No], sc.SurveyCustomerID, sc.SurveyDate, l.Salutation + '' + l.FullName SaleName,
            //sc.CustomerCode, sc.CustomerName, sc.PhoneNumber, sc.[Address],
            //sc.GoogleMap_FullAddress, sc.GoogleMap_Latitude, sc.GoogleMap_Longitude, 
            //pd.PackageName, sc.Duration, sg.GroupName, isnull(sc.[Status], 'Pending') [Status]
            //from dbo.TblSurveyCustomer sc
            //inner join dbo.TblPackageDetail pd on pd.PackageDetailID = sc.PackageDetailID
            //inner join dbo.TblLeads l on l.LeadID = sc.SaleID
            //inner join dbo.TblSupportGroup sg on sg.GroupID = sc.AssignTo
            //where sc.SystemID = " + html.SystemID + " and sc.WarehouseID = " + html.WarehouseID;
            //html.OrderBy = "SurveyCustomerID desc";
            html.GH.SQL = @"select ROW_NUMBER() OVER(ORDER BY sc.SurveyCustomerID) AS [No], sc.SurveyCustomerID,
sc.SurveyDate, l.FullName SaleName, sc.CustomerName, sc.PhoneNumber,
case when sc.GoogleMap_FullAddress <> '' then '<a target=""_blank"" 
href=""https://www.google.com/maps/place/'+convert(varchar, sc.GoogleMap_Latitude)+','+convert(varchar, sc.GoogleMap_Longitude)
+'/@'+convert(varchar, sc.GoogleMap_Latitude)+','+convert(varchar,sc.GoogleMap_Longitude)+',15z"">' + sc.GoogleMap_FullAddress + '</a>' else '' end GoogleMap_FullAddress,
sc.GoogleMap_Latitude, sc.GoogleMap_Longitude,isnull(sc.[Address],'') + case when sc.CustomerCommune <> '' then ', ' + sc.CustomerCommune else '' end + case when sc.CustomerDistrict <> '' then ', ' + sc.CustomerDistrict else '' end
+ case when sc.CustomerProvinceCity <> '' then ', ' + sc.CustomerProvinceCity else '' end [Address], sc.PackageName, DurationName, s.[Status]
from dbo.TblSurveyCustomer sc
inner join dbo.TblLeads l on l.LeadID = sc.SaleID
inner join dbo.TblStatus s on s.StatusID = sc.StatusID
where sc.SystemID =" + html.SystemID + " and sc.WarehouseID = " + html.WarehouseID;
            html.IsHaveWhereCondition = true;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "SurveyDate"
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSale",
                Label = "Sale's Name:",
                DBField = "sc.SaleID",
                SQL_CBO = "select distinct  sc.SaleID, l.Salutation + '' + l.FullName SaleName\r\nfrom dbo.TblSurveyCustomer sc\r\ninner join dbo.TblLeads l on l.LeadID = sc.SaleID where sc.SystemID = " + html.SystemID + " and sc.WarehouseID = " + html.WarehouseID,
                DisplayText = "SaleName",
                Value = "SaleID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Customer Name",
                ISAutoComplete = true,
                SQL = "select distinct top 10 CustomerName from dbo.TblSurveyCustomer where lower(CustomerName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by CustomerName",
                RadioValue = EncryAndDecry.Encryp("sc.CustomerName"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Date", FieldName = "SurveyDate", Format="dd/MM/yyyy", IsGroupByField = true, Colspan = 9 },
                new GridField() { Header = "Sale's Name", FieldName = "SaleName", IsGroupByField = true, Colspan = 9 },
                //new GridField() { Header = "CID", FieldName = "CustomerCode" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Phone Number", FieldName = "PhoneNumber" },
                new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "Package Name", FieldName = "PackageName" },
                new GridField() { Header = "Duration", FieldName = "DurationName" },
                //new GridField() { Header = "Assign To", FieldName = "AssignTo" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "Action", FieldName = "SurveyCustomerID", TextAlign = "Center",  IsModifyField = true }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Survey Customer, Edit Survey Customer")]
        public ActionResult AddSurveyCustomer(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "FrmSurveyCustomer",
                SaveChangeAndClose = true,
                SaveChangeCallbackFunction = "SaveChangeSurveyCustomerCallbackFunction(response)"
            };
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    TableName="TblSurveyCustomer",
                    FormTitle="Survey Customer Information",
                    GetIDENTITYByFiled="SurveyCustomerID",
                    PrimaryFieldName="SurveyCustomerID",
                    ReturnFieldNameValue="SurveyCustomerID, CustomerName",
                    ReturnFieldNameValueFromTblName = "TblSurveyCustomer",
                    dbField = new DBField[]
                    {
                        new DBField() { DBFieldName="StatusID", Value="Status_StatusID", GetValueFromInputName= true, }
                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Date", ID="Date", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SurveyDate",
                            SingleDatePicker=true,
                            //DivClassCol="col-sm"
                        },
                        new HtmlTextBox()
                        {
                            Label = "Sale's Name:",
                            ID = "SaleName",
                            Required = true,
                            Class="input2 CustInfo",
                            IsCustomForm = true,
                            SQL = "select LeadID SaleID, Salutation + ' ' + FullName SaleName from dbo.TblLeads where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            DBField="SaleName", HiddenField="SaleID", isTypeahead = true
                        },
                        new HtmlTextBox()
                        {
                            DivClassCol="col-sm-auto", Hide = true,
                            Label="Customer Code", ID="CustomerCode", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CustomerCode", IsReadOnly = true,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = Prefix, @Module = Module from dbo.TblPrefix where Module = 'Survey Cusotmer' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID+",'"+html.DomainID+"'",
                            CallBackToControlID="CustomerCode, PrefixCustomerCode", UpdatePrefixModule="Survey Cusotmer", Prefix="PrefixCustomerCode", SetPrefixToDbFieldName="CustomerCode"
                        },
                        new HtmlTextBox()
                        {
                            DivClassCol="col-sm-auto", Hide = true,
                            Label="Ticket Number", ID="TicketNumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="TicketNumber", IsReadOnly = true,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = Prefix, @Module = Module from dbo.TblPrefix where Module = 'Support Ticket' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + ",'" + html.DomainID + "'",
                            CallBackToControlID="TicketNumber, PrefixTicketNumber", UpdatePrefixModule="Support Ticket", Prefix="PrefixTicketNumber", SetPrefixToDbFieldName="TicketNumber"
                        },
                        new HtmlTextBox()
                        {
                            Label = "Customer's Full Name:",
                            ID = "TxtCustomerName",
                            DBField = "CustomerName",
                            Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            //DivClassCol="col-sm-auto",
                        },
                        new HtmlTextBox()
                        {
                            Label = "Phone Number:",
                            ID = "TxtContactPhoneNumber",
                            DBField = "PhoneNumber",
                            Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                        }
                    }
                },
                //new  FormData() {
                //    htmlControl = new HTMLControl[]
                //    {
                //        new HtmlTextBox()
                //        {
                //            DivClassCol="col-sm-auto", Hide = true,
                //            Label="Customer Code", ID="CustomerCode", Required=true,
                //            Class="input2 CustInfo", IsCustomForm = true,
                //            DBField="CustomerCode", IsReadOnly = true,
                //            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = Prefix, @Module = Module from dbo.TblPrefix where Module = 'Survey Cusotmer' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID,
                //            CallBackToControlID="CustomerCode, PrefixCustomerCode", UpdatePrefixModule="Survey Cusotmer", Prefix="PrefixCustomerCode", SetPrefixToDbFieldName="CustomerCode"
                //        },
                //        new HtmlTextBox()
                //        {
                //            DivClassCol="col-sm-auto", Hide = true,
                //            Label="Ticket Number", ID="TicketNumber", Required=true,
                //            Class="input2 CustInfo", IsCustomForm = true,
                //            DBField="TicketNumber", IsReadOnly = true,
                //            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = Prefix, @Module = Module from dbo.TblPrefix where Module = 'Support Ticket' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID,
                //            CallBackToControlID="TicketNumber, PrefixTicketNumber", UpdatePrefixModule="Support Ticket", Prefix="PrefixTicketNumber", SetPrefixToDbFieldName="TicketNumber"
                //        },
                //        new HtmlTextBox()
                //        {
                //            Label = "Customer's Full Name:",
                //            ID = "TxtCustomerName",
                //            DBField = "CustomerName",
                //            Required = true,
                //            Class="input2 CustInfo", IsCustomForm = true,
                //            //DivClassCol="col-sm-auto",
                //        },
                //        new HtmlTextBox()
                //        {
                //            Label = "Phone Number:",
                //            ID = "TxtContactPhoneNumber",
                //            DBField = "PhoneNumber",
                //            Required = true,
                //            Class="input2 CustInfo", IsCustomForm = true,
                //        }
                //    }
                //},
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Address:",
                            ID = "TxtAddress",
                            DBField = "Address",
                            Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Commune:",
                            ID = "TxtCustomerCommune",
                            DBField = "CustomerCommune",
                            Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            isTypeahead = true,
                            SQL="select c.Commune CustomerCommune, d.District, p.ProvinceCity from dbo.TblCommune c\r\ninner join dbo.TblDistrict d on d.DistrictID = c.DistrictID\r\ninner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID\r\nwhere c.SystemID = "+html.SystemID+" and c.WarehouseID = "+html.WarehouseID+" order by c.Commune",
                            DisplayOnFields = JsonConvert.SerializeObject(new { TxtCustomerDistrict="District",TxtCustomerProvinceCity="ProvinceCity"})
                        },
                        new HtmlTextBox()
                        {
                            Label = "District:",
                            ID = "TxtCustomerDistrict",
                            DBField = "CustomerDistrict",
                            Class="input2 CustInfo", IsCustomForm = true, Required = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Province/City:",
                            ID = "TxtCustomerProvinceCity",
                            DBField = "CustomerProvinceCity",
                            Class="input2 CustInfo", IsCustomForm = true, Required = true,
                        },
                    }
                },
                new FormData()
                {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Google Map:",
                            ID = "TxtGoogleMap_FullAddress",
                            DBField = "GoogleMap_FullAddress",
                            IsSetLocationWithGoogleMap = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            InputLatitude="GoogleMap_Latitude", InputLongitude="GoogleMap_Longitude"
                        },
                        new HtmlTextBox()
                        {
                            Label = "GoogleMap_Latitude:",
                            ID = "GoogleMap_Latitude",
                            DBField = "GoogleMap_Latitude",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label = "GoogleMap_Longitude:",
                            ID = "GoogleMap_Longitude",
                            DBField = "GoogleMap_Longitude",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Hide = true
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label = "Package Name:",
                            ID = "CboPackageName",
                            HiddenField = "PackageDetailID",
                            DBField="PackageName",
                            Required = true, isTypeahead = true,
                            SQL = "select distinct PackageDetailID, PackageName + ' ' + convert(varchar, Bandwidth) + ' Mbps' PackageName from dbo.TblPackageDetail where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            //DisplayText = "PackageName",
                            //Value = "PackageDetailID",
                            //Infinity = false,
                            //IsCallBack = true,
                            //SQLCallBack = "select convert(varchar, Duration) + ' ' + UM Duration from dbo.TblPackageDetail where PackageDetailID = @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            //DisplayOnFields = "Duration",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label = "Duration:",
                            ID = "Duration",
                            HiddenField="Duration",
                            DBField = "DurationName",
                            Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DivClassCol="col-sm-auto",
                            SQL="select distinct Duration, case when convert(varchar, Duration) = 1 then convert(varchar, Duration) + ' Month' else convert(varchar, Duration) + ' Months' end DurationName from dbo.TblPackageDetail",
                            isTypeahead = true,
                        },
                        //new HtmlTextBox()
                        //{
                        //    DivClassCol="col-sm-auto",
                        //    Label = "Assign To:",
                        //    ID = "CboAssign",
                        //    DBField = "AssignTo",
                        //    HiddenFeild="GroupID",
                        //    Required = true,
                        //    SQL = "select GroupID, GroupName AssignTo from dbo.TblSupportGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by GroupName",
                        //    Class="input2 CustInfo", IsCustomForm = true, isTypeahead=true,
                        //},
                        new HtmlTextBox()
                        {
                            Hide = true,
                            DivClassCol="col-sm-auto",
                            Label = "Status:",
                            ID = "Status",
                            DBField = "Status",
                            Class="input2 CustInfo", IsCustomForm = true,
                            Value="Pending", HiddenField="StatusID", HiddenFieldValue="101"
                        }
                    }
                }
            };
            html.FieldIDFocus = "SaleName";
            html.FocusFieldIDAfterReset = "TxtCustomerName";
            html.RestFields = "TxtCustomerName, TxtContactPhoneNumber, TxtAddress, TxtGoogleMap_FullAddress, GoogleMap_Latitude, GoogleMap_Longitude, TxtCustomerCommune,TxtCustomerDistrict,TxtCustomerProvinceCity";
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            //html.TblAlign = "center";
            //html.FormID = "AddSurveyCustomer";
            //html.FieldIDFocus = "TxtCustomerName";
            //html.TableName = "TblSurveyCustomer";
            //html.PrimaryField = "SurveyCustomerID";
            //html.PrimaryFieldValue = "0";
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Sale's Name:",
            //    ID = "CboSaleName",
            //    DBField = "SaleID",
            //    Required = true,
            //    SQL_CBO="select LeadID SaleID, Salutation + ' ' + FullName SaleName from dbo.TblLeads where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
            //    DisplayText= "SaleName",
            //    Value= "SaleID"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Customer's Code:",
            //    ID = "TxtCustomerCode",
            //    DBField = "CustomerCode",
            //    Required = true,
            //    SQLPreFix = "declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = Prefix, @Module = Module from dbo.TblPrefix where Module = 'Survey Cusotmer' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + "; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID,
            //    CallBackToControlID = "TxtCustomerCode, PrefixCustomerCode",
            //    UpdatePrefixModule = "Survey Cusotmer",
            //    Prefix = "SC-{000}",
            //    SetPrefixToDbFieldName = "CustomerCode"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Customer's Full Name:",
            //    ID = "TxtCustomerName",
            //    DBField = "CustomerName",
            //    Required = true
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Contact Phone Number:",
            //    ID = "TxtContactPhoneNumber",
            //    DBField = "PhoneNumber",
            //    Required = true
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Address:",
            //    ID = "TxtAddress",
            //    DBField = "Address",
            //    Required = true
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Google Map:",
            //    ID = "TxtGoogleMap_FullAddress",
            //    DBField = "GoogleMap_FullAddress",
            //    IsSetLocationWithGoogleMap = true,
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "GoogleMap_Latitude:",
            //    ID = "GoogleMap_Latitude",
            //    DBField = "GoogleMap_Latitude",
            //    Hide = true
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "GoogleMap_Longitude:",
            //    ID = "GoogleMap_Longitude",
            //    DBField = "GoogleMap_Longitude",
            //    Hide = true
            //});
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Package Name:",
            //    ID = "CboPackageName",
            //    DBField = "PackageDetailID",
            //    Required = true,
            //    SQL_CBO = "select distinct PackageDetailID, PackageName + ' ' + convert(varchar, Bandwidth) + ' Mbps' PackageName from dbo.TblPackageDetail where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
            //    DisplayText = "PackageName",
            //    Value = "PackageDetailID",
            //    Infinity = false,
            //    IsCallBack = true,
            //    SQLCallBack = "select convert(varchar, Duration) + ' ' + UM Duration from dbo.TblPackageDetail where PackageDetailID = @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
            //    DisplayOnFields = "Duration"
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Duration:",
            //    ID = "Duration",
            //    DBField = "Duration",
            //    Required = true
            //});
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Assign To:",
            //    ID = "CboAssign",
            //    DBField = "AssignTo",
            //    Required = true,
            //    SQL_CBO = "select GroupID, GroupName from dbo.TblSupportGroup where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by GroupName",
            //    DisplayText = "GroupName",
            //    Value = "GroupID"
            //});
            //ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        public async Task<JsonResult> SendNotification(int SurveyCustomerID)
        {
            HTML html = new HTML();
            string sql = @"select sc.SurveyDate, l.Salutation + '' + l.FullName SaleName,
sc.CustomerCode, sc.CustomerName, sc.PhoneNumber, sc.[Address],
sc.GoogleMap_FullAddress, sc.GoogleMap_Latitude, sc.GoogleMap_Longitude, 
pd.PackageName, sc.DurationName, sc.AssignTo, sc.CustomerCommune, sc.CustomerDistrict, sc.CustomerProvinceCity,
sg.TelegramGroupID
from dbo.TblSurveyCustomer sc
inner join dbo.TblPackageDetail pd on pd.PackageDetailID = sc.PackageDetailID
inner join dbo.TblLeads l on l.LeadID = sc.SaleID
inner join dbo.TblSupportGroup sg on sg.GroupID = sc.GroupID
where sc.SurveyCustomerID = @SurveyCustomerID and sc.SystemID = @SystemID and sc.WarehouseID = @WarehouseID";
            Dictionary<string, string> para = new Dictionary<string, string>()
            {
                {"SurveyCustomerID", SurveyCustomerID.ToString() },
                { "SystemID", html.SystemID},
                {"WarehouseID", html.WarehouseID }
            };
            DataSet ds = html.LoadData(sql, false, para);
            DataRow dr = ds.Tables[0].Rows[0];
            NotificationTelegram notificationTelegram = new NotificationTelegram();
            string msg = "Dear Team,\n\n";
            msg += "Now we have new customer survey from <b>" + html.UserFullName + "</b>.\n\n";
            msg += "Sale Representative: " + dr["SaleName"].ToString() + "\n\n";
            //msg += "CID: " + dr["CustomerCode"].ToString() + "\n";
            msg += "<b>Customer Name: " + dr["CustomerName"].ToString() + "</b>\n";
            msg += "<b>Phone Number: " + dr["PhoneNumber"].ToString() + "</b>\n";
            msg += "Address: " + dr["Address"].ToString() + "\n";
            msg += "Commune: " + dr["CustomerCommune"].ToString() + "\n";
            msg += "District: " + dr["CustomerDistrict"].ToString() + "\n";
            msg += "Province/City: " + dr["CustomerProvinceCity"].ToString() + "\n";
            if (dr["GoogleMap_FullAddress"].ToString() != "")
                msg += "Google Map: <a href=\"https://www.google.com/maps/place/" + dr["GoogleMap_Latitude"].ToString() + "," + dr["GoogleMap_Longitude"].ToString() + "/@" + dr["GoogleMap_Latitude"].ToString() + "," + dr["GoogleMap_Longitude"].ToString() + ",15z\">" + dr["GoogleMap_FullAddress"].ToString() + "</a>\n";
            msg += "\n<b>Package Name: " + dr["PackageName"].ToString() + "</b>\n";
            msg += "<b>Duration: " + dr["DurationName"].ToString() + "</b>\n";
            msg += "<b>Assign To: " + dr["AssignTo"].ToString() + "</b>\n";
            msg += "\n\nPlease click button below for verifying can install or cannot install.";
            msg += "\n\nNSM ISP BILLING SYSTEM AUTO SENT NOTIFICATION @" + DateTime.Parse(html.GetCurrentDate()).ToString("dd/MM/yyyy hh:mm:ss tt");
            SqlResult sqlResult = await notificationTelegram.NotificationTelegramBotSurveyCustomer(msg, int.Parse(dr["TelegramGroupID"].ToString()), SurveyCustomerID);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}