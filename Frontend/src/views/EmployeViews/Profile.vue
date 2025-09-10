<template>
  <div class="profile">
    <!-- Toast Container -->
    <div class="toast-container">
      <div 
        v-for="toast in toasts" 
        :key="toast.id" 
        :class="['toast', toast.type]"
        @click="removeToast(toast.id)"
      >
        <div class="toast-content">
          <i :class="toast.icon"></i>
          <span>{{ toast.message }}</span>
        </div>
        <button class="toast-close" @click.stop="removeToast(toast.id)">
          <i class="fas fa-times"></i>
        </button>
      </div>
    </div>

    <header class="page-header">
      <div class="header-content">
        <h1 class="page-title">Profile Settings</h1>
        <p class="page-subtitle">Manage your account and preferences</p>
      </div>
    </header>

    <!-- Loading State -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Loading profile data...</p>
    </div>

    <!-- Error State -->
    <div v-if="error && !loading" class="error-state">
      <div class="error-icon">
        <i class="fas fa-exclamation-triangle"></i>
      </div>
      <p>{{ error }}</p>
      <button @click="loadProfileData" class="retry-btn">
        <i class="fas fa-refresh"></i>
        Retry
      </button>
    </div>

    <div v-if="!loading && !error" class="profile-grid">
      <!-- Personal Information -->
      <div class="panel personal-info-panel">
        <h3 class="panel-title">Personal Information</h3>
        <div class="profile-avatar-section">
          <div class="avatar-wrapper">
            <div class="profile-avatar-initials" :class="{ 'has-image': userHasCustomAvatar }">
              <img v-if="userHasCustomAvatar" :src="userAvatar" :alt="userName" class="profile-avatar-img">
              <span v-else class="initials-text">{{ userInitials }}</span>
            </div>
            <button class="avatar-edit-btn">
              <i class="fas fa-camera"></i>
            </button>
          </div>
          <div class="avatar-info">
            <h4>{{ userName }}</h4>
            <p>{{ userRole }}</p>
            <p v-if="managerName" class="manager-info">
              <i class="fas fa-user-tie"></i>
              Manager: {{ managerName }}
            </p>
            <p v-else class="manager-info">
              <i class="fas fa-user-slash"></i>
              No manager assigned
            </p>
          </div>
        </div>
        
        <form class="profile-form" @submit.prevent="saveProfile">
          <div class="form-row">
            <div class="form-group">
              <label for="firstName">First Name</label>
              <input 
                type="text" 
                id="firstName" 
                v-model="profileForm.firstName"
                :disabled="saving"
              >
            </div>
            <div class="form-group">
              <label for="lastName">Last Name</label>
              <input 
                type="text" 
                id="lastName" 
                v-model="profileForm.lastName"
                :disabled="saving"
                required
              >
            </div>
          </div>
          
          <div class="form-group">
            <label for="email">Email Address</label>
            <input 
              type="email" 
              id="email" 
              v-model="profileForm.email"
              :disabled="saving"
              required
            >
          </div>

          <div class="form-group">
            <label for="poste">Position/Role</label>
            <input 
              type="text" 
              id="poste" 
              v-model="profileForm.poste"
              :disabled="saving"
              required
              placeholder="e.g., Software Developer, Manager"
            >
          </div>

          <div class="form-group">
            <label for="manager">Manager</label>
            <div class="manager-select-wrapper">
              <select 
                id="manager" 
                v-model="profileForm.managerId" 
                :disabled="saving || loadingManagers"
                class="manager-select"
              >
                <option value="">No Manager</option>
                <option 
                  v-for="manager in availableManagers" 
                  :key="manager.id" 
                  :value="manager.id"
                >
                  {{ manager.displayName }}
                </option>
              </select>
              <button 
                type="button" 
                @click="loadAvailableManagers" 
                class="refresh-managers-btn"
                :disabled="loadingManagers"
              >
                <i class="fas fa-refresh" :class="{ 'fa-spin': loadingManagers }"></i>
              </button>
            </div>
          </div>
          
          <button type="submit" class="save-btn" :disabled="saving">
            <i class="fas fa-save" v-if="!saving"></i>
            <i class="fas fa-spinner fa-spin" v-if="saving"></i>
            {{ saving ? 'Saving...' : 'Save Changes' }}
          </button>
        </form>
      </div>

      <!-- Vacation Balance -->
      <div class="panel vacation-balance-panel">
        <h3 class="panel-title">
          Vacation Balance
          <button @click="refreshVacationBalance" class="refresh-btn" :disabled="refreshingBalance">
            <i class="fas fa-refresh" :class="{ 'fa-spin': refreshingBalance }"></i>
          </button>
        </h3>
        <div class="balance-overview">
          <div class="balance-card primary">
            <div class="balance-icon">
              <i class="fas fa-calendar-alt"></i>
            </div>
            <div class="balance-content">
              <div class="balance-number">{{ currentYearBalance.joursRestants || 0 }}</div>
              <div class="balance-label">Available This Year</div>
            </div>
          </div>
          
          <div class="balance-card secondary">
            <div class="balance-icon">
              <i class="fas fa-calendar-check"></i>
            </div>
            <div class="balance-content">
              <div class="balance-number">{{ 30 - (currentYearBalance.joursRestants || 0) }}</div>
              <div class="balance-label">Days Used</div>
            </div>
          </div>
          
          <div class="balance-card accent">
            <div class="balance-icon">
              <i class="fas fa-calendar-plus"></i>
            </div>
            <div class="balance-content">
              <div class="balance-number">{{ nextYearBalance.joursRestants || 0 }}</div>
              <div class="balance-label">Available Next Year</div>
            </div>
          </div>
        </div>
        
        <div class="progress-section">
          <div class="progress-label">
            <span>Vacation Usage ({{ currentYear }})</span>
            <span>{{ vacationUsagePercentage }}%</span>
          </div>
          <div class="progress-bar">
            <div class="progress-fill" :style="{ width: vacationUsagePercentage + '%' }"></div>
          </div>
        </div>
        
        <div class="balance-year">
          <span>Current Year: {{ currentYear }} | Next Year: {{ nextYear }}</span>
        </div>
      </div>

      <!-- Preferences -->
      <div class="panel preferences-panel">
        <h3 class="panel-title">Preferences</h3>
        <div class="preferences-form">
          <div class="preference-item">
            <label class="preference-label">
              <span>Email Notifications</span>
              <label class="toggle-switch">
                <input type="checkbox" v-model="preferences.emailNotifications">
                <span class="toggle-slider"></span>
              </label>
            </label>
          </div>
          
          <div class="preference-item">
            <label class="preference-label">
              <span>Desktop Notifications</span>
              <label class="toggle-switch">
                <input type="checkbox" v-model="preferences.desktopNotifications">
                <span class="toggle-slider"></span>
              </label>
            </label>
          </div>
          
          <div class="preference-item">
            <label class="preference-label">
              <span>Weekly Summary</span>
              <label class="toggle-switch">
                <input type="checkbox" v-model="preferences.weeklySummary">
                <span class="toggle-slider"></span>
              </label>
            </label>
          </div>
          
          <div class="form-group">
            <label for="timezone">Timezone</label>
            <select id="timezone" v-model="preferences.timezone">
              <option value="UTC-8">Pacific Time (UTC-8)</option>
              <option value="UTC-7">Mountain Time (UTC-7)</option>
              <option value="UTC-6">Central Time (UTC-6)</option>
              <option value="UTC-5">Eastern Time (UTC-5)</option>
            </select>
          </div>
          
          <div class="form-group">
            <label for="language">Language</label>
            <select id="language" v-model="preferences.language">
              <option value="en">English</option>
              <option value="fr">Français</option>
              <option value="es">Español</option>
              <option value="de">Deutsch</option>
            </select>
          </div>
          
          <button type="button" class="save-btn" @click="savePreferences" :disabled="savingPreferences">
            <i class="fas fa-save" v-if="!savingPreferences"></i>
            <i class="fas fa-spinner fa-spin" v-if="savingPreferences"></i>
            {{ savingPreferences ? 'Saving...' : 'Save Preferences' }}
          </button>
        </div>
      </div>

      <!-- Security Settings -->
      <div class="panel security-panel">
        <h3 class="panel-title">Security Settings</h3>
        <div class="security-options">
          <div class="security-item">
            <div class="security-info">
              <h4>Change Password</h4>
              <p>Update your password to keep your account secure</p>
            </div>
            <button class="security-btn" @click="changePassword">
              <i class="fas fa-key"></i>
              Change
            </button>
          </div>
          
          <div class="security-item">
            <div class="security-info">
              <h4>Two-Factor Authentication</h4>
              <p>Add an extra layer of security to your account</p>
            </div>
            <button class="security-btn enabled" @click="manageTwoFactor">
              <i class="fas fa-shield-alt"></i>
              Enabled
            </button>
          </div>
          
          <div class="security-item">
            <div class="security-info">
              <h4>Login Sessions</h4>
              <p>Manage active sessions and devices</p>
            </div>
            <button class="security-btn" @click="manageSessionsFn">
              <i class="fas fa-laptop"></i>
              Manage
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { userService } from '../../services/UserService.js'
import { vacationService } from '../../services/vacationService.js'

