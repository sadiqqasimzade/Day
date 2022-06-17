$(".category").click(function(){
    $(this).addClass("active");
    var attr = $(this).attr("data-target")
    $(".product").addClass("d-none");
    $(`.product[data-id='${attr}']`).removeClass("d-none")

})

$(".category").eq(0).click();