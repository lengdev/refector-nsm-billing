"use strict";
app
  .controller("TemplateToggleCtrl", [
    "$scope",
    function ($scope) {
      $scope.template = { isNew: true };

      var tokenInfo = JSON.parse(getStorage("TokenInfo"));
      var userLoginId = getStorage("User_ID");
      var domainId = getStorage("DomainID");

      // $scope.showTemplateSwitch =
      //     tokenInfo &&
      //     [167].includes(Number(userLoginId)) &&
      //     domainId == "001";

      $scope.showTemplateSwitch = false;

      const sidebarKey = "sidebarCollapsed";
      const isCollapsed = sessionStorage.getItem(sidebarKey) !== "false";

      if (isCollapsed) {
        $("body").addClass("sidebar-collapse");

        // Manually apply collapsed icon/image styles on load
        $("#menuIcon").attr("class", "ti ti-chevrons-right");
        $(".brand-image")[0].style.setProperty("width", "34px", "important");
        $(".brand-image").css({
          height: "34px",
          "object-fit": "contain",
        });
      }

      if (sessionStorage.getItem(sidebarKey) === null) {
        sessionStorage.setItem(sidebarKey, "true");
      }

      $("[data-widget=pushmenu]").on("click", function () {
        const willBeOpen = $("body").hasClass("sidebar-collapse");
        sessionStorage.setItem(sidebarKey, willBeOpen ? "false" : "true");
      });
    },
  ])
  .directive("currentTime", [
    "$interval",
    "dateFilter",
    function ($interval, dateFilter) {
      // return the directive link function. (compile function not needed)
      return function (scope, element, attrs) {
        var format, // date format
          stopTime; // so that we can cancel the time updates

        // used to update the UI
        function updateTime() {
          element.text(dateFilter(new Date(), format));
        }

        // watch the expression, and update the UI on change.
        scope.$watch(attrs.currentTime, function (value) {
          format = value;
          updateTime();
        });

        stopTime = $interval(updateTime, 1000);

        // listen on DOM destroy (removal) event, and cancel the next UI update
        // to prevent updating time after the DOM element was removed.
        element.on("$destroy", function () {
          $interval.cancel(stopTime);
        });
      };
    },
  ]);

app.controller("CustomerProfileCtrl", [
  "$rootScope",
  "$scope",
  "Form",
  "$location",
  function ($rootScope, $scope, Form, $location) {
    $rootScope.title = "Customer Profile";
    let ID = $location.search().ID;
    let ISSO =
      typeof $location.search().ISSO == "undefined"
        ? 0
        : $location.search().ISSO;
    let InvoiceNo =
      typeof $location.search().InvoiceNo == "undefined"
        ? ""
        : $location.search().InvoiceNo;
    let ISPaidInvoice =
      typeof $location.search().ISPaidInvoice == "undefined"
        ? false
        : $location.search().ISPaidInvoice;

    // Plain function (hoisted) — callable internally without $scope,
    // and exposed on $scope below so the HTML template can call it too.
    function LoadData() {
      Form.LoadHtmlData(
        "CustomerProfile/Index?ID=" +
          ID +
          "&ISSO=" +
          ISSO +
          "&ISPaidInvoice=" +
          ISPaidInvoice +
          "&InvoiceNo=" +
          InvoiceNo,
        function (result) {
          $scope.HtmlData = result;
        },
        true,
      );
    }

    $scope.LoadData = LoadData;

    LoadData();
  },
]);

