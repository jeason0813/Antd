$("#EnableCa").on("click", function () {
    jQuery.support.cors = true;
    var aj = $.ajax({
        url: "/services/ca/enable",
        type: "POST",
        success: function () {
            location.reload(true);
        }
    });
    _requests.push(aj);
});

$("#DisableCa").on("click", function () {
    jQuery.support.cors = true;
    var aj = $.ajax({
        url: "/services/ca/disable",
        type: "POST",
        success: function () {
            location.reload(true);
        }
    });
    _requests.push(aj);
});

$("#ApplyConfigCa").on("click", function () {
    jQuery.support.cors = true;
    var aj = $.ajax({
        url: "/services/ca/set",
        type: "POST",
        success: function () {
            location.reload(true);
        }
    });
    _requests.push(aj);
});