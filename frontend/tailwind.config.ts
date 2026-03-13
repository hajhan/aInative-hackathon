import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      // 최소 폰트 크기 16px 보장 (고령자 접근성)
      fontSize: {
        xs: ["0.875rem", { lineHeight: "1.25rem" }],   // 14px (최소)
        sm: ["1rem", { lineHeight: "1.5rem" }],         // 16px
        base: ["1.125rem", { lineHeight: "1.75rem" }],  // 18px
        lg: ["1.25rem", { lineHeight: "1.75rem" }],     // 20px
        xl: ["1.5rem", { lineHeight: "2rem" }],         // 24px
        "2xl": ["1.75rem", { lineHeight: "2.25rem" }],  // 28px
        "3xl": ["2rem", { lineHeight: "2.5rem" }],      // 32px
      },
      colors: {
        primary: {
          50: "#eff6ff",
          100: "#dbeafe",
          200: "#bfdbfe",
          300: "#93c5fd",
          400: "#60a5fa",
          500: "#3b82f6",
          600: "#2563eb",
          700: "#1d4ed8",
          800: "#1e40af",
          900: "#1e3a8a",
        },
      },
      // 최소 터치 영역 44x44px (모바일 접근성)
      minHeight: {
        touch: "44px",
      },
      minWidth: {
        touch: "44px",
      },
    },
  },
  plugins: [],
};

export default config;
