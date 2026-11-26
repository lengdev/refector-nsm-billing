using System.Text;

namespace MVC
{
  public class HtmlFile : HTMLControl
  {
    public HtmlFile()
    {
      ISFile = true;
    }
    public override string GetHtml()
    {
      //Common common = new Common();
      StringBuilder stb_result = new StringBuilder();
      string AcceptImage = "";
      string IsHide = "";
      if (Hide)
      {
        IsHide = " style=\"display:none\"";
      }
      ID = ID == null ? "" : ID;
      Name = Name == "" ? ID : Name;
      if (AcceptImageOnly) AcceptImage = " accept=\"image/*\"";//" accept=\".png, .jpg, .jpeg\"";
      if (!IsCustomForm)
      {
        if (!UseTable) stb_result.Append("<tr" + IsHide + ">");

        stb_result.Append("<td" + IsHide + " align=\"" + LabelAlign + "\" valign=\"" + LabelValign + "\"><label class=\"col-form-label text-nowrap\">" + Label + "</label></td>");
        if (Multiple)
        {
          stb_result.Append("<td" + IsHide + " valign=\"" + LabelValign + "\"><input type=\"hidden\" name=\"hidden" + Name + "\" id=\"hidden" + ID + "\" value=\"" + Value + "\"/>");
          if (!IsDragDropFile)
            stb_result.AppendLine("<input type=\"file\" multiple name=\"" + Name + "\" id=\"" + ID + "\" class=\"jfilestyle " + Class + "\" value=\"" + Value + "\" " + AcceptImage + "/>");
          else
          {
            stb_result.AppendLine("<div class=\"container\">");
            stb_result.AppendLine("<div class=\"file-loading\">");
            stb_result.AppendLine("<input type=\"file\" multiple name=\"" + Name + "\" id=\"" + ID + "\" class=\"file " + Class + "\" value=\"" + Value + "\" " + AcceptImage + "/>");
            stb_result.Append("</div></div>");
          }
          stb_result.Append("</td>");
        }
        else
        {
          stb_result.Append("<td" + IsHide + " valign=\"" + LabelValign + "\"><input type=\"hidden\" name=\"hidden" + Name + "\" id=\"hidden" + ID + "\" value=\"" + Value + "\"/>");
          if (!IsDragDropFile)
            stb_result.AppendLine("<input type=\"file\" name=\"" + Name + "\" id=\"" + ID + "\" class=\"jfilestyle " + Class + "\" value=\"" + Value + "\" " + AcceptImage + "/>");
          else
          {
            stb_result.AppendLine("<div class=\"container\">");
            stb_result.AppendLine("<div class=\"file-loading\">");
            stb_result.AppendLine("<input type=\"file\" name=\"" + Name + "\" id=\"" + ID + "\" class=\"file " + Class + "\" value=\"" + Value + "\" " + AcceptImage + "/>");
            stb_result.Append("</div></div>");
          }
          stb_result.Append("</td>");
        }
        //stb_result.AppendLine("<td"+ IsHide + " width=\"22\"><label name=\"LCheckTextBox\" id=\"L" + ID.Replace("Txt", "") + "\" for=\"" + ID + "\"></label></td>");
        if (!UseTable) stb_result.Append("</tr>");
      }
      else
      {
        if (DivClass != "")
          DivClass = "";//wrap-input2
        if (Label != "")
        {
          stb_result.AppendLine("<div " + IsHide + " class=\"col-sm-auto\">");
          stb_result.AppendLine("<label for=\"" + ID + "\" class=\"col-form-label khmerFont1\">" + Label + "</label></div>");
        }
        stb_result.AppendLine("<div class=\"" + DivClassCol + " align-text-bottom\">");
        stb_result.AppendLine("<div class=\"" + DivClass + " " + (Required == true ? "validate-input" : "") + (Required == true && Value == "" ? " alert-validate" : "") + "\" " + (Required == true ? "data-validate=\"" + Label + " is required\"" : "") + ">");
        if (Multiple)
        {
          if (!IsDragDropFile)
            stb_result.AppendLine("<input type=\"file\" multiple name=\"" + Name + "\" id=\"" + ID + "\" class=\"jfilestyle " + Class + "\" value=\"" + Value + "\" " + AcceptImage + "/>");
          else
          {
            stb_result.AppendLine("<div class=\"container\">");
            stb_result.AppendLine("<div class=\"file-loading\">");
            stb_result.AppendLine("<input type=\"file\" multiple name=\"" + Name + "\" id=\"" + ID + "\" class=\"file " + Class + "\" value=\"" + Value + "\" " + AcceptImage + "/>");
            stb_result.Append("</div></div>");
          }
        }
        else
        {
          if (!IsDragDropFile)
            stb_result.AppendLine("<input type=\"file\" name=\"" + Name + "\" id=\"" + ID + "\" class=\"jfilestyle " + Class + "\" value=\"" + Value + "\" " + AcceptImage + "/>");
          else
          {
            stb_result.AppendLine("<div class=\"container\">");
            stb_result.AppendLine("<div class=\"file-loading\">");
            stb_result.AppendLine("<input type=\"file\" name=\"" + Name + "\" id=\"" + ID + "\" class=\"file " + Class + "\" value=\"" + Value + "\" " + AcceptImage + "/>");
            stb_result.Append("</div></div>");
          }
        }
        stb_result.AppendLine("<span class=\"focus-input2\"></span>");
        //stb_result.AppendLine("<input type=\"file\" name=\"" + Name + "\" id=\"" + ID + "\" value=\"" + Value + "\" placeholder=\"" + placeholder + "\" class=\"" + Class + "\"><span class=\"focus-input2\"></span>");

        stb_result.AppendLine("</div></div>");
      }
      stb_result.AppendLine(@"<script>
window.nsmCompressImageFile = window.nsmCompressImageFile || function (file) {
    var supportedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!file || supportedTypes.indexOf(file.type) === -1) return Promise.resolve(file);

    return new Promise(function (resolve) {
        var image = new Image();
        var objectUrl = URL.createObjectURL(file);

        image.onload = function () {
            URL.revokeObjectURL(objectUrl);

            var maxDimension = 1920;
            var largestSide = Math.max(image.naturalWidth, image.naturalHeight);
            var scale = Math.min(1, maxDimension / largestSide);
            if (scale === 1 && file.size <= 1024 * 1024) {
                resolve(file);
                return;
            }

            var canvas = document.createElement('canvas');
            canvas.width = Math.round(image.naturalWidth * scale);
            canvas.height = Math.round(image.naturalHeight * scale);
            canvas.getContext('2d').drawImage(image, 0, 0, canvas.width, canvas.height);
            canvas.toBlob(function (blob) {
                if (!blob || blob.size >= file.size) {
                    resolve(file);
                    return;
                }

                resolve(new File([blob], file.name, {
                    type: blob.type || file.type,
                    lastModified: file.lastModified
                }));
            }, file.type, file.type === 'image/png' ? undefined : 0.82);
        };

        image.onerror = function () {
            URL.revokeObjectURL(objectUrl);
            resolve(file);
        };
        image.src = objectUrl;
    });
};
</script>");

      if (!UploadFilesV2)
      {
        if (!Hide && !IsDragDropFile)
        {
          stb_result.AppendLine("<script>");
          stb_result.AppendLine("$(function(){");
          stb_result.AppendLine("$(\"input[name=" + ID + "]\").jfilestyle({");
          //stb_result.AppendLine("image: \""+ common.ServerRoot+ "/Content/images/choose-file.gif\",");
          //stb_result.AppendLine("imageheight : 26,");
          //stb_result.AppendLine("imagewidth : 82,");
          //stb_result.AppendLine("width : 200,");
          //stb_result.AppendLine("value : \"" + Value + "\",");
          stb_result.AppendLine("input : false");
          stb_result.AppendLine("});");

          //stb_result.AppendLine(string.Concat("$(\"#", base.ID, "\").val(\"" + Value + "\")"));
          stb_result.AppendLine("});");
          stb_result.AppendLine("</script>");
        }
        else
        {
          stb_result.AppendLine("<script>");
          stb_result.AppendLine("$(function(){");
          stb_result.AppendLine("$(\"input[name=" + (ID == "" ? Name : ID) + "]\").fileinput({");
          if (ShowDropZoneEnabled)
          {
            stb_result.AppendLine("theme: 'fa5', deleteUrl: \"/site/file-delete\",");
            stb_result.AppendLine("initialPreviewFileType:'image', initialPreviewShowDelete: true,fileActionSettings: {showRemove: true, showDrag: true},");
          }
          if (!ShowDropZoneEnabled)
          {
            stb_result.AppendLine("theme: 'fa5', showCaption: false,showPreview: false, inputGroupClass: \"input-group-sm\", dropZoneEnabled: false,");
          }
          else if (Value != "")
          {
            string[] str = Value.Split('~');
            HTML html = new HTML();
            string url = "";
            foreach (string str2 in str)
            {
              if (url == "")
                url = "'" + html.ServerRoot + DestinationFile + "/" + str2 + "'";
              else
                url += ",'" + html.ServerRoot + DestinationFile + "/" + str2 + "'";
            }
            stb_result.AppendLine("initialPreview:[" + url + "],");
            //stb_result.AppendLine("deleteUrl:'" + html.ServerRoot + "AddUserLogin/DeleteSignature',");
            stb_result.AppendLine("initialPreviewAsData:true,");
            stb_result.AppendLine("initialPreviewShowDelete: false,");
            stb_result.AppendLine("showRotate: false,");
            stb_result.AppendLine("fileActionSettings: {");
            stb_result.AppendLine("showRotate: false,");
            stb_result.AppendLine("},");
            //stb_result.AppendLine("autoReplace: true,");
            //stb_result.AppendLine("showClose: false,");
            //stb_result.AppendLine("showRemove: false,");

          }
          if (AcceptImageOnly)
          {
            //stb_result.AppendLine("allowedFileExtensions: ['jpg', 'png', 'gif'],");
            stb_result.AppendLine("allowedFileTypes: ['image'],");
            stb_result.AppendLine("showCancel: false,");
            stb_result.AppendLine("showUpload:false,");
          }
          stb_result.AppendLine("});");
          stb_result.AppendLine("$(\"input[name=" + (ID == "" ? Name : ID) + "]\").on('change', function (event) {");
          stb_result.AppendLine("var input = event.currentTarget;");
          stb_result.AppendLine("var selectedFiles = Array.prototype.slice.call(input.files);");
          stb_result.AppendLine("Promise.all(selectedFiles.map(function (file, index) {");
          stb_result.AppendLine("return window.nsmCompressImageFile(file).then(function (preparedFile) {");
          stb_result.AppendLine("var extensionIndex = file.name.lastIndexOf('.');");
          stb_result.AppendLine("var hasExtension = extensionIndex > 0;");
          stb_result.AppendLine("var extension = hasExtension ? file.name.substring(extensionIndex) : '';");
          stb_result.AppendLine("var baseName = hasExtension ? file.name.substring(0, extensionIndex) : file.name;");
          stb_result.AppendLine("var safeName = baseName.replace(/[^\\w]/gi, '') || 'image';");
          stb_result.AppendLine("var newName = safeName + '_' + Date.now() + '_' + index + extension;");
          stb_result.AppendLine("return new File([preparedFile], newName, { type: preparedFile.type || file.type, lastModified: file.lastModified });");
          stb_result.AppendLine("});");
          stb_result.AppendLine("})).then(function (preparedFiles) {");
          stb_result.AppendLine("var dataTransfer = new DataTransfer();");
          stb_result.AppendLine("preparedFiles.forEach(function (file) { dataTransfer.items.add(file); });");
          stb_result.AppendLine("input.files = dataTransfer.files;");
          stb_result.AppendLine("});");
          //stb_result.AppendLine("$(\"input[name=" + (ID == "" ? Name : ID) + "]\").fileinput('refresh', { showDrag: true });");
          stb_result.AppendLine("});");
          if (Value != "")
          {
            stb_result.AppendLine("let value = '" + Value + "';");
            stb_result.AppendLine("let filenames = value.split(\"~\");");
            stb_result.AppendLine("let dT = new DataTransfer();");
            stb_result.AppendLine("for (i = 0; i < filenames.length; i++) {");
            stb_result.AppendLine("if (filenames[i].trim() != \"\") {");
            stb_result.AppendLine("let file = ServerRoot + '" + DestinationFile + "' + \"/\" + filenames[i].trim();");
            stb_result.AppendLine("let f = getFileFromUrl(file, filenames[i].trim());");
            stb_result.AppendLine("Promise.all([f]).then((responses) => {");
            stb_result.AppendLine("let newFile = responses[0];");
            stb_result.AppendLine("dT.items.add(newFile);");
            stb_result.AppendLine("$(\"input[name=" + (ID == "" ? Name : ID) + "]\").prop('files', dT.files);");
            stb_result.AppendLine("});");
            stb_result.AppendLine("}}");
          }
          stb_result.AppendLine("});");
          stb_result.AppendLine("</script>");
        }
      }
      else
      {
        string str = @"<script>
GetdTData = new DataTransfer(); var filefromUrl="""";
var AllowOnlyImage = """ + (AcceptImageOnly == true ? "True" : "False") + @";"";
$(function(){";
        if (Value != "")
        {
          string[] strfile = Value.Split('~');
          //str += "let dT = new DataTransfer();";
          foreach (string str2 in strfile)
          {
            str += " filefromUrl = getFileFromUrl(ServerRoot+\"" + DestinationFile + "/" + str2.Trim() + "\", \"" + str2.Trim() + "\");";
            str += @"Promise.all([filefromUrl]).then((responses) => {
    let newFile = responses[0];
    GetdTData.items.add(newFile);
});";
            //str += " GetdTData.items.add(new File([ServerRoot+\"" + DestinationFile + "/" + str2 + "\"],\"" + str2 + "\"));";
          }
        }
        str += @"
if (AllowOnlyImage == ""True"")
            $('input[name=" + (ID == "" ? Name : ID) + @"]').attr(""accept"", ""image/*"");

        AllowOnlyImage = ""True"";
$('input[name=" + (ID == "" ? Name : ID) + @"]').imageuploadify();
});
</script>";
        stb_result.AppendLine(str);
      }
      return stb_result.ToString();
    }
  }
}
