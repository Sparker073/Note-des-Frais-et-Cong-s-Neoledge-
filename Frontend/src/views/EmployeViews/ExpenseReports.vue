<template>
  <div class="expense-reports">
    <!-- Loading State -->
    <div v-if="loading.page" class="loading-container">
      <div class="loading-spinner"></div>
      <p>Loading expense reports...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error.page" class="error-container">
      <div class="error-icon">
        <i class="fas fa-exclamation-triangle"></i>
      </div>
      <h3>Unable to load expense reports</h3>
      <p>{{ error.page }}</p>
      <button class="retry-btn" @click="loadExpenseReports">
        <i class="fas fa-redo"></i>
        Try Again
      </button>
    </div>

    <!-- Main Content -->
    <div v-else>
      <header class="page-header">
        <div class="header-content">
          <h1 class="page-title">Expense Reports</h1>
          <p class="page-subtitle">Track and manage your business expenses</p>
        </div>
        <div class="header-actions">
          <button class="create-btn" @click="showCreateModal = true">
            <i class="fas fa-plus"></i>
            <span>New Report</span>
          </button>
        </div>
      </header>

      <!-- Success/Error Messages -->
      <div v-if="successMessage" class="alert alert-success">
        <i class="fas fa-check-circle"></i>
        {{ successMessage }}
        <button @click="successMessage = ''" class="alert-close">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <div v-if="error.operation" class="alert alert-error">
        <i class="fas fa-exclamation-circle"></i>
        {{ error.operation }}
        <button @click="error.operation = ''" class="alert-close">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <div class="content-layout">
        <!-- Reports List -->
        <div class="panel reports-panel">
          <div class="panel-header">
            <h3 class="panel-title">My Expense Reports</h3>
            <div class="reports-stats" v-if="expenseReports.length > 0">
              <span class="stat">{{ expenseReports.length }} reports</span>
              <span class="stat">{{ getTotalReportsAmount() }} total</span>
            </div>
          </div>

          <div v-if="loading.reports" class="loading-small">
            <div class="loading-spinner-small"></div>
            <span>Loading reports...</span>
          </div>

          <div v-else-if="expenseReports.length === 0" class="empty-state">
            <div class="empty-icon">
              <i class="fas fa-receipt"></i>
            </div>
            <h4>No expense reports yet</h4>
            <p>Create your first expense report to get started</p>
            <button class="create-first-btn" @click="showCreateModal = true">
              <i class="fas fa-plus"></i>
              Create First Report
            </button>
          </div>

          <div v-else class="reports-list">
            <div 
              v-for="report in expenseReports" 
              :key="report.id" 
              class="report-card" 
              :class="{ active: selectedReport?.id === report.id }"
              @click="selectReport(report)"
            >
              <div class="report-header">
                <div class="report-info">
                  <div class="report-title">{{ formatDate(report.dateSoumission) }}</div>
                  <div class="report-project" v-if="report.projet">{{ report.projet.nom }}</div>
                  <div class="report-project" v-else>No project assigned</div>
                </div>
                <div class="report-status" :class="getStatusClass(report.statut)">
                  <i :class="getStatusIcon(report.statut)"></i>
                  {{ formatStatus(report.statut) }}
                </div>
              </div>
              <div class="report-summary">
                <div class="expense-count">{{ report.lignes?.length || 0 }} expenses</div>
                <div class="total-amount">{{ formatCurrency(getTotalAmount(report.lignes || [])) }}</div>
              </div>
              
              <!-- Quick Action Buttons in Card -->
              <div class="report-actions-inline">
                <button 
                  v-if="canEditReport(report)" 
                  class="edit-btn-inline"
                  @click.stop="editReport(report)"
                  :disabled="loading.submit"
                >
                  <i class="fas fa-edit"></i>
                  Edit
                </button>
                <button 
                  v-if="canDeleteReport(report)" 
                  class="delete-btn-inline"
                  @click.stop="confirmDeleteReport(report)"
                  :disabled="loading.delete"
                >
                  <i class="fas fa-trash"></i>
                  Delete
                </button>
              </div>
              
              <div class="report-comment" v-if="report.commentaireManager">
                <strong>Manager Comment:</strong> {{ report.commentaireManager }}
              </div>
            </div>
          </div>
        </div>

        <!-- Report Details -->
        <div class="panel details-panel">
          <div v-if="!selectedReport" class="empty-selection">
            <div class="empty-icon">
              <i class="fas fa-mouse-pointer"></i>
            </div>
            <h4>Select a report</h4>
            <p>Choose a report from the list to view its details</p>
          </div>

          <div v-else>
            <div class="details-header">
              <h3 class="panel-title">Report Details</h3>
              <div class="report-actions">
                <button 
                  v-if="canEditReport(selectedReport)" 
                  class="edit-btn"
                  @click="editReport(selectedReport)"
                  :disabled="loading.submit"
                >
                  <i class="fas fa-edit"></i>
                  Edit Report
                </button>
                <button 
                  v-if="canDeleteReport(selectedReport)" 
                  class="delete-btn"
                  @click="confirmDeleteReport(selectedReport)"
                  :disabled="loading.delete"
                >
                  <i class="fas fa-trash"></i>
                  Delete Report
                </button>
              </div>
            </div>
            
            <div class="report-metadata">
              <div class="metadata-item">
                <strong>Submitted:</strong> {{ formatDate(selectedReport.dateSoumission) }}
              </div>
              <div class="metadata-item">
                <strong>Status:</strong> 
                <span :class="getStatusClass(selectedReport.statut)">
                  {{ formatStatus(selectedReport.statut) }}
                </span>
              </div>
              <div class="metadata-item" v-if="selectedReport.projet">
                <strong>Project:</strong> {{ selectedReport.projet.nom }}
              </div>
              <div class="metadata-item">
                <strong>Total Amount:</strong> 
                <span class="total-highlight">{{ formatCurrency(getTotalAmount(selectedReport.lignes || [])) }}</span>
              </div>
            </div>
            
            <div class="expense-lines">
              <h4>Expense Lines</h4>
              <div v-if="!selectedReport.lignes || selectedReport.lignes.length === 0" class="no-lines">
                <p>No expense lines found for this report.</p>
                <button 
                  v-if="canEditReport(selectedReport)"
                  class="add-lines-btn"
                  @click="editReport(selectedReport)"
                >
                  <i class="fas fa-plus"></i>
                  Add Expense Lines
                </button>
              </div>
              <div v-else>
                <div v-for="ligne in selectedReport.lignes" :key="ligne.id" class="expense-line">
                  <div class="line-header">
                    <div class="line-date">{{ formatDate(ligne.date) }}</div>
                    <div class="line-amount">{{ formatCurrency(ligne.montant) }}</div>
                  </div>
                  <div class="line-description">{{ ligne.description }}</div>
                  <div class="line-details" v-if="ligne.distanceKm">
                    <div class="travel-info">
                      <i class="fas fa-car"></i>
                      <span>{{ ligne.TarifKmId }}km</span>
                      <span v-if="ligne.tarifKm">@ {{ formatCurrency(ligne.tarifKm.tarifParKm) }}/km</span>
                    </div>
                  </div>
                  <div class="line-receipt" v-if="ligne.justificatifPath">
                    <i class="fas fa-paperclip"></i>
                    <span>Receipt attached</span>
                    <button class="view-receipt-btn" @click="viewReceipt(ligne.justificatifPath)">
                      <i class="fas fa-eye"></i>
                      View
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create/Edit Report Modal -->
    <div v-if="showCreateModal" class="modal-overlay" @click="closeCreateModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ editingReport ? 'Edit Expense Report' : 'New Expense Report' }}</h2>
          <button class="close-btn" @click="closeCreateModal">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <form @submit.prevent="submitReport" class="modal-body">
          <div v-if="error.form" class="alert alert-error">
            <i class="fas fa-exclamation-circle"></i>
            {{ error.form }}
          </div>

          <div class="form-group">
            <label for="project">Project</label>
            <select id="project" v-model="newReport.projetId">
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

          <div class="expense-lines-section">
            <div class="section-header">
              <h4>Expense Lines</h4>
              <button type="button" class="add-line-btn" @click="addExpenseLine">
                <i class="fas fa-plus"></i>
                Add Line
              </button>
            </div>

            <div v-for="(line, index) in newReport.lignes" :key="index" class="expense-line-form">
              <div class="line-form-header">
                <span>Expense #{{ index + 1 }}</span>
                <button 
                  type="button" 
                  class="remove-line-btn" 
                  @click="removeLine(index)" 
                  v-if="newReport.lignes.length > 1"
                >
                  <i class="fas fa-trash"></i>
                </button>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label>Date *</label>
                  <input 
                    type="date" 
                    v-model="line.date" 
                    required
                    :max="getCurrentDate()"
                  >
                </div>
                <div class="form-group">
                  <label>Amount *</label>
                  <input 
                    type="number" 
                    step="0.01" 
                    min="0"
                    v-model="line.montant" 
                    required
                    placeholder="0.00"
                  >
                </div>
              </div>

              <div class="form-group">
                <label>Description *</label>
                <input 
                  type="text" 
                  v-model="line.description" 
                  placeholder="Describe the expense..." 
                  required
                  maxlength="500"
                >
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
                  >
                  <label :for="`receipt-${index}`" class="file-upload-label">
                    <i class="fas fa-cloud-upload-alt"></i>
                    <span v-if="!line.receiptFile">Choose file</span>
                    <span v-else>{{ line.receiptFile.name }}</span>
                  </label>
                  
                  <div v-if="line.receiptFile" class="file-info">
                    <div class="file-details">
                      <div class="file-name">{{ line.receiptFile.name }}</div>
                      <div class="file-size">{{ formatFileSize(line.receiptFile.size) }}</div>
                    </div>
                    <button 
                      type="button" 
                      class="remove-file-btn" 
                      @click="removeFile(line, index)"
                    >
                      <i class="fas fa-times"></i>
                    </button>
                  </div>
                  
                  <div v-if="line.existingReceiptPath && !line.receiptFile" class="existing-file">
                    <div class="file-details">
                      <div class="file-name">Existing receipt</div>
                      <div class="file-actions">
                        <button 
                          type="button" 
                          class="view-file-btn" 
                          @click="viewReceipt(line.existingReceiptPath)"
                        >
                          <i class="fas fa-eye"></i>
                          View
                        </button>
                        <button 
                          type="button" 
                          class="remove-existing-btn" 
                          @click="removeExistingFile(line)"
                        >
                          <i class="fas fa-times"></i>
                        </button>
                      </div>
                    </div>
                  </div>
                  
                  <div class="file-upload-hint">
                    <i class="fas fa-info-circle"></i>
                    Supported formats: JPG, PNG, PDF (max 5MB)
                  </div>
                </div>
              </div>

              <div class="travel-section">
                <label class="checkbox-label">
                  <input type="checkbox" v-model="line.isTravel" @change="toggleTravel(line)">
                  <span>Travel Expense</span>
                </label>

                <div v-if="line.isTravel" class="travel-details">
                  <div class="form-row">
                    <div class="form-group">
                      <label>Distance (km)</label>
                      <input 
                        type="number" 
                        v-model="line.distanceKm" 
                        @input="calculateTravelCost(line)"
                        min="0"
                        placeholder="0"
                      >
                    </div>
                    <div class="form-group">
                      <label>Vehicle Type</label>
                      <select v-model="line.tarifKmId" @change="calculateTravelCost(line)">
                        <option value="">Select vehicle</option>
                        <option v-for="tarif in tarifKms" :key="tarif.id" :value="tarif.id">
                          {{ tarif.categorieVehicule }} - {{ formatCurrency(tarif.tarifParKm) }}/km
                        </option>
                      </select>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="modal-actions">
            <button type="button" class="cancel-btn" @click="closeCreateModal" :disabled="loading.submit">
              Cancel
            </button>
            <button type="submit" class="submit-btn" :disabled="loading.submit || !isFormValid()">
              <i v-if="loading.submit" class="fas fa-spinner fa-spin"></i>
              <i v-else class="fas fa-save"></i>
              {{ loading.submit ? 'Submitting...' : (editingReport ? 'Update Report' : 'Submit Report') }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div v-if="showDeleteModal" class="modal-overlay" @click="showDeleteModal = false">
      <div class="modal-content modal-small" @click.stop>
        <div class="modal-header">
          <h2>Confirm Delete</h2>
        </div>
        <div class="modal-body">
          <div class="delete-confirmation">
            <div class="warning-icon">
              <i class="fas fa-exclamation-triangle"></i>
            </div>
            <h3>Delete Expense Report?</h3>
            <p>Are you sure you want to delete this expense report?</p>
            <div class="report-info-delete">
              <div><strong>Date:</strong> {{ formatDate(reportToDelete?.dateSoumission) }}</div>
              <div><strong>Amount:</strong> {{ formatCurrency(getTotalAmount(reportToDelete?.lignes || [])) }}</div>
              <div v-if="reportToDelete?.projet"><strong>Project:</strong> {{ reportToDelete.projet.nom }}</div>
            </div>
            <p class="delete-warning">This action cannot be undone.</p>
          </div>
          
          <div class="modal-actions">
            <button class="cancel-btn" @click="showDeleteModal = false" :disabled="loading.delete">
              Cancel
            </button>
            <button class="delete-btn" @click="deleteReport" :disabled="loading.delete">
              <i v-if="loading.delete" class="fas fa-spinner fa-spin"></i>
              <i v-else class="fas fa-trash"></i>
              {{ loading.delete ? 'Deleting...' : 'Delete Report' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Receipt Viewer Modal -->
    <div v-if="showReceiptModal" class="modal-overlay receipt-modal" @click="closeReceiptModal">
      <div class="modal-content receipt-content" @click.stop>
        <div class="modal-header">
          <h2>Receipt View</h2>
          <button class="close-btn" @click="closeReceiptModal">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body receipt-body">
          <div v-if="currentReceiptPath" class="receipt-container">
            <img 
              v-if="isImageFile(currentReceiptPath)" 
              :src="getReceiptUrl(currentReceiptPath)" 
              alt="Receipt"
              class="receipt-image"
              @error="handleReceiptError"
            >
            <div v-else-if="isPdfFile(currentReceiptPath)" class="pdf-container">
              <i class="fas fa-file-pdf pdf-icon"></i>
              <p>PDF Document</p>
              <a 
                :href="getReceiptUrl(currentReceiptPath)" 
                target="_blank" 
                class="view-pdf-btn"
              >
                <i class="fas fa-external-link-alt"></i>
                Open in new tab
              </a>
            </div>
            <div v-else class="unsupported-file">
              <i class="fas fa-file"></i>
              <p>File format not supported for preview</p>
              <a 
                :href="getReceiptUrl(currentReceiptPath)" 
                download
                class="download-btn"
              >
                <i class="fas fa-download"></i>
                Download
              </a>
            </div>
          </div>
          <div v-if="receiptError" class="receipt-error">
            <i class="fas fa-exclamation-triangle"></i>
            <p>Unable to load receipt</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
// Import the correct service
import expenseService from '../../services/expenseReportService.js'
import userService from '../../services/UserService.js'

export default {
  name: 'ExpenseReports',
  props: {
    user: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      // Loading states
      loading: {
        page: true,
        reports: false,
        projects: false,
        submit: false,
        delete: false
      },
      
      // Error states
      error: {
        page: '',
        operation: '',
        form: ''
      },
      
      // Success messages
      successMessage: '',
      
      // Data
      expenseReports: [],
      projects: [],
      tarifKms: [],
      selectedReport: null,
      
      // Modals
      showCreateModal: false,
      showDeleteModal: false,
      showReceiptModal: false,
      
      // Forms
      newReport: this.getEmptyReport(),
      editingReport: null,
      reportToDelete: null,
      
      // Receipt viewer
      currentReceiptPath: null,
      receiptError: false
    }
  },
  
  async mounted() {
    await this.initializeComponent()
    
    // Listen for auth expiration
    window.addEventListener('auth-expired', this.handleAuthExpiration)
  },
  
  beforeUnmount() {
    window.removeEventListener('auth-expired', this.handleAuthExpiration)
  },
  
  methods: {
    async initializeComponent() {
      this.loading.page = true
      this.error.page = ''
      
      try {
        // Load all required data
        await Promise.all([
          this.loadExpenseReports(),
          this.loadProjects(),
          this.loadTarifKms()
        ])
      } catch (error) {
        console.error('Failed to initialize component:', error)
        this.error.page = 'Failed to load initial data. Please refresh the page.'
      } finally {
        this.loading.page = false
      }
    },
    
    async loadExpenseReports() {
      this.loading.reports = true
      this.error.operation = ''
      
      try {
        const result = await expenseService.getUserExpenseReports()
        
        if (result.success) {
          this.expenseReports = result.data || []
          
          // Auto-select first report if available
          if (this.expenseReports.length > 0 && !this.selectedReport) {
            this.selectedReport = this.expenseReports[0]
          }
          
          // Don't show error for empty results - the empty state will handle this
          if (this.expenseReports.length === 0) {
          }
        } else {
          // Only show error for actual failures, not empty results
          if (result.message && !result.message.includes('No expense reports found')) {
            this.error.operation = result.message
          }
        }
      } catch (error) {
        console.error('Failed to load expense reports:', error)
        this.error.operation = 'Unable to load expense reports. Please refresh the page or contact support.'
      } finally {
        this.loading.reports = false
      }
    },
    
    async loadProjects() {
      this.loading.projects = true
      
      try {
        const result = await expenseService.getAllProjects()
        
        if (result.success) {
          this.projects = result.data || []
        } else {
          console.warn('Failed to load projects:', result.message)
          this.projects = []
        }
      } catch (error) {
        console.error('Failed to load projects:', error)
        this.projects = []
      } finally {
        this.loading.projects = false
      }
    },
    
    async loadTarifKms() {
      try {
        const result = await expenseService.getAllKmRates()
        
        if (result.success) {
          this.tarifKms = result.data || []
        } else {
          console.warn('Failed to load km rates:', result.message)
          this.tarifKms = []
        }
      } catch (error) {
        console.error('Failed to load km rates:', error)
        this.tarifKms = []
      }
    },
    
    selectReport(report) {
      this.selectedReport = report
    },
    
    // Replace your existing canEditReport and canDeleteReport methods with these fixed versions:

    canEditReport(report) {
      const currentUser = userService.getCurrentUser();
      if (!currentUser) {
        return false;
      }
      
      const userRole = currentUser.role || currentUser.Role || currentUser.Poste || currentUser.poste;
      
      // Case-insensitive comparison for status
      const isPending = report.statut?.toLowerCase() === 'enattente';
      const isAdmin = userRole?.toLowerCase() === 'admin';
      
      return isPending || isAdmin;
    },

    canDeleteReport(report) {
      const currentUser = userService.getCurrentUser();
      if (!currentUser) {
        return false;
      }
      
      const userRole = currentUser.role || currentUser.Role || currentUser.Poste || currentUser.poste;
      
      // Case-insensitive comparison for status and role
      const isPending = report.statut?.toLowerCase() === 'enattente';
      const isAdmin = userRole?.toLowerCase() === 'admin';
      
      return isPending || isAdmin;
    },
    
    editReport(report) {
      this.editingReport = report
      this.newReport = {
        id: report.id,
        projetId: report.projetId || null,
        lignes: report.lignes?.map(ligne => ({
          id: ligne.id,
          date: this.formatDateForInput(ligne.date),
          description: ligne.description,
          montant: ligne.montant,
          distanceKm: ligne.distanceKm,
          tarifKmId: ligne.tarifKmId,
          isTravel: !!(ligne.distanceKm && ligne.tarifKmId),
          existingReceiptPath: ligne.justificatifPath,
          receiptFile: null
        })) || [this.getEmptyLine()]
      }
      this.showCreateModal = true
    },
    
    confirmDeleteReport(report) {
      this.reportToDelete = report
      this.showDeleteModal = true
    },
    
    async deleteReport() {
      if (!this.reportToDelete) return
      
      this.loading.delete = true
      
      try {
        const result = await expenseService.deleteExpenseReport(this.reportToDelete.id)
        
        if (result.success) {
          this.successMessage = 'Expense report deleted successfully'
          
          // Remove from list
          this.expenseReports = this.expenseReports.filter(r => r.id !== this.reportToDelete.id)
          
          // Clear selection if deleted report was selected
          if (this.selectedReport?.id === this.reportToDelete.id) {
            this.selectedReport = this.expenseReports[0] || null
          }
          
          this.showDeleteModal = false
          this.reportToDelete = null
        } else {
          throw new Error(result.message)
        }
      } catch (error) {
        console.error('Failed to delete report:', error)
        this.error.operation = error.message || 'Failed to delete expense report'
      } finally {
        this.loading.delete = false
      }
    },
    
    closeCreateModal() {
      this.showCreateModal = false
      this.editingReport = null
      this.newReport = this.getEmptyReport()
      this.error.form = ''
    },
    
    getEmptyReport() {
      return {
        projetId: null,
        lignes: [this.getEmptyLine()]
      }
    },
    
    getEmptyLine() {
      return {
        date: '',
        description: '',
        montant: 0,
        distanceKm: null,
        tarifKmId: null,
        isTravel: false,
        receiptFile: null,
        existingReceiptPath: null
      }
    },
    
    addExpenseLine() {
      this.newReport.lignes.push(this.getEmptyLine())
    },
    
    removeLine(index) {
      if (this.newReport.lignes.length > 1) {
        this.newReport.lignes.splice(index, 1)
      }
    },
    
    toggleTravel(line) {
      if (!line.isTravel) {
        line.distanceKm = null
        line.tarifKmId = null
        // Don't reset amount as user might have manually entered it
      }
    },
    
    calculateTravelCost(line) {
      if (line.distanceKm && line.tarifKmId) {
        const tarif = this.tarifKms.find(t => t.id === parseInt(line.tarifKmId))
        if (tarif) {
          line.montant = expenseService.calculateTravelExpense(line.distanceKm, tarif.tarifParKm).toFixed(2)
        }
      }
    },
    
    // File handling methods
    handleFileSelect(event, line, index) {
      const file = event.target.files[0]
      if (!file) return
      
      // Validate file size (5MB limit)
      if (file.size > 5 * 1024 * 1024) {
        this.error.form = 'File size must be less than 5MB'
        event.target.value = ''
        return
      }
      
      // Validate file type
      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'application/pdf']
      if (!allowedTypes.includes(file.type)) {
        this.error.form = 'Only JPG, PNG and PDF files are allowed'
        event.target.value = ''
        return
      }
      
      line.receiptFile = file
      this.error.form = '' // Clear any previous error
    },
    
    removeFile(line, index) {
      line.receiptFile = null
      // Clear the file input
      const fileInput = document.getElementById(`receipt-${index}`)
      if (fileInput) {
        fileInput.value = ''
      }
    },
    
    removeExistingFile(line) {
      line.existingReceiptPath = null
      line.justificatifPath = null
    },
    
    formatFileSize(bytes) {
      if (bytes === 0) return '0 Bytes'
      const k = 1024
      const sizes = ['Bytes', 'KB', 'MB', 'GB']
      const i = Math.floor(Math.log(bytes) / Math.log(k))
      return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
    },
    
    // Receipt viewing methods
    viewReceipt(receiptPath) {
      this.currentReceiptPath = receiptPath
      this.receiptError = false
      this.showReceiptModal = true
    },
    
    closeReceiptModal() {
      this.showReceiptModal = false
      this.currentReceiptPath = null
      this.receiptError = false
    },
    
    handleReceiptError() {
      this.receiptError = true
    },
    
    isImageFile(path) {
      if (!path) return false
      const extension = path.split('.').pop().toLowerCase()
      return ['jpg', 'jpeg', 'png', 'gif', 'webp'].includes(extension)
    },
    
    isPdfFile(path) {
      if (!path) return false
      const extension = path.split('.').pop().toLowerCase()
      return extension === 'pdf'
    },
    
    getReceiptUrl(path) {
      if (!path) return ''
      // Adjust this URL based on your backend setup
      // Assuming receipts are served from a static files endpoint
      return `http://localhost:5032/uploads/receipts/${path}`
    },
    
    isFormValid() {
      if (!this.newReport.lignes || this.newReport.lignes.length === 0) {
        return false
      }
      
      return this.newReport.lignes.every(line => 
        line.date && 
        line.description && 
        line.montant && 
        parseFloat(line.montant) > 0
      )
    },
    
    // Fixed submitReport method in your Vue component
async submitReport() {
  if (!this.isFormValid()) {
    this.error.form = 'Please fill in all required fields'
    return
  }
  
  this.loading.submit = true
  this.error.form = ''
  
  try {
    // First create or update the expense report
    let reportResult
    if (this.editingReport) {
      reportResult = await expenseService.updateExpenseReport(this.editingReport.id, {
        projetId: this.newReport.projetId,
        dateSoumission: new Date().toISOString(),
        statut: 'EnAttente'
      })
    } else {
      reportResult = await expenseService.createExpenseReport({
        projetId: this.newReport.projetId,
        dateSoumission: new Date().toISOString(),
        statut: 'EnAttente'
      })
    }
    
    if (!reportResult.success) {
      throw new Error(reportResult.message)
    }
    
    const reportId = this.editingReport ? this.editingReport.id : reportResult.data.id
    
    // Get existing lines if editing
    const existingLines = this.editingReport ? (this.editingReport.lignes || []) : []
    const existingLineIds = existingLines.map(line => line.id)
    
    // Process expense lines
    const processedLineIds = []
    
    for (const line of this.newReport.lignes) {
      // Handle file upload first if there's a new file
      let justificatifPath = line.existingReceiptPath
      
      if (line.receiptFile) {
        // For now, we'll simulate file upload by generating a path
        const timestamp = Date.now()
        const fileExtension = line.receiptFile.name.split('.').pop()
        justificatifPath = `${timestamp}_${Math.random().toString(36).substr(2, 9)}.${fileExtension}`
        
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
      
      if (line.id && this.editingReport) {
        // ✅ FIXED: Only update if line still exists
        try {
          lineData.id = line.id
          const lineResult = await expenseService.updateExpenseLine(lineData)
          if (lineResult.success) {
            processedLineIds.push(line.id)
          } else {
            // If update fails, try to create new line instead
            console.warn(`Update failed for line ${line.id}, creating new line:`, lineResult.message)
            delete lineData.id
            const createResult = await expenseService.createExpenseLine(lineData)
            if (!createResult.success) {
              throw new Error(createResult.message)
            }
          }
        } catch (error) {
          console.error(`Error updating line ${line.id}:`, error)
          // Fallback: create new line
          delete lineData.id
          const createResult = await expenseService.createExpenseLine(lineData)
          if (!createResult.success) {
            throw new Error(createResult.message)
          }
        }
      } else {
        // Create new line
        const lineResult = await expenseService.createExpenseLine(lineData)
        if (!lineResult.success) {
          throw new Error(lineResult.message)
        }
      }
    }
    
    
    this.successMessage = this.editingReport ? 
      'Expense report updated successfully' : 
      'Expense report created successfully'
    
    this.closeCreateModal()
    await this.loadExpenseReports()
    
  } catch (error) {
    console.error('Failed to submit report:', error)
    this.error.form = error.message || 'Failed to submit expense report'
  } finally {
    this.loading.submit = false
  }
},
    
    handleAuthExpiration() {
      this.error.page = 'Your session has expired. Please log in again.'
    },
    
    // Utility methods
    formatDate(dateString) {
      if (!dateString) return ''
      return new Date(dateString).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        year: 'numeric'
      })
    },
    
    formatDateForInput(dateString) {
      if (!dateString) return ''
      return new Date(dateString).toISOString().split('T')[0]
    },
    
    formatCurrency(amount) {
      if (!amount) return '$0.00'
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
      }).format(amount)
    },
    
    formatStatus(status) {
      return expenseService.formatExpenseStatus(status)
    },
    
    getStatusClass(status) {
      const classMap = {
        'EnAttente': 'pending',
        'Approuvee': 'approved',
        'Refusee': 'rejected'
      }
      return classMap[status] || 'pending'
    },
    
    getStatusIcon(status) {
      return expenseService.getStatusIcon(status)
    },
    
    getTotalAmount(lignes) {
      return expenseService.calculateExpenseTotal(lignes)
    },
    
    getTotalReportsAmount() {
      const total = this.expenseReports.reduce((sum, report) => 
        sum + this.getTotalAmount(report.lignes || []), 0
      )
      return this.formatCurrency(total)
    },
    
    getCurrentDate() {
      return new Date().toISOString().split('T')[0]
    }
  }
}
</script>

