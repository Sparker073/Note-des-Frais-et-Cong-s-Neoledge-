<template>
  <div class="auth-layout">
    <!-- Left Panel -->
    <div class="auth-left-panel">
      <div class="brand-section">
        <Logo class="logo" :width="240" :height="80" />
        <h2 class="brand-tagline">{{ tagline }}</h2>
        <p class="brand-description">{{ description }}</p>
      </div>
      
      
    </div>
    
    <!-- Right Panel -->
    <div class="auth-right-panel">
      <div class="auth-form-container">
        <slot />
      </div>
    </div>
  </div>
</template>

<script setup>
import Logo from './Logo.vue'

defineProps({
  tagline: {
    type: String,
    default: 'Welcome to the Future'
  },
  description: {
    type: String,
    default: 'Experience seamless management and innovation at your fingertips.'
  }
})
</script>

<style scoped>
.auth-layout {
  display: flex;
  min-height: 100vh;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

.auth-left-panel {
  flex: 1;
  background: linear-gradient(135deg, #F6BF00 0%, #F6C900 100%);
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 3rem;
  position: relative;
  overflow: hidden;
}

.brand-section {
  text-align: center;
  z-index: 2;
  
}

.brand-tagline {
  color: white;
  font-size: 2.5rem;
  font-weight: 700;
  margin: 2rem 0 1rem 0;
  line-height: 1.2;
}

.brand-description {
  color: rgba(255, 255, 255, 0.9);
  font-size: 1.1rem;
  font-weight: 400;
  max-width: 400px;
  line-height: 1.6;
  margin: 0;
}

.decorative-elements {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  overflow: hidden;
}

.logo{
  position: relative;
  top:-50px;
}

@keyframes float {
  0%, 100% {
    transform: translateY(0px) rotate(0deg);
  }
  50% {
    transform: translateY(-20px) rotate(180deg);
  }
}

.auth-right-panel {
  flex: 1;
  background: #FFFFFF;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
}

.auth-form-container {
  width: 100%;
  max-width: 400px;
}

@media (max-width: 768px) {
  .auth-layout {
    flex-direction: column;
  }
  
  .auth-left-panel {
    min-height: 300px;
    padding: 2rem;
  }
  
  .brand-tagline {
    font-size: 2rem;
  }
  
  .auth-right-panel {
    flex: 1;
  }
}
</style>