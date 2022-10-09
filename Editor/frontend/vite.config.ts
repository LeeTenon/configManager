import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'

export default defineConfig({
  base: './',
  //1.别名配置
  resolve: {
    alias: [
      { find: '@', replacement: path.resolve(__dirname, 'src') },
      { find: 'comps', replacement: path.resolve(__dirname, 'src/components') },
    ],
  },
  //2.插件相关配置
  plugins: [vue()],
  // 3.服务有关配置
  server: {
    host: '0.0.0.0',
    port: 3000,
    https: false,
  }
})