export default {
  name: 'Profile',  
  props: {
    user: {
      type: Object,
      default: () => ({})
    }
  },
  data() {
    return {
      loading: false,
      saving: false,
      savingPreferences: false,
      refreshingBalance: false,
      loadingManagers: false,
      profileForm: {
        firstName: '',
        lastName: '',
        email: '',
        poste: '',
        managerId: null
      },
      availableManagers: [],
      currentYearBalance: {
        joursTotal: 30,
        joursUtilises: 0,
        joursRestants: 30
      },
      nextYearBalance: {
        joursTotal: 30,
        joursUtilises: 0,
        joursRestants: 30
      },
      preferences: {
        emailNotifications: true,
        desktopNotifications: false,
        weeklySummary: true,
        timezone: 'UTC-8',
        language: 'en'
      },
      error: null,
      currentYear: new Date().getFullYear(),
      nextYear: new Date().getFullYear() + 1,
      userSubscription: null,
      managerDisplayName: null,
      userRequests: [],
      // Toast system
      toasts: [],
      toastIdCounter: 0
    }
  },
  computed: {
    userName() {
      const currentUser = userService.getCurrentUser()
      if (currentUser) {
        return userService.formatUserDisplayName(currentUser)
      }
      return this.user?.name || `${this.profileForm.firstName} ${this.profileForm.lastName}` || 'Unknown User'
    },
    userRole() {
      const currentUser = userService.getCurrentUser()
      return currentUser?.Poste || currentUser?.poste || this.user?.role || 'Employee'
    },
    userInitials() {
      const currentUser = userService.getCurrentUser()
      if (currentUser) {
        return userService.getUserInitials()
      }
      
      const firstName = this.profileForm.firstName || 'U'
      const lastName = this.profileForm.lastName || ''
      
      if (lastName) {
        return (firstName.charAt(0) + lastName.charAt(0)).toUpperCase()
      }
      return firstName.charAt(0).toUpperCase()
    },
    userHasCustomAvatar() {
      return this.user?.avatar && this.user.avatar !== 'default'
    },
    userAvatar() {
      return this.user?.avatar || null
    },
    managerName() {
      return this.managerDisplayName
    },
    calculatedDaysUsed() {
      const available = this.currentYearBalance.joursRestants || 0
      const used = 30 - available
      return Math.max(0, used)
    },
    vacationUsagePercentage() {
      const used = this.calculatedDaysUsed
      const percentage = Math.round((used / 30) * 100)
      return Math.min(100, Math.max(0, percentage))
    }
  },
  async mounted() {
    this.userSubscription = userService.subscribe((userData) => {
      this.populateFormFromUserData(userData)
    })

    await this.loadProfileData()
    await this.loadAvailableManagers()
  },
  beforeUnmount() {
    if (this.userSubscription) {
      this.userSubscription()
    }
  },
  methods: {
    // Toast methods
    showToast(message, type = 'success') {
      const toast = {
        id: ++this.toastIdCounter,
        message,
        type,
        icon: type === 'success' ? 'fas fa-check-circle' : 'fas fa-exclamation-circle'
      }
      
      this.toasts.push(toast)
      
      // Auto-remove toast after 4 seconds
      setTimeout(() => {
        this.removeToast(toast.id)
      }, 4000)
    },

    removeToast(toastId) {
      const index = this.toasts.findIndex(toast => toast.id === toastId)
      if (index > -1) {
        this.toasts.splice(index, 1)
      }
    },

    async loadProfileData() {
      this.loading = true
      this.error = null

      try {
        const currentUser = userService.getCurrentUser()
        if (currentUser) {
          this.populateFormFromUserData(currentUser)
          await this.loadManagerInfo(currentUser)
        } else if (this.user) {
          this.populateFormFromUserData(this.user)
        }

        await this.loadVacationBalances()
        await this.loadUserVacationRequests()

      } catch (error) {
        this.error = 'Failed to load profile data. Please try again.'
      } finally {
        this.loading = false
      }
    },

    // Replace the loadAvailableManagers method in your Vue component with this:

async loadAvailableManagers() {
  this.loadingManagers = true
  try {
    
    // Use the existing UserService method instead of direct API call
    const result = await userService.getAvailableManagers()
    
    if (result.success && result.data) {
      this.availableManagers = result.data
    } else {
      this.availableManagers = []
    }
  } catch (error) {
    this.availableManagers = []
  } finally {
    this.loadingManagers = false
  }
},

    async loadVacationBalances() {
      try {
        
        const currentYearResult = await vacationService.getVacationBalance(this.currentYear)
        if (currentYearResult.success && currentYearResult.data) {
          this.currentYearBalance = {
            joursTotal: currentYearResult.data.joursTotal || 30,
            joursUtilises: currentYearResult.data.joursUtilises || 0,
            joursRestants: currentYearResult.data.joursRestants || 30
          }
          
        } else {
          if (currentYearResult.message && currentYearResult.message.includes('Session')) {
            this.error = currentYearResult.message
            return
          }
          this.currentYearBalance = {
            joursTotal: 30,
            joursUtilises: 0,
            joursRestants: 30
          }
        }

        const nextYearResult = await vacationService.getVacationBalance(this.nextYear)
        if (nextYearResult.success && nextYearResult.data) {
          this.nextYearBalance = {
            joursTotal: nextYearResult.data.joursTotal || 30,
            joursUtilises: nextYearResult.data.joursUtilises || 0,
            joursRestants: nextYearResult.data.joursRestants || 30
          }
        } else {
          this.nextYearBalance = {
            joursTotal: 30,
            joursUtilises: 0,
            joursRestants: 30
          }
        }
        
      } catch (error) {
        this.error = 'Failed to load vacation data. Please try again.'
      }
    },

    async loadUserVacationRequests() {
      try {
        const requestsResult = await vacationService.getUserVacationRequests()
        
        if (requestsResult.success && requestsResult.data) {
          this.userRequests = requestsResult.data
        } else {
          this.userRequests = []
        }
      } catch (error) {
        this.userRequests = []
      }
    },

    async refreshVacationBalance() {
      this.refreshingBalance = true
      try {
        await this.loadVacationBalances()
      } catch (error) {
        this.error = 'Failed to refresh vacation data.'
      } finally {
        this.refreshingBalance = false
      }
    },

    async loadManagerInfo(userData) {
      try {
        
        const currentUserId = userService.getUserId() || userData.id || userData.Id
        
        if (currentUserId) {
          const freshUserResult = await userService.getUserById(currentUserId)
          
          if (freshUserResult.success && freshUserResult.data) {
            const freshUserData = freshUserResult.data
            
            if (freshUserData.manager && freshUserData.manager.nom) {
              this.managerDisplayName = freshUserData.manager.nom
              return
            }
            
            const managerId = freshUserData.managerId || freshUserData.ManagerId
            if (managerId) {
              const managerResult = await userService.getUserById(managerId)
              
              if (managerResult.success && managerResult.data) {
                this.managerDisplayName = userService.formatUserDisplayName(managerResult.data)
              }
            }
          }
        } else {
          this.managerDisplayName = null
        }
      } catch (error) {
        this.managerDisplayName = 'Error Loading Manager'
      }
    },

    populateFormFromUserData(userData) {
  if (!userData) return

  const email = userData.email || userData.Email || ''
  const poste = userData.poste || userData.Poste || ''
  
  // Handle manager ID - be careful with the field name
  const managerId = userData.managerId || userData.ManagerId || null
  
  // Get the nom field from database
  const nom = userData.nom || userData.Nom || userData.name || ''
  
  // Check if nom contains multiple words (first name + last name)
  if (nom && nom.includes(' ')) {
    const nameParts = nom.trim().split(' ')
    this.profileForm.firstName = nameParts[0] || ''
    this.profileForm.lastName = nameParts.slice(1).join(' ') || ''
  } else {
    // If only one word, put it in lastName
    this.profileForm.firstName = ''
    this.profileForm.lastName = nom
  }

  this.profileForm.email = email
  this.profileForm.poste = poste
  this.profileForm.managerId = managerId

},

  async saveProfile() {
  this.saving = true
  this.error = null

  try {
    const currentUser = userService.getCurrentUser()
    const userId = userService.getUserId() || currentUser?.Id || currentUser?.id
    
    if (!userId) {
      throw new Error('User ID not found')
    }

    // Store the selected manager ID before API call
    const selectedManagerId = this.profileForm.managerId

    // Prepare the data to match your UpdateUserDto
    const updateData = {
      nom: userService.combineUserName(this.profileForm.firstName, this.profileForm.lastName),
      email: this.profileForm.email,
      role: currentUser?.Role || currentUser?.role || 'Employe',
      poste: this.profileForm.poste || '',
      managerId: selectedManagerId
    }

    // Validate the data before sending
    const validation = userService.validateUpdateData(updateData)
    if (!validation.isValid) {
      this.showToast(validation.errors.join(', '), 'error')
      return
    }

    // Use the enhanced updateUserProfile method
    const result = await userService.updateUserProfile(userId, updateData)

    if (result.success) {
      // Wait for the backend to fully process the change
      await new Promise(resolve => setTimeout(resolve, 500))
      
      // Clear cache to force fresh data fetch
      userService.clearCache()
      
      // Fetch completely fresh user data
      const freshUserResult = await userService.getUserById(userId)
      if (freshUserResult.success) {
        
        // Update the current user data
        userService.setCurrentUser(freshUserResult.data)
        
        // Re-populate form with fresh data (this should now have the correct manager)
        this.populateFormFromUserData(freshUserResult.data)
        
        // Ensure the manager ID is preserved if the fresh data is still wrong
        if (this.profileForm.managerId !== selectedManagerId) {
          this.profileForm.managerId = selectedManagerId
        }
      }
      
      // Reload manager info to reflect changes
      await this.loadManagerInfo(freshUserResult.data || currentUser)
      
      this.showToast('Profile updated successfully!')
    } else {
      // Handle specific error cases
      if (result.status === 401) {
        this.handleAuthError()
      } else {
        this.showToast(result.message || 'Failed to update profile', 'error')
      }
    }
    
  } catch (error) {
    this.showToast(error.message || 'Failed to save profile. Please try again.', 'error')
  } finally {
    this.saving = false
  }
}
,

// Also update your updateUserProfile method in UserService to handle managerId properly:

async updateUserProfile(userId, updateData) {
  
  // Validate required fields before sending
  if (!updateData.nom || !updateData.email) {
    return {
      success: false,
      message: 'Name and email are required fields'
    }
  }

  // Handle managerId conversion - ensure it's a number or null
  let managerId = updateData.managerId
  if (managerId === '' || managerId === 'null' || managerId === undefined) {
    managerId = null
  } else if (managerId && typeof managerId === 'string') {
    // Convert string to number if needed
    const parsed = parseInt(managerId, 10)
    managerId = isNaN(parsed) ? null : parsed
  }

  // Prepare the data to match your UpdateUserDto exactly
  const payload = {
    nom: updateData.nom.trim(),
    email: updateData.email.trim(),
    role: updateData.role || 'Employe',
    poste: updateData.poste?.trim() || '',
    managerId: managerId // Use the properly formatted manager ID
  }


  const result = await this.makeRequest(
    `${this.apiBaseUrl}/User/${userId}`,
    {
      method: 'PUT',
      body: JSON.stringify(payload)
    }
  )

  if (result.success) {
   
    // Update cache with new data
    if (result.data?.data) {
      this.userCache.set(userId, result.data.data)
    }
    
    // If updating current user, refresh local data
    if (this.currentUser && (this.currentUser.Id === userId || this.currentUser.id === userId)) {
      await this.refreshCurrentUserData()
    }
    
    return {
      success: true,
      data: result.data,
      message: 'Profile updated successfully'
    }
  } else {
    return {
      success: false,
      message: result.message || 'Failed to update profile',
      status: result.status
    }
  }
},

    async savePreferences() {
      this.savingPreferences = true
      
      try {
    
        // Here you would typically send the preferences to your backend
        await new Promise(resolve => setTimeout(resolve, 800))
        
        this.showSuccessMessage('These functionnalities can be added in a future update')
        
      } catch (error) {
        this.error = 'Failed to save preferences. Please try again.'
      } finally {
        this.savingPreferences = false
      }
    },

    changePassword() {
      this.showInfoMessage('This can be added in a future update')
    },

    manageTwoFactor() {
      this.showInfoMessage('This can be added in a future update')
    },

    manageSessionsFn() {
      this.showInfoMessage('This can be added in a future update')
    },

    showSuccessMessage(message) {
      alert(message)
    },

    showInfoMessage(message) {
      alert(message)
    },

    handleAuthError() {
      this.error = 'Session expired. Please login again.'
      this.$emit('auth-expired')
    }
  }
}
</script>

