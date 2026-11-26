'use strict';
app.controller('TelegramNotificationController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Telegram Notification";
    $scope.LoadData = function () {
        Form.LoadHtmlData("TelegramNotification", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);