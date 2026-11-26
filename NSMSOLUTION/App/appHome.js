"use strict";

var CurrentTicketIDOpened = "";
var chatHub = $.connection.chatHub;
var audioNotification = `<audio style="display:none;" id="audioContainer"><source src="Content/Audio/wrong-answer-129254.mp3" type="audio/mpeg" /></audio>`;
audioNotification +=
  '<button style="display:none;" id="playMp3" onclick="playMp3()" type="button">Play Mp3</button>';

function startSignalR() {
  $.connection.hub
    .start()
    .done(function () {
      $("#audioContainer").remove();
      $(document.body).append(audioNotification);

      var tokenInfo = JSON.parse(getStorage("TokenInfo"));

      if (tokenInfo && tokenInfo.UserRolesID) {
        chatHub.server.joinRoleGroup(tokenInfo.UserRolesID);
      }
    })
    .fail(function () {
      setTimeout(function () {
        startSignalR();
      }, 5000);
    });
}

/* Start first time */
startSignalR();

/* Auto reconnect when disconnected */
$.connection.hub.disconnected(function () {
  setTimeout(function () {
    startSignalR();
  }, 5000);
});

/* Optional connection state logs */
$.connection.hub.reconnecting(function () {});

$.connection.hub.reconnected(function () {});

function playMp3() {
  const audioContainer = document.getElementById("audioContainer");
  var playPromise = audioContainer.play();
  if (playPromise !== undefined) {
    playPromise
      .then((_) => {})
      .catch((error) => {
        DialogSweetAlert(
          "error",
          "Please allow audio play on the top of your browser.",
        );
      });
  }
}

chatHub.client.ReceiveMessage = function (
  user,
  UserProfilePicture,
  message,
  projectName,
  CommentText,
  TicketID,
  TicketNumber,
  Data,
) {
  Data = new Array(JSON.parse(Data)[0].UserLoginID.split(","))[0];
  Data = Data.includes(User_ID);
  const msg = stripHtml(CommentText);

  if (Data) {
    var body = `<a onclick="onGridTicketClick('','${TicketID}','${TicketNumber}');" href="javascript:void(0);" class="text-white">
			<div class="row gap-8">
				<div class="col-auto">
					<div class="row"><div class="col"><div class="image">
						<img class="direct-chat-img elevation-2" src="${UserProfilePicture}" onerror="this.onerror=null;src='Content/images/avatar/user.png'"/>
					</div></div></div>
				</div>
				<div class="col">${msg}</div>
			</div>
		</a>`;

    if (user !== User_Full_Name) {
      if (
        $("div[name=messages]").length > 0 &&
        CurrentTicketIDOpened === TicketID
      ) {
        if (message !== "") $("div[name=messages]:last").after(message);
        new Audio("Content/Audio/notification.mp3").play();
      } else {
        $(document).Toasts("create", {
          body: body,
          title: projectName,
          subtitle: user,
          class: "bg-success",
          icon: "fas fa-envelope fa-lg",
          autohide: true,
          delay: 10000,
        });
        playMp3();
      }
    }
  }
};

chatHub.client.ReceiveCustomerNotification = function (
  CustomerID,
  CustomerName,
  CustomerCode,
) {
  // Reserved for future use
};

chatHub.client.ReceiveAlertCustomer = function (
  ReferenceID,
  AlertID,
  CustomerID,
  CustomerName,
  CustomerCode,
  CustomerMobile,
  ExpireDate,
  Comment,
  Reason,
  SentBy,
) {
  playMp3();

  $(document).Toasts("create", {
    class: "bg-info-light",
    title: "Customer Notification",
    subtitle: CustomerCode,
    body: `
            <div>
                CID: ${CustomerCode}<br/>
                <b>Name: ${CustomerName}</b><br/>
                <b>Remark: ${Comment || ""}</b><br/>
                <b>Reason: ${Reason || ""}</b><br/>
            </div>
        `,
    autohide: true,
    delay: 20000,
  });
};

var LoginUrl = apiBaseUrl + "Login";
var getUserAccessingSystem = new Object();

function setUserAccessingSystem(objData) {
  getUserAccessingSystem = objData;
}

function getUserAccessingSystemWarehouse() {
  return getUserAccessingSystem;
}

app.controller("LogoutController", [
  "$scope",
  "LoginService",
  "$window",
  function ($scope, LoginService, $window) {
    $scope.Logout = function () {
      LoginService.LogOut();
    };
    $scope.RefreshToken = function () {
      LoginService.refreshToken();
    };
  },
]);

