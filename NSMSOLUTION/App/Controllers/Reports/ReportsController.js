"use strict";

app.controller("RouterController", [
    "$scope",
    "$http",
    "$timeout",
    function ($scope, $http, $timeout) {
        const url = "CustomerReports/Router";

        // scope
        $scope.routers = [];
        $scope.loadingRouters = false;

        $scope.selectedRouterId = null;
        $scope.selectedRouterName = "";
        $scope.activeTab = "PPPoE";

        $scope.pppoeData = [];
        $scope.staticData = [];
        $scope.filters = { text: "" };
        $scope.filteredCount = 0;

        let pppoeDataRaw = [];
        let staticDataRaw = [];

        function getRouterFromDB() {
            $scope.loadingRouters = true;

            $http
                .get(url, { cache: true, ignoreLoadingBar: true })
                .then((response) => {
                    const data = JSON.parse(response.data.data);

                    $scope.routers = data;
                })
                .catch((error) => {
                    console.error(
                        "Failed to get router:",
                        error.status,
                        error.data,
                    );
                })
                .finally(() => {
                    $scope.loadingRouters = false;
                });
        }

        // List all routers
        getRouterFromDB();

        // Get router log
        $scope.loadingPPPoE = true;
        $scope.getRouterLogs = (routerId) => {
            getPPPoE(routerId);
            getStatic(routerId);
        };

        const getPPPoE = (routerId) => {
            const url = `api/mikrotik/pppoe/users?routerId=${routerId}`;
            $scope.loadingPPPoE = true;
            $http
                .get(url, {
                    ignoreLoadingBar: true,
                })
                .then((res) => {
                    pppoeDataRaw = (res.data && res.data.data) || [];
                    $scope.applyFilter();
                })
                .catch((err) => {
                    console.error(
                        "Failed to get pppoe router:",
                        err.status,
                        err.data,
                    );
                    pppoeDataRaw = [];
                    $scope.applyFilter();
                })
                .finally(() => {
                    $scope.loadingPPPoE = false;
                });
        };

        $scope.loadingStatic = true;
        const getStatic = (routerId) => {
            const url = `api/mikrotik/vlans?routerId=${routerId}`;
            $scope.loadingStatic = true;
            $http
                .get(url, {
                    ignoreLoadingBar: true,
                })
                .then((res) => {
                    staticDataRaw = (res.data && res.data.data) || [];
                    $scope.applyFilter();
                })
                .catch((err) => {
                    console.error(
                        "Failed to get vlan router:",
                        err.status,
                        err.data,
                    );
                    staticDataRaw = [];
                    $scope.applyFilter();
                })
                .finally(() => {
                    $scope.loadingStatic = false;
                });
        };

        // Filtering — matches against the columns visible in each tab's table
        $scope.applyFilter = () => {
            const term = ($scope.filters.text || "").trim().toLowerCase();

            const filteredPppoe = !term
                ? pppoeDataRaw
                : pppoeDataRaw.filter((row) =>
                      [
                          row.id,
                          row.name,
                          row.group,
                          row.actualProfile,
                          row.comment,
                      ].some((field) =>
                          String(field || "")
                              .toLowerCase()
                              .includes(term),
                      ),
                  );
            $scope.pppoeData = filteredPppoe;

            const filteredStatic = !term
                ? staticDataRaw
                : staticDataRaw.filter((row) =>
                      [row.id, row.name, row.iface, row.comment].some((field) =>
                          String(field || "")
                              .toLowerCase()
                              .includes(term),
                      ),
                  );
            $scope.staticData = filteredStatic;

            $scope.filteredCount =
                $scope.activeTab === "PPPoE"
                    ? filteredPppoe.length
                    : filteredStatic.length;
        };

        // Handle active state
        $scope.selectRouter = (router) => {
            const routerId = router.RouterID;
            const routerName = router.RouterName;

            $scope.selectedRouterId = routerId;
            $scope.selectedRouterName = routerName;
            $scope.filters.text = "";

            $scope.getRouterLogs(routerId);
        };

        $scope.selectConnectionType = (tab, routerId) => {
            $scope.activeTab = tab;
            $scope.filters.text = "";
            if (tab === "PPPoE") getPPPoE(routerId);
            else getStatic(routerId);
            $scope.applyFilter();
        };
    },
]);
