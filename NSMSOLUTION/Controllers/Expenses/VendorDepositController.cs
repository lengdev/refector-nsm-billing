using MVC;
using NSMSOLUTION.Controllers.Expenses.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers.Expenses
{
    public class VendorDepositController : Controller
    {
        [HttpGet]



        public async Task<JsonResult> GetAvailableDeposits(
    int vendorId,
    int? vendorDepositId = null,
    int? expenseID = null,
    int? systemID = null,
    int? warehouseID = null,
    string domainID = null)
        {
            HTML html = new HTML();
            systemID = int.Parse(html.SystemID);
            warehouseID = int.Parse(html.WarehouseID);
            domainID = html.DomainID;
            if (vendorId <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Please select a vendor."
                }, JsonRequestBehavior.AllowGet);
            }

            if (!systemID.HasValue || systemID.Value <= 0 ||
                !warehouseID.HasValue || warehouseID.Value <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "System and warehouse are required."
                }, JsonRequestBehavior.AllowGet);
            }

            var deposits = new List<VendorDepositOptionViewModel>();

            try
            {
                //HTML html = new HTML();
                string connectionString = html.GetConnectionString();

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    const string sql = @"
SET NOCOUNT ON;

SELECT
    D.VendorDepositID,
    D.VendorID,
    D.TransactionID,
    D.DepositNo,
    D.DepositDate,
    D.Amount,
    ISNULL(D.AppliedAmount, 0) AS AppliedAmount,
    D.RemainingAmount,

    ISNULL(A.CurrentExpenseAppliedAmount, 0)
        AS CurrentExpenseAppliedAmount,

    -- Actual remaining amount for other expenses.
    D.Amount - ISNULL(D.AppliedAmount, 0)
        AS RemainingAvailableAmount,

    -- Add back this expense's previous application.
    D.Amount
        - ISNULL(D.AppliedAmount, 0)
        + ISNULL(A.CurrentExpenseAppliedAmount, 0)
        AS AvailableAmount,

    D.Description,
    D.Status

FROM dbo.TblVendorDeposit D

OUTER APPLY
(
    SELECT
        SUM(DA.AppliedAmount) AS CurrentExpenseAppliedAmount

    FROM dbo.TblVendorDepositApplication DA

    WHERE DA.VendorDepositID = D.VendorDepositID
      AND DA.ExpenseID = @ExpenseID
      AND DA.SystemID = @SystemID
      AND (
            DA.DomainID = @DomainID
            OR (DA.DomainID IS NULL AND @DomainID IS NULL)
          )

) A

WHERE D.VendorID = @VendorID
  AND D.SystemID = @SystemID
  AND D.WarehouseID = @WarehouseID
  AND (
        D.DomainID = @DomainID
        OR (D.DomainID IS NULL AND @DomainID IS NULL)
      )

  -- Include deposits with an available balance.
  -- Also include the previous deposit used by this expense.
  AND
  (
      (
          D.Amount
              - ISNULL(D.AppliedAmount, 0)
              + ISNULL(A.CurrentExpenseAppliedAmount, 0)
          > 0.005
      )

      OR

      (
          D.VendorDepositID = @VendorDepositID
          AND ISNULL(A.CurrentExpenseAppliedAmount, 0) > 0
      )
  )

ORDER BY
    CASE
        WHEN D.VendorDepositID = @VendorDepositID
            THEN 0
        ELSE 1
    END,

    D.DepositDate DESC,
    D.VendorDepositID DESC;
";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = 60;

                        command.Parameters.Add(
                            "@VendorID", SqlDbType.Int).Value = vendorId;

                        command.Parameters.Add(
                            "@VendorDepositID", SqlDbType.Int).Value =
                            vendorDepositId ?? 0;

                        command.Parameters.Add(
                            "@ExpenseID", SqlDbType.Int).Value =
                            expenseID ?? 0;

                        command.Parameters.Add(
                            "@SystemID", SqlDbType.Int).Value =
                            systemID.Value;

                        command.Parameters.Add(
                            "@WarehouseID", SqlDbType.Int).Value =
                            warehouseID.Value;

                        command.Parameters.Add(
                            "@DomainID", SqlDbType.NVarChar, 100).Value =
                            string.IsNullOrWhiteSpace(domainID)
                                ? (object)DBNull.Value
                                : domainID;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                deposits.Add(
                                    new VendorDepositOptionViewModel
                                    {
                                        VendorDepositID =
                                            Convert.ToInt32(
                                                reader["VendorDepositID"]),

                                        VendorID =
                                            Convert.ToInt32(
                                                reader["VendorID"]),

                                        TransactionID =
                                            reader["TransactionID"] == DBNull.Value
                                                ? 0
                                                : Convert.ToInt32(
                                                    reader["TransactionID"]),

                                        DepositNo =
                                            Convert.ToString(
                                                reader["DepositNo"]),

                                        DepositDate =
                                            reader["DepositDate"] == DBNull.Value
                                                ? ""
                                                : Convert.ToDateTime(
                                                    reader["DepositDate"])
                                                    .ToString("dd/MM/yyyy"),

                                        Amount =
                                            reader["Amount"] == DBNull.Value
                                                ? 0m
                                                : Convert.ToDecimal(
                                                    reader["Amount"]),

                                        AppliedAmount =
                                            reader["AppliedAmount"] == DBNull.Value
                                                ? 0m
                                                : Convert.ToDecimal(
                                                    reader["AppliedAmount"]),

                                        RemainingAmount =
                                            reader["RemainingAmount"] == DBNull.Value
                                                ? 0m
                                                : Convert.ToDecimal(
                                                    reader["RemainingAmount"]),

                                        CurrentExpenseAppliedAmount =
                                            reader["CurrentExpenseAppliedAmount"]
                                                == DBNull.Value
                                                ? 0m
                                                : Convert.ToDecimal(
                                                    reader["CurrentExpenseAppliedAmount"]),

                                        AvailableAmount =
                                            reader["AvailableAmount"] == DBNull.Value
                                                ? 0m
                                                : Convert.ToDecimal(
                                                    reader["AvailableAmount"]),

                                        Description =
                                            reader["Description"] == DBNull.Value
                                                ? ""
                                                : Convert.ToString(
                                                    reader["Description"]),

                                        Status =
                                            Convert.ToString(
                                                reader["Status"])
                                    });
                            }
                        }
                    }
                }

                return Json(new
                {
                    success = true,
                    data = deposits
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Logger.Error(ex);

                Response.StatusCode = 500;

                return Json(new
                {
                    success = false,
                    message = "Unable to load vendor deposits.",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}