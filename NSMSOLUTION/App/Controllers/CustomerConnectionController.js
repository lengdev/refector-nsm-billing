'use strict';
app.controller('CustomerConnectionController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Connection";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Customer/Connection", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);