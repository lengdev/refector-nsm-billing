'use strict';
app.controller('StockAlertController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Stock Alert";
    $scope.LoadData = function () {
        Form.LoadHtmlData("StockAlert", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);