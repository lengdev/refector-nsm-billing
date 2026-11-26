'use strict';
var objMenuList = {};
var MenuList = "";
app.controller('HomeController', ['$rootScope', '$scope', '$location', 'AuthData', 'LoginService', 'AuthenticationService', '$filter', '$window', 'Form', function ($rootScope, $scope, $location, AuthData, LoginService, AuthenticationService, $filter, $window, Form) {    
    if (AuthenticationService.getTokenInfo() != "") {
        $rootScope.title = "Home";
        //$rootScope.DefaultDashboardPage = getStorage("DefaultDashboardPage");
        var MenuData = {};
        var PPageCode = "";
        var SystemID = AuthenticationService.getSystemID();
        var WarehouseID = AuthenticationService.getWarehouseID();
        $scope.Menu = AuthenticationService.getMenu();
        $scope.Menu = $scope.Menu.filter(function (currentElement) {
            if (currentElement.SystemID = SystemID && currentElement.WarehouseID == WarehouseID)
                return true;
        });
        $scope.Menus = _.groupBy($scope.Menu, 'PageType');

        MenuList = $scope.Menu;
        //MenuList.sort(dynamicSort("PageType"));
        MenuList = _.groupBy(MenuList, 'PageType');

        //Form.LoadDataView("Home/LoadDashboardInfo", function (result) {
        //    var Table = result.Table[0];
        //    var Table1 = result.Table1[0];
        //    var Table2 = result.Table2[0];
        //    $("#thismonth").html(Table.ThisMonth);
        //    $("#lastmonth").html(Table.LastMonth);
        //    var breadbarValue = Object.values(Table1);
        //    var breadbar = {
        //        series: [
        //            {
        //                name: "",
        //                data: breadbarValue,
        //            },
        //        ],
        //        chart: {
        //            type: "bar",
        //            width: 70,
        //            height: 40,
        //            toolbar: {
        //                show: false,
        //            },
        //            sparkline: {
        //                enabled: true,
        //            },
        //        },
        //        colors: ["var(--bs-primary)"],
        //        grid: {
        //            show: false,
        //        },
        //        plotOptions: {
        //            bar: {
        //                horizontal: false,
        //                borderRadius: 2,
        //                columnWidth: "50%",
        //                barHeight: "100%",
        //            },
        //        },
        //        dataLabels: {
        //            enabled: false,
        //        },
        //        stroke: {
        //            show: true,
        //            width: 0,
        //            colors: ["transparent"],
        //        },
        //        xaxis: {
        //            axisBorder: {
        //                show: false,
        //            },
        //            axisTicks: {
        //                show: false,
        //            },
        //            labels: {
        //                show: false,
        //            },
        //        },
        //        yaxis: {
        //            labels: {
        //                show: false,
        //            },
        //        },
        //        axisBorder: {
        //            show: false,
        //        },
        //        fill: {
        //            opacity: 1,
        //        },
        //        tooltip: {
        //            theme: "dark",
        //            style: {
        //                fontFamily: "inherit",
        //            },
        //            x: {
        //                show: false,
        //            },
        //            y: {
        //                formatter: undefined,
        //            },
        //        },
        //    };
        //    var chart_column_breadbar = new ApexCharts(
        //        document.querySelector(".breadbar"),
        //        breadbar
        //    );
        //    chart_column_breadbar.render();
        //    breadbarValue = Object.values(Table2);
        //    var breadbar2 = {
        //        series: [
        //            {
        //                name: "",
        //                data: breadbarValue,
        //            },
        //        ],
        //        chart: {
        //            type: "bar",
        //            width: 70,
        //            height: 40,
        //            toolbar: {
        //                show: false,
        //            },
        //            sparkline: {
        //                enabled: true,
        //            },
        //        },
        //        colors: ["var(--bs-secondary)"],
        //        grid: {
        //            show: false,
        //        },
        //        plotOptions: {
        //            bar: {
        //                horizontal: false,
        //                borderRadius: 2,
        //                columnWidth: "50%",
        //                barHeight: "100%",
        //            },
        //        },
        //        dataLabels: {
        //            enabled: false,
        //        },
        //        stroke: {
        //            show: true,
        //            width: 0,
        //            colors: ["transparent"],
        //        },
        //        xaxis: {
        //            axisBorder: {
        //                show: false,
        //            },
        //            axisTicks: {
        //                show: false,
        //            },
        //            labels: {
        //                show: false,
        //            },
        //        },
        //        yaxis: {
        //            labels: {
        //                show: false,
        //            },
        //        },
        //        axisBorder: {
        //            show: false,
        //        },
        //        fill: {
        //            opacity: 1,
        //        },
        //        tooltip: {
        //            theme: "dark",
        //            style: {
        //                fontFamily: "inherit",
        //            },
        //            x: {
        //                show: false,
        //            },
        //            y: {
        //                formatter: undefined,
        //            },
        //        },
        //    };
        //    var chart_column_breadbar2 = new ApexCharts(
        //        document.querySelector(".breadbar2"),
        //        breadbar2
        //    );
        //    chart_column_breadbar2.render();

        //    //end show result by this month and last month

        //    // -----------------------------------------------------------------------
        //    // Our visitor
        //    // -----------------------------------------------------------------------
        //    var Table3 = result.Table3;
        //    var Table4 = result.Table4[0].CustomerID;
        //    var serie = Table3;
        //    var label = Table3;
        //    $("#span_totalCustomer").html(Table4);
        //    var series = serie.map(function (item) {
        //        return item.CustomerID;
        //    });
        //    var labels = label.map(function (item) {
        //        return item.CustomerGroup;
        //    });
        //    var options = {
        //        series: [{
        //            data: series,//[400, 430, 448, 470, 540, 580, 690, 1100, 1200, 1380]
        //        }],
        //        chart: {
        //            type: 'bar',
        //            height: 350
        //        },
        //        plotOptions: {
        //            bar: {
        //                borderRadius: 4,
        //                borderRadiusApplication: 'end',
        //                horizontal: false,
        //            }
        //        },
        //        dataLabels: {
        //            enabled: true,
        //            style: {
        //                fontSize: '12px',
        //                colors: ['#000']
        //            }
        //        },
        //        xaxis: {
        //            categories: labels,//['South Korea', 'Canada', 'United Kingdom', 'Netherlands', 'Italy', 'France', 'Japan',
        //                //'United States', 'China', 'Germany'
        //            //],
        //        },
        //        fill: {
        //            opacity: 1,
        //            colors: ["var(--bs-primary)"],
        //        },
        //    };

        //    var chart = new ApexCharts(document.querySelector("#our-visitors"), options);
        //    chart.render();

        //    var Table5 = result.Table5;
        //    var Table6 = result.Table6;
        //    var lastYear = Table5.map(function (item) {
        //        return item.Amount;
        //    });
        //    var thisYear = Table6.map(function (item) {
        //        return item.Amount;
        //    });

        //    var sumLastYear = lastYear.reduce((accumulator, currentValue) => accumulator + currentValue, 0);
        //    var sumThisYear = thisYear.reduce((accumulator, currentValue) => accumulator + currentValue, 0);
        //    $("#span_last_year").html(accounting.formatMoney(sumLastYear));
        //    $("#span_this_year").html(accounting.formatMoney(sumThisYear));

        //    var options = {
        //        series: [{
        //            name: 'Last Year',
        //            data: lastYear,//[44, 55, 57, 56, 61, 58, 63, 60, 66]
        //        }, {
        //            name: 'This Year',
        //            data: thisYear,//[76, 85, 101, 98, 87, 105, 91, 114, 94]
        //        },],
        //        chart: {
        //            //type: 'bar',
        //            //height: 350,
        //            //fontFamily: "inherit",
        //            type: "bar",
        //            height: 335,
        //            //foreColor: "#adb0bb",
        //            //offsetY: 10,
        //            //offsetX: -15,
        //            //toolbar: {
        //             //   show: false,
        //            //},
        //        },
        //        plotOptions: {
        //            bar: {
        //                //horizontal: false,
        //                //columnWidth: '55%',
        //                endingShape: 'rounded',
        //                horizontal: false,
        //                columnWidth: "55%",
        //                //endingShape: "flat",
        //                borderRadius: 6,
        //            },
        //        },
        //        dataLabels: {
        //            enabled: false
        //        },
        //        stroke: {
        //            show: true,
        //            width: 2,
        //            colors: ['transparent']
        //        },
        //        xaxis: {
        //            categories: ['Jan','Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        //        },
        //        //yaxis: {
        //        //    title: {
        //        //        text: '$ (thousands)'
        //        //    }
        //        //},
        //        fill: {
        //            opacity: 1,
        //            colors: ["var(--bs-info)", "var(--bs-primary)"],
        //        },
        //        tooltip: {
        //            y: {
        //                formatter: function (val) {
        //                    return accounting.formatMoney(val)// + " thousands"
        //                }
        //            }
        //        }
        //    };            
        //    var chart = new ApexCharts(document.querySelector("#sales-overview"), options);
        //    chart.render();

        //    var exchang_rate = result.Table7[0];
        //    $("#exchange_text").html("Exchange Rate " + exchang_rate.Date);
        //    $("#h4_exchang_rate").html(exchang_rate.ExchangeRate);
        //});

        $scope.init = function () {
            $(".myLoading").remove();
            $("button").removeAttr("disabled");
            $(".popover").show();
            $('[data-original-title]').popover('show');
            isPopoverClick = false;
            //$rootScope.init();
        };
        //$scope.init();

        //var Menu = JSON.parse(AuthenticationService.getTokenInfo().Menu);
        //Menu = Menu.sort(dynamicSort("PageType"));
        //$scope.MenuLists = _.groupBy(Menu, 'PageType');
        //Menu = _.groupBy(Menu, 'PageType');

        //var row = '<nav><ul class="nav nav-pills nav-sidebar flex-column" data-widget="treeview" role="menu" data-accordion="false">';
        //var i = 0;
        //$.each(Menu, function (key, value) {
        //    row += '<li class="nav-item has-treeview ' + (i == 0 ? 'menu-open' : '') + '">';
        //    row += '<a href="javascript:void(0);" class="nav-link active">';
        //    row += '<i class="nav-icon fas fa-table"></i>';
        //    row += '<p>' + key + '<i class="right fas fa-angle-left"></i></p></a>';
        //    row += '<ul class="nav nav-treeview">';
        //    var objMainMenu = [];
        //    var objSubMenu = [];
        //    var obj = {};

        //    value = value.sort(dynamicSort("PermissionPageCode"));
        //    objMainMenu = value.filter(function (currentElement) {
        //        if (currentElement.ParentPageCode == '') {
        //            return true;
        //        }
        //    });
        //    if (objMainMenu.length > 0) {
        //        var submenuHead = "";
        //        $.each(objMainMenu, function (k, val) {
        //            objSubMenu = value.filter(function (currentElement) {
        //                if (currentElement.ParentPageCode == val.PermissionPageCode) {
        //                    submenuHead = '<li class="nav-item has-treeview">';
        //                    submenuHead += '<a href="javascript:void(0);" class="nav-link">';
        //                    submenuHead += '<i class="nav-icon ' + val.NAVICON + '"></i>';
        //                    submenuHead += '<p>' + val.PermissionPageCode + '<i class="fas fa-angle-left right"></i>';
        //                    //<span class="badge badge-info right">6</span>
        //                    submenuHead += '</p></a>';
        //                    submenuHead += '<ul class="nav nav-treeview">';
        //                    submenuHead += '<li class="nav-item">';
        //                    submenuHead += '<a href="#!/' + val.PageLink + '" class="nav-link navlink">';
        //                    submenuHead += '<i class="' + val.NAVICON + ' nav-icon"></i>';
        //                    submenuHead += '<p>' + val.PermissionPageCode + '</p></a>';
        //                    submenuHead += '</li>';
        //                    return true;
        //                }
        //            });
        //            if (objSubMenu.length > 0) {
        //                objSubMenu = objSubMenu.sort(dynamicSort("PermissionPageCode"));
        //                $.each(objSubMenu, function (kk, vals) {
        //                    submenuHead += '<li class="nav-item">';
        //                    submenuHead += '<a href="#!/' + vals.PageLink + '" class="nav-link navlink">';
        //                    submenuHead += '<i class="' + vals.NAVICON + ' nav-icon"></i>';
        //                    submenuHead += '<p>' + vals.PermissionPageCode + '</p></a>';
        //                    submenuHead += '</li>';
        //                });
        //                submenuHead += '</ul></li>';
        //                row += submenuHead;
        //            }
        //            else {
        //                row += '<li class="nav-item">';
        //                row += '<a href="#!/' + val.PageLink + '" class="nav-link navlink">';
        //                row += '<i class="nav-icon ' + val.NAVICON + '"></i>';
        //                row += '<p>' + val.PermissionPageCode + '</p></a></li>';
        //            }
        //        });
        //    }
        //    row += '</ul>';
        //    row += '</li>';
        //    i++;
        //});
        //row += '</ul></nav>';
        ////$scope.LeftMenu = $sce.trustAsHtml(row);		

        //$scope.initLeftMenu = function () {
        //    $("#LeftMenu").html(row);
        //    $('[data-widget="treeview"]').Treeview('init');
        //    $('.navlink').click(function () {
        //        $('.navlink').removeClass('active');
        //        $(this).addClass('active');
        //    });
        //    console.log('ok....');
        //}
        //$scope.initLeftMenu();

        var head = '<div class="row">';
        $.each(MenuList, function (key, val) {
            if (val[0].PageLink != '') {
                head += '<div class="col-6">';
                head += '<div class="card card-default">';
                head += '<div class="card-header"><h3 class="col-12 card-title">';
                head += '<i class="fas fa-table"></i> ' + key + '</h3></div>';
                head += '<div class="card-body">';
                var body = '<div class="row">';
                val = val.sort(dynamicSort("PermissionPageCode"));

                $.each(val, function (k, value) {
                    if (value.ParentPageCode == '' && value.PageLink != '') {
                        body += '<div class="col-3 p-2 myMenu">';
                        body += "<button name=\"popoverMenu\" PageLink=\"" + value.PageLink + "\" class=\"row btn btn-block btn-lg btn-outline-success MainMenu\" onmouseenter=\"MenuListClick(this, '" + key + "','" + value.PermissionPageCode + "');\">";
                        body += '<div class="col caption">';
                        body += '<div class="row"><div class="col caption"><i class="' + value.NAVICON + ' fa-2x"></i></div></div>';
                        body += '<div class="row"><div class="col caption text-nowrap"><h6>' + value.PermissionPageCode + '</h6></div></div>';
                        body += '</div></button></div>';
                    }
                });
                body += '</div>';
                head += body;
                head += '</div></div></div>';
            }
        });
        head += '</div>';
        $("#MenuList").html(head);



        $scope.Clicked = false;
        $scope.IsMenuClick = false;

        $scope.DashboardClick = function () {
            $("#spanMenu").remove();
            $("#divBack").remove();
            $("#breadcrumb").html("MENU");
            $scope.Menu = AuthenticationService.getMenu();
            $scope.Menus = _.groupBy($scope.Menu, 'PageType');
            $scope.IsMenuClick = false;
            removeStorage("MenuData", "");
            removeStorage("PermissionPageCode");
            MenuData = getStorage("MenuData");
            $scope.$apply();
        }


        MenuData = getStorage("MenuData");
        PPageCode = getStorage("PermissionPageCode");
        if (MenuData != "") {
            MenuData = JSON.parse(MenuData).Data;
            $("#breadcrumb").html("");
            $("#divBack").remove();
            $("#spanMenu").remove();
            var $row = '<span id="spanMenu"><a href="javascript:void(0);" onclick="angular.element(this).scope().DashboardClick();">MENU</a> / ' + PPageCode + '</span>';
            $("#breadcrumb").append($row);
            $row = '<div id="divBack" class="row float-right"><div class="col">' +
                '<a href="javascript:void(0);" class="text-danger" onclick="angular.element(this).scope().DashboardClick();"><i class="fas fa-angle-double-left"></i> BACK</a>' +
                '</div></div>';
            $('#breadcrumb').append($row);
            $scope.Menus = MenuData;
            $scope.IsMenuClick = true;
            setStorage("MenuData", MenuData);
            setStorage("PermissionPageCode", PPageCode);
        }

        $scope.MenuClick = function (PermissionPageCode, values, pageType) {
            var Menus = $scope.Menus;
            var FilterMenu = [];
            angular.forEach(Menus, function (value, key) {
                if (key != pageType) {
                    //FilterMenu.push(value);
                    angular.forEach(value, function (val, key) {
                        //if (val.ParentPageCode == '')
                        FilterMenu.push(val);
                    });
                }
            });

            //console.log("FilterMenu ",FilterMenu);
            //console.log(values);

            let Menu = values;
            let filteredMenu = Menu.filter(function (currentElement) {
                if (currentElement.PermissionPageCode == PermissionPageCode || currentElement.ParentPageCode == PermissionPageCode) {
                    return true;
                }
            });

            //console.log(filteredMenu);

            FilterMenu = filteredMenu.concat(FilterMenu);
            FilterMenu.sort(dynamicSort("PageType"));
            FilterMenu = _.groupBy(FilterMenu, 'PageType');

            //console.log(FilterMenu);

            if (filteredMenu.length == 1) {
                $window.location.href = "#!/" + filteredMenu[0].PageLink;
            }
            else {
                $("#breadcrumb").html("");
                $("#divBack").remove();
                $("#spanMenu").remove();
                var $row = '<span id="spanMenu"><a href="javascript:void(0);" onclick="angular.element(this).scope().DashboardClick();">MENU</a> / ' + PermissionPageCode + '</span>';
                $("#breadcrumb").append($row);
                $row = '<div id="divBack" class="row float-right"><div class="col">' +
                    '<a href="javascript:void(0);" class="text-danger" onclick="angular.element(this).scope().DashboardClick();"><i class="fas fa-angle-double-left"></i> BACK</a>' +
                    '</div></div>';
                $('#breadcrumb').append($row);
                MenuData = FilterMenu;
                $scope.Menus = FilterMenu;
                $scope.IsMenuClick = true;
                var obj = {};
                obj["Data"] = $scope.Menus;
                setStorage("MenuData", JSON.stringify(obj));
                setStorage("PermissionPageCode", PermissionPageCode);
                $("#showMenu").hide(100, function () {
                    $("#showMenu").fadeIn(500);
                });
            }
        }

        $scope.PageLinkClick = function (PageLink) {
            $window.location.href = "#!/" + PageLink;
        }
    }
}]);

