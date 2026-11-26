using MVC;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    [Authorize]
    public class ReportCustomerInfrastructureController : Controller
    {
        [Authorize(Roles = "List Report Customer Infrastructure")]
        public ActionResult Index()
        {
            return View();
        }
        public string LoadListReport()
        {
            HTML html = new HTML();
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Register Date:",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtInfraProvider",
                Label = "Infra Provider:",
                DBField = "InfraProviderName",
                SQL_CBO = @"select * from dbo.TblInfraProvider",
                DisplayText = "InfraProviderName",
                Value = "InfraProviderName",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Status:",
                DBField = "StatusID",
                SQL_CBO = "select StatusID, Status from dbo.TblStatus where StatusID in (40, 41, 42)",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = @"select distinct top 10 c.CustomerCode
from dbo.TblCustomer c where lower(c.CustomerCode) like @SearchText",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("SID") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerNameLatin"),
                ISAutoComplete = true,
                DefaultCheckBok = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "SID",
                SQL = @"select distinct top 10 bi.[SID] from dbo.TblCustomerBillingInvoice bi where lower([SID]) like @SearchText",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("SID") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerNameLatin"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Name KH",
                SQL = @"select distinct top 10 c.CustomerName
from dbo.TblCustomer c where lower(c.CustomerName) like @SearchText",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("SID") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerNameLatin"),
                ISAutoComplete = true,
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Name Latin",
                SQL = @"select distinct top 10 c.CustomerNameLatin
from dbo.TblCustomer c where lower(c.CustomerNameLatin) like @SearchText",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("SID") + "," + EncryAndDecry.Encryp("CustomerName") + "," + EncryAndDecry.Encryp("CustomerNameLatin"),
                ISAutoComplete = true,
            });

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right", },
                new GridField() { Header = "Register Date", FieldName = "RegisterDate", FormatDateTime = "dd-MM-yyyy", ClassName = "text-center"},
                new GridField() { Header = "Infra Provider", FieldName = "InfraProviderName", IsGroupByField = true},
                new GridField() { Header = "Infra Name", FieldName = "CustomerInfraName"},
                new GridField() { Header = "SurveyID", FieldName = "CustomerInfraSurveyID",},
                new GridField() { Header = "AID", FieldName = "CustomerInfraAID",},
                new GridField() { Header = "SID", FieldName = "CustomerInfraSID",},
                new GridField() { Header = "NSM Name", FieldName = "CustomerName", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true },
                new GridField() { Header = "CID", FieldName = "CustomerCode", Link="#!/CustomerProfile?ID=", LinkParameterField="CustomerID", NewPage = true},
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "Phone Number", FieldName = "CustomerMobile" },
                new GridField() { Header = "Install Address", FieldName = "InstallAddress" },
                new GridField() { Header = "Lat/Long", FieldName = "LatLong" },
                new GridField() { Header = "Package", FieldName = "PackageName" },
                new GridField() { Header = "Start Date", FieldName = "StartDate", FormatDateTime = "dd-MM-yyyy" },
                new GridField() { Header = "End Date", FieldName = "EndDate", FormatDateTime = "dd-MM-yyyy" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "Remark", FieldName = "Remark" },
            };
            html.ShowPaging = false;
            html.TblID = "tblCustomerInfra";
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                HeaderReportNameExcel = "Report Customer Infra",
                ServerPaging = true,
                ProcedureName = "SP_REPORT_CUSTOMER_INFRA"
            };

            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
        }
        public string LoadNewInfra()
        {
            HTML html = new HTML
            {
                AllowAdd = true,
                AllowEdit = true,
                DialogBoxWidth = "800",
                AddButton = "Add New Infra",
                AddURL = "ReportCustomerInfrastructure/CreateNewInfra"
            };

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "CID",
                SQL = "select top 10 CustomerCode from dbo.TblCustomer where lower(CustomerCode) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by CustomerCode",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("SID"),
                ISAutoComplete = true,
                DefaultCheckBok = true
            });
            htmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                OPTLabel = "SID",
                SQL = "select distinct top 10 SID from dbo.TblCustomerInvoice where lower(SID) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%' order by SID",
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("SID"),
                ISAutoComplete = true,
            });

            string sql = @"select distinct row_number() over(order by ci.CustomerInfraID desc) as [No], ci.CustomerInfraID, ci.CID, ci.SID, coalesce(nullif(isnull(c.CustomerName, ''), ''), c.CustomerNameLatin) as CustomerName, c.CustomerID,
