(function () {
    "use strict";

    app.service("AuthenticationService", [
        "$window",
        "AuthData",
        "localStorageService",
        function ($window, AuthData, localStorageService) {
            var tokenInfo = "";
            var accessingSystem = "";
            var Menu = "";
            this.setMenu = function (Menu) {
                setStorage("Menu", Menu);
            };
            this.setSystemID = function (SystemID) {
                setStorage("SystemID", SystemID);
            };
            this.setWarehouseID = function (WarehouseID) {
                setStorage("WarehouseID", WarehouseID);
            };
            this.setDomainID = function (DomainID) {
                setStorage("DomainID", DomainID);
            };
            this.getSystemID = function () {
                var SystemID = getStorage("SystemID");
                if (SystemID == "") {
                    this.removeToken();
                    return false;
                } else {
                    return getStorage("SystemID");
                }
            };
            this.getWarehouseID = function () {
                var WarehouseID = getStorage("WarehouseID");
                if (WarehouseID == "") {
                    this.removeToken();
                    return false;
                } else {
                    return getStorage("WarehouseID");
                }
            };
            this.getDomainID = function () {
                var DomainID = getStorage("DomainID");
                if (DomainID == "") {
                    this.removeToken();
                    return false;
                } else {
                    return getStorage("DomainID");
                }
            };
            this.setAccessingSystem = function (data) {
                setStorage("accessingSystem", data);
            };
            this.setUserAccessingSystem = function (data) {
                accessingSystem = JSON.stringify(data);
                setStorage("userAccessingSystem", accessingSystem);
            };
            this.getAccessingSystem = function () {
                var accessingSystem = getStorage("accessingSystem");
                if (accessingSystem == "") {
                    this.removeToken();
                    return false;
                } else {
                    return JSON.parse(accessingSystem);
                }
            };
            this.getUserAccessingSystem = function () {
                var accessingSystem = getStorage("userAccessingSystem");
                if (accessingSystem == "") {
                    this.removeToken();
                    return false;
                } else {
                    return JSON.parse(accessingSystem);
                }
            };
            this.getMenu = function () {
                var Menu = getStorage("Menu");
                if (Menu == "") {
                    this.removeToken();
                    return false;
                } else {
                    return JSON.parse(Menu);
                }
            };
            this.getUserMenu = function () {
                var Menu = getStorage("UserMenu");
                if (Menu == "") {
                    this.removeToken();
                    return false;
                } else {
                    return JSON.parse(Menu);
                }
            };
            this.setTokenInfo = function (data) {
                tokenInfo = JSON.stringify(data);
                accessingSystem = JSON.parse(tokenInfo).SysAndWH;
                Menu = JSON.parse(tokenInfo).Menu;
                setStorage("UserMenu", JSON.parse(tokenInfo).UserMenu);
                setStorage("Menu", Menu);
                setStorage("TokenInfo", tokenInfo);
                this.setAccessingSystem(accessingSystem);
                var newObj = {};
                accessingSystem = JSON.parse(accessingSystem);
                setStorage("DomainID", accessingSystem[0].DomainID);
                newObj = accessingSystem[0];
                newObj["Poistion"] = JSON.parse(tokenInfo).Poistion;
                newObj["UserFullName"] = JSON.parse(tokenInfo).UserFullName;
                newObj["UserProfilePicture"] =
                    JSON.parse(tokenInfo).UserProfilePicture;
                this.setUserAccessingSystem(newObj);
                AuthData.authenticationData.IsAuthenticated = true;
                AuthData.authenticationData.userName =
                    JSON.parse(tokenInfo).userName;
                AuthData.authenticationData.UserFullName =
                    JSON.parse(tokenInfo).UserFullName;
                AuthData.authenticationData.UserProfilePicture =
                    JSON.parse(tokenInfo).UserProfilePicture;
                setStorage("userName", JSON.parse(tokenInfo).userName);
                AuthData.authenticationData.SysAndWH =
                    JSON.parse(tokenInfo).SysAndWH;
                return tokenInfo;
            };
            this.getTokenInfo = function () {
                var accessToken = getStorage("TokenInfo");
                if (accessToken != "") {
                    accessToken = JSON.parse(accessToken);
                    AuthData.authenticationData.IsAuthenticated = true;
                    AuthData.authenticationData.userName = accessToken.userName;
                    AuthData.authenticationData.UserFullName =
                        accessToken.UserFullName;
                    AuthData.authenticationData.UserProfilePicture =
                        accessToken.UserProfilePicture;
                    AuthData.authenticationData.SysAndWH = accessToken.SysAndWH;
                }
                return accessToken;
            };
            this.removeToken = function (BaseUrl) {
                BaseUrl = typeof BaseUrl == "undefined" ? "" : BaseUrl;
                if (BaseUrl != "") BaseUrl += "/";
                tokenInfo = "";
                removeStorage("TokenInfo");
                removeStorage("accessingSystem");
                removeStorage("Menu");
                removeStorage("userAccessingSystem");
                removeStorage("userName");
                localStorage.clear();
                $window.location.href = apiBaseUrl + BaseUrl + "Login";
            };
            this.init = function () {
                //var accessToken = (typeof localStorageService.get("TokenInfo") == 'undefined') ? "" : localStorageService.get("TokenInfo");//(typeof sessionStorage.getItem('TokenInfo') == 'undefined') ? "" : sessionStorage.getItem('TokenInfo');
                var accessToken =
                    typeof getStorage("TokenInfo") == "undefined"
                        ? ""
                        : getStorage("TokenInfo");
                if (accessToken != "" && accessToken != null) {
                    tokenInfo = accessToken;
                    AuthData.authenticationData.IsAuthenticated = true;
                    AuthData.authenticationData.userName = tokenInfo.userName;
                    AuthData.authenticationData.UserFullName =
                        tokenInfo.UserFullName;
                    AuthData.authenticationData.UserProfilePicture =
                        tokenInfo.UserProfilePicture;
                    AuthData.authenticationData.SysAndWH = tokenInfo.SysAndWH;
                }
                return accessToken;
                //else {
                //    //var IsloggedIn = (typeof localStorageService.get("IsOnloginForm") == 'undefined') ? null : localStorageService.get("IsOnloginForm");
                //    //if (IsloggedIn == null) {
                //    //    localStorageService.set("IsOnloginForm", "true");
                //    //    console.log(IsloggedIn);
                //    DialogSweetAlert("load", "Please wait...");
                //    $window.location.href = apiBaseUrl + "Login";
                //    //}
                //}
            };
            this.setHeader = function (http) {
                delete http.defaults.headers.common["X-Requested-With"];
                if (
                    tokenInfo != undefined &&
                    tokenInfo.accessToken != undefined &&
                    tokenInfo.accessToken != null &&
                    tokenInfo.accessToken != ""
                ) {
                    http.defaults.headers.common["Authorization"] =
                        "Bearer " + tokenInfo.accessToken;
                    http.defaults.headers.common["Content-Type"] =
                        "application/x-www-form-urlencoded;charset=utf-8";
                }
            };
            this.init();
        },
    ]);
})();
