'use strict';
app.controller('AddItemController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Item";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddItem", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);