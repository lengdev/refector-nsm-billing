'use strict';
app.controller('CustomerInvoiceController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Invoice";
    $scope.LoadData = function () {
        let loc = window.location.href;
        let Type = getQueryStringValue(loc, 'Type');
        let StatusID = getQueryStringValue(loc, 'StatusID');
        let url = "Customer/Invoice";
        if (Type == "OfficialInvoice")
            url += "/?Type=" + Type;
        if (StatusID != "")
            url += "/?Type=" + Type + "&StatusID=" + StatusID;
        Form.LoadHtmlData(url, function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);