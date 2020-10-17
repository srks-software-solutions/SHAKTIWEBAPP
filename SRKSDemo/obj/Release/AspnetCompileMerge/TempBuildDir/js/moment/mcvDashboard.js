$(document).ready(function () {
   
    //GetMachineDetailsAll();
    getmachines();
    setInterval(getmachines, 1000 * 60*2);

    // setTimeout(GetMachineDetalsAll, 1000 * 60);

    //function GetMachineDetailsAll() {
    //    $('#loading').css('display', 'block');
    //    $.get("/Dashboard/GetAllMachineDetails", {}, function (msg) {

    //        if (msg != '') {
    //            var data = JSON.parse(msg);
    //            MachinesList = data;
    //            $('#loading').css('display', 'none');
    //        }
    //    });
    //}


    function swiping() {
        var swiper = new Swiper('.blog-slider', {
            spaceBetween: 30,
            effect: 'fade',
            loop: true,
            mousewheel: {
                invert: false,
            },
            autoplay: {
                delay: 20000,
            },
            autoHeight: true,
            pagination: {
                el: '.blog-slider__pagination',
                clickable: true,
            }
        });
    }

    function getmachines() {
        $.get("/Dashboard/MachineDashboard", {}, function (msg) {
            if (msg != '') {
                $('.MachinesHeader').html('');
                var data = JSON.parse(msg);
                //MachinesList = data[4].machineModel;
                var cssdat = '';

                for (var i = 0; i < data.length; i++) {
                    //if (data[4].machineModel != null)
                    MachinesList = data[i].machineModel;
                    cssdat += '<div class="card-header"><div class="col-sm-12 text-center" style="height:35px;background-color:#3F51B5;"><div class="col-sm-3"></div>';
                    // cssdat += '<div class="col-sm-1"><h4 class="dash-h4 text-uppercase font-weight-bold">Plant: </h4></div> <div class="col-sm-2"> <h4 style="color:#fff;">' + data[i].plantName + '</h4></div>';
                    //cssdat += ' <div class="col-sm-1"><h4 class="dash-h4 text-uppercase font-weight-bold">Shop: </h4></div> <div class="col-sm-2"><h4 style="color:white;">' + data[i].shopName + '</h4></div>';
                    cssdat += ' <div class="col-sm-6"><h4 style="color:#fff;">' + data[i].cellName + '</h4></div></div></div>';

                    var machines = data[i].machines;
                    cssdat += '<div class="card-body">';
                    for (var j = 0; j < machines.length; j++) {
                        var color = '';
                        var tagetval = '#';
                        var cls = 'dash-h5';
                        if (machines[j].Color == 'GREEN') {
                            color = 'dash-green';
                            tagetval = '#running';
                        }
                        else if (machines[j].Color == 'YELLOW') {
                            color = 'dash-amber';
                            tagetval = '#idle';
                            cls = 'dash-h5-Idle';
                        }
                        else if (machines[j].Color == 'BLUE') {
                            color = 'dash-blue';
                            tagetval = '#poweroff';
                        }
                        else if (machines[j].Color == 'RED') {
                            color = 'dash-red';
                            tagetval = '#breakdown';
                        }
                        cssdat += '<div class="col-sm-3 dash-margin"><div class="col-sm-2"></div><div class="col-sm-10 dash-row-padding"><div class="' + color + ' dash-border macid">';
                        cssdat += '<a class="machineDetails"  id="' + machines[j].CurrentStatus + ' ' + machines[j].MachineID + ' "  style="cursor: pointer;" data-toggle="modal" data-target="' + tagetval + '">';
                        cssdat += '<img src="/images/CNC_MACHINE.png" class="img-responsive dash-img"><h5 class="' + cls + '">' + machines[j].MachineName + '</h5><h5 class="' + cls + '">Time: ' + machines[j].Time + '</h5></a></div></div></div>';

                    }
                    cssdat += '</div>';

                }
                $(cssdat).appendTo($('.MachinesHeader'));
                $('#loading').css('display', 'none');

            }

        })
    }

    function xyz() {
       
       
    }

    $(document).on('click', '.machineDetails', function (e) {
        var MachinesList;
        $.ajax({
            url: "/Dashboard/GetAllMachineDetails",
            async: false,
            type: "GET",
            dataType: "text",
            success: function (data) {
                MachinesList = JSON.parse(data);
            }
        });

        var anchor = document.querySelector("a");
        var cssdata1 = '';
        var dat = $(this).text();
        var mid = this.id;
        var ret = mid.split(" ");
        var status = ret[0];
        var id = ret[1];
        $('#loading').css('display', 'block');
        if (MachinesList != null && MachinesList != '') {
            $('.MACDETNW1').html('');
            for (var i = 0; i < MachinesList.length; i++) {
                var cls = 'dash-desc';
                if (MachinesList[i].Color == 'YELLOW')
                    cls = 'dash-desc-Idle';
                if (MachinesList[i].MachineID == id) {

                    cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-5 ' + cls + '" style="padding-left: 2px;display: contents;font-size: 20px;">' + MachinesList[i].MachineName + '</span></div>';

                    cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 ' + cls + ' text-right" style="padding:0">PowerOn Time :</span><span class="col-sm-5 ' + cls + '" style="padding-left: 2px;">' + MachinesList[i].PowerOnTime + '</span></div>';

                    cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 ' + cls + ' text-right" style="padding:0">Running Time:</span><span class="col-sm-5 ' + cls + '" style="padding-left: 2px;">' + MachinesList[i].RunningTime + '</span></div>';

                    cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 ' + cls + ' text-right" style="padding:0">Cutting Time:</span><span class="col-sm-5 ' + cls + '" style="padding-left: 2px;">' + MachinesList[i].CuttingTime + '</span></div>';

                    cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 ' + cls + ' text-right" style="padding:0">Total Parts Count:</span><span class="col-sm-5 ' + cls + '" style="padding-left: 2px;">' + MachinesList[i].PartsCount + '</span></div>';

                    cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 ' + cls + ' text-right" style="padding:0">Idle Duration:</span><span class="col-sm-5 ' + cls + '" style="padding-left: 2px;">' + MachinesList[i].IdleTime + '</span></div>';

                    cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 ' + cls + ' text-right" style="padding:0">Exe Prog Name:</span><span class="col-sm-5 ' + cls + '" style="padding-left: 2px;">' + MachinesList[i].ExeProgramName + '</span></div>';

                    cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 ' + cls + ' text-right" style="padding:0">Cycle Time:</span><span class="col-sm-5 ' + cls + '" style="padding-left: 2px;">' + MachinesList[i].CycleTime + '</span></div>';
                    cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 ' + cls + ' text-right" style="padding:0">Cutting Ratio:</span><span class="col-sm-5 ' + cls + '" style="padding-left: 2px;">' + MachinesList[i].CuttingRatio + '</span></div>';

                }
            }
            $(cssdata1).appendTo('.MACDETNW1');
            $('#loading').css('display', 'none');
            $(window).scrollTop(0);
        }
        //$.get("/Dashboard/MachineConnectivity", { MID: id, Status: status }, function (msg) {
        //    $('.MACDETNW1').empty();
        //    var data = JSON.parse(msg);
        //    if (id == data[0].MachineID) {

        //        cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-5 dash-desc" style="padding-left: 2px;display: contents;font-size: 20px;">' + data[0].MachineName + '</span></div>';

        //        cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 dash-desc text-right" style="padding:0">PowerOn Time :</span><span class="col-sm-5 dash-desc" style="padding-left: 2px;">' + data[0].PowerOnTime + '</span></div>';

        //        cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 dash-desc text-right" style="padding:0">Running Time:</span><span class="col-sm-5 dash-desc" style="padding-left: 2px;">' + data[0].RunningTime + '</span></div>';

        //        cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 dash-desc text-right" style="padding:0">Cutting Time:</span><span class="col-sm-5 dash-desc" style="padding-left: 2px;">' + data[0].CuttingTime + '</span></div>';

        //        cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 dash-desc text-right" style="padding:0">Total Parts Count:</span><span class="col-sm-5 dash-desc" style="padding-left: 2px;">' + data[0].PartsCount + '</span></div>';

        //        cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 dash-desc text-right" style="padding:0">Idle Duration:</span><span class="col-sm-5 dash-desc" style="padding-left: 2px;">' + data[0].IdleTime + '</span></div>';

        //        cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 dash-desc text-right" style="padding:0">Exe Prog Name:</span><span class="col-sm-5 dash-desc" style="padding-left: 2px;">' + data[0].ExeProgramName + '</span></div>';

        //        cssdata1 += '<div class="row dash-dasc-pad1"><span class="col-sm-7 dash-desc text-right" style="padding:0">Cycle Time:</span><span class="col-sm-5 dash-desc" style="padding-left: 2px;">' + data[0].CycleTime + '</span></div>';

        //        $(cssdata1).appendTo('.MACDETNW1');
        //        windowResize();
        //    }

        //});

    });

});


//function windowResize() {
//    $(window).scrollTop(0);
//    $('body').css('overflow', 'hidden');
//    $(window).resize(function () {

//        var alerttop = $(window).height() / 2 - $('.MACDETNW1').height() / 2;
//        $('.MACDETNW1').css('padding-top', alerttop);

//        var alertheight = $('body').height();
//        $('.MACDETNW1').css('height', alertheight);

//    }).trigger('resize');
//}

