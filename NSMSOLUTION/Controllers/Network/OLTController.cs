using MVC;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Utilities;

namespace NSMSOLUTION.Controllers.Network
{
    [Authorize]
    public class OLTController : Controller
    {
        // GET: OLT
        [Authorize(Roles = "Add OLT, Edit OLT, Delete OLT")]
        public ActionResult Index()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add OLT"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit OLT"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete OLT"))
                html.AllowDelete = true;

            html.AddButton = "Add OLT";
            html.AddURL = "OLT/AddOLT";
            html.TableName = "TblOLT";
            html.PrimaryField = "OLTID";
            html.OrderBy = "OLTID desc";
            html.DialogBoxWidth = "auto";
            html.GH.SQL = @"select ROW_NUMBER() OVER(ORDER BY OLTID) AS[No], OLTID, OLTName, 
SplitterName, PON,[Port],Length, LossDB,
case when GoogleMap_FullAddress <> '' then '<a target=""_blank"" 
href=""https://www.google.com/maps/place/'+convert(varchar, GoogleMap_Latitude)+','+convert(varchar, GoogleMap_Longitude)+'/@'+convert(varchar, GoogleMap_Latitude)+','+convert(varchar, GoogleMap_Longitude)+',15z"">' + GoogleMap_FullAddress + '</a>' else '' end GoogleMap_FullAddress,GoogleMap_Latitude, GoogleMap_Longitude,isnull([Address],'') + case when Commune <> '' then ', ' + Commune else '' end + case when Commune <> '' then ', ' + Commune else '' end + case when District <> '' then ', ' + District else '' end
+ case when ProvinceCity <> '' then ', ' + ProvinceCity else '' end [Address]
from dbo.TblOLT where SystemID=" + html.SystemID + " and WarehouseID=" + html.WarehouseID;
            html.IsHaveWhereCondition = true;
            html.HtmlDateTimePickerFromDateToDate.Add(new HtmlDateTimePickerFromDateToDate()
            {
                DefaultValueDate = false,
                //DateTimePickerWithRanges = true,
                DateRangePicker = true,
                LabelFromDate = "Date:",
                DBField = "Date"
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "OLT Name",
                SQL = "select OLTID, OLTName from dbo.TblOLT where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by OLTName",
                SelectValue = "OLTID",
                DisplayField = "OLTName",
                RadioValue = EncryAndDecry.Encryp("OLTID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "OLT Name", FieldName = "OLTName" },
                new GridField() { Header = "Splitter Name", FieldName = "SplitterName" },
                new GridField() { Header = "PON", FieldName = "PON" },
                new GridField() { Header = "Port", FieldName = "Port" },
                new GridField() { Header = "Length", FieldName = "Length" },
                new GridField() { Header = "LossDB", FieldName = "LossDB" },
                new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "Google Map", FieldName = "GoogleMap_FullAddress" },
                new GridField() { Header = "Action", FieldName = "OLTID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }
        [Authorize(Roles = "Add OLT, Edit OLT")]
        public ActionResult AddOLT(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FieldIDFocus = "OLTName",
                FormID = "FormAddOLT",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle ="OLT Information",
                    TableName="TblOLT",
                    GetIDENTITYByFiled="OLTID",
                    PrimaryFieldName="OLTID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Date:", ID="Date",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "Date", Required = true, ISDateTime=true,
                            SingleDatePicker=true,
                        },
                        new HtmlTextBox()
                        {
                            Label="OLT Name:", ID="OLTName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "OLTName", Required = true
                        },
                        new HtmlTextBox()
                        {
                            Label="PON:", ID="PON",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PON", Required = true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                         new HtmlTextBox()
                        {
                            Label="Splitter Name:", ID="SplitterName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SplitterName", Required = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Port:", ID="Port", Required=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "Port", ISNumeric=true,
                            ISInteger= true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Length:", ID="Length",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Length"
                        },
                        new HtmlTextBox()
                        {
                            Label="Loss (DB):", ID="LossDB",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="LossDB"
                        },
                    },
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Address:", ID="Address",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Address"
                        },
                        new HtmlTextBox()
                        {
                            Label="Commune:", ID="Commune",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "Commune",
                            SQL="select distinct c.Commune, d.District, p.ProvinceCity\r\nfrom [dbo].[TblCommune] c\r\ninner join [dbo].[TblDistrict] d on d.DistrictID = c.DistrictID\r\ninner join [dbo].[TblProvinceCity] p on p.ProvinceCityID = d.ProvinceCityID where c.SystemID = "+ html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by c.Commune",
                            DisplayOnFields = "District, ProvinceCity",
                            isTypeahead = true
                        },
                        new HtmlTextBox()
                        {
                            Label="District:", ID="District",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="District"
                        },
                        new HtmlTextBox()
                        {
                            Label="Province/City:", ID="ProvinceCity",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ProvinceCity"
                        },
                    },
                },
                    new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="GoogleMap:", ID="GoogleMap_FullAddress",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMap_FullAddress",
                            IsSetLocationWithGoogleMap=true, InputLatitude="GoogleMap_Latitude",
                            InputLongitude="GoogleMap_Longitude"
                        },
                        new HtmlTextBox()
                        {
                            Label="GoogleMap_Latitude:", ID="GoogleMap_Latitude",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMap_Latitude", Hide = true
                        },
                        new HtmlTextBox()
                        {
                            Label="GoogleMap_Longitude:", ID="GoogleMap_Longitude",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMap_Longitude", Hide = true
                        },
                    }
                },
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        [Authorize(Roles = "Add OLT, Edit OLT, Delete OLT")]
        public ActionResult OLTName()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add OLT Name"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit OLT Name"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete OLT Name"))
                html.AllowDelete = true;

            html.AddButton = "Add OLT Name";
            html.AddURL = "OLT/AddOLTName";
            html.TableName = "TblOLTName";
            html.PrimaryField = "OLTNameID";
            //html.OrderBy = "OLTNameID desc";
            html.DialogBoxWidth = "auto";
            html.GH.SQL = @"select ROW_NUMBER() OVER(ORDER BY OLTNameID) AS[No], OLTNameID, OLTName, 