app.controller("CustomerProfileV2Ctrl", [
  "$rootScope",
  "$scope",
  "$http",
  "$location",
  "$q",
  "$sce",
  "$timeout",
  "$element",
  "$filter",
  "$httpParamSerializer",
  "AgreementFormService",
  function (
    $rootScope,
    $scope,
    $http,
    $location,
    $q,
    $sce,
    $timeout,
    $element,
    $filter,
    $httpParamSerializer,
    AgreementFormService,
  ) {
    $rootScope.title = "Customer Profile";

    $scope.profile = null;
    $scope.balance = null;
    $scope.connections = [];
    $scope.unpaidList = [];
    $scope.paidList = [];
    $scope.quotations = [];
    $scope.proformaInvoices = [];
    $scope.totalTransactions = 0;

    const ID = $location.search().ID;
    let customerName = "";

    $scope.activeTab = "invoice";
    $scope.activeSubTab = "inv-transaction";
    $scope.activeSvcTab = "services";
    $scope.activeQuoteTab = "quoteList";
    $scope.activeFileTab = "id";

    $scope.loading = {
      profile: true,
      balance: true,
      connections: true,
      unpaid: true,
      paid: false,
      quotation: false,
      pi: false,
      transaction: false,
      attachment: false,
      files: false,
      deposit: false,
      products: false,
      devices: false,
      billInfo: false,
    };

    $scope.showPassword = {};
    $scope.showWifiKey = {};
    $scope.showWifiLoginPwd = {};

    $scope.sidOpen = false;

    $scope.svcIdx = "0";

    $scope.uploadModal = {
      open: false,
      typeFor: null,
      files: [],
      uploading: false,
    };

    $scope.devicesByConn = [];
    $scope.expandedConn = {};

    $scope.billInformation = [];
    $scope.expandedBill = {};
    $scope.editingBill = {};
    $scope.billBackup = {};

    $scope.expandedQuote = {};
    $scope.editingQuote = {};
    $scope.quoteBackup = {};
    $scope.savingQuote = {};

    $scope.mapCoord = {};
    $scope.mapLoading = true;

    // Per-table search
    $scope.txSearch = "";
    $scope.piSearch = "";
    $scope.quoSearch = "";
    $scope.unpaidSearch = "";
    $scope.paidSearch = "";
    $scope.deviceSearch = "";
    $scope.ticketSearch = "";

    $scope.isHidden = localStorage.getItem("DomainID") === "001,002";

    const DELAY = 10;

    function api(url, loadingKey) {
      if (loadingKey) $scope.loading[loadingKey] = true;
      return $http
        .get(url, { ignoreLoadingBar: true })
        .then((r) => r.data)
        .then((data) => (DELAY ? $timeout(() => data, DELAY) : data))
        .finally(() => {
          if (loadingKey) $scope.loading[loadingKey] = false;
        });
    }

    var customers = [];
    var customersPromise = null;

    function loadCustomers() {
      if (!customersPromise) {
        customersPromise = api(`${ServerRoot}api/Customer/GetCustomerLists`, {
          cache: true,
        }).then((data) => {
          customers = data || [];
          return customers;
        });
      }
      return customersPromise;
    }

    loadCustomers().then(function () {
      ApplyTypeahead(
        "quick-search",
        customers,
        { CustomerID: "CustomerID" },
        {
          Name: "CustomerName",
        },
      );

      $("#quick-search").on("typeahead:select", function (e, item) {
        $scope.$apply(function () {
          const customerId = Number($element.find("#CustomerID").val());

          $location.path("/CustomerProfile").search({ ID: customerId });
        });
      });
    });

    function stripHtml(html) {
      return $("<div>")
        .html(html || "")
        .text();
    }

    function setTab(tab) {
      $scope.activeTab = tab;
      if (tab === "invoice" && !$scope.unpaidLoaded) {
        loadUnpaid();
      }

      if (tab === "quotation" && !$scope.quotationLoaded) {
        loadQuotation();
      }

      if (tab === "device" && !$scope.devicesLoaded) {
        loadDevices();
      }

      if (tab === "billing" && !$scope.billLoaded) {
        loadBillInfo();
      }

      if (
        (tab === "files" || tab === "serviceform") &&
        !$scope.idSserviceFormLoaded
      ) {
        loadIDServiceForm();
      }

      if (tab === "map") {
        $scope.mapLoading = true;
        if (!$scope.billInfoLoaded) {
          loadBillInfo().then(function () {
            buildMapCoord();
          });
        } else {
          buildMapCoord();
        }
      }

      // When click on hero-right
      $timeout(function () {
        var target = document.querySelector("#invoice-tabs");
        if (target) {
          target.scrollIntoView({
            behavior: "smooth",
            block: "start",
          });
        }
      }, 100);
    }

    function setSubTab(sub) {
      $scope.activeSubTab = sub;

      $timeout(function () {
        if (sub === "inv-paid" && !$scope.paidLoaded) loadPaid();
        if (sub === "inv-pi" && !$scope.piLoaded) loadProformaInvoice();
        if (sub === "inv-transaction" && !$scope.transactionLoaded)
          loadTransaction();
        if (sub === "inv-attachment" && !$scope.attachmentLoaded)
          loadAttachment();
        if (sub === "inv-deposit" && !$scope.depositLoaded) loadDeposit();
      }, 120);
    }

    function setQuoteTab(sub) {
      $scope.activeQuoteTab = sub;
    }

    function setSvcTab(sub) {
      $scope.activeSvcTab = sub;

      if (sub === "products" && !$scope.productsLoaded) {
        loadProduct();
      }
    }

    function setFileTab(t) {
      $scope.activeFileTab = t;
    }

    function goSvc(n) {
      const max = Math.max(0, $scope.connections.length - 1);
      $scope.svcIdx = String(Math.max(0, Math.min(max, Math.round(+n))));
    }

    $scope.$watch("connLoading", function (loading) {
      if (
        loading ||
        !$scope.connections.length ||
        $scope.connections.length <= 1
      )
        return;
      $timeout(function () {
        var el = document.querySelector("#all-connection .svc-slider");
        if (!el || el._swipeAttached) return;
        el._swipeAttached = true;

        var startX = 0,
          startY = 0;

        el.addEventListener(
          "touchstart",
          function (e) {
            startX = e.touches[0].clientX;
            startY = e.touches[0].clientY;
          },
          { passive: true },
        );

        el.addEventListener(
          "touchend",
          function (e) {
            var dx = startX - e.changedTouches[0].clientX;
            var dy = startY - e.changedTouches[0].clientY;
            if (Math.abs(dx) < 40 || Math.abs(dx) < Math.abs(dy)) return;
            $scope.$apply(function () {
              goSvc(+$scope.svcIdx + (dx > 0 ? 1 : -1));
            });
          },
          { passive: true },
        );

        // Touchpad two-finger swipe (wheel event)
        var wheelAccumX = 0;
        var wheelAccumY = 0;
        var wheelCooldown = false;
        var wheelFired = false;
        var wheelIdleTimer = null;
        var isHovering = false;

        el.addEventListener("mouseenter", function () {
          isHovering = true;
        });
        el.addEventListener("mouseleave", function () {
          isHovering = false;
          wheelAccumX = 0;
          wheelAccumY = 0;
          wheelFired = false;
          clearTimeout(wheelIdleTimer);
        });

        function onWheel(e) {
          if (!isHovering) return;

          // if clearly vertical — let page scroll, don't touch it
          if (Math.abs(e.deltaY) > Math.abs(e.deltaX)) return;

          // horizontal gesture inside carousel — ALWAYS block browser navigation
          e.preventDefault();
          e.stopPropagation();

          clearTimeout(wheelIdleTimer);
          wheelIdleTimer = setTimeout(function () {
            wheelAccumX = 0;
            wheelAccumY = 0;
            wheelFired = false;
          }, 150);

          if (wheelFired) return;

          wheelAccumX += e.deltaX;

          if (Math.abs(wheelAccumX) > 30) {
            var dir = wheelAccumX > 0 ? 1 : -1;
            var currentIdx = +$scope.svcIdx;
            var maxIdx = $scope.connections.length - 1;
            var canMove =
              (dir === 1 && currentIdx < maxIdx) ||
              (dir === -1 && currentIdx > 0);

            if (canMove) {
              wheelFired = true;
              $scope.$apply(function () {
                goSvc(currentIdx + dir);
              });
            }
          }
        }

        window.addEventListener("wheel", onWheel, { passive: false });

        $scope.$on("$destroy", function () {
          window.removeEventListener("wheel", onWheel);
        });
      }, 0);
    });

    $(document).on("click.sidDrop", function (e) {
      if (!$(e.target).closest('[ng-init="sidOpen=false"]').length) {
        $scope.$apply(function () {
          $scope.sidOpen = false;
        });
      }
    });

    function copyText(txt, event) {
      // must be captured synchronously, currentTarget is null inside promises
      var el = event && event.currentTarget;
      if (!el || !txt) return;

      function done() {
        showCopied(el);
        $scope.copied = true;
        $timeout(function () {
          $scope.copied = false;
        }, 1500);
      }

      if (navigator.clipboard && window.isSecureContext) {
        navigator.clipboard.writeText(txt).then(function () {
          $scope.$applyAsync(done);
        });
      } else {
        // fallback for non-HTTPS
        var ta = document.createElement("textarea");
        ta.value = txt;
        ta.style.position = "fixed";
        ta.style.opacity = "0";
        document.body.appendChild(ta);
        ta.select();
        try {
          document.execCommand("copy");
          done();
        } catch (e) {}
        document.body.removeChild(ta);
      }
    }

    function showCopied(el) {
      // remove an existing bubble on this element
      var old = el.querySelector(":scope > .copy-bubble");
      if (old) old.remove();

      // the bubble is absolute, so the element must be a positioning context
      if (getComputedStyle(el).position === "static") {
        el.style.position = "relative";
      }

      var bubble = document.createElement("span");
      bubble.className = "copy-bubble";
      bubble.textContent = "Copied!";
      el.appendChild(bubble);

      setTimeout(function () {
        bubble.remove();
      }, 1500);
    }

    function loadProfile() {
      return api(`api/Customer/Profile?CustomerID=${ID}`, "profile").then(
        (data) => {
          $scope.profile = data[0] || null;
          $scope.shortName = getShortName($scope.profile?.CustomerName);
          customerName = $scope.profile?.CustomerName;
        },
      );
    }

    function loadBalance() {
      $scope.loading.balance = true;
      return api(`api/Customer/CustomerTotalBalance?CustomerID=${ID}`)
        .then((data) => {
          $scope.balance = data[0] || null;
        })
        .finally(() => ($scope.loading.balance = false));
    }

    function getStatusBadge(status) {
      var badges = {
        40: '<span class="badge badge-green">Active</span>',
        41: '<span class="badge badge-yellow">Locked</span>',
        42: '<span class="badge badge-red">Terminated</span>',
      };
      return $sce.trustAsHtml(
        badges[status] || '<span class="badge badge-gray">Inactive</span>',
      );
    }

    function buildDisplayCycle(conn) {
      const DAY = 86400000;
      const now = new Date();
      const ended = (c) => new Date(c.EndDate) < now;
      const started = (c) => new Date(c.StartDate) <= now;

      let running = conn;
      let queued = null;

      if (conn.prevCycle) {
        const prevEnded = ended(conn.prevCycle);
        // Normal case: old cycle still running, newest invoice is queued behind it.
        // If the old cycle already ended AND the newest invoice has started,
        // the newest invoice is the one running now.
        if (prevEnded && started(conn) && !ended(conn)) {
          running = conn;
        } else {
          running = conn.prevCycle;
          queued = conn;
        }
      }

      const start = new Date(running.StartDate);
      const end = new Date(running.EndDate);
      const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
      const startDay = new Date(
        start.getFullYear(),
        start.getMonth(),
        start.getDate(),
      );
      const endDay = new Date(end.getFullYear(), end.getMonth(), end.getDate());
      const totalDays = Math.max(1, Math.ceil((end - start) / DAY));
      const rawLeft = Math.ceil((endDay - today) / DAY);
      const isUpcoming = startDay > today;
      const isExpired = endDay <= today;

      const state = isUpcoming ? "upcoming" : isExpired ? "expired" : "active";
      const daysLeft = Math.max(0, rawLeft);
      const daysUsed =
        state === "upcoming" ? 0 : Math.max(0, totalDays - daysLeft);
      const percent =
        state === "expired"
          ? 100
          : Math.min(100, Math.max(0, (daysUsed / totalDays) * 100));

      conn.cycle = {
        state,
        invoiceNo: running.InvoiceNo,
        packageName: running.PackageName,
        bandwidth: running.Bandwidth,
        upSpeed: running.UpSpeed,
        dowSpeed: running.DowSpeed,
        um: running.UM,
        startDate: running.StartDate,
        endDate: running.EndDate,
        renewDate: running.RenewDate,
        invoiceType: running.InvoiceType,
        daysUsed,
        daysLeft,
        expiredDays: state === "expired" ? Math.abs(rawLeft) : 0,
        daysUntilStart:
          state === "upcoming" ? Math.ceil((start - now) / DAY) : 0,
        percent,
      };

      conn.queued = queued
        ? {
            invoiceNo: queued.InvoiceNo,
            isChangePlan: !!queued.IsChangePlan,
            packageName: queued.PackageName,
            bandwidth: queued.Bandwidth,
            um: queued.UM,
            startDate: queued.StartDate,
            endDate: queued.EndDate,
          }
        : null;

      // Keep the fields other parts of the page already read consistent
      const oneMonthAgo = new Date();
      oneMonthAgo.setMonth(oneMonthAgo.getMonth() - 1);
      conn.isExpired = state === "expired";
      conn.isUpcoming = state === "upcoming";
      conn.expiryDays = conn.cycle.expiredDays;
      conn.daysUntilStart = conn.cycle.daysUntilStart;
      conn.isTerminated = end < oneMonthAgo && conn.ClassName === "NSM";

      return conn;
    }

    async function loadConnections() {
      $scope.connLoading = true;
      $scope.routerActiveCount = 0;
      $scope.routerDisableCount = 0;

      try {
        const raw = await api(
          `api/Customer/AllConnection?CustomerID=${ID}`,
          "connections",
        );

        if (!raw?.length) {
          $scope.connections = [];
          $scope.$applyAsync();
          return;
        }

        const grouped = Object.groupBy(raw, (row) => row.SID);

        const bySid = Object.entries(grouped).map(([sid, rows]) => {
          const sorted = [...rows].sort((a, b) => a.rn - b.rn);
          const primary = sorted.find((r) => r.rn === 1) || sorted[0];
          const prevCycle = sorted.find((r) => r.rn === 2) || null;
          return { ...primary, prevCycle };
        });

        const enriched = await Promise.all(
          bySid.map(async (conn) => {
            let router = { success: false, disabled: null };
            try {
              router =
                conn.ConnectionType === "PPPoE"
                  ? await api(
                      `api/mikrotik/pppoe/users/status?routerId=${conn.RouterID}&username=${encodeURIComponent(conn.PPPoEUserName)}`,
                    )
                  : await api(
                      `api/mikrotik/vlans/status?routerId=${conn.RouterID}&vlanId=${conn.VLanID}&vlanName=${encodeURIComponent(conn.VLanName)}`,
                    );
            } catch (_) {}

            $scope.routerActiveCount += router.disabled === "false" ? 1 : 0;
            $scope.routerDisableCount += router.disabled === "true" ? 1 : 0;

            const isActive = conn.ConnectivityStatus === 40;
            const isSuspended =
              conn.ConnectivityStatus === 41 || conn.ConnectivityStatus == 42;

            const isMismatch =
              (isActive && router.disabled === "true") ||
              (isSuspended && router.disabled === "false");

            const prevCycle = conn.prevCycle
              ? {
                  ...conn.prevCycle,
                  dayLeft: Math.max(
                    0,
                    Math.ceil(
                      (new Date(conn.prevCycle.EndDate) - new Date()) /
                        86400000,
                    ),
                  ),
                  totalCycleDays: Math.max(
                    1,
                    Math.ceil(
                      (new Date(conn.prevCycle.EndDate) -
                        new Date(conn.prevCycle.StartDate)) /
                        86400000,
                    ),
                  ),
                }
              : null;

            const now = new Date();
            const startDate = new Date(conn.StartDate);
            const isUpcoming = startDate > now;
            var today = new Date();
            var oneMonthAgo = today.setMonth(today.getMonth() - 1);

            return {
              ...conn,
              router,
              isMismatch,
              syncStatusId: isActive && router.disabled === "true" ? 41 : 40,
              isExpired: new Date(conn.EndDate) < new Date(),
              isTerminated:
                new Date(conn.EndDate) < oneMonthAgo &&
                conn.ClassName === "NSM",
              isUpcoming,
              daysUntilStart: isUpcoming
                ? Math.ceil((startDate - now) / 86400000)
                : 0,
              expiryDays: Math.abs(
                Math.ceil((new Date(conn.EndDate) - new Date()) / 86400000),
              ),
              statusText: getStatusText(conn.ConnectivityStatus),
              prevCycle,
            };
          }),
        );

        enriched.forEach(buildDisplayCycle);
        //console.log(enriched);

        $scope.connections = enriched;
        $scope.$applyAsync();

        const first = enriched.find((c) => c.ConnectionType === "PPPoE");
        if (first) trackOnline(first.RouterID, first.PPPoEUserName);
      } catch (err) {
        console.error("loadConnections error", err);
      } finally {
        $scope.connLoading = false;
        $scope.$applyAsync();
      }
    }

    function refreshConnections() {
      if ($scope.connLoading) return;
      loadConnections();
    }

    $scope.refreshingAll = false;

    function refreshCustomer() {
      if ($scope.refreshingAll) return;
      $scope.refreshingAll = true;

      // Reset all lazy flags so every tab re-fetches fresh on next visit
      $scope.unpaidLoaded =
        $scope.paidLoaded =
        $scope.quotationLoaded =
        $scope.piLoaded =
        $scope.transactionLoaded =
        $scope.attachmentLoaded =
        $scope.depositLoaded =
        $scope.productsLoaded =
        $scope.devicesLoaded =
        $scope.billLoaded =
        $scope.billInfoLoaded =
        $scope.idSserviceFormLoaded =
          false;

      // Re-run the current tab (reuses existing lazy-load logic)
      setTab($scope.activeTab);
      setSubTab($scope.activeSubTab);

      // Profile + balance + connections always refresh
      $q.all([loadProfile(), loadBalance()]).finally(function () {
        $scope.refreshingAll = false;
        toastr.success("Customer data refreshed.");
      });

      // Connections is async — run separately, not in $q.all
      loadConnections();
    }

    function trackOnline(routerId, username) {
      api(
        `api/mikrotik/sessions/history?routerId=${routerId}&username=${encodeURIComponent(username)}`,
      ).then((res) => {
        $scope.isOnline = res?.data?.active === "true";
      });
    }

    function syncMismatch(conn) {
      conn.syncing = true;
      $http
        .post("api/Customer/UpdateStatus", {
          CustomerID: ID,
          ActivateCustomerID: conn.ActivateCustomerID,
          EndDate: conn.EndDate,
          StatusID: conn.syncStatusId,
          RouterID: conn.RouterID,
          PPPoEUserName: conn.PPPoEUserName || "",
          VLanName: conn.VLanName || "",
        })
        .then(() => {
          conn.isMismatch = false;
          conn.ConnectivityStatus = conn.syncStatusId;
          conn.statusText = getStatusText(conn.syncStatusId);
          DialogSweetAlert("success", "Done Sync 🎉");
        })
        .catch(() => DialogSweetAlert("error", "Sync failed"))
        .finally(() => (conn.syncing = false));
    }

    function loadUnpaid() {
      return api(`api/Customer/Unpaid?CustomerID=${ID}`, "unpaid").then(
        (data) => {
          $scope.unpaidLoaded = true;
          const today = new Date();
          today.setHours(0, 0, 0, 0);

          let overdueCount = 0,
            overdueTotal = 0,
            latestDue = null;
          let totalAmount = 0,
            totalUnpaid = 0,
            totalPaid = 0;

          $scope.unpaidList = (data || []).map((inv, i) => {
            const due = new Date(inv.PaymentDueDate);
            due.setHours(0, 0, 0, 0);
            let status = "Unpaid",
              badge = "badge-red";

            if (inv.Unpaid <= 0) {
              status = "Paid";
              badge = "badge-green";
            } else if (due < today) {
              status = "Overdue";
              badge = "badge-red";
              overdueCount++;
              overdueTotal += inv.Unpaid;
              if (!latestDue || due > latestDue) latestDue = due;
            } else if (due.getTime() === today.getTime()) {
              status = "Due Today";
              badge = "badge-warning";
            } else if (inv.Paid > 0) {
              status = "Partial";
              badge = "badge-blue";
            }

            totalAmount += inv.GrandTotal;
            totalUnpaid += inv.Unpaid;
            totalPaid += inv.Paid;

            return {
              ...inv,
              status,
              badge,
            };
          });

          $scope.overdueCount = overdueCount;
          $scope.overdueTotal = overdueTotal;
          $scope.latestDue = latestDue;
          $scope.unpaidCount = (data || []).length;
          $scope.totalAmount = totalAmount;
          $scope.totalUnpaid = totalUnpaid;
          $scope.amtPaid = totalPaid;

          $scope.activeSubTab =
            $scope.unpaidList.length > 0 ? "inv-unpaid" : "inv-transaction";
        },
      );
    }

    function loadPaid() {
      return api(`api/Customer/Paid?CustomerID=${ID}`, "paid").then((data) => {
        $scope.paidLoaded = true;
        let totalPaid = 0;
        $scope.paidList = (data || []).map((inv, i) => {
          totalPaid += inv.Paid;
          return {
            ...inv,
          };
        });
        $scope.totalPaid = totalPaid;
        $scope.paidCount = (data || []).length;
      });
    }

    function loadQuotation() {
      return api(`api/Customer/Quotation?CustomerID=${ID}`, "quotation").then(
        (data) => {
          $scope.quotationLoaded = true;
          $scope.quotations = (data || []).map((inv, i) => ({
            ...inv,
          }));
        },
      );
    }

    function loadProformaInvoice() {
      return api(`api/Customer/PI?CustomerID=${ID}`, "pi").then((data) => {
        $scope.piLoaded = true;
        $scope.proformaInvoices = (data || []).map((inv, i) => ({
          ...inv,
        }));
      });
    }

    function loadTransaction() {
      return api(
        `api/Customer/Transactions?CustomerID=${ID}`,
        "transaction",
      ).then((data) => {
        $scope.transactionLoaded = true;
        $scope.transactions = (data || []).map((inv, i) => ({
          ...inv,
        }));

        $scope.totalTransactions = (data || []).length;
      });
    }

    function loadAttachment() {
      return api(
        `api/Customer/AttachmentFile?CustomerID=${ID}`,
        "attachment",
      ).then((data) => {
        $scope.attachmentLoaded = true;
        $scope.attachments = (data || []).map((file, i) => ({
          ...file,
        }));
      });
    }

    function loadDeposit() {
      return api(`api/Customer/Deposit?CustomerID=${ID}`, "deposit").then(
        (data) => {
          $scope.depositLoaded = true;
          $scope.deposits = (data || []).map((inv, i) => ({
            ...inv,
          }));
        },
      );
    }

    function loadProduct() {
      return api(
        `api/Customer/CustomerProduct?CustomerID=${ID}`,
        "products",
      ).then((data) => {
        $scope.productsLoaded = true;
        $scope.products = (data || []).map((prod, i) => ({
          ...prod,
        }));
      });
    }

    function isImage(fileName) {
      if (!fileName) return false;
      const ext = fileName.split(".").pop().toLowerCase();
      return ["jpg", "jpeg", "png", "gif", "webp"].includes(ext);
    }

    function isPdf(fileName) {
      if (!fileName) return false;
      return fileName.split(".").pop().toLowerCase() === "pdf";
    }

    function loadIDServiceForm() {
      return api(`api/Customer/IDServiceForm?CustomerID=${ID}`, "files").then(
        (data) => {
          $scope.idSserviceFormLoaded = true;

          $scope.files = (data || []).map((value) => {
            const destinationFolder = (value.DestinationFolder || "").replace(
              /\/$/,
              "",
            );

            const fileNames = (value.AttachmentFileName || "")
              .split("~")
              .filter(Boolean);

            return {
              ...value,
              DestinationFolder: destinationFolder,
              AttachmentFileName: fileNames,
            };
          });

          $scope.idFiles = $scope.files
            .filter((f) => f.TypeFor === "Attachment File")
            .flatMap((file) =>
              file.AttachmentFileName.map((attName) => ({
                ...file,
                AttachmentFileName: attName,
              })),
            );
          $scope.bankFiles = $scope.files
            .filter((f) => f.TypeFor === "BankSlip")
            .flatMap((file) =>
              file.AttachmentFileName.map((attName) => ({
                ...file,
                AttachmentFileName: attName,
              })),
            );
          $scope.serviceForms = $scope.files
            .filter((f) => f.TypeFor === "ServiceForm")
            .flatMap((file) =>
              file.AttachmentFileName.map((attName) => ({
                ...file,
                AttachmentFileName: attName,
              })),
            );
        },
      );
    }

    const typeForMap = {
      id: "Attachment File",
      bank: "BankSlip",
      serviceform: "ServiceForm",
    };

    function openUploadModal(tabKey) {
      $scope.uploadModal.open = true;
      $scope.uploadModal.typeFor = typeForMap[tabKey] || tabKey;
      $scope.uploadModal.files = [];
    }

    function closeUploadModal() {
      $scope.uploadModal.open = false;
      $scope.uploadModal.files = [];
    }

    function onFilesSelected(fileList) {
      if (!fileList) return;
      $scope.uploadModal.files = Array.from(fileList).map((file) => ({
        file: file,
        name: file.name,
        progress: 0,
        status: "pending", // pending | uploading | done | error
      }));
    }

    function removeSelectedFile(index) {
      $scope.uploadModal.files.splice(index, 1);
    }

    function submitUpload() {
      if (!$scope.uploadModal.files.length) return;
      $scope.uploadModal.uploading = true;

      const formData = new FormData();
      $scope.uploadModal.files.forEach((entry) => {
        formData.append("file", entry.file);
        entry.status = "uploading";
      });

      formData.append(
        "Data",
        JSON.stringify({
          CustomerID: ID,
          CustomerName:
            $scope.profile?.CustomerName ||
            $scope.profile?.CustomerNameLatin ||
            "",
          CustomerNameLatin: $scope.profile?.CustomerNameLatin || "",
          CustomerCode: $scope.profile?.CustomerCode || "",
          TypeFor: $scope.uploadModal.typeFor,
        }),
      );

      $http
        .post("CustomerProfile/SaveChangeCustomerUploadFile", formData, {
          transformRequest: angular.identity,
          headers: { "Content-Type": undefined },
          uploadEventHandlers: {
            progress: function (e) {
              if (e.lengthComputable) {
                const percent = Math.round((e.loaded / e.total) * 100);
                $scope.$applyAsync(function () {
                  $scope.uploadModal.files.forEach((entry) => {
                    entry.progress = percent;
                  });
                });
              }
            },
          },
        })
        .then(function (res) {
          if (res.data?.Result !== 2 && res.data?.Result !== "Successful") {
            throw new Error(res.data?.Message || "Upload failed");
          }
          $scope.uploadModal.files.forEach((entry) => {
            entry.status = "done";
          });
        })
        .catch(function (err) {
          console.error("Upload failed", err);
          $scope.uploadModal.files.forEach((entry) => {
            entry.status = "error";
          });
        })
        .finally(function () {
          $scope.uploadModal.uploading = false;
          const anyFailed = $scope.uploadModal.files.some(
            (f) => f.status === "error",
          );
          if (!anyFailed) {
            closeUploadModal();
          }
          loadIDServiceForm();
        });
    }

    function triggerFilePicker() {
      document.getElementById("uploadFilePicker").click();
    }

    function loadDevices() {
      return api(`api/Customer/DeviceInfo?CustomerID=${ID}`, "devices").then(
        (data) => {
          $scope.devicesLoaded = true;

          const grouped = Object.values(
            data.reduce((groups, item) => {
              const key = item.ActivateCustomerID;

              if (!groups[key]) {
                groups[key] = {
                  ...item,
                  history: [],
                };
              }

              groups[key].history.push(item);

              return groups;
            }, {}),
          );
          $scope.devicesByConn = grouped;

          // Auto-expand first card
          if (grouped.length) {
            $scope.expandedConn[grouped[0].ConnectionID] = true;
          }
        },
      );
    }

    function loadBillInfo() {
      return api(
        `api/Customer/BillInformation?CustomerID=${ID}`,
        "billInfo",
      ).then((data) => {
        $scope.billInfoLoaded = true;
        const groups = {};
        (data || []).forEach((b) => {
          if (!groups[b.InvoiceNo]) {
            groups[b.InvoiceNo] = { ...b, history: [] };
          }
          groups[b.InvoiceNo].history.push(b);
        });
        $scope.billInformation = Object.values(groups);
      });
    }

    function toggleExpandBill($event, invoiceNo) {
      if (angular.element($event.target).closest("button").length) return;
      $scope.expandedBill[invoiceNo] = !$scope.expandedBill[invoiceNo];
    }

    function saveBillInfo(bill) {
      var payload = angular.copy(bill);
      delete payload.history;

      return $http
        .post("api/Customer/SaveBillInfo", payload, {
          ignoreLoadingBar: true,
        })
        .then(
          function (res) {
            toastr.success("Bill information saved successfully.");
            return res;
          },
          function (err) {
            toastr.error("Failed to save bill information. Please try again.");
            return $q.reject(err);
          },
        );
    }

    function toggleEditBill($event, bill) {
      $event.stopPropagation();
      if ($scope.editingBill[bill.InvoiceNo]) {
        saveBillInfo(bill).then(function () {
          $scope.editingBill[bill.InvoiceNo] = false;
          delete $scope.billBackup[bill.InvoiceNo];
        });
      } else {
        $scope.billBackup[bill.InvoiceNo] = angular.copy(bill);
        $scope.editingBill[bill.InvoiceNo] = true;
        $scope.expandedBill[bill.InvoiceNo] = true;
      }
    }

    function cancelEditBill($event, bill) {
      $event.stopPropagation();
      angular.extend(bill, $scope.billBackup[bill.InvoiceNo]);
      $scope.editingBill[bill.InvoiceNo] = false;
      delete $scope.billBackup[bill.InvoiceNo];
    }

    function toggleExpandQuote($event, invoiceNo) {
      if (angular.element($event.target).closest("button").length) return;
      $scope.expandedQuote[invoiceNo] = !$scope.expandedQuote[invoiceNo];
    }

    function saveQuoteInfo(quote) {
      var payload = angular.copy(quote);

      $scope.savingQuote[quote.InvoiceNo] = true;
      return $http
        .post("api/Customer/SaveBillInfo", payload, {
          ignoreLoadingBar: true,
        })
        .then(
          function (res) {
            toastr.success("Quotation saved successfully.");
            $scope.savingQuote[quote.InvoiceNo] = false;
            return res;
          },
          function (err) {
            toastr.error("Failed to save quotation. Please try again.");
            $scope.savingQuote[quote.InvoiceNo] = false;
            return $q.reject(err);
          },
        );
    }

    function toggleEditQuote($event, quote) {
      $event.stopPropagation();
      if ($scope.editingQuote[quote.InvoiceNo]) {
        saveQuoteInfo(quote).then(function () {
          $scope.editingQuote[quote.InvoiceNo] = false;
          delete $scope.quoteBackup[quote.InvoiceNo];
        });
      } else {
        $scope.quoteBackup[quote.InvoiceNo] = angular.copy(quote);
        $scope.editingQuote[quote.InvoiceNo] = true;
        $scope.expandedQuote[quote.InvoiceNo] = true;
      }
    }

    function cancelEditQuote($event, quote) {
      $event.stopPropagation();
      angular.extend(quote, $scope.quoteBackup[quote.InvoiceNo]);
      $scope.editingQuote[quote.InvoiceNo] = false;
      delete $scope.quoteBackup[quote.InvoiceNo];
    }

    function buildMapCoord() {
      $scope.mapLoading = true;
      $scope.mapCoord = {};

      var conn = $scope.connections[+$scope.svcIdx];
      if (!conn) {
        $scope.mapLoading = false;
        return;
      }

      var bill = ($scope.billInformation || []).find(function (b) {
        return b.ConnectionID === conn.ConnectionID;
      });

      var lat =
        bill && bill.GoogleMap_Latitude ? bill.GoogleMap_Latitude : null;
      var lng =
        bill && bill.GoogleMap_Longitude ? bill.GoogleMap_Longitude : null;

      var installParts = [];
      if (bill) {
        if (bill.InstallAddress) installParts.push(bill.InstallAddress);
        if (bill.InstallCommune) installParts.push(bill.InstallCommune);
        if (bill.InstallDistrict) installParts.push(bill.InstallDistrict);
        if (bill.InstallProvinceCity)
          installParts.push(bill.InstallProvinceCity);
      }
      var installAddress = installParts.length
        ? installParts.join(", ")
        : $scope.profile.CustomerAddress +
          ", " +
          $scope.profile.CustomerProvinceCity;

      var embedUrl = null;
      if (lat && lng) {
        embedUrl = $sce.trustAsResourceUrl(
          "https://maps.google.com/maps?q=" +
            lat +
            "," +
            lng +
            "&z=16&output=embed",
        );
      } else if (installAddress) {
        embedUrl = $sce.trustAsResourceUrl(
          "https://maps.google.com/maps?q=" +
            encodeURIComponent(installAddress) +
            "&z=16&output=embed",
        );
      }

      $scope.mapCoord = {
        lat: lat,
        lng: lng,
        connectionID: conn.ConnectionID,
        sid: conn.SID,
        installAddress: installAddress,
        googleMapAddress: bill ? bill.GoogleMap_FullAddress : null,
        embedUrl: embedUrl,
      };

      // small delay so the iframe has time to start loading
      $timeout(function () {
        $scope.mapLoading = false;
      }, 300);
    }

    // rebuild map when active connection changes
    $scope.$watch("svcIdx", function () {
      if ($scope.activeTab === "map" && $scope.billInfoLoaded) {
        $scope.mapLoading = true;
        buildMapCoord();
      }
    });

    function openDelayPayment() {
      ShowDialogBox2(
        "Delay Payment",
        "",
        "",
        "CustomerProfile/DelayPayment",
        ID,
        "DialogDelayPayment",
        angular.noop,
      );
    }

    function openTelegram() {
      ShowDialogBox2(
        "Customer Telegram",
        "auto",
        500,
        `CustomerProfile/TelegramNotification/?CustomerName=${encodeURIComponent($scope.profile?.CustomerName ?? "")}&CustomerCode=${$scope.profile?.CustomerCode}`,
        ID,
        "DialogCustTelegram",
        function () {
          var $saveBtn = $("#DialogCustTelegram").find("#SaveChange2");
          $saveBtn.html('<i class="fa fa-print" aria-hidden="true"></i> Print');
          $saveBtn.hide();
          $(document)
            .off("click", "#custom-tabs-five-messages-tab")
            .on("click", "#custom-tabs-five-messages-tab", function () {
              $saveBtn.show();
            });
          $(document)
            .off(
              "click",
              "#custom-tabs-four-home-tab, #custom-tabs-four-profile-tab, #custom-tabs-four-messages-tab",
            )
            .on(
              "click",
              "#custom-tabs-four-home-tab, #custom-tabs-four-profile-tab, #custom-tabs-four-messages-tab",
              function () {
                $saveBtn.hide();
              },
            );
          $saveBtn.off("click").on("click", function () {
            $("#CustInvoice").printThis();
          });
        },
      );
    }

    function openFeedback() {
      ShowDialogBox2(
        "Customer Feedback",
        "auto",
        "",
        `CustomerFeedBack/?CustomerCode=${$scope.profile?.CustomerCode}`,
        "",
        "",
        () => $("#SaveChange2").remove(),
      );
    }

    function printServiceForm() {
      var doprint = function () {
        var conn = ($scope.connections || [])[+($scope.svcIdx || 0)] || {};
        var bill =
          ($scope.billInformation || []).find(function (b) {
            return b.ConnectionID === conn.ConnectionID;
          }) || {};

        AgreementFormService.printAgreement($scope.profile, conn, bill);
      };

      // If billInformation not loaded yet, load it first, then print
      if (!$scope.billInfoLoaded) {
        loadBillInfo().then(doprint);
      } else {
        doprint();
      }
    }

    function editProfile() {
      EditCustomerWithRefNumber(ID, "", "NewRegister");
    }

    function editInvoice(inv) {
      EditCustomerWithRefNumber(
        ID,
        stripHtml(inv.InvoiceNo),
        "Invoice",
        false,
        null,
        inv.IsRenew,
        inv.IsChangePlan,
      );
    }

    const previewInvoice = (invoiceNo) =>
      PrintInvoice(ID, invoiceNo, "1", "10", "", false, this);

    function toInvoiceDialog(inv) {
      ToInvoiceDialog(ID, stripHtml(inv.InvoiceNo));
    }

    // Delete Invoice with SweetAlert2 confirmation
    function deleteInvoice(inv) {
      if (typeof Swal === "undefined" && typeof swal === "undefined") {
        alert("SweetAlert2 is not loaded!");
        return;
      }
      const swalInstance = window.Swal || window.swal;
      swalInstance
        .fire({
          title: "Are you sure?",
          text: "This will permanently delete the invoice.",
          icon: "warning",
          showCancelButton: true,
          confirmButtonColor: "#d33",
          cancelButtonColor: "#3085d6",
          confirmButtonText: "Yes, delete it!",
        })
        .then((result) => {
          if (result.isConfirmed) {
            $http
              .post(
                "DoActionData/DeleteData",
                {
                  FieldName: "InvoiceNo",
                  DataID: stripHtml(inv.InvoiceNo),
                  DeleteUseProcedureName: "SP_DELETE_CUSTOMER_INVOICE",
                  FormID: "",
                },
                { ignoreLoadingBar: true },
              )
              .then(function (res) {
                swalInstance.fire(
                  "Deleted!",
                  "Invoice has been deleted.",
                  "success",
                );
                loadUnpaid();
                loadBalance();
                loadConnections();
              })
              .catch(function (err) {
                swalInstance.fire(
                  "Error",
                  "Failed to delete invoice.",
                  "error",
                );
              });
          }
        });
    }

    function onAutoLockChange(conn) {
      const swalInstance = window.Swal || window.swal;
      const newState = conn.AutoLock; // true = turning ON, false = turning OFF
      const previousState = !newState;

      swalInstance
        .fire({
          title: "Are you sure?",
          text: `Are you sure you want to turn ${newState ? "on" : "off"} auto lock?`,
          icon: "warning",
          showCancelButton: true,
          confirmButtonColor: "#d33",
          cancelButtonColor: "#3085d6",
          confirmButtonText: "Yes, do it!",
        })
        .then((result) => {
          if (!result.isConfirmed) {
            // user cancelled — revert the checkbox back to its previous state
            conn.AutoLock = previousState;
            return;
          }

          $http
            .post(
              "CustomerProfile/SetAutoLockOnOff",
              {
                CustomerID: ID,
                SID: conn.SID,
                AutoLock: newState ? 1 : 0,
              },
              { ignoreLoadingBar: true },
            )
            .then(function (res) {
              swalInstance.fire(
                "Success!",
                `This service has been turned ${newState ? "on" : "off"} for auto lock.`,
                "success",
              );
            })
            .catch(function (err) {
              // revert checkbox on failure too
              conn.AutoLock = previousState;
              swalInstance.fire(
                "Error",
                `Failed to turn ${newState ? "on" : "off"} auto lock.`,
                "error",
              );
            });
        });
    }

    function renewDialog(conn) {
      RenewDialog(conn.CustomerID, conn.InvoiceNo);
    }

    function changePlanDialog(conn) {
      ChangePlanDialog(conn.CustomerID, conn.InvoiceNo);
    }

    function showLog(conn) {
      GetPPPeEUserLog(conn);
    }

    function newTicket(conn) {
      NewTicket(0, conn.ConnectionID);
    }

    function payNow(inv) {
      Payment(
        stripHtml(inv.InvoiceNo),
        customerName,
        inv.Unpaid,
        10,
        ID,
        "Official Invoice",
        "",
        () => refreshCustomer(),
      );
    }

    function rollBackInvoice(inv) {
      Payment(stripHtml(inv.InvoiceNo), customerName, inv.GrandTotal, 11, ID);
    }

    function receiveDevice() {
      ShowDialogBox2(
        "Receive Device",
        "auto",
        "",
        `CustomerProfile/ReceivedDevice?CustomerID=${ID}`,
      );
    }

    function changeDevice() {
      ShowDialogBox2(
        "Change Device",
        "auto",
        "",
        `CustomerProfile/ChangeDevice?CustomerID=${ID}`,
      );
    }

    function getStatusText(s) {
      return s === 40
        ? "Active"
        : s === 41
          ? "Locked"
          : s === 42
            ? "Terminate"
            : "Inactivate";
    }

    function getShortName(name) {
      if (!name) return "";
      const words = name
        .trim()
        .split(/\s+/)
        .filter(
          (w) =>
            !["mr", "mrs", "ms", "dr", "miss"].includes(
              w.toLowerCase().replace(".", ""),
            ),
        );
      if (!words.length) return "";
      const isKh = /[\u1780-\u17FF]/.test(name);
      if (words.length >= 2)
        return isKh
          ? words[0].charAt(0) + words[words.length - 1].charAt(0)
          : (words[0][0] + words[words.length - 1][0]).toUpperCase();
      return isKh ? words[0].charAt(0) : words[0].substring(0, 2).toUpperCase();
    }

    function ActivateCustomerService(conn) {
      const data = {
        PPPoEUserName: conn.PPPoEUserName ?? "",
        ConnectionName: conn.ConnectionName ?? "",
        InvoiceNo: conn.InvoiceNo ?? "",
        CID: conn.CustomerCode,
        CustomerID: conn.CustomerID,
        CustomerName: `${conn.CustomerName} [${conn.CustomerCode}]`,
        ServiceID: 0,
        ConnectionTypeID: conn.ConnectionTypeID,
        PackageDetailID: conn.PackageDetailID,
        BillingInvoiceID: conn.BillingInvoiceID,
        IsOnCustProfile: true,
        ActivateCustomerID: conn.ActivateCustomerID ?? 0,
        RouterID: conn.RouterID,
      };

      ShowDialogBox(
        "Activation Customer",
        "Auto",
        "500",
        "ActivateCustomer/Activate",
        data,
      );
    }

    function GetPPPeEUserLog(conn) {
      const url = `CustomerProfile/ShowCustActivateLog?CustomerID=${conn.CustomerID}&ShowDetail=true&PPPoEUserName=${conn.PPPoEUserName}&ConnectionID=${conn.ConnectionID}`;
      ShowDialogBox2(
        "User History",
        "Auto",
        "500",
        url,
        0,
        "DialogShowUserHistory",
        () => {
          $("#SaveChange2").remove();
        },
      );
    }

    function createMatcher(searchKey) {
      return function (item) {
        const term = ($scope[searchKey] || "").trim().toLowerCase();
        if (!term) return true;

        return Object.keys(item).some(function (key) {
          let val = item[key];
          if (val == null) return false;

          if (typeof val === "object") {
            if (val.$$unwrapTrustedValue) {
              val = stripHtml(val.toString());
            } else {
              return false; // skip nested objects (e.g. conn.router)
            }
          }

          return String(val).toLowerCase().indexOf(term) !== -1;
        });
      };
    }

    // ═════════════ ACTIVATION ═════════════
    const ACT = {
      defaults: {
        dns1: "103.134.177.26",
        dns2: "8.8.8.8",
        wifiIP: "192.168.1.1",
        wifiUser: "user",
        wifiPass: "user",
      },
      ipv4: /^(25[0-5]|2[0-4]\d|1?\d?\d)(\.(25[0-5]|2[0-4]\d|1?\d?\d)){3}$/,
    };
    $scope.act = { open: false };

    const blankItem = (connId) => ({
      ItemID: 0,
      ItemSerialDetailID: 0,
      CustomerDeviceInfoDetailID: 0,
      ItemName: "",
      UM: "",
      Type: "",
      LocationID: 0,
      WarehouseName: "",
      QTY: 1,
      SerialNumber: "",
      MACAddress: "",
      CostPrice: 0,
      Connection: connId,
      Reason: "",
    });
    const nowNoSec = () => {
      const d = new Date();
      d.setSeconds(0, 0);
      return d;
    };
    function genPwd() {
      const c = "abcdefghjkmnpqrstuvwxyz23456789",
        a = new Uint32Array(8);
      crypto.getRandomValues(a);
      return Array.from(a, (n) => c[n % c.length]).join("");
    }

    function activateService(conn) {
      const a = ($scope.act = {
        open: true,
        loading: true,
        error: "",
        conn,
        ctx: null,
        mode: "wizard",
        busy: false,
        done: false,
        isEdit: false,
        verify: null,
        errs: {},
        msg: "",
        tiles: [],
        f: {},
        dev: {},
        items: [],
        opts: { vlans: [], used: [], groups: [], users: [], loading: false },
      });

      api(
        `api/activation/context?billingInvoiceId=${conn.BillingInvoiceID}&activateCustomerId=${conn.ActivateCustomerID || 0}`,
      )
        .then((ctx) => {
          // Bulk "apply connections" invoices still use the legacy dialog
          if (ctx.bulk && !ctx.activate) {
            $scope.act = { open: false };
            return ActivateCustomerService(conn);
          }
          a.ctx = ctx;
          actInit(a);
        })
        .catch(
          () => (a.error = "Could not load activation data. Please try again."),
        )
        .finally(() => (a.loading = false));
    }

    // The API may send camelCase or PascalCase keys (activateCustomerID vs ActivateCustomerID).
    // Wrap the objects once so every later read (s.RouterID, d.PONPort, p.CustomerName ...) works for both.
    const ciObj = (o) => {
      const src = o || {},
        low = {};
      Object.keys(src).forEach((k) => (low[k.toLowerCase()] = src[k]));
      return new Proxy(src, {
        get: (t, k) =>
          typeof k !== "string" || k in t ? t[k] : low[k.toLowerCase()],
      });
    };

    function actInit(a) {
      const c = a.ctx;
      c.activate = c.activate ? ciObj(c.activate) : c.activate;
      c.plan = ciObj(c.plan);
      c.device = ciObj(c.device);
      const s = c.activate,
        p = c.plan,
        d = c.device,
        D = ACT.defaults;
      a.mode = s ? "manage" : "wizard";

      // normalise router list (handles either casing from the API)
      c.routers = (c.routers || []).map((r) => ({
        ...r,
        RouterID: r.RouterID != null ? r.RouterID : r.routerID,
        RouterName: r.RouterName != null ? r.RouterName : r.routerName,
      }));
      a.routerOpts = c.routers.map((r) => ({
        value: r.RouterID,
        label: r.RouterName,
      }));

      a.typeOpts = [
        { value: "PPPoE", label: "PPPoE" },
        { value: "Static", label: "Static" },
        { value: "Manual", label: "Manual" },
      ];

      // default router: activated one → plan → connection card → the only router
      const wanted = s ? s.RouterID : p.RouterID || a.conn.RouterID;
      const hit = c.routers.find((r) => String(r.RouterID) === String(wanted));
      const defaultRouterId = hit
        ? hit.RouterID
        : c.routers.length === 1
          ? c.routers[0].RouterID
          : "";

      a.f = {
        date:
          s && s.ActivateDate && !isNaN(new Date(s.ActivateDate))
            ? new Date(s.ActivateDate)
            : nowNoSec(),
        routerId: defaultRouterId,
        type: s
          ? s.ConnectionType
          : a.conn.ConnectionType === "Static"
            ? "Static"
            : "PPPoE",
        pppoeUser: (s && s.PPPoEUserName) || c.recommendedUser,
        pppoePass: (s && s.PPPoEPassword) || genPwd(),
        pppoeIp: (s && s.PPPoEIPAddress) || "",
        userGroup: (s && s.UserGroup) || "",
        existingUser: null,
        existingUserName: (s && s.PPPoEUserName) || "",
        pppoeProfileName: (s && s.PPPoEProfileName) || "",
        vlanId: s ? s.VLanID || "" : "",
        vlanName: s ? s.VLanName || "" : "",
        vlanIp: s ? s.VLanIPAddress || "" : "",
        ip: s ? s.IPAddress || "" : "",
        gateway: s ? s.GetwayIP || "" : "",
        subnet: s ? s.SubnetIP || "" : "",
        dns1: (s && s.DNS1) || D.dns1,
        dns2: (s && s.DNS2) || D.dns2,
        custDevice: s ? s.CustDeviceName || "" : "",
        wifiIP: (s && s.WifiIP) || D.wifiIP,
        wifiName: s ? s.WifiName || "" : "",
        wifiKey: s ? s.WifiKey || "" : "",
        wifiLoginName: (s && s.WifiLoginName) || D.wifiUser,
        wifiLoginPassword: (s && s.WifiLoginPassword) || D.wifiPass,
      };

      a.dev = {
        deviceName: d.DeviceName,
        deviceNameId: d.DeviceNameID,
        pon: d.PONPort,
        oltId: d.OLTID,
        splitter: d.SplitterName,
        splitterNameId: d.SplitterNameID,
        port: d.Port,
        length: d.Length,
        lossDb: d.LossDB,
        googleMap: d.GoogleMap,
        warehouse: d.Warehouse,
        wid: d.WID,
        installBy: d.InstallBy,
        installDate: d.CompleteInstallDate
          ? new Date(d.CompleteInstallDate)
          : nowNoSec(),
        notes: (d.CustomerNotes || "").replace(/<br\s*\/?>/gi, "\n"),
      };

      // equipment grid starts EMPTY (no blank row) — items are picked from stock
      a.items = (c.items || []).map((r) => {
        const src = ciObj(r),
          row = blankItem(p.ConnectionID);
        Object.keys(row).forEach((k) => {
          if (src[k] != null) row[k] = src[k];
        });
        console.log(row);

        return row;
      });
      a.stock = { sel: "", rows: [], loading: false, loaded: false };
      a.productOpts = [];

      const serviceName =
        p.ServiceName ||
        p.PackageLabel ||
        p.PackageName ||
        a.conn.PackageName ||
        a.conn.ServiceName ||
        "—";
      const bandwidth = p.Bandwidth ?? a.conn.Bandwidth;
      const downSpeed = p.DowSpeed ?? a.conn.DowSpeed;
      const upSpeed = p.UpSpeed ?? a.conn.UpSpeed;
      const startDate = p.StartDate ?? a.conn.StartDate;
      const endDate = p.EndDate ?? a.conn.EndDate;

      a.tiles = [
        ["Service", serviceName],
        [
          "Bandwidth",
          bandwidth == null
            ? "—"
            : `${bandwidth} Mbps (↓${downSpeed ?? "—"} ↑${upSpeed ?? "—"})`,
        ],
        [
          "Start Date",
          startDate ? $filter("date")(startDate, "dd-MMM-yyyy") : "—",
        ],
        ["End Date", endDate ? $filter("date")(endDate, "dd-MMM-yyyy") : "—"],
        ["Invoice", p.InvoiceNo ?? a.conn.InvoiceNo ?? "—"],
        ["Room #", p.ATTName ?? a.conn.ATTName ?? "—"],
      ];

      a.tiles.push(
        ["Type", s?.ConnectionType || a.conn.ConnectionType || "—"],
        ["SID", s?.SID || a.conn.SID || "—"],
        ["Connection ID", a.conn.ConnectionID || "—"],
        ["IP", s?.IPAddress || "—"],
        ["Activated", `${s?.ActivateBy || ""}\n${s?.ActivateDate || ""}`],
        [
          "Locked",
          s?.LockedBy && [41, 42].includes(Number(s.ConnectivityStatus))
            ? `${s.LockedBy} ${s.LockedDate || ""}`
            : "",
        ],
      );

      // load VLAN / group / user lists for the preselected router.
      // restoring=true only when editing an existing activation (keep its saved group).
      if (a.f.routerId) actLoadRouter(a.f.routerId, !!s);
      actLoadLookups(a); // device / splitter / installer combos
      actLoadMoves(a);
    }

    function actLoadRouter(rid, restoring) {
      const a = $scope.act;
      if (!rid) return;
      a.opts.loading = true;
      const id = a.ctx.activate ? a.ctx.activate.ActivateCustomerID : 0;
      return api(
        `api/activation/router-options?routerId=${rid}&activateCustomerId=${id}`,
      )
        .then((o) => {
          a.opts = Object.assign({ loading: false }, o);
          const used = o.used || [];
          a.vlanOpts = (o.vlans || []).map((v) => ({
            value: v.id,
            label: v.name,
            meta: used.indexOf(v.id) > -1 ? "in use" : "",
          }));
          a.groupOpts = (o.groups || []).map((g) => ({
            value: g.name,
            label: g.name,
            data: g,
          }));
          a.userOpts = (o.users || []).map((u) => ({
            value: u.name,
            label: u.name,
            data: u,
          }));
          a.profileOpts = (o.profiles || []).map((p) => ({
            value: p.name,
            label: p.name,
            data: p,
          }));

          if (!restoring && a.f.type === "PPPoE" && !a.f.userGroup) {
            const bw = String(a.ctx.plan.Bandwidth);
            a.f.userGroup =
              (o.groups || []).find((g) => g === bw || g.indexOf(bw) > 0) || "";
          }
        })
        .catch(() => {
          a.opts.loading = false;
          a.msg = "Could not read the router. Check that it is online.";
        });
    }

    function actOnRouter() {
      const a = $scope.act;
      Object.assign(a.f, {
        vlanId: "",
        vlanName: "",
        vlanIp: "",
        existingUser: null,
        existingUserName: "",
      });
      a.vlanOpts = [];
      a.userOpts = [];
      a.profileOpts = [];
      a.groupOpts = [];
      actLoadRouter(a.f.routerId, false);
    }

    function actPickUser(opt) {
      const f = $scope.act.f;
      f.existingUser = opt ? opt.data : null;
      if (f.existingUser) {
        f.pppoeUser = f.existingUser.name;
        f.pppoePass = f.existingUser.password;
      }
    }

    function actOnVlan() {
      const a = $scope.act,
        f = a.f;
      const v = a.opts.vlans.find((x) => x.id === f.vlanId);
      f.vlanName = v ? v.name : "";
      if (!v) return;
      api(
        `api/activation/vlan-address?routerId=${f.routerId}&vlanName=${encodeURIComponent(v.name)}&vlanId=${encodeURIComponent(v.id)}`,
      ).then((o) => {
        f.vlanIp = o.address || "";
        if (!o.address) return;
        const x = calculateIPCIDR(o.address); // existing global helper
        Object.assign(f, {
          ip: x.CustomerIP,
          subnet: x.SubnetMask,
          gateway: x.DefaultGetwayIP,
          dns1: ACT.defaults.dns1,
          dns2: ACT.defaults.dns2,
        });
      });
    }

    const actGenPwd = () => ($scope.act.f.pppoePass = genPwd());
    const actSetType = (t) => {
      $scope.act.f.type = t;
      $scope.act.errs = {};
      $scope.act.msg = "";
    };
    const actEdit = () => {
      const a = $scope.act;
      a.isEdit = true;
      a.mode = "wizard";
    };
    function actClose(force) {
      if (!force && $scope.act && $scope.act.busy) return;
      $scope.act = { open: false };
    }

    function actValidate(step) {
      const a = $scope.act,
        f = a.f,
        d = a.dev,
        errs = {};
      const need = (k, v) => {
        if (!String(v == null ? "" : v).trim()) errs[k] = 1;
      };
      if (step === 1) {
        need("routerId", f.routerId);
        if (f.type === "PPPoE") {
          need("pppoeUser", f.pppoeUser);
          need("pppoePass", f.pppoePass);
        }
        if (f.type === "Static") {
          need("vlanId", f.vlanId);
          ["ip", "gateway", "subnet", "dns1", "dns2"].forEach((k) => {
            if (!ACT.ipv4.test((f[k] || "").trim())) errs[k] = 1;
          });
        }
      }
      if (step === 2) {
        need("deviceName", d.deviceName);
        need("pon", d.pon);
        need("splitter", d.splitter);
        need("port", d.port);
        need("installBy", d.installBy);
        need("installDate", d.installDate);
        if (a.items.some((i) => i.ItemName && !i.ItemID)) errs.items = 1;
      }
      a.errs = errs;
      const bad = Object.keys(errs);
      a.msg = errs.items
        ? "Pick each item from the suggestion list."
        : bad.length
          ? "Please fix the highlighted fields."
          : "";
      if (
        !bad.length &&
        step === 1 &&
        f.type === "PPPoE" &&
        !a.ctx.canCreatePppoe &&
        !a.ctx.activate &&
        !f.existingUser
      ) {
        a.msg =
          "You don't have permission to create new PPPoE users. Pick an existing user under the PPPoE fields.";
        return false;
      }
      return !bad.length;
    }

    // Which tab owns a validation key
    const ACT_TAB_OF = {
      routerId: "connection",
      pppoeUser: "connection",
      pppoePass: "connection",
      vlanId: "connection",
      ip: "connection",
      gateway: "connection",
      subnet: "connection",
      dns1: "connection",
      dns2: "connection",
      deviceName: "device",
      pon: "device",
      splitter: "device",
      port: "device",
      installBy: "device",
      installDate: "device",
      items: "equipment",
    };

    function actSave() {
      const a = $scope.act;
      if (a.done) return actClose();

      const ok1 = actValidate(1);
      const errs1 = a.errs;
      const msg1 = a.msg;
      const ok2 = actValidate(2);

      a.errs = Object.assign({}, errs1, a.errs);
      if (!ok1 || !ok2) {
        if (!ok1 && msg1) a.msg = msg1;
        const firstKey = Object.keys(a.errs)[0];
        a.tab = ACT_TAB_OF[firstKey] || (!ok1 ? "connection" : "device");
        $timeout(function () {
          const el = document.querySelector(".av .av-f.bad, .av .av-list.bad");
          if (el && el.scrollIntoView)
            el.scrollIntoView({ behavior: "smooth", block: "center" });
        }, 0);
        return;
      }
      a.msg = "";
      actSubmit();
    }
    function actSubmit() {
      const a = $scope.act,
        f = a.f,
        d = a.dev,
        c = a.ctx,
        p = c.plan,
        s = c.activate;
      a.busy = true;
      a.busyText = a.isEdit ? "Saving and applying…" : "Activating connection…";
      a.msg = "";

      const router = c.routers.find((r) => r.RouterID === f.routerId);
      const stamp = moment(d.installDate).format("YYYY-MM-DD hh:mm A");

      const ipCheck =
        f.type === "Static" && (!s || s.IPAddress !== f.ip)
          ? $http
              .get("ActivateCustomer/CheckIPIsExisiting", {
                ignoreLoadingBar: true,
                params: {
                  oldIP: s ? s.IPAddress || "" : "",
                  NewIP: f.ip,
                  RouterID: f.routerId,
                },
              })
              .then((r) => {
                if (r.data.Result !== 2) throw new Error(r.data.Message);
              })
          : $q.resolve();

      const act = {
        ActivateCustomerID: s ? s.ActivateCustomerID : 0,
        RouterID: f.routerId,
        RouterName: router ? router.RouterName : "",
        BillingInvoiceID: p.BillingInvoiceID,
        InvoiceNo: p.InvoiceNo,
        SID: p ? p.SID : "",
        CustomerID: p.CustomerID,
        ConnectionID: p.ConnectionID,
        ConnectionType: f.type,
        PPPoEUserName: f.pppoeUser || "",
        PPPoEPassword: f.pppoePass || "",
        PPPoEIPAddress: f.pppoeIp || "",
        PPPoEProfileName: f.pppoeProfileName || "",
        TicketNumber: p.TicketNumber || "",
        DeviceName: f.custDevice || "",
        WifiIP: f.wifiIP,
        WifiName: f.wifiName,
        WifiKey: f.wifiKey,
        WifiLoginName: f.wifiLoginName,
        WifiLoginPassword: f.wifiLoginPassword,
        ISReActivate: a.isEdit,
        ActivateDate: moment(f.date).format("YYYY-MM-DD hh:mm A"),
        ManualPPPoEProfile: "",
        UserGroup: f.userGroup || "",
        UserPPPoEOnRadiusID: f.existingUser ? f.existingUser.password : "",
        UserPPPoEOnRadius: f.existingUser ? f.existingUser.name : "",
      };
      if (f.type === "Static")
        Object.assign(act, {
          VLanID: f.vlanId,
          VLanName: f.vlanName,
          VLanIPAddress: f.vlanIp,
          IPAddress: f.ip,
          GetwayIP: f.gateway,
          SubnetIP: f.subnet,
          DNS1: f.dns1,
          DNS2: f.dns2,
        });

      // Same rule the old script used to decide when the router user must be created/updated
      let checkUser = false;
      if (f.type === "PPPoE" && !f.existingUser) {
        if (!s || !s.SID) checkUser = true;
        else if (
          a.isEdit &&
          (f.pppoeUser !== s.PPPoEUserName || f.pppoePass !== s.PPPoEPassword)
        )
          checkUser = true;
      }
      const chk = {
        CheckPPPoEUserName: checkUser,
        RouterID: f.routerId,
        ActivateCustomerID: act.ActivateCustomerID,
        oldPPPoEUserName: s ? s.PPPoEUserName || "" : "",
        oldPPPoEPassword: s ? s.PPPoEPassword || "" : "",
        PPPoEUserName: f.pppoeUser || "",
        PPPoEPassword: f.pppoePass || "",
      };

      const dev = {
        DeviceName: d.deviceName,
        DeviceNameID: d.deviceNameId || "",
        PONPORT: d.pon,
        OLTID: d.oltId || "",
        SplitterName: d.splitter,
        SplitterNameID: d.splitterNameId || "",
        Port: d.port,
        Length: d.length || "",
        LossDB: d.lossDb || "",
        GoogleMap: d.googleMap || "",
        Warehouse: d.warehouse || "",
        WID: d.wid || "",
        InstallBy: d.installBy,
        CompleteInstallDate: stamp,
        CustomerNotes: encodeURIComponent(
          (d.notes || "").replace(/\r?\n/g, "<br>"),
        ),
      };

      const detail = a.items
        .filter((i) => i.ItemName)
        .map((i) => ({
          ItemID: i.ItemID,
          ItemSerialDetailID: i.ItemSerialDetailID,
          CustomerDeviceInfoDetailID: i.CustomerDeviceInfoDetailID || 0,
          ItemName: i.ItemName,
          UM: i.UM,
          Type: i.Type,
          LocationID: i.LocationID,
          WarehouseName: i.WarehouseName,
          QTY: i.QTY,
          SerialNo: i.SerialNumber,
          MACAddress: i.MACAddress,
          CostPrice: i.CostPrice,
          Connection: i.Connection,
          Reason: i.Reason,
          InstallBy: d.installBy,
          CompleteInstallDate: stamp,
        }));

      const payload = {
        CheckPPPoEUserName: chk,
        DeviceInformation: [dev],
        DeviceInformationDetail: detail,
        ActivateCustomerData: [act],
        ApplyCons: [],
      };

      // console.log(payload);
      // return;

      ipCheck
        .then(() =>
          $http.post("api/activation/save-change-activate", payload, {
            headers: { "Content-Type": "application/json" },
            ignoreLoadingBar: true,
          }),
        )
        .then((r) => {
          const ok = r.data && (r.data.result === 2 || r.data.success === true);
          if (!ok)
            throw new Error((r.data && r.data.message) || "Activation failed");
          a.done = true;
          $scope.devicesLoaded = false;
          $scope.billLoaded = false;
          toastr.success("Saved");
          loadConnections();
          actVerify(a);
        })
        .catch((e) => {
          console.error("Activation save error:", e);

          a.msg =
            (e.data && (e.data.message || e.data.Message)) ||
            e.statusText ||
            "Request failed";

          if (e.status === 400) {
            toastr.warning(a.msg);
          } else {
            toastr.error(a.msg);
          }
        })
        .finally(() => {
          a.busy = false;
          a.busyText = "";
        });
    }

    function actVerify(a) {
      const f = a.f;
      if (f.type === "Manual")
        return (a.verify = {
          state: "ok",
          title: "Saved",
          sub: "Manual connection: nothing to check on the router.",
        });
      a.verify = { state: "wait", title: "Checking router…", sub: "" };
      const url =
        f.type === "PPPoE"
          ? `api/mikrotik/pppoe/users/status?routerId=${f.routerId}&username=${encodeURIComponent(f.pppoeUser)}`
          : `api/mikrotik/vlans/status?routerId=${f.routerId}&vlanId=${f.vlanId}&vlanName=${encodeURIComponent(f.vlanName)}`;
      api(url)
        .then(
          (r) =>
            (a.verify =
              r && r.success && r.disabled === "false"
                ? {
                    state: "ok",
                    title: "Active on router",
                    sub: "Saved and confirmed on the device.",
                  }
                : {
                    state: "warn",
                    title: "Saved, but the router shows it disabled",
                    sub: "Use <b>Sync</b> on the connection card.",
                  }),
        )
        .catch(
          () =>
            (a.verify = {
              state: "warn",
              title: "Saved",
              sub: "The router could not be reached for the check.",
            }),
        );
    }

    const ACT_BUSY_TEXT = {
      40: "Re-activating service…",
      41: "Locking service…",
      42: "Terminating service…",
    };

    function actManage(status) {
      const a = $scope.act,
        s = a.ctx.activate,
        swal = window.Swal || window.swal;
      const t = {
        40: ["Re-activate this service?", "Activate"],
        41: ["Lock this service?", "Lock"],
        42: ["Terminate this service?", "Terminate"],
      }[status];

      swal
        .fire({
          title: t[0],
          input: "textarea",
          inputPlaceholder: "Reason (required)",
          inputValidator: (v) => !v && "Please enter a reason",
          showCancelButton: true,
          confirmButtonText: t[1],
          confirmButtonColor: status === 40 ? "#07c580" : "#d33",
          didOpen: (popup) => {
            popup.parentElement.style.zIndex = "20000";
          },
        })
        .then((res) => {
          if (!res.isConfirmed) return;

          // SweetAlert's promise is native, so tell Angular to repaint
          $scope.$applyAsync(() => {
            a.busy = true;
            a.busyAction = status;
            a.busyText = ACT_BUSY_TEXT[status];
          });

          $http
            .post(
              "api/activation/set-connectivity",
              {
                ActivateCustomerID: s.ActivateCustomerID,
                Status: status,
                Comment: res.value,
              },
              { ignoreLoadingBar: true },
            )
            .then((r) => {
              const d = r.data || {};
              if (!d.success)
                return swal.fire(
                  "Failed",
                  d.message || "Action failed",
                  "error",
                );
              toastr.success(d.message);
              actClose(true);
              loadConnections();
            })
            .catch(() => swal.fire("Error", "Request failed", "error"))
            .finally(() => {
              a.busy = false;
              a.busyAction = null;
              a.busyText = "";
            });
        });
    }

    function actLoadMoves(a) {
      a.moves = [];
      a.moveGroups = [];
      a.movesLoading = true;
      const id = a.ctx.activate ? a.ctx.activate.ActivateCustomerID : 0;
      if (!id) {
        a.movesLoading = false;
        return;
      }
      api(`api/activation/movements?activateCustomerId=${id}`)
        .then((r) => {
          a.moves = (r || [])
            .map((x) => ciObj(x))
            .sort((x, y) => new Date(y.CreateDate) - new Date(x.CreateDate)); // newest first
          a.moveGroups = actGroupMoves(a.moves);
        })
        .catch(() => {
          a.moves = [];
          a.moveGroups = [];
        })
        .finally(() => (a.movesLoading = false));
    }
    function actGroupMoves(moves) {
      const map = {},
        out = [];
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      (moves || []).forEach((m) => {
        const d = new Date(m.CreateDate);
        d.setHours(0, 0, 0, 0);
        const key = d.getTime();
        if (!map[key]) {
          const diff = Math.round((today - d) / 86400000);
          map[key] = {
            key,
            label:
              diff === 0
                ? "Today"
                : diff === 1
                  ? "Yesterday"
                  : $filter("date")(d, "dd-MMM-yyyy"),
            items: [],
          };
          out.push(map[key]);
        }
        map[key].items.push(m);
      });
      return out;
    }
    const actMoveClass = (m) =>
      m.ActionType === "Lock" || m.ActionType === "Terminate"
        ? "bad"
        : m.ActionType === "Edit" ||
            m.ActionType === "ChangePlan" ||
            m.ActionType === "Sync"
          ? "warn"
          : "ok";

    function actMoveIcon(m) {
      const t = (m.ActionType || "").toLowerCase();
      if (t.indexOf("reactivate") > -1) return "ti-refresh";
      if (t.indexOf("lock") > -1) return "ti-lock";
      if (t.indexOf("terminat") > -1) return "ti-wifi-off";
      if (t.indexOf("activate") > -1) return "ti-bolt";
      if (t.indexOf("plan") > -1) return "ti-switch-3";
      if (t.indexOf("edit") > -1 || t.indexOf("sync") > -1) return "ti-edit";
      return "ti-point";
    }
    // 40 = Active, anything else = locked / off
    const actStatusClass = (s) => (+s === 40 ? "ok" : "bad");

    const CHG_EMPTY = /^(~|-|—|–|null|none|n\/a|\(empty\))?$/i;
    function actChanges(m) {
      if (!m.Changes) return [];

      if (m._chg) return m._chg;

      const rows = String(m.Changes)
        .split(";")
        .map((s) => {
          s = s.trim();
          if (!s) return null;

          const i = s.indexOf(":");
          const field = i > -1 ? s.slice(0, i).trim() : "Change";
          const val = i > -1 ? s.slice(i + 1).trim() : s;
          const p = val.split(/→|->/);

          if (p.length < 2) return { field, from: "", to: val };

          const from = p[0].trim();

          return {
            field,
            from: CHG_EMPTY.test(from) ? "" : from,
            to: p[1].trim(),
          };
        })
        .filter(Boolean);

      rows.allNew = rows.length > 0 && rows.every((c) => !c.from);

      return (m._chg = rows);
    }

    // Drop "reactivated by:LENG.DEV; 10-11-2026 14:49:28 PM" (already shown in the header)
    function actNote(m) {
      if (!m.Comment) return "";
      return String(m.Comment)
        .split(";")
        .map((s) => s.trim())
        .filter(
          (s) => s && !/\bby\s*:/i.test(s) && !/^\d{2}-\d{2}-\d{4}/.test(s),
        )
        .join(" · ");
    }
    function actApplyPlan() {
      const a = $scope.act,
        swal = window.Swal || window.swal;
      a.busy = true;
      a.busyAction = "plan";
      a.busyText = "Applying change plan…";
      $http
        .post(
          "ActivateCustomer/ApplyChangePlanNow",
          $httpParamSerializer({
            InvoiceNo: a.ctx.plan.InvoiceNo,
            ProfileName: "",
          }),
          {
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            ignoreLoadingBar: true,
          },
        )
        .then((r) => {
          if (r.data.Result !== 2)
            return swal.fire(
              "Failed",
              r.data.Message || "Could not apply",
              "error",
            );
          toastr.success("Change plan applied");
          actClose(true);
          loadConnections();
        })
        .catch(() => swal.fire("Error", "Request failed", "error"))
        .finally(() => {
          a.busy = false;
          a.busyAction = null;
          a.busyText = "";
        });
    }

    const actIsTaken = (r) =>
      !!r.ItemSerialDetailID &&
      $scope.act.items.some(
        (i) => i.ItemSerialDetailID === r.ItemSerialDetailID,
      );

    function actStockAdd(r) {
      const a = $scope.act;
      if (actIsTaken(r)) return;
      if (!r.ItemSerialDetailID) {
        const ex = a.items.find(
          (i) => i.ItemID === r.id && !i.ItemSerialDetailID,
        );
        if (ex) {
          ex.QTY = (+ex.QTY || 0) + 1;
          return;
        }
      }
      const row = blankItem(a.ctx.plan.ConnectionID);
      actPickItem(row, r);
      row.MACAddress = r.MACAddress || "";
      row.QTY = 1;
      a.items.push(row);
    }

    const actDelItem = (i) => {
      $scope.act.items.splice(i, 1); // removes only that row
      $scope.act.productOpts = actProductOpts($scope.act);
    };

    const actQty = (i, d) => {
      i.QTY = Math.max(1, (+i.QTY || 1) + d);
    };

    // ── Lookups for <ui-combo> (same endpoint as before: api/activation/lookup) ──
    const actLookup = (kind, q, loc) =>
      $http
        .get("api/activation/lookup", {
          ignoreLoadingBar: true,
          params: { kind, q: q || "", loc: loc || "" },
        })
        .then((r) => r.data || []);

    // Device tab: value = the saved NAME string (payload unchanged)
    const ACT_LOOKUP_FIELD = {
      device: ["deviceOpts", "deviceName"],
      splitter: ["splitterOpts", "splitter"],
      installer: ["installerOpts", "installBy"],
    };

    const actPickDevice = (opt) => {
      $scope.act.dev.deviceNameId = opt && opt.data ? opt.data.id : "";
    };

    const actPickSplitter = (opt) => {
      $scope.act.dev.splitterNameId = opt && opt.data ? opt.data.id : "";
    };

    function actBuildOpts(a, kind, rows, q) {
      const cur = a.dev[ACT_LOOKUP_FIELD[kind][1]];
      const opts = (rows || []).map((r) => ({
        value: r.name,
        label: r.name,
        meta: r.meta || "",
        data: r,
      }));
      if (!q && cur && !opts.some((o) => String(o.value) === String(cur)))
        opts.unshift({ value: cur, label: cur, meta: "" });
      return opts;
    }

    function actWarehouseOpts(a, rows, q) {
      const opts = (rows || []).map((r) => ({
        value: r.id,
        label: r.name,
        meta: r.meta || "",
        data: r,
      }));
      if (
        !q &&
        a.dev.wid &&
        !opts.some((o) => String(o.value) === String(a.dev.wid))
      )
        opts.unshift({
          value: a.dev.wid,
          label: a.dev.warehouse || String(a.dev.wid),
          meta: "",
        });
      return opts;
    }

    function actPickWarehouse(opt) {
      const a = $scope.act;
      a.dev.warehouse = opt ? opt.label : "";
      a.stock.sel = "";
      a.stock.rows = [];
      a.stock.loaded = false;
      a.productOpts = [];
      actLoadProducts("");
    }

    function actLoadProducts(q) {
      const a = $scope.act;
      if (!a || !a.stock) return;
      if (!a.dev.wid) {
        a.stock.rows = [];
        a.productOpts = [];
        a.stock.loading = false;
        a.stock.loaded = false;
        return;
      }
      // show the spinner immediately, not after the 220ms debounce
      a.stock.loading = true;
      $timeout.cancel(prodTimer);
      prodTimer = $timeout(() => {
        const my = ++prodSeq;
        actLookup("item", q, a.dev.wid)
          .then((rows) => {
            if (my !== prodSeq || $scope.act !== a) return; // stale
            a.stock.rows = rows;
            a.productOpts = actProductOpts(a);
          })
          .catch(() => {
            if (my === prodSeq) {
              a.stock.rows = [];
              a.productOpts = [];
            }
          })
          .finally(() => {
            if (my === prodSeq) {
              a.stock.loading = false;
              a.stock.loaded = true;
            }
          });
      }, 220);
    }

    // Equipment tab: products available in the chosen warehouse
    let prodTimer,
      prodSeq = 0;
    function actProductOpts(a) {
      return ((a.stock && a.stock.rows) || []).map((r) => {
        const taken = actIsTaken(r);
        const extra = [r.SerialNumber, r.MACAddress]
          .filter(Boolean)
          .join(" · ");
        return {
          value: r.ItemSerialDetailID ? "s" + r.ItemSerialDetailID : "i" + r.id,
          label: r.name,
          meta: taken ? "Added" : [extra, r.meta].filter(Boolean).join(" · "),
          disabled: taken,
          data: r,
        };
      });
    }

    function actPickProduct(opt) {
      console.log(opt);

      const a = $scope.act;
      if (!opt || !opt.data) return;
      a.msg = "";
      if (!actIsTaken(opt.data)) actStockAdd(opt.data);
      $timeout(() => {
        a.stock.sel = ""; // clear the box for the next product
        a.productOpts = actProductOpts(a); // mark the new row as "Added"
      });
    }

    function actLoadLookups(a) {
      Object.keys(ACT_LOOKUP_FIELD).forEach((kind) => {
        const key = ACT_LOOKUP_FIELD[kind][0];
        a[key] = actBuildOpts(a, kind, []);
        actLookup(kind)
          .then((rows) => {
            if ($scope.act !== a) return;

            // saved name is "0"/empty but we have the id → resolve the name from it
            if (kind === "device" && !a.dev.deviceName && a.dev.deviceNameId) {
              const hit = (rows || []).find(
                (r) => String(r.id) === String(a.dev.deviceNameId),
              );
              if (hit) a.dev.deviceName = hit.name;
            }

            a[key] = actBuildOpts(a, kind, rows);
          })
          .catch(() => {});
      });
      a.warehouseOpts = actWarehouseOpts(a, []);
      actLookup("warehouse")
        .then((rows) => {
          if ($scope.act === a) a.warehouseOpts = actWarehouseOpts(a, rows);
        })
        .catch(() => {});
      if (a.dev.wid) actLoadProducts("");
    }

    const lookupTimers = {},
      lookupSeq = {};

    function actSearchLookup(kind, q) {
      const a = $scope.act;
      if (!a || !a.open) return;
      $timeout.cancel(lookupTimers[kind]);
      lookupTimers[kind] = $timeout(() => {
        const my = (lookupSeq[kind] = (lookupSeq[kind] || 0) + 1);
        actLookup(kind, q)
          .then((rows) => {
            if (my !== lookupSeq[kind] || $scope.act !== a) return; // stale
            if (kind === "warehouse")
              a.warehouseOpts = actWarehouseOpts(a, rows, q);
            else a[ACT_LOOKUP_FIELD[kind][0]] = actBuildOpts(a, kind, rows, q);
          })
          .catch(() => {});
      }, 220);
    }

    function actPickItem(row, r) {
      Object.assign(row, {
        ItemID: r.id,
        ItemName: r.name,
        UM: r.um || "",
        Type: r.stockName || "",
        LocationID: r.locationID || $scope.act.dev.wid || 0,
        WarehouseName: r.warehouseName || "",
        ItemSerialDetailID: r.itemSerialDetailID || 0,
        SerialNumber: r.serialNumber,
        CostPrice: r.costPrice || 0,
      });
    }

    Object.assign($scope, {
      stripHtml,
      setTab,
      setSubTab,
      setQuoteTab,
      setSvcTab,
      setFileTab,
      goSvc,
      copyText,
      loadProfile,
      loadBalance,
      loadConnections,
      refreshConnections,
      refreshCustomer,
      trackOnline,
      syncMismatch,
      loadUnpaid,
      loadPaid,
      loadQuotation,
      loadProformaInvoice,
      loadTransaction,
      loadAttachment,
      loadDeposit,
      loadProduct,
      loadIDServiceForm,
      isImage,
      isPdf,
      openUploadModal,
      closeUploadModal,
      onFilesSelected,
      removeSelectedFile,
      submitUpload,
      triggerFilePicker,
      loadDevices,
      loadBillInfo,
      toggleExpandBill,
      toggleEditBill,
      cancelEditBill,
      toggleExpandQuote,
      toggleEditQuote,
      cancelEditQuote,
      buildMapCoord,
      openDelayPayment,
      openTelegram,
      openFeedback,
      printServiceForm,
      editProfile,
      editInvoice,
      previewInvoice,
      toInvoiceDialog,
      deleteInvoice,
      onAutoLockChange,
      renewDialog,
      changePlanDialog,
      showLog,
      newTicket,
      payNow,
      rollBackInvoice,
      receiveDevice,
      changeDevice,
      getStatusText,
      getStatusBadge,
      activateService,
      actClose,
      actSetType,
      actOnRouter,
      actOnVlan,
      actPickUser,
      actGenPwd,
      actEdit,
      actManage,
      actApplyPlan,
      actDelItem,
      actQty,
      actPickItem,
      actStockAdd,
      actIsTaken,
      actSave,
      actPickDevice,
      actPickSplitter,
      actPickWarehouse,
      actLoadProducts,
      actPickProduct,
      actMoveClass,
      actMoveIcon,
      actStatusClass,
      actChanges,
      actNote,
      actSearchLookup,
    });

    // Only needed for tables containing $sce-trusted HTML fields (InvoiceNo)
    $scope.matchTransactions = createMatcher("txSearch");
    $scope.matchPI = createMatcher("piSearch");
    $scope.matchQuotation = createMatcher("quoSearch");
    $scope.matchUnpaid = createMatcher("unpaidSearch");
    $scope.matchPaid = createMatcher("paidSearch");
    $scope.matchDevice = createMatcher("deviceSearch");
    $scope.matchTicket = createMatcher("ticketSearch");
    $scope.matchServices = createMatcher("servicesSearch");
    $scope.matchDevices = createMatcher("devicesSearch");
    $scope.matchBillInfo = createMatcher("billSearch");

    loadProfile();
    loadBalance();
    loadProformaInvoice();
    loadUnpaid();
    loadTransaction();
    loadConnections();
  },
]);

