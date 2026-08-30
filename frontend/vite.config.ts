import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { fileURLToPath, URL } from "node:url";

// The dev server proxies /api and /jobs to the ASP.NET host so the SPA and API
// share an origin in development, matching the production single-origin deploy.
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) },
  },
  server: {
    port: 5173,
    proxy: {
      "/api": { target: "http://localhost:5080", changeOrigin: true },
      "/hubs": { target: "http://localhost:5080", changeOrigin: true, ws: true },
      "/jobs": { target: "http://localhost:5080", changeOrigin: true },
      "/health": { target: "http://localhost:5080", changeOrigin: true },
    },
  },
  build: {
    // Emit straight into the API's wwwroot so `dotnet publish` ships the SPA.
    outDir: "../src/SopaTalk.Api/wwwroot",
    emptyOutDir: true,
  },
});
