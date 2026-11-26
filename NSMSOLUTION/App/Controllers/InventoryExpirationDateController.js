'use strict';
app.controller('InventoryExpirationDateController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Inventory Expiry Date";
    $scope.LoadData = function () {
        Form.LoadHtmlData("InventoryExpirationDate", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);