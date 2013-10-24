


var $searchbox = $('input#ubersearch');
var $header = $('div#header');
var $top_nav = $('div#top-nav');
var $search = $('div#search');
var $sidebar = $('div#sidebar');
var $content = $('div#content');
var $footer = $('div#footer');
var $fb_btn = $('div.bar-sharing a.fb');
var $twit_btn = $('div.bar-sharing a.twit');
var $user_review = $('#write-reviews');
var $read_reviews = $('#read-reviews');
var $panel1 = $('#panel-1');
var $panel2 = $('#panel-2');
var $panel3 = $('#panel-3');
var $panel4 = $('#panel-4');
var $hr = $('.hr10');
var $breadcrumb_zone = $('#breadcrumb-zone');
var $experts_choice = $('#experts_choice');
var $cities_page = $('#cities_page');
var $articles = $('#articles');
var $generic = $('#generic');


//$('#content').addClass('hide');

function fadeallout() {
    $('#overlay').show();

}

function fadeallin() {
    $('#overlay').hide();
}

// pop ups
$('div.popup a.close').click(function (e) {
    $('div.popup').fadeOut();
    fadeallin();
    e.preventDefault();
});

$('#overlay').click(function (e) {
    $('div.popup').fadeOut();
    fadeallin();
    closeall();
    e.preventDefault();
});

function closeall() {

    if ($('#bar-info_overlay').length > 0) {
        $('#bar-info_overlay').hide();
    }

    if ($('.review_popup').length > 0) {
        $('.review_popup').hide();
    }
}
var $searchboxDefault = 'Type search terms here';


$searchbox.focus(function () {
    if ($(this).val() == $searchboxDefault) {
        $(this).val('');
    }
    $searchbox.css("color", "black");
}).blur(function () {
    if ($(this).val() == '') {
        $(this).val($searchboxDefault);
    }
    $searchbox.css("color", "#333");
});
$('#search form').submit(function (e) {
    if ($searchbox.val() == $searchboxDefault) {
        e.preventDefault();
    }
});

/* slider inits */
if ($.fn.scrollable) {
    $(".scrollable").scrollable({ circular: true, mousewheel: false }).navigator();
    $(".scrollable2").scrollable({ circular: true, mousewheel: false }).navigator().autoscroll(4000);
    var barImageScroller = $(".bar-image-scroller");
    var autoScroll = $(".bar-image-scroller .items div").length > 1;
    barImageScroller.scrollable({ circular: true, mousewheel: false }).navigator();
    if (autoScroll) {
        barImageScroller.autoscroll(4000);
    }
}
/* video expander */
$(".flashExpander").each(function () {
    var configPath = $("img:eq(0)", this).attr("longdesc");
    var flashId = $("img:eq(0)", this).attr("id");

    var flashvars = { config: configPath };
    var params = { allowfullscreen: "false", wmode: "opaque" };

    var bigId = flashId + '-big';
    var bigFlashId = flashId + '-flash';

    $(this).click(function (e) {
        e.preventDefault();

        $(this).append('<div class="flashExpanderBig" id="' + bigId + '"><div id="' + bigFlashId + '"><iframe width="640" height="360" src="http://www.youtube.com/embed/22HzxayTHSg" frameborder="0" allowfullscreen></iframe></div></div>');

        $('div#overlay').show();
        $('.flashExpanderBig').css("opacity", "1");

        $("#" + bigId).fadeIn("fast").mouseleave(function () { $(this).remove(); $('div#overlay').hide(); });
    });
});

$('#signin_form').submit(function (e) {
    e.preventDefault();

    $.ajax({
        url: $(this).attr('action'),
        type: 'post',
        data: {
            email: $('input[name=email]', this).val(),
            password: $('input[name=password]', this).val()
        },
        success: function (data) {
            if (data.success) {
                window.location.reload();
            }
            else {
                alert('We couldn\'t log you in because ' + data.reason);
            }
        },
        error: function () {
            alert('Something went wrong trying to log you in.');
        }
    });
});

$('#signup_form').submit(function (e) {
    e.preventDefault();

    $.ajax({
        url: $(this).attr('action'),
        type: 'post',
        data: {
            name: $('input[name=name]', this).val(),
            city: $('input[name=city]', this).val(),
            email: $('input[name=email]', this).val(),
            dobDay: $('select[name=dobDay]', this).val(),
            dobMonth: $('select[name=dobMonth]', this).val(),
            dobYear: $('select[name=dobYear]', this).val(),
            password: $('input[name=password]', this).val(),
            passwordConfirm: $('input[name=passwordConfirm]', this).val()
        },
        success: function (data) {
            if (data.success) {
                window.location.reload();
            }
            else {
                alert('We couldn\'t register you because ' + data.reason);
            }
        },
        error: function () {
            alert('Something went wrong trying to register your account.');
        }
    });
});

$('#logout').click(function (e) {
    e.preventDefault();

    $.ajax({
        url: $(this).attr('href'),
        type: 'post',
        success: function (data) {
            if (data.success) {
                window.location.reload();
            }
            else {
                alert('We couldn\'t log you out because ' + data.reason);
            }
        },
        error: function () {
            alert('Something went wrong trying to log you out.');
        }
    });
});

$('a#login').click(function (e) {
    e.preventDefault();
    closepopup();
    fadeallout();
    $('div.login_popup').show();
});

function closepopup() {
    $('div.popup').hide();
    fadeallin();
}

function closesignuppopup() {
    $('div.login_popup').hide();
}


//review reply

$('a.reply_btn').click(function (e) {
    $('div.reply-area').hide();

    $(this).next().toggle();
    e.preventDefault();
});

/*if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(function (position) {
        $('input[name=location]').val(position.coords.latitude + ',' + position.coords.longitude);
    },
    function () {
        // error
    });
}*/
$('#container').show();
