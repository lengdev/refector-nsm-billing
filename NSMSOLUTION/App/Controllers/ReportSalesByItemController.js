'use strict';
app.controller('ReportSalesByItemController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Sales By Item";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/SalesByItem", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);