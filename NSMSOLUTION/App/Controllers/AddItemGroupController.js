'use strict';
app.controller('AddItemGroupController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Item Group";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddItemGroup", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);