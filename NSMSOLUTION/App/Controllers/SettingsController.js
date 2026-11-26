"use strict";
app.controller("SettingsCtrl", [
    "$rootScope",
    "$scope",
    "$timeout",
    "Form",
    function ($rootScope, $scope, $timeout, Form) {
        // ── Active menu ──────────────────────────────────────────────
        $scope.activeMenu = "company";
        $scope.menuLabel = {
            company: "Company Profile",
            invoice: "Invoice Template",
            nationality: "Nationality",
            user: "User Login",
            prefix: "Prefix",
            commune: "Commune",
            district: "District",
            province: "Province / City",
            cblock: "Customer Block",
            cgroup: "Customer Group",
            business: "Business Type",
            billing: "Billing Config",
            maintenance: "Maintenance",
        };
        $scope.setMenu = function (key) {
            $scope.activeMenu = key;
        };

        // ── Toast ────────────────────────────────────────────────────
        $scope.toast = { show: false, message: "", error: false };
        function showToast(msg, isError) {
            $scope.toast = { show: true, message: msg, error: !!isError };
            $timeout(function () {
                $scope.toast.show = false;
            }, 3000);
        }

        // ── Settings model ───────────────────────────────────────────
        $scope.settings = {
            companyName: "ISP Corp Ltd.",
            supportEmail: "support@ispcorp.com",
            supportPhone: "+855 23 000 000",
            timezone: "Asia/Phnom_Penh",
            address: "",
            currency: "USD",
            billingCycle: "monthly",
            dueDays: 15,
            taxRate: 10,
            lateFee: 5,
            twoFA: true,
            sessionTimeout: 30,
            maxAttempts: 5,
            pwdMinLen: 8,
        };

        // ── Payment Gateways ─────────────────────────────────────────
        $scope.gateways = [
            {
                name: "Cash / Manual",
                icon: "fas fa-money-bill-wave",
                enabled: true,
            },
            { name: "ABA Pay", icon: "fas fa-qrcode", enabled: true },
            { name: "ACLEDA KHQR", icon: "fas fa-qrcode", enabled: false },
            { name: "Wing Money", icon: "fas fa-mobile-alt", enabled: false },
            { name: "Stripe", icon: "fab fa-stripe-s", enabled: false },
        ];

        // ── Maintenance ──────────────────────────────────────────────
        $scope.maintenanceMode = false;
        $scope.maintenance = {
            title: "Scheduled System Maintenance",
            message:
                "We are currently performing scheduled maintenance to improve our services. We expect to be back online shortly.",
            endTime: "",
            style: "warning",
        };

        $scope.onMaintenanceToggle = function () {
            if ($scope.maintenanceMode) {
                showToast(
                    "Maintenance mode ENABLED — users are blocked.",
                    true,
                );
            } else {
                showToast("Maintenance mode disabled — system is online.");
            }
            $rootScope.$broadcast("maintenanceModeChanged", {
                active: $scope.maintenanceMode,
                maintenance: $scope.maintenance,
            });
        };

        var actionMessages = {
            cache: "Cache cleared successfully.",
            logs: "Old logs purged.",
            backup: "Database backup initiated.",
            sessions: "All active sessions terminated.",
        };
        $scope.runAction = function (action) {
            showToast(actionMessages[action] || "Action completed.");
        };

        // ── Save ─────────────────────────────────────────────────────
        $scope.save = function (section) {
            showToast("Settings saved successfully.");
            $rootScope.$broadcast("settingsSaved", {
                section: section,
                settings: $scope.settings,
            });
        };
    },
]);
