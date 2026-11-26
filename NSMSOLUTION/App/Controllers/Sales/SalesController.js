'use strict';
app.controller('SalesController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Profile";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/CustomerProfileInfo", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('SalesOrderController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Sales Order";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/SalesOrder", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('CustomerProfilesController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Customer Profile";
    let ID = $location.search().ID;
    let ISSO = typeof $location.search().ISSO == "undefined" ? 0 : $location.search().ISSO;
    let InvoiceNo = typeof $location.search().InvoiceNo == "undefined" ? "" : $location.search().InvoiceNo;
    let ISPaidInvoice = typeof $location.search().ISPaidInvoice == "undefined" ? false : $location.search().ISPaidInvoice;
    Form.LoadHtmlData("CustomerProfiles?ID=" + ID + "&ISSO=" + ISSO + "&ISPaidInvoice=" + ISPaidInvoice + "&InvoiceNo=" + InvoiceNo, function (result) {
        $scope.HtmlData = result;
    });
}]);

app.controller('InvoicesController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Invoices";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/Invoices", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('QuotationController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Quotation";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/Quotations", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('DeliveryNoteController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Delivery Note";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/DeliveryNote", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('ConvertInvoicesController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Convert Invoices";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/ConvertInvoices", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('CustInfoController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Customer Info";
    $scope.LoadData = function () {
        var ID = $location.search().ID;
        Form.LoadHtmlData("Sales?ID=" + ID, function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('InvoicesReportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Invoices Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/InvoicesReport", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('SOReportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "SO Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/SOReport", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);


app.controller('CMInvoicesReportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "CM Invoices Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/CMInvoicesReport", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('InvoicesDetailReportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Invoices Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/InvoicesDetailReport", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('SODetailReportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "SO Detail Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/SODetailReport", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

// add for Quote Detail Report
app.controller('QuoteDetailReportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Quote Detail Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/QuoteDetailReport", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
/////////

app.controller('CMInvoicesDetailReportController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "CM Invoices Detail Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Sales/CMInvoicesDetailReport", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);