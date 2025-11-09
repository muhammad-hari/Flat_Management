window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
};

window.removeGlobalClickHandler = function () {
    if (window.globalClickHandler) {
        document.removeEventListener('click', window.globalClickHandler);
        window.globalClickHandler = null;
    }
};

window.addGlobalClickHandler = function () {
    // Remove existing handler if any
    window.removeGlobalClickHandler();
    
    // Create and store the handler
    window.globalClickHandler = function (e) {
        try {
            const path = e.composedPath();
            const elementIds = path
                .map(el => el.id || '')
                .filter(id => id)
                .join(',');
                
            DotNet.invokeMethodAsync('MyApp.Web', 'HandleGlobalClick', elementIds);
        } catch (error) {
            console.error('Error in global click handler:', error);
        }
    };
    
    // Add the handler
    document.addEventListener('click', window.globalClickHandler);
};
