$(document).ready(function () {

    var mobile = false;
    var device = "";


    var customizeForDevice = function () {
        var ua = navigator.userAgent;
        var checker = {
            iphone: ua.match(/iPhone/),
            ipad: ua.match(/iPad/),
            blackberry: ua.match(/BlackBerry/),
            android: ua.match(/Android/)
        };
        if (checker.android) {
            device = "android";
            mobile = true;
        }
        else if (checker.iphone) {
            mobile = true;
            device = "iphone";
        }
        else if (checker.blackberry) {
            mobile = true;
            device = "blackberry";
        }
        else if (checker.ipad) {
            mobile = true;
            device = "ipad";
        }
        else {
            mobile = false;
            device = "probably a desktop";
        }
    }
    customizeForDevice();


    function add_ads() {

        var ads_array = new Array();
        ads_array[0] = '/content/gfx/ads/absolutvodka.png';
        ads_array[1] = '/content/gfx/ads/ballantines.png';
        ads_array[2] = '/content/gfx/ads/beefeater.png';
        ads_array[3] = '/content/gfx/ads/havana.png';
        ads_array[4] = '/content/gfx/ads/jameson.png';
        var ad_w = 609;
        var ad_h = 142;
        var randomnumber = Math.floor(Math.random() * 5);
        var ad_url = new Array();
        ad_url[0] = 'http://www.absolut.com/uk';
        ad_url[1] = 'http://www.ballantines.com/en';
        ad_url[2] = 'http://www.beefeatergin.com/';
        ad_url[3] = 'http://www.havana-club.co.uk/';
        ad_url[4] = 'http://www.jamesonwhiskey.com/';

        adObj = new Object();
        adObj = { imgpath: ads_array[randomnumber], width: ad_w, height: ad_h, url: ad_url[randomnumber] };
        createElement(adObj);

    }

    function createElement(obj) {
        var code = "<div class='mobile_ad'><a href='" + adObj.url + "' target='_blank'><img src='" + adObj.imgpath + "' width='" + adObj.width + "' height='" + adObj.height + "'/></a></div>";

        $('div#content').append(code);

    }


    if (mobile == true) {

        $('body').addClass('mobile');
        $('#search form').attr('action', '/search/mobile');

        // check where we are
        // 1=home
        // 2=experts choice
        // 8=experts choice subpage
        // 3=readers top 100
        // 4=talked about
        // 5=cocktails (hide)
        // 6=search
        // 7=other
        // 9=bar detail page
        // 10=cities page
        // 11=countries page

        // stuff to hide

        if (mobile == true && device != "ipad") {
            $('#search').hide();
            $('#top-nav').hide();
            $('.tertiary-content').hide();
            $('div#signup').addClass('changemargin');
            $('div.popup').addClass('changemargin');

            var pagelocation = 7;
            var checkification = $('#panel-1 h2').text();

            if (checkification == "Top Cities") {
                pagelocation = 1;
                $('#search').show();
                $('#top-nav').show();
            }


            if ($('div.header_box h2').text() == "Expert's Choice") {
                pagelocation = 2;
            }


            if (checkification == "Readers' Top 100") {
                pagelocation = 3;
            }

            if (checkification == "Most talked about") {
                pagelocation = 4;
            }

            if ($("#search_results").length > 0) {
                pagelocation = 6;
            }

            if ($("#experts_choice #panel-1 #bar-image").length > 0) {
                pagelocation = 8;
            }

            if ($("#panel-1 #bar-image div.bar-image-scroller").length > 0) {
                pagelocation = 9;
            }

            if ($("#cities_page").length > 0) {
                pagelocation = 10;
            }

            if ($("div.map_box").length > 0) {
                pagelocation = 11;
            }


            // reformatting

            // home page 
            if (pagelocation == 1) {
                $('.fifty-fifty').addClass('darken');
                $('#ubersearch').val('Find a bar');

                $('#panel-1').hide();
                $('#panel-2').hide();
                $('#panel-3').hide();
                $('#panel-4').hide();
                $('div#signup').removeClass('changemargin');
                $('div.popup').removeClass('changemargin');





                $searchbox.focus(function () {
                    var $searchboxDefault = 'Find a bar';
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


                add_ads();
            }

            //experts choice
            if (pagelocation == 2) {

                $('#experts_choice .header_box').addClass("hb_ow");
                $('#experts_choice .header_box').css("background-color", "black");
                $('#experts_choice div.main_box').css("background-color", "black");
                $('#experts_choice div.main_box').css("width", "640px");
                $('#experts_choice div.main_box').css("margin-top", "30px");
                $('#experts_choice div.main_box').css("background-image", "none");
                $('#experts_choice').removeClass("wb");
                $('#experts_choice div.expert').addClass("exp_ow");
                $('#experts_choice h2').addClass("hb_ow_h2");
                add_ads();
            }


            // search page 
            if (pagelocation == 6) {
                $('#header').append('<div class="new_title"><h2>Your Search Results ...</h2></div>');
                $('#results_filter').hide();
                $('#search_results .secondary-content').css("width", "620px");
                //$('div.results').not('div.results.bar').hide();
                $('div.results').not('div.results.bar, div.results.location').attr("data-filter-hidden", "false");
                //$('div.results.location').attr("data-filter-hidden", "false");
                add_ads();
            }

            //experts choice subpage
            if (pagelocation == 8) {

                $('#experts_choice').removeClass("wb");
                $('#experts_choice .full_width').addClass("hb_ow");
                $('#experts_choice div.panel').addClass("exp_ow");
                $('#experts_choice h2').addClass("hb_ow_h2");
                $('#breadcrumb-zone').addClass("exp_det");

                $('#experts_choice .full_width').css("background-color", "black");
                $('#experts_choice div#bar_reviews_list').css("background-color", "black");
                $('#experts_choice div#bar_reviews_list').css("width", "640px");
                $('#experts_choice div#bar_reviews_list').css("margin-top", "30px");
                $('#experts_choice div#bar_reviews_list').css("background-image", "none");

                add_ads();
            }


            // bar detail page

            if (pagelocation == 9) {

                $('#panel-1 .full_width').addClass("hb_ow");

                $('#breadcrumb-zone').hide();
                $('div#panel-2').hide();
                $('div#bar-header').hide();

                // $('div#bar-image img').width('600px');
                //$('div#bar-image img').height('443px');

                if (device == "iphone") {
                    var the_location = $('div#bar-panel h4').html();
                    var new_url = 'maps://maps.google.com/maps?q=' + the_location;

                    $('#show_on_map').unbind('click');
                    $('#show_on_map').attr('href', new_url);

                    var pathname = window.location.pathname;

                    var fb_url = 'fb://publish'; ///?url=#'+pathname+'#';
                    $('a.fb').unbind('click');
                    $('a.fb').attr('href', fb_url);
                }

                add_ads();
            }


            //country page
            if (pagelocation == 10) {
                $('#breadcrumb-zone').addClass("loc_det");
                $('div#panel-2').hide();
                $('#panel-1 .full_width').addClass("hb_ow");
                $('div#bar-header').hide();
                $('div.map_box h3').hide();
                $('#panel-4 .secondary-content .panel').addClass("hb_ow");
                addswitchBtn();
            }

            //city page
            if (pagelocation == 11) {
                $('#breadcrumb-zone').addClass("loc_det");
                $('div#panel-2').hide();
                $('#panel-1 .full_width').addClass("hb_ow");
                $('div#bar-header').hide();
                $('div.map_box h3').hide();
                $('#panel-4 .secondary-content .panel').addClass("hb_ow");
                addswitchBtn();
            }

            //readers top 100 page
            if (pagelocation == 3) {
                $('#panel-1 .full_width').addClass("hb_ow");
                $('#panel-4 .secondary-content .result').addClass("hb_ow");
                add_ads();
            }

            //most talked about page
            if (pagelocation == 4) {
                $('#panel-1 .full_width').addClass("hb_ow");
                $('#panel-4 .secondary-content .result').addClass("hb_ow");

                add_ads();
            }



        } // end of checking for mobile/device




        function addswitchBtn() {

            // html code
            var newcode = '';

            newcode = "<div id='location_switch'><a href='#' id='map_btn'>Map</a><a href='#' id='list_btn'>List</a></div>";
            // add to page
            $('#cities_page').prepend(newcode);
            // add event monitor

            $('#map_btn').click(function () {
                $('#cities_page #panel-1').show();
                $('#bar_list_pagination').hide();
            });

            $('#list_btn').click(function () {
                $('#cities_page #panel-1').hide();
                $('#cities_page #panel-4').show();
                $('#bar_list_pagination').show();

                refreshPage();
            });


        }



        function refreshPage() {

            $('#bar_list_pagination span span a').removeClass('active');
            $('#bar_list_pagination span span a:first').addClass('active');
            $('#bar_list_container div.panel').attr('data-filter-hidden', 'false');

            $('div.panel').each(function (index, domEle) {
                if ($(domEle).attr('data-page') == '1') {
                    $(domEle).show();
                }
            });
        }



        // add nav if on landing page..
        // add mobile specific nav + view on desktop view

        // geolocation stuff + alter search
        var geocoder;

        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(successFunction, errorFunction);
        }
        function successFunction(position) {
            var lat = position.coords.latitude;
            var lng = position.coords.longitude;
            var new_loc = '<li id="tab-5"><a href="/search/mobile?location=' + lat + ',' + lng + '">Bars near me</a></li>';
            $('div#top-nav ul').prepend($(new_loc));
        }

        function errorFunction() {
            alert("Cannot find your GPS position. Do you have location services switched on?");
        }

    }

});