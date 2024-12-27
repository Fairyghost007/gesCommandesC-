/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Pages/**/*.cshtml",
    "./Views/**/*.cshtml",
    "./wwwroot/**/*.html",
  ],
  theme: {
    extend: {
      colors: {
        customLightYellow: "rgb(255, 250, 235)",
        greenMe: "#243132",
        darkPurple: "#141124",
        lightPurple: "#2F2B43",
        palePurple: "#393351",
        purplle: "#524A7B",
        darkRose: "#A868A0",
        paleRose: {
          DEFAULT: "#D4CEE3",
          50: "rgba(212, 206, 227, 0.5)",
          75: "rgba(212, 206, 227, 0.75)",
          90: "rgba(212, 206, 227, 0.9)",
        },
      },
      backgroundImage: {
        "custom-gradient": "linear-gradient(to right, , #3b82f6)", // cyan-500 to blue-500
      },
    },
  },
  plugins: [],
};
