'use strict';
app.controller('AddRouterDeviceStatusController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Router Device Status";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddRouterDeviceStatus", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);