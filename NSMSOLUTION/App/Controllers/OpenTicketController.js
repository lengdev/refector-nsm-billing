'use strict';
app.controller('OpenTicketController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Open Ticket";
    $scope.LoadData = function () {
        Form.LoadHtmlData("OpenTicket", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);