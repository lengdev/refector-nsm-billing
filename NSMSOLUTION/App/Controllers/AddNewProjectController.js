'use strict';
app.controller('AddNewProjectController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Project";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddNewProject", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
