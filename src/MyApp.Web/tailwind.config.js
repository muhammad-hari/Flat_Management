// tailwind.config.js

module.exports = {
  content: [
    "./**/*.razor",
    "./**/*.cshtml",
    "./**/*.html"
  ],
  theme: {
    // Masukkan keyframes dan animation di dalam extend ini
    extend: {
      keyframes: {
        'fade-in-slide-left': {
          '0%': { opacity: '0', transform: 'translateX(100%)' },
          '100%': { opacity: '1', transform: 'translateX(0)' },
        },
        'fade-out-slide-right': {
          '0%': { opacity: '1', transform: 'translateX(0)' },
          '100%': { opacity: '0', transform: 'translateX(100%)' },
        }
      },
      animation: {
        'fade-in-slide-left': 'fade-in-slide-left 0.5s ease-out forwards',
        'fade-out-slide-right': 'fade-out-slide-right 0.5s ease-in forwards',
      }
    },
  },
  plugins: [],
}