'use strict';
app.controller('SubscriptionsController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Subscriptions";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Subscriptions", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);