app.controller('LoadDashboardInfo', ['$rootScope', '$scope', 'Form', function ($rootScope, $scope, Form) {
    Form.LoadData("Home/LoadDashboardInfo", function (result) {
        var Table = result.Table[0];
        var Table1 = result.Table1[0];
        var Table2 = result.Table2[0];
        $("#thismonth").html(Table.ThisMonth);
        $("#lastmonth").html(Table.LastMonth);
        var breadbarValue = Object.values(Table1);
        var breadbar = {
            series: [
                {
                    name: "",
                    data: breadbarValue,
                },
            ],
            chart: {
                type: "bar",
                width: 70,
                height: 40,
                toolbar: {
                    show: false,
                },
                sparkline: {
                    enabled: true,
                },
            },
            colors: ["var(--bs-primary)"],
            grid: {
                show: false,
            },
            plotOptions: {
                bar: {
                    horizontal: false,
                    borderRadius: 2,
                    columnWidth: "50%",
                    barHeight: "100%",
                },
            },
            dataLabels: {
                enabled: false,
            },
            stroke: {
                show: true,
                width: 0,
                colors: ["transparent"],
            },
            xaxis: {
                axisBorder: {
                    show: false,
                },
                axisTicks: {
                    show: false,
                },
                labels: {
                    show: false,
                },
            },
            yaxis: {
                labels: {
                    show: false,
                },
            },
            axisBorder: {
                show: false,
            },
            fill: {
                opacity: 1,
            },
            tooltip: {
                theme: "dark",
                style: {
                    fontFamily: "inherit",
                },
                x: {
                    show: false,
                },
                y: {
                    formatter: undefined,
                },
            },
        };
        var chart_column_breadbar = new ApexCharts(
            document.querySelector(".breadbar"),
            breadbar
        );
        chart_column_breadbar.render();
        breadbarValue = Object.values(Table2);
        var breadbar2 = {
            series: [
                {
                    name: "",
                    data: breadbarValue,
                },
            ],
            chart: {
                type: "bar",
                width: 70,
                height: 40,
                toolbar: {
                    show: false,
                },
                sparkline: {
                    enabled: true,
                },
            },
            colors: ["var(--bs-secondary)"],
            grid: {
                show: false,
            },
            plotOptions: {
                bar: {
                    horizontal: false,
                    borderRadius: 2,
                    columnWidth: "50%",
                    barHeight: "100%",
                },
            },
            dataLabels: {
                enabled: false,
            },
            stroke: {
                show: true,
                width: 0,
                colors: ["transparent"],
            },
            xaxis: {
                axisBorder: {
                    show: false,
                },
                axisTicks: {
                    show: false,
                },
                labels: {
                    show: false,
                },
            },
            yaxis: {
                labels: {
                    show: false,
                },
            },
            axisBorder: {
                show: false,
            },
            fill: {
                opacity: 1,
            },
            tooltip: {
                theme: "dark",
                style: {
                    fontFamily: "inherit",
                },
                x: {
                    show: false,
                },
                y: {
                    formatter: undefined,
                },
            },
        };
        var chart_column_breadbar2 = new ApexCharts(
            document.querySelector(".breadbar2"),
            breadbar2
        );
        chart_column_breadbar2.render();
    });
}]);
var loading = "";
var isShowpopover = true;
var isPopoverClick = false;
var TimeOutSubMenuList = "";
function MenuListClick(THIS, PageType, PageCode) {
    window.clearTimeout(TimeOutSubMenuList);
    isPopoverClick = false;
    $(".popover").remove();
    //$(".popover").popover('hide');
    //$('.popover').popover('dispose');

    loading = '<div class="myLoading"><i class="fas fa-2x fa-sync-alt fa-spin"></i></div>';
    var PageLink = "";
    let filteredMenu = MenuList[PageType].filter(function (currentElement) {
        if (currentElement.PageType == PageType && currentElement.ParentPageCode == PageCode) {
            PageLink = currentElement.PageLink;
            return true;
        }
    });
    if (filteredMenu.length == 0) {
        filteredMenu = MenuList[PageType].filter(function (currentElement) {
            if (currentElement.PermissionPageCode == PageCode) {
                return true;
            }
        });
        $(THIS).on('click', function () {
            $(THIS).addClass("active");
            $(THIS).attr("disabled", "true");            
            $(THIS).children('.caption').append(loading);
            var PageLink = filteredMenu[0].PageLink;
            //$("#LeftMenu nav").hide();
            //var row = '<nav id="showleft_menu"><ul class="nav nav-pills nav-sidebar flex-column" data-widget="treeview" role="menu" data-accordion="false">';
            //row += '<li class="nav-item has-treeview menu-open">';
            //row += '<a href="javascript:void(0);" class="nav-link active">';
            //row += '<i class="nav-icon fas fa-table"></i>';
            //row += '<p>' + filteredMenu[0].PermissionPageCode + '</p></a>';            
            //row += '</ul></nav>';
            //$("#showleft_menu").remove();
            //$("#LeftMenu").append(row);
            window.location.href = "#!/" + PageLink;

        });
    }
    else {
        filteredMenu = MenuList[PageType].filter(function (currentElement) {
            if (currentElement.PageType == PageType && currentElement.ParentPageCode == PageCode) { //|| currentElement.PermissionPageCode == PageCode
                PageLink = currentElement.PageLink;
                return true;
            }
        });        
        var row = '<div class="row SubMenu" id="SubMenuList">';
        filteredMenu = filteredMenu.sort(dynamicSort("PermissionPageCode")); 
        var obj = new Array();
        obj = JSON.stringify(filteredMenu).replace(/"/g, '\'');
        $.each(filteredMenu, function (key, value) {
            row += '<div class="col-4 p-2 myMenu">';
            row += "<button name=\"SubMenuList\" class=\"row btn btn-block btn-lg btn-outline-success\" onclick=\"openLinkMenu(this, '" + value.PageLink + "', ";
            row += obj;
            row += ", '" + value.PermissionPageCode + "');\">";
            row += '<div class="col caption">';
            row += '<div class="row"><div class="col caption"><i class="' + value.NAVICON + ' fa-2x"></i></div></div>';
            row += '<div class="row"><div class="col caption text-nowrap"><h6>' + value.PermissionPageCode + '</h6></div></div>';
            row += '</div></button></div>';
        });
        row += '</div>';

        //var index = $("button[name=popoverMenu]").index(THIS);
        //$("button[name=popoverMenu]").eq(index).parent().parent().hide(100, function () {
        //    $("button[name=popoverMenu]").eq(index).parent().parent().after(row);
        //    $("button[name=SubMenuList]").fadeIn(500);
        //    var TextHeader = $("button[name=popoverMenu]").eq(index).parent().parent().parent().parent().children('.card-header').children('.card-title').text();
        //    row = "<span><span><i class=\"fas fa-table\"></i> <a href=\"javascript:void(0);\" onclick=\"clickBack(this,'" + PageType + "');\">" + TextHeader + "</a> / " + PageCode;
        //    row += '</span></span><div class="row float-right"><div class="col">';
        //    row += "<a href=\"javascript:void(0);\" onclick=\"clickBack(this,'" + PageType + "');\" class=\"text-danger\"><i class=\"fas fa-angle-double-left\"></i> BACK</a>";
        //    row+='</div></div>';
        //    $("button[name=popoverMenu]").eq(index).parent().parent().parent().parent().children('.card-header').children('.card-title').html(row);

        //});

        $(".popover").remove();
        $(THIS).popover({
            sanitize: false,
            //toggle: true,
            //trigger: 'focus',
            placement: 'bottom',
            html: true,
            content: row,
            //selector: true,
            //delay: { "show": 900, "hide": 500 }
        });
        //$('.popover').popover('dispose');
        //
        
        $(THIS).popover('show');
        $(".popover").show();
        //$(THIS).on('click', function () {
        //    if (isShowpopover) {
        //        //isShowpopover = false;
        //        $(".popover").hide();
        //    }
        //    else if (!isShowpopover) {
        //        //isShowpopover = true;
        //        $(".popover").show();
        //        isPopoverClick = true;
        //    }
        //});
        $(THIS).on('shown.bs.popover', function () {
            isShowpopover = true;
            isPopoverClick = true;
            window.clearTimeout(TimeOutSubMenuList);
        });
        $(THIS).on('hidden.bs.popover', function () {
            isShowpopover = false;
            isPopoverClick = false;
            $(".popover").hide();            
        });
        
        $(THIS).on('mouseleave', function () {
            isPopoverClick = false;
            window.clearTimeout(TimeOutSubMenuList);
            TimeOutSubMenuList = setTimeout(function () {
                $(".popover").hide();
            }, 250);
        });
        $("button[name=SubMenuList]").on('mousemove', function () {
            window.clearTimeout(TimeOutSubMenuList);          
        });
        $("button[name=SubMenuList]").on('mouseleave', function () {
            window.clearTimeout(TimeOutSubMenuList);
            TimeOutSubMenuList = setTimeout(function () {
                $(".popover").hide();
            }, 250);
        });

        $(THIS).on('click', function () {
            $(THIS).addClass("active");
            $(THIS).attr("disabled", "true");
            $(THIS).children('.caption').append(loading);
            var PageLink = $(THIS).attr("PageLink");
            //$("#LeftMenu nav").hide();
            //var row = '<nav id="showleft_menu"><ul class="nav nav-pills nav-sidebar flex-column" data-widget="treeview" role="menu" data-accordion="false">';
            //row += '<li class="nav-item has-treeview menu-open">';
            //row += '<a href="javascript:void(0);" class="nav-link active">';
            //row += '<i class="nav-icon fas fa-table"></i>';
            //row += '<p>' + filteredMenu[0].PermissionPageCode + '</p></a>';
            //row += '</ul></nav>';
            //$("#showleft_menu").remove();
            //$("#LeftMenu").append(row);
            window.location.href = "#!/" + PageLink;

        });
    }    
}

//$('html').on('click', function (e) {
//    if (typeof $(e.target).data('original-title') == 'undefined' &&
//        !$(e.target).parents().is('.popover.in') &&
//        !isPopoverClick) {
//        //$('[data-original-title]').popover('hide');
//        $(".popover").remove();
//    }
//});

function clickBack(THIS, PageType) {
    $(THIS).parent().parent().parent().parent().parent().children('.card-body').children('.row').fadeIn(500, function () {
        $(THIS).parent().parent().parent().parent().parent().children('.card-body').children('.SubMenu').remove();
        $(THIS).parent().parent().parent().parent().html('<h3 class="col-12 card-title"><i class=\"fas fa-table\"></i> ' + PageType + '</h3>');
    });

    //var index = $("button[name=popoverMenu]").index(THIS);
    //$("button[name=popoverMenu]").eq(index).parent().parent().hide(100, function () {
    //    $("button[name=popoverMenu]").eq(index).parent().parent().after(row);
    //    $("button[name=SubMenuList]").fadeIn(500);

    //$(THIS).hide(100, function () {
    //    $("button[name=popoverMenu]").fadeIn(500);
    //});
}
function openLinkMenu(THIS, PageLink, filteredMenu, PageCode) {
    //$("#LeftMenu nav").hide();
    var row = '<nav id="showleft_menu"><ul class="nav nav-pills nav-sidebar flex-column" data-widget="treeview" role="menu" data-accordion="false">';
    row += '<li class="nav-item has-treeview menu-open">';
    row += '<a href="javascript:void(0);" class="nav-link active">';
    row += '<i class="nav-icon fas fa-table"></i>';
    row += '<p>' + PageCode + '<i class="right fas fa-angle-left"></i></p></a>';
    row += '<ul class="nav nav-treeview">';
    $.each(filteredMenu, function (key, val) {
        row += '<li class="nav-item">';
        row += '<a href="#!/' + val.PageLink + '" class="nav-link navlink ' + (val.PermissionPageCode == PageCode ? 'active' : '') + '">';
        row += '<i class="' + val.NAVICON + ' nav-icon"></i>';
        row += '<p>' + val.PermissionPageCode + '</p></a>';
        row += '</li>';
    });
    row += '</ul></li></ul></nav>';
    //$("#showleft_menu").remove();
    //$("#LeftMenu").append(row);
    isPopoverClick = true;
    $(THIS).attr("disabled", "true");
    $(THIS).addClass("active");
    $(THIS).children('.caption').append(loading);
    //DialogSweetAlert("load");
    //showToast();
    //console.log('lll');
    //return;
    window.location.href = "#!/" + PageLink;
    $('.navlink').click(function () {
        $('.navlink').removeClass('active');
        $(this).addClass('active');
    });
}
