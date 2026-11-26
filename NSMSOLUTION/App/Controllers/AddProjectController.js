'use strict';
app.controller('AddProjectController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Project";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Project", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);