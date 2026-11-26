'use strict';
app.controller('AddCompanyController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Company";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddCompany", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('InvoiceTemplateController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Invoice Template";
    Form.LoadHtmlData("AddCompany/InvoiceTemplate", function (result) {
        $scope.HtmlData = result;
    });
}]);

app.controller('CompanyBranchesController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Company Branches";
    Form.LoadHtmlData("CompanyBranches", function (result) {
        $scope.HtmlData = result;
    });
}]);