using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class RADIUSController : Controller
    {
        // GET: RADIUS

        [Authorize(Roles = "Add Radius Profile, Edit Radius Profile, Delete Radius Profile")]
        public ActionResult RadiusProfile()
        {
            HTML html = new HTML
            {
                ReportTitle = "List Radius Profile"
            };
            if (html.HasPermission("Add Radius Profile"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Radius Profile"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Radius Profile"))
                html.AllowDelete = true;

            html.AddButton = "Add Radius Mikrotik Profile";
            html.AddURL = "RADIUS/CreateRediusProfile";
            html.TableName = "TblRediusProfile";
            html.PrimaryField = "RediusProfileID";
            html.OrderBy = "ProfileName";
            html.DialogBoxWidth = "Auto";

            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Profile Name",
                SQL = "select RediusProfileID, ProfileName from dbo.TblRediusProfile where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by ProfileName",
                SelectValue = "RediusProfileID",
                DisplayField = "ProfileName",
                RadioValue = EncryAndDecry.Encryp("RediusProfileID"),
                DefaultCheckBok = true
            };
            html.IsHaveWhereCondition = true;
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.SQL = "select ROW_NUMBER() OVER(ORDER BY ProfileName) AS[No], RediusProfileID, ProfileName, IPPOOL, case when FromTime1 <> '' then (FromTime1 + ' - ' + FromTime2) else ToTime3 end FromTime, case when ToTime1 <> '' then (ToTime1 + ' - ' + ToTime2) else ToTime4 end ToTime from dbo.TblRediusProfile where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Profile Name", FieldName = "ProfileName", Width = "100%" },
                new GridField() { Header = "IP POOL", FieldName = "IPPOOL" },
                new GridField() { Header = "From Time", FieldName = "FromTime" },
                new GridField() { Header = "To Time", FieldName = "ToTime" },
                new GridField() { Header = "Action", FieldName = "RediusProfileID", TextAlign = "Center",  IsModifyField = true, DeleteFunction="DeleteRediusProfile(this);" }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Radius Profile, Edit Radius Profile")]
        public ActionResult CreateRediusProfile(object[] PrimaryFieldValue)
        {
            dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(PrimaryFieldValue[0].ToString());
            int PackageDetailID = 0;
            int PFValue = PrimaryFieldValue[0].ToString().Contains("PrimaryFieldValue") ? (int)DataInfo["PrimaryFieldValue"] : (int)DataInfo;
            if (PrimaryFieldValue[0].ToString().Contains("PackageDetailID"))
                PackageDetailID = DataInfo["PackageDetailID"];

            HTML html = new HTML
            {
                FormID = "CreateRadiusProfileForm",
                FieldIDFocus = "ProfileName",
                TableName = "TblRediusProfile",
                PrimaryField = "RediusProfileID",
                PrimaryFieldValue = PFValue.ToString()
            };
            ViewBag.EditData = "";
            ViewBag.PackageData = "";
            if (PFValue != 0)
            {
                string sql = "select IsDoubleSpeedAtNight, FromTime1, FromTime2, ToTime1, ToTime2, ToTime3, ToTime4 ";
                sql += "from dbo.TblRediusProfile ";
                sql += "where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " and RediusProfileID = " + PFValue;
                //html.SQLForEdit = sql;

                string Data = html.JsonData(sql);
                ViewBag.EditData = Data;
            }
            if (PackageDetailID != 0)
            {
                string sql = "select p.ServiceID, p.PackageTypeID, pd.PackageDetailID from dbo.TblPackageDetail pd ";
                sql += "inner join dbo.TblPackages p on p.PackageID = pd.PackageID ";
                sql += "where pd.PackageDetailID = " + PackageDetailID;
                ViewBag.PackageData = html.JsonData(sql);
            }
            html.UseTable = true;
            html.TableColumn = 2;
            html.SaveChangeFunction = "SaveChagenRediusProfile();";
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Router Name:",
                ID = "RouterID",
                DBField = "RouterID",
                Required = true,
                SQL_CBO = "select RouterID, RouterName from dbo.TblRouter where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                DisplayText = "RouterName",
                Value = "RouterID"
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Service Name:",
                ID = "ServiceID",
                DBField = "ServiceID",
                Required = true,
                SQL_CBO = "select ServiceID, ServiceName from dbo.TblServices where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                DisplayText = "ServiceName",
                Value = "ServiceID",
                ISCallBackToCbo = true,
                SQLCallBack = "select PackageTypeID, PackageTypeName from dbo.TblPackageType where ServiceID = @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by PackageTypeName",
                CallBackDisplayText = "PackageTypeName",
                CallBackDisplayValue = "PackageTypeID",
                CallBackToControlID = "ConnectionTypeID",
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Connection Type:",
                ID = "ConnectionTypeID",
                DBField = "ConnectionTypeID",
                Infinity = false,
                Required = true,
                ISCallBackToCbo = true,
                SQLCallBack = "select pd.PackageDetailID, (p.PackageName + ' - ' + convert(varchar, pd.Bandwidth) + ' Mbps - ' + convert(varchar, pd.Duration) + ' ' + pd.UM) PackageName from dbo.TblPackageDetail pd inner join dbo.TblPackages p on p.PackageID = pd.PackageID where p.PackageTypeID = @SearchText and pd.SystemID = " + html.SystemID + " and pd.WarehouseID=" + html.WarehouseID + " order by p.PackageName",
                CallBackDisplayText = "PackageName",
                CallBackDisplayValue = "PackageDetailID",
                CallBackToControlID = "PackageDetailID",
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Package Name:",
                ID = "PackageDetailID",
                DBField = "PackageDetailID",
                Infinity = false,
                Required = true,
                IsCallBack = true,
                SQLCallBack = "select Duration, isnull(FreeDuration,0) FreeDuration, DowSpeed, UpSpeed, format(Price,'#,#.00') Price, (PackageName + ' - ' + convert(varchar, Bandwidth) + ' Mbps - ' + convert(varchar, Duration) + ' ' + UM) PackageName from dbo.TblPackageDetail where PackageDetailID = @SearchText and SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID,
                DisplayOnFields = "Duration, FreeDuration, Download, Upload, Price, ProfileName",
                CallBackFunction = "CallbackOnPackageChange()"
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Duration:",
                ID = "Duration",
                DBField = "Duration",
                Required = true,
                IsReadOnly = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Free Duration:",
                ID = "FreeDuration",
                DBField = "FreeDuration",
                Required = true,
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Download:",
                ID = "Download",
                DBField = "DownloadSpeed",
                Required = true,
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Upload:",
                ID = "Upload",
                DBField = "UploadSpeed",
                Required = true,
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Price:",
                ID = "Price",
                DBField = "Price",
                Required = true,
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Profile Name:",
                ID = "ProfileName",
                DBField = "ProfileName",
                Required = true,
                ISUniqueField = true,
                IsReadOnly = true,
                CheckExistingWithControlName = "RouterID",
                CheckExistingWithField = "RouterID",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Limitation Name:",
                ID = "LimitationName",
                DBField = "LimitationName",
                Required = true,
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "IP POOL:",
                ID = "IPPOOL",
                DBField = "IPPool",
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Validity:",
                ID = "Validity",
                DBField = "Validity",
                Required = true,
                IsReadOnly = true
            });
            html.HTMLControls.Add(new HtmlDateTimePicker()
            {
                Label = "Start / End Date:",
                ID = "TxtStartEndDate",
                Required = true,
                DateRangePicker = true,
                Hide = true
            });
            html.HTMLControls.Add(new HtmlCheckBox()
            {
                Label = "Is Double Speed At Night:",
                ID = "IsDSpeed",
                DBField = "IsDoubleSpeedAtNight",
                Required = true,
                Checked = true,
            });
            ViewBag.RediusProfileID = PFValue;
            ViewBag.PackageDetailID = PackageDetailID;
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        private void EnsureSuccess(SqlResult r)
        {
            if (r == null || r.Result != ResultType.Successful)
                throw new Exception(r != null ? r.Message : "Unknown MikroTik error");
        }

        [Authorize(Roles = "Add Radius Profile, Edit Radius Profile")]
        public ActionResult SaveChangeRediusProfile(object[] Data)
        {
            SqlResult result = new SqlResult();

            try
            {
                // ---------- Deserialize safely ----------
                RadiusProfileRequest req =
                    JsonConvert.DeserializeObject<RadiusProfileRequest>(Data[0].ToString());

                HTML html = new HTML();
                Mikrotik MK = new Mikrotik();

                const int KB_MULTIPLIER = 1049;
                const string SPEED_PREFIX = "KB";

                int downloadKb = req.Download * KB_MULTIPLIER;
                int uploadKb = req.Upload * KB_MULTIPLIER;

                bool isDoubleSpeedAtNight = false;

                MK.SetConnection(req.RouterID);

                // ---------- UPDATE ----------
                if (req.RediusProfileID > 0)
                {
                    EnsureSuccess(MK.UpdateLimitationName(
                        req.LimitationName,
                        downloadKb.ToString(),
                        uploadKb.ToString(),
                        req.IPPOOL,
                        SPEED_PREFIX));

                    EnsureSuccess(MK.UpdateProfileName(
                        req.ProfileName,
                        req.Validity,
                        req.Price.ToString()));

                    EnsureSuccess(MK.RemoveProfileLimitation(req.ProfileName));
                }
                // ---------- CREATE ----------
                else
                {
                    EnsureSuccess(MK.CreateLimitationName(
                        req.LimitationName,
                        downloadKb.ToString(),
                        uploadKb.ToString(),
                        req.IPPOOL,
                        SPEED_PREFIX));

                    EnsureSuccess(MK.CreateProfileName(
                        req.ProfileName,
                        req.Validity,
                        req.Price.ToString()));
                }

                // ---------- DOUBLE SPEED ----------
                if (!string.IsNullOrEmpty(req.LimitationName_DoubleSpeed))
                {
                    isDoubleSpeedAtNight = true;

                    int dDownload = downloadKb * 2;
                    int dUpload = uploadKb * 2;

                    EnsureSuccess(MK.CreateLimitationName(
                        req.LimitationName_DoubleSpeed,
                        dDownload.ToString(),
                        dUpload.ToString(),
                        req.IPPOOL,
                        SPEED_PREFIX));

                    EnsureSuccess(MK.ApplyProfileLimitation(req.ProfileName, req.LimitationName, req.FromTime1, req.FromTime2));
                    EnsureSuccess(MK.ApplyProfileLimitation(req.ProfileName, req.LimitationName_DoubleSpeed, req.ToTime1, req.ToTime2));
                    EnsureSuccess(MK.ApplyProfileLimitation(req.ProfileName, req.LimitationName_DoubleSpeed, req.ToTime3, req.ToTime4));
                }
                else
                {
                    EnsureSuccess(MK.ApplyProfileLimitation(req.ProfileName, req.LimitationName, req.ToTime3, req.ToTime4));
                }

                MK.CloseMKConnection();

                // ---------- DATABASE ----------
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                string sql;

                if (req.RediusProfileID == 0)
                {
                    sql =
                        "INSERT INTO dbo.TblRediusProfile " +
                        "(RouterID, Name, UserName, ServiceID, ConnectionTypeID, PackageDetailID, Duration, FreeDuration, " +
                        "DownloadSpeed, UploadSpeed, Price, ProfileName, LimitationName, IPPOOL, Validity, IsDoubleSpeedAtNight, " +
                        "LimitationDoubleSpeedName, FromTime1, FromTime2, ToTime1, ToTime2, ToTime3, ToTime4, SystemID, WarehouseID) " +
                        "VALUES (@RouterID, @Name, @UserName, @ServiceID, @ConnectionTypeID, @PackageDetailID, @Duration, @FreeDuration, " +
                        "@DownloadSpeed, @UploadSpeed, @Price, @ProfileName, @LimitationName, @IPPOOL, @Validity, @IsDoubleSpeedAtNight, " +
                        "@LimitationDoubleSpeedName, @FromTime1, @FromTime2, @ToTime1, @ToTime2, @ToTime3, @ToTime4, @SystemID, @WarehouseID);" +
                        "SELECT @@IDENTITY;";
                }
                else
                {
                    sql =
                        "UPDATE dbo.TblRediusProfile SET " +
                        "RouterID=@RouterID, ServiceID=@ServiceID, ConnectionTypeID=@ConnectionTypeID, PackageDetailID=@PackageDetailID, " +
                        "Duration=@Duration, FreeDuration=@FreeDuration, DownloadSpeed=@DownloadSpeed, UploadSpeed=@UploadSpeed, " +
                        "Price=@Price, ProfileName=@ProfileName, LimitationName=@LimitationName, IPPOOL=@IPPOOL, Validity=@Validity, " +
                        "IsDoubleSpeedAtNight=@IsDoubleSpeedAtNight, LimitationDoubleSpeedName=@LimitationDoubleSpeedName, " +
                        "FromTime1=@FromTime1, FromTime2=@FromTime2, ToTime1=@ToTime1, ToTime2=@ToTime2, ToTime3=@ToTime3, ToTime4=@ToTime4 " +
                        "WHERE RediusProfileID=@RediusProfileID;";
                    parameters.Add("RediusProfileID", req.RediusProfileID.ToString());
                }

                parameters.Add("RouterID", req.RouterID.ToString());
                parameters.Add("Name", "Radius Mikrotik Profile");
                parameters.Add("UserName", html.UserName);
                parameters.Add("ServiceID", req.ServiceID.ToString());
                parameters.Add("ConnectionTypeID", req.ConnectionTypeID.ToString());
                parameters.Add("PackageDetailID", req.PackageDetailID.ToString());
                parameters.Add("Duration", req.Duration.ToString());
                parameters.Add("FreeDuration", req.FreeDuration.ToString());
                parameters.Add("DownloadSpeed", downloadKb.ToString());
                parameters.Add("UploadSpeed", uploadKb.ToString());
                parameters.Add("Price", req.Price.ToString());
                parameters.Add("ProfileName", req.ProfileName);
                parameters.Add("LimitationName", req.LimitationName);
                parameters.Add("IPPOOL", req.IPPOOL);
                parameters.Add("Validity", req.Validity);
                parameters.Add("IsDoubleSpeedAtNight", isDoubleSpeedAtNight.ToString());
                parameters.Add("LimitationDoubleSpeedName", req.LimitationName_DoubleSpeed);
                parameters.Add("FromTime1", req.FromTime1);
                parameters.Add("FromTime2", req.FromTime2);
                parameters.Add("ToTime1", req.ToTime1);
                parameters.Add("ToTime2", req.ToTime2);
                parameters.Add("ToTime3", req.ToTime3);
                parameters.Add("ToTime4", req.ToTime4);
                parameters.Add("SystemID", html.SystemID);
                parameters.Add("WarehouseID", html.WarehouseID);

                result = html.SaveChangeData(sql, false, parameters, true);
            }
            catch (Exception ex)
            {
                result.Result = ResultType.Failed;
                result.Message = ex.Message;
            }

            return Json(new { Message = result.Message, Result = result.Result }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult SaveChangeRediusProfile(object[] Data)
        //{
        //    dynamic DataInfo = new JavaScriptSerializer().DeserializeObject(Data[0].ToString());
        //    HTML html = new HTML();
        //    string RouterID = DataInfo["RouterID"];
        //    string RediusProfileID = DataInfo["RediusProfileID"];
        //    string ServiceID = DataInfo["ServiceID"];
        //    string ConnectionTypeID = DataInfo["ConnectionTypeID"];
        //    string PackageDetailID = DataInfo["PackageDetailID"];
        //    string Duration = DataInfo["Duration"];
        //    string FreeDuration = DataInfo["FreeDuration"];
        //    string Download = DataInfo["Download"];
        //    string Upload = DataInfo["Upload"];
        //    Download = (int.Parse(Download.ToString()) * 1049).ToString();
        //    Upload = (int.Parse(Upload.ToString()) * 1049).ToString();
        //    string Prefix = "KB";
        //    string Price = DataInfo["Price"];
        //    string ProfileName = DataInfo["ProfileName"];
        //    string LimitationName = DataInfo["LimitationName"];
        //    string IPPOOL = DataInfo["IPPOOL"];//pppoe-pool
        //    string Validity = DataInfo["Validity"];
        //    string LimitationName_DoubleSpeed = DataInfo["LimitationName_DoubleSpeed"];
        //    string FromTime1 = DataInfo["FromTime1"];
        //    string FromTime2 = DataInfo["FromTime2"];
        //    string ToTime1 = DataInfo["ToTime1"];
        //    string ToTime2 = DataInfo["ToTime2"];
        //    string ToTime3 = DataInfo["ToTime3"];
        //    string ToTime4 = DataInfo["ToTime4"];
        //    bool IsDoubleSpeedAtNight = true;
        //    SqlResult result = new SqlResult();
        //    try
        //    {
        //        //ITikConnection connection = ConnectionFactory.OpenConnection(TikConnectionType.Api, HOST, USER, PASS);
        //        Mikrotik MK = new Mikrotik();
        //        MK.SetConnection(int.Parse(RouterID));
        //        if (RediusProfileID != "0")
        //        {
        //            result = MK.UpdateLimitationName(LimitationName, Download, Upload, IPPOOL, Prefix);
        //            result = MK.UpdateProfileName(ProfileName, Validity, Price);
        //            result = MK.RemoveProfileLimitation(ProfileName);

        //            if (LimitationName_DoubleSpeed != "")
        //            {
        //                int dDownload = int.Parse(Download.ToString()) * 2;
        //                int dUpload = int.Parse(Upload.ToString()) * 2;
        //                result = MK.CheckIsNotExisitingLimitationNameCreate(LimitationName_DoubleSpeed, dDownload.ToString(), dUpload.ToString(), IPPOOL, Prefix);
        //                result = MK.ApplyProfileLimitation(ProfileName, LimitationName, FromTime1, FromTime2);
        //                result = MK.ApplyProfileLimitation(ProfileName, LimitationName_DoubleSpeed, ToTime1, ToTime2);
        //                result = MK.ApplyProfileLimitation(ProfileName, LimitationName_DoubleSpeed, ToTime3, ToTime4);
        //            }
        //            else
        //            {
        //                result = MK.ApplyProfileLimitation(ProfileName, LimitationName, ToTime3, ToTime4);
        //                IsDoubleSpeedAtNight = false;
        //            }
        //        }
        //        else
        //        {
        //            result = MK.CreateLimitationName(LimitationName, Download, Upload, IPPOOL, Prefix);
        //            result = MK.CreateProfileName(ProfileName, Validity, Price);
        //            if (LimitationName_DoubleSpeed != "")
        //            {
        //                int dDownload = int.Parse(Download.ToString()) * 2;
        //                int dUpload = int.Parse(Upload.ToString()) * 2;
        //                result = MK.CreateLimitationName(LimitationName_DoubleSpeed, dDownload.ToString(), dUpload.ToString(), IPPOOL, Prefix);
        //                result = MK.ApplyProfileLimitation(ProfileName, LimitationName, FromTime1, FromTime2);
        //                result = MK.ApplyProfileLimitation(ProfileName, LimitationName_DoubleSpeed, ToTime1, ToTime2);
        //                result = MK.ApplyProfileLimitation(ProfileName, LimitationName_DoubleSpeed, ToTime3, ToTime4);
        //            }
        //            else
        //            {
        //                result = MK.ApplyProfileLimitation(ProfileName, LimitationName, ToTime3, ToTime4);
        //                IsDoubleSpeedAtNight = false;
        //            }
        //        }
        //        MK.CloseMKConnection();
        //        if (result.Result == ResultType.Successful)
        //        {
        //            Dictionary<string, string> parameters = new Dictionary<string, string>();
        //            string sql = "insert into dbo.TblRediusProfile (RouterID, Name, UserName, ServiceID, ConnectionTypeID, ";
        //            sql += "PackageDetailID, Duration, FreeDuration, DownloadSpeed, UploadSpeed, Price, ProfileName, LimitationName, ";
        //            sql += "IPPOOL, Validity, IsDoubleSpeedAtNight, LimitationDoubleSpeedName, FromTime1, FromTime2,";
        //            sql += "ToTime1, ToTime2, ToTime3, ToTime4, SystemID, WarehouseID) ";
        //            sql += "values (@RouterID, @Name, @UserName, @ServiceID, @ConnectionTypeID, ";
        //            sql += "@PackageDetailID, @Duration, @FreeDuration, @DownloadSpeed, @UploadSpeed, ";
        //            sql += "@Price, @ProfileName, @LimitationName, @IPPOOL, @Validity, @IsDoubleSpeedAtNight, ";
        //            sql += "@LimitationDoubleSpeedName, @FromTime1, @FromTime2, @ToTime1, @ToTime2, @ToTime3, ";
        //            sql += "@ToTime4, @SystemID, @WarehouseID);SELECT @@IDENTITY AS 'Identity';";
        //            if (RediusProfileID != "0")
        //            {
        //                sql = "update dbo.TblRediusProfile set RouterID = @RouterID, Name = @Name, UserName = @UserName, ServiceID = @ServiceID, ConnectionTypeID = @ConnectionTypeID, ";
        //                sql += "PackageDetailID = @PackageDetailID, Duration = @Duration, FreeDuration = @FreeDuration, DownloadSpeed = @DownloadSpeed, UploadSpeed = @UploadSpeed, Price = @Price, ProfileName = @ProfileName, LimitationName = @LimitationName, ";
        //                sql += "IPPOOL = @IPPOOL, Validity = @Validity, IsDoubleSpeedAtNight = @IsDoubleSpeedAtNight, LimitationDoubleSpeedName = @LimitationDoubleSpeedName, FromTime1 = @FromTime1, FromTime2 = @FromTime2,";
        //                sql += "ToTime1 = @ToTime1, ToTime2 = @ToTime2, ToTime3 = @ToTime3, ToTime4 = @ToTime4, SystemID = @SystemID, WarehouseID = @WarehouseID ";
        //                sql += "where RediusProfileID = @RediusProfileID;select @RediusProfileID";
        //                parameters.Add("RediusProfileID", RediusProfileID);
        //            }
        //            parameters.Add("RouterID", RouterID);
        //            parameters.Add("Name", "Radius Mikrotik Profile");
        //            parameters.Add("UserName", html.UserName);
        //            parameters.Add("ServiceID", ServiceID);
        //            parameters.Add("ConnectionTypeID", ConnectionTypeID);
        //            parameters.Add("PackageDetailID", PackageDetailID);
        //            parameters.Add("Duration", Duration);
        //            parameters.Add("FreeDuration", FreeDuration);
        //            parameters.Add("DownloadSpeed", Download);
        //            parameters.Add("UploadSpeed", Upload);
        //            parameters.Add("Price", Price);
        //            parameters.Add("ProfileName", ProfileName);
        //            parameters.Add("LimitationName", LimitationName);
        //            parameters.Add("IPPOOL", IPPOOL);
        //            parameters.Add("Validity", Validity);
        //            parameters.Add("IsDoubleSpeedAtNight", IsDoubleSpeedAtNight.ToString());
        //            parameters.Add("LimitationDoubleSpeedName", LimitationName_DoubleSpeed);
        //            parameters.Add("FromTime1", FromTime1);
        //            parameters.Add("FromTime2", FromTime2);
        //            parameters.Add("ToTime1", ToTime1);
        //            parameters.Add("ToTime2", ToTime2);
        //            parameters.Add("ToTime3", ToTime3);
        //            parameters.Add("ToTime4", ToTime4);
        //            parameters.Add("SystemID", html.SystemID);
        //            parameters.Add("WarehouseID", html.WarehouseID);

        //            result = html.SaveChangeData(sql, false, parameters, true);
        //            parameters.Clear();
        //            sql = "update dbo.TblPackageDetail set IsApplyRediusProfile = 1, RediusProfileID = @RediusProfileID where PackageDetailID = @PackageDetailID";
        //            parameters.Add("RediusProfileID", result.Value);
        //            parameters.Add("PackageDetailID", PackageDetailID);
        //            result = html.SaveChangeData(sql, false, parameters);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Result = ResultType.Failed;
        //        result.Message = ex.Message;
        //        return Json(new { Message = result.Message, Result = result.Result }, JsonRequestBehavior.AllowGet);
        //    }
        //    return Json(new { Message = result.Message, Result = result.Result }, JsonRequestBehavior.AllowGet);
        //}

        [Authorize(Roles = "Delete Radius Profile")]
        public ActionResult DeleteRediusProfile(int ID)
        {
            string sql = "select ProfileName, LimitationName, LimitationDoubleSpeedName, RouterID from dbo.TblRediusProfile where RediusProfileID = @RediusProfileID";
            Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "RediusProfileID", ID.ToString() }
            };
            HTML html = new HTML();
            DataSet ds = html.LoadData(sql, false, para);
            string ProfileName = ds.Tables[0].Rows[0]["ProfileName"].ToString();
            string LimitationName = ds.Tables[0].Rows[0]["LimitationName"].ToString();
            string LimitationDoubleSpeedName = ds.Tables[0].Rows[0]["LimitationDoubleSpeedName"].ToString();
            string RouterID = ds.Tables[0].Rows[0]["RouterID"].ToString();
            SqlResult sqlResult = new SqlResult();
            Mikrotik MK = new Mikrotik();
            MK.SetConnection(int.Parse(RouterID));
            sqlResult = MK.RemoveLimitationName(LimitationName);
            sqlResult = MK.RemoveProfileName(ProfileName);
            if (LimitationDoubleSpeedName != "")
            {
                sqlResult = MK.RemoveLimitationName(LimitationDoubleSpeedName);
            }
            //if (sqlResult.Result == ResultType.Successful)
            //{
            sql = "delete from dbo.TblRediusProfile where RediusProfileID = @RediusProfileID;";
            sql += "update dbo.TblPackageDetail set IsApplyRediusProfile = 0 where RediusProfileID = @RediusProfileID";
            sqlResult = html.SaveChangeData(sql, false, para);
            //}
            return Json(new { Data = "Delete Successful.", Result = sqlResult.Result, Message = sqlResult.Message }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "User Lists PPPoE")]
        public ActionResult RadiusUserListsPPPoE()
        {
            HTML html = new HTML
            {
                OverrideSearch = "ShowRadiusUserListsPPPoE()"
            };
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCID",
                Label = "CID:",
                SQLAutoComplete = "select distinct top 10 c.CustomerCode from dbo.TblCustomer c inner join dbo.TblCustomerBillingInvoice ci on ci.CustomerID = c.CustomerID where lower(c.CustomerCode) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                ISAutoComplete = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                Label = "Status:",
                SQL_CBO = "select  'false' [Status], 'Active' [StatusText]\r\nunion\r\nselect 'true' [Status], 'Disabled' [StatusText]\r\nunion\r\nselect 'Expired' [Status], 'Expired' [StatusText] order by StatusText",
                DisplayText = "StatusText",
                Value = "Status",
                ComboBox = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                ID = "txtRouterName",
                OPTLabel = "Router Name",
                SQL = "select RouterID, RouterName  from dbo.TblRouter where SystemID = " + html.SystemID + " and WarehouseID =" + html.WarehouseID,
                DisplayField = "RouterName",
                SelectValue = "RouterID",
                DefaultCheckBok = true,
                RadioValue = EncryAndDecry.Encryp("RouterID")
            });
            ViewBag.DrawHtmlTable = html.DrawHTMLSearchForm();
            return View();
        }
        [Authorize(Roles = "User Lists PPPoE")]
        public string ShowRadiusUserListsPPPoE(string CID, string Status, string RouterID)
        {
            HTML html = new HTML();
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            sqlResult = MK.ListsUserPPPoE(CID, Status, RouterID);
            StringBuilder sb = new StringBuilder();
            //if (sqlResult.Result == ResultType.Successful)
            //{
            html.UsingJqueryDataTable = true;
            html.JqueryDataTableResponsive = true;
            if (sqlResult.Data.Tables.Count > 0)
            {
                foreach (DataTable dt in sqlResult.Data.Tables)
                {
                    sb.AppendLine("<div class=\"row\"><div class=\"col\">");
                    sb.AppendLine("<h3> Router Name: " + dt.TableName + "</h3");
                    sb.AppendLine("</div></div>");
                    html.TblID = dt.TableName.Replace(" ", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);
                    sb.AppendLine("<div class=\"row\"><div class=\"col\">");
                    DataView dv = dt.DefaultView;
                    if (Status == "Expired")
                        dv.RowFilter = "Remain='<span class=\"text-danger\">Expired</span>'";
                    dv.Sort = "OrderBy ASC";
                    DataTable dts = dv.ToTable();
                    sb.AppendLine(html.DrawHtmlTable(dts));
                    sb.AppendLine("</div></div>");
                    sb.AppendLine("<div class=\"row\"><div class=\"col\">&nbsp;</div></div>");
                }
            }
            //}
            return sb.ToString();
        }

        [Authorize(Roles = "User Lists Vlan")]
        public ActionResult RadiusUserListsVlan()
        {
            HTML html = new HTML();

            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtCID",
                Label = "CID:",
                SQLAutoComplete = "select distinct top 10 c.CustomerCode from dbo.TblCustomer c inner join dbo.TblCustomerBillingInvoice ci on ci.CustomerID = c.CustomerID where lower(c.CustomerCode) like @SearchText and c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID,
                ISAutoComplete = true
            });
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtStatus",
                Label = "Status:",
                SQL_CBO = "select  'false' [Status], 'Active' [StatusText]\r\nunion\r\nselect 'true' [Status], 'Disabled' [StatusText]\r\nunion\r\nselect 'Expired' [Status], 'Expired' [StatusText] order by StatusText",
                DisplayText = "StatusText",
                Value = "Status",
                ComboBox = true
            });
            html.HtmlRadioBindToComboBox.Add(new RadioOptionBindDataToCbo()
            {
                ID = "txtRouterName",
                OPTLabel = "Router Name",
                SQL = "select RouterID, RouterName  from dbo.TblRouter where SystemID = " + html.SystemID + " and WarehouseID =" + html.WarehouseID,
                DisplayField = "RouterName",
                SelectValue = "RouterID",
                DefaultCheckBok = true,
                RadioValue = EncryAndDecry.Encryp("RouterID")
            });
            ViewBag.DrawHtmlTable = html.DrawHTMLSearchForm();
            return View();
        }

        [Authorize(Roles = "User Lists Vlan")]
        public string ShowRadiusUserListsVlan(string CID, string Status, string RouterID)
        {
            HTML html = new HTML();
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            sqlResult = MK.ListsUserVlan(CID, Status, RouterID);
            StringBuilder sb = new StringBuilder();
            html.UsingJqueryDataTable = true;
            if (sqlResult.Data.Tables.Count > 0)
            {
                foreach (DataTable dt in sqlResult.Data.Tables)
                {

                    DataView dv = dt.DefaultView;
                    if (Status == "Expired")
                        dv.RowFilter = "Remain='<span class=\"text-danger\">Expired</span>'";
                    dv.Sort = "OrderBy ASC";
                    DataTable dts = dv.ToTable();
                    sb.AppendLine("<div class=\"row\"><div class=\"col\">");
                    sb.AppendLine("<h3> Router Name: " + dt.TableName + "</h3>");
                    sb.AppendLine("</div></div>");
                    html.TblID = dt.TableName.Replace(" ", string.Empty);
                    sb.AppendLine("<div class=\"row\"><div class=\"col\">");
                    sb.AppendLine(html.DrawHtmlTable(dts));
                    sb.AppendLine("</div></div>");
                    sb.AppendLine("<div class=\"row\"><div class=\"col\">&nbsp;</div></div>");
                }
            }
            return sb.ToString();
        }

        [Authorize(Roles = "Add Radius User")]
        public ActionResult AddRadiusUser()
        {
            return View();
        }
        [Authorize(Roles = "Show PPPoE Package")]
        public ActionResult ShowPPPoEPackage()
        {
            HTML html = new HTML();
            string sql = "select RouterID, RouterName from dbo.TblRouter where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            ViewBag.CboRouterID = html.LoadDataToCBO("CboRouterID", sql, "RouterID", "RouterName");
            return View();
        }
        public ActionResult ShowPPPoEUserUsingPackage(int RouterID, string PackageID, string PackageName)
        {
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            MK.SetConnection(RouterID);
            sqlResult = MK.ShowPPPoEUserUsingPackage(PackageID, PackageName);
            ViewBag.Data = sqlResult.DataRowRouter.ToList().ToJsonString();
            return View();
        }
        [Authorize(Roles = "Show PPPoE User")]
        public ActionResult ShowPPPoEUser()
        {
            HTML html = new HTML();
            string sql = "select RouterID, RouterName from dbo.TblRouter where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            ViewBag.CboRouterID = html.LoadDataToCBO("CboRouterID", sql, "RouterID", "RouterName");
            return View();
        }
        [Authorize(Roles = "Add PPPoE Limitation, Edit PPPoE Limitation")]
        public ActionResult ShowPPPoELimitation()
        {
            HTML html = new HTML();
            string sql = "select RouterID, RouterName from dbo.TblRouter where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID;
            ViewBag.CboRouterID = html.LoadDataToCBO("CboRouterID", sql, "RouterID", "RouterName");
            return View();
        }
        [HttpPost]
        public ActionResult GetLimitation(string RouterIP)
        {
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            HTML html = new HTML();
            string sql = "select * from dbo.TblRouter where RouterID = " + RouterIP + " and SystemID=" + html.SystemID + " and WarehouseID =" + html.WarehouseID;
            DataSet ds = html.LoadData(sql);
            DataRow dr = ds.Tables[0].Rows[0];
            RouterIP = dr["IPAddress"].ToString();
            int RouterID = int.Parse(dr["RouterID"].ToString());
            int Port = int.Parse(dr["Port"].ToString());
            string Routerusername = dr["UserName"].ToString();
            string RouterPassword = dr["Password"].ToString();
            MK.SetConnection(RouterID);
            sqlResult = MK.GetLimitation(RouterIP, Port, Routerusername, RouterPassword);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = sqlResult.Value, Version = MK.VERSION }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetPackageName(string RouterIP, int Port = 0, string Routerusername = "", string RouterPassword = "")
        {
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            bool getRawData = Routerusername == "";
            int RouterID = 0;
            if (Routerusername == "")
            {
                HTML html = new HTML();
                string sql = "select * from dbo.TblRouter where RouterID = " + RouterIP + " and SystemID=" + html.SystemID + " and WarehouseID =" + html.WarehouseID;
                DataSet ds = html.LoadData(sql);
                DataRow dr = ds.Tables[0].Rows[0];
                RouterID = int.Parse(dr["RouterID"].ToString());
                RouterIP = dr["IPAddress"].ToString();
                Port = int.Parse(dr["Port"].ToString());
                Routerusername = dr["UserName"].ToString();
                RouterPassword = dr["Password"].ToString();
            }
            MK.SetConnection(RouterID);
            sqlResult = MK.GetProfileName(RouterIP, Port, Routerusername, RouterPassword, getRawData);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = sqlResult.Value, ProfileLimitation = sqlResult.Parameters.ProfileLimitation }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteProfileName(string RouterID, string ProfileID)
        {
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            MK.SetConnection(int.Parse(RouterID));
            sqlResult = MK.DeleteProfileName(ProfileID);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = sqlResult.Value }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteLimitation(string RouterID, string ProfileID)
        {
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            MK.SetConnection(int.Parse(RouterID));
            sqlResult = MK.DeleteLimitation(ProfileID);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = sqlResult.Value }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteProfileLimitation(string RouterID, string ProfileID)
        {
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            MK.SetConnection(int.Parse(RouterID));
            sqlResult = MK.DeleteProfileLimitation(ProfileID);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = sqlResult.Value }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CreatePPPoEUser(string RouterIP, int Port, string Routerusername, string RouterPassword, string pppoeusername, string pppoeuserpassword, string packagename, int StartNumber, int EndNumber)
        {
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            for (int i = StartNumber; i <= EndNumber; i++)
            {
                sqlResult = MK.CreatePPPoEUser(RouterIP, Port, Routerusername, RouterPassword, pppoeusername + "_" + i, pppoeuserpassword, packagename);
            }
            MK.CloseMKConnection();
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = sqlResult.Value }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateLimitation(string RouterID, string LimitationName, int minrx, int mintx, int rx, int tx, string IPPOOL)
        {
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            MK.SetConnection(int.Parse(RouterID));
            sqlResult = MK.UpdateLimitationName(LimitationName, minrx, mintx, rx, tx, IPPOOL);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = sqlResult.Value }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeletePPPoEUser(int RouterID, string UserID)
        {
            Mikrotik MK = new Mikrotik();
            SqlResult sqlResult = new SqlResult();
            MK.SetConnection(RouterID);
            sqlResult = MK.RemoveUserByID(UserID);
            return Json(new { Result = sqlResult.Result, Message = sqlResult.Message, Data = sqlResult.Value }, JsonRequestBehavior.AllowGet);
        }
    }
    public class RadiusProfileRequest
    {
        public int RouterID { get; set; }
        public int RediusProfileID { get; set; }
        public int ServiceID { get; set; }
        public int ConnectionTypeID { get; set; }
        public int PackageDetailID { get; set; }

        public int Duration { get; set; }
        public int FreeDuration { get; set; }

        public int Download { get; set; }
        public int Upload { get; set; }

        public string ProfileName { get; set; }
        public string LimitationName { get; set; }
        public string LimitationName_DoubleSpeed { get; set; }

        public string IPPOOL { get; set; }
        public string Validity { get; set; }

        public string FromTime1 { get; set; }
        public string FromTime2 { get; set; }
        public string ToTime1 { get; set; }
        public string ToTime2 { get; set; }
        public string ToTime3 { get; set; }
        public string ToTime4 { get; set; }

        public decimal Price { get; set; }
    }
}