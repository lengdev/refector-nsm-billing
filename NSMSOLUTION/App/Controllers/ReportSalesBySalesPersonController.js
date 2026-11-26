'use strict';
app.controller('ReportSalesBySalesPersonController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Sales By Sales Person";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/SalesBySalesPerson", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);