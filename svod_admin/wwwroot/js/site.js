// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    $("#search_inp").on("keyup", function () {
        var value = $(this).val().toLowerCase();
        $("#tuser-tbl tbody tr").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
        $("#form_list option").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });
    $.each($('.navbar-nav').find('li'), function () {
        $(this).toggleClass('active', window.location.pathname.indexOf($(this).find('a').attr('href')) > -1);
    });
});