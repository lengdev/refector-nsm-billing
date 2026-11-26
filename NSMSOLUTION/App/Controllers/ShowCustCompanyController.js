'use strict';
app.controller('ShowCustCompanyController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Company";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ShowCustCompany", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);