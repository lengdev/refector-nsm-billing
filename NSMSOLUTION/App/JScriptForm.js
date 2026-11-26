var appForm = angular.module('Form', []);
var files = [], fileNames = [], img = "", imgFile = [], jcrop_api = "", viewerImg = "";
var imageObj = new Image(), canvas = "", context = "", fileIndex = 0, fileName = []; 
var sourceX = 0, sourceY = 0, sourceWidth = 0, sourceHeight = 0;
var destWidth = 0, destHeight = 0, destX = 0, destY = 0;

var removeFile = function(index){
	files.splice(index, 1);
	fileNames.splice(index, 1);
	fileName.splice(index, 1);
	imgFile.splice(index, 1);
}
appForm.factory('Form', function ($rootScope, $compile, Restangular, $http, $q, $sce, AuthenticationService) {
	$rootScope.isHaveUnique = false;
	$rootScope.errors = {};
	$rootScope.selectedFiles = [];
	$rootScope.uploadRightAway = false;
	$rootScope.progress = [];
	$rootScope.uploadResult = [];
	$rootScope.showProgress = false;
	$rootScope.cobData = {};
	$rootScope.checkboxs = {};
	$rootScope.cbModel = {};
	$rootScope.isRequired = {};	
	statusNotify = "", result = false;
	$rootScope.formName = "";
	$rootScope.ClearInputFile = function(){
		img="";
		$('.fileinput').fileinput('clear');
		$rootScope.selectedFiles = [];
	}
	$rootScope.epochToDate = function(epoch){
		return moment.fromOADate(epoch).format('DD/MM/YYYY');
	}
	$rootScope.dateToEpoch = function(date){
		return moment(date).toOADate();
	}
	$rootScope.ConvertToDate = function(str){
		var date = str.substring(6, 10) + "/" + str.substring(3,5) + "/" + str.substring(0,2) 
		return date;
	}
	function SaveChange(endPointName, JsonVale, Callback){	
		if (statusNotify != "")
			statusNotify.hide();
		setTimeout(function(){
			statusNotify = ui.notify('Message', 'Saving...').effect('slide').sticky();
		}, 500);
		$("button").attr("disabled", "disabled");
		if(typeof(JsonVale.id)!="undefined"){			
			$rootScope.JsonVale = Restangular.copy(JsonVale);
			$rootScope.JsonVale.put().then(function (response) {
				statusNotify.hide();
				ui.notify('Message', 'Save change successful.').effect('slide').closable().hide(900);
				result = {};
				result["data"] = response;
				result["result"] = true;
				$("button").removeAttr("disabled");
				$rootScope.JsonVale = {};
				$rootScope.ClearInputFile();
				img = "";
				files = [];
				fileNames = [];
				fileName = [];
				imgFile = [];				
				$("ul.fileinput-preview").remove();	
				setTimeout(function(){
					statusNotify.hide();
				}, 800);
				return Callback(result);
			}, function (error) {
				statusNotify.hide();
				var err = JSON.parse(JSON.stringify(error));
				err = getErrorStatus(err.status).errors;
				statusNotify = ui.error(err.message, err.description).effect('slide').sticky();				
				result = false;
				$("button").removeAttr("disabled");								
				return Callback(result);
			});
		}
		else {
			Restangular.all(endPointName).post(JsonVale).then(function (response) {				
				statusNotify.hide();
				ui.notify('Message', 'Save change successful.').effect('slide').closable().hide(900);
				result = {};
				result["data"] = response;
				result["result"] = true;
				$("button").removeAttr("disabled");
				$rootScope.ClearInputFile();
				$("ul.fileinput-preview").remove();
				img = "";
				files = [];
				fileNames = [];
				fileName = [];
				imgFile = [];
				setTimeout(function(){
					statusNotify.hide();
				}, 800);
				return Callback(result);
			}, function (error) {
				statusNotify.hide();
				var err = JSON.parse(JSON.stringify(error));
				err = getErrorStatus(err.status).errors;
				statusNotify = ui.error(err.message, err.description).effect('slide').sticky();				
				result = false;
				$("button").removeAttr("disabled");
				return Callback(result);
			});
		}
	}
	
	function validateForm(endPointName, JsonVale, Callback){
		if (statusNotify != "")
			statusNotify.hide();
		var ErrorMessage = "";
		i = 0;
		for (var key in $rootScope.errors) {
			if($rootScope.errors[key]!=""){
				i++;
				ErrorMessage += i + ". " + $rootScope.errors[key] + "<br />";
				if(i==1)
					$("#" + key).focus();
			}				
		}
		if(ErrorMessage!="") {			
			if (statusNotify != ""){
				statusNotify.hide();
				setTimeout(function(){
					statusNotify = ui.error('Message', ErrorMessage).effect('slide').closable().hide(5000);
				}, 500);
			}
			return false;
		}
		if (typeof(files[0]) != "undefined") {			
			$rootScope.start(0, function(result){
				if(result["result"]){
					JsonVale["imageurl"] = result["imageurl"].toString();
					SaveChange(endPointName, JsonVale, Callback);		
				}
			});
		}
		else {
			//if(img != "")
				JsonVale["imageurl"] = img.toString();
			SaveChange(endPointName, JsonVale, Callback);
		}
	}
	
    return {
        Create: function (params) {
			$rootScope.checkboxs = {};
			$rootScope.cbModel = {};
			$rootScope.errors = {};
            var PType = {};
            PType.ngController = "";
            PType.formName = "frmEntry";
            PType.formControl = [];
            PType.submitButton = [];
            PType.resetButton = [];

            if (typeof params != 'undefined') {
                for (var i = 0; i < arguments.length; i++)
                    for (var key in arguments[i]) {
                        PType[key] = arguments[i][key];
                    }
            }
			
			$rootScope.formName = PType.formName;
			var SetDate = false;
			var SetDateTime = false;
			var SetTime = false;
			var SetInputTime = false;
			var SetCheckBoxList = false;
			var formControl = '<form class="form-horizontal" role="form" name="' + PType.formName + '" id = "' + PType.formName + '" novalidate autocomplete="off">';			
			for (var key in PType.formControl) {
                var ID = (typeof (PType.formControl[key].ID) == "undefined") ? "" : PType.formControl[key].ID;
                var Label = (typeof (PType.formControl[key].Label) == "undefined") ? "" : PType.formControl[key].Label;
                var ngModel = (typeof (PType.formControl[key].ngModel) == "undefined") ? "" : PType.formControl[key].ngModel;
				var Hidden = (typeof (PType.formControl[key].Hidden) == "undefined") ? false : true;
				var CheckBox = (typeof (PType.formControl[key].CheckBox) == "undefined") ? false : true;
				var Required = (typeof (PType.formControl[key].Required) == "undefined") ? "" : 'required="required"';
				var MinLength = (typeof (PType.formControl[key].MinLength) == "undefined") ? 'ng-minlength = "2"' : PType.formControl[key].MinLength;
				var Unique = (typeof (PType.formControl[key].Unique) == "undefined") ? "" : "ensure-unique=\"{{" + ngModel + "}}\" endpoint=\"" + PType.formControl[key].Unique + "\"";
				var IsEmail= (typeof (PType.formControl[key].IsEmail) == "undefined") ? false : true;
				var IsDate = (typeof (PType.formControl[key].IsDate) == "undefined") ? false : true;
				var IsDateTime = (typeof (PType.formControl[key].IsDateTime) == "undefined") ? false : true;
				var IsTime = (typeof (PType.formControl[key].IsTime) == "undefined") ? false : true;
				var IsInputTime = (typeof (PType.formControl[key].IsInputTime) == "undefined") ? false : true;
				var IsCbo = (typeof (PType.formControl[key].IsCbo) == "undefined") ? "" : PType.formControl[key].IsCbo;
				var IsNumber = (typeof (PType.formControl[key].IsNumber) == "undefined") ? false : true;
				var IsReadOnly = (typeof (PType.formControl[key].IsReadOnly) == "undefined") ? "" : " readonly=\"readonly\"";
				var IsCboCustom = (typeof (PType.formControl[key].IsCboCustom) == "undefined") ? "" : PType.formControl[key].IsCboCustom; 
				var IsFile = (typeof (PType.formControl[key].IsFile) == "undefined") ? false : true;
				var IsMutipleFile = (typeof (PType.formControl[key].IsMutipleFile) == "undefined") ? false : true;
				var IsPassword = (typeof (PType.formControl[key].IsPassword) == "undefined") ? false : true;
				var IsCPassword = (typeof (PType.formControl[key].IsCPassword) == "undefined") ? "" : "valid-password-c=\""+ PType.formControl[key].IsCPassword + "\"";
				var MultiLine = (typeof (PType.formControl[key].MultiLine) == "undefined") ? false : true;
				var IsCheckBoxList = (typeof (PType.formControl[key].IsCheckBoxList) == "undefined") ? "" : PType.formControl[key].IsCheckBoxList;
				var CheckBoxList = (typeof (PType.formControl[key].CheckBoxList) == "undefined") ? "" : PType.formControl[key].CheckBoxList;
				var DisplayText = (typeof (PType.formControl[key].DisplayText) == "undefined") ? false : true;
				var ValidateNumber = (typeof (PType.formControl[key].ValidateNumber) == "undefined") ? "" : "validatenumber";
				var OnClick = (typeof (PType.formControl[key].OnClick) == "undefined") ? "" : "ng-click=" + PType.formControl[key].OnClick;
				var VerifyUser = (typeof (PType.formControl[key].VerifyUser) == "undefined") ? "" : "VerifyUser=\"{{" + ngModel + "}}\" username=\"{{userName}}\" endpoint=\"" + PType.formControl[key].VerifyUser + "\"";
				var CLEditor = (typeof (PType.formControl[key].CLEditor) == "undefined") ? false : true;
				
				var Type = "text"
				if(IsEmail)
					Type = "email";
				else if(IsNumber){
					MinLength = 1;
					Type = "number";
				}
					
				if (Unique!=""){
					$rootScope.isHaveUnique = true;
					var FieldName = ngModel.split(".");
					if(FieldName.length>0)
						Unique += " fieldname=\"" + FieldName[1] + "\" fieldid=\"{{" + FieldName[0] + ".id}}\"";
					else
						Unique += " fieldname=\"" + FieldName + "\" fieldid=\"{{id}}\"";
				}
				if(Required!="")
					Required += " " + MinLength;
					
                formControl += '<div class="form-group"';
				if(Hidden){
					formControl += ' style="display:none"';
				}
				formControl += '>';
				
				if(Label!=""){
                	formControl += '<label for="' + ID + '" class="col-xs-3 control-label">' + Label + '</label>';
                	formControl += '<div class="col-xs-9">';
				}
				else
					formControl += '<div class="col-xs-12">';
					
				if(IsInputTime){
					SetInputTime = true;
					formControl += '<div class="input-append bootstrap-timepicker">';
					formControl += '<input type="time" class="form-control input-time" id="' + ID + '" name="' + ID + '" ng-model="' + ngModel + '" ' + Required + Unique + ' />';
					formControl += '</div>';
				}
				else if(IsTime){
					SetTime = true;
					formControl += '<input type="time" data-date-format="HH:ii P" class="form-control time-picker" id="' + ID + '" name="' + ID + '" ng-model="' + ngModel + '" ' + Required + Unique + ' />';
				}
				else if(IsDateTime){
					SetDateTime = true;
					formControl += '<input type="text" data-date-format="dd/MM/yyyy HH:ii P" class="form-control date-picker" id="' + ID + '" name="' + ID + '" ng-model="' + ngModel + '" ' + Required + Unique + ' />';
				}
				else if(IsDate){
					SetDate = true;
					formControl += '<input type="text" data-date-format="dd/mm/yyyy" class="form-control date-picker" id="' + ID + '" name="' + ID + '" ng-model="' + ngModel + '" ' + Required + Unique + ' />';
				}
				else if(IsCbo != "") {
					var Data = IsCbo.split(",");					
					var endPoint = Data[0].trim();
					var Value =  Data[1].trim();
					var displayText = Data[2].trim();
					var query = "";
					if(Data.length==4)
						query = Data[3].trim() + ',';
					if(endPoint!=""){
						formControl += '<select cbo="' + ID + '" endpoint="' + endPoint + '" value="' + Value + '" displaytext="' + displayText + '" query=\'' + query + '\' name="' + ID + '" id="' + ID + '" class="form-control" ng-model="' + ngModel +'" ' + Required + '>';
						formControl += '<option ng-repeat="' + ID + ' in cobData.' + ID + ' | orderBy:\'' + displayText + '\'" value="{{' + ID + '.' + Value + '}}">{{' + ID + '.' + displayText + '}}</option>';
						formControl += '</select>';						
					}
					else{
						formControl += '<select name="' + ID + '" id="' + ID + '" class="form-control" ng-model="' + ngModel +'" ' + Required + '>';
						formControl += '<option ng-repeat="' + ID + ' in cobData.' + ID + ' | orderBy:\'' + displayText + '\'" value="{{' + ID + '.' + Value + '}}">{{' + ID + '.' + displayText + '}}</option>';
						formControl += '</select>';
					}
				}
				else if(IsCboCustom != "") {
					formControl += '<select name="' + ID + '" id="' + ID + '" class="form-control" ng-model="' + ngModel +'" ' + Required + '>';
					for (var key in IsCboCustom) {
						var Value = (typeof (IsCboCustom[key].value) == "undefined") ? "" : IsCboCustom[key].value;
						var DisplayText = (typeof (IsCboCustom[key].displayText) == "undefined") ? "" : IsCboCustom[key].displayText;
						formControl += '<option value="' + Value + '">' + DisplayText + '</option>';						
					}
					formControl += '</select>';
				}
				else if(CheckBox)
				{
                	formControl += '<label class="switch switch-primary">';
					formControl += '<input type="checkbox" checked="" class="switch-input" id="' + ID + '" name="' + ID + '" ng-model="' + ngModel + '" ' + Required + ' />';
					formControl += '<span data-off="Off" data-on="On" class="switch-label"></span>';
					formControl += '<span class="switch-handle"></span></label>';
				}
				else if(IsFile)
				{					
					formControl += '<div class="fileinput fileinput-new" data-provides="fileinput">';
					formControl += '<div class="fileinput-new thumbnail" style="width: 200px; height: 150px;">';
					formControl += '<img ng-show="' + ngModel + '" src="http://192.168.1.53:8383/filecollection/{{' + ngModel + '}}">';
					formControl += '</div><div class="fileinput-preview fileinput-exists thumbnail" style="max-width: 200px; max-height: 150px;"></div>';
					formControl += '<div><span class="btn btn-default btn-file"><span class="fileinput-new">Select image</span><span class="fileinput-exists">Change</span><input type="file" ng-file-select="onFileSelect($files)" id="' + ID + '" name="' + ID + '" ng-model="' + ngModel + '" ' + Required  + ' accept="image/*" /></span>';
					formControl += '&nbsp;<a href="#" class="btn btn-default fileinput-exists" data-dismiss="fileinput">Remove</a></div></div>';
					formControl += '<div ng-show="showProgress" class="progress progressUploadAnimate progressBlue ui-progressbar ui-widget ui-widget-content ui-corner-all" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-valuenow="1"><div class="ui-progressbar-value ui-widget-header ui-corner-left" style="width: {{progress[0]}}%;"></div></div>';
				}
				else if(IsMutipleFile){
					formControl += '<div class="b-button js-fileapi-wrapper">';
					formControl += '<div class="b-button__text">Select photo</div>';
					formControl += '<input name="files" class="b-button__input" type="file" ng-file-select="onFileSelect($files)" multiple accept="image/*" />';
					formControl += '</div>';
				}
				else if(IsPassword)
					formControl += '<input type="password" class="form-control" id="' + ID + '" name="' + ID + '" ng-model="' + ngModel + '" ' + Required + Unique + IsReadOnly + VerifyUser + ' password-validate />';
				else if(IsCPassword)
					formControl += '<input type="password" class="form-control" id="' + ID + '" name="' + ID + '" ng-model="' + ngModel + '" ' + Required + Unique + IsReadOnly + ' '+ IsCPassword +' />';
				else if(MultiLine)
					formControl += '<textarea class="form-control" id="' + ID + '" name="' + ID + '" ng-model="' + ngModel + '" ' + Required + Unique + IsReadOnly + '></textarea>';				
				else if(IsCheckBoxList != "") {
					var Data = IsCheckBoxList.split(",");
					var endPoint = Data[0].trim();
					var CValue =  Data[1].trim();
					var CLabel =  Data[2].trim();
					var query = "";
					if(Data.length==4)
						query = Data[3].trim() + ',';
					formControl += '<span checkboxlist="' + ID + '" endpoint="' + endPoint + '" value="' + CValue + '" label="' + CLabel + '" query=\'' + query + '\'';
					if(Required != "") 
						formControl += ' ng-class="{checkboxGroupInvalid:isRequired[\'' + ID + '\']}"';
					formControl += '>';
					formControl += '<ul class="list-unstyled"><span ng-repeat="' + ID + ' in checkboxs.' + ID + ' | orderBy:\'' + CLabel + '\'">';
					if(Required != "")
						formControl += '<li><input type="checkbox" class="css-checkbox med" id="{{' + ID + '.' + CValue + '}}" ng-model="cbModel[' + ID + '.' + CValue + ']" ' + Required + ' ng-required="Required(\''+ ID + '\')" />';
					else
						formControl += '<li><input type="checkbox" class="css-checkbox med" id="{{' + ID + '.' + CValue + '}}" ng-model="cbModel[' + ID + '.' + CValue + ']" ' + Required + ' />';
					formControl += '<label class="css-label med elegant" for="{{' + ID + '.' + CValue + '}}">{{' + ID + '.' + CLabel + '}}</label></li>';
					formControl += '</span></ul></span>';
				}
				else if(CheckBoxList != ""){
					$rootScope.checkboxs[ID] = CheckBoxList;
					formControl += '<span checkboxlist="' + ID + '" value="Value"';
					if(Required != "")
						formControl += ' ng-class="{checkboxGroupInvalid:isRequired[\'' + ID + '\']}"';
					formControl += '>';
					formControl += '<ul class="col-xs-11 list-inline" style="padding:0; margin:0"><div class="row">';
					for(i=0; i < CheckBoxList.length; i++){
						if(Required != "")
							formControl += '<li class="col-xs-3"><input type="checkbox" class="css-checkbox med" value="' + CheckBoxList[i].Value + '" id="' + CheckBoxList[i].Value + '" name="' + ID + '" ng-model="cbModel[\'' + CheckBoxList[i].Value + '\']" ' + Required + ' ng-required="Required(\''+ ID + '\')" ' + OnClick + ' />';
						else
							formControl += '<li class="col-xs-3"><input type="checkbox" class="css-checkbox med" value="' + CheckBoxList[i].Value + '" id="' + CheckBoxList[i].Value + '" name="' + ID + '" ng-model="cbModel[\'' + CheckBoxList[i].Value + '\']" ' + Required + ' ' + OnClick + ' />';
						formControl += '<label class="css-label med elegant" for="' + CheckBoxList[i].Value + '">' + CheckBoxList[i].Label + '</label></li>';
					}
					formControl += '</div></ul></span>';
				}
				else if(DisplayText)
					formControl += '<label class="control-label">{{' + ngModel + '}}</label>';
				else if(CLEditor)
					formControl += '<textarea cl-editor class="cleditor" id="'+ ID +'" rows="3" ng-model="' + ngModel + '"></textarea>';
					//formControl += '<textarea id="'+ ID +'" rows="3" ng-model="' + ngModel + '" ck-editor></textarea>';
					
				else
					formControl += '<input type="' + Type + '" class="form-control" id="' + ID + '" name="' + ID + '" ng-model="' + ngModel + '" ' + Required + Unique + IsReadOnly + ' ' + ValidateNumber + ' />'; // ng-change="' + PType.formName + '.' + ID + '.$pristine;"
				
				if(Required!="") {					
					if(IsPassword){
						formControl += '<span ng-if="' + PType.formName + '.' + ID + '.$error.required" ng-init="errors.' + ID + '=\'Password must be at last 6 characters long.\'" ></span>';
						formControl += '<div ng-show="' + PType.formName + '.' + ID + '.$error.required && ' + PType.formName + '.' + ID + '.$invalid && !' + PType.formName + '.' + ID + '.$pristine">Password must be at last 6 characters long.</div>';
					}
					else if(IsCPassword){
						formControl += '<span ng-if="' + PType.formName + '.' + ID + '.$error.required" ng-init="errors.' + ID + '=\'Confirm password is required.\'" ></span>';
						formControl += '<span ng-if="!' + PType.formName + '.' + ID + '.$error.required && ' + PType.formName + '.' + ID + '.$error.passwordVerify" ng-init="errors.' + ID + '=\'Passwords do not match.\'" ></span>';
						formControl += '<div ng-show="' + PType.formName + '.' + ID + '.$error.required && ' + PType.formName + '.' + ID + '.$invalid && !' + PType.formName + '.' + ID + '.$pristine">Confirm password is required.</div>';
						formControl += '<div ng-show="!' + PType.formName + '.' + ID + '.$error.required && ' + PType.formName + '.' + ID + '.$error.passwordVerify">Passwords do not match.</div>';
					}	
					else if(IsCheckBoxList!=""){
						formControl += '<span ng-if="isRequired[\'' + ID + '\']" ng-init="errors.' + ID + '=\'' + Label.replace(":", "") + ' is required.\'"></span>';
						formControl += '<span ng-if="!isRequired[\'' + ID + '\']" ng-init="errors.' + ID + '=\'\'"></span>';						
						formControl += '<div ng-show="isRequired[\'' + ID + '\']">' + Label.replace(":", "") + ' is required.</div>';
					}
					else if(CheckBoxList!=""){
						formControl += '<span ng-if="isRequired[\'' + ID + '\']" ng-init="errors.' + ID + '=\'' + Label.replace(":", "") + ' is required.\'"></span>';
						formControl += '<span ng-if="!isRequired[\'' + ID + '\']" ng-init="errors.' + ID + '=\'\'"></span>';						
						formControl += '<div ng-show="isRequired[\'' + ID + '\']">' + Label.replace(":", "") + ' is required.</div>';
					}
					else{
						formControl += '<span ng-if="' + PType.formName + '.' + ID + '.$error.required" ng-init="errors.' + ID + '=\'' + Label.replace(":", "") + ' is required.\'" ></span>';
						formControl += '<div ng-show="' + PType.formName + '.' + ID + '.$error.required && ' + PType.formName + '.' + ID + '.$invalid && !' + PType.formName + '.' + ID + '.$pristine">' + Label.replace(":", "") + ' is required.</div>';
					}
					formControl += '<div ng-show="' + PType.formName + '.' + ID + '.$error.minlength">' + Label.replace(":", "") + ' is too short.</div>';
					formControl += '<span ng-if="' + PType.formName + '.' + ID + '.$error.minlength" ng-init="errors.' + ID + '=\'' + Label.replace(":", "") + ' is too short.\'" ></span>';					
				}
				
				if(Unique!="")
				{
					formControl += '<div ng-show="!' + PType.formName + '.' + ID + '.$error.required && !' + PType.formName + '.' + ID + '.$error.minlength && ' + PType.formName + '.' + ID + '.$error.unique">That ' + Label.replace(":", "") + ' is taken, please try another.</div>';
					formControl += '<span ng-if="' + PType.formName + '.' + ID + '.$error.unique" ng-init="errors.' + ID + '=\'That ' + Label.replace(":", "") + ' is taken, please try another.\'" ></span>';
				}
				if(VerifyUser!="")
				{
					formControl += '<div ng-show="!' + PType.formName + '.' + ID + '.$error.required && !' + PType.formName + '.' + ID + '.$error.minlength && ' + PType.formName + '.' + ID + '.$error.unique">Invalid old password, please try another.</div>';
					formControl += '<span ng-if="' + PType.formName + '.' + ID + '.$error.unique" ng-init="errors.' + ID + '=\'Invalid old password, please try another.\'" ></span>';
				}
				if(ValidateNumber!="")
				{
					formControl += '<div ng-show="!' + PType.formName + '.' + ID + '.$error.required && !' + PType.formName + '.' + ID + '.$error.minlength && ' + PType.formName + '.' + ID + '.$error.vnumber">That ' + Label.replace(":", "") + ' is not valid number.</div>';
					formControl += '<span ng-if="!' + PType.formName + '.' + ID + '.$error.required && !' + PType.formName + '.' + ID + '.$error.minlength && ' + PType.formName + '.' + ID + '.$error.vnumber" ng-init="errors.' + ID + '=\'That ' + Label.replace(":", "") + ' is not valid number.\'" ></span>';
				}
				if(IsNumber){
					formControl += '<span ng-if="' + PType.formName + '.' + ID + '.$error.number" ng-init="errors.' + ID + '=\'Not valid number.\'" ></span>';
					formControl += '<div ng-show="' + PType.formName + '.' + ID + '.$error.number">Not valid number.</div>';
				}
				if(IsEmail)
					formControl += '<div ng-show="' + PType.formName + '.' + ID + '.$error.email && ' + PType.formName + '.' + ID + '.$dirty">That is not a valid email. Please input a valid email.</div>';				
				formControl += '<span ng-if="' + PType.formName + '.' + ID + '.$valid" ng-init="errors.' + ID + '=\'\'" ></span>';
				formControl += '</div></div>';
            };
			
            var SubmitButton = "";
			var DrawButton = false;
			SubmitButton += '<div class="form-group">';
			SubmitButton += '<label class="col-xs-3 control-label"></label>';
			SubmitButton += '<div class="col-xs-9" align="right">';
			for (var key in PType.resetButton) {
				DrawButton = true;
                var ID = (typeof (PType.resetButton[key].ID) == "undefined") ? "btnReset" : PType.resetButton[key].ID;
                var ngClick = (typeof (PType.resetButton[key].ngClick) == "undefined") ? "" : PType.resetButton[key].ngClick;
                var Text = (typeof (PType.resetButton[key].Text) == "undefined") ? "Reset" : PType.resetButton[key].Text;                
				SubmitButton += '<button type="button" class="btn btn-default" id="' + ID + '" name="' + ID + '" ng-click="ClearInputFile(); ' + PType.formName + '.$setPristine();' + ngClick + '">' + Text + "</button>&nbsp;";
            };
			
			for (var key in PType.submitButton) {
				DrawButton = true;
                var ID = (typeof (PType.submitButton[key].ID) == "undefined") ? "btnSubmit" : PType.submitButton[key].ID;
                var ngClick = (typeof (PType.submitButton[key].ngClick) == "undefined") ? "" : PType.submitButton[key].ngClick;
                var Label = (typeof (PType.submitButton[key].Label) == "undefined") ? "Submit" : PType.submitButton[key].Label;
                SubmitButton += '<button type="submit" class="btn btn-primary" id="' + ID + '" name="' + ID + '" ng-click="' + ngClick + '">' + Label + "</button>";
            };			

            if (DrawButton)
                SubmitButton += '</div></div>';
			formControl += SubmitButton;			
			formControl += '<div ng-if="' + PType.formName + '.$valid">{{' + PType.formName + '.$setPristine()}}</div>';
            formControl += '</form>';
			
			/*if(CLEditor){
				formControl += '<script>var editor = $(".cleditor").cleditor()[0];';
				formControl += 'var sResult = editor.$area.val();';
				formControl += '  console.log(sResult);';
				formControl += '</script>';
			}*/
				
			if(SetInputTime)
			{
				formControl += '<script>';
				formControl += '$(".input-time").timepicker({showMeridian:false, minuteStep:1, showInputs: false});';
				formControl += '</script>';
			}
			if(SetDateTime){
				formControl += '<script>';
				formControl += '$(".date-picker").datetimepicker({';
				formControl += 'showMeridian: true,	autoclose: true, todayBtn: true});';
				formControl += '</script>';
			}
			if(SetDate){
				formControl += '<script>';
				formControl += '$(".date-picker").datetimepicker({';
				formControl += 'autoclose: true, todayBtn: true, minView:2});';
				formControl += '</script>';
			}			
			if(SetTime){
				formControl += '<script>';
				formControl += '$(".time-picker").datetimepicker({';
				formControl += 'startView: 1, showMeridian: true, autoclose: true})';
				formControl += '.on("show", function(){ $("th.switch").css("visibility", "hidden"); $("th.switch").html(""); })';
				formControl += '</script>';
			}
            return formControl;
        },
		SaveChange:function(endPointName, JsonVale, Callback){			
			if($rootScope.isHaveUnique) {
				if (statusNotify != "")
					statusNotify.hide();
				statusNotify = ui.notify('Message', 'Checking...').effect('slide').sticky();
				$rootScope.validate(function(result){
					if(result)
					{
						statusNotify.hide();
						validateForm(endPointName, JsonVale, Callback);
					}
				});
			}
			else
				validateForm(endPointName, JsonVale, Callback);
			
						
		},
		LoadHtmlData: function (endPointName, Callback, ignoreLoadingBar = false) {
			Restangular.all(endPointName)
                .withHttpConfig({ ignoreLoadingBar: ignoreLoadingBar })
                .getList()
                .then(
                    function (response) {
                        Swal.close();
                        return Callback($sce.trustAsHtml(response));
                    },
                    function (error) {
                        Swal.close();
                        DialogSweetAlert(
                            "error",
                            error.status + " " + error.statusText,
                        );
                        return Callback(error.status + " " + error.statusText);
                    },
                );
			//var TokenInfo = getStorage("TokenInfo");
			//$.ajax({
			//	url: endPointName,
			//	headers: { "Authorization": "Bearer " + JSON.parse(TokenInfo).accessToken },
			//	dataType: "html",
			//	async: false,
			//	cache: false,
			//	type: 'GET',
			//	beforeSend: function (xhr) {
			//		xhr.setRequestHeader('x-custom-header', JSON.stringify(headerPara));
			//	},
			//	complete: function (err, xhr) {
			//		//$(".popover").popover('hide');
			//		//$(".popover").hide();
   //             },
			//	success: function (response) {
			//		return Callback($sce.trustAsHtml(response));
			//	},
			//	error: function (response, callStatus, errorThrown) {
			//		DialogSweetAlert("error", response.status + " " + response.statusText);
			//		return Callback(response.status + " " + response.statusText);
			//	}
			//});
		},
		LoadData: function (endPointName, Callback) {
			//var url = appSetting.apiBaseUrl + endPointName;
			//var deferred = $q.defer();
			//$http.get(url).then(function () {
			//	deferred.resolve(null);
			//}, function (error) {
			//	deferred.reject(error);
			//});
			//return deferred.promise;
			//DialogSweetAlert("load");			
			//$("button").attr("disabled", "disabled");
			Restangular.all(endPointName).getList().then(function (data) {
				//result = true;
				//$("button").removeAttr("disabled");
				//DialogSweetAlert("loadsuccess");	
				Swal.close();
				return Callback(data);
			}, function (error) {
					//console.log(error);
					DialogSweetAlert("error");
					
                //var err = JSON.parse(JSON.stringify(error));
                //err = getErrorStatus(err.status).errors;
                //statusNotify = ui.error(err.message, err.description).effect('slide').sticky();				
				//result = false;
				//$("button").removeAttr("disabled");
				return Callback(result);
            });
		},
		LoadDataView: function (endPointName, Callback) {
			//DialogSweetAlert("load");
			var url = endPointName;
			//var deferred = $q.defer();
			$http.get(url).then(function (response) {
				//console.log(response.data);
				//DialogSweetAlert("loadsuccess");
				//deferred.resolve(null);
				Swal.close();
				return Callback(response.data);
			}, function (error) {
				DialogSweetAlert("error");
				//deferred.reject(error);
				return Callback(error.data);
			});
			//return deferred.promise;			
		},
		GetData: function(endPointName, query, Callback){
			$rootScope.ClearInputFile();
			if (statusNotify != "")
            	statusNotify.hide();
			statusNotify = ui.notify('Message', 'Loading...').effect('slide').sticky();
			$("button").attr("disabled", "disabled");
			Restangular.all(endPointName).getList(query).then(function (data) {
				statusNotify.hide();
				result = true;
				$("button").removeAttr("disabled");
				return Callback(data);
			}, function (error) {
                statusNotify.hide();
                var err = JSON.parse(JSON.stringify(error));
                err = getErrorStatus(err.status).errors;
                statusNotify = ui.error(err.message, err.description).effect('slide').sticky();				
				result = false;
				$("button").removeAttr("disabled");
				return Callback(result);
            });
		},
		Delete: function(endPointName, id, Callback){
			if (statusNotify != "")
            	statusNotify.hide();
			statusNotify = ui.notify('Message', 'Deleting...').effect('slide').sticky();
			$("button").attr("disabled", "disabled");
			Restangular.one(endPointName, id).remove().then(function(){
				statusNotify.hide();
				result = true;
				$("button").removeAttr("disabled");
				$rootScope.ClearInputFile();
				return Callback(true);
			}, function (error) {
                statusNotify.hide();
                var err = JSON.parse(JSON.stringify(error));
                err = getErrorStatus(err.status).errors;
                statusNotify = ui.error(err.message, err.description).effect('slide').sticky();				
				result = false;
				$("button").removeAttr("disabled");
				return Callback(false);
            });
		},
		Update: function(JsonVale, Callback){
			if (statusNotify != "")
            	statusNotify.hide();
			statusNotify = ui.notify('Message', 'Updating...').effect('slide').sticky();
			$("button").attr("disabled", "disabled");
			$rootScope.JsonVale = Restangular.copy(JsonVale);
			$rootScope.JsonVale.put().then(function (response) {				
				statusNotify.hide();
				ui.notify('Message', 'Save change successful.').effect('slide').closable().hide(900);				
				result = true;
				$("button").removeAttr("disabled");
				$rootScope.JsonVale = {};
				$rootScope.ClearInputFile();
				return Callback(result);
			}, function (error) {
                statusNotify.hide();
                var err = JSON.parse(JSON.stringify(error));
                err = getErrorStatus(err.status).errors;
                statusNotify = ui.error(err.message, err.description).effect('slide').sticky();				
				result = false;
				$("button").removeAttr("disabled");
				return Callback(false);
            });
		}
	}
});

