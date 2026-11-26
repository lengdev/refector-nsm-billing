'use strict';
app.controller('StockTransfersController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Stock Transfers";
    $scope.LoadData = function () {
        Form.LoadHtmlData("StockTransfers", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);