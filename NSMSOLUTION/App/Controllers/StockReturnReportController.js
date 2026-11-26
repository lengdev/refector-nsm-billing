'use strict';
app.controller('StockReturnReportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Stock Return";
    $scope.LoadData = function () {
        Form.LoadHtmlData("StockReturnReport", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);