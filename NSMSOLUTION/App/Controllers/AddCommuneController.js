'use strict';
app.controller('AddCommuneController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Commune";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddCommune", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);