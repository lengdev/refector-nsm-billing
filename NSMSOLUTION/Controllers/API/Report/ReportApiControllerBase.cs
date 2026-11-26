using AllReportAPi.Services;
using MVC;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Models.EFCore.DataFields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace AllReportAPi.Controllers
{
    public abstract class ReportApiControllerBase : ApiController
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 500;
        private readonly HTML html = new HTML();

        private ReportScope scope;
        private bool scopeResolved;

        protected static bool IsPaged(ReportFilter f) =>
            !f.AllRows && (f.Page.HasValue || f.PageSize.HasValue);

        protected static int PageOf(ReportFilter f) =>
            Math.Max(1, f.Page.GetValueOrDefault(1));

        protected static int PageSizeOf(ReportFilter f) =>
            Math.Max(1, Math.Min(MaxPageSize, f.PageSize.GetValueOrDefault(DefaultPageSize)));

        protected IHttpActionResult Fail(Exception ex)
        {
            var root = ex;
            while (root.InnerException != null) root = root.InnerException;
            return Content(HttpStatusCode.InternalServerError,
                new { Message = root.Message, Type = root.GetType().Name });
        }

        // current system + warehouse, resolved once per request
        protected ReportScope Scope
        {
            get
            {
                if (!scopeResolved)
                {
                    scopeResolved = true;
                    int systemId, warehouseId;
                    if (int.TryParse(html.SystemID, out systemId) &&
                        int.TryParse(html.WarehouseID, out warehouseId))
                        scope = new ReportScope { SystemId = systemId, WarehouseId = warehouseId };
                }
                return scope;
            }
        }

        /// <summary>
        /// Opens ReportServices for the current scope, runs a service method and returns
        /// the paged / sorted response. The DbContext stays open until the response is built.
        /// </summary>
        protected IHttpActionResult RunService<TDto>(
            ReportFilter f,
            Func<ReportServices, ReportFilter, IQueryable<TDto>> call,
            Action<ReportServices, IList<TDto>> enrich = null)
        {
            var current = Scope;
            if (current == null)
                return BadRequest("The current system or warehouse could not be resolved.");

            f = f ?? new ReportFilter();
            try
            {
                using (var services = new ReportServices(current))
                {
                    Action<IList<TDto>> afterLoad = null;
                    if (enrich != null)
                        afterLoad = rows => enrich(services, rows);

                    return RespondQuery(call(services, f), f, afterLoad);
                }
            }
            catch (Exception ex) { return Fail(ex); }
        }

        protected IHttpActionResult RespondQuery<TDto>(
            IQueryable<TDto> query, ReportFilter f, Action<IList<TDto>> enrich = null)
        {
            query = query.OrderByName(f.SortBy,
                string.Equals(f.SortDir, "desc", StringComparison.OrdinalIgnoreCase));

            var totalCount = query.Count();
            var totals = query.SumFields(f.TotalFields);

            if (IsPaged(f))
            {
                var page = PageOf(f);
                var pageSize = PageSizeOf(f);
                var pageRows = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                if (enrich != null) enrich(pageRows);
                return Ok(new { rows = pageRows, totalCount, page, pageSize, totals });
            }

            var rows = query.ToList();
            if (enrich != null) enrich(rows);
            if (f.AllRows) return Ok(new { rows, totalCount, totals });
            return Ok(rows);
        }
    }
}