<template>
  <div class="employee-layout">


    <!-- Overlay for mobile -->
    <div 
      v-if="!isSidebarCollapsed && isMobile" 
      class="sidebar-overlay" 
      @click="closeSidebar"
    ></div>

    <Sidebar 
      :user="user" 
      :is-collapsed="isSidebarCollapsed"
      @navigate="navigateToView"
      @toggle="toggleSidebar"
    />
    
    <main class="main-content" :class="{ 'sidebar-collapsed': isSidebarCollapsed }">
      <transition name="fade" mode="out-in">
        <router-view />
      </transition>
    </main>
  </div>
</template>

<script>
import Sidebar from '../../components/Sidebar.vue'
import { router } from '../../routers/index.js'
import { ref, onMounted, onUnmounted } from 'vue'

export default {
  name: 'EmployeeLayout',
  components: {
    Sidebar
  },
  setup() {
    const isSidebarCollapsed = ref(false)
    const isMobile = ref(window.innerWidth <= 768)

    const handleResize = () => {
      isMobile.value = window.innerWidth <= 768
      // Auto-collapse on mobile
      if (isMobile.value) {
        isSidebarCollapsed.value = true
      }
    }

    const toggleSidebar = () => {
      isSidebarCollapsed.value = !isSidebarCollapsed.value
    }

    const closeSidebar = () => {
      if (isMobile.value) {
        isSidebarCollapsed.value = true
      }
    }

    onMounted(() => {
      window.addEventListener('resize', handleResize)
      // Auto-collapse sidebar on mobile by default
      if (isMobile.value) {
        isSidebarCollapsed.value = true
      }
    })

    onUnmounted(() => {
      window.removeEventListener('resize', handleResize)
    })

    return {
      isSidebarCollapsed,
      isMobile,
      toggleSidebar,
      closeSidebar
    }
  },
  data() {
    return {
      user: {
        id: 1,
        name: 'Abeslam',
        role: 'Employee',
        avatar: 'https://images.pexels.com/photos/1040880/pexels-photo-1040880.jpeg?auto=compress&cs=tinysrgb&w=60&h=60&fit=crop&crop=face',
        pendingRequests: 3,
        approvedRequests: 12,
        teamMembers: [
          { id: 2, name: 'Alex Chen', role: 'Developer' },
          { id: 3, name: 'Maria Garcia', role: 'Designer' },
          { id: 4, name: 'John Smith', role: 'Manager' }
        ]
      }
    }
  },
  methods: {
    navigateToView(section) {
      // Use the router to navigate to employee sections
      router.goToEmployeeSection(section)
      
      // Auto-close sidebar on mobile after navigation
      if (this.isMobile) {
        this.closeSidebar()
      }
    }
  }
}
</script>

<style scoped>
.employee-layout {
  display: flex;
  width: 100%;
  min-height: 100vh;
  position: relative;
}



/* Sidebar Overlay for Mobile */
.sidebar-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 999;
  backdrop-filter: blur(4px);
}

.main-content {
  flex: 1;
  padding: 2rem;
  overflow-y: auto;
  background: linear-gradient(135deg, #EEEEEE 0%, #FFFFFF 100%);
  position: relative;
  margin-left: 280px;
  transition: margin-left 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.main-content.sidebar-collapsed {
  margin-left: 0;
}

.main-content::before {
  content: '';
  position: fixed;
  top: 0;
  left: 280px;
  right: 0;
  bottom: 0;
  background: 
    radial-gradient(circle at 20% 80%, rgba(246, 201, 0, 0.1) 0%, transparent 50%),
    radial-gradient(circle at 80% 20%, rgba(246, 191, 0, 0.05) 0%, transparent 50%);
  pointer-events: none;
  z-index: 0;
  transition: left 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.main-content.sidebar-collapsed::before {
  left: 0;
}

.main-content > * {
  position: relative;
  z-index: 1;
}

.fade-enter-active, .fade-leave-active {
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}

.fade-enter-from, .fade-leave-to {
  opacity: 0;
  transform: translateY(20px);
}

/* Mobile Styles */
@media (max-width: 768px) {
  .main-content {
    margin-left: 0;
    padding: 1rem;
  }
  
  .main-content::before {
    left: 0;
  }
}

/* Tablet Styles */
@media (max-width: 1024px) and (min-width: 769px) {
  .main-content {
    margin-left: 240px;
  }
  
  .main-content::before {
    left: 240px;
  }
  
  .main-content.sidebar-collapsed {
    margin-left: 0;
  }
  
  .main-content.sidebar-collapsed::before {
    left: 0;
  }
}
</style>