module.exports = {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: '#106ad7',
        accent: '#2b6fb6',
        bg: '#f1f5f9',
        panel: '#ffffff',
        success: '#2ecc71',
        warning: '#f59e0b',
        danger: '#ef4444'
      },
      boxShadow: {
        soft: '0 6px 18px rgba(16,106,215,0.08)'
      }
    }
  },
  plugins: [],
}
