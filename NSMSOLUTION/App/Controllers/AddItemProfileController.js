'use strict';
app.controller('AddItemProfileController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Item Profile";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddItemProfile", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);