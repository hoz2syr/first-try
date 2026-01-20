/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  transpilePackages: ['@first-try/shared'],
  output: 'standalone',
}

module.exports = nextConfig
