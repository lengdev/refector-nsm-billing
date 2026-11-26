using NSMSOLUTION.Models.EFCore;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace NSMSOLUTION.Controllers.API
{
    //[Authorize]
    [RoutePrefix("api/OtherInformation")]
    public class APIOtherInformationController : ApiController
    {
        // GET api/OtherInformation/GetList
        [HttpGet]
        [Route("GetList")]
        public async Task<IHttpActionResult> GetList(
            int categoryId = 0,
            int typeId = 0,
            string search = "",
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    var query = ctx.TblOtherInformation
                        .Include(x => x.OtherInformationCategory)
                        .Include(x => x.OtherInformationType)
                        .AsQueryable();

                    if (categoryId > 0)
                        query = query.Where(x => x.OtherInformationCategoryID == categoryId);

                    if (typeId > 0)
                        query = query.Where(x => x.TypeID == typeId);

                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        string s = search.ToLower();
                        query = query.Where(x => x.Title.ToLower().Contains(s)
                                              || (x.NewsContent != null && x.NewsContent.ToLower().Contains(s)));
                    }

                    int totalRecords = await query.CountAsync();

                    var items = await query
                        .OrderByDescending(x => x.OtherInformationID)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(x => new
                        {
                            x.OtherInformationID,
                            x.OtherInformationCategoryID,
                            CategoryName = x.OtherInformationCategory != null ? x.OtherInformationCategory.CategoryName : null,
                            x.TypeID,
                            TypeName = x.OtherInformationType != null ? x.OtherInformationType.TypeName : null,
                            x.Title,
                            x.NewsContent,
                            x.CreateBy,
                            x.CreateDate
                        })
                        .ToListAsync();

                    // Load images separately from TblOtherChargeImage using ReferenceType = 'InformationNews'
                    var ids = items.Select(x => x.OtherInformationID).ToList();
                    var images = await ctx.TblOtherChargeImage
                        .Where(img => img.ReferenceType == "InformationNews" && ids.Contains(img.ReferenceID))
                        .Select(img => new { img.ReferenceID, img.FileName })
                        .ToListAsync();

                    var data = items.Select(x => new
                    {
                        x.OtherInformationID,
                        x.OtherInformationCategoryID,
                        x.CategoryName,
                        x.TypeID,
                        x.TypeName,
                        x.Title,
                        x.NewsContent,
                        x.CreateBy,
                        x.CreateDate,
                        Images = images.Where(img => img.ReferenceID == x.OtherInformationID)
                                       .Select(img => img.FileName).ToList()
                    }).ToList();

                    return Ok(new { success = true, data, total = totalRecords, page, pageSize });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/OtherInformation/GetServices
        // Returns only Services (CategoryID = 7) with exposed Thumbnail and Video
        [HttpGet]
        [Route("GetServices")]
        public async Task<IHttpActionResult> GetServices()
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    var items = await ctx.TblOtherInformation
                        .Include(x => x.OtherInformationCategory)
                        .Where(x => x.OtherInformationCategory != null &&
                                   (x.OtherInformationCategory.CategoryName == "Services" ||
                                    x.OtherInformationCategory.CategoryName.ToLower().Contains("service")))
                        .OrderByDescending(x => x.OtherInformationID)
                        .ToListAsync();

                    var itemIds = items.Select(x => x.OtherInformationID).ToList();
                    var mediaFiles = await ctx.TblOtherChargeImage
                        .Where(img => itemIds.Contains(img.ReferenceID) && (img.ReferenceType == "Thumbnail" || img.ReferenceType == "Video" || img.ReferenceType == "InformationNews"))
                        .Select(img => new { img.ReferenceID, img.ReferenceType, img.FileName })
                        .ToListAsync();

                    var result = items.Select(item =>
                    {
                        var itemMedia = mediaFiles.Where(img => img.ReferenceID == item.OtherInformationID).ToList();

                        var videoFile = itemMedia
                            .FirstOrDefault(img => string.Equals(img.ReferenceType, "Video", StringComparison.OrdinalIgnoreCase))?.FileName;

                        var thumbnails = itemMedia
                            .Where(img => string.Equals(img.ReferenceType, "Thumbnail", StringComparison.OrdinalIgnoreCase) || string.Equals(img.ReferenceType, "InformationNews", StringComparison.OrdinalIgnoreCase))
                            .Select(img => img.FileName).ToList();

                        var primaryThumbnail = thumbnails.FirstOrDefault();

                        return new
                        {
                            item.OtherInformationID,
                            item.OtherInformationCategoryID,
                            CategoryName = item.OtherInformationCategory != null ? item.OtherInformationCategory.CategoryName : "Services",
                            item.Title,
                            item.NewsContent,
                            Thumbnail = primaryThumbnail,
                            Video = videoFile,
                            item.CreateBy,
                            item.CreateDate
                        };
                    }).ToList();

                    return Ok(new { success = true, total = result.Count, data = result });
                }
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += " | Inner: " + ex.InnerException.Message;
                }
                return InternalServerError(new Exception(errorMsg));
            }
        }

        // GET api/OtherInformation/GetById/{id}
        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<IHttpActionResult> GetById(int id)
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    var item = await ctx.TblOtherInformation
                        .Include(x => x.OtherInformationCategory)
                        .Include(x => x.OtherInformationType)
                        .FirstOrDefaultAsync(x => x.OtherInformationID == id);

                    if (item == null)
                        return NotFound();

                    var images = await ctx.TblOtherChargeImage
                        .Where(img => img.ReferenceType == "InformationNews" && img.ReferenceID == id)
                        .Select(img => img.FileName)
                        .ToListAsync();

                    var dto = new
                    {
                        item.OtherInformationID,
                        item.OtherInformationCategoryID,
                        CategoryName = item.OtherInformationCategory != null ? item.OtherInformationCategory.CategoryName : null,
                        item.TypeID,
                        TypeName = item.OtherInformationType != null ? item.OtherInformationType.TypeName : null,
                        item.Title,
                        item.NewsContent,
                        item.CreateBy,
                        item.CreateDate,
                        Images = images
                    };

                    return Ok(new { success = true, data = dto });
                }
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += " | Inner: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                        errorMsg += " | Inner2: " + ex.InnerException.InnerException.Message;
                }
                return InternalServerError(new Exception(errorMsg));
            }
        }

        // GET api/OtherInformation/GetByCategory/{categoryId}
        [HttpGet]
        [Route("GetByCategory/{categoryId}")]
        public async Task<IHttpActionResult> GetByCategory(int categoryId)
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    var items = await ctx.TblOtherInformation
                        .Include(x => x.OtherInformationCategory)
                        .Include(x => x.OtherInformationType)
                        .Where(x => x.OtherInformationCategoryID == categoryId)
                        .OrderByDescending(x => x.OtherInformationID)
                        .ToListAsync();

                    var itemIds = items.Select(x => x.OtherInformationID).ToList();
                    var images = await ctx.TblOtherChargeImage
                        .Where(img => img.ReferenceType == "InformationNews" && itemIds.Contains(img.ReferenceID))
                        .ToListAsync();

                    var result = items.Select(item => new
                    {
                        item.OtherInformationID,
                        item.OtherInformationCategoryID,
                        CategoryName = item.OtherInformationCategory != null ? item.OtherInformationCategory.CategoryName : null,
                        item.TypeID,
                        TypeName = item.OtherInformationType != null ? item.OtherInformationType.TypeName : null,
                        item.Title,
                        item.NewsContent,
                        item.CreateBy,
                        item.CreateDate,
                        Images = images.Where(img => img.ReferenceID == item.OtherInformationID)
                                       .Select(img => img.FileName).ToList()
                    }).ToList();

                    return Ok(new { success = true, data = result });
                }
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += " | Inner: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                        errorMsg += " | Inner2: " + ex.InnerException.InnerException.Message;
                }
                return InternalServerError(new Exception(errorMsg));
            }
        }

        // GET api/OtherInformation/GetCategories
        // Returns all categories
        [HttpGet]
        [Route("GetCategories")]
        public async Task<IHttpActionResult> GetCategories()
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    var categories = await ctx.TblOtherInformationCategory
                        .OrderBy(x => x.CategoryName)
                        .Select(x => new
                        {
                            otherInformationCategoryID = x.OtherInformationCategoryID,
                            categoryName = x.CategoryName
                        })
                        .ToListAsync();

                    return Ok(new { success = true, data = categories });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/OtherInformation/GetTypes
        // Returns all badge types (TblOtherInformationType) with linked category info
        [HttpGet]
        [Route("GetTypes")]
        public async Task<IHttpActionResult> GetTypes()
        {
            try
            {
                using (var ctx = new DBContext())
                {
                    var types = await ctx.TblOtherInformationType
                        .Include(x => x.OtherInformationCategory)
                        .OrderBy(x => x.TypeName)
                        .Select(x => new
                        {
                            typeID = x.OtherInformationTypeID,
                            typeName = x.TypeName,
                            categoryID = x.CategoryID,
                            categoryName = x.OtherInformationCategory != null ? x.OtherInformationCategory.CategoryName : null
                        })
                        .ToListAsync();

                    return Ok(new { success = true, data = types });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
