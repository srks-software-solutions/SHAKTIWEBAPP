$(document).ready(function () {

    PageLoad();

    var MenuId = [];
    var UncheckedMenuId = [];


    $(document).on('change', '.chkmenu', function (event) {
        var Dat = $(this).siblings();
        var Id = Dat[0].innerHTML;
        var isCheked = this.checked;
        if (isCheked == true) {
            MenuId.push(Id);
            UncheckedMenuId.pop(Id);
        }
        else if (isCheked == false) {
            MenuId.pop(Id);
            UncheckedMenuId.push(Id);
        }
    });


    $("#selectRoles").change(function () {

        var roleId = $('#selectRoles').val();
        if (roleId == '') {
            $(".chkmenu").prop('checked', false);
        }
        if (roleId != '0') {
            $.ajax({
                url: '/User/GetCheckboxesForUser',
                contentType: "application/json; charset=utf-8",
                data: { roleId },
                datatype: "json",
                success: function (msg) {
                    $('.chkmenu').prop('checked', false);
                    var response = JSON.parse(msg);
                    if (response.length > 0) {

                        for (var i = 0; i < response.length; i++) {

                            var id = response[i].MenuName.replace(/[&/]/g, '');
                            id = $.trim(id.split(" ").join(""));
                            if (response[i].MenuStatus == 'True')
                                $('#Chk_' + id + '').prop('checked', true);
                            else
                                $('#Chk_' + id + '').prop('checked', false);
                        }
                    }
                },
            });
        }
        else
            Reset();

    });


    $(document).on('click', '#btnReset', function (event) {
        Reset();
    });

    function Reset() {

        $(".chkmenu").prop('checked', false);

        $('#selectUsers').val('0');
    }


    $(document).on('click', '#btnSubmit', function (event) {
        var roleId = $('#selectRoles').val();

        $('.warning').css("display", "none");
        if (roleId != '0') {
            var Menuids = JSON.stringify(MenuId);
            var UnMenuids = JSON.stringify(UncheckedMenuId);
            $.ajax({
                url: "/User/UpdateMenus",
                data: { roleId, Menuids, UnMenuids },
                contentType: "application/json; charset=utf-8",
                dataType: "text",
                success: function (response) {
                    if (response != "") {
                        MenuId = [];
                        UncheckedMenuId = [];
                        Reset();
                        $('.ttb-popup').removeClass('is-visible');
                        windowResize();
                        $('#DvSuccess').text('Menus Updated Successfully');
                        $('.alert-section').css("display", "block");
                        $('.success').css("display", "block");
                        //pageload();
                    }
                },
                failure: function (response) {
                    //alert(response.responseText);
                },
                error: function (e) {
                    //alert(e);
                }
            });

        }
        else {
            // windowResize();
            $('.success').css("display", "none");
            $('#divWarn').text('Please Select User to Submit');
            $('.alert-section').css("display", "block");
            $('.warning').css("display", "block");
        }

        window.location.href = '/User/UserManagement';
    });


    function PageLoad() {
        $('.UserMng').html('');
        $.get('/User/GetUserMenus', function (msg) {
            var cssst = "";

            if (msg != '') {
                var response = JSON.parse(msg);
                cssst += '<div class="row form-group " style="margin-top: 35px;margin-left:10%">';

                for (var i = 0; i < response.length; i++) {
                    var id = response[i].MenuName.replace(/[&/]/g, '');
                    id = $.trim(id.split(" ").join(""));
                    cssst += '<div class="col col-md-4"><input type="checkbox" class="chkmenu" id="Chk_' + id + '" name="tech" value="" style="transform: scale(1.5); margin-right: 10px"><h1 style="display:none;">' + response[i].MenuId + '</h1>';
                    cssst += '<label for="text-input" class=" form-control-label" style="font-size: 16px; ">' + response[i].MenuName + '</label></div>';

                }
                cssst += '</div>';
                $(cssst).appendTo('.UserMng');
            }
            else
                window.location.href = '/Login/Login';
        });
    }
});