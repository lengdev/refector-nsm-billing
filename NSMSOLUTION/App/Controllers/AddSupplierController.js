'use strict';
app.controller('AddSupplierController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Supplier";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddSupplier", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);