'use strict';
app.controller('AddPONController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add PON";
    $scope.LoadData = function () {
        Form.LoadHtmlData("OLT/PON", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);