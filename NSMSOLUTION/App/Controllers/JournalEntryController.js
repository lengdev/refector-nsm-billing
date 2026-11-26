"use strict";
app.controller("JournalEntryController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "Journal Entry";

        $scope.LoadData = function () {
            var id = $location.search().id;
            var action = $location.search().action;

            if (action === "Create" || action === "create") {
                Form.LoadHtmlData("JournalEntry/Create" + (id ? "?id=" + id : ""), function (result) {
                    $scope.HtmlData = result;
                });
            } else if (action === "Invoice" || action === "invoice") {
                Form.LoadHtmlData("JournalEntry/Invoice?id=" + (id || 0), function (result) {
                    $scope.HtmlData = result;
                });
            } else {
                Form.LoadHtmlData("JournalEntry", function (result) {
                    $scope.HtmlData = result;
                });
            }
        };

        $scope.$watch(
            function () {
                return ($location.search().id || "") + "|" + ($location.search().action || "");
            },
            function (newVal, oldVal) {
                if (newVal !== oldVal) {
                    $scope.LoadData();
                }
            }
        );

        $scope.LoadData();
    }
]);
