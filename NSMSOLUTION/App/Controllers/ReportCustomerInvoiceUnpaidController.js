'use strict';
app.controller('ReportCustomerInvoiceUnpaidController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Customer Invoice Unpaid";
    $scope.LoadData = function () {
        var ID = (typeof $location.search().StatusID == "undefined" ? 0 : $location.search().StatusID);
        Form.LoadHtmlData("Report/CustomerInvoiceUnpaid?StatusID=" + ID, function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
