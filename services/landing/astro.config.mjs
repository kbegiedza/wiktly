// @ts-check
import path from "node:path";
import { fileURLToPath } from "node:url";
import { defineConfig } from "astro/config";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

import vue from "@astrojs/vue";
import tailwindcss from "@tailwindcss/vite";

// https://astro.build/config
export default defineConfig({
  integrations: [vue()],
  vite: {
    plugins: [tailwindcss()],
    resolve: {
      alias: {
        "#": path.resolve(__dirname, "./src"),
        "#lib": path.resolve(__dirname, "./src/lib"),
        "#assets": path.resolve(__dirname, "./src/assets"),
        "#images": path.resolve(__dirname, "./src/assets/images"),
        "#icons": path.resolve(__dirname, "./src/components/icons"),
        "#layouts": path.resolve(__dirname, "./src/layouts"),
        "#components": path.resolve(__dirname, "./src/components"),
      },
    },
  },
});
