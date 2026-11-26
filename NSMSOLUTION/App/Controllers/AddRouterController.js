'use strict';
app.controller('AddRouterController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Router";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Router", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);