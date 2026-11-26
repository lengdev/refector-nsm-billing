//var ServerRoot = location.protocol + '//' + location.host + "/";//window.location.origin + "/";
var UserProfilePicture = "";
var SaveChangeResponse = [];
var TimeOut = "";
var PageSize = 30;
var RowSeleted = -1;
var ImgData_PDFData_SendMail = {};
var GoogleMap = {
  IsSaved: false,
  GoogleMap_FullAddress: "",
  GoogleMap_Latitude: 0,
  GoogleMap_Longitude: 0,
};
var Token_Info = {};
var access_Token = "";
var User_ID = "";
var System_ID = "";
var Warehouse_ID = "";
var Domain_ID = "";
var User_Name = "";
var User_Full_Name = "";
var User_Poistion = "";
var IsValidSession = false;
var TokenExpires = "";
var TimeTokenExpires = "";
var headerPara = {};
var ShowToastLoading = "";
var FilesUpload = new Array();

toastr.options = {
  closeButton: true,
  debug: false,
  newestOnTop: false,
  progressBar: true,
  positionClass: "toast-top-right",
  preventDuplicates: false,
  onclick: null,
  showDuration: "300",
  hideDuration: "1000",
  timeOut: "5000",
  extendedTimeOut: "1000",
  showEasing: "swing",
  hideEasing: "linear",
  showMethod: "fadeIn",
  hideMethod: "fadeOut",
};

function UpdateGlobalAuthState(userInfo) {
  access_Token = userInfo.accessToken;
  let sysandwarehouseinfo = JSON.parse(userInfo.SysAndWH)[0];
  User_ID = sysandwarehouseinfo.UserID;
  User_Name = userInfo.userName;
  User_Full_Name = userInfo.UserFullName;
  UserProfilePicture = userInfo.UserProfilePicture;
  User_Poistion = userInfo.Poistion;
  headerPara = {
    UserID: User_ID,
    UserName: User_Name,
    SystemID: System_ID,
    WarehouseID: Warehouse_ID,
    UserFullName: User_Full_Name,
    DomainID: Domain_ID,
    UserPosition: User_Poistion,
    SystemLogo: getStorage("SysWLogo"),
  };
}

function locationHashChanged() {
  //if (location.hash === "#cool-feature") {
  //console.log(window.location.href);
  //}
}

window.onhashchange = locationHashChanged;
function prepareUpload(event) {
  if (event.isDefaultPrevented()) return;
  event.preventDefault();
  FilesUpload.push(event.target.files);
  //console.log(FilesUpload);
}
// removes a given file name from the list of attempted uploads
function removeAttemptedUpload(fileName) {
  $.each(FilesUpload, function (key, value) {
    $.each(value, function (ks, vs) {
      $.each(fileName, function (k, val) {
        if (val.name == vs.name) {
          FilesUpload.splice(key, 1);
        }
      });
    });
  });
}
function LoadData(endPointName, Callback, asyncData, dataType = "html") {
  $.ajax({
    url: ServerRoot + endPointName,
    dataType: dataType,
    async:
      typeof asyncData === "undefined" || asyncData === "" ? false : asyncData,
    cache: true,
    type: "GET",
    headers: { Authorization: "Bearer " + access_Token },
    beforeSend: function (xhr) {
      xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
    },
    success: function (response) {
      $("button").removeAttr("disabled");
      return Callback(response);
    },
    error: function (response, callStatus, errorThrown) {
      $("button").removeAttr("disabled");
      DialogSweetAlert("error", response.status + " " + response.statusText);
      return Callback(response.status + " " + response.statusText);
    },
  });
}
function PostDataV1(
  endPointName,
  DATA,
  CallBackFunction,
  asyncData,
  showSaving = false,
) {
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  asyncData = typeof asyncData == "undefined" ? true : asyncData;
  $.ajax({
    url: ServerRoot + endPointName,
    headers: { Authorization: "Bearer " + access_Token },
    //contentType: "application/json; charset=utf-8",
    dataType: "html",
    async: asyncData,
    cache: false,
    type: "POST",
    data: DATA,
    beforeSend: function (xhr) {
      if (showSaving) {
        DialogSweetAlert("load", "Saving...");
        $("button").attr("disabled", true);
      }
      xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
    },
    success: function (response) {
      $("button").removeAttr("disabled");
      var result = safeJsonParse(response);

      if (result && result.Result == 1) {
        setTimeout(function () {
          DialogSweetAlert("error", result.Message);
        }, 1000);
      }
    },
    error: function (response, callStatus, errorThrown) {
      $("button").removeAttr("disabled");
      DialogSweetAlert("error", response.status + " " + response.statusText);
    },
  }).done(function (response) {
    if (typeof CallBackFunction === "function") {
      CallBackFunction(response);
    }
  });
}

function PostData(
    endPointName,
    DATA,
    CallBackFunction,
    asyncData = true,
    showSaving = false,
    dataType = "html",
    jsonData = false
) {
    const isFormData = DATA instanceof FormData;

    $.ajax({
        url: ServerRoot + endPointName,
        headers: {
            Authorization: "Bearer " + access_Token
        },
        dataType: dataType,
        async: asyncData,
        cache: false,
        type: "POST",

        data: jsonData && !isFormData
            ? JSON.stringify(DATA)
            : DATA,

        processData: !isFormData && !jsonData,

        contentType: isFormData
            ? false
            : jsonData
                ? "application/json; charset=utf-8"
                : "application/x-www-form-urlencoded; charset=UTF-8",

        beforeSend: function (xhr) {
            if (showSaving) {
                DialogSweetAlert("load", "Saving...");
                $("button").attr("disabled", true);
            }

            xhr.setRequestHeader(
                "x-custom-header",
                JSON.stringify(headerPara)
            );
        },

        success: function (response) {
            $("button").removeAttr("disabled");

            var result = safeJsonParse(response);

            if (result && result.Result == 1) {
                setTimeout(function () {
                    DialogSweetAlert("error", result.Message);
                }, 1000);
            }
        },

        error: function (response) {
            $("button").removeAttr("disabled");
            DialogSweetAlert(
                "error",
                response.status + " " + response.statusText
            );
        }
    }).done(function (response) {
        if (typeof CallBackFunction === "function") {
            CallBackFunction(response);
        }
    });
}

function safeJsonParse(str) {
  try {
    return JSON.parse(str);
  } catch (e) {
    return null; // not valid JSON
  }
}

function HTMLDataToPDF(
  HTMLData,
  FileName,
  CallBackFunction,
  ReturnUrlImg,
  scale = 1,
) {
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  ReturnUrlImg = typeof ReturnUrlImg == "undefined" ? "False" : "True";

  Swal.fire({
    title: "Exporting...",
    html: '<div class="swal-inline-spinner"></div>',
    showConfirmButton: false,
    allowOutsideClick: false,
    didOpen: () => {
      setTimeout(function () {
        $.ajax({
          url: ServerRoot + "Print/ConvertToPDF",
          headers: { Authorization: "Bearer " + access_Token },
          dataType: "html",
          async: true,
          cache: false,
          type: "POST",
          data: {
            HTMLData: HTMLData,
            FileName: FileName,
            ReturnUrlImg: ReturnUrlImg,
            Title: FileName,
            Scale: scale,
          },
          beforeSend: function (xhr) {
            xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
          },
          success: function (response) {
            $("button").removeAttr("disabled");
          },
          error: function (response, callStatus, errorThrown) {
            $("button").removeAttr("disabled");
            DialogSweetAlert(
              "error",
              response.status + " " + response.statusText,
            );
          },
        }).done(function (response) {
          Swal.close();
          if (typeof CallBackFunction === "function") {
            CallBackFunction(response);
          }
        });
      }, 100);
    },
  });
}
function LoadDataToComboBox(
  DIV,
  CboID,
  SQLStatement,
  Value,
  DisplayText,
  Event,
  ShowAll,
  SetSelectedValue,
  Encrypt,
  ReadOnly,
  IsAppend,
  IsUseConnection2,
  ConditionFielVale,
  ISCbo,
  ISMultiple,
  ISChosenStyle,
  IsCallBackToComboBox,
  ISComboboxCallback,
) {
  ShowAll = typeof ShowAll == "undefined" ? "false" : ShowAll;
  SetSelectedValue =
    typeof SetSelectedValue == "undefined" ? "" : SetSelectedValue;
  Event = typeof Event == "undefined" ? "LoadTableGrid(1)" : Event;
  Encrypt = typeof Encrypt == "undefined" ? "true" : Encrypt;
  ReadOnly = typeof ReadOnly == "undefined" ? "false" : ReadOnly;
  IsAppend = typeof IsAppend == "undefined" ? "false" : IsAppend;
  IsUseConnection2 =
    typeof IsUseConnection2 == "undefined" ? "false" : IsUseConnection2;
  ConditionFielVale =
    typeof IsUseConnection2 == "undefined" ? "" : ConditionFielVale;
  IsCallBackToComboBox =
    typeof IsCallBackToComboBox == "undefined" ? "" : IsCallBackToComboBox;
  if (ConditionFielVale != "") {
    $("#" + ConditionFielVale).val();
  }
  ISCbo = typeof ISCbo == "undefined" ? true : ISCbo;
  ISMultiple = typeof ISMultiple == "undefined" ? "false" : ISMultiple;
  ISChosenStyle = typeof ISChosenStyle == "undefined" ? "true" : ISChosenStyle;
  ISComboboxCallback =
    typeof ISComboboxCallback == "undefined" ? "false" : ISComboboxCallback;

  $("div#" + DIV).html(
    '<select id="' +
      CboID +
      '" class="form-control">' +
      '<option value="">-- All --</option>' +
      "</select>",
  );
  $("#" + CboID).select2({
    theme: "bootstrap4",
    dropdownParent: $("#" + CboID).parent(),
  });

  var url = ServerRoot + "DoActionData/LoadDataToCBO";
  $.ajax({
    url: url,
    dataType: "html",
    cache: true,
    type: "GET",
    headers: { Authorization: "Bearer " + access_Token },
    data: {
      CboID: CboID,
      SQL: SQLStatement,
      Value: Value,
      DisplayText: DisplayText,
      Event: Event,
      ShowAll: ShowAll,
      Encrypt: Encrypt,
      ReadOnly: ReadOnly,
      IsAppend: IsAppend,
      IsUseConnection2: IsUseConnection2,
      ConditionFielVale: ConditionFielVale,
      Multiple: ISMultiple,
      IsCallBackToComboBox: IsCallBackToComboBox,
    },
    beforeSend: function (xhr) {
      xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
    },
    success: function (responseText) {
      if (DIV != "") {
        // destroy the placeholder select2 instance before replacing markup
        if ($("#" + CboID).hasClass("select2-hidden-accessible")) {
          $("#" + CboID).select2("destroy");
        }
        $("div#" + DIV).html(responseText);
        $("#" + CboID).select2({
          theme: "bootstrap4",
          dropdownParent: $("#" + CboID).parent(),
        });
        if (SetSelectedValue != "")
          $("#" + CboID)
            .val(SetSelectedValue)
            .trigger("change");
      }
    },
    error: function (response, callStatus, errorThrown) {
      $("button").removeAttr("disabled");
      DialogSweetAlert("error", response.status + " " + response.statusText);
    },
  });
}
function closeDialogClick(DialogID) {
  $("#" + DialogID + "").modal("hide");
  //$('.modal').remove();
  //$('.modal-backdrop').remove();
  //$('body').removeClass("modal-open");
}
function closeCurrentDialogClick(THIS, DialogID) {
  $(THIS).modal("hide");
  //$('.modal').remove();
  //$('.modal-backdrop').remove();
  //$('body').removeClass("modal-open");
}

function ShowDialogBox(
  Title,
  Width,
  Height,
  URi,
  PrimaryFieldValue,
  DialogID,
  DestroyDateTimePicker,
  ModalWidth,
  ModalHeight,
  CallbackFunction,
  btnCloseID,
  btnSaveID,
) {
  btnCloseID = typeof btnCloseID == "undefined" ? "CloseModal" : btnCloseID;
  btnSaveID = typeof btnSaveID == "undefined" ? "SaveChange" : btnSaveID;
  $(".modal").remove();
  $(".modal-backdrop").remove();
  $("body").removeClass("modal-open");
  Title = typeof Title == "undefined" ? "Entry Form" : Title;
  Width = typeof Width == "undefined" || Width == null ? "" : Width;
  Height = typeof Height == "undefined" || Height == null ? "" : Height;

  URi = typeof URi == "undefined" ? "" : URi;
  PrimaryFieldValue =
    typeof PrimaryFieldValue == "undefined" ? "0" : PrimaryFieldValue;
  DialogID = typeof DialogID == "undefined" ? "modal-default" : DialogID;
  ModalWidth = typeof ModalWidth == "undefined" ? "" : ModalWidth;
  ModalHeight = typeof ModalHeight == "undefined" ? "" : ModalHeight;
  CallbackFunction =
    typeof CallbackFunction == "undefined" ? "" : CallbackFunction;

  if (DialogID == "") DialogID = "modal-default";
  $("#" + DialogID).remove();
  $("#" + DialogID + "modalbody p").remove();
  $(".modal").remove();
  $(".modal-backdrop").remove();
  $("body").removeClass("modal-open");
  DestroyDateTimePicker =
    typeof DestroyDateTimePicker == "undefined" ? false : DestroyDateTimePicker;
  if (DestroyDateTimePicker == "") DestroyDateTimePicker = false;
  let modal = '<div class="modal fade" role="dialog" id="' + DialogID + '">'; //tabindex="-1"
  if (Width.toString().toLowerCase() == "Auto".toLowerCase())
    modal += '<div class="modal-dialog modal-fullscreen" role="dialog">'; //tabindex="-1"
  else modal += '<div class="modal-dialog" role="dialog">'; //tabindex="-1"
  modal += '<div class="modal-content">';
  modal += '<div class="card-default">';
  modal +=
    '<div class="card-header"><div class="row dm-sans"><div class="col"><i class="ti ti-edit"></i>&nbsp;' +
    Title +
    "</div>";

  if (Width.toString().toLowerCase() == "Auto".toLowerCase())
    modal +=
      '<div class="col-auto text-end"><button type="button" class="close" data-dismiss="modal" onclick="closeDialogClick(\'' +
      DialogID +
      '\');" aria-label="Close">';
  else
    modal +=
      '<div class=\"col-auto text-end\"><button type="button" class="close" data-dismiss="modal" aria-label="Close">';

  modal +=
    '<span aria-hidden="true">&times;</span></button></div></div></div></div>';
  modal +=
    '<div class="modal-body table-responsive" id="' + DialogID + 'modalbody">';
  modal += '<p style="padding:0 20px 0 0"></p>';
  modal += "</div>";
  modal += '<div class="modal-footer">';
  modal +=
    '<button type="button" id="' +
    btnSaveID +
    '" name="SaveChange" class="btn btn-outline-primary"><i class="ti ti-device-floppy"></i> Save Change</button>';
  modal +=
    '<button type="button" id="' +
    btnCloseID +
    '" name="CloseModal" class="btn btn-outline-danger btnclose" data-dismiss="modal"><i class="ti ti-x"></i> Close</button>';
  modal += "</div></div>";
  modal += "</div></div>";
  if ($("#" + DialogID).length == 0) $(document.body).append(modal);
  //$('.modal').draggable({
  //    handle: ".card-default"
  //});
  //$("#" + DialogID).modal({
  //    keyboard: false,
  //    backdrop: 'static',
  //    show: true
  //});
  //let url = new URL(window.location.href);
  let myModal = new bootstrap.Modal(document.getElementById(DialogID), {
    keyboard: false,
    backdrop: "static",
    focus: true,
  });
  myModal.show();
  $("#" + DialogID).on("shown.bs.modal", function () {
    $(this).find(".card-default").drags();
    if (
      typeof PrimaryFieldValue === "object" ||
      Array.isArray(PrimaryFieldValue)
    ) {
      PrimaryFieldValue = JSON.stringify(PrimaryFieldValue);
    }
    if (URi != "") {
      $.ajax({
        url: URi,
        data: {
          PrimaryFieldValue: PrimaryFieldValue,
        },
        headers: { Authorization: "Bearer " + access_Token },
        dataType: "html",
        async: true,
        cache: true,
        type: "GET",
        beforeSend: function (xhr) {
          xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
          $("button").attr("disabled", true);
        },
        success: function (responetext) {
          $("button").attr("disabled", false);
          $("#" + DialogID + "modalbody p").html(responetext);
          $("#CloseModal").click(function () {
            $("#" + DialogID + "").modal("hide");
          });
          if (Width != "") {
            if (Width.toString().toLowerCase() != "Auto".toLowerCase()) {
              $(".modal .modal-dialog").css({
                "min-width": Width + "px",
                "max-width": "fit-content",
              });
            }
          }
          if (Height != "") {
            if (Width.toString().toLowerCase() != "Auto".toLowerCase()) {
              $(".modal-body").css({
                height: Number(Height) ? Height + "px" : "auto",
                "overflow-y": "scroll",
              });
            }
          }
          //if (Width != "") {
          //    if (Width.toString().toLowerCase() == "Auto".toLowerCase()) {
          //        $(".modal .modal-dialog").css({
          //            "width": "auto",
          //            "height": "auto",
          //            "min-width": "300px",
          //            "max-width": "100%",
          //            "max-height": "100%"
          //        });
          //    }
          //    else {
          //        $(".modal .modal-dialog").css({
          //            "max-width": Width + "px"
          //        });
          //    }
          //}
          //if (Height != "") {
          //    if (Width.toString().toLowerCase() != "Auto".toLowerCase()) {
          //        $(".modal-body").css({
          //            "height": Height + "px",
          //            "overflow-y": "scroll"
          //        });
          //    }
          //}
          if (CallbackFunction != "") return CallbackFunction(responetext);
        },
        error: function (XMLHttpRequest, callStatus, errorThrown) {
          $("button").attr("disabled", false);
          $("#" + DialogID + "modalbody p").html(XMLHttpRequest.statusText);
          $(".modal").remove();
          $(".modal-backdrop").remove();
          $("body").removeClass("modal-open");
          DialogSweetAlert("error", XMLHttpRequest.statusText);
        },
      });
    }
  });

  $("#" + DialogID).on("hide.bs.modal", function () {
    $(this).data("bs.modal", null);
    $("#" + DialogID).remove();
    $(".modal").remove();
    $(".modal-backdrop").remove();
    $("body").removeClass("modal-open");
  });
  const userId = localStorage.getItem("User_ID");
  if (userId == 129) {
    $("#" + DialogID + "modalbody p").html(`
        <div class="section-loader">
			<video class="duck-loader" autoplay loop muted playsinline>
                <source src="../Uploads/duck-loading.webm" type="video/webm" style="width: 180px;" />
            </video>
        </div>
    `);
  } else {
    $("#" + DialogID + "modalbody p").html(
      '<div class="modal-loading-center">' +
        '<div class="modal-spinner"></div>' +
        '<div class="modal-loading-text">Loading...</div>' +
        "</div>",
    );
  }
}

function AddAttachmentFile(Title, ContolName, AllowOnlyImage) {
  Title =
    typeof Title == "undefined" ? "Add Attachment File to Customer" : Title;
  ContolName = typeof ContolName == "undefined" ? "" : ContolName;
  AllowOnlyImage =
    typeof AllowOnlyImage == "undefined" ? "False" : AllowOnlyImage;
  if (AllowOnlyImage == "") AllowOnlyImage = "False";
  var PrimaryFieldValue = JSON.stringify({
    ContolName: ContolName,
    AllowOnlyImage: AllowOnlyImage,
  });
  ShowDialogBox2(Title, 700, 500, "UploadFileForm", PrimaryFieldValue);
}
function isJson(str) {
  try {
    JSON.parse(str);
  } catch (e) {
    return false;
  }
  return isNaN(str);
}
var keepDataTypeahead = [];
function ApplyTypeahead_v1(
  ID,
  DataTypeahead,
  callbackfield,
  pushDatafield,
  UseNameInput,
  callback,
  AddNewFunctionName,
  isApplyTypeaheadOnCallBack,
  CallBackFunction,
  DisplayOnFields,
  Index,
) {
  DataTypeahead = typeof DataTypeahead == "undefined" ? "" : DataTypeahead;
  if (DataTypeahead == "") return;
  let selected = null;
  let _ID = ID;
  let originalVal = null;
  var PrimaryValue = "";
  var KeepThisValue = "";
  var KeepOldValue = "";
  var keepCallBackToControlIDValue = {};
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  UseNameInput = typeof UseNameInput == "undefined" ? "" : UseNameInput;
  callback = typeof callback == "undefined" ? "" : callback;
  AddNewFunctionName =
    typeof AddNewFunctionName == "undefined" ? "" : AddNewFunctionName;
  isApplyTypeaheadOnCallBack =
    typeof isApplyTypeaheadOnCallBack == "undefined"
      ? ""
      : isApplyTypeaheadOnCallBack;
  DisplayOnFields =
    typeof DisplayOnFields == "undefined" ? "" : DisplayOnFields;
  Index = typeof Index == "undefined" ? "" : Index;
  if (callback != "") {
    var CallBackToControlID =
      typeof callback.CallBackToControlID == "undefined"
        ? ""
        : callback.CallBackToControlID;
    if (CallBackToControlID != "") {
      var DFiled = CallBackToControlID.split(",");
      for (i = 0; i < DFiled.length; i++) {
        if ($("#" + DFiled[i].trim() + "").val() != "") {
          keepCallBackToControlIDValue[DFiled[i].trim()] = $(
            "#" + DFiled[i].trim() + "",
          ).val();
        }
      }
    }
  }
  if (UseNameInput == "") {
    if (typeof $(ID).val() != "undefined") {
      //console.log(ID);
    } else {
      ID = "#" + ID + "";
    }
  } else ID = "input[name=" + UseNameInput + "]";
  $(ID).typeahead("destroy");
  $(ID).unbind();
  $(ID)
    .typeahead(
      {
        hint: true,
        highlight: true,
        minLength: 0,
        //searchOnFocus: true
      },
      {
        limit: 9999,
        display: "Name",
        source: function (query, cb) {
          var matches = [];
          keepDataTypeahead[_ID] = DataTypeahead;
          $.each(DataTypeahead, function (i, str) {
            //Object.keys(str).map(key => str[key]);
            const value = str[pushDatafield.Name];
            const globalRegex = new RegExp(query, "\ig");
            if (globalRegex.test(value)) {
              var obj = new Object();
              $.each(pushDatafield, function (key, val) {
                obj[key] = decode(str[val]);
              });
              if (DisplayOnFields != "") {
                var objField = [];
                objField.push(DisplayOnFields);
                if (isJson(objField)) {
                  objField = JSON.parse(objField);
                  $.each(objField, function (key, val) {
                    obj[key] = decode(str[val]);
                  });
                } else {
                  $.each(objField, function (key, val) {
                    obj[val] = decode(str[val]);
                  });
                }
                matches.push(obj);
              } else matches.push(obj);
            }
          });
          cb(matches);
        },
        templates: {
          header:
            AddNewFunctionName == ""
              ? ""
              : '<div onclick="' +
                AddNewFunctionName +
                '" class="tt-suggestion tt-selectable"><a href="javascript:void(0);"><i class=\"fa fa-plus\"></i> Add New</a></div>',
        },
      },
    )
    .on("focus", function (aEvent) {
      if (aEvent.isDefaultPrevented()) return;
      aEvent.preventDefault();
      $(this).select();
    })
    .on("click", function (aEvent) {
      if (aEvent.isDefaultPrevented()) return;
      aEvent.preventDefault();
      $(this).typeahead("open");
    })
    .on("typeahead:change", function (aEvent, aSuggestion) {
      if (aEvent.isDefaultPrevented()) return;
      aEvent.preventDefault();
      if (keepDataTypeahead[_ID]) {
        DataTypeahead = keepDataTypeahead[_ID];
        let data = DataTypeahead.filter(
          (name) => decode(name[pushDatafield.Name]) === aSuggestion,
        );
        let index = $(ID).index(this);
        if (DisplayOnFields != "") {
          var objField = [];
          objField.push(DisplayOnFields);
          if (isJson(objField)) {
            objField = JSON.parse(objField);
            $.each(objField, function (key, val) {
              $("#" + key).typeahead("val", data[0][val]);
              let value =
                typeof data[0][val] == "undefined"
                  ? ""
                  : data[0][val].toString();
              $("#" + key)
                .val(value)
                .trigger("change");
            });
          } else {
            let objFields = objField.toString().split(",");
            if (objFields.length > 0) {
              if (data[0]) {
                for (i = 0; i < objFields.length; i++) {
                  let val = objFields[i].trim();
                  $("#" + val).typeahead("val", data[0][val]);
                  $("#" + val)
                    .val(data[0][val])
                    .trigger("change");
                }
              } else {
                for (i = 0; i < objFields.length; i++) {
                  let val = objFields[i].trim();
                  $("#" + val).typeahead("val", "");
                  $("#" + val)
                    .val("")
                    .trigger("change");
                }
              }
            } else {
              $.each(objField, function (key, val) {
                if (data[0]) {
                  $("#" + val).typeahead("val", data[0][val]);
                  $("#" + val)
                    .val(data[0][val])
                    .trigger("change");
                }
              });
            }
          }
        }
        KeepThisValue = $(this).val();
        originalVal = $(this).val();
        let isObj = isJQuery(callbackfield);
        if (isObj) {
          $.each(callbackfield, function (key, val) {
            if (UseNameInput == "") {
              $("#" + key + "").val("");
              if ($("#" + key).is("select")) {
                $("#" + key)
                  .val(0)
                  .trigger("change");
              }
            } else
              $("input[name=" + key + "]")
                .eq(index)
                .val("")
                .trigger("change");
            if (data[0]) {
              if (data[0][val]) {
                if (UseNameInput == "") {
                  if ($("#" + key).is("select")) {
                    let Values = new Array();
                    let item = data[0][val].split(",");
                    for (i = 0; i < item.length; i++) {
                      Values.push(item[i]);
                    }
                    //let $d = [item.map(item => "'" + item + "'").join()];
                    $("#" + key + "")
                      .val(Values)
                      .trigger("change");
                  } else
                    $("#" + key + "")
                      .val(data[0][val])
                      .trigger("change");
                } else
                  $("input[name=" + key + "]")
                    .eq(index)
                    .val(data[0][val])
                    .trigger("change");
              }
            } else {
              if (UseNameInput == "") {
                $("#" + key + "").val("");
                if ($("#" + key).is("select")) {
                  $("#" + key)
                    .val(0)
                    .trigger("change");
                }
              } else
                $("input[name=" + key + "]")
                  .eq(index)
                  .val("")
                  .trigger("change");
            }
          });
        } else {
          let applycallbackfield = callbackfield.split(",");
          if (applycallbackfield[0] == "" && DisplayOnFields != "")
            applycallbackfield = DisplayOnFields.split(",");
          if (data[0]) {
            for (i = 0; i < applycallbackfield.length; i++) {
              if (applycallbackfield[i].trim() != "") {
                //if ($("input[name=" + applycallbackfield[i].trim() + "]").eq(Index).val()) {
                if (
                  $("input[name=" + applycallbackfield[i].trim() + "]").eq(
                    Index,
                  )[0]
                ) {
                  $("input[name=" + applycallbackfield[i].trim() + "]")
                    .eq(Index)
                    .val(data[0][applycallbackfield[i].trim()])
                    .trigger("change");
                }
              }
            }
          } else {
            var a = DisplayOnFields.split(",");
            for (i = 0; i < a.length; i++) {
              if (a[i].trim() != "") {
                if ($("input[name=" + a[i].trim() + "]").eq(Index)[0])
                  $("input[name=" + a[i].trim() + "]")
                    .eq(Index)
                    .val("")
                    .trigger("change");
              }
            }
          }
        }
        if (UseNameInput == "") {
          $(ID).typeahead("val", KeepThisValue);
        } else {
          $(ID).eq(index).typeahead("val", KeepThisValue);
        }
      }
      if (callback != "") {
        var CallBackToControlID =
          typeof callback.CallBackToControlID == "undefined"
            ? ""
            : callback.CallBackToControlID;
        let keepCurrentValue = "";
        if (CallBackToControlID != "") {
          if (isApplyTypeaheadOnCallBack == "True") {
            $("#" + CallBackToControlID).unbind();
          }
          var SQLCallBack = callback.SQLCallBack;
          var FieldName = callback.FieldName;
          var Value = callback.Value;
          let CurrentValue = callback.CurrentValue;
          PrimaryValue = callback.PrimaryValue;
          Value = $("#" + Value + "").val();
          var LoadCallBackData = false;
          if (PrimaryValue != "0") {
            if (CurrentValue == $(this).val()) {
              keepCurrentValue = $(this).val();
              LoadCallBackData = true;
            } else LoadCallBackData = true;
          } else {
            $("#" + CallBackToControlID).val("");
            LoadCallBackData = true;
          }
          if (LoadCallBackData) {
            PostData(
              "DoActionData/ReturnSQLJson/",
              {
                SQL: SQLCallBack,
                FieldName: FieldName,
                Value: Value,
              },
              function (response) {
                let Data =
                  typeof JSON.parse(JSON.parse(response).Data)[0] == "undefined"
                    ? ""
                    : JSON.parse(JSON.parse(response).Data)[0];
                if (isApplyTypeaheadOnCallBack == "True") {
                  let callbackfield = [];
                  let pushDatafield = [];
                  let obj = {};
                  //obj["Name"] = Object.keys(Data)[1];
                  //obj["Name"] = (typeof Object.keys(Data)[1] == "undefined" ? Object.keys(Data)[0] : Object.keys(Data)[1]);

                  //let k = (typeof Object.keys(Data)[0] == "undefined" ? "" : Object.keys(Data)[0]);
                  //if (k) {
                  //    obj[CallBackToControlID + '_' + k] = k;
                  //    pushDatafield.push(obj);
                  //    obj = {};
                  //    k = Object.keys(Data)[0];
                  //    obj[CallBackToControlID + '_' + k] = Object.keys(Data)[0];//CallBackToControlID + '_' + Object.keys(Data)[0];
                  //    callbackfield.push(obj);
                  //    obj = {};
                  //    k = Object.keys(Data)[1];
                  //    obj[k] = Object.keys(Data)[1];
                  //    callbackfield.push(obj);
                  //    Data = (typeof JSON.parse(JSON.parse(response).Data) == "undefined" ? "" : JSON.parse(JSON.parse(response).Data));
                  //    if (PrimaryValue == "0") {
                  //        $("#" + CallBackToControlID).val('');
                  //        $("#" + CallBackToControlID).typeahead('val', '');
                  //    }
                  //}
                  var objpushDatafield = {},
                    objcallbackfield = {};
                  $.each(Data, function (k, value) {
                    objpushDatafield["Name"] =
                      typeof Object.keys(Data)[1] == "undefined"
                        ? Object.keys(Data)[0]
                        : Object.keys(Data)[1];
                    objpushDatafield[CallBackToControlID + "_" + k] = k;
                    objcallbackfield[CallBackToControlID + "_" + k] = k;
                  });
                  pushDatafield.push(objpushDatafield);
                  callbackfield.push(objcallbackfield);
                  Data =
                    typeof JSON.parse(JSON.parse(response).Data) == "undefined"
                      ? ""
                      : JSON.parse(JSON.parse(response).Data);
                  if (PrimaryValue == "0") {
                    $("#" + CallBackToControlID).val("");
                    $("#" + CallBackToControlID).typeahead("val", "");
                  }
                  ApplyTypeahead(
                    CallBackToControlID,
                    Data,
                    callbackfield[0],
                    pushDatafield[0],
                  );
                } else {
                  if (Data != "") {
                    Data = Object.values(Data);
                    var DFiled = CallBackToControlID.split(",");
                    for (i = 0; i < DFiled.length; i++) {
                      $("#" + DFiled[i].trim() + "").val(Data[i]);
                    }
                    if (PrimaryValue != "0") {
                      if (CurrentValue == KeepThisValue) {
                        //$(this).val(KeepOldValue);
                        LoadCallBackData = false;
                        if (
                          Object.keys(
                            keepCallBackToControlIDValue,
                          ).toString() != ""
                        )
                          $(
                            "#" +
                              Object.keys(
                                keepCallBackToControlIDValue,
                              ).toString(),
                          ).val(Object.values(keepCallBackToControlIDValue));
                      }
                    }
                  } else {
                    var DFiled = CallBackToControlID.split(",");
                    for (i = 0; i < DFiled.length; i++) {
                      $("#" + DFiled[i].trim() + "").val("");
                    }
                  }
                }

                if (PrimaryValue != "0") {
                  if (CurrentValue == keepCurrentValue) {
                    if (
                      Object.keys(keepCallBackToControlIDValue).toString() != ""
                    )
                      $(
                        "#" +
                          Object.keys(keepCallBackToControlIDValue).toString(),
                      ).val(Object.values(keepCallBackToControlIDValue));
                    var DFiled = CallBackToControlID.split(",");
                    for (i = 0; i < DFiled.length; i++) {
                      $("#" + DFiled[i].trim() + "").val(
                        keepCallBackToControlIDValue[DFiled[i].trim()],
                      );
                    }
                  }
                }
              },
              false,
            );
          }
        }
      }
      setTimeout(function () {
        if (CallBackFunction != "") {
          var fn = window[CallBackFunction];
          if (typeof fn != "undefined") {
            if (typeof fn === "function") {
              return fn();
            } else {
              let fn = new Function(CallBackFunction);
              fn.call(this);
            }
          } else {
            return CallBackFunction();
          }
        }
      }, 50);
    })
    .on("typeahead:active", function (aEvent) {
      if (aEvent.isDefaultPrevented()) return;
      aEvent.preventDefault();
      selected = null;
      if ($(this).typeahead("val") != "")
        originalVal = $(this).typeahead("val");
      if (originalVal == null) {
        originalVal = $(this).val();
        $(this).typeahead("val", originalVal);
      }
    })
    .on("typeahead:selected", function (obj, data) {
      //$(this).trigger("change");
      //console.log(data);
      //$(this).trigger('typeahead:selected', data);
      //console.log(data);
      //.bind("typeahead:selected", function (obj, data, name) {
    });
  $(ID).on("typeahead:select", function (aEvent, aSuggestion) {
    if (aEvent.isDefaultPrevented()) return;
    aEvent.preventDefault();
    selected = aSuggestion;
    //console.log(aSuggestion);
    $(this).trigger("typeahead:selected", selected);
  });
  //// When the typeahead becomes active reset these values.
  //$(ID).on("typeahead:active", function (aEvent) {
  //    if (aEvent.isDefaultPrevented()) return;
  //    aEvent.preventDefault();
  //    selected = null;
  //    //if ($(this).typeahead("val") != "")
  //    //    originalVal = $(this).typeahead("val");
  //});
  //$(ID).on("typeahead:close", function (aEvent, aSuggestion) {
  //    if (aEvent.isDefaultPrevented()) return;
  //    aEvent.preventDefault();
  //    if (!selected) {
  //        $(this).typeahead("val", originalVal);
  //    }

  //    // Do something with the selected value here as needed...
  //});

  //$(ID).on("click", function () {
  //    if (KeepThisValue == "")
  //        KeepThisValue = $(this).val();
  //    var index = $(ID).index(this);
  //    if (UseNameInput == "") {
  //        //$(ID).typeahead("destroy");
  //        var thisval = $(this).val();
  //        $(ID).typeahead('val', '');
  //        $(ID).typeahead("open");
  //    }
  //    else {
  //        KeepThisValue = "";
  //        //index = $(this).closest("tr").index();
  //        //$(this).eq(index).typeahead("destroy");
  //        //$(this).typeahead('val', '');
  //        $(this).typeahead("open");
  //    }
  //    //$(this).val(KeepThisValue).select();

  //});
  //$(ID).on("change", function () {
  //    KeepThisValue = $(this).val();
  //    $(this).typeahead('val', $(this).val());
  //});
  //$(ID).on("blur", function () {
  //    //if ($(this).typeahead('val') != "")
  //    //    KeepThisValue = $(this).typeahead('val');
  //    if ($(this).val() != "")
  //        KeepThisValue = $(this).val();
  //    var index = $(ID).index(this);
  //    if (UseNameInput == "") {
  //        $(ID).typeahead('val', KeepThisValue);
  //    }
  //    else {
  //        index = $(this).closest("tr").index();
  //        $(ID).eq(index).typeahead('val', KeepThisValue);
  //    }
  //    //$(this).typeahead("destroy");
  //});
}
function ApplyTypeahead(
  ID,
  DataTypeahead,
  callbackfield,
  pushDatafield,
  UseNameInput,
  callback,
  AddNewFunctionName,
  isApplyTypeaheadOnCallBack,
  CallBackFunction,
  DisplayOnFields,
  Index,
  ShowHint = true,
  MinLength = 0,
) {
  // Initial Guards and Defaults
  DataTypeahead = typeof DataTypeahead == "undefined" ? "" : DataTypeahead;
  if (DataTypeahead == "") return;

  let _ID = ID;
  let selected = null;
  let originalVal = null;
  let KeepThisValue = "";

  // Normalize parameters
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  UseNameInput = typeof UseNameInput == "undefined" ? "" : UseNameInput;
  callback = typeof callback == "undefined" ? "" : callback;
  AddNewFunctionName =
    typeof AddNewFunctionName == "undefined" ? "" : AddNewFunctionName;
  isApplyTypeaheadOnCallBack =
    typeof isApplyTypeaheadOnCallBack == "undefined"
      ? ""
      : isApplyTypeaheadOnCallBack;
  DisplayOnFields =
    typeof DisplayOnFields == "undefined" ? "" : DisplayOnFields;
  Index = typeof Index == "undefined" ? "" : Index;

  // Identify Element
  let $el;
  if (UseNameInput == "") {
    ID = typeof $(ID).val() != "undefined" ? ID : "#" + ID;
    $el = $(ID);
  } else {
    ID = "input[name=" + UseNameInput + "]";
    $el = $(ID);
  }

  // Initialize Typeahead
  $el.typeahead("destroy").unbind();
  $el.typeahead(
    {
      //hint: ShowHint,
      highlight: true,
      minLength: MinLength,
    },
    {
      limit: 9999,
      display: "Name",
      source: function (query, cb) {
        var matches = [];
        keepDataTypeahead[_ID] = DataTypeahead;
        $.each(DataTypeahead, function (i, str) {
          const value = str[pushDatafield.Name];
          const globalRegex = new RegExp(query, "ig");
          if (globalRegex.test(value)) {
            var obj = new Object();
            $.each(pushDatafield, function (key, val) {
              obj[key] = decode(str[val]);
            });

            if (DisplayOnFields != "") {
              var objField = [DisplayOnFields];
              if (isJson(objField)) {
                var parsedFields = JSON.parse(objField);
                $.each(parsedFields, function (key, val) {
                  obj[key] = decode(str[val]);
                });
              } else {
                $.each(objField, function (key, val) {
                  obj[val] = decode(str[val]);
                });
              }
            }
            matches.push(obj);
          }
        });
        cb(matches);
      },
      templates: {
        header:
          AddNewFunctionName == ""
            ? ""
            : '<div onclick="' +
              AddNewFunctionName +
              '" class="tt-suggestion tt-selectable"><a href="javascript:void(0);"><i class="fa fa-plus"></i> Add New</a></div>',
      },
    },
  );

  // Unified Action Handler
  $el
    .on("focus", function () {
      $(this).select();
    })
    .on("click", function () {
      $(this).typeahead("open");
    })
    .on("typeahead:select", function (ev, suggestion) {
      selected = suggestion;
      handleAction($(this), $(this).val(), true);
    })
    .on("typeahead:change", function () {
      const currentVal = $(this).typeahead("val");
      if (currentVal === "" || currentVal === null) {
        clearCallbackFields($(this));
      }
    });

  // Clears all callbackfield and DisplayOnFields targets when input is cleared
  function clearCallbackFields($input) {
    let index = $el.index($input);

    // Clear DisplayOnFields targets
    if (DisplayOnFields != "") {
      let objField = [DisplayOnFields];
      if (isJson(objField)) {
        let fields = JSON.parse(objField);
        $.each(fields, function (key) {
          let $target = $("#" + key);
          $target.typeahead("val", "").val("").trigger("change");
        });
      } else {
        let objFields = DisplayOnFields.toString().split(",");
        for (let i = 0; i < objFields.length; i++) {
          let fieldID = objFields[i].trim();
          let $target = $("#" + fieldID);
          $target.typeahead("val", "").val("").trigger("change");
        }
      }
    }

    // Clear callbackfield targets
    if (typeof callbackfield === "object" && callbackfield !== null) {
      $.each(callbackfield, function (key) {
        let $target =
          UseNameInput == ""
            ? $("#" + key)
            : $("input[name=" + key + "]").eq(index);
        if ($target.is("select")) {
          $target.val("0").trigger("change");
        } else {
          $target.val("").trigger("change");
        }
      });
    }

    // Fire CallBackFunction to notify consumer of the clear
    setTimeout(function () {
      if (CallBackFunction != "") {
        let fn = window[CallBackFunction];
        if (typeof fn != "undefined") {
          if (typeof fn === "function") return fn(false);
          new Function("isSelect", CallBackFunction + "(isSelect)").call(
            $input,
            false,
          );
        } else {
          return CallBackFunction();
        }
      }
    }, 50);
  }

  function handleAction($input, aSuggestion, isSelect) {
    if (!keepDataTypeahead[_ID]) return;

    let currentData = keepDataTypeahead[_ID];
    let data = currentData.filter(
      (name) => decode(name[pushDatafield.Name]) === aSuggestion,
    );
    let index = $el.index($input);
    KeepThisValue = aSuggestion;

    if (DisplayOnFields != "") {
      let objField = [DisplayOnFields];
      if (isJson(objField)) {
        let fields = JSON.parse(objField);
        $.each(fields, function (key, val) {
          let value =
            data[0] && typeof data[0][val] != "undefined"
              ? data[0][val].toString()
              : "";
          let $target = $("#" + key);
          $target.typeahead("val", value).val(value);

          // Only trigger change if a real item was selected
          if (isSelect) $target.trigger("change");
        });
      } else {
        let objFields = DisplayOnFields.toString().split(",");
        for (let i = 0; i < objFields.length; i++) {
          let fieldID = objFields[i].trim();
          let val = data[0] ? data[0][fieldID] : "";
          let $target = $("#" + fieldID);
          $target.typeahead("val", val).val(val);

          if (isSelect) $target.trigger("change");
        }
      }
    }

    if (typeof callbackfield === "object" && callbackfield !== null) {
      $.each(callbackfield, function (key, val) {
        let $target =
          UseNameInput == ""
            ? $("#" + key)
            : $("input[name=" + key + "]").eq(index);
        let targetVal = data[0] && data[0][val] ? data[0][val] : "";
        if ($target.is("select")) {
          $target
            .val(targetVal ? targetVal.toString().split(",") : "0")
            .trigger("change");
        } else {
          $target.val(targetVal);
          // CRITICAL FIX: Only trigger change on selection to prevent price snap-back
          if (isSelect) $target.trigger("change");
        }
      });
    }

    if (UseNameInput == "") {
      $el.typeahead("val", KeepThisValue);
    } else {
      $el.eq(index).typeahead("val", KeepThisValue);
    }

    // SQL Callback only runs on explicit selection
    if (callback != "" && callback.CallBackToControlID && isSelect) {
      let valToPost = $("#" + callback.Value).val();
      PostData(
        "DoActionData/ReturnSQLJson/",
        {
          SQL: callback.SQLCallBack,
          FieldName: callback.FieldName,
          Value: valToPost,
        },
        function (response) {
          let respData = JSON.parse(JSON.parse(response).Data);
          let firstRow = respData[0] || "";

          if (isApplyTypeaheadOnCallBack == "True") {
            let objPS = {
              Name: Object.keys(firstRow)[1] || Object.keys(firstRow)[0],
            };
            let objCB = {};
            $.each(firstRow, function (k) {
              objPS[callback.CallBackToControlID + "_" + k] = k;
              objCB[callback.CallBackToControlID + "_" + k] = k;
            });
            ApplyTypeahead(
              callback.CallBackToControlID,
              respData,
              objCB,
              objPS,
            );
          } else {
            let fields = callback.CallBackToControlID.split(",");
            let rowValues = Object.values(firstRow);
            for (let i = 0; i < fields.length; i++) {
              $("#" + fields[i].trim()).val(rowValues[i] || "");
            }
          }
        },
        false,
      );
    }

    setTimeout(function () {
      if (CallBackFunction != "") {
        let fn = window[CallBackFunction];
        if (typeof fn != "undefined") {
          if (typeof fn === "function") return fn(isSelect);
          new Function("isSelect", CallBackFunction + "(isSelect)").call(
            $input,
            isSelect,
          );
        } else {
          return CallBackFunction();
        }
      }
    }, 50);
  }
}

