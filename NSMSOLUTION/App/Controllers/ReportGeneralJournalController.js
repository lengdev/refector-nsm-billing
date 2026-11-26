"use strict";
app.controller("ReportGeneralJournalController", [
  "$rootScope",
  "$scope",
  "Form",
  "$location",
  function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Report General Journal";
    $scope.LoadData = function () {
      var ID =
        typeof $location.search().StatusID == "undefined"
          ? 0
          : $location.search().StatusID;
      Form.LoadHtmlData(
        "Report/GeneralJournal?StatusID=" + ID,
        function (result) {
          $scope.HtmlData = result;
        },
      );
    };
    $scope.LoadData();
  },
]);
