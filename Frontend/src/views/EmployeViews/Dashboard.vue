<template>
  <div class="dashboard">
    <header class="dashboard-header">
      <div class="welcome-section">
        <h1 class="welcome-title">Welcome back, {{ displayName }}</h1>
        <p class="welcome-subtitle">Here's what's happening with your team today</p>
      </div>
      <div class="date-display">
        <div class="date-main">{{ formatDate(new Date()) }}</div>
        <div class="date-sub">{{ formatTime(new Date()) }}</div>
      </div>
    </header>

    <div class="dashboard-grid">
      <div class="panel quick-actions floating-animation">
        <h3 class="panel-title">Quick Actions</h3>
        <div class="actions-grid">
          <button class="action-btn primary" @click="openVacationModal">
            <i class="fas fa-plus"></i>
            <span>Request Vacation</span>
          </button>
          <button class="action-btn secondary" @click="openExpenseModal">
            <i class="fas fa-receipt"></i>
            <span>Add Expense</span>
          </button>
        </div>
      </div>

      <div class="panel stats-panel pulse-animation">
        <div class="stats-grid">
          <div class="stat-card">
            <div class="stat-icon primary">
              <i class="fas fa-calendar-check"></i>
            </div>
            <div class="stat-content">
              <div class="stat-number">{{ stats.vacationDays }}</div>
              <div class="stat-label">Days Off Used</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon secondary">
              <i class="fas fa-clock"></i>
            </div>
            <div class="stat-content">
              <div class="stat-number">{{ stats.pendingRequests }}</div>
              <div class="stat-label">Pending Requests</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon success">
              <i class="fas fa-money-bill-wave"></i>
            </div>
            <div class="stat-content">
              <div class="stat-number">${{ stats.expenses }}</div>
              <div class="stat-label">This Month</div>
            </div>
          </div>
        </div>
      </div>

      <div class="panel activity-panel">
        <h3 class="panel-title">Recent Activity (This can be added in a future update )</h3>
        <div class="activity-list">
          <div v-for="activity in recentActivity" :key="activity.id" class="activity-item">
            <div class="activity-icon" :class="activity.type">
              <i :class="activity.icon"></i>
            </div>
            <div class="activity-content">
              <div class="activity-title">{{ activity.title }}</div>
              <div class="activity-time">{{ activity.time }}</div>
            </div>
            <div class="activity-status" :class="activity.status">
              {{ activity.statusText }}
            </div>
          </div>
        </div>
      </div>

      <div class="panel team-panel" v-if="isManager">
        <h3 class="panel-title">Team Overview</h3>
        <div class="team-grid">
          <div v-for="member in teamMembers" :key="member.id" class="team-member">
            <div class="member-avatar" :style="generateAvatarStyle(member.name)">
              {{ getInitials(member.name) }}
            </div>
            <div class="member-info">
              <div class="member-name">{{ member.name }}</div>
              <div class="member-role">{{ member.role }}</div>
            </div>
            <div class="member-status online">
              <i class="fas fa-circle"></i>
            </div>
          </div>
        </div>
      </div>

      <div class="panel profile-panel" v-if="!isManager">
        <h3 class="panel-title">Your Profile</h3>
        <div class="profile-content">
          <div class="profile-avatar" :style="userAvatarStyle">
            {{ userInitials }}
          </div>
          <div class="profile-info">
            <h4>{{ displayName }}</h4>
            <p>{{ userPoste }}</p>
            <p class="user-email">{{ userEmail }}</p>
            <div class="profile-stats">
              <div class="profile-stat">
                <span class="stat-value">{{ userRole }}</span>
                <span class="stat-label">Role</span>
              </div>
              <div class="profile-stat" v-if="managerId">
                <span class="stat-value">Yes</span>
                <span class="stat-label">Has Manager</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Vacation Modal -->
    <div v-if="showVacationModal" class="modal-overlay" @click="closeVacationModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>New Vacation Request</h2>
          <button class="close-btn" @click="closeVacationModal">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <form @submit.prevent="submitVacationRequest" class="modal-body">
          <div class="form-group">
            <label for="vacationType">Type of Leave</label>
            <select id="vacationType" v-model="vacationRequest.type" required>
              <option value="CongeAnnuel">Annual Leave</option>
              <option value="Maladie">Sick Leave</option>
              <option value="Maternite">Maternity Leave</option>
              <option value="Paternite">Paternity Leave</option>
              <option value="DecesProche">Bereavement Leave</option>
            </select>
          </div>
          
          <div class="form-row">
            <div class="form-group">
              <label for="startDate">Start Date</label>
              <input 
                type="date" 
                id="startDate" 
                v-model="vacationRequest.dateDebut" 
                required
                :min="minDate"
              >
            </div>
            <div class="form-group">
              <label for="endDate">End Date</label>
              <input 
                type="date" 
                id="endDate" 
                v-model="vacationRequest.dateFin" 
                required
                :min="vacationRequest.dateDebut || minDate"
              >
            </div>
          </div>

          <div class="duration-info" v-if="vacationRequest.dateDebut && vacationRequest.dateFin">
            <p>Duration: {{ calculateDuration(vacationRequest.dateDebut, vacationRequest.dateFin) }} days</p>
          </div>

          <div class="form-group">
            <label for="reason">Comment</label>
            <textarea 
              id="reason" 
              v-model="vacationRequest.commentaire" 
              rows="3" 
              placeholder="Add a comment to your request..."
            ></textarea>
          </div>

          <div class="validation-errors" v-if="validationErrors.length > 0">
            <div v-for="error in validationErrors" :key="error" class="error-message">
              <i class="fas fa-exclamation-triangle"></i>
              {{ error }}
            </div>
          </div>
          
          <div class="modal-actions">
            <button type="button" class="cancel-btn" @click="closeVacationModal">Cancel</button>
            <button type="submit" class="submit-btn" :disabled="submitting">
              {{ submitting ? 'Submitting...' : 'Submit Request' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Enhanced Expense Modal -->
    <div v-if="showExpenseModal" class="modal-overlay" @click="closeExpenseModal">
      <div class="modal-content expense-modal" @click.stop>
        <div class="modal-header">
          <h2>New Expense Report</h2>
          <button class="close-btn" @click="closeExpenseModal">
            <i class="fas fa-times"></i>
          </button>
        </div>
        
        <form @submit.prevent="submitExpenseReport" class="modal-body">
          <!-- General form error -->
          <div v-if="error.form" class="alert alert-error">
            <i class="fas fa-exclamation-circle"></i>
            <div class="alert-content">
              <div class="alert-message">{{ error.form }}</div>
              <button v-if="retryCount > 0 && retryCount < maxRetries" type="button" class="retry-btn" @click="submitExpenseReport">
                <i class="fas fa-redo"></i>
                Retry ({{ retryCount }}/{{ maxRetries }})
              </button>
            </div>
          </div>

          <!-- Server validation errors -->
          <div v-if="serverErrors.length > 0" class="alert alert-error">
            <i class="fas fa-server"></i>
            <div class="alert-content">
              <div class="error-title">Server Validation Errors:</div>
              <ul class="error-list">
                <li v-for="error in serverErrors" :key="error">{{ error }}</li>
              </ul>
            </div>
          </div>

          <!-- General form errors -->
          <div v-if="formErrors.general" class="alert alert-error">
            <i class="fas fa-exclamation-triangle"></i>
            {{ formErrors.general }}
          </div>

          <!-- Form validation summary -->
          <div v-if="hasFormErrors && !error.form" class="validation-summary">
            <i class="fas fa-exclamation-triangle"></i>
            <span>Please fix {{ Object.keys(formErrors).length }} error(s) before submitting</span>
          </div>

          <!-- Project Selection -->
          <div class="form-group">
            <label for="project">Project</label>
            <select id="project" v-model="newExpenseReport.projetId">
              <option value="">Select a project (optional)</option>
              <option v-for="project in projects" :key="project.id" :value="project.id">
                {{ project.nom }}
              </option>
            </select>
            <div v-if="loading.projects" class="form-loading">
              <i class="fas fa-spinner fa-spin"></i>
              Loading projects...
            </div>
          </div>

          <!-- Expense Lines Section -->
          <div class="expense-lines-section">
            <div class="section-header">
              <h4>Expense Lines</h4>
              <button type="button" class="add-line-btn" @click="addExpenseLine" :disabled="isSubmitting">
                <i class="fas fa-plus"></i>
                Add Line
              </button>
            </div>

            <div v-for="(line, index) in newExpenseReport.lignes" :key="index" 
                 class="expense-line-form" 
                 :class="{ 'has-errors': formErrors[`line_${index}`] }">
              
              <div class="line-form-header">
                <span>Expense #{{ index + 1 }}</span>
                <button 
                  type="button" 
                  class="remove-line-btn" 
                  @click="removeLine(index)" 
                  v-if="newExpenseReport.lignes.length > 1"
                  :disabled="isSubmitting"
                >
                  <i class="fas fa-trash"></i>
                </button>
              </div>

              <!-- Line-specific error summary -->
              <div v-if="formErrors[`line_${index}`] && Object.keys(formErrors[`line_${index}`]).length > 0" 
                   class="line-errors-summary">
                <i class="fas fa-exclamation-triangle"></i>
                <span>This expense line has {{ Object.keys(formErrors[`line_${index}`]).length }} error(s)</span>
              </div>

              <!-- Date and Amount -->
              <div class="form-row">
                <div class="form-group">
                  <label>Date *</label>
                  <input 
                    type="date" 
                    v-model="line.date" 
                    required
                    :max="getCurrentDate()"
                    :class="{ 'error': getLineError(index, 'date') }"
                    @blur="validateField(index, 'date')"
                    @input="clearLineError(index, 'date')"
                  >
                  <div v-if="getLineError(index, 'date')" class="field-error">
                    <i class="fas fa-exclamation-circle"></i>
                    {{ getLineError(index, 'date') }}
                  </div>
                </div>
                
                <div class="form-group">
                  <label>Amount *</label>
                  <div class="amount-input-wrapper">
                    <span class="currency-symbol">$</span>
                    <input 
                      type="number" 
                      step="0.01" 
                      min="0"
                      max="50000"
                      v-model="line.montant" 
                      required
                      placeholder="0.00"
                      :class="{ 'error': getLineError(index, 'montant') }"
                      @blur="validateField(index, 'montant')"
                      @input="clearLineError(index, 'montant')"
                    >
                  </div>
                  <div v-if="getLineError(index, 'montant')" class="field-error">
                    <i class="fas fa-exclamation-circle"></i>
                    {{ getLineError(index, 'montant') }}
                  </div>
                </div>
              </div>

              <!-- Description -->
              <div class="form-group">
                <label>Description *</label>
                <div class="description-input-wrapper">
                  <input 
                    type="text" 
                    v-model="line.description" 
                    placeholder="Describe the expense..." 
                    required
                    maxlength="500"
                    :class="{ 'error': getLineError(index, 'description') }"
                    @blur="validateField(index, 'description')"
                    @input="clearLineError(index, 'description')"
                  >
                  <div class="character-count" :class="{ 'warning': line.description && line.description.length > 450 }">
                    {{ line.description ? line.description.length : 0 }}/500
                  </div>
                </div>
                <div v-if="getLineError(index, 'description')" class="field-error">
                  <i class="fas fa-exclamation-circle"></i>
                  {{ getLineError(index, 'description') }}
                </div>
              </div>

              <!-- Receipt Upload Section -->
              <div class="receipt-section">
                <label class="section-label">
                  <i class="fas fa-receipt"></i>
                  Receipt (Optional)
                </label>
                
                <div class="file-upload-container">
                  <input 
                    type="file" 
                    :id="`receipt-${index}`"
                    @change="handleFileSelect($event, line, index)"
                    accept="image/*,.pdf"
                    class="file-input"
                    :disabled="isSubmitting"
                  >
                  <label :for="`receipt-${index}`" 
                         class="file-upload-label" 
                         :class="{ 'error': getLineError(index, 'receipt'), 'disabled': isSubmitting }">
                    <i class="fas fa-cloud-upload-alt"></i>
                    <span v-if="!line.receiptFile">Choose file</span>
                    <span v-else class="file-selected">{{ line.receiptFile.name }}</span>
                  </label>
                  
                  <div v-if="line.receiptFile && !getLineError(index, 'receipt')" class="file-info">
                    <div class="file-details">
                      <div class="file-name">{{ line.receiptFile.name }}</div>
                      <div class="file-size">{{ formatFileSize(line.receiptFile.size) }}</div>
                      <div class="file-status success">
                        <i class="fas fa-check-circle"></i>
                        Valid file
                      </div>
                    </div>
                    <button 
                      type="button" 
                      class="remove-file-btn" 
                      @click="removeFile(line, index)"
                      :disabled="isSubmitting"
                    >
                      <i class="fas fa-times"></i>
                    </button>
                  </div>

                  <div v-if="getLineError(index, 'receipt')" class="field-error">
                    <i class="fas fa-exclamation-circle"></i>
                    {{ getLineError(index, 'receipt') }}
                  </div>
                  
                  <div class="file-upload-hint">
                    <i class="fas fa-info-circle"></i>
                    Supported formats: JPG, PNG, PDF (max 5MB)
                  </div>
                </div>
              </div>

              <!-- Travel Section -->
              <div class="travel-section">
                <label class="checkbox-label">
                  <input type="checkbox" v-model="line.isTravel" @change="toggleTravel(line)" :disabled="isSubmitting">
                  <span>Travel Expense</span>
                </label>

                <div v-if="line.isTravel" class="travel-details">
                  <div class="form-row">
                    <div class="form-group">
                      <label>Distance (km) *</label>
                      <input 
                        type="number" 
                        v-model="line.distanceKm" 
                        @input="calculateTravelCost(line)"
                        @blur="validateField(index, 'distanceKm')"
                        min="0"
                        max="10000"
                        placeholder="0"
                        :class="{ 'error': getLineError(index, 'distanceKm') }"
                        :disabled="isSubmitting"
                      >
                      <div v-if="getLineError(index, 'distanceKm')" class="field-error">
                        <i class="fas fa-exclamation-circle"></i>
                        {{ getLineError(index, 'distanceKm') }}
                      </div>
                    </div>
                    
                    <div class="form-group">
                      <label>Vehicle Type *</label>
                      <select 
                        v-model="line.tarifKmId" 
                        @change="calculateTravelCost(line)"
                        @blur="validateField(index, 'tarifKmId')"
                        :class="{ 'error': getLineError(index, 'tarifKmId') }"
                        :disabled="isSubmitting"
                      >
                        <option value="">Select vehicle</option>
                        <option v-for="tarif in tarifKms" :key="tarif.id" :value="tarif.id">
                          {{ tarif.categorieVehicule }} - {{ formatCurrency(tarif.tarifParKm) }}/km
                        </option>
                      </select>
                      <div v-if="getLineError(index, 'tarifKmId')" class="field-error">
                        <i class="fas fa-exclamation-circle"></i>
                        {{ getLineError(index, 'tarifKmId') }}
                      </div>
                    </div>
                  </div>
                  
                  <!-- Travel calculation preview -->
                  <div v-if="line.distanceKm && line.tarifKmId" class="travel-calculation">
                    <i class="fas fa-calculator"></i>
                    <span>Calculated: {{ line.distanceKm }}km × {{ formatCurrency(tarifKms.find(t => t.id === parseInt(line.tarifKmId))?.tarifParKm || 0) }} = {{ formatCurrency(line.montant) }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Submission Progress -->
          <div v-if="isSubmitting" class="submission-progress">
            <div class="progress-bar">
              <div class="progress-fill" :style="{ width: submissionProgress + '%' }"></div>
            </div>
            <div class="progress-text">
              <i class="fas fa-spinner fa-spin"></i>
              {{ submissionStatus }}
            </div>
          </div>
          
          <div class="modal-actions">
            <button type="button" class="cancel-btn" @click="closeExpenseModal" :disabled="isSubmitting">
              <i class="fas fa-times"></i>
              Cancel
            </button>
            <button type="submit" class="submit-btn" :disabled="isSubmitting || hasFormErrors">
              <i v-if="isSubmitting" class="fas fa-spinner fa-spin"></i>
              <i v-else class="fas fa-save"></i>
              {{ isSubmitting ? submissionStatus : 'Submit Report' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Manager Error Modal -->
    <div v-if="showManagerError" class="modal-overlay" @click="closeManagerError">
      <div class="modal-content manager-error-modal" @click.stop>
        <div class="modal-header">
          <div class="error-icon">
            <i class="fas fa-user-times"></i>
          </div>
          <button class="close-btn" @click="closeManagerError">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body manager-error-content">
          <h2>No Manager Assigned</h2>
          <p class="error-description">
            You cannot create a vacation request because no manager is assigned to your account.
          </p>
          <div class="error-details">
            <div class="detail-item">
              <i class="fas fa-info-circle"></i>
              <span>Vacation requests require manager approval</span>
            </div>
            <div class="detail-item">
              <i class="fas fa-user-cog"></i>
              <span>Contact your system administrator to resolve this issue</span>
            </div>
          </div>
        </div>
        <div class="modal-actions">
          <button class="understand-btn" @click="closeManagerError">
            <i class="fas fa-check"></i>
            I Understand
          </button>
        </div>
      </div>
    </div>

    <!-- Toast Notification -->
    <div v-if="toastMessage" :class="['toast', toastType]" @click="hideToast">
      <i :class="getToastIcon()"></i>
      <span>{{ toastMessage }}</span>
      <button class="toast-close" @click="hideToast">
        <i class="fas fa-times"></i>
      </button>
    </div>
  </div>
</template>

<script>
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { userService } from '../../services/UserService'
import { authService } from '../../services/authService'
import { vacationService } from '../../services/vacationService.js'
import { expenseService } from '../../services/expenseReportService.js'

// Custom error class for validation errors
class ValidationError extends Error {
  constructor(message, errors) {
    super(message)
    this.name = 'ValidationError'
    this.errors = errors
  }
}

export default {
  name: 'Dashboard',
  emits: ['navigate', 'vacation-created'],
  setup(props, { emit }) {
    const currentUser = ref(null)
    const stats = ref({
      vacationDays: 0,
      pendingRequests: 0,
      expenses: 0
    })
    const recentActivity = ref([])
    const teamMembers = ref([])
    
    // Vacation request modal state
    const showVacationModal = ref(false)
    const showManagerError = ref(false)
    const submitting = ref(false)
    const validationErrors = ref([])
    const vacationRequest = ref({
      type: 'CongeAnnuel',
      dateDebut: '',
      dateFin: '',
      commentaire: ''
    })

    // Enhanced expense report modal state
    const showExpenseModal = ref(false)
    const newExpenseReport = ref(getEmptyReport())
    const projects = ref([])
    const tarifKms = ref([])
    const loading = ref({
      projects: false,
      tarifKms: false
    })
    const error = ref({
      form: ''
    })

    // Enhanced error handling state
    const formErrors = ref({})
    const serverErrors = ref([])
    const isSubmitting = ref(false)
    const retryCount = ref(0)
    const maxRetries = 3
    const submissionProgress = ref(0)
    const submissionStatus = ref('Submitting...')

    // Toast state
    const toastMessage = ref('')
    const toastType = ref('success')
    const toastTimeout = ref(null)

    let unsubscribe = null

    // User data computed properties
    const displayName = computed(() => {
      const name = userService.getDisplayName()
      return name !== 'Unknown User' ? name : 'User'
    })

    const userInitials = computed(() => userService.getUserInitials())
    const userEmail = computed(() => userService.getUserEmail())
    const userPoste = computed(() => userService.getUserPoste())
    const userRole = computed(() => {
      const role = authService.getStoredRole()
      return role ? role.charAt(0).toUpperCase() + role.slice(1).toLowerCase() : 'User'
    })
    const managerId = computed(() => userService.getManagerId())
    const isManager = computed(() => authService.hasRole('manager'))

    // Generate avatar style for current user
    const userAvatarStyle = computed(() => generateAvatarStyle(displayName.value))

    // Vacation modal computed properties
    const minDate = computed(() => {
      return new Date().toISOString().split('T')[0]
    })

    // Enhanced form validation computed
    const hasFormErrors = computed(() => {
      return Object.keys(formErrors.value).length > 0 || serverErrors.value.length > 0
    })

    // Expense modal helper functions
    function getEmptyReport() {
      return {
        projetId: null,
        lignes: [getEmptyLine()]
      }
    }

    function getEmptyLine() {
      return {
        date: '',
        description: '',
        montant: 0,
        distanceKm: null,
        tarifKmId: null,
        isTravel: false,
        receiptFile: null
      }
    }

    // Enhanced validation function
    const validateExpenseForm = () => {
      const errors = {}
      const lines = newExpenseReport.value.lignes

      // Check if we have at least one line
      if (!lines || lines.length === 0) {
        errors.general = 'At least one expense line is required'
        return { isValid: false, errors }
      }

      // Validate each line
      lines.forEach((line, index) => {
        const lineErrors = validateExpenseLine(line, index)
        if (Object.keys(lineErrors).length > 0) {
          errors[`line_${index}`] = lineErrors
        }
      })

      // Check for duplicate descriptions on same date
      const dateDescriptionPairs = new Set()
      lines.forEach((line, index) => {
        if (line.date && line.description) {
          const pair = `${line.date}_${line.description.trim().toLowerCase()}`
          if (dateDescriptionPairs.has(pair)) {
            if (!errors[`line_${index}`]) errors[`line_${index}`] = {}
            errors[`line_${index}`].description = 'Duplicate expense on same date'
          }
          dateDescriptionPairs.add(pair)
        }
      })

      formErrors.value = errors
      const isValid = Object.keys(errors).length === 0
      return { isValid, errors }
    }

    // Validate individual expense line
    const validateExpenseLine = (line, index) => {
      const lineErrors = {}
      
      // Date validation
      if (!line.date) {
        lineErrors.date = 'Date is required'
      } else {
        const lineDate = new Date(line.date)
        const today = new Date()
        today.setHours(23, 59, 59, 999)
        
        if (lineDate > today) {
          lineErrors.date = 'Date cannot be in the future'
        }
        
        const oneYearAgo = new Date()
        oneYearAgo.setFullYear(oneYearAgo.getFullYear() - 1)
        if (lineDate < oneYearAgo) {
          lineErrors.date = 'Date cannot be more than 1 year ago'
        }
      }
      
      // Description validation
      if (!line.description || line.description.trim() === '') {
        lineErrors.description = 'Description is required'
      } else if (line.description.trim().length < 3) {
        lineErrors.description = 'Description must be at least 3 characters'
      } else if (line.description.trim().length > 500) {
        lineErrors.description = 'Description cannot exceed 500 characters'
      }
      
      // Amount validation
      if (!line.montant || line.montant === '' || line.montant === 0) {
        lineErrors.montant = 'Amount is required'
      } else {
        const amount = parseFloat(line.montant)
        if (isNaN(amount) || amount <= 0) {
          lineErrors.montant = 'Amount must be a positive number'
        } else if (amount > 50000) {
          lineErrors.montant = 'Amount cannot exceed $50,000'
        } else if (!/^\d+(\.\d{1,2})?$/.test(line.montant.toString())) {
          lineErrors.montant = 'Amount can have at most 2 decimal places'
        }
      }
      
      // Travel expense validation
      if (line.isTravel) {
        if (!line.distanceKm) {
          lineErrors.distanceKm = 'Distance is required for travel expenses'
        } else {
          const distance = parseInt(line.distanceKm)
          if (isNaN(distance) || distance <= 0) {
            lineErrors.distanceKm = 'Distance must be a positive number'
          } else if (distance > 10000) {
            lineErrors.distanceKm = 'Distance cannot exceed 10,000 km'
          }
        }
        
        if (!line.tarifKmId) {
          lineErrors.tarifKmId = 'Vehicle type is required for travel expenses'
        }
      }
      
      // File validation
      if (line.receiptFile) {
        const file = line.receiptFile
        const maxSize = 5 * 1024 * 1024 // 5MB
        const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'application/pdf']
        
        if (file.size > maxSize) {
          lineErrors.receipt = 'File size must be less than 5MB'
        } else if (!allowedTypes.includes(file.type)) {
          lineErrors.receipt = 'Only JPG, PNG and PDF files are allowed'
        }
      }
      
      return lineErrors
    }

    // Validate individual field
    const validateField = (lineIndex, field) => {
      const line = newExpenseReport.value.lignes[lineIndex]
      const lineErrors = validateExpenseLine(line, lineIndex)
      
      if (lineErrors[field]) {
        setLineError(lineIndex, field, lineErrors[field])
      } else {
        clearLineError(lineIndex, field)
      }
    }

    // Enhanced form submission with better error handling
    const submitExpenseReport = async () => {
      // Clear previous errors
      formErrors.value = {}
      serverErrors.value = []
      error.value.form = ''
      submissionProgress.value = 0
      
      // Client-side validation
      const validation = validateExpenseForm()
      if (!validation.isValid) {
        showToast('Please fix the errors before submitting', 'error')
        return
      }
      
      isSubmitting.value = true
      submissionStatus.value = 'Preparing submission...'
      
      try {
        await submitWithRetry()
      } catch (err) {
        console.error('Failed to submit expense report after retries:', err)
        handleSubmissionError(err)
      } finally {
        isSubmitting.value = false
        submissionProgress.value = 0
      }
    }

    // Retry mechanism for network issues
    const submitWithRetry = async () => {
      for (let attempt = 1; attempt <= maxRetries; attempt++) {
        try {
          retryCount.value = attempt
          submissionProgress.value = (attempt - 1) * 30
          
          // Add artificial delay for retries to handle temporary network issues
          if (attempt > 1) {
            submissionStatus.value = `Retrying... (${attempt}/${maxRetries})`
            await new Promise(resolve => setTimeout(resolve, 1000 * attempt))
          }
          
          const result = await performSubmission()
          
          if (result.success) {
            submissionProgress.value = 100
            submissionStatus.value = 'Success!'
            showToast('Expense report created successfully', 'success')
            closeExpenseModal()
            loadUserStats()
            return
          } else {
            // If it's a validation error, don't retry
            if (result.errors && Array.isArray(result.errors)) {
              throw new ValidationError('Server validation failed', result.errors)
            }
            
            // If it's the last attempt, throw the error
            if (attempt === maxRetries) {
              throw new Error(result.message || 'Failed to submit expense report')
            }
          }
        } catch (err) {
          // Don't retry validation errors or authentication errors
          if (err instanceof ValidationError || 
              err.message.includes('Session expirée') ||
              err.message.includes('401')) {
            throw err
          }
          
          // If it's the last attempt, throw the error
          if (attempt === maxRetries) {
            throw err
          }
                  }
      }
    }

    // Actual submission logic
    const performSubmission = async () => {
      submissionStatus.value = 'Creating expense report...'
      submissionProgress.value = 20
      
      // First create the expense report
      const reportResult = await expenseService.createExpenseReport({
        projetId: newExpenseReport.value.projetId,
        dateSoumission: new Date().toISOString(),
        statut: 'EnAttente'
      })
      
      if (!reportResult.success) {
        throw new Error(reportResult.message || 'Failed to create expense report')
      }
      
      const reportId = reportResult.data.id
      submissionProgress.value = 40
      const lineResults = []
      
      // Process expense lines with individual error handling
      submissionStatus.value = 'Adding expense lines...'
      const totalLines = newExpenseReport.value.lignes.length
      
      for (const [index, line] of newExpenseReport.value.lignes.entries()) {
        try {
          submissionStatus.value = `Processing line ${index + 1} of ${totalLines}...`
          submissionProgress.value = 40 + (index / totalLines) * 50
          
          // Handle file upload first if there's a file
          let justificatifPath = null
          
          if (line.receiptFile) {
            submissionStatus.value = `Uploading receipt for line ${index + 1}...`
            justificatifPath = await simulateFileUpload(line.receiptFile)
          }
          
          const lineData = {
            noteDeFraisId: reportId,
            date: line.date,
            description: line.description.trim(),
            montant: parseFloat(line.montant),
            distanceKm: line.isTravel && line.distanceKm ? parseInt(line.distanceKm) : null,
            tarifKmId: line.isTravel && line.tarifKmId ? parseInt(line.tarifKmId) : null,
            justificatifPath: justificatifPath
          }
          
          const lineResult = await expenseService.createExpenseLine(lineData)
          
          if (!lineResult.success) {
            throw new Error(`Line ${index + 1}: ${lineResult.message}`)
          }
          
          lineResults.push(lineResult)
        } catch (lineError) {
          // If any line fails, we should clean up the created report
          console.error(`Failed to create line ${index + 1}:`, lineError)
          
          submissionStatus.value = 'Cleaning up incomplete submission...'
          // Attempt to delete the created report
          try {
            await expenseService.deleteExpenseReport(reportId)
          } catch (cleanupError) {
            console.error('Failed to cleanup incomplete report:', cleanupError)
          }
          
          throw new Error(`Failed to create expense line ${index + 1}: ${lineError.message}`)
        }
      }
      
      submissionProgress.value = 95
      submissionStatus.value = 'Finalizing...'
      
      return { success: true, data: { id: reportId, lines: lineResults } }
    }

    // Handle different types of submission errors
    const handleSubmissionError = (err) => {
      console.error('Submission error:', err)
      
      if (err instanceof ValidationError) {
        serverErrors.value = err.errors
        error.value.form = 'Server validation failed. Please check the errors below.'
      } else if (err.message.includes('Session expirée') || err.message.includes('401')) {
        error.value.form = 'Your session has expired. Please log in again.'
        // Trigger auth error handling
        window.dispatchEvent(new CustomEvent('auth-expired'))
      } else if (err.message.includes('Network') || err.message.includes('fetch')) {
        error.value.form = `Network error after ${maxRetries} attempts. Please check your internet connection and try again.`
      } else if (err.message.includes('500')) {
        error.value.form = 'Server error. Please try again later or contact support if the problem persists.'
      } else {
        error.value.form = err.message || 'An unexpected error occurred. Please try again.'
      }
      
      showToast(error.value.form, 'error')
    }

    // Enhanced file handling with better error feedback
    const handleFileSelect = (event, line, index) => {
      const file = event.target.files[0]
      if (!file) return
      
      // Clear previous file errors for this line
      clearLineError(index, 'receipt')
      
      // Validate file size (5MB limit)
      if (file.size > 5 * 1024 * 1024) {
        setLineError(index, 'receipt', 'File size must be less than 5MB')
        event.target.value = ''
        return
      }
      
      // Validate file type
      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'application/pdf']
      if (!allowedTypes.includes(file.type)) {
        setLineError(index, 'receipt', 'Only JPG, PNG and PDF files are allowed')
        event.target.value = ''
        return
      }
      
      // Validate file name
      if (file.name.length > 255) {
        setLineError(index, 'receipt', 'File name is too long (max 255 characters)')
        event.target.value = ''
        return
      }
      
      // Check for potentially malicious files
      const suspiciousExtensions = ['.exe', '.bat', '.cmd', '.scr', '.vbs', '.js']
      const hasSuspicious = suspiciousExtensions.some(ext => 
        file.name.toLowerCase().endsWith(ext)
      )
      
      if (hasSuspicious) {
        setLineError(index, 'receipt', 'File type not allowed for security reasons')
        event.target.value = ''
        return
      }
      
      line.receiptFile = file
      
      // Show success feedback
      showToast(`File "${file.name}" selected successfully`, 'success')
    }

    // Utility functions for error management
    const setLineError = (lineIndex, field, message) => {
      if (!formErrors.value[`line_${lineIndex}`]) {
        formErrors.value[`line_${lineIndex}`] = {}
      }
      formErrors.value[`line_${lineIndex}`][field] = message
    }

    const clearLineError = (lineIndex, field) => {
      if (formErrors.value[`line_${lineIndex}`] && formErrors.value[`line_${lineIndex}`][field]) {
        delete formErrors.value[`line_${lineIndex}`][field]
        
        // If no more errors for this line, remove the line entry
        if (Object.keys(formErrors.value[`line_${lineIndex}`]).length === 0) {
          delete formErrors.value[`line_${lineIndex}`]
        }
      }
    }

    const getLineError = (lineIndex, field) => {
      return formErrors.value[`line_${lineIndex}`]?.[field]
    }

    // Simulate file upload (replace with actual upload logic)
    const simulateFileUpload = async (file) => {
      // Simulate upload delay
      await new Promise(resolve => setTimeout(resolve, 500 + Math.random() * 1000))
      
      // Generate a mock file path
      const timestamp = Date.now()
      const fileExtension = file.name.split('.').pop()
      return `receipts/${timestamp}_${Math.random().toString(36).substr(2, 9)}.${fileExtension}`
    }

    // Methods for avatar generation
    const generateAvatarStyle = (name) => {
      const colors = [
        '#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4', '#FFEAA7',
        '#DDA0DD', '#98D8C8', '#F7DC6F', '#BB8FCE', '#85C1E9'
      ]
      
      let hash = 0
      const nameToHash = name || 'User'
      for (let i = 0; i < nameToHash.length; i++) {
        hash = nameToHash.charCodeAt(i) + ((hash << 5) - hash)
      }
      const colorIndex = Math.abs(hash) % colors.length
      
      return {
        backgroundColor: colors[colorIndex],
        color: 'white'
      }
    }

    const getInitials = (name) => {
      if (!name) return 'U'
      return name
        .split(' ')
        .map(word => word.charAt(0).toUpperCase())
        .join('')
        .substring(0, 2)
    }

    // Toast methods
    const showToast = (message, type = 'success') => {
      toastMessage.value = message
      toastType.value = type
      
      if (toastTimeout.value) {
        clearTimeout(toastTimeout.value)
      }
      
      toastTimeout.value = setTimeout(() => {
        hideToast()
      }, 4000)
    }

    const hideToast = () => {
      toastMessage.value = ''
      if (toastTimeout.value) {
        clearTimeout(toastTimeout.value)
        toastTimeout.value = null
      }
    }

    const getToastIcon = () => {
      switch (toastType.value) {
        case 'success': return 'fas fa-check-circle'
        case 'error': return 'fas fa-exclamation-circle'
        case 'warning': return 'fas fa-exclamation-triangle'
        case 'info': return 'fas fa-info-circle'
        default: return 'fas fa-check-circle'
      }
    }

    // Vacation request methods
    const openVacationModal = async () => {
      showVacationModal.value = true
    }

    const closeVacationModal = () => {
      showVacationModal.value = false
      resetVacationForm()
    }

    const closeManagerError = () => {
      showManagerError.value = false
    }

    const resetVacationForm = () => {
      vacationRequest.value = {
        type: 'CongeAnnuel',
        dateDebut: '',
        dateFin: '',
        commentaire: ''
      }
      validationErrors.value = []
    }

    const calculateDuration = (startDate, endDate) => {
      if (!startDate || !endDate) return 0
      const start = new Date(startDate)
      const end = new Date(endDate)
      const timeDiff = end.getTime() - start.getTime()
      const dayDiff = Math.ceil(timeDiff / (1000 * 3600 * 24)) + 1
      return Math.max(dayDiff, 0)
    }

    const submitVacationRequest = async () => {
      // Reset validation errors
      validationErrors.value = []

      // Basic validation
      if (!vacationRequest.value.dateDebut || !vacationRequest.value.dateFin) {
        validationErrors.value.push('Please select both start and end dates')
        return
      }

      if (new Date(vacationRequest.value.dateDebut) > new Date(vacationRequest.value.dateFin)) {
        validationErrors.value.push('End date must be after start date')
        return
      }

      if (new Date(vacationRequest.value.dateDebut) < new Date()) {
        validationErrors.value.push('Start date cannot be in the past')
        return
      }

      submitting.value = true

      try {
        if (typeof vacationService !== 'undefined' && vacationService.createVacationRequest) {
          const response = await vacationService.createVacationRequest(vacationRequest.value)
          
          if (response.success) {
            showToast(response.message || 'Vacation request submitted successfully', 'success')
            closeVacationModal()
            emit('vacation-created')
            loadUserStats()
          } else {
            if (response.message) {
              if (response.message.includes('manager')) {
                showManagerError.value = true
                closeVacationModal()
                return
              } else if (response.message.includes('conflit')) {
                validationErrors.value.push('A vacation request already exists for the selected dates.')
              } else {
                validationErrors.value.push(response.message)
              }
            } else if (response.errors && Array.isArray(response.errors)) {
              validationErrors.value = response.errors
            } else {
              validationErrors.value = ['Error submitting vacation request']
            }
          }
        } else {
          showToast('Vacation request submitted successfully', 'success')
          closeVacationModal()
          emit('vacation-created')
        }
      } catch (error) {
        console.error('Error submitting vacation request:', error)
        validationErrors.value = [error.message || 'Error submitting vacation request']
      } finally {
        submitting.value = false
      }
    }

    // Expense report methods
    const openExpenseModal = async () => {
      showExpenseModal.value = true
      await loadProjects()
      await loadTarifKms()
    }

    const closeExpenseModal = () => {
      showExpenseModal.value = false
      resetExpenseForm()
    }

    const resetExpenseForm = () => {
      newExpenseReport.value = getEmptyReport()
      formErrors.value = {}
      serverErrors.value = []
      error.value.form = ''
      retryCount.value = 0
      submissionProgress.value = 0
    }

    const loadProjects = async () => {
      loading.value.projects = true
      
      try {
        const result = await expenseService.getAllProjects()
        
        if (result.success) {
          projects.value = result.data || []
        } else {
          console.warn('Failed to load projects:', result.message)
          projects.value = []
        }
      } catch (error) {
        console.error('Failed to load projects:', error)
        projects.value = []
      } finally {
        loading.value.projects = false
      }
    }

    const loadTarifKms = async () => {
      try {
        const result = await expenseService.getAllKmRates()
        
        if (result.success) {
          tarifKms.value = result.data || []
        } else {
          console.warn('Failed to load km rates:', result.message)
          tarifKms.value = []
        }
      } catch (error) {
        console.error('Failed to load km rates:', error)
        tarifKms.value = []
      }
    }

    const addExpenseLine = () => {
      newExpenseReport.value.lignes.push(getEmptyLine())
    }

    const removeLine = (index) => {
      if (newExpenseReport.value.lignes.length > 1) {
        newExpenseReport.value.lignes.splice(index, 1)
        // Clean up any errors for removed lines
        delete formErrors.value[`line_${index}`]
      }
    }

    const toggleTravel = (line) => {
      if (!line.isTravel) {
        line.distanceKm = null
        line.tarifKmId = null
      }
    }

    const calculateTravelCost = (line) => {
      if (line.distanceKm && line.tarifKmId) {
        const tarif = tarifKms.value.find(t => t.id === parseInt(line.tarifKmId))
        if (tarif) {
          line.montant = (line.distanceKm * tarif.tarifParKm).toFixed(2)
        }
      }
    }

    const removeFile = (line, index) => {
      line.receiptFile = null
      // Clear the file input
      const fileInput = document.getElementById(`receipt-${index}`)
      if (fileInput) {
        fileInput.value = ''
      }
      clearLineError(index, 'receipt')
    }

    const formatFileSize = (bytes) => {
      if (bytes === 0) return '0 Bytes'
      const k = 1024
      const sizes = ['Bytes', 'KB', 'MB', 'GB']
      const i = Math.floor(Math.log(bytes) / Math.log(k))
      return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
    }

    const formatCurrency = (amount) => {
      if (!amount) return '$0.00'
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
      }).format(amount)
    }

    const getCurrentDate = () => {
      return new Date().toISOString().split('T')[0]
    }

    const isExpenseFormValid = () => {
      if (!newExpenseReport.value.lignes || newExpenseReport.value.lignes.length === 0) {
        return false
      }
      
      return newExpenseReport.value.lignes.every(line => 
        line.date && 
        line.description && 
        line.montant && 
        parseFloat(line.montant) > 0
      )
    }

    const loadUserStats = async () => {
      try {
        const userId = userService.getUserId()
        if (!userId) {
          console.warn('No user ID available for loading stats')
          return
        }

        // Initialize with default values
        const newStats = {
          vacationDays: 0,
          pendingRequests: 0,
          expenses: 0
        }

        // Load vacation balance to calculate days off used (30 - remaining balance)
        try {
          const balanceResult = await vacationService.getVacationBalance()
          if (balanceResult.success && balanceResult.data) {
            const totalAnnualDays = 30
            const remainingDays = balanceResult.data.joursRestants || 0
            newStats.vacationDays = Math.max(0, totalAnnualDays - remainingDays)
            
          }
        } catch (error) {
          console.error('Failed to load vacation balance:', error)
        }

        // Load vacation requests to count pending ones
        try {
          const vacationResult = await vacationService.getUserVacationRequests()
          if (vacationResult.success && vacationResult.data) {
            const pendingRequests = vacationResult.data.filter(request => 
              request.statut === 'enAttente'
            )
            newStats.pendingRequests = pendingRequests.length
          }
        } catch (error) {
          console.error('Failed to load vacation requests:', error)
        }

        // Load expense reports to calculate current month total
        try {
          const expenseResult = await expenseService.getUserExpenseReports()
          if (expenseResult.success && expenseResult.data) {
            const currentMonth = new Date().getMonth()
            const currentYear = new Date().getFullYear()
            
            const currentMonthExpenses = expenseResult.data.filter(report => {
              const reportDate = new Date(report.dateSoumission || report.DateSoumission)
              return reportDate.getMonth() === currentMonth && 
                    reportDate.getFullYear() === currentYear
            })

            // Calculate total amount for current month
            let monthlyTotal = 0
            for (const report of currentMonthExpenses) {
              if (report.lignes && Array.isArray(report.lignes)) {
                monthlyTotal += expenseService.calculateExpenseTotal(report.lignes)
              } else if (report.Lignes && Array.isArray(report.Lignes)) {
                monthlyTotal += expenseService.calculateExpenseTotal(report.Lignes)
              }
            }
            
            newStats.expenses = Math.round(monthlyTotal * 100) / 100 // Round to 2 decimal places
          }
        } catch (error) {
          console.error('Failed to load expense reports:', error)
        }

        // Update the stats
        stats.value = newStats
      } catch (error) {
        console.error('Failed to load user stats:', error)
        // Keep default values if everything fails
        stats.value = {
          vacationDays: 0,
          pendingRequests: 0,
          expenses: 0
        }
      }
    }

    // Load recent activity (mock for now)
    const loadRecentActivity = async () => {
      try {
        recentActivity.value = [
          { id: 1, title: 'Vacation request approved', time: '2 hours ago', icon: 'fas fa-check-circle', type: 'success', status: 'approved', statusText: 'Approved' },
          { id: 2, title: 'Expense report submitted', time: '1 day ago', icon: 'fas fa-file-invoice', type: 'info', status: 'pending', statusText: 'Pending' },
          { id: 3, title: 'Team meeting scheduled', time: '2 days ago', icon: 'fas fa-calendar', type: 'primary', status: 'completed', statusText: 'Completed' }
        ]
      } catch (error) {
        console.error('Failed to load recent activity:', error)
      }
    }

    // Load team members (for managers)
    const loadTeamMembers = async () => {
      try {
        if (isManager.value) {
          teamMembers.value = [
            { id: 1, name: 'Sarah Johnson', role: 'Project Manager' },
            { id: 2, name: 'Mike Chen', role: 'Frontend Developer' },
            { id: 3, name: 'Lisa Rodriguez', role: 'UX Designer' },
            { id: 4, name: 'David Kim', role: 'Backend Developer' }
          ]
        }
      } catch (error) {
        console.error('Failed to load team members:', error)
      }
    }

    // Initialize dashboard data
    const initializeDashboard = () => {
      loadUserStats()
      loadRecentActivity()
      loadTeamMembers()
    }

    // Date formatting methods
    const formatDate = (date) => {
      return date.toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
    }

    const formatTime = (date) => {
      return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
    }

    onMounted(() => {
      currentUser.value = userService.getCurrentUser()
      // Subscribe to user data changes
      unsubscribe = userService.subscribe((userData) => {
        currentUser.value = userData
        if (userData) {
          initializeDashboard()
        }
      })
      // Initialize if user data is already available
      if (currentUser.value) {
        initializeDashboard()
      } else {
        // Try to get user data from auth check
        authService.checkAuth().then((result) => {
          if (result.authenticated && result.user) {
            initializeDashboard()
          }
        })
      }
    })

    onUnmounted(() => {
      if (unsubscribe) {
        unsubscribe()
      }
      if (toastTimeout.value) {
        clearTimeout(toastTimeout.value)
      }
    })

    return {
      currentUser,
      displayName,
      userInitials,
      userEmail,
      userPoste,
      userRole,
      managerId,
      isManager,
      userAvatarStyle,
      stats,
      recentActivity,
      teamMembers,
      showVacationModal,
      showManagerError,
      submitting,
      validationErrors,
      vacationRequest,
      minDate,
      showExpenseModal,
      newExpenseReport,
      projects,
      tarifKms,
      loading,
      error,
      formErrors,
      serverErrors,
      isSubmitting,
      retryCount,
      maxRetries,
      submissionProgress,
      submissionStatus,
      hasFormErrors,
      toastMessage,
      toastType,
      generateAvatarStyle,
      getInitials,
      formatDate,
      formatTime,
      openVacationModal,
      closeVacationModal,
      closeManagerError,
      calculateDuration,
      submitVacationRequest,
      openExpenseModal,
      closeExpenseModal,
      addExpenseLine,
      removeLine,
      toggleTravel,
      calculateTravelCost,
      handleFileSelect,
      removeFile,
      formatFileSize,
      formatCurrency,
      getCurrentDate,
      isExpenseFormValid,
      submitExpenseReport,
      validateExpenseForm,
      validateField,
      setLineError,
      clearLineError,
      getLineError,
      resetExpenseForm,
      showToast,
      hideToast,
      getToastIcon
    }
  }
}
</script>

<style scoped>
/* All existing dashboard styles remain the same */
.dashboard {
  max-width: 1400px;
  margin: 0 auto;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 3rem;
}

.welcome-section {
  flex: 1;
}

.welcome-title {
  font-size: 2.5rem;
  font-weight: 700;
  color: #1D1D1D;
  margin-bottom: 0.5rem;
  line-height: 1.2;
}

.welcome-subtitle {
  color: rgba(29, 29, 29, 0.6);
  font-size: 1.1rem;
  font-weight: 400;
}

.date-display {
  text-align: right;
  padding: 1rem;
  background: rgba(246, 201, 0, 0.1);
  border-radius: 16px;
  border: 1px solid rgba(246, 201, 0, 0.2);
}

.date-main {
  font-size: 1.1rem;
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 0.25rem;
}

.date-sub {
  color: rgba(29, 29, 29, 0.6);
  font-size: 0.9rem;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 2rem;
}

.panel {
  background: #FFFFFF;
  border-radius: 20px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.05);
  padding: 2rem;
  position: relative;
  overflow: hidden;
  transition: transform 0.3s ease;
}