app.component("searchBox", {
  bindings: {
    searchTerm: "=", // two-way bound search string
    placeholder: "@", // optional placeholder text
  },
  template: `
        <div class="topbar-search" style="max-width:280px;">
            <i class="ti ti-search"></i>
            <input type="text"
                    style="border-radius: 0 !important; background-color: transparent !important;"
                   ng-model="$ctrl.searchTerm"
                   ng-model-options="{ debounce: 200 }"
                   placeholder="{{ $ctrl.placeholder || 'Search this table...' }}"
                   autocomplete="off"
                   style="border-radius:0 !important;" />
            <i class="ti ti-x" ng-show="$ctrl.searchTerm" ng-click="$ctrl.clear()" style="cursor:pointer"></i>
        </div>
    `,
  controller: function () {
    this.clear = () => {
      this.searchTerm = "";
    };
  },
});

app.component("actSuggest", {
  bindings: {
    value: "=",
    kind: "@",
    loc: "<",
    placeholder: "@",
    onPick: "&",
    onChange: "&",
  },
  template: `
    <div class="act2-sg">
      <input ng-model="$ctrl.value" ng-change="$ctrl.type()" ng-focus="$ctrl.search()"
             ng-blur="$ctrl.hide()" placeholder="{{ $ctrl.placeholder }}" autocomplete="off">
      <div class="act2-sg-list" ng-if="$ctrl.open && $ctrl.rows.length">
        <div ng-repeat="r in $ctrl.rows" ng-mousedown="$ctrl.pick(r)">
          <b>{{ r.name }}</b><small>{{ r.meta }}</small>
        </div>
      </div>
    </div>`,
  controller: [
    "$http",
    "$timeout",
    function ($http, $timeout) {
      const ctrl = this;
      let t;
      ctrl.rows = [];
      ctrl.open = false;
      ctrl.type = () => {
        ctrl.onChange();
        ctrl.search();
      };
      ctrl.search = () => {
        $timeout.cancel(t);
        t = $timeout(() => {
          if (ctrl.kind === "item" && !ctrl.loc) {
            ctrl.rows = [];
            return;
          }
          $http
            .get("api/activation/lookup", {
              ignoreLoadingBar: true,
              params: {
                kind: ctrl.kind,
                q: ctrl.value || "",
                loc: ctrl.loc || "",
              },
            })
            .then((r) => {
              ctrl.rows = r.data || [];
              ctrl.open = true;
            });
        }, 220);
      };
      ctrl.pick = (r) => {
        ctrl.value = r.name;
        ctrl.open = false;
        ctrl.onPick({ r: r });
      };
      ctrl.hide = () => $timeout(() => (ctrl.open = false), 150);
    },
  ],
});

