'use strict';
app.controller('AddSupportUserLoginController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Support User Login";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddSupportUserLogin", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);