.panel:hover {
  transform: translateY(-5px);
}

.panel-title {
  font-size: 1.2rem;
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 1.5rem;
}

/* Quick Actions Panel */
.quick-actions {
  grid-column: span 1;
}

.actions-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
}

.action-btn {
  width: 100%;
  padding: 1.25rem;
  font-size: 1rem;
  font-weight: 600;
  border-radius: 12px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.action-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.action-btn i {
  font-size: 1.2rem;
}

.action-btn.primary {
  background: #3B82F6;
  color: white;
}

.action-btn.secondary {
  background: #F3F4F6;
  color: #1D1D1D;
}

/* Stats Overview */
.stats-panel {
  grid-column: span 3;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.5rem;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  background: #F9FAFB;
  padding: 1rem 1.5rem;
  border-radius: 16px;
  border: 1px solid #E5E7EB;
}

.stat-icon {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 1.2rem;
}

.stat-icon.primary { background: #3B82F6; }
.stat-icon.secondary { background: #F59E0B; }
.stat-icon.success { background: #10B981; }

.stat-content {
  display: flex;
  flex-direction: column;
}

.stat-number {
  font-size: 1.75rem;
  font-weight: 700;
  color: #1D1D1D;
}

.stat-label {
  font-size: 0.9rem;
  color: rgba(29, 29, 29, 0.6);
  font-weight: 500;
}

/* Recent Activity */
.activity-panel {
  grid-column: span 2;
  min-height: 350px;
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.activity-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  border-bottom: 1px solid #F3F4F6;
  padding-bottom: 1.5rem;
}

.activity-item:last-child {
  border-bottom: none;
  padding-bottom: 0;
}

.activity-icon {
  width: 45px;
  height: 45px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 1rem;
}

.activity-icon.success { background: #10B981; }
.activity-icon.info { background: #3B82F6; }
.activity-icon.primary { background: #6B46C1; }

.activity-content {
  flex: 1;
}

.activity-title {
  font-weight: 600;
  color: #1D1D1D;
}

.activity-time {
  font-size: 0.875rem;
  color: rgba(29, 29, 29, 0.6);
}

.activity-status {
  padding: 0.25rem 0.75rem;
  border-radius: 8px;
  font-size: 0.8rem;
  font-weight: 600;
  text-transform: uppercase;
}

.activity-status.approved { background: #D1FAE5; color: #065F46; }
.activity-status.pending { background: #FEF3C7; color: #92400E; }
.activity-status.completed { background: #DBEAFE; color: #1E40AF; }

/* Team Overview & User Profile */
.team-panel, .profile-panel {
  grid-column: span 2;
}

.team-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.5rem;
}

.team-member, .profile-content {
  display: flex;
  align-items: center;
  gap: 1rem;
  background: #F9FAFB;
  padding: 1rem;
  border-radius: 12px;
}

.member-avatar, .profile-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 1.1rem;
}

.member-info, .profile-info {
  flex: 1;
}

.member-name, h4 {
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 0.25rem;
}

.member-role, p {
  font-size: 0.9rem;
  color: rgba(29, 29, 29, 0.6);
}

.member-status {
  font-size: 0.75rem;
  color: #10B981;
}

.user-email {
  font-size: 0.8rem;
}

.profile-stats {
  display: flex;
  gap: 1rem;
  margin-top: 0.75rem;
}

.profile-stat {
  display: flex;
  flex-direction: column;
}

.profile-stat .stat-value {
  font-weight: 600;
  color: #1D1D1D;
}

.profile-stat .stat-label {
  font-size: 0.75rem;
  color: rgba(29, 29, 29, 0.6);
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(5px);
  animation: fade-in 0.3s ease;
}

.modal-content {
  background-color: white;
  padding: 2.5rem;
  border-radius: 20px;
  width: 90%;
  max-width: 700px;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
  animation: slide-up 0.3s cubic-bezier(0.68, -0.55, 0.27, 1.55);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
  color: #1D1D1D;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  color: rgba(29, 29, 29, 0.6);
  cursor: pointer;
  transition: color 0.2s ease;
}

.close-btn:hover {
  color: #1D1D1D;
}

.modal-body {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-group label {
  font-size: 0.9rem;
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 0.5rem;
}

.form-group input,
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #E5E7EB;
  border-radius: 8px;
  font-size: 1rem;
  background-color: #F9FAFB;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #3B82F6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.25);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

.duration-info {
  background-color: #F3F4F6;
  border-radius: 8px;
  padding: 1rem;
  text-align: center;
}

.duration-info p {
  margin: 0;
  font-weight: 600;
  color: #1D1D1D;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1rem;
}

.submit-btn, .cancel-btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 12px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-width: 120px;
  justify-content: center;
}

.submit-btn {
  background-color: #3B82F6;
  color: white;
}

.submit-btn:disabled {
  background-color: #D1D5DB;
  cursor: not-allowed;
}

.cancel-btn {
  background-color: #E5E7EB;
  color: #1D1D1D;
}

.validation-errors {
  background-color: #FEF2F2;
  border: 1px solid #FCA5A5;
  border-radius: 8px;
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.error-message {
  color: #EF4444;
  font-size: 0.9rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.error-message i {
  color: #F87171;
}

/* Alert Messages */
.alert {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem 1.5rem;
  border-radius: 12px;
  margin-bottom: 1rem;
  position: relative;
}

.alert-error {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
  border: 1px solid rgba(248, 113, 113, 0.2);
}

.form-loading {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-top: 0.5rem;
  font-size: 0.875rem;
  color: rgba(29, 29, 29, 0.6);
}

/* Expense Lines Section */
.expense-lines-section {
  margin-bottom: 2rem;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.section-header h4 {
  font-size: 1.1rem;
  font-weight: 600;
  color: #1D1D1D;
}

.add-line-btn {
  padding: 0.5rem 1rem;
  background: rgba(246, 201, 0, 0.1);
  color: #1D1D1D;
  border: 1px solid rgba(246, 201, 0, 0.2);
  border-radius: 8px;
  font-size: 0.875rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.add-line-btn:hover {
  background: rgba(246, 201, 0, 0.2);
}

.expense-line-form {
  background: rgba(246, 201, 0, 0.03);
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 1rem;
}

.line-form-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.line-form-header span {
  font-weight: 600;
  color: #1D1D1D;
}

.remove-line-btn {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
  border: 1px solid rgba(248, 113, 113, 0.2);
  border-radius: 6px;
  padding: 0.25rem 0.5rem;
  cursor: pointer;
  font-size: 0.875rem;
  transition: all 0.3s ease;
}

.remove-line-btn:hover {
  background: rgba(248, 113, 113, 0.2);
}

/* File Upload Styles */
.receipt-section {
  margin: 1.5rem 0;
  padding: 1rem;
  background: rgba(246, 201, 0, 0.03);
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 12px;
}

.section-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 1rem;
  font-weight: 500;
  color: #1D1D1D;
}

.file-upload-container {
  position: relative;
}

.file-input {
  display: none;
}

.file-upload-label {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 2rem;
  border: 2px dashed rgba(246, 201, 0, 0.3);
  border-radius: 12px;
  background: rgba(246, 201, 0, 0.05);
  cursor: pointer;
  transition: all 0.3s ease;
  color: #1D1D1D;
}

.file-upload-label:hover {
  border-color: #F6C900;
  background: rgba(246, 201, 0, 0.08);
}

.file-upload-label i {
  font-size: 2rem;
  color: #F6C900;
}

.file-info {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: rgba(246, 201, 0, 0.08);
  border-radius: 8px;
  margin-top: 0.5rem;
}

.file-details {
  flex: 1;
}

.file-name {
  font-weight: 500;
  color: #1D1D1D;
}

.file-size {
  font-size: 0.875rem;
  color: rgba(29, 29, 29, 0.6);
}

.remove-file-btn {
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
  cursor: pointer;
  border: 1px solid;
  display: flex;
  align-items: center;
  gap: 0.25rem;
  transition: all 0.3s ease;
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
  border-color: rgba(248, 113, 113, 0.2);
}

.remove-file-btn:hover {
  background: rgba(248, 113, 113, 0.15);
}

.file-upload-hint {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-top: 0.5rem;
  font-size: 0.75rem;
  color: rgba(29, 29, 29, 0.6);
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  margin-bottom: 1rem;
  font-weight: 500;
}

.checkbox-label input[type="checkbox"] {
  width: auto;
  margin: 0;
}

.travel-section {
  margin-top: 1rem;
}

.travel-details {
  padding: 1rem;
  background: rgba(246, 201, 0, 0.05);
  border-radius: 8px;
  border: 1px solid rgba(246, 201, 0, 0.1);
  margin-top: 0.5rem;
}

/* Manager Error Modal Specifics */
.manager-error-modal .modal-content {
  text-align: center;
}

.manager-error-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1.5rem;
}

.manager-error-content h2 {
  font-size: 1.75rem;
  margin-bottom: 0;
}

.error-icon {
  width: 64px;
  height: 64px;
  background-color: #FEE2E2;
  color: #DC2626;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
}

.error-description {
  max-width: 400px;
  line-height: 1.6;
}

.error-details {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  width: 100%;
}

.detail-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: 0.95rem;
  color: #4B5563;
  background-color: #F9FAFB;
  padding: 0.75rem 1rem;
  border-radius: 8px;
}

.detail-item i {
  color: #6B7280;
}

.understand-btn {
  width: 100%;
  padding: 0.75rem;
  background-color: #3B82F6;
  color: white;
  border: none;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.understand-btn:hover {
  background-color: #2563EB;
}

/* Toast Styles */
.toast {
  position: fixed;
  top: 20px;
  right: 20px;
  background: #FFFFFF;
  border-radius: 12px;
  padding: 1rem 1.5rem;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.15);
  display: flex;
  align-items: center;
  gap: 0.75rem;
  z-index: 10000;
  min-width: 300px;
  max-width: 500px;
  animation: slide-in-right 0.3s ease;
  cursor: pointer;
  border-left: 4px solid;
}

.toast.success {
  border-left-color: #10B981;
  color: #065F46;
}

.toast.success i {
  color: #10B981;
}

.toast.error {
  border-left-color: #EF4444;
  color: #991B1B;
}

.toast.error i {
  color: #EF4444;
}

.toast.warning {
  border-left-color: #F59E0B;
  color: #92400E;
}

.toast.warning i {
  color: #F59E0B;
}

.toast.info {
  border-left-color: #3B82F6;
  color: #1E40AF;
}

.toast.info i {
  color: #3B82F6;
}

.toast-close {
  background: none;
  border: none;
  font-size: 0.875rem;
  color: rgba(0, 0, 0, 0.4);
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 4px;
  transition: all 0.2s ease;
  margin-left: auto;
}
</style>