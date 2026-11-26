using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Finance
{
    public class DepositController : Controller
    {
        // GET: Deposit
        [Authorize(Roles = "Add Deposit, Edit Deposit, Delete Deposit")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Deposit"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Deposit"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Deposit"))
                html.AllowDelete = true;

            html.AddButton = "Add Deposit";
            html.AddURL = "Deposit/AddDeposit";
            html.TableName = "TblAccount_BankDeposit";
            html.PrimaryField = "BankDepositID";
            html.OrderBy = "BankDepositID desc";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "AccountName",
                SQL = "select ab.AccountID, coa.AccountNo + '-' + coa.AccountName AccountName from TblAccount_BankDeposit ab inner join dbo.TblChartOfAccount coa on coa.ChartOfAccountID = ab.AccountID where ab.SystemID = " + html.SystemID + " and ab.WarehouseID = " + html.WarehouseID + " order by coa.AccountNo + '-' + coa.AccountName",
                SelectValue = "AccountID",
                DisplayField = "AccountName",
                RadioValue = EncryAndDecry.Encryp("AccountID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Account Name", FieldName = "AccountName" },
                new GridField() { Header = "Action", FieldName = "BankDepositID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowGridOnly = false;
            html.DialogBoxWidth = "auto";
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Deposit, Edit Deposit")]
        public ActionResult AddDeposit(string PrimaryFieldValue)
        {
            HTML html = new HTML();
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle="Add funds to this deposit",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlMyTable()
                        {
                            ShowFooter=false,
                            HtmlTblID="Table_AddFundsToDeposit",
                            InputName="ReceivedFrom",
                            TRName="TR_No",
                            TDName="TD_No",
                            //ScriptCallBackFunction="CallSetAutoComplete_Attribute",
                            ScriptCallBackFunction="Reload_Account",
                            tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>",
                                    Label="#"
                                },
                                new TblTHead()
                                {
                                     Label="Received From", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Account", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Description", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Payment Method", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Reference #", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Amount", Class="text-nowrap",
                                     CloseTR="</tr>"
                                },
                            },
                            tblTBody= new List<TblTBody>()
                            {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TR_No\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TD_No"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ReceivedFrom",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Received From",
                                            DeleteTDRowFunction="RemoveTableRow('TR_No', 'TD_No', this,'','ReceivedFrom');",
                                            //SQL_AutoComplete = "select distinct top 10 [Attribute] from dbo.TblItemAttribute where lower([Attribute]) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID +" order by Attribute",
                                            //ISAutoComplete = true
                                        },
                                        new HtmlComboBox()
                                        {
                                            Name="Account",
                                            Class="input2", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Account",
                                            SQL_CBO=@"select distinct coa.ChartOfAccountID AccountID, ac.AccountCategory, act.AccountTypes, coa.AccountNo, coa.AccountName
from dbo.TblAccountTypes act
inner join dbo.TblAccountCategories ac on ac.AccountCategoryID = act.AccountCategoryID
inner join dbo.TblChartOfAccount coa on coa.AccountType = act.AccountTypesID
where ac.AccountCategoryID = 1 and act.SystemID = " + html.SystemID + " and act.WarehouseID = " + html.WarehouseID,
                                            DBField = "AccountID",
                                            Value = "AccountID",
                                            DisplayText = "AccountName",
                                            GroupByField = "AccountTypes",
                                            IsKendoMultiColumnComboBox = true,
                                            KendoComboBoxColumn = new List<KendoComboBoxColumns>
                                            {
                                                new KendoComboBoxColumns() { field = "AccountNo", title = "Account No", width = "120" },
                                                new KendoComboBoxColumns() { field = "AccountName", title = "Account Name", width = "550" }
                                            },
                                            KendoMultiColumnComboBoxHeight = 350,
                                            Required = true,
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Description",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Description"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="PaymentMethod",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Payment Method"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Reference",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Reference"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Amount",
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DivClass="wrap-input3",
                                            placeholder="Enter Amount"
                                        },
                                    },
                                    CloseTR="</tr>"
                                }
                            }
                        }
                    }
                },
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
    }
}