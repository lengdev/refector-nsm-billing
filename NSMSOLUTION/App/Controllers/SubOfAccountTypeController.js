'use strict';
app.controller('SubOfAccountTypeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Sub Of Account Type";
    $scope.LoadData = function () {
        Form.LoadHtmlData("SubOfAccountType", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);