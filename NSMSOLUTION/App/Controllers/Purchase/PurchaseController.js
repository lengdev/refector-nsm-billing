'use strict';
app.controller('PurchaseRequestController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Purchase Request";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Purchase/PurchaseRequest", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
app.controller('AuthorizePurchaseRequestController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Authorize Purchase Request";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Purchase/AuthorizePurchaseRequest", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
app.controller('AuthorizePurchaseOrderController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Authorize Purchase Order";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Purchase/AuthorizePurchaseOrder", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('EnterBillController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Enter Bill";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Purchase/EnterBill", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('GoodsReceivedController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Goods Received";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Purchase/GoodsReceived", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

