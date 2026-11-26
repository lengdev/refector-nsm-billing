'use strict';
app.controller('ReportCustomerAttachmentController', ['$rootScope', '$scope', 'Form', '$location', function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report Customer Attachment";
    $scope.LoadData = function () {
        Form.LoadHtmlData("ReportCustomerAttachment", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);