PON, case when GoogleMapFullUrl <> '' then '<a target=""_blank"" href=""'+GoogleMapFullUrl+'"">'+ GoogleMapFullUrl +'</a>' else case when GoogleMap_FullAddress <> '' then '<a target=""_blank"" 
href=""https://www.google.com/maps/place/'+convert(varchar, GoogleMap_Latitude)+','+convert(varchar, GoogleMap_Longitude)+'/@'+convert(varchar, GoogleMap_Latitude)+','+convert(varchar, GoogleMap_Longitude)+',15z"">' + GoogleMap_FullAddress + '</a>' else '' end end GoogleMap_FullAddress,GoogleMap_Latitude, GoogleMap_Longitude,isnull([Address],'') + case when Commune <> '' then ', ' + Commune else '' end + case when Commune <> '' then ', ' + Commune else '' end + case when District <> '' then ', ' + District else '' end
+ case when ProvinceCity <> '' then ', ' + ProvinceCity else '' end [Address]
from dbo.TblOLTName where SystemID=" + html.SystemID + " and WarehouseID=" + html.WarehouseID;
            html.IsHaveWhereCondition = true;
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "OLT Name",
                SQL = "select OLTNameID, OLTName from dbo.TblOLTName where SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by OLTName",
                SelectValue = "OLTNameID",
                DisplayField = "OLTName",
                RadioValue = EncryAndDecry.Encryp("OLTNameID"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "OLT Name", FieldName = "OLTName" },
                //new GridField() { Header = "Splitter Name", FieldName = "SplitterName" },
                new GridField() { Header = "PON", FieldName = "PON" },
                //new GridField() { Header = "Port", FieldName = "Port" },
                //new GridField() { Header = "Length", FieldName = "Length" },
                //new GridField() { Header = "LossDB", FieldName = "LossDB" },
                new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "Google Map", FieldName = "GoogleMap_FullAddress" },
                new GridField() { Header = "Action", FieldName = "OLTNameID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add OLT Name, Edit OLT Name")]
        public ActionResult AddOLTName(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FieldIDFocus = "OLTName",
                FormID = "FormAddOLT",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle ="OLT Information",
                    TableName="TblOLTName",
                    GetIDENTITYByFiled="OLTNameID",
                    PrimaryFieldName="OLTNameID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="OLT Name:", ID="OLTName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "OLTName", Required = true
                        },
                        new HtmlTextBox()
                        {
                            Label="PON:", ID="PON",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="PON", Required = true, ISNumeric=true,
                            ISInteger=true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Address:", ID="Address",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Address"
                        },
                        new HtmlTextBox()
                        {
                            Label="Commune:", ID="Commune",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "Commune",
                            //SQL="select distinct c.Commune, d.District, p.ProvinceCity\r\nfrom [dbo].[TblCommune] c\r\ninner join [dbo].[TblDistrict] d on d.DistrictID = c.DistrictID\r\ninner join [dbo].[TblProvinceCity] p on p.ProvinceCityID = d.ProvinceCityID where c.SystemID = "+ html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by c.Commune",
                            SQL=@"select c.CommuneID, c.Commune, d.DistrictID, d.District, p.ProvinceCity, p.ProvinceCityID, 0 OrderBy
from dbo.TblCommune c
inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID
inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + @"
union
select c.CommuneID, c.CommuneKH Commune, d.DistrictID, d.DistrictKH District, p.ProvinceCityKH ProvinceCity, p.ProvinceCityID, 1 OrderBy
from dbo.TblCommune c
inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID
inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + @"
order by c.Commune, OrderBy",
                            DisplayOnFields = "District, ProvinceCity",
                            isTypeahead = true
                        },
                        new HtmlTextBox()
                        {
                            Label="District:", ID="District",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="District"
                        },
                        new HtmlTextBox()
                        {
                            Label="Province/City:", ID="ProvinceCity",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ProvinceCity"
                        },
                    },
                },
                    new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Map URL:", ID="GoogleMapFullUrl",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMapFullUrl", //Hide = true
                            DivClassCol="col-sm-auto", Type="url"
                        },
                        new HtmlTextBox()
                        {
                            Label="Set Map:", ID="GoogleMap_FullAddress",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMap_FullAddress",
                            IsSetLocationWithGoogleMap=true, InputLatitude="GoogleMap_Latitude",
                            InputLongitude="GoogleMap_Longitude"
                        },
                        new HtmlTextBox()
                        {
                            Label="Latitude:", ID="GoogleMap_Latitude",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMap_Latitude", //Hide = true
                            DivClassCol="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Longitude:", ID="GoogleMap_Longitude",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMap_Longitude", //Hide = true
                            DivClassCol="col-sm-auto"
                        },
                    }
                },
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        [Authorize(Roles = "Add Splitter Type, Edit Splitter Type, Delete Splitter Type")]
        public ActionResult SplitterType()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Splitter Type"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Splitter Type"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Splitter Type"))
                html.AllowDelete = true;

            html.AddButton = "Add Splitter Type";
            html.AddURL = "OLT/AddSplitterType";
            html.TableName = "TblSplitterType";
            html.PrimaryField = "SplitterTypeID";
            //html.OrderBy = "OLTNameID desc";
            //html.DialogBoxWidth = "auto";
            //            html.GH.SQL = @"select ROW_NUMBER() OVER(ORDER BY sn.SplitterID) AS[No], ot.OLTNameID, ot.OLTName, 
            //sn.SplitterName, sn.SplitterID, sn.[Port],
            //case when sn.GoogleMap_FullAddress <> '' then '<a target=""_blank"" 
            //href=""https://www.google.com/maps/place/'+convert(varchar, sn.GoogleMap_Latitude)+','+convert(varchar, sn.GoogleMap_Longitude)
            //+'/@'+convert(varchar, sn.GoogleMap_Latitude)+','+convert(varchar,sn.GoogleMap_Longitude)+',15z"">' + sn.GoogleMap_FullAddress + '</a>' else '' end GoogleMap_FullAddress,
            //sn.GoogleMap_Latitude, sn.GoogleMap_Longitude,isnull(sn.[Address],'') + case when sn.Commune <> '' then ', ' + sn.Commune else '' end + case when sn.Commune <> '' then ', ' + sn.Commune else '' end + case when sn.District <> '' then ', ' + sn.District else '' end
            //+ case when sn.ProvinceCity <> '' then ', ' + sn.ProvinceCity else '' end [Address]
            //from dbo.TblOLTName ot
            //inner join dbo.TblSplitterName sn on sn.OLTNameID = ot.OLTNameID where sn.SystemID=" + html.SystemID + " and sn.WarehouseID=" + html.WarehouseID;
            //            html.IsHaveWhereCondition = true;
            //html.HtmlAddControls.Add(new HtmlAddControls()
            //{
            //    ID = "txtSplitterTypeID",
            //    Label = "Splitter Type:",
            //    DBField = "SplitterTypeID",
            //    SQL_CBO = "select distinct SplitterType from dbo.TblSplitterType where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by SplitterType",
            //    DisplayText = "SplitterType",
            //    Value = "SplitterType",
            //    ComboBox = true
            //});
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Splitter Type",
                SQL = "select distinct SplitterType from dbo.TblSplitterType where SystemID = " + html.SystemID + " and WarehouseID = " + html.WarehouseID + " order by SplitterType",
                SelectValue = "SplitterType",
                DisplayField = "SplitterType",
                //ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("SplitterType"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "Splitter Type", FieldName = "SplitterType" },
                new GridField() { Header = "Port", FieldName = "Port", TextAlign="right" },
                new GridField() { Header = "Splitter For", FieldName = "SplitterFor" , Width = "100%"},
                new GridField() { Header = "Action", FieldName = "SplitterTypeID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Splitter Type, Edit Splitter Type")]
        public ActionResult AddSplitterType(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FieldIDFocus = "SplitterType",
                FormID = "FormSplitterType",
                PrimaryField = "SplitterTypeID",
                PrimaryFieldValue = PrimaryFieldValue,
                TableName = "TblSplitterType"
            };
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Splitter Type:",
                ID = "SplitterType",
                DBField = "SplitterType",
                Required = true,
            });
            html.HTMLControls.Add(new HtmlTextBox()
            {
                Label = "Port:",
                ID = "Port",
                DBField = "Port",
                Required = true,
                ISInteger = true,
                ISNumeric = true,
            });
            html.HTMLControls.Add(new HtmlComboBox()
            {
                Label = "Splitter For:",
                ID = "SplitterFor",
                DBField = "SplitterFor",
                Required = true,
                SQL_CBO = "select 'OLT' [Type] union select 'Splitter' [Type]",
                DisplayText = "Type",
                Value = "Type",
                Infinity = false
            });
            ViewBag.DrawHTMLForm = html.DrawHTMLForm();
            return View();
        }
        [Authorize(Roles = "Add PON, Edit PON, Delete PON")]
        public ActionResult PON()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add PON"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit PON"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete PON"))
                html.AllowDelete = true;

            html.AddButton = "Add OLT Info";
            html.AddURL = "OLT/AddPON";
            html.TableName = "TblAddOLTInfo";
            html.PrimaryField = "SplitterID";
            //html.OrderBy = "OLTNameID desc";
            html.DialogBoxWidth = "auto";
            html.GH.SQL = @"select ROW_NUMBER() OVER(ORDER BY sn.OLTName, sn.PON) AS [No], ot.OLTNameID, ot.OLTName, 
