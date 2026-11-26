'use strict';
app.controller('AddCustomerTypeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Customer Type";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddCustomerType", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('AddCustomerBlockController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Customer Block";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddCustomerBlock", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);