using MVC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class OpenTicketController : Controller
    {
        // GET: OpenTicket
        [Authorize(Roles = "Add Ticket, Edit Ticket, Delete Ticket")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Ticket"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Ticket"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Ticket"))
                html.AllowDelete = true;

            html.AddButton = "Open Ticket";
            html.AddURL = "OpenTicket/CreateTicket";
            html.TableName = "TblSupportOpenTicket";
            html.PrimaryField = "OpenTicketID";
            html.DialogBoxWidth = "Auto";

            string sql = @"select ROW_NUMBER() over(order by ot.OpenTicketID desc) [No],ot.OpenTicketID, sp.ProjectName, ot.TicketNumber, ot.CreateDate, ot.[Subject], ot.[Priority], ot.AssigneeID,
            STUFF((SELECT ', ' + convert(varchar, ul.FullName) FROM dbo.TblUserLogin ul
            where convert(varchar, ul.UserLoginID) in (select value from string_split(ot.AssigneeID,','))
            ORDER BY ul.UserLoginID FOR XML PATH('')), 1, 1, '') Assignee, 
            ot.CoWorkerID, ot.CoWorkerName,
            ot.[Status], ot.SupportDone,
            case when ot.AssigneeName <> '<< ME >>' then ot.AssigneeName else 
            (select ul.FullName from dbo.TblUserLogin ul where ul.UserLoginID = ot.AssigneeID)
            end AssigneeName
            from dbo.TblSupportOpenTicket ot
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            where ot.SystemID = " + html.SystemID + " and ot.WarehouseID = " + html.WarehouseID + " and ot.DomainID like '%" + html.DomainID + "%'";
            html.GH.SQL = sql;
            html.OrderBy = "ot.OpenTicketID desc";
            html.IsHaveWhereCondition = true;
            html.TblID = "TblTicketLists";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "ot.CreateDate",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtSupportProjectID",
                Label = "SupportProjectID:",
                DBField = "ot.SupportProjectID",
                SQL_CBO = "select distinct sp.SupportProjectID, sp.ProjectName\r\nfrom dbo.TblSupportProject sp\r\ninner join dbo.TblSupportOpenTicket ot on ot.SupportProjectID = sp.SupportProjectID where sp.SystemID = " + html.SystemID + " and sp.WarehouseID = " + html.WarehouseID + " and sp.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "ProjectName",
                Value = "SupportProjectID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Ticket Number",
                ISAutoComplete = true,
                SQL = "select top 10 TicketNumber from dbo.TblSupportOpenTicket where lower(TicketNumber) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'",
                RadioValue = EncryAndDecry.Encryp("ot.TicketNumber"),
                DefaultCheckBok = true,
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Project Name", FieldName = "ProjectName" },
                new GridField() { Header = "Ticket#", FieldName = "TicketNumber" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate" },
                new GridField() { Header = "Subject", FieldName = "Subject" },
                new GridField() { Header = "Priority", FieldName = "Priority" },
                new GridField() { Header = "Assignee", FieldName = "AssigneeName" },
                new GridField() { Header = "Co-Worker", FieldName = "CoWorkerName" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "Done", FieldName = "SupportDone" },
                new GridField() { Header = "Action", FieldName = "OpenTicketID", TextAlign = "Center",  IsModifyField = true, DeleteWithTableName="TblSupportOpenTicketComment" }
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "Add Ticket, Edit Ticket")]
        public ActionResult CreateTicket(string PrimaryFieldValue, string ConnectionID = "")
        {
            ViewBag.ConnectionID = ConnectionID;
            HTML html = new HTML
            {
                FormID = "CreateTicket",
                PrimaryFieldValue = PrimaryFieldValue,
                SaveChangeCallbackFunction = "MyTicket(response)"
            };
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Ticket Information",
                    TableName="TblSupportOpenTicket",
                    GetIDENTITYByFiled="OpenTicketID",
                    PrimaryFieldName="OpenTicketID",
                    ReturnFieldNameValue="OpenTicketID,SupportProjectID,TicketNumber,ProjectName,[Description]",
                    ReturnFieldNameValueFromTblName = "TblSupportOpenTicket",
                    dbField=new DBField[]
                    {
                        new DBField()
                        {
                            DBFieldName="SupportProjectID", Value="ProjectName_SupportProjectID", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="ProjectName", Value="ProjectName", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="PostAudience", Value="PostAudience", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="PostAudienceID", Value="PostAudience_PostAudienceID", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="PostAudienceUserLoginID", Value="PostAudienceUserLoginID", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Subject", Value="Subject", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="ProblemType", Value="ProblemType", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Status", Value="Status", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="StatusID", Value="Status_StatusID", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="PriorityID", Value="Priority_PriorityID", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="Priority", Value="Priority", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="SupportDoneID", Value="SupportDone_SupportDoneID", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="SupportDone", Value="SupportDone", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="AssigneeID", Value="Assignee_AssigneeID", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="AssigneeName", Value="Assignee", GetValueFromInputName= true
                        },
                        new DBField()
                        {
                            DBFieldName="CoWorkerID", Value="CoWorker", GetValueFromInputName=true, IsComboBox = true
                        },
                        new DBField()
                        {
                            DBFieldName="CoWorkerName", Value="CoWorker", GetValueFromInputName=true, IsComboBox = true, GetComboBoxText = true
                        },
                        new DBField()
                        {
                            DBFieldName="SID", Value="SID", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="ConnectionID", Value="ConnectionID", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="CustomerID", Value="CustomerID", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="CustomerName", Value="CustomerName", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="Phone", Value="Phone", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="ServiceID", Value="ServiceName_ServiceID", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="ServiceName", Value="ServiceName", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="PackageDetailID", Value="PackageDetailID", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="PackageName", Value="PackageName", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="StartDate", Value="StartDate", GetValueFromInputName=true, IsDate = true
                        },
                        new DBField()
                        {
                            DBFieldName="EndDate", Value="EndDate", GetValueFromInputName=true, IsDate = true
                        },
                        new DBField()
                        {
                            DBFieldName="LOTSwitch", Value="LOTSwitch", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="PONPort", Value="PONPort", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="ONTID", Value="ONTID", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="ONTDeviceModel", Value="ONTDeviceModel", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="SerialNumber", Value="SerialNumber", GetValueFromInputName=true
                        },
                        new DBField()
                        {
                            DBFieldName="Description", Value="Description", GetValueFromTextAreaName=true
                        },

                    },
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Ticket Number", ID="TicketNumber", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="TicketNumber", IsReadOnly = true,
                            SQLPreFix="declare @Prefix nvarchar(50), @Module nvarchar(50); select @Prefix = Prefix, @Module = Module from dbo.TblPrefix where Module = 'Support Ticket' and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%'; exec [SP_SHOW_PREFIX] @Prefix, @Module, " + html.SystemID + ", " + html.WarehouseID + ", '"+html.DomainID+"'",
                            CallBackToControlID="TicketNumber, PrefixTicketNumber", UpdatePrefixModule="Support Ticket", Prefix="PrefixTicketNumber", SetPrefixToDbFieldName="TicketNumber", //Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="CreateByFullName", ID="CreateByFullName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CreateByFullName", IsReadOnly = true,
                            Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="UserLoginID", ID="UserLoginID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="UserLoginID", IsReadOnly = true,
                            Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Project Name:", ID="ProjectName", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            isTypeahead = true,
                            SQL="select sp.SupportProjectID, sp.ProjectName from dbo.TblSupportProject sp inner join dbo.TblSupportUserProject up on up.SupportProjectID = sp.SupportProjectID and up.UserLoginID = "+ html.UserID +" where sp.SystemID = "+html.SystemID+" and sp.WarehouseID = "+html.WarehouseID+" and sp.DomainID like '%"+html.DomainID+"%' order by ProjectName",
                            HiddenField="SupportProjectID",
                            DBField="ProjectName",
                            SQLCallBack="select st.TrackerID, st.TrackerName from dbo.TblSupportTracker st inner join dbo.TblSupportTrackerProject tp on tp.TrackerID = st.TrackerID where tp.SupportProjectID = @SupportProjectID and st.SystemID = "+html.SystemID+" and st.WarehouseID = "+html.WarehouseID+" and st.DomainID like '%"+html.DomainID+"%' order by st.TrackerName",
                            CallBackToControlID = "TrackerName",
                            isApplyTypeaheadOnCallBack = true, //CallBackFunction="onSupportProjectChange",
                        },
                        new HtmlTextBox()
                        {
                            Label="Post Audience:", ID="PostAudience", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            isTypeahead = true,
                            SQL="select StatusID PostAudienceID, [Status] PostAudience from dbo.TblSupportStatus where StatusID in (20,21,22,23) order by StatusID",
                            HiddenField="PostAudienceID",
                            DBField="PostAudience",
                            DivClassCol="col-sm-2",
                            CallBackFunction="onPostAudienceChange",
                        },
                        new HtmlTextBox()
                        {
                            Label="PostAudienceUserLoginID:", ID="PostAudienceUserLoginID",
                            Class="input2 CustInfo", IsCustomForm = true, Hide = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Deadline:", ID="Deadline",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Deadline",
                            SingleDatePicker = true, TimePicker = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Subject:", ID="Subject", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "Subject",
                            SQL_AutoComplete="select distinct top 10 [Subject] from dbo.TblSupportOpenTicket where lower([Subject]) like @SearchText and SupportProjectID = @Search_Text2 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' order by [Subject]",
                            ISAutoComplete = true, CboValue="ProjectName_SupportProjectID",
                        },
                        new HtmlTextBox()
                        {
                            Label="Problem Type:", ID="ProblemType", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "ProblemType",
                            SQL_AutoComplete="select distinct top 10 ProblemType from dbo.TblSupportOpenTicket where lower(ProblemType) like @SearchText and SupportProjectID = @Search_Text2 and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and DomainID like '%"+html.DomainID+"%' order by ProblemType",
                            ISAutoComplete = true, CboValue="ProjectName_SupportProjectID",
                        },
                        new HtmlTextBox()
                        {
                            Label="Status:", ID="Status", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Status",
                            isTypeahead = true,
                            SQL="select StatusID, [Status] from dbo.TblSupportStatus  where StatusID not in (20,21,22,23) order by StatusID",
                            HiddenField="StatusID",
                        },
                        new HtmlTextBox()
                        {
                            Label="Priority:", ID="Priority", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Priority",
                            isTypeahead = true,
                            SQL="select PriorityID, [Priority] from dbo.TblSupportPriority order by [PriorityID]",
                            HiddenField="PriorityID",
                        },
                        new HtmlTextBox()
                        {
                            Label="% Done:", ID="SupportDone", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SupportDone",
                            isTypeahead = true,
                            SQL="select SupportDoneID, SupportDone from dbo.TblSupportDone",
                            HiddenField="SupportDoneID", HiddenFieldValue = "1",
                            DivClassCol ="col-sm-1", Hide = true, Value = "0 %"
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Assignee:", ID="Assignee", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="AssigneeName",
                            SQL = $@"select distinct convert(varchar, ul.UserLoginID) AssigneeID, ul.FullName AssigneeName, 'USER' [Type]
            from dbo.TblSupportUserProject up
            inner join dbo.TblUserLogin ul on ul.UserLoginID = up.UserLoginID
            inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserLoginID = ul.UserLoginID
            where sw.SystemID = {html.SystemID}",
                            isTypeahead = true,
                            HiddenField="AssigneeID",
                        },
                        new HtmlTextBox()
                        {
                            Label="Co-Worker:", ID="CoWorker", //Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="CoWorkerName",
                            SQL = $@"select distinct convert(varchar, ul.UserLoginID) CoWorkerID, ul.FullName CoWorkerName, 'USER' [Type]
            from dbo.TblSupportUserProject up
            inner join dbo.TblUserLogin ul on ul.UserLoginID = up.UserLoginID
            inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserLoginID = ul.UserLoginID
            where sw.SystemID = {html.SystemID}",
                            isTypeahead = true,
                            HiddenField="CoWorkerID",
                        },
                    }
                },
                new  FormData() {
                    FormTitle="Customer Information",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="SID:", ID="SID", Required = true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            SQL = "select distinct bi.[SID] from dbo.TblCustomer c inner join dbo.TblActivateCustomer bi on bi.CustomerID = c.CustomerID where isnull(bi.SID, '') <> '' and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + "order by bi.SID",
                            isTypeahead = true,
                            DBField = "SID",
                        },
                        new HtmlTextBox()
                        {
                            Label="Connection ID:", ID="ConnectionID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "ConnectionID",
                            SQL = "select distinct bi.ConnectionID from dbo.TblCustomer c inner join dbo.TblActivateCustomer bi on bi.CustomerID = c.CustomerID where isnull(bi.ConnectionID, '') <> '' and bi.SystemID = " + html.SystemID + " and bi.WarehouseID = " + html.WarehouseID + "order by bi.ConnectionID",
                            isTypeahead = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Customer Name:", ID="CustomerName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "CustomerName",
                            SQL_AutoComplete="select distinct top 10 c.CustomerID, (c.CustomerCode + ' - ' + c.CustomerName) CustomerName from dbo.TblCustomer c inner join dbo.TblCustomerBillInformation bi on bi.CustomerID = c.CustomerID where lower(c.CustomerCode + ' - ' + c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%"+html.DomainID+"%' order by (c.CustomerCode + ' - ' + c.CustomerName)",
                            HiddenField="CustomerID", ISAutoComplete = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Phone:", ID="Phone",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "Phone",
                            SQL_AutoComplete="select distinct top 10 bi.CompanyContactMobileNumber as Phone from dbo.TblCustomer c inner join dbo.TblCustomerBillInformation bi on bi.CustomerID = c.CustomerID where lower(bi.CompanyContactMobileNumber) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%"+html.DomainID+"%' order by bi.CompanyContactMobileNumber",
                            ISAutoComplete = true,
                            DivClassCol ="col-sm-auto",
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Service Name:", ID="ServiceName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            SQL = "select ServiceID, ServiceName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                            isTypeahead = true,
                            HiddenField = "ServiceID",
                            DBField = "ServiceName",
                        },
                        new HtmlTextBox()
                        {
                            Label="Package Name:", ID="PackageName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "PackageName",
                            SQL_AutoComplete = "select distinct top 10 pd.PackageDetailID, (p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' - ' + pd.UM) PackageName from dbo.TblPackageDetail pd inner join dbo.TblPackages p on p.PackageID = pd.PackageID where lower((p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' - ' + pd.UM)) like @SearchText and p.SystemID = " + html.SystemID + " and p.WarehouseID = " + html.WarehouseID + " and p.DomainID like '%"+html.DomainID+"%' order by (p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' - ' + pd.UM)",
                            HiddenField = "PackageDetailID",
                            ISAutoComplete = true
                        }
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Start Date", ID="StartDate",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="StartDate",
                            DefaultShowDateValue = false,
                            SingleDatePicker = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="End Date:", ID="EndDate",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="EndDate",
                            SingleDatePicker = true,
                            DefaultShowDateValue=false
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="LOT/Switch:", ID="LOTSwitch",
                            DBField = "LOTSwitch",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="PON/Port:", ID="PONPort",
                            DBField = "PONPort",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="ONT ID:", ID="ONTID",
                            DBField = "ONTID",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },                        
                        //new HtmlTextBox()
                        //{
                        //    Label="Address:", ID="Address",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    Value = data[29]
                        //},
                        //new HtmlTextBox()
                        //{
                        //    Label="Commune:", ID="Commune",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    FieldName="Commune",
                        //    SQL="select c.CommuneID, c.DistrictID, c.Commune, d.District, d.ProvinceCityID, p.ProvinceCity from dbo.TblCommune c inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by c.Commune",
                        //    isTypeahead = true, HiddenFeild = "CommuneID", DisplayOnFields="{\"District\":\"District\",\"ProvinceCity\":\"ProvinceCity\"}",
                        //    Value = data[30]
                        //},
                        //new HtmlTextBox()
                        //{
                        //    Label="District:", ID="District",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    Value = data[31]
                        //},
                        //new HtmlTextBox()
                        //{
                        //    Label="Province/City:", ID="ProvinceCity",
                        //    Class="input2 CustInfo", IsCustomForm = true,
                        //    Value = data[32]
                        //},
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="ONT Device Model:", ID="ONTDeviceModel",
                            DBField = "ONTDeviceModel",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Serial Number:", ID="SerialNumber",
                            DBField = "SerialNumber",
                            Class="input2 CustInfo", IsCustomForm = true,
                        },
                    }
                },new  FormData() {
                    FormTitle="Description",
                    htmlControl = new HTMLControl[]
                    {
                       new HtmlTextarea()
                       {
                           ID="Description", IsCustomForm = true,
                           DBField = "Description",
                           IsTextEditorSummernote= true,
                           DivClass="noborder", Required = true,
                       }
                    }
                }
            };
            html.SaveChangeAndClose = true;
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
        public JsonResult LoadCustDetails(int CustomerID)
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() over(order by ci.[SID]) as [No], c.CustomerID, case when ci.invTmp = 1 then ci.ServiceName + ' - ' + ci.PackageName else ci.ServiceName end ServiceName,
            ci.StartDate, ci.EndDate, DATEDIFF(D, getdate(), ci.EndDate) RemainDay, [SID], ConnectionID,
            ci.PPPoEUserName, ci.ActivateDate,
            (bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity) InstallAddress
            from dbo.TblCustomer c
            inner join dbo.TblCustomerBillingInvoice ci on ci.CustomerID = c.CustomerID
            inner join dbo.TblCustomerBillInformation bf on bf.CustomerID = c.CustomerID
            where c.CustomerID = " + CustomerID + " and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + @"%' 
			group by c.CustomerID, case when ci.invTmp = 1 then ci.ServiceName + ' - ' + ci.PackageName else ci.ServiceName end, ci.StartDate, ci.EndDate, DATEDIFF(D, getdate(), ci.EndDate), [SID], ConnectionID, ci.PPPoEUserName, ci.ActivateDate, (bf.InstallAddress + ', ' + bf.InstallCommune + ', ' + bf.InstallDistrict + ', ' + bf.InstallProvinceCity)";
            html.GH.SQL = sql;
            html.OrderBy = "ci.[SID]";
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.DefaultSearch = false;
            html.TblID = "tblCustDetail";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CustomerID", FieldName = "CustomerID", IsHiddenField= true },
                new GridField() { Header = "Action", FieldName = "CustomerID", DisplayCheckBox = true },
                new GridField() { Header = "Service Name", FieldName = "ServiceName" },
                new GridField() { Header = "Activate Date", FieldName = "ActivateDate", Format = "dd-MM-yyyy" },
                new GridField() { Header = "Start Date", FieldName = "StartDate", Format = "dd-MM-yyyy" },
                new GridField() { Header = "End Date", FieldName = "EndDate", Format = "dd-MM-yyyy"},
                new GridField() { Header = "Remain Day", FieldName = "RemainDay", TextAlign = "right" },
                new GridField() { Header = "PPPoE User Name", FieldName = "PPPoEUserName" },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "Connection ID", FieldName = "ConnectionID" },
                new GridField() { Header = "Install Address", FieldName = "InstallAddress" },
            };
            string DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "LoadCustDetail");
            return Json(new { Data = DrawHTMLSearchForm }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadAssignee(int SupportProjectID)
        {
            HTML html = new HTML();
            string sqlAssignee = @"select distinct convert(varchar, ul.UserLoginID) AssigneeID, '<< ME >>' AssigneeName, 'USER' [Type], 0 OrderBy
            from dbo.TblSupportUserProject up
            inner join dbo.TblUserLogin ul on ul.UserLoginID = up.UserLoginID
            inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserLoginID = ul.UserLoginID
            where ul.UserLoginID = " + html.UserID + " and sw.SystemID = " + html.SystemID + " and sw.WarehouseID = " + html.WarehouseID + " and sw.DomainID like '%" + html.DomainID + "%' and up.SupportProjectID=" + SupportProjectID + @"
            union
            select distinct convert(varchar, ul.UserLoginID) AssigneeID, ul.FullName AssigneeName, 'USER' [Type], 0 OrderBy
            from dbo.TblSupportUserProject up
            inner join dbo.TblUserLogin ul on ul.UserLoginID = up.UserLoginID
            inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserLoginID = ul.UserLoginID
            where ul.UserLoginID <> " + html.UserID + " and sw.SystemID = " + html.SystemID + " and sw.WarehouseID = " + html.WarehouseID + " and sw.DomainID like '%" + html.DomainID + "%' and up.SupportProjectID=" + SupportProjectID + @"
            union
            select distinct 
            STUFF((SELECT ',' + convert(varchar, ul.UserLoginID)  FROM dbo.TblUserLogin ul
            inner join dbo.TblSupportUserGroup ug on ug.UserLoginID = ul.UserLoginID and ug.GroupID = sg.GroupID
            ORDER BY ul.UserLoginID FOR XML PATH('')), 1, 1, '') AssigneeID, 
            sgn.GroupName AssigneeName, 'GROUP' [Type], 1 OrderBy
            from dbo.TblSupportGroupProject sg
            inner join dbo.TblSupportGroup sgn on sgn.GroupID = sg.GroupID
            where sgn.SystemID = " + html.SystemID + " and sgn.WarehouseID = " + html.WarehouseID + " and sg.SupportProjectID=" + SupportProjectID + " and sgn.DomainID like '%" + html.DomainID + "%'" + @"
            order by OrderBy, AssigneeName";
            HtmlTextBox text = new HtmlTextBox()
            {
                Label = "Assignee:",
                ID = "Assignee",
                Required = true,
                Class = "input2 CustInfo",
                IsCustomForm = true,
                DBField = "AssigneeName",
                isTypeahead = true,
                HiddenField = "AssigneeID",
                SQL = sqlAssignee,
                //GroupByField= "Type",
                //DisplayText= "AssigneeName",
                //Value= "UserLoginID",
                TypeaheadGroupByField = "Type",
                DivClassCol = "col-sm-3",
                //Value = "<< ME >>",
                HiddenFieldValue = html.UserID
            };
            string data = text.GetHtml();
            return Json(new { Data = data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadCoWorker(int SupportProjectID)
        {
            HTML html = new HTML();
            //string sql = @"select distinct ul.UserLoginID, ul.FullName
            //from dbo.TblUserLogin ul
            //inner join dbo.TblSupportUserProject up on up.UserLoginID = ul.UserLoginID
            //where up.SupportProjectID = " + SupportProjectID + " and ul.SystemID = " + html.SystemID + " and ul.WarehouseID = " + html.WarehouseID + "order by ul.FullName";

            string sqlAssignee = @"
            select distinct convert(varchar, ul.UserLoginID) CoWorkerID, ul.FullName, 'USER' [Type], 0 OrderBy
            from dbo.TblSupportUserProject up
            inner join dbo.TblUserLogin ul on ul.UserLoginID = up.UserLoginID
            inner join dbo.TblSetUserAccessSystemAndWarehouse sw on sw.UserLoginID = ul.UserLoginID
            where ul.UserLoginID <> " + html.UserID + " and sw.SystemID = " + html.SystemID + " and sw.WarehouseID = " + html.WarehouseID + " and sw.DomainID like '%" + html.DomainID + "%' and up.SupportProjectID=" + SupportProjectID + @"
            union
            select distinct 
            STUFF((SELECT ',' + convert(varchar, ul.UserLoginID)  FROM dbo.TblUserLogin ul
            inner join dbo.TblSupportUserGroup ug on ug.UserLoginID = ul.UserLoginID and ug.GroupID = sg.GroupID
            ORDER BY ul.UserLoginID FOR XML PATH('')), 1, 1, '') CoWorkerID, 
            sgn.GroupName + ' (Group)' FullName, 'GROUP' [Type], 1 OrderBy
            from dbo.TblSupportGroupProject sg
            inner join dbo.TblSupportGroup sgn on sgn.GroupID = sg.GroupID
            where isnull(STUFF((SELECT ',' + convert(varchar, ul.UserLoginID) FROM dbo.TblUserLogin ul
            inner join dbo.TblSupportUserGroup ug on ug.UserLoginID = ul.UserLoginID and ug.GroupID = sg.GroupID
            ORDER BY ul.UserLoginID FOR XML PATH('')), 1, 1, ''), '') <> ''
            and sgn.SystemID = " + html.SystemID + " and sgn.WarehouseID = " + html.WarehouseID + " and sg.SupportProjectID=" + SupportProjectID + " and sgn.DomainID like '%" + html.DomainID + "%'" + @"
            order by OrderBy, FullName";

            HtmlComboBox text = new HtmlComboBox()
            {
                Label = "Co-Worker:",
                ID = "CoWorker",
                Class = "input2 CustInfo",
                IsCustomForm = true,
                DBField = "CoWorkerID",
                SQL_CBO = sqlAssignee,
                DisplayText = "FullName",
                Value = "CoWorkerID",
                Multiple = true,
            };
            string data = text.GetHtml();
            return Json(new { Data = data }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadCustInfo(string SID = "", string ConnectionID = "", int CustomerID = 0, string Phone = "")
        {
            HTML html = new HTML();
            string sql = @"select top 1 bi.[SID], bi.ConnectionID, (c.CustomerCode + ' - ' + c.CustomerName) CustomerName,
            bf.CompanyContactMobileNumber, s.ServiceName, c.CustomerID,s.ServiceID, pd.PackageDetailID, 
            (p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' - ' + pd.UM) PackageName,
            bi.StartDate, bi.EndDate,
            bf.InstallAddress, bf.InstallCommune, bf.InstallDistrict, bf.InstallProvinceCity
            from dbo.TblCustomer c
            inner join dbo.TblCustomerBillInformation bf on bf.CustomerID = c.CustomerID
            inner join dbo.TblCustomerBillingInvoice bi on bi.CustomerID = c.CustomerID
            inner join dbo.TblServices s on bi.ServiceID = s.ServiceID
            inner join dbo.TblPackageDetail pd on pd.PackageDetailID = bi.PackageDetailID
            inner join dbo.TblPackages p on p.PackageID = bi.PackageID
            where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'";
            if (SID != "")
                sql += " and bi.[SID]='" + SID + "'";
            else if (ConnectionID != "")
                sql += " and bi.ConnectionID = '" + ConnectionID + "'";
            else if (CustomerID != 0)
                sql += " and c.CustomerID = " + CustomerID;
            else if (Phone != "")
                sql += " and bf.CompanyContactMobileNumber='" + Phone + "'";
            string data = html.JsonData(sql);
            return Json(new { Data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadUser(int SupportProjectID)
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() over(order by ul.FullName) as [No],
            ul.UserLoginID, ul.FullName
            from dbo.TblUserLogin ul
            inner join dbo.TblSupportUserProject up on up.UserLoginID = ul.UserLoginID
            where up.SupportProjectID = " + SupportProjectID + " and ul.SystemID =" + html.SystemID + " and ul.WarehouseID = " + html.WarehouseID + " and ul.DomainID like '%" + html.DomainID + "%'" + @"
            group by ul.UserLoginID, ul.FullName";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.ShowGridOnly = true;
            html.ShowPaging = false;
            html.ShowSearchButton = false;
            html.UsingJqueryDataTable = true;
            html.TblID = "AddUser";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "User Name", FieldName = "FullName" },
                new GridField() { Header = "Action", FieldName = "UserLoginID", DisplayCheckBox= true },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "LoadUser");
            return View();
        }

        public JsonResult SendEmail(int OpenTiketID)
        {
            HTML html = new HTML();
            string FilePath = HttpContext.Server.MapPath("~/Templates/EmailSupportTemplate.html");
            StreamReader str = new StreamReader(FilePath);
            string Text = str.ReadToEnd();
            str.Close();
            //Text = Text.Replace("/", html.ServerRoot);
            Text = Text.Replace("{Title}", "this test");
            Text = Text.Replace("{BodyMessage}", "this test");
            EmailProperty emailProperty = new EmailProperty();
            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            SqlResult sqlResult = new SqlResult();
            smtp.Port = emailProperty.Port;
            smtp.Host = emailProperty.Host;
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(emailProperty.CredentialsEmail, emailProperty.CredentialsPassword);
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            message.From = new MailAddress(emailProperty.EmailFrom, "NSM SOLUTION CO.,LTD");
            try
            {
                //string sql = @"select * from dbo.TblSupportOpenTicket where SystemID = @SystemID and WarehouseID = @WarehouseID and OpenTicketID = @OpenTiketID";
                Dictionary<string, string> para = new Dictionary<string, string>
                {
                    { "SystemID", html.SystemID },
                    { "WarehouseID", html.WarehouseID },
                    { "OpenTiketID", OpenTiketID.ToString() }
                };
                //DataSet ds = html.LoadData(sql, false, para);

                string EmailTo = "sothea8989@gmail.com";
                message.To.Add(new MailAddress(EmailTo));
                message.Subject = "hello test";
                message.SubjectEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                                                              Text,
                                                              Encoding.UTF8,
                                                              MediaTypeNames.Text.Html);
                message.AlternateViews.Add(htmlView);
                //smtp.Send(message);
                smtp.Dispose();
                sqlResult.Result = ResultType.Successful;
                sqlResult.Message = "Send Mail Successful.";
            }
            catch (Exception ex)
            {
                sqlResult.Result = ResultType.Failed;
                sqlResult.Message = ex.Message;
            }
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}