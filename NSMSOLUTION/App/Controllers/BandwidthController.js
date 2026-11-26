(function () {
    "use strict";

    // NOTE: ShowCustomerDetail now returns JSON (like ShowBandwidth /
    // ShowCustomer), so it's rendered via ng-repeat in bandwidth.html
    // instead of being injected as an HTML fragment. ShowCustomerByBlock
    // is left in place below in case it's still used elsewhere, but it's
    // no longer wired into the customer-detail table — let us know if you
    // want a drill-down action added to a table row instead.
    window.ShowCustomerByBlock = function (
        CustomerGroupID,
        CustomerTypeID,
        startDate,
        endDate,
    ) {
        var $http = angular.element(document.body).injector().get("$http");
        var ctx = window.__bwContext || {};

        $http
            .get("CustomerReports/ShowCustomerByBlock", {
                params: {
                    CustomerGroupID: CustomerGroupID,
                    CustomerTypeID: CustomerTypeID,
                    startDate: startDate,
                    endDate: endDate,
                    Bandwidth: ctx.Bandwidth,
                    CustomerID: ctx.CustomerID,
                    RouterID: ctx.RouterID,
                    ConnectionType: ctx.ConnectionType,
                },
                dataType: "json",
                ignoreLoadingBar: true,
            })
            .then(function (response) {
                var el = document.getElementById("CustomerByBlock");
                if (el) el.innerHTML = response.data;
            })
            .catch(function (error) {
                console.error("Failed to load customer block", error);
            });
    };

    app.controller("BandwidthController", [
        "$rootScope",
        "$scope",
        "$http",
        "$location",
        "$q",
        "$sce",
        "$timeout",
        "$cacheFactory",
        "$httpParamSerializer",
        function (
            $rootScope,
            $scope,
            $http,
            $location,
            $q,
            $sce,
            $timeout,
            $cacheFactory,
            $httpParamSerializer,
        ) {
            var vm = this;

            // ===== Cache =====
            // A dedicated $http cache instance (Angular's built-in
            // $cacheFactory), not the shared default $http cache — so
            // clearing it doesn't nuke every other controller's cached
            // requests app-wide.
            //
            // $cacheFactory('bandwidth') returns the SAME instance if a
            // cache with this name already exists (e.g. controller was
            // torn down and recreated on a route change), so entries
            // persist across navigation just like the old module-level
            // object did — but it's Angular managing storage/lookup now,
            // not us.
            var bwCache =
                $cacheFactory.get("bandwidth") || $cacheFactory("bandwidth");

            // Angular's cache has no built-in expiry (nothing does — SQL
            // Server doesn't push change notifications to the browser
            // either, so *something* has to decide "this is old enough,
            // refetch"). This is just that one small piece: a timestamp
            // per request URL, checked before each fetch.
            var cacheTimestamps = {};
            var CACHE_TTL_MS = 3 * 60 * 1000; // 3 minutes

            // Theme is no longer a per-component concern — this page reads
            // colors from body.dark-mode via CSS custom properties, same
            // as the rest of the app.

            // ===== State =====
            vm.bandwidthList = [];
            vm.customerGroups = [];
            vm.customerDetails = [];
            vm.selectedBandwidth = null;
            vm.selectedGroupId = null;
            vm.loadingBandwidth = false;
            vm.loadingGroups = false;
            vm.loadingDetail = false;
            vm.collapsed = false;

            // ===== Public methods =====
            vm.showCustomersByBandwidth = showCustomersByBandwidth;
            vm.selectGroup = selectGroup;
            vm.toggleCollapsed = toggleCollapsed;
            vm.totalConnections = totalConnections;
            vm.topBandwidth = topBandwidth;
            vm.totalGroupCustomers = totalGroupCustomers;
            vm.refresh = refresh;

            activate();

            // ===== Implementation =====

            function activate() {
                loadBandwidthData();
            }

            // Reads a field regardless of whether the API returns PascalCase
            // (.NET's default JSON casing, e.g. "Bandwidth") or camelCase
            // ("bandwidth"). Avoids blank values when casing doesn't match.
            function pick(obj, camelKey, pascalKey) {
                if (obj[camelKey] !== undefined) return obj[camelKey];
                if (obj[pascalKey] !== undefined) return obj[pascalKey];
                return null;
            }

            function normalizeBandwidthItem(raw) {
                return {
                    bandwidth: pick(raw, "bandwidth", "Bandwidth"),
                    // SQL aliases the count column as "Connection", not "CustomerCount"
                    customerCount: pick(raw, "connection", "Connection"),
                };
            }

            // Shared response-shape handler for every JSON endpoint here.
            // Handles:
            //  1. A raw array: [{...}, ...]
            //  2. A wrapped object with a JSON string: { data: "[...]" }
            //  3. A wrapped object with an already-parsed array: { data: [...] }
            function normalizeResponse(data) {
                if (!data) return [];
                if (Array.isArray(data)) return data;
                if (data.data !== undefined) {
                    if (typeof data.data === "string") {
                        try {
                            var parsed = JSON.parse(data.data);
                            return Array.isArray(parsed) ? parsed : [];
                        } catch (e) {
                            console.error(
                                "Failed to parse response data string",
                                e,
                            );
                            return [];
                        }
                    }
                    if (Array.isArray(data.data)) return data.data;
                }
                return [];
            }

            function normalizeGroupItem(raw) {
                return {
                    customerGroupId: pick(
                        raw,
                        "customerGroupId",
                        "CustomerGroupID",
                    ),
                    customerGroup: pick(raw, "customerGroup", "CustomerGroup"),
                    customer: pick(raw, "customer", "Customer") || 0,
                };
            }

            // Builds the exact URL string Angular's $http will use as the
            // cache key internally (url + '?' + sorted, serialized params),
            // so we can look up / expire our own timestamp for the same
            // entry Angular is storing.
            function buildCacheKey(url, params) {
                var paramsStr = $httpParamSerializer(params || {});
                return paramsStr ? url + "?" + paramsStr : url;
            }

            // Single shared "cached GET" used by all three endpoints below.
            // cache: bwCache hands storage/retrieval entirely to Angular's
            // $cacheFactory — we're not storing response data ourselves
            // anywhere. We only touch cacheTimestamps to decide whether an
            // existing cache entry is old enough to evict before asking.
            function cachedGet(url, params) {
                var cacheKey = buildCacheKey(url, params);
                var lastFetched = cacheTimestamps[cacheKey];
                var isStale =
                    lastFetched && Date.now() - lastFetched > CACHE_TTL_MS;

                if (isStale) {
                    bwCache.remove(cacheKey);
                    delete cacheTimestamps[cacheKey];
                }

                return $http
                    .get(url, {
                        params: params,
                        dataType: "json",
                        ignoreLoadingBar: true,
                        cache: bwCache,
                    })
                    .then(function (response) {
                        cacheTimestamps[cacheKey] = Date.now();
                        return response;
                    });
            }

            function loadBandwidthData() {
                var asOfDate = moment().format("YYMMDD");

                vm.loadingBandwidth = true;
                vm.selectedBandwidth = null;
                vm.customerGroups = [];
                vm.customerDetails = [];

                cachedGet("CustomerReports/ShowBandwidth")
                    .then(function (response) {
                        var rows = normalizeResponse(response.data);
                        vm.bandwidthList = rows.map(normalizeBandwidthItem);
                    })
                    .catch(function (error) {
                        console.error("Failed to load bandwidth data", error);
                        vm.bandwidthList = [];
                    })
                    .finally(function () {
                        vm.loadingBandwidth = false;
                    });
            }

            function showCustomersByBandwidth(bandwidth) {
                vm.selectedBandwidth = bandwidth;
                vm.collapsed = false;
                vm.selectedGroupId = null;
                vm.customerGroups = [];
                vm.customerDetails = [];
                vm.loadingGroups = true;

                var asOfDate = moment().format("YYMMDD");

                // Shared context read by the legacy global bridge function
                // (window.ShowCustomerByBlock), kept for backward compatibility.
                window.__bwContext = {
                    Bandwidth: bandwidth,
                    CustomerID: vm.customerId || "",
                    RouterID: vm.routerId || "",
                    ConnectionType: vm.connectionType || "",
                    startDate: asOfDate,
                    endDate: asOfDate,
                };

                cachedGet("CustomerReports/ShowCustomer", {
                    Bandwidth: bandwidth,
                })
                    .then(function (response) {
                        var rows = normalizeResponse(response.data);
                        vm.customerGroups = rows.map(normalizeGroupItem);
                        if (vm.customerGroups.length) {
                            selectGroup(vm.customerGroups[0]);
                        }
                    })
                    .catch(function (error) {
                        console.error(
                            "Failed to load customer groups for bandwidth",
                            bandwidth,
                            error,
                        );
                        vm.customerGroups = [];
                    })
                    .finally(function () {
                        vm.loadingGroups = false;
                    });
            }

            function selectGroup(group) {
                vm.selectedGroupId = group.customerGroupId;
                vm.loadingDetail = true;
                vm.customerDetails = [];

                var ctx = window.__bwContext;

                cachedGet("CustomerReports/ShowCustomerDetail", {
                    CustomerGroupID: group.customerGroupId,
                    Bandwidth: ctx.Bandwidth,
                })
                    .then(function (response) {
                        var rows = normalizeResponse(response.data);
                        vm.customerDetails = rows;
                    })
                    .catch(function (error) {
                        console.error("Failed to load customer detail", error);
                        vm.customerDetails = [];
                    })
                    .finally(function () {
                        vm.loadingDetail = false;
                    });
            }

            function toggleCollapsed() {
                vm.collapsed = !vm.collapsed;
            }

            function totalConnections() {
                return vm.bandwidthList.reduce(function (sum, item) {
                    return sum + (item.customerCount || 0);
                }, 0);
            }

            function topBandwidth() {
                if (!vm.bandwidthList.length) return 0;
                return vm.bandwidthList.reduce(function (max, item) {
                    return item.bandwidth > max ? item.bandwidth : max;
                }, 0);
            }

            function totalGroupCustomers() {
                return vm.customerGroups.reduce(function (sum, g) {
                    return sum + (g.customer || 0);
                }, 0);
            }

            // Manually bust the cache and pull fresh data — wire a button
            // to this if/when you want a visible "Refresh" control.
            function refresh() {
                bwCache.removeAll();
                cacheTimestamps = {};

                var currentBandwidth = vm.selectedBandwidth;

                loadBandwidthData();
                if (currentBandwidth !== null) {
                    // Re-selects the same tier with fresh data. Note this
                    // resets the group selection to the first group again,
                    // same as opening the tier fresh — acceptable for an
                    // explicit "refresh" action.
                    showCustomersByBandwidth(currentBandwidth);
                }
            }
        },
    ]);
})();
