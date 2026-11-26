using MVC;
using Newtonsoft.Json.Linq;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace NSMSOLUTION.Controllers
{

    public class CreateQuotationController : Controller
    {
        // GET: CreateQuotation
        [Authorize(Roles = "Add New Quotation, Edit Quotation, Delete Quotation")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            bool AdminEdit = HttpContext.GetOwinContext().Request.User.IsInRole("Admin Edit");
            html.ReportTitle = "List Quotation";
            if (html.HasPermission("Add New Quotation"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Quotation"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Quotation"))
                html.AllowDelete = true;

            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add New Quotation";
            html.AddURL = "AddCustomer/CreateCustomerV2/?FormType=Quotation";
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                DateRangePicker = true,
                LabelFromDate = "Quotation Date:",
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatusID",
                Label = "Status:",
                DBField = "StatusID",
                SQL_CBO = "select distinct s.StatusID, s.[Status] from dbo.TblStatus s where s.StatusID in (60, 90) order by s.Status",
                DisplayText = "Status",
                Value = "StatusID",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtQuotationNo",
                Label = "Quotation No:",
                DBField = "QuotationNo",
                SQLAutoComplete = "select distinct top 10 InvoiceNo as QuotationNo from dbo.TblCustomerInvoice where Type='Quotation Invoice' and lower(InvoiceNo) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by InvoiceNo",
                ISAutoComplete = true,
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtClassName",
                Label = "Class Name:",
                DBField = "ClassName",
                SQL_CBO = @"select distinct c.ClassID, c.ClassName from dbo.TblClass c where c.ClassName <> 'NSM no VAT' and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " and c.DomainID like '%" + html.DomainID + "%'",
                DisplayText = "ClassName",
                Value = "ClassName",
                ComboBox = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtBlock",
                Label = "Block:",
                DBField = "Block",
                SQL_CBO = @"select distinct CustomerType as Block from dbo.TblCustomerType where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                DisplayText = "Block",
                Value = "Block",
                ComboBox = true
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
                SQL = "select top 10 c.CustomerName from dbo.TblCustomer c where lower(c.CustomerName) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID + " union all select top 10 c.CustomerNameLatin as CustomerName from dbo.TblCustomer c where lower(c.CustomerNameLatin) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID=" + html.WarehouseID,
                RadioValue = EncryAndDecry.Encryp("CustomerCode") + "," + EncryAndDecry.Encryp("CustomerName"),
                ISAutoComplete = true,
            });

            html.ShowPaging = false;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No.", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Type", FieldName = "TypeOfInvoice", },
                new GridField() { Header = "Quotation Date", FieldName = "QuotationDate", TextAlign="center", FormatDateTime = "dd-MM-yyyy" },
                new GridField() { Header = "Quotation No.", FieldName = "InvoiceNo", OnClickFunction="PrintInvoice(#=CustomerID#,'#=InvoiceNo#','0','10','',false,this);"},
                new GridField() { Header = "CID", FieldName = "CustomerCode", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID="},
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", LinkParameterField="CustomerID", NewPage = true, Link="#!/CustomerProfile?ID="},
                new GridField() { Header = "ATT Name", FieldName = "ATTName", },
                new GridField() { Header = "Block", FieldName = "CustomerType", },
                new GridField() { Header = "Class Name", FieldName = "ClassName", },
                new GridField() { Header = "Bank Slip", FieldName = "AttachBankSlip" },
                new GridField() { Header = "Type", FieldName = "IsService", ClassName = "text-center" },
                new GridField() { Header = "Grand Total", FieldName = "GrandTotal", ClassName="text-right text-primary", Format="#,#0.00" },
                new GridField() { Header = "Create Date", FieldName = "CreateDate", TextAlign="center", FormatDateTime = "dd-MM-yyyy" },
                new GridField() { Header = "Create By", FieldName = "CreateBy" },
                new GridField() { Header = "Action", FieldName = "CustomerInvoiceID", TextAlign = "Center",
                    IsModifyField = true, EditFunction ="EditCustomerWithRefNumber(#=CustomerID#,'#=InvoiceNo#', 'Quotation')",
                    DeleteWithTableName="TblCustomerInvoice, TblCustomerBillInformation, TblCustomerDetail, TblCustomerBalance, TblCustomerBillingInvoice",
                    DeletedCallBackFunction="LoadCustQuotation(#=CustomerID#)",
                    DeletePrimaryTblName="TblCustomer",
                    DeletePrimaryFiledName="CustomerID",
                    DeletePrimaryTblNameDependOnTblName="TblCustomerBalance",
                    HideEditOnActionNameNotMatch = true,
                    ShowActionNameOnConditionField ="IsDoneLayCable",
                    ShowActionNameOnConditionValue="0",
                    HideDeteleOnNotMatch=true
                }
            };

            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = true,
                ServerPaging = true,
                ProcedureName = "SP_QUOTATION_LIST"
            };

            html.OrderBy = "[No]";
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View();
        }
        public JsonResult LoadUserApproval()
        {
            HTML html = new HTML();
            string sql = @"select * from (select ul.UserLoginID, ul.FullName, ul.TelegramUserName, 3 OrderBy
from dbo.TblUserLogin ul
inner join dbo.TblSetUserAccessSystemAndWarehouse ua on ua.UserLoginID = ul.UserLoginID
where ua.SystemID = " + html.SystemID + @" and ul.TelegramUserName <> '' and ul.StatusID = 3) as aa
order by aa.OrderBy, case when aa.FullName = 'Ms. Meng Daneth' then '2' else aa.FullName end asc";
            return Json(new { data = html.JsonData(sql) }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public async Task<JsonResult> QuotationAttBankSlip(string data)
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    NotificationTelegram notification = new NotificationTelegram();
                    var notifyResult = new SqlResult();
                    HTML html = new HTML();

                    JObject json = JObject.Parse(data);

                    int customerId = json["CustomerID"]?.Value<int>() ?? 0;
                    string invoiceNo = json["InvoiceNo"]?.ToString();
                    int approvalBy = json["NeedApproveByID"]?.Value<int>() ?? 0;
                    string memo = json["Memo"]?.ToString();
                    decimal amount = json["Amount"]?.Value<decimal>() ?? 0;

                    string payBy = json["PayBy"]?.ToString() ?? "bank";
                    bool isCash = payBy.Equals("cash", StringComparison.OrdinalIgnoreCase);

                    // Validate bank slip only for Bank Transfer
                    var uploadedFile = Request.Files["BankSlip"];

                    if (!isCash && (uploadedFile == null || uploadedFile.ContentLength == 0))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Bank slip attachment is required."
                        });
                    }

                    if (customerId <= 0 || string.IsNullOrWhiteSpace(invoiceNo))
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Invalid customer or invoice."
                        });
                    }

                    if (approvalBy <= 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Please select Approval By."
                        });
                    }

                    using (var transaction = ctx.Database.BeginTransaction())
                    {
                        try
                        {
                            var customer = await ctx.TblCustomer
                                .Where(x => x.CustomerID == customerId)
                                .Select(x => new
                                {
                                    x.CustomerCode,
                                    x.CustomerMobile,
                                    x.RegisterDate,
                                    x.RegisterBy,
                                    x.CustomerGender,
                                    CustomerName = string.IsNullOrEmpty(x.CustomerName)
                                        ? x.CustomerNameLatin
                                        : x.CustomerName
                                })
                                .FirstOrDefaultAsync();

                            if (customer == null)
                            {
                                return Json(new
                                {
                                    success = false,
                                    message = "Customer not found"
                                });
                            }

                            var approver = await ctx.TblUserLogin
                                .Where(u => u.UserLoginID == approvalBy)
                                .Select(u => new
                                {
                                    u.TelegramUserName,
                                    u.FullName
                                })
                                .FirstOrDefaultAsync();

                            if (approver == null || string.IsNullOrWhiteSpace(approver.TelegramUserName))
                            {
                                return Json(new
                                {
                                    success = false,
                                    message = "Approval user or Telegram username not found."
                                });
                            }

                            var customerInvoice = await ctx.TblCustomerInvoice
                                .Where(x => x.CustomerID == customerId && x.InvoiceNo == invoiceNo)
                                .Select(x => new
                                {
                                    PackageName = x.ItemDescription,
                                    ServiceSort = (x.ServiceID != null && x.ServiceID != 0) ? 1 : 0,
                                    x.ServiceID,
                                    x.ATTName,
                                    x.StartDate,
                                    x.EndDate,
                                    x.InstallDate,
                                    x.ClassName
                                })
                                .OrderByDescending(x => x.ServiceSort)
                                .FirstOrDefaultAsync();

                            var infra = await ctx.TblCustomerInfra
                                .Where(x => x.CustomerID == customerId && x.InvoiceNo == invoiceNo)
                                .Join(ctx.TblInfraProvider,
                                    c => c.InfraProviderID,
                                    i => i.InfraProviderID,
                                    (c, i) => new
                                    {
                                        c.CustomerInfraName,
                                        c.CustomerInfraSurveyID,
                                        c.CustomerInfraCID,
                                        c.CustomerInfraSID,
                                        c.CustomerInfraAID,
                                        i.InfraProviderName
                                    })
                                .FirstOrDefaultAsync();

                            var billInfo = await ctx.TblCustomerBillInformation
                                .Where(x => x.CustomerID == customerId && x.InvoiceNo == invoiceNo)
                                .Select(x => new
                                {
                                    x.GoogleMap_FullAddress,
                                    x.GoogleMap_Latitude,
                                    x.GoogleMap_Longitude
                                })
                                .FirstOrDefaultAsync();

                            // =====================================
                            // FILE UPLOAD - BANK TRANSFER ONLY
                            // =====================================

                            string destinationFolder = null;
                            string filePath = null;
                            string fileName = null;

                            if (!isCash)
                            {
                                string strCustomer = html.ToUrlSlug(
                                    $"{customer.CustomerName} [{customer.CustomerCode}]"
                                );

                                destinationFolder =
                                    $"/Uploads/CustomerAttFiles/{strCustomer}/BankSlip/";

                                var uploadResult = html.UploadFile(destinationFolder);

                                fileName = uploadResult?.Value;

                                if (string.IsNullOrWhiteSpace(fileName))
                                {
                                    return Json(new
                                    {
                                        success = false,
                                        message = "Failed to upload bank slip."
                                    });
                                }

                                filePath = destinationFolder + fileName;
                            }

                            // =====================================
                            // SAVE BANK SLIP / CASH PAYMENT RECORD
                            // =====================================

                            var bankSlip = await ctx.TblCustomerBankSlip
                                .FirstOrDefaultAsync(x =>
                                    x.CustomerID == customerId &&
                                    x.InvoiceNo == invoiceNo
                                );

                            if (bankSlip == null)
                            {
                                bankSlip = new BankSlip
                                {
                                    CustomerID = customerId,
                                    InvoiceNo = invoiceNo,
                                    CreateDate = DateTime.Now
                                };

                                ctx.TblCustomerBankSlip.Add(bankSlip);
                            }

                            bankSlip.Destination = destinationFolder;
                            bankSlip.FileName = fileName;
                            bankSlip.Amount = amount;
                            bankSlip.StatusID = 60;
                            bankSlip.NeedApproveByID = approvalBy;
                            bankSlip.Memo = memo;
                            bankSlip.UserID = Utility.TryParse(html.UserID, "int", 0);

                            await ctx.SaveChangesAsync();

                            transaction.Commit();

                            // =====================================
                            // TELEGRAM MESSAGE
                            // =====================================

                            var mapLink =
                                $"https://www.google.com/maps?q={billInfo?.GoogleMap_Latitude},{billInfo?.GoogleMap_Longitude}";

                            string type = customerInvoice?.ClassName == "NSM"
                                ? "NSM"
                                : "ISP";

                            bool isKhmer = customer.CustomerName?.Any(c =>
                                c >= '\u1780' && c <= '\u17FF'
                            ) ?? false;

                            string genderPrefix = "";

                            if (customer.CustomerGender == "Male")
                                genderPrefix = isKhmer ? "លោក" : "Mr.";
                            else if (customer.CustomerGender == "Female")
                                genderPrefix = isKhmer ? "អ្នកស្រី" : "Ms.";

                            var message = new StringBuilder();

                            message.AppendLine(
                                $"<b>📋 {type} QUOTATION ({(isCash ? "Cash Payment" : "Bank Slip")})</b>\n"
                            );

                            message.AppendLine($"<i><b>No.</b>: {invoiceNo}</i>");
                            message.AppendLine("<b>────────────────────</b>");
                            message.AppendLine("<b><i>👤 CUSTOMER INFO</i></b>");
                            message.AppendLine($"‣ <b>Name:</b> {genderPrefix} {customer.CustomerName}");
                            message.AppendLine($"‣ <b>Phone:</b> {customer.CustomerMobile}");
                            message.AppendLine($"‣ <b>Register Date:</b> {customer.RegisterDate}");
                            message.AppendLine($"‣ <b>Install Date:</b> {customerInvoice?.InstallDate}\n");

                            message.AppendLine("<b><i>⚙️ INFRASTRUCTURE</i></b>");
                            message.AppendLine($"‣ <b>Infra:</b> {infra?.InfraProviderName}");
                            message.AppendLine($"‣ <b>Name:</b> {infra?.CustomerInfraName}");
                            message.AppendLine($"‣ <b>Survey ID:</b> {infra?.CustomerInfraSurveyID}");
                            message.AppendLine($"‣ <b>CID:</b> {infra?.CustomerInfraCID}, <b>SID:</b> {infra?.CustomerInfraSID}, <b>AID:</b> {infra?.CustomerInfraAID}\n");

                            message.AppendLine("<b><i>🛜 PACKAGE INFO</i></b>");
                            message.AppendLine($"‣ <i><b>Plan:</b> {customerInvoice?.PackageName}</i>");
                            message.AppendLine($"‣ <i><b>Start Date:</b> {customerInvoice?.StartDate?.ToString("dd-MM-yyyy")}</i>");
                            message.AppendLine($"‣ <i><b>End Date:</b> {customerInvoice?.EndDate?.ToString("dd-MM-yyyy")}</i>\n");

                            message.AppendLine("<b><i>📍 LOCATION</i></b>");
                            message.AppendLine($"‣ <b>Map:</b> <a href='{mapLink}'>{billInfo?.GoogleMap_FullAddress}</a>");
                            message.AppendLine($"‣ <b>Lat / Long:</b> {billInfo?.GoogleMap_Latitude}, {billInfo?.GoogleMap_Longitude}\n");

                            message.AppendLine($"ℹ️ <i><b>Note:</b></i> {memo}\n");
                            message.AppendLine($"<b>Need approve by:</b> {approver.TelegramUserName}");

                            string captions = message.ToString();

                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(
                                        "Approve ✅",
                                        $"ApproveQuotation~{invoiceNo}~{approver.TelegramUserName}"
                                    ),
                                    InlineKeyboardButton.WithCallbackData(
                                        "Disapprove ❌",
                                        $"DisapproveQuotation~{invoiceNo}~{approver.TelegramUserName}"
                                    )
                                }
                            });

                            // Bank Transfer sends attachment.
                            // Cash sends text message only.
                            if (isCash)
                            {
                                notifyResult = await notification.NotificationTelegramBot(
                                    "",
                                    -1003351799592,
                                    Caption: captions,
                                    replyMarkupInlineKeyboardMarkup: true,
                                    inlineKeyboard: inlineKeyboard
                                );
                            }
                            else
                            {
                                notifyResult = await notification.NotificationTelegramBot(
                                    "",
                                    -1003351799592,
                                    PDFFile: filePath,
                                    Caption: captions,
                                    replyMarkupInlineKeyboardMarkup: true,
                                    inlineKeyboard: inlineKeyboard
                                );
                            }

                            if (notifyResult.Result == ResultType.Successful)
                            {
                                bankSlip.TelegramGroupID = notifyResult?.ChatID;
                                bankSlip.TelegramTopicID = notifyResult?.TopicID;
                                bankSlip.TelegramMessageID =
                                    Utility.TryParse(notifyResult?.Value, "int", 0);

                                bankSlip.TelegramCaption = captions;

                                await ctx.SaveChangesAsync();
                            }
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }

                    return Json(new
                    {
                        success = notifyResult.Result == ResultType.Successful,
                        message = notifyResult.Message ?? "Success"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}