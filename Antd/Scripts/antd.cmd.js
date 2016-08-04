$(document).ready(function () {
    $('[data="CmdGet"]').each(function () {
        var self = $(this);
        var command = self.attr("data-name");
        var values = "";
        var par = self.attr("data-par");
        if (par != undefined) {
            var res = par.split(",");
            var container = self.parents('[data-role="CmdContainer"]');
            $.each(res, function (i, v) {
                var app = v + ":";
                var val = container.find('[data="' + v + '"]').val();
                app += val;
                app += ";";
                if (val != null && val.length > 1) {
                    values += app;
                }
            });
        }
        jQuery.support.cors = true;
        $.ajax({
            url: "/cmd/launch",
            type: "POST",
            data: {
                Command: command,
                Matches: values
            },
            success: function (data) {
                self.html(data.replace(/\n/g, "<br/>"));
            }
        });
    });
});

$('[data-role="CmdLaunch"]').click(function () {
    var container = $(this).parents('[data-role="CmdContainer"]');
    var button = container.find('[data-role="CmdLaunch"]');
    var command = button.attr("data-name");
    var values = "";
    var par = button.attr("data-par");
    var res = par.split(",");
    $.each(res, function (i, v) {
        var app = v + ":";
        var val = container.find('[data="' + v + '"]').val();
        app += val;
        app += ";";
        if (val != null && val.length > 1) {
            values += app;
        }
    });
    jQuery.support.cors = true;
    $.ajax({
        url: "/cmd/launch",
        type: "POST",
        data: {
            Command: command,
            Matches: values
        },
        success: function (data) {
            container.find('[data-role="CmdResult"]').html(data.replace(/\n/g, "<br/>"));
        }
    });
});

$(document).ready(function () {
    $('[data-role="ContextSelection"]').each(function () {
        $(this).windowed({
            change: function (event, selected) {
                selected = $(selected);
                var val = selected.val();
                var container = selected.parents('table.context');
                container.find('[data-select]').hide();
                container.find('[data-select="' + val + '"]').show();
            }
        });
    });
});

$(document).ready(function () {
    $('textarea').each(function () {
        this.style.height = (this.scrollHeight + 10) + 'px';
    });
});