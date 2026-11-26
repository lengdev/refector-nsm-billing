'use strict';
app.controller('AddCustomerGroupController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Customer Group";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddCustomerGroup", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);