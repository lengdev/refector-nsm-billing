'use strict';
app.controller('ReportCustomerInvoiceController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Customer Invoice";
    $scope.LoadData = function () {
        var ID = (typeof $location.search().StatusID == "undefined" ? 0 : $location.search().StatusID);
        Form.LoadHtmlData("Report/CustomerInvoice?StatusID=" + ID, function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);