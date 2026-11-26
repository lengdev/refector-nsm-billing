'use strict';
app.controller('CustomerReportsController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Reports";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
app.controller('CustomerCommissionController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Reports";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/CustomerCommission", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
app.controller('InvoiceToDMCTAXController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Invoice To DMC TAX";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/InvoiceToDMCTAX", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
app.controller('CustomerBlockController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Block Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/CustomerBlock", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
app.controller('CustomerStatusController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Status Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/CustomerStatus", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
app.controller('CustomersController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Status Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/Customer", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('BandwidthController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Bandwidth Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/Bandwidth", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

// app.controller('RouterController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
//     $rootScope.title = "Router Report";
//     $scope.LoadData = function () {
//         Form.LoadHtmlData("CustomerReports/Router", function (result) {
//             $scope.HtmlData = result;
//         });
//     };
//     $scope.LoadData();
// }]);

app.controller('ExportedInvoiceToDMCTAXController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Router Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/ExportedInvoiceToDMCTAX", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('CustInfoWillSendToDMCController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Router Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/CustInfoWillSendToDMC", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
app.controller('CustomerDeviceController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Device Report";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/CustomerDevice", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
app.controller('CustomerConnectionLogController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Connection Log";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerProfile/CustomerConnectionLog", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
app.controller('CustomerPrefixByBlockController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Prefix By Block";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/CustomerPrefixByBlock", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('ReportCustomerByBlockController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Report Customer By Block";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerReports/ReportCustomerByBlock", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);