'use strict';
app.controller('ReportSentToDMCController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Sent To DMC";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ReportSentToDMC", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);