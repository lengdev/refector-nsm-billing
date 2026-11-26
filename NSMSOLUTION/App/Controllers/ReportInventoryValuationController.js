'use strict';
app.controller('ReportInventoryValuationController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Inventory Valuation";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/InventoryValuationSummary", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);