import { defineConfig } from 'vitest/config';

export default defineConfig({
  esbuild: {
    target: 'esnext',
    supported: {
      'top-level-await': true,
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
  },
});