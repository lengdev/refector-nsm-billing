'use strict';
app.controller('ReportCustomerInfrastructureController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Customer Infrastructure";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ReportCustomerInfrastructure", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
