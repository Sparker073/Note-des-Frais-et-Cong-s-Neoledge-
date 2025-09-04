// main.js
import { createApp } from 'vue'
import App from './App.vue'
import { vueRouterInstance } from './routers/index.js'
import '@fortawesome/fontawesome-free/css/all.css'

const app = createApp(App)

// Use Vue Router
app.use(vueRouterInstance)

app.mount('#app')