appForm.directive('dynamic', function ($compile) {	
    return {
        restrict: 'A',
        replace: true,
		controller: function($rootScope, $scope, $element, $upload){
			$scope.onFileSelect = function($files) {
				$rootScope.selectedFiles = $files;				
				files = FileAPI.getFiles($files);
				var Divimg = '';
				$("ul.fileinput-preview").remove();
				$("div.b-button").parent().append('<ul style="margin-top:20px" class="list-unstyled list-inline fileinput-preview"></ul>');
				$("div.b-button").parent().append(Divimg);
				var i = 0, j = 0;
				fileNames = [];
				FileAPI.each(files, function (file){					
					if( /^image/.test(file.type) ){						
						FileAPI.Image(file).resize(1024, 768, 'max').get(function (err, img){
							if( !err ){
								fileName[j] = file.name;
								imgFile[j] = img.toDataURL();
								j++;
							}
						});					
						FileAPI.Image(file).preview(130).rotate('auto').get(function (err, img){
							if( !err ){							
								$("ul.fileinput-preview").append('<li><div class="preview-img"></div>&nbsp;<button onClick="removeFile($(this).parent().index()); $(this).parent().remove();" class="btn btn-default">Remove</button><div>&nbsp;</div></li>');
								$("div.preview-img").eq(i).append('<div class="thumbnail" align="right"><div class="editImg"><a href="javascript:;" name="editImg" onClick="editImg(this);"><i class="fa fa-edit"></i></a></div><img id="croppic' + i + '" src="' + img.toDataURL() + '" /></div>');
								i++;
							}
						});
					}					
				});
			};			
			$rootScope.start = function(indexs, Callback) {
				$("button").attr("disabled", "disabled");
				statusNotify = typeof(statusNotify) == "undefined" ? "" : statusNotify;
				if (statusNotify != "")
					statusNotify.hide();
				var result = false;
				statusNotify = ui.notify('Message', 'Uploading...').effect('slide').sticky();
				$rootScope.showProgress = true;
				index = typeof(indexs) == "undefined" ? 0 : indexs;
				$rootScope.progress[index] = 0;
				var formData = new FormData();
				$.each(imgFile, function(key, value)
				{
					var blob = dataURItoBlob(value);
					formData.append("file[]", blob, fileName[key]);
				});				
				$.ajax({
					url: urlBase + "/Uploadfile",
					type: 'POST',
					data: formData,
					cache: false,
					dataType: 'html',
					processData: false, // Don't process the files
					contentType: false, // Set content type to false as jQuery will tell the server its a query string request
					success: function(data, textStatus, jqXHR)
					{
						$("button").removeAttr("disabled");
						statusNotify.hide();
						statusNotify = ui.notify('Message', 'Upload successful.').effect('slide').closable().hide(900);
						var result = {};
						result["result"] = true;
						if(img != ""){
							img.push(data);
							result["imageurl"] = img;
						}
						else
							result["imageurl"] = data;
						return Callback(result);
					},
					error: function(jqXHR, textStatus, errorThrown){$("button").removeAttr("disabled");}
				});
				
				//var file = FileAPI.getFiles($rootScope.selectedFiles[index]);
				//FileAPI.upload({
//					  //url: 'http://localhost:8989/lemartinmode_website/LemartinmodeWebsite/api/Uploadfile'
//					  url: 'http://localhost:89/LemartinmodeWebsite/LemartinmodeWebsite/api/Uploadfile'
//					, files: { file: formData }
//					, imageTransform: {
//						//type: 'image/png'
//						'huge': { maxWidth: 1920, maxHeight: 1920 }
//						, 'medium': { maxWidth: 800, maxHeight: 800 }
//						, 'small': { maxWidth: 300, maxHeight: 397 }
//						// maxWidth: 1920
//						 //, maxHeight: 600
//						//, width: 300
//						//, height: 0
//						//, quality: 0.80
//					}
//					, upload: function (){
//						//document.getElementById('uploading').innerHTML = '(uploading&hellip;)';
//					}
//					, progress: function (evt){
//						$rootScope.progress[index] = Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
//					},
//					filecomplete: function (err/**String*/, xhr/**Object*/, file/**Object/, options/**Object*/){
//						if( !err ){
//							// File successfully uploaded
//							var result = xhr.responseText;
//							fileNames.push(result);
//						}
//					}
//					, complete: function (err, xhr){
//						if( !err ){
//							//var res = xhr.responseText;
//							statusNotify.hide();
//							statusNotify = ui.notify('Message', 'Upload successful.').effect('slide').closable().hide(900);
//							var result = {};
//							result["result"] = true;
//							if(img != "")
//								fileNames.push(img);
//							result["imageurl"] = fileNames; //=res;
//							return Callback(result);
//						}
//						else
//							Callback(false);
//					}, error: function()
//					{
//						statusNotify.hide();
//						statusNotify = ui.error('Message', 'Upload failed.').effect('slide').closable().hide(5000);
//						return Callback(result);
//					}
//				});
				/*$upload.upload({
					url: 'http://192.168.1.53:8383/filecollection',
					file: file,
					method: 'POST',
				}).progress(function(evt) {
					$rootScope.progress[index] = Math.min(100, parseInt(100.0 * evt.loaded / evt.total));
				}).success(function(data) {
					statusNotify.hide();
					statusNotify = ui.notify('Message', 'Upload successful.').effect('slide').closable().hide(900);
					var result = {};
					result["result"] = true;
					result["imageurl"] = data;
					return Callback(result);
				}).error(function()
				{
					statusNotify.hide();
					statusNotify = ui.error('Message', 'Upload failed.').effect('slide').closable().hide(5000);
					return Callback(result);
				});*/				
				$rootScope.showProgress = false;
			}
    	},
        link: function (scope, element, attrs) {
            scope.$watch(attrs.dynamic, function (html) {
				setTimeout(function(){
					element.html(html);
					$compile(element.contents())(scope);
				}, 10);
            });
        }
		
    };
});

