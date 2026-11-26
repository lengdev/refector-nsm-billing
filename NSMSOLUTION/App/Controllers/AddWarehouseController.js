'use strict';
app.controller('AddWarehouseController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Warehouse";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddWarehouse", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);