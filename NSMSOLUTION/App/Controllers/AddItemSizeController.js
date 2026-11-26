'use strict';
app.controller('AddItemSizeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Item Size";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ItemSize", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);