ci.CustomerInfraName, ip.InfraProviderName, ci.CustomerInfraSurveyID, ci.CustomerInfraSID, ci.CustomerInfraAID, ci.CustomerInfraCID
from dbo.TblCustomerInfra ci 
inner join dbo.TblInfraProvider ip on ip.InfraProviderID = ci.InfraProviderID
inner join dbo.TblCustomer c on c.CustomerID = ci.CustomerID where ci.ConnectionID is not null";
            html.GH.SQL = sql;
            html.IsHaveWhereCondition = true;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "CID", FieldName = "CID", Link = "#!/CustomerProfile?ID=", LinkParameterField = "CustomerID", NewPage = true },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", Link = "#!/CustomerProfile?ID=", LinkParameterField = "CustomerID", NewPage = true },
                new GridField() { Header = "SID", FieldName = "SID" },
                new GridField() { Header = "Infra Provider", FieldName = "InfraProviderName" },
                new GridField() { Header = "Infa Name", FieldName = "CustomerInfraName" },
                new GridField() { Header = "Infa SurveyID", FieldName = "CustomerInfraSurveyID" },
                new GridField() { Header = "Infa SID", FieldName = "CustomerInfraSID" },
                new GridField() { Header = "Infa AID", FieldName = "CustomerInfraAID" },
                new GridField() { Header = "Infa CID", FieldName = "CustomerInfraCID" },
                new GridField() { Header = "Action", FieldName = "CustomerInfraID", TextAlign = "Center",  IsModifyField = true }
            };
            html.OrderBy = "[No]";
            html.ShowPaging = false;
            return html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
        }
        public ActionResult CreateNewInfra(string PrimaryFieldValue)
        {
            HTML html = new HTML();
            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            html.TableColumn = 2;
            html.UseTable = true;
            html.PrimaryFieldValue = PrimaryFieldValue;
            html.TableName = "TblCustomerInfra";
            html.PrimaryField = "CustomerInfraID";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Customer:",
                ID = "CustomerID",
                DBField = "CustomerID",
                Required = true,
                SQL_CBO = @"select CustomerID, CustomerCode, coalesce(nullif(isnull(CustomerName, ''), ''), CustomerNameLatin) as CustomerName from dbo.TblCustomer where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by CustomerID desc",
                DisplayText = "CustomerName",
                Value = "CustomerID",
                CboValue = "0",
                IsCallBack = true,
                SQLCallBack = "select CustomerCode, SID, ConnectionID, CustomerID from (\r\n  select c.CustomerID, c.CustomerCode, ac.SID, ac.ConnectionID \r\n  from TblCustomer c \r\n  inner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID\r\n  union all \r\n  select cb.CustomerID, cb.UCID as CustomerCode, cb.USID as SID, cb.ConnectionID\r\n  from dbo.TblCustomerBankSlip cb \r\n) as t where t.CustomerID = @SearchText",
                DisplayOnFields = "CustomerCode, SID, ConnectionID"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Customer Code:",
                ID = "CustomerCode",
                DBField = "CID",
                Required = true,
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "SID:",
                ID = "SID",
                DBField = "SID",
                Required = true,
                isTypeahead = true,
                CboValue = "CustomerID",
                DataValueField = "CustomerID",
                SQL = "select c.CustomerCode, ac.SID, ac.ConnectionID \r\nfrom TblCustomer c \r\ninner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID\r\nunion all \r\nselect cb.UCID as CustomerCode, cb.USID as SID, cb.ConnectionID\r\nfrom dbo.TblCustomerBankSlip cb ",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "ConnectionID:",
                ID = "ConnectionID",
                DBField = "ConnectionID",
                Required = true,
                isTypeahead = true,
                CboValue = "CustomerID",
                DataValueField = "CustomerID",
                SQL = "select c.CustomerCode, ac.SID, ac.ConnectionID \r\nfrom TblCustomer c \r\ninner join dbo.TblActivateCustomer ac on ac.CustomerID = c.CustomerID\r\nunion all \r\nselect cb.UCID as CustomerCode, cb.USID as SID, cb.ConnectionID\r\nfrom dbo.TblCustomerBankSlip cb ",
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Infra Provider:",
                ID = "InfraProviderID",
                DBField = "InfraProviderID",
                Required = true,
                SQL_CBO = @"select InfraProviderID, InfraProviderName from dbo.TblInfraProvider where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by InfraProviderName",
                DisplayText = "InfraProviderName",
                Value = "InfraProviderID",
                CboValue = "0"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Infra Name:",
                ID = "CustomerInfraName",
                DBField = "CustomerInfraName",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Infra SurveyID:",
                ID = "CustomerInfraSurveyID",
                DBField = "CustomerInfraSurveyID",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Infra SID:",
                ID = "CustomerInfraSID",
                DBField = "CustomerInfraSID",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Infra AID:",
                ID = "CustomerInfraAID",
                DBField = "CustomerInfraAID",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Infra CID:",
                ID = "CustomerInfraCID",
                DBField = "CustomerInfraCID",
            });

            html.SaveChangeFunction = "saveChange()";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> SaveChange(CustomerInfra data)
        {
            try
            {
                var html = new HTML();
                using (var ctx = new DBContext())
                {
                    var infra = await ctx.TblCustomerInfra
                        .Where(x => x.CustomerInfraID == data.CustomerInfraID)
                        .FirstOrDefaultAsync();

                    if (infra == null)
                    {
                        infra = new CustomerInfra
                        {
                            UserID = int.Parse(html.UserID),
                            CreateBy = html.UserFullName,
                            CreateDate = DateTime.Now,
                            SystemID = int.Parse(html.SystemID),
                            WarehouseID = int.Parse(html.WarehouseID),
                            DomainID = html.DomainID
                        };

                        ctx.TblCustomerInfra.Add(infra);
                    }
                    infra.CustomerID = data.CustomerID;
                    infra.CID = data.CID;
                    infra.SID = data.SID;
                    infra.ConnectionID = data.ConnectionID;
                    infra.InfraProviderID = data.InfraProviderID;
                    infra.CustomerInfraName = data.CustomerInfraName;
                    infra.CustomerInfraSurveyID = data.CustomerInfraSurveyID;
                    infra.CustomerInfraSID = data.CustomerInfraSID;
                    infra.CustomerInfraAID = data.CustomerInfraAID;
                    infra.CustomerInfraCID = data.CustomerInfraCID;

                    await ctx.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            return Json(new { success = true, message = "Changes saved successfully." });
        }
    }
}