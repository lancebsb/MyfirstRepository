//弹出警告信息
showWarning = function (title, mes, lock, autoClose) {
    var icon = "warning";
    if (autoClose) {//自动关闭        
        showAlertAutodialog(title, mes, icon);
    } else {
        showAlertDialog(title, mes, lock, icon);
    }
}
//弹出错误信息
showError = function (title, mes, lock, autoClose) {
    var icon = "error";   
    if (autoClose) {//自动关闭        
        showAlertAutodialog(title, mes, icon);
    } else {
        showAlertDialog(title, mes, lock, icon);
    }
}
//弹出成功信息
showSucceed = function (title, mes,lock, autoClose) {
    var icon = "succeed";
    if (autoClose) {//自动关闭        
        showAlertAutodialog(title,mes,icon);
    } else {
        showAlertDialog(title, mes, lock, icon);
    }
}
showAlertDialog = function (title, mes, lock, icon) {
    var html = '<table><tr><td><img src="/Images/icons/' + icon + '.png" /></td><td>' + mes + '</td></tr></table>';
    var d = top.dialog({
        title: title,
        padding: 10,
        content: html,
        ok: true
    });
    if (lock)
        d.showModal();
    else
        d.show();
}
showAlertAutodialog = function (title, mes, icon) {
    var html = '<table><tr><td><img src="/Images/icons/' + icon + '.png" /></td><td>' + mes + '</td></tr></table>';
    var d = top.dialog({
        content: html,
        padding: 10
    });
    d.show();
    setTimeout(function () {
        d.close().remove();
    }, 2000);
}
//弹出询问信息
showQuestion = function (title, mes, lock, okFunc,cancelFunc) {
    var icon = "question"
    var html = '<table><tr><td><img src="/Images/icons/' + icon + '.png" /></td><td>' + mes + '</td></tr></table>';
    var d = top.dialog({
        title: title,
        width:150,
        padding: 20,
        content: html,
        ok: okFunc,
        cancel: cancelFunc
    });
    if (lock)
        d.showModal();
    else
        d.show();
}



