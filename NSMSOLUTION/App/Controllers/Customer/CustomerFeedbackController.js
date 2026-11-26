"use strict";
app.controller("CustFeedbackCtrl", [
  "$scope",
  "$element",
  "$http",
  "$timeout",
  function ($scope, $element, $http, $timeout) {
    var defaultFeedback = {
      CustomerID: 0,
      CustomerName: "",
      InternetSpeed: "",
      Pricing: "",
      CustomerServices: "",
      Ratingus: 0,
      CustomerComment: "",
      ContactBy: "",
      Complaint: "",
      AssignTo: "", // NEW: team to follow up (new feedback only)
      IsNoAnswer: false,
      IsBusy: false,
      IsDelete: false,
      CommentNoAnswerOrBusy: "",
    };

    var defaultFlow = {
      open: false,
      tab: "update", // update | history
      item: {},
      logs: [],
      loadingLogs: false,
      assignTo: "",
      status: "",
      message: "",
      showErrors: false,
    };

    $scope.stars = [1, 2, 3, 4, 5];
    $scope.modalOpen = false;
    $scope.feedback = angular.copy(defaultFeedback);
    $scope.isSaving = false;
    $scope.loading = {};
    $scope.flow = angular.copy(defaultFlow);

    // ---- Workflow config ---------------------------------------------
    $scope.teams = ["Sales Team", "Account Team", "Technical Team"];
    $scope.statuses = ["Pending", "In Progress", "Done"];
    $scope.currentTeam = ""; // loaded from api/CustomerFeedback/Context; the server re-checks every action

    var TEAM_ICON = {
      "Sales Team": "ti-briefcase",
      "Account Team": "ti-receipt",
      "Technical Team": "ti-tool",
    };
    var STATUS_CLASS = {
      Pending: "cf-badge-warning",
      "In Progress": "cf-badge-info",
      Done: "cf-badge-success",
    };
    var TL_ICON = {
      Created: "ti-circle-plus",
      Handover: "ti-arrows-exchange",
      Status: "ti-progress-check",
      Comment: "ti-message-circle",
    };

    $scope.teamIcon = function (t) {
      return TEAM_ICON[t] || "ti-users";
    };
    $scope.statusClass = function (s) {
      return STATUS_CLASS[s] || "cf-badge-neutral";
    };
    $scope.tlIcon = function (a) {
      return TL_ICON[a] || "ti-point";
    };
    $scope.tlLabel = function (log) {
      switch (log.ActionType) {
        case "Created":
          return "logged this feedback";
        case "Handover":
          return "handed over to " + log.AssignedTeam;
        case "Status":
          return "changed status";
        default:
          return "added a comment";
      }
    };

    // ---- Queue filter chips --------------------------------------------
    $scope.queues = [
      { key: "all", label: "All", icon: "ti-list" },
      { key: "mine", label: "My team's queue", icon: "ti-inbox" },
      { key: "Pending", label: "Pending", icon: "ti-clock" },
      { key: "In Progress", label: "In Progress", icon: "ti-loader" },
      { key: "Done", label: "Done", icon: "ti-circle-check" },
    ];
    $scope.queue = "all";

    function matchesQueue(item, key) {
      switch (key) {
        case "all":
          return true;
        case "mine":
          return (
            !!$scope.currentTeam &&
            item.FollowUpByTeam === $scope.currentTeam &&
            item.FollowUpStatus !== "Done"
          );
        default:
          return item.FollowUpStatus === key;
      }
    }

    $scope.queueCount = function (key) {
      return $scope.feedbackHistory.filter(function (i) {
        return matchesQueue(i, key);
      }).length;
    };

    $scope.setQueue = function (key) {
      $scope.queue = key;
      $scope.historyPage = 1;
    };

    // ---- Feedback history table state ----------------------------------
    $scope.feedbackHistory = [];
    $scope.historyLoading = false;
    $scope.historySearch = "";
    $scope.historyPage = 1;
    $scope.historyPageSize = 10;
    $scope.pageSizes = [10, 25, 50, 100];
    $scope.stats = { total: 0, avgRating: 0, complaints: 0 };

    $scope.isEditMode = false;
    $scope.hoverRating = 0;

    var customersObj = [];
    var customersPromise = null;

    const contactByObj = [
      { ContactKeyMap: "Phone Call" },
      { ContactKeyMap: "Telegram Message" },
      { ContactKeyMap: "Telegram Call" },
      { ContactKeyMap: "Email" },
      { ContactKeyMap: "SMS" },
    ];
    const complaintObj = [
      { ComplaintKeyMap: "Complaint" },
      { ComplaintKeyMap: "No Complaint" },
    ];

    // JSON in / JSON out. (Customer lookup below uses the same helper.)
    const api = (url, opts) => {
      opts = opts || {};
      if (opts.loadingKey) $scope.loading[opts.loadingKey] = true;

      var cfg = {
        method: opts.method || (opts.data ? "POST" : "GET"),
        url: url,
        ignoreLoadingBar: true,
        cache: opts.cache !== undefined ? opts.cache : false,
      };
      if (opts.data) {
        if (cfg.method === "GET") cfg.params = opts.data;
        else cfg.data = opts.data;
      }

      return $http(cfg)
        .then((r) => r.data)
        .finally(() => {
          if (opts.loadingKey) $scope.loading[opts.loadingKey] = false;
        });
    };

    const fbUrl = (path) => ServerRoot + "api/CustomerFeedback" + (path || "");

    // works whether the API serializes PascalCase or camelCase
    const norm = (r) => {
      r = r || {};
      return {
        Result: r.Result !== undefined ? r.Result : r.result,
        Message: r.Message !== undefined ? r.Message : r.message,
        Data: r.Data !== undefined ? r.Data : r.data,
        Action: r.Action !== undefined ? r.Action : r.action,
      };
    };

    const loadCustomers = () => {
      if (!customersPromise) {
        customersPromise = api(`${ServerRoot}api/Customer/GetCustomerLists`, {
          cache: true,
        }).then((data) => {
          customersObj = data || [];
          return customersObj;
        });
      }
      return customersPromise;
    };

    const initTypeahead = () => {
      $timeout(function () {
        loadCustomers().then(function () {
          ApplyTypeahead(
            "CustomerName",
            customersObj,
            { CustomerID: "CustomerID", CustomerCode: "CustomerCode" },
            { Name: "CustomerName" },
          );

          $("#CustomerName")
            .off("typeahead:select.cf")
            .on("typeahead:select.cf", function (e, item) {
              $scope.$apply(function () {
                // hidden fields are filled by ApplyTypeahead; fall back to the list itself
                var match =
                  (customersObj || []).filter(function (c) {
                    return c.CustomerName === item.Name;
                  })[0] || {};
                $scope.feedback.CustomerID =
                  Number($element.find("#CustomerID").val()) ||
                  Number(item.CustomerID) ||
                  Number(match.CustomerID) ||
                  0;
                $scope.feedback.CustomerCode =
                  $element.find("#CustomerCode").val() ||
                  item.CustomerCode ||
                  match.CustomerCode ||
                  "";
                $scope.feedback.CustomerName = item.Name;
              });
            });
        });

        ApplyTypeahead("ContactBy", contactByObj, "", {
          Name: "ContactKeyMap",
        });
        ApplyTypeahead("Complaint", complaintObj, "", {
          Name: "ComplaintKeyMap",
        });

        $("#ContactBy")
          .off("typeahead:select.cf")
          .on("typeahead:select.cf", function (e, item) {
            $scope.$apply(function () {
              $scope.feedback.ContactBy = item.Name;
            });
          });
        $("#Complaint")
          .off("typeahead:select.cf")
          .on("typeahead:select.cf", function (e, item) {
            $scope.$apply(function () {
              $scope.feedback.Complaint = item.Name;
            });
          });
      }, 300);
    };

    // ---- Rating ---------------------------------------------------------
    $scope.setRating = function (val) {
      $scope.feedback.Ratingus = val;
    };
    $scope.previewRating = function (val) {
      $scope.hoverRating = val;
    };
    $scope.clearRatingPreview = function () {
      $scope.hoverRating = 0;
    };
    $scope.displayedRating = function () {
      return $scope.hoverRating || $scope.feedback.Ratingus;
    };

    // ---- Feedback CRUD --------------------------------------------------
    $scope.editFeedback = function (item) {
      $scope.feedback = angular.copy(item);
      $scope.editFeedbackID = item.FeedbackID;
      $scope.isEditMode = true;
      $scope.modalOpen = true;
      initTypeahead();
    };

    $scope.deleteFeedback = function (item) {
      DialogSweetCofirm(
        function () {
          api(fbUrl("/" + item.FeedbackID), { method: "DELETE" })
            .then(norm)
            .then(function (r) {
              if (r.Result == 2) {
                $scope.loadFeedbackHistory();
                DialogSweetAlert("deleteted", "Feedback has been deleted.");
              } else {
                DialogSweetAlert("error", r.Message);
              }
            })
            .catch(function () {
              DialogSweetAlert("error", "Could not delete the feedback.");
            });
        },
        "",
        "Delete this feedback?",
        "Yes, delete it",
      );
    };

    $scope.newFeedback = function () {
      $scope.isEditMode = false;
      $scope.editFeedbackID = 0;
      $scope.feedback = angular.copy(defaultFeedback);
      $scope.modalOpen = true;
      initTypeahead();
    };

    $scope.closeFeedback = function () {
      $scope.modalOpen = false;
      $("body").removeClass("cf-modal-open");
    };

    $scope.onNoAnswer = function () {
      SweetCofirmYesNoCancel(
        "",
        "No Answer",
        "",
        function (comment) {
          $scope.feedback.IsNoAnswer = true;
          $scope.feedback.CommentNoAnswerOrBusy = comment;
        },
        "",
        true,
      );
    };

    $scope.onBusy = function () {
      SweetCofirmYesNoCancel(
        "",
        "Busy",
        "",
        function (comment) {
          $scope.feedback.IsBusy = true;
          $scope.feedback.CommentNoAnswerOrBusy = comment;
        },
        "",
        true,
      );
    };

    $scope.saveFeedback = function (form) {
      if (form.$invalid) return;

      if (!Number($scope.feedback.CustomerID)) {
        DialogSweetAlert(
          "error",
          "Please pick the customer from the dropdown list.",
        );
        return;
      }

      $scope.isSaving = true;
      var id = $scope.editFeedbackID || 0;
      var wasNew = id === 0;
      var assigned = $scope.feedback.AssignTo;

      api(wasNew ? fbUrl("") : fbUrl("/" + id), {
        method: wasNew ? "POST" : "PUT",
        data: $scope.feedback,
      })
        .then(norm)
        .then(function (r) {
          if (r.Result == 2) {
            $scope.modalOpen = false;
            $scope.editFeedbackID = null;
            $scope.loadFeedbackHistory();
            DialogSweetAlert(
              "success",
              wasNew
                ? assigned
                  ? "Feedback created and sent to " + assigned + "."
                  : "Feedback created successfully."
                : "Feedback updated successfully.",
            );
          } else {
            DialogSweetAlert("error", r.Message);
          }
        })
        .catch(function () {
          DialogSweetAlert(
            "error",
            "Something went wrong while saving the feedback.",
          );
        })
        .finally(function () {
          $scope.isSaving = false;
        });
    };

    // =====================================================================
    //  FOLLOW-UP WORKFLOW
    // =====================================================================

    // 0 = not assigned, 1 = in a team's hands, 2 = done
    $scope.stage = function (item) {
      if (!item || !item.FollowUpByTeam) return 0;
      return item.FollowUpStatus === "Done" ? 2 : 1;
    };

    // Owner team (or Sales) may act; anyone can claim an unassigned item.
    // Server enforces the same rule — this only drives the UI.
    $scope.canAct = function (item) {
      if (!item || !item.FollowUpByTeam) return true;
      return (
        item.FollowUpByTeam === $scope.currentTeam ||
        $scope.currentTeam === "Sales Team"
      );
    };

    $scope.openFollowUp = function (item, tab) {
      $scope.flow = angular.extend(angular.copy(defaultFlow), {
        open: true,
        tab: tab || "update",
        item: item,
        assignTo: item.FollowUpByTeam || $scope.currentTeam || "",
        status: item.FollowUpStatus || "Pending",
      });
      $scope.isSaving = false;
      loadLogs(item.FeedbackID);
    };

    $scope.closeFollowUp = function () {
      $scope.flow = angular.copy(defaultFlow);
      $scope.isSaving = false;
    };

    function loadLogs(feedbackId) {
      $scope.flow.loadingLogs = true;
      return api(fbUrl("/" + feedbackId + "/Log"))
        .then(norm)
        .then(function (r) {
          if ($scope.flow.item && $scope.flow.item.FeedbackID === feedbackId) {
            $scope.flow.logs = r.Data || [];
          }
        })
        .catch(function () {
          DialogSweetAlert("error", "Could not load the step history.");
        })
        .finally(function () {
          $scope.flow.loadingLogs = false;
        });
    }

    // What will happen when the user presses the main button?
    $scope.isHandover = function () {
      var f = $scope.flow;
      return (
        !!f.assignTo && !!f.item && f.assignTo !== (f.item.FollowUpByTeam || "")
      );
    };

    $scope.flowAction = function () {
      var f = $scope.flow;
      if (!f.item) return "Comment";
      if ($scope.isHandover()) return "Handover";
      if (f.status && f.status !== (f.item.FollowUpStatus || ""))
        return "Status";
      return "Comment";
    };

    $scope.onTeamChange = function () {
      // receiving team always starts at Pending; switching back restores current status
      $scope.flow.status = $scope.isHandover()
        ? "Pending"
        : $scope.flow.item.FollowUpStatus || "Pending";
    };

    $scope.commentRequired = function () {
      var a = $scope.flowAction();
      return (
        a === "Handover" ||
        a === "Comment" ||
        ($scope.flow.status === "Done" && a === "Status")
      );
    };

    $scope.commentError = function () {
      var a = $scope.flowAction();
      if (a === "Handover") return "Explain what the next team needs to do.";
      if (a === "Status")
        return "A comment is required to close this follow-up.";
      return "Please write a comment.";
    };

    $scope.commentPlaceholder = function () {
      switch ($scope.flowAction()) {
        case "Handover":
          return (
            "What should " + $scope.flow.assignTo + " do for this customer?"
          );
        case "Status":
          return $scope.flow.status === "Done"
            ? "What was done to resolve it?"
            : "Optional note about the progress...";
        default:
          return "Write the follow-up note here...";
      }
    };

    $scope.actionLabel = function () {
      switch ($scope.flowAction()) {
        case "Handover":
          return "Hand over to " + $scope.flow.assignTo;
        case "Status":
          return $scope.flow.status === "Done"
            ? "Mark as Done"
            : "Update status";
        default:
          return "Add comment";
      }
    };

    $scope.actionIcon = function () {
      switch ($scope.flowAction()) {
        case "Handover":
          return "ti-arrows-exchange";
        case "Status":
          return $scope.flow.status === "Done"
            ? "ti-circle-check"
            : "ti-progress-check";
        default:
          return "ti-send";
      }
    };

    // Button is enabled when there is something to send.
    $scope.hasChange = function () {
      return (
        $scope.flowAction() !== "Comment" ||
        !!($scope.flow.message || "").trim()
      );
    };

    $scope.submitFollowUp = function () {
      var f = $scope.flow;
      if (!f.item || $scope.isSaving) return;

      f.showErrors = true;
      if ($scope.commentRequired() && !(f.message || "").trim()) return;

      $scope.isSaving = true;
      var id = f.item.FeedbackID;
      var wasHandover = $scope.isHandover();
      var target = f.assignTo;

      api(fbUrl("/" + id + "/FollowUp"), {
        method: "POST",
        data: {
          AssignTo: wasHandover ? f.assignTo : "",
          FollowUpStatus: f.status,
          CommentText: (f.message || "").trim(),
        },
      })
        .then(norm)
        .then(function (r) {
          if (r.Result == 2) {
            DialogSweetAlert(
              "success",
              wasHandover
                ? "Handed over to " + target + "."
                : "Follow-up updated.",
            );
            // refresh table + log, keep the popup open so the new step appears
            $scope.loadFeedbackHistory().then(function () {
              var fresh = $scope.feedbackHistory.filter(function (i) {
                return i.FeedbackID === id;
              })[0];
              if (fresh && $scope.flow.open) {
                $scope.flow.item = fresh;
                $scope.flow.assignTo = fresh.FollowUpByTeam || "";
                $scope.flow.status = fresh.FollowUpStatus || "Pending";
                $scope.flow.message = "";
                $scope.flow.showErrors = false;
                $scope.flow.tab = "history";
              }
            });
            loadLogs(id);
          } else {
            DialogSweetAlert("error", r.Message);
            // conflict (someone else changed it) -> refresh so the popup shows the truth
            $scope.loadFeedbackHistory();
            loadLogs(id);
          }
        })
        .catch(function () {
          DialogSweetAlert(
            "error",
            "Something went wrong while saving the follow-up.",
          );
        })
        .finally(function () {
          $scope.isSaving = false;
        });
    };

    // ---- Feedback history ------------------------------------------------
    $scope.loadFeedbackHistory = function (customerId) {
      $scope.historyLoading = true;

      return api(fbUrl(""), {
        data: customerId ? { customerId: customerId } : undefined,
      })
        .then(norm)
        .then(function (r) {
          $scope.feedbackHistory = r.Data || [];
          $scope.computeStats();
        })
        .catch(function () {
          DialogSweetAlert("error", "Could not load the feedback list.");
        })
        .finally(function () {
          $scope.historyLoading = false;
        });
    };

    $scope.filteredHistory = function () {
      var term = ($scope.historySearch || "").toLowerCase().trim();

      return $scope.feedbackHistory.filter(function (item) {
        if (!matchesQueue(item, $scope.queue)) return false;
        if (!term) return true;
        return Object.keys(item).some(function (key) {
          var value = item[key];
          if (value === null || value === undefined) return false;
          return value.toString().toLowerCase().indexOf(term) > -1;
        });
      });
    };

    $scope.pagedHistory = function () {
      var list = $scope.filteredHistory();
      var totalPages = Math.max(
        1,
        Math.ceil(list.length / $scope.historyPageSize),
      );
      if ($scope.historyPage > totalPages) $scope.historyPage = totalPages;
      if ($scope.historyPage < 1) $scope.historyPage = 1;
      var start = ($scope.historyPage - 1) * $scope.historyPageSize;
      return list.slice(start, start + $scope.historyPageSize);
    };

    $scope.totalHistoryPages = function () {
      return Math.max(
        1,
        Math.ceil($scope.filteredHistory().length / $scope.historyPageSize),
      );
    };

    $scope.$watch("historySearch", function () {
      $scope.historyPage = 1;
    });

    $scope.historyRangeEnd = function () {
      return Math.min(
        $scope.historyPage * $scope.historyPageSize,
        $scope.filteredHistory().length,
      );
    };

    $scope.prevPage = function () {
      if ($scope.historyPage > 1) $scope.historyPage--;
    };
    $scope.nextPage = function () {
      if ($scope.historyPage < $scope.totalHistoryPages()) $scope.historyPage++;
    };

    $scope.exportExcel = function () {
      var ws = XLSX.utils.json_to_sheet($scope.getExportData());
      var wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, "Feedback");
      XLSX.writeFile(wb, "CustomerFeedback.xlsx");
    };

    $scope.getExportData = function () {
      return angular.copy($scope.filteredHistory());
    };

    var badgeMap = {
      speed: {
        Smoothly: "badge-success",
        Slow: "badge-warning",
        "Not Stable": "badge-danger",
      },
      price: {
        Reasonable: "badge-success",
        Cheap: "badge-info",
        Expensive: "badge-warning",
      },
      service: {
        "Very Good": "badge-success",
        Good: "badge-info",
        Bad: "badge-danger",
      },
    };

    $scope.badgeClass = function (type, value) {
      return (badgeMap[type] && badgeMap[type][value]) || "badge-neutral";
    };

    $scope.viewFeedback = function (item) {
      ModalDialogBox(
        "Customer Feedback Result",
        "CustomerFeedBack/ShowCustFeedbackResult?FeedbackID=" +
          item.FeedbackID +
          "&CustomerID=" +
          (item.CustomerID || ""),
        "",
        "",
        "",
        "",
        "",
        true,
      );
    };

    $scope.computeStats = function () {
      var list = $scope.feedbackHistory;
      var rated = list.filter(function (i) {
        return i.Ratingus;
      });
      var complaints = list.filter(function (i) {
        return !i.IsNoAnswer && !i.IsBusy && i.Complaint === "Complaint";
      });
      var uniqueCustomers = {};
      list.forEach(function (i) {
        if (i.CID) uniqueCustomers[i.CID] = true;
      });

      $scope.stats = {
        total: list.length,
        avgRating: rated.length
          ? rated.reduce(function (sum, i) {
              return sum + Number(i.Ratingus);
            }, 0) / rated.length
          : 0,
        complaints: complaints.length,
        noAnswer: list.filter(function (i) {
          return i.IsNoAnswer;
        }).length,
        busy: list.filter(function (i) {
          return i.IsBusy;
        }).length,
        customers: Object.keys(uniqueCustomers).length,
      };
    };

    // ---- Report counts (used by the report page) ------------------------
    $scope.report = null;
    $scope.loadReport = function (startDate, endDate) {
      return api(fbUrl("/Report"), {
        data: { startDate: startDate || "", endDate: endDate || "" },
      })
        .then(norm)
        .then(function (r) {
          $scope.report = r.Data || null;
          return $scope.report;
        });
    };

    // ---- Initial load: who am I (team) -> then the list -------------------
    api(fbUrl("/Context"))
      .then(function (r) {
        r = r || {};
        $scope.currentTeam = r.Team || r.team || "";
        var t = r.Teams || r.teams;
        var st = r.Statuses || r.statuses;
        if (t && t.length) $scope.teams = t;
        if (st && st.length) $scope.statuses = st;
      })
      .finally(function () {
        $scope.loadFeedbackHistory();
      });
  },
]);

function checkcust() {
  var isFeedback = $("#Customer_Code_CID_IsFeedback").val();
  if (isFeedback == "True") {
    DialogSweetAlert("error", "This customer already feedback.", function () {
      $("#Customer_Code_CID_IsFeedback").focus();
    });
  }
}
