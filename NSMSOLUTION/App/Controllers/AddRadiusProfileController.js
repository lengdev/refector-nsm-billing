'use strict';
app.controller('AddRadiusProfileController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Radius Profile";
    $scope.LoadData = function () {
        Form.LoadHtmlData("RADIUS/RadiusProfile", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);