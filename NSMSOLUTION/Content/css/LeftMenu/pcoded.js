"use strict";
//$(document).ready(function() {
    
    //$(".mobile-menu").on('click', function() {
    //    var $this = $(this);
    //    $this.toggleClass('on');
    //});
    //$("#mobile-collapse").on('click', function() {
    //    $(".pcoded-navbar").toggleClass("navbar-collapsed");
    //});

    //$(".search-btn").on('click', function() {
    //    var $this = $(this);
    //    $(".main-search").addClass('open');
    //    $(".main-search .form-control").css({
    //        'width': '90px'
    //    });
    //});
    //$(".search-close").on('click', function() {
    //    var $this = $(this);
    //    $(".main-search").removeClass('open');
    //    $(".main-search .form-control").css({
    //        'width': '0'
    //    });
    //});
    
    //// open messages
    //$('.header-user-list .userlist-box').on('click', function() {
    //    $('.header-chat').addClass('open');
    //    $('.header-user-list').toggleClass('msg-open');
    //});
    //// close user list
    //$('.h-back-user-list').on('click', function() {
    //    $('.header-chat').removeClass('open');
    //    $('.header-user-list').removeClass('msg-open');
    //});
    ////  full chat
    //$('.h-close-text').on('click', function() {
    //    $('.header-chat').removeClass('open');
    //    $('.header-user-list').removeClass('open');
    //    $('.header-user-list').removeClass('msg-open');
    //});
    //$('.btn-attach').click(function() {
    //    $('.chat-attach').trigger('click');
    //});
    //$('.h-send-chat').on('keyup', function(e) {
    //    fc(e);
    //});
    //$('.btn-send').on('click', function(e) {
    //    cfc(e);
    //});
    //if (vw <= 991) {
    //    $(".main-search").addClass('open');
    //    $(".main-search .form-control").css({
    //        'width': '90px'
    //    });
    //}
    //// Friend scroll
    //if (vw >= 1024) {
    //    var px = new PerfectScrollbar('.main-friend-cont', {
    //        wheelSpeed: .5,
    //        swipeEasing: 0,
    //        suppressScrollX: !0,
    //        wheelPropagation: 1,
    //        minScrollbarLength: 40,
    //    });
    //    var px = new PerfectScrollbar('.main-chat-cont', {
    //        wheelSpeed: .5,
    //        swipeEasing: 0,
    //        suppressScrollX: !0,
    //        wheelPropagation: 1,
    //        minScrollbarLength: 40,
    //    });
    //}
    // Menu scroll
    //if (!$('.pcoded-navbar').hasClass('theme-horizontal')) {
    //    var vw = $(window)[0].innerWidth;
    //    if (vw < 992 || $('.pcoded-navbar').hasClass('menupos-static')) {
    //        var px = new PerfectScrollbar('.navbar-content', {
    //            wheelSpeed: .5,
    //            swipeEasing: 0,
    //            suppressScrollX: !0,
    //            wheelPropagation: 1,
    //            minScrollbarLength: 40,
    //        });
    //    }else{
    //        var px = new PerfectScrollbar('.navbar-content', {
    //            wheelSpeed: .5,
    //            swipeEasing: 0,
    //            suppressScrollX: !0,
    //            wheelPropagation: 1,
    //            minScrollbarLength: 40,
    //        });
    //    }
    //}

    //function fc(e) {
    //    if (e.which == 13) {
    //        cfc(e)
    //    }
    //};

    //function cfc(e) {
    //    $('.header-chat .main-friend-chat').append('' +
    //        '<div class="media chat-messages">' +
    //            '<div class="media-body chat-menu-reply">' +
    //                '<div class="">' +
    //                    '<p class="chat-cont">' + $('.h-send-chat').val() + '</p>' +
    //                '</div>' +
    //                '<p class="chat-time">now</p>' +
    //            '</div>' +
    //        '</div>' +
    //        '');
    //    frc($('.h-send-chat').val());
    //    fsc();
    //    $('.h-send-chat').val(null);
    //};

    //function frc(wrmsg) {
    //    setTimeout(function() {
    //        $('.header-chat .main-friend-chat').append('' +
    //            '<div class="media chat-messages typing">' +
    //                '<a class="media-left photo-table" href="#!"><img class="media-object img-radius img-radius m-t-5" src="assets/images/user/avatar-2.jpg" alt="Generic placeholder image"></a>' +
    //                '<div class="media-body chat-menu-content">' +
    //                    '<div class="rem-msg">' +
    //                        '<p class="chat-cont">Typing . . .</p>' +
    //                    '</div>' +
    //                    '<p class="chat-time">now</p>' +
    //                '</div>' +
    //            '</div>' +
    //            '');
    //        fsc();
    //    }, 1500);
    //    setTimeout(function() {
    //        document.getElementsByClassName("rem-msg")[0].innerHTML = "<p class='chat-cont'>hello superior personality you write</p> <p class='chat-cont'>" + wrmsg + "</p>";
    //        $('.rem-msg').removeClass("rem-msg");
    //        $('.typing').removeClass("typing");
    //        fsc();
    //    }, 3000);
    //};

    //function fsc() {
    //    var tmph = $('.header-chat .main-friend-chat');
    //    $('.main-chat-cont.scroll-div').scrollTop(tmph.outerHeight());
    //}

