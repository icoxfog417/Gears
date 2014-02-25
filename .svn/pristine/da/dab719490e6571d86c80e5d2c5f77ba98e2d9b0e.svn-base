/*
  gears-script
  Gearsフレームワークで使用するJavaScript関数をまとめたもの。
  JQuery前提
*/


//名称空間宣言
var gears;
if(!gears) gears = {};
if(!gears.fn) gears.fn = {};

(function(){
/* オブジェクト宣言 */
var GS_LIBRARY = gears.fn;

/* 関数/プロパティ宣言部 */
GS_LIBRARY.LoadingImage = "./css/ajax-loader.gif";

//汎用要素削除関数
GS_LIBRARY.removeElementById = function (id) {
  var el = document.getElementById(id);
  el.parentNode.removeChild(el);
}

//警告表示などの際、画面をロックする
GS_LIBRARY.lock = function (area) {
    var windowWidth = $(window).width();
    var windowHeight = $(window).height();
    //スクロールさせないことで対処
    //if (document.body.scrollHeight > windowHeight) { windowHeight = document.body.scrollHeight; }
    var topNow = $(window).scrollTop();

    var screen = '<div id="LockScreen" style="position:absolute;left:0;top:0;width:' + windowWidth + 'px;height:' + windowHeight + 'px;" class="gs-modal-overlay">';
    screen += "<img src='" + GS_LIBRARY.LoadingImage + "' style='position:relative;top:35%;left:40%'/>";
    screen += "</div>";

    $("body").append(screen);
    $(document).bind('scroll', function () {
        $(window).scrollTop();
    });

    if(area !== undefined){
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function(sender, args) {
            if(Array.contains(sender._updatePanelClientIDs,area)){ GS_LIBRARY.unlock(); }
        })
    }

}

//ロックした画面の開放
GS_LIBRARY.unlock = function() {
    $("#LockScreen").remove();
    $(document).unbind('scroll');
}


})();