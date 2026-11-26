"use strict";

app.controller("AlertMessagesCtrl", [
  "$scope",
  "$http",
  "$timeout",
  function ($scope, $http, $timeout) {
    // ── State ──────────────────────────────────────────────────────────────────
    $scope.alerts = [];
    $scope.filteredAlerts = [];
    $scope.currentFilter = "all";
    $scope.searchQuery = "";
    $scope.loading = false;
    $scope.page = 1;
    $scope.pageSize = 18;
    $scope.hasMore = true;
    $scope.allSelected = false;
    $scope.metrics = {
      Total: 0,
      Unread: 0,
      Urgent: 0,
      Customer: 0,
      Ticket: 0,
      Finance: 0,
    };

    $scope.filterTypes = [
      { key: "all", label: "All", icon: "ti-bell" },
      { key: "customer", label: "Customer", icon: "ti-user" },
      { key: "ticket", label: "Ticket", icon: "ti-ticket" },
      { key: "system", label: "System", icon: "ti-server" },
      { key: "finance", label: "Finance", icon: "ti-report-money" },
    ];

    // ── Add New modal state ───────────────────────────────────────────────────
    $scope.showAddModal = false;
    $scope.isSavingNew = false;
    $scope.customers = [];
    $scope.newAlert = {};
    $scope.typeOptions = $scope.filterTypes.filter(function (t) {
      return t.key !== "all";
    });

    // ── Data loading ──────────────────────────────────────────────────────────
    $scope.loadAlerts = function (reset) {
      if (reset) {
        $scope.page = 1;
        $scope.alerts = [];
        $scope.hasMore = true;
      }
      if (!$scope.hasMore || $scope.loading) return;

      $scope.loading = true;

      $http
        .get(apiBaseUrl + "AlertMessages/GetAlerts", {
          params: {
            page: $scope.page,
            pageSize: $scope.pageSize,
            type: $scope.currentFilter === "all" ? null : $scope.currentFilter,
            search: $scope.searchQuery || null,
          },
          ignoreLoadingBar: true,
        })
        .then(function (res) {
          var data = res.data;
          $scope.alerts = $scope.alerts.concat(data.Items || []);
          $scope.metrics = data.Metrics || $scope.metrics;
          $scope.hasMore = data.HasMore || false;
          $scope.page++;
          $scope.applyFilter();
        })
        .catch(function (err) {
          console.error("Failed to load alerts", err);
        })
        .finally(function () {
          $scope.loading = false;
        });
    };

    // ── Filtering & sorting ───────────────────────────────────────────────────
    $scope.applyFilter = function () {
      var q = ($scope.searchQuery || "").toLowerCase();

      $scope.filteredAlerts = $scope.alerts.filter(function (a) {
        if (a.IsDismissed) return false;
        if (
          $scope.currentFilter !== "all" &&
          a.AlertType !== $scope.currentFilter
        )
          return false;
        if (q) {
          return (
            (a.CustomerName || "").toLowerCase().indexOf(q) >= 0 ||
            (a.CustomerCode || "").toLowerCase().indexOf(q) >= 0 ||
            (a.CustomerMobile || "").toLowerCase().indexOf(q) >= 0 ||
            (a.Comment || "").toLowerCase().indexOf(q) >= 0
          );
        }
        return true;
      });

      // Sort: unread → urgent → newest
      $scope.filteredAlerts.sort(function (a, b) {
        var unreadDiff = (a.IsRead ? 0 : 1) - (b.IsRead ? 0 : 1);
        var urgentDiff = (b.IsUrgent ? 1 : 0) - (a.IsUrgent ? 1 : 0);
        var dateDiff = new Date(b.CreatedAt) - new Date(a.CreatedAt);
        return -unreadDiff || urgentDiff || dateDiff;
      });

      $scope.recalcMetrics();
    };

    $scope.recalcMetrics = function () {
      var active = $scope.alerts.filter(function (a) {
        return !a.IsDismissed;
      });
      $scope.metrics.Total = active.length;
      $scope.metrics.Unread = active.filter(function (a) {
        return !a.IsRead;
      }).length;
      $scope.metrics.Urgent = active.filter(function (a) {
        return a.IsUrgent;
      }).length;
      $scope.metrics.Customer = active.filter(function (a) {
        return a.AlertType === "customer";
      }).length;
      $scope.metrics.Ticket = active.filter(function (a) {
        return a.AlertType === "ticket";
      }).length;
      $scope.metrics.Finance = active.filter(function (a) {
        return a.AlertType === "finance";
      }).length;
    };

    $scope.setFilter = function (type) {
      $scope.currentFilter = type;
      $scope.allSelected = false;
      $scope.applyFilter();
    };

    $scope.filterCount = function (type) {
      var active = $scope.alerts.filter(function (a) {
        return !a.IsDismissed;
      });
      return type === "all"
        ? active.length
        : active.filter(function (a) {
            return a.AlertType === type;
          }).length;
    };

    // ── Selection (header checkbox) ───────────────────────────────────────────
    $scope.toggleAll = function () {
      $scope.filteredAlerts.forEach(function (a) {
        a._selected = $scope.allSelected;
      });
    };

    // ── Actions ───────────────────────────────────────────────────────────────
    $scope.markRead = function (alert) {
      if (alert.IsRead) return;
      alert.IsRead = true;
      $scope.recalcMetrics();
      $http.post(
        apiBaseUrl + "AlertMessages/MarkRead",
        { id: alert.AlertID },
        { ignoreLoadingBar: true },
      );
    };

    $scope.markAllRead = function () {
      $scope.alerts.forEach(function (a) {
        a.IsRead = true;
      });
      $scope.applyFilter();
      $http.post(
        apiBaseUrl + "AlertMessages/MarkAllRead",
        {},
        { ignoreLoadingBar: true },
      );
    };

    $scope.dismiss = function (alert, $event) {
      $event.stopPropagation();
      alert.IsDismissed = true;
      $scope.applyFilter();
      $http.post(
        apiBaseUrl + "AlertMessages/Dismiss",
        { id: alert.AlertID },
        { ignoreLoadingBar: true },
      );
    };

    $scope.viewAlert = function (alert, $event) {
      $event.stopPropagation();
      $scope.markRead(alert);
      var url = "";
      if (alert.AlertType === "ticket" && alert.ReferenceID) {
        url = "#!/TicketSupport?ID=" + alert.ReferenceID;
      } else if (alert.CustomerID) {
        url = "#!/CustomerProfile?ID=" + alert.CustomerID;
      }

      if (url) {
        window.open(url, "_blank");
      }
    };

    // ── Add New alert ─────────────────────────────────────────────────────────
    function loadCustomers() {
      // NOTE: backend endpoint needs to exist — returns [{CustomerID, CustomerCode, CustomerName}]
      $http
        .get(apiBaseUrl + "AlertMessages/GetCustomers", {
          ignoreLoadingBar: true,
        })
        .then(function (res) {
          $scope.customers = res.data || [];
          $scope.cb.loading = false;
          $scope.filterCustomers();
        })
        .catch(function (err) {
          console.error("Failed to load customers", err);
          $scope.cb.loading = false;
        });
    }

    $scope.openAddModal = function () {
      $scope.newAlert = {
        CustomerID: null,
        Comment: "", // Alert topic
        Reason: "",
        PostComment: "", // Comment
        AlertType: "customer",
        IsUrgent: false,
      };
      $scope.isSavingNew = false;
      $scope.addError = "";
      $scope.showAddModal = true;
      $scope.resetCustomerCombo();
      if (!$scope.customers.length) {
        $scope.cb.loading = true;
        loadCustomers();
      }
    };

    $scope.closeAddModal = function () {
      if ($scope.isSavingNew) return;
      $scope.showAddModal = false;
    };

    $scope.saveNewAlert = function (form) {
      form.$setSubmitted();
      if (form.$invalid) return;

      var n = $scope.newAlert;
      $scope.addError = "";
      $scope.isSavingNew = true;

      // NOTE: backend endpoint needs to exist
      $http
        .post(
          apiBaseUrl + "AlertMessages/CreateAlert",
          {
            customerId: n.CustomerID,
            comment: n.Comment, // alert topic
            reason: n.Reason,
            postComment: n.PostComment,
            postCommentBy: localStorage.getItem("User_Full_Name"),
            alertType: n.AlertType,
            isUrgent: !!n.IsUrgent,
          },
          { ignoreLoadingBar: true },
        )
        .then(function (res) {
          if (res.data && res.data.success === false) {
            $scope.addError = res.data.error || "Failed to create alert.";
            return;
          }
          $scope.showAddModal = false;
          $scope.loadAlerts(true);
        })
        .catch(function (err) {
          console.error("Failed to create alert", err);
          $scope.addError = "Could not reach the server. Please try again.";
        })
        .finally(function () {
          $scope.isSavingNew = false;
        });
    };

    // ── Customer combobox (searchable) ────────────────────────────────────────
    $scope.cb = {
      open: false,
      loading: false,
      query: "",
      index: 0,
      results: [],
      style: {},
    };

    function norm(s) {
      return (s == null ? "" : String(s)).toLowerCase();
    }

    function positionCustomerList() {
      var input = document.getElementById("am-f-customer");
      if (!input) return;
      var r = input.getBoundingClientRect();
      var below = window.innerHeight - r.bottom;
      var style = { left: r.left + "px", width: r.width + "px" };
      if (below < 260 && r.top > below) {
        // not enough room below → open upwards
        style.bottom = window.innerHeight - r.top + 4 + "px";
        style.maxHeight = Math.min(260, r.top - 16) + "px";
      } else {
        style.top = r.bottom + 4 + "px";
        style.maxHeight = Math.min(260, below - 16) + "px";
      }
      $scope.cb.style = style;
    }

    function scrollActiveOption() {
      $timeout(function () {
        var el = document.querySelector(".am-combo-option.active");
        if (el && el.scrollIntoView) el.scrollIntoView({ block: "nearest" });
      });
    }

    $scope.resetCustomerCombo = function () {
      $scope.cb.open = false;
      $scope.cb.query = "";
      $scope.cb.index = 0;
      $scope.cb.results = [];
      $scope.filterCustomers();
    };

    $scope.customerMeta = function (c) {
      return [c.CustomerCode, c.CustomerMobile]
        .filter(function (x) {
          return x && String(x).trim();
        })
        .join(" · ");
    };

    $scope.customerLabel = function (c) {
      var name = (c.CustomerName || "").trim();
      var code = (c.CustomerCode || "").trim();
      return name || code || "";
    };

    $scope.filterCustomers = function () {
      var q = norm($scope.cb.query).trim();
      var out = [];
      var list = $scope.customers || [];
      for (var i = 0; i < list.length && out.length < 50; i++) {
        var c = list[i];
        if (
          !q ||
          norm(c.CustomerName).indexOf(q) >= 0 ||
          norm(c.CustomerCode).indexOf(q) >= 0 ||
          norm(c.CustomerMobile).indexOf(q) >= 0
        ) {
          out.push(c);
        }
      }
      $scope.cb.results = out;
      $scope.cb.index = 0;
    };

    $scope.openCustomerList = function () {
      positionCustomerList();
      $scope.cb.open = true;
      if (!$scope.cb.results.length) $scope.filterCustomers();
    };

    // user typed → selection no longer valid until they pick again
    $scope.onCustomerInput = function () {
      $scope.newAlert.CustomerID = null;
      positionCustomerList();
      $scope.cb.open = true;
      $scope.filterCustomers();
    };

    $scope.selectCustomer = function (c) {
      if (!c) return;
      $scope.newAlert.CustomerID = c.CustomerID;
      $scope.cb.query = $scope.customerLabel(c);
      $scope.cb.open = false;
    };

    $scope.clearCustomer = function () {
      $scope.newAlert.CustomerID = null;
      $scope.cb.query = "";
      $scope.filterCustomers();
      $scope.cb.open = true;
      positionCustomerList();
    };

    $scope.onCustomerKey = function (e) {
      var cb = $scope.cb;
      switch (e.keyCode) {
        case 40: // down
          e.preventDefault();
          if (!cb.open) {
            $scope.openCustomerList();
          } else {
            cb.index = Math.min(cb.index + 1, cb.results.length - 1);
            scrollActiveOption();
          }
          break;
        case 38: // up
          e.preventDefault();
          cb.index = Math.max(cb.index - 1, 0);
          scrollActiveOption();
          break;
        case 13: // enter
          if (cb.open) {
            e.preventDefault();
            $scope.selectCustomer(cb.results[cb.index]);
          }
          break;
        case 27: // escape closes the list first, not the modal
          if (cb.open) {
            e.stopPropagation();
            cb.open = false;
          }
          break;
        case 9: // tab
          cb.open = false;
          break;
      }
    };

    // Close modal on Escape
    function onKeyDown(e) {
      if (e.keyCode === 27 && $scope.showAddModal) {
        $scope.$apply($scope.closeAddModal);
      }
    }
    angular.element(document).on("keydown", onKeyDown);

    // ── Template helpers ──────────────────────────────────────────────────────
    $scope.typeLabel = function (a) {
      if (a.IsUrgent) return "Urgent";
      var map = {
        customer: "Customer",
        ticket: "Ticket",
        system: "System",
        finance: "Finance",
      };
      return map[a.AlertType] || "Alert";
    };

    $scope.typeBadgeClass = function (a) {
      if (a.IsUrgent) return "b-urgent";
      var map = {
        customer: "b-customer",
        ticket: "b-ticket",
        system: "b-system",
        finance: "b-finance",
      };
      return map[a.AlertType] || "b-customer";
    };

    $scope.accentClass = function (a) {
      if (a.IsUrgent) return "ac-urgent";
      var map = {
        customer: "ac-customer",
        ticket: "ac-ticket",
        system: "ac-system",
        finance: "ac-finance",
      };
      return map[a.AlertType] || "ac-customer";
    };

    $scope.avatarClass = function (a) {
      var map = {
        customer: "av-blue",
        ticket: "av-green",
        system: "av-amber",
        finance: "av-pink",
      };
      return map[a.AlertType] || "av-blue";
    };

    // Falls back to customer code when the name is empty (no more "??")
    $scope.avatarInitials = function (name, code) {
      var source = (name && name.trim()) || (code && code.trim()) || "";
      if (!source) return "??";
      var parts = source.split(/\s+/);
      return parts.length >= 2
        ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
        : source.substring(0, 2).toUpperCase();
    };

    $scope.relativeTime = function (dateStr) {
      if (!dateStr) return "";
      var diff = Date.now() - new Date(dateStr).getTime();
      var mins = Math.floor(diff / 60000);
      var hours = Math.floor(diff / 3600000);
      var days = Math.floor(diff / 86400000);
      if (mins < 1) return "Just now";
      if (mins < 60) return mins + " min ago";
      if (hours < 24) return hours + " hr ago";
      if (days < 7) return days + (days === 1 ? " day ago" : " days ago");
      return new Date(dateStr).toLocaleDateString();
    };

    $scope.toggleComment = function (item, $event) {
      $event.stopPropagation();

      if (!item.NewComment) {
        item.NewComment = item.PostComment || "";
      }
      item.showComment = !item.showComment;
    };

    $scope.saveComment = function (item) {
      const newComment = item.NewComment;
      const userName = localStorage.getItem("User_Full_Name");

      if (!newComment?.trim()) return;

      item.isSending = true;

      $http
        .post(
          apiBaseUrl + "AlertMessages/SaveComment",
          {
            id: item.AlertID,
            comment: newComment,
            commentBy: userName,
          },
          { ignoreLoadingBar: true },
        )
        .then(function () {
          item.PostComment = newComment;
          item.PostCommentBy = userName;
          item.NewComment = "";
          item.showComment = false;
        })
        .finally(function () {
          item.isSending = false;
        });
    };

    var _originalReceiveAlertCustomer = chatHub.client.ReceiveAlertCustomer;

    chatHub.client.ReceiveAlertCustomer = function (
      ReferenceID,
      AlertID,
      CustomerID,
      CustomerName,
      CustomerCode,
      CustomerMobile,
      ExpireDate,
      Comment,
      Reason,
      SentBy,
    ) {
      $scope.$apply(function () {
        var existingAlert = $scope.alerts.find(function (x) {
          return x.CustomerID == CustomerID;
        });

        if (existingAlert) {
          existingAlert.AlertID = AlertID;
          existingAlert.CustomerName = CustomerName;
          existingAlert.CustomerCode = CustomerCode;
          existingAlert.CustomerMobile = CustomerMobile;
          existingAlert.ExpireDate = ExpireDate;
          existingAlert.Comment = Comment || "";
          existingAlert.Reason = Reason || "";
          existingAlert.SentBy = SentBy || "System";
          existingAlert.CreatedAt = new Date().toISOString();
          existingAlert.IsRead = false;
          existingAlert._isNew = true;

          $scope.alerts = $scope.alerts.filter(function (x) {
            return x.CustomerID != CustomerID;
          });
          $scope.alerts.unshift(existingAlert);

          $timeout(function () {
            existingAlert._isNew = false;
          }, 500);
        } else {
          var incomingAlert = {
            AlertID: AlertID,
            AlertType: "customer",
            CustomerID: CustomerID,
            CustomerName: CustomerName,
            CustomerCode: CustomerCode,
            CustomerMobile: CustomerMobile,
            ExpireDate: ExpireDate,
            Comment: Comment || "",
            Reason: Reason || "",
            SentBy: SentBy || "System",
            IsRead: false,
            IsDismissed: false,
            IsUrgent: false,
            CreatedAt: new Date().toISOString(),
            _isNew: true,
          };

          $scope.alerts.unshift(incomingAlert);
          $timeout(function () {
            incomingAlert._isNew = false;
          }, 500);
        }

        $scope.applyFilter();
        playMp3();
      });

      if (typeof _originalReceiveAlertCustomer === "function") {
        _originalReceiveAlertCustomer(
          ReferenceID,
          AlertID,
          CustomerID,
          CustomerName,
          CustomerCode,
          CustomerMobile,
          ExpireDate,
          Comment,
          Reason,
          SentBy,
        );
      }
    };

    // Restore original handler + remove key listener
    $scope.$on("$destroy", function () {
      chatHub.client.ReceiveAlertCustomer = _originalReceiveAlertCustomer;
      angular.element(document).off("keydown", onKeyDown);
    });

    // ── Init ──────────────────────────────────────────────────────────────────
    $scope.loadAlerts(true);
  },
]);
