'use strict';
app.controller('AddConnectionTypeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Connection Type";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddConnectionType", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);