window.IdleDetector = {
    start: function (dotNetObjRef, timeoutMs) {
        let timer;

        function resetTimer() {
            clearTimeout(timer);
            timer = setTimeout(() => {
                dotNetObjRef.invokeMethodAsync('OnUserIdle');
            }, timeoutMs);
        }

        // Aktivitas yang dianggap “user aktif”
        ['mousemove', 'keydown', 'click', 'scroll', 'touchstart']
            .forEach(evt => document.addEventListener(evt, resetTimer));

        resetTimer();
    }
};
