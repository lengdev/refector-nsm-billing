'use strict';
app.controller('AddVendorController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Vendor";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddVendor", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('AddBankerNameController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Banker Name";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddVendor/BankerName", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('VendorsController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Vendors Center";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Vendors/VendorsIndex", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);