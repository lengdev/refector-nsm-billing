'use strict';
app.controller('AddItemBrandController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Item Brand";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ItemBrand", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);