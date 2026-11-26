'use strict';
app.controller('ReportInventorySummaryController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Inventory Summary";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/InventorySummary", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);