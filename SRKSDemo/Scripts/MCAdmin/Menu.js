$(function () {
    $('.sidemenubar').html('');
    $.get('/User/GetSideMenubar', function (msg) {
        var cssst = "";

        if (msg != '') {
            var response = JSON.parse(msg);
            for (var i = 0; i < response.length; i++) {

                if (response[i].SubMenus.length > 0) {
                    for (var j = 0; j < response[i].SubMenus.length; j++) {
                        if (j == 0) {
                            cssst += '<li><a> <img src=' + response[i].ImageURL + '><span class="set_name">' + response[i].MenuName + '</span><span class="fa fa-chevron-down"></span></a>';

                            cssst += '<ul class="nav child_menu"><li><a href=' + response[i].SubMenus[j].SubMenuURL + '>' + response[i].SubMenus[j].SubMenuName + '</a></li>';
                        }
                        if (j != 0) {
                            cssst += '<li><a href=' + response[i].SubMenus[j].SubMenuURL + '>' + response[i].SubMenus[j].SubMenuName + '</a></li>';
                        }
                        if (j == response[i].SubMenus.length - 1) {
                            cssst += '</ul></li>';
                        }
                    }
                }
                else {
                    cssst += '<li> <a href=' + response[i].MenuURL + '> <img src= ' + response[i].ImageURL + '><span class="set_name" >' + response[i].MenuName + '</span> </a></li>';
                }
            }
            $('.sidemenubar').html(cssst);
        }
        else {
            $('.sidemenubar').html('');
        }
    });
});

$('#sidebar-menu').on('click', 'a', function (e) {
    console.log('clicked - sidebar_menu');
    var $li = $(this).parent();

    if ($li.is('.active')) {
        $li.removeClass('active active-sm');
        $('ul:first', $li).slideUp(function () {
            setContentHeight();
        });
    } else {
        // prevent closing menu if we are on child menu
        if (!$li.parent().is('.child_menu')) {
            $SIDEBAR_MENU.find('li').removeClass('active active-sm');
            $SIDEBAR_MENU.find('li ul').slideUp();
        } else {
            if ($BODY.is(".nav-sm")) {
                $SIDEBAR_MENU.find("li").removeClass("active active-sm");
                $SIDEBAR_MENU.find("li ul").slideUp();
            }
        }
        if ($li.is('.active')) {
            $li.removeClass('active');
        }
        else {
            $li.addClass('active');
        }

        $('ul:first', $li).slideDown(function () {
            setContentHeight();
        });
    }
});

function setContentHeight() {
    // reset height
    $RIGHT_COL.css('min-height', $(window).height());

    var bodyHeight = $BODY.outerHeight(),
        footerHeight = $BODY.hasClass('footer_fixed') ? -10 : $FOOTER.height(),
        leftColHeight = $LEFT_COL.eq(1).height() + $SIDEBAR_FOOTER.height(),
        contentHeight = bodyHeight < leftColHeight ? leftColHeight : bodyHeight;

    // normalize content
    contentHeight -= $NAV_MENU.height() + footerHeight;

    $RIGHT_COL.css('min-height', contentHeight);
}