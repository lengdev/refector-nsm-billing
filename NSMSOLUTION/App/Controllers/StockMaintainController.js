'use strict';
app.controller('StockMaintainController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Stock Maintain";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Profile/LoadStockMaintain", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);