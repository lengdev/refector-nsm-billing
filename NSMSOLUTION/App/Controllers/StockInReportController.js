'use strict';
app.controller('StockInReportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Stock In";
    $scope.LoadData = function () {
        Form.LoadHtmlData("StockInReport", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);