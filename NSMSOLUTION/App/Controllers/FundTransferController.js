"use strict";
app.controller("FundTransferController", [
  "$rootScope",
  "$scope",
  "Form",
  "$location",
  function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Fund Transfer";

    $scope.LoadData = function () {
      var InvoiceID = $location.search().InvoiceID;
      var PrimaryFieldValue = $location.search().PrimaryFieldValue;
      if (InvoiceID && InvoiceID !== "" && InvoiceID !== "0") {
        Form.LoadHtmlData(
          "FundTransfer/Invoice?FundTransferID=" + InvoiceID,
          function (result) {
            $scope.HtmlData = result;
          },
        );
      } else if (
        PrimaryFieldValue &&
        PrimaryFieldValue !== "" &&
        PrimaryFieldValue !== "0"
      ) {
        Form.LoadHtmlData(
          "FundTransfer/CreateFundTransfer?PrimaryFieldValue=" +
            PrimaryFieldValue,
          function (result) {
            $scope.HtmlData = result;
          },
        );
      } else {
        Form.LoadHtmlData("FundTransfer", function (result) {
          $scope.HtmlData = result;
        });
      }
    };

    $scope.$watch(
      function () {
        return (
          ($location.search().InvoiceID || "") +
          "|" +
          ($location.search().PrimaryFieldValue || "")
        );
      },
      function (newValue, oldValue) {
        if (newValue !== oldValue) {
          $scope.LoadData();
        }
      },
    );

    $scope.LoadData();
  },
]);
