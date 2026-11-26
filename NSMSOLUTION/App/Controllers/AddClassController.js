'use strict';
app.controller('AddClassController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Class";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddClass", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);