'use strict';
app.controller('ReportGeneralJournalController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report General Journal";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ReportGeneralJournal", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);