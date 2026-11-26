'use strict';
app.controller('AddOLTController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add OLT";
    $scope.LoadData = function () {
        Form.LoadHtmlData("OLT", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);