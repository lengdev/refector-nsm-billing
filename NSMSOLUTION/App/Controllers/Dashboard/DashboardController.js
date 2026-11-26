"use strict";

app.factory("DashboardCacheService", [
    "$http",
    "$q",
    function ($http, $q) {
        var CACHE_TTL_MS = 5 * 60 * 1000; // 5 minutes — adjust as needed
        var cache = {}; // key -> { promise, timestamp }

        function getCached(key, fetchFn, forceRefresh) {
            var entry = cache[key];
            var isStale = entry && Date.now() - entry.timestamp > CACHE_TTL_MS;

            if (entry && !isStale && !forceRefresh) {
                return entry.promise;
            }

            var promise = fetchFn().catch(function (err) {
                delete cache[key]; // don't cache failures
                return $q.reject(err);
            });

            cache[key] = { promise: promise, timestamp: Date.now() };
            return promise;
        }

        function clear(key) {
            if (key) {
                delete cache[key];
            } else {
                cache = {};
            }
        }

        return {
            getCached: getCached,
            clear: clear,
        };
    },
]);

app.controller("DashboardCtrl", DashboardCtrl);

DashboardCtrl.$inject = [
    "$scope",
    "$http",
    "$timeout",
    "$window",
    "NotificationService",
    "DashboardCacheService",
];

