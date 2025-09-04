<template>
  <AuthLayout 
    tagline="Join Us Today" 
    description="Create your account and start your journey with our innovative platform."
  >
    <div class="auth-form">
      <div class="form-header">
        <h1 class="form-title">Create Account</h1>
        <p class="form-subtitle">Fill in your information to get started</p>
      </div>
      
      <!-- Error Alert -->
      <div v-if="apiError" class="error-alert">
        <div class="error-icon">
          <svg width="20" height="20" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
          </svg>
        </div>
        <div class="error-content">
          <p class="error-title">Registration Failed</p>
          <p class="error-message">{{ apiError }}</p>
        </div>
        <button @click="clearError" class="error-close">
          <svg width="14" height="14" viewBox="0 0 14 14" fill="currentColor">
            <path d="M1 1l12 12M13 1L1 13" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          </svg>
        </button>
      </div>
      
      <form @submit.prevent="handleSignup" class="signup-form" novalidate>
        <!-- Nom complet -->
        <FormInput
          id="name"
          type="text"
          label="Full Name *"
          placeholder="Enter your full name"
          v-model="form.name"
          :required="true"
          :error="errors.name"
          @blur="validateField('name')"
          @input="clearFieldError('name')"
        />
        
        <!-- Email -->
        <FormInput
          id="email"
          type="email"
          label="Email Address *"
          placeholder="Enter your email"
          v-model="form.email"
          :required="true"
          :error="errors.email"
          @blur="validateField('email')"
          @input="clearFieldError('email')"
        />
        
        <!-- Password avec indicateur de force -->
        <div class="form-field">
          <FormInput
            id="password"
            type="password"
            label="Password *"
            placeholder="Create a strong password"
            v-model="form.password"
            :required="true"
            :error="errors.password"
            @blur="validateField('password')"
            @input="onPasswordChange"
          />
          
          <!-- Indicateur de force du mot de passe -->
          <div v-if="form.password" class="password-strength">
            <div class="strength-bar">
              <div 
                class="strength-fill"
                :class="passwordStrength.class"
                :style="{ width: passwordStrength.width }"
              ></div>
            </div>
            <p class="strength-text" :class="passwordStrength.class">
              {{ passwordStrength.text }}
            </p>
          </div>
          
          <!-- Critères du mot de passe -->
          <div v-if="form.password" class="password-criteria">
            <p class="criteria-title">Password requirements:</p>
            <ul class="criteria-list">
              <li :class="{ 'valid': passwordCriteria.length }">
                <span class="criteria-icon">{{ passwordCriteria.length ? '✓' : '×' }}</span>
                At least 8 characters
              </li>
              <li :class="{ 'valid': passwordCriteria.uppercase }">
                <span class="criteria-icon">{{ passwordCriteria.uppercase ? '✓' : '×' }}</span>
                One uppercase letter
              </li>
              <li :class="{ 'valid': passwordCriteria.lowercase }">
                <span class="criteria-icon">{{ passwordCriteria.lowercase ? '✓' : '×' }}</span>
                One lowercase letter
              </li>
              <li :class="{ 'valid': passwordCriteria.number }">
                <span class="criteria-icon">{{ passwordCriteria.number ? '✓' : '×' }}</span>
                One number
              </li>
            </ul>
          </div>
        </div>
        
        <!-- Confirmation password -->
        <FormInput
          id="confirmPassword"
          type="password"
          label="Confirm Password *"
          placeholder="Confirm your password"
          v-model="form.confirmPassword"
          :required="true"
          :error="errors.confirmPassword"
          @blur="validateField('confirmPassword')"
          @input="clearFieldError('confirmPassword')"
        />
        
        <!-- Message de correspondance des mots de passe -->
        <div v-if="form.password && form.confirmPassword" class="password-match">
          <p v-if="form.password === form.confirmPassword" class="match-success">
            <span class="match-icon">✓</span>
            Passwords match
          </p>
          <p v-else class="match-error">
            <span class="match-icon">×</span>
            Passwords do not match
          </p>
        </div>
        
        <!-- Poste -->
        <FormInput
          id="poste"
          type="text"
          label="Position/Job Title *"
          placeholder="Enter your position"
          v-model="form.poste"
          :required="true"
          :error="errors.poste"
          @blur="validateField('poste')"
          @input="clearFieldError('poste')"
        />
        
        <!-- Manager ID -->
        <FormInput
          id="managerId"
          type="number"
          label="Manager ID (Optional)"
          placeholder="Enter manager ID if applicable"
          v-model="form.managerId"
          :error="errors.managerId"
          @blur="validateField('managerId')"
          @input="clearFieldError('managerId')"
        />
        
        <!-- Checkbox Terms -->
        <div class="form-checkbox">
          <label class="checkbox-label">
            <input 
              type="checkbox" 
              v-model="form.agreeToTerms" 
              class="checkbox" 
              required
              @change="validateField('agreeToTerms')"
            >
            <span class="checkmark"></span>
            I agree to the <a href="#" class="terms-link">Terms of Service</a> and <a href="#" class="terms-link">Privacy Policy</a> *
          </label>
          <p v-if="errors.agreeToTerms" class="field-error">{{ errors.agreeToTerms }}</p>
        </div>
        
        <!-- Submit Button -->
        <button 
          type="submit" 
          class="submit-btn" 
          :disabled="isLoading || !isFormValid"
          :class="{ 'disabled': !isFormValid }"
        >
          <span v-if="!isLoading">Create Account</span>
          <span v-else class="loading">
            <div class="spinner"></div>
            Creating account...
          </span>
        </button>
        
        <!-- Validation Summary -->
        <div v-if="showValidationSummary && Object.keys(errors).some(key => errors[key])" class="validation-summary">
          <h4>Please fix the following errors:</h4>
          <ul>
            <li v-if="errors.name">{{ errors.name }}</li>
            <li v-if="errors.email">{{ errors.email }}</li>
            <li v-if="errors.password">{{ errors.password }}</li>
            <li v-if="errors.confirmPassword">{{ errors.confirmPassword }}</li>
            <li v-if="errors.poste">{{ errors.poste }}</li>
            <li v-if="errors.managerId">{{ errors.managerId }}</li>
            <li v-if="errors.agreeToTerms">{{ errors.agreeToTerms }}</li>
          </ul>
        </div>
        
        <div class="form-footer">
          <p class="switch-form">
            Already have an account? 
            <button type="button" @click="$emit('switch-to-login')" class="switch-link">Sign in</button>
          </p>
        </div>
      </form>
    </div>
  </AuthLayout>
