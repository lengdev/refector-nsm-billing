'use strict';
app.controller('SurveyCustomerController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Survey Customer";
    $scope.LoadData = function () {
        Form.LoadHtmlData("SurveyCustomer", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);