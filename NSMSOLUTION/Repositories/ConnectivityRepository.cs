using MVC;
using NSMSOLUTION.Models.EFCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Utilities;   // HTML, SqlResult (your namespace)

namespace NSMSOLUTION.Services
{
    public static class ConnectivityRepository
    {
        public static ActivateCustomer Load(int activateCustomerId)
        {
            const string sql = @"
                select ac.*
                from dbo.TblActivateCustomer ac
                where ac.ActivateCustomerID = @ActivateCustomerID";

            using (var ctx = new DBContext())
            {
                return ctx.Database
                    .SqlQuery<ActivateCustomer>(sql, new SqlParameter("@ActivateCustomerID", activateCustomerId))
                    .FirstOrDefault();
            }
        }
        public static SqlResult SaveStatus(string userFullName, int id, int oldStatus, int status,
            string message, string comment, string actionType, string source, string routerResult, DateTime now)
        {
            string who =
                status == ConnectivityStatusCodes.Active
                    ? ", ReactivateBy = @UserFullName, ReactivateDate = @Now "
                : (status == ConnectivityStatusCodes.Locked || status == ConnectivityStatusCodes.Terminated)
                    ? ", LockedBy = @UserFullName, LockedDate = @Now "
                : "";

            string statusName = status == ConnectivityStatusCodes.Active
                    ? "Activated"
                : status == ConnectivityStatusCodes.Locked
                    ? "Locked"
                : status == ConnectivityStatusCodes.Terminated
                    ? "Terminated" : "Inativated";

            string sql = @"
                UPDATE dbo.TblCustomerBillingInvoice
                   SET ConnectivityStatus = @Status, ConnectivityStatusName = @StatusName
                 WHERE ActivateCustomerID = @ActivateCustomerID;

                UPDATE dbo.TblActivateCustomer
                   SET [Date] = @Now, ConnectivityStatus = @Status, ConnectivityStatusName = @StatusName, Comment = @Comment " + who + @"
                 WHERE ActivateCustomerID = @ActivateCustomerID;

                UPDATE ci
                   SET ci.ConnectivityStatus = @Status, ci.ConnectivityStatusName = @StatusName
                  FROM dbo.TblCustomerInvoice ci
                 INNER JOIN dbo.TblActivateCustomer ac
                         ON ac.InvoiceNo = ci.InvoiceNo AND ac.ConnectionID = ci.ConnectionID
                 WHERE ac.ActivateCustomerID = @ActivateCustomerID;

                INSERT INTO dbo.TblHistoryActivateCustomer
                 (ConnectionType, CustomerID, ServiceID, PackageDetailID, ActivateDate, ActivateBy, Comment,
                  StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, ActivateCustomerID,
                  LockedDate, LockedBy, ConnectionID, [Date])
                SELECT ConnectionType, CustomerID, ServiceID, PackageDetailID, ActivateDate, @UserFullName, @Comment,
                       StartDate, EndDate, Duration, ConnectivityStatus, SystemID, WarehouseID, ActivateCustomerID,
                       LockedDate, LockedBy, ConnectionID, @Now
                  FROM dbo.TblActivateCustomer
                 WHERE ActivateCustomerID = @ActivateCustomerID;";

            using (var ctx = new DBContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                try
                {
                    ctx.Database.ExecuteSqlCommand(sql,
                        new SqlParameter("@Status", status),
                        new SqlParameter("@StatusName", (object)statusName ?? DBNull.Value),
                        new SqlParameter("@ActivateCustomerID", id),
                        new SqlParameter("@UserFullName", (object)userFullName ?? DBNull.Value),
                        new SqlParameter("@Comment", (object)comment ?? DBNull.Value),
                        new SqlParameter("@Now", now));

                    InsertMovement(ctx, id, actionType, oldStatus, status, comment, source, routerResult, userFullName, now);

                    tx.Commit();
                    return new SqlResult { Result = ResultType.Successful };
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    return new SqlResult { Result = ResultType.Failed, Message = ex.Message };
                }
            }
        }

        public static void AddTicketComment(HTML html, string ticketNumber, string commentHtml)
        {
            const string sql = @"
                INSERT INTO dbo.TblSupportOpenTicketComment
                 (SupportProjectID, StatusID, [Status], PriorityID, [Priority], OpenTicketID, FromUserLoginID, FromUserName,
                  ReplyFromUserLoginID, ReplyFromUserName, Comment, [Date], CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus)
                SELECT SupportProjectID, StatusID, [Status], PriorityID, [Priority], OpenTicketID, UserLoginID, CreateBy,
                       @UserID, @UserName, @Comment, @Now, @UserName, @Now, @SystemID, @WarehouseID, ''
                  FROM dbo.TblSupportOpenTicket
                 WHERE TicketNumber=@TicketNumber AND SystemID=@SystemID AND WarehouseID=@WarehouseID;";

            html.SaveChangeData(sql, false, new Dictionary<string, string>
            {
                { "TicketNumber", ticketNumber },
                { "SystemID", html.SystemID }, { "WarehouseID", html.WarehouseID },
                { "UserID", html.UserID.ToString() }, { "UserName", html.UserFullName },
                { "Comment", commentHtml }, { "Now", html.GetCurrentDate() }
            });
        }

