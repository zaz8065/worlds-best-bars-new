$('.carousel').carousel();

$("a[data-rel=popover]").each(function () {
    $(this).popover({ html: true })
        .click(function (e) {
            e.preventDefault()
        });
});

var initSearches = function (context) {

  $('[data-type="location-search"]', context).each(function () {
    var input = this;
    var target = document.getElementById($(this).attr('data-target'));
    var labels, mapped;

    $(this).typeahead({
      source: function (query, process) {

        $.get('/locations/search/json', { query: query }, function (data) {
          labels = [];
          mapped = {};

          $(data).each(function () {
            mapped[this.name] = this.id;
            labels.push(this.name);
          });

          process(labels);
        });
      },
      updater: function (item) {
        target.value = mapped[item];
        return item;
      },
      matcher: function (item) {
        if (item.toLowerCase().indexOf(this.query.trim().toLowerCase()) != -1) {
          return true;
        }
      },
      sorter: function (items) {
        return items.sort();
      },
      highlighter: function (item) {
        var regex = new RegExp('(' + this.query + ')', 'gi');
        return item.replace(regex, "<strong>$1</strong>");
      },
    });

    $(this).next('button').click(function () {
      input.value = '';
      target.value = '';
      input.focus();
    });

  });

  $('[data-type="bar-search"]', context).each(function () {
    var input = this;
    var target = document.getElementById($(this).attr('data-target'));
    var labels, mapped;

    $(this).typeahead({
      source: function (query, process) {

        $.get('/bars/search/json', { query: query }, function (data) {
          labels = [];
          mapped = {};

          $(data).each(function () {
            mapped[this.name] = this.id;
            labels.push(this.name);
          });

          process(labels);
        });
      },
      updater: function (item) {
        target.value = mapped[item];
        return item;
      },
      matcher: function (item) {
        if (item.toLowerCase().indexOf(this.query.trim().toLowerCase()) != -1) {
          return true;
        }
      },
      sorter: function (items) {
        return items.sort();
      },
      highlighter: function (item) {
        var regex = new RegExp('(' + this.query + ')', 'gi');
        return item.replace(regex, "<strong>$1</strong>");
      },
    });

    $(this).next('button').click(function (e) {
      e.preventDefault();

      input.value = '';
      target.value = '';
      input.focus();
    });

  });

  $('[data-type="list-search"]', context).each(function () {
    var input = this;
    var target = document.getElementById($(this).attr('data-target'));
    var labels, mapped;

    $(this).typeahead({
      source: function (query, process) {

        $.get('/lists/search/json', { query: query }, function (data) {
          labels = [];
          mapped = {};

          $(data).each(function () {
            mapped[this.name] = this.id;
            labels.push(this.name);
          });

          process(labels);
        });
      },
      updater: function (item) {
        target.value = mapped[item];
        return item;
      },
      matcher: function (item) {
        if (item.toLowerCase().indexOf(this.query.trim().toLowerCase()) != -1) {
          return true;
        }
      },
      sorter: function (items) {
        return items.sort();
      },
      highlighter: function (item) {
        var regex = new RegExp('(' + this.query + ')', 'gi');
        return item.replace(regex, "<strong>$1</strong>");
      },
    });

    $(this).next('button').click(function (e) {
      e.preventDefault();

      input.value = '';
      target.value = '';
      input.focus();
    });

  });
};

initSearches(document);

if ($('.update.bar').length) {

    (function () {
        var addOpenHoursTemplate = Handlebars.compile($('#add-open-hour-tpl').html());

        var updateOpenHours = function () {
            $('#opening-times .list > div').each(function (index) {
                $('[name]', this).each(function () {
                    var field = $(this).attr('name').split('.')[1];
                    $(this).attr('name', 'OpenHours[' + index + '].' + field);
                    $(this).attr('id', 'OpenHours_' + index + '__' + field);
                });
            });
        };

        $('#opening-times .add').click(function (e) {
            e.preventDefault();
            $('#opening-times .list').append(addOpenHoursTemplate());

            updateOpenHours();
        });

        $('#opening-times .list').on('click', '.remove', function (e) {
            e.preventDefault();

            $(this).parent().remove();

            updateOpenHours();
        });
    })();

    (function () {
        var addFeatureTemplate = Handlebars.compile($('#add-feature-tpl').html());

        var updateFeatures = function () {
            $('#features .list > div').each(function (index) {
                $('[name]', this).each(function () {
                    var field = $(this).attr('name').split('.')[1];
                    $(this).attr('name', 'Features[' + index + '].' + field);
                    $(this).attr('id', 'Features_' + index + '__' + field);
                });
            });
        };

        $('#features .add').click(function (e) {
            e.preventDefault();
            $('#features .list').append(addFeatureTemplate());

            updateFeatures();
        });

        $('#features .list').on('click', '.remove', function (e) {
            e.preventDefault();

            $(this).parent().remove();

            updateFeatures();
        });
    })();

    $('#media .image [type=checkbox]').click(function () {
        var hidden = $('[type=hidden]', $(this).parents('.image'));
        if (this.checked) {
            hidden.attr('name', 'x-Images');
        }
        else {
            hidden.attr('name', 'Images');
        }
    });
}

$(document).on('submit', 'form.ajax', function(e) {
  e.preventDefault();
  var form = this;

  $.post($(this).attr('action'), $(this).serialize(), function (data) {
    if (typeof data == 'string') {
      form = $(form).replaceWith(data);

      initSearches(form);
    }
    else {
      if (data.ok) {
        location.reload();
      }
    }
  });
});