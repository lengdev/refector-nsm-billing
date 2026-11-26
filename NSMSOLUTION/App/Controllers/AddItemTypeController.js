'use strict';
app.controller('AddItemTypeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Item Type";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddItemType", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);