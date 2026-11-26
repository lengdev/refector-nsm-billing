'use strict';
app.controller('StockOnHandReportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Stock On Hnad";
    $scope.LoadData = function () {
        Form.LoadHtmlData("StockOnHandReport", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);