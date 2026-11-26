using MVC;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class AddInformationNewsController : Controller
    {
        // GET: AddInformationNews
        [Authorize]
        public ActionResult Index()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Information News",

                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,

                AddButton = "Add Information News",
                AddURL = "AddInformationNews/CreateInformationNews",
                TableName = "TblOtherInformation",
                PrimaryField = "OtherInformationID",
                OrderBy = "OtherInformationID desc",
                DialogBoxHeight = 500,
                DialogBoxWidth = "1000",
                IsHaveWhereCondition = true
            };

            string sysId = string.IsNullOrWhiteSpace(html.SystemID) ? "0" : html.SystemID;
            string whId = string.IsNullOrWhiteSpace(html.WarehouseID) ? "0" : html.WarehouseID;
            string domId = html.DomainID ?? "";

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCategoryID",
                Label = "Category Name:",
                DBField = "info.CategoryID",
                SQL_CBO = "select CategoryID, CategoryName from dbo.TblOtherInformationCategory order by CategoryName",
                DisplayText = "CategoryName",
                Value = "CategoryID",
                ComboBox = true
            });

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Title",
                SQL = "select distinct top 10 Title from dbo.TblOtherInformation where lower(Title) like @SearchText order by Title",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("info.Title"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);

            string whereClause = "";
            if (sysId != "0" && whId != "0")
            {
                whereClause = " where info.SystemID = " + sysId + " and info.WarehouseID = " + whId + " and info.DomainID like '%" + domId + "%'";
            }

            string sql = @"select ROW_NUMBER() over (order by cat.CategoryName, info.OtherInformationID desc) [No],
