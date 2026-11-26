'use strict';
app.controller('AddServicesController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Services";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddServices", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);