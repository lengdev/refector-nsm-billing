'use strict';
app.controller('ReportBalanceSheetController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Balance Sheet";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ReportBalanceSheet", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);