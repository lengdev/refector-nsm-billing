'use strict';
app.controller('ReportCustomerConnectionListController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Customer Connection List";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Report/CustomerConnectionList", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);