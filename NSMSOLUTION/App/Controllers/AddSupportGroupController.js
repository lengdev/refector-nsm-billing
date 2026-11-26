'use strict';
app.controller('AddSupportGroupController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Support Group";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Group", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);