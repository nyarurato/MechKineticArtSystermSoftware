function getBrowserLocale() {
    return (navigator.languages && navigator.languages.length) ? navigator.languages[0] : navigator.userLanguage || navigator.language || navigator.browserLanguage || 'en';
}
window.blazorCulture = {
    get: () => window.localStorage['BlazorCulture'],
    set: (value) => window.localStorage['BlazorCulture'] = value
};

function scrollTextareaToBottom(textarea) {
    textarea.scrollTop = textarea.scrollHeight;
}