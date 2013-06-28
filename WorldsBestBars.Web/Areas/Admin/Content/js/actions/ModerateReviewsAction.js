var ModerateReviewsAction = function () {
    var that = this;
    this.dialog = false;
    this.data = false;
};

ModerateReviewsAction.prototype.init = function () {
    var that = this;

    var initDialog = function () {
        $.ajax({
            url: site_base + 'admin/dialogs/moderatereviews'
        }).success(function (data) {
            that.dialog = $(data);

            that.dialog.dialog({
                title: 'Moderate reviews',
                autoOpen: false,
                width: 500,
                height: 600,
                resizable: false,
                show: 'fade',
                hide: 'fade',
                create: function (event, ui) {
                    var rand = parseInt(Math.random() * 1000);

                    $('ul li.tab-head', that.dialog).each(function (index, el) {
                        var target = $('a', el).attr('href').substr(1);

                        $('.' + target, that.dialog).attr('id', rand + target);
                        $('a', el).attr('href', '#' + rand + target);
                    });

                    $('.tabs', that.dialog).tabs();

                    /*$('.moderate-reviews-awaiting .reviews', that.dialog).jScrollPane({ autoReinitialise: true });
                    $('.moderate-reviews-recent .reviews', that.dialog).jScrollPane({ autoReinitialise: true });*/

                    that.reloadData();
                }
            });
        });
    };

    initDialog();
};

ModerateReviewsAction.prototype.execute = function () {
    $(this.dialog).dialog('open');
};

ModerateReviewsAction.prototype.reloadData = function (selected) {
    var that = this;

    $.ajax({
        url: site_base + 'admin/json/getmoderationdata'
    }).success(function (data) {
        that.data = data;

        var generateDom = function (review) {
            var dom = $('<div class="review" data-id="' + review.id + '"></div>');

            dom.append('<div class="actions"><button class="approve">Approve</button><button class="reject">Reject</button></div>');
            dom.append('<span class="bar" data-id="' + review.bar.id + '"><a href=' + review.url + ' target="_blank">' + review.bar.name + '</a></span>');

            if (review.user == null) {
                dom.append('<span class="user">user - Anonymous<span class="user-ip">' + review.ip + '</span></span>');
            }
            else {
                dom.append('<span class="user" data-id="' + review.user.id + '">user - ' + review.user.name + '<span class="user-ip">' + review.ip + '</span></span>');
            }

            dom.append('<span class="title">' + review.title + '</span><span class="comment">' + review.comment + '</span>');

            dom.append('<div class="rating"><strong>Rating</strong> design: <input class="design" value="' + (review.rating ? (review.rating.design / 255.0 * 5.0).toFixed(1) : '') + '" /> drinks: <input class="drinks" value="' + (review.rating ? (review.rating.drinks / 255.0 * 5.0).toFixed(1) : '') + '" /> service: <input class="service" value="' + (review.rating ? (review.rating.service / 255.0 * 5.0).toFixed(1) : '') + '" /><button class="update">Update</button></div>');

            dom.append('<span class="created">' + new Date(review.created).toString() + '</span>');

            dom.append('<hr />');

            return dom;
        };

        $('.moderate-reviews-awaiting .reviews', that.dialog).empty().append('<div></div>');
        $(data.awaiting).each(function (index, review) {
            var dom = generateDom(review);
            $('.moderate-reviews-awaiting .reviews div:first', that.dialog).append(dom);

            $('button.approve', dom).button().click(function (e) { e.preventDefault(); that.approveReview($(this).parent().parent().attr('data-id')); });
            $('button.reject', dom).button().click(function (e) { e.preventDefault(); that.rejectReview($(this).parent().parent().attr('data-id')); });
            $('button.update', dom).button().click(function (e) {
                e.preventDefault();
                that.updateReview($(this).parent().parent().attr('data-id'), $('.design', $(this).parent()).val(), $('.drinks', $(this).parent()).val(), $('.service', $(this).parent()).val());
            });
        });

        $('.moderate-reviews-recent .reviews', that.dialog).empty().append('<div></div>');
        $(data.recent).each(function (index, review) {
            var dom = generateDom(review);

            $('.moderate-reviews-recent .reviews div:first', that.dialog).append(dom);

            $('button.approve', dom).button({ disabled: review.active }).click(function (e) { e.preventDefault(); that.approveReview($(this).parent().parent().attr('data-id')); });
            $('button.reject', dom).button({ disabled: !review.active }).click(function (e) { e.preventDefault(); that.rejectReview($(this).parent().parent().attr('data-id')); });
            $('button.update', dom).button().click(function (e) {
                e.preventDefault();
                that.updateReview($(this).parent().parent().attr('data-id'), $('.design', $(this).parent()).val(), $('.drinks', $(this).parent()).val(), $('.service', $(this).parent()).val());
            });
        });
    });
};

ModerateReviewsAction.prototype.approveReview = function (id) {
    var that = this;

    $.ajax({
        url: site_base + 'admin/process/moderateuserreview/' + id,
        data: { approve: true },
        success: function (data) {
            if (data == 'false') {
                Helpers.alertDialog('Something went wrong.');
            }
            else {
                var dom = $('.review[data-id="' + id + '"]');

                dom.detach().prependTo($('.moderate-reviews-recent .reviews div:first', that.dialog));

                $('button.approve', dom).button('option', 'disabled', true);
                $('button.reject', dom).button('option', 'disabled', false);
            }
        },
        error: function (data) {
            Helpers.alertDialog('Something went wrong.');
        }
    });
};

ModerateReviewsAction.prototype.rejectReview = function (id) {
    var that = this;

    $.ajax({
        url: site_base + 'admin/process/moderateuserreview/' + id,
        data: { approve: false },
        success: function (data) {
            if (data == 'false') {
                Helpers.alertDialog('Something went wrong.');
            }
            else {
                var dom = $('.review[data-id="' + id + '"]');

                dom.detach().prependTo($('.moderate-reviews-recent .reviews div:first', that.dialog));

                $('button.approve', dom).button('option', 'disabled', false);
                $('button.reject', dom).button('option', 'disabled', true);
            }
        },
        error: function (data) {
            Helpers.alertDialog('Something went wrong.');
        }
    });
};

ModerateReviewsAction.prototype.updateReview = function (id, design, drinks, service) {
    var that = this;

    $.ajax({
        url: site_base + 'admin/process/updateuserreview/' + id,
        data: { design: design, drinks: drinks, service: service },
        success: function (data) {
            if (data == 'false') {
                Helpers.alertDialog('Something went wrong.');
            }
            else {
            }
        },
        error: function (data) {
            Helpers.alertDialog('Something went wrong.');
        }
    });
};