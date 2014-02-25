function setPPPMenuAction() {
    displayPPPDefault(true);
    $("li.ppp-menu-item").hover(
                    function () {
                        $(this).addClass("ppp-selected");
                        $contents = $("div.ppp-menu-content-item:not(.default)");
                        $($contents[$(this).index()]).css("display", "inline");

                    },
                    function () {
                        $(this).removeClass("ppp-selected");
                        $contents = $("div.ppp-menu-content-item:not(.default)");
                        $($contents[$(this).index()]).css("display", "none");

                    }
            );

    $("div#ppp-menu-list").mouseover(
                function () { displayPPPDefault(false); })
            .mouseout(
                function () { displayPPPDefault(true); })
}

function displayPPPDefault(isVisible) {
    if (isVisible == true) {
        $("div.ppp-menu-content-item.default").css("display", "inline");
    } else {
        $("div.ppp-menu-content-item.default").css("display", "none");

    }
}

//IEでcss inheritが使えない対応。とりあえず困るanchor-backgroundについて対応
$(function () {
    if ($.browser.msie) {
        $("a.ppp-link-void").each(function () {
            $(this).css('background-color', $(this).parent().css('background-color'));
        });
    }
});


function makePPPSwitchArea(activenum) {
    var active = 0;
    if (arguments.length > 0) {
        active = arguments[0];
    }

	$(".ppp-switch-area").accordion(
		{ heightStyle: "content" }, 
		{ header: "h3.ppp-switch-header" },
		{ collapsible: true },
		/*{ animated: 'bounceslide' },*/
		{icons: {
			'header': 'ui-icon-circle-plus',
			'headerSelected': 'ui-icon-circle-minus'
			}
		}
	);
	
	if(active > 0){
		$(".ppp-switch-area").accordion("option","active",active);
	}else if(active < 0){
		$(".ppp-switch-area").accordion("option","active",false);
	}
}
function switchPPPSwitchArea(id, section) {
    var target = 0;
    if (arguments.length > 1) {
        target = arguments[1];
    }

	if(!$("#"+id).children("div").eq(target).hasClass("ui-accordion-content-active")){
		$("#" + id ).accordion("activate",target);
	}
}

function displayPPPTable() {
    $("table.ppp-table").each(
        function () {
            $(this).find("tr:odd").find("td").addClass("ppp-table-odd");
            $(this).find("tr:even").find("td").addClass("ppp-table-even");
        }
    )

}

//AjaxControlToolkitでコンボボックスがバグる件について対応するスクリプト
function adjustCombobox(){
	$(".ppp-combo .ajax__combobox_itemlist").each(function(){
		//コンボボックスの高さ指定
		var liCount = $(this).find("li").length;
		var h = 20 * liCount;
		$(this).css("height", h + "px !important");	
		
		//コンボボックスの幅指定
		var textId = $(this).attr("id").replace("OptionList","TextBox");	
		var boxWidth = $("#"+textId).width();
		var boxHeight = $("#"+textId).height();
		$(this).css("width", boxWidth + "px !important");							
		
		//ボタンのサイズ指定(6という数字に特に意味はないが・・・)
		var btnId = $(this).attr("id").replace("OptionList","Button");	
		$("#"+btnId).css("width",( boxHeight + 6 )+ "px !important");
		$("#"+btnId).css("height",( boxHeight + 6 ) + "px !important");
		
	})
}

//RaioButtonListの値が取得しにくい対応
function getCheckedRadioButtonList(id) {
    var val = "";
    $("#" + id).find(":radio").each(function () {
        if ($(this).attr("checked") == true) {
            val = $(this).val();
        }
    });
    return val;
}

//大文字・小文字のスタイルを値に反映
function applyTransformToVal(){

	$(":text").each(function(){
		var attr = $(this).css("text-transform");
		if (attr == "uppercase") {
			$(this).blur(function(){ val = $(this).val(); $(this).val(val.toUpperCase()); });
		} else if (attr == "lowercase") {
			$(this).blur(function(){ val = $(this).val(); $(this).val(val.toLowerCase()); });
		}
		
	})
	
}
