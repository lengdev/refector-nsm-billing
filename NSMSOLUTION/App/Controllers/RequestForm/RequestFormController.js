'use strict';
app.controller('RequestFormController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Request Form";
    $scope.LoadData = function () {
        Form.LoadHtmlData("RequestForm", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('RequestCashAdvanceController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Request Cash Advance";
    $scope.LoadData = function () {
        Form.LoadHtmlData("RequestForm/RequestCashAdvance", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('RequestInventoryController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Request Inventory";
    $scope.LoadData = function () {
        Form.LoadHtmlData("RequestForm/RequestInventory", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);




