app.controller('CreateWarehouseController', ['$rootScope', '$scope', 'LoginService', 'Form', function ($rootScope, $scope, LoginService, Form) {
    $rootScope.title = "Create Warehouse";
    $rootScope.header = "Create Warehouse";
    $scope.LoadCreateWarehouse = function () {
        Form.LoadHtmlData("Administrator/CreateWarehouse", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadCreateWarehouse();
}]);