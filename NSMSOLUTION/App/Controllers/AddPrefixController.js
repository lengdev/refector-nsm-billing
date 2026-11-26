'use strict';
app.controller('AddPrefixController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Prefix";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddPrefix", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);