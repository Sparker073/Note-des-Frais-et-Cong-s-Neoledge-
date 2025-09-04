<template>
  <div class="vacation-requests">
    <header class="page-header">
      <div class="header-content">
        <h1 class="page-title">Demandes de Congé</h1>
        <p class="page-subtitle">Gérez vos demandes de congé</p>
      </div>
      <div class="header-actions">
        <div class="balance-card" v-if="vacationBalance">
          <div class="balance-content">
            <div class="balance-main">
              <div class="balance-icon">
                <i class="fas fa-calendar-check"></i>
              </div>
              <div class="balance-details">
                <span class="balance-label">Solde Congés Restants</span>
                <div class="balance-numbers">
                  <span class="balance-value">{{ formatBalanceValue(vacationBalance) }}</span>
                  <span class="balance-unit">jours</span>
                </div>
                <div class="balance-period" v-if="vacationBalance.annee">
                  Année {{ vacationBalance.annee }}
                </div>
              </div>
            </div>
            <div class="balance-progress">
              <div class="progress-bar">
                <div 
                  class="progress-fill" 
                  :style="{ width: getBalancePercentage(vacationBalance) + '%' }"
                ></div>
              </div>
              <div class="progress-text">
                {{ getUsedDays(vacationBalance) }} / 30 utilisés
              </div>
            </div>
          </div>
        </div>
        <button class="create-btn" @click="checkManagerBeforeCreate" :disabled="loading">
          <i class="fas fa-plus"></i>
          <span>Nouvelle Demande</span>
        </button>
      </div>
    </header>

    <!-- Toast Messages -->
    <div v-if="toastMessage" :class="['toast', toastType]" @click="hideToast">
      <i :class="getToastIcon()"></i>
      <span>{{ toastMessage }}</span>
      <button class="toast-close" @click="hideToast">
        <i class="fas fa-times"></i>
      </button>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>Chargement des données...</p>
    </div>

    <!-- Error State -->
    <div v-if="error" class="error-state">
      <i class="fas fa-exclamation-triangle"></i>
      <p>{{ error }}</p>
      <button @click="loadData" class="retry-btn">Réessayer</button>
    </div>

    <div class="content-grid" v-if="!loading && !error">
      <!-- Personal Requests -->
      <div class="panel requests-panel">
        <div class="panel-header">
          <h3 class="panel-title">Mes Demandes</h3>
          <div class="filter-tabs">
            <button 
              v-for="status in statusFilters" 
              :key="status.value"
              @click="selectedStatusFilter = status.value"
              :class="['filter-tab', { active: selectedStatusFilter === status.value }]"
            >
              {{ status.label }}
            </button>
          </div>
        </div>
        <div class="requests-list">
          <div v-if="filteredPersonalRequests.length === 0" class="empty-state">
            <i class="fas fa-calendar-alt"></i>
            <p>Aucune demande trouvée</p>
          </div>
          <div 
            v-for="request in filteredPersonalRequests" 
            :key="request.id" 
            class="request-card"
          >
            <div class="request-header">
              <div class="request-dates">
                <div class="date-range">
                  {{ formatDate(request.dateDebut) }} - {{ formatDate(request.dateFin) }}
                </div>
                <div class="duration">{{ calculateDuration(request.dateDebut, request.dateFin) }} jours</div>
              </div>
              <div class="request-status" :class="getStatusClass(request.statut)">
                <i :class="getStatusIcon(request.statut)"></i>
                {{ formatStatus(request.statut) }}
              </div>
            </div>
            <div class="request-body">
              <div class="request-type">
                <i :class="getTypeIcon(request.type)"></i>
                {{ formatType(request.type) }}
              </div>
              <div class="request-reason" v-if="request.commentaire">
                {{ request.commentaire }}
              </div>
            </div>
            <div class="request-footer" v-if="request.commentaireManager">
              <div class="manager-comment">
                <strong>Note du Manager:</strong> {{ request.commentaireManager }}
              </div>
            </div>
            <div class="request-actions">
              <button class="edit-btn" @click="editRequest(request)">
                <i class="fas fa-edit"></i>
                Modifier
              </button>
              <button class="delete-btn" @click="deleteRequest(request.id)" :disabled="deletingId === request.id">
                <i class="fas fa-trash"></i>
                {{ deletingId === request.id ? 'Suppression...' : 'Supprimer' }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Team Requests (if manager) -->
      <div class="panel team-requests-panel" v-if="isManager && teamRequests.length > 0">
        <h3 class="panel-title">Demandes de l'Équipe</h3>
        <div class="requests-list">
          <div 
            v-for="request in teamRequests" 
            :key="request.id" 
            class="request-card team-request"
          >
            <div class="request-header">
              <div class="employee-info">
                <div class="employee-avatar">
                  {{ getEmployeeInitials(request.employe?.nom) }}
                </div>
                <div>
                  <div class="employee-name">{{ request.employe?.nom || 'Employé' }}</div>
                  <div class="request-dates">{{ formatDate(request.dateDebut) }} - {{ formatDate(request.dateFin) }}</div>
                </div>
              </div>
              <div class="request-status" :class="getStatusClass(request.statut)">
                {{ formatStatus(request.statut) }}
              </div>
            </div>
            <div class="request-body">
              <div class="request-details">
                <span class="request-type">
                  <i :class="getTypeIcon(request.type)"></i>
                  {{ formatType(request.type) }}
                </span>
                <span class="duration">{{ calculateDuration(request.dateDebut, request.dateFin) }} jours</span>
              </div>
              <div class="request-reason" v-if="request.commentaire">
                {{ request.commentaire }}
              </div>
            </div>
            <div class="request-actions" v-if="request.statut === 'EnAttente'">
              <button 
                class="approve-btn" 
                @click="updateRequestStatus(request.id, 'Approuve')"
                :disabled="updatingStatusId === request.id"
              >
                <i class="fas fa-check"></i>
                {{ updatingStatusId === request.id ? 'Traitement...' : 'Approuver' }}
              </button>
              <button 
                class="reject-btn" 
                @click="showRejectModal(request)"
                :disabled="updatingStatusId === request.id"
              >
                <i class="fas fa-times"></i>
                Refuser
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create/Edit Request Modal -->
    <div v-if="showCreateModal" class="modal-overlay" @click="closeCreateModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ editingRequest ? 'Modifier la Demande' : 'Nouvelle Demande de Congé' }}</h2>
          <button class="close-btn" @click="closeCreateModal">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <form @submit.prevent="submitRequest" class="modal-body">
          <div class="form-group">
            <label for="vacationType">Type de Congé</label>
            <select id="vacationType" v-model="newRequest.type" required>
              <option value="CongeAnnuel">Congé Annuel</option>
              <option value="Maladie">Maladie</option>
              <option value="Maternite">Maternité</option>
              <option value="Paternite">Paternité</option>
              <option value="DecesProche">Décès Proche</option>
            </select>
          </div>
          
          <div class="form-row">
            <div class="form-group">
              <label for="startDate">Date de Début</label>
              <input 
                type="date" 
                id="startDate" 
                v-model="newRequest.dateDebut" 
                required
                :min="minDate"
              >
            </div>
            <div class="form-group">
              <label for="endDate">Date de Fin</label>
              <input 
                type="date" 
                id="endDate" 
                v-model="newRequest.dateFin" 
                required
                :min="newRequest.dateDebut || minDate"
              >
            </div>
          </div>

          <div class="duration-info" v-if="newRequest.dateDebut && newRequest.dateFin">
            <p>Durée: {{ calculateDuration(newRequest.dateDebut, newRequest.dateFin) }} jours</p>
          </div>

          <div class="form-group">
            <label for="reason">Commentaire</label>
            <textarea 
              id="reason" 
              v-model="newRequest.commentaire" 
              rows="3" 
              placeholder="Ajoutez un commentaire à votre demande..."
            ></textarea>
          </div>

          <div class="validation-errors" v-if="validationErrors.length > 0">
            <div v-for="error in validationErrors" :key="error" class="error-message">
              <i class="fas fa-exclamation-triangle"></i>
              {{ error }}
            </div>
          </div>
          
          <div class="modal-actions">
            <button type="button" class="cancel-btn" @click="closeCreateModal">Annuler</button>
            <button type="submit" class="submit-btn" :disabled="submitting">
              {{ submitting ? 'Traitement...' : (editingRequest ? 'Modifier' : 'Soumettre') }}
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
          <h2>Aucun Manager Assigné</h2>
          <p class="error-description">
            Vous ne pouvez pas créer de demande de congé car aucun manager n'est assigné à votre compte.
          </p>
          <div class="error-details">
            <div class="detail-item">
              <i class="fas fa-info-circle"></i>
              <span>Les demandes de congé nécessitent l'approbation d'un manager</span>
            </div>
            <div class="detail-item">
              <i class="fas fa-user-cog"></i>
              <span>Contactez votre administrateur système pour résoudre ce problème</span>
            </div>
          </div>
          <div class="contact-info">
            <h4>Que faire ?</h4>
            <ul>
              <li>Contactez votre département des ressources humaines</li>
              <li>Demandez à votre administrateur système d'assigner un manager à votre compte</li>
              <li>Vérifiez votre structure organisationnelle dans le système</li>
            </ul>
          </div>
        </div>
        <div class="modal-actions">
          <button class="understand-btn" @click="closeManagerError">
            <i class="fas fa-check"></i>
            J'ai Compris
          </button>
        </div>
      </div>
    </div>

    <!-- Reject Modal -->
    <div v-if="showRejectModalFlag" class="modal-overlay" @click="closeRejectModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Refuser la Demande</h2>
          <button class="close-btn" @click="closeRejectModal">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label for="rejectReason">Raison du refus</label>
            <textarea 
              id="rejectReason" 
              v-model="rejectReason" 
              rows="3" 
              placeholder="Expliquez pourquoi cette demande est refusée..."
              required
            ></textarea>
          </div>
          <div class="modal-actions">
            <button type="button" class="cancel-btn" @click="closeRejectModal">Annuler</button>
            <button 
              class="reject-btn" 
              @click="confirmReject"
              :disabled="!rejectReason.trim() || updatingStatusId"
            >
              {{ updatingStatusId ? 'Traitement...' : 'Confirmer le Refus' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { vacationService } from '../../services/vacationService.js'
import { userService } from '../../services/userService.js'

export default {
  name: 'VacationRequests',
  data() {
    return {
      loading: false,
      error: null,
      showCreateModal: false,
      showRejectModalFlag: false,
      showManagerError: false,
      submitting: false,
      deletingId: null,
      updatingStatusId: null,
      editingRequest: null,
      selectedRequestToReject: null,
      rejectReason: '',
      selectedStatusFilter: 'all',
      
      // Toast system
      toastMessage: '',
      toastType: 'success',
      toastTimeout: null,
      
      currentUser: null,
      personalRequests: [],
      teamRequests: [],
      vacationBalance: null,
      
      newRequest: {
        type: 'CongeAnnuel',
        dateDebut: '',
        dateFin: '',
        commentaire: ''
      },
      
      validationErrors: [],
      
      statusFilters: [
        { value: 'all', label: 'Toutes' },
        { value: 'EnAttente', label: 'En Attente' },
        { value: 'Approuve', label: 'Approuvées' },
        { value: 'Refuse', label: 'Refusées' }
      ]
    }
  },
  
  computed: {
    isManager() {
      return this.currentUser?.isManager || false
    },
    
    minDate() {
      return new Date().toISOString().split('T')[0]
    },
    
    filteredPersonalRequests() {
      if (!Array.isArray(this.personalRequests)) {
        return []
      }
      
      if (this.selectedStatusFilter === 'all') {
        return this.personalRequests
      }
      
      return this.personalRequests.filter(request => {
        if (!request || !request.statut) return false
        // Normalize both values for comparison (remove accents, spaces, make lowercase)
        const normalizeStatus = (status) => {
          return status
            .toLowerCase()
            .replace(/é/g, 'e')
            .replace(/è/g, 'e')
            .replace(/à/g, 'a')
            .replace(/ç/g, 'c')
            .replace(/\s+/g, '')
        }
        
        return normalizeStatus(request.statut) === normalizeStatus(this.selectedStatusFilter)
      })
    }
  },
  
  async mounted() {
    this.currentUser = userService.getCurrentUser()
    await this.loadData()
  },
  
  methods: {
    // Toast methods
    showToast(message, type = 'success') {
      this.toastMessage = message
      this.toastType = type
      
      if (this.toastTimeout) {
        clearTimeout(this.toastTimeout)
      }
      
      this.toastTimeout = setTimeout(() => {
        this.hideToast()
      }, 4000)
    },
    
    hideToast() {
      this.toastMessage = ''
      if (this.toastTimeout) {
        clearTimeout(this.toastTimeout)
        this.toastTimeout = null
      }
    },
    
    getToastIcon() {
      switch (this.toastType) {
        case 'success': return 'fas fa-check-circle'
        case 'error': return 'fas fa-exclamation-circle'
        case 'warning': return 'fas fa-exclamation-triangle'
        case 'info': return 'fas fa-info-circle'
        default: return 'fas fa-check-circle'
      }
    },
    
    async loadData() {
      this.loading = true
      this.error = null
      
      try {
        if (!this.currentUser) {
          throw new Error('Utilisateur non authentifié. Veuillez vous reconnecter.')
        }

        // Load user vacation requests
        const personalResponse = await vacationService.getUserVacationRequests()
        if (personalResponse.success) {
          this.personalRequests = Array.isArray(personalResponse.data) ? personalResponse.data : []
        } else {
          if (personalResponse.message && personalResponse.message.includes('401')) {
            throw new Error('Session expirée. Veuillez vous reconnecter.')
          }
          console.warn('Failed to load personal requests:', personalResponse.message)
          this.personalRequests = []
        }
        
        // Load vacation balance
        const balanceResponse = await vacationService.getVacationBalance()
        if (balanceResponse.success) {
          this.vacationBalance = balanceResponse.data
        } else {
          if (!balanceResponse.message || !balanceResponse.message.includes('401')) {
            console.warn('Failed to load vacation balance:', balanceResponse.message)
          }
          this.vacationBalance = null
        }
        
        // Load team requests if user is manager
        if (this.isManager) {
          const teamResponse = await vacationService.getTeamVacationRequests()
          if (teamResponse.success) {
            this.teamRequests = Array.isArray(teamResponse.data) ? teamResponse.data : []
          } else {
            if (!teamResponse.message || !teamResponse.message.includes('401')) {
              console.warn('Failed to load team requests:', teamResponse.message)
            }
            this.teamRequests = []
          }
        } else {
          this.teamRequests = []
        }
        
      } catch (error) {
        this.error = error.message || 'Erreur lors du chargement des données'
        console.error('Error loading vacation data:', error)
        
        if (error.message.includes('authentifié') || error.message.includes('Session')) {
          this.$emit?.('auth-error')
        }
        
        this.personalRequests = []
        this.teamRequests = []
        this.vacationBalance = null
      } finally {
        this.loading = false
      }
    },
    
    formatBalanceValue(balance) {
      if (!balance) return '0'
      
      if (typeof balance === 'object' && balance.joursRestants !== undefined) {
        return Math.round(balance.joursRestants * 10) / 10
      }
      
      if (typeof balance === 'number') {
        return Math.round(balance * 10) / 10
      }
      
      return '0'
    },
    
    // Fixed balance calculation methods based on 30 total days
    getUsedDays(balance) {
      if (!balance) return 0
      
      // Calculate used days as: 30 - remaining days
      const remaining = balance.joursRestants || 0
      const used = 30 - remaining
      
      return Math.max(used, 0) // Ensure non-negative
    },
    
    getBalancePercentage(balance) {
      if (!balance) return 0
      
      const used = this.getUsedDays(balance)
      const total = 30 // Fixed total of 30 days
      
      const percentage = (used / total) * 100
      return Math.min(Math.max(percentage, 0), 100)
    },
    
    formatDate(dateString) {
      return vacationService.formatDate(dateString)
    },
    
    formatType(type) {
      return vacationService.formatVacationType(type)
    },
    
    formatStatus(status) {
      return vacationService.formatVacationStatus(status)
    },
    
    getStatusClass(status) {
      if (!status) return 'pending'
      return status.toLowerCase().replace(/é/g, 'e').replace(/è/g, 'e')
    },
    
    getStatusIcon(status) {
      switch(status) {
        case 'Approuve': return 'fas fa-check-circle'
        case 'Refuse': return 'fas fa-times-circle'
        case 'EnAttente': return 'fas fa-clock'
        default: return 'fas fa-circle'
      }
    },
    
    getTypeIcon(type) {
      return vacationService.getTypeIcon(type)
    },
    
    calculateDuration(startDate, endDate) {
      return vacationService.calculateDuration(startDate, endDate)
    },
    
    getEmployeeInitials(name) {
      if (!name) return 'E'
      return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
    },
    
    editRequest(request) {
      this.editingRequest = request
      this.newRequest = {
        type: request.type,
        dateDebut: request.dateDebut.split('T')[0],
        dateFin: request.dateFin.split('T')[0],
        commentaire: request.commentaire || ''
      }
      this.showCreateModal = true
    },
    
    async deleteRequest(id) {
      if (!confirm('Êtes-vous sûr de vouloir supprimer cette demande ?')) {
        return
      }
      
      this.deletingId = id
      try {
        const response = await vacationService.deleteVacationRequest(id)
        if (response.success) {
          this.personalRequests = this.personalRequests.filter(r => r.id !== id)
          this.showToast(response.message || 'Demande supprimée avec succès', 'success')
        } else {
          throw new Error(response.message)
        }
      } catch (error) {
        this.showToast(error.message || 'Erreur lors de la suppression', 'error')
      } finally {
        this.deletingId = null
      }
    },
    
    async submitRequest() {
      this.validationErrors = vacationService.validateVacationDates(
        this.newRequest.dateDebut, 
        this.newRequest.dateFin
      )
      
      if (this.validationErrors.length > 0) {
        return
      }
      
      this.submitting = true
      
      try {
        let response
        if (this.editingRequest) {
          response = await vacationService.updateVacationRequest(this.editingRequest.id, this.newRequest)
        } else {
          response = await vacationService.createVacationRequest(this.newRequest)
        }
        
        if (response.success) {
          const message = response.message || (this.editingRequest ? 'Demande modifiée avec succès' : 'Demande créée avec succès')
          this.showToast(message, 'success')
          await this.loadData()
          this.closeCreateModal()
        } else {
          if (response.message && response.message.includes('manager')) {
            this.showManagerError = true
            this.closeCreateModal()
            return
          }
          
          if (response.errors && Array.isArray(response.errors) && response.errors.length > 0) {
            this.validationErrors = response.errors
          } else {
            this.validationErrors = [response.message || 'Erreur lors du traitement de la demande']
          }
        }
      } catch (error) {
        this.validationErrors = [error.message || 'Erreur lors du traitement de la demande']
      } finally {
        this.submitting = false
      }
    },
    
    closeCreateModal() {
      this.showCreateModal = false
      this.editingRequest = null
      this.newRequest = {
        type: 'CongeAnnuel',
        dateDebut: '',
        dateFin: '',
        commentaire: ''
      }
      this.validationErrors = []
    },
    
    showRejectModal(request) {
      this.selectedRequestToReject = request
      this.rejectReason = ''
      this.showRejectModalFlag = true
    },
    
    closeRejectModal() {
      this.showRejectModalFlag = false
      this.selectedRequestToReject = null
      this.rejectReason = ''
    },
    
    async confirmReject() {
      if (!this.selectedRequestToReject || !this.rejectReason.trim()) {
        return
      }
      
      await this.updateRequestStatus(this.selectedRequestToReject.id, 'Refuse', this.rejectReason)
      this.closeRejectModal()
    },
    
    async updateRequestStatus(id, status, commentaire = null) {
      this.updatingStatusId = id
      
      try {
        const response = await vacationService.updateVacationRequestStatus(id, status, commentaire)
        if (response.success) {
          this.showToast(response.message || 'Statut mis à jour avec succès', 'success')
          await this.loadData()
        } else {
          throw new Error(response.message)
        }
      } catch (error) {
        this.showToast(error.message || 'Erreur lors de la mise à jour du statut', 'error')
      } finally {
        this.updatingStatusId = null
      }
    },
    
    checkManagerBeforeCreate() {
      this.showCreateModal = true
    },
    
    closeManagerError() {
      this.showManagerError = false
    }
  }
}
</script>

<style scoped>
.vacation-requests {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
  position: relative;
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

.toast-close:hover {
  color: rgba(0, 0, 0, 0.6);
  background: rgba(0, 0, 0, 0.05);
}

@keyframes slide-in-right {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2rem;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.balance-card {
  background: linear-gradient(135deg, #FFFFFF 0%, #F8FAFC 100%);
  border: 2px solid rgba(246, 201, 0, 0.2);
  border-radius: 20px;
  padding: 1.5rem;
  box-shadow: 0 8px 30px rgba(246, 201, 0, 0.15);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  min-width: 280px;
  position: relative;
  overflow: hidden;
}

.balance-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(90deg, #F6C900 0%, #F6BF00 100%);
}

.balance-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 40px rgba(246, 201, 0, 0.25);
  border-color: rgba(246, 201, 0, 0.4);
}

.balance-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.balance-main {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.balance-icon {
  width: 48px;
  height: 48px;
  background: linear-gradient(135deg, #F6C900 0%, #F6BF00 100%);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #1D1D1D;
  font-size: 1.25rem;
  box-shadow: 0 4px 15px rgba(246, 201, 0, 0.3);
  flex-shrink: 0;
}

.balance-details {
  flex: 1;
  min-width: 0;
}

.balance-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: rgba(29, 29, 29, 0.7);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  display: block;
  margin-bottom: 0.25rem;
}

.balance-numbers {
  display: flex;
  align-items: baseline;
  gap: 0.5rem;
  margin-bottom: 0.25rem;
}

.balance-value {
  font-size: 2rem;
  font-weight: 800;
  color: #1D1D1D;
  line-height: 1;
}

.balance-unit {
  font-size: 1rem;
  font-weight: 600;
  color: rgba(29, 29, 29, 0.6);
}

.balance-period {
  font-size: 0.75rem;
  color: rgba(29, 29, 29, 0.5);
  font-weight: 500;
}

.balance-progress {
  margin-top: 0.5rem;
}

.progress-bar {
  width: 100%;
  height: 6px;
  background: rgba(29, 29, 29, 0.1);
  border-radius: 3px;
  overflow: hidden;
  margin-bottom: 0.5rem;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #F6C900 0%, #F6BF00 100%);
  border-radius: 3px;
  transition: width 0.5s ease;
  position: relative;
}

.progress-fill::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(90deg, transparent 0%, rgba(255, 255, 255, 0.3) 50%, transparent 100%);
  animation: shimmer 2s infinite;
}

@keyframes shimmer {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(100%); }
}

.progress-text {
  font-size: 0.75rem;
  color: rgba(29, 29, 29, 0.6);
  text-align: center;
  font-weight: 500;
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

.create-btn {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #F6C900 0%, #F6BF00 100%);
  color: #1D1D1D;
  border: none;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 15px rgba(246, 201, 0, 0.3);
}

.create-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(246, 201, 0, 0.4);
}

.create-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.loading-state, .error-state {
  text-align: center;
  padding: 3rem;
  color: rgba(29, 29, 29, 0.6);
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(246, 201, 0, 0.1);
  border-top: 4px solid #F6C900;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.retry-btn {
  margin-top: 1rem;
  padding: 0.5rem 1rem;
  background: #F6C900;
  color: #1D1D1D;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 500;
}

.content-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 2rem;
}

.panel {
  background: #FFFFFF;
  border-radius: 24px;
  padding: 2rem;
  box-shadow: 0 4px 20px rgba(29, 29, 29, 0.08);
  border: 1px solid rgba(246, 201, 0, 0.1);
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

.filter-tabs {
  display: flex;
  gap: 0.5rem;
}

.filter-tab {
  padding: 0.5rem 1rem;
  border: 1px solid rgba(246, 201, 0, 0.2);
  background: transparent;
  border-radius: 20px;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.3s ease;
  color: rgba(29, 29, 29, 0.7);
}

.filter-tab.active {
  background: linear-gradient(135deg, #F6C900 0%, #F6BF00 100%);
  color: #1D1D1D;
  border-color: #F6C900;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  color: rgba(29, 29, 29, 0.5);
}

.empty-state i {
  font-size: 3rem;
  margin-bottom: 1rem;
  color: rgba(246, 201, 0, 0.3);
}

.request-card {
  background: rgba(246, 201, 0, 0.03);
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 16px;
  padding: 1.5rem;
  margin-bottom: 1rem;
  transition: all 0.3s ease;
}

.request-card:hover {
  background: rgba(246, 201, 0, 0.08);
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(29, 29, 29, 0.1);
}

.request-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.date-range {
  font-weight: 600;
  color: #1D1D1D;
  font-size: 1.1rem;
  margin-bottom: 0.25rem;
}

.duration {
  color: rgba(29, 29, 29, 0.6);
  font-size: 0.875rem;
}

.request-status {
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 0.875rem;
  font-weight: 600;
  text-transform: capitalize;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.request-status.approuve {
  background: rgba(74, 222, 128, 0.1);
  color: #16A34A;
}

.request-status.refuse {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
}

.request-status.enattente {
  background: rgba(251, 146, 60, 0.1);
  color: #EA580C;
}

.request-body {
  margin-bottom: 1rem;
}

.request-type {
  font-weight: 600;
  color: #F6BF00;
  font-size: 0.875rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 0.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.request-reason {
  color: rgba(29, 29, 29, 0.7);
  line-height: 1.5;
}

.request-footer .manager-comment {
  padding: 1rem;
  background: rgba(246, 201, 0, 0.05);
  border-radius: 12px;
  border-left: 4px solid #F6C900;
  color: rgba(29, 29, 29, 0.8);
  font-size: 0.9rem;
}

.employee-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.employee-avatar {
  width: 40px;
  height: 40px;
  border-radius: 8px;
  background: linear-gradient(135deg, #F6C900 0%, #F6BF00 100%);
  color: #1D1D1D;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 0.875rem;
}

.employee-name {
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 0.25rem;
}

.request-details {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 0.5rem;
}

.request-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1rem;
}

.edit-btn, .delete-btn, .approve-btn, .reject-btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
  font-size: 0.875rem;
}

.edit-btn {
  background: rgba(59, 130, 246, 0.1);
  color: #2563EB;
}

.edit-btn:hover {
  background: rgba(59, 130, 246, 0.2);
}

.delete-btn {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
}

.delete-btn:hover:not(:disabled) {
  background: rgba(248, 113, 113, 0.2);
}

.approve-btn {
  background: rgba(74, 222, 128, 0.1);
  color: #16A34A;
}

.approve-btn:hover:not(:disabled) {
  background: rgba(74, 222, 128, 0.2);
}

.reject-btn {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
}

.reject-btn:hover:not(:disabled) {
  background: rgba(248, 113, 113, 0.2);
}

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

.modal-content {
  background: #FFFFFF;
  border-radius: 24px;
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  animation: slide-up 0.3s cubic-bezier(0.4, 0, 0.2, 1);
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
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
}

.close-btn:hover {
  color: #1D1D1D;
  background: rgba(29, 29, 29, 0.05);
}

.modal-body {
  padding: 2rem;
}

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
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid rgba(246, 201, 0, 0.2);
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.3s ease;
  font-family: inherit;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #F6C900;
  box-shadow: 0 0 0 3px rgba(246, 201, 0, 0.1);
}

.duration-info {
  background: rgba(246, 201, 0, 0.05);
  padding: 0.75rem;
  border-radius: 8px;
  border-left: 4px solid #F6C900;
  margin-bottom: 1.5rem;
  font-weight: 500;
  color: #1D1D1D;
}

.validation-errors {
  margin-bottom: 1.5rem;
}

.error-message {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
  padding: 0.75rem;
  border-radius: 8px;
  margin-bottom: 0.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
  margin-top: 2rem;
}

.cancel-btn, .submit-btn {
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 1rem;
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

.cancel-btn:hover {
  background: rgba(29, 29, 29, 0.15);
}

.submit-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 4px 15px rgba(246, 201, 0, 0.3);
}

.submit-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

/* Manager Error Modal Styles */
.manager-error-modal {
  max-width: 600px;
}

.manager-error-modal .modal-header {
  position: relative;
  background: linear-gradient(135deg, #FEF3C7 0%, #FDE68A 100%);
  border-bottom: 1px solid rgba(245, 158, 11, 0.2);
  padding: 2rem;
  text-align: center;
  border-radius: 24px 24px 0 0;
}

.error-icon {
  width: 80px;
  height: 80px;
  margin: 0 auto 1rem;
  background: linear-gradient(135deg, #F59E0B 0%, #D97706 100%);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  color: white;
  box-shadow: 0 8px 20px rgba(245, 158, 11, 0.3);
}

.manager-error-modal .close-btn {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: rgba(255, 255, 255, 0.9);
}

.manager-error-content {
  text-align: center;
  padding: 2rem;
}

.manager-error-content h2 {
  font-size: 1.75rem;
  font-weight: 700;
  color: #92400E;
  margin-bottom: 1rem;
}

.error-description {
  font-size: 1.1rem;
  color: rgba(29, 29, 29, 0.7);
  line-height: 1.6;
  margin-bottom: 2rem;
}

.error-details {
  background: rgba(245, 158, 11, 0.05);
  border: 1px solid rgba(245, 158, 11, 0.1);
  border-radius: 16px;
  padding: 1.5rem;
  margin-bottom: 2rem;
}

.detail-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1rem;
  padding: 0.75rem;
  background: rgba(245, 158, 11, 0.05);
  border-radius: 12px;
  color: #92400E;
  font-weight: 500;
}

.detail-item:last-child {
  margin-bottom: 0;
}

.detail-item i {
  font-size: 1.1rem;
  color: #F59E0B;
  width: 20px;
  text-align: center;
}

.contact-info {
  text-align: left;
  background: rgba(59, 130, 246, 0.05);
  border: 1px solid rgba(59, 130, 246, 0.1);
  border-radius: 16px;
  padding: 1.5rem;
  margin-bottom: 2rem;
}

.contact-info h4 {
  color: #1D4ED8;
  font-weight: 600;
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.contact-info ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.contact-info li {
  padding: 0.5rem 0;
  padding-left: 1.5rem;
  position: relative;
  color: rgba(29, 29, 29, 0.8);
  line-height: 1.5;
}

.contact-info li:before {
  content: '•';
  color: #3B82F6;
  font-weight: bold;
  position: absolute;
  left: 0.5rem;
}

.understand-btn {
  background: linear-gradient(135deg, #F59E0B 0%, #D97706 100%);
  color: white;
  border: none;
  padding: 0.875rem 2rem;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
  font-size: 1rem;
  margin: 0 auto;
  min-width: 160px;
  justify-content: center;
}

.understand-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(245, 158, 11, 0.4);
  background: linear-gradient(135deg, #D97706 0%, #B45309 100%);
}

.understand-btn:active {
  transform: translateY(0);
}

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
  .content-grid {
    grid-template-columns: 1fr;
  }
  
  .toast {
    right: 10px;
    left: 10px;
    min-width: auto;
  }
}

@media (max-width: 768px) {
  .vacation-requests {
    padding: 0.5rem;
  }
  
  .page-header {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
  
  .header-actions {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
  
  .balance-card {
    min-width: auto;
    width: 100%;
  }
  
  .balance-main {
    gap: 0.75rem;
  }
  
  .balance-icon {
    width: 40px;
    height: 40px;
    font-size: 1.1rem;
  }
  
  .balance-value {
    font-size: 1.75rem;
  }
  
  .panel {
    padding: 1rem;
  }
  
  .panel-header {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
  
  .filter-tabs {
    justify-content: center;
  }
  
  .form-row {
    grid-template-columns: 1fr;
  }
  
  .request-actions {
    flex-direction: column;
  }
  
  .modal-actions {
    flex-direction: column;
  }
  
  .request-header {
    flex-direction: column;
    gap: 1rem;
  }
  
  .request-details {
    flex-direction: column;
    gap: 0.5rem;
    align-items: flex-start;
  }
  
  .toast {
    top: 10px;
    right: 10px;
    left: 10px;
    min-width: auto;
  }
}

@media (max-width: 480px) {
  .page-title {
    font-size: 1.75rem;
  }
  
  .page-subtitle {
    font-size: 1rem;
  }
  
  .modal-content {
    width: 95%;
    margin: 1rem;
  }
  
  .manager-error-modal {
    max-width: 95%;
  }
  
  .manager-error-content h2 {
    font-size: 1.5rem;
  }
  
  .error-description {
    font-size: 1rem;
  }
  
  .error-icon {
    width: 60px;
    height: 60px;
    font-size: 1.5rem;
  }
  
  .modal-header,
  .modal-body {
    padding: 1.5rem;
  }
  
  .balance-card {
    padding: 1rem;
  }
  
  .balance-value {
    font-size: 1.5rem;
  }
  
  .balance-unit {
    font-size: 0.875rem;
  }
}
</style>