appForm.directive('cbo', ['Restangular', function( Restangular) {	
  return {
    link: function($rootScope, scope, attrs) {
		if(attrs.endpoint!=""){
			var endPoint = attrs.endpoint;		
			Restangular.all(endPoint).getList().then(function (data) {
				$rootScope.cobData[attrs.cbo] = data;
			});
		}
    }
  }
}]);

appForm.directive('checkboxlist', ['Restangular', function( Restangular) {	
  return {
    link: function($rootScope, scope, attrs) {
		if(typeof(attrs.endpoint)!="undefined"){
			var endPoint = attrs.endpoint + '?{' + attrs.query + '"$fields":{"' + attrs.value + '":1, "' + attrs.label + '":1}}';
			Restangular.all(endPoint).getList().then(function (data) {
				$rootScope.checkboxs[attrs.checkboxlist] = data;
			});
		}
		$rootScope.Required = function(ID){
			$rootScope.isRequired[ID] = true;
			for(i = 0; i < $rootScope.checkboxs[ID].length; i++){
				for(var c in $rootScope.cbModel){
					if($rootScope.cbModel[c]){
						if($rootScope.checkboxs[ID][i][attrs.value] == c){
							$rootScope.isRequired[ID] = false;
							break;
							return false;
						}
						if(!$rootScope.isRequired[ID]){
							break;
							return false;
						}
					}
				}
			}
			return true;
		};
    }
  }
}]);

