using AllReportAPi.Services;
using MVC;
using NSMSOLUTION.Models.Collections;
using NSMSOLUTION.Models.EFCore;
using NSMSOLUTION.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NSMSOLUTION.Controllers
{
    [Authorize]
    public class CollectionApiController : Controller
    {
        private readonly HTML html = new HTML();
        private DBContext db;
        private CollectionService svc;
        private CollectionNotifier notifier;

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            var scope = ResolveScope();
            if (scope == null) return;

            db = new DBContext();
            svc = new CollectionService(
                db,
                invoices => invoices.WhereScope(scope),
                scope.SystemId == int.Parse(html.SystemID));

            var telegram = new NotificationTelegram();
            notifier = new CollectionNotifier(async (paths, caption, chatId, bot) =>
            {
                var r = await telegram.SendPhotoToGroup(paths, caption, chatId, bot);
                return r.Result == ResultType.Successful ? null : r.Message;
            });
        }

        private ReportScope ResolveScope()
        {
            int systemId, warehouseId;
            if (int.TryParse(html.SystemID, out systemId) && int.TryParse(html.WarehouseID, out warehouseId))
                return new ReportScope { SystemId = systemId, WarehouseId = warehouseId };
            return null;
        }

        protected override void OnActionExecuting(ActionExecutingContext ctx)
        {
            if (svc == null)
                ctx.Result = Fail("The current system or warehouse could not be resolved. Please sign in again.");
            else
                base.OnActionExecuting(ctx);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null) db.Dispose();
            base.Dispose(disposing);
        }

        private JsonResult Ok(object data)
        {
            return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
        }

        private JsonResult Fail(string message, int status = 400)
        {
            Response.StatusCode = status;
            return Json(new { success = false, message }, JsonRequestBehavior.AllowGet);
        }

        protected override void OnException(ExceptionContext ctx)
        {
            ctx.ExceptionHandled = true;
            ctx.HttpContext.Response.Clear();
            ctx.HttpContext.Response.StatusCode = 500;
            ctx.HttpContext.Response.TrySkipIisCustomErrors = true;

            bool showDetail = ctx.HttpContext.IsDebuggingEnabled || ctx.HttpContext.Request.IsLocal;
            ctx.Result = Json(new
            {
                success = false,
                message = showDetail ? ctx.Exception.GetBaseException().Message : "Server error. Please contact support."
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet] public ActionResult Lookups() { return Ok(svc.GetLookups()); }
        [HttpGet]
        public ActionResult Customers(string search = null, int take = 30)
        {
            return Ok(svc.GetCustomers(search, take));
        }

        [HttpGet]
        public ActionResult OpenInvoices(int customerId, int? collectionId = null)
        {
            return Ok(svc.GetOpenInvoices(customerId, collectionId));
        }
        [HttpGet]
        public ActionResult GetCredits(int customerId, int? collectionId = null)
        {
            return Ok(svc.GetCredits(customerId, collectionId));
        }
        [HttpGet] public ActionResult List() { return Ok(svc.GetList()); }
        [HttpGet] public ActionResult Details(int id) { return Ok(svc.GetDetails(id)); }
        [HttpGet]
        public ActionResult NextNumber(int classId)
        {
            try
            {
                return Ok(svc.GetNextNumber(classId));
            }
            catch (CollectionException ex)
            {
                return Fail(ex.Message);
            }
        }

        // notifySlip is the "Send Telegram alert" switch in the UI. It is read straight from the JSON,
        // so this works even if CollectionSaveRequest has no property for it. Missing = on, like the UI default.
        private CollectionSaveRequest ReadRequest(out bool notifySlip)
        {
            string json;
            if (Request.ContentType != null && Request.ContentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
            {
                json = Request.Form["data"];
            }
            else
            {
                Request.InputStream.Position = 0;
                using (var reader = new StreamReader(Request.InputStream))
                    json = reader.ReadToEnd();
            }

            if (string.IsNullOrWhiteSpace(json))
                throw new CollectionException("Invalid request.");

            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

            notifySlip = true;
            var raw = serializer.DeserializeObject(json) as IDictionary<string, object>;
            object flag;
            if (raw != null && raw.TryGetValue("notifySlip", out flag) && flag is bool)
                notifySlip = (bool)flag;

            return serializer.Deserialize<CollectionSaveRequest>(json);
        }

        [HttpPost]
        public async Task<ActionResult> Save()
        {
            try
            {
                bool notifySlip;
                var request = ReadRequest(out notifySlip);
                var slips = CollectionNotifier.ReadSlips(Request.Files);

                var saved = await svc.SaveAsync(request, html.UserFullName);

                var notified = false;
                string notifyMessage = null;

                try
                {
                    // slips are always kept with the receipt, whether or not Telegram is used
                    var stored = CollectionNotifier.StoreSlips(saved.id, slips);
                    svc.SaveSlips(saved.id, stored);

                    // switch off in the UI: save only, send nothing
                    if (notifySlip)
                    {
                        var caption = svc.BuildNotice(saved.id, html.UserFullName);
                        var paths = stored.Select(x => x.FullPath).ToList();
                        notifyMessage = await notifier.SendAsync(caption, paths);
                        notified = notifyMessage == null;
                    }
                }
                catch (Exception ex)
                {
                    notifyMessage = ex.GetBaseException().Message;
                }

                return Ok(new { saved.id, saved.number, saved.unapplied, notified, notifyMessage });
            }
            catch (CollectionException ex) { return Fail(ex.Message); }
        }


        [HttpPost]
        public async Task<ActionResult> Update(int id)
        {
            try
            {
                bool notifySlip;
                var request = ReadRequest(out notifySlip);
                var slips = CollectionNotifier.ReadSlips(Request.Files);

                var saved = await svc.UpdateAsync(
                    id,
                    request,
                    html.UserFullName
                );

                var notified = false;
                string notifyMessage = null;

                try
                {
                    var stored = CollectionNotifier.StoreSlips(saved.id, slips);

                    svc.SaveSlips(saved.id, stored);

                    if (notifySlip)
                    {
                        var caption = svc.BuildNotice(
                            saved.id,
                            html.UserFullName
                        );

                        var paths = stored.Select(x => x.FullPath).ToList();

                        notifyMessage = await notifier.SendAsync(caption, paths);
                        notified = notifyMessage == null;
                    }
                }
                catch (Exception ex)
                {
                    notifyMessage = ex.GetBaseException().Message;
                }

                return Ok(new
                {
                    saved.id,
                    saved.number,
                    saved.unapplied,
                    notified,
                    notifyMessage
                });
            }
            catch (CollectionException ex)
            {
                return Fail(ex.Message);
            }
        }


        [HttpPost]
        public ActionResult Rollback(int id)
        {
            try { svc.Rollback(id, html.UserFullName); return Ok(true); }
            catch (CollectionException ex) { return Fail(ex.Message); }
        }
    }
}