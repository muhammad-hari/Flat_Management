window.owlEyes = {
    init: function () {
        const leftEye  = document.getElementById('eye-left');
        const rightEye = document.getElementById('eye-right');
        const container = document.querySelector('.owl-container');

        document.addEventListener('mousemove', e => {
            const rect = container.getBoundingClientRect();
            const centerX = rect.left + rect.width / 2;
            const centerY = rect.top + rect.height / 2;

            // sudut antara mouse dan pusat logo
            const angle = Math.atan2(e.clientY - centerY, e.clientX - centerX);
            const distance = 8; // seberapa jauh pupil bergerak

            const dx = Math.cos(angle) * distance;
            const dy = Math.sin(angle) * distance;

            leftEye.style.transform  = `translate(${dx}px, ${dy}px)`;
            rightEye.style.transform = `translate(${dx}px, ${dy}px)`;
        });
    }
};