<style scoped>
/* Base Styles */
.expense-reports {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

/* Loading States */
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  color: rgba(29, 29, 29, 0.6);
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid rgba(246, 201, 0, 0.2);
  border-top: 3px solid #F6C900;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

.loading-small {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem;
  color: rgba(29, 29, 29, 0.6);
}

.loading-spinner-small {
  width: 20px;
  height: 20px;
  border: 2px solid rgba(246, 201, 0, 0.2);
  border-top: 2px solid #F6C900;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.form-loading {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-top: 0.5rem;
  font-size: 0.875rem;
  color: rgba(29, 29, 29, 0.6);
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* Error States */
.error-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  text-align: center;
  color: #DC2626;
}

.error-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.retry-btn {
  margin-top: 1rem;
  padding: 0.75rem 1.5rem;
  background: #F6C900;
  color: #1D1D1D;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.retry-btn:hover {
  background: #F6BF00;
  transform: translateY(-1px);
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

.alert-success {
  background: rgba(74, 222, 128, 0.1);
  color: #16A34A;
  border: 1px solid rgba(74, 222, 128, 0.2);
}

.alert-error {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
  border: 1px solid rgba(248, 113, 113, 0.2);
}

.alert-close {
  position: absolute;
  right: 1rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: inherit;
  cursor: pointer;
  font-size: 1rem;
  opacity: 0.7;
  transition: opacity 0.3s ease;
}

.alert-close:hover {
  opacity: 1;
}

/* Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
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

.header-actions {
  display: flex;
  gap: 1rem;
}

.create-btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  text-decoration: none;
  background: linear-gradient(135deg, #F6C900 0%, #F6BF00 100%);
  color: #1D1D1D;
  box-shadow: 0 4px 15px rgba(246, 201, 0, 0.3);
}

.create-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(246, 201, 0, 0.4);
}

/* Layout */
.content-layout {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
}

.panel {
  background: #FFFFFF;
  border-radius: 24px;
  padding: 2rem;
  box-shadow: 0 4px 20px rgba(29, 29, 29, 0.08);
  border: 1px solid rgba(246, 201, 0, 0.1);
  height: fit-content;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.panel-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1D1D1D;
}

.reports-stats {
  display: flex;
  gap: 1rem;
  font-size: 0.875rem;
  color: rgba(29, 29, 29, 0.6);
}

.stat {
  padding: 0.25rem 0.75rem;
  background: rgba(246, 201, 0, 0.1);
  border-radius: 16px;
}

/* Empty States */
.empty-state {
  text-align: center;
  padding: 3rem 1rem;
  color: rgba(29, 29, 29, 0.6);
}

.empty-selection {
  text-align: center;
  padding: 4rem 1rem;
  color: rgba(29, 29, 29, 0.6);
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
  color: rgba(246, 201, 0, 0.3);
}

.create-first-btn {
  margin-top: 1rem;
  padding: 0.75rem 1.5rem;
  background: #F6C900;
  color: #1D1D1D;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.create-first-btn:hover {
  background: #F6BF00;
  transform: translateY(-1px);
}

.add-lines-btn {
  margin-top: 1rem;
  padding: 0.5rem 1rem;
  background: rgba(246, 201, 0, 0.1);
  color: #1D1D1D;
  border: 1px solid rgba(246, 201, 0, 0.2);
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.add-lines-btn:hover {
  background: rgba(246, 201, 0, 0.2);
}

/* Report Cards */
.reports-list {
  max-height: 600px;
  overflow-y: auto;
}

.report-card {
  background: rgba(246, 201, 0, 0.03);
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 16px;
  padding: 1.5rem;
  margin-bottom: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.report-card:hover {
  background: rgba(246, 201, 0, 0.08);
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(29, 29, 29, 0.1);
}

.report-card.active {
  background: rgba(246, 201, 0, 0.1);
  border-color: #F6C900;
  box-shadow: 0 4px 15px rgba(246, 201, 0, 0.2);
}

.report-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.report-title {
  font-weight: 600;
  color: #1D1D1D;
  font-size: 1.1rem;
  margin-bottom: 0.25rem;
}

.report-project {
  color: #F6BF00;
  font-size: 0.875rem;
  font-weight: 500;
}

.report-status {
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 0.875rem;
  font-weight: 600;
  text-transform: capitalize;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.report-status.approved {
  background: rgba(74, 222, 128, 0.1);
  color: #16A34A;
}

.report-status.rejected {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
}

.report-status.pending {
  background: rgba(251, 146, 60, 0.1);
  color: #EA580C;
}

.report-summary {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.expense-count {
  color: rgba(29, 29, 29, 0.6);
  font-size: 0.875rem;
}

.total-amount {
  font-weight: 700;
  color: #1D1D1D;
  font-size: 1.25rem;
}

.total-highlight {
  font-weight: 700;
  color: #F6BF00;
  font-size: 1.1rem;
}

/* Inline Action Buttons */
.report-actions-inline {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.edit-btn-inline, .delete-btn-inline {
  padding: 0.4rem 0.8rem;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.4rem;
  transition: all 0.3s ease;
  border: 1px solid;
}

.edit-btn-inline {
  background: rgba(59, 130, 246, 0.1);
  color: #2563EB;
  border-color: rgba(59, 130, 246, 0.2);
}

.delete-btn-inline {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
  border-color: rgba(248, 113, 113, 0.2);
}

.edit-btn-inline:hover:not(:disabled) {
  background: rgba(59, 130, 246, 0.15);
  transform: translateY(-1px);
}

.delete-btn-inline:hover:not(:disabled) {
  background: rgba(248, 113, 113, 0.15);
  transform: translateY(-1px);
}

.edit-btn-inline:disabled, .delete-btn-inline:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.report-comment {
  padding: 1rem;
  background: rgba(246, 201, 0, 0.05);
  border-radius: 12px;
  border-left: 4px solid #F6C900;
  color: rgba(29, 29, 29, 0.8);
  font-size: 0.9rem;
}

/* Report Details */
.details-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.report-actions {
  display: flex;
  gap: 0.75rem;
}

.edit-btn, .delete-btn {
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
  border: 1px solid;
}

.edit-btn {
  background: rgba(59, 130, 246, 0.1);
  color: #2563EB;
  border-color: rgba(59, 130, 246, 0.2);
}

.delete-btn {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
  border-color: rgba(248, 113, 113, 0.2);
}

.edit-btn:hover:not(:disabled) {
  background: rgba(59, 130, 246, 0.15);
}

.delete-btn:hover:not(:disabled) {
  background: rgba(248, 113, 113, 0.15);
}

.edit-btn:disabled, .delete-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.report-metadata {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
  padding: 1.5rem;
  background: rgba(246, 201, 0, 0.03);
  border-radius: 12px;
  border: 1px solid rgba(246, 201, 0, 0.1);
}

.metadata-item {
  font-size: 0.9rem;
}

.metadata-item strong {
  color: #1D1D1D;
}

/* Delete Confirmation */
.delete-confirmation {
  text-align: center;
  padding: 1rem;
}

.warning-icon {
  font-size: 4rem;
  color: #DC2626;
  margin-bottom: 1rem;
}

.delete-confirmation h3 {
  color: #1D1D1D;
  margin-bottom: 1rem;
}

.report-info-delete {
  background: rgba(248, 113, 113, 0.05);
  border: 1px solid rgba(248, 113, 113, 0.1);
  border-radius: 8px;
  padding: 1rem;
  margin: 1rem 0;
  text-align: left;
}

.report-info-delete div {
  margin-bottom: 0.5rem;
}

.delete-warning {
  color: #DC2626;
  font-weight: 500;
  margin-top: 1rem;
}

/* Expense Lines */
.expense-lines h4 {
  font-size: 1.1rem;
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 1rem;
}

.no-lines {
  text-align: center;
  padding: 2rem;
  color: rgba(29, 29, 29, 0.6);
}

.expense-line {
  background: rgba(246, 201, 0, 0.03);
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 12px;
  padding: 1.25rem;
  margin-bottom: 1rem;
}

.line-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}

.line-date {
  color: rgba(29, 29, 29, 0.6);
  font-size: 0.875rem;
  font-weight: 500;
}

.line-amount {
  font-weight: 700;
  color: #1D1D1D;
  font-size: 1.1rem;
}

.line-description {
  color: #1D1D1D;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.line-details {
  margin-bottom: 0.5rem;
}

.travel-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #F6BF00;
  font-size: 0.875rem;
  font-weight: 500;
}

.line-receipt {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: rgba(29, 29, 29, 0.6);
  font-size: 0.875rem;
}

.view-receipt-btn {
  background: rgba(59, 130, 246, 0.1);
  color: #2563EB;
  border: 1px solid rgba(59, 130, 246, 0.2);
  border-radius: 6px;
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.25rem;
  margin-left: auto;
  transition: all 0.3s ease;
}

.view-receipt-btn:hover {
  background: rgba(59, 130, 246, 0.15);
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  animation: fade-in 0.3s ease;
}

.modal-overlay.receipt-modal {
  background: rgba(0, 0, 0, 0.8);
}

.modal-content {
  background: #FFFFFF;
  border-radius: 24px;
  width: 90%;
  max-width: 700px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  animation: slide-up 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.modal-small {
  max-width: 500px;
}

.receipt-content {
  max-width: 800px;
  max-height: 95vh;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 2rem;
  border-bottom: 1px solid rgba(246, 201, 0, 0.1);
}

.modal-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1D1D1D;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.25rem;
  color: rgba(29, 29, 29, 0.5);
  cursor: pointer;
  transition: color 0.3s ease;
  padding: 0.5rem;
}

.close-btn:hover {
  color: #1D1D1D;
}

.modal-body {
  padding: 2rem;
}

.receipt-body {
  padding: 1rem;
  text-align: center;
}

/* Receipt Viewer Styles */
.receipt-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}

.receipt-image {
  max-width: 100%;
  max-height: 70vh;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}

.pdf-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 3rem;
}

.pdf-icon {
  font-size: 4rem;
  color: #DC2626;
}

.view-pdf-btn, .download-btn {
  padding: 0.75rem 1.5rem;
  background: #F6C900;
  color: #1D1D1D;
  border: none;
  border-radius: 8px;
  text-decoration: none;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.view-pdf-btn:hover, .download-btn:hover {
  background: #F6BF00;
  transform: translateY(-1px);
}

.unsupported-file {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 3rem;
  color: rgba(29, 29, 29, 0.6);
}

.unsupported-file i {
  font-size: 3rem;
}

.receipt-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 3rem;
  color: #DC2626;
}

.receipt-error i {
  font-size: 3rem;
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

.existing-file {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: rgba(59, 130, 246, 0.05);
  border: 1px solid rgba(59, 130, 246, 0.2);
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

.file-actions {
  display: flex;
  gap: 0.5rem;
}

.view-file-btn, .remove-file-btn, .remove-existing-btn {
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
  cursor: pointer;
  border: 1px solid;
  display: flex;
  align-items: center;
  gap: 0.25rem;
  transition: all 0.3s ease;
}

.view-file-btn {
  background: rgba(59, 130, 246, 0.1);
  color: #2563EB;
  border-color: rgba(59, 130, 246, 0.2);
}

.remove-file-btn, .remove-existing-btn {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
  border-color: rgba(248, 113, 113, 0.2);
}

.view-file-btn:hover {
  background: rgba(59, 130, 246, 0.15);
}

.remove-file-btn:hover, .remove-existing-btn:hover {
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

/* Form Styles */
.form-group {
  margin-bottom: 1.5rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
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
  transition: border-color 0.3s ease, box-shadow 0.3s ease;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: #F6C900;
  box-shadow: 0 0 0 3px rgba(246, 201, 0, 0.1);
}

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

/* Modal Actions */
.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px solid rgba(246, 201, 0, 0.1);
}

.cancel-btn, .submit-btn {
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-width: 120px;
  justify-content: center;
}

.cancel-btn {
  background: rgba(29, 29, 29, 0.1);
  color: #1D1D1D;
  border: 1px solid rgba(29, 29, 29, 0.2);
}

.submit-btn {
  background: linear-gradient(135deg, #F6C900 0%, #F6BF00 100%);
  color: #1D1D1D;
  border: none;
}

.delete-btn {
  background: linear-gradient(135deg, #DC2626 0%, #B91C1C 100%);
  color: #FFFFFF;
  border: none;
}

.cancel-btn:hover:not(:disabled) {
  background: rgba(29, 29, 29, 0.15);
}

.submit-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 15px rgba(246, 201, 0, 0.3);
}

.delete-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 15px rgba(220, 38, 38, 0.3);
}

.submit-btn:disabled, .cancel-btn:disabled, .delete-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

/* Animations */
@keyframes fade-in {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slide-up {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Responsive Design */
@media (max-width: 1024px) {
  .content-layout {
    grid-template-columns: 1fr;
  }
  
  .details-panel {
    order: -1;
  }
}

@media (max-width: 768px) {
  .expense-reports {
    padding: 0.5rem;
  }
  
  .page-header {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
  
  .header-actions {
    justify-content: space-between;
  }
  
  .form-row {
    grid-template-columns: 1fr;
  }
  
  .modal-actions {
    flex-direction: column;
  }
  
  .panel {
    padding: 1.5rem;
    border-radius: 16px;
  }
  
  .modal-content {
    width: 95%;
    margin: 1rem;
  }
  
  .reports-stats {
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .report-metadata {
    grid-template-columns: 1fr;
  }
  
  .file-upload-label {
    padding: 1.5rem;
  }
  
  .file-info, .existing-file {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
  
  .file-actions {
    width: 100%;
    justify-content: flex-end;
  }

  .report-actions-inline {
    flex-wrap: wrap;
  }

  .report-actions {
    flex-direction: column;
    gap: 0.5rem;
  }
}

@media (max-width: 480px) {
  .page-title {
    font-size: 1.75rem;
  }
  
  .modal-header, .modal-body {
    padding: 1.5rem;
  }
  
  .report-card {
    padding: 1rem;
  }
  
  .expense-line-form {
    padding: 1rem;
  }
  
  .receipt-body {
    padding: 0.5rem;
  }
  
  .receipt-image {
    max-height: 60vh;
  }

  .edit-btn-inline, .delete-btn-inline {
    font-size: 0.7rem;
    padding: 0.3rem 0.6rem;
  }
}
</style>