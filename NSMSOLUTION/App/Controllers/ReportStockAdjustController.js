'use strict';
app.controller('ReportStockAdjustController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Stock Adjustment";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ReportStockAdjust", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);