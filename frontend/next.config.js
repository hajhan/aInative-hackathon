/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  output: 'standalone',
  // PWA 설정은 next-pwa 또는 @serwist/next를 통해 관리
  // 개발 환경에서는 Service Worker 비활성화
};

// next-pwa 통합 (설치된 경우)
let config = nextConfig;

try {
  const withPWA = require('next-pwa')({
    dest: 'public',
    disable: process.env.NODE_ENV === 'development',
    register: true,
    skipWaiting: true,
    runtimeCaching: [
      {
        urlPattern: /^https?:\/\/.*\/api\/.*/,
        handler: 'NetworkFirst',
        options: {
          cacheName: 'api-cache',
          expiration: {
            maxEntries: 50,
            maxAgeSeconds: 300,
          },
        },
      },
      {
        urlPattern: /\.(png|jpg|jpeg|svg|gif|webp|ico)$/,
        handler: 'CacheFirst',
        options: {
          cacheName: 'image-cache',
          expiration: {
            maxEntries: 100,
            maxAgeSeconds: 30 * 24 * 60 * 60,
          },
        },
      },
    ],
  });
  config = withPWA(nextConfig);
} catch {
  // next-pwa not installed, use base config
  config = nextConfig;
}

module.exports = config;
