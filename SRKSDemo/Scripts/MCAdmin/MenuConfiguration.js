$(document).ready(function () {


    $("#selectMenus").change(function () {
        var MenuId = $('#selectMenus').val();
        Menus(MenuId);
    });

    $("#selectMenus").on('click', '#selectMenus', function (event) {
        var MenuId = $('#selectMenus').val();
        Menus(MenuId);
    });

    $(document).on('click', '.deleteItem', function (event) {
        var id = $(this).attr('id');
        $.ajax({
            url: '/Menu/Delete',
            contentType: "application/json; charset=utf-8",
            data: { id },
            datatype: "text",
            success: function (data) {
                var MenuId = $('#selectMenus').val();
                Menus(MenuId);
                return false;
            },
            error: function (e) {
                alert(e);
            }
        });
        return false;
    });

    function Menus(MenuId) {

        $('.SubMenus').html('');
        var cssst = "";

        if (MenuId != 0) {
            $.ajax({
                url: '/Menu/GetSubMenus',
                contentType: "application/json; charset=utf-8",
                data: { MenuId },
                datatype: "json",
                success: function (msg) {
                    if (msg != null) {
                        var response = JSON.parse(msg);
                        if (response.length > 0) {
                            var Slno = 1;
                            for (var i = 0; i < response.length; i++) {

                                if (i == 0) {
                                    cssst += '<div class="" style="margin-top: 35px;"><div class="x_content"><table id="datatable-fixed-header" class="table table-striped table-bordered dt-responsive nowrap" cellspacing="0" width:"100%" >'
                                        + '<thead> <tr>'
                                        + '<th>Slno</th>'
                                        + '<th> Sub Menu Name</th> <th>Sub Menu URL</th>'
                                        + '<th>Action</th>'
                                        + '</tr></thead>'
                                        + '<tbody>';
                                }

                                cssst += '<tr><td>' + Slno + '</td>'
                                    + '<td>' + response[i].SubMenuName + '</td>'
                                    + '<td>' + response[i].SubMenuURL + '</td>'
                                    + '<td><ul class="actionbtnn" style="padding-left: 50px;">'
                                    + '<li class="actionbtn"><a href="' + response[i].EditUrl + '/' + response[i].Id + '" class="btn btn-round btn-info"><i class="fa fa-pencil fa-lg" style="line-height: 24px;"></i></a></li >'
                                    + '<li class="actionbtn"><a href="" id=' + response[i].Id + ' class="btn btn-round btn-danger deleteItem"><i class="fa fa-trash fa-lg"></i></button></li>'
                                    + '</ul></td></tr>';

                                if (i == response.length) {
                                    cssst += '</tbody></table></div></div> ';
                                }
                                Slno++;
                            }
                            $('.SubMenus').html(cssst);
                        }
                        else {
                            $('.SubMenus').html('');
                        }
                    }
                },
                error: function (e) {
                    alert(e);
                }
            });
        }
    }
});