<template>
  <AuthLayout 
    tagline="Welcome Back" 
    description="Sign in to your account and continue your journey with us."
  >
    <div class="auth-form">
      <div class="form-header">
        <h1 class="form-title">Sign In</h1>
        <p class="form-subtitle">Enter your credentials to access your account</p>
      </div>
      
      <!-- Error Alert -->
      <div v-if="apiError" class="error-alert">
        <div class="error-icon">
          <svg width="20" height="20" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
          </svg>
        </div>
        <div class="error-content">
          <p class="error-title">Authentication Failed</p>
          <p class="error-message">{{ apiError }}</p>
        </div>
        <button @click="clearError" class="error-close">
          <svg width="14" height="14" viewBox="0 0 14 14" fill="currentColor">
            <path d="M1 1l12 12M13 1L1 13" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          </svg>
        </button>
      </div>
      
      <form @submit.prevent="handleLogin" class="login-form">
        <FormInput
          id="email"
          type="email"
          label="Email Address"
          placeholder="Enter your email"
          v-model="form.email"
          :required="true"
          :error="errors.email"
        />
        
        <FormInput
          id="password"
          type="password"
          label="Password"
          placeholder="Enter your password"
          v-model="form.password"
          :required="true"
          :error="errors.password"
        />
        
        <div class="form-options">
          <label class="checkbox-label">
            <input type="checkbox" v-model="form.rememberMe" class="checkbox">
            <span class="checkmark"></span>
            Remember me
          </label>
          
          <a href="#" class="forgot-link">Forgot password?</a>
        </div>
        
        <button type="submit" class="submit-btn" :disabled="isLoading">
          <span v-if="!isLoading">Sign In</span>
          <span v-else class="loading">
            <div class="spinner"></div>
            Signing in...
          </span>
        </button>
        
        <div class="form-footer">
          <p class="switch-form">
            Don't have an account? 
            <button type="button" @click="$emit('switch-to-signup')" class="switch-link">Sign up</button>
          </p>
        </div>
      </form>
    </div>
  </AuthLayout>
</template>

<script setup>
// Update your LoginView.vue <script setup> section with this:

import { ref, reactive } from 'vue'
import AuthLayout from '../../components/AuthLayout.vue'
import FormInput from '../../components/FormInput.vue'
import authService from '../../services/authService.js'

const emit = defineEmits(['switch-to-signup', 'login-success'])

const form = reactive({
  email: '',
  password: '',
  rememberMe: false
})

const errors = reactive({
  email: '',
  password: ''
})

const isLoading = ref(false)
const apiError = ref('')

const validateForm = () => {
  errors.email = ''
  errors.password = ''
  
  if (!form.email) {
    errors.email = 'Email is required'
    return false
  }
  
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
    errors.email = 'Please enter a valid email address'
    return false
  }
  
  if (!form.password) {
    errors.password = 'Password is required'
    return false
  }
  
  if (form.password.length < 3) {
    errors.password = 'Password must be at least 3 characters'
    return false
  }
  
  return true
}

const clearError = () => {
  apiError.value = ''
}

const handleLogin = async () => {
  if (!validateForm()) return
  
  isLoading.value = true
  apiError.value = ''
  
  try {
    const result = await authService.login({
      email: form.email,
      password: form.password,
      rememberMe: form.rememberMe
    })
    
    if (result.success) {
      
      // Emit login success with role information
      emit('login-success', {
        role: result.role,
        user: result.data,
        rememberMe: form.rememberMe
      })
      
    } else {
      // Display error from auth service
      apiError.value = result.message
    }
    
  } catch (error) {
    apiError.value = 'An unexpected error occurred. Please try again.'
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.auth-form {
  width: 100%;
}

.form-header {
  text-align: center;
  margin-bottom: 2rem;
}

.form-title {
  color: #1D1D1D;
  font-size: 2rem;
  font-weight: 700;
  margin: 0 0 0.5rem 0;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

.form-subtitle {
  color: #6B7280;
  font-size: 1rem;
  margin: 0;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

/* Error Alert Styles */
.error-alert {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  background-color: #FEF2F2;
  border: 1px solid #FECACA;
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1.5rem;
  animation: slideIn 0.3s ease-out;
}

.error-icon {
  color: #DC2626;
  flex-shrink: 0;
  margin-top: 0.125rem;
}

.error-content {
  flex: 1;
}

.error-title {
  font-size: 0.875rem;
  font-weight: 600;
  color: #DC2626;
  margin: 0 0 0.25rem 0;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

.error-message {
  font-size: 0.875rem;
  color: #B91C1C;
  margin: 0;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  line-height: 1.4;
}

.error-close {
  background: none;
  border: none;
  color: #DC2626;
  cursor: pointer;
  padding: 0.125rem;
  border-radius: 4px;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.error-close:hover {
  background-color: #FECACA;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.login-form {
  width: 100%;
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.checkbox-label {
  display: flex;
  align-items: center;
  cursor: pointer;
  font-size: 0.875rem;
  color: #1D1D1D;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

.checkbox {
  display: none;
}

.checkmark {
  width: 18px;
  height: 18px;
  border: 2px solid #EEEEEE;
  border-radius: 4px;
  margin-right: 0.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.checkbox:checked + .checkmark {
  background-color: #F6BF00;
  border-color: #F6BF00;
}

.checkbox:checked + .checkmark::after {
  content: '✓';
  color: white;
  font-size: 12px;
  font-weight: bold;
}

.forgot-link {
  color: #F6BF00;
  text-decoration: none;
  font-size: 0.875rem;
  font-weight: 500;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  transition: color 0.2s ease;
}

.forgot-link:hover {
  color: #F6C900;
}

.submit-btn {
  width: 100%;
  background: linear-gradient(135deg, #F6BF00 0%, #F6C900 100%);
  color: white;
  border: none;
  padding: 1rem;
  border-radius: 12px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  margin-bottom: 1.5rem;
}

.submit-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(246, 191, 0, 0.3);
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.spinner {
  width: 18px;
  height: 18px;
  border: 2px solid transparent;
  border-top: 2px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.form-footer {
  text-align: center;
}

.switch-form {
  color: #6B7280;
  font-size: 0.875rem;
  margin: 0;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

.switch-link {
  color: #F6BF00;
  background: none;
  border: none;
  cursor: pointer;
  font-weight: 600;
  font-size: 0.875rem;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  transition: color 0.2s ease;
}

.switch-link:hover {
  color: #F6C900;
}

@media (max-width: 480px) {
  .form-options {
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }
  
  .error-alert {
    padding: 0.875rem;
    gap: 0.5rem;
  }
  
  .error-title,
  .error-message {
    font-size: 0.8125rem;
  }
}
</style>