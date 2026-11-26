'use strict';
app.controller('SetDefaultAccTypeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Set Default Acc Type";
    $scope.LoadData = function () {
        Form.LoadHtmlData("SetDefaultAccType", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);