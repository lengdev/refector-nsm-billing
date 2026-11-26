'use strict';
app.controller('ConnectionTypeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Connection Type";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Customer/ConnectionType", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);