app
  .config(function ($routeProvider, $httpProvider, RestangularProvider) {
    $routeProvider
      .when("/", {
        templateUrl: function () {
          return apiBaseUrl + "App/views/Home/index.html";
        },
      })
      .when("/:controller", {
        templateUrl: function (attr) {
          return apiBaseUrl + "App/views/" + attr.controller + "/index.html";
        },
      })
      .when("/:controller/:action", {
        templateUrl: function (attr) {
          var action =
            typeof attr.action === "undefined"
              ? "index.html"
              : attr.action + ".html";
          return apiBaseUrl + "App/views/" + attr.controller + "/" + action;
        },
      })
      .when("/:controller/:action:id", {
        templateUrl: function (attr) {
          var action =
            typeof attr.action === "undefined"
              ? "index.html"
              : attr.action + ".html";
          return (
            apiBaseUrl + "/" + attr.controller + "/" + action + "?id=" + attr.id
          );
        },
      })
      .otherwise({ redirectTo: "/404" });

    $httpProvider.interceptors.push(
      function (
        $q,
        $rootScope,
        $window,
        $location,
        AuthenticationService,
        AuthData,
        $injector,
      ) {
        return {
          request: function (config) {
            var accessToken = AuthenticationService.getTokenInfo();
            accessToken = accessToken === "" ? "" : accessToken.accessToken;

            if (!accessToken) {
              $window.location.href = apiBaseUrl + "Login";
            } else {
              let TokenInfo = getStorage("TokenInfo");
              let sysandwarehouseinfo = JSON.parse(
                JSON.parse(TokenInfo).SysAndWH,
              )[0];
              let userinfo = JSON.parse(TokenInfo);

              access_Token = userinfo.accessToken;
              User_ID = sysandwarehouseinfo.UserID;
              System_ID = getStorage("SystemID");
              Warehouse_ID = getStorage("WarehouseID");
              Domain_ID = getStorage("DomainID");
              User_Name = userinfo.userName;
              User_Full_Name = userinfo.UserFullName;
              UserProfilePicture = userinfo.UserProfilePicture;

              headerPara = {
                UserID: User_ID,
                UserName: User_Name,
                SystemID: System_ID,
                WarehouseID: Warehouse_ID,
                UserFullName: User_Full_Name,
                DomainID: Domain_ID,
                UserPosition: User_Poistion,
                SystemLogo: getStorage("SysWLogo"),
              };

              $rootScope.UserName = getStorage("User_Name");
              config.headers["x-custom-header"] = JSON.stringify(headerPara);
              config.headers["Authorization"] =
                "Bearer " + userinfo.accessToken;
            }
            return config;
          },
          requestError: function (rejection) {
            return $q.reject(rejection);
          },
          response: function (response) {
            return response;
          },
          responseError: function (rejection) {
            if (rejection.config && rejection.config.skipErrorRedirect)
              return $q.reject(rejection);

            var isTokenEndpoint =
              rejection.config &&
              rejection.config.url &&
              rejection.config.url.indexOf("token") !== -1;

            if (
              rejection.status === 401 &&
              !isTokenEndpoint &&
              !rejection.config.__isRetry
            ) {
              var $http = $injector.get("$http");
              var LoginService = $injector.get("LoginService");

              return LoginService.refreshToken().then(
                function (userInfo) {
                  rejection.config.__isRetry = true;
                  rejection.config.headers["Authorization"] =
                    "Bearer " + userInfo.accessToken;
                  return $http(rejection.config);
                },
                function () {
                  $window.location.href = apiBaseUrl + "Login";
                  return $q.reject(rejection);
                },
              );
            }

            if (rejection.status === 404) {
              $location.path("/Pagenotfound");
            }
            return $q.reject(rejection);
          },
        };
      },
    );
  })
  .run([
    "$location",
    "$rootScope",
    "cfpLoadingBar",
    function ($location, $rootScope, cfpLoadingBar) {
      $rootScope.$on("$routeChangeStart", function (event, next) {
        // next.originalPath is the raw path matched against $routeProvider,
        // e.g. "/" for the bare base URL / manual "#!/"
        if (next && next.originalPath === "/") {
          var defaultDashboard = getStorage("DefaultDashboardPage");

          // Only redirect if the user actually has a non-generic default set
          if (
            defaultDashboard &&
            defaultDashboard !== "#!/" &&
            defaultDashboard !== "/"
          ) {
            event.preventDefault();
            // strip the "#!" prefix since $location.path() expects a plain path
            var target = defaultDashboard.replace(/^#!/, "");
            $location.path(target);
          }
        }
      });

      $rootScope.$on("$routeChangeSuccess", function (event, current) {
        $rootScope.title = current.$$route.title;
      });
    },
  ]);

function flatten(arr) {
  var flat = [];
  for (var i = 0; i < arr.length; i++) {
    flat = flat.concat(arr[i]);
  }
  return flat;
}

app.controller("HomeMenuController", [
  "$scope",
  "AuthenticationService",
  "$sce",
  "$rootScope",
  "$location",
  function ($scope, AuthenticationService, $sce, $rootScope, $location) {
    var port = $location.port();
    $scope.absUrl =
      $location.protocol() + "://" + $location.host() + ":" + port + "/";
    $scope.SysWLogo = getStorage("SysWLogo");

    let tokenInfo = AuthenticationService.getTokenInfo();
    let imageLink =
      localStorage.getItem("ImageLink") ||
      tokenInfo.ImageLink ||
      tokenInfo.UserProfilePicture;

    document.querySelectorAll(".userimg").forEach(function (img) {
      img.src = imageLink;
    });

    const systemId = localStorage.getItem("SystemID");
    const sysObj = tokenInfo.SysAndWH
      ? JSON.parse(tokenInfo.SysAndWH).find(
          (item) => item.SystemID === systemId,
        )
      : null;

    const company = sysObj.Company;

    $rootScope.CompanyName = company ? company.CompanyName : "";
    $rootScope.CompanyNameKH = company ? company.CompanyNameKH : "";

    $rootScope.UserFullName = tokenInfo.UserFullName;
    $rootScope.UserRole = tokenInfo.UserRolesName;

    var setselectsubmen = "",
      issetselectsubmenu = false,
      submenuname = "";

    jQuery(document).ready(function ($) {
      let SystemID = getStorage("SystemID");
      let WarehouseID = getStorage("WarehouseID");
      let DefaultDashboardPage = getStorage("DefaultDashboardPage");
      let Menus = AuthenticationService.getUserMenu();
      let Table = Menus.Table;
      let locationpath = $location.path();
      let usermenu = Table.filter(
        (item) => item.SystemID == SystemID && item.WarehouseID == WarehouseID,
      );

      let row = `<ul class="nav nav-pills nav-sidebar flex-column" role="menu" data-accordion="true">`;

      if (!DefaultDashboardPage || DefaultDashboardPage === "#!/") {
        row += `<li class="nav-item">
				<a href="#!/" class="nav-link active">
					<i class="nav-icon ti ti-layout-dashboard"></i>
					<p class="text-nowrap">Dashboard</p>
				</a>
			</li>`;
      }

      $.each(usermenu, function (key, value) {
        let PageCategoryID = value.PageCategoryID;
        let PageLink = value.PageLink || "javascript:void(0);";
        let subusermenu = Menus.Table1.filter(
          (el) => el.SystemID == SystemID && el.WarehouseID == WarehouseID,
        );
        let Table1 = subusermenu.filter(
          (val) =>
            val.PageCategoryID == PageCategoryID &&
            val.ParentPageCode == "" &&
            val.PageSubCategory == "",
        );

        const chevronSVG = `<svg xmlns="http://www.w3.org/2000/svg" aria-hidden="true" role="img" class="right iconify iconify--ph" width="1em" height="1em" viewBox="0 0 256 256"><path fill="currentColor" d="m181.66 133.66l-80 80a8 8 0 0 1-11.32-11.32L164.69 128L90.34 53.66a8 8 0 0 1 11.32-11.32l80 80a8 8 0 0 1 0 11.32"></path></svg>`;

        row += `<li class="nav-item">
				<a href="${PageLink}" class="nav-link active">
					<i class="nav-icon ${value.NAVICON}"></i>
					<p class="text-nowrap">${value.PageCategory}
						${Table1.length > 0 ? chevronSVG : ""}
					</p>
				</a>`;

        if (Table1.length > 0) row += `<ul class="nav nav-treeview">`;

        $.each(Table1, function (k, val) {
          let NAVICON = val.NAVICON || "ti ti-inner-shadow-bottom-left";
          let parentMenu = subusermenu.filter(
            (rst) => rst.ParentPageCode == val.PermissionPageCode,
          );

          if (val.PageLink === "") return;

          if (parentMenu.length > 0) {
            if (locationpath === "/" + val.PageLink) {
              setselectsubmen = "setselectsubmen_" + val.PageLink;
              issetselectsubmenu = true;
            }
            row += `<li class="nav-item" name="setselectsubmen_${val.PageLink}">
						<a href="javascript:void(0);" class="nav-link bg-gray">
							<p class="text-nowrap">${val.PermissionPageCode} ${chevronSVG}</p>
						</a>
						<ul class="nav nav-treeview">
							<li class="nav-item">
								<a name="subMenuLeft" href="#!/${val.PageLink}" class="nav-link">
									<i class="${NAVICON}"></i>
									<p class="text-nowrap">${val.PermissionPageCode}</p>
								</a>
							</li>`;

            $.each(parentMenu, function (key, v) {
              let icon = v.NAVICON || "ti ti-inner-shadow-bottom-left";
              if (locationpath === "/" + v.PageLink) {
                setselectsubmen = "setselectsubmen_" + val.PageLink;
                issetselectsubmenu = true;
                submenuname = "submenuname_" + v.PageLink;
              }
              row += `<li class="nav-item" name="submenuname_${v.PageLink}">
							<a name="subMenuLeft" href="#!/${v.PageLink}" class="nav-link">
								<i class="${icon}"></i>
								<p class="text-nowrap">${v.PermissionPageCode}</p>
							</a>
						</li>`;
            });

            row += `</ul></li>`;
          } else {
            if (locationpath === "/" + val.PageLink) {
              setselectsubmen = "setselectsubmen_" + val.PageLink;
            }
            row += `<li class="nav-item" name="setselectsubmen_${val.PageLink}">
						<a name="subMenuLeft" href="#!/${val.PageLink}" class="nav-link">
							<i class="${NAVICON}"></i>
							<p class="text-nowrap">${val.PermissionPageCode}</p>
						</a>
					</li>`;
          }
        });

        let PageSubCategory = Menus.Table1.filter(
          (rst) =>
            rst.SystemID == SystemID &&
            rst.WarehouseID == WarehouseID &&
            rst.PageCategoryID == value.PageCategoryID &&
            rst.PageSubCategory !== "",
        );

        if (PageSubCategory.length > 0) {
          row += `<li class="nav-item" name="setselectsubmen_${PageSubCategory[0].PageLink}">
					<a href="javascript:void(0);" class="nav-link bg-gray">
						<p class="text-nowrap">${PageSubCategory[0].PageSubCategory} ${chevronSVG}</p>
					</a>
					<ul class="nav nav-treeview">`;

          $.each(PageSubCategory, function (key, v) {
            let icon = v.NAVICON || "ti ti-inner-shadow-bottom-left";
            if (locationpath === "/" + v.PageLink) {
              setselectsubmen =
                "setselectsubmen_" + PageSubCategory[0].PageLink;
              issetselectsubmenu = true;
              submenuname = "submenuname_" + v.PageLink;
            }
            row += `<li class="nav-item" name="submenuname_${v.PageLink}">
						<a name="subMenuLeft" href="#!/${v.PageLink}" class="nav-link">
							<i class="${icon}"></i>
							<p class="text-nowrap">${v.PermissionPageCode}</p>
						</a>
					</li>`;
          });

          row += `</ul></li>`;
        }

        if (Table1.length > 0) row += "</ul>";
        row += "</li>";
      });

      row += "</ul>";
      $("#DivLeftMenu").html(row);

      $("ul.nav-treeview .nav-link").on("click", function () {
        if ($(this).hasClass("bg-gray")) return;
        $(".sidebar ul.nav-treeview .nav-link").removeClass("active");
        $(this).addClass("active");
      });

      $(
        ".nav.nav-pills.nav-sidebar.flex-column > .nav-item, .nav.nav-treeview > .nav-item",
      ).each(function () {
        const $el = $(this);
        const $submenu = $el.children(".nav-treeview");

        if ($submenu.length) {
          $el.children("a").on("click", function (e) {
            e.preventDefault();
            e.stopPropagation();

            if ($el.hasClass("menu-open")) {
              $submenu.stop(true, true).slideUp(200, function () {
                $el.removeClass("menu-open menu-is-opening");
              });
            } else {
              $el.siblings(".nav-item.menu-open").each(function () {
                $(this)
                  .removeClass("menu-open menu-is-opening")
                  .children(".nav-treeview")
                  .stop(true, true)
                  .slideUp(200);
              });
              $el.addClass("menu-is-opening");
              $submenu.stop(true, true).slideDown(200, function () {
                $el.addClass("menu-open").removeClass("menu-is-opening");
              });
            }
          });
        }
      });

      if (setselectsubmen !== "") {
        $("li").removeClass("menu-is-opening menu-open");
        const $submenuLi = $(`li[name="${setselectsubmen}"]`);
        $submenuLi
          .closest("ul.nav-treeview")
          .parent("li")
          .addClass("menu-is-opening menu-open");
        $submenuLi
          .closest("ul.nav-treeview")
          .parent("li")
          .children("a.nav-link")
          .addClass("active");
        $submenuLi.children("a.nav-link").addClass("active");
      }
    });
  },
]);

app.controller("HomeMenuTMP1Controller", [
  "$scope",
  "AuthenticationService",
  "$sce",
  "$rootScope",
  "$location",
  function ($scope, AuthenticationService, $sce, $rootScope, $location) {
    var port = $location.port();
    $scope.absUrl =
      $location.protocol() + "://" + $location.host() + ":" + port + "/";
    $scope.SysWLogo = getStorage("SysWLogo");

    let imgs = document.getElementById("userimg");
    imgs.src = AuthenticationService.getTokenInfo().UserProfilePicture;
    $scope.UserFullName = AuthenticationService.getTokenInfo().UserFullName;

    var setselectsubmen = "",
      issetselectsubmenu = false,
      submenuname = "";

    jQuery(document).ready(function ($) {
      let SystemID = getStorage("SystemID");
      let WarehouseID = getStorage("WarehouseID");
      let Menus = AuthenticationService.getUserMenu();
      let Table = Menus.Table;
      let usermenu = Table.filter(
        (item) => item.SystemID == SystemID && item.WarehouseID == WarehouseID,
      );
      let locationpath = $location.path();

      let row = `<ul id="sidebarnav">
			<li class="nav-small-cap">
				<iconify-icon icon="solar:menu-dots-bold" class="nav-small-cap-icon fs-4"></iconify-icon>
				<span class="hide-menu"></span>
			</li>`;

      $.each(usermenu, function (key, value) {
        let PageCategoryID = value.PageCategoryID;
        let PageLink = value.PageLink || "javascript:void(0);";
        let subusermenu = Menus.Table1.filter(
          (el) => el.SystemID == SystemID && el.WarehouseID == WarehouseID,
        );
        let Table1 = subusermenu.filter(
          (val) =>
            val.PageCategoryID == PageCategoryID &&
            val.ParentPageCode == "" &&
            val.PageSubCategory == "",
        );
        let hasArrow = Table1.length > 0 ? " has-arrow" : "";

        row += `<li class="sidebar-item">
				<a href="${PageLink}" class="sidebar-link${hasArrow}" aria-expanded="false">
					<i class="nav-icon ${value.NAVICON}"></i>
					<span class="hide-menu text-nowrap">${value.PageCategory}</span>
				</a>`;

        if (Table1.length > 0)
          row += `<ul aria-expanded="false" class="collapse first-level">`;

        $.each(Table1, function (k, val) {
          let NAVICON = val.NAVICON || "far fa-circle nav-icon";
          let parentMenu = subusermenu.filter(
            (rst) => rst.ParentPageCode == val.PermissionPageCode,
          );

          if (val.PageLink === "") return;

          if (parentMenu.length > 0) {
            if (locationpath === "/" + val.PageLink) {
              setselectsubmen = "setselectsubmen_" + val.PageLink;
              issetselectsubmenu = true;
            }
            row += `<li class="sidebar-item" name="setselectsubmen_${val.PageLink}">
						<a name="subMenuLeft" href="javascript:void(0);" class="sidebar-link has-arrow">
							<div class="round-16 d-flex align-items-center justify-content-center">
								<i class="right ${NAVICON}"></i>
							</div>
							<span class="hide-menu">${val.PermissionPageCode}</span>
						</a>
						<ul class="collapse two-level in" aria-expanded="false">
							<li class="sidebar-item">
								<a href="#!/${val.PageLink}" class="sidebar-link sublink">
									<div class="round-16 d-flex align-items-center justify-content-center">
										<i class="${NAVICON}"></i>
									</div>
									<span class="hide-menu text-nowrap">${val.PermissionPageCode}</span>
								</a>
							</li>`;

            $.each(parentMenu, function (key, v) {
              let icon = v.NAVICON || "far fa-dot-circle nav-icon";
              if (locationpath === "/" + v.PageLink) {
                setselectsubmen = "setselectsubmen_" + val.PageLink;
                issetselectsubmenu = true;
                submenuname = "submenuname_" + v.PageLink;
              }
              row += `<li class="sidebar-item" name="submenuname_${v.PageLink}">
							<a href="#!/${v.PageLink}" class="sidebar-link sublink">
								<div class="round-16 d-flex align-items-center justify-content-center">
									<i class="${icon}"></i>
								</div>
								<span class="hide-menu">${v.PermissionPageCode}</span>
							</a>
						</li>`;
            });

            row += `</ul></li>`;
          } else {
            if (locationpath === "/" + val.PageLink) {
              setselectsubmen = "setselectsubmen_" + val.PageLink;
            }
            row += `<li class="sidebar-item" name="setselectsubmen_${val.PageLink}">
						<a name="subMenuLeft" href="#!/${val.PageLink}" class="sidebar-link sublink">
							<div class="round-16 d-flex align-items-center justify-content-center">
								<i class="${NAVICON}"></i>
							</div>
							<span class="hide-menu text-nowrap">${val.PermissionPageCode}</span>
						</a>
					</li>`;
          }
        });

        let PageSubCategory = Menus.Table1.filter(
          (rst) =>
            rst.SystemID == SystemID &&
            rst.WarehouseID == WarehouseID &&
            rst.PageCategoryID == value.PageCategoryID &&
            rst.PageSubCategory !== "",
        );

        if (PageSubCategory.length > 0) {
          let icon = PageSubCategory[0].NAVICON || "far fa-circle nav-icon";
          row += `<li class="sidebar-item" name="setselectsubmen_${PageSubCategory[0].PageLink}">
					<a href="javascript:void(0);" class="sidebar-link has-arrow bg-primary-subtle">
						<div class="round-16 d-flex align-items-center justify-content-center">
							<i class="right ${icon}"></i>
						</div>
						<span class="hide-menu text-nowrap">${PageSubCategory[0].PageSubCategory}</span>
					</a>
					<ul class="collapse two-level">`;

          $.each(PageSubCategory, function (key, v) {
            let vIcon = v.NAVICON || "far fa-dot-circle nav-icon";
            if (locationpath === "/" + v.PageLink) {
              setselectsubmen =
                "setselectsubmen_" + PageSubCategory[0].PageLink;
              issetselectsubmenu = true;
              submenuname = "submenuname_" + v.PageLink;
            }
            row += `<li class="sidebar-item" name="submenuname_${v.PageLink}">
						<a name="subMenuLeft" href="#!/${v.PageLink}" class="sidebar-link sublink">
							<div class="round-16 d-flex align-items-center justify-content-center">
								<i class="${vIcon}"></i>
							</div>
							<span class="hide-menu text-nowrap">${v.PermissionPageCode}</span>
						</a>
					</li>`;
          });

          row += `</ul></li>`;
        }

        if (Table1.length > 0) row += "</ul>";
        row += "</li>";
      });

      row += "</ul>";
      $("#DivLeftMenu").html(row);
      applyLeftMenu();
    });
  },
]);

app.directive("tooltip", function () {
  return {
    restrict: "A",
    link: function (scope, element, attrs) {
      attrs.$observe("title", function (value) {
        if (!value) return;

        element.attr("title", value);

        if (element.data("bs.tooltip")) {
          element.tooltip("dispose");
        }

        element.tooltip({
          trigger: "hover",
          placement: "top",
          container: "body",
        });
      });
    },
  };
});

app.directive("slideIndicator", [
  "$timeout",
  "$window",
  function ($timeout, $window) {
    return {
      restrict: "A",
      link: function (scope, el, attrs) {
        // slide-indicator            -> uses ".active"
        // slide-indicator=".my-tab.on" -> custom selector
        var sel = attrs.slideIndicator || ".active";
        var host = el[0];
        var ind = angular.element('<span class="slide-ind"></span>');
        el.append(ind);
        var ready = false;

        function place(t) {
          if (!t) {
            ind.css("opacity", 0);
            return;
          }
          host.style.setProperty("--si-x", t.offsetLeft + "px");
          host.style.setProperty("--si-y", t.offsetTop + "px");
          host.style.setProperty("--si-w", t.offsetWidth + "px");
          host.style.setProperty("--si-h", t.offsetHeight + "px");
          ind.css("opacity", 1);
          if (!ready) {
            // no animation on first placement
            ready = true;
            $timeout(
              function () {
                ind.addClass("ready");
              },
              30,
              false,
            );
          }
        }
        function move() {
          place(host.querySelector(sel));
        }

        // instant response on press, before Angular's digest
        el.on("mousedown touchstart", function (e) {
          var t = e.target;
          while (t && t.parentNode !== host) t = t.parentNode;
          if (t && t !== ind[0]) place(t);
        });

        // follow the active tab (layout, active change, late render)
        scope.$watch(
          function () {
            var a = host.querySelector(sel);
            return a
              ? a.offsetLeft + "|" + a.offsetTop + "|" + a.offsetWidth
              : "";
          },
          function () {
            $timeout(move, 0, false);
          },
        );

        // fonts / container resize
        var ro = window.ResizeObserver ? new ResizeObserver(move) : null;
        if (ro) ro.observe(host);
        angular.element($window).on("resize", move);

        scope.$on("$destroy", function () {
          if (ro) ro.disconnect();
          angular.element($window).off("resize", move);
          el.off("mousedown touchstart");
        });
      },
    };
  },
]);

app.directive("datePicker", function () {
  return {
    restrict: "A",
    require: "ngModel",
    link: function (scope, element, attrs, ngModel) {
      if (!jQuery.fn.daterangepicker) return;

      var format = attrs.dateFormat || "DD-MM-YYYY";
      element.daterangepicker({
        singleDatePicker: true,
        autoApply: true,
        autoUpdateInput: false,
        showDropdowns: true,
        locale: { format: format, cancelLabel: "Clear" },
      });
      var picker = element.data("daterangepicker");
      picker.container.addClass("rp-report-picker");

      // Point the calendar at the date already in the field (strict parse,
      // so half-typed text is ignored).
      function syncPicker(value) {
        if (!value || !window.moment) return false;
        var m = window.moment(value, format, true);
        if (!m.isValid()) return false;
        picker.setStartDate(m);
        picker.setEndDate(m);
        return true;
      }

      // Model -> input (preset dropdown, Reset filters, ...).
      ngModel.$render = function () {
        var value = ngModel.$viewValue || "";
        element.val(value);
        syncPicker(value);
      };

      // Open on the current value. The library has already rendered by the
      // time "show" fires, so redraw after moving the date.
      element.on("show.daterangepicker.reportDatePicker", function () {
        var value = element.val() || ngModel.$viewValue;
        if (!syncPicker(value)) return;

        var m = picker.startDate.clone().date(2); // date(2) avoids month-end overflow
        picker.leftCalendar.month = m;
        if (picker.rightCalendar)
          picker.rightCalendar.month = m.clone().add(1, "month");
        if (picker.updateView) picker.updateView();
        if (picker.renderCalendar) picker.renderCalendar("left");
      });

      // autoUpdateInput is false, so the library never writes the picked date
      // into the box. $setViewValue only updates the model, not the DOM, so
      // the input must be set here or it keeps showing the old value.
      element.on("apply.daterangepicker.reportDatePicker", function () {
        var value = picker.startDate.format(format);
        element.val(value);
        scope.$applyAsync(function () {
          ngModel.$setViewValue(value);
        });
      });
      element.on("cancel.daterangepicker.reportDatePicker", function () {
        element.val("");
        scope.$applyAsync(function () {
          ngModel.$setViewValue("");
        });
      });

      scope.$on("$destroy", function () {
        element.off(".reportDatePicker");
        var picker = element.data("daterangepicker");
        if (picker) picker.remove();
      });
    },
  };
});

app.directive("dateRangePicker", [
  "$parse",
  function ($parse) {
    return {
      restrict: "A",
      link: function (scope, element, attrs) {
        if (!jQuery.fn.daterangepicker) return;

        var getStart = $parse(attrs.startModel);
        var getEnd = $parse(attrs.endModel);
        var setStart = getStart.assign;
        var setEnd = getEnd.assign;
        if (!setStart || !setEnd) return;

        var format = attrs.dateFormat || "DD-MM-YYYY";
        element.daterangepicker({
          autoApply: true,
          autoUpdateInput: false,
          showDropdowns: true,
          locale: { format: format, cancelLabel: "Clear", separator: " - " },
        });
        element.data("daterangepicker").container.addClass("rp-report-picker");

        element.on(
          "apply.daterangepicker.reportDateRangePicker",
          function (_event, picker) {
            scope.$applyAsync(function () {
              setStart(scope, picker.startDate.format(format));
              setEnd(scope, picker.endDate.format(format));
            });
          },
        );
        element.on("cancel.daterangepicker.reportDateRangePicker", function () {
          scope.$applyAsync(function () {
            setStart(scope, "");
            setEnd(scope, "");
            element.val("");
          });
        });

        var stopWatching = scope.$watchGroup(
          [attrs.startModel, attrs.endModel],
          function (values) {
            var start = values[0];
            var end = values[1];
            if (!start || !end) {
              element.val("");
              return;
            }

            var picker = element.data("daterangepicker");
            if (!picker) return;
            picker.setStartDate(start);
            picker.setEndDate(end);
            element.val(start + " - " + end);
          },
        );

        scope.$on("$destroy", function () {
          stopWatching();
          element.off(".reportDateRangePicker");
          var picker = element.data("daterangepicker");
          if (picker) picker.remove();
        });
      },
    };
  },
]);

app.controller("EventsCtrl", [
  "$scope",
  "AuthenticationService",
  "LoginService",
  "webNotification",
  "$window",
  "$location",
  "NotificationService",
  function (
    $scope,
    AuthenticationService,
    LoginService,
    webNotification,
    $window,
    $location,
    NotificationService,
  ) {
    $scope.AsideLightSidebar = getStorage("AsideLightSidebar");
    $scope.Body = getStorage("Body");
    var port = $location.port();
    $scope.absUrl =
      $location.protocol() + "://" + $location.host() + ":" + port + "/";
    $scope.SysWLogo = getStorage("SysWLogo");

    var tokenInfo = AuthenticationService.getTokenInfo();
    var userRoleID = tokenInfo.UserRolesID;
    var domainId = getStorage("DomainID");
    var systemId = getStorage("SystemID");

    var notificationAllowedRoles = ["6", "16"];
    $scope.showNotification =
      notificationAllowedRoles.includes(userRoleID) &&
      systemId === "13" &&
      domainId === "001";
    $scope.isShowSwitchSysOpt = domainId === "001";
    $scope.isShowSettingsOpt = domainId === "001";

    $scope.nearlyExpiredList = [];
    $scope.nearlyExpiredCount = 0;
    $scope.nearlyExpiredTotalText = "0 Nearly";

    function loadNearlyExpired() {
      if (!$scope.showNotification) return;

      NotificationService.getNearlyExpiredCustomers().then(
        function (result) {
          $scope.nearlyExpiredList = result.data || [];
          $scope.nearlyExpiredCount = result.total || 0;
          $scope.nearlyExpiredTotalText =
            $scope.nearlyExpiredList.length + " Nearly";
        },
        function () {
          $scope.nearlyExpiredList = [];
          $scope.nearlyExpiredCount = 0;
          $scope.nearlyExpiredTotalText = "0 Nearly";
        },
      );
    }

    loadNearlyExpired();

    $scope.viewAllCustomerExpired = function () {
      $window.location.href = "#!/ReportCustomerNearlyExpired";
    };
  },
]);

function display_time_nsmips() {
  var x = new Date();
  var hours = x.getHours() % 12 || 12;
  hours = hours.toString().padStart(2, "0");
  var minutes = x.getMinutes().toString().padStart(2, "0");
  var seconds = x.getSeconds().toString().padStart(2, "0");
  var month = (x.getMonth() + 1).toString().padStart(2, "0");
  var dt = x.getDate().toString().padStart(2, "0");
  var x1 = x.getFullYear() + month + dt + hours + minutes + seconds;

  if (x1 >= TokenExpires) {
    TokenExpires = $.format.date(
      addMinutes(new Date(TimeTokenExpires), -1),
      "yyyyMMddHHmmss",
    );
    Swal.fire({
      title:
        "Your session will expire in 5 minutes. Please enter your password to continue your session.",
      input: "text",
      inputAttributes: { autocapitalize: "off" },
      showCancelButton: true,
      confirmButtonText: "Look up",
      showLoaderOnConfirm: true,
      preConfirm: (login) => {
        return fetch(`//api.github.com/users/${login}`)
          .then((response) => {
            if (!response.ok) throw new Error(response.statusText);
            return response.json();
          })
          .catch((error) =>
            Swal.showValidationMessage(`Request failed: ${error}`),
          );
      },
      allowOutsideClick: () => !Swal.isLoading(),
    }).then((result) => {
      if (result.isConfirmed) {
        Swal.fire({
          title: `${result.value.login}'s avatar`,
          imageUrl: result.value.avatar_url,
        });
      }
    });
  }

  resfresh_time_nsmisp();
}

function resfresh_time_nsmisp() {
  setTimeout("display_time_nsmips()", 1000);
}
