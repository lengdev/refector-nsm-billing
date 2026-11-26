'use strict';
app.controller('SaleOrderController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Sale Order";
    $scope.LoadData = function () {
        Form.LoadHtmlData("SaleOrder", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);