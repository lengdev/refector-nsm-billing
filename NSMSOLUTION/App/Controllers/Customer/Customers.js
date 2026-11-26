'use strict';
app.controller('CustomerFeedbackController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Feedback";
    $scope.LoadData = function () {
        Form.LoadHtmlData("CustomerFeedBack", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);

app.controller('CustomerController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Profile";
    Form.LoadHtmlData("Customers/Profile", function (result) {
        $scope.HtmlData = result;
    });
}]);
app.controller('CustomerCenterController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Customer Center";
    Form.LoadHtmlData("CustomerCenter", function (result) {
        $scope.HtmlData = result;
    });
}]);