sn.SplitterName, sn.OLTInfoID, sn.[Port],
case when isnull(sn.GoogleMapFullUrl, '') <> '' then '<a target=""_blank"" href=""'+sn.GoogleMapFullUrl+'"" >' + sn.GoogleMapFullUrl + '</a>' else
case when sn.GoogleMap_FullAddress <> '' then '<a target=""_blank"" 
href=""https://www.google.com/maps/place/'+convert(varchar, sn.GoogleMap_Latitude)+','+convert(varchar, sn.GoogleMap_Longitude)
+'/@'+convert(varchar, sn.GoogleMap_Latitude)+','+convert(varchar,sn.GoogleMap_Longitude)+',15z"">' + sn.GoogleMap_FullAddress + '</a>' else '' end end GoogleMap_FullAddress,
sn.GoogleMap_Latitude, sn.GoogleMap_Longitude, case when isnull(sn.[Address],'') <> '' then sn.[Address] + ', ' else '' end + case when sn.Commune <> '' then sn.Commune else '' end + case when sn.District <> '' then ', ' + sn.District else '' end
+ case when sn.ProvinceCity <> '' then ', ' + sn.ProvinceCity else '' end [Address], convert(varchar, sn.Length) + ' ' + sn.UM Length, sn.LossDB, sn.PON
from dbo.TblOLTName ot
inner join dbo.TblAddOLTInfo sn on sn.OLTNameID = ot.OLTNameID where sn.SystemID=" + html.SystemID + " and sn.WarehouseID=" + html.WarehouseID;
            html.OrderBy = "[No]";
            html.IsHaveWhereCondition = true;
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtOLTNameID",
                Label = "OLT Name:",
                DBField = "sn.OLTNameID",
                SQL_CBO = "select distinct ot.OLTNameID, ot.OLTName from dbo.TblOLTName ot\r\ninner join dbo.TblAddOLTInfo sn on sn.OLTNameID = ot.OLTNameID\r\n where ot.SystemID=" + html.SystemID + " and ot.WarehouseID = " + html.WarehouseID + " order by ot.OLTName",
                DisplayText = "OLTName",
                Value = "OLTNameID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Splitter Name",
                SQL = "select distinct top 10 SplitterName from dbo.TblAddOLTInfo where lower(SplitterName) like @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by SplitterName",
                //SelectValue = "SplitterName",
                //DisplayField = "SplitterName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("SplitterName"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "OLT Name", FieldName = "OLTName", GroupLabel="OLT Name: ", IsGroupByField = true, Colspan=9 },
                new GridField() { Header = "PON", FieldName = "PON", TextAlign = "right" },
                new GridField() { Header = "Splitter Name", FieldName = "SplitterName" },
                new GridField() { Header = "Port", FieldName = "Port", TextAlign = "right" },
                new GridField() { Header = "Length", FieldName = "Length" },
                new GridField() { Header = "LossDB", FieldName = "LossDB" },
                new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "Google Map", FieldName = "GoogleMap_FullAddress" },
                new GridField() { Header = "Action", FieldName = "OLTInfoID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add PON, Edit PO")]

        public ActionResult AddPON(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FieldIDFocus = "OLTNameID",
                FormID = "FormSplitterName",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle ="PON Information",
                    TableName="TblAddOLTInfo",
                    GetIDENTITYByFiled="OLTInfoID",
                    PrimaryFieldName="OLTInfoID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="OLT Name:", ID="OLTNameID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "OLTName", Required = true,
                            SQL="select tt.OLTNameID, tt.OLTName, tt.PON from dbo.TblOLTName tt where tt.SystemID = "+html.SystemID+" and tt.WarehouseID = "+html.WarehouseID+" and tt.PON > (select isnull(count(t.OLTNameID),0)  from TblAddOLTInfo t where t.OLTNameID = tt.OLTNameID) order by tt.OLTName",
                            isTypeahead = true,
                            HiddenField="OLTNameID",
                            //DisplayOnFields="PONID, PON",
                            SQLCallBack="DECLARE @i int = 0;DECLARE @PON int=0; select @PON = PON from TblOLTName where OLTNameID = @OLTNameID and SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+@"
DECLARE @TblPON TABLE(PONID int IDENTITY(1,1), PON INT)
WHILE @i < @PON
BEGIN
    SET @i = @i + 1
	if(not exists (select * from dbo.TblAddOLTInfo where PON = @i and OLTNameID = @OLTNameID))
	begin
		insert into @TblPON (PON)values (@i)
	end
END
select * from @TblPON;",
                            isApplyTypeaheadOnCallBack=true,
                            //DisplayOnFieldsTypeahead="PON",
                            CallBackToControlID="PON",
                            FieldName="PON",
                        },
                        new HtmlTextBox()
                        {
                            Label="PON:", ID="PON", ISInteger=true, ISNumeric=true,
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "PON", Required = true,
                            HiddenField="PONID", isTypeahead = true

                        },
                        new HtmlTextBox()
                        {
                            Label="Splitter Type:", ID="SplitterType",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SplitterType", Required = true,
                            isTypeahead=true,
                            SQL="select SplitterTypeID, SplitterType, [Port] from dbo.TblSplitterType where SplitterFor = 'OLT'\r\n and SystemID="+html.SystemID+" and WarehouseID = "+ html.WarehouseID +" order by [Port]",
                            HiddenField="SplitterTypeID", DisplayOnFields = "Port"
                        },
                        new HtmlTextBox()
                        {
                            Label="Splitter Name:", ID="SplitterName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SplitterName", Required = true, ISUniqueField = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Port:", ID="Port",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Port", Required = true, ISNumeric=true,
                            ISInteger=true, IsReadOnly=true, Hide = true
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Length:", ID="Length",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Length", ISNumeric=true,
                            IsAllowInputOnlyNumber=true, Digit="0.000"
                        },
                        new HtmlTextBox()
                        {
                            Label="UM:", ID="UM",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="UM", Value = "M",
                            isTypeahead = true,
                            SQL="select 'M' UM union select 'KM' UM"
                        },
                        new HtmlTextBox()
                        {
                            Label="Loss DB:", ID="LossDB",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="LossDB", ISNumeric = true,
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Address:", ID="Address",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Address"
                        },
                        new HtmlTextBox()
                        {
                            Label="Commune:", ID="Commune",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "Commune",
                            //SQL="select distinct c.Commune, d.District, p.ProvinceCity\r\nfrom [dbo].[TblCommune] c\r\ninner join [dbo].[TblDistrict] d on d.DistrictID = c.DistrictID\r\ninner join [dbo].[TblProvinceCity] p on p.ProvinceCityID = d.ProvinceCityID where c.SystemID = "+ html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by c.Commune",
                            SQL=@"select c.CommuneID, c.Commune, d.DistrictID, d.District, p.ProvinceCity, p.ProvinceCityID, 0 OrderBy
from dbo.TblCommune c
inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID
inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + @"
union
select c.CommuneID, c.CommuneKH Commune, d.DistrictID, d.DistrictKH District, p.ProvinceCityKH ProvinceCity, p.ProvinceCityID, 1 OrderBy
from dbo.TblCommune c
inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID
inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + @"
order by c.Commune, OrderBy",
                            DisplayOnFields = "District, ProvinceCity",
                            isTypeahead = true
                        },
                        new HtmlTextBox()
                        {
                            Label="District:", ID="District",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="District"
                        },
                        new HtmlTextBox()
                        {
                            Label="Province/City:", ID="ProvinceCity",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ProvinceCity"
                        },
                    },
                },
                    new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Map URL:", ID="GoogleMapFullUrl",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMapFullUrl", //Hide = true
                            DivClassCol="col-sm-auto", Type="url"
                        },
                        new HtmlTextBox()
                        {
                            Label="Set Map:", ID="GoogleMap_FullAddress",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMap_FullAddress",
                            IsSetLocationWithGoogleMap=true, InputLatitude="GoogleMap_Latitude",
                            InputLongitude="GoogleMap_Longitude"
                        },
                        new HtmlTextBox()
                        {
                            Label="Latitude:", ID="GoogleMap_Latitude",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMap_Latitude", //Hide = true
                            DivClassCol="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Longitude:", ID="GoogleMap_Longitude",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMap_Longitude", //Hide = true
                            DivClassCol="col-sm-auto"
                        },
                    }
                },
                new  FormData() {
                TableName="TblStockInDetail",
                GetIDENTITYByFiled="StockInID",
                PrimaryFieldName="StockInID",
                htmlControl = new HTMLControl[]
                {
                    new HtmlMyTable()
                    {
                        TableResponsive=false,
                        ShowFooter = false,
                        ShowTermsConditions = false,
                        ShowCustomerNotes = false,
                        ShowBalance=false,
                        ShowDeposit=false,
                        GetIDENTITYByFiled="OLTInfoID",
                        PrimaryFieldName="OLTInfoID",
                        TableName="TblSplitterName",
                        HtmlTblID="TableSplitter",
                        InputName="ToPort",
                        TRName="TRNo",
                        TDName="TDNo",
                        //ScriptCallBackFunction="CallSetAutoComplete_ItemName,ItemExpirationDate_ApplyDatePicker,ApplySummernote('summernote'),TotalAll, Reload_Reason()",
                        ScriptCallBackFunction="Reload_ToSplitterType()",
                        tblTHead= new List<TblTHead>()
                            {
                                new TblTHead()
                                {
                                    OpenTR="<tr>", Label="No"
                                },
                            new TblTHead()
                                {
                                     Label="Port"
                                },
                            new TblTHead()
                                {
                                     Label="Spliiter Type", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Spliiter Name", Class="text-nowrap"
                                },
                                new TblTHead()
                                {
                                     Label="Length"
                                },
                                //new TblTHead()
                                //{
                                //     Label="UM"
                                //},
                                new TblTHead()
                                {
                                     Label="Loss DB", Class="text-nowrap col-auto"
                                },
                                new TblTHead()
                                {
                                     Label="Address"
                                },
                                new TblTHead()
                                {
                                     Label="Commune"
                                },
                                //new TblTHead()
                                //{
                                //     Label="District"
                                //},
                                //new TblTHead()
                                //{
                                //     Label="Province/City"
                                //},
                                new TblTHead()
                                {
                                     Label="Map URL", Class="text-nowrap",
                                     CloseTR="</tr>"
                                },
                                //new TblTHead()
                                //{
                                //     Label="Set Map", Class="text-nowrap"
                                //},
                                //new TblTHead()
                                //{
                                //     Label="Latitude", Class="col-auto"
                                //},
                                //new TblTHead()
                                //{
                                //     Label="Longitude", Class="col-auto",
                                //     CloseTR="</tr>"
                                //},
                        },
                        tblTBody= new List<TblTBody>()
                        {
                                new TblTBody()
                                {
                                    OpenTR="<tr name=\"TRNo\">",
                                    html_Control = new HTMLControl[]
                                    {
                                        new HtmlLabel()
                                        {
                                            Label="1", Align="right", TDName="TDNo"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ToPort",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Port",
                                            DivClass="wrap-input3",
                                            placeholder="Enter Port",
                                            DeleteTDRowFunction="RemoveTableRow('TRNo', 'TDNo', this,'','ToPort');",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ToSplitterType", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="SplitterType", DivClass="wrap-input3",
                                            placeholder="Enter Splitter Type",
                                            SQL="select SplitterTypeID, [Port], SplitterType from dbo.TblSplitterType where SplitterFor = 'Splitter'\r\n and SystemID="+html.SystemID+" and WarehouseID = "+ html.WarehouseID +" order by [Port]",
                                            isTypeahead = true, HiddenField = "SplitterTypeID, Port", HiddenFieldValue="0,0"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="ToSplitterName", Required=true,
                                            Class="input2 CustInfo", IsCustomForm = true,
                                            DBField="ToSplitterName", DivClass="wrap-input3",
                                            placeholder="Enter Splitter Name",
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Length",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Length",
                                            DivClass="wrap-input3",
                                            placeholder="Enter Length"
                                        },
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="UM",
                                        //    Class="input2 CustInfo text-center", IsCustomForm = true,
                                        //    DBField="UM",
                                        //    DivClass="wrap-input3",
                                        //    placeholder="Enter UM"
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="LossDB",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="LossDB",
                                            DivClass="wrap-input3",
                                            placeholder="Enter Loss DB"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Address",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Address",
                                            DivClass="wrap-input3",
                                            placeholder="Enter Address"
                                        },
                                        new HtmlTextBox()
                                        {
                                            Name="Commune",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="Commune",
                                            DivClass="wrap-input3",
                                            placeholder="Enter Commune"
                                        },
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="District",
                                        //    Class="input2 CustInfo text-center", IsCustomForm = true,
                                        //    DBField="District",
                                        //    DivClass="wrap-input3",
                                        //    placeholder="Enter District"
                                        //},
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="ProvinceCity",
                                        //    Class="input2 CustInfo text-center", IsCustomForm = true,
                                        //    DBField="ProvinceCity",
                                        //    DivClass="wrap-input3",
                                        //    placeholder="Enter Province/City"
                                        //},
                                        new HtmlTextBox()
                                        {
                                            Name="MapURL",
                                            Class="input2 CustInfo text-center", IsCustomForm = true,
                                            DBField="MapURL",
                                            DivClass="wrap-input3",
                                            placeholder="Enter Map URL"
                                        },
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="SetMap",
                                        //    Class="input2 CustInfo text-center", IsCustomForm = true,
                                        //    DBField="SetMap",
                                        //    DivClass="wrap-input3",
                                        //    placeholder="Enter Set Map"
                                        //},
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="Latitude",
                                        //    Class="input2 CustInfo text-center", IsCustomForm = true,
                                        //    DBField="Latitude",
                                        //    DivClass="wrap-input3",
                                        //    placeholder="Enter Latitude"
                                        //},
                                        //new HtmlTextBox()
                                        //{
                                        //    Name="Longitude",
                                        //    Class="input2 CustInfo text-center", IsCustomForm = true,
                                        //    DBField="Longitude",
                                        //    DivClass="wrap-input3",
                                        //    placeholder="Enter Longitude"
                                        //},
                                    },
                                    CloseTR="</tr>"
                                }
                        }
                    }
                }
                }
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }

        [Authorize(Roles = "Add Splitter Name, Edit Splitter Name, Delete Splitter Name")]
        public ActionResult SplitterName()
        {
            HTML html = new HTML();
            if (html.HasPermission("Add Splitter Name"))
                html.AllowAdd = true;
            if (html.HasPermission("Edit Splitter Name"))
                html.AllowEdit = true;
            if (html.HasPermission("Delete Splitter Name"))
                html.AllowDelete = true;

            html.AddButton = "Add Splitter Info";
            html.AddURL = "OLT/AddSplitterName";
            html.TableName = "TblSplitterName";
            html.PrimaryField = "SplitterID";
            //html.OrderBy = "OLTNameID desc";
            html.DialogBoxWidth = "auto";
            html.GH.SQL = @"select ROW_NUMBER() over (order by sn.SplitterID) as [No], sn.SplitterID,
oi.OLTName, oi.PON, sn.SplitterName, sn.[Port],
st.SplitterType, sn.ToSplitterName, sn.ToPort, convert(varchar, sn.Length) + ' ' + sn.UM Length, sn.LossDB,
isnull(sn.[Address],'') + case when sn.Commune <> '' then ', ' + sn.Commune else '' end + case when sn.Commune <> '' then ', ' + sn.Commune else '' end + case when sn.District <> '' then ', ' + sn.District else '' end
+ case when sn.ProvinceCity <> '' then ', ' + sn.ProvinceCity else '' end [Address],
case when sn.GoogleMap_FullAddress <> '' then '<a target=""_blank""
href=""https://www.google.com/maps/place/'+convert(varchar, sn.GoogleMap_Latitude)+','+convert(varchar, sn.GoogleMap_Longitude)
+'/@'+convert(varchar, sn.GoogleMap_Latitude)+','+convert(varchar,sn.GoogleMap_Longitude)+',15z"">' + sn.GoogleMap_FullAddress + '</a>' else '' end GoogleMap_FullAddress,
sn.GoogleMap_Latitude, sn.GoogleMap_Longitude
from dbo.TblSplitterName sn
inner join dbo.TblAddOLTInfo oi on oi.OLTInfoID = sn.OLTInfoID
inner join dbo.TblSplitterType st on st.SplitterTypeID = sn.SplitterTypeID where sn.SystemID=" + html.SystemID + " and sn.WarehouseID=" + html.WarehouseID;
            html.IsHaveWhereCondition = true;
            html.HtmlAddControls.Add(new HtmlAddControls()
            {
                ID = "txtOLTNameID",
                Label = "OLT Name:",
                DBField = "sn.OLTInfoID",
                SQL_CBO = "select distinct sn.OLTInfoID, ot.OLTName from dbo.TblOLTName ot\r\ninner join dbo.TblAddOLTInfo ai on ai.OLTNameID = ot.OLTNameID\r\ninner join dbo.TblSplitterName sn on sn.OLTInfoID = ai.OLTInfoID where ot.SystemID=" + html.SystemID + " and ot.WarehouseID = " + html.WarehouseID + " order by ot.OLTName",
                DisplayText = "OLTName",
                Value = "OLTInfoID",
                ComboBox = true
            });
            List<RadioOptionBindDataToCbo> htmlRadioBindToComboBox = html.HtmlRadioBindToComboBox;
            RadioOptionBindDataToCbo radioOptionBindDataToCbo = new RadioOptionBindDataToCbo()
            {
                OPTLabel = "Splitter Name",
                SQL = "select distinct top 10 SplitterName from dbo.TblSplitterName where lower(SplitterName) = @SearchText and SystemID = " + html.SystemID + " and WarehouseID=" + html.WarehouseID + " order by OLTName",
                //SelectValue = "SplitterName",
                //DisplayField = "SplitterName",
                ISAutoComplete = true,
                RadioValue = EncryAndDecry.Encryp("sn.SplitterName"),
                DefaultCheckBok = true
            };
            htmlRadioBindToComboBox.Add(radioOptionBindDataToCbo);
            html.GH.GridTable = new GridField[]
            {
                new GridField() { Header = "#", FieldName = "No", TextAlign = "right" },
                new GridField() { Header = "OLT Name", FieldName = "OLTName" },
                new GridField() { Header = "PON", FieldName = "PON" },
                new GridField() { Header = "SplitterName", FieldName = "SplitterName" },
                new GridField() { Header = "Port", FieldName = "Port", TextAlign="right" },
                new GridField() { Header = "To Splitter Name", FieldName = "ToSplitterName" },
                new GridField() { Header = "To Port", FieldName = "ToPort", TextAlign="right" },
                new GridField() { Header = "Length", FieldName = "Length" },
                new GridField() { Header = "Loss DB", FieldName = "LossDB" },
                new GridField() { Header = "Address", FieldName = "Address" },
                new GridField() { Header = "Google Map", FieldName = "GoogleMap_FullAddress" },
                new GridField() { Header = "Action", FieldName = "SplitterID", TextAlign = "Center",  IsModifyField = true }
            };
            html.ShowPaging = false;
            ViewBag.DrawHTMLSearchForm = html.DrawHTMLSearchForm(1, "", true, html.GH.GridTable.ToList().ToJsonString());
            return View();
        }

        [Authorize(Roles = "Add Splitter Name, Edit Splitter Name")]
        public ActionResult AddSplitterName(string PrimaryFieldValue)
        {
            HTML html = new HTML
            {
                FieldIDFocus = "SplitterName",
                FormID = "FormSplitterName",
                PrimaryFieldValue = PrimaryFieldValue
            };
            html.FormData = new HTMLControl[]
            {
                new  FormData() {
                    FormTitle ="Splitter Information",
                    TableName="TblSplitterName",
                    GetIDENTITYByFiled="SplitterID",
                    PrimaryFieldName="SplitterID",
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Splitter Name:", ID="SplitterName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SplitterName", Required = true,
                            SQL=@"select tt.OLTInfoID, tt.SplitterName, tt.[Port] from dbo.TblAddOLTInfo tt
where tt.SystemID = "+html.SystemID+" and tt.WarehouseID = "+html.WarehouseID+@"
and tt.[Port] > (select isnull(count(t.OLTInfoID),0) 
from TblSplitterName t where t.OLTInfoID = tt.OLTInfoID) order by tt.SplitterName",
                            isTypeahead=true, HiddenField="OLTInfoID",
                            SQLCallBack=@"DECLARE @i int = 0;DECLARE @Port int=0;
select @Port = [Port] from TblAddOLTInfo 
where OLTInfoID = @OLTInfoID and SystemID = "+html.SystemID+" and WarehouseID = "+html.WarehouseID+@"
DECLARE @TblPort TABLE(PortID int IDENTITY(1,1), [Port] INT)
WHILE @i < @Port
BEGIN
    SET @i = @i + 1
	if(not exists (select * from dbo.TblSplitterName where [Port] = @i and OLTInfoID = @OLTInfoID))
	begin
		insert into @TblPort ([Port])values (@i)
	end
END
select * from @TblPort;",
                            isApplyTypeaheadOnCallBack=true,
                            CallBackToControlID="Port",
                            FieldName="Port",
                        },
                        new HtmlTextBox()
                        {
                            Label="Port:", ID="Port",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Port", Required = true, ISNumeric=true,
                            ISInteger=true,
                        },
                        new HtmlTextBox()
                        {
                            Label="Splitter Type:", ID="SplitterTypeID",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="SplitterType", Required = true,
                            isTypeahead=true,
                            SQL="select SplitterTypeID, SplitterType, [Port] ToPort from dbo.TblSplitterType where SplitterFor = 'Splitter'\r\n and SystemID="+html.SystemID+" and WarehouseID = "+ html.WarehouseID +" order by [Port]",
                            HiddenField="SplitterTypeID", DisplayOnFields = "ToPort"
                        },
                        new HtmlTextBox()
                        {
                            Label="Splitter Name:", ID="ToSplitterName",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ToSplitterName", Required = true, ISUniqueField = true
                        },
                        new HtmlTextBox()
                        {
                            Label="Port:", ID="ToPort",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ToPort", Required = true, ISNumeric=true,
                            ISInteger=true, IsReadOnly=true,
                        },

                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Length:", ID="Length",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Length"
                        },
                        new HtmlTextBox()
                        {
                            Label="UM:", ID="UM",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="UM", Value = "M",
                            isTypeahead = true,
                            SQL="select 'M' UM union select 'KM' UM"
                        },
                        new HtmlTextBox()
                        {
                            Label="Loss DB:", ID="LossDB",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="LossDB"
                        },
                    }
                },
                new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Address:", ID="Address",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="Address"
                        },
                        new HtmlTextBox()
                        {
                            Label="Commune:", ID="Commune",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField = "Commune",
                            //SQL="select distinct c.Commune, d.District, p.ProvinceCity\r\nfrom [dbo].[TblCommune] c\r\ninner join [dbo].[TblDistrict] d on d.DistrictID = c.DistrictID\r\ninner join [dbo].[TblProvinceCity] p on p.ProvinceCityID = d.ProvinceCityID where c.SystemID = "+ html.SystemID + " and c.WarehouseID = " + html.WarehouseID + " order by c.Commune",
                            SQL=@"select c.CommuneID, c.Commune, d.DistrictID, d.District, p.ProvinceCity, p.ProvinceCityID, 0 OrderBy
from dbo.TblCommune c
inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID
inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + @"
union
select c.CommuneID, c.CommuneKH Commune, d.DistrictID, d.DistrictKH District, p.ProvinceCityKH ProvinceCity, p.ProvinceCityID, 1 OrderBy
from dbo.TblCommune c
inner join dbo.TblDistrict d on d.DistrictID = c.DistrictID
inner join dbo.TblProvinceCity p on p.ProvinceCityID = d.ProvinceCityID
where c.SystemID = " + html.SystemID + " and c.WarehouseID = " + html.WarehouseID + @"
order by c.Commune, OrderBy",
                            DisplayOnFields = "District, ProvinceCity",
                            isTypeahead = true
                        },
                        new HtmlTextBox()
                        {
                            Label="District:", ID="District",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="District"
                        },
                        new HtmlTextBox()
                        {
                            Label="Province/City:", ID="ProvinceCity",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="ProvinceCity"
                        },
                    },
                },
                    new  FormData() {
                    htmlControl = new HTMLControl[]
                    {
                        new HtmlTextBox()
                        {
                            Label="Map URL:", ID="GoogleMapFullUrl",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMapFullUrl", //Hide = true
                            DivClassCol="col-sm-auto", Type="url"
                        },
                        new HtmlTextBox()
                        {
                            Label="Set Map:", ID="GoogleMap_FullAddress",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMap_FullAddress",
                            IsSetLocationWithGoogleMap=true, InputLatitude="GoogleMap_Latitude",
                            InputLongitude="GoogleMap_Longitude"
                        },
                        new HtmlTextBox()
                        {
                            Label="Latitude:", ID="GoogleMap_Latitude",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMap_Latitude", //Hide = true
                            DivClassCol="col-sm-auto"
                        },
                        new HtmlTextBox()
                        {
                            Label="Longitude:", ID="GoogleMap_Longitude",
                            Class="input2 CustInfo", IsCustomForm = true,
                            DBField="GoogleMap_Longitude", //Hide = true
                            DivClassCol="col-sm-auto"
                        },
                    }
                },
            };
            ViewBag.DrawHTMLForm = html.DrawHTMLFormV1();
            return View();
        }
    }
}