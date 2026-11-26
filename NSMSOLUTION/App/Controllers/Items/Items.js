"use strict";
app.controller("BarcodeController", [
    "$rootScope",
    "$scope",
    "Form",
    function ($rootScope, $scope, Form) {
        $rootScope.title = "Item Barcode";
        $scope.LoadData = function () {
            Form.LoadHtmlData("Items/Barcode", function (result) {
                $scope.HtmlData = result;
            });
        };
        $scope.LoadData();
    },
]);

app.controller("ItemProfileController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "Item Profile";
        $scope.LoadData = function () {
            let ID = $location.search().ID;
            Form.LoadHtmlData("Items/Profile?ID=" + ID, function (result) {
                $scope.HtmlData = result;
            });
        };
        $scope.LoadData();
    },
]);
app.controller("ManufacturerController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "Manufacturer";
        Form.LoadHtmlData("Manufacturer", function (result) {
            $scope.HtmlData = result;
        });
    },
]);
app.controller("ItemGroupController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "Item Group";
        Form.LoadHtmlData("Items/ItemGroup", function (result) {
            $scope.HtmlData = result;
        });
    },
]);
app.controller("InventoryRequestController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "Inventory Request";
        Form.LoadHtmlData("Items/InventoryRequest", function (result) {
            $scope.HtmlData = result;
        });
    },
]);
app.controller("RequestApprovalItemOutController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "Request Approval Inventory Out";
        Form.LoadHtmlData("Items/RequestApprovalItemOut", function (result) {
            $scope.HtmlData = result;
        });
    },
]);
app.controller("PrintItemBarcodeController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "Print Item Barcode";
        Form.LoadHtmlData("Items/PrintItemBarcode", function (result) {
            $scope.HtmlData = result;
        });
    },
]);
app.controller("SearchItemByBarcodeController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "Search Item By Barcode";
        Form.LoadHtmlData("Items/SearchItemByBarcode", function (result) {
            $scope.HtmlData = result;
        });
    },
]);

app.controller("ItemSerialController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "Item Serial";
        Form.LoadHtmlData("Items/ItemSerial", function (result) {
            $scope.HtmlData = result;
        });
    },
]);

app.controller("RequestReturnController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "Request Return";
        Form.LoadHtmlData("Items/RequestReturn", function (result) {
            $scope.HtmlData = result;
        });
    },
]);

app.controller("StockReturnController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "Stock Return";
        Form.LoadHtmlData("Items/StockReturn", function (result) {
            $scope.HtmlData = result;
        });
    },
]);
app.controller("PrintItemExpireBarcodeController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "Item Expire Barcode";
        Form.LoadHtmlData("Items/PrintItemExpireBarcode", function (result) {
            $scope.HtmlData = result;
        });
    },
]);
