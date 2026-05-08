window.getWindowHeight = function() {
    return window.innerHeight
};

window.setupResizeListener = function(dotnetHelper) {
    window.addEventListener('resize', async () =>{
        await dotnetHelper.invokeMethodAsync('onWindowResize');
    });
};

window.removeResizeListener = function() {
    window.removeEventListener('resize', null)
};