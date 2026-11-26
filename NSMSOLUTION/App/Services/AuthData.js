'use strict';
app.factory('AuthData', [ function () {
    var authDataFactory = {};
    var IsAuthenticated = false;
    //var accessToken = (typeof sessionStorage.getItem('TokenInfo') == 'undefined') ? "" : sessionStorage.getItem('TokenInfo');
    //if (accessToken != "" && accessToken != null) {
    //    IsAuthenticated = true;
    //}
    var _authentication = {
        IsAuthenticated: IsAuthenticated,
        userName: "",
        SysAndWH: "",
        SystemName: "",
        UserFullName: "",
        UserProfilePicture: "",
        DefaultDashboardPage:"",
    };
    authDataFactory.authenticationData = _authentication;

    //var _Accessing_SysAndWH = {
    //    SystemID: "",
    //    SystemName: "",
    //    WarehouseID: "",
    //    WarehouseName: ""
    //};
    //authDataFactory.AccessingSysAndWH = _Accessing_SysAndWH;

    return authDataFactory;
}]);