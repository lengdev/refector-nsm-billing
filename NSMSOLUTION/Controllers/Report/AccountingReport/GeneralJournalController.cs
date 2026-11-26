using MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Report.AccountingReport
{
    public class GeneralJournalController : Controller
    {
        [Authorize(Roles = "Report General Journal")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            SetCompanyHeader(html);
            List<HtmlAddControls> htmlAddControls = html.HtmlAddControls;

            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = true,
                DateRangePicker = true,
                LabelFromDate = "Date Range:",
                DateTimePickerWithRanges = true,
                FromDateID = "TxtFromDate"
            });

            html.GH.GridPara = new GridPara[] 
            {
                new GridPara() { ParaName = "SystemID", Value = html.SystemID },
                new GridPara() { ParaName = "WarehouseID", Value = html.WarehouseID },
                new GridPara() { ParaName = "DomainID", Value = html.DomainID },
            };

            html.GH.SQL = "";
            html.ShowPaging = false;
            html.IsHaveWhereCondition = false;
            html.GroupableKendoGrid = true;
            html.ReportTitle = "Report General Journal";

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "Nº",           FieldName = "No",          TextAlign = "center", Width = "100px"},
                new GridField() { Header = "Class Name",   FieldName = "ClassName", IsHiddenField = true },
                new GridField() { Header = "Type",         FieldName = "InvoiceNoLink", TextAlign = "center", Width = "180px"},
                new GridField() { Header = "Date",         FieldName = "InvoiceDate",  FormatDateTime = "dd/MM/yyyy", ClassName = "text-center", Width = "150px" },
                new GridField() { Header = "Reference Nº", FieldName = "ReferenceNo",  ClassName = "text-center",Width = "250px"},
                new GridField() { Header = "Account Name", FieldName = "AccountName", Width = "250px" },
                new GridField() { Header = "Name",         FieldName = "CustomerName", Width = "250px"},
                new GridField() { Header = "Description",  FieldName = "Description", Width = "400px", ClassName = "wrap-cell" },
                new GridField() { Header = "Debit",        FieldName = "Debit",        Format = "#,##0.00;-#,##0.00;-", TextAlign = "right", ClassName = "text-right", IsTotalFooter = true,},
                new GridField() { Header = "Credit",       FieldName = "Credit",       Format = "#,##0.00;-#,##0.00;-", TextAlign = "right", ClassName = "text-right", IsTotalFooter = true,},
                new GridField() { Header = "IsSubtotal",   FieldName = "IsSubtotal",   IsHiddenField = true },
            };

            html.GH.GridProcedureName = "sp_GetGeneralJournal";
            KendoGridSetting kendoGridSetting = new KendoGridSetting
            {
                ShowHeaderInfoExcel = false,
                ServerPaging = false,
                ProcedureName = "sp_GetGeneralJournal"
            };
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString(), kendoGridSetting.ToJsonString());
            return View("~/Views/GeneralJournal/index.cshtml");
        }

        private void SetCompanyHeader(HTML html)
        {
            int systemId;
            if (!int.TryParse(html.SystemID, out systemId) || systemId <= 0)
                systemId = 13;

            int warehouseId;
            if (!int.TryParse(html.WarehouseID, out warehouseId))
                warehouseId = 0;

            int defaultClassId;
            switch (systemId)
            {
                case 13: defaultClassId = 5; break;
                case 28: defaultClassId = 14; break;
                case 25: defaultClassId = 10; break;
                case 27: defaultClassId = 12; break;
                default: defaultClassId = 0; break;
            }

            const string companySql = @"
                SELECT TOP 1 *
                FROM dbo.TblClass
                WHERE (@DefaultClassID > 0 AND ClassID = @DefaultClassID)
                   OR (@DefaultClassID = 0 AND (@SystemID = 0 OR SystemID = @SystemID))
                ORDER BY
                    CASE WHEN @DefaultClassID > 0 AND ClassID = @DefaultClassID THEN 0 ELSE 1 END,
                    CASE WHEN StatusID = 40 THEN 0 ELSE 1 END,
                    CASE WHEN (@WarehouseID > 0 AND WarehouseID = @WarehouseID) THEN 0 ELSE 1 END,
                    ClassID ASC";

            var parameters = new Dictionary<string, string>
            {
                { "DefaultClassID", defaultClassId.ToString() },
                { "SystemID", systemId.ToString() },
                { "WarehouseID", warehouseId.ToString() }
            };

            try
            {
                var companyData = html.LoadData(companySql, false, parameters);
                if (companyData == null || companyData.Tables.Count == 0 || companyData.Tables[0].Rows.Count == 0)
                {
                    System.Diagnostics.Trace.TraceWarning(
                        "General Journal report: company header was not found for SystemID {0}, WarehouseID {1}.",
                        systemId, warehouseId);
                    return;
                }

                var row = companyData.Tables[0].Rows[0];
                string companyKh = row.Table.Columns.Contains("KHName") && row["KHName"] != DBNull.Value
                    ? row["KHName"].ToString().Trim()
                    : row.Table.Columns.Contains("CompanyNameKH") && row["CompanyNameKH"] != DBNull.Value
                        ? row["CompanyNameKH"].ToString().Trim()
                        : "";
                string companyEn = row.Table.Columns.Contains("FullName") && row["FullName"] != DBNull.Value
                    ? row["FullName"].ToString().Trim()
                    : row.Table.Columns.Contains("CompanyName") && row["CompanyName"] != DBNull.Value
                        ? row["CompanyName"].ToString().Trim()
                        : row.Table.Columns.Contains("ClassName") && row["ClassName"] != DBNull.Value
                            ? row["ClassName"].ToString().Trim()
                            : "";

                if (!string.IsNullOrWhiteSpace(companyKh) && !companyKh.StartsWith("ក្រុមហ៊ុន"))
                    companyKh = "ក្រុមហ៊ុន " + companyKh;

                ViewBag.CompanyNameKh = companyKh;
                ViewBag.CompanyNameEn = companyEn;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("General Journal report: failed to load company header: {0}", ex.Message);
            }
        }
    }
}