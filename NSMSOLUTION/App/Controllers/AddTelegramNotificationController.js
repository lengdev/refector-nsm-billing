'use strict';
app.controller('AddTelegramNotificationController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Telegram Notification";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddTelegramNotification", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);