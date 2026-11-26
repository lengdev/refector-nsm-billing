'use strict';
app.controller('TelegramBotController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Telegram Bot";
    $scope.LoadData = function () {
        Form.LoadHtmlData("TelegramBot", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);