</template>

<script setup>
import { ref, reactive, computed, watch } from 'vue'
import AuthLayout from '../../components/AuthLayout.vue'
import FormInput from '../../components/FormInput.vue'
import authService from '../../services/authService.js'

const emit = defineEmits(['switch-to-login', 'signup-success'])

const form = reactive({
  name: '',
  email: '',
  password: '',
  confirmPassword: '',
  poste: '',
  managerId: null,
  agreeToTerms: false
})

const errors = reactive({
  name: '',
  email: '',
  password: '',
  confirmPassword: '',
  poste: '',
  managerId: '',
  agreeToTerms: ''
})

const isLoading = ref(false)
const apiError = ref('')
const showValidationSummary = ref(false)

// Computed pour la force du mot de passe
const passwordCriteria = computed(() => ({
  length: form.password.length >= 8,
  uppercase: /[A-Z]/.test(form.password),
  lowercase: /[a-z]/.test(form.password),
  number: /\d/.test(form.password)
}))

const passwordStrength = computed(() => {
  const score = Object.values(passwordCriteria.value).filter(Boolean).length
  
  if (score === 0) return { class: '', width: '0%', text: '' }
  if (score === 1) return { class: 'weak', width: '25%', text: 'Weak' }
  if (score === 2) return { class: 'fair', width: '50%', text: 'Fair' }
  if (score === 3) return { class: 'good', width: '75%', text: 'Good' }
  return { class: 'strong', width: '100%', text: 'Strong' }
})

