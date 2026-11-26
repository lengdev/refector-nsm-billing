'use strict';
app.controller('ProfileController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Profile";
    $scope.LoadData = function () {
        var ID = $location.search().ID;
        Form.LoadHtmlData("Profile?ID=" + ID, function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);