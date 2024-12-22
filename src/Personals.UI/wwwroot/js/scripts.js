window.clipboardUtils = {
    copyText: function (text) {
        navigator.clipboard.writeText(text)
            .catch(function (error) {
                alert(error);
            });
    },
    copyTextFromElement: function (codeElement) {
        navigator.clipboard.writeText(codeElement.textContent)
            .catch(function (error) {
                alert(error);
            });
    }
}
