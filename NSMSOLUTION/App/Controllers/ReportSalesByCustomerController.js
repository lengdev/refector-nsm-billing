'use strict';
app.controller('ReportSalesByCustomerController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Sales By Customer";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/SalesByCustomer", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);