//focus directive
appForm.directive('setfocus', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {			
            scope.$watch(attr.setfocus, function (n, o) {
                if (n != 0 && n) {
                    element[0].focus();
                }
            });
        }
    };
});

//blur directive
appForm.directive('setblur', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            element.bind('blur', function () {
				scope.$eval(attr.setfocus + '= false');
            });
        }
    };
});
appForm.directive('clEditor', [function () {
    return {
        require: '?ngModel',
        link: function ($scope, elm, attr, ngModel) {			
			$("#" + attr.id).cleditor();
			var editor = $("#" + attr.id).cleditor()[0];
            editor.change(function(){
				var sResult = editor.$area.val();
				$scope.$apply(function () {
                    ngModel.$setViewValue(sResult);
                });
			});
			
			ngModel.$render = function () {
				if(ngModel.$modelValue){
					editor.$area.val(ngModel.$modelValue);
					setTimeout(function(){
						editor.updateFrame();
					}, 100);
				}
            };
			
			attr.$observe('ngModel', function(value){
				$scope.$watch(value,function(newValue){
				  if(typeof(newValue) === "undefined"){
					  setTimeout(function(){
					  	editor.clear().focus();
					  }, 100);
				  }
				});
			});			
        }
    };
}]);

appForm.directive('ckEditor', [function () {
    return {
        require: '?ngModel',
        link: function ($scope, elm, attr, ngModel) {
			
            var ck = CKEDITOR.replace(elm[0], {toolbar: 'Basic',  uiColor: '#9AB8F3'});
            ck.on('pasteState', function () {
                $scope.$apply(function () {
                    ngModel.$setViewValue(ck.getData());
                });
            });

            ngModel.$render = function (value) {
                ck.setData(ngModel.$modelValue);
            };
        }
    };
}]);