function DashboardCtrl(
    $scope,
    $http,
    $timeout,
    $window,
    NotificationService,
    DashboardCacheService,
) {
    var vm = this;

    // ── View state ────────────────────────────────────────────
    vm.exchangeText = "Exchange rate";
    vm.exchangeRate = "—";

    vm.invoice = {
        InvoiceAmount: 0,
        PaidAmount: 0,
        UnpaidAmount: 0,
        DepositFee: 0,
        PartiallyPaid: 0,
    };

    vm.stats = {
        Customer: 0,
        CustActivated: 0,
        CustInactive: 0,
        CustLocked: 0,
        Terminate: 0,
        Companies: 0,
        CountnewCustomer: 0,
        CustomerSupport: 0,
        CustomerGeneratedInvoice: 0,
        CustomerInvoiceUnpaid: 0,
        CustomerNearlyExpired: 0,
        PPPoEUserOnline: 0,
    };

    vm.salesLastYear = 0;
    vm.salesThisYear = 0;
    vm.groupTotal = 0;
    vm.groupBreakdown = [];
    vm.revenueChangePct = 0;
    vm.paidPct = 0;

    vm.availableYears = [];
    vm.selectedYear = new Date().getFullYear();

    vm.pppoeDialogVisible = false;
    vm.pppoeSearch = "";
    vm.pppoeRows = [];

    // ── Public methods bound from the template ───────────────────
    vm.formatMoney = function (value) {
        return accounting.formatMoney(value);
    };

    vm.formatNumber = function (value) {
        return accounting.formatNumber(value);
    };

    vm.formatPercent = function (value) {
        var sign = value > 0 ? "+" : "";
        return sign + value.toFixed(1) + "%";
    };

    vm.formatMoneyStr = (value, decimals = 1, currency = "$") => {
        if (value == null || isNaN(value)) return `${currency}0`;

        const abs = Math.abs(value);
        const units = [
            { value: 1e12, suffix: "T" },
            { value: 1e6, suffix: "M" },
            { value: 1e3, suffix: "K" },
        ];

        for (const unit of units) {
            if (abs >= unit.value) {
                let num = parseFloat((abs / unit.value).toFixed(decimals));
                return `${value < 0 ? "-" : ""}${currency}${num}${unit.suffix}`;
            }
        }

        return `${value < 0 ? "-" : ""}${currency}${parseFloat(abs.toFixed(decimals))}`;
    };

    vm.showActivated = function (statusId) {
        $window.location.href = "#!/CustomerConnection?StatusID=" + statusId;
    };

    vm.onYearChange = function () {
        getDashboardInfo(vm.selectedYear);
    };

    // Not cached — reflects live router/session state.
    vm.getPPPoEUserOnline = function (showDetail) {
        showDetail = showDetail === true;

        $http
            .get("Home/GetPPPeEUserOnline", {
                params: { ShowDetail: showDetail },
                ignoreLoadingBar: true,
            })
            .then(function (response) {
                var payload = JSON.parse(response.data.Data);
                var data = payload.results[0];

                vm.stats.PPPoEUserOnline = data.CoutUserOnline;

                if (showDetail) {
                    vm.pppoeRows = buildPPPoERows(data.UserOnline || []);
                    vm.pppoeDialogVisible = true;
                }
            });
    };

    vm.cutSession = function (routerId, id, pppoeUserName) {
        var url =
            "Home/RemovePPPeEUserSession?RouterID=" + routerId + "&ID=" + id;

        DoSaveChangeSweetCofirm(url, "", function () {
            // Session cut can affect connectivity stats — drop the cache
            // so the next dashboard visit reflects the change.
            DashboardCacheService.clear("custConnectivity");
            $timeout(function () {
                vm.getPPPoEUserOnline(true);
            }, 1500);
        });
    };

    // ── Private helpers ────────────────────────────────────────
    function buildPPPoERows(userOnlineList) {
        return userOnlineList.map(function (raw, index) {
            var value = JSON.parse(raw);

            function find(key) {
                var entry = value.find(function (e) {
                    return e.Key === key;
                });
                return entry ? entry.Value : null;
            }

            return {
                no: index + 1,
                routerId: find("RouterID"),
                id: find(".id"),
                router: find("RouterName"),
                pppoeUser: find("user"),
                customerId: find("CustomerID"),
                userIp: find("user-ip"),
                profile: find("PPPoEProfileName"),
                remainDay: find("RemainDay"),
                callingStationId: find("calling-station-id"),
                fromTime: moment(find("from-time")).format(
                    "DD-MM-YYYY HH:mm:ss",
                ),
                tillTime: moment(find("till-time")).format(
                    "DD-MM-YYYY HH:mm:ss",
                ),
            };
        });
    }

    function getCustConnectivity() {
        DashboardCacheService.getCached("custConnectivity", function () {
            return $http.get("Home/GetCustConnectivity", {
                ignoreLoadingBar: true,
            });
        }).then(function (response) {
            var data = JSON.parse(response.data.data);

            vm.stats.Customer = data.Table[0].Customer;

            if (data.Table1.length)
                vm.stats.CustActivated = data.Table1[0].Customer;
            if (data.Table2.length)
                vm.stats.CustInactive = data.Table2[0].Customer;
            if (data.Table3.length)
                vm.stats.CustLocked = data.Table3[0].Customer;
            if (data.Table4.length)
                vm.stats.Terminate = data.Table4[0].Customer;
            if (data.Table5.length)
                vm.stats.Companies = data.Table5[0].Customer;
            if (data.Table6.length)
                vm.stats.CountnewCustomer = data.Table6[0].Customer;
            if (data.Table7.length)
                vm.stats.CustomerSupport = data.Table7[0].Ticket;
            if (data.Table9.length)
                vm.stats.CustomerGeneratedInvoice =
                    data.Table9[0].BillingInvoice;
            if (data.Table10.length)
                vm.stats.CustomerInvoiceUnpaid =
                    data.Table10[0].CustomerInvoice;
        });

        getTotalCustExpire();
    }

    function getTotalCustExpire() {
        // NotificationService already caches internally.
        NotificationService.getNearlyExpiredCustomers().then(function (body) {
            if (body.data && body.data.length > 0) {
                vm.stats.CustomerNearlyExpired = body.total;
            }
        });
    }

    function getCustPayment() {
        DashboardCacheService.getCached("custPayment", function () {
            return $http.get("Home/GetCustPayment", {
                ignoreLoadingBar: true,
            });
        }).then(function (response) {
            var data = JSON.parse(response.data.data)[0];
            vm.invoice = data;

            var invoiced = Number(data.InvoiceAmount) || 0;
            var paid = Number(data.PaidAmount) || 0;
            vm.paidPct = invoiced > 0 ? Math.round((paid / invoiced) * 100) : 0;
        });
    }

    var salesChart = null;
    var salesChartOverview = null;
    var groupChart = null;
    var lastDashboardResult = null;

    var themeObserver = new MutationObserver(function () {
        renderCharts();
    });
    themeObserver.observe(document.body, {
        attributes: true,
        attributeFilter: ["class"],
    });
    themeObserver.observe(document.documentElement, {
        attributes: true,
        attributeFilter: ["class"],
    });

    $scope.$on("$destroy", function () {
        themeObserver.disconnect();
        if (salesChart) salesChart.destroy();
        if (salesChartOverview) salesChartOverview.destroy();
        if (groupChart) groupChart.destroy();
    });

    function isDarkMode() {
        return (
            document.body.classList.contains("dark-mode") ||
            document.documentElement.classList.contains("dark-mode")
        );
    }

    function fillTwelveMonths(rows, label) {
        var byMonth = new Array(12).fill(0);
        var matchedAny = false;

        (rows || []).forEach(function (r) {
            var monthRaw = r.Monthnumber ?? r.MonthNumber ?? r.Month;
            var amountRaw = r.Amount;

            var idx = Number(monthRaw) - 1;
            var amount = Number(amountRaw);

            if (idx >= 0 && idx < 12 && !isNaN(amount)) {
                byMonth[idx] = amount;
                matchedAny = true;
            }
        });

        if (rows && rows.length && !matchedAny) {
            console.warn(
                "Home dashboard: couldn't match month/amount fields for " +
                    (label || "series") +
                    ". First row keys:",
                Object.keys(rows[0]),
                rows[0],
            );
        }

        return byMonth;
    }

    function getDashboardInfo(year) {
        vm.selectedYear = year || vm.selectedYear;
        var cacheKey = "dashboardInfo_" + vm.selectedYear;

        DashboardCacheService.getCached(cacheKey, function () {
            return $http.get("Home/LoadDashboardInfo", {
                params: { Year: vm.selectedYear },
                ignoreLoadingBar: true,
            });
        }).then(function (response) {
            lastDashboardResult = response.data;
            var result = lastDashboardResult;

            if (result.Table7[0]) {
                var exchange = result.Table7[0];
                vm.exchangeText = "Exchange rate " + exchange.Date;
                vm.exchangeRate = exchange.ExchangeRate;
            } else {
                vm.exchangeText = "Exchange rate";
                vm.exchangeRate = "—";
            }

            vm.groupTotal = result.Table4[0].CustomerID;

            if (vm.availableYears.length === 0) {
                var currentYear = new Date().getFullYear();
                var generatedYears = [];
                for (var y = currentYear; y >= currentYear - 5; y--) {
                    generatedYears.push(y);
                }

                var backendYears = [];
                if (result.Table11 && result.Table11.length) {
                    backendYears = result.Table11.map(function (r) {
                        return r.Yr;
                    }).filter(function (v) {
                        return typeof v === "number" && !isNaN(v);
                    });
                }

                vm.availableYears = (
                    backendYears.length ? backendYears : generatedYears
                ).sort(function (a, b) {
                    return b - a;
                });

                if (vm.availableYears.indexOf(vm.selectedYear) === -1) {
                    vm.selectedYear = vm.availableYears[0];
                }
            }

            renderCharts();

            if (window.Swal) Swal.close();
        });
    }

    function renderCharts() {
        if (!lastDashboardResult) return;

        var groupEl = document.querySelector("#lead-sources-chart");
        var revenueEl = document.querySelector("#revenue-trend-chart");
        var overviewEl = document.querySelector("#sales-overview");

        // Bail if the dashboard view isn't in the DOM (e.g. a queued
        // theme-change mutation fired after navigating away).
        if (!groupEl || !revenueEl || !overviewEl) return;

        var result = lastDashboardResult;
        var isDark = isDarkMode();

        var lastYear = fillTwelveMonths(result.Table5, "Table5 (last year)");
        var thisYear = fillTwelveMonths(result.Table6, "Table6 (this year)");

        vm.salesLastYear = lastYear.reduce(function (a, b) {
            return a + b;
        }, 0);
        vm.salesThisYear = thisYear.reduce(function (a, b) {
            return a + b;
        }, 0);
        vm.revenueChangePct =
            vm.salesLastYear > 0
                ? ((vm.salesThisYear - vm.salesLastYear) / vm.salesLastYear) *
                  100
                : 0;

        var donutPalette = isDark
            ? ["#e5e7eb", "#9ca3af", "#6b7280", "#374151", "#1f2937"]
            : ["#111827", "#4b5563", "#9ca3af", "#d1d5db", "#e5e7eb"];

        // ── Panel 1: Customers by Group -> donut ──────────────────
        var groupSeries = result.Table3.map(function (i) {
            return i.CustomerID;
        });
        var groupLabels = result.Table3.map(function (i) {
            return i.CustomerGroup;
        });

        vm.groupBreakdown = groupLabels.map(function (label, i) {
            return {
                label: label,
                value: groupSeries[i],
                color: donutPalette[i % donutPalette.length],
            };
        });

        if (groupChart) groupChart.destroy();
        groupChart = new ApexCharts(groupEl, {
            series: groupSeries,
            labels: groupLabels,
            chart: {
                type: "donut",
                height: 150,
                width: 150,
                fontFamily: "inherit",
            },
            colors: donutPalette,
            stroke: { width: 2, colors: [isDark ? "#1f2937" : "#fff"] },
            dataLabels: { enabled: false },
            legend: { show: false },
            plotOptions: {
                pie: {
                    donut: {
                        size: "72%",
                        labels: {
                            show: true,
                            total: {
                                show: true,
                                label: "total customers",
                                fontSize: "11px",
                                color: isDark ? "#9ca3af" : "#6b7280",
                                formatter: function () {
                                    return accounting.formatNumber(
                                        vm.groupTotal,
                                    );
                                },
                            },
                            value: {
                                fontSize: "22px",
                                fontWeight: 800,
                                color: isDark ? "#fff" : "#111827",
                                offsetY: -4,
                            },
                        },
                    },
                },
            },
            tooltip: {
                y: {
                    formatter: function (v) {
                        return accounting.formatNumber(v);
                    },
                },
            },
        });
        groupChart.render();

        // ── Panel 2: Revenue trend -> sparkline area ──────────────
        if (salesChart) salesChart.destroy();
        salesChart = new ApexCharts(revenueEl, {
            series: [{ name: "Revenue", data: thisYear }],
            chart: {
                type: "area",
                height: 110,
                sparkline: { enabled: true },
                fontFamily: "inherit",
            },
            colors: [isDark ? "#e5e7eb" : "#111827"],
            stroke: { curve: "straight", width: 2 },
            fill: {
                type: "gradient",
                gradient: {
                    shadeIntensity: 0,
                    opacityFrom: 0.25,
                    opacityTo: 0,
                    stops: [0, 100],
                },
            },
            tooltip: {
                theme: isDark ? "dark" : "light",
                x: { show: false },
                y: {
                    formatter: function (v) {
                        return accounting.formatMoney(v);
                    },
                },
                marker: { show: false },
            },
        });
        salesChart.render();

        // ── Sales Overview -> single-year trend with amount labels ──
        if (salesChartOverview) salesChartOverview.destroy();
        salesChartOverview = new ApexCharts(overviewEl, {
            series: [{ name: "Revenue", data: thisYear }],
            chart: {
                type: "area",
                height: 280,
                toolbar: { show: false },
                zoom: { enabled: false },
                fontFamily: "inherit",
            },
            colors: [isDark ? "#9ca3af" : "#6b7280"],
            stroke: { curve: "straight", width: 2 },
            markers: {
                size: 4,
                colors: [isDark ? "#9ca3af" : "#6b7280"],
                strokeColors: isDark ? "#1f2937" : "#fff",
                strokeWidth: 2,
                hover: { size: 6 },
            },
            fill: {
                type: "gradient",
                gradient: {
                    shadeIntensity: 1,
                    opacityFrom: 0.35,
                    opacityTo: 0,
                    stops: [0, 100],
                },
            },
            grid: {
                borderColor: isDark ? "#374151" : "#f2f3f5",
                yaxis: { lines: { show: true } },
                xaxis: { lines: { show: false } },
            },
            xaxis: {
                categories: [
                    "Jan",
                    "Feb",
                    "Mar",
                    "Apr",
                    "May",
                    "Jun",
                    "Jul",
                    "Aug",
                    "Sep",
                    "Oct",
                    "Nov",
                    "Dec",
                ],
                axisBorder: { show: false },
                axisTicks: { show: false },
                labels: { style: { colors: "#8a8f98", fontSize: "11px" } },
            },
            yaxis: { show: false },
            dataLabels: {
                enabled: true,
                offsetY: -12,
                style: {
                    fontSize: "11px",
                    fontWeight: 600,
                    colors: [isDark ? "#e5e7eb" : "#374151"],
                },
                background: { enabled: false },
                formatter: function (v) {
                    return vm.formatMoneyStr(v);
                },
            },
            tooltip: {
                theme: isDark ? "dark" : "light",
                y: {
                    formatter: function (v) {
                        return accounting.formatMoney(v);
                    },
                },
            },
        });
        salesChartOverview.render();
    }

    // ── Init ─────────────────────────────────────────────────────
    function init() {
        getCustConnectivity();
        getCustPayment();
        getDashboardInfo(vm.selectedYear);
    }

    init();
}
