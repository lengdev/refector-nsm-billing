"use strict";
app.controller("dashboardController", [
  "$scope",
  "AuthData",
  "LoginService",
  "$window",
  "AuthenticationService",
  "$location",
  function (
    $scope,
    AuthData,
    LoginService,
    $window,
    AuthenticationService,
    $location,
  ) {
    var port = $location.port();
    $scope.absUrl =
      $location.protocol() + "://" + $location.host() + ":" + port + "/";
    $scope.Logout = function () {
      LoginService.LogOut();
    };
    var getTokenInfo = AuthenticationService.getTokenInfo();
    $scope.UserName = AuthData.authenticationData.userName;
    $scope.SysAndWH = new Object();
    $scope.SysAndWH = AuthenticationService.getAccessingSystem().sort(
      (a, b) => a.SystemID - b.SystemID,
    );
    var SysInfo = $scope.SysAndWH[0];
    if ($scope.SysAndWH.length == 1) {
      var AccSW = new Object();
      AccSW = {
        UserID: SysInfo.UserID,
        SystemID: SysInfo.SystemID,
        SystemName: SysInfo.SystemName,
        WarehouseID: SysInfo.WarehouseID,
        WarehouseName: SysInfo.WarehouseName,
        DefaultDashboardPage: SysInfo.DefaultDashboardPage,
      };
      setStorage("SysWLogo", SysInfo.WarehouseLogo);
      setStorage("AsideLightSidebar", SysInfo.AsideLightSidebar);
      setStorage("Body", SysInfo.Body);
      setStorage("DefaultDashboardPage", SysInfo.DefaultDashboardPage);
      AuthenticationService.setUserAccessingSystem(AccSW);
      AuthenticationService.setSystemID(SysInfo.SystemID);
      AuthenticationService.setWarehouseID(SysInfo.WarehouseID);

      setStorage("User_ID", SysInfo.UserID);
      setStorage("User_Name", getTokenInfo.userName);
      setStorage("User_Poistion", getTokenInfo.Poistion);
      setStorage("User_Full_Name", getTokenInfo.UserFullName);
      setStorage("System_ID", SysInfo.SystemID);
      setStorage("Warehouse_ID", SysInfo.WarehouseID);
      setStorage("DomainID", SysInfo.DomainID);

      $window.location.href =
        apiBaseUrl +
        "?SystemID=" +
        SysInfo.SystemID +
        "&WarehouseID=" +
        SysInfo.WarehouseID +
        "#!/" +
        SysInfo.DefaultDashboardPage;
    }
    $scope.Click = function (
      UserID,
      SystemID,
      SystemName,
      WarehouseID,
      WarehouseName,
      DefaultDashboardPage,
      WarehouseLogo,
      AsideLightSidebar,
      Body,
    ) {
      setStorage("SysWLogo", WarehouseLogo);
      setStorage("AsideLightSidebar", AsideLightSidebar);
      setStorage("Body", Body);
      var AccSW = new Object();
      AccSW = {
        UserID: UserID,
        SystemID: SystemID,
        SystemName: SystemName,
        WarehouseID: WarehouseID,
        WarehouseName: WarehouseName,
        DefaultDashboardPage: DefaultDashboardPage,
      };
      setStorage("DefaultDashboardPage", DefaultDashboardPage);
      AuthenticationService.setUserAccessingSystem(AccSW);
      AuthenticationService.setSystemID(SystemID);
      AuthenticationService.setWarehouseID(WarehouseID);
      var uri =
        apiBaseUrl +
        "?SystemID=" +
        SystemID +
        "&WarehouseID=" +
        WarehouseID +
        "#!/" +
        DefaultDashboardPage;

      setStorage("User_ID", UserID);
      setStorage("User_Name", getTokenInfo.userName);
      setStorage("User_Full_Name", getTokenInfo.UserFullName);
      setStorage("System_ID", SystemID);
      setStorage("Warehouse_ID", WarehouseID);
      setStorage("DomainID", SysInfo.DomainID);
      $window.location.href = uri;
    };
  },
]);
