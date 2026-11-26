'use strict';
app.controller('InfrastructureProviderController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Infrastructure Provider";
    $scope.LoadData = function () {
        Form.LoadHtmlData("InfrastructureProvider", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);