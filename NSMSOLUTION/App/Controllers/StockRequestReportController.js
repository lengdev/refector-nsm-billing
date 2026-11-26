'use strict';
app.controller('StockRequestReportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Stock Request";
    $scope.LoadData = function () {
        Form.LoadHtmlData("StockRequestReport", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);