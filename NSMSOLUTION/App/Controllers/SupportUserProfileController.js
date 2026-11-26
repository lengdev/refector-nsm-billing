'use strict';
app.controller('SupportUserProfileController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "User Profile";
    $scope.LoadData = function () {
        Form.LoadHtmlData("SupportUserProfile", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);