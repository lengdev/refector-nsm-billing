'use strict';
app.controller('AddInformationCategoryController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Information Category";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddInformationCategory", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
'use strict';
app.controller('AddInformationNewsController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Information News";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddInformationNews", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
'use strict';
app.controller('AddTypeBadgeController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    $rootScope.title = "Add Type Badge";
    $scope.LoadData = function () {
        Form.LoadHtmlData("AddTypeBadge", function (result) {
            $scope.HtmlData = result;
        });
    };
    $scope.LoadData();
}]);
// app.controller('AuthorizePurchaseRequestController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
//     $rootScope.title = "Authorize Purchase Request";
//     $scope.LoadData = function () {
//         Form.LoadHtmlData("Purchase/AuthorizePurchaseRequest", function (result) {
//             $scope.HtmlData = result;
//         });
//     };
//     $scope.LoadData();
// }]);
// app.controller('AuthorizePurchaseOrderController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
//     $rootScope.title = "Authorize Purchase Order";
//     $scope.LoadData = function () {
//         Form.LoadHtmlData("Purchase/AuthorizePurchaseOrder", function (result) {
//             $scope.HtmlData = result;
//         });
//     };
//     $scope.LoadData();
// }]);

// app.controller('EnterBillController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
//     $rootScope.title = "Enter Bill";
//     $scope.LoadData = function () {
//         Form.LoadHtmlData("Purchase/EnterBill", function (result) {
//             $scope.HtmlData = result;
//         });
//     };
//     $scope.LoadData();
// }]);

// app.controller('GoodsReceivedController', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
//     $rootScope.title = "Goods Received";
//     $scope.LoadData = function () {
//         Form.LoadHtmlData("Purchase/GoodsReceived", function (result) {
//             $scope.HtmlData = result;
//         });
//     };
//     $scope.LoadData();
// }]);

