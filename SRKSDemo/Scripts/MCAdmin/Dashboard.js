$(document).ready(function () {
    GetDashboardMenus();
});

function GetDashboardMenus() {
    var sessionValue = $("#hdnSession").data('value');
    if (sessionValue == "") {
        window.location.href = '/Login/Login';
    }
    $('.dashboardmenu').html('');
    $.get('/User/GetDashboardMenus', function (msg) {
        var cssst = "";

        if (msg != '') {
            var response = JSON.parse(msg);
            var j = 3;
            for (var i = 0; i < response.length; i++) {
                if (i % 4 == 0) {
                    cssst += '<div class="row">';
                }
                cssst += ' <div class="col-md-3"> <a href = ' + response[i].MenuURL + ' data-placement="bottom" > <div class="tile-box tile-box-alt bg-' + response[i].ColourDiv + '"style="' + response[i].Style + '">'
                    + '<div class="tile-header"> ' + response[i].MenuName + '</div>  <div class="tile-content-wrapper"><img src=' + response[i].ImageURL + '></div>'
                    + '<div><div class="tile-footer tooltip-button">view details<i class="fa fa-arrow-right glyph-icon" aria-hidden="true" ></i ></div></div></div></a></div>';

                if (i == j || i == length - 1) {
                    cssst += '</div>';
                    j = j + 4;
                }
            }
            $('.dashboardmenu').html(cssst);
        }
        else {
            $('.dashboardmenu').html('');
        }
    });
}