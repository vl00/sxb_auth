//UE.registerUI('h2',function(editor,uiName){
//    var me = this;
//    //参考addCustomizeButton.js
//    var btn = new UE.ui.Button({
//        name:'button' + uiName,
//        title:'标题',
//        //需要添加的额外样式，指定icon图标，这里默认使用一个重复的icon
//        cssRules :'background-position: -500px 0;',
//        onclick: function () {
//            editor.execCommand('Paragraph', btn.isChecked()?'p':'h2');
//        }
//    });

//    return btn;
//}/*index 指定添加到工具栏上的那个位置，默认时追加到最后,editorId 指定这个UI是那个编辑器实例上的，默认是页面上所有的编辑器都会添加这个按钮*/);

UE.ui.h2 = function (editor, t, uiName) {
    uiName = "H2标题";
    var i = "h2",
        btn = new UE.ui.Button({
            className: "edui-for-" + i,
            title: uiName,
            onclick: function () {
                editor.execCommand("paragraph", btn.isChecked() ? "p" : "h2");
            }
        });
    return UE.ui.buttons[i] = btn,
        editor.addListener("selectionchange", function (t, n, a) {
            if (!a) {
                var i = editor.queryCommandState("Paragraph");
                if (-1 === i)
                    btn.setDisabled(!0), btn.setChecked(!1);
                else {
                    btn.setDisabled(!1);
                    var s = editor.queryCommandValue("Paragraph");
                    btn.setChecked("h2" === s);
                }
            }
        }), btn;
};