'use strict';
app.controller('DepositController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Deposit";
    Form.LoadHtmlData("Deposit", function (result) {
        $scope.HtmlData = result;
    });
}]);

app.controller('FinanceReportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Finance Report";
    Form.LoadHtmlData("FinanceReport", function (result) {
        $scope.HtmlData = result;
    });
}]);