using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace NSMSOLUTION.Controllers.API
{
    /// <summary>
    /// Other Charges API - Provides product catalog endpoints for System 27
    /// </summary>
    //[Authorize]
    [RoutePrefix("api/products")]
    public class APIBSController : ApiController
    {
        // ─────────────────────────────────────────────────────────────
        // Constants
        // ─────────────────────────────────────────────────────────────
        private const int TARGET_SYSTEM = 27;
        private const string IMAGE_REFERENCE_TYPE = "OtherCharge";

        /// <summary>
        /// Ensures the filename includes the Uploads/Services path as a relative URL.
        /// </summary>
        private string BuildRelativeImageUrl(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            if (fileName.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return fileName;

            string relativePath = fileName;
            if (fileName.IndexOf("Uploads/Services/", StringComparison.OrdinalIgnoreCase) < 0)
            {
                relativePath = "/Uploads/Services/" + fileName.TrimStart('/');
            }

            return relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
        }

        /// <summary>
        /// Loads all images for a set of OtherChargeIDs from TblOtherChargeImage
        /// using the new ReferenceID / ReferenceType schema.
        /// </summary>
        private async Task<ILookup<int, string>> LoadImagesLookupAsync(
            DBContext ctx, IEnumerable<int> otherChargeIds)
        {
            var ids = otherChargeIds.ToList();
            var images = await ctx.TblOtherChargeImage
                .Where(img => img.ReferenceType == IMAGE_REFERENCE_TYPE
                           && ids.Contains(img.ReferenceID))
                .ToListAsync();

            return images.ToLookup(
                img => img.ReferenceID,
                img => BuildRelativeImageUrl(img.FileName));
        }

        // ─────────────────────────────────────────────────────────────
        // GET api/v1/products?page=1&pageSize=10&status=Enable&search=Banner&serviceId=5
        //   Returns a flat paginated list of Products belonging to System 27.
        // ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Returns a flat paginated list of Other Charges (products) belonging to System 27.
        /// </summary>
        /// <param name="page">Page number (default: 1, min: 1)</param>
        /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
        /// <param name="status">Optional Status filter (e.g., "Enable", "Disable")</param>
        /// <param name="search">Search text for OtherChargeItem (case-insensitive)</param>
        /// <param name="serviceId">Optional ServiceID / CategoryID filter</param>
        /// <param name="categoryId">Optional CategoryID filter (alias for serviceId)</param>
        /// <returns>ApiResult with paginated OtherCharge data and metadata</returns>
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetAll(
            int page = 1,
            int pageSize = 10,
            string status = null,
            string search = null,
            int? serviceId = null,
            int? categoryId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            try
            {
                using (var ctx = new DBContext())
                {
                    var query = ctx.TblOtherCharge
                        .Include(oc => oc.Service)
                        .Where(oc => oc.SystemID == TARGET_SYSTEM);

                    if (!string.IsNullOrWhiteSpace(status))
                        query = query.Where(oc => oc.Status == status);

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        search = search.Trim();
                        query = query.Where(oc => oc.OtherChargeItem != null &&
                                                  oc.OtherChargeItem.Contains(search));
                    }

                    int? targetCategoryId = categoryId ?? serviceId;
                    if (targetCategoryId.HasValue)
                        query = query.Where(oc => oc.ServiceID == targetCategoryId.Value);

                    var totalItems = await query.CountAsync();

                    var items = await query
                        .OrderByDescending(oc => oc.OtherChargeID)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    // Load images separately using ReferenceID / ReferenceType
                    var chargeIds = items.Select(oc => oc.OtherChargeID).ToList();
                    var imagesLookup = await LoadImagesLookupAsync(ctx, chargeIds);

                    var result = items.Select(oc => new OtherChargeResponseDTO
                    {
                        ProductID = oc.OtherChargeID,
                        ProductName = oc.OtherChargeItem,
                        Description = oc.Description,
                        // Fee             = oc.Fee,
                        // CostPrice       = oc.CostPrice,
                        UM = oc.UM,
                        // Status          = oc.Status,
                        CategoryID = oc.ServiceID,
                        Category = oc.Service?.ServiceName ?? string.Empty,
                        Images = imagesLookup[oc.OtherChargeID].ToList()
                    }).ToList();

                    int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                    return Ok(new
                    {
                        success = true,
                        data = result,
                        pagination = new
                        {
                            currentPage = page,
                            pageSize = pageSize,
                            totalItems = totalItems,
                            totalPages = totalPages,
                            hasNext = page < totalPages,
                            hasPrevious = page > 1
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new ApiResult<object>
                {
                    Success = false,
                    Message = ex.ToString()
                });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET api/v1/products/categories?status=Enable
        //   Returns Categories with their nested Products.
        // ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Returns Services (categories) of System 27 with their nested Other Charges.
        /// </summary>
        /// <param name="status">Optional Status filter for products (e.g., "Enable", "Disable")</param>
        /// <returns>ApiResult with list of ServiceCategories containing nested OtherCharges</returns>
        [HttpGet]
        [Route("categories")]
        public async Task<IHttpActionResult> GetGrouped(string status = null)
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    var query = ctx.TblOtherCharge
                        .Where(oc => oc.SystemID == TARGET_SYSTEM);

                    if (!string.IsNullOrWhiteSpace(status))
                        query = query.Where(oc => oc.Status == status);

                    var categories = await query
                        .Where(oc => oc.Service != null)
                        .Select(oc => oc.Service)
                        .Distinct()
                        .OrderBy(s => s.ServiceName)
                        .Select(s => new
                        {
                            categoryID = s.ServiceID,
                            categoryName = s.ServiceName
                        })
                        .ToListAsync();

                    return Ok(new
                    {
                        success = true,
                        data = categories
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new ApiResult<object>
                {
                    Success = false,
                    Message = ex.ToString()
                });
            }
        }

        // ─────────────────────────────────────────────────────────────
        // GET api/v1/products/{id}
        //   Returns a single Product by ID if it belongs to System 27.
        // ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Returns a single Other Charge by ID if it belongs to System 27.
        /// </summary>
        /// <param name="id">OtherChargeID to retrieve</param>
        /// <returns>
        /// ApiResult with OtherChargeResponseDTO if found,
        /// or failure message if not found or doesn't belong to System 27
        /// </returns>
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    var oc = await ctx.TblOtherCharge
                        .Include(x => x.Service)
                        .FirstOrDefaultAsync(x => x.OtherChargeID == id && x.SystemID == TARGET_SYSTEM);

                    if (oc == null)
                    {
                        return Ok(new ApiResult<object>
                        {
                            Success = false,
                            Message = $"OtherCharge with ID {id} was not found on System 27."
                        });
                    }

                    // Load images for this single charge
                    var imagesLookup = await LoadImagesLookupAsync(ctx, new[] { id });

                    var dto = new OtherChargeResponseDTO
                    {
                        ProductID = oc.OtherChargeID,
                        ProductName = oc.OtherChargeItem,
                        Description = oc.Description,
                        // Fee             = oc.Fee,
                        // CostPrice       = oc.CostPrice,
                        UM = oc.UM,
                        // Status          = oc.Status,
                        CategoryID = oc.ServiceID,
                        Category = oc.Service?.ServiceName ?? string.Empty,
                        Images = imagesLookup[id].ToList()
                    };

                    return Ok(new ApiResult<OtherChargeResponseDTO>
                    {
                        Success = true,
                        Data = dto
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new ApiResult<object>
                {
                    Success = false,
                    Message = ex.ToString()
                });
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // API Result wrapper
    // ─────────────────────────────────────────────────────────────────
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    // DTOs are in: DTOs/OtherCharge/OtherChargeDTOs.cs  (namespace NSMSOLUTION.DTOs.OtherCharge)
}