app.component("uiCombo", {
  bindings: {
    model: "=",
    options: "<",
    placeholder: "@",
    clearable: "<",
    disabled: "<",
    remote: "<", // true = options are searched on the server, skip local filtering
    onSearch: "&", // called with { q } while typing (use with remote)
    onChange: "&",
  },
  template: `
    <div class="cbx" ng-class="{ open: $ctrl.open, disabled: $ctrl.disabled }">
      <div class="cbx-trigger" ng-click="$ctrl.toggle($event)">
        <input class="cbx-input" type="text" autocomplete="off"
               ng-model="$ctrl.text" ng-change="$ctrl.onType()"
               ng-disabled="$ctrl.disabled" ng-keydown="$ctrl.onKey($event)"
               placeholder="{{ $ctrl.placeholder || 'Select…' }}">
        <button type="button" class="cbx-clear" tabindex="-1"
                ng-if="$ctrl.clearable && $ctrl.model !== '' && $ctrl.model != null && !$ctrl.disabled"
                ng-mousedown="$ctrl.clear($event)"><i class="ti ti-x"></i></button>
        <span class="cbx-caret"><i class="ti ti-chevron-down"></i></span>
      </div>
      <div class="cbx-panel" ng-if="$ctrl.open">
        <div class="cbx-opt" ng-repeat="o in $ctrl.filtered track by $index"
             ng-class="{ hl: $index === $ctrl.hl, sel: $ctrl.isSel(o), dis: o.disabled }"
             ng-mouseenter="$ctrl.hl = $index" ng-mousedown="$ctrl.pick(o, $event)">
          <span class="cbx-label">{{ o.label }}</span>
          <span class="cbx-meta" ng-if="o.meta">{{ o.meta }}</span>
          <i class="ti ti-check" ng-if="$ctrl.isSel(o)"></i>
        </div>
        <div class="cbx-empty" ng-if="!$ctrl.filtered.length">No matches</div>
      </div>
    </div>`,
  controller: [
    "$element",
    "$document",
    "$timeout",
    function ($element, $document, $timeout) {
      const c = this;
      let typed = false;
      let lastModel;

      c.open = false;
      c.hl = 0;
      c.text = "";
      c.filtered = [];

      const same = (a, b) => a != null && b != null && String(a) === String(b);
      const find = () => (c.options || []).find((o) => same(o.value, c.model));
      const label = () => {
        const o = find();
        return o ? o.label : "";
      };
      function refresh() {
        const q = typed && !c.remote ? (c.text || "").trim().toLowerCase() : "";
        c.filtered = (c.options || []).filter(
          (o) =>
            !q ||
            String(o.label).toLowerCase().includes(q) ||
            String(o.meta || "")
              .toLowerCase()
              .includes(q),
        );
        const i = c.filtered.findIndex((o) => same(o.value, c.model));
        c.hl = i > -1 ? i : 0;
      }
      function close() {
        if (!c.open) return;
        c.open = false;
        if (typed && c.remote) c.onSearch({ q: "" });
        typed = false;
        c.text = label();
      }
      const outside = (e) => {
        if (!$element[0].contains(e.target)) $timeout(close);
      };

      c.$onInit = () => $document.on("mousedown", outside);
      c.$onDestroy = () => $document.off("mousedown", outside);
      c.$onChanges = () => {
        if (!c.open) c.text = label();
        refresh();
      };
      // keep text in sync when the model is changed from the outside (defaults, resets)
      c.$doCheck = () => {
        if (c.model !== lastModel) {
          lastModel = c.model;
          if (!c.open) c.text = label();
        }
      };

      c.isSel = (o) => same(o.value, c.model);
      c.toggle = (e) => {
        if (c.disabled) return;
        if (c.open && e && e.target.tagName !== "INPUT") return close();
        if (!c.open) {
          c.open = true;
          typed = false;
          refresh();
          const input = $element[0].querySelector("input");
          $timeout(() => input && input.select());
        }
      };
      c.onType = () => {
        c.open = true;
        typed = true;
        refresh();
        c.onSearch({ q: c.text || "" });
      };
      c.pick = (o, e) => {
        if (e) e.preventDefault();
        if (o.disabled) return;
        c.model = o.value;
        lastModel = o.value;
        c.open = false;
        typed = false;
        c.text = o.label;
        $timeout(() => c.onChange({ opt: o })); // after the parent model has updated
      };
      c.clear = (e) => {
        e.preventDefault();
        c.model = "";
        lastModel = "";
        c.text = "";
        $timeout(() => c.onChange({ opt: null }));
      };
      c.onKey = (e) => {
        const n = c.filtered.length;
        if (e.key === "ArrowDown" || e.key === "ArrowUp") {
          e.preventDefault();
          if (!c.open) return c.toggle();
          c.hl = (c.hl + (e.key === "ArrowDown" ? 1 : -1) + n) % (n || 1);
        } else if (e.key === "Enter") {
          if (c.open && c.filtered[c.hl]) {
            e.preventDefault();
            c.pick(c.filtered[c.hl]);
          }
        } else if (e.key === "Escape" || e.key === "Tab") {
          close();
        }
      };
    },
  ],
});
// Legacy: no longer used by the dialog markup (it uses <act-suggest kind="item">).
// Kept registered so older markup still renders; safe to delete.
app.component("actItemPicker", {
  bindings: {
    loc: "<",
    disabled: "<",
    isTaken: "&",
    onPick: "&",
  },
  template: `
    <div class="av-pk" ng-class="{ open: $ctrl.open, disabled: $ctrl.disabled }">
      <i class="ti ti-search"></i>
      <input type="text" ng-model="$ctrl.q" ng-change="$ctrl.search()"
             ng-focus="$ctrl.focus()" ng-blur="$ctrl.hide()" ng-keydown="$ctrl.key($event)"
             ng-disabled="$ctrl.disabled" autocomplete="off"
             placeholder="{{ $ctrl.disabled ? 'Select a warehouse first' : 'Search item name or serial…' }}">
      <i class="ti ti-loader-2 ti-spin" ng-if="$ctrl.loading"></i>
      <div class="av-pk-list" ng-if="$ctrl.open">
        <div class="av-pk-opt" ng-repeat="r in $ctrl.rows track by $index"
             ng-class="{ hl: $index === $ctrl.hl, dis: $ctrl.isTaken({ r: r }) }"
             ng-mouseenter="$ctrl.hl = $index" ng-mousedown="$ctrl.pick(r, $event)">
          <span class="av-pk-main">
            <b>{{ r.name }}</b>
            <small ng-if="r.SerialNumber || r.MACAddress">{{ r.SerialNumber }}<span ng-if="r.SerialNumber && r.MACAddress"> · </span>{{ r.MACAddress }}</small>
          </span>
          <span class="av-pk-meta">{{ $ctrl.isTaken({ r: r }) ? 'Added' : r.meta }}</span>
        </div>
        <div class="av-pk-empty" ng-if="!$ctrl.rows.length && !$ctrl.loading">No available items</div>
      </div>
    </div>`,
  controller: [
    "$http",
    "$timeout",
    function ($http, $timeout) {
      const c = this;
      let t,
        seq = 0;
      c.rows = [];
      c.q = "";
      c.open = false;
      c.loading = false;
      c.hl = 0;

      c.$onChanges = (ch) => {
        if (ch.loc) {
          c.rows = [];
          c.q = "";
        }
      };

      c.focus = () => {
        if (c.disabled) return;
        c.open = true;
        c.search();
      };

      c.search = () => {
        if (c.disabled || !c.loc) return;
        c.open = true;
        $timeout.cancel(t);
        t = $timeout(() => {
          const my = ++seq;
          c.loading = true;
          $http
            .get("api/activation/lookup", {
              ignoreLoadingBar: true,
              params: { kind: "item", q: c.q || "", loc: c.loc },
            })
            .then((r) => {
              if (my !== seq) return; // stale response
              c.rows = r.data || [];
              c.hl = 0;
            })
            .catch(() => {
              if (my === seq) c.rows = [];
            })
            .finally(() => {
              if (my === seq) c.loading = false;
            });
        }, 220);
      };

      c.pick = (r, e) => {
        if (e) e.preventDefault();
        if (c.isTaken({ r: r })) return;
        c.onPick({ r: r });
        c.q = "";
        c.open = false;
      };

      c.hide = () => $timeout(() => (c.open = false), 150);

      c.key = (e) => {
        const n = c.rows.length;
        if (e.key === "ArrowDown" || e.key === "ArrowUp") {
          e.preventDefault();
          if (!c.open) return c.focus();
          c.hl = (c.hl + (e.key === "ArrowDown" ? 1 : -1) + n) % (n || 1);
        } else if (e.key === "Enter") {
          if (c.open && c.rows[c.hl]) {
            e.preventDefault();
            c.pick(c.rows[c.hl]);
          }
        } else if (e.key === "Escape") {
          c.open = false;
        }
      };
    },
  ],
});
app.directive("fileModel", [
  "$parse",
  function ($parse) {
    return {
      restrict: "A",
      link: function (scope, element, attrs) {
        const model = $parse(attrs.fileModel);
        const onChangeFn = attrs.fileModelChange
          ? $parse(attrs.fileModelChange)
          : null;

        element.on("change", function () {
          scope.$apply(function () {
            const files =
              element[0].files.length > 1
                ? element[0].files
                : element[0].files[0];
            model.assign(scope, files);
            if (onChangeFn) {
              onChangeFn(scope, { $files: element[0].files });
            }
          });
        });
      },
    };
  },
]);
