'use strict';
app.controller('AccountCategoryController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Account Categories";
    Form.LoadHtmlData("Account/AccountCategory", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('AccountTypesController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Account Types";
    Form.LoadHtmlData("Account/AccountTypes", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('OpeningBalanceController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Opening Balance";
    Form.LoadHtmlData("Account/OpeningBalance", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('GetHierarchyTrialBalanceController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Get Hierarchy Trial Balance";
    Form.LoadHtmlData("Account/HierarchyTrialBalance", function (result) {
        $scope.HtmlData = result;
    });
}]);

//Report
app.controller('GeneralLedgerController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "General Ledger";
    Form.LoadHtmlData("GeneralLedger", function (result) {
        $scope.HtmlData = result;
    });
}]);

app.controller("GeneralJournalController", [
    "$rootScope",
    "$scope",
    "Form",
    function ($rootScope, $scope, Form) {
        $rootScope.title = "General Journal";
        Form.LoadHtmlData("GeneralJournal", function (result) {
            $scope.HtmlData = result;
        });
    },
]);

app.controller("TrialBalanceController", [
    "$rootScope",
    "$scope",
    "Form",
    function ($rootScope, $scope, Form) {
        $rootScope.title = "Trial Balance";
        Form.LoadHtmlData("TrialBalance", function (result) {
            $scope.HtmlData = result;
        });
    },
]);
//Report
app.controller('CustomerAgingController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Aging";
    Form.LoadHtmlData("CustomerAging", function (result) {
        $scope.HtmlData = result;
    });
}]);

app.controller("ProfitAndLossController", [
    "$rootScope",
    "$scope",
    "Form",
    function ($rootScope, $scope, Form) {
        $rootScope.title = "Profit And Loss";
        Form.LoadHtmlData("ProfitAndLoss", function (result) {
            $scope.HtmlData = result;
        });
    },
]);

app.controller("ExpenseDetailController", [
    "$rootScope",
    "$scope",
    "Form",
    function ($rootScope, $scope, Form) {
        $rootScope.title = "Expense Detail";
        Form.LoadHtmlData("ExpenseDetail", function (result) {
            $scope.HtmlData = result;
        });
    },
]);

app.controller("ExpenseSummaryController", [
    "$rootScope",
    "$scope",
    "Form",
    function ($rootScope, $scope, Form) {
        $rootScope.title = "Expense Summary";
        Form.LoadHtmlData("ExpenseSummary", function (result) {
            $scope.HtmlData = result;
        });
    },
]);

app.controller("TransactionAccountController", [
    "$rootScope",
    "$scope",
    "Form",
    function ($rootScope, $scope, Form) {
        $rootScope.title = "Transaction Account";
        Form.LoadHtmlData("TransactionAccount", function (result) {
            $scope.HtmlData = result;
        });
    },
]);

app.controller("CashFlowReportController", [
    "$rootScope",
    "$scope",
    "Form",
    function ($rootScope, $scope, Form) {
        $rootScope.title = "Cash Flow Report";
        Form.LoadHtmlData("CashFlowReport", function (result) {
            $scope.HtmlData = result;
        });
    },
]);
app.controller("EnterBillController", [
    "$rootScope",
    "$scope",
    "Form",
    function ($rootScope, $scope, Form) {
        $rootScope.title = "Enter Bill";
        Form.LoadHtmlData("EnterBill", function (result) {
            $scope.HtmlData = result;
        });
    },
]);
app.controller("PayBillController", [
    "$rootScope",
    "$scope",
    "Form",
    function ($rootScope, $scope, Form) {
        $rootScope.title = "Pay Bill";
        Form.LoadHtmlData("PayBill", function (result) {
            $scope.HtmlData = result;
        });
    },
]);


