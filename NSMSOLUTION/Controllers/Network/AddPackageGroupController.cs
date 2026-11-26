using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddPackageGroupController : Controller
    {
        // GET: AddPackageGroupController
        [Authorize(Roles = "Add Package Group, Edit Package Group, Delete Package Group")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Package Group"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Package Group"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Package Group"))
                html.AllowDelete = true;
            html.DialogBoxWidth = "Auto";
            html.AddButton = "Add Package Group";
            html.AddURL = "AddPackageGroup/CreatePackageGroup";
            html.TableName = "TblPackageGroup";
            html.PrimaryField = "PackageGroupID";
            html.OrderBy = "PackageGroupID desc";
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtRouterName",
                Label = "Router Name:",
                DBField = "RouterID",
                SQL_CBO = "select distinct r.RouterID,r.RouterName from TblPackageGroup pg\r\ninner join dbo.TblRouter r on r.RouterID = pg.RouterID where pg.SystemID = " + html.SystemID + " and pg.WarehouseID = " + html.WarehouseID + " order by r.RouterName",
                DisplayText = "RouterName",
                Value = "RouterID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Package Group Name",
                SQL = "select PackageGroupID, PackageGroupName from dbo.TblPackageGroup where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by PackageGroupName",
                SelectValue = "PackageGroupID",
                DisplayField = "PackageGroupName",
                RadioValue = EncryAndDecry.Encryp("PackageGroupID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Package Group Name", FieldName = "PackageGroupName" },
                new GridField() { Header = "Bandwidth (Mbps)", FieldName = "Bandwidth", TextAlign="right" },
//                new GridField() { Header = "Duration", FieldName = "Duration", TextAlign="right" },
  //              new GridField() { Header = "UM", FieldName = "UM" },
                new GridField() { Header = "Is.D.S9", FieldName = "IsDoubleSpeedAtNight" , Width = "100%"},
                new GridField() { Header = "Action", FieldName = "PackageGroupID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add Package Group, Edit Package Group")]
        public ActionResult CreatePackageGroup(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FormID = "CreatePackageGroup",
                FieldIDFocus = "TxtPackageGroupName",
                TableName = "TblPackageGroup",
                PrimaryField = "PackageGroupID",
                PrimaryFieldValue = PrimaryFieldValue
            };
            string sql = "select FromTime1,FromTime2,ToTime1,ToTime2,ToTime3,ToTime4, IsDoubleSpeedAtNight from dbo.TblPackageGroup where PackageGroupID=@PrimaryFieldValue";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                {"PrimaryFieldValue",PrimaryFieldValue.ToString() }
            };
            ViewBag.data = html.JsonData(sql, false, para);

            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Service Name:",
            //    ID = "TxtServiceName",
            //    //DBField = "ServiceID",
            //    SQL_CBO = "select ServiceID, ServiceName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by ServiceName",
            //    Value = "ServiceID",
            //    DisplayText = "ServiceName",
            //    ISCallBackToCbo = true,
            //    SQLCallBack = "select PackageTypeID, PackageTypeName from dbo.TblPackageType where ServiceID = @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by PackageTypeName",
            //    CallBackDisplayText = "PackageTypeName",
            //    CallBackDisplayValue = "PackageTypeID",
            //    CallBackToControlID = "TxtPackageTypeName",
            //});
            //html.HTMLControls.Add(new HtmlComboBox()
            //{
            //    Label = "Connection Type:",
            //    ID = "TxtPackageTypeName",
            //});
            html.UseTable = true;
            html.TableColumn = 2;
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Router:",
                ID = "TxtRouter",
                SQL_CBO = "select RouterID, RouterName from dbo.TblRouter where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by RouterName",
                Value = "RouterID",
                DisplayText = "RouterName",
                DBField = "RouterID",
                Required = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Package Group Name:",
                ID = "TxtPackageGroupName",
                DBField = "PackageGroupName",
                Required = true,
                ISUniqueField = true,
                CheckExistingWithField = "WarehouseID",
                CheckExistingWithDefaultValue = html.WarehouseID
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Bandwidth (Mbps):",
                ID = "TxtBandwidth",
                DBField = "Bandwidth",
                Required = true,
                ISNumeric = true,
                ISInteger = true,
            });
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "Duration:",
            //    ID = "TxtDuration",
            //    DBField = "Duration",
            //    Required = true,
            //    ISNumeric = true,
            //    ISInteger = true,
            //});
            //html.HTMLControls.Add(new HtmlTextBox()
            //{
            //    Label = "UM:",
            //    ID = "TxtUM",
            //    DBField = "UM",
            //    Required = true,
            //});
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "IP POOL / Group Name:",
                ID = "TxIPPOOL",
                DBField = "IPPOOL",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Status:",
                ID = "TxtStatus",
                DBField = "StatusID",
                SQL_CBO = "select StatusID, [Status] from dbo.TblStatus where StatusID in (40,43)",
                DisplayText = "Status",
                Value = "StatusID"
            });
            html.HTMLControls.Add(new HtmlCheckBox()
            {
                ISCheckBox = true,
                Label = "Is Double Speed At Night:",
                ID = "IsDSpeed",
                DBField = "IsDoubleSpeedAtNight",
                Checked = true,
                Value = "True"
            });
            //html.HTMLControls.Add(new HtmlTextarea()
            //{
            //    Label = "Memo:",
            //    ID = "TxtMemo",
            //    DBField = "Memo",
            //});
            Dictionary<string, string> addCustomFields = new Dictionary<string, string> {
                {"FromTime1","($(\"#FromTime\").data('daterangepicker').startDate.format('HH:mm:ss') == '00:00:00' ? '0s' : $(\"#FromTime\").data('daterangepicker').startDate.format('HH') + 'h' + $(\"#FromTime\").data('daterangepicker').startDate.format('mm') + 'm' + $(\"#FromTime\").data('daterangepicker').startDate.format('ss') + 's')" },
                {"FromTime2","($(\"#FromTime\").data('daterangepicker').endDate.format('HH:mm:ss') == '00:00:00' ? '0s' : $(\"#FromTime\").data('daterangepicker').endDate.format('HH') + 'h' + $(\"#FromTime\").data('daterangepicker').endDate.format('mm') + 'm' + $(\"#FromTime\").data('daterangepicker').endDate.format('ss') + 's')" },
                {"ToTime1","($(\"#ToTime\").data('daterangepicker').startDate.format('HH:mm:ss') == '00:00:00' ? '0s' : $(\"#ToTime\").data('daterangepicker').startDate.format('HH') + 'h' + $(\"#ToTime\").data('daterangepicker').startDate.format('mm') + 'm' + $(\"#ToTime\").data('daterangepicker').startDate.format('ss') + 's')" },
                {"ToTime2","($(\"#ToTime\").data('daterangepicker').endDate.format('HH:mm:ss') == '00:00:00' ? '0s' : $(\"#ToTime\").data('daterangepicker').endDate.format('HH') + 'h' + $(\"#ToTime\").data('daterangepicker').endDate.format('mm') + 'm' + $(\"#ToTime\").data('daterangepicker').endDate.format('ss') + 's')" },
                {"ToTime3","'0s'" },
                //{"var HH","$(\"#ToTime\").data('daterangepicker').startDate.format('HH');HH = (parseInt(HH) - 13) + \"h\";" },
                {"ToTime4","(parseInt($(\"#ToTime\").data('daterangepicker').startDate.format('HH'))- 13)+'h59m59s'" },
            };

            html.AddCustomFields = addCustomFields;
            html.RestFields = "TxtPackageGroupName, TxtBandwidth, TxIPPOOL";
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
    }
}