<style scoped>
.profile {
  max-width: 1400px;
  margin: 0 auto;
}

/* Toast Styles */
.toast-container {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  pointer-events: none;
}

.toast {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.25rem;
  border-radius: 12px;
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
  backdrop-filter: blur(10px);
  pointer-events: auto;
  cursor: pointer;
  min-width: 300px;
  max-width: 400px;
  animation: slideInRight 0.3s ease-out;
  transition: all 0.3s ease;
}

.toast:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 35px rgba(0, 0, 0, 0.2);
}

.toast.success {
  background: rgba(34, 197, 94, 0.95);
  border: 1px solid rgba(34, 197, 94, 0.3);
  color: white;
}

.toast.error {
  background: rgba(239, 68, 68, 0.95);
  border: 1px solid rgba(239, 68, 68, 0.3);
  color: white;
}

.toast-content {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex: 1;
}

.toast-content i {
  font-size: 1.1rem;
}

.toast-close {
  background: none;
  border: none;
  color: inherit;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 4px;
  opacity: 0.8;
  transition: opacity 0.2s ease;
}

.toast-close:hover {
  opacity: 1;
  background: rgba(255, 255, 255, 0.1);
}

@keyframes slideInRight {
  from {
    opacity: 0;
    transform: translateX(100%);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

.page-header {
  margin-bottom: 2rem;
}

.page-title {
  font-size: 2.25rem;
  font-weight: 700;
  color: #1D1D1D;
  margin-bottom: 0.5rem;
}

.page-subtitle {
  color: rgba(29, 29, 29, 0.6);
  font-size: 1.1rem;
}

/* Loading and Error States */
.loading-state, .error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  text-align: center;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(246, 201, 0, 0.2);
  border-top: 4px solid #F6C900;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-state {
  color: #ef4444;
}

.error-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.retry-btn {
  margin-top: 1rem;
  padding: 0.75rem 1.5rem;
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: background-color 0.3s ease;
}

.retry-btn:hover {
  background: #dc2626;
}

.profile-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-template-rows: auto auto;
  gap: 2rem;
}

