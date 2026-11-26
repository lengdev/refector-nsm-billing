'use strict';
app.controller('ReportInvoicesController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Invoices";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/Invoices", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);