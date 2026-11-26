'use strict';
app.controller('AddExpenseTypeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Create Expense Type";
    $rootScope.header = "Create Expense Type";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddExpenseType", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);