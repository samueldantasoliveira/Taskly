/** @type {import('next').NextConfig} */
const nextConfig = {
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: "http://localhost:5219/api/:path*",
      },
    ];
  },
};

export default nextConfig;
