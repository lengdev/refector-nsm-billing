'use strict';
app.controller('AddVendorTypeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Vendor Type";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddVendorType", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);