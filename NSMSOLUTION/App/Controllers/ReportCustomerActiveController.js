'use strict';
app.controller('ReportCustomerActiveController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Customer Active ";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/ReportCustomerActive", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
