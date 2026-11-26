using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NSMSOLUTION.Controllers.Support
{
    [Authorize]
    public class TicketsController : Controller
    {
        // GET: Tickets
        public ActionResult Index(int ID = 0)
        {
            ViewBag.OpentTicketID = ID.ToString();
            return View();
        }
        public JsonResult Dashboard()
        {
            HTML html = new HTML();
            string sql = @"select sp.ProjectName, ot.[Status],
            count(distinct ot.OpenTicketID) OpenTicket,
            case when ot.AssigneeName = '<< ME >>' then (select ull.FullName from dbo.TblUserLogin ull where ull.UserLoginID = ot.AssigneeID) else ot.AssigneeName end AssigneeName
            from dbo.TblSupportOpenTicket ot
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            inner join dbo.TblSupportUserProject up on up.SupportProjectID = ot.SupportProjectID
            inner join dbo.TblSupportGroupProject gp on gp.SupportProjectID = ot.SupportProjectID
            inner join dbo.TblSupportUserGroup ug on ug.GroupID = gp.GroupID
            where (up.UserLoginID = @UserID OR ug.UserLoginID = @UserID) and ot.SystemID = @SystemID and ot.WarehouseID = @WarehouseID            
            group by sp.ProjectName, ot.[Status], ot.[StatusID], ot.AssigneeName, ot.AssigneeID
            order by sp.ProjectName, ot.[StatusID];
            
            select distinct sp.SupportProjectID, sp.ProjectName, sp.Memo, sp.CreateDate, sp.CreateBy,
            STUFF((SELECT ', ' + '<a onclick=""ShowProjectListsStatus('''+sp.ProjectName+''', '+convert(varchar, sp.SupportProjectID)+','+convert(varchar, ul.UserLoginID)+')"" class=""text-white"" href=""javascript:void(0);"">'+convert(varchar, ul.FullName)+'</a>'
            FROM dbo.TblUserLogin ul
            inner join dbo.TblSupportUserProject up on up.UserLoginID = ul.UserLoginID
            and up.SupportProjectID = sp.SupportProjectID
            where ul.SystemID = @SystemID and ul.WarehouseID = @WarehouseID
            ORDER BY ul.UserLoginID FOR XML PATH('')), 1, 1, '') Member,
            isnull(STUFF((SELECT ', ' + convert(varchar, g.GroupName) FROM dbo.TblSupportGroup g
            inner join dbo.TblSupportGroupProject gp on gp.GroupID = g.GroupID and gp.SupportProjectID = sp.SupportProjectID
            group by g.GroupName
            ORDER BY g.GroupName FOR XML PATH('')), 1, 1, ''), '') GroupName
            from dbo.TblSupportProject sp
            inner join dbo.TblSupportUserProject up on up.SupportProjectID = sp.SupportProjectID
            left join dbo.TblSupportGroupProject gp on gp.SupportProjectID = sp.SupportProjectID
            left join dbo.TblSupportUserGroup ug on ug.GroupID = gp.GroupID
            where (up.UserLoginID = @UserID OR ug.UserLoginID = @UserID) and sp.SystemID = @SystemID and sp.WarehouseID = @WarehouseID
            order by sp.ProjectName;
            
            select distinct sg.GroupID, gp.GroupProjectID, sg.GroupName, sg.CreateBy, sg.CreateDate, sp.ProjectName, sg.Memo,
            case when 
            isnull(STUFF((SELECT ', ' + convert(varchar, ul.FullName)  FROM dbo.TblUserLogin ul
            inner join dbo.TblSupportUserGroup ug on ug.UserLoginID = ul.UserLoginID and ug.GroupID = sg.GroupID
            ORDER BY ul.FullName FOR XML PATH('')), 1, 1, ''), '') <> '' then
            isnull(STUFF((SELECT ', ' + convert(varchar, ul.FullName)  FROM dbo.TblUserLogin ul
            inner join dbo.TblSupportUserGroup ug on ug.UserLoginID = ul.UserLoginID and ug.GroupID = sg.GroupID
            where ul.SystemID = @SystemID and WarehouseID = @WarehouseID
            ORDER BY ul.FullName FOR XML PATH('')), 1, 1, ''), '')
            else 'No Member.' end Member
            from dbo.TblSupportGroup sg
            inner join dbo.TblSupportGroupProject gp on gp.GroupID = sg.GroupID
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = gp.SupportProjectID
            inner join dbo.TblSupportUserGroup ug on ug.GroupID = sg.GroupID
            where ug.UserLoginID = @UserID and sg.SystemID = @SystemID and sg.WarehouseID = @WarehouseID
            order by sg.GroupName;

            select count(distinct ot.OpenTicketID) Ticket, ot.[Status], ot.SupportProjectID, ot.StatusID
            from dbo.TblSupportOpenTicket ot 
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            inner join dbo.TblSupportUserProject up on up.SupportProjectID = ot.SupportProjectID
            left join dbo.TblSupportGroupProject gp on gp.SupportProjectID = ot.SupportProjectID
            left join dbo.TblSupportUserGroup ug on ug.GroupID = gp.GroupID
            where (up.UserLoginID = @UserID or ug.UserLoginID = @UserID) and ot.SystemID = @SystemID and ot.WarehouseID = @WarehouseID
            group by ot.[Status], ot.SupportProjectID, ot.StatusID
            order by ot.StatusID, ot.SupportProjectID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "UserID", html.UserID },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            return Json(new { Data = html.JsonData(sql, false, para, true) }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ShowGroup()
        {
            HTML html = new HTML();
            string sql = @"select sg.GroupID, gp.GroupProjectID, sg.GroupName, sg.CreateBy, sg.CreateDate, sg.Memo,
            case when 
            isnull(STUFF((SELECT ', ' + convert(varchar, ul.FullName)  FROM dbo.TblUserLogin ul
            inner join dbo.TblSupportUserGroup ug on ug.UserLoginID = ul.UserLoginID and ug.GroupID = sg.GroupID
            ORDER BY ul.FullName FOR XML PATH('')), 1, 1, ''), '') <> '' then
            isnull(STUFF((SELECT ', ' + convert(varchar, ul.FullName)  FROM dbo.TblUserLogin ul
            inner join dbo.TblSupportUserGroup ug on ug.UserLoginID = ul.UserLoginID and ug.GroupID = sg.GroupID
            ORDER BY ul.FullName FOR XML PATH('')), 1, 1, ''), '')
            else 'No Member.' end
            Member
            from dbo.TblSupportGroup sg
            inner join dbo.TblSupportGroupProject gp on gp.GroupID = sg.GroupID
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = gp.SupportProjectID
            inner join dbo.TblSupportUserGroup ug on ug.GroupID = sg.GroupID
            where ug.UserLoginID = @UserID and sg.SystemID = @SystemID and sg.WarehouseID = @WarehouseID
            order by sg.GroupName";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "UserID", html.UserID },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID }
            };
            return Json(new { Data = html.JsonData(sql, false, para) }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowProjectListsStatus(int ProjectID = 0, int UserID = 0)
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() over(order by ot.StatusID) as [No], ot.AssigneeID, sp.ProjectName, ot.[Status] + ' ' + convert(varchar, count(distinct ot.OpenTicketID)) [Status], ot.StatusID, ot.SupportProjectID,
            case when ot.AssigneeName = '<< ME >>' then (select ull.FullName from dbo.TblUserLogin ull where ull.UserLoginID = ot.AssigneeID) else ot.AssigneeName end AssigneeName
            from dbo.TblSupportOpenTicket ot
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            inner join dbo.TblSupportUserProject up on up.SupportProjectID = ot.SupportProjectID
            where 1 = 1";
            if (UserID != 0)
                sql += "and (select value from string_split(ot.AssigneeID, ',') where value = " + UserID + ") = " + UserID;
            //sql += " and ot.AssigneeID=" + UserID;
            sql += " and ot.SystemID = " + html.SystemID + " and ot.WarehouseID = " + html.WarehouseID;
            if (ProjectID != 0)
                sql += " and ot.SupportProjectID=" + ProjectID;
            sql += @"group by ot.AssigneeID, sp.ProjectName, ot.[Status], ot.[StatusID], ot.AssigneeName, ot.AssigneeID, ot.SupportProjectID
            order by ot.[StatusID], sp.ProjectName";
            html.GH.SQL = sql;
            html.GridEventClick = "ShowTicket";
            html.TblID = "gridShowProjectListsStatus";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right", Filterable= false },
                new GridField() { Header = "AssigneeID", FieldName = "AssigneeID", IsHiddenField = true },
                new GridField() { Header = "Project Name", FieldName = "ProjectName", IsGroupByField = true, Width="auto", IsHiddenField= true },
                new GridField() { Header = "Assignee Name", FieldName = "AssigneeName", IsGroupByField = true, Width="auto", IsHiddenField= true },
                new GridField() { Header = "StatusID", FieldName = "StatusID", IsHiddenField = true },
                new GridField() { Header = "SupportProjectID", FieldName = "SupportProjectID", IsHiddenField = true },
                new GridField() { Header = "Status", FieldName = "Status", Width="auto" },
            };
            ViewBag.DrawKendoGrid = html.DrawKendoGrid();
            return View();
        }
        public JsonResult CountAssignedMe(int OpentTicketID = 0)
        {
            HTML html = new HTML();
            string sql = @"select count(distinct ot.OpenTicketID) Assignee
            from dbo.TblSupportOpenTicket ot
            inner join dbo.TblSupportUserProject up on up.SupportProjectID = ot.SupportProjectID
            where ((select value from string_split(ot.AssigneeID, ',')
            where value = @AssigneeID) = @AssigneeID OR
            (select value from string_split(ot.CoWorkerID, ',')
            where value = @AssigneeID) = @AssigneeID)
            and ot.StatusID in(1,2,3,4)
            and ot.SystemID = @SystemID and ot.WarehouseID = @WarehouseID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "AssigneeID", html.UserID }
            };
            string Data = html.JsonData(sql, false, para);
            return Json(new { Data = Data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowTicket(string Status, int UserID = 0, int StatusID = 0, int ProjectID = 0)
        {
            HTML html = new HTML();//' + ot.CustomerName + '<br />
            string sql = @"select ROW_NUMBER() over(order by ot.OpenTicketID desc) as [No], sp.ProjectName PName, ot.[Subject] + case when ot.CID <> '' then ' [CID: ' + ot.CID + ' - Con.ID: ' + ot.ConnectionID + ']' else '' end Subject, ot.ProblemType, ot.CustomerID, ot.CustomerName,
            format(ot.CreateDate, 'dd-MM-yy') + ' Project: ' + sp.ProjectName [Name],
            format(ot.CreateDate,'yyyy/MM/dd') CreateDate, ot.TicketNumber TicketNo, ot.TicketNumber, 
            ot.TicketNumber + '<br /><small>Date: ' + format(ot.CreateDate, 'dd-MM-yy') + '</small>' Ticket, ot.OpenTicketID, ot.[Priority], ot.[Status],
            ('<h6>' + sp.ProjectName + '</h6>' + ot.[Subject] + case when ot.CID <> '' then ' [CID: ' + ot.CID + ' - Con.ID: ' + ot.ConnectionID + ']' else '' end + '<br />' + ot.[ProblemType] + '<br /><small>Created By: ' + ot.CreateBy + ' - ' + dbo.GetTimeFromCurrentDate(ot.CreateDate) + '</small>') ProjectName, ot.[Description],
            STUFF((SELECT ', ' + convert(varchar, ul.FullName) FROM dbo.TblUserLogin ul
            where convert(varchar, ul.UserLoginID) in (select value from string_split(ot.AssigneeID,','))
            ORDER BY ul.UserLoginID FOR XML PATH('')), 1, 1, '') Assignee, ot.CoWorkerName, ot.CreateBy,
            STUFF((SELECT ', ' + ul.FullName FROM dbo.TblUserLogin ul
            inner join dbo.TblSupportUserProject up on up.SupportProjectID = sp.SupportProjectID 
            and ul.UserLoginID = up.UserLoginID
            ORDER BY ul.FullName FOR XML PATH('')), 1, 1, '') TeamMembers,
            case when ot.AssigneeName = '<< ME >>' then (select ull.FullName from dbo.TblUserLogin ull where ull.UserLoginID = ot.AssigneeID) else ot.AssigneeName end AssigneeName, ot.CID
            from dbo.TblSupportOpenTicket ot
            inner join dbo.TblSupportUserProject up on up.SupportProjectID = ot.SupportProjectID
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            left join dbo.TblSupportGroupProject gp on gp.SupportProjectID = ot.SupportProjectID
			left join dbo.TblSupportUserGroup ug on ug.GroupID = gp.GroupID";

            if (Status == "MyTicket")
            {
                if (UserID == 0)
                    sql += " where ((select value from string_split(ot.AssigneeID, ',') where value = " + html.UserID + ") = " + html.UserID + " OR (select value from string_split(ot.CoWorkerID, ',') where value = " + html.UserID + ") = " + html.UserID + ")";
                else
                {
                    sql += " where ot.SupportProjectID=" + ProjectID + " and ((select value from string_split(ot.AssigneeID, ',') where value = " + UserID + ") = " + UserID + " OR (select value from string_split(ot.CoWorkerID, ',') where value = " + UserID + ") = " + UserID + ")";
                    sql += " and ot.StatusID =" + StatusID;
                }
            }
            else
            {
                sql += " where (up.UserLoginID = " + html.UserID + " OR ug.UserLoginID = " + html.UserID + ")";
            }
            sql += " and ot.SystemID = " + html.SystemID + " and ot.WarehouseID = " + html.WarehouseID;
            sql += " group by ot.TicketNumber, ot.OpenTicketID, ot.[Priority], ot.[StatusID], ot.[Status], ot.[Subject], ot.[Description], ot.AssigneeID, ot.AssigneeName, dbo.GetTimeFromCurrentDate(ot.CreateDate),ot.CoWorkerName, sp.ProjectName, ot.CreateBy, sp.SupportProjectID, ot.ProblemType, ot.CustomerName, ot.CreateDate, ot.CustomerID, ot.CID, ot.ConnectionID;";
            html.GH.SQL = sql;
            html.OrderBy = "[No]";
            html.GridEventClick = "onGridTicketClick";
            //html.GridHeight = "800";
            html.GH.GridTable = UserID != 0 || Status == "AllTickets"
                ? (new GridField[]
                {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right"},
                new GridField() { Header = "OpenTicketID", FieldName = "OpenTicketID", IsHiddenField = true},
                new GridField() { Header = "Ticket#", FieldName = "TicketNo" },
                new GridField() { Header = "Date", FieldName = "CreateDate",},
                new GridField() { Header = "Status", FieldName = "Status"},
                new GridField() { Header = "Project", FieldName = "PName", },
                new GridField() { Header = "Ticket#", FieldName = "TicketNumber" },
                new GridField() { Header = "Assignee", FieldName = "AssigneeName" },
                new GridField() { Header = "CID", FieldName = "CID" },
                new GridField() { Header = "Customer", FieldName = "CustomerName" },
                new GridField() { Header = "Subject", FieldName = "Subject" },
                new GridField() { Header = "Problem Type", FieldName = "ProblemType" },
                new GridField() { Header = "Priority", FieldName = "Priority" },
                new GridField() { Header = "Co-Worker", FieldName = "CoWorkerName" },
                //new GridField() { Header = "Team Members", FieldName = "TeamMembers" },
                new GridField() { Header = "Description", FieldName = "Description", DecodeEntities = true, ClassName="text-truncate" },
                //new GridField() { Header = "Create Time", FieldName = "CreateTime" },
                })
                : (new GridField[]
                {
                    new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                    new GridField() { Header = "OpenTicketID", FieldName = "OpenTicketID", IsHiddenField = true },
                    new GridField() { Header = "Ticket#", FieldName = "Ticket" },
                    new GridField() { Header = "Project", FieldName = "ProjectName"},
                    new GridField() { Header = "CID", FieldName = "CID" },
                    new GridField() { Header = "Customer", FieldName = "CustomerName" },
                    new GridField() { Header = "Status", FieldName = "Status" },
                    new GridField() { Header = "Priority", FieldName = "Priority" },
                    new GridField() { Header = "Assignee", FieldName = "AssigneeName"},
                    new GridField() { Header = "Co-Worker", FieldName = "CoWorkerName" },
                    new GridField() { Header = "Description", FieldName = "Description", DecodeEntities = true, ClassName="text-truncate" },
                });
            ViewBag.Data = html.DrawKendoGrid();
            return View();
            //return Json(new { Data = ViewBag.Data }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowTicketDetails(int OpenTicketID)
        {
            HTML html = new HTML();
            string sql = @"select ot.SupportProjectID, ot.OpenTicketID, ot.TicketNumber, ot.ProjectName, ot.[Subject], ot.CreateBy, ot.CreateDate,
            ot.AssigneeName, ot.CoWorkerName, isnull(ot.CreateByFullName, '') CreateByFullName, isnull(ot.UserLoginID,'') UserLoginID, 
            STUFF((SELECT ', ' + convert(varchar, ul.FullName) FROM dbo.TblUserLogin ul
            where convert(varchar, ul.UserLoginID) in (select value from string_split(ot.AssigneeID,','))
            ORDER BY ul.UserLoginID FOR XML PATH('')), 1, 1, '') Assignee,
            dbo.GetTimeFromCurrentDate(ot.CreateDate) CreateTime,
            STUFF((SELECT ', ' + ul.FullName FROM dbo.TblUserLogin ul
            inner join dbo.TblSupportUserProject up on up.SupportProjectID = sp.SupportProjectID 
            and ul.UserLoginID = up.UserLoginID ORDER BY ul.FullName FOR XML PATH('')), 1, 1, '') TeamMembers,
            ot.[Status], ot.StatusID, ot.[Priority], ot.PriorityID, ot.PostAudience, case when isnull(ot.CID, '') <> '' then '<a class=""text-red"" href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, ot.CustomerID) + ''');"">'+ot.CID+'</a>' else '' end CID,
            ot.[SID], case when isnull(ot.CustomerID, '') <> '' then '<a class=""text-red"" href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, ot.CustomerID) + ''');"">'+ot.ConnectionID+'</a>'
			else ot.ConnectionID end ConnectionID, 
            case when isnull(ot.CustomerID, '') <> '' then '<a class=""text-red"" href=""javascript:void(0);"" onclick=""OpenNewTap(''#!/CustomerProfile?ID=' + convert(varchar, ot.CustomerID) + ''');"">'+ot.CustomerName+'</a>'
			else ot.CustomerName end CustomerName, ot.Phone,
            ot.ServiceName, ot.PackageName, ot.StartDate, ot.EndDate,
            ot.[Address], ot.Commune, ot.District, ot.ProvinceCity, ot.[Description],
            ot.LOTSwitch, ot.PONPort, ot.ONTID, ot.ONTDeviceModel, ot.SerialNumber, 
            case when isnull(ul.UserPicture, '') = '' then 'Content/images/avatar/user.png' else '" + html.ServerRoot + "Uploads/UserProfilePicture/'+ul.FullName+'" + @"/'+ ul.UserPicture end UserPicture,
            case when isnull(ot.CustomerID, '') <> '' then (select Salutation + FullName from dbo.TblLeads where LeadID in (select EmployeeID from dbo.TblCustomer where CustomerID = ot.CustomerID)) else '' end SaleRepresentative
            from dbo.TblSupportOpenTicket ot
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            left join dbo.TblUserLogin ul on ul.UserLoginID = ot.UserLoginID
            where ot.OpenTicketID = @OpenTicketID and ot.SystemID = @SystemID and ot.WarehouseID = @WarehouseID and ot.DomainID like '%'+@DomainID+'%'";
            sql += @"; select sc.SupportOpenTicketCommentID, sc.StatusID, sc.[Status], sc.PriorityID, sc.[Priority], sc.OpenTicketID, 
            sc.FromUserLoginID, sc.FromUserName, sc.ReplyFromUserLoginID, sc.ReplyFromUserName, 
            sc.Comment + '<br /><small>' + sc.DescriptionChangeStatus + '</small>' Comment, 
            sc.[Date], sc.CreateBy, sc.CreateDate, sc.SystemID, sc.WarehouseID,
            case when isnull(ul.UserPicture, '') = '' then 'Content/images/avatar/user.png' else '" + html.ServerRoot + "Uploads/UserProfilePicture/'+ul.FullName+'" + @"/'+ ul.UserPicture end UserPicture
            from dbo.TblSupportOpenTicketComment sc
            inner join dbo.TblUserLogin ul on ul.UserLoginID = sc.ReplyFromUserLoginID
            where sc.OpenTicketID = @OpenTicketID and sc.SystemID = @SystemID and sc.WarehouseID = @WarehouseID and sc.DomainID like '%'+@DomainID+'%'";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "OpenTicketID", OpenTicketID.ToString() },
                { "DomainID", html.DomainID }
            };
            ViewBag.Data = JsonConvert.SerializeObject(html.JsonData(sql, false, para, true));

            return View();
        }

        [HttpPost]
        public JsonResult SaveChangeOpenTicketComment(string[] Data)
        {
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(Data[0].ToString());
            Dictionary<string, string> para = new Dictionary<string, string>();
            foreach (dynamic item in DataInfo)
            {
                string key = item.Key;
                string value = HttpUtility.UrlDecode(html.EscapeXml(item.Value.ToString()));
                para.Add(key, value);
            }
            para.Add("Date", DateTime.Parse(html.GetCurrentDate()).ToString("yyyy/MM/dd HH:mm:ss tt"));
            para.Add("CreateBy", html.UserName);
            para.Add("CreateDate", DateTime.Parse(html.GetCurrentDate()).ToString("yyyy/MM/dd HH:mm:ss tt"));
            para.Add("SystemID", html.SystemID);
            para.Add("WarehouseID", html.WarehouseID);
            para.Add("UserFullName", html.UserFullName);
            para.Add("DomainID", html.DomainID);
            string EditCommentTextID = para["EditCommentTextID"];
            string sql;
            if (EditCommentTextID == "")
            {
                sql = @"insert into dbo.TblSupportOpenTicketComment (SupportProjectID, StatusID,
                Status, PriorityID, Priority, OpenTicketID, FromUserLoginID, FromUserName,
                ReplyFromUserLoginID, ReplyFromUserName, Comment, Date, CreateBy, CreateDate, SystemID, WarehouseID, DescriptionChangeStatus, DomainID)
                values (@SupportProjectID, @StatusID, @Status, @PriorityID, @Priority, @OpenTicketID, 
                @FromUserLoginID, @FromUserName, @ReplyFromUserLoginID, @ReplyFromUserName, @Comment,
                @Date, @CreateBy, @CreateDate, @SystemID, @WarehouseID, case when @UpdateStatus = 'True' then 'Change Status to '+ @Status + ' by ' + @UserFullName else '' end, @DomainID); SELECT @@IDENTITY;";
                sql += @"update dbo.TblSupportOpenTicket set StatusID = @StatusID, Status=@Status, PriorityID = @PriorityID, Priority = @Priority where OpenTicketID = @OpenTicketID";
            }
            else
            {
                sql = @"update dbo.TblSupportOpenTicketComment set SupportProjectID = @SupportProjectID, StatusID = @StatusID,
                Status = @Status, PriorityID = @PriorityID, Priority = @Priority,
                OpenTicketID = @OpenTicketID, FromUserLoginID = @FromUserLoginID,
                FromUserName = @FromUserName, ReplyFromUserLoginID = @ReplyFromUserLoginID,
                ReplyFromUserName = @ReplyFromUserName, Comment = @Comment,
                Date = @Date, CreateBy = @CreateBy, CreateDate = @CreateDate,
                SystemID = @SystemID, WarehouseID = @WarehouseID, DomainID = @DomainID,
                DescriptionChangeStatus = case when @UpdateStatus = 'True' then 'Change Status to '+ @Status + ' by ' + @UserFullName else '' end
                where SupportOpenTicketCommentID = @EditCommentTextID; select @EditCommentTextID;";
                sql += @"update dbo.TblSupportOpenTicket set StatusID = @StatusID, Status=@Status where OpenTicketID = @OpenTicketID";
            }
            sqlResult = html.SaveChangeData(sql, false, para, true);

            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = sqlResult.Value }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteComment(int TicketCommentID)
        {
            string sql = "delete from dbo.TblSupportOpenTicketComment where SupportOpenTicketCommentID=@TicketCommentID";
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "TicketCommentID", TicketCommentID.ToString() }
            };
            sqlResult = html.SaveChangeData(sql, false, para);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult DeleteTicket(int ID)
        {
            string sql = "delete from dbo.TblSupportOpenTicket where OpenTicketID=@ID;";
            sql += "delete from dbo.TblSupportOpenTicketComment where OpenTicketID=@ID;";
            HTML html = new HTML();
            SqlResult sqlResult = new SqlResult();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ID", ID.ToString() }
            };
            sqlResult = html.SaveChangeData(sql, false, para);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProjectStatus(int StatusID = 0, int ProjectID = 0, string Subject = "", string ProblemType = "")
        {
            HTML html = new HTML();
            string sql = @"select ROW_NUMBER() over(order by ot.StatusID, ot.OpenTicketID desc) as[No], ot.OpenTicketID, 
            ot.SupportProjectID, sp.ProjectName, ot.TicketNumber TicketNo, ot.TicketNumber, ot.[Subject], ot.ProblemType, ot.[Priority],
            ot.CustomerName, ot.[Description], format(ot.CreateDate,'yyyy/MM/dd') TicketDate,
            STUFF((SELECT ', ' + convert(varchar, ul.FullName) FROM dbo.TblUserLogin ul
            where convert(varchar, ul.UserLoginID) in (select value from string_split(ot.AssigneeID,','))
            ORDER BY ul.UserLoginID FOR XML PATH('')), 1, 1, '') Assignee, ot.CoWorkerName, ot.[Status]
            from dbo.TblSupportOpenTicket ot
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            where ot.StatusID= case when " + StatusID + "<> 0 then " + StatusID + " else ot.StatusID end and ot.SupportProjectID = " + ProjectID + " and ot.SystemID = " + html.SystemID + " and ot.WarehouseID = " + html.WarehouseID + " and ot.DomainID like '%" + html.DomainID + "%'";

            if (Subject != "")
                sql += " and ot.Subject=N'" + Subject + "'";
            if (ProblemType != "")
                sql += " and ot.ProblemType=N'" + ProblemType + "'";
            //html.OrderBy = "ot.StatusID, ot.OpenTicketID desc";
            sql += " order by ot.StatusID, ot.OpenTicketID desc";
            html.GH.SQL = sql;
            //html.IsHaveWhereCondition = true;
            //html.ShowGridOnly = true;
            html.GridEventClick = "onGridTicketClick";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right", Filterable = false },
                new GridField() { Header = "OpenTicketID", FieldName = "OpenTicketID", IsHiddenField = true },
                new GridField() { Header = "TicketNumber", FieldName = "TicketNo", IsHiddenField = true },
                new GridField() { Header = "Date", FieldName = "TicketDate", Width="220px", FilterableDate = true, FormatDateTime="dd/MM/yyyy" },//, Format="dd-MM-yy"
                new GridField() { Header = "Ticket#", FieldName = "TicketNumber" },
                //new GridField() { Header = "Project Name", FieldName = "ProjectName" },               
                //new GridField() { Header = "Ticket#", FieldName = "TicketNumber" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName" },
                new GridField() { Header = "Subject", FieldName = "Subject" },//, IsGroupByField = true, GroupLabel="Subject: ", Colspan=7
                new GridField() { Header = "Problem Type", FieldName = "ProblemType"},//, IsGroupByField = true, GroupLabel="Problem Type: ", Colspan=7
                new GridField() { Header = "Status", FieldName = "Status" },//, IsGroupByField = true, GroupLabel="Status: ", Colspan=7 
                new GridField() { Header = "Priority", FieldName = "Priority" },
                new GridField() { Header = "Assignee", FieldName = "Assignee" },
                new GridField() { Header = "Co-Worker", FieldName = "CoWorkerName" },
                new GridField() { Header = "Description", FieldName = "Description", DecodeEntities = true, ClassName="text-truncate" },
            };
            ViewBag.DrawHTMLSearchForm = html.DrawKendoGrid(); //html.DrawHTMLSearchForm(1, "ProjectStatus");
            return View();
        }

        public ActionResult showProjectDetail(int ProjectID, string ProjectName)
        {
            ViewBag.ProjectName = ProjectName;
            HTML html = new HTML();
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "ProjectID", ProjectID.ToString() },
                { "SystemID", html.SystemID },
                { "WarehouseID", html.WarehouseID },
                { "DomainID", html.DomainID }
            };
            //@"select ROW_NUMBER() over(order by ot.StatusID) as [No],
            //sp.ProjectName, ot.[Subject], ot.StatusID, ot.[Status],
            //(select count (ott.[Subject]) from dbo.TblSupportOpenTicket ott
            //where ott.SupportProjectID = ot.SupportProjectID
            //and ott.[Subject] = ot.[Subject] and ott.StatusID = ot.StatusID
            //) CountSubject, ot.ProblemType,
            //(select count (ott.ProblemType) from dbo.TblSupportOpenTicket ott
            //where ott.SupportProjectID = ot.SupportProjectID
            //and ott.ProblemType = ot.ProblemType and ott.StatusID = ot.StatusID
            //) CountProblemType
            //from dbo.TblSupportOpenTicket ot
            //inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            //where ot.SupportProjectID = " + ProjectID.ToString() + " and ot.SystemID = " + html.SystemID + " and ot.WarehouseID = " + html.WarehouseID + @"
            //group by ot.SupportProjectID, sp.ProjectName, ot.[Subject], ot.StatusID,
            //ot.[Status], ot.ProblemType
            //order by ot.StatusID;
            //select count(ot.StatusID) CountStatus, ot.[Status], ot.[Subject]
            //from dbo.TblSupportOpenTicket ot
            //where ot.SupportProjectID=1
            //group by ot.[Status], ot.[Subject], ot.StatusID
            //order by ot.[Subject], ot.StatusID";

            ////html.IsUseHavingCondition = true;
            ////html.OrderBy = "ot.[StatusID], ot.OpenTicketID desc";
            //html.ShowGridOnly = true;
            //html.ShowPaging = false;
            //html.ShowSearchButton = false;
            //html.GH.SQL = sql;
            //html.UsingJqueryDataTable = true;
            //html.IsShowJqueryDataTablePrint = false;
            //html.GH.GridTable = new GridField[]
            //{
            //    new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
            //    new GridField() { Header = "Project Name", FieldName = "ProjectName", IsGroupByField = true, GroupLabel="Project Name: ", Colspan = 3 },
            //    new GridField() { Header = "Subject", FieldName = "Subject", IsGroupByField = true, GroupLabel="Subject: ", Colspan = 3 },
            //    new GridField() { Header = "CountSubject", FieldName = "CountSubject", IsGroupByField = true, Colspan = 3 },
            //    new GridField() { Header = "Problem Type", FieldName = "ProblemType" },
            //    new GridField() { Header = "CountProblemType", FieldName = "CountProblemType", TextAlign ="right" },
            //};
            //ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm();
            string sql = @"select ot.SupportProjectID, sp.ProjectName, ot.[Subject], count(ot.[Subject]) CountSubject,
            (select count(distinct ott.CustomerName) from dbo.TblSupportOpenTicket ott where ott.SupportProjectID = ot.SupportProjectID
            and ott.[Subject] = ot.[Subject] and ott.CustomerName <> '') CountCustomer
            from dbo.TblSupportOpenTicket ot
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            where ot.SupportProjectID = @ProjectID and ot.SystemID = @SystemID and ot.WarehouseID = @WarehouseID and ot.DomainID like '%'+@DomainID+'%'
            group by ot.SupportProjectID, ot.[Subject], sp.ProjectName;

            select ot.SupportProjectID, sp.ProjectName, ot.ProblemType, count(ot.ProblemType) CountProblemType,
            (select count(distinct ott.CustomerName) from dbo.TblSupportOpenTicket ott where ott.SupportProjectID = ot.SupportProjectID
            and ott.[ProblemType] = ot.[ProblemType] and ott.CustomerName <> '') CountCustomer
            from dbo.TblSupportOpenTicket ot
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            where ot.SupportProjectID = @ProjectID and ot.SystemID = @SystemID and ot.WarehouseID = @WarehouseID and ot.DomainID like '%'+@DomainID+'%'
            group by ot.SupportProjectID, ot.ProblemType, sp.ProjectName;
            
            select count(ot.StatusID) CountStatus, ot.[Status], ot.[Subject], ot.StatusID
            from dbo.TblSupportOpenTicket ot
            where ot.SupportProjectID = @ProjectID and ot.SystemID = @SystemID and ot.WarehouseID = @WarehouseID and ot.DomainID like '%'+@DomainID+'%'
            group by ot.[Status], ot.[Subject], ot.StatusID
            order by ot.[Subject], ot.StatusID;
        
            select count(ot.StatusID) CountStatus, ot.[Status], ot.ProblemType, ot.StatusID
            from dbo.TblSupportOpenTicket ot
            where ot.SupportProjectID = @ProjectID and ot.SystemID = @SystemID and ot.WarehouseID = @WarehouseID and ot.DomainID like '%'+@DomainID+'%'
            group by ot.[Status], ot.ProblemType, ot.StatusID
            order by ot.ProblemType, ot.StatusID";
            ViewBag.Data = JsonConvert.SerializeObject(html.JsonData(sql, false, para, true));
            return View();
        }

        public ActionResult ShowByCustomer(string ProjectName = "", int ProjectID = 0, string Subject = "", string ProblemType = "", int CustomerID = 0)
        {
            ViewBag.ProjectName = ProjectName;
            ViewBag.Subject = Subject != "" ? "Subject: " + Subject : "Problem Type: " + ProblemType;
            HTML html = new HTML
            {
                ReportTitle = "Customer Ticket Lists"
            };
            string sql = @"select ROW_NUMBER() over(order by ot.StatusID, ot.CustomerName) as [No], ot.OpenTicketID, ot.TicketNumber TicketNo, ot.TicketNumber,
            format(ot.CreateDate, 'yyyy/MM/dd') TicketDate, ot.CustomerName, ot.ServiceName, ot.PackageName, format(ot.StartDate, 'dd/MM/yy') StartDate,
            format(ot.EndDate, 'dd/MM/yy') EndDate, ot.LOTSwitch, ot.PONPort, ot.ONTID, ot.ONTDeviceModel, ot.SerialNumber,
            ot.ProjectName, ot.[Subject], ot.ProblemType, 
            STUFF((SELECT ', ' + convert(varchar, ul.FullName) FROM dbo.TblUserLogin ul
            where convert(varchar, ul.UserLoginID) in (select value from string_split(ot.AssigneeID,','))
            ORDER BY ul.UserLoginID FOR XML PATH('')), 1, 1, '') Assignee, ot.CoWorkerName,
            ot.[Description], ot.[Status]
            from dbo.TblSupportOpenTicket ot
            where ot.CustomerName <> ''";
            if (ProjectID != 0)
                sql += " and ot.SupportProjectID = " + ProjectID;
            if (Subject != "")
                sql += " and ot.[Subject] =N'" + Subject + "'";
            if (ProblemType != "")
                sql += " and ot.ProblemType =N'" + ProblemType + "'";
            sql += " and ot.SystemID = " + html.SystemID + " and ot.WarehouseID = " + html.WarehouseID + " and ot.DomainID like '%" + html.DomainID + "%'";
            if (CustomerID != 0)
            {
                sql += " and ot.CustomerID = " + CustomerID;
                html.OrderBy = "ot.TicketNumber desc, format(ot.CreateDate, 'yyyy/MM/dd') desc";
            }
            else
                sql += " order by ot.TicketNumber desc, format(ot.CreateDate, 'yyyy/MM/dd') desc";
            html.GH.SQL = sql;
            html.GridEventClick = CustomerID == 0 ? "onGridTicketClick" : "onGridTicketClick(this)";
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right", Filterable = false },
                new GridField() { Header = "OpenTicketID", FieldName = "OpenTicketID", IsHiddenField = true },
                new GridField() { Header = "TicketNumber", FieldName = "TicketNo", IsHiddenField = true },
                new GridField() { Header = "Date", FieldName = "TicketDate", Width="220px", FilterableDate = true, IsHiddenField = CustomerID!=0, FormatDateTime="dd/MM/yyyy" },//, Format="dd-MM-yy"
                new GridField() { Header = "Date", FieldName = "TicketDate", Format="dd/MM/yy", IsHiddenField = CustomerID==0 },
                new GridField() { Header = "Ticket#", FieldName = "TicketNumber" },
                //new GridField() { Header = "Project Name", FieldName = "ProjectName" },
                new GridField() { Header = "Customer Name", FieldName = "CustomerName", IsHiddenField = CustomerID!=0 },
                new GridField() { Header = "Service Name", FieldName = "ServiceName" },
                new GridField() { Header = "Package Name", FieldName = "PackageName" },
                new GridField() { Header = "Start Date", FieldName = "StartDate" },
                new GridField() { Header = "End Date", FieldName = "EndDate" },
                new GridField() { Header = "LOT/Switch", FieldName = "LOTSwitch" },
                new GridField() { Header = "PON/Port", FieldName = "PONPort" },
                new GridField() { Header = "ONT ID", FieldName = "ONTID" },
                new GridField() { Header = "ONT Device Model", FieldName = "ONTDeviceModel" },
                new GridField() { Header = "Serial#", FieldName = "SerialNumber" },
                //new GridField() { Header = "Subject", FieldName = "Subject" },
                new GridField() { Header = "Problem Type", FieldName = "ProblemType"},
                new GridField() { Header = "Assignee", FieldName = "Assignee" },
                new GridField() { Header = "Co-Worker", FieldName = "CoWorkerName" },
                new GridField() { Header = "Status", FieldName = "Status" },
                new GridField() { Header = "Description", FieldName = "Description", DecodeEntities = true, ClassName="text-truncate", IsHiddenField = CustomerID!=0 },
            };
            if (CustomerID == 0)
                ViewBag.DrawHTMLSearchForm = html.DrawKendoGrid();
            else
            {
                html.ShowGridOnly = true;
                html.TblID = "TableCustTicket";
                ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "LoadCustTicket");
            }
            return View();
        }

        public JsonResult GetComment()
        {
            HTML html = new HTML();
            string sql = @"select top 30 ot.OpenTicketID, ot.TicketNumber, ot.CreateDate, isnull(tc.Comment, '') Comment, ot.SupportProjectID, sp.ProjectName PName,
            case when isnull(ul.UserPicture, '') = '' then 'Content/images/avatar/user.png' else '" + html.ServerRoot + "Uploads/UserProfilePicture/'+ul.FullName+'" + @"/'+ ul.UserPicture end UserPicture,
            ('<h6>' + sp.ProjectName + '<small> Assignee To: ' +
            STUFF((SELECT ', ' + convert(varchar, ul.FullName) FROM dbo.TblUserLogin ul
            where convert(varchar, ul.UserLoginID) in (select value from string_split(ot.AssigneeID,','))
            ORDER BY ul.UserLoginID FOR XML PATH('')), 1, 1, '')
            + '</small></h6>' + ot.[Subject] + '<br />' + ot.[ProblemType] + '<br /><small>Created By: ' + ot.CreateBy + ' - ' + dbo.GetTimeFromCurrentDate(ot.CreateDate) + '</small>') ProjectName,
            ot.[Description], ot.[Status], ot.StatusID, ot.[Priority], ot.PriorityID, tc.ReplyFromUserName,
            isnull(tc.Comment, '') Comment, tc.[Date], tc.SupportOpenTicketCommentID, tc.ReplyFromUserLoginID,
            case when isnull(tc.FromUserLoginID,'')= '' then ot.UserLoginID else tc.FromUserLoginID end FromUserLoginID,
            case when isnull(tc.FromUserName,'')='' then ot.CreateByFullName else tc.FromUserName end FromUserName,
            case when isnull(ull.UserPicture, '') = '' then 'Content/images/avatar/user.png' else '" + html.ServerRoot + "Uploads/UserProfilePicture/'+ull.FullName+'" + @"/'+ ull.UserPicture end FromUserPicture
            from dbo.TblSupportOpenTicket ot
            left join dbo.TblSupportOpenTicketComment tc on tc.OpenTicketID = ot.OpenTicketID
            left join dbo.TblUserLogin ull on ull.UserLoginID = tc.ReplyFromUserLoginID
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            inner join dbo.TblUserLogin ul on ul.UserLoginID = ot.UserLoginID
            inner join dbo.TblSupportUserProject up on up.SupportProjectID = ot.SupportProjectID
            left join dbo.TblSupportGroupProject gp on gp.SupportProjectID = ot.SupportProjectID
            left join dbo.TblSupportUserGroup ug on ug.GroupID = gp.GroupID
            where (up.UserLoginID = " + html.UserID + " or ug.UserLoginID = " + html.UserID + ") and ot.SystemID = " + html.SystemID + " and ot.WarehouseID = " + html.WarehouseID + " and ot.DomainID like '%" + html.DomainID + "%'" + @" 
            group by ot.OpenTicketID, ot.TicketNumber, ot.CreateDate, tc.Comment, tc.ReplyFromUserLoginID,
            ot.[Description], ot.[Status], ot.StatusID, ot.[Priority], ot.PriorityID,
            tc.Comment, tc.[Date], ul.UserPicture, ul.FullName, ot.AssigneeID, sp.ProjectName,
            ot.[Subject], ot.ProblemType, ot.CreateBy, tc.FromUserLoginID, ot.SupportProjectID,
            ull.UserLoginID, ot.UserLoginID, tc.FromUserName, ot.CreateByFullName,
            ull.UserPicture, ull.FullName, tc.ReplyFromUserName, tc.SupportOpenTicketCommentID
            order by ot.StatusID, ot.OpenTicketID desc, tc.SupportOpenTicketCommentID";
            string data = html.JsonData(sql);
            return Json(new { Data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RecentComments()
        {
            HTML html = new HTML();
            //string sql = @"select top 30 ROW_NUMBER() over(order by tc.SupportOpenTicketCommentID desc) as [No],
            //ot.OpenTicketID, tc.[Date] CommentDate,ot.TicketNumber, ot.CreateDate, isnull(tc.Comment, '') Comment, ot.SupportProjectID, sp.ProjectName PName,
            //case when isnull(ul.UserPicture, '') = '' then 'Content/images/avatar/user.png' else '" + html.ServerRoot + "Uploads/UserProfilePicture/'+ul.FullName+'" + @"/'+ ul.UserPicture end UserPicture,
            //('<h6>' + sp.ProjectName + '<small> Assignee To: ' +
            //STUFF((SELECT ', ' + convert(varchar, ul.FullName) FROM dbo.TblUserLogin ul
            //where convert(varchar, ul.UserLoginID) in (select value from string_split(ot.AssigneeID,','))
            //ORDER BY ul.UserLoginID FOR XML PATH('')), 1, 1, '')
            //+ '</small></h6>' + ot.[Subject] + '<br />' + ot.[ProblemType] + '<br /><small>Created By: ' + ot.CreateBy + ' - ' + dbo.GetTimeFromCurrentDate(ot.CreateDate) + '</small>') ProjectName,
            //ot.[Description], ot.[Status], ot.StatusID, ot.[Priority], ot.PriorityID, tc.ReplyFromUserName,
            //isnull(tc.Comment, '') Comment, tc.[Date], tc.SupportOpenTicketCommentID, tc.ReplyFromUserLoginID,
            //case when isnull(tc.FromUserLoginID,'')= '' then ot.UserLoginID else tc.FromUserLoginID end FromUserLoginID,
            //case when isnull(tc.FromUserName,'')='' then ot.CreateByFullName else tc.FromUserName end FromUserName,
            //case when isnull(ull.UserPicture, '') = '' then 'Content/images/avatar/user.png' else '" + html.ServerRoot + "Uploads/UserProfilePicture/'+ull.FullName+'" + @"/'+ ull.UserPicture end FromUserPicture
            //from dbo.TblSupportOpenTicket ot
            //inner join dbo.TblSupportOpenTicketComment tc on tc.OpenTicketID = ot.OpenTicketID
            //inner join dbo.TblUserLogin ull on ull.UserLoginID = tc.ReplyFromUserLoginID
            //inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            //inner join dbo.TblUserLogin ul on ul.UserLoginID = ot.UserLoginID
            //inner join dbo.TblSupportUserProject up on up.SupportProjectID = ot.SupportProjectID
            //left join dbo.TblSupportGroupProject gp on gp.SupportProjectID = ot.SupportProjectID
            //left join dbo.TblSupportUserGroup ug on ug.GroupID = gp.GroupID
            //where (up.UserLoginID = " + html.UserID + " or ug.UserLoginID = " + html.UserID + ") and ot.SystemID = " + html.SystemID + " and ot.WarehouseID = " + html.WarehouseID + @" 
            //group by ot.OpenTicketID, ot.TicketNumber, ot.CreateDate, tc.Comment, tc.ReplyFromUserLoginID,
            //ot.[Description], ot.[Status], ot.StatusID, ot.[Priority], ot.PriorityID,
            //tc.Comment, tc.[Date], ul.UserPicture, ul.FullName, ot.AssigneeID, sp.ProjectName,
            //ot.[Subject], ot.ProblemType, ot.CreateBy, tc.FromUserLoginID, ot.SupportProjectID,
            //ull.UserLoginID, ot.UserLoginID, tc.FromUserName, ot.CreateByFullName, tc.[Date],
            //ull.UserPicture, ull.FullName, tc.ReplyFromUserName, tc.SupportOpenTicketCommentID, tc.SupportProjectID
            //order by tc.SupportOpenTicketCommentID desc, format(tc.[Date],'yyMMdd hh:mm:ss'), tc.SupportProjectID, 
            //ot.StatusID, ot.OpenTicketID desc";
            //html.GH.SQL = sql;
            //html.GridEventClick = "onGridTicketClick";
            //html.GroupableKendoGrid = false;
            //html.ShowToolbarKendoGrid = false;
            //html.PageableKendoGrid = false;
            //html.FilterableKendoGrid = false;
            //html.KendoAvatarGrid = true;
            //html.GH.GridTable = new GridField[]
            //{
            //    //new GridField() { Header = "#", FieldName = "No", TextAlign = "right", Filterable = false },
            //    new GridField() { Header = "OpenTicketID", FieldName = "OpenTicketID", IsHiddenField = true },
            //    new GridField() { Header = "TicketNumber", FieldName = "TicketNo", IsHiddenField = true },
            //    //new GridField() { Header = "Date", FieldName = "TicketDate", Width="220px", FilterableDate = true, FormatDateTime="dd/MM/yyyy" },//, Format="dd-MM-yy"
            //    new GridField() { Header = "ProjectName", FieldName = "ProjectName", AvatarKendoGrid="UserPicture" },
            //    new GridField() { Header = "Description", FieldName = "Description" },
            //    new GridField() { Header = "Comment", FieldName = "Comment" },
            //};
            //ViewBag.DrawHTMLSearchForm = html.DrawKendoGrid();
            string sql = @"select top 30 ot.OpenTicketID, ot.TicketNumber, ot.CreateDate, isnull(tc.Comment, '') Comment, ot.SupportProjectID, sp.ProjectName PName,
            case when isnull(ul.UserPicture, '') = '' then 'Content/images/avatar/user.png' else '" + html.ServerRoot + "Uploads/UserProfilePicture/'+ul.FullName+'" + @"/'+ ul.UserPicture end UserPicture,
            ('<h6>' + sp.ProjectName + '<small> Assignee To: ' +
            STUFF((SELECT ', ' + convert(varchar, ul.FullName) FROM dbo.TblUserLogin ul
            where convert(varchar, ul.UserLoginID) in (select value from string_split(ot.AssigneeID,','))
            ORDER BY ul.UserLoginID FOR XML PATH('')), 1, 1, '')
            + '</small></h6>' + ot.[Subject] + '<br />' + ot.[ProblemType] + '<br /><small>Created By: ' + ot.CreateBy + ' - ' + dbo.GetTimeFromCurrentDate(ot.CreateDate) + '</small>') ProjectName,
            ot.[Description], ot.[Status], ot.StatusID, ot.[Priority], ot.PriorityID, tc.ReplyFromUserName,
            isnull(tc.Comment, '') Comment, tc.[Date], tc.SupportOpenTicketCommentID, tc.ReplyFromUserLoginID,
            case when isnull(tc.FromUserLoginID,'')= '' then ot.UserLoginID else tc.FromUserLoginID end FromUserLoginID,
            case when isnull(tc.FromUserName,'')='' then ot.CreateByFullName else tc.FromUserName end FromUserName,
            case when isnull(ull.UserPicture, '') = '' then 'Content/images/avatar/user.png' else '" + html.ServerRoot + "Uploads/UserProfilePicture/'+ull.FullName+'" + @"/'+ ull.UserPicture end FromUserPicture
            from dbo.TblSupportOpenTicket ot
            left join dbo.TblSupportOpenTicketComment tc on tc.OpenTicketID = ot.OpenTicketID
            left join dbo.TblUserLogin ull on ull.UserLoginID = tc.ReplyFromUserLoginID
            inner join dbo.TblSupportProject sp on sp.SupportProjectID = ot.SupportProjectID
            inner join dbo.TblUserLogin ul on ul.UserLoginID = ot.UserLoginID
            inner join dbo.TblSupportUserProject up on up.SupportProjectID = ot.SupportProjectID
            left join dbo.TblSupportGroupProject gp on gp.SupportProjectID = ot.SupportProjectID
            left join dbo.TblSupportUserGroup ug on ug.GroupID = gp.GroupID
            where (up.UserLoginID = " + html.UserID + " or ug.UserLoginID = " + html.UserID + ") and ot.SystemID = " + html.SystemID + " and ot.WarehouseID = " + html.WarehouseID + " and ot.DomainID like '%" + html.DomainID + "%' " + @" 
            group by ot.OpenTicketID, ot.TicketNumber, ot.CreateDate, tc.Comment, tc.ReplyFromUserLoginID,
            ot.[Description], ot.[Status], ot.StatusID, ot.[Priority], ot.PriorityID,
            tc.Comment, tc.[Date], ul.UserPicture, ul.FullName, ot.AssigneeID, sp.ProjectName,
            ot.[Subject], ot.ProblemType, ot.CreateBy, tc.FromUserLoginID, ot.SupportProjectID,
            ull.UserLoginID, ot.UserLoginID, tc.FromUserName, ot.CreateByFullName,
            ull.UserPicture, ull.FullName, tc.ReplyFromUserName, tc.SupportOpenTicketCommentID
            order by tc.SupportOpenTicketCommentID desc";
            string data = html.JsonData(sql);
            return Json(new { Data = data }, JsonRequestBehavior.AllowGet);
            //return View();
        }
    }
}