function showToast(Message, icon) {
  Message = typeof Message == "undefined" ? "Loading..." : Message;
  icon = typeof icon == "undefined" ? "" : icon;
  const Toast = Swal.mixin({
    showConfirmButton: false,
    allowOutsideClick: false,
    allowEscapeKey: false,
    toast: true,
    position: "top",
    timer: 2000,
    timerProgressBar: true,
  });
  if (icon == "") {
    Swal.fire({
      toast: true,
      position: "top",
      title: Message,
      //timer: 2000,
      showConfirmButton: false,
      allowOutsideClick: false,
      allowEscapeKey: false,
      timerProgressBar: true,
      willOpen: () => {
        Swal.showLoading();
      },
      willClose: () => {
        Swal.hideLoading();
      },
    });
    //Toast.fire({
    //    title: Message,
    //    onBeforeOpen: () => {
    //        Swal.showLoading()
    //    }
    //});
    //let timerInterval
    //Swal.fire({
    //    title: 'Loading...',
    //    //html: 'I will close in <b></b> milliseconds.',
    //    timer: 2000,
    //    position: 'top',
    //    timerProgressBar: true,
    //    didOpen: () => {
    //        Swal.showLoading();
    //    },
    //    willClose: () => {
    //        clearInterval(timerInterval)
    //    }
    //}).then((result) => {
    //    /* Read more about handling dismissals below */
    //    //if (result.dismiss === Swal.DismissReason.timer) {
    //    //    console.log('I was closed by the timer')
    //    //}
    //})
  } else {
    Toast.fire({
      title: Message,
      icon: icon,
    });
  }
}

