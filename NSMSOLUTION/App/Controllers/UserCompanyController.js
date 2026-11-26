'use strict';
app.controller('UserCompanyController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Company";
    $scope.LoadData = function () {
        Form.LoadHtmlData("UserCompany", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);