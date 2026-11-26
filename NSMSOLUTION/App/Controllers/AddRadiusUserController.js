'use strict';
app.controller('AddRadiusUserController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Radius User";
    $scope.LoadData = function () {
        Form.LoadHtmlData("RADIUS/AddRadiusUser", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);