// Validation du formulaire en temps réel
const isFormValid = computed(() => {
  return form.name.trim() && 
         form.email && 
         form.password && 
         form.confirmPassword && 
         form.poste.trim() && 
         form.agreeToTerms &&
         form.password === form.confirmPassword &&
         Object.values(passwordCriteria.value).every(Boolean) &&
         /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email) &&
         !Object.values(errors).some(error => error)
})

// Validation individuelle des champs
const validateField = (field) => {
  switch (field) {
    case 'name':
      if (!form.name.trim()) {
        errors.name = 'Full name is required'
      } else if (form.name.trim().length < 2) {
        errors.name = 'Name must be at least 2 characters'
      } else {
        errors.name = ''
      }
      break
      
    case 'email':
      if (!form.email) {
        errors.email = 'Email address is required'
      } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
        errors.email = 'Please enter a valid email address'
      } else {
        errors.email = ''
      }
      break
      
    case 'password':
      if (!form.password) {
        errors.password = 'Password is required'
      } else if (!Object.values(passwordCriteria.value).every(Boolean)) {
        errors.password = 'Password does not meet requirements'
      } else {
        errors.password = ''
      }
      break
      
    case 'confirmPassword':
      if (!form.confirmPassword) {
        errors.confirmPassword = 'Password confirmation is required'
      } else if (form.password !== form.confirmPassword) {
        errors.confirmPassword = 'Passwords do not match'
      } else {
        errors.confirmPassword = ''
      }
      break
      
    case 'poste':
      if (!form.poste.trim()) {
        errors.poste = 'Position is required'
      } else if (form.poste.trim().length < 2) {
        errors.poste = 'Position must be at least 2 characters'
      } else {
        errors.poste = ''
      }
      break
      
    case 'managerId':
      if (form.managerId !== null && form.managerId !== '' && form.managerId !== undefined) {
        const managerIdNum = Number(form.managerId)
        if (isNaN(managerIdNum) || managerIdNum <= 0) {
          errors.managerId = 'Manager ID must be a valid positive number'
        } else {
          errors.managerId = ''
        }
      } else {
        errors.managerId = ''
      }
      break
      
    case 'agreeToTerms':
      if (!form.agreeToTerms) {
        errors.agreeToTerms = 'You must agree to the terms and conditions'
      } else {
        errors.agreeToTerms = ''
      }
      break
  }
}

const clearFieldError = (field) => {
  errors[field] = ''
}

const onPasswordChange = () => {
  clearFieldError('password')
  // Revalider la confirmation si elle existe
  if (form.confirmPassword) {
    validateField('confirmPassword')
  }
}

const clearError = () => {
  apiError.value = ''
}

// Validation complète du formulaire
const validateForm = () => {
  const fields = ['name', 'email', 'password', 'confirmPassword', 'poste', 'managerId', 'agreeToTerms']
  fields.forEach(field => validateField(field))
  
  return Object.values(errors).every(error => !error)
}

const handleSignup = async () => {
  showValidationSummary.value = true
  
  if (!validateForm()) {
    // Faire défiler vers la première erreur
    const firstErrorField = Object.keys(errors).find(key => errors[key])
    if (firstErrorField) {
      const element = document.getElementById(firstErrorField)
      if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'center' })
        element.focus()
      }
    }
    return
  }
  
  isLoading.value = true
  apiError.value = ''
  
  try {
    let parsedManagerId = null
    if (form.managerId !== null && form.managerId !== '' && form.managerId !== undefined) {
      parsedManagerId = parseInt(form.managerId)
      if (isNaN(parsedManagerId)) {
        parsedManagerId = null
      }
    }
    
    const requestData = {
      Nom: form.name.trim(),
      Email: form.email.trim(),
      motDePasse: form.password,
      Poste: form.poste.trim(),
      ManagerId: parsedManagerId
    }
    
    const result = await authService.signup(requestData)
    
    if (result.success) {
      console.log('Signup successful:', result.data)
      emit('signup-success', {
        role: result.role || 'employe',
        user: result.user || result.data
      })
    } else {
      apiError.value = result.message
      console.error('Signup failed:', result.message)
    }
    
  } catch (error) {
    console.error('Signup error:', error)
    
    if (error.response) {
      const errorMessage = error.response.data?.message || 'Registration failed'
      apiError.value = errorMessage
    } else if (error.request) {
      apiError.value = 'Network error. Please check your connection and try again.'
    } else {
      apiError.value = 'An unexpected error occurred. Please try again.'
    }
  } finally {
    isLoading.value = false
  }
}

