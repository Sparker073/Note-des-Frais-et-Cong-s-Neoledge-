<template>
  <div id="app">
    <router-view 
      @switch-to-login="handleSwitchToLogin" 
      @switch-to-signup="handleSwitchToSignup"
      @login-success="handleLoginSuccess"
      @signup-success="handleSignupSuccess"
    />
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import '@fontsource/inter/300.css'
import '@fontsource/inter/400.css'
import '@fontsource/inter/500.css'
import '@fontsource/inter/600.css'
import '@fontsource/inter/700.css'
import { router } from './routers/index.js'

// Initialize routing when app mounts
onMounted(() => {
  router.initializeRoute()
})

// Event handlers for authentication flow
const handleSwitchToLogin = () => {
  router.goToLogin()
}

const handleSwitchToSignup = () => {
  router.goToSignup()
}

const handleLoginSuccess = (userData) => {
  // Extract role from the login response
  const role = userData?.role || userData?.Role
  
  if (role) {
    // Store the role for persistence
    const storage = userData.rememberMe ? localStorage : sessionStorage
    storage.setItem('userRole', role)
    
    // Route based on role
    router.routeByRole(role)
  } else {
    console.error('No role provided in login response')
    router.goToAdminDashboard()
  }
}

const handleSignupSuccess = (userData) => {
  // Extract role from the signup response
  const role = userData?.role || userData?.Role
  
  if (role) {
    // Store the role for persistence (default to sessionStorage for new signups)
    sessionStorage.setItem('userRole', role)
    
    // Route based on role
    router.routeByRole(role)
  } else {
    console.error('No role provided in signup response')
    router.goToEmployeeDashboard()
  }
}
</script>

<style>
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  line-height: 1.6;
  color: #1D1D1D;
  background: linear-gradient(135deg, #EEEEEE 0%, #FFFFFF 100%);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  overflow-x: hidden;
}

#app {
  width: 100%;
  min-height: 100vh;
}

/* Transitions and animations */
.fade-enter-active, .fade-leave-active {
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}

.fade-enter-from, .fade-leave-to {
  opacity: 0;
  transform: translateY(20px);
}

.slide-up-enter-active {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.slide-up-enter-from {
  opacity: 0;
  transform: translateY(30px);
}

.bounce-enter-active {
  animation: bounce-in 0.5s cubic-bezier(0.68, -0.55, 0.265, 1.55);
}

@keyframes bounce-in {
  0% {
    transform: scale(0.95) translateY(20px);
    opacity: 0;
  }
  50% {
    transform: scale(1.02) translateY(-5px);
  }
  100% {
    transform: scale(1) translateY(0);
    opacity: 1;
  }
}

.floating-animation {
  animation: floating 6s ease-in-out infinite;
}

@keyframes floating {
  0%, 100% { transform: translateY(0px) rotate(0deg); }
  50% { transform: translateY(-10px) rotate(1deg); }
}

.pulse-animation {
  animation: pulse 4s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: 0.8; transform: scale(1.02); }
}
</style>