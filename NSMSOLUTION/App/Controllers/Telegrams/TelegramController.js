'use strict';
app.controller('TelegramController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Telegram";
    Form.LoadHtmlData("Telegrams/Index", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('TelegramMessageController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Telegram Message";
    Form.LoadHtmlData("Telegrams/TelegramMessage", function (result) {
        $scope.HtmlData = result;
    });
}]);