info.OtherInformationID, cat.CategoryName, isnull(tp.TypeName,'') TypeName, info.Title, info.NewsContent, info.CreateBy, info.CreateDate,
isnull(STUFF((
    select ',' + 
    case 
        when FileName like '%/%' then right(FileName, charindex('/', reverse(FileName)) - 1) 
        else FileName 
    end 
    from dbo.TblOtherChargeImage 
    where ReferenceID = info.OtherInformationID AND ReferenceType IN ('InformationNews', 'Thumbnail', 'Video')
    order by OtherChargeImageID
    for xml path('')
), 1, 1, ''), '') AttachmentFileName
from dbo.TblOtherInformation info
left join dbo.TblOtherInformationCategory cat on cat.CategoryID = info.CategoryID
left join dbo.TblOtherInformationType tp on tp.TypeID = info.TypeID" + whereClause;

            html.OrderBy = " cat.CategoryName, info.OtherInformationID desc";
            html.GH.SQL = sql;
            html.ShowPaging = false;

            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "No", FieldName = "No", TextAlign = "right", DeletedCallBackFunction = "CleanupImagesJS(DataID, responetext);" },
                new GridField() { Header = "Category Name", FieldName = "CategoryName", IsGroupByField = true, Colspan = 10 },
                new GridField() { Header = "Badge Type", FieldName = "TypeName" },
                new GridField() { Header = "Title", FieldName = "Title", Width = "300px", ClassName = "truncate-cell" },
                new GridField() { Header = "News Content", FieldName = "NewsContent", Width = "300px", ClassName = "truncate-cell" },
                new GridField() { Header = "Attachment", FieldName = "AttachmentFileName", ImagePath = "Uploads/Services/", TextAlign = "center" },
                new GridField() { Header = "Created By", FieldName = "CreateBy" },
                new GridField() { Header = "Created Date", FieldName = "CreateDate", FormatDateTime = "dd-MM-yy" },
                new GridField() { Header = "Action", FieldName = "OtherInformationID", TextAlign = "Center", IsModifyField = true }
            };

            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        public ActionResult CreateInformationNews(string PrimaryFieldValue)
        {
            PrimaryFieldValue = PrimaryFieldValue ?? "";
            HTML html = new HTML
            {
                UseTable = true,
                TableColumn = 2,
                FormID = "CreateInformationNews",
                FieldIDFocus = "TxtTitle",
                TableName = "TblOtherInformation",
                PrimaryField = "OtherInformationID",
                PrimaryFieldValue = PrimaryFieldValue
            };

            // Ground-truth TypeID for edit mode, so the view never has to
            // guess it from the raw <select>'s default browser selection.
            string currentTypeId = "";
            if (!string.IsNullOrWhiteSpace(PrimaryFieldValue) &&
                int.TryParse(PrimaryFieldValue, out int pid) && pid > 0)
            {
                string typeSql = "SELECT TypeID FROM dbo.TblOtherInformation WHERE OtherInformationID = @ID";
                var typePara = new Dictionary<string, string> { { "ID", pid.ToString() } };
                var typeDs = html.LoadData(typeSql, false, typePara);
                if (typeDs != null && typeDs.Tables.Count > 0 && typeDs.Tables[0].Rows.Count > 0)
                {
                    var val = typeDs.Tables[0].Rows[0]["TypeID"];
                    if (val != null && val != DBNull.Value)
                        currentTypeId = val.ToString();
                }
            }
            ViewBag.CurrentTypeID = currentTypeId;

            string catSql = "select CategoryID, CategoryName from dbo.TblOtherInformationCategory";
            string cboTypeSql = "select t.TypeID, t.TypeName from dbo.TblOtherInformationType t";
            if (!string.IsNullOrWhiteSpace(html.SystemID) && !string.IsNullOrWhiteSpace(html.WarehouseID))
            {
                catSql += " where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " and DomainID like '%" + html.DomainID + "%'";
                cboTypeSql += " where t.SystemID = " + html.SystemID + " and t.WarehouseID=" + html.WarehouseID + " and t.DomainID like '%" + html.DomainID + "%'";
            }
            catSql += " order by CategoryName";
            cboTypeSql += " order by t.TypeName";

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Category Name:",
                ID = "TxtCategoryID",
                DBField = "CategoryID",
                SQL_CBO = catSql,
                Value = "CategoryID",
                DisplayText = "CategoryName",
                Required = true
            });

            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Badge Type:",
                ID = "TxtTypeID",
                DBField = "",  // intentionally blank - handled separately via UpdateTypeID to avoid FK conflict on empty value
                SQL_CBO = cboTypeSql,
                Value = "TypeID",
                DisplayText = "TypeName",
                Required = false
            });

            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Title:",
                ID = "TxtTitle",
                DBField = "Title",
                Required = true
            });

            html.HTMLControls.Add(new HtmlTextarea()
            {
                Label = "News Content:",
                ID = "TxtNewsContent",
                DBField = "NewsContent",
                // Style = "style=\"height:180px;\""
            });

            ViewBag.PrimaryFieldValue = PrimaryFieldValue;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }

        /// <summary>
        /// Updates TypeID for a saved OtherInformation record.
        /// Handles empty/zero typeId by writing NULL (avoids FK constraint on INT column).
        /// Called from the view's ajaxComplete after the main DoActionData/SaveChange succeeds.
        /// </summary>
        [HttpPost]
        public JsonResult UpdateTypeID(int id, string typeId)
        {
            try
            {
                int? parsedTypeId = null;
                if (!string.IsNullOrWhiteSpace(typeId) && typeId != "0" && int.TryParse(typeId, out int parsed))
                    parsedTypeId = parsed;

                string sql = "UPDATE dbo.TblOtherInformation SET TypeID = @TypeID WHERE OtherInformationID = @ID";

                // Execute directly with proper NULL handling
                using (var conn = new System.Data.SqlClient.SqlConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["AuthContextNSM"].ConnectionString))
                {
                    conn.Open();
                    using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@ID", id));
                        if (parsedTypeId.HasValue)
                            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@TypeID", parsedTypeId.Value));
                        else
                            cmd.Parameters.Add(new System.Data.SqlClient.SqlParameter("@TypeID", System.DBNull.Value));
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { Result = 0, Message = "TypeID updated" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 1, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult CleanupImages(int id)
        {
            try
            {
                string logFile = Server.MapPath("~/Uploads/cleanup_log.txt");
                System.IO.File.AppendAllText(logFile, "\n[" + DateTime.Now + "] CleanupImages called for News with ID: " + id);

                if (id <= 0) return Json(new { Result = 0, Error = "Invalid ID" });

                string uploadPath = Server.MapPath("~/Uploads/Services/");
                HTML html = new HTML();

                string selSql = "SELECT FileName FROM dbo.TblOtherChargeImage WHERE ReferenceID = @ReferenceID AND ReferenceType = 'InformationNews'";
                var selPara = new Dictionary<string, string> { { "ReferenceID", id.ToString() } };
                var ds = html.LoadData(selSql, false, selPara);

                System.IO.File.AppendAllText(logFile, " | Found news images: " + (ds?.Tables[0]?.Rows.Count ?? 0));

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        string fileName = row["FileName"].ToString();
                        if (fileName.Contains("/"))
                            fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);
                        string physPath = Path.Combine(uploadPath, fileName);
                        if (System.IO.File.Exists(physPath))
                        {
                            System.IO.File.Delete(physPath);
                            System.IO.File.AppendAllText(logFile, " | Deleted file: " + fileName);
                        }
                    }
                }

                var delPara = new Dictionary<string, string>
                {
                    { "ReferenceID", id.ToString() },
                    { "ReferenceType", "InformationNews" }
                };
                var sqlRes = html.SaveChangeData(
                    "DELETE FROM dbo.TblOtherChargeImage WHERE ReferenceID = @ReferenceID AND ReferenceType = @ReferenceType",
                    false, delPara);

                System.IO.File.AppendAllText(logFile, " | Delete news query result: " + sqlRes.Result);

                return Json(new { Result = 1 });
            }
            catch (Exception ex)
            {
                try { System.IO.File.AppendAllText(Server.MapPath("~/Uploads/cleanup_log.txt"), " | ERROR: " + ex.Message); } catch { }
                return Json(new { Result = 0, Error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UploadImage(int id, string remainingFiles = "")
        {
            if (id <= 0)
                return Json(new { Result = 0, Message = "Invalid OtherInformationID: " + id });

            try
            {
                string uploadPath = Server.MapPath("~/Uploads/Services/");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                HTML html = new HTML();

                // 1. Determine Category to decide ReferenceType for images ('Thumbnail' or 'InformationNews')
                string refType = "InformationNews";
                string catSql = @"SELECT cat.CategoryName 
                                  FROM dbo.TblOtherInformation info
                                  LEFT JOIN dbo.TblOtherInformationCategory cat ON cat.CategoryID = info.CategoryID
                                  WHERE info.OtherInformationID = @ID";
                var catPara = new Dictionary<string, string> { { "ID", id.ToString() } };
                var catDs = html.LoadData(catSql, false, catPara);
                if (catDs != null && catDs.Tables.Count > 0 && catDs.Tables[0].Rows.Count > 0)
                {
                    string catName = (catDs.Tables[0].Rows[0]["CategoryName"] ?? "").ToString().Trim();
                    if (catName.Equals("Services", StringComparison.OrdinalIgnoreCase) || catName.ToLower().Contains("service"))
                    {
                        refType = "Thumbnail";
                    }
                }

                // 2. Handle Video Upload if present
                HttpPostedFileBase videoFile = Request.Files["VideoUpload"];
                if (videoFile != null && videoFile.ContentLength > 0)
                {
                    string videoFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(videoFile.FileName);
                    videoFile.SaveAs(Path.Combine(uploadPath, videoFileName));

                    // Delete existing video record and physical file
                    string oldVideoSql = "SELECT FileName FROM dbo.TblOtherChargeImage WHERE ReferenceID = @ReferenceID AND ReferenceType = 'Video'";
                    var oldVideoPara = new Dictionary<string, string> { { "ReferenceID", id.ToString() } };
                    var oldVideoDs = html.LoadData(oldVideoSql, false, oldVideoPara);
                    if (oldVideoDs != null && oldVideoDs.Tables.Count > 0 && oldVideoDs.Tables[0].Rows.Count > 0)
                    {
                        string oldFileName = oldVideoDs.Tables[0].Rows[0]["FileName"].ToString();
                        if (oldFileName.Contains("/"))
                            oldFileName = oldFileName.Substring(oldFileName.LastIndexOf('/') + 1);
                        string oldPhysPath = Path.Combine(uploadPath, oldFileName);
                        if (System.IO.File.Exists(oldPhysPath))
                        {
                            try { System.IO.File.Delete(oldPhysPath); } catch { }
                        }

                        // Remove from DB
                        html.SaveChangeData("DELETE FROM dbo.TblOtherChargeImage WHERE ReferenceID = @ReferenceID AND ReferenceType = 'Video'", false, oldVideoPara);
                    }

                    // Insert new video record
                    string insVideoSql = @"INSERT INTO dbo.TblOtherChargeImage
                        (ReferenceID, ReferenceType, FileName, CreateBy, CreateDate, SystemID, WarehouseID, DomainID)
                    VALUES
                        (@ReferenceID, 'Video', @FileName, @CreateBy, @CreateDate, @SystemID, @WarehouseID, @DomainID)";
                    var insVideoPara = new Dictionary<string, string>
                    {
                        { "ReferenceID",   id.ToString() },
                        { "FileName",      "/Uploads/Services/" + videoFileName },
                        { "CreateBy",      html.UserFullName ?? "System" },
                        { "CreateDate",    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                        { "SystemID",      html.SystemID },
                        { "WarehouseID",   html.WarehouseID },
                        { "DomainID",      html.DomainID ?? "" }
                    };
                    html.SaveChangeData(insVideoSql, false, insVideoPara);
                }

                // 3. Save any newly uploaded image files to disk (sent under the key 'files')
                var newFileNames = new List<string>();
                if (Request.Files != null && Request.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        string fileKey = Request.Files.GetKey(i);
                        if (fileKey == "files") // matches the key appended in JS
                        {
                            HttpPostedFileBase file = Request.Files[i];
                            if (file != null && file.ContentLength > 0)
                            {
                                string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                                file.SaveAs(Path.Combine(uploadPath, fileName));
                                newFileNames.Add(fileName);
                            }
                        }
                    }
                }

                // 4. Build list of files the user wants to keep
                var keepFiles = new List<string>();
                if (!string.IsNullOrWhiteSpace(remainingFiles))
                {
                    foreach (var f in remainingFiles.Split(','))
                    {
                        if (!string.IsNullOrWhiteSpace(f))
                        {
                            string cleanF = f.Trim();
                            if (cleanF.Contains("/"))
                                cleanF = cleanF.Substring(cleanF.LastIndexOf('/') + 1);
                            keepFiles.Add(cleanF);
                        }
                    }
                }
                keepFiles.AddRange(newFileNames);

                // 5. Load existing DB image records
                string selSql = "SELECT OtherChargeImageID, FileName FROM dbo.TblOtherChargeImage WHERE ReferenceID = @ReferenceID AND ReferenceType = @ReferenceType";
                var selPara = new Dictionary<string, string>
                {
                    { "ReferenceID", id.ToString() },
                    { "ReferenceType", refType }
                };
                var ds = html.LoadData(selSql, false, selPara);

                var dbFiles = new List<Tuple<int, string>>();
                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                        dbFiles.Add(Tuple.Create(Convert.ToInt32(row["OtherChargeImageID"]), row["FileName"].ToString()));
                }

                // 6. Delete images the user removed
                foreach (var dbRow in dbFiles)
                {
                    string dbFileName = dbRow.Item2;
                    if (dbFileName.Contains("/"))
                        dbFileName = dbFileName.Substring(dbFileName.LastIndexOf('/') + 1);

                    if (!keepFiles.Contains(dbFileName))
                    {
                        var delPara = new Dictionary<string, string> { { "OtherChargeImageID", dbRow.Item1.ToString() } };
                        html.SaveChangeData("DELETE FROM dbo.TblOtherChargeImage WHERE OtherChargeImageID = @OtherChargeImageID", false, delPara);

                        string physPath = Path.Combine(uploadPath, dbFileName);
                        if (System.IO.File.Exists(physPath))
                            System.IO.File.Delete(physPath);
                    }
                }

                // 7. Insert newly uploaded images
                var existingFileNames = dbFiles.Select(t =>
                {
                    string fn = t.Item2;
                    if (fn.Contains("/")) fn = fn.Substring(fn.LastIndexOf('/') + 1);
                    return fn;
                }).ToList();

                foreach (var fn in newFileNames)
                {
                    if (!existingFileNames.Contains(fn))
                    {
                        string insSql = @"INSERT INTO dbo.TblOtherChargeImage
                            (ReferenceID, ReferenceType, FileName, CreateBy, CreateDate, SystemID, WarehouseID, DomainID)
                        VALUES
                            (@ReferenceID, @ReferenceType, @FileName, @CreateBy, @CreateDate, @SystemID, @WarehouseID, @DomainID)";
                        var insPara = new Dictionary<string, string>
                        {
                            { "ReferenceID",   id.ToString() },
                            { "ReferenceType", refType },
                            { "FileName",      "/Uploads/Services/" + fn },
                            { "CreateBy",      html.UserFullName ?? "System" },
                            { "CreateDate",    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                            { "SystemID",      html.SystemID },
                            { "WarehouseID",   html.WarehouseID },
                            { "DomainID",      html.DomainID ?? "" }
                        };
                        html.SaveChangeData(insSql, false, insPara);
                    }
                }

                return Json(new { Result = 1, Message = "Upload successful." });
            }
            catch (Exception ex)
            {
                return Json(new { Result = 0, Message = "Upload error: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetImages(int id)
        {
            var files = new List<string>();
            string videoFile = "";
            try
            {
                HTML html = new HTML();
                string sql = "SELECT FileName, ReferenceType FROM dbo.TblOtherChargeImage WHERE ReferenceID = @id AND ReferenceType IN ('InformationNews', 'Thumbnail', 'Video') ORDER BY OtherChargeImageID";
                var para = new Dictionary<string, string> { { "id", id.ToString() } };
                var ds = html.LoadData(sql, false, para);
                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        string rType = row["ReferenceType"] != null ? row["ReferenceType"].ToString() : "";
                        string fName = row["FileName"] != null ? row["FileName"].ToString() : "";

                        if (rType.Equals("Video", StringComparison.OrdinalIgnoreCase))
                        {
                            videoFile = fName;
                        }
                        else
                        {
                            files.Add(fName);
                        }
                    }
                }
            }
            catch { }
            return Json(new { files = files, video = videoFile }, JsonRequestBehavior.AllowGet);
        }
    }
}