appForm.directive("sectionLoader", function () {
	const userId = localStorage.getItem("User_ID");
	
	return {
		restrict: "A",
		transclude: true,
		template: userId == 129 ? `
		<div class="loader-wrapper">
                <div ng-transclude></div>

                <div class="section-loader" ng-if="loading">
				   <video class="duck-loader" autoplay loop muted playsinline>
                        <source src="../Uploads/duck-loading.webm" type="video/webm" style="width: 180px;" />
                    </video>
                </div>
            </div>
		` : ` 
            <div class="loader-wrapper">
                <div ng-transclude></div>

                <div class="section-loader" ng-if="loading">
                    <div class="loader"></div>
					<div class="loading loading03">
						<span>L</span>
						<span>O</span>
						<span>A</span>
						<span>D</span>
						<span>I</span>
						<span>N</span>
						<span>G</span>
					</div>
                </div>
            </div>
        `,
		link: function (scope, element) {
			scope.loading = false;

			element.css("position", "relative");

			scope.$on("cfpLoadingBar:loading", function () {
				scope.loading = true;
			});

			scope.$on("cfpLoadingBar:loaded", function () {
				scope.loading = false;
			});
		}
	};
});


function initJcrop()
{
	$('#viewerImg').Jcrop({
		//setSelect:   [ 0, 0, 300, 397 ],
		//minSize: [ 300, 397 ],
		allowSelect:false,
		onChange: updatePreview,
		onSelect: updatePreview
	},function(){
		jcrop_api = this;
		jcrop_api.animateTo([0, 0, 300, 397]);
  	});
};
function updatePreview(c)
{				
	canvas.width = canvas.width;
	sourceX = c.x;
	sourceY = c.y;
	sourceWidth = c.w;
	sourceHeight = c.h;
	canvas.width = c.w;
	canvas.height = c.h;
	destWidth = c.w;
	destHeight = c.h;
	destX = c.x;
	destY = c.y;
	setTimeout(function(){
		context.drawImage(imageObj, sourceX, sourceY, sourceWidth, sourceHeight, 0, 0, sourceWidth, sourceHeight);
	}, 500);
}

