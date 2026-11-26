using Microsoft.Office.Interop.Excel;
using MVC;
using NSMSOLUTION.Features.Infrastructure;
using NSMSOLUTION.Features.Vendors.Models;
using NSMSOLUTION.Features.Vendors.Repositories;
//using NSMSOLUTION.Features.Vendors.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace NSMSOLUTION.Vendors.Contorllers
{
    //[Authorize]
    public class VendorsController : Controller
    {
        private readonly VendorCreateRepository _repo = new VendorCreateRepository();
        //private readonly IVendorService _service;

        //public VendorsController()
        //    : this(new VendorService())
        //{
        //}

        //public VendorsController(IVendorService service)
        //{
        //    _service = service;
        //}

        private CurrentUserContext CurrentContext
        {
            get
            {
                return new CurrentUserContext();
            }
        }

        // =========================================
        // VendorsIndex
        // =========================================
        public ActionResult LoadVendor()
        {
            HTML html = new HTML();
            string sql = @"SELECT DISTINCT
    v.VendorID,
    v.VendorName,
    v.VendorCode
    FROM dbo.TblVendor v
    where v.SystemID = " + html.SystemID + " and v.WarehouseID = " + html.WarehouseID + " and v.DomainID like '%" + html.DomainID + @"%'
ORDER BY v.VendorID desc;";

            var Data = html.JsonData(sql);
            return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult VendorsIndex()
        {
            if (string.IsNullOrWhiteSpace(
                CurrentContext.DomainID))
                return new HttpStatusCodeResult(403);

            return View();
        }
        public ActionResult ExpenseForm()
        {
            HTML html = new HTML();

            var classList = new List<SelectListItem>();
            var paymentMethodList = new List<SelectListItem>();
            var UserList = new List<SelectListItem>();
            var expenseAccountList = new List<object>();

            string sql = @"

        -- =========================================================
        -- CLASS
        -- =========================================================
        SELECT
            ClassID,
            ClassName
        FROM dbo.TblClass
        WHERE SystemID = @SystemID
          AND WarehouseID = @WarehouseID
        ORDER BY ClassName;


        -- =========================================================
        -- PAYMENT METHOD
        -- =========================================================
        SELECT
            PaymentMethodID,
            PaymentMethod
        FROM dbo.TblPaymentMethod
        WHERE SystemID = @SystemID
          AND WarehouseID = @WarehouseID
          AND PaymentMethod IN ('Bank Transfer', 'Cheque', 'Petty Cash')
        ORDER BY PaymentMethod;


        -- =========================================================
        -- EXPENSE ACCOUNT
        -- =========================================================
        SELECT
    coa.ChartOfAccountID,
    cat.AccountCategory,
    ct.AccountTypes,
    coa.AccountNo,
    coa.AccountName,
    coa.ParentAccountID
FROM dbo.TblChartOfAccount AS coa
INNER JOIN dbo.TblAccountTypes AS ct
    ON ct.AccountTypesID = coa.AccountTypesID
INNER JOIN dbo.TblAccountCategories AS cat
    ON cat.AccountCategoryID = coa.AccountCategoryID
WHERE
    coa.SystemID = @SystemID
    AND coa.WarehouseID = @WarehouseID
ORDER BY
    ct.AccountTypes ASC,
    coa.AccountName ASC;

select u.UserLoginID, u.FullName
from dbo.TblUserLogin u
where isnull(u.TelegramUserName,'') <> '' and u.StatusID = 3 and u.SystemID = @SystemID order by u.FullName

    ";

            using (var cn = new SqlConnection(html.GetConnectionString()))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value = html.SystemID;
                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value = html.WarehouseID;

                cn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    // =====================================================
                    // 1. CLASS LIST
                    // =====================================================
                    while (reader.Read())
                    {
                        classList.Add(new SelectListItem
                        {
                            Value = reader["ClassID"].ToString(),
                            Text = reader["ClassName"].ToString()
                        });
                    }

                    // =====================================================
                    // 2. PAYMENT METHOD LIST
                    // =====================================================
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            paymentMethodList.Add(new SelectListItem
                            {
                                Value = reader["PaymentMethodID"].ToString(),
                                Text = reader["PaymentMethod"].ToString()
                            });
                        }
                    }                    

                    // =====================================================
                    // 3. EXPENSE ACCOUNT LIST
                    // =====================================================
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            expenseAccountList.Add(new
                            {
                                ChartOfAccountID = reader["ChartOfAccountID"],
                                AccountCategory = reader["AccountCategory"].ToString(),
                                AccountTypes = reader["AccountTypes"].ToString(),
                                AccountNo = reader["AccountNo"].ToString(),
                                AccountName = reader["AccountName"].ToString(),
                                ParentAccountID = reader["ParentAccountID"] == DBNull.Value
                                    ? (int?)null
                                    : Convert.ToInt32(reader["ParentAccountID"])
                            });
                        }
                    }
                    
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            UserList.Add(new SelectListItem
                            {
                                Value = reader["UserLoginID"].ToString(),
                                Text = reader["FullName"].ToString()
                            });
                        }
                    }
                }
            }

            // =============================================================
            // VIEWBAG
            // =============================================================
            ViewBag.ClassList = classList;
            ViewBag.PaymentMethodList = paymentMethodList;
            ViewBag.ExpenseAccountList = expenseAccountList;
            ViewBag.UserList = UserList;

            return View();
        }
        
        public ActionResult GetPayeeList()
        {
            HTML html = new HTML();

            string sqlPayee = @"
        SELECT 
            a.PayeeID,
            a.Code,
            a.[Name],
            a.[Type],
            a.OrderBy,
            ISNULL(a.Gender, '') AS Gender,
            ISNULL(a.PhoneNumber, '') AS PhoneNumber
        FROM
        (
            -- ========================
            -- Vendor
            -- ========================
            SELECT
                v.VendorID AS PayeeID,
                CAST(v.VendorCode AS NVARCHAR(100)) COLLATE DATABASE_DEFAULT AS Code,
                CAST(v.VendorName AS NVARCHAR(200)) COLLATE DATABASE_DEFAULT AS [Name],
                CAST('Vendor' AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS [Type],
                1 AS OrderBy,
                CAST(ISNULL(v.Gender, '') AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS Gender,
                CAST(ISNULL(v.ContactPhoneNumber, '') AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS PhoneNumber
            FROM dbo.TblVendor v
            WHERE v.SystemID = @SystemID
              AND v.WarehouseID = @WarehouseID

            UNION ALL

            -- ========================
            -- Customer
            -- ========================
            SELECT
                c.CustomerID AS PayeeID,
                CAST(c.CustomerCode AS NVARCHAR(100)) COLLATE DATABASE_DEFAULT AS Code,
                CAST(c.CustomerName AS NVARCHAR(200)) COLLATE DATABASE_DEFAULT AS [Name],
                CAST('Customer' AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS [Type],
                3 AS OrderBy,
                CAST(ISNULL(c.CustomerGender, '') AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS Gender,
                CAST(ISNULL(c.CustomerContactMobileNumber, '') AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS PhoneNumber
            FROM dbo.TblCustomer c
            WHERE ISNULL(c.CustomerCode, '') <> ''
              AND c.SystemID = @SystemID
              AND c.WarehouseID = @WarehouseID

            UNION ALL

            -- ========================
            -- Employee
            -- ========================
            SELECT
                e.EmployeeID AS PayeeID,
                CAST('' AS NVARCHAR(100)) COLLATE DATABASE_DEFAULT AS Code,
                CAST(e.FullName AS NVARCHAR(200)) COLLATE DATABASE_DEFAULT AS [Name],
                CAST('Employee' AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS [Type],
                2 AS OrderBy,
                CAST(ISNULL(g.GenderName, '') AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS Gender,
                CAST(ISNULL(p.PhoneNumber1, '') AS NVARCHAR(50)) COLLATE DATABASE_DEFAULT AS PhoneNumber
            FROM HRMS_DB.dbo.Employees e
            LEFT JOIN HRMS_DB.dbo.Gender g
                ON g.GenderID = e.GenderID
            LEFT JOIN HRMS_DB.dbo.Staffing_EmployeePhones p
                ON p.EmployeeID = e.EmployeeID

        ) AS a

        -- Exclude NULL, empty, and whitespace-only names
        WHERE LTRIM(RTRIM(ISNULL(a.[Name], ''))) <> ''

        ORDER BY
            a.OrderBy,
            a.[Name] ASC;
    ";

            var payeeList = new List<object>();

            using (var cn = new SqlConnection(html.GetConnectionString()))
            using (var cmd = new SqlCommand(sqlPayee, cn))
            {
                cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value = html.SystemID;
                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value = html.WarehouseID;

                cn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        payeeList.Add(new
                        {
                            PayeeID = reader["PayeeID"],
                            Code = reader["Code"].ToString(),
                            Name = reader["Name"].ToString(),
                            Type = reader["Type"].ToString(),
                            OrderBy = reader["OrderBy"],
                            Gender = reader["Gender"].ToString(),
                            PhoneNumber = reader["PhoneNumber"].ToString()
                        });
                    }
                }
            }

            return Json(payeeList, JsonRequestBehavior.AllowGet);
        }
        // =========================================
        // GRID DATA
        // =========================================

        //[HttpGet]
        //public ActionResult GetVendors(string searchText = null)
        //{
        //    if (string.IsNullOrWhiteSpace(
        //        CurrentContext.DomainID))
        //        return new HttpStatusCodeResult(403);

        //    var result = _service.GetAll(
        //        CurrentContext.DomainID,
        //        CurrentContext.SystemID,
        //        CurrentContext.WarehouseID,
        //        searchText);

        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        // =========================================
        // DETAILS
        // =========================================

        //public ActionResult Details(int id)
        //{
        //    var model = _service.GetById(
        //        id,
        //        CurrentContext.DomainID,
        //        CurrentContext.SystemID,
        //        CurrentContext.WarehouseID);

        //    if (model == null)
        //        return HttpNotFound();

        //    return View(model);
        //}

        // =========================================
        // CREATE GET
        // =========================================


        [HttpGet]
        public ActionResult Create_Vendor(int? PrimaryFieldValue)
        {
            HTML html = new HTML();

            bool isEdit = PrimaryFieldValue.GetValueOrDefault() > 0;

            var model = new VendorModel
            {
                RegisterDate = DateTime.Today,
                PaymentTerms = "Due on Receipt",
                Status = "Enable"
            };

            // ==========================================
            // EDIT: LOAD EXISTING VENDOR
            // ==========================================
            if (isEdit)
            {
                string vendorSql = @"
            SELECT *
            FROM dbo.TblVendor
            WHERE VendorID = @VendorID
              AND (@SystemID IS NULL OR SystemID = @SystemID)
              AND ISNULL(IsDeleted, 0) = 0;
        ";

                using (var cn = new SqlConnection(html.GetConnectionString()))
                using (var cmd = new SqlCommand(vendorSql, cn))
                {
                    cmd.Parameters.Add("@VendorID", SqlDbType.Int)
                        .Value = PrimaryFieldValue.Value;

                    //cmd.Parameters.Add("@DomainID", SqlDbType.NVarChar, 50)
                    //    .Value = html.DomainID;

                    cmd.Parameters.Add("@SystemID", SqlDbType.Int)
                        .Value = html.SystemID;

                    cn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return HttpNotFound("Vendor not found.");

                        // Map database values to your existing VendorModel.
                        model.VendorID = Convert.ToInt32(reader["VendorID"]);

                        model.VendorCode = Convert.ToString(reader["VendorCode"]);
                        model.VendorName = Convert.ToString(reader["VendorName"]);
                        model.VendorTypeID = reader["VendorTypeID"] == DBNull.Value
                            ? (int?)null
                            : Convert.ToInt32(reader["VendorTypeID"]);

                        model.VendorType = Convert.ToString(reader["VendorType"]);
                        model.Gender = Convert.ToString(reader["Gender"]);
                        model.Phone = Convert.ToString(reader["Phone"]);
                        model.Address = Convert.ToString(reader["Address"]);
                        model.Email = Convert.ToString(reader["Email"]);
                        model.Website = Convert.ToString(reader["Website"]);

                        model.ContactName = Convert.ToString(reader["ContactName"]);
                        model.ContactPersonName = Convert.ToString(reader["ContactPersonName"]);
                        model.ContactPhoneNumber = Convert.ToString(reader["ContactPhoneNumber"]);
                        model.Position = Convert.ToString(reader["Position"]);

                        model.PaymentTerms = Convert.ToString(reader["PaymentTerms"]);
                        model.CompanyName = Convert.ToString(reader["CompanyName"]);
                        model.VATTIN = Convert.ToString(reader["VATTIN"]);
                        model.CompanyAddress = Convert.ToString(reader["CompanyAddress"]);
                        model.Remark = Convert.ToString(reader["Remark"]);

                        model.BankerName = Convert.ToString(reader["BankerName"]);
                        model.SwiftCode = Convert.ToString(reader["SwiftCode"]);
                        model.BankerAddress = Convert.ToString(reader["BankerAddress"]);
                        model.AccountNo = Convert.ToString(reader["AccountNo"]);
                        model.AccountName = Convert.ToString(reader["AccountName"]);
                        model.BankerRemark = Convert.ToString(reader["BankerRemark"]);

                        model.RegisterDate = reader["RegisterDate"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(reader["RegisterDate"]);

                        // Map additional fields used by your Razor view.
                        model.HouseNo = Convert.ToString(reader["HouseNo"]);
                        model.Street = Convert.ToString(reader["Street"]);
                        model.Commune = Convert.ToString(reader["Commune"]);
                        model.District = Convert.ToString(reader["District"]);
                        model.City = Convert.ToString(reader["City"]);

                        model.LocalVendorName = Convert.ToString(reader["LocalVendorName"]);
                        model.LocalHouseNo = Convert.ToString(reader["LocalHouseNo"]);
                        model.LocalStreet = Convert.ToString(reader["LocalStreet"]);
                        model.LocalCommune = Convert.ToString(reader["LocalCommune"]);
                        model.LocalDistrict = Convert.ToString(reader["LocalDistrict"]);
                        model.LocalCity = Convert.ToString(reader["LocalCity"]);

                        model.VATTINPrefix = Convert.ToString(reader["VATTINPrefix"]);
                        model.Currency = Convert.ToString(reader["Currency"]);
                    }
                }
            }

            // ==========================================
            // LOAD VENDOR TYPES (NEW + EDIT)
            // ==========================================
            model.VendorTypes = new List<SelectListItem>();

            string typeSql = @"
        SELECT VendorTypeID, VendorType
        FROM dbo.TblVendorType
        WHERE SystemID = @SystemID
          AND WarehouseID = @WarehouseID
        ORDER BY VendorType;
    ";

            using (var cn = new SqlConnection(html.GetConnectionString()))
            using (var cmd = new SqlCommand(typeSql, cn))
            {
                cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value = html.SystemID;
                cmd.Parameters.Add("@WarehouseID", SqlDbType.Int).Value = html.WarehouseID;

                cn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.VendorTypes.Add(new SelectListItem
                        {
                            Value = reader["VendorTypeID"].ToString(),
                            Text = reader["VendorType"].ToString(),
                            Selected = model.VendorTypeID.HasValue &&
                                model.VendorTypeID.Value ==
                                Convert.ToInt32(reader["VendorTypeID"])
                        });
                    }
                }
            }

            ViewBag.PrimaryFieldValue = isEdit ? PrimaryFieldValue.Value : 0;
            ViewBag.IsEdit = isEdit;

            ViewBag.SuccessMessage = TempData["VendorSuccess"];
            ViewBag.CreatedVendorCode = model.VendorCode;

            return View(model);
        }

        //[HttpGet]
        //public ActionResult Create()
        //{
        //    var model = new VendorCreateViewModel
        //    {
        //        Status = "Enable",
        //        PaymentTerms = "Due on Receipt",
        //        Currency = "USD",
        //        RegisterDate = DateTime.Today
        //    };

        //    return View(model);
        //}


        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create_Vendor(int? PrimaryFieldValue, VendorModel model)
        {
            try
            {
                HTML html = new HTML();

                var repository = new VendorCreateRepository();

                var scope = new VendorCreateScope
                {
                    DomainID = html.DomainID,
                    SystemID = int.Parse(html.SystemID),
                    WarehouseID = int.Parse(html.WarehouseID),
                    UserID = int.Parse(html.UserID),
                    CreateByUserID = int.Parse(html.UserID),
                    Username = User.Identity.Name
                };

                bool isEdit = PrimaryFieldValue.GetValueOrDefault() > 0;

                if (isEdit)
                {
                    // ==================================
                    // UPDATE EXISTING VENDOR
                    // ==================================

                    bool updated = repository.Update(
                        PrimaryFieldValue.Value,
                        model,
                        scope
                    );

                    if (!updated)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Unable to update vendor."
                        });
                    }

                    return Json(new
                    {
                        success = true,
                        message = "Vendor updated successfully.",
                        vendorId = PrimaryFieldValue.Value,
                        vendorCode = "",
                        vendorName = model.VendorName
                    });
                }
                else
                {
                    // ==================================
                    // INSERT NEW VENDOR
                    // ==================================

                    var result = repository.Create(model, scope);

                    return Json(new
                    {
                        success = true,
                        message = "Vendor created successfully.",
                        vendorId = result.VendorID,
                        vendorCode = result.VendorCode,
                        vendorName = model.VendorName
                    });
                }
            }
            catch (SqlException ex)
            {
                string message = ex.Number == 50001
                    ? "A vendor with this name already exists."
                    : ex.Number == 50002
                        ? "Vendor not found or access denied."
                        : "Database error: " + ex.Message;

                return Json(new
                {
                    success = false,
                    message = message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        // Return JSON error with HTTP status code
        private ActionResult JsonError(string message, int statusCode)
        {
            Response.StatusCode = statusCode;

            return Json(new
            {
                success = false,
                message = message,
                errors = (object)null
            });
        }

        // Collect MVC ModelState validation errors
        private object GetModelErrors()
        {
            return ModelState
                .Where(x => x.Value.Errors.Any())
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.Errors
                        .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? "Invalid value."
                            : e.ErrorMessage)
                        .ToArray()
                );
        }
        private VendorCreateScope GetScope()
        {
            // Adapt these lookups to your real authenticated multi-branch context.
            //var ctx = Session["CurrentUserContext"];
            HTML html = new HTML();
            return new VendorCreateScope
            {
                DomainID = html.DomainID,
                SystemID = int.Parse(html.SystemID),
                WarehouseID = int.Parse(html.WarehouseID),
                UserID = int.Parse(html.UserID),
                CreateByUserID = int.Parse(html.UserID),
                Username = html.UserFullName
            };
        }

        private int? GetSessionInt(string key)
        {
            int value;
            return int.TryParse(Convert.ToString(Session[key]), out value) ? (int?)value : null;
        }

        private static string GetProperty(object source, string name)
        {
            if (source == null) return null;
            var p = source.GetType().GetProperty(name);
            return p == null ? null : Convert.ToString(p.GetValue(source, null));
        }

        private static int? GetIntProperty(object source, string name)
        {
            int value;
            var text = GetProperty(source, name);
            return int.TryParse(text, out value) ? (int?)value : null;
        }
        // =========================================
        // EDIT GET
        // =========================================

        //[HttpGet]
        //public ActionResult Edit(int id)
        //{
        //    var model = _service.GetById(
        //        id,
        //        CurrentContext.DomainID,
        //        CurrentContext.SystemID,
        //        CurrentContext.WarehouseID);

        //    if (model == null)
        //        return HttpNotFound();

        //    return View(model);
        //}

        // =========================================
        // EDIT POST
        // =========================================

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(VendorModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    try
        //    {
        //        bool updated = _service.Update(
        //            model,
        //            CurrentContext.DomainID,
        //            CurrentContext.SystemID,
        //            CurrentContext.WarehouseID);

        //        if (!updated)
        //            return HttpNotFound();

        //        TempData["Success"] =
        //            "Vendor updated successfully.";

        //        return RedirectToAction(
        //            "Details",
        //            new { id = model.VendorID });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        ModelState.AddModelError("", ex.Message);
        //    }
        //    catch (Exception)
        //    {
        //        ModelState.AddModelError("",
        //            "Unable to update vendor. Please try again.");
        //    }

        //    return View(model);
        //}

        // =========================================
        // DELETE POST
        // =========================================

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id)
        //{
        //    try
        //    {
        //        bool deleted = _service.Delete(
        //            id,
        //            CurrentContext.DomainID,
        //            CurrentContext.SystemID,
        //            CurrentContext.WarehouseID);

        //        if (!deleted)
        //        {
        //            TempData["Error"] =
        //                "Vendor cannot be deleted. It may have accounting transactions or may not exist.";
        //        }
        //        else
        //        {
        //            TempData["Success"] =
        //                "Vendor deactivated successfully.";
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        TempData["Error"] =
        //            "Unable to deactivate vendor.";
        //    }

        //    return RedirectToAction("Index");
        //}
    }
}