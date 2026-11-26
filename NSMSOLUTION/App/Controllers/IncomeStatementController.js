'use strict';
app.controller('IncomeStatementController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Customer Attachment";
    $scope.LoadData = function () {
        Form.LoadHtmlData("IncomeStatement", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);