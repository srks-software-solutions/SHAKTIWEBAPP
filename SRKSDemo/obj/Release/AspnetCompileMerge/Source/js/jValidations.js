//jvalidation for entering ONLY ALPHABETS & Space and Backspace
$('.j_text').keypress(function (e) {
    var regex = new RegExp("^[a-zA-Z\b ]$");
    var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
    if (!regex.test(str)) {
        e.preventDefault();
        alert("Alphabets Only")
        //$(event.target).next("span").html("Alphabets Only. Maximum:20 ").show().fadeOut(2000);
        return false;
    }
    return true;
});

//jvalidation for entering ONLY Alph Numeric & Space and Backspace
$('.j_AlphNum').keypress(function (e) {
    var regex = new RegExp("^[a-zA-Z0-9\b \r\n]$");
    var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
    if (!regex.test(str)) {
        e.preventDefault();
        alert("Alphabet & Numbers Only")
        //$(event.target).next("span").html("Alphabets Only. Maximum:20 ").show().fadeOut(2000);
        return false;
    }
    return true;
});

//jAllow all alphabets and few special characters
$('.j_all').keyup(function (e) {
    //!”$%&’()*\+,\/;\[\\\]\^_`{|}~
    var regex = new RegExp("^[a-zA-Z]$!”$%&’()*\+,\/;\[\\\]\^_`{|}~\b");
    var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
    // alert(str)
    if (!regex.test(str)) {
        e.preventDefault();
        alert("All Characters.")
        //$(event.target).next("span").html("All Characters.").show().fadeOut(3000);
        return false;
    }
    return true;
});

////jAllow only Numbers   
$('.j_int').on("keydown", function (e) {
    // var regex = new RegExp("^[0-9] *$ "); 
    //// var regex = new RegExp("[^0-9]/g");
    // var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
    // if (!regex.test(str)) {
    //     e.preventDefault();
    //     alert("Numbers Only");
    //     $(event.target).next("span").html("Numbers Only.").show().fadeOut(40000);
    //     return false;
    // }
    // return true;

    //2017-01-17
    // Allow: backspace, delete, tab, escape, enter and .
    if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
        // Allow: Ctrl+A, Command+A
        (e.keyCode === 65 && (e.ctrlKey === true || e.metaKey === true)) ||
        // Allow: home, end, left, right, down, up
        (e.keyCode >= 35 && e.keyCode <= 40)) {
        // let it happen, don't do anything
        return;
    }
    // Ensure that it is a number and stop the keypress
    if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
        e.preventDefault();
        alert("Numbers Only");
        return false;
    }
})
;

//jAllow only Decimal   
$('.j_decimal').keyup(function (e) {
    //var regex = new RegExp("/[0-9]|\./");
    //var regex = new RegExp("^[0-9]*(\.[0-9]{1,4})?$"); ^-?(0|[1-9]\d*)(?<!-0)$
    //alert("blah");
    //var regex = new RegExp("^[0-9]{1,3}?\.[0-9]{1,2}?$");
    //var regex = new RegExp("/^[0-9]*(\.[0-9]+)?$/");
    //var regex = new RegExp("^-?\d+(,\d+)*(\.\d+(e\d+)?)?$");
    var regex = new RegExp("\d{0,2}(\.\d{1,2})?");
    var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
    //alert(str);
    if (!regex.test(str)) {
        alert(str.length);
        e.preventDefault();
        alert("Numbers Only");
        return false;
    }
    return true;
});


//jFor restricting any field to its maxlength 
// $(document).ready(function () {
$('.j_length').keyup(function (e) {
    var len = $(this).attr("maxlength");
    if (this.value.length > $(this).attr("maxlength")) {
        e.preventDefault();
        var len = $(this).attr("maxlength");
        alert(" Maximum Length allowed is: " + len)
        $(this).val("");
        return false;
    }
    return true;
});
//  });

//jMobile number to start with 7 or 8 or 9
$(".j_mobilenumber").keydown(function (e) {
    if ($(this).value != "") {
        var y = $(this).val();
        if (y.charAt(0) == "9" || y.charAt(0) == "8" || y.charAt(0) == "7")
            return true;
        else {
            $(this).val("");
            $(this).focus();
            alert("Mobile number should start with 7 or 8 or 9")
            return false;
        }
    }
});

//jEmail validation
$('.j_mailid').focusout(function (e) {
    var inputVal = $(this).val();
    // alert(inputVal)
    var emailReg = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    if (!emailReg.test(inputVal)) {
        alert("Please enter something like abc@xyz.com")
        $(this).focus();
        //    // $(event.target).next("span").html("abc@gmail.com").show().fadeOut(2000);
    }
});

//jIP validation //not yet tested
$('.j_ip').focusout(function (e) {
    var inputVal = $(this).val();
    // alert(inputVal)
    var ipReg = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
    if (!ipReg.test(inputVal)) {
        alert("Please enter something like 255.255.255.255")
        $(this).focus();
        //    // $(event.target).next("span").html("abc@gmail.com").show().fadeOut(2000);
    }
});


//to validate time
// Compare to get Greatest of Integers.
//this method returns,
//1 if int1 is Greater
//0 if equal
//-1 if int2 is Greater
function greatestOfInts(int1, int2) {
    try {
        var variable1 = parseInt(int1);
        var variable2 = parseInt(int2);
        return variable1 > variable2 ? 1 : variable1 == variable2 ? 0 : -1;
    } catch (ex) {
        alert("Enter only Integers")
        return -2;
    }
}

//Assumption, TIME Format : HHxmm where x can be single Char and whatever symbol( : or . ). 
//Input: 2 time in HH:mm format 
//Output:  
// 1 if time1 is Greater
// 0 if equal
// -1 if time2 is Greater
function TimeCompareHHmm(time1, time2) {
    //fH => from Hour, tH => to Hour
    //1st check if they are valid times.
    // Are they Empty, Hours < 24 and Minutes < 60 
    if ($.trim(time1).length < 1 || $.trim(time2).length < 1) {
        return -2;
    }
    var fH = time1.substring(0, 2);
    var tH = time2.substring(0, 2);
    var fM = time1.substring(3, 5);
    var tM = time2.substring(3, 5);
    var HResult = greatestOfInts(fH, tH);
    //alert("Hours: " + HResult)
    if (HResult == -1) // Time2 is greater. Valid
    {
        return -1;
    }
    else if (HResult == 0) { // Both r equal so check for minutes
        var MResult = greatestOfInts(fM, tM);
        //alert("Minutes: " + MResult)
        if (MResult == 1) { // Minute1 is greater. Invalid
            return 11;
        }
        else if (MResult == -1) { // Minute2 is greater. Valid
            return -11;
        }
        else if (MResult == 0) { //Both are equal . Invalid
            return 10;
        }
    }
    else if (HResult == 1) {// Time1 is greater. Invalid
        return 1;
    }
}