// Watch pour revalider la confirmation du password quand le password change
watch(() => form.password, () => {
  if (form.confirmPassword) {
    validateField('confirmPassword')
  }
})
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

.signup-form {
  width: 100%;
}

.form-field {
  margin-bottom: 1.5rem;
}

/* Password Strength Indicator */
.password-strength {
  margin-top: 0.5rem;
}

.strength-bar {
  width: 100%;
  height: 4px;
  background-color: #E5E7EB;
  border-radius: 2px;
  overflow: hidden;
}

.strength-fill {
  height: 100%;
  transition: width 0.3s ease, background-color 0.3s ease;
  border-radius: 2px;
}

.strength-fill.weak { background-color: #EF4444; }
.strength-fill.fair { background-color: #F59E0B; }
.strength-fill.good { background-color: #10B981; }
.strength-fill.strong { background-color: #059669; }

.strength-text {
  font-size: 0.75rem;
  font-weight: 500;
  margin: 0.25rem 0 0 0;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

.strength-text.weak { color: #EF4444; }
.strength-text.fair { color: #F59E0B; }
.strength-text.good { color: #10B981; }
.strength-text.strong { color: #059669; }

/* Password Criteria */
.password-criteria {
  margin-top: 0.75rem;
  padding: 0.75rem;
  background-color: #F9FAFB;
  border-radius: 6px;
  border: 1px solid #E5E7EB;
}

.criteria-title {
  font-size: 0.75rem;
  font-weight: 600;
  color: #374151;
  margin: 0 0 0.5rem 0;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

.criteria-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.criteria-list li {
  display: flex;
  align-items: center;
  font-size: 0.75rem;
  color: #6B7280;
  margin-bottom: 0.25rem;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

.criteria-list li.valid {
  color: #059669;
}

.criteria-icon {
  width: 14px;
  height: 14px;
  margin-right: 0.5rem;
  font-weight: bold;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Password Match Indicator */
.password-match {
  margin-top: 0.5rem;
}

.match-success, .match-error {
  display: flex;
  align-items: center;
  font-size: 0.875rem;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  margin: 0;
}

.match-success {
  color: #059669;
}

.match-error {
  color: #DC2626;
}

.match-icon {
  margin-right: 0.5rem;
  font-weight: bold;
}

/* Field Error */
.field-error {
  color: #DC2626;
  font-size: 0.875rem;
  margin: 0.25rem 0 0 0;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

/* Validation Summary */
.validation-summary {
  background-color: #FEF2F2;
  border: 1px solid #FECACA;
  border-radius: 6px;
  padding: 1rem;
  margin-bottom: 1rem;
}

.validation-summary h4 {
  color: #DC2626;
  font-size: 0.875rem;
  font-weight: 600;
  margin: 0 0 0.5rem 0;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

.validation-summary ul {
  margin: 0;
  padding-left: 1rem;
}

.validation-summary li {
  color: #B91C1C;
  font-size: 0.875rem;
  margin-bottom: 0.25rem;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
}

.form-checkbox {
  margin-bottom: 2rem;
}

.checkbox-label {
  display: flex;
  align-items: flex-start;
  cursor: pointer;
  font-size: 0.875rem;
  color: #1D1D1D;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  line-height: 1.5;
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
  margin-top: 2px;
  flex-shrink: 0;
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

.terms-link {
  color: #F6BF00;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.2s ease;
}

.terms-link:hover {
  color: #F6C900;
  text-decoration: underline;
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

.submit-btn:disabled,
.submit-btn.disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
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
  .form-checkbox {
    margin-bottom: 1.5rem;
  }
  
  .error-alert {
    padding: 0.875rem;
    gap: 0.5rem;
  }
  
  .error-title,
  .error-message {
    font-size: 0.8125rem;
  }
  
  .password-criteria {
    padding: 0.5rem;
  }
  
  .criteria-list li {
    margin-bottom: 0.125rem;
  }
}</style>