'use strict';
app.controller('AddDeviceNameController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Device Name";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddDeviceName", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);