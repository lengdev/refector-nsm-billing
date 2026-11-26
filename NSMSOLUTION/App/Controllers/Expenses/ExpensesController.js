'use strict';
app.controller('ExpensesController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Expenses";
    Form.LoadHtmlData("Expenses", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('ExpenseTransactionsController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Expenses";
    Form.LoadHtmlData("ExpenseTransactions/LoadExpenses", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('TaxsController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Tax";
    Form.LoadHtmlData("Taxs", function (result) {
        $scope.HtmlData = result;
    });
}]);

app.controller('AdvanceExpenses', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Advance Expenses";
    Form.LoadHtmlData("Expenses/LoadAdvanceExpenses", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('VendorCenterController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Vendor Center";
    Form.LoadHtmlData("VendorCenter", function (result) {
        $scope.HtmlData = result;
    });
}]);

app.controller("ExpenseDetailReportController", [
    "$rootScope",
    "$scope",
    "Form",
    function ($rootScope, $scope, Form) {
        $rootScope.title = "Expense Detail Report";
        Form.LoadHtmlData("ExpenseDetailReport", function (result) {
            $scope.HtmlData = result;
        });
    },
]);

app.controller("ExpenseSummaryReportController", [
    "$rootScope",
    "$scope",
    "Form",
    function ($rootScope, $scope, Form) {
        $rootScope.title = "Expense Summary Report";
        Form.LoadHtmlData("ExpenseSummaryReport", function (result) {
            $scope.HtmlData = result;
        });
    },
]);