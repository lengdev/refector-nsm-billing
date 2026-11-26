'use strict';
app.controller('StockOutController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Stock OUT";
    $scope.LoadData = function () {
        Form.LoadHtmlData("StockOut", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);