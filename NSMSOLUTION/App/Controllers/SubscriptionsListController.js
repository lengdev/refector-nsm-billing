'use strict';
app.controller('SubscriptionsListController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Subscriptions List";
    $scope.LoadData = function () {
        Form.LoadHtmlData("Subscriptions/SubscriptionsList", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);