var ExpertsChoicePage = function () {
    this.init();
};

ExpertsChoicePage.prototype.init = function () {
    $('.expert-image').hover(function () {
        $(this).attr('src', $(this).attr('src').replace('.sep', '.orig'));
    },
    function () {
        $(this).attr('src', $(this).attr('src').replace('.orig', '.sep'));
    });
};