function editImg(Parent){	
	fileIndex = $("a[name=editImg]").index(Parent);
	var id = 'croppic' + fileIndex;
	var tr = "";
	imageObj.src = imgFile[fileIndex];
	var tr='<div class="wrapper">';
	tr+='<img id="viewerImg" src="' + imgFile[fileIndex] +'" />';
	tr+='<canvas id="drawcanvas" style="display:none"></canvas>';
	tr+='</div>';	
	
	BootstrapDialog.show({
            title: 'Crop Photo',
			draggable: true,
			onshow: function(dialog){
				dialog.setMessage(tr);				
				setTimeout(function(){
					canvas = document.getElementById('drawcanvas');
					context = canvas.getContext('2d');					
					initJcrop();				
					setTimeout(function(){
						$(".jcrop-keymgr").remove();
					}, 200);
				}, 200);
			},
			buttons: [{
			label: 'Crop',
			cssClass: 'btn-success',
			action: function(dialogItself){
				imgFile[fileIndex] = canvas.toDataURL('image/png');
				$("#" + id).attr("src", imgFile[fileIndex]).css({"height":"130", "maxWidth":"300"});
				dialogItself.close();
			}
			},
			{
			label: 'Close',
			action: function(dialogItself){
				dialogItself.close();
			}
			}]
        });
}

function toSeconds(time_str) {
    // Extract hours, minutes and seconds
    var parts = time_str.split(':');
	var hour = parts[0] * 3600;
	var minute = parts[1] * 60;
	var seconds = typeof(parts[2])=="undefined" ? 0:parts[2];
    // compute  and return total seconds
    return hour + // an hour has 3600 seconds
    minute + // a minute has 60 seconds
    + seconds  // seconds
}

function dataURItoBlob(dataURI)
{
     // convert base64 to raw binary data held in a string
    // doesn't handle URLEncoded DataURIs
    var byteString;
    if (dataURI.split(',')[0].indexOf('base64') >= 0)
        byteString = atob(dataURI.split(',')[1]);
    else
        byteString = unescape(dataURI.split(',')[1]);
    // separate out the mime component
    var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];

    // write the bytes of the string to an ArrayBuffer
    var ab = new ArrayBuffer(byteString.length);
    var ia = new Uint8Array(ab);
    for (var i = 0; i < byteString.length; i++) {
        ia[i] = byteString.charCodeAt(i);
    }

    // write the ArrayBuffer to a blob, and you're done
    return new Blob([ab],{type: mimeString});
}