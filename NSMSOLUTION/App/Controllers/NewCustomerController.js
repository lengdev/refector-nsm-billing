'use strict';
app.controller('NewCustomerController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "New Customer";
    $scope.LoadData = function () {
        let FormStatus = (typeof $location.search().FormStatus == "undefined" ? "" : $location.search().FormStatus);
        Form.LoadHtmlData("NewCustomer?FormStatus=" + FormStatus || 'CustomerProfile', function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);