//// menu [ compact ]
//function togglemenu() {
//    var vw = $(window)[0].innerWidth;
//    if ($(".pcoded-navbar").hasClass('theme-horizontal') == false) {
//        if (vw <= 1200 && vw >= 992) {
//            $(".pcoded-navbar").addClass("navbar-collapsed");
//        }
//        if (vw < 992) {
//            $(".pcoded-navbar").removeClass("navbar-collapsed");
//        }
//    }
//}
// ===============

// toggle full screen
//function toggleFullScreen() {
//    var a = $(window).height() - 10;

//    if (!document.fullscreenElement && // alternative standard method
//        !document.mozFullScreenElement && !document.webkitFullscreenElement) { // current working methods
//        if (document.documentElement.requestFullscreen) {
//            document.documentElement.requestFullscreen();
//        } else if (document.documentElement.mozRequestFullScreen) {
//            document.documentElement.mozRequestFullScreen();
//        } else if (document.documentElement.webkitRequestFullscreen) {
//            document.documentElement.webkitRequestFullscreen(Element.ALLOW_KEYBOARD_INPUT);
//        }
//    } else {
//        if (document.cancelFullScreen) {
//            document.cancelFullScreen();
//        } else if (document.mozCancelFullScreen) {
//            document.mozCancelFullScreen();
//        } else if (document.webkitCancelFullScreen) {
//            document.webkitCancelFullScreen();
//        }
//    }
//    $('.full-screen > i').toggleClass('icon-maximize');
//    $('.full-screen > i').toggleClass('icon-minimize');
//}
// =============   layout builder   =============
$.fn.pcodedmenu = function(settings) {
    var oid = this.attr("id");
    var defaults = {
        themelayout: 'vertical',
        MenuTrigger: 'click',
        SubMenuTrigger: 'click',
    };
    var settings = $.extend({}, defaults, settings);
    var PcodedMenu = {
        PcodedMenuInit: function() {
            PcodedMenu.HandleMenuTrigger();
            PcodedMenu.HandleSubMenuTrigger();
            PcodedMenu.HandleOffset();
        },
        HandleSubMenuTrigger: function() {
            var $window = $(window);
            var newSize = $window.width();
            if ($('.pcoded-navbar').hasClass('theme-horizontal') == true) {
                if (newSize >= 768) {
                    var $dropdown = $(".pcoded-inner-navbar .pcoded-submenu > li.pcoded-hasmenu");
                    $dropdown.off('click').off('mouseenter mouseleave').hover(
                        function() {
                            $(this).addClass('pcoded-trigger');
                        },
                        function() {
                            $(this).removeClass('pcoded-trigger');
                        }
                    );
                } else {
                    var $dropdown = $(".pcoded-inner-navbar .pcoded-submenu > li > .pcoded-submenu > li");
                    $dropdown.off('mouseenter mouseleave').off('click').on('click',
                        function() {
                            var str = $(this).closest('.pcoded-submenu').length;
                            if (str === 0) {
                                if ($(this).hasClass('pcoded-trigger')) {
                                    $(this).removeClass('pcoded-trigger');
                                    $(this).children('.pcoded-submenu').slideUp();
                                } else {
                                    $('.pcoded-submenu > li > .pcoded-submenu > li.pcoded-trigger').children('.pcoded-submenu').slideUp();
                                    $(this).closest('.pcoded-inner-navbar').find('li.pcoded-trigger').removeClass('pcoded-trigger');
                                    $(this).addClass('pcoded-trigger');
                                    $(this).children('.pcoded-submenu').slideDown();
                                }
                            } else {
                                if ($(this).hasClass('pcoded-trigger')) {
                                    $(this).removeClass('pcoded-trigger');
                                    $(this).children('.pcoded-submenu').slideUp();
                                } else {
                                    $('.pcoded-submenu > li > .pcoded-submenu > li.pcoded-trigger').children('.pcoded-submenu').slideUp();
                                    $(this).closest('.pcoded-submenu').find('li.pcoded-trigger').removeClass('pcoded-trigger');
                                    $(this).addClass('pcoded-trigger');
                                    $(this).children('.pcoded-submenu').slideDown();
                                }
                            }
                        });
                    $(".pcoded-inner-navbar .pcoded-submenu > li > .pcoded-submenu > li").on('click', function(e) {
                        e.stopPropagation();
                        alert( "click call" );
                        var str = $(this).closest('.pcoded-submenu').length;
                        if (str === 0) {
                            if ($(this).hasClass('pcoded-trigger')) {
                                $(this).removeClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideUp();
                            } else {
                                $('.pcoded-hasmenu li.pcoded-trigger').children('.pcoded-submenu').slideUp();
                                $(this).closest('.pcoded-inner-navbar').find('li.pcoded-trigger').removeClass('pcoded-trigger');
                                $(this).addClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideDown();
                            }
                        } else {
                            if ($(this).hasClass('pcoded-trigger')) {
                                $(this).removeClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideUp();
                            } else {
                                $('.pcoded-hasmenu li.pcoded-trigger').children('.pcoded-submenu').slideUp();
                                $(this).closest('.pcoded-submenu').find('li.pcoded-trigger').removeClass('pcoded-trigger');
                                $(this).addClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideDown();
                            }
                        }
                    });
                }
            }
            switch (settings.SubMenuTrigger) {
                case 'click':
                    $('.pcoded-navbar .pcoded-hasmenu').removeClass('is-hover');
                    $(".pcoded-inner-navbar .pcoded-submenu > li > .pcoded-submenu > li").on('click', function(e) {
                        e.stopPropagation();
                        var str = $(this).closest('.pcoded-submenu').length;
                        if (str === 0) {
                            if ($(this).hasClass('pcoded-trigger')) {
                                $(this).removeClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideUp();
                            } else {
                                $('.pcoded-submenu > li > .pcoded-submenu > li.pcoded-trigger').children('.pcoded-submenu').slideUp();
                                $(this).closest('.pcoded-inner-navbar').find('li.pcoded-trigger').removeClass('pcoded-trigger');
                                $(this).addClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideDown();
                            }
                        } else {
                            if ($(this).hasClass('pcoded-trigger')) {
                                $(this).removeClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideUp();
                            } else {
                                $('.pcoded-submenu > li > .pcoded-submenu > li.pcoded-trigger').children('.pcoded-submenu').slideUp();
                                $(this).closest('.pcoded-submenu').find('li.pcoded-trigger').removeClass('pcoded-trigger');
                                $(this).addClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideDown();
                            }
                        }
                    });
                    $(".pcoded-submenu > li").on('click', function(e) {
                        e.stopPropagation();
                        var str = $(this).closest('.pcoded-submenu').length;
                        if (str === 0) {
                            if ($(this).hasClass('pcoded-trigger')) {
                                $(this).removeClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideUp();
                            } else {
                                $('.pcoded-hasmenu li.pcoded-trigger').children('.pcoded-submenu').slideUp();
                                $(this).closest('.pcoded-inner-navbar').find('li.pcoded-trigger').removeClass('pcoded-trigger');
                                $(this).addClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideDown();
                            }
                        } else {
                            if ($(this).hasClass('pcoded-trigger')) {
                                $(this).removeClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideUp();
                            } else {
                                $('.pcoded-hasmenu li.pcoded-trigger').children('.pcoded-submenu').slideUp();
                                $(this).closest('.pcoded-submenu').find('li.pcoded-trigger').removeClass('pcoded-trigger');
                                $(this).addClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideDown();
                            }
                        }
                    });
                    break;
            }
        },
        HandleMenuTrigger: function() {
            var $window = $(window);
            var newSize = $window.width();
            if ($('.pcoded-navbar').hasClass('theme-horizontal') == true) {
                var $dropdown = $(".pcoded-inner-navbar > li");
                if (newSize >= 768) {
                    $dropdown.off('click').off('mouseenter mouseleave').hover(
                        function() {
                            $(this).addClass('pcoded-trigger');
                            if ($('.pcoded-submenu', this).length) {
                                var elm = $('.pcoded-submenu:first', this);
                                var off = elm.offset();
                                var l = off.left;
                                var w = elm.width();
                                var docH = $(window).height();
                                var docW = $(window).width();

                                var isEntirelyVisible = (l + w <= docW);
                                if (!isEntirelyVisible) {
                                    var temp = $('.sidenav-inner').attr('style');
                                    $('.sidenav-inner').css({'margin-left': (parseInt(temp.slice(12, temp.length - 3)) - 80)});
                                    $('.sidenav-horizontal-prev').removeClass('disabled');
                                } else {
                                    $(this).removeClass('edge');
                                }
                            }
                        },
                        function() {
                            $(this).removeClass('pcoded-trigger');
                        }
                    );
                } else {
                    $dropdown.off('mouseenter mouseleave').off('click').on('click',
                        function() {
                            if ($(this).hasClass('pcoded-trigger')) {
                                $(this).removeClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideUp();
                            } else {
                                $('li.pcoded-trigger').children('.pcoded-submenu').slideUp();
                                $(this).closest('.pcoded-inner-navbar').find('li.pcoded-trigger').removeClass('pcoded-trigger');
                                $(this).addClass('pcoded-trigger');
                                $(this).children('.pcoded-submenu').slideDown();
                            }
                        }
                    );
                }
            }
            switch (settings.MenuTrigger) {
                case 'click':
                    $('.pcoded-navbar').removeClass('is-hover');
                    $(".pcoded-inner-navbar > li:not(.pcoded-menu-caption) ").on('click', function() {
                        if ($(this).hasClass('pcoded-trigger')) {
                            $(this).removeClass('pcoded-trigger');
                            $(this).children('.pcoded-submenu').slideUp();
                        } else {
                            $('li.pcoded-trigger').children('.pcoded-submenu').slideUp();
                            $(this).closest('.pcoded-inner-navbar').find('li.pcoded-trigger').removeClass('pcoded-trigger');
                            $(this).addClass('pcoded-trigger');
                            $(this).children('.pcoded-submenu').slideDown();
                        }
                    });
                    break;
            }
        },
        HandleOffset: function() {
            switch (settings.themelayout) {
                case 'horizontal':
                    var trigger = settings.SubMenuTrigger;
                    if (trigger === "hover") {
                        $("li.pcoded-hasmenu").on('mouseenter mouseleave', function(e) {
                            if ($('.pcoded-submenu', this).length) {
                                var elm = $('.pcoded-submenu:first', this);
                                var off = elm.offset();
                                var l = off.left;
                                var w = elm.width();
                                var docH = $(window).height();
                                var docW = $(window).width();

                                var isEntirelyVisible = (l + w <= docW);
                                if (!isEntirelyVisible) {
                                    $(this).addClass('edge');
                                } else {
                                    $(this).removeClass('edge');
                                }
                            }
                        });
                    } else {
                        $("li.pcoded-hasmenu").on('click', function(e) {
                            e.preventDefault();
                            if ($('.pcoded-submenu', this).length) {
                                var elm = $('.pcoded-submenu:first', this);
                                var off = elm.offset();
                                var l = off.left;
                                var w = elm.width();
                                var docH = $(window).height();
                                var docW = $(window).width();

                                var isEntirelyVisible = (l + w <= docW);
                                if (!isEntirelyVisible) {
                                    $(this).toggleClass('edge');
                                }
                            }
                        });
                    }
                    break;
                default:
            }
        },
    };
    PcodedMenu.PcodedMenuInit();
};
$("#pcoded").pcodedmenu({
    MenuTrigger: 'click',
    SubMenuTrigger: 'click',
});
// menu [ Mobile ]
$("#mobile-collapse,#mobile-collapse1").click(function(e) {
    var vw = $(window)[0].innerWidth;
    if (vw < 992) {
        $(".pcoded-navbar").toggleClass('mob-open');
        e.stopPropagation();
    }
});
$(window).ready(function() {
    var vw = $(window)[0].innerWidth;
    $(".pcoded-navbar").on('click tap', function(e) {
        e.stopPropagation();
    });
    $('.pcoded-main-container,.pcoded-header').on("click", function() {
        if (vw < 992) {
            if ($(".pcoded-navbar").hasClass("mob-open") == true) {
                $(".pcoded-navbar").removeClass('mob-open');
                $("#mobile-collapse,#mobile-collapse1").removeClass('on');
            }
        }
    });
    // mobile header
    $("#mobile-header").on('click', function() {
        $(".navbar-collapse,.m-header").slideToggle();
    });
});
// Layout 1 navbar start
$('.layout-1 .sidemenu a').on('click',function(){
    var port = $(this);
    port.parents('li').siblings().removeClass('active');
    port.parents('li').addClass('active');
    $('.side-content .sidelink').slideUp();
    $('.side-content .sidelink.' + port.attr('data-cont') ).slideDown();
});
$('.layout-1 .toggle-sidemenu').on('click',function(){
    var port = $(this);
    $('.pcoded-navbar').toggleClass('hide-sidemenu');
});
// Layout 1 navbar End