        // column -> label used in the "Changes" text. Constants only, never user input.
        private static readonly (string Col, string Label)[] TrackedCols =
        {
            ("ConnectionType","Type"), ("RouterID","Router"), ("PPPoEUserName","PPPoE user"),
            ("PPPoEProfileName","Profile"), ("UserGroup","Group"), ("VLanName","VLAN"),
            ("VLanIPAddress","VLAN IP"), ("IPAddress","IP"), ("GetwayIP","Gateway"),
            ("SubnetIP","Subnet"), ("DNS1","DNS1"), ("DNS2","DNS2")
        };

        private static string BuildChangesSql()
        {
            var parts = TrackedCols.Select(t =>
                $"CASE WHEN ISNULL(CONVERT(nvarchar(200),p.{t.Col}),N'')<>ISNULL(CONVERT(nvarchar(200),ac.{t.Col}),N'') " +
                $"THEN N'{t.Label}: '+ISNULL(NULLIF(CONVERT(nvarchar(200),p.{t.Col}),N''),N'(empty)')+N' → '+" +
                $"ISNULL(NULLIF(CONVERT(nvarchar(200),ac.{t.Col}),N''),N'(empty)')+N'; ' ELSE N'' END");

            return "NULLIF(CONCAT(CASE WHEN p.MovementID IS NULL THEN N'' ELSE CONCAT(" +
                   string.Join(",", parts) + ") END, @Extra),N'')";
        }

        // Runs inside the caller's transaction so the movement and the status change commit together.
        public static void InsertMovement(DBContext ctx, int activateCustomerId, string actionType,
            int? oldStatus, int? newStatus, string comment, string source, string routerResult,
            string userName, DateTime now, string extraChanges = null)
        {
            string sql = @"
                INSERT INTO dbo.TblActivateCustomerMovement
                 (ActivateCustomerID, CustomerID, ConnectionID, [SID], InvoiceNo, ActionType, OldStatus, NewStatus,
                  Comment, Source, RouterResult, Changes,
                  ConnectionType, RouterID, RouterName, PPPoEUserName, PPPoEProfileName, UserGroup, ActualProfile,
                  VLanID, VLanName, VLanIPAddress, IPAddress, GetwayIP, SubnetIP, DNS1, DNS2, DeviceName, WifiName,
                  SystemID, WarehouseID, CreateBy, CreateDate)
                SELECT ac.ActivateCustomerID, ac.CustomerID, ac.ConnectionID, ac.[SID], ac.InvoiceNo, @ActionType, @OldStatus, @NewStatus,
                       @Comment, @Source, @RouterResult, " + BuildChangesSql() + @",
                       ac.ConnectionType, ac.RouterID, r.RouterName, ac.PPPoEUserName, ac.PPPoEProfileName, ac.UserGroup, ac.ActualProfile,
                       ac.VLanID, ac.VLanName, ac.VLanIPAddress, ac.IPAddress, ac.GetwayIP, ac.SubnetIP, ac.DNS1, ac.DNS2, ac.DeviceName, ac.WifiName,
                       ac.SystemID, ac.WarehouseID, @User, @Now
                  FROM dbo.TblActivateCustomer ac
                  LEFT JOIN dbo.TblRouter r ON r.RouterID = ac.RouterID
                  OUTER APPLY (SELECT TOP 1 * FROM dbo.TblActivateCustomerMovement m
                                WHERE m.ActivateCustomerID = ac.ActivateCustomerID
                                ORDER BY m.MovementID DESC) p
                 WHERE ac.ActivateCustomerID = @Id;";

            ctx.Database.ExecuteSqlCommand(sql,
                new SqlParameter("@Id", activateCustomerId),
                new SqlParameter("@ActionType", actionType),
                new SqlParameter("@OldStatus", (object)oldStatus ?? DBNull.Value),
                new SqlParameter("@NewStatus", (object)newStatus ?? DBNull.Value),
                new SqlParameter("@Comment", (object)Trunc(comment, 1000) ?? DBNull.Value),
                new SqlParameter("@Source", (object)source ?? DBNull.Value),
                new SqlParameter("@RouterResult", (object)Trunc(routerResult, 300) ?? DBNull.Value),
                new SqlParameter("@User", (object)userName ?? DBNull.Value),
                new SqlParameter("@Extra", (object)Trunc(extraChanges, 3000) ?? DBNull.Value),
                new SqlParameter("@Now", now));
        }

        private static string Trunc(string s, int n) => s == null ? null : (s.Length <= n ? s : s.Substring(0, n));

        // Standalone use (legacy controller actions that don't go through SaveStatus)
        public static void AddMovement(int id, string actionType, int? oldStatus, int? newStatus,
            string comment, string source, string routerResult, string userName)
        {
            using (var ctx = new DBContext())
                InsertMovement(ctx, id, actionType, oldStatus, newStatus, comment, source, routerResult, userName, AppTime.Now);
        }
    }
}