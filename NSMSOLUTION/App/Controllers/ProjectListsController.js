'use strict';
app.controller('ProjectListsController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Project Lists";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Project/ProjectLists", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);