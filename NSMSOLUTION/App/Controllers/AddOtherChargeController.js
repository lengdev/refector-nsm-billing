'use strict';
app.controller('AddOtherChargeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Other Charge";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddOtherCharge", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);