.personal-info-panel {
  grid-column: 1 / 2;
  grid-row: 1 / 3;
}

.vacation-balance-panel {
  grid-column: 2 / 3;
  grid-row: 1 / 2;
}

.preferences-panel {
  grid-column: 2 / 3;
  grid-row: 2 / 3;
}

.security-panel {
  grid-column: 1 / 3;
  grid-row: 3 / 4;
}

.panel {
  background: #FFFFFF;
  border-radius: 24px;
  padding: 2rem;
  box-shadow: 0 4px 20px rgba(29, 29, 29, 0.08);
  border: 1px solid rgba(246, 201, 0, 0.1);
}

.panel-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 1.5rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.refresh-btn {
  background: none;
  border: none;
  color: #F6C900;
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 50%;
  transition: all 0.3s ease;
}

.refresh-btn:hover {
  background: rgba(246, 201, 0, 0.1);
}

.refresh-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.profile-avatar-section {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  margin-bottom: 2rem;
  padding: 1.5rem;
  background: rgba(246, 201, 0, 0.03);
  border-radius: 16px;
  border: 1px solid rgba(246, 201, 0, 0.1);
}

.avatar-wrapper {
  position: relative;
}

.profile-avatar-initials {
  width: 80px;
  height: 80px;
  border-radius: 16px;
  border: 3px solid rgba(246, 201, 0, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #F6C900 0%, #F6BF00 100%);
  color: #1D1D1D;
  font-weight: 700;
  font-size: 1.5rem;
  position: relative;
  overflow: hidden;
}

.profile-avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 13px;
}

.initials-text {
  user-select: none;
}

.manager-info {
  color: rgba(29, 29, 29, 0.7);
  font-size: 0.9rem;
  margin-top: 0.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.avatar-edit-btn {
  position: absolute;
  bottom: -5px;
  right: -5px;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: linear-gradient(135deg, #F6C900 0%, #F6BF00 100%);
  border: none;
  color: #1D1D1D;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 10px rgba(246, 201, 0, 0.3);
  transition: all 0.3s ease;
}

.avatar-edit-btn:hover {
  transform: scale(1.1);
  box-shadow: 0 4px 15px rgba(246, 201, 0, 0.4);
}

.avatar-info h4 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 0.25rem;
}

.avatar-info p {
  color: rgba(29, 29, 29, 0.6);
  font-size: 1rem;
}

.profile-form {
  space-y: 1.5rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #1D1D1D;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid rgba(246, 201, 0, 0.2);
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.3s ease;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: #F6C900;
  box-shadow: 0 0 0 3px rgba(246, 201, 0, 0.1);
}

.manager-select-wrapper {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.manager-select {
  flex: 1;
}

.refresh-managers-btn {
  padding: 0.75rem;
  background: rgba(246, 201, 0, 0.1);
  border: 1px solid rgba(246, 201, 0, 0.2);
  border-radius: 8px;
  color: #F6C900;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s ease;
  min-width: 48px;
}

.refresh-managers-btn:hover:not(:disabled) {
  background: rgba(246, 201, 0, 0.2);
}

.refresh-managers-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.save-btn {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #F6C900 0%, #F6BF00 100%);
  color: #1D1D1D;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 15px rgba(246, 201, 0, 0.3);
}

.save-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 8px 25px rgba(246, 201, 0, 0.4);
}

.save-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.balance-overview {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
  margin-bottom: 2rem;
}

.balance-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1.25rem;
  border-radius: 12px;
  border: 1px solid rgba(246, 201, 0, 0.1);
}

.balance-card.primary {
  background: rgba(246, 201, 0, 0.05);
}

.balance-card.secondary {
  background: rgba(246, 191, 0, 0.05);
}

.balance-card.accent {
  background: rgba(238, 238, 238, 0.5);
}

.balance-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  background: linear-gradient(135deg, #F6C900 0%, #F6BF00 100%);
  color: #1D1D1D;
}

.balance-number {
  font-size: 2rem;
  font-weight: 700;
  color: #1D1D1D;
  line-height: 1;
  margin-bottom: 0.25rem;
}

.balance-label {
  color: rgba(29, 29, 29, 0.6);
  font-size: 0.875rem;
  font-weight: 500;
}

.progress-section {
  background: rgba(246, 201, 0, 0.03);
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 12px;
  padding: 1.25rem;
}

.progress-label {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
  font-weight: 500;
  color: #1D1D1D;
}

.progress-bar {
  width: 100%;
  height: 8px;
  background: rgba(29, 29, 29, 0.1);
  border-radius: 4px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #F6C900 0%, #F6BF00 100%);
  border-radius: 4px;
  transition: width 0.3s ease;
}

.balance-year {
  margin-top: 1rem;
  text-align: center;
  color: rgba(29, 29, 29, 0.6);
  font-size: 0.875rem;
}

.preferences-form {
  space-y: 1.5rem;
}

.preference-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem;
  background: rgba(246, 201, 0, 0.03);
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 12px;
  margin-bottom: 1rem;
}

.preference-label {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  cursor: pointer;
  font-weight: 500;
  color: #1D1D1D;
}

.toggle-switch {
  position: relative;
  width: 50px;
  height: 24px;
}

.toggle-switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.toggle-slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(29, 29, 29, 0.2);
  transition: 0.3s;
  border-radius: 24px;
}

.toggle-slider:before {
  position: absolute;
  content: "";
  height: 18px;
  width: 18px;
  left: 3px;
  bottom: 3px;
  background-color: white;
  transition: 0.3s;
  border-radius: 50%;
}

input:checked + .toggle-slider {
  background: linear-gradient(90deg, #F6C900 0%, #F6BF00 100%);
}

input:checked + .toggle-slider:before {
  transform: translateX(26px);
}

.security-options {
  space-y: 1rem;
}

.security-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem;
  background: rgba(246, 201, 0, 0.03);
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 12px;
  margin-bottom: 1rem;
}

.security-info h4 {
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 0.25rem;
}

.security-info p {
  color: rgba(29, 29, 29, 0.6);
  font-size: 0.875rem;
}

.security-btn {
  padding: 0.5rem 1rem;
  border: 1px solid rgba(246, 201, 0, 0.2);
  border-radius: 8px;
  background: rgba(246, 201, 0, 0.1);
  color: #1D1D1D;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
  font-weight: 500;
}

.security-btn:hover {
  background: rgba(246, 201, 0, 0.2);
}

.security-btn.enabled {
  background: rgba(74, 222, 128, 0.1);
  border-color: rgba(74, 222, 128, 0.2);
  color: #16A34A;
}

@media (max-width: 1024px) {
  .profile-grid {
    grid-template-columns: 1fr;
    grid-template-rows: auto;
  }
  
  .personal-info-panel,
  .vacation-balance-panel,
  .preferences-panel,
  .security-panel {
    grid-column: 1;
    grid-row: auto;
  }
  
  .toast-container {
    right: 10px;
    left: 10px;
  }
  
  .toast {
    min-width: auto;
    max-width: none;
  }
}

@media (max-width: 768px) {
  .form-row {
    grid-template-columns: 1fr;
  }
  
  .profile-avatar-section {
    flex-direction: column;
    text-align: center;
  }
  
  .security-item {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
  
  .panel-title {
    flex-direction: column;
    gap: 0.5rem;
    align-items: flex-start;
  }
  
  .balance-overview {
    gap: 0.75rem;
  }
  
  .balance-card {
    padding: 1rem;
  }
  
  .balance-number {
    font-size: 1.5rem;
  }
  
  .manager-select-wrapper {
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .refresh-managers-btn {
    width: 100%;
  }
}

@media (max-width: 480px) {
  .panel {
    padding: 1.5rem;
  }
  
  .page-title {
    font-size: 1.75rem;
  }
  
  .profile-avatar-section {
    padding: 1rem;
  }
  
  .profile-avatar-initials {
    width: 60px;
    height: 60px;
    font-size: 1.25rem;
  }
  
  .avatar-edit-btn {
    width: 24px;
    height: 24px;
    bottom: -3px;
    right: -3px;
  }
  
  .toast-container {
    top: 10px;
    right: 10px;
    left: 10px;
  }
}
</style>