"use strict";

// =====================================================================
// Helper: generic data loader (same contract as loadData() in
// CreateCustomerV2Ctrl — kept identical so both controllers behave the
// same way on failure).
// =====================================================================
function loadData($http, endpoint) {
  return $http
    .get(endpoint)
    .then(function (response) {
      return response.status === 200 ? response.data || [] : [];
    })
    .catch(function (error) {
      console.error("Failed to load " + endpoint + ":", error);
      return [];
    });
}

function _filterByName(list, term, field, maxResults) {
  maxResults = maxResults || 20;
  const q = (term || "").toLowerCase();

  if (!q) return list.slice(0, maxResults);

  const matches = [];
  for (let i = 0; i < list.length && matches.length < maxResults; i++) {
    const value = list[i][field];
    if (value && value.toLowerCase().includes(q)) matches.push(list[i]);
  }
  return matches;
}

// Single-field / joined-field typeahead-input-formatter factories
// (same pattern used for formatSeller/formatCustomerGroup/etc.)
function makeFieldFormatter(fieldName) {
  return (obj) => (obj ? obj[fieldName] || "" : "");
}
function makeJoinFormatter(fields, separator = " ") {
  return (obj) => {
    if (!obj) return "";
    return fields
      .map((f) => obj[f])
      .filter(Boolean)
      .join(separator);
  };
}

// =====================================================================
// Step 2: Product / Serial selection rows
// =====================================================================
class ProductSerialRows {
  constructor() {
    this.rowUid = 0;
    this.rows = [this.blank()];
  }

  blank() {
    this.rowUid++;
    return {
      _id: this.rowUid,
      item: null, // { itemID, itemName, um }
      serial: null, // { serialDetailID, serialNo, macAddress, warehouseName }
      availableSerials: [],
      qty: 1,
      isLoadingSerials: false,
    };
  }

  add() {
    this.rows.push(this.blank());
  }

  remove(index) {
    this.rows.splice(index, 1);
    if (!this.rows.length) this.rows.push(this.blank());
  }

  // Clears the previously-picked serial whenever the item changes,
  // since a serial number only makes sense for the item it belongs to
  onItemChanged(row) {
    row.serial = null;
    row.availableSerials = [];
  }

  isComplete() {
    return this.rows.some((r) => r.item && r.serial);
  }

  // Shapes rows for the activation payload; drops incomplete rows
  toPayload() {
    return this.rows
      .filter((r) => r.item && r.serial)
      .map((r) => ({
        ItemID: r.item.itemID,
        ItemName: r.item.itemName,
        SerialDetailID: r.serial.serialDetailID,
        SerialNo: r.serial.serialNo,
        MACAddress: r.serial.macAddress || "",
        Qty: parseFloat(r.qty) || 1,
      }));
  }
}

// =====================================================================
// Step 3: Network configuration
// Encapsulates the PPPoE / Static / Manual switching + VLAN/IP state
// that the legacy jQuery version wired by hand; this keeps all of that
// logic in one testable object instead of scattered DOM calls.
// =====================================================================
class NetworkConfig {
  constructor() {
    this.router = null; // { routerID, routerName }
    this.connectionType = "PPPoE"; // "PPPoE" | "Static" | "Manual"

    // PPPoE
    this.pppoeUsername = "";
    this.pppoePassword = "";
    this.pppoeIPAddress = "";
    this.pppoeProfile = null; // { profileName, userGroup }

    // Static
    this.vlan = null; // { vlanID, vlanName }
    this.vlanIPAddress = "";
    this.network = "";
    this.customerIP = "";
    this.gatewayIP = "";
    this.subnetIP = "";
    this.dns1 = "103.134.177.26";
    this.dns2 = "8.8.8.8";

    // Wifi / device, common to all connection types
    this.device = {
      name: "",
      routerIP: "192.168.1.1",
      wifiName: "",
      wifiKey: "",
      loginName: "user",
      loginPassword: "user",
    };
  }

  // Random, reasonably strong PPPoE password — mirrors what the
  // legacy "Generate Password" button was meant to do, done
  // client-side so no extra round trip is required.
  generatePppoePassword() {
    const chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
    let out = "";
    for (let i = 0; i < 10; i++) {
      out += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    this.pppoePassword = out;
    return out;
  }

  // Field-level validation per connection type; used both for the
  // "Next" step gate and before submit.
  validate() {
    const errors = [];
    if (!this.router) errors.push("Router is required.");

    if (this.connectionType === "PPPoE") {
      if (!this.pppoeUsername) errors.push("PPPoE User Name is required.");
      if (!this.pppoePassword) errors.push("PPPoE Password is required.");
    } else if (this.connectionType === "Static") {
      if (!this.vlan) errors.push("Vlan Name is required.");
      if (!this.vlanIPAddress) errors.push("Address IP is required.");
      if (!this.customerIP) errors.push("Customer IP is required.");
      if (!this.gatewayIP) errors.push("Default Gateway IP is required.");
      if (!this.subnetIP) errors.push("Subnet IP is required.");
      if (!this.dns1) errors.push("DNS1 is required.");
      if (!this.dns2) errors.push("DNS2 is required.");
    }
    // "Manual" has no required network fields by design.

    return errors;
  }

  toPayload() {
    const base = {
      RouterID: this.router ? this.router.routerID : null,
      RouterName: this.router ? this.router.routerName : "",
      ConnectionType: this.connectionType,
      DeviceName: this.device.name,
      WifiIP: this.device.routerIP,
      WifiName: this.device.wifiName,
      WifiKey: this.device.wifiKey,
      WifiLoginName: this.device.loginName,
      WifiLoginPassword: this.device.loginPassword,
    };

    if (this.connectionType === "PPPoE") {
      return Object.assign(base, {
        PPPoEUserName: this.pppoeUsername,
        PPPoEPassword: this.pppoePassword,
        PPPoEIPAddress: this.pppoeIPAddress,
        PPPoEProfileName: this.pppoeProfile
          ? this.pppoeProfile.profileName
          : "",
        UserGroup: this.pppoeProfile ? this.pppoeProfile.userGroup : "",
      });
    }

    if (this.connectionType === "Static") {
      return Object.assign(base, {
        VLanID: this.vlan ? this.vlan.vlanID : null,
        VLanName: this.vlan ? this.vlan.vlanName : "",
        VLanIPAddress: this.vlanIPAddress,
        Network: this.network,
        IPAddress: this.customerIP,
        GetwayIP: this.gatewayIP,
        SubnetIP: this.subnetIP,
        DNS1: this.dns1,
        DNS2: this.dns2,
      });
    }

    return base; // Manual
  }
}

// =====================================================================
// Controller
// =====================================================================
app.controller("ActivateServiceCtrl", [
  "$rootScope",
  "$scope",
  "$http",
  "$element",
  "$timeout",
  function ($rootScope, $scope, $http, $element, $timeout) {
    $rootScope.title = "Activate Service";

    // ---------- Wizard state ----------
    // 1 = Customer Info, 2 = Product & Serial, 3 = Network Config
    $scope.currentStep = 1;
    $scope.steps = [
      { id: 1, label: "Customer Info" },
      { id: 2, label: "Product & Serial" },
      { id: 3, label: "Network Config" },
    ];

    $scope.goToStep = function (step) {
      // Only allow jumping to a step that's already reachable
      // (current or earlier), or forward if the ones before are valid.
      if (step < $scope.currentStep) {
        $scope.currentStep = step;
        return;
      }
      for (let s = $scope.currentStep; s < step; s++) {
        if (!$scope.isStepValid(s)) return;
      }
      $scope.currentStep = step;
    };

    $scope.nextStep = function () {
      if (!$scope.isStepValid($scope.currentStep)) {
        $scope.showStepErrors = true;
        return;
      }
      $scope.showStepErrors = false;
      if ($scope.currentStep < 3) $scope.currentStep++;
    };

    $scope.prevStep = function () {
      $scope.showStepErrors = false;
      if ($scope.currentStep > 1) $scope.currentStep--;
    };

    $scope.isStepValid = function (step) {
      if (step === 1) return !!$scope.customer;
      if (step === 2) return productRows.isComplete();
      if (step === 3) return networkConfig.validate().length === 0;
      return true;
    };

    // ---------- Step 1: Customer info (read-only summary) ----------
    $scope.customer = null;
    $scope.isLoadingCustomer = true;
    $scope.loadError = "";

    const customerID = $scope.activateCustomerID || null; // set by caller before controller runs

    loadData(
      $http,
      "api/ActivateService/Init?customerId=" + (customerID || ""),
    ).then(function (data) {
      $scope.isLoadingCustomer = false;

      if (!data || !data.customer) {
        $scope.loadError = "Unable to load customer for activation.";
        return;
      }

      $scope.customer = data.customer;
      $scope.routerList = data.routers || [];
      $scope.productList = data.products || [];
    });

    // ---------- Step 2: Product & Serial ----------
    const productRows = new ProductSerialRows();
    $scope.productRows = productRows.rows;

    $scope.addProductRow = function () {
      productRows.add();
    };
    $scope.removeProductRow = function (index) {
      productRows.remove(index);
    };

    $scope.formatItem = makeFieldFormatter("itemName");
    $scope.formatSerial = makeJoinFormatter(["serialNo"]);

    $scope.getItems = function (term) {
      return _filterByName($scope.productList || [], term, "itemName");
    };

    // Loads available (unassigned) serials for the row's chosen item
    $scope.onItemSelected = function (row, item) {
      row.item = item;
      productRows.onItemChanged(row);
      row.isLoadingSerials = true;

      loadData(
        $http,
        "api/ActivateService/ProductSerials?itemId=" + item.itemID,
      ).then(function (serials) {
        row.availableSerials = serials || [];
        row.isLoadingSerials = false;
      });
    };

    $scope.getSerials = function (row, term) {
      return _filterByName(row.availableSerials || [], term, "serialNo");
    };

    $scope.onSerialSelected = function (row, serial) {
      row.serial = serial;
    };

    // ---------- Step 3: Network configuration ----------
    const networkConfig = new NetworkConfig();
    $scope.net = networkConfig;
    $scope.routerList = $scope.routerList || [];
    $scope.vlanList = [];
    $scope.pppoeUserList = [];

    $scope.formatRouter = makeFieldFormatter("routerName");
    $scope.formatVlan = makeFieldFormatter("vlanName");

    $scope.getRouters = function (term) {
      return _filterByName($scope.routerList || [], term, "routerName");
    };

    $scope.onRouterSelected = function (router) {
      networkConfig.router = router;
      networkConfig.vlan = null;
      $scope.vlanList = [];
      $scope.pppoeUserList = [];

      if (networkConfig.connectionType === "Static") {
        $scope.loadVlans();
      } else if (networkConfig.connectionType === "PPPoE") {
        $scope.loadPppoeProfile();
      }
    };

    $scope.onConnectionTypeChanged = function () {
      networkConfig.vlan = null;
      if (networkConfig.connectionType === "Static" && networkConfig.router) {
        $scope.loadVlans();
      } else if (
        networkConfig.connectionType === "PPPoE" &&
        networkConfig.router
      ) {
        $scope.loadPppoeProfile();
      }
    };

    $scope.loadVlans = function () {
      if (!networkConfig.router) return;
      loadData(
        $http,
        "api/ActivateService/Vlans?routerId=" + networkConfig.router.routerID,
      ).then(function (vlans) {
        $scope.vlanList = vlans || [];
      });
    };

    $scope.getVlans = function (term) {
      return _filterByName($scope.vlanList || [], term, "vlanName");
    };

    // Selecting a VLAN auto-fills address/network/DNS the same way
    // the legacy LoadVlanIPAddress() call did
    $scope.onVlanSelected = function (vlan) {
      networkConfig.vlan = vlan;

      loadData(
        $http,
        "api/ActivateService/VlanIpAddress?vlanName=" +
          encodeURIComponent(vlan.vlanName) +
          "&routerId=" +
          networkConfig.router.routerID,
      ).then(function (result) {
        if (!result) return;
        networkConfig.vlanIPAddress = result.addressIP || "";
        networkConfig.network = result.network || "";
        networkConfig.customerIP = result.customerIP || "";
        networkConfig.subnetIP = result.subnetMask || "";
        networkConfig.gatewayIP = result.defaultGatewayIP || "";
        if (result.dns1) networkConfig.dns1 = result.dns1;
        if (result.dns2) networkConfig.dns2 = result.dns2;
      });
    };

    $scope.loadPppoeProfile = function () {
      if (!networkConfig.router) return;
      loadData(
        $http,
        "api/ActivateService/PppoeProfile?routerId=" +
          networkConfig.router.routerID,
      ).then(function (result) {
        if (!result) return;
        networkConfig.pppoeProfile = {
          profileName: result.profileName || "",
          userGroup: result.userGroup || "",
        };
      });
    };

    $scope.generatePassword = function () {
      networkConfig.generatePppoePassword();
    };

    // Confirms a static IP isn't already assigned on this router
    // before allowing activation to proceed
    function checkIpAvailable() {
      if (networkConfig.connectionType !== "Static") {
        return Promise.resolve(true);
      }
      return $http
        .get("api/ActivateService/CheckIpExisting", {
          params: {
            oldIp: "",
            newIp: networkConfig.customerIP,
            routerId: networkConfig.router
              ? networkConfig.router.routerID
              : null,
          },
        })
        .then(function (response) {
          const result = response.data;
          if (!result || !result.isAvailable) {
            $scope.submitError =
              (result && result.message) ||
              "This IP address is already in use.";
            return false;
          }
          return true;
        })
        .catch(function () {
          $scope.submitError = "Unable to verify IP address availability.";
          return false;
        });
    }

    // ---------- Submit: validation + payload build + POST ----------
    $scope.isSaving = false;
    $scope.submitError = "";

    $scope.submitActivation = function () {
      if ($scope.isSaving) return;

      if (
        !$scope.isStepValid(1) ||
        !$scope.isStepValid(2) ||
        !$scope.isStepValid(3)
      ) {
        $scope.showStepErrors = true;
        // Jump back to the first invalid step so the user sees why
        for (let s = 1; s <= 3; s++) {
          if (!$scope.isStepValid(s)) {
            $scope.currentStep = s;
            break;
          }
        }
        return;
      }

      $scope.isSaving = true;
      $scope.submitError = "";

      checkIpAvailable().then(function (ok) {
        if (!ok) {
          $scope.isSaving = false;
          return;
        }

        const payload = {
          CustomerID: $scope.customer.customerID,
          Products: productRows.toPayload(),
          NetworkConfig: networkConfig.toPayload(),
        };

        $http
          .post("api/ActivateService/Activate", payload)
          .then(function (response) {
            const result =
              typeof response.data === "string"
                ? JSON.parse(response.data)
                : response.data;

            if (!result || result.success !== true) {
              throw new Error(
                (result && result.message) || "Unable to activate service.",
              );
            }

            if (typeof DialogSweetAlert === "function") {
              DialogSweetAlert(
                "success",
                result.message || "Service activated successfully.",
              );
            }
            if (typeof CloseDialogBox === "function") CloseDialogBox();
          })
          .catch(function (error) {
            const response = error && error.data;
            const message =
              response && response.message
                ? response.message
                : error.message || "Unable to activate service.";

            $scope.submitError = message;
            if (typeof DialogSweetAlert === "function")
              DialogSweetAlert("error", message);
          })
          .finally(function () {
            $scope.isSaving = false;
          });
      });
    };

    // ---------- External Save button ----------
    // Same convention as CreateCustomerV2Ctrl: the activate button
    // lives in the modal footer outside the Angular form, bound
    // manually and wrapped in $scope.$apply.
    const activateBtn = document.getElementById("ActivateService");
    if (activateBtn) {
      const handleActivateClick = function (event) {
        if (event) event.preventDefault();
        $scope.$apply(function () {
          $scope.submitActivation();
        });
      };
      activateBtn.addEventListener("click", handleActivateClick);
      $scope.$on("$destroy", function () {
        activateBtn.removeEventListener("click", handleActivateClick);
      });
    }
  },
]);