function validateInput(FormName) {
  FormName =
    typeof FormName == "undefined" ? "" : "form[name=" + FormName + "] ";
  var check = true;
  var strRequired = {};
  var THIS_ = {};
  var jj = 0;
  if ($(FormName + 'input[type="email"]')) {
    $(FormName + 'input[type="email"]').each(function () {
      if ($(this).val() != "") {
        var email = $(this).val();
        var thisAlert = $(this).parent();
        //var regex = /^([a-zA-Z0-9_.+-])+\@@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;
        var regex = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
        check = regex.test(email);
        if (!check) {
          $(thisAlert).addClass("alert-validate");
          $(thisAlert).attr("data-validate", "Invalid Email Address.");
          strRequired[$(thisAlert).attr("data-validate")] =
            $(thisAlert).attr("data-validate");
          THIS_[jj] = $(this);
          jj++;
        } else {
          $(thisAlert).removeClass("alert-validate");
        }
      }
      $(this).on("blur", function () {
        if ($(this).val().trim() == "") {
          $(this).parent().addClass("alert-validate");
        } else {
          $(this).parent().removeClass("alert-validate");
        }
      });
      $(this).on("focus", function () {
        $(this).parent().removeClass("alert-validate");
      });
    });
  }
  if ($(FormName + 'input[type="url"]')) {
    $(FormName + 'input[type="url"]').each(function () {
      if ($(this).val() != "") {
        var ur = $(this).val();
        var thisAlert = $(this).parent();
        var regex =
          /(ftp|http|https):\/\/(\w+:{0,1}\w*@)?(\S+)(:[0-9]+)?(\/|\/([\w#!:.?+=&%@!\-\/]))?/;
        check = regex.test(ur);
        if (!check) {
          $(thisAlert).addClass("alert-validate");
          $(thisAlert).attr("data-validate", "Invalid URL.");
          strRequired[$(thisAlert).attr("data-validate")] =
            $(thisAlert).attr("data-validate");
          THIS_[jj] = $(this);
          jj++;
        } else {
          $(thisAlert).removeClass("alert-validate");
        }
      }
      $(this).on("blur", function () {
        if ($(this).val().trim() == "") {
          $(this).parent().addClass("alert-validate");
        } else {
          $(this).parent().removeClass("alert-validate");
        }
      });
      $(this).on("focus", function () {
        $(this).parent().removeClass("alert-validate");
      });
    });
  }
  $(FormName + ".validate-input input").each(function () {
    if ($(this).attr("name")) {
      var thisAlert = $(this).closest("div");
      if ($(this).val().trim() == "") {
        $(thisAlert).addClass("alert-validate");
        if ($(thisAlert).attr("data-validate")) {
          strRequired[$(thisAlert).attr("data-validate")] =
            $(thisAlert).attr("data-validate");
          THIS_[jj] = $(this);
          jj++;
          check = false;
        }
      } else {
        $(thisAlert).removeClass("alert-validate");
      }
      $(this).on("blur", function () {
        if ($(this).val().trim() == "") {
          $(this).parent().addClass("alert-validate");
        } else {
          $(this).parent().removeClass("alert-validate");
        }
      });
      $(this).on("focus", function () {
        $(this).parent().removeClass("alert-validate");
      });
    }
  });

  if ($(FormName + ".validate-input select")) {
    $(FormName + ".validate-input select").each(function () {
      var thisAlert = $(this).closest("div"); //$(this).parent();
      if ($(this).attr("name")) {
        if ($(this).val() == "") {
          $(thisAlert).addClass("alert-validate");
          strRequired[$(thisAlert).attr("data-validate")] =
            $(thisAlert).attr("data-validate");
          THIS_[jj] = $(this);
          jj++;
          check = false;
        } else {
          $(thisAlert).removeClass("alert-validate");
        }
        $(this).on("blur", function () {
          if ($(this).val() == "") {
            $(this).parent().addClass("alert-validate");
          } else {
            $(this).parent().removeClass("alert-validate");
          }
        });
        $(this).on("focus", function () {
          $(this).parent().removeClass("alert-validate");
        });
      }
    });
  }

  if ($(FormName + ".validate-input textarea")) {
    $(FormName + ".validate-input textarea").each(function () {
      if ($(this).attr("name")) {
        var thisAlert = $(this).closest("div"); //$(this).parent();
        if ($(this).val() == "") {
          $(thisAlert).addClass("alert-validate");
          strRequired[$(thisAlert).attr("data-validate")] =
            $(thisAlert).attr("data-validate");
          THIS_[jj] = $(this);
          jj++;
          check = false;
        } else {
          $(thisAlert).removeClass("alert-validate");
        }
        $(this).on("blur", function () {
          if ($(this).val() == "") {
            $(this).parent().addClass("alert-validate");
          } else {
            $(this).parent().removeClass("alert-validate");
          }
        });
        $(this).on("focus", function () {
          $(this).parent().removeClass("alert-validate");
        });
      }
    });
  }

  var otherCharge = "";
  $(FormName + ".validateInput input").each(function (index, element) {
    var thisAlert = $(this).parent();
    if (element.name != "" && element.name == "otherCharge") {
      otherCharge = $(this).val();
    }
    if (otherCharge != "") $(thisAlert).removeClass("alert-validate");
    if (
      otherCharge != "" &&
      element.name != "" &&
      element.name != "otherCharge"
    ) {
      if ($(this).val().trim() == "") {
        $(thisAlert).addClass("alert-validate");
        strRequired[$(thisAlert).attr("data-validate")] =
          $(thisAlert).attr("data-validate");
        THIS_[jj] = $(this);
        jj++;
        check = false;
      } else {
        $(thisAlert).removeClass("alert-validate");
      }
    }
    $(this).on("blur", function () {
      if ($(this).val().trim() == "") {
        $(this).parent().addClass("alert-validate");
      } else {
        $(this).parent().removeClass("alert-validate");
      }
    });
    $(this).on("focus", function () {
      $(this).parent().removeClass("alert-validate");
    });
  });

  if (!check) {
    var errorMsg = "";
    let i = 0;
    $.each(strRequired, function (key, value) {
      i++;
      errorMsg += i + ". " + value + "<br />";
    });
    DialogSweetAlert("error", errorMsg, function () {
      if (THIS_[0][0]) {
        setTimeout(function () {
          $(THIS_[0][0]).focus();
        }, 800);
      } else {
        setTimeout(function () {
          THIS_[0].focus();
        }, 800);
      }
    });
  }
  return check;
}
function checkISUniqueFiled(
  OldValue,
  NewValue,
  TblName,
  FieldName,
  THIS,
  callback,
  CheckExsitingWithControllName,
  CheckExsitingWithField,
  CustomCondition,
) {
  callback = typeof callback == "undefined" ? "" : callback;
  CheckExsitingWithControllName =
    typeof CheckExsitingWithControllName == "undefined"
      ? ""
      : CheckExsitingWithControllName;
  CheckExsitingWithField =
    typeof CheckExsitingWithField == "undefined" ? "" : CheckExsitingWithField;
  CustomCondition =
    typeof CustomCondition == "undefined" ? "" : CustomCondition;
  var check = false;
  var thisAlert = THIS.parent();
  if (OldValue == NewValue) {
    thisAlert.removeClass("alert-validate");
    check = true;
    if (typeof callback === "function") {
      return callback(check);
    } else return check;
  }
  if (CheckExsitingWithControllName != "") {
    if ($("#" + CheckExsitingWithControllName + "").val())
      CheckExsitingWithControllName = $(
        "#" + CheckExsitingWithControllName + "",
      ).val();
  }
  LoadData(
    "DoActionData/checkISUniqueFiled?OldValue=" +
      encodeURIComponent(OldValue) +
      "&NewValue=" +
      encodeURIComponent(NewValue) +
      "&TblName=" +
      encodeURIComponent(TblName) +
      "&FieldName=" +
      encodeURIComponent(FieldName) +
      "&CheckExsitingWithControllName=" +
      CheckExsitingWithControllName +
      "&CheckExsitingWithField=" +
      CheckExsitingWithField +
      "&CustomCondition=" +
      CustomCondition,
    function (respone) {
      respone = JSON.parse(respone);
      if (respone.Result == 1) {
        thisAlert.attr("data-validate", respone.Message);
        thisAlert.addClass("alert-validate");
        check = false;
        DialogSweetAlert("error", respone.Message, function () {
          $(THIS).focus().select();
        });
      } else {
        thisAlert.attr("data-validate", "This Field is required");
        thisAlert.removeClass("alert-validate");
        check = true;
      }
      $(THIS).focus(function () {
        thisAlert.removeClass("alert-validate");
      });
      if (typeof callback === "function") {
        return callback(check);
      } else return check;
    },
  );
}

function checkCustCode(OldCustCode, NewCustCode, THIS, CallBack) {
  var check = false;
  var thisAlert = THIS.parent();
  if (OldCustCode == NewCustCode) {
    thisAlert.removeClass("alert-validate");
    check = true;
    return CallBack(check);
  }
  LoadData(
    "/AddCustomer/CheckCustCode?CustCode=" +
      OldCustCode +
      "&NewValue=" +
      NewCustCode,
    function (respone) {
      respone = JSON.parse(respone);
      if (respone.Result == 1) {
        thisAlert.attr("data-validate", respone.Message);
        thisAlert.addClass("alert-validate");
        check = false;
      } else {
        thisAlert.attr("data-validate", "CID is required");
        thisAlert.removeClass("alert-validate");
        check = true;
      }
      return CallBack(check);
    },
  );
}

function PrintInvoice(
  ID,
  InvoiceNo,
  ISVAT,
  StatusID,
  CallBackFunction,
  AutoPrint,
  THIS,
  InvoiceType,
  IsPreview,
  ShowReceipt,
  CustIDToDMC,
  InvoiceToDMC,
  Type,
  IsHideSerialNumber = false,
) {
  StatusID = typeof StatusID == "undefined" ? "0" : StatusID;
  CustIDToDMC = typeof CustIDToDMC == "undefined" ? "0" : CustIDToDMC;
  InvoiceToDMC = typeof InvoiceToDMC == "undefined" ? "" : InvoiceToDMC;
  THIS = typeof THIS == "undefined" ? "" : THIS;
  InvoiceType =
    typeof InvoiceType == "undefined" ? "UnpaidInvoice" : InvoiceType;
  ShowReceipt = typeof ShowReceipt == "undefined" ? false : ShowReceipt;
  Type = typeof Type == "undefined" ? "" : Type;
  var index = "";
  if (THIS != "") index = $(THIS).closest("tr").index();
  AutoPrint = typeof AutoPrint == "undefined" ? true : AutoPrint;
  IsPreview = typeof IsPreview == "undefined" ? false : IsPreview;
  if (InvoiceType == "") InvoiceType = "UnpaidInvoice";
  if (IsPreview == "") IsPreview = false;
  if (ShowReceipt == "") ShowReceipt = false;

  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  //let searchParams = new URLSearchParams(window.location.search);
  //let SystemID = searchParams.get('SystemID');
  //let WarehouseID = searchParams.get('WarehouseID');
  //if (searchParams.has('SystemID') && searchParams.has('WarehouseID')) {
  var URL =
    "CustomerInvoice?SystemID=" +
    System_ID +
    "&WarehouseID=" +
    Warehouse_ID +
    "&ID=" +
    ID +
    "&InvNo=" +
    InvoiceNo +
    "&ISVAT=" +
    ISVAT +
    "&StatusID=" +
    StatusID +
    "&AutoPrint=" +
    AutoPrint +
    "&ISShowPeriod=true&CustIDToDMC=" +
    CustIDToDMC +
    "&InvoiceToDMC=" +
    InvoiceToDMC +
    "&Type=" +
    Type +
    "&DomainID=" +
    Domain_ID;
  if (AutoPrint) {
    OpenNewTap(URL, CallBackFunction);
    return false;
  } else if (!IsPreview) {
    URL =
      "CustomerInvoice/ShowInvoice?SystemID=" +
      System_ID +
      "&WarehouseID=" +
      Warehouse_ID +
      "&ID=" +
      ID +
      "&InvNo=" +
      InvoiceNo +
      "&ISVAT=" +
      ISVAT +
      "&StatusID=" +
      StatusID +
      "&AutoPrint=" +
      AutoPrint +
      "&index=" +
      index +
      "&InvoiceType=" +
      InvoiceType +
      "&ShowReceipt=" +
      ShowReceipt +
      "&CustIDToDMC=" +
      CustIDToDMC +
      "&InvoiceToDMC=" +
      InvoiceToDMC +
      "&Type=" +
      Type +
      "&DomainID=" +
      Domain_ID +
      "&IsHideSerialNumber=" +
      IsHideSerialNumber;
    ShowDialogBox2("Preview Invoice No.: " + InvoiceNo, "Auto", "500", URL);
    var row = '<i class="ti ti-printer" aria-hidden="true"></i> Print';
    $("#SaveChange2").html(row);
    $("#SaveChange2").click(function () {
      $("#div_CustInvoice").printThis({
        //title: "NSM_Customer_Invoice_For",
        //deferred: $.Deferred().done(function () {
        //  window.close();
        //})
      });
      //URL = 'CustomerInvoice?SystemID=' + SystemID + '&WarehouseID=' + WarehouseID + '&ID=' + ID + '&InvNo=' + InvoiceNo + '&ISVAT=' + ISVAT + '&StatusID=' + StatusID;
      //OpenNewTap(URL);
    });
    return false;
  } else {
    OpenNewTap(URL, CallBackFunction);
    return false;
  }
  // }
}

function CreatePDFfromHTML(
  ElementID,
  FileName,
  Download,
  ReturnImageData,
  imgscale,
  ReturnPDFFile,
  AutoWidthHeight,
) {
  showToast();
  $("button").attr("disabled", true);
  var divWidth = $("#" + ElementID).width();
  var divHeight = $("#" + ElementID).height();
  FileName = typeof FileName == "undefined" ? "Result" : FileName;
  Download = typeof Download == "undefined" ? "" : Download;
  ReturnImageData =
    typeof ReturnImageData == "undefined" ? "" : ReturnImageData;
  ReturnPDFFile = typeof ReturnPDFFile == "undefined" ? "" : ReturnPDFFile;
  AutoWidthHeight =
    typeof AutoWidthHeight == "undefined" ? true : AutoWidthHeight;
  //$("#" + ElementID).css("-ms-transform", "scale(0.6)");
  //$("#" + ElementID).css("transform", "scale(0.6)");
  if (AutoWidthHeight) {
    $("#" + ElementID).width(1024);
    //$("#" + ElementID).height(1448);
  }

  var HTML_Width = $("#" + ElementID).width();
  var HTML_Height = $("#" + ElementID).height();
  var top_left_margin = 5;
  var PDF_Width = HTML_Width + top_left_margin * 2;
  var PDF_Height = PDF_Width * 1.5 + top_left_margin * 2;
  var canvas_image_width = HTML_Width;
  var canvas_image_height = HTML_Height;
  var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;
  html2canvas($("#" + ElementID)[0], {
    logging: false,
    scale: typeof imgscale == "undefined" ? 5 : imgscale,
    //letterRendering: false,
    //useCORS: false,
    letterRendering: true,
    //onrendered: function (mycanvas) {
    //    var context = mycanvas.getContext('2d');
    //    context.drawImage(img, 0, 0, HTML_Width, HTML_Height);
    //    // Draw the Watermark
    //    context.font = '48px sans-serif';
    //    context.globalCompositeOperation = 'lighter';
    //    context.fillStyle = 'red';
    //    context.textAlign = 'center';
    //    context.textBaseline = 'middle';
    //    context.fillText('watermark', HTML_Width / 2, HTML_Height / 2);
    //    $("#testimg").attr("src", img.src);
    //    //console.log(img.src);
    //}
  }).then(function (canvas) {
    $("button").attr("disabled", false);
    Swal.close();
    var imgData = canvas.toDataURL("image/jpeg", 0.99);
    if (ReturnImageData == "" && ReturnPDFFile == "") {
      var pdf = new jsPDF("p", "pt", [PDF_Width, PDF_Height]);
      //pdf = addWaterMark(pdf);
      //pdf.setTextColor(150);
      //pdf.text(50, pdf.internal.pageSize.height - 30, 'Watermark');

      pdf.addImage(
        imgData,
        "JPG",
        top_left_margin,
        top_left_margin,
        canvas_image_width,
        canvas_image_height,
      );

      //pdf.text(canvas_image_width / 2, canvas_image_height / 2, "UNPAID");
      //pdf.setFontSize(22);
      for (var i = 1; i <= totalPDFPages; i++) {
        pdf.addPage(PDF_Width, PDF_Height);
        pdf.addImage(
          imgData,
          "JPG",
          top_left_margin,
          -(PDF_Height * i) + top_left_margin * 4,
          canvas_image_width,
          canvas_image_height,
        );
      }

      $("#" + ElementID).width(divWidth);
      $("#" + ElementID).height(divHeight);
      if (Download == "") {
        var win = window.open(pdf.output("bloburl"), "_blank");
        if (win) win.focus();
        else
          DialogSweetAlert(
            "nopermission",
            "Please allow popups for this website or contact administrator.",
          );
      } else pdf.save(FileName);
    } else if (ReturnImageData != "") {
      var pdf = new jsPDF("p", "pt", [PDF_Width, PDF_Height]);
      pdf.addImage(
        imgData,
        "JPG",
        top_left_margin,
        top_left_margin,
        canvas_image_width,
        canvas_image_height,
      );
      for (var i = 1; i <= totalPDFPages; i++) {
        pdf.addPage(PDF_Width, PDF_Height);
        pdf.addImage(
          imgData,
          "JPG",
          top_left_margin,
          -(PDF_Height * i) + top_left_margin * 4,
          canvas_image_width,
          canvas_image_height,
        );
      }
      $("#" + ElementID).width(divWidth);
      $("#" + ElementID).height(divHeight);
      return ReturnImageData(imgData, pdf.output("datauristring"));
    }
  });
}
function HTMLToPDF(ElementID, FileName, callback, DownloadPDF, DownloadIMG) {
  showToast();
  $("button").attr("disabled", true);
  callback = typeof callback == "undefined" ? "" : callback;
  var divWidth = $("#" + ElementID).width();
  var divHeight = $("#" + ElementID).height();
  FileName = typeof FileName == "undefined" ? "Result" : FileName;
  DownloadPDF = typeof DownloadPDF == "undefined" ? "" : DownloadPDF;
  DownloadIMG = typeof DownloadIMG == "undefined" ? "" : DownloadIMG;
  $("#" + ElementID).width(1448);
  //$("#" + ElementID).height(1448);

  var HTML_Width = $("#" + ElementID).width();
  var HTML_Height = $("#" + ElementID).height();
  var top_left_margin = 5;
  var PDF_Width = HTML_Width + top_left_margin * 2;
  var PDF_Height = PDF_Width * 1.5 + top_left_margin * 2;
  var canvas_image_width = HTML_Width;
  var canvas_image_height = HTML_Height;
  var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;
  var row = '<div id="divTmpHTMLTOPDF"></div>';
  //$("body").append(row);
  //$("#divTmpHTMLTOPDF").html($("#" + ElementID)[0]);
  html2canvas($("#" + ElementID)[0], {
    logging: false,
    scale: 5,
  }).then(function (canvas) {
    Swal.close();
    $("button").attr("disabled", false);
    var imgData = canvas.toDataURL("image/jpeg", 0.99);
    var pdf = new jsPDF("p", "pt", [PDF_Width, PDF_Height]);
    pdf.addImage(
      imgData,
      "JPG",
      top_left_margin,
      top_left_margin,
      canvas_image_width,
      canvas_image_height,
    );
    for (var i = 1; i <= totalPDFPages; i++) {
      pdf.addPage(PDF_Width, PDF_Height);
      pdf.addImage(
        imgData,
        "JPG",
        top_left_margin,
        -(PDF_Height * i) + top_left_margin * 4,
        canvas_image_width,
        canvas_image_height,
      );
    }
    $("#" + ElementID).width(divWidth);
    $("#" + ElementID).height(divHeight);
    //pdf.save(FileName);
    if (DownloadPDF != "") pdf.save(FileName);
    if (DownloadIMG != "") {
      var newData = imgData.replace(
        /^data:image\/jpeg/,
        "data:application/octet-stream",
      );
      var link = document.createElement("a");
      link.href = newData;
      link.download = FileName + ".jpg";
      link.click();
      link.remove();
    }
    //$("#divTmpHTMLTOPDF").remove();
    if (callback != "") return callback(pdf.output("datauristring"));
  });
}

//function imgToBase64(imgUrl, returnURL, callback) {
//    returnURL = (typeof returnURL == "undefined" ? false : returnURL);
//    var img = new Image();
//    img.setAttribute('crossOrigin', 'anonymous');
//    img.src = imgUrl;
//    img.onload = () => {
//        const canvas = document.createElement("canvas");
//        canvas.width = img.width;
//        canvas.height = img.height;
//        const ctx = canvas.getContext("2d");
//        ctx.drawImage(img, 0, 0);
//        const dataURL = canvas.toDataURL("image/png");
//        canvas.style.display = 'none';
//        if (!returnURL)
//            callback(dataURL);
//        else {
//            const base64 = dataURL;
//            var contentType = 'image/png', chunkLength = 512;
//            const byteCharsArray = Array.from(atob(base64.substr(base64.indexOf(',') + 1)));
//                const chunksIterator = new Array(Math.ceil(byteCharsArray.length / chunkLength));
//                const bytesArrays = [];
//            for (let c = 0; c < chunksIterator.length; c++) {
//                bytesArrays.push(new Uint8Array(byteCharsArray.slice(c * chunkLength, chunkLength * (c + 1)).map(s => s.charCodeAt(0))));
//            }
//            const blob = new Blob(bytesArrays, { type: contentType });
//            const url = URL.createObjectURL(blob);
//            return callback(url);
//        }
//    }
//}

function addWaterMark(doc) {
  var totalPages = doc.internal.getNumberOfPages();

  for (i = 1; i <= totalPages; i++) {
    doc.setPage(i);
    //doc.addImage(imgData, 'PNG', 40, 40, 75, 75);
    doc.setTextColor(150);
    doc.text(50, doc.internal.pageSize.height - 30, "Watermark");
  }

  return doc;
}

function Renew(InvNo, NextInvoiceDate, CustID, ConnectionID) {
  CustID = typeof CustID == "undefined" ? "" : CustID;
  ConnectionID = typeof ConnectionID == "undefined" ? "" : ConnectionID;
  Edit_Tbl_Row("", "", InvNo, true, NextInvoiceDate, CustID, "", ConnectionID);
}
function Edit_Tbl_Row(
  THIS,
  isRegisterForm,
  InvoiceNo,
  IsRenew,
  NextInvoiceDate,
  CustID,
  ToInvoice,
  ConnectionID,
  Type,
) {
  Type = typeof Type == "undefined" ? "" : Type;
  CustID = typeof CustID == "undefined" ? "" : CustID;
  ConnectionID = typeof ConnectionID == "undefined" ? "" : ConnectionID;
  NextInvoiceDate =
    typeof NextInvoiceDate == "undefined" ? "" : NextInvoiceDate;
  IsRenew = typeof IsRenew == "undefined" ? false : IsRenew;
  isRegisterForm = typeof isRegisterForm == "undefined" ? 0 : isRegisterForm;
  ToInvoice = typeof ToInvoice == "undefined" ? false : ToInvoice;
  if (ToInvoice == "") ToInvoice = false;
  if (CustID != "") isRegisterForm = 0;
  THIS = typeof THIS == "undefined" ? "" : THIS;
  InvoiceNo = typeof InvoiceNo == "undefined" ? "" : InvoiceNo;
  let CloneInvoice = false;
  if (InvoiceNo != "") CloneInvoice = true;
  let url = window.location.hash;
  let CustomerID = url.substring(url.indexOf("?"));
  CustomerID = CustomerID.substring(CustomerID.indexOf("=")).replace(/=/g, "");
  let ISSO = Type == "Sale Order Invoice" ? 2 : 0; //getQueryStringValue(window.location.hash, "ISSO");
  let CID = CustomerID.split("&");
  CustomerID = CID[0];

  if (CustID != "") CustomerID = CustID;
  if (InvoiceNo == "") InvoiceNo = $(THIS).attr("id");
  let obj = {};
  obj = {
    PrimaryFieldValue: "1",
    InvoiceNo: InvoiceNo,
    CustomerID: CustomerID,
    FormStatus: "CreateQuotation",
    ConnectionID: ConnectionID,
    EditInvoice: true,
    IsRenew: IsRenew,
    ISSO: ISSO,
  };
  if (isRegisterForm != 0) AddQuotation(obj, "Edit Quotation");
  else {
    if (!CloneInvoice) {
      obj = {
        PrimaryFieldValue: "0",
        InvoiceNo: InvoiceNo,
        CustomerID: CustomerID,
        FormStatus: "CreateInvoice",
        ConnectionID: ConnectionID,
        EditInvoice: true,
        IsRenew: IsRenew,
        ISSO: ISSO,
      };
      AddQuotation(obj, "Edit Customer Invoice");
    } else {
      var Title = IsRenew == true ? "Customer Renew Services" : "Clone Invoice";
      var frmStatus = "CreateInvoice";
      if (ToInvoice) {
        frmStatus = "ToInvoice";
        ISSO = 0;
      } else if (CustID != "") frmStatus = "";
      let ApprovalPO = false;
      if (Type == "Sale Order Invoice") {
        Title = "Approval Invoice#" + InvoiceNo;
        ApprovalPO = true;
      }
      obj = {
        PrimaryFieldValue: "0",
        InvoiceNo: InvoiceNo,
        CustomerID: CustomerID,
        FormStatus: frmStatus,
        CloneInvoice: CloneInvoice,
        IsRenew: IsRenew,
        NextInvoiceDate: NextInvoiceDate,
        ConnectionID: ConnectionID,
        EditInvoice: true,
        ISSO: ISSO,
        ApprovalPO: ApprovalPO,
      };

      AddQuotation(obj, Title);
    }
  }
}
function AddQuotation(PrimaryFieldValue, Title) {
  Title = typeof Title == "undefined" ? "Add Quotation" : Title;
  //var URL = ServerRoot + 'AddCustomer/AddQuotation';
  var URL = ServerRoot + "AddCustomer/CreateCustomer";
  ShowDialogBox(Title, "Auto", 600, URL, PrimaryFieldValue);
}

function OpenNewTap(URL, CallBackFunction, AutoPrint) {
  $("button").removeAttr("disabled");
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  AutoPrint = typeof AutoPrint == "undefined" ? false : AutoPrint;
  var win = window.open(URL, "_blank");
  if (win) {
    //Browser has allowed it to be opened
    //if (CallBackFunction != "")
    //console.log(CallBackFunction);
    //win.onclose = function () { win.RunCallbackFunction = CallBackFunction; };
    win.focus();
    if (AutoPrint) {
      setTimeout(function () {
        win.print();
      }, 1050);
    }
    if (CallBackFunction != "") {
      return CallBackFunction();
    }
  } else if (win == null) {
    setTimeout(function () {
      DialogSweetAlert(
        "nopermission",
        "Please allow popups for this website or contact administrator.",
      );
    }, 1500);
  } else {
    //Browser has blocked it
    //alert('Please allow popups for this website');
    DialogSweetAlert(
      "nopermission",
      "Please allow popups for this website or contact administrator.",
    );
  }
}
function CreateCustNewInvoice(CustomerID, FormStatus) {
  FormStatus = typeof FormStatus == "undefined" ? "CreateInvoice" : FormStatus;
  //number 2 for create invoicing...
  var Data = [];
  var obj = {};
  obj = {
    PrimaryFieldValue: "2",
    InvoiceNo: "0",
    CustomerID: "" + CustomerID + "",
    FormStatus: FormStatus,
  };
  Data.push(obj);
  AddQuotation(obj, "Create Customer Invoice");
}
function Payment(
  InvoiceNo,
  CustName,
  Amount,
  Status,
  CustomerID,
  Type,
  IsChangeFlow,
) {
  CustomerID = typeof CustomerID == "undefined" ? "0" : CustomerID;
  var Title =
    Status == 10
      ? "Payment - Invoice No.: " + InvoiceNo
      : "Rollback Payment - Invoice No.: " + InvoiceNo;
  var URL =
    ServerRoot +
    "Payment?InvoiceNo=" +
    InvoiceNo +
    "&CustomerName=" +
    CustName +
    "&Amount=" +
    Amount +
    "&Status=" +
    Status +
    "&CustomerID=" +
    CustomerID +
    "&Type=" +
    Type +
    "&IsChangeFlow=" +
    IsChangeFlow;
  ShowDialogBox(Title, 900, 550, URL);
}

function ShowDialogBox2(
  Title,
  Width,
  Height,
  URi,
  PrimaryFieldValue,
  DialogID,
  CallbackFunction,
  btnCloseID,
  btnSaveID,
) {
  //var deferred = $.Deferred();
  URi = typeof URi == "undefined" ? "" : URi;
  PrimaryFieldValue =
    typeof PrimaryFieldValue == "undefined" ? 0 : PrimaryFieldValue;
  DialogID = typeof DialogID == "undefined" ? "NewDialogID" : DialogID;
  CallbackFunction =
    typeof CallbackFunction == "undefined" ? "" : CallbackFunction;
  btnCloseID = typeof btnCloseID == "undefined" ? "CloseModal2" : btnCloseID;
  btnSaveID = typeof btnSaveID == "undefined" ? "SaveChange2" : btnSaveID;
  if (DialogID == "NewDialogID" || DialogID == "")
    DialogID = "NewDialogID_" + uniqueID();
  let modal =
    '<div class="modal fade" data-bs-focus="false" role="dialog" id="' +
    DialogID +
    '">';
  //modal += '<div class="modal-dialog" role="document">';
  if (Width.toString().toLowerCase() == "Auto".toLowerCase())
    modal +=
      '<div class="modal-dialog modal-fullscreen" data-bs-focus="false" role="dialog">';
  else modal += '<div class="modal-dialog" role="dialog">';
  modal += '<div class="modal-content">';
  modal += '<div class="card-default">';
  modal +=
    '<div class="card-header"><div class="row dm-sans"><div class="col"><i class="ti ti-edit"></i>&nbsp;' +
    Title +
    "</div>";
  modal +=
    `<div class="col-auto text-right"><button type="button" onclick="closeDialogClick('` +
    DialogID +
    `');" class="close btnclose" data-dismiss="modal" aria-label="Close">`;
  modal +=
    '<span aria-hidden="true">&times;</span></button></div></div></div></div>';
  modal +=
    '<div class="modal-body table-responsive" id="' + DialogID + 'modalbody">';
  modal += '<p style="padding:0 20px 0 0"></p>';
  modal += "</div>";
  modal += '<div class="modal-footer">';
  modal +=
    '<button type="button" id="' +
    btnSaveID +
    '" name="SaveChange" class="btn btn-outline-primary"><i class="ti ti-device-floppy"></i> Save Change</button>';
  modal +=
    '<button type="button" id="' +
    btnCloseID +
    '" name="CloseModal" class="btn btn-outline-danger btnclose" data-dismiss="modal"><i class="ti ti-x"></i> Close</button>';
  modal += "</div></div>";
  modal += "</div></div>";
  if ($("#" + DialogID).length == 0) $(document.body).append(modal);

  //$("#" + DialogID + ".modal").draggable({
  //    handle: $("#" + DialogID + " .card-default")
  //});
  let url = new URL(window.location.href);
  $("#" + DialogID).on("shown.bs.modal", function () {
    $(this).find(".card-default").drags();

    //let accessToken = getStorage("TokenInfo");
    //if (accessToken == null || accessToken == "") {
    //    $("#" + DialogID + "modalbody p").html('Session is Expired. Please login.');
    //    location.href = LoginUrl;
    //    return false;
    //}
    //else
    //    accessToken = JSON.parse(getStorage('TokenInfo')).accessToken;
    //let userAccessingSystem = "";
    //let UserID = "";
    //let SystemID = "";
    //let WarehouseID = "";
    //let UserName = "";
    //let UserFullName = "";
    //if (typeof getUserAccessingSystemWarehouse == 'function') {
    //    userAccessingSystem = getUserAccessingSystemWarehouse();
    //    UserID = userAccessingSystem.UserID;
    //    SystemID = userAccessingSystem.SystemID;
    //    WarehouseID = userAccessingSystem.WarehouseID;
    //    UserName = userAccessingSystem.UserName;
    //    UserFullName = userAccessingSystem.UserFullName;
    //}
    //else {
    //    let TokenInfo = getStorage("TokenInfo");
    //    let SysAndWH = JSON.parse(JSON.parse(TokenInfo).SysAndWH)[0];
    //    UserID = SysAndWH.UserID;
    //    SystemID = SysAndWH.SystemID;
    //    WarehouseID = SysAndWH.WarehouseID;
    //    UserName = JSON.parse(TokenInfo).userName;
    //    UserFullName = JSON.parse(TokenInfo).UserFullName;
    //}
    //let headerPara = {
    //    UserID: UserID,
    //    UserName: UserName,
    //    SystemID: SystemID,
    //    WarehouseID: WarehouseID,
    //    UserFullName: User_Full_Name
    //};
    if (
      typeof PrimaryFieldValue === "object" ||
      Array.isArray(PrimaryFieldValue)
    ) {
      PrimaryFieldValue = JSON.stringify(PrimaryFieldValue);
    }
    if (URi != "") {
      $.ajax({
        url: ServerRoot + URi,
        data: {
          PrimaryFieldValue: PrimaryFieldValue,
        },
        headers: { Authorization: "Bearer " + access_Token },
        dataType: "html",
        async: true,
        cache: true,
        type: "GET",
        beforeSend: function (xhr) {
          xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
          $("button").attr("disabled", true);
        },
        success: function (responetext) {
          //window.history.pushState(null, null, url + "#" + DialogID);
          //deferred.resolve(responetext);
          $("button").attr("disabled", false);
          $("#" + DialogID + "modalbody p").html(responetext);
          $("#CloseModal2").click(function () {
            $("#" + DialogID + "").modal("hide");
            $("#" + DialogID + " .modal").remove();
            $("#" + DialogID + " .modal-backdrop").remove();
            //$('body').removeClass("modal-open");
          });
          if (Width != "") {
            if (Width.toString().toLowerCase() != "Auto".toLowerCase()) {
              //    $("#" + DialogID + " .modal-dialog").css({
              //        "width": "auto",
              //        "height": "auto",
              //        "min-width": "300px",
              //        "max-width": "100%",
              //        "max-height": "100%"
              //    });
              //}
              //else {
              $("#" + DialogID + " .modal-dialog").css({
                "min-width": Width + "px",
                "max-width": "fit-content",
              });
            }
          }
          if (Width.toString().toLowerCase() != "Auto".toLowerCase()) {
            if (Height != "") {
              $("#" + DialogID + " .modal-body").css({
                height: Height + "px",
                "overflow-y": "scroll",
              });
            }
          }
          if (CallbackFunction != "") return CallbackFunction(responetext);
        },
        error: function (XMLHttpRequest, callStatus, errorThrown) {
          //deferred.reject(XMLHttpRequest.statusText);
          $("button").attr("disabled", false);
          $("#" + DialogID + "modalbody p").html(XMLHttpRequest.statusText);
          $("#" + DialogID + " .modal").remove();
          $("#" + DialogID + " .modal-backdrop").remove();
          //$('body').removeClass("modal-open");
          DialogSweetAlert("error", XMLHttpRequest.statusText);
        },
      });
    } else {
      if (CallbackFunction != "") return CallbackFunction();
    }
  });

  //$("#" + DialogID).modal({
  //    keyboard: false,
  //    backdrop: 'static',
  //    show: true
  //});
  let myModal = new bootstrap.Modal(document.getElementById(DialogID), {
    keyboard: false,
    backdrop: "static",
    focus: true,
  });
  myModal.show();
  const userId = localStorage.getItem("User_ID");
  if (userId == 129) {
    $("#" + DialogID + "modalbody p").html(`
        <div class="section-loader">
			<video class="duck-loader" autoplay loop muted playsinline>
                <source src="../Uploads/duck-loading.webm" type="video/webm" style="width: 180px;" />
            </video>
        </div>
    `);
  } else {
    $("#" + DialogID + "modalbody p").html(
      '<div class="modal-loading-center">' +
        '<div class="modal-spinner"></div>' +
        '<div class="modal-loading-text">Loading...</div>' +
        "</div>",
    );
  }

  $("#" + DialogID).on("hide.bs.modal", function () {
    //window.history.replaceState(null, null, url);
    $(this).data("bs.modal", null);
    //if ($('.tooltipstered').tooltipster()) {
    //    $('.tooltipstered').tooltipster('destroy');
    //}
    $("#" + DialogID).remove();
  });
}

function LoadURL(DIVID, URL, ASYNC, DATA, CallBackFunction) {
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  ASYNC = typeof ASYNC == "undefined" ? true : false;
  $("#" + DIVID).html(
    '<div class="modal-loading-center">' +
      '<div class="modal-spinner"></div>' +
      '<div class="modal-loading-text">Loading...</div>' +
      "</div>",
  );

  var url = ServerRoot + URL;
  DATA = typeof DATA == "undefined" ? { ClearByDates: Date() } : DATA;
  $.ajax({
    url: url,
    dataType: "html",
    type: "POST",
    data: DATA,
    success: function (responseText) {
      $("#" + DIVID).html(responseText);
      if (typeof CallBackFunction === "function") {
        CallBackFunction(responseText);
      }
    },
    error: function (XMLHttpRequest, callStatus, errorThrown) {
      alert(callStatus);
    },
  });
}

function LoadCallBackDataToCBO(
  ID,
  SQL,
  Value,
  DisplayText,
  ShowAll,
  CboValue,
  CallBackFuncion,
  SetValueToNextControlID,
  SetValueToNextControl,
  CallBackWithControllValueID,
  ConditionField,
  ConditionFieldSearch2,
) {
  ShowAll = typeof ShowAll == "undefined" ? "true" : ShowAll;
  CboValue = typeof CboValue == "undefined" ? "" : CboValue;
  CallBackFuncion =
    typeof CallBackFuncion == "undefined" ? "" : CallBackFuncion;
  SetValueToNextControl =
    typeof SetValueToNextControl == "undefined" ? "" : SetValueToNextControl;
  SetValueToNextControlID =
    typeof SetValueToNextControlID == "undefined"
      ? ""
      : SetValueToNextControlID; //$("#" + ID).parent().html('<div align="center" style="width:100%"><img width="16" src= "' + ServerRoot + '/Content/images/loadinfo.net (1).gif" /></div>');
  CallBackWithControllValueID =
    typeof CallBackWithControllValueID == "undefined"
      ? ""
      : CallBackWithControllValueID;
  ConditionField = typeof ConditionField == "undefined" ? "" : ConditionField;
  ConditionFieldSearch2 =
    typeof ConditionFieldSearch2 == "undefined" ? "" : ConditionFieldSearch2;
  var Value_ = "";
  if (CallBackWithControllValueID != "") {
    Value_ = $("#" + CallBackWithControllValueID).val();
    if (Value_ == "") Value_ = "0";
    //SQL = SQL.replace('@SearchText', Value_);
    //SQL = SQL.replace('@Search_Text2', '@SearchText');
  }
  if (CboValue != null) {
    var url = ServerRoot + "DoActionData/LoadCBOData";
    //var TokenInfo = getStorage("TokenInfo");
    //var accessToken = JSON.parse(TokenInfo).accessToken;
    //var UserID = "";
    //var SystemID = "";
    //var WarehouseID = "";
    //var UserName = "";
    //var SysAndWH = JSON.parse(JSON.parse(TokenInfo).SysAndWH)[0];
    //UserID = SysAndWH.UserID;
    //SystemID = SysAndWH.SystemID;
    //WarehouseID = SysAndWH.WarehouseID;
    //UserName = JSON.parse(TokenInfo).userName;
    //var headerPara = {
    //    UserID: UserID,
    //    UserName: UserName,
    //    SystemID: SystemID,
    //    WarehouseID: WarehouseID,
    //    UserFullName: User_Full_Name
    //};
    $.ajax({
      url: url,
      dataType: "html",
      type: "POST",
      headers: { Authorization: "Bearer " + access_Token },
      data: {
        CboValue: CboValue,
        SQL: SQL,
        Value: Value,
        DisplayText: DisplayText,
        ShowAll: ShowAll,
        CallBackWithControllValueID: Value_,
        ConditionField: ConditionField,
        ConditionFieldSearch2: ConditionFieldSearch2,
      },
      success: function (responseText) {
        //var DFiled = DisplayOnFields.split(",");
        //for (i = 0; i < DFiled.length; i++) {
        //    if (DF == "")
        //        DF = "input[name=" + DFiled[i] + "]:eq(" + Index + ")";
        //    else
        //        DF += ", " + "input[name=" + DFiled[i] + "]:eq(" + Index + ")";
        //}
        //$("#" + ID + " option").remove();

        var dID = ID.split(",");
        var data = responseText.split("~");
        if (dID != "") {
          var dataSetValueToNextControl = SetValueToNextControl.split("~");
          var DataSetValueToNextControlID = SetValueToNextControlID.split(",");

          setTimeout(function () {
            for (i = 0; i < dID.length; i++) {
              $("#" + dID[i].trim() + " option").remove();
            }
            for (i = 0; i < data.length; i++) {
              if (data[i].trim() != "") {
                $("#" + dID[i].trim()).append(data[i]);
                $("#" + dID[i].trim()).trigger("change");
                //$("#" + dID[i].trim()).select2({
                //    theme: 'bootstrap4',
                //    dropdownParent: $("#" + dID[i].trim() + "").parent().parent()
                //}).trigger('change');
              }
            }
            if (SetValueToNextControl != "" && SetValueToNextControl != "~") {
              for (i = 0; i < dataSetValueToNextControl.length; i++) {
                if ($("#" + DataSetValueToNextControlID[i].trim())) {
                  $("#" + DataSetValueToNextControlID[i].trim()).val(
                    dataSetValueToNextControl[i].trim(),
                  );
                }
              }
            }
            if (
              CallBackFuncion != "" &&
              typeof CallBackFuncion === "function"
            ) {
              return CallBackFuncion();
            }
          }, 300);
        } else {
          if (CallBackFuncion != "" && typeof CallBackFuncion === "function") {
            return CallBackFuncion();
          }
        }
      },
      error: function (response, callStatus, errorThrown) {
        $("button").removeAttr("disabled");
        DialogSweetAlert("error", response.status + " " + response.statusText);
      },
    });
  }
}

function LoadCallBackData(
  ID,
  SQL,
  DisplayOnFields,
  IsUseConnection2,
  SQL2,
  SQLCallBack2,
  Encry,
  Index,
  CallBackByGrid,
  localValue,
  CallBackFuncion,
  CallBackWithControllValueID,
  isApplyTypeaheadOnCallBack,
  CallBackToControlID,
  FieldName,
  CallBackDisplayText,
  TypeaheadCallBackFunction,
  SQLTypeaheadCallBack,
  DisplayOnFieldsTypeahead,
  ExceptApplyNoValueOnFields,
) {
  DisplayOnFields =
    typeof DisplayOnFields == "undefined" ? "" : DisplayOnFields;
  ExceptApplyNoValueOnFields =
    typeof ExceptApplyNoValueOnFields == "undefined"
      ? ""
      : ExceptApplyNoValueOnFields;
  IsUseConnection2 =
    typeof IsUseConnection2 == "undefined" ? "False" : IsUseConnection2;
  SQL2 = typeof SQL2 == "undefined" ? "" : SQL2;
  SQLCallBack2 = typeof SQLCallBack2 == "undefined" ? "" : SQLCallBack2;
  Encry = typeof Encry == "undefined" ? "true" : Encry;
  Index = typeof Index == "undefined" ? "0" : Index;
  if (Index == "") Index = 0;
  CallBackByGrid =
    typeof CallBackByGrid == "undefined" ? false : CallBackByGrid;
  if (CallBackByGrid == "") CallBackByGrid = false;
  localValue = typeof localValue == "undefined" ? "" : localValue;
  CallBackFuncion =
    typeof CallBackFuncion == "undefined" ? "" : CallBackFuncion;
  CallBackWithControllValueID =
    typeof CallBackWithControllValueID == "undefined"
      ? ""
      : CallBackWithControllValueID;
  isApplyTypeaheadOnCallBack =
    typeof isApplyTypeaheadOnCallBack == "undefined"
      ? ""
      : isApplyTypeaheadOnCallBack;
  CallBackToControlID =
    typeof CallBackToControlID == "undefined" ? "" : CallBackToControlID;
  FieldName = typeof FieldName == "undefined" ? "" : FieldName;
  CallBackDisplayText =
    typeof CallBackDisplayText == "undefined" ? "" : CallBackDisplayText;
  TypeaheadCallBackFunction =
    typeof TypeaheadCallBackFunction == "undefined"
      ? ""
      : TypeaheadCallBackFunction;
  SQLTypeaheadCallBack =
    typeof SQLTypeaheadCallBack == "undefined" ? "" : SQLTypeaheadCallBack;
  DisplayOnFieldsTypeahead =
    typeof DisplayOnFieldsTypeahead == "undefined"
      ? ""
      : DisplayOnFieldsTypeahead;
  let TypeaheadOnCallBackDisplayOnFields = DisplayOnFieldsTypeahead;
  var Value = "";
  if (typeof $("#" + ID).val() != "undefined") {
    Value = $("#" + ID).val();
    ID = $("#" + ID);
    if (CallBackToControlID != "")
      CallBackToControlID = $("#" + CallBackToControlID);
  } else if (typeof $(ID).eq(Index).val() != "undefined") {
    Value = $(ID).eq(Index).val();
    ID = $("#" + ID).eq(Index);
    if (CallBackToControlID != "")
      CallBackToControlID = $(CallBackToControlID).eq(Index);
  } else {
    Value = $("input[name='" + ID + "']")
      .eq(Index)
      .val();
    ID = $("input[name='" + ID + "']").eq(Index);
    if (CallBackToControlID != "")
      CallBackToControlID = $("input[name='" + CallBackToControlID + "']").eq(
        Index,
      );
  }
  if (Value == "") Value = 0;
  if (!CallBackByGrid == "false" || !CallBackByGrid) {
    var DF = "";
    if (DisplayOnFields != "") {
      var DFiled = DisplayOnFields.split(",");
      for (i = 0; i < DFiled.length; i++) {
        if (DF == "") {
          if (
            $("textarea[name=" + $.trim(DFiled[i]) + "]:eq(" + Index + ")").is(
              "textarea",
            )
          )
            DF = "textarea[name=" + $.trim(DFiled[i]) + "]:eq(" + Index + ")";
          else if (
            $("select[name=" + $.trim(DFiled[i]) + "]:eq(" + Index + ")").is(
              "select",
            )
          )
            DF = "select[name=" + $.trim(DFiled[i]) + "]:eq(" + Index + ")";
          else DF = "input[name=" + $.trim(DFiled[i]) + "]:eq(" + Index + ")";
        } else {
          if (
            $("textarea[name=" + $.trim(DFiled[i]) + "]:eq(" + Index + ")").is(
              "textarea",
            )
          )
            DF +=
              ", " +
              "textarea[name=" +
              $.trim(DFiled[i]) +
              "]:eq(" +
              Index +
              ")";
          else if (
            $("select[name=" + $.trim(DFiled[i]) + "]:eq(" + Index + ")").is(
              "select",
            )
          )
            DF +=
              ", " + "select[name=" + $.trim(DFiled[i]) + "]:eq(" + Index + ")";
          else
            DF +=
              ", " + "input[name=" + $.trim(DFiled[i]) + "]:eq(" + Index + ")";
        }
      }
    }
    DisplayOnFields = DF;
  }

  var Value_ = "";
  if (CallBackWithControllValueID != "") {
    Value_ = $("#" + CallBackWithControllValueID).val();
    if (Value_ == "") Value_ = 0;
    if (SQL == "") Value = Value_;
  }
  //var TokenInfo = getStorage("TokenInfo");
  //var accessToken = JSON.parse(TokenInfo).accessToken;
  //var UserID = "";
  //var SystemID = "";
  //var WarehouseID = "";
  //var UserName = "";
  //var SysAndWH = JSON.parse(JSON.parse(TokenInfo).SysAndWH)[0];
  //UserID = SysAndWH.UserID;
  //SystemID = SysAndWH.SystemID;
  //WarehouseID = SysAndWH.WarehouseID;
  //UserName = JSON.parse(TokenInfo).userName;
  //var headerPara = {
  //    UserID: UserID,
  //    UserName: UserName,
  //    SystemID: SystemID,
  //    WarehouseID: WarehouseID,
  //    UserFullName: User_Full_Name
  //};
  if (SQL != "") {
    $.ajax({
      url: ServerRoot + "DoActionData/LoadCallBackData",
      data: {
        SQL: SQL,
        SQL2: SQL2,
        Value: Value,
        IsUseConnection2: IsUseConnection2,
        SQLCallBack2: SQLCallBack2,
        Encry: Encry,
        CallBackWithControllValueID: Value_,
      },
      headers: { Authorization: "Bearer " + access_Token },
      dataType: "html",
      cache: true,
      type: "GET",
      beforeSend: function (xhr) {
        xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
      },
      success: function (responetext) {
        if (responetext != "") {
          var Dsf = DisplayOnFields.split(",");
          if (Dsf.toString() != "") {
            var Result = responetext.split("|");
            for (i = 0; i < Dsf.length; i++) {
              var C = trim(Dsf[i]);
              if (typeof $("#" + C).val() != "undefined") {
                $("#" + C).val("");
                $("#" + C).typeahead("val", "");
                if (typeof Result[i] != "undefined") {
                  $("#" + $.trim(Dsf[i])).val(Result[i]);
                  $("#" + $.trim(Dsf[i])).typeahead("val", Result[i]);
                  $("#" + $.trim(Dsf[i]))
                    .val(Result[i])
                    .trigger("change");
                }
              }
              //else if (typeof $("input[name=" + C+"]").val() != 'undefined') {
              //    $("input[name=" + C+"]").val('');
              //    if (typeof Result[i] != 'undefined') {
              //        $("input[name=" + trim(Dsf[i]) + "]").val(Result[i]);
              //    }
              //}
              else {
                $(C).val("");
                $(C).typeahead("val", "");
                if (typeof Result[i] != "undefined") {
                  if (typeof DFiled != "undefined") {
                    if (
                      $("input[name=" + DFiled[i] + "]:eq(" + Index + ")").is(
                        "input",
                      )
                    ) {
                      $(C).typeahead("val", Result[i]);
                      $(C).val(Result[i]).trigger("change");
                    } else if (
                      $("select[name=" + DFiled[i] + "]:eq(" + Index + ")").is(
                        "select",
                      )
                    ) {
                      $("select[name=" + DFiled[i] + "]:eq(" + Index + ")").val(
                        decode(Result[i]),
                      );
                    } else {
                      $(
                        "textarea[name=" + DFiled[i] + "]:eq(" + Index + ")",
                      ).val(decode(Result[i]));
                      //$('div[name=' + DFiled[i] + ']:eq(' + Index + ')').summernote('code', Result[i]);
                    }
                  } else {
                    if (
                      $("input[name=" + Dsf[i] + "]:eq(" + Index + ")").is(
                        "input",
                      )
                    ) {
                      $("input[name=" + C + "]:eq(" + Index + ")").typeahead(
                        "val",
                        Result[i],
                      );
                      $("input[name=" + C + "]:eq(" + Index + ")")
                        .val(Result[i])
                        .trigger("change");
                    } else if (
                      $("select[name=" + Dsf[i] + "]:eq(" + Index + ")").is(
                        "select",
                      )
                    ) {
                      $("select[name=" + Dsf[i] + "]:eq(" + Index + ")").val(
                        decode(Result[i]),
                      );
                    } else {
                      $("textarea[name=" + Dsf[i] + "]:eq(" + Index + ")").val(
                        decode(Result[i]),
                      );
                      //$('div[name=' + DFiled[i] + ']:eq(' + Index + ')').summernote('code', Result[i]);
                    }
                  }
                }
              }
            }
          }
          if (typeof CallBackFuncion === "function") {
            CallBackFuncion(Index);
          }
        } else {
          var Dsf = DisplayOnFields.split(",");
          if (Dsf.toString() != "") {
            for (i = 0; i < Dsf.length; i++) {
              var C = trim(Dsf[i]);
              let epf = "";
              let isepf = false;
              if (ExceptApplyNoValueOnFields != "")
                epf = ExceptApplyNoValueOnFields.split(",");
              if (epf != "") {
                for (let j = 0; j < epf.length; j++) {
                  let D = trim(epf[j]);
                  if (D == C || D == $(C).attr("name")) {
                    isepf = true;
                    break;
                  }
                }
              }
              if (!isepf) {
                if (typeof $("#" + C).val() != "undefined") {
                  $("#" + C).typeahead("val", "");
                  $("#" + C)
                    .val("")
                    .trigger("change");
                } else {
                  $(C).typeahead("val", "");
                  $(C).val("").trigger("change");
                }
              }
            }
          }
          if (typeof CallBackFuncion === "function") {
            CallBackFuncion(Index);
          }
        }
      },
      error: function (response, callStatus, errorThrown) {
        $("button").removeAttr("disabled");
      },
    });
  }

  if (isApplyTypeaheadOnCallBack == "True") {
    PostData(
      "DoActionData/ReturnSQLJson/",
      { SQL: SQLTypeaheadCallBack, FieldName: FieldName, Value: Value },
      function (response) {
        let Data =
          typeof JSON.parse(JSON.parse(response).Data) == "undefined"
            ? ""
            : JSON.parse(JSON.parse(response).Data);
        let obj = {};
        obj["Name"] = CallBackDisplayText;
        ApplyTypeahead(
          CallBackToControlID,
          Data,
          "",
          obj,
          "",
          "",
          "",
          "",
          TypeaheadCallBackFunction,
          TypeaheadOnCallBackDisplayOnFields,
          Index,
        );

        //if (isApplyTypeaheadOnCallBack == "True") {
        //    let callbackfield = [];
        //    let pushDatafield = [];
        //    let obj = {};
        //    obj["Name"] = Object.keys(Data)[1];
        //    let k = (typeof Object.keys(Data)[0] == "undefined" ? "" : Object.keys(Data)[0]);
        //    if (k) {
        //        obj[CallBackToControlID + '_' + k] = k;
        //        pushDatafield.push(obj);
        //        obj = {};
        //        k = Object.keys(Data)[0];
        //        obj[CallBackToControlID + '_' + k] = Object.keys(Data)[0];//CallBackToControlID + '_' + Object.keys(Data)[0];
        //        callbackfield.push(obj);
        //        obj = {};
        //        k = Object.keys(Data)[1];
        //        obj[k] = Object.keys(Data)[1];
        //        callbackfield.push(obj);
        //        Data = (typeof JSON.parse(JSON.parse(response).Data) == "undefined" ? "" : JSON.parse(JSON.parse(response).Data));
        //        if (PrimaryValue == "0") {
        //            $("#" + CallBackToControlID).val('');
        //            $("#" + CallBackToControlID).typeahead('val', '');
        //        }
        //        ApplyTypeahead(CallBackToControlID, Data, callbackfield[0], pushDatafield[0]);

        //    }
        //}
      },
      false,
    );
  }
}
function ApplySummernote(className) {
  $("." + className).summernote({
    toolbar: false,
    placeholder: "Description",
    disableResizeEditor: true,
  });
  $(".note-statusbar").hide();
}
function SetNumeric(ControlName) {
  $("input[name=" + ControlName + "]").autoNumeric({ aSep: "" });
}

function SetNumericInteger(ControlName) {
  $("input[name=" + ControlName + "]").autoNumeric({ aPad: false });
}

function SetInteger(ControlName) {
  $("input[name=" + ControlName + "]").autoNumeric({
    aSep: "",
    aPad: false,
    vMin: 0,
    vMax: 999999999,
  });
}

function trim(stringToTrim) {
  stringToTrim =
    typeof stringToTrim == "undefined" ? "" : stringToTrim.toString();
  if (stringToTrim == "") return stringToTrim;
  return stringToTrim.replace(/^\s+|\s+$/g, "");
}

function daysInMonth(month, year) {
  var dd = new Date(year, month, 0);
  return dd.getDate();
}

function ReturnComboValue(ComboID) {
  return $("#" + ComboID).val();
}

function SetAutoComplete(
  ID,
  SQL,
  Hidden,
  Name,
  ControlNameID,
  ConditionField,
  IsUseConnection2,
  SQL2,
  CboValue,
  CallBackFunction,
  DisplayOnFields,
  HiddenFeild,
) {
  var CID = "",
    KeepUI = "";
  var currentID = ID;
  ControlNameID = typeof ControlNameID == "undefined" ? "" : ControlNameID;
  ConditionField = typeof ConditionField == "undefined" ? "" : ConditionField;
  Hidden = typeof Hidden == "undefined" ? "" : Hidden;
  Name = typeof Name == "undefined" ? "" : Name;
  IsUseConnection2 =
    typeof IsUseConnection2 == "undefined" ? "" : IsUseConnection2;
  SQL2 = typeof SQL2 == "undefined" ? "" : SQL2;
  CboValue = typeof CboValue == "undefined" ? "" : CboValue;
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  DisplayOnFields =
    typeof DisplayOnFields == "undefined" ? "" : DisplayOnFields;
  HiddenFeild = typeof HiddenFeild == "undefined" ? "" : HiddenFeild;
  CID = ControlNameID;
  var URL = ServerRoot + "DoActionData/Autocomplete";
  if (typeof $("#" + ID).val() != "undefined") ID = $("#" + ID);
  else if (typeof $(ID).val() != "undefined") ID = $(ID);
  else ID = $("input[name='" + ID + "']");
  if (Name != "") ID = $("input[name='" + Name + "']");
  ID.autocomplete({
    source: function (request, response) {
      if (ControlNameID != "") ControlNameID = ReturnComboValue(CID);
      var Cbo_Value = "";
      if (CboValue != "") {
        Cbo_Value = $("#" + CboValue).val();
      }

      window.clearTimeout(TimeOut);
      TimeOut = setTimeout(function () {
        $.ajax({
          url: URL,
          data: {
            term: request.term,
            SQL: SQL,
            IsUseConnection2: IsUseConnection2,
            SQL2: SQL2,
            ValueControlNameID: ControlNameID,
            ConditionField: ConditionField,
            CboValue: Cbo_Value,
            DisplayOnFields: DisplayOnFields,
            HiddenFeild: HiddenFeild,
          },
          dataType: "json",
          dataFilter: function (data) {
            return data;
          },
          converters: {
            "json jsond": function (msg) {
              return msg.hasOwnProperty("d") ? msg.d : msg;
            },
          },
          beforeSend: function (xhr) {
            xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
          },
          headers: { Authorization: "Bearer " + access_Token },
          success: function (data) {
            response(
              $.map(data, function (item) {
                return {
                  label: item.value, // jQuery UI autocomplete reads "label" for the text shown in the dropdown
                  value: item.value,
                  id: item.id,
                  fields: item.fields,
                  HiddenFeild: item.HiddenFeild,
                };
              }),
            );
          },
          error: function (response, callStatus, errorThrown) {
            $("button").removeAttr("disabled");
            DialogSweetAlert(
              "error",
              response.status + " " + response.statusText,
            );
          },
        });
      }, 200); // bumped from 10ms — 10ms fires an AJAX call on almost every keystroke instead of waiting for a pause
    },
    change: function (event, ui) {
      if (typeof ui.item == "undefined") {
        if (KeepUI != "") {
          ui = KeepUI;
          KeepUI = "";
        }
      }
      if (event.isDefaultPrevented()) return;
      event.preventDefault();

      let index = $(ID).index(this);
      let Dsf = Hidden.split(",");
      for (i = 0; i < Dsf.length; i++) {
        let Hidden = trim(Dsf[i]);
        if (Hidden != "") {
          if ($("input[name=" + Hidden + "]").eq(index)) {
            if ($("input[name=" + Hidden + "]").eq(index).length > 0) {
              $("input[name=" + Hidden + "]")
                .eq(index)
                .val("");
              let Item_ID = ui.item == null ? "" : ui.item.id;
              $("input[name=" + Hidden + "]")
                .eq(index)
                .val(Item_ID)
                .trigger("change");
            } else {
              if (ui.item) {
                var hfield = JSON.parse(ui.item.HiddenFeild);
                $("input[name=" + currentID + "_" + Hidden + "]")
                  .eq(index)
                  .val("");
                $("input[name=" + currentID + "_" + Hidden + "]")
                  .eq(index)
                  .val(hfield[Hidden])
                  .trigger("change");
              } else {
                $("input[name=" + currentID + "_" + Hidden + "]")
                  .eq(index)
                  .val("")
                  .trigger("change");
              }
            }
          }
        }
      }

      Dsf = DisplayOnFields.split(",");
      if (Dsf.toString() != "") {
        let Item_Value = ui.item == null ? "" : JSON.parse(ui.item.fields);
        for (i = 0; i < Dsf.length; i++) {
          let C = trim(Dsf[i]);
          if ($("input[name=" + C + "]").eq(index)) {
            $("input[name=" + C + "]")
              .eq(index)
              .val("");
            $("input[name=" + C + "]")
              .eq(index)
              .val(Item_Value[C])
              .trigger("change");
          }
        }
      }

      if (CallBackFunction != "") {
        let fn = window[CallBackFunction];
        if (typeof fn === "function") {
          return fn();
        } else {
          let fn = new Function(CallBackFunction);
          fn.call(this);
        }
      }
    },
    select: function (event, ui) {
      KeepUI = ui;
    },
    autoFocus: true,
    open: function () {
      $(this).autocomplete("widget").css("z-index", 9999);
      return false;
    },
    focus: function (event, ui) {
      if (Hidden != "") {
        var Item_ID = ui.item == null ? "" : ui.item.id;
        $("#" + Hidden).val(Item_ID);
      }
      return false;
    },
  });
}

function createDialog(
  Title,
  Width,
  Height,
  URL,
  Data,
  DialogID,
  ResponseJsonData,
  CallBackFunction,
) {
  URL = typeof URL == "undefined" ? "" : URL;
  if (URL != "") URL = ServerRoot + URL;
  Data = typeof Data == "undefined" ? "" : Data;
  DialogID = typeof DialogID == "undefined" ? "createDialog" : DialogID;
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  ResponseJsonData =
    typeof ResponseJsonData == "undefined" ? "html" : ResponseJsonData;
  Data = typeof Data == "undefined" ? "" : Data;
  $("#" + DialogID).dialog("destroy");
  $("#" + DialogID).remove();
  if ($("#" + DialogID).length == 0)
    $(document.body).append('<div id="' + DialogID + '"><p></p></div>');
  $("#" + DialogID).dialog({
    position: "center",
    bgiframe: true,
    modal: true,
    resizable: false,
    //show: "fade",
    //hide: "fade",
    title: Title,
    width: Width,
    height: Height,
    stack: true,
    sticky: true,
    closeOnEscape: false,
    create: function (event, ui) {
      $(event.target).parent().css("position", "fixed");
    },
    open: function (event, ui) {
      if (URL != "") {
        $("#" + DialogID + " p").html(
          '<div class="modal-loading-center">' +
            '<div class="modal-spinner"></div>' +
            '<div class="modal-loading-text">Loading...</div>' +
            "</div>",
        );

        $.ajax({
          url: URL,
          data: Data,
          dataType: ResponseJsonData,
          cache: false,
          type: "POST",
          success: function (responetext) {
            if (ResponseJsonData == "html")
              $("#" + DialogID + " p").html(responetext);
            else $("#" + DialogID + " p").html(responetext.Message);
            $(".ui-datepicker").hide();
            $("input.submit, button.submit, button").button("reset");
            if (typeof CallBackFunction === "function") {
              CallBackFunction();
            }
            //setTimeout(function () {
            //    var tabindex = 1;
            //    $('input, select, textarea').each(function () {
            //        if (this.type != "hidden") {
            //            var $input = $(this);
            //            $input.attr("tabindex", tabindex);
            //            tabindex++;
            //        }
            //    });
            //}, 900);
          },
          error: function (XMLHttpRequest, callStatus, errorThrown) {
            alert(callStatus);
          },
        });
      }
    },
    close: function (event, ui) {
      $("#" + DialogID).dialog("destroy");
    },
  });
}

function SetAutoCompleteAndCallBack(
  ID,
  SQL,
  IDHiddenCallBack,
  SQLCallBack,
  DisplayOnFields,
  ValueControlNameID,
  ConditionField,
  CallBackByGrid,
  CallBackFuncion,
  ExceptApplyNoValueOnFields,
  Index,
) {
  var ItemImage = "",
    KeepUI = "";
  var IsHaveData = false;
  IDHiddenCallBack =
    typeof IDHiddenCallBack == "undefined" ? "" : IDHiddenCallBack;
  SQLCallBack = typeof SQLCallBack == "undefined" ? "" : SQLCallBack;
  DisplayOnFields =
    typeof DisplayOnFields == "undefined" ? "" : DisplayOnFields;
  ValueControlNameID =
    typeof ValueControlNameID == "undefined" ? "" : ValueControlNameID;
  ConditionField = typeof ConditionField == "undefined" ? "" : ConditionField;
  CallBackByGrid =
    typeof CallBackByGrid == "undefined" ? false : CallBackByGrid;
  CallBackByGrid = CallBackByGrid == "" ? false : true;
  CallBackFuncion =
    typeof CallBackFuncion == "undefined" ? "" : CallBackFuncion;
  ExceptApplyNoValueOnFields =
    typeof ExceptApplyNoValueOnFields == "undefined"
      ? ""
      : ExceptApplyNoValueOnFields;

  Index = typeof Index == "undefined" ? "0" : Index;

  var URL = ServerRoot + "DoActionData/Autocomplete";
  var ControlID = ID;
  if (typeof $("#" + ID).val() != "undefined") ID = $("#" + ID);
  else ID = $(ID);
  var ResponseIDValue = 0;
  if (ValueControlNameID != "") {
    if (typeof $("#" + ValueControlNameID).val() != "undefined")
      ValueControlNameID = $("#" + ValueControlNameID).val();
    else ValueControlNameID = $(ValueControlNameID).val();
  }
  ID.autocomplete({
    source: function (request, response) {
      window.clearTimeout(TimeOut);
      TimeOut = setTimeout(function () {
        $.ajax({
          url: URL,
          async: true,
          headers: { Authorization: "Bearer " + access_Token },
          beforeSend: function (xhr) {
            xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
          },
          data: {
            term: request.term,
            SQL: SQL,
            IsUseConnection2: "False",
            SQL2: "",
            ValueControlNameID: ValueControlNameID,
            ConditionField: ConditionField,
          },
          //contentType: "application/json; charset=utf-8",
          dataType: "json",
          dataFilter: function (data) {
            return data;
          },
          converters: {
            "json jsond": function (msg) {
              return msg.hasOwnProperty("d") ? msg.d : msg;
            },
          },
          success: function (data) {
            IsHaveData = false;
            response(
              $.grep(data, function (item) {
                ItemImage = item.image;
                IsHaveData = true;
                return {
                  value: item.value,
                  id: item.id,
                  image: item.image,
                };
              }),
            );
          },
        });
      }, 100);
    },
    change: function (event, ui) {
      if (typeof ui.item == "undefined") {
        if (KeepUI != "") {
          ui = KeepUI;
          KeepUI = "";
        }
      }

      if (!IsHaveData) {
        var Dsf = DisplayOnFields.split(",");
        if (Dsf.toString() != "") {
          for (i = 0; i < Dsf.length; i++) {
            var C = trim(Dsf[i]);
            let epf = "";
            let isepf = false;
            if (ExceptApplyNoValueOnFields != "")
              epf = ExceptApplyNoValueOnFields.split(",");
            if (epf != "") {
              for (let j = 0; j < epf.length; j++) {
                let D = trim(epf[j]);
                if (D == C) {
                  isepf = true;
                  break;
                }
              }
            }
            if (!isepf) {
              if (typeof $("#" + C).val() != "undefined") {
                $("#" + C).typeahead("val", "");
                $("#" + C)
                  .val("")
                  .trigger("change");
              } else {
                $(C).typeahead("val", "");
                $(C).val("").trigger("change");
              }
            }
          }
        }
        if (typeof CallBackFuncion === "function") {
          CallBackFuncion(IsHaveData);
        }
      }
      var localIndex = 0;
      if (Index != 0) localIndex = $(this).parent().parent().index();
      var localValue = "";
      if (IsHaveData) {
        ResponseIDValue = 0;
        if (IDHiddenCallBack != "") {
          if (typeof ui.item != "undefined") {
            if (ui.item != null) {
              ResponseIDValue = ui.item.id;
              localValue = ui.item.value;
            } else ResponseIDValue = 0;
          } else ResponseIDValue = 0;

          if (typeof $("#" + IDHiddenCallBack).val() != "undefined")
            $("#" + IDHiddenCallBack).val(ResponseIDValue);
          else $(IDHiddenCallBack).val(ResponseIDValue);
          LoadCallBackData(
            IDHiddenCallBack,
            SQLCallBack,
            DisplayOnFields,
            "False",
            "",
            "",
            true,
            localIndex,
            CallBackByGrid,
            localValue,
            CallBackFuncion,
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            ExceptApplyNoValueOnFields,
          );
        } else if (SQLCallBack != "") {
          setTimeout(function () {
            //LoadCallBackData(ID, SQL, DisplayOnFields, IsUseConnection2, SQL2, SQLCallBack2, Encry, Index, CallBackByGrid, localValue, CallBackFuncion, CallBackWithControllValueID, isApplyTypeaheadOnCallBack, CallBackToControlID, FieldName, CallBackDisplayText, TypeaheadCallBackFunction, SQLTypeaheadCallBack, DisplayOnFieldsTypeahead, ExceptApplyNoValueOnFields)
            LoadCallBackData(
              ControlID,
              SQLCallBack,
              DisplayOnFields,
              "False",
              "",
              "",
              true,
              localIndex,
              CallBackByGrid,
              localValue,
              CallBackFuncion,
              "",
              "",
              "",
              "",
              "",
              "",
              "",
              "",
              ExceptApplyNoValueOnFields,
            );
          }, 300);
        }
        if (typeof ui.item != "undefined") {
          if (ui.item != null) {
            $("hidden_selector").val(ui.item.term);
            $(this).data("uiItem", ui.item.value);
          } else {
            $("hidden_selector").val("");
            $(this).data("uiItem", "");
          }
        } else {
          $("hidden_selector").val("");
          $(this).data("uiItem", "");
        }
      }
    },
    select: function (event, ui) {
      KeepUI = ui;
      //var aa = $(this).data("ui-autocomplete");
      //aa._trigger("change");

      //if (!IsHaveData) {
      //    var Dsf = DisplayOnFields.split(',');
      //    if (Dsf.toString() != "") {
      //        for (i = 0; i < Dsf.length; i++) {
      //            var C = trim(Dsf[i]);
      //            let epf = "";
      //            let isepf = false;
      //            if (ExceptApplyNoValueOnFields != "")
      //                epf = ExceptApplyNoValueOnFields.split(',');
      //            if (epf != "") {
      //                for (let j = 0; j < epf.length; j++) {
      //                    let D = trim(epf[j]);
      //                    if (D == C) {
      //                        isepf = true;
      //                        break;
      //                    }
      //                }
      //            }
      //            if (!isepf) {
      //                if (typeof $("#" + C).val() != 'undefined') {
      //                    $("#" + C).typeahead('val', '');
      //                    $("#" + C).val('').trigger("change");
      //                }
      //                else {
      //                    $(C).typeahead('val', '');
      //                    $(C).val('').trigger("change");
      //                }
      //            }
      //        }
      //    }
      //    if (typeof CallBackFuncion === 'function') {
      //        CallBackFuncion(IsHaveData);
      //    }
      //}
      //var localIndex = $(this).parent().parent().index();
      //var localValue = "";
      //if (IsHaveData) {
      //    ResponseIDValue = 0;
      //    if (IDHiddenCallBack != "") {
      //        if (typeof ui.item != 'undefined') {
      //            if (ui.item != null) {
      //                ResponseIDValue = ui.item.id
      //                localValue = ui.item.value;
      //            }
      //            else
      //                ResponseIDValue = 0;
      //        }
      //        else
      //            ResponseIDValue = 0;

      //        if (typeof $("#" + IDHiddenCallBack).val() != 'undefined')
      //            $("#" + IDHiddenCallBack).val(ResponseIDValue);
      //        else
      //            $(IDHiddenCallBack).val(ResponseIDValue);
      //        LoadCallBackData(IDHiddenCallBack, SQLCallBack, DisplayOnFields, "False", "", "", true, localIndex, CallBackByGrid, localValue, CallBackFuncion, "", "", "", "", "", "", "", "", ExceptApplyNoValueOnFields);
      //    }
      //    else if (SQLCallBack != "") {
      //        setTimeout(function () {
      //            //LoadCallBackData(ID, SQL, DisplayOnFields, IsUseConnection2, SQL2, SQLCallBack2, Encry, Index, CallBackByGrid, localValue, CallBackFuncion, CallBackWithControllValueID, isApplyTypeaheadOnCallBack, CallBackToControlID, FieldName, CallBackDisplayText, TypeaheadCallBackFunction, SQLTypeaheadCallBack, DisplayOnFieldsTypeahead, ExceptApplyNoValueOnFields)
      //            LoadCallBackData(ControlID, SQLCallBack, DisplayOnFields, "False", "", "", true, localIndex, CallBackByGrid, localValue, CallBackFuncion, "", "", "", "", "", "", "", "", ExceptApplyNoValueOnFields);
      //        }, 300);
      //    }
      //    if (typeof ui.item != 'undefined') {
      //        if (ui.item != null) {
      //            $("hidden_selector").val(ui.item.term);
      //            $(this).data("uiItem", ui.item.value);
      //        }
      //        else {
      //            $("hidden_selector").val("");
      //            $(this).data("uiItem", "");
      //        }
      //    } else {
      //        $("hidden_selector").val("");
      //        $(this).data("uiItem", "");
      //    }
      //}
    },
    //autoFocus: true,
    open: function (event, ui) {
      $(this).autocomplete("widget").css("z-index", 9999);
      if (ItemImage != null)
        $(".ui-autocomplete").css({ width: "300px", height: "auto" });
      return false;
    },
    //focus: function (event, ui) {
    //    if (IDHiddenCallBack != "") {
    //        var Item_ID = ui.item == null ? "" : ui.item.id;
    //        $("#" + IDHiddenCallBack).val(Item_ID);
    //    }
    //    return false;
    //},
  });
  //    .data("autocomplete")._renderItem = function (ul, item) {
  //    if (item.image != null && item.image != "")
  //        var inner_html = '<a><div class="list_item_container"><div class="image"><img src="' + item.image + '" width=\"30\" /></div><div class="description">' + item.value + '</div></div></a>';
  //    else
  //        var inner_html = '<a><div><div>' + item.value + '</div></div></a>';
  //    return $("<li></li>")
  //        .data("item.autocomplete", item)
  //        .append(inner_html)
  //        .appendTo(ul);
  //};
  //ID.keypress(function (event) {});
  //ID.blur(function () {});
}

function MessageBox(Message) {
  Swal.fire({
    position: "top",
    icon: "error",
    title: "Oops...",
    html: Message,
  });
  //if ($("#MessageBox").length == 0)
  //    $(document.body).append("<div id=\"MessageBox\"><p></p></div>");
  //$("#MessageBox").dialog({
  //    position: "center",
  //    bgiframe: true,
  //    modal: true,
  //    resizable: false,
  //    title: "Message",
  //    width: 400,
  //    height: 210,
  //    stack: true,
  //    sticky: true,
  //    closeOnEscape: false,
  //    create: function (event, ui) {
  //        $(event.target).parent().css("position", "fixed");
  //    },
  //    open: function (event, ui) {
  //        $("#MessageBox p").html(Message);
  //    },
  //    close: function (event, ui) { $("#MessageBox").dialog("destroy"); },
  //    buttons: {
  //        Close: function () { $("#MessageBox").dialog("destroy"); }
  //    }
  //});
}

function DialogMessageBox(Message, ShowProgress) {
  ShowProgress = typeof ShowProgress == "undefined" ? "" : ShowProgress;
  if ($("#DialogMessageBox").length == 0)
    $(document.body).append('<div id="DialogMessageBox"><p></p></div>');
  $("#DialogMessageBox").dialog({
    position: "center",
    bgiframe: true,
    modal: true,
    resizable: false,
    title: "Message",
    width: 400,
    height: 200,
    stack: true,
    sticky: true,
    closeOnEscape: false,
    create: function (event, ui) {
      $(event.target).parent().css("position", "fixed");
    },
    open: function (event, ui) {
      $("#div_ProgressBar").remove();
      var msg =
        '<div align="center"><h4>' +
        Message +
        '</h4><img width="36" src="' +
        ServerRoot +
        'Content/TableGrid/Images/loadinfo.gif"></div>';

      if (ShowProgress != "") {
        msg +=
          '<div id="div_ProgressBar"><div class="progress" id="progressDivId">';
        msg += '<div class="progress-bar" id="progressBar"></div>';
        msg += '<div class="percent" id="percent">0%</div></div>';
        msg += '<div style="height: 10px;"></div></div>';
      }
      $("#DialogMessageBox p").html(msg);
    },
    close: function (event, ui) {
      $("#DialogMessageBox").dialog("destroy");
    },
    //buttons: {
    //    Close: function () { $("#MessageBox").dialog("destroy"); }
    //}
  });
  $("#DialogMessageBox")
    .parent()
    .children()
    .children(".ui-dialog-titlebar-close")
    .hide();
  $("#DialogMessageBox")
    .dialog("widget")
    .find(".ui-dialog-buttonpane .ui-button:first")
    .hide();
}

function SaveChageDialog(URL, DATA, Message, CallBackFunction, Getresponetext) {
  Message = typeof Message == "undefined" ? "Saving Data..." : Message;
  Getresponetext = typeof Getresponetext == "undefined" ? false : true;
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  DialogMessageBox(Message);
  $.ajax({
    url: URL,
    data: DATA,
    dataType: "json",
    cache: false,
    type: "POST",
    success: function (responetext) {
      $("#DialogMessageBox").dialog("destroy");
      if (typeof CallBackFunction === "function") {
        if (Getresponetext) CallBackFunction(responetext);
        else CallBackFunction();
      }
    },
    error: function (XMLHttpRequest, callStatus, errorThrown) {
      alert(callStatus);
    },
  });
}

var Dialog = function (params) {
  var PType = createDialog.prototype;
  PType.Title = "Message";
  PType.Width = 430;
  PType.Height = 160;
  PType.Message =
    "This record will be permanently deleted and cannot be recovered. <br /> Are you sure?";
  PType.URL = "";
  PType.Data = "";
  PType.DialogID = "DeleteDialog";
  PType.YesText = "Yes";
  PType.NoText = "No";
  PType.YesFunction = function () {};
  PType.NoFunction = function () {
    $("#" + PType.DialogID).dialog("destroy");
  };
  PType.Buttons = "";

  if (typeof params != "undefined") {
    for (var i = 0; i < arguments.length; i++)
      for (var key in arguments[i]) {
        PType[key] = arguments[i][key];
      }
  }

  if (PType.Buttons == "") {
    PType.Buttons = [
      {
        id: "BtnYes",
        text: PType.YesText,
        click: PType.YesFunction,
      },
      {
        id: "BtnNo",
        text: PType.NoText,
        click: PType.NoFunction,
      },
    ];
  }

  if ($("#" + PType.DialogID).length == 0)
    $(document.body).append('<div id="' + PType.DialogID + '"><p></p></div>');
  $("#" + PType.DialogID).dialog({
    position: "center",
    bgiframe: true,
    modal: true,
    resizable: false,
    title: PType.Title,
    width: PType.Width,
    height: PType.Height,
    stack: true,
    sticky: true,
    closeOnEscape: false,
    create: function (event, ui) {
      $(event.target).parent().css("position", "fixed");
    },
    open: function (event, ui) {
      $("#" + PType.DialogID + " p").html(PType.Message);
      if (PType.URL != "") {
        $("#" + PType.DialogID + " p").html(
          '<div class="modal-loading-center">' +
            '<div class="modal-spinner"></div>' +
            '<div class="modal-loading-text">Loading...</div>' +
            "</div>",
        );

        $.ajax({
          url: ServerRoot + PType.URL,
          data: PType.Data,
          dataType: "html",
          cache: false,
          type: "POST",
          success: function (responetext) {
            $("#" + DialogID + " p").html(responetext);
            $(".ui-datepicker").hide();
            $("input.submit, button.submit, button").button("reset");
          },
          error: function (XMLHttpRequest, callStatus, errorThrown) {
            alert(callStatus);
          },
        });
      }
    },
    close: function (event, ui) {
      $("#" + PType.DialogID).dialog("destroy");
    },
    buttons: PType.Buttons,
  });
};

function StringToDate(string) {
  string = typeof string == "undefined" ? "" : string;
  if (string != "") {
    var day = string.substring(0, 2);
    var year = string.substring(6, 10);
    var month = string.substring(3, 5);
    string = year + "/" + month + "/" + day;
    let isDate = new Date(string);
    isDate = isDate instanceof Date && !isNaN(isDate.valueOf());
    if (!isDate) {
      var day = string.substring(0, 2);
      var year = string.substring(6, 10);
      var month = string.substring(3, 5);
      string = year + "/" + month + "/" + day;
      return string;
    } else {
      return string == "" ? null : string;
    }
  } else {
    return string == "" ? null : string;
  }
}
Date.prototype.getAmPm = function () {
  if (this.getHours() >= 12) {
    return 1;
  } // pm
  return 0; // am
};

var locale = {
  en: {
    month: [
      "January",
      "February",
      "March",
      "April",
      "May",
      "June",
      "July",
      "August",
      "September ",
      "October",
      "November",
      "December",
    ],
    ampm: ["am", "pm"],
  },
  km: {
    month: [
      "មករា",
      "កុម្ភៈ",
      "មីនា",
      "មេសា",
      "ឧសភា",
      "មិថុនា",
      "កក្កដា",
      "សីហា",
      "កញ្ញា",
      "តុលា",
      "វិច្ឆិកា",
      "ធ្នូ",
    ],
    ampm: ["ព្រឹក", "ល្ងាច"],
  },
};

var toLocaleNumber = function (num, lang, zeroPadding) {
  if (typeof num !== "number") return null;

  var numInteger = parseInt(num);
  var numString = numInteger.toString();

  if (zeroPadding > 0 && numString.length < zeroPadding) {
    numString = "0" + numString;
  }

  // support only khmer
  if (lang !== "km") {
    return numString;
  }

  var khmerNumber = "";
  //var numbersKhmer = ['០', '១', '២', '៣', '៤', '៥', '៦', '៧', '៨', '៩'];
  var numbersKhmer = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"];

  for (var i = 0; i < numString.length; i++) {
    khmerNumber += numbersKhmer[parseInt(numString[i])];
  }

  return khmerNumber;
};

var formatToKhmerDate = function (date, lang) {
  var formattedDate = null;
  var hours = d.getHours();
  if (hours > 12) {
    hours -= 12;
  }

  formattedDate =
    toLocaleNumber(d.getDate(), lang, 2) +
    "-" +
    locale[lang]["month"][d.getMonth()] +
    "-" +
    toLocaleNumber(d.getFullYear(), lang);
  //+ ' '
  //+ toLocaleNumber( hours, lang, 2 )
  //+ ':'
  //+ toLocaleNumber( d.getMinutes(), lang, 2 )
  //+ ' '
  //+ locale[lang]['ampm'][d.getAmPm()];

  return formattedDate;
};
//var d = new Date('2014-02-03');

// for example: "28-May-2016 3:44 pm" "២៨-ឧសភា-២០១៦ ៣:៤៤ ល្ងាច"
//console.log( formatDate( d, 'en' ), formatDate( d, 'km' ) );

function setUM(value) {
  if (value == 1) return "Monthly";
  else if (value > 1 && value < 12) return value + " Months";
  else if (value == 12) return "Yearly";
}
function returnMonth(month) {
  if (month == 1) return " Month";
  else return " Months";
}
function addMonths(date, months) {
  var d = date.getDate();
  date.setMonth(date.getMonth() + +months);
  if (date.getDate() != d) {
    date.setDate(0);
  }
  return date;
}
function addMinutes(date, minutes) {
  return new Date(date.getTime() + minutes * 60000);
}

async function SetValueTableRow(
  Data,
  inputName,
  TblID,
  TRName,
  TblRow,
  TDNoName,
  THIS,
  callback,
  SQL,
  SQLCallBack,
  DisplayOnFiled,
  callbackOnAddRow,
  isTagsInput,
  onTagsInputAddedFunction,
  RowFields,
) {
  callback = typeof callback == "undefined" ? "" : callback;
  SQL = typeof SQL == "undefined" ? "" : SQL;
  SQLCallBack = typeof SQLCallBack == "undefined" ? "" : SQLCallBack;
  DisplayOnFiled = typeof DisplayOnFiled == "undefined" ? "" : DisplayOnFiled;
  callbackOnAddRow =
    typeof callbackOnAddRow == "undefined" ? "" : callbackOnAddRow;
  isTagsInput = typeof isTagsInput == "undefined" ? "" : isTagsInput;
  onTagsInputAddedFunction =
    typeof onTagsInputAddedFunction == "undefined"
      ? ""
      : onTagsInputAddedFunction;
  RowFields = typeof RowFields == "undefined" ? "" : RowFields;
  if (RowFields != "") {
    for (let i = 0; i < Data.length; i++) {
      //setTimeout(async function () {
      let index = i;
      await AddTableRow(
        inputName,
        TblID,
        TRName,
        TblRow,
        TDNoName,
        THIS,
        callback,
        SQL,
        SQLCallBack,
        DisplayOnFiled,
        callbackOnAddRow,
        isTagsInput,
        onTagsInputAddedFunction,
        RowFields,
        i,
      ).then(function () {
        for (let j = 0; j < RowFields.length; j++) {
          let val = RowFields[j];
          let value =
            typeof Data[index][val.FieldName] == "undefined"
              ? ""
              : Data[index][val.FieldName];
          value = value == null ? "" : value;
          if (val.IsKendoMultiColumnComboBox == "True") {
            setTimeout(function () {
              var multicolumncombobox = $(
                "#" +
                  TblID +
                  " input[name=" +
                  val.Value +
                  "]:eq(" +
                  index +
                  ")",
              ).data("kendoMultiColumnComboBox");

              if (multicolumncombobox) {
                multicolumncombobox.value(value);
                multicolumncombobox.trigger("change");
              } else {
                multicolumncombobox = $(
                  "#" +
                    TblID +
                    " input[name=" +
                    val.Value +
                    "]:eq(" +
                    index +
                    ")",
                ).data("kendoComboBox");
                if (multicolumncombobox) {
                  multicolumncombobox.value(value);
                  multicolumncombobox.trigger("change");
                }
              }
            }, 500);
          } else if (val.Value != "" && val.ISSummerNote == "True") {
            $(
              "#" + TblID + " div[name=" + val.Value + "]:eq(" + index + ")",
            ).summernote("code", value);
          } else if (
            val.Value != "" &&
            val.GetValueFromTextareaName == "True"
          ) {
            $(
              "#" +
                TblID +
                " textarea[name=" +
                val.Value +
                "]:eq(" +
                index +
                ")",
            ).html(decodeHTMLForTextarea(value));
          } else if (
            val.Value != "" &&
            $(
              "#" + TblID + " input[name=" + val.Value + "]:eq(" + index + ")",
            ).is("input")
          ) {
            let type = $(
              "#" + TblID + " input[name=" + val.Value + "]:eq(" + index + ")",
            ).attr("type");
            if (type != "file") {
              if (val.Format == "IsDate" && value != "" && value != null) {
                value = moment(new Date(value)).format("DD/MM/YYYY");
              }
              $(
                "#" +
                  TblID +
                  " input[name=" +
                  val.Value +
                  "]:eq(" +
                  index +
                  ")",
              ).val(decode(value));
            } else if (type == "file") {
              if (value != "") {
                let filenames = value.split("~");
                let urlfile = "";
                let dT = new DataTransfer();
                for (i = 0; i < filenames.length; i++) {
                  if (filenames[i].trim() != "") {
                    let file =
                      ServerRoot +
                      val.DestinationFile +
                      "/" +
                      filenames[i].trim();
                    if (urlfile == "") urlfile = '["' + file + '"';
                    else urlfile += '*$$*"' + file + '"';

                    let f = getFileFromUrl(file, filenames[i].trim());
                    Promise.all([f]).then((responses) => {
                      let newFile = responses[0];
                      dT.items.add(newFile);
                      $(
                        "#" +
                          TblID +
                          " input[name=" +
                          val.Value +
                          "]:eq(" +
                          index +
                          ")",
                      ).prop("files", dT.files);
                    });
                  }
                }
                urlfile += "]";
                $(
                  "#" +
                    TblID +
                    " input[name=" +
                    val.Value +
                    "]:eq(" +
                    index +
                    ")",
                ).fileinput("destroy");
                $(
                  "#" +
                    TblID +
                    " input[name=" +
                    val.Value +
                    "]:eq(" +
                    index +
                    ")",
                ).fileinput({
                  theme: "fa5",
                  showCaption: false,
                  showPreview: false,
                  inputGroupClass: "input-group-sm",
                  dropZoneEnabled: false,
                  allowedFileTypes: ["image"],
                  showCancel: false,
                  showUpload: false,
                  initialPreview: urlfile.toString(),
                  initialPreviewAsData: true,
                });
              }
            }
          } else if (
            val.Value != "" &&
            $(
              "#" +
                TblID +
                " textarea[name=" +
                val.Value +
                "]:eq(" +
                index +
                ")",
            ).is("textarea")
          ) {
            $(
              "#" +
                TblID +
                " textarea[name=" +
                val.Value +
                "]:eq(" +
                index +
                ")",
            ).val(decodeHTMLForTextarea(value));
          }
        }
      });
      //}, 400);
    }
  }
}
async function getFileFromUrl(url, name) {
  const response = await fetch(url);
  const data = await response.blob();
  return new File([data], name, {
    type: data.type,
  });
}

async function AddTableRow(
  inputName,
  TblID,
  TRName,
  TblRow,
  TDNoName,
  THIS,
  callback,
  SQL,
  SQLCallBack,
  DisplayOnFiled,
  callbackOnAddRow,
  isTagsInput,
  onTagsInputAddedFunction,
  RowFields,
  RowIndex,
) {
  callback = typeof callback == "undefined" ? "" : callback;
  SQL = typeof SQL == "undefined" ? "" : SQL;
  SQLCallBack = typeof SQLCallBack == "undefined" ? "" : SQLCallBack;
  DisplayOnFiled = typeof DisplayOnFiled == "undefined" ? "" : DisplayOnFiled;
  callbackOnAddRow =
    typeof callbackOnAddRow == "undefined" ? "" : callbackOnAddRow;
  isTagsInput = typeof isTagsInput == "undefined" ? "" : isTagsInput;
  onTagsInputAddedFunction =
    typeof onTagsInputAddedFunction == "undefined"
      ? ""
      : onTagsInputAddedFunction;
  RowFields = typeof RowFields == "undefined" ? "" : RowFields;
  RowIndex = typeof RowIndex == "undefined" ? "" : RowIndex;
  var Index = $(THIS).closest("tr").index(); //$(THIS).parent().parent().index();

  var TR = TblRow;
  var TRLen = $("tr[name=" + TRName + "]").length;
  var inName = inputName.split(",");
  if (Index + 1 == TRLen || Index == -1) {
    if (Index == -1) {
      Index = 0;
      $("#" + TblID).append(TR);
    } else {
      $("#" + TblID + " > tbody > tr:last").after(TR);
    }
    //if (inputName == "") {
    //    return true;
    //}
    $(".tt-menu").hide();
    if (inputName != "") {
      setTimeout(function () {
        for (i = 0; i < inName.length; i++) {
          if ($("input[name=" + $.trim(inName[i].toString()) + "]")) {
            if (
              $("input[name=" + $.trim(inName[i].toString()) + "]").is("input")
            ) {
              $("input[name=" + $.trim(inName[i].toString()) + "]").on(
                "focus",
                function (event) {
                  AddTableRow(
                    inputName,
                    TblID,
                    TRName,
                    TblRow,
                    TDNoName,
                    this,
                    callback,
                    SQL,
                    SQLCallBack,
                    DisplayOnFiled,
                    callbackOnAddRow,
                    isTagsInput,
                    onTagsInputAddedFunction,
                    RowFields,
                  );
                },
              );
            } else if (
              $("textarea[name=" + $.trim(inName[i].toString()) + "]").is(
                "textarea",
              )
            ) {
              $("textarea[name=" + $.trim(inName[i].toString()) + "]").on(
                "focus",
                function (event) {
                  AddTableRow(
                    inputName,
                    TblID,
                    TRName,
                    TblRow,
                    TDNoName,
                    this,
                    callback,
                    SQL,
                    SQLCallBack,
                    DisplayOnFiled,
                    callbackOnAddRow,
                    isTagsInput,
                    onTagsInputAddedFunction,
                    RowFields,
                  );
                },
              );
            }
          } else return;
        }
      }, 100);
    }
    $("tr[name=" + TRName + "]").each(function (i) {
      $("td[name=" + TDNoName + "]")
        .eq(i)
        .html(i + 1);
    });

    if (SQL != "" && SQLCallBack != "") {
      for (i = 0; i < inName.length; i++) {
        //inputName = $.trim(inName[i]);
        var ID = "input[name=" + $.trim(inName[i]) + "]";
        SetAutoCompleteAndCallBack(
          ID,
          SQL,
          "",
          SQLCallBack,
          DisplayOnFiled,
          "",
          "",
          true,
          callbackOnAddRow,
        );
      }
    } else if (SQL != "") {
      for (i = 0; i < inName.length; i++) {
        //inputName = $.trim(inName[i]);
        var ID = "input[name=" + $.trim(inName[i]) + "]";
        SetAutoCompleteAndCallBack(ID, SQL);
      }
    }
    if (RowFields != "") {
      RowFields.forEach(function (item) {
        if (item.Value != "") {
          $("input[name=" + item.Value + "]:eq(" + TRLen + ")").focus(
            function () {
              $(this).select();
            },
          );
          if (trim(item.SetValue) != "") {
            $("input[name=" + item.Value + "]:eq(" + TRLen + ")").val(
              item.SetValue,
            );
          }
          if (trim(item.Format) == "Unformat") {
            if (trim(item.ISInteger) == "True") {
              $("input[name=" + item.Value + "]:eq(" + TRLen + ")").autoNumeric(
                {
                  vMin: "0",
                  vMax: "999999999",
                },
              );
            } else {
              var vMin = "-99999999999" + item.Digit.replace(0, 9).toString();
              var vMax =
                "9999999999999999999999999999999999999999999999999999999" +
                item.Digit.replace(0, 9).toString();
              $("input[name=" + item.Value + "]:eq(" + TRLen + ")").autoNumeric(
                { vMin: vMin, vMax: vMax },
              );
            }
            $("input[name=" + item.Value + "]:eq(" + TRLen + ")").on(
              "keydown",
              function (e) {
                if (e.which == 40) {
                  var index = $(
                    "input[name=" + $(this).attr("name") + "]",
                  ).index($(this));
                  var THIS = $("input[name=" + $(this).attr("name") + "]");
                  $(this)
                    .closest("tr")
                    .next()
                    .find("input[name=" + $(this).attr("name") + "]")
                    .focus()
                    .select();
                  var scrollPosition = THIS.eq(index).offset().top - 120;
                  $(window).scrollTop(scrollPosition);
                  delete e.which;
                  return false;
                }
                if (e.which == 38) {
                  var index = $(
                    "input[name=" + $(this).attr("name") + "]",
                  ).index($(this));
                  var THIS = $("input[name=" + $(this).attr("name") + "]");
                  $(this)
                    .closest("tr")
                    .prev()
                    .find("input[name=" + $(this).attr("name") + "]")
                    .focus()
                    .select();
                  var scrollPosition = THIS.eq(index).offset().top - 200;
                  $(window).scrollTop(scrollPosition);
                  delete e.which;
                  return false;
                }
              },
            );
          }

          let type = $("input[name=" + item.Value + "]:eq(" + TRLen + ")").attr(
            "type",
          );
          if (type == "file") {
            $("input[name=" + item.Value + "]:eq(" + TRLen + ")").fileinput({
              theme: "fa5",
              showCaption: false,
              showPreview: false,
              inputGroupClass: "input-group-sm",
              dropZoneEnabled: false,
              allowedFileTypes: ["image"],
              showCancel: false,
              showUpload: false,
            });
            $("input[name=" + item.Value + "]:eq(" + TRLen + ")").on(
              "change",
              function (event, files) {
                if (event.isDefaultPrevented()) return;
                event.preventDefault();
                let input = event.currentTarget;
                let dT = new DataTransfer();
                $.each(event.target.files, function (key, value) {
                  let fileExtension = "." + value.name.split(".").pop();
                  let oldName = value.name;
                  let fName = oldName
                    .slice(0, oldName.lastIndexOf("."))
                    .replace(/[^\w]/gi, "");
                  let newName =
                    fName +
                    "_" +
                    new Date().getTime().toString().substring(8) +
                    fileExtension;
                  let previousFile = value;
                  let newFile = new File([previousFile], newName);
                  dT.items.add(newFile);
                });
                input.files = dT.files;
              },
            );
          }
        }
      });
    }
    if (isTagsInput != "") {
      $("input[name=" + isTagsInput + "]").tagsinput({
        trimValue: true,
        tagClass: "badge bg-primary",
      });
      $("div.bootstrap-tagsinput").css({
        border: "none",
        "box-shadow": "none",
      });
    }

    if (onTagsInputAddedFunction != "") {
      $("input[name=" + isTagsInput + "]").on("itemAdded", function (event) {
        if (event.isDefaultPrevented()) return;
        event.preventDefault();
        var fn = window[onTagsInputAddedFunction];
        // is object a function?
        if (typeof fn === "function") fn(event, this);
      });
      $("input[name=" + isTagsInput + "]").on("itemRemoved", function (event) {
        if (event.isDefaultPrevented()) return;
        event.preventDefault();
        var fn = window[onTagsInputAddedFunction];
        // is object a function?
        if (typeof fn === "function") fn(event, this);
      });
    }

    let allobj = [];
    let KeepTblRowData = [];
    $("#" + TblID + " > tbody > tr").each(function (key, val) {
      let trname = $(val).attr("name");
      let objtblval = {};
      let objtblrowval = {};
      $("tr[name=" + trname + "]:eq(" + key + ") td input").each(
        function (k, v) {
          let inputname =
            typeof $(v).attr("name") == "undefined" ? "" : $(v).attr("name");
          if (inputname != "") {
            objtblval[inputname] = $(v).val();
            if ($(v).val() != "") objtblrowval[inputname] = $(v).val();
            $(v).change(function (event) {
              if (event.isDefaultPrevented()) return;
              event.preventDefault();
              //let rowindex = $(v).closest("tr").index();
              //console.log(rowindex, allobj);
              //console.log(allobj[rowindex]);
            });
          }
        },
      );
      KeepTblRowData.push(objtblrowval);
      allobj.push(objtblval);
    });

    if (callbackOnAddRow != "") {
      let fns = callbackOnAddRow.split(",");
      for (i = 0; i < fns.length; i++) {
        let fn = window[fns[i]];
        if (typeof fn === "function") fn();
        else {
          fn = fns[i];
          fn = new Function(fn);
          fn.call(this);
        }
      }
    }

    if (callback != "") {
      if (RowIndex != "") Index = RowIndex;
      if (typeof callback === "function") {
        return callback(TRLen);
      }
      // find object
      var fn = window[callback];
      // is object a function?
      if (typeof fn === "function") {
        return fn(Index + 1);
      }

      var $currentRow = $(THIS).closest("tr");
      var _Index = $currentRow.index(`#${TblID} tbody tr`);
      if (_Index < 0) _Index = 0;
      var fns = callback.split(",");
      for (i = 0; i < fns.length; i++) {
        let fn = window[fns[i]];
        var _i = 0;
        //console.log('fn', fn, 'Index', Index + 1, 'TRLen', TRLen, '_Index', _Index);
        if (typeof fn === "function") {
          //fn(Index + 1);
          //console.log(Index, _Index)
          if (_Index == 0) fn(_Index);

          if (Index >= 0 && TRLen >= 1) {
            _i = Index + 1;
            fn(_i);
          }
        } else {
          fn = fns[i];
          fn = new Function(fn);
          fn.call(this);
        }
      }
    }
  }
}

//async function AddTableRow(
//    inputName,
//    TblID,
//    TRName,
//    TblRow,
//    TDNoName,
//    THIS,
//    callback,
//    SQL,
//    SQLCallBack,
//    DisplayOnFiled,
//    callbackOnAddRow,
//    isTagsInput,
//    onTagsInputAddedFunction,
//    RowFields,
//    RowIndex
//) {

//    // ================= DEFAULTS =================
//    callback ??= "";
//    SQL ??= "";
//    SQLCallBack ??= "";
//    DisplayOnFiled ??= "";
//    callbackOnAddRow ??= "";
//    isTagsInput ??= "";
//    onTagsInputAddedFunction ??= "";
//    RowFields ??= [];
//    RowIndex ??= "";

//    const $table = $("#" + TblID);
//    const $tbody = $table.find("tbody");
//    const $currentRow = $(THIS).closest("tr");

//    // SAFE row index (thead-proof)
//    let Index = $currentRow.index(`#${TblID} tbody tr`);

//    let TRLen = $tbody.find(`tr[name="${TRName}"]`).length;
//    let inName = inputName ? inputName.split(",") : [];

//    // ================= ADD ROW ONLY IF LAST =================
//    if ((Index + 1) !== TRLen && Index !== -1) return;

//    if (Index === -1) {
//        Index = 0;
//        $tbody.append(TblRow);
//    } else {
//        $tbody.find("tr:last").after(TblRow);
//    }

//    $(".tt-menu").hide();

//    // ================= AUTO-ADD ROW ON FOCUS =================
//    if (inputName) {
//        setTimeout(() => {
//            inName.forEach(name => {
//                let selector = `input[name="${name.trim()}"], textarea[name="${name.trim()}"]`;

//                $table
//                    .off("focus.addRow", selector)
//                    .on("focus.addRow", selector, function () {
//                        AddTableRow(
//                            inputName, TblID, TRName, TblRow, TDNoName,
//                            this, callback, SQL, SQLCallBack, DisplayOnFiled,
//                            callbackOnAddRow, isTagsInput, onTagsInputAddedFunction,
//                            RowFields
//                        );
//                    });
//            });
//        }, 100);
//    }

//    // ================= RENUMBER ROW =================
//    $tbody.find(`tr[name="${TRName}"]`).each(function (i) {
//        $(this).find(`td[name="${TDNoName}"]`).html(i + 1);
//    });

//    // ================= AUTOCOMPLETE =================
//    if (SQL) {
//        inName.forEach(name => {
//            let ID = `input[name="${name.trim()}"]`;
//            SetAutoCompleteAndCallBack(
//                ID,
//                SQL,
//                "",
//                SQLCallBack || "",
//                DisplayOnFiled || "",
//                "",
//                "",
//                true,
//                callbackOnAddRow || ""
//            );
//        });
//    }

//    // ================= ROW FIELDS =================
//    if (RowFields.length) {
//        RowFields.forEach(item => {
//            if (!item.Value) return;

//            let $input = $tbody.find(`input[name="${item.Value}"]`).eq(TRLen);
//            if (!$input.length) return;

//            $input.on("focus", function () { this.select(); });

//            if (item.SetValue?.trim()) {
//                $input.val(item.SetValue);
//            }

//            if (item.Format === "Unformat") {
//                let autoOpt = item.ISInteger === "True"
//                    ? { vMin: "0", vMax: "999999999" }
//                    : { vMin: "-999999999", vMax: "999999999999" };

//                $input.autoNumeric(autoOpt);

//                $input.on("keydown", function (e) {
//                    let $inputs = $(`input[name="${item.Value}"]`);
//                    let idx = $inputs.index(this);

//                    if (e.which === 40) {
//                        $inputs.eq(idx + 1).focus().select();
//                        e.preventDefault();
//                    }
//                    if (e.which === 38) {
//                        $inputs.eq(idx - 1).focus().select();
//                        e.preventDefault();
//                    }
//                });
//            }

//            // FILE INPUT
//            if ($input.attr("type") === "file") {
//                $input.fileinput({
//                    theme: 'fa5',
//                    showCaption: false,
//                    showPreview: false,
//                    inputGroupClass: "input-group-sm",
//                    dropZoneEnabled: false,
//                    allowedFileTypes: ['image'],
//                    showCancel: false,
//                    showUpload: false,
//                });

//                $input.on("change", function (e) {
//                    let dt = new DataTransfer();
//                    [...e.target.files].forEach(file => {
//                        let ext = file.name.split('.').pop();
//                        let name = file.name.replace(/\W/g, '').slice(0, 10);
//                        dt.items.add(new File([file], `${name}_${Date.now()}.${ext}`));
//                    });
//                    this.files = dt.files;
//                });
//            }
//        });
//    }

//    // ================= TAGS INPUT =================
//    if (isTagsInput) {
//        let $tag = $(`input[name="${isTagsInput}"]`);
//        if (!$tag.data("tagsinput")) {
//            $tag.tagsinput({ trimValue: true, tagClass: 'badge bg-primary' });
//        }

//        if (onTagsInputAddedFunction) {
//            $tag.off(".tags").on("itemAdded.tags itemRemoved.tags", function (e) {
//                let fn = window[onTagsInputAddedFunction];
//                if (typeof fn === "function") fn(e, this);
//            });
//        }
//    }

//    // ================= CALLBACKS =================
//    function runCallbacks(cbs, index) {
//        if (!cbs) return;
//        cbs.split(",").forEach(fnName => {
//            let fn = window[fnName.trim()];
//            if (typeof fn === "function") fn(index);
//        });
//    }

//    runCallbacks(callbackOnAddRow, TRLen);

//    if (callback) {
//        let idx = RowIndex !== "" ? RowIndex : Index + 1;
//        runCallbacks(callback, idx);
//    }
//}

function RemoveTableRow(
  TRName,
  TDNoName,
  THIS,
  RemoveCallBack,
  inputName,
  callbackfunction,
) {
  // ================= DEFAULTS =================
  callbackfunction =
    typeof callbackfunction == "undefined" ? "" : callbackfunction;
  RemoveCallBack = typeof RemoveCallBack == "undefined" ? "" : RemoveCallBack;
  inputName = typeof inputName == "undefined" ? "" : inputName;

  // ================= SAFE INDEX (thead-proof) =================
  var $row = $(THIS).closest("tr");
  var index = $row.index(`tr[name="${TRName}"]`);

  var $rows = $(`tr[name="${TRName}"]`);
  var TRLen = $rows.length;

  // ================= FOCUS WHEN LAST ROW =================
  if (TRLen === 1) {
    $(`input[name="${inputName}"]`).eq(0).focus();
    $(`textarea[name="${inputName}"]`).eq(0).focus();
  }

  // ================= REMOVE ROW =================
  $rows.eq(index).remove();

  // ================= RE-NUMBER =================
  $(`tr[name="${TRName}"]`).each(function (i) {
    $(this)
      .find(`td[name="${TDNoName}"]`)
      .html(i + 1);
  });

  // ================= REMOVE CALLBACK =================
  if (typeof RemoveCallBack === "function") {
    if ($(`tr[name="${TRName}"]`).length === 0) {
      RemoveCallBack();
    }
  }

  // ================= STRING CALLBACK =================
  if (callbackfunction !== "") {
    var fn = window[callbackfunction];
    if (typeof fn === "function") fn();
  }
}

//function RemoveTableRow(TRName, TDNoName, THIS, RemoveCallBack, inputName, callbackfunction) {
//    callbackfunction = (typeof callbackfunction == 'undefined') ? "" : callbackfunction;
//    RemoveCallBack = (typeof RemoveCallBack == 'undefined') ? "" : RemoveCallBack;
//    inputName = (typeof inputName == 'undefined') ? "" : inputName;
//    var index = $(THIS).closest("tr").index();//$(THIS).parent().parent().parent().index();
//    var TRLen = $("tr[name=" + TRName + "]").length;
//    if (TRLen == 1) {
//        $("input[name=" + inputName + "]").eq(0).focus();
//        $("textarea[name=" + inputName + "]").eq(0).focus();
//    }
//    $("tr[name=" + TRName + "]").eq(index).remove();
//    $("tr[name=" + TRName + "]").each(function (Index) {
//        $("td[name=" + TDNoName + "]").eq(Index).html(Index + 1);
//    });
//    if (typeof RemoveCallBack === 'function') {
//        var TRLen = $("tr[name=" + TRName + "]").length;
//        if (TRLen == 0) {
//            RemoveCallBack();
//        }
//    }
//    if (callbackfunction != "") {
//        var fn = window[callbackfunction];
//        // is object a function?
//        if (typeof fn === "function") fn();
//    }
//}
function RemoveAllTableRow(TRName, InputName) {
  var index = $("tr[name=" + TRName + "]:last").index();
  $("input[name=" + InputName + "]")
    .eq(index)
    .focus();
  $("tr[name=" + TRName + "]:not(:last)").remove();
  $("tr[name=" + TRName + "] td:eq(0)").html(1);
}
var RowFields = [];
function addRowFiled(
  FieldName,
  Value,
  Required,
  Format,
  TableName,
  RequiredInputIfOtherFieldIsNotNull,
  AutoGenerateBarcodeNumberIfNotEntry,
  AutoGenerateBarcodeNumberDigit,
  DestinationFile,
  ISInteger,
  SetValue,
  ISSummerNote,
  GetValueFromTextareaName,
  IsKendoMultiColumnComboBox,
) {
  Required = typeof Required == "undefined" ? "" : Required;
  Format = typeof Format == "undefined" ? "" : Format;
  TableName = typeof TableName == "undefined" ? "" : TableName;
  RequiredInputIfOtherFieldIsNotNull =
    typeof RequiredInputIfOtherFieldIsNotNull == "undefined"
      ? ""
      : RequiredInputIfOtherFieldIsNotNull;
  AutoGenerateBarcodeNumberIfNotEntry =
    typeof AutoGenerateBarcodeNumberIfNotEntry == "undefined"
      ? ""
      : AutoGenerateBarcodeNumberIfNotEntry;
  AutoGenerateBarcodeNumberDigit =
    typeof AutoGenerateBarcodeNumberDigit == "undefined"
      ? ""
      : AutoGenerateBarcodeNumberDigit;
  DestinationFile =
    typeof DestinationFile == "undefined" ? "" : DestinationFile;
  ISInteger = typeof ISInteger == "undefined" ? "" : ISInteger;
  SetValue = typeof SetValue == "undefined" ? "" : SetValue;
  ISSummerNote = typeof ISSummerNote == "undefined" ? "" : ISSummerNote;
  GetValueFromTextareaName =
    typeof GetValueFromTextareaName == "undefined"
      ? ""
      : GetValueFromTextareaName;
  IsKendoMultiColumnComboBox =
    typeof IsKendoMultiColumnComboBox == "undefined"
      ? ""
      : IsKendoMultiColumnComboBox;
  RowFields.push({
    FieldName: FieldName,
    Value: Value.toString().trim(),
    Required: Required,
    Format: Format,
    TableName: TableName,
    RequiredInputIfOtherFieldIsNotNull: RequiredInputIfOtherFieldIsNotNull,
    AutoGenerateBarcodeNumberIfNotEntry: AutoGenerateBarcodeNumberIfNotEntry,
    AutoGenerateBarcodeNumberDigit: AutoGenerateBarcodeNumberDigit,
    DestinationFile: DestinationFile,
    ISInteger: ISInteger,
    SetValue: SetValue,
    ISSummerNote: ISSummerNote,
    GetValueFromTextareaName: GetValueFromTextareaName,
    IsKendoMultiColumnComboBox: IsKendoMultiColumnComboBox,
  });
}

function getRowData(TRNAME, FieldName, TableName, InputName, HTMLTblID) {
  TableName = typeof TableName == "undefined" ? "" : TableName;
  InputName = typeof InputName == "undefined" ? "" : InputName;
  HTMLTblID = typeof HTMLTblID == "undefined" ? "" : HTMLTblID;
  //let TableName = "", TableName2 = "";
  let TableNames = "";
  if (TableName != "") TableNames = TableName.split(",");
  //var DFiled = CallBackToControlID.split(",");
  //for (i = 0; i < DFiled.length; i++) {
  var Data = [];
  var Message = "";
  var objValidate = new Object();
  var objValidateRequiredInputIfOtherFieldIsNotNull = new Object();
  var CountFiledNeedValidate = 0;
  var KeepCountFiledNeedValidate = 0;
  var Message_Emty_Data = "Please fill data as below: <br />";

  let RequiredOtherFieldIsNotNull = false;
  let RequiredOtherField_IsNotNull = false;
  let NeedCheckRequiredOtherFieldIsNotNull = false;
  let NeedCheckRequiredOtherFieldIs_NotNull = false;
  FieldName.forEach(function (item) {
    if (trim(item.Required) == "Required") {
      objValidate["" + item.FieldName + ""] = item.Value;
      Message_Emty_Data += item.FieldName + "<br />";
      CountFiledNeedValidate++;
      KeepCountFiledNeedValidate++;
    } else if (
      trim(item.RequiredInputIfOtherFieldIsNotNull) ==
      "RequiredInputIfOtherFieldIsNotNull"
    ) {
      objValidateRequiredInputIfOtherFieldIsNotNull["" + item.FieldName + ""] =
        item.Value;
      Message_Emty_Data += item.FieldName + "<br />";
      CountFiledNeedValidate++;
      KeepCountFiledNeedValidate++;
    }
  });
  var trID = "";
  if (HTMLTblID == "") trID = "tr[name=" + TRNAME + "]";
  else trID = "#" + HTMLTblID + " tr[name=" + TRNAME + "]";
  $(trID).each(function (index) {
    var obj = new Object();
    var IsNullFiled = 0;

    RequiredOtherFieldIsNotNull = false;
    NeedCheckRequiredOtherFieldIsNotNull = false;
    for (var key in objValidateRequiredInputIfOtherFieldIsNotNull) {
      RequiredOtherFieldIsNotNull = true;
      RequiredOtherField_IsNotNull = true;
      var value = $(
        (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
          "input[name=" +
          objValidateRequiredInputIfOtherFieldIsNotNull[key] +
          "]:eq(" +
          index +
          ")",
      ).val();
      if (typeof value == "undefined")
        value = $(
          (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
            "textarea[name=" +
            objValidateRequiredInputIfOtherFieldIsNotNull[key] +
            "]:eq(" +
            index +
            ")",
        ).val();
      if (value != "") {
        // || accounting.unformat(value) != 0 || accounting.unformat(value) != 1) {
        IsNullFiled++;
        NeedCheckRequiredOtherFieldIsNotNull = true;
        NeedCheckRequiredOtherFieldIs_NotNull = true;
        CountFiledNeedValidate = KeepCountFiledNeedValidate;
      }
    }
    if (!RequiredOtherFieldIsNotNull) {
      for (var key in objValidate) {
        var value = $(
          (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
            "input[name=" +
            objValidate[key] +
            "]:eq(" +
            index +
            ")",
        ).val();
        if (typeof value == "undefined")
          value = $(
            (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
              "textarea[name=" +
              objValidate[key] +
              "]:eq(" +
              index +
              ")",
          ).val();
        if (value != "") {
          // || accounting.unformat(value) != 0 || accounting.unformat(value) != 1) {
          IsNullFiled++;
        }
      }
    } else if (IsNullFiled == 0) CountFiledNeedValidate = 0;

    if (NeedCheckRequiredOtherFieldIsNotNull) {
      objValidate = objValidateRequiredInputIfOtherFieldIsNotNull;
    }

    if (
      (IsNullFiled != 0 && IsNullFiled == CountFiledNeedValidate) ||
      CountFiledNeedValidate == 0
    ) {
      if (TableNames != "") {
        for (i = 0; i < TableNames.length; i++) {
          var haveValue = false;
          var j = 0;
          var generatebarcodeIfazeo = false;
          FieldName.forEach(function (item) {
            haveValue = false;
            var a = Object.values(objValidate);
            if (a.length == 0) {
              var b = {};
              b = Object.values(item);
              if (b[1] != "") {
                a.push(b[1]);
                generatebarcodeIfazeo = true;
              }
            }
            if (
              a[0] &&
              a[0] != "" &&
              ($(
                (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                  "input[name=" +
                  a[0] +
                  "]:eq(" +
                  index +
                  ")",
              ).val() ||
                $(
                  (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                    "textarea[name=" +
                    a[0] +
                    "]:eq(" +
                    index +
                    ")",
                ) ||
                $(
                  (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                    "textarea[name=" +
                    a[0] +
                    "]:eq(" +
                    index +
                    ")",
                ).val())
            ) {
              haveValue = true;
              if (
                $(
                  (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                    "input[name=" +
                    a[0] +
                    "]:eq(" +
                    index +
                    ")",
                ).is("input")
              )
                a = $(
                  (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                    "input[name=" +
                    a[0] +
                    "]:eq(" +
                    index +
                    ")",
                ).val();
              else if (
                $(
                  (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                    "textarea[name=" +
                    a[0] +
                    "]:eq(" +
                    index +
                    ")",
                ).is("textarea")
              )
                a = $(
                  (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                    "textarea[name=" +
                    a[0] +
                    "]:eq(" +
                    index +
                    ")",
                ).val();

              if (a != "") {
                let tblName = item.TableName.split(",");
                tblName =
                  typeof tblName[i] == "undefined" ? "" : tblName[i].trim();
                //console.log(tblName, TableNames[i].trim());
                //console.log('TableNames[i].trim()', TableNames[i].trim(), TableNames);
                if (tblName == TableNames[i].trim()) {
                  //console.log(item.Value)
                  if (
                    trim(item.SetValue) == "" &&
                    trim(item.ISSummerNote).toLowerCase() == "true"
                  ) {
                    var value = $(
                      (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                        "div[name=" +
                        item.Value +
                        "]:eq(" +
                        index +
                        ")",
                    ).summernote("code");
                    obj["" + item.FieldName + ""] = encodeURIComponent(
                      nl2br(value),
                    );
                    if (value != "") j++;
                  } else if (
                    trim(item.SetValue) == "" &&
                    trim(item.GetValueFromTextareaName).toLowerCase() == "true"
                  ) {
                    var value = $(
                      (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                        "textarea[name=" +
                        item.Value +
                        "]:eq(" +
                        index +
                        ")",
                    ).val();
                    obj["" + item.FieldName + ""] = encodeURIComponent(
                      nl2br(value),
                    );
                    if (value != "") j++;
                  } else if (trim(item.SetValue) != "") {
                    if (trim(item.Format) == "IsDate") {
                      var value = StringToDate(item.SetValue);
                      obj["" + item.FieldName + ""] = value;
                      if (value != "") j++;
                    } else if (trim(item.Format) == "Unformat") {
                      var value = item.SetValue;
                      obj["" + item.FieldName + ""] =
                        accounting.unformat(value);
                      if (value != "") j++;
                    } else if (
                      trim(item.GetValueFromTextareaName).toLowerCase() ==
                      "true"
                    ) {
                      obj["" + item.FieldName + ""] = encodeURIComponent(
                        nl2br(item.SetValue),
                      );
                      if (value != "") j++;
                    } else if (item.Value == "") {
                      obj["" + item.FieldName + ""] = item.SetValue;
                      if (value != "") j++;
                    } else {
                      $(
                        (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                          "input[name=" +
                          item.Value +
                          "]:eq(" +
                          index +
                          ")",
                      ).val(item.SetValue);
                      var value = $(
                        (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                          "input[name=" +
                          item.Value +
                          "]:eq(" +
                          index +
                          ")",
                      ).val();
                      obj["" + item.FieldName + ""] = value;
                      if (value != "") j++;
                    }
                  } else if (trim(item.Format) == "Unformat") {
                    var value = $(
                      (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                        "input[name=" +
                        item.Value +
                        "]:eq(" +
                        index +
                        ")",
                    ).val();
                    obj["" + item.FieldName + ""] = accounting.unformat(value);
                    if (value != "") j++;
                  } else if (trim(item.Format) == "IsDate") {
                    if (item.Value != "") {
                      if (
                        $(
                          (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                            "input[name=" +
                            item.Value +
                            "]:eq(" +
                            index +
                            ")",
                        ).val() != ""
                      ) {
                        var value = $(
                          (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                            "input[name=" +
                            item.Value +
                            "]:eq(" +
                            index +
                            ")",
                        ).val();
                        obj["" + item.FieldName + ""] = StringToDate(value);
                        if (value != "") j++;
                      }
                    } else {
                      if (
                        $(
                          (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                            "input[name=" +
                            item.FieldName +
                            "]:eq(" +
                            index +
                            ")",
                        ).val() != ""
                      ) {
                        var value = $(
                          (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                            "input[name=" +
                            item.FieldName +
                            "]:eq(" +
                            index +
                            ")",
                        ).val();
                        obj["" + item.FieldName + ""] = StringToDate(value);
                        if (value != "") j++;
                      }
                    }
                  } else {
                    var value = "";
                    if (
                      item.Value != "" &&
                      $(
                        (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                          "input[name=" +
                          item.Value +
                          "]:eq(" +
                          index +
                          ")",
                      ).is("input[type='checkbox']")
                    ) {
                      if (
                        $(
                          (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                            "input[name=" +
                            item.Value +
                            "]:eq(" +
                            index +
                            ")",
                        ).is(":checked")
                      ) {
                        value = $(
                          (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                            "input[name=" +
                            item.Value +
                            "]:eq(" +
                            index +
                            ")",
                        ).val();
                      } else value = "";
                    } else if (
                      item.Value != "" &&
                      $(
                        (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                          "input[name=" +
                          item.Value +
                          "]:eq(" +
                          index +
                          ")",
                      ).is("input")
                    )
                      value = $(
                        (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                          "input[name=" +
                          item.Value +
                          "]:eq(" +
                          index +
                          ")",
                      )
                        .val()
                        .toString()
                        .trim();
                    else if (
                      item.Value != "" &&
                      $(
                        (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                          "textarea[name=" +
                          item.Value +
                          "]:eq(" +
                          index +
                          ")",
                      ).is("textarea")
                    )
                      value = nl2br(
                        $(
                          (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                            "textarea[name=" +
                            item.Value +
                            "]:eq(" +
                            index +
                            ")",
                        ).val(),
                      );
                    else if (
                      item.Value != "" &&
                      $(
                        (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                          "select[name=" +
                          item.Value +
                          "]:eq(" +
                          index +
                          ")",
                      ).is("select")
                    )
                      value = $(
                        (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                          "select[name=" +
                          item.Value +
                          "]:eq(" +
                          index +
                          ")",
                      ).val();
                    value = encodeURIComponent(value);
                    obj["" + item.FieldName + ""] = value;
                    if (value != "") j++;
                  }

                  if (item.AutoGenerateBarcodeNumberIfNotEntry != "") {
                    if (value == "") {
                      let d = 0;
                      if (d == 0) d = new Date().getTime();
                      else
                        d += parseInt(d) + parseInt(Math.random() * 100000 + 1);
                      d = parseInt(d) + parseInt(Math.random() * 100000 + 1);
                      for (let j = 0; j <= 10; j++) {
                        d += parseInt(
                          parseInt(d) * parseInt(Math.random() * 100 + j),
                        );
                      }
                      let result = d.toString();
                      result = result
                        .substring(0, item.AutoGenerateBarcodeNumberDigit)
                        .replace(".", 9);
                      $(
                        (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                          "input[name=" +
                          item.Value +
                          "]:eq(" +
                          index +
                          ")",
                      ).val(result);
                      obj["" + item.FieldName + ""] = result;
                    }
                  }
                  if (item.Value != "") {
                    let type = $(
                      (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                        "input[name=" +
                        item.Value +
                        "]",
                    )
                      .eq(index)
                      .attr("type");
                    let fileName = "";
                    if (type == "file") {
                      if (
                        $(
                          (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                            "input[name=" +
                            item.Value +
                            "]",
                        )
                          .eq(index)
                          .prop("files")[0]
                      ) {
                        let files = $(
                          (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                            "input[name=" +
                            item.Value +
                            "]",
                        )
                          .eq(index)
                          .prop("files");
                        $.each(files, function (key, value) {
                          if (fileName == "") fileName = value.name;
                          else fileName += "~" + value.name;
                        });
                        obj["" + item.FieldName + ""] = fileName;
                        if (item.DestinationFile != "")
                          obj["DestinationFolder"] = item.DestinationFile;
                      }
                    }
                  }
                }
              }
            } else {
              haveValue = true;
              let value = trim(item.SetValue);
              //if (item.Value != "" && item.SetValue == "" && trim(item.GetValueFromTextareaName).toLowerCase() == "true")
              //    value = $("text")
              let tblName = item.TableName.split(",");
              tblName =
                typeof tblName[i] == "undefined" ? "" : tblName[i].trim();
              if (tblName.trim() == TableNames[i].trim()) {
                if (trim(item.ISSummerNote).toLowerCase() == "true") {
                  obj["" + item.FieldName + ""] = encodeURIComponent(
                    nl2br(value),
                  );
                } else if (
                  trim(item.GetValueFromTextareaName).toLowerCase() == "true"
                ) {
                  obj["" + item.FieldName + ""] = encodeURIComponent(
                    nl2br(value),
                  );
                } else if (trim(item.Format) == "IsDate") {
                  obj["" + item.FieldName + ""] = StringToDate(value);
                } else if (trim(item.Format) == "Unformat") {
                  obj["" + item.FieldName + ""] = accounting.unformat(value);
                } else if (
                  trim(item.GetValueFromTextareaName).toLowerCase() == "true"
                ) {
                  obj["" + item.FieldName + ""] = encodeURIComponent(
                    nl2br(value),
                  );
                } else if (trim(item.Format) == "Unformat") {
                  obj["" + item.FieldName + ""] = accounting.unformat(value);
                } else if (trim(item.Format) == "IsDate") {
                  obj["" + item.FieldName + ""] = StringToDate(value);
                } else if (
                  item.Value != "" &&
                  $(
                    (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                      "input[name=" +
                      item.Value +
                      "]",
                  ).is("input[type='checkbox']")
                ) {
                  if (
                    $(
                      (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                        "input[name=" +
                        item.Value +
                        "]",
                    ).is(":checked")
                  ) {
                    value = $(
                      (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                        "input[name=" +
                        item.Value +
                        "]",
                    ).val();
                  } else value = "";
                  obj["" + item.FieldName + ""] = value;
                } else {
                  value = encodeURIComponent(value);
                  obj["" + item.FieldName + ""] = value;
                }
              }
            }
            if (generatebarcodeIfazeo) {
              if (item.AutoGenerateBarcodeNumberIfNotEntry != "") {
                if (
                  $(
                    (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                      "input[name=" +
                      a[0] +
                      "]:eq(" +
                      index +
                      ")",
                  ).val() == ""
                ) {
                  let d = 0;
                  if (d == 0) d = new Date().getTime();
                  else d += parseInt(d) + parseInt(Math.random() * 100000 + 1);
                  d = parseInt(d) + parseInt(Math.random() * 100000 + 1);
                  for (let j = 0; j <= 10; j++) {
                    d += parseInt(
                      parseInt(d) * parseInt(Math.random() * 100 + j),
                    );
                  }
                  let result = d.toString();
                  result = result
                    .substring(0, item.AutoGenerateBarcodeNumberDigit)
                    .replace(".", 9);
                  $(
                    (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                      "input[name=" +
                      a[0] +
                      "]:eq(" +
                      index +
                      ")",
                  ).val(result);
                  obj["" + item.FieldName + ""] = result;
                }
              }
            }
          });
          if (haveValue && j > 0) {
            let objDt = {};
            objDt[TableNames[i].trim()] = obj;
            obj = {};
            Data.push(objDt);
          }
        }
      } else {
        FieldName.forEach(function (item) {
          if (trim(item.Format) == "Unformat") {
            if (item.SetValue != "" && item.Value == "") {
              var value = item.SetValue;
              obj["" + item.FieldName + ""] = accounting.unformat(value);
            } else {
              var value = $(
                (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                  "input[name=" +
                  item.Value +
                  "]:eq(" +
                  index +
                  ")",
              ).val();
              obj["" + item.FieldName + ""] = accounting.unformat(value);
            }
          } else if (trim(item.Format) == "IsDate") {
            if (item.SetValue != "" && item.Value == "") {
              var value = item.SetValue;
              obj["" + item.FieldName + ""] = StringToDate(value);
            } else {
              if (
                $(
                  (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                    "input[name=" +
                    item.Value +
                    "]:eq(" +
                    index +
                    ")",
                ).val() != ""
              ) {
                var value = $(
                  (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                    "input[name=" +
                    item.Value +
                    "]:eq(" +
                    index +
                    ")",
                ).val();
                obj["" + item.FieldName + ""] = StringToDate(value);
              }
            }
          } else {
            var value = "";
            if (
              $(
                (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                  "input[name=" +
                  item.Value +
                  "]:eq(" +
                  index +
                  ")",
              ).is("input[type='checkbox']")
            ) {
              if (
                $(
                  (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                    "input[name=" +
                    item.Value +
                    "]:eq(" +
                    index +
                    ")",
                ).is(":checked")
              ) {
                value = $(
                  (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                    "input[name=" +
                    item.Value +
                    "]:eq(" +
                    index +
                    ")",
                ).val();
              } else value = "";
            } else if (
              $(
                (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                  "input[name=" +
                  item.Value +
                  "]:eq(" +
                  index +
                  ")",
              ).is("input")
            )
              value = $(
                (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                  "input[name=" +
                  item.Value +
                  "]:eq(" +
                  index +
                  ")",
              ).val();
            else if (
              $(
                (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                  "textarea[name=" +
                  item.Value +
                  "]:eq(" +
                  index +
                  ")",
              ).is("textarea")
            )
              value = nl2br(
                $(
                  (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                    "textarea[name=" +
                    item.Value +
                    "]:eq(" +
                    index +
                    ")",
                ).val(),
              );
            else if (
              $(
                (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                  "select[name=" +
                  item.Value +
                  "]:eq(" +
                  index +
                  ")",
              ).is("select")
            )
              value = $(
                (HTMLTblID == "" ? "" : "#" + HTMLTblID + " ") +
                  "select[name=" +
                  item.Value +
                  "]:eq(" +
                  index +
                  ")",
              ).val();
            if (item.SetValue != "") value = item.SetValue;
            value = encodeURIComponent(value);
            obj["" + item.FieldName + ""] = value;
          }
        });
        Data.push(obj);
      }
    } else if (IsNullFiled != 0) {
      for (var key in objValidate) {
        let selector =
          (HTMLTblID ? "#" + HTMLTblID + " " : "") +
          "[name=" +
          objValidate[key] +
          "]";

        // get current value (input or textarea at given index)
        let value = $(selector).eq(index).val();

        // reference all table rows
        if (HTMLTblID == "") break;
        let rows = $("#" + HTMLTblID + " tbody tr");

        // find inputs/textareas inside last row
        let lastValue = rows
          .last()
          .find(
            "input[name=" +
              objValidate[key] +
              "], textarea[name=" +
              objValidate[key] +
              "]",
          )
          .val();

        // if last row exists and is empty → just return
        if (rows.length > 1 && lastValue === "") {
          return;
        }
        // validation
        if (!value) {
          Data = [];
          Message = "Please enter value: " + key;
          DialogSweetAlert("error", Message, function () {
            let target = $(selector).eq(index);
            if (target.length) {
              target.focus();
            }
          });
          Data = false;
          return false;
        }
      }
      if (Message != "") {
        Data = false;
        return false;
      }
    }
  });
  if (CountFiledNeedValidate > 0 && Data.length == 0) {
    DialogSweetAlert("nopermission", Message_Emty_Data);
    Data = false;
    return false;
  }
  if (!NeedCheckRequiredOtherFieldIs_NotNull && RequiredOtherField_IsNotNull)
    Data = [];
  return Data;
}
function openfile(fileName) {
  window.open(fileName, "_blank");
}

function LoadURLData(DIVID, URL, DATA, ASYNC, CallBackFunction, dataType) {
  DIVID = typeof DIVID == "undefined" ? "" : DIVID;
  URL = typeof URL == "undefined" ? "" : URL;
  DATA = typeof DATA == "undefined" ? "" : DATA;
  ASYNC = typeof ASYNC == "undefined" ? true : false;
  dataType = typeof dataType == "undefined" ? "json" : "html";
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  //DialogMessageBox("Loading Data...");
  //$("#" + DIVID).html('<div align="center" style="width:100%"><img src= "' + ServerRoot + '/Content/images/loadinfo.net (1).gif" /></div>');
  DialogSweetAlert("load");
  if (DATA != "") DATA = { Data: JSON.stringify(DATA) };
  var url = ServerRoot + URL;
  $.ajax({
    url: url,
    dataType: dataType,
    type: "POST",
    data: DATA,
    success: function (responseText) {
      DialogSweetAlert("loadsuccess");
      if (typeof CallBackFunction === "function") {
        CallBackFunction(responseText);
      } else {
        $("#" + DIVID).html(responseText);
      }
      //$("#DialogMessageBox").dialog("destroy");
    },
    error: function (XMLHttpRequest, callStatus, errorThrown) {
      //$("#DialogMessageBox").dialog("destroy");
      //alert(callStatus + '. NO RECORD.');
      DialogSweetAlert("error");
    },
  });
}
function SaveChageDialogAjaxForm(
  FormName,
  URL,
  DATA,
  Message,
  CallBackFunction,
  Getresponetext,
) {
  FormName = typeof FormName == "undefined" ? "" : FormName;
  Message = typeof Message == "undefined" ? "Saving Data..." : Message;
  Getresponetext = typeof Getresponetext == "undefined" ? false : true;
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  $("form[name=" + FormName + "]").ajaxSubmit({
    url: ServerRoot + URL,
    dataType: "json",
    type: "POST",
    data: { Data: JSON.stringify(DATA) },
    beforeSend: function () {
      DialogMessageBox(Message, "True");
    },
    uploadProgress: function (event, position, total, percentComplete) {
      var percentValue = percentComplete + "%";
      $("#progressBar").animate(
        {
          width: "" + percentValue + "",
        },
        {
          duration: 5000,
          easing: "linear",
          step: function (x) {
            percentText = Math.round((x * 100) / percentComplete);
            $("#percent").text(percentText + "%");
            if (percentText == "100") {
            }
          },
        },
      );
    },
    complete: function (response) {
      $("#DialogMessageBox").dialog("destroy");
      if (typeof CallBackFunction === "function") {
        if (Getresponetext) CallBackFunction(response.responseJSON);
        else CallBackFunction();
      }
    },
    success: function (response) {
      DialogMessageBox("Save Sucessfull.");
    },
    error: function (XMLHttpRequest, callStatus, errorThrown) {
      $("#DialogMessageBox").dialog("destroy");
      alert(callStatus);
    },
  });
}
function DialogSweetAlert(type, message = "", callback = null) {
  const isFn = typeof callback === "function";

  function cleanUp() {
    Swal.hideLoading();
    Swal.close();
    $("#DialogMessageBox, .swal2-container, #toast-container").remove();
    $("button").removeAttr("disabled");
  }

  function runCallback() {
    if (isFn) callback();
  }

  Swal.close();

  switch (type) {
    case "load":
      Swal.fire({
        title: message || "Loading...",
        html: '<div class="swal-inline-spinner"></div>',
        allowEscapeKey: false,
        allowOutsideClick: false,
        showConfirmButton: false,
        customClass: { popup: "swal2-loading-popup" },
        didOpen: () => {
          setTimeout(runCallback, 300);
        },
      });
      break;

    case "loadsuccess":
      Swal.fire({
        icon: "success",
        title: message || "Success",
        timer: 1400,
        showConfirmButton: false,
        didClose: runCallback,
      });
      break;

    case "error":
      Swal.fire({
        icon: "error",
        title: "Oops...",
        html: message || "Something went wrong!",
        showConfirmButton: true,
        didClose: runCallback,
      });
      break;

    case "warning":
      Swal.fire({
        icon: "warning",
        html: message || "Something went wrong!",
        showConfirmButton: true,
        didClose: runCallback,
      });
      break;

    case "deleteted":
      Swal.fire({
        icon: "success",
        title: "Deleted!",
        html: message || "Your data has been deleted.",
        timer: 1500,
        position: "top",
        showConfirmButton: false,
      });
      break;

    case "Savesuccess":
      toastr.success(message || "Data saved successfully");

      setTimeout(() => {
        cleanUp();
        runCallback();
      }, 2000);
      break;

    case "success":
      toastr.success(message || "Saved successfully");
      break;

    case "message":
      Swal.fire({
        icon: "info",
        html: message || "Information message",
        showConfirmButton: true,
      });
      break;

    case "loginsuccess":
      Swal.mixin({
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer: 2500,
        timerProgressBar: true,
      }).fire({
        icon: "success",
        title: "Signed in successfully",
      });
      break;

    case "nopermission":
      Swal.fire({
        icon: "error",
        title: "Access Denied",
        html: message || "You do not have permission",
        showConfirmButton: true,
      });
      break;

    case "close":
      cleanUp();
      break;

    case "ToastrLoading":
      toastr.info(message || "Loading...");
      break;
  }
}

function DialogSweetCofirm(
  YesFunction,
  NoFunction,
  Title,
  confirmButtonText,
  InputText,
  MultipleInputs,
  CallBackFunction,
  IspreDeny,
  IspreConfirm,
) {
  InputText = typeof InputText == "undefined" ? "" : InputText;
  MultipleInputs = typeof MultipleInputs == "undefined" ? "" : MultipleInputs;
  NoFunction = typeof NoFunction == "undefined" ? "" : NoFunction;
  YesFunction = typeof YesFunction == "undefined" ? "" : YesFunction;
  Title = typeof Title == "undefined" ? "Want to save change?" : Title;
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  confirmButtonText =
    typeof confirmButtonText == "undefined"
      ? "Yes, delete it!"
      : confirmButtonText;
  IspreDeny = typeof IspreDeny == "undefined" ? false : IspreDeny;
  IspreConfirm = typeof IspreConfirm == "undefined" ? true : IspreConfirm;
  if (confirmButtonText == "") confirmButtonText = "Yes, delete it!";
  const swalWithBootstrapButtons = Swal.mixin({
    customClass: {
      confirmButton: "btn btn-success",
      cancelButton: "btn btn-danger",
    },
    buttonsStyling: false,
  });
  Swal.fire({
    position: "top",
    title: Title,
    text: InputText,
    html: MultipleInputs,
    icon: "question",
    showCancelButton: true,
    confirmButtonColor: "#007BFF",
    cancelButtonColor: "#DC3545",
    cancelButtonText: "No, cancel!",
    confirmButtonText: confirmButtonText,
    allowEscapeKey: false,
    allowOutsideClick: false,
    returnInputValueOnDeny: IspreDeny,
    didOpen: function () {
      if (CallBackFunction != "") return CallBackFunction();
    },
    //reverseButtons: true
    preDeny: (value) => {
      //let InvoiceComment = Swal.getPopup().querySelector('#TxtInvoiceComment').value;
      if (IspreDeny) {
        if (value == "") {
          Swal.showValidationMessage("You need to write something!");
          // Set a return here to prevent any further progress
          return false;
        }
      }
    },
    preConfirm: (value) => {
      return IspreConfirm;
    },
  }).then((result) => {
    if (result.value) {
      if (typeof YesFunction === "function") {
        YesFunction();
      }
      //swalWithBootstrapButtons.fire(
      //    'Deleted!',
      //    'Your file has been deleted.',
      //    'success'
      //)
    } else if (
      /* Read more about handling dismissals below */
      result.dismiss === Swal.DismissReason.cancel
    ) {
      swalWithBootstrapButtons.fire({
        title: "Cancelled",
        text: "",
        icon: "error",
        showConfirmButton: false,
        position: "top",
        timer: 1000,
      });
      if (typeof NoFunction === "function") {
        NoFunction();
      }
    }
  });
}
function CloseSweetAlert() {
  $("#DialogMessageBox").remove();
  SweetAlert.close();
}
function nl2br(str) {
  if (str == null) str = "";
  str = typeof str == "undefined" ? "" : str.toString();
  if (str != "" && str != "null")
    return str.replace(/(?:\r\n|\r|\n)/g, "&#013;&#010;"); //'&lt;br&gt;'
  else return str;
}

function EscapeXml(str) {
  if (str == "" || str == null) return str;
  if (str.replace) {
    str = str
      .replace(/&amp;/g, "&")
      .replace(/&#39;/g, "'")
      .replace(/&gt;/g, ">")
      .replace(/&lt;/g, "<");
    return str.replace(/&#013;&#010;/g, "\r\n");
  } else return str;
}
function DoSaveChangeData(
  endPointName,
  DATA,
  CallBackFunction,
  UploadFileOnCompleted,
  DialogSweetAlertMessageSaving,
  DialogSweetAlertMessageCompleted,
) {
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  UploadFileOnCompleted =
    typeof UploadFileOnCompleted == "undefined" ? "" : UploadFileOnCompleted;
  DialogSweetAlertMessageSaving =
    typeof DialogSweetAlertMessageSaving == "undefined"
      ? "Saving..."
      : DialogSweetAlertMessageSaving;
  DialogSweetAlertMessageCompleted =
    typeof DialogSweetAlertMessageCompleted == "undefined"
      ? "Your data has been saved."
      : DialogSweetAlertMessageCompleted;
  $.ajax({
    url: ServerRoot + endPointName,
    headers: { Authorization: "Bearer " + access_Token },
    //contentType: "application/json; charset=utf-8",
    dataType: "json",
    async: true,
    cache: false,
    type: "POST",
    data: DATA,
    complete: function (response) {
      response = JSON.parse(response.responseText);
      if (response.Result == 2) {
        if (UploadFileOnCompleted == "")
          DialogSweetAlert("loadsuccess", DialogSweetAlertMessageCompleted);
        if (typeof CallBackFunction === "function" && CallBackFunction != "") {
          CallBackFunction(response);
        }
      } else DialogSweetAlert("error", response.Message);

      //var InvoiceData = responseData.InvoiceData;
      //var CustomerName = responseData.CustomerName;
      //var InvoiceNo = responseData.InvoiceNo;
      //$("#BoyMasterPage").append(InvoiceData);
      //HTMLToPDF("div_Cust_Invoice", "CustomerInvoice.pdf", function (pdfData) {
      //    $("#div_Cust_Invoice").remove();
      //    var DATA = { CustomerName: CustomerName, InvoiceNo: InvoiceNo, pdfData: pdfData };
      //    UploadFile("CustomerInvoice/SaveInvoiceToPDF", DATA, function (response) {
      //        //console.log(response);
      //    });
      //});
    },
    beforeSend: function (xhr) {
      DialogSweetAlert("load", DialogSweetAlertMessageSaving);
      $("button").attr("disabled", true);
      xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
    },
    success: function (response) {
      $("button").attr("disabled", false);
      $("button").removeAttr("disabled");
      if (UploadFileOnCompleted != "") {
        if (response.Data != "") {
          var Data = JSON.parse(response.Data);
          $.each(Data, function (key, value) {
            var CustomerID = value[key].CustomerID;
            var InvoiceNo = value[key].InvoiceNumber;
            var IsIncludedVAT = value[key].IsIncludedVAT;
            var StatusID = value[key].StatusID;
            var SystemID = value[key].SystemID;
            var WarehouseID = value[key].WarehouseID;
            var DomainID = value[key].DomainID;
            var CustomerName = value[key].CustomerName;
            var dt = {};
            dt = {
              CustomerID: CustomerID,
              InvoiceNo: InvoiceNo,
              IsIncludedVAT: IsIncludedVAT,
              StatusID: StatusID,
              SystemID: SystemID,
              WarehouseID: WarehouseID,
              CustomerName: CustomerName,
              DomainID: DomainID,
            };
            var Data = [];
            Data = { Data: JSON.stringify(dt) };
            PostData("CustomerInvoice/InvoiceToPDF", Data, function (response) {
              response = JSON.parse(response);
              var InvoiceData =
                '<div id=\"div_Cust_Invoice' +
                key +
                '\" class=\"bg-white p-2\">';
              InvoiceData += response.InvData;
              InvoiceData += "</div>";
              var CustomerName = response.CustomerName;
              $("#BoyMasterPage").append(InvoiceData);
              $("#div_Cust_Invoice" + key).css("visibility", "hidden");
              HTMLToPDF(
                "div_Cust_Invoice" + key,
                "CustomerInvoice.pdf",
                function (pdfData) {
                  $("#div_Cust_Invoice" + key).remove();
                  var DATA = {
                    CustomerName: CustomerName,
                    InvoiceNo: InvoiceNo,
                    pdfData: pdfData,
                  };
                  UploadFile(
                    "CustomerInvoice/SaveInvoiceToPDF",
                    DATA,
                    function (response) {
                      //console.log(response);
                    },
                  );
                },
              );
            });
          });
        }
      }
    },
    error: function (response, callStatus, errorThrown) {
      $("button").attr("disabled", false);
      $("button").removeAttr("disabled");
      DialogSweetAlert("error", response.status + " " + response.statusText);
    },
  })
    .done(function (response) {})
    .then(function (response) {
      $("button").attr("disabled", false);
    });
}
function DoSaveChageAjaxForm(
  FormName,
  URL,
  DATA,
  Message,
  CallBackFunction,
  Getresponetext,
  RestFields,
  FieldIDFocus,
) {
  FormName = typeof FormName == "undefined" ? "" : FormName;
  Message = typeof Message == "undefined" ? "Saving Data..." : Message;
  RestFields = typeof RestFields == "undefined" ? "" : RestFields;
  FieldIDFocus = typeof FieldIDFocus == "undefined" ? "" : FieldIDFocus;
  if (Message == "") Message = "Saving Data...";
  Getresponetext = typeof Getresponetext == "undefined" ? false : true;
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;

  var showtoastr = false;
  var toast = "";
  SaveChangeResponse = $("form[name=" + FormName + "]").serializeArray();
  $("form[name=" + FormName + "]").ajaxSubmit({
    url: ServerRoot + URL,
    headers: { Authorization: "Bearer " + access_Token },
    contentType: "application/x-www-form-urlencoded; charset=UTF-8",
    dataType: "json",
    enctype: "multipart/form-data",
    async: true,
    cache: false,
    type: "POST",
    data: DATA,
    beforeSend: function (xhr) {
      DialogSweetAlert("load", "Saving...");
      xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
    },
    uploadProgress: function (event, position, total, percentComplete) {
      var percentValue = percentComplete + "%";
      if (showtoastr) {
        toastr.remove();
        showtoastr = true;
        toastr.info("", "Progress: " + percentValue);
        $("#toast-container .toast-title").html("Progress: " + percentValue);
      }
    },
    complete: function (response) {
      toastr.remove();
      toastr.success("Your data has been saved.");
    },
    success: function (response) {
      SaveChangeResponse["Data"] = response;
      if (response.Result == 2) {
        if (typeof CallBackFunction === "function") {
          CallBackFunction(response);
        }
        var Validator = $("form[name=" + FormName + "]").validate();
        Validator = typeof Validator == "undefined" ? "" : Validator;
        if (Validator != "" && RestFields == "") {
          Validator.resetForm();
          $("form[name=" + FormName + "]")[0].reset();
          $("input").typeahead("val", "");
        } else {
          RestFields = RestFields.split(",");
          for (i = 0; i < RestFields.length; i++) {
            if (RestFields[i].trim() != "") {
              if (
                (typeof $("#" + RestFields[i].trim() + "") == "undefined"
                  ? ""
                  : $("#" + RestFields[i].trim() + "").val()) != ""
              ) {
                $("#" + RestFields[i].trim() + "").val("");
                $("#" + RestFields[i].trim() + "").typeahead("val", "");
              }
            }
          }
        }
        if (FieldIDFocus != "") {
          setTimeout(function () {
            $("#" + FieldIDFocus + "").focus();
          }, 500);
        }
        DialogSweetAlert("Savesuccess", "Your data has been saved.");
        toastr.remove();
      } else {
        DialogSweetAlert("error", response.Message);
        toastr.remove();
      }
    },
    error: function (XMLHttpRequest, callStatus, errorThrown) {
      DialogSweetAlert("error", callStatus);
      toastr.remove();
    },
  });
}
function DoSaveChageAjaxForm_UploadPDF(
  FormName,
  URL,
  DATA,
  Message,
  CallBackFunction,
  Getresponetext,
) {
  FormName = typeof FormName == "undefined" ? "" : FormName;
  Message = typeof Message == "undefined" ? "Saving Data..." : Message;
  if (Message == "") Message = "Saving Data...";
  Getresponetext = typeof Getresponetext == "undefined" ? false : true;
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  //var TokenInfo = getStorage("TokenInfo");
  //var accessToken = JSON.parse(TokenInfo).accessToken;
  //var UserID = "";
  //var SystemID = "";
  //var WarehouseID = "";
  //var UserName = "";
  //var SysAndWH = JSON.parse(JSON.parse(TokenInfo).SysAndWH)[0];
  //UserID = SysAndWH.UserID;
  //SystemID = SysAndWH.SystemID;
  //WarehouseID = SysAndWH.WarehouseID;
  //UserName = JSON.parse(TokenInfo).userName;
  //var headerPara = {
  //    UserID: UserID,
  //    UserName: UserName,
  //    SystemID: SystemID,
  //    WarehouseID: WarehouseID,
  //    UserFullName: User_Full_Name
  //};
  $("form[name=" + FormName + "]").ajaxSubmit({
    url: ServerRoot + URL,
    headers: { Authorization: "Bearer " + access_Token },
    dataType: "json",
    //contentType: "application/json; charset=utf-8",
    type: "POST",
    data: DATA,
    beforeSend: function (xhr) {
      DialogSweetAlert("load", "Saving...");
      xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
    },
    uploadProgress: function (event, position, total, percentComplete) {
      var percentValue = percentComplete + "%";
      $("#progressBar").animate(
        {
          width: "" + percentValue + "",
        },
        {
          duration: 5000,
          easing: "linear",
          step: function (x) {
            percentText = Math.round((x * 100) / percentComplete);
            $("#percent").text(percentText + "%");
            if (percentText == "100") {
            }
          },
        },
      );
    },
    complete: function (response) {
      response = JSON.parse(response.responseText);
      if (response.Result == 2) {
        var Validator = $("form[name=" + FormName + "]").validate();
        Validator = typeof Validator == "undefined" ? "" : Validator;
        if (Validator != "") {
          Validator.resetForm();
          $("form[name=" + FormName + "]")[0].reset();
        }
        $("input").jfilestyle("clear");
        //$('.tooltipstered').tooltipster('destroy');
        var InvoiceData = response.InvoiceData;
        var CustomerName = response.CustomerName;
        var InvoiceNo = response.InvoiceNo;
        $("#BoyMasterPage").append(InvoiceData);
        DialogSweetAlert("loadsuccess", "Your data has been saved.");
        if (typeof CallBackFunction === "function") {
          if (Getresponetext) {
            var ResponseText = $.parseJSON(response.responseText);
            CallBackFunction(ResponseText);
          } else CallBackFunction(response);
        }
        $("#div_Cust_Invoice").css("visibility", "hidden");
        HTMLToPDF(
          "div_Cust_Invoice",
          "CustomerInvoice.pdf",
          function (pdfData) {
            $("#div_Cust_Invoice").remove();
            var DATA = {
              CustomerName: CustomerName,
              InvoiceNo: InvoiceNo,
              pdfData: pdfData,
            };
            UploadFile(
              "CustomerInvoice/SaveInvoiceToPDF",
              DATA,
              function (response) {
                //console.log(response);
              },
            );
          },
        );
      } else {
        DialogSweetAlert("error", response.Message);
      }
    },
    success: function (response) {},
    error: function (XMLHttpRequest, callStatus, errorThrown) {
      DialogSweetAlert("error");
    },
  });
}
function UploadFile(endPointName, DATA, CallBackFunction) {
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  //var TokenInfo = getStorage("TokenInfo");
  //var accessToken = JSON.parse(TokenInfo).accessToken;
  //var UserID = "";
  //var SystemID = "";
  //var WarehouseID = "";
  //var UserName = "";
  //var SysAndWH = JSON.parse(JSON.parse(TokenInfo).SysAndWH)[0];
  //UserID = SysAndWH.UserID;
  //SystemID = SysAndWH.SystemID;
  //WarehouseID = SysAndWH.WarehouseID;
  //UserName = JSON.parse(TokenInfo).userName;
  //var headerPara = {
  //    UserID: UserID,
  //    UserName: UserName,
  //    SystemID: SystemID,
  //    WarehouseID: WarehouseID,
  //    UserFullName: User_Full_Name
  //};
  $.ajax({
    //contentType: 'application/x-www-form-urlencoded',
    //processData: false,
    //contentType: false,
    url: ServerRoot + endPointName,
    headers: { Authorization: "Bearer " + access_Token },
    dataType: "html",
    async: false,
    cache: false,
    type: "POST",
    data: DATA,
    beforeSend: function (xhr) {
      xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
    },
    success: function (response) {},
    error: function (response, callStatus, errorThrown) {
      DialogSweetAlert("error", response.status + " " + response.statusText);
    },
  }).done(function (response) {
    var result = JSON.parse(response);
    if (typeof CallBackFunction === "function") {
      CallBackFunction(response);
    }
  });
}
function DoDelete(URL, DATA, CallBackFunction, Message, Text) {
  Message = typeof Message == "undefined" ? "Deleting Data..." : Message;
  Text =
    typeof Text == "undefined" ? "You won't be able to revert this!" : Text;
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;

  Swal.fire({
    position: "top",
    title: "Are you sure?",
    text: Text,
    icon: "question",
    showCancelButton: true,
    confirmButtonColor: "#d33",
    cancelButtonColor: "#3085d6",
    confirmButtonText: "Yes, delete it!",
    cancelButtonText: "No, cancel!",
    //reverseButtons: true
  }).then((result) => {
    if (result.value) {
      DialogSweetAlert("deleteted");
      //var TokenInfo = getStorage("TokenInfo");
      //var accessToken = JSON.parse(TokenInfo).accessToken;
      //var UserID = "";
      //var SystemID = "";
      //var WarehouseID = "";
      //var UserName = "";
      //var SysAndWH = JSON.parse(JSON.parse(TokenInfo).SysAndWH)[0];
      //UserID = SysAndWH.UserID;
      //SystemID = SysAndWH.SystemID;
      //WarehouseID = SysAndWH.WarehouseID;
      //UserName = JSON.parse(TokenInfo).userName;
      //var headerPara = {
      //    UserID: UserID,
      //    UserName: UserName,
      //    SystemID: SystemID,
      //    WarehouseID: WarehouseID,
      //    UserFullName: User_Full_Name
      //};
      $.ajax({
        url: URL,
        dataType: "json",
        async: false,
        type: "POST",
        headers: { Authorization: "Bearer " + access_Token },
        data: DATA,
        beforeSend: function (xhr) {
          xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
        },
        error: function (response, callStatus, errorThrown) {
          $("button").removeAttr("disabled");
          DialogSweetAlert(
            "error",
            response.status + " " + response.statusText,
          );
        },
        success: function (responetext) {
          if (typeof CallBackFunction === "function") {
            //if (Getresponetext)
            //  CallBackFunction(responetext);
            //else
            CallBackFunction(responetext);
          }
        },
      });
    } else if (result.dismiss === Swal.DismissReason.cancel) {
      Swal.fire({
        position: "top",
        title: "Cancelled",
        text: "",
        icon: "error",
        showConfirmButton: false,
        allowOutsideClick: false,
        timerProgressBar: true,
        allowEscapeKey: false,
        //heightAuto: false,
        timer: 700,
      });
    }
  });
}

function SweetCofirmYesNoCancel(
  title,
  confirmButtonText,
  denyButtonText,
  confirmButtonFunction,
  denyButtonFunction,
  inputValidatorOnConfirm,
  url,
) {
  title =
    typeof title == "undefined" ? "Do you want to save the changes?" : title;
  confirmButtonText =
    typeof confirmButtonText == "undefined" ? "Yes" : confirmButtonText;
  denyButtonText = typeof denyButtonText == "undefined" ? "No" : denyButtonText;
  confirmButtonFunction =
    typeof confirmButtonFunction == "undefined" ? "" : confirmButtonFunction;
  denyButtonFunction =
    typeof denyButtonFunction == "undefined" ? "" : denyButtonFunction;
  inputValidatorOnConfirm =
    typeof inputValidatorOnConfirm == "undefined" ? false : true;
  url = typeof url == "undefined" ? "" : url;
  if (title == "") title = "Do you want to save change?";
  Swal.fire({
    allowOutsideClick: false,
    allowEscapeKey: false,
    icon: "question",
    position: "top",
    title: title,
    showDenyButton: denyButtonText == "" ? false : true,
    showCancelButton: true,
    confirmButtonColor: "#007BFF",
    cancelButtonColor: "#A9A9A9",
    confirmButtonText: confirmButtonText,
    denyButtonText: denyButtonText,
    input: "textarea",
    inputAutoFocus: true,
    inputAttributes: {
      id: "TxtInvoiceComment",
      name: "TxtInvoiceComment",
    },
    didOpen: () => {
      setTimeout(function () {
        $("#TxtInvoiceComment").focus();
      }, 500);
    },
    //inputLabel: 'Your email address',
    showCloseButton: true,
    showLoaderOnConfirm: true,
    showLoaderOnDeny: true,
    returnInputValueOnDeny: true,
    inputPlaceholder: "Enter your text",
    //inputValidator: (value) => {
    //    if (!value) {
    //        return 'You need to write something!'
    //    }
    //},
    preConfirm: async (value) => {
      if (value == "" && inputValidatorOnConfirm) {
        Swal.showValidationMessage("You need to write something!");
        // Set a return here to prevent any further progress
        return false;
      } else if (inputValidatorOnConfirm) {
        if (url != "") {
          try {
            var response = await fetch(ServerRoot + url);
            if (!response.ok) {
              return Swal.showValidationMessage(
                `${JSON.stringify(await response.json())}`,
              );
            }
            return response.json();
          } catch (error) {
            Swal.showValidationMessage(`Request failed: ${error}`);
          }
        }
      }
      return value;
    },
    preDeny: (value) => {
      //let InvoiceComment = Swal.getPopup().querySelector('#TxtInvoiceComment').value;
      if (value == "") {
        Swal.showValidationMessage("You need to write something!");
        // Set a return here to prevent any further progress
        return false;
      }
    },
  }).then((result) => {
    if (result.isConfirmed) {
      if (confirmButtonFunction != "")
        return confirmButtonFunction(result.value);
    } else if (result.isDenied) {
      if (denyButtonFunction != "") return denyButtonFunction(result.value);
    }
  });
}
function DoSaveChangeSweetCofirm(
  URL,
  DATA,
  CallBackFunction,
  Message,
  Text,
  InputText,
) {
  Message = typeof Message == "undefined" ? "Loading..." : Message;
  if (Message == "") Message = "Loading...";

  Text =
    typeof Text == "undefined" ? "You won't be able to revert this!" : Text;
  CallBackFunction =
    typeof CallBackFunction == "undefined" ? "" : CallBackFunction;
  InputText = typeof InputText == "undefined" ? "" : InputText;
  Swal.fire({
    position: "top",
    title: "Are you sure?",
    html: Text,
    icon: "question",
    showCancelButton: true,
    confirmButtonColor: "#d33",
    cancelButtonColor: "#3085d6",
    confirmButtonText: "Yes, do it now!",
    cancelButtonText: "No, cancel!",
    showLoaderOnConfirm: true,
    //reverseButtons: true,
    allowOutsideClick: false,
  }).then((result) => {
    if (result.value) {
      if (InputText != "") {
        Swal.fire({
          position: "top",
          title: "Please enter comment",
          input: "text",
          inputAttributes: {
            required: "true",
            autocapitalize: "off",
            placeholder: "Type your comment",
            id: "txtComment",
          },
          target: ".modal",
          allowOutsideClick: false,
          allowEscapeKey: false,
          showCancelButton: true,
          confirmButtonText: InputText,
          showLoaderOnConfirm: true,
          showLoaderOnConfirm: true,
          inputValidator: (value) => {
            return !value && "Please type your comment.";
          },
          preConfirm: () => {
            var comment = encodeURIComponent($("#txtComment").val());
            DATA["Comment"] = comment;
            DialogSweetAlert("success", "Data save change successful.");
            URL = ServerRoot + URL;
            DialogSweetAlert("load", Message);
            $.ajax({
              url: URL,
              headers: {
                Authorization: "Bearer " + access_Token,
              },
              dataType: "json",
              async: true,
              type: "POST",
              data: DATA,
              beforeSend: function (xhr) {
                xhr.setRequestHeader(
                  "x-custom-header",
                  JSON.stringify(headerPara),
                );
                $("button").attr("disabled", true);
              },
              success: function (responetext) {
                $("button").removeAttr("disabled");
                if (
                  responetext.Result == 2 ||
                  responetext.success ||
                  responetext.result == 2
                ) {
                  DialogSweetAlert("success", "Data save change successful.");
                  if (typeof CallBackFunction === "function") {
                    //if (Getresponetext)
                    //  CallBackFunction(responetext);
                    //else
                    CallBackFunction(responetext);
                  }
                } else {
                  DialogSweetAlert("nopermission", responetext.Message);
                }
              },
              error: function (XMLHttpRequest, callStatus, errorThrown) {
                $("button").attr("disabled", false);
              },
            });
          },
          //allowOutsideClick: () => !Swal.isLoading()
        }).then((result) => {
          //console.log(result);
          //if (result.isConfirmed) {
          //    Swal.fire({
          //        title: `${result.value.login}'s avatar`,
          //        imageUrl: result.value.avatar_url
          //    })
          //}
        });
      } else {
        DialogSweetAlert("load", Message);
        URL = ServerRoot + URL;
        $.ajax({
          url: URL,
          headers: { Authorization: "Bearer " + access_Token },
          dataType: "json",
          async: true,
          type: "POST",
          data: DATA,
          beforeSend: function (xhr) {
            xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
            $("button").attr("disabled", true);
          },
          success: function (responetext) {
            $("button").attr("disabled", false);
            $("button").removeAttr("disabled");
            if (
              responetext.Result == 2 ||
              responetext.success ||
              responetext.result == 2
            ) {
              DialogSweetAlert("success", "Data save change successful.");
              if (typeof CallBackFunction === "function") {
                //if (Getresponetext)
                //  CallBackFunction(responetext);
                //else
                CallBackFunction(responetext);
              }
            } else {
              DialogSweetAlert("nopermission", responetext.Message);
            }
          },
          error: function (response, callStatus, errorThrown) {
            DialogSweetAlert("nopermission", response.statusText);
            $("button").attr("disabled", false);
            $("button").removeAttr("disabled");
          },
        });
      }
    } else if (result.dismiss === Swal.DismissReason.cancel) {
      Swal.fire({
        position: "top",
        title: "Cancelled",
        text: "",
        icon: "error",
        showConfirmButton: false,
        allowOutsideClick: false,
        timerProgressBar: true,
        allowEscapeKey: false,
        //heightAuto: false,
        timer: 700,
      });
    }
  });
}

function ModalDialogBox(
  Title,
  URL,
  DATA,
  DialogID,
  SaveChageFunction,
  CallBackFunction,
  ModalBody,
  modalfullscreen,
) {
  DialogID = typeof DialogID == "undefined" ? "modal-default" : DialogID;
  Title = typeof Title == "undefined" ? "Entry Form" : Title;
  URL = typeof URL == "undefined" ? "" : URL;
  ModalBody = typeof ModalBody == "undefined" ? "" : ModalBody;
  SaveChageFunction =
    typeof SaveChageFunction == "undefined" ? "" : SaveChageFunction;
  modalfullscreen =
    typeof modalfullscreen == "undefined" ? false : modalfullscreen;
  if (modalfullscreen) modalfullscreen = " modal-fullscreen";
  else modalfullscreen = "";
  DATA = typeof DATA == "undefined" ? "" : DATA;
  if (DialogID == "") DialogID = "modal-default";
  var modal =
    '<div class="modal fade" tabindex="-1" role="dialog" id="' +
    DialogID +
    '">';
  modal += '<div class="modal-dialog' + modalfullscreen + '" role="document">'; // modal-dialog-centered
  modal += '<div class="modal-content">';
  modal += '<div class="card-success">';
  modal +=
    '<div class="card-header"><h4"><i class="ti ti-edit"></i>&nbsp;&nbsp;' +
    Title +
    "</h4>";
  modal +=
    '<button type="button" class="close" data-dismiss="modal" aria-label="Close">';
  modal += '<span aria-hidden="true">&times;</span></button></div></div>';
  //modal += '<div class="modal-header">';
  //modal += '<h4 class="modal-title">' + Title + '</h4>';
  //modal += '<button type="button" class="close" data-dismiss="modal" aria-label="Close">';
  //modal += '<span aria-hidden="true">&times;</span></button></div>'
  modal += '<div class="modal-body" id="' + DialogID + 'modalbody">';
  modal += "<p></p>";
  modal += "</div>";
  modal += '<div class="modal-footer">';
  if (SaveChageFunction != "")
    modal +=
      '<button type="button" id="SaveChangeModal" class="btn btn-outline-success"><i class="ti ti-device-floppy"></i> Save Change</button>';
  modal +=
    '<button type="button" id="CloseModal" class="btn btn-outline-danger" data-dismiss="modal"><i class="ti ti-x"></i> Close</button>';
  modal += "</div></div>";
  modal += "</div></div>";

  if ($("#" + DialogID).length == 0) $(document.body).append(modal);
  //$("#" + DialogID).draggable();
  $(".modal-dialog").draggable({
    handle: ".card-success",
  });
  $("#" + DialogID).modal({
    keyboard: false,
    backdrop: "static",
    show: true,
  });
  $("#" + DialogID).on("hide.bs.modal", function () {
    $(this).data("bs.modal", null);
    //if ($('.tooltipstered').tooltipster()) {
    //    $('.tooltipstered').tooltipster('destroy');
    //}
    $("button").attr("disabled", false);
    $("#" + DialogID).remove();
  });
  $("#" + DialogID + "modalbody p").html(
    '<div class="modal-loading-center">' +
      '<div class="modal-spinner"></div>' +
      '<div class="modal-loading-text">Loading...</div>' +
      "</div>",
  );
  if (URL != "") {
    URL = ServerRoot + URL;
    $.ajax({
      url: URL,
      data: DATA,
      dataType: "html",
      async: true,
      cache: false,
      type: "POST",
      headers: { Authorization: "Bearer " + access_Token },
      beforeSend: function (xhr) {
        $("button").attr("disabled", true);
        xhr.setRequestHeader("x-custom-header", JSON.stringify(headerPara));
      },
      success: function (responetext) {
        $("#" + DialogID + "modalbody p").html(responetext);
        if (typeof SaveChageFunction === "function") {
          $("#SaveChangeModal").click(function () {
            //$('.tooltipstered').tooltipster('destroy');
            if (SaveChageFunction()) $("#CloseModal").click();
          });
        }
        if (typeof CallBackFunction === "function") {
          CallBackFunction();
        }
        $("button").attr("disabled", false);
      },
      error: function (XMLHttpRequest, callStatus, errorThrown) {
        $("button").attr("disabled", false);
        DialogSweetAlert("error");
      },
    });
  } else if (ModalBody != "") {
    $("button").attr("disabled", false);
    $("#" + DialogID + "modalbody p").html(ModalBody);
    if (typeof SaveChageFunction === "function") {
      $("#SaveChangeModal").click(function () {
        //$('.tooltipstered').tooltipster('destroy');
        if (SaveChageFunction()) {
          $("#CloseModal").click();
        }
      });
    }
    if (typeof CallBackFunction === "function") {
      CallBackFunction();
    }
  }
}

function checkBoxesGroup(CheckBoxGroupID, CheckBoxGroupClass) {
  var checkboxesGroup = $(
    "#" + CheckBoxGroupID + " ." + CheckBoxGroupClass + ":checkbox",
  );
  var checkboxGroupLength = checkboxesGroup.length;
  var firstCheckboxGroup = checkboxGroupLength > 0 ? checkboxesGroup[0] : null;
  var i = 0;
  if (firstCheckboxGroup) {
    for (i = 0; i < checkboxGroupLength; i++) {
      //checkboxesGroup[i].addEventListener('change', checkValidity(CheckBoxGroupID, CheckBoxGroupClass));
      $(
        "#" +
          CheckBoxGroupID +
          " input." +
          CheckBoxGroupClass +
          ":eq(" +
          i +
          ")",
      ).change(function () {
        checkValidity(CheckBoxGroupID, CheckBoxGroupClass);
      });
    }
    //checkValidity(CheckBoxGroupID, CheckBoxGroupClass);
  }
}
function isChecked(CheckBoxGroupID, CheckBoxGroupClass) {
  //var checkboxesGroup = $("#" + CheckBoxGroupID + " ." + CheckBoxGroupClass + ":checkbox");
  var checkboxesGroup = $("." + CheckBoxGroupClass + ":checkbox");
  var checkboxGroupLength = checkboxesGroup.length;
  for (var i = 0; i < checkboxGroupLength; i++) {
    if (checkboxesGroup[i].checked) return true;
  }
  return false;
}
function checkValidity(CheckBoxGroupID, CheckBoxGroupClass) {
  var i = 0;
  var IsValidCheckBoxGroup = false;
  //var checkboxesGroup = $("#" + CheckBoxGroupID + " ." + CheckBoxGroupClass + ":checkbox");
  var checkboxesGroup = $("." + CheckBoxGroupClass + ":checkbox");
  var checkboxGroupLength = checkboxesGroup.length;
  var errorMessage = !isChecked(CheckBoxGroupID, CheckBoxGroupClass)
    ? "At least one checkbox must be selected."
    : "";
  for (i = 0; i < checkboxGroupLength; i++) {
    checkboxesGroup[i].setCustomValidity(errorMessage);
    //$("#" + CheckBoxGroupID + " input." + CheckBoxGroupClass)[i].setCustomValidity(errorMessage);
    //$("." + CheckBoxGroupClass)[i].setCustomValidity(errorMessage);
  }
  if (errorMessage != "") {
    $("#" + CheckBoxGroupID + " div:last()").append(
      '<div id="' +
        CheckBoxGroupID +
        '-error" class="invalid-tooltip">' +
        errorMessage +
        "</div>",
    );
    //$("." + CheckBoxGroupClass + " div:last()").append('<div id="' + CheckBoxGroupID + '-error" class="invalid-tooltip">' + errorMessage + '</div>');
    IsValidCheckBoxGroup = false;
    //var index = $("#" + CheckBoxGroupID).closest('tr').index();
    if ($("#" + CheckBoxGroupID + " input." + CheckBoxGroupClass)[0]) {
      var frmname = $("#" + CheckBoxGroupID + " input." + CheckBoxGroupClass)[0]
        .form.name;
      $("form[name=" + frmname + "]").addClass("was-validated");
    }
  } else {
    IsValidCheckBoxGroup = true;
    //$("#" + CheckBoxGroupID + "-error").remove();
    $("." + CheckBoxGroupClass + "-error").remove();
  }
  return IsValidCheckBoxGroup;
}
function resetCheckedBoxesGroup(CheckBoxGroupID, CheckBoxGroupClass) {
  var checkboxesGroup = $("." + CheckBoxGroupClass + ":checkbox");
  var checkboxGroupLength = checkboxesGroup.length;
  var errorMessage = "At least one checkbox must be selected.";
  for (var i = 0; i < checkboxGroupLength; i++) {
    checkboxesGroup[i].setCustomValidity(errorMessage);
  }
  if (errorMessage != "") {
    $("#" + CheckBoxGroupID + " div:last()").append(
      '<div id="' +
        CheckBoxGroupID +
        '-error" class="invalid-tooltip">' +
        errorMessage +
        "</div>",
    );
    IsValidCheckBoxGroup = false;
  } else {
    IsValidCheckBoxGroup = true;
    $("#" + CheckBoxGroupID + "-error").remove();
  }
}
function dynamicSort(property) {
  var sortOrder = 1;
  if (property[0] === "-") {
    sortOrder = -1;
    property = property.substr(1);
  }
  return function (a, b) {
    /* next line works with strings and numbers,
     * and you may want to customize it to your needs
     */
    var result =
      a[property] < b[property] ? -1 : a[property] > b[property] ? 1 : 0;
    return result * sortOrder;
  };
}
function getBase64Image(img) {
  // Create an empty canvas element
  var canvas = document.createElement("canvas");
  canvas.width = img.width;
  canvas.height = img.height;

  // Copy the image contents to the canvas
  var ctx = canvas.getContext("2d");
  ctx.drawImage(img, 0, 0);

  // Get the data-URL formatted image
  // Firefox supports PNG and JPEG. You could check img.src to
  // guess the original format, but be aware the using "image/jpg"
  // will re-encode the image.
  var dataURL = canvas.toDataURL("image/png");

  return dataURL.replace(/^data:image\/(png|jpg);base64,/, "");
}
function base64ToBlob(base64, contentType = "image/png", chunkLength = 512) {
  const byteCharacters = window.atob(base64); //Array.from(atob(base64.substr(base64.indexOf(',') + 1)));
  //const chunksIterator = new Array(Math.ceil(byteCharsArray.length / chunkLength));
  //const bytesArrays = [];
  //for (let c = 0; c < chunksIterator.length; c++) {
  //    bytesArrays.push(new Uint8Array(byteCharsArray.slice(c * chunkLength, chunkLength * (c + 1)).map(s => s.charCodeAt(0))));
  //}
  //const blob = new Blob(bytesArrays, { type: contentType });
  const byteArrays = [];
  for (let offset = 0; offset < byteCharacters.length; offset += sliceSize) {
    const slice = byteCharacters.slice(offset, offset + sliceSize);
    const byteNumbers = new Array(slice.length);
    for (let i = 0; i < slice.length; i++) {
      byteNumbers[i] = slice.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    byteArrays.push(byteArray);
  }
  const blob = new Blob(byteArrays, {
    type: contentType,
  }); // statement which creates the blob
  return URL.createObjectURL(blob);
}
//const blobToBase64 = blob => {
//    const reader = new FileReader();
//    reader.readAsDataURL(blob);
//    return new Promise(resolve => {
//        reader.onloadend = () => {
//            resolve(reader.result);
//        };
//    });
//};
function DeleteAttFile(URL, DestinationFile, THIS, ID) {
  DialogSweetCofirm(
    function () {
      var DeleteFileName = $(THIS)
        .parent()
        .find("input[name='Hidden_file_Name']")
        .val();
      var KeepFileName = $(THIS)
        .parent()
        .find("input[name='Current_Hidden_file_Name_" + ID + "']")
        .val();
      KeepFileName = KeepFileName.split(",");
      var _searchedIndex = $.inArray(DeleteFileName, KeepFileName);
      if (_searchedIndex >= 0) {
        KeepFileName.splice(_searchedIndex, 1);
        $("input[name='Current_Hidden_file_Name_" + ID + "']").val(
          KeepFileName,
        );
        $(THIS).parent().parent().parent().remove();
        if (KeepFileName.length == 0) KeepFileName = "";
        var DATA = {
          ID: ID,
          DestinationFile: DestinationFile,
          DeleteFileName: DeleteFileName,
          KeepFileName: KeepFileName,
        };

        DoSaveChangeData(URL, DATA);
      }
    },
    "",
    "Do you to delete this file?",
    "Yes, delete it!",
  );
}
function downloadFile(URL, OutPutFileName) {
  var a = document.createElement("a");
  a.href = URL;
  a.download = OutPutFileName;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
}
function printFile(URL) {
  printJS({ printable: URL, type: "image" });
  //$("img[name=" + ElementName +"]").off("load").on("load", function () {
  //    window.print();
  //}).attr("src", URL);
}
function generatePassword(passwordLength) {
  var numberChars = "0123456789";
  var upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
  var lowerChars = "abcdefghijklmnopqrstuvwxyz";
  var allChars = numberChars + upperChars + lowerChars;
  var randPasswordArray = Array(passwordLength);
  randPasswordArray[0] = numberChars;
  randPasswordArray[1] = upperChars;
  randPasswordArray[2] = lowerChars;
  randPasswordArray = randPasswordArray.fill(allChars, 3);
  return shuffleArray(
    randPasswordArray.map(function (x) {
      return x[Math.floor(Math.random() * x.length)];
    }),
  ).join("");
}
function validatePassword(p, callback) {
  var valid = true;
  var errors = [];
  if (p.length < 8) {
    errors.push("Your password must be at least 8 characters");
  }
  if (p.length > 32) {
    errors.push("Your password must be at max 32 characters");
  }
  if (p.search(/[a-z]/) < 0) {
    errors.push("Your password must contain at least one lower case letter.");
    valid = false;
  }
  if (p.search(/[A-Z]/) < 0) {
    errors.push("Your password must contain at least one upper case letter.");
  }
  if (p.search(/[0-9]/) < 0) {
    errors.push("Your password must contain at least one digit.");
  }
  //if (p.search(/[!@@#\$%\^&\*_]/) < 0) {
  //     errors.push("Your password must contain at least special char from -[ ! @@ # $ % ^ & * _ ]");
  //      valid = false;
  // }
  if (errors.length > 0) {
    //console.log(errors.join("\n"));
    valid = false;
    return callback(valid, errors.join("\n\r<br />"));
  }
  return callback(valid, errors);
}
function shuffleArray(array) {
  for (var i = array.length - 1; i > 0; i--) {
    var j = Math.floor(Math.random() * (i + 1));
    var temp = array[i];
    array[i] = array[j];
    array[j] = temp;
  }
  return array;
}
function dateDiffInDays(a, b) {
  a = new Date(a);
  b = new Date(b);
  const _MS_PER_DAY = 1000 * 60 * 60 * 24;
  // Discard the time and time-zone information.
  const utc1 = Date.UTC(a.getFullYear(), a.getMonth(), a.getDate());
  const utc2 = Date.UTC(b.getFullYear(), b.getMonth(), b.getDate());

  return Math.floor((utc2 - utc1) / _MS_PER_DAY);
}
function pluralizeWord(singularWord, count, pluralWord) {
  pluralWord = typeof pluralWord == "undefined" ? "s" : pluralWord;
  return count > 1
    ? count + " " + singularWord + pluralWord
    : count + " " + singularWord;
}
function allowInputOnlyNumber(evt) {
  var theEvent = evt || window.event;

  // Handle paste
  if (theEvent.type === "paste") {
    key = evt.clipboardData.getData("text/plain");
  } else {
    // Handle key press
    var key = theEvent.keyCode || theEvent.which;
    key = String.fromCharCode(key);
  }
  var regex = /[0-9]|\./;
  if (!regex.test(key)) {
    theEvent.returnValue = false;
    if (theEvent.preventDefault) theEvent.preventDefault();
  }
}
function GetGeneratPWD() {
  var pwd = generatePassword(8);
  $("#PPPoEPassword").val(pwd);
}
function ApplyBoxStyleVAT() {
  if ($(".VATNumber span").hasClass("border border-dark rounded")) {
    return true;
  }
  $(".VATNumber").each(function (index) {
    var text = $(".VATNumber:eq(" + index + ")").html();
    $(".VATNumber").eq(index).html("");
    for (i = 0; i <= text.length - 1; i++) {
      var html = text.substr(i, 1);
      if (html != "-") {
        var sp =
          "<span class='border border-dark rounded'>&nbsp;" +
          html +
          "&nbsp;</span>&nbsp;";
      } else {
        var sp = "<span>&nbsp;" + html + "&nbsp;</span>&nbsp;";
      }
      $(".VATNumber").eq(index).append(sp);
    }
  });
}
function NewInvoice(CustomerID) {
  var obj = {};
  obj = {
    PrimaryFieldValue: "2",
    InvoiceNo: "0",
    CustomerID: "" + CustomerID + "",
    FormStatus: "CreateInvoice",
  };
  AddQuotation(obj, "Create Customer Invoice");
}
function MarkerMap(InputID, InputLatitude, InputLongitude, MapURL) {
  MapURL = typeof MapURL == "undefined" ? "" : MapURL;
  InputID =
    typeof InputID == "undefined" ? "GoogleMap_FullAddress" : trim(InputID);
  InputLatitude =
    typeof InputLatitude == "undefined"
      ? "GoogleMap_Latitude"
      : trim(InputLatitude);
  InputLongitude =
    typeof InputLongitude == "undefined"
      ? "GoogleMap_Longitude"
      : trim(InputLongitude);
  var URi = "AddCustomer/SetCustomerLocationByGoogleMap";
  if (MapURL != "") URi = MapURL;
  ShowDialogBox2(
    "Set Location From Google Map",
    "Auto",
    500,
    URi,
    0,
    "DialogGoogleMap",
    function () {
      $("#DialogGoogleMap").on("hide.bs.modal", function () {
        //if (MapURL == "" && GoogleMap.IsSaved) {
        if (InputID != "") {
          if ($("#" + InputID)) {
            $("#" + InputID).val(GoogleMap.GoogleMap_FullAddress);
          }
        }

        if (InputLatitude != "") {
          if ($("#" + InputLatitude)) {
            $("#" + InputLatitude).val(GoogleMap.GoogleMap_Latitude);
            $("#" + InputLongitude).val(GoogleMap.GoogleMap_Longitude);
          }
        }
        //}
      });
    },
  );
}
function AddCreditNotes(Customer_ID, InvoiceNo) {
  InvoiceNo = typeof InvoiceNo == "undefined" ? "" : InvoiceNo;
  Customer_ID = typeof Customer_ID == "undefined" ? "" : Customer_ID;
  if (Customer_ID == "" && Customer_ID != 0) {
    var url = window.location.hash;
    var ID = url.substring(url.indexOf("?"));
    Customer_ID = ID.substring(ID.indexOf("=")).replace(/=/g, "");
  }
  //number 2 for create invoicing...
  var obj = {};
  if (InvoiceNo != "") {
    obj = {
      PrimaryFieldValue: "2",
      InvoiceNo: InvoiceNo,
      CustomerID: Customer_ID,
      FormStatus: "CreateInvoice",
      InvoiceType: "Credit Notes",
    };
  } else {
    obj = {
      PrimaryFieldValue: "2",
      InvoiceNo: "0",
      CustomerID: "" + Customer_ID + "",
      FormStatus: "CreateInvoice",
      InvoiceType: "Credit Notes",
    };
  }
  AddQuotation(obj, "New Credit Notes");
}
function NewCustomerInvoice(CustomerID) {
  var obj = {};
  obj = {
    PrimaryFieldValue: "2",
    InvoiceNo: "0",
    CustomerID: "" + CustomerID + "",
    FormStatus: "CreateInvoice",
  };
  AddQuotation(obj, "Create Customer Invoice");
}
function AddNewVendorType() {
  var URL =
    "AddVendorType/CreateVendorType?PrimaryFieldValue=0&Type=AddNewVendorType";
  ShowDialogBox2(
    "Add Vendor Type",
    "",
    "",
    URL,
    0,
    "DialogAddNewVendorType",
    function () {
      $("#DialogAddNewVendorType").on("hide.bs.modal", function () {
        if (SaveChangeResponse.length > 0) {
          var name = SaveChangeResponse[0].value;
          $("#VendorType").val("");
          Reload_VendorType(function () {
            $("#VendorType").val(name);
            setTimeout(function () {
              $("#VendorType").val(name).trigger("change");
              $("#VendorType").focus();
            }, 500);
          });
          SaveChangeResponse = [];
        }
      });
    },
  );
}
function AddNewDepartment() {
  var URL = "AddDepartment/CreateDepartment?PrimaryFieldValue=0";
  ShowDialogBox2(
    "Add Department",
    "",
    "",
    URL,
    0,
    "DialogAddNewDepartment",
    function () {
      $("#DialogAddNewDepartment").on("hide.bs.modal", function () {
        if (SaveChangeResponse.length > 0) {
          var name = SaveChangeResponse[0].value;
          $("#DepartmentName").val("");
          Reload_DepartmentName(function () {
            $("#DepartmentName").val(name);
            setTimeout(function () {
              $("#DepartmentName").val(name).trigger("change");
              $("#DepartmentName").focus();
            }, 500);
          });
          SaveChangeResponse = [];
        }
      });
    },
  );
}
function callbackSaveChangeAddVendorType(rst) {
  //$("#DialogAddNewVendorType").modal('hide');
  //$("#VendorType").typeahead("destroy");
  Reload_VendorType();
  //$("#VendorType").val('');
}
function AddNewVendor(ID) {
  ID = typeof ID == "undefined" ? 0 : ID;
  var Type = "AddNewVendor";
  var Title = "New Vendor";
  if (ID != 0) {
    Type = "EditVendor";
    Title = "Edit Vendor";
  }
  var URL = "AddVendor/CreateVendor?PrimaryFieldValue=" + ID + "&Type=" + Type;
  ShowDialogBox2(Title, "Auto", "", URL, ID, "DialogAddNewVendor", function () {
    if (ID != 0) {
      $("#DialogAddNewVendor").on("hide.bs.modal", function () {
        LoadVendorInfo();
      });
    } else {
      $("#DialogAddNewVendor").on("hide.bs.modal", function () {
        if (SaveChangeResponse.length > 0) {
          var name =
            SaveChangeResponse[4].value + " - " + SaveChangeResponse[5].value;
          $("#Vendor_Name").val("");
          var fn = window["Reload_VendorName"];
          if (typeof fn != "undefined") {
            if (typeof fn === "function") {
              Reload_VendorName(function () {
                $("#Vendor_Name").val(name);
                setTimeout(function () {
                  $("#Vendor_Name").val(name).trigger("change");
                  $("#Vendor_Name").focus();
                }, 500);
              });
            }
          } else {
            Reload_Vendor_Name(function () {
              $("#Vendor_Name").val(name);
              setTimeout(function () {
                $("#Vendor_Name").val(name).trigger("change");
                $("#Vendor_Name").focus();
              }, 500);
            });
          }

          SaveChangeResponse = [];
        }
      });
    }
  });
}
function callbackSaveChangeAddVendor() {
  var VendorCode = $("#VendorCode").val();
  var VendorName = $("#VendorName").val();
  VendorCode = VendorCode + " - " + VendorName;
  $("#Vendor_Name").val("").trigger("typeahead:change", "");
  Reload_Vendor_Name(function () {
    $("#Vendor_Name").val(VendorCode).trigger("change");
    setTimeout(function () {
      $("#Vendor_Name").focus();
    }, 500);
  });
  $("#DialogAddNewVendor").modal("hide");
}
function AddNewStockName() {
  var URL = "AddStockName/CreateStockName?PrimaryFieldValue=0&Type=NewItem";
  ShowDialogBox2(
    "Add Category",
    "500",
    "500",
    URL,
    0,
    "DialogAddStockName",
    function () {
      $("#DialogAddStockName").on("hide.bs.modal", function () {
        if (SaveChangeResponse.length > 0) {
          var name = SaveChangeResponse[0].value;
          var id = SaveChangeResponse["Data"].Data;
          $("#StockName").val("0");
          $("#StockName_StockNameID").val("0");
          Reload_StockName(function () {
            setTimeout(function () {
              $("#StockName").val(id);
              $("#StockName_StockNameID").val(id);
              $("#StockName").focus();
              $("#StockName").val(id).trigger("change");
            }, 500);
          });
          SaveChangeResponse = [];
        }
      });
    },
  );
}
function AddNewUM() {
  ShowDialogBox2(
    "Add Unit of measure",
    "",
    "",
    "AddUOM/CreateUOM?Type=AddNewUnitofmeasure",
    0,
    "DialogAddNewUnitofmeasure",
    function () {
      $("#DialogAddNewUnitofmeasure").on("hide.bs.modal", function () {
        if (SaveChangeResponse.length > 0) {
          var name = SaveChangeResponse[0].value;
          var id = SaveChangeResponse["Data"].Data;
          $("#Txt_UMName").val("");
          $("#Txt_UMName_UomID").val("");
          Reload_Txt_UMName(function () {
            $("#Txt_UMName").val(name);
            $("#Txt_UMName_UomID").val(id);
            $("#Txt_UMName").typeahead("val", name).trigger("change");
            $("#Txt_UMName").focus();
            $("#Txt_UMName").typeahead("close");
          });
          SaveChangeResponse = [];
        }
      });
    },
  );
}
function AddNewItemGroup() {
  let StockName = $("#StockName").val();
  var URL = "AddItemGroup/CreateItemGroup?PrimaryFieldValue=0&Type=NewItem";
  ShowDialogBox2(
    "Add Item Group",
    "",
    "300",
    URL,
    0,
    "DialogAddNewItemGroup",
    function () {
      let val = $("#Txt_Stock_Name")
        .find("option:contains('" + StockName + "')")
        .val();
      $("#Txt_Stock_Name").val(val).trigger("typeahead:change", val);
      $("#DialogAddNewItemGroup").on("hide.bs.modal", function () {
        $("#StockName").trigger("typeahead:change", $("#StockName").val());
        if (SaveChangeResponse.length > 0) {
          var name = SaveChangeResponse[1].value;
          $("#ItemGroup").val(name);
          setTimeout(function () {
            $("#ItemGroup").val(name).trigger("change");
            $("#ItemGroup").focus();
          }, 500);
          SaveChangeResponse = [];
        }
      });
    },
  );
}
function AddNewService() {
  var URL = "AddServices/CreateService?PrimaryFieldValue=0&Type=NewItem";
  ShowDialogBox2(
    "Add New Services",
    "",
    "",
    URL,
    0,
    "DialogAddServices",
    function () {
      $("#DialogAddServices").on("hide.bs.modal", function () {
        if (SaveChangeResponse.length > 0) {
          var name = SaveChangeResponse[1].value;
          var id = SaveChangeResponse["Data"].Data;
          $("#TypeName").val("");
          $("#TypeName_ItemTypeID").val("0");
          Reload_TypeName(function () {
            $("#TypeName").val(name);
            $("#TypeName").typeahead("val", name).trigger("change");
            $("#TypeName").focus();
            $("#TypeName").typeahead("close");
            $("#TypeName_ItemTypeID").val(id);
          });
          SaveChangeResponse = [];
        }
      });
    },
  );
}
function AddNewItemSize() {
  var URL = "ItemSize/AddItemSize?PrimaryFieldValue=0&Type=NewItem";
  ShowDialogBox2(
    "Add Item Size",
    "",
    "",
    URL,
    0,
    "DialogAddNewItemSize",
    function () {
      $("#DialogAddNewItemSize").on("hide.bs.modal", function () {
        if (SaveChangeResponse.length > 0) {
          var name = SaveChangeResponse[0].value;
          var id = SaveChangeResponse["Data"].Data;
          $("#ItemSize").val("");
          $("#ItemSize_ItemSizeID").val("0");
          Reload_ItemSize(function () {
            $("#ItemSize").val(name);
            $("#ItemSize").typeahead("val", name).trigger("change");
            $("#ItemSize").focus();
            $("#ItemSize").typeahead("close");
            $("#ItemSize_ItemSizeID").val(id);
          });
          SaveChangeResponse = [];
        }
      });
    },
  );
}
function AddNewItemBrand() {
  var URL = "ItemBrand/AddItemBrand?PrimaryFieldValue=0&Type=NewItem";
  ShowDialogBox2(
    "Add Item Brand",
    "",
    "",
    URL,
    0,
    "DialogAddNewItemBrand",
    function () {
      $("#DialogAddNewItemBrand").on("hide.bs.modal", function () {
        if (SaveChangeResponse.length > 0) {
          var name = SaveChangeResponse[0].value;
          var id = SaveChangeResponse["Data"].Data;
          $("#ItemBrand").val("");
          $("#ItemBrand_ItemBrandID").val("0");
          Reload_ItemBrand(function () {
            $("#ItemBrand").val(name);
            $("#ItemBrand").typeahead("val", name).trigger("change");
            $("#ItemBrand").focus();
            $("#ItemBrand").typeahead("close");
            $("#ItemBrand_ItemBrandID").val(id);
          });
          SaveChangeResponse = [];
        }
      });
    },
  );
}
function AddNewManufacturer() {
  var URL = "Manufacturer/CreateManufacturer?PrimaryFieldValue=0&Type=NewItem";
  ShowDialogBox2(
    "Add Manufacturer",
    "",
    "",
    URL,
    0,
    "DialogAddNewManufacturer",
    function () {
      $("#DialogAddNewManufacturer").on("hide.bs.modal", function () {
        if (SaveChangeResponse.length > 0) {
          var name = SaveChangeResponse[0].value;
          var id = SaveChangeResponse["Data"].Data;
          Reload_Txt_Manufacturer(function () {
            $("#Txt_Manufacturer").val(name).focus();
          });
          SaveChangeResponse = [];
        }
      });
    },
  );
}
function AddNewBranch() {
  var URL = "AddBranch/CreateBranch?PrimaryFieldValue=0&Type=NewItem";
  ShowDialogBox2(
    "Add Branch",
    "1000",
    "500",
    URL,
    0,
    "DialogAddNewBranch",
    function () {
      $("#DialogAddNewBranch").on("hide.bs.modal", function () {
        Reload_TxtBranchID();
      });
    },
  );
}
function AddNewWarehouse() {
  var URL = "AddWarehouse/CreateWarehouse?PrimaryFieldValue=0&Type=NewItem";
  ShowDialogBox2(
    "Add Branch",
    "1000",
    "500",
    URL,
    0,
    "DialogAddNewWarehouse",
    function () {
      $("#DialogAddNewWarehouse").on("hide.bs.modal", function () {
        Reload_DeliverTo();
      });
    },
  );
}
function AddNewPaymentTerms() {
  var URL =
    "AddPaymentTerms/CreatePaymentTerms?PrimaryFieldValue=0&Type=NewItem";
  ShowDialogBox2(
    "Add Payment Terms",
    "",
    "",
    URL,
    0,
    "DialogAddNewPaymentTerms",
    function () {
      $("#DialogAddNewPaymentTerms").on("hide.bs.modal", function () {
        Reload_PaymentTerms();
      });
    },
  );
}
function AddNewItem() {
  var URL = "AddItem/CreateItem?PrimaryFieldValue=0&Type=NewItem";
  ShowDialogBox2(
    "Add Item",
    "Auto",
    "600",
    URL,
    0,
    "DialogAddNewItem",
    function () {
      $("#DialogAddNewItem").on("hide.bs.modal", function () {
        //loadItem();
      });
    },
  );
}
function callbackSaveChangeAddNewItem() {
  //loadItem();
  //$("#DialogAddNewItem").modal('hide');
}
function LoadTermCondition() {
  LoadData("PurchaseOrder/LoadTermCondition", function (result) {
    var data = JSON.parse(result).data;
    data = JSON.parse(data)[0];
    if ($("#Tax").val() == "") {
      $("#TermsConditions").html(data.TermConditionNonVAT);
    } else {
      $("#TermsConditions").html(data.TermConditionVAT);
    }
  });
}
function getQueryStringValue(uri, key) {
  var regEx = new RegExp("[\\?&]" + key + "=([^&#]*)");
  var matches = uri.match(regEx);
  return matches == null ? "" : decodeURIComponent(matches[1]);
}
function onGridClickShowCustProfile(THIS) {
  var index = $(THIS).parent().index() + 1;
  var CustID = $("#TableGride tr:eq(" + index + ") td")
    .eq(1)
    .html();
  var url = "#!/CustomerProfile?ID=" + CustID;
  OpenNewTap(url);
}
function AddNewChartOfAccount(ID) {
  ID = typeof ID == "undefined" ? "" : ID;
  var URL =
    "ChartOfAccount/CreateChartOfAccount?PrimaryFieldValue=0&Type=NewChartOfAccount";
  ShowDialogBox2(
    "New Chart Of Account",
    "600",
    "600",
    URL,
    0,
    "DialogNewChartOfAccount",
    function () {
      $("#DialogNewChartOfAccount").on("hide.bs.modal", function () {
        if (ID != "") {
          //$("#" + ID + "").select2('destroy');
          var fun = "Reload_" + ID;
          var fn = window[fun];
          if (typeof fn === "function") {
            return fn();
          }
        }
      });
    },
  );
}
function callbackSaveChangeAddNewChartOfAccount() {
  //loadItem();
  $("#DialogNewChartOfAccount").modal("hide");
}
function AddPaymentMethod(ID) {
  var URL =
    "AddPaymentMethod/CreatePaymentMethod?PrimaryFieldValue=0&Type=NewPaymentMethod";
  ShowDialogBox2(
    "New Payment Method",
    "",
    "",
    URL,
    0,
    "DialogNewPaymentMethod",
    function () {
      $("#DialogNewPaymentMethod").on("hide.bs.modal", function () {
        $("#" + ID + "").select2("destroy");
        var fun = "Reload_" + ID;
        var fn = window[fun];
        if (typeof fn === "function") {
          return fn();
        }
      });
    },
  );
}
function callbackSaveChangeAddNewPaymentMethod() {
  $("#DialogNewPaymentMethod").modal("hide");
}
function AddPaymentCategory(ID) {
  var URL =
    "AddPaymentCategory/CreatePaymentCategory?PrimaryFieldValue=0&Type=NewPaymentCategory";
  ShowDialogBox2(
    "New Payment Category",
    "",
    "",
    URL,
    0,
    "DialogNewPaymentCategory",
    function () {
      $("#DialogNewPaymentCategory").on("hide.bs.modal", function () {
        $("#" + ID + "").select2("destroy");
        var fun = "Reload_" + ID;
        var fn = window[fun];
        if (typeof fn === "function") {
          return fn();
        }
      });
    },
  );
}
function callbackSaveChangeAddNewPaymentCategory() {
  $("#DialogNewPaymentCategory").modal("hide");
}
function NewBill(ID, Type) {
  Type = typeof Type == "undefined" ? "Bill" : Type;
  var URL = "Bills/CreateBill?PrimaryFieldValue=" + ID + "&Type=" + Type;
  var Title = "New Bill";
  if (Type != "") {
    Title = Type;
    URL =
      "Bills/CreateBill?PrimaryFieldValue=" +
      ID +
      "&Type=" +
      Type +
      "&Status=" +
      Type;
  }
  ShowDialogBox(Title, "Auto", "600", URL, ID);
}
function NewPurchaseOrder(ID) {
  var URL =
    "PurchaseOrder/CreatePurchaseOrder?PrimaryFieldValue=" + ID + "&Type=NewPO";
  ShowDialogBox("New Purchase Order", "Auto", "600", URL, ID);
}
function VendorPaymentsMade(VendorID, onItsForm, BillID) {
  VendorID = typeof VendorID == "undefined" ? "0" : VendorID;
  onItsForm = typeof onItsForm == "undefined" ? false : onItsForm;
  BillID = typeof BillID == "undefined" ? "" : BillID;
  if (onItsForm == "") onItsForm = false;
  var URL = "VendorPaymentsMade?onItsForm=" + onItsForm + "&BillID=" + BillID;
  ShowDialogBox(
    "Vendor Payments Made",
    "Auto",
    "600",
    URL,
    VendorID,
    "DialogAddNewItem",
  );
}
function onGridVendorBill_Click(THIS, Type) {
  Type = typeof Type == "undefined" ? "Bill" : Type;
  if ($(THIS).index() != 14) {
    var index = $(THIS).closest("tr").index();
    var BillID = accounting.unformat(
      $(
        "#TblVendorBill" +
          Type +
          " tr[name=TrRowNo]:eq(" +
          index +
          ") td:eq(1)",
      ).html(),
    );
    if (BillID != 0) {
      var VendorID = accounting.unformat(
        $(
          "#TblVendorBill" +
            Type +
            " tr[name=TrRowNo]:eq(" +
            index +
            ") td:eq(2)",
        ).html(),
      );
      var BillNumber = $(
        "#TblVendorBill" +
          Type +
          " tr[name=TrRowNo]:eq(" +
          index +
          ") td:eq(3)",
      ).html();
      var URL =
        "Profile/InvoiceTMP?VendorID=" +
        VendorID +
        "&POID=" +
        BillID +
        "&Type=" +
        Type;
      ShowDialogBox("Preview Bill No.: " + BillNumber, "Auto", "500", URL);
    }
  }
}
function onGridStockIn(THIS) {
  if ($(THIS).index() != 10 && $(THIS).index() != 8) {
    var index = $(THIS).closest("tr").index();
    var VendorID = $(
      "#tblStock tr[name=TrRowNo]:eq(" + index + ") td:eq(1)",
    ).html();
    if (VendorID != "Total QTY IN:") {
      var BillID = $(
        "#tblStock tr[name=TrRowNo]:eq(" + index + ") td:eq(3)",
      ).html();
      var BillNumber = $(
        "#tblStock  tr[name=TrRowNo]:eq(" + index + ") td:eq(4)",
      ).html();
      var URL =
        "Profile/InvoiceTMP?VendorID=" +
        VendorID +
        "&POID=" +
        BillID +
        "&Type=ReceivingNoteFromStockInOnly&ReturnHtmlData=true";
      ShowDialogBox2(
        "Receive Note No.: " + BillNumber,
        "Auto",
        "500",
        URL,
        0,
        "dialogShowBill",
        function (result) {
          //dialogShowBillmodalbody
          result = JSON.parse(result).Data;
          $("#dialogShowBillmodalbody p").html(result);
          setTimeout(function () {
            $("div#div_inv_footer").remove();
            $("#p_TermsConditions").remove();
          }, 500);
          var row = '<i class="ti ti-printer" aria-hidden="true"></i> Print';
          $("#SaveChange2").html(row);
          $("#SaveChange2").click(function () {
            $("#dialogShowBillmodalbody").printThis();
          });
        },
      );
    }
  }
}
function onGridStockInClick(THIS) {
  if ($(THIS).index() != 10 && $(THIS).index() != 8) {
    var index = $(THIS).closest("tr").index();
    var VendorID = $(
      "#tblStock tr[name=TrRowNo]:eq(" + index + ") td:eq(1)",
    ).html();
    if (VendorID != "Total QTY IN:") {
      var BillID = $(
        "#tblStock tr[name=TrRowNo]:eq(" + index + ") td:eq(2)",
      ).html();
      var BillNumber = $(
        "#tblStock  tr[name=TrRowNo]:eq(" + index + ") td:eq(5)",
      ).html();
      var URL =
        "Profile/InvoiceTMP?VendorID=" +
        VendorID +
        "&POID=" +
        BillID +
        "&Type=ReceivingNote&ReturnHtmlData=true";
      ShowDialogBox2(
        "Bill No.: " + BillNumber,
        "Auto",
        "500",
        URL,
        0,
        "dialogShowBill",
        function (result) {
          //dialogShowBillmodalbody
          result = JSON.parse(result).Data;
          $("#dialogShowBillmodalbody p").html(result);
          setTimeout(function () {
            $("div#div_inv_footer").remove();
            $("#p_TermsConditions").remove();
          }, 500);
          var row = '<i class="ti ti-printer" aria-hidden="true"></i> Print';
          $("#SaveChange2").html(row);
          $("#SaveChange2").click(function () {
            $("#dialogShowBillmodalbody").printThis();
          });
        },
      );
    }
  }
}
function decodeHTML(str) {
  if (str != "")
    return str.replace(/(?:\r\n|\r|\n)/g, "&lt;b&gt;"); //'&lt;br&gt;'
  else return str;
}
function decodeHTMLForTextarea(str) {
  if (!str) return "";

  // 1️⃣ Decode HTML entities properly
  let txt = document.createElement("textarea");
  txt.innerHTML = str;
  let decoded = txt.value;

  // 2️⃣ Convert <br> to newline if exists
  decoded = decoded.replace(/<br\s*\/?>/gi, "\n");

  // 3️⃣ Remove remaining HTML tags
  decoded = decoded.replace(/<[^>]*>/g, "");

  return decoded;
}

function stripHtml(html) {
  // Create a temporary element to decode HTML entities
  const temp = document.createElement("div");
  temp.innerHTML = html; // This converts &lt;br&gt; to <br>
  const decoded = temp.textContent;

  // Now parse and strip all HTML tags
  const doc = new DOMParser().parseFromString(decoded, "text/html");
  return doc.body.textContent || "";
}

var decodeEntities = (function () {
  // this prevents any overhead from creating the object each time
  var element = document.createElement("span");

  function decodeHTMLEntities(str) {
    if (str && typeof str === "string") {
      // strip script/html tags
      str = str.replace(/<script[^>]*>([\S\s]*?)<\/script>/gim, "");
      str = str.replace(/<\/?\w(?:[^"'>]|"[^"]*"|'[^']*')*>/gim, "");
      element.innerHTML = str;
      str = element.textContent;
      element.textContent = "";
    }

    return str;
  }

  return decodeHTMLEntities;
})();
function NewTicket(ID, ConnectionID) {
  ConnectionID = typeof ConnectionID == "undefined" ? "" : ConnectionID;
  ID = typeof ID == "undefined" ? "0" : ID;
  var URL = "OpenTicket/CreateTicket?ConnectionID=" + ConnectionID;
  var Title = "Open Ticket";
  if (ID != "0") Title = "Eidt Ticket";
  ShowDialogBox(
    Title,
    "Auto",
    "600",
    URL,
    ID,
    "dialogNewTick",
    "",
    "",
    "",
    function (response) {
      $("#dialogNewTick").on("hide.bs.modal", function () {
        $("#AssignedMe_Tap").click();
      });
    },
  );
}
function onGridTicketClick(THIS, OpenTicketID, TicketNumber, callback) {
  OpenTicketID = typeof OpenTicketID == "undefined" ? "" : OpenTicketID;
  TicketNumber = typeof TicketNumber == "undefined" ? "" : TicketNumber;
  callback = typeof callback == "undefined" ? "" : callback;
  if (THIS && THIS != "") {
    if ($("#TableGride").data("kendoGrid")) {
      var selectedRows = $($(THIS).select()[0].currentTarget);
      var grid = $("#TableGride").data("kendoGrid");
      var selectedItem = grid.dataItem(selectedRows);
      OpenTicketID = selectedItem.OpenTicketID; //selectedRows.find('td:eq(1)').text();
      TicketNumber = selectedItem.TicketNumber; //selectedRows.find('td:eq(2)').text();
    }
    if (OpenTicketID != "" && TicketNumber != "") {
      var URL = "Tickets/ShowTicketDetails?OpenTicketID=" + OpenTicketID;
      ShowDialogBox2(
        "Preview Tickets No.: " + TicketNumber,
        "Auto",
        "500",
        URL,
        0,
        "modalTicketDetail",
        function () {
          $("#modalTicketDetail").on("hide.bs.modal", function () {
            if (callback == "") {
              if (IsCommented) {
                if (typeof CountAssignedMe === "function") CountAssignedMe();
                //ShowTicket('MyTicket');
                $("#AssignedMe_Tap").click();
                IsCommented = false;
              }
            } else if (callback === "function") {
              if (IsCommented) {
                IsCommented = false;
                return callback;
              }
            }
          });
        },
      );
    } else {
      var index = $(THIS).closest("tr").index();
      OpenTicketID = $(
        "#TableCustTicket tr[name=TrRowNo]:eq(" + index + ") td:eq(1)",
      ).html();
      TicketNumber = $(
        "#TableCustTicket tr[name=TrRowNo]:eq(" + index + ") td:eq(2)",
      ).html();
      var URL = "Tickets/ShowTicketDetails?OpenTicketID=" + OpenTicketID;
      ShowDialogBox2(
        "Preview Tickets No.: " + TicketNumber,
        "Auto",
        "500",
        URL,
        0,
        "modalTicketDetail",
        function () {
          setTimeout(function () {
            $("#modalTicketDetailmodalbody").animate(
              {
                scrollTop: $("#modalTicketDetailmodalbody p").height(),
              },
              "slow",
            );
            $("#message").summernote("code", "");
            $("#message").summernote("focus");
            $("#toastsContainerTopRight").remove();
          }, 500);
          //$("#modalTicketDetail").on('hide.bs.modal', function () {
          //    $("#Customer-Ticket-tab").click();
          //});
        },
      );
    }
  } else {
    var URL = "Tickets/ShowTicketDetails?OpenTicketID=" + OpenTicketID;
    ShowDialogBox2(
      "Preview Tickets No.: " + TicketNumber,
      "Auto",
      "500",
      URL,
      0,
      "modalTicketDetail",
      function () {
        setTimeout(function () {
          $("#modalTicketDetailmodalbody").animate(
            {
              scrollTop: $("#modalTicketDetailmodalbody p").height(),
            },
            "slow",
          );
          $("#message").summernote("code", "");
          $("#message").summernote("focus");
          $("#toastsContainerTopRight").remove();
        }, 500);

        $("#modalTicketDetail").on("hide.bs.modal", function () {
          if (callback == "") {
            if (typeof CountAssignedMe === "function") {
              if (IsCommented) {
                CountAssignedMe();
                //ShowTicket('MyTicket');
                $("#AssignedMe_Tap").click();
                IsCommented = false;
              }
            }
          } else {
            if (IsCommented) {
              IsCommented = false;
              $("#Comment_Tap").click();
            }
          }
        });
      },
    );
  }
}
function AddUserToProject(ProjectID) {
  ShowDialogBox2(
    "List Member",
    "Auto",
    "500",
    "Project/AddUser?ProjectID=" + ProjectID,
    0,
    "dialogAddUser",
    function (result) {
      $("#SaveChange2").click(function () {
        var Data = [];
        $.each($("input[name=UserLoginID]:checked"), function (key, val) {
          var obj = {};
          obj = { ProjectID: ProjectID, UserID: $(val).val() };
          Data.push(obj);
        });
        if (Data.length == 0) {
          DialogSweetAlert("error", "Please select at last one user.");
        } else {
          Data = { Data: JSON.stringify(Data) };
          DoSaveChangeData("Project/SaveAddUser", Data, function (result) {
            $("#lblusername").val("");
            $("#dialogAddUser").modal("hide");
            $("#btnsearchUProject").click();
          });
        }
      });
    },
  );
}
function AddUserToGroup(GroupID) {
  ShowDialogBox2(
    "List Member",
    "Auto",
    "500",
    "Group/AddUser?GroupID=" + GroupID,
    0,
    "dialogAddUser",
    function (result) {
      $("#SaveChange2").click(function () {
        var Data = [];
        $.each($("input[name=UserLoginID]:checked"), function (key, val) {
          var obj = {};
          obj = { GroupID: GroupID, UserID: $(val).val() };
          Data.push(obj);
        });
        if (Data.length == 0) {
          DialogSweetAlert("error", "Please select at last one user.");
        } else {
          Data = { Data: JSON.stringify(Data) };
          DoSaveChangeData("Group/SaveAddUser", Data, function (result) {
            $("#lblusername").val("");
            $("#dialogAddUser").modal("hide");
            $("#btnsearchUGroup").click();
          });
        }
      });
    },
  );
}

function SaveChangeAgency(CustomerID) {
  let objData = [];
  let obj = {};
  if ($("#AgencyName").val() == "") {
    DialogSweetAlert("error", "Please enter Agency Name.", function () {
      $("#AgencyName").focus();
    });
    return;
  } else if ($("#AgencyPhone").val() == "") {
    DialogSweetAlert("error", "Please enter Agency Phone.", function () {
      $("#AgencyPhone").focus();
    });
    return;
  } else if ($("#AgencyCommissionType").val() == "") {
    DialogSweetAlert(
      "error",
      "Please enter Agency Commission Type.",
      function () {
        $("#AgencyCommissionType").focus();
      },
    );
    return;
  }
  $("#frmAgencyCommission .CustInfo").each(function (key, value) {
    let name =
      typeof $(value).attr("id") == "undefined" ? "" : $(value).attr("id");
    if (name != "") {
      obj[name] = encodeURIComponent(nl2br($(value).val().toString()));
    }
  });
  obj["CustomerID"] = CustomerID;
  objData.push(obj);
  objData = { Data: JSON.stringify(objData[0]) };
  PostData(
    "NewCustomerProfile/SaveChangeAgency",
    objData,
    function (response) {
      response = JSON.parse(response);
      if (response.Result == 2) {
        DialogSweetAlert("loadsuccess", response.Message);
        $("#frmAgencyCommission #AgencyCommissionID").val(response.Value);
      } else DialogSweetAlert("error", response.Message);
    },
    false,
  );
}
(function ($) {
  $.fn.drags = function (opt) {
    return false;
    opt = $.extend({ handle: "", cursor: "move" }, opt);

    var $el = null;
    if (opt.handle === "") {
      $el = this;
    } else {
      $el = this.find(opt.handle);
    }

    return $el
      .css("cursor", opt.cursor)
      .on("mousedown", function (e) {
        var $drag = null;
        if (opt.handle === "") {
          $drag = $(this).parents(".modal-dialog").addClass("draggable");
        } else {
          $drag = $(this)
            .parents(".modal-dialog")
            .addClass("active-handle")
            .parent()
            .addClass("draggable");
        }
        var z_idx = $drag.css("z-index"),
          drg_h = $drag.outerHeight(),
          drg_w = $drag.outerWidth(),
          pos_y = $drag.offset().top + drg_h - e.pageY,
          pos_x = $drag.offset().left + drg_w - e.pageX;
        $drag
          .css("z-index", 1000)
          .parents()
          .on("mousemove", function (e) {
            $(".draggable")
              .offset({
                top: e.pageY + pos_y - drg_h,
                left: e.pageX + pos_x - drg_w,
              })
              .on("mouseup", function () {
                $(this).removeClass("draggable").css("z-index", z_idx);
              });
          });
        e.preventDefault(); // disable selection
      })
      .on("mouseup", function () {
        if (opt.handle === "") {
          $(this).removeClass("draggable");
        } else {
          $(this)
            .removeClass("active-handle")
            .parent()
            .removeClass("draggable");
        }
      });
  };
})(jQuery);
function TapScroll() {
  var scrollBarWidths = 40;

  var widthOfList = function () {
    var itemsWidth = 0;
    $(".list li").each(function () {
      var itemWidth = $(this).outerWidth();
      itemsWidth += itemWidth;
    });
    return itemsWidth;
  };

  var widthOfHidden = function () {
    var ww = 0 - $(".wrapper-nav").outerWidth();

    var hw =
      $(".wrapper-nav").outerWidth() -
      widthOfList() -
      getLeftPosi() -
      scrollBarWidths;

    var rp =
      $(document).width() -
      ($(".list li").last().offset().left + $(".list li").last().outerWidth());
    if (ww > hw) {
      return ww;
      //return (rp > ww ? rp : ww);
    } else {
      return hw;
      //return (rp > hw ? rp : hw);
    }
  };

  var getLeftPosi = function () {
    var ww = 0 - $(".wrapper-nav").outerWidth();
    var lp = $(".list").position().left;

    if (ww > lp) {
      return ww;
    } else {
      return lp;
    }
  };

  var reAdjust = function () {
    // check right pos of last nav item
    if ($(".list li")) {
      if ($(".list li").last().offset()) {
        var rp =
          $(document).width() -
          ($(".list li").last().offset().left +
            $(".list li").last().outerWidth());

        if ($(".wrapper-nav").outerWidth() < widthOfList() && rp < 0) {
          $(".scroller-right").show().css("display", "flex");
        } else {
          $(".scroller-right").hide();
        }

        if (getLeftPosi() < 0) {
          $(".scroller-left").show().css("display", "flex");
        } else {
          $(".scroller-left").hide();
        }
      }
    }
  };

  var divwrapperTap = document.getElementById("divwrapperTap");
  new ResizeSensor(divwrapperTap, function (size) {
    reAdjust();
  });

  $(window).on("resize", function (e) {
    reAdjust();
  });

  $(".scroller-right").click(function () {
    $(".scroller-left").fadeIn("slow");
    $(".scroller-right").fadeOut("slow");
    $(".list").animate(
      { left: "+=" + widthOfHidden() + "px" },
      "slow",
      function () {
        //console.log('');
      },
    );
    //$('.list').animate({ left: `+=${widthOfHidden()}px` }, 'slow', function () {
    //  reAdjust();
    //});
  });

  $(".scroller-left").click(function () {
    $(".scroller-right").fadeIn("slow");
    $(".scroller-left").fadeOut("slow");

    $(".list").animate({ left: `-=${getLeftPosi()}px` }, "slow", function () {
      reAdjust();
    });
  });
}
function ShowCustomerInfo(THIS) {
  var cur = $(THIS).index();
  if (cur != 2 && cur != 1) {
    var index = $(THIS).parent().index() + 1;
    $("#TableGride tr:eq(" + index + ") td:eq(1) a")[0].click();
  }
}
function AddNewStockReason() {
  ShowDialogBox2(
    "Add Stock Reason",
    "",
    "",
    "AddStockReason/CreateStockReason?Type=AddNewStockReason",
    0,
    "DialogAddNewStockReason",
    function () {
      $("#DialogAddNewStockReason").on("hide.bs.modal", function () {
        Reload_Reason();
      });
    },
  );
}
function CustomerPaymentsMade(CustomerID) {
  ShowDialogBox(
    "Customer Payments Made",
    "Auto",
    "600",
    "CustomerPaymentsMade",
    CustomerID,
    "DialogCustomerPaymentsMade",
  );
}
function downloadInnerHtml(HtmlData, filename, mimeType) {
  filename = typeof filename == "undefined" ? "dataExport.html" : filename;
  mimeType =
    typeof mimeType == "undefined" ? "text/html;charset=UTF-8" : mimeType;
  //var elHtml = HtmlData;//document.getElementById(elId).innerHTML;
  //var link = document.createElement('a');
  ////mimeType = mimeType || 'text/plain';

  //link.setAttribute('download', filename);
  //link.setAttribute('href', 'data:' + mimeType + ';charset=utf-8,' + encodeURIComponent(elHtml));
  ////link.click();
  ////OpenNewTap(link.href);
  //console.log(link.href);
  //link.remove();
  var textToSave = HtmlData;
  var textToSaveAsBlob = new Blob([textToSave], { type: mimeType });
  var textToSaveAsURL = window.URL.createObjectURL(textToSaveAsBlob);
  //inputFileNameToSaveAs-->The text field in which the user input for
  //the desired file name is input into.
  //var fileNameToSaveAs = filename;
  OpenNewTap(textToSaveAsURL, "", true);
  //var downloadLink = document.createElement("a");
  //downloadLink.download = fileNameToSaveAs;
  //downloadLink.innerHTML = "Download File";
  //downloadLink.href = textToSaveAsURL;
  //downloadLink.onclick = destroyClickedElement;
  //downloadLink.style.display = "none";
  //document.body.appendChild(downloadLink);
  //downloadLink.click();
  //downloadLink.remove();
}
function destroyClickedElement(event) {
  document.body.removeChild(event.target);
}

function clickAndDrag(
  selector,
  scroll_speed = 3,
  classOnEvent = "grabbed_elem",
) {
  if (getQuerySelector(selector) != null) {
    var slider =
      typeof document.querySelector(selector) == "undefined"
        ? ""
        : document.querySelector(selector);
    let isDown = false;
    let startX;
    let scrollLeft;
    if (slider != null && slider != "") {
      slider.addEventListener("mousedown", (e) => {
        //e.preventDefault();
        isDown = true;
        slider.classList.add(classOnEvent);
        startX = e.pageX - slider.offsetLeft;
        scrollLeft = slider.scrollLeft;

        // prevent default child behavior
        document.body.addEventListener("click", function (event) {
          //if (slider.contains(event.target)) {
          //    event.preventDefault();
          //}
        });
      });
      slider.addEventListener("mouseleave", () => {
        isDown = false;
        slider.classList.remove(classOnEvent);
      });
      slider.addEventListener("mouseup", () => {
        isDown = false;
        slider.classList.remove(classOnEvent);
      });
      slider.addEventListener("mousemove", (e) => {
        if (!isDown) return;
        e.preventDefault();
        const x = e.pageX - slider.offsetLeft;
        const walk = (x - startX) * scroll_speed; //scroll-fast
        slider.scrollLeft = scrollLeft - walk;
      });
    }
  }
}
function getQuerySelector(txt) {
  try {
    return document.querySelector(txt);
  } catch (e) {
    return null;
  }
}
function isJQuery(obj) {
  return jQuery.isPlainObject(obj);
}
function malformedJSON2Object(tar) {
  var obj = {};
  tar = tar.replace(/^\{|\}$/gi, "").split(",");
  for (var i = 0, cur, pair; (cur = tar[i]); i++) {
    pair = cur.split(":");
    if (pair[1]) {
      obj[pair[0].replace(/"/gi, "")] = /^\d*$/.test(pair[1])
        ? +pair[1]
        : pair[1].replace(/"/gi, "");
    } else {
      var p = pair;
      cur = tar[i - 1];
      pair = cur.split(":");
      obj[pair[0].replace(/"/gi, "")] = (pair[1] + "," + p).replace(/"/gi, "");
    }
  }
  return obj;
}

function stringformedJSON2Object(tar, removeDoublequote) {
  removeDoublequote = typeof removeDoublequote == "undefined" ? false : true;
  var obj = {};
  tar = tar.split(","); //tar = tar.replace(/^\{|\}$/ig, '').split(',');
  for (var i = 0, cur, pair; (cur = tar[i]); i++) {
    pair = cur.split(":");
    //console.log(pair);
    if (pair[1]) {
      if (!removeDoublequote)
        obj[pair[0].replace(/"/gi, "")] = pair[1].replace(/"/gi, "`"); ///^\d*$/.test(pair[1]) ? +pair[1] : pair[1].replace(/"/ig, '`');
      else {
        obj[pair[0].replace(/"/gi, "")] = pair[1]
          .toString()
          .replaceAll(/\"/g, "`");
      }
    }
    //else {
    //    var p = pair;
    //    cur = tar[i - 1];
    //    pair = cur.split(':');
    //    obj[pair[0].replace(/"/ig, '')] = (pair[1] + "," + p).replace(/"/ig, '');
    //}
  }
  return obj;
}
function decode(str) {
  let txt = new DOMParser().parseFromString(str, "text/html");

  return txt.documentElement.textContent;
}
function ExportToExcel(responetext, TableID) {
  TableID = typeof TableID == "undefined" ? "" : TableID;
  //TableID = "123";
  if (TableID != "") {
    $("#divExportTableToExcel").remove();
    var row =
      `<div id="divExportTableToExcel" style="display:none;">` +
      responetext +
      `</div>`;
    $(document.body).append(row);

    //console.log(responetext);
    var tab_text = '<table border="1px" style="font-size:20px" ">';
    var textRange;
    var j = 0;
    var tab = document.getElementById(TableID); //$("#divExportTableToExcel table:eq(1)")[0];//document.getElementById(TableID); // id of table
    //console.log(tab);
    var lines = tab.rows.length;

    // the first headline of the table
    if (lines > 0) {
      tab_text =
        tab_text + '<tr bgcolor="#DFDFDF">' + tab.rows[0].innerHTML + "</tr>";
    }

    // table data lines, loop starting from 1
    for (j = 1; j < lines; j++) {
      tab_text = tab_text + "<tr>" + tab.rows[j].innerHTML + "</tr>";
    }

    tab_text = tab_text + "</table>";
    tab_text = tab_text.replace(/<A[^>]*>|<\/A>/g, ""); //remove if u want links in your table
    //tab_text = tab_text.replace(/<img[^>]*>/gi, "");                 // remove if u want images in your table
    tab_text = tab_text.replace(/<input[^>]*>|<\/input>/gi, ""); // reomves input params
    // console.log(tab_text); // aktivate so see the result (press F12 in browser)

    var ua = window.navigator.userAgent;
    var msie = ua.indexOf("MSIE ");

    // if Internet Explorer
    if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./)) {
      txtArea1.document.open("txt/html", "replace");
      txtArea1.document.write(tab_text);
      txtArea1.document.close();
      txtArea1.focus();
      sa = txtArea1.document.execCommand("SaveAs", true, "DataTableExport.xls");
    } // other browser not tested on IE 11
    else
      sa = window.open(
        "data:application/vnd.ms-excel," + encodeURIComponent(tab_text),
      );
    $("#divExportTableToExcel").remove();
    return sa;
  } else {
    exportExcel(responetext);
  }
}
function formatBytes(bytes) {
  var marker = 1000; //1024; // Change to 1000 if required
  var decimal = 0; // Change as required
  var kiloBytes = marker; // One Kilobyte is 1024 bytes
  var megaBytes = marker * marker; // One MB is 1024 KB
  var gigaBytes = marker * marker * marker; // One GB is 1024 MB

  // return bytes if less than a KB
  if (bytes < kiloBytes) return bytes + " Bytes";
  // return KB if less than a MB
  else if (bytes < megaBytes)
    return (bytes / kiloBytes).toFixed(decimal) + " KB";
  // return MB if less than a GB
  else if (bytes < gigaBytes)
    return (bytes / megaBytes).toFixed(decimal) + " MB";
  // return GB if less than a TB
  else return (bytes / gigaBytes).toFixed(decimal) + " GB";
}

function PrintHTML(
  HTML,
  Title,
  ISPrintHeader,
  ISAutoDownlaod,
  FileName,
  GetContent,
  arrCompanyInfo,
) {
  let companyObj =
    typeof arrCompanyInfo != "undefined" && arrCompanyInfo.length > 0
      ? JSON.parse(arrCompanyInfo[0])[0] || {}
      : {};
  let logPath =
    typeof arrCompanyInfo != "undefined" && arrCompanyInfo.length > 0
      ? arrCompanyInfo[1].replace("../", "") || ""
      : "";
  ISPrintHeader = typeof ISPrintHeader == "undefined" ? true : ISPrintHeader;
  FileName = typeof FileName == "undefined" ? "SaveFile.pdf" : FileName;
  Title = typeof Title == "undefined" ? "Print Preview" : Title;
  ISAutoDownlaod =
    typeof ISAutoDownlaod == "undefined" ? false : ISAutoDownlaod;
  GetContent = typeof GetContent == "undefined" ? false : GetContent;
  var row =
    `<div class="row p-2">
            <div class="col">
                <img class="img-fluid p-2" width="240" src="${ServerRoot + logPath}" />
            </div>
            <div class="col-auto">
                <h5 class="khmerFontMoul">
                    ${Object.keys(companyObj).length > 0 ? companyObj.CompanyNameKH : ""}<br />
                </h5>
                <h5 class="text-center">
                    ${Object.keys(companyObj).length > 0 ? companyObj.CompanyName : ""}
                </h5>
                <div class="text-center">
                    <span style="color: #555;">~~~~~~~<i style="font-size: 12px" class="fa-duotone fa-asterisk"></i>~~~~~~~</span>
                </div><br />
                <div class="text-center">
                    <h5 class="text-uppercase">
                        ` +
    Title +
    `
                    </h5>
                </div>
            </div>
            <div class="col">
                &nbsp;
            </div>
        </div>`;
  var dt =
    '<html><head><meta http-equiv="Content-Type" content="text/html; charset=UTF-8"><title>' +
    Title +
    '</title><meta charset="utf-8" />';
  dt +=
    `<link href="` +
    ServerRoot +
    `Scripts/kendo/css/kendo.bootstrap-v4.min.css" rel="stylesheet" />`;
  dt +=
    `<link href="` + ServerRoot + `Content/css/custom.css" rel="stylesheet" />`;
  dt += `<link href="https://fonts.googleapis.com/css2?family=Khmer&family=Moul&display=swap" rel="stylesheet" crossorigin>
    <link href="https://fonts.googleapis.com/css?family=Source+Sans+Pro:300,400,400i,700" rel="stylesheet" crossorigin />
    <link href='https://fonts.googleapis.com/css?family=Bayon' rel='stylesheet' type='text/css' crossorigin />
    <link href='https://fonts.googleapis.com/css?family=Bayon|Francois+One' rel='stylesheet' type='text/css' crossorigin />
    <link rel="preconnect" href="https://fonts.googleapis.com" crossorigin />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Kantumruy+Pro:wght@200;300&display=swap" rel="stylesheet" crossorigin>`;
  dt += '<script src="' + ServerRoot + 'Scripts/jquery-3.7.1.min.js"></script>';
  dt +=
    '<script src="' + ServerRoot + 'Scripts/kendo/kendo.all.min.js"></script>';
  dt +=
    '<link href="' +
    ServerRoot +
    'Content/fontawesome-free-6.3.0-web/css/all.css" rel="stylesheet" />';
  dt +=
    '<link href="' +
    ServerRoot +
    'Content/fontawesome-free-6.3.0-web/css/sharp-regular.css" rel="stylesheet" />';
  dt +=
    '<link href="' +
    ServerRoot +
    'Content/AdminLTE-3.2.0/dist/css/adminlte.min.css" rel="stylesheet" />';
  dt += `<style>
    .khmerFont {
            font-family: 'Noto Sans Khmer', sans-serif;
        }

        .khmerFontMoul {
            font-family: 'Moul', cursive;
        }

        .khmerFont1-Header {
            font-family: 'Kantumruy Pro', sans-serif;
            font-weight: 900;
            font-size: 130%;
            color: white;
        }


    </style>`;
  dt += '</head><body class="divcontent">';
  if (ISPrintHeader) dt += row;
  dt += '<div id="divPrintPreview">';
  dt += HTML;
  dt += "</div>";
  var src = `<script>
    $(function(){
  var curDown = false,
      curYPos = 0,
      curXPos = 0;

  $(window).mousemove(function(m){
    if(curDown){
      window.scrollBy(curXPos - m.pageX, curYPos - m.pageY)
    }
  });

  $(window).mousedown(function(m){
    curYPos = m.pageY;
    curXPos = m.pageX;
    curDown = true;
  });

  $(window).mouseup(function(){
    curDown = false;
  });
})
</script>`;
  dt += src;
  dt += "</body></html>";

  var textToSave = dt;
  var textToSaveAsBlob = new Blob([textToSave], {
    type: "text/html;charset=UTF-8",
  });
  var textToSaveAsURL = window.URL.createObjectURL(textToSaveAsBlob);
  if (!GetContent) OpenNewTap(textToSaveAsURL, "", true);
  else return dt;
  //else {
  //  console.log(textToSaveAsBlob);
  //var blob = b64toBlob(textToSaveAsBlob, 'application/pdf');
  //CreatePDFfromHTML("divPrintPreview", "Customer_Notification.pdf", true);
  //var downloadLink = document.createElement("a");
  //downloadLink.download = FileName;
  //downloadLink.innerHTML = "Download File";
  //downloadLink.href = textToSaveAsURL;
  //downloadLink.onclick = destroyClickedElement;
  //downloadLink.style.display = "none";
  //document.body.appendChild(downloadLink);
  //downloadLink.click();
  //downloadLink.remove();
  //downloadFile(textToSaveAsURL, FileName);
  //}
}
function GetUserONUInfoByConnection(PPPeoEUserName, key) {
  var url =
    ServerRoot +
    "CustomerProfile/GetUserONUInfo?SearchInfo=" +
    encodeURIComponent(PPPeoEUserName);
  //$.get(url, "", function (data, status, xhr) {
  //    var tblData = data.Data;
  //    $("#div_" + key + "").html(tblData);
  //}, "json");
}
function GetUserONUInfo(PPPeoEUserName, key) {
  var url =
    ServerRoot +
    "CustomerProfile/GetUserONUInfo?SearchInfo=" +
    encodeURIComponent(PPPeoEUserName);
  $.get(
    url,
    "",
    function (data, status, xhr) {
      var tblData = data.Data;
      $("#lblPPPoEStatus_" + key).html(tblData);
    },
    "json",
  );
}
function RebootONUDevice(PPPeoEUserName, key) {
  DoSaveChangeSweetCofirm(
    "CustomerProfile/GetUserONUInfo?SearchInfo=" +
      encodeURIComponent(PPPeoEUserName) +
      "&getContent=false&Reboot=true",
    "",
    function (response) {
      //console.log(response)
      $("span[name=ONUStatus]").eq(key).removeClass("badge badge-primary");
      $("span[name=ONUStatus]").eq(key).addClass("badge badge-danger");
      $("span[name=ONUStatus]").eq(key).html("Offline");
      var timer = '&nbsp;<span id="timer_' + key + '"></span>';
      $("span[name=ONUStatus]").eq(key).append(timer);
      var runEvery = 2 * 60; // 15 minutes in seconds
      var showRemaining = () => {
        // get current time in seconds and find when the next run starts in seconds
        var seconds = Math.round(Date.now() / 1000);
        var nextRun = runEvery * Math.ceil(seconds / runEvery);
        var timeLeft = nextRun - seconds;
        var minutesLeft = Math.floor(timeLeft / 60);
        var secondsLeft = timeLeft % 60;
        $("#timer_" + key).html(
          minutesLeft + ":" + ("0" + secondsLeft).slice(-2),
        );
      };
      showRemaining();
      setInterval(showRemaining, 1000);
      setTimeout(function () {
        GetUserONUInfo(PPPeoEUserName, key);
      }, 70000);
    },
    "Rebooting...",
  );
}
function DeactivateONUDevice(PPPeoEUserName, key) {
  DoSaveChangeSweetCofirm(
    "CustomerProfile/GetUserONUInfo?SearchInfo=" +
      encodeURIComponent(PPPeoEUserName) +
      "&getContent=false&Deactivate=true",
    "",
    function (response) {
      //console.log(response)
      var timer = '&nbsp;<span id="timer_' + key + '"></span>';
      $("span[name=ONUStatus]").eq(key).append(timer);
      var runEvery = 60; // 15 minutes in seconds
      var showRemaining = () => {
        // get current time in seconds and find when the next run starts in seconds
        var seconds = Math.round(Date.now() / 1000);
        var nextRun = runEvery * Math.ceil(seconds / runEvery);
        var timeLeft = nextRun - seconds;
        var minutesLeft = Math.floor(timeLeft / 60);
        var secondsLeft = timeLeft % 60;
        $("#timer_" + key).html(
          minutesLeft + ":" + ("0" + secondsLeft).slice(-2),
        );
      };
      showRemaining();
      setInterval(showRemaining, 1000);
      GetUserONUInfo(PPPeoEUserName, key);
    },
    "Deactivating...",
  );
}

function PrintProductLable(Barcode, QTY, Code) {
  ShowDialogBox(
    "Print Product Label",
    "500",
    "500",
    "Items/PrintProductLabel?Barcode=" +
      Barcode +
      "&QTY=" +
      QTY +
      "&Code=" +
      Code,
    0,
    "DialogPrintLabel",
    "",
    "",
    "",
    function () {
      $("#SaveChange").html(
        '<i class="ti ti-printer" aria-hidden="true"></i> Print',
      );
      $("#SaveChange").click(function () {
        $("#div_PrintProductLable").printThis({
          title: "NSM_Product_Label",
          //deferred: $.Deferred().done(function () {
          //  window.close();
          //})
        });
      });
    },
  );
}

function ApplyToInvoices(CreditNoteNo, CustomerID, CustomerName) {
  ShowDialogBox2(
    "Apply Credits to Invoices",
    "auto",
    500,
    "CustomerProfile/ApplyToInvoices?CreditNoteNo=" +
      encodeURIComponent(CreditNoteNo) +
      "&CustomerID=" +
      CustomerID +
      "&CustomerName=" +
      encodeURIComponent(CustomerName),
    0,
    "DialogApplyToInvoices",
    function () {
      setTimeout(function () {
        $("input[name=CreditstoApply]").val("0.00");
        $("input[name=CreditstoApply]").css({ "text-align": "right" });
        $("input[name=CreditstoApply]").autoNumeric({
          vMin: "0.00",
          vMax: "999999999.00",
        });
      }, 500);
      //$("#DialogApplyToInvoices").on('hide.bs.modal', function () {

      //});
    },
  );
}

function renameKey(obj, oldKey, newKey) {
  obj[newKey] = obj[oldKey];
  delete obj[oldKey];
}
function SendNotificationRequestItem() {
  if (SaveChangeResponse.length > 0) {
    var ID = SaveChangeResponse["Data"].IDENTITYValue;
    var IDs = ID.split("~");
    ID = IDs[0].split("|")[1];
    SaveChangeResponse = [];
    PostData(
      "Items/SendNotificationRequestItem?ID=" + ID,
      function (response) {},
    );
  }
}
function ApproveDisapproveItemRequest(RequestItemID, Status, RequestMode) {
  RequestMode =
    typeof RequestMode == "undefined" ? "Item Request" : RequestMode;
  var msg =
    "Approval this Inventory Request. You won't be able to revert this!";
  var m = "Approving...";
  if (Status == "Disapproved") {
    m = "Disapproving...";
    msg =
      "Disapprove this Inventory Request. You won't be able to revert this!";
  }
  DoSaveChangeSweetCofirm(
    "Items/ApproveDisapproveItemRequest?ID=" +
      RequestItemID +
      "&Status=" +
      Status +
      "&RequestMode=" +
      RequestMode,
    "",
    function (response) {
      $("#BunSearch").click();
    },
    m,
    msg,
  );
}
function onGridItemRequest(THIS, IsRequestApprovalItemOut) {
  var index = $(THIS).closest("tr").index();
  var tdindex = $("tr[name=TrRowNo]:eq(" + index + ") td").index(THIS);
  if (tdindex < 6) {
    var RequestItemID = $("tr[name=TrRowNo]:eq(" + index + ") td:eq(1)").html();
    var RequestNo = $("tr[name=TrRowNo]:eq(" + index + ") td:eq(3)").html();
    ShowDialogBox2(
      "Request No:. " + RequestNo,
      "auto",
      "500",
      "Items/ItemRequestForm?ID=" +
        RequestItemID +
        "&IsRequestApprovalItemOut=" +
        IsRequestApprovalItemOut,
      0,
      "dialogitemrequest",
      function () {
        $("#SaveChange2").html(
          '<i class="ti ti-printer" aria-hidden="true"></i> Print',
        );
        $("#SaveChange2").click(function () {
          $("#divItemRequestForm").printThis();
        });
      },
    );
  }
}
function ValidateQTYOUT() {
  var checked = true;
  $("tr[name=TR_Items_Request]").each(function (key, value) {
    var index = $(this).index();
    var AvariableQTY = accounting.unformat(
      $("input[name=AvariableQTY]").eq(key).val(),
    );
    var QTY = accounting.unformat($("input[name=QTY]").eq(key).val());
    if (QTY > AvariableQTY) {
      DialogSweetAlert(
        "error",
        "You cannot request QTY bigger then Avariable QTY.",
        function () {
          $("input[name=QTY]").eq(key).focus();
        },
      );
      checked = false;
      return checked;
    }
  });
  return checked;
}
function uniqueID() {
  return Math.floor(Math.random() * Date.now());
}
function JSONToCSVConvertor(JSONData, ReportTitle, ShowLabel) {
  //If JSONData is not an object then JSON.parse will parse the JSON string in an Object
  var arrData = typeof JSONData != "object" ? JSON.parse(JSONData) : JSONData;

  var CSV = "";
  //Set Report title in first row or line

  CSV += ReportTitle + "\r\n\n";

  //This condition will generate the Label/Header
  if (ShowLabel) {
    var row = "";

    //This loop will extract the label from 1st index of on array
    for (var index in arrData[0]) {
      //Now convert each value to string and comma-seprated
      row += index + ",";
    }

    row = row.slice(0, -1);

    //append Label row with line break
    CSV += row + "\r\n";
  }

  //1st loop is to extract each row
  for (var i = 0; i < arrData.length; i++) {
    var row = "";

    //2nd loop will extract each column and convert it in string comma-seprated
    for (var index in arrData[i]) {
      row += '"' + arrData[i][index] + '",';
    }

    row.slice(0, row.length - 1);

    //add a line break after each row
    CSV += row + "\r\n";
  }

  if (CSV == "") {
    alert("Invalid data");
    return;
  }

  //Generate a file name
  var fileName = "MyReport_";
  //this will remove the blank-spaces from the title and replace it with an underscore
  fileName += ReportTitle.replace(/ /g, "_");

  //Initialize file format you want csv or xls
  var uri = "data:text/csv;charset=utf-8," + escape(CSV);

  // Now the little tricky part.
  // you can use either>> window.open(uri);
  // but this will not work in some browsers
  // or you will not get the correct file extension

  //this trick will generate a temp <a /> tag
  var link = document.createElement("a");
  link.href = uri;

  //set the visibility hidden so it will not effect on your web-layout
  link.style = "visibility:hidden";
  link.download = fileName + ".csv";

  //this part will append the anchor tag and remove it after automatic click
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

function detectType(value) {
  var a = Number(value);
  var b = /^-?\d+\,?\d+(\.\d+)?$/.test(value);

  if (a.toString() == "NaN" && !b) return value;
  else return accounting.unformat(value);
  //if (typeof variable === 'number') {
  //    return accounting.unformat(variable);
  //} else if (typeof variable === 'string') {
  //    return variable;
  //} else {
  //    return variable;
  //}
}
function isNumber(value) {
  return /^-?\d+(\,?\d+)+(\.\d+)?$/.test(value);
}
function GetStockUnit(ItemID, Qty) {
  try {
    const response = $.ajax({
      type: "GET",
      url: ServerRoot + "Items/GetConvertUnit?ItemID=" + ItemID + "&Qty=" + Qty,
    });
    return response;
  } catch (error) {
    console.error("Error fetching stock unit:", error);
  }
}

function correctKhmerAddress(address) {
  if (address == ".") return address;
  // Step 1: Remove leading or misplaced non-letter characters
  address = address.replace(/^[^\p{L}]*/u, ""); // Remove non-letter characters from the start

  // Step 2: Replace double commas with a single comma and ensure proper spacing after punctuation
  address = address.replace(/\s*,\s*,/g, ", "); // Remove double commas
  address = address.replace(/\s*,\s*/g, ", "); // Ensure single space after commas
  address = address.replace(/\s*៖ុ\s*/g, "៖ "); // Correct Khmer colon

  // Step 3: Add spaces between Khmer letters and numbers, but ignore Latin characters (like "Unit B0601")
  address = address.replace(/([\u1780-\u17FF])(\d)/gu, "$1 $2"); // Add space between Khmer letters and numbers
  address = address.replace(/(\d)([\u1780-\u17FF])/gu, "$1 $2"); // Add space between numbers and Khmer letters

  // Step 4: Handle misplaced punctuation for Khmer-specific characters
  address = address.replace(/\s*។\s*/g, "។ "); // Ensure single space after "។"
  address = address.replace(/\s*៖\s*/g, "៖ "); // Ensure single space after "៖"

  // Step 5: Trim leading and trailing spaces
  address = address.trim();

  // Step 6: Remove extra spaces between words (if more than one space exists)
  address = address.replace(/\s{2,}/g, " "); // Replace multiple spaces with a single space

  // Step 7: Fix specific mistakes (e.g., space after "ផ្ទះលេខ")
  address = address.replace(/ផ្ទះលេខ\s*(\w+)/g, "ផ្ទះលេខ $1");

  return address;
}

function fullyDecodeHtml(html) {
  const parser = new DOMParser();
  let prev = html;
  let decoded = parser.parseFromString(prev, "text/html").body.textContent;

  while (decoded !== prev) {
    prev = decoded;
    decoded = parser.parseFromString(prev, "text/html").body.textContent;
  }

  return decoded;
}
function printVoucher(id) {
  //sendVoucherToTelegram(id);
  window.open("/Expenses/PrintVoucherPdf?id=" + id, "_blank");
}
function showPaymentVoucher(ExpenseID, No, Status, PaymentNo) {
  ShowDialogBox(
    `Payment Voucher: ${No}`,
    "Auto",
    500,
    "Expenses/PrintVoucher/?id=" + ExpenseID + "&PaymentNo=" + PaymentNo,
    0,
    "",
    false,
    0,
    0,
    function () {
      if (Status == "Draft") {
        var btn = `<button type="button" id="btnCheck" onclick="Check('Checked',${ExpenseID},'${PaymentNo}')" class="btn btn-outline-primary me-2">`;
        btn += '<i class="ti ti-check me-1"></i>Check</button>';
        $("#SaveChange").before(btn);

        btn = `<button type="button" id="btnReject" onclick="Check('Rejected',${ExpenseID},'${PaymentNo}')" class="btn btn-outline-danger me-2">`;
        btn += '<i class="ti ti-x me-1"></i>Reject</button>';
        $("#SaveChange").before(btn);
      } else if (Status == "Checked") {
        var btn = `<button type="button" id="btnCheck" onclick="Check('Confirm Verified',${ExpenseID},'${PaymentNo}')" class="btn btn-outline-primary me-2">`;
        btn += '<i class="ti ti-check me-1"></i>Confirm Verified</button>';
        $("#SaveChange").before(btn);

        btn = `<button type="button" id="btnReject" onclick="Check('Confirm Rejected',${ExpenseID},'${PaymentNo}')" class="btn btn-outline-danger me-2">`;
        btn += '<i class="ti ti-x me-1"></i>Reject</button>';
        $("#SaveChange").before(btn);
      }
      $("#SaveChange").html(
        `<i class="ti ti-printer" aria-hidden="true"></i> Print`,
      );
      $("#SaveChange").click(function () {
        printVoucher(ExpenseID);
        //$(".voucher-container").printThis({
        //    title: "Payment Voucher",
        //    //deferred: $.Deferred().done(function () {
        //    //  window.close();
        //    //})
        //});
      });
    },
  );
}

function Check(Status, ExpenseID, PaymentNo) {
  var Data = {
    ExpenseID: ExpenseID,
    Status: Status,
    PaymentNo: PaymentNo,
  };

  DoSaveChangeSweetCofirm(
    "Expenses/ApprovalPaymentVoucherAsync",
    Data,
    function (Response) {
      if (!Response.success) {
        DialogSweetAlert("error", Response.message);
        return;
      }
      $("#CloseModal").click();
      $("#BunSearch").click();

      if ($("#Transcation_tab").length > 0) $("#Transcation_tab").click();
      console.log($("#Transcation_tab").length);
    },
  );
}
