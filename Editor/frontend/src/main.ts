// vue
import { createApp } from 'vue'
import App from './App.vue'
// element ui
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import 'xe-utils'
import VXETable from 'vxe-table'
import 'vxe-table/lib/style.css'
function useTable (app:any) {
    app.use(VXETable)

  }
createApp(App).use(ElementPlus).use(useTable).mount('#app')
