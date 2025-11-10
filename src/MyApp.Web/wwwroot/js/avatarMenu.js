window.initializeAvatarMenu = () => {
    window.removeGlobalClickHandler = function () {
        document.removeEventListener('click', window.globalClickHandler);
    };

    window.addGlobalClickHandler = function () {
        // Remove existing handler if any
        window.removeGlobalClickHandler();
        
        // Create and store the handler
        window.globalClickHandler = function (e) {
            const elementId = e.target.id || e.target.parentElement.id;
        };
        
        // Add the handler
        document.addEventListener('click', window.globalClickHandler);
    };
};