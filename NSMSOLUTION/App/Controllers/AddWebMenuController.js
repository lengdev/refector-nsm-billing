'use strict';
app.controller('AddWebMenuController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Menu";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddWebMenu", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);