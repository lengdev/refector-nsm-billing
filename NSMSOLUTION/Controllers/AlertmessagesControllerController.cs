using MVC;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers
{
    public class AlertMessagesController : Controller
    {
        private readonly DBContext db = new DBContext();

        private static readonly string[] AllowedAlertTypes =
            { "customer", "ticket", "system", "finance" };

        // ── GET AlertMessages/GetAlerts ──────────────────────────────────────────
        [HttpGet]
        public JsonResult GetAlerts(int page = 1, int pageSize = 18, string type = null, string search = null)
        {
            try
            {
                var query = from alert in db.TblAlertMessages
                            join customer in db.TblCustomer
                                on alert.CustomerID equals customer.CustomerID
                            where !alert.IsDismissed
                            select new
                            {
                                Alert = alert,
                                Customer = customer,
                            };

                // Filter by type
                if (!string.IsNullOrWhiteSpace(type))
                {
                    query = query.Where(x => x.Alert.AlertType == type);
                }

                // Search
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(x =>
                        x.Customer.CustomerName.Contains(search) ||
                        x.Customer.CustomerCode.Contains(search) ||
                        (x.Alert.Comment != null &&
                         x.Alert.Comment.Contains(search)));
                }

                // Total count
                int totalCount = query.Count();

                // Metrics (single SQL query)
                var metrics = db.TblAlertMessages
                    .Where(x => !x.IsDismissed)
                    .GroupBy(x => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Unread = g.Count(x => !x.IsRead),
                        Urgent = g.Count(x => x.IsUrgent),
                        Customer = g.Count(x => x.AlertType == "customer"),
                        Ticket = g.Count(x => x.AlertType == "ticket"),
                        Finance = g.Count(x => x.AlertType == "finance")
                    })
                    .FirstOrDefault();

                // Paging + sorting + projection in SQL
                var items = query
                    .OrderBy(x => x.Alert.IsRead)
                    .ThenByDescending(x => x.Alert.IsUrgent)
                    .ThenByDescending(x => x.Alert.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsEnumerable()
                    .Select(x => new
                    {
                        x.Alert.AlertID,
                        x.Alert.AlertType,
                        x.Customer.CustomerID,
                        CustomerName = string.IsNullOrEmpty(x.Customer.CustomerName)
                            ? x.Customer.CustomerNameLatin
                            : x.Customer.CustomerName,
                        x.Customer.CustomerCode,
                        x.Customer.CustomerMobile,
                        x.Alert.Comment,
                        x.Alert.SentBy,
                        x.Alert.IsRead,
                        x.Alert.IsDismissed,
                        x.Alert.IsUrgent,
                        x.Alert.ReferenceID,
                        x.Alert.PostComment,
                        x.Alert.PostCommentBy,
                        x.Alert.Reason,
                        CreatedAt = x.Alert.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
                    })
                    .ToList();

                return Json(new
                {
                    Items = items,
                    Metrics = metrics,
                    HasMore = (page * pageSize) < totalCount,
                    Total = totalCount
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // ── GET AlertMessages/GetCustomers ──────────────────────────────────────
        // Lightweight list for the "Add New" modal dropdown.
        [HttpGet]
        public JsonResult GetCustomers()
        {
            try
            {
                var customers = db.TblCustomer
                    .OrderBy(c => c.CustomerCode)
                    .Select(c => new
                    {
                        c.CustomerID,
                        c.CustomerCode,
                        c.CustomerName,
                        c.CustomerNameLatin,
                        c.CustomerMobile
                    })
                    .AsEnumerable()
                    .Select(c => new
                    {
                        c.CustomerID,
                        c.CustomerCode,
                        c.CustomerMobile,
                        CustomerName = string.IsNullOrEmpty(c.CustomerName)
                            ? c.CustomerNameLatin
                            : c.CustomerName
                    })
                    .ToList();

                return Json(customers, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message },
                    JsonRequestBehavior.AllowGet);
            }
        }

        // ── POST AlertMessages/CreateAlert ──────────────────────────────────────
        [HttpPost]
        public JsonResult CreateAlert(
            int customerId,
            string comment,
            string reason,
            string postComment,
            string postCommentBy,
            string alertType,
            bool isUrgent)
        {
            try { 

                HTML html = new HTML();

                if (string.IsNullOrWhiteSpace(comment))
                {
                    return Json(new { success = false, error = "Alert topic is required." });
                }

                alertType = (alertType ?? "customer").Trim().ToLowerInvariant();
                if (!AllowedAlertTypes.Contains(alertType))
                {
                    return Json(new { success = false, error = "Invalid alert type." });
                }

                bool customerExists = db.TblCustomer.Any(c => c.CustomerID == customerId);
                if (!customerExists)
                {
                    return Json(new { success = false, error = "Customer not found." });
                }

                var hasComment = !string.IsNullOrWhiteSpace(postComment);

                // NOTE: rename TblAlertMessage if your entity class is named differently.
                var alert = new AlertMessage
                {
                    CustomerID = customerId,
                    AlertType = alertType,
                    Comment = comment.Trim(),
                    Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
                    PostComment = hasComment ? postComment.Trim() : null,
                    PostCommentBy = hasComment ? postCommentBy : null,
                    SentBy = string.IsNullOrWhiteSpace(postCommentBy) ? "System" : postCommentBy,
                    IsUrgent = isUrgent,
                    IsRead = false,
                    IsDismissed = false,
                    CreatedAt = DateTime.Now,
                    SystemID = int.Parse(html.SystemID),
                    WarehouseID = int.Parse(html.WarehouseID),
                    DomainID = html.DomainID
                };

                db.TblAlertMessages.Add(alert);
                db.SaveChanges();

                return Json(new { success = true, alertId = alert.AlertID });
            }
            catch (Exception ex)
            {
                // EF hides the real reason (NULL column, FK, string too long...) in the inner exception
                return Json(new
                {
                    success = false,
                    error = GetInnerMessage(ex)
                });
            }
        }

        // ── POST AlertMessages/MarkRead ─────────────────────────────────────────
        [HttpPost]
        public JsonResult MarkRead(int? id)
        {
            try
            {
                if (id == null)
                {
                    return Json(new
                    {
                        success = false,
                        error = "ID is required."
                    });
                }

                var alert = db.TblAlertMessages
                              .FirstOrDefault(x => x.AlertID == id);

                if (alert == null)
                {
                    return Json(new
                    {
                        success = false,
                        error = "Alert not found."
                    });
                }

                alert.IsRead = true;

                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        // ── POST AlertMessages/MarkAllRead ──────────────────────────────────────
        [HttpPost]
        public JsonResult MarkAllRead()
        {
            try
            {
                var alerts = db.TblAlertMessages
                               .Where(x => !x.IsRead)
                               .ToList();

                foreach (var alert in alerts)
                {
                    alert.IsRead = true;
                }

                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        // ── POST AlertMessages/Dismiss ──────────────────────────────────────────
        [HttpPost]
        public JsonResult Dismiss(int id)
        {
            try
            {
                var alert = db.TblAlertMessages
                              .FirstOrDefault(x => x.AlertID == id);

                if (alert == null)
                {
                    return Json(new
                    {
                        success = false,
                        error = "Alert not found."
                    });
                }

                alert.IsDismissed = true;

                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        // ── POST AlertMessages/SaveComment ──────────────────────────────────────────
        [HttpPost]
        public async Task<JsonResult> SaveComment(int id, string comment, string commentBy)
        {
            try
            {
                var alert = db.TblAlertMessages
                              .FirstOrDefault(x => x.AlertID == id);

                if (alert == null)
                {
                    return Json(new
                    {
                        success = false,
                        error = "Alert not found."
                    });
                }

                alert.PostComment = comment;
                alert.PostCommentBy = commentBy;

                db.SaveChanges();

                // Manually created alerts have no Telegram group, so skip the bot call.
                long telegramGroupId;
                if (!string.IsNullOrWhiteSpace(alert.TelegramGroupID) &&
                    long.TryParse(alert.TelegramGroupID, out telegramGroupId))
                {
                    string caption = $"{comment} ({commentBy})";

                    var notification = new NotificationTelegram();
                    await notification.NotificationTelegramBot("",
                        telegramGroupId, Caption: caption, replyMarkupInlineKeyboardMarkup: true, MessageID: alert.MessageID ?? 0, edit: false);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        private static string GetInnerMessage(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex.Message;
        }

        // ── Dispose DBContext ───────────────────────────────────────────────────
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}