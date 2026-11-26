(function () {
  "use strict";
  app.service("LoginService", [
    "$window",
    "$http",
    "$q",
    "AuthenticationService",
    "AuthData",
    "Restangular",
    function (
      $window,
      $http,
      $q,
      AuthenticationService,
      AuthData,
      Restangular,
    ) {
      var deferred;

      this.Login = function (userName, password, callback) {
        var userInfo = {};
        var data =
          "grant_type=password&username=" +
          encodeURIComponent(userName) +
          "&password=" +
          encodeURIComponent(password);
        callback = typeof callback == "undefined" ? false : callback;
        deferred = $q.defer();
        $http
          .post(apiBaseUrl + "token", data, {
            headers: {
              "Content-Type":
                "application/x-www-form-urlencoded; charset=UTF-8",
            },
          })
          .then(
            function (response) {
              var payload = response.data || {};

              userInfo = {
                accessToken: payload.access_token,
                userName: userName,
                UserRolesID: payload.UserRolesID,
                UserRolesName: payload.UserRolesName,
                ImageLink: payload.ImageLink,
                Menu: payload.Menu,
                UserMenu: payload.UserMenu,
                SysAndWH: payload.SysAndWH,
                refreshToken: payload.refresh_token,
                TokenExpires: payload.TokenExpires,
                LoginTime: payload.LoginTime,
                UserFullName: payload.UserFullName,
                UserProfilePicture: payload.UserProfilePicture,
                DefaultDashboardPage: payload.DefaultDashboardPage,
                ISChangePasswordOnNextLogon:
                  payload.ISChangePasswordOnNextLogon,
                Poistion: JSON.parse(payload.SysAndWH)[0].Position,
              };
              if (payload.ISChangePasswordOnNextLogon == "True") {
                Swal.fire({
                  title: "Choose new password",
                  html: `<input type="password" id="TxtPassword" class="swal2-input" placeholder="Password">
                                   <input type="password" id="TxtConfirmPassword" class="swal2-input" placeholder="Confirm Password">`,
                  focusConfirm: false,
                  width: "700px",
                  position: "top",
                  showLoaderOnConfirm: true,
                  preConfirm: () => {
                    const Pass_word =
                      Swal.getPopup().querySelector("#TxtPassword").value;
                    const ConfirmPassword = Swal.getPopup().querySelector(
                      "#TxtConfirmPassword",
                    ).value;
                    if (!Pass_word) {
                      Swal.showValidationMessage(`Please enter Password.`);
                      Swal.getPopup().querySelector("#TxtPassword").focus();
                      return false;
                    } else if (!ConfirmPassword) {
                      Swal.showValidationMessage(
                        `Please enter Confirm Password.`,
                      );
                      Swal.getPopup()
                        .querySelector("#TxtConfirmPassword")
                        .focus();
                      return false;
                    } else if (Pass_word != ConfirmPassword) {
                      Swal.showValidationMessage(
                        `Your Password and Confirm Password not the same.`,
                      );
                      Swal.getPopup().querySelector("#TxtPassword").focus();
                      return false;
                    }
                    validatePassword(Pass_word, function (valid, message) {
                      if (!valid) {
                        Swal.showValidationMessage(message);
                        return false;
                      } else {
                        var DATA = {
                          UserName: "",
                          Phone: "",
                          Password: Pass_word,
                          UserID: payload.UserID,
                          ISChangePasswordOnNextLogon: true,
                        };
                        $.ajax({
                          url: ServerRoot + "Login/SaveChangeResetPassword",
                          dataType: "html",
                          async: false,
                          cache: false,
                          type: "POST",
                          data: DATA,
                          beforeSend: function (xhr) {
                            $("button").attr("disabled", true);
                          },
                          success: function (response) {
                            $("button").attr("disabled", false);
                            var result = JSON.parse(response);
                            if (result.Result == 2) {
                              $("#password").val(Pass_word);
                              $("#password").trigger("input");
                              $("#Login").click();
                              return true;
                            } else {
                              Swal.showValidationMessage(result.Message);
                              return false;
                            }
                          },
                          error: function (response, callStatus, errorThrown) {
                            $("button").attr("disabled", false);
                            Swal.showValidationMessage(
                              response.status + " " + response.statusText,
                            );
                            return false;
                          },
                        });
                      }
                    });
                    return false;
                  },
                }).then((result) => {
                  if (result.isConfirmed) {
                  }
                });
              } else {
                AuthenticationService.setTokenInfo(userInfo);
                deferred.resolve(userName);
                if (typeof callback === "function") {
                  callback({
                    Logged: true,
                    ISChangePasswordOnNextLogon:
                      payload.ISChangePasswordOnNextLogon,
                  });
                }
              }
              return userInfo;
            },
            function (err) {
              AuthData.authenticationData.IsAuthenticated = false;
              AuthData.authenticationData.userName = "";

              var errorMessage = "Unable to sign in.";
              if (err && err.data) {
                if (typeof err.data === "string") {
                  errorMessage = err.data;
                } else if (err.data.Message) {
                  errorMessage = err.data.Message;
                } else if (err.data.error_description) {
                  errorMessage = err.data.error_description;
                } else if (err.data.error) {
                  errorMessage = err.data.error;
                }
              }

              if (typeof callback === "function") {
                callback({
                  Logged: false,
                  Message: errorMessage,
                });
              }

              return deferred.resolve(err);
            },
          );
        return deferred.promise;
      };

      this.refreshToken = function () {
        var userInfo = {};
        var refreshToken = AuthenticationService.getTokenInfo().refreshToken;

        var data =
          "grant_type=refresh_token&refresh_token=" +
          encodeURIComponent(refreshToken) +
          "&client_id=";

        deferred = $q.defer();

        $http
          .post(apiBaseUrl + "token", data, {
            headers: {
              "Content-Type":
                "application/x-www-form-urlencoded; charset=UTF-8",
            },
            ignoreLoadingBar: true,
          })
          .then(
            function (response) {
              var payload = response.data || {};
              userInfo = {
                accessToken: payload.access_token,
                userName:
                  payload.UserName ||
                  AuthenticationService.getTokenInfo().userName,
                UserRolesID: payload.UserRolesID,
                UserRolesName: payload.UserRolesName,
                ImageLink: payload.ImageLink,
                Menu: payload.Menu,
                UserMenu: payload.UserMenu,
                SysAndWH: payload.SysAndWH,
                refreshToken: payload.refresh_token,
                TokenExpires: payload.TokenExpires,
                LoginTime: payload.LoginTime,
                UserFullName: payload.UserFullName,
                UserProfilePicture: payload.UserProfilePicture,
                DefaultDashboardPage: payload.DefaultDashboardPage,
              };
              AuthenticationService.setTokenInfo(userInfo);
              if (typeof UpdateGlobalAuthState === "function") {
                UpdateGlobalAuthState(userInfo);
              }
              deferred.resolve(userInfo);
            },
            function (err) {
              AuthData.authenticationData.IsAuthenticated = false;
              AuthData.authenticationData.userName = "";
              AuthenticationService.removeToken();
              deferred.reject(err);
            },
          );

        return deferred.promise;
      };

      this.LogOut = function (BaseUrl) {
        BaseUrl = typeof BaseUrl == "undefined" ? "" : BaseUrl;
        AuthData.authenticationData.IsAuthenticated = false;
        AuthData.authenticationData.userName = "";
        AuthenticationService.removeToken(BaseUrl);
      };

      this.InitLogin = function () {
        if (!AuthData.authenticationData.IsAuthenticated) this.LogOut();
      };
    },
  ]);
})();
