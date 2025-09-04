<template>
  <div class="team-management">
    <header class="page-header">
      <div class="header-content">
        <h1 class="page-title">Team Management</h1>
        <p class="page-subtitle">Review and approve team requests</p>
      </div>
    </header>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <div class="loading-spinner"></div>
      <p>Loading pending requests...</p>
    </div>

    <!-- Error State -->
    <div v-if="error" class="error-container">
      <div class="error-message">
        <i class="fas fa-exclamation-triangle"></i>
        {{ error }}
      </div>
      <button @click="loadData" class="retry-btn">
        <i class="fas fa-redo"></i>
        Retry
      </button>
    </div>

    <div v-if="!loading && !error" class="management-grid">
      <!-- Pending Approvals -->
      <div class="panel pending-panel">
        <h3 class="panel-title">
          <i class="fas fa-clock"></i>
          Pending Approvals
        </h3>
        
        <div v-if="pendingApprovals.length === 0" class="no-requests">
          <i class="fas fa-check-circle"></i>
          <p>No pending requests to review</p>
        </div>
        
        <div v-else class="approval-list">
          <div v-for="item in pendingApprovals" :key="item.id" class="approval-card">
            <div class="approval-header">
              <div class="employee-info">
                <div class="employee-avatar">
                  {{ getInitials(item.employeeName) }}
                </div>
                <div>
                  <div class="employee-name">{{ item.employeeName }}</div>
                  <div class="request-type">{{ item.type }}</div>
                </div>
              </div>
              <div class="request-badge" :class="item.category">
                <i :class="getCategoryIcon(item.category)"></i>
                {{ item.category }}
              </div>
            </div>
            
            <div class="approval-content">
              <div v-if="item.category === 'vacation'" class="vacation-details">
                <div class="date-range">
                  {{ formatDate2(item.dateDebut) }} --> {{ formatDate2(item.dateFin) }}
                </div>
                <div class="duration">{{ calculateDuration(item.dateDebut, item.dateFin) }} days</div>
                <div class="reason">{{ item.commentaire || 'No comment provided' }}</div>
              </div>
              
              <div v-else-if="item.category === 'expense'" class="expense-details">
                <div class="expense-header">
                  <div class="expense-amount">${{ formatCurrency(item.totalMontant || 0) }}</div>
                  <div class="expense-project" v-if="item.projetNom">{{ item.projetNom }}</div>
                </div>
                
                <div class="expense-summary">
                  <span class="expense-count">{{ item.lignes ? item.lignes.length : 0 }} expense item(s)</span>
                  <button 
                    class="toggle-details-btn" 
                    @click="toggleExpenseDetails(item.id)"
                    :class="{ active: expandedExpenses.includes(item.id) }"
                  >
                    <i :class="expandedExpenses.includes(item.id) ? 'fas fa-chevron-up' : 'fas fa-chevron-down'"></i>
                    {{ expandedExpenses.includes(item.id) ? 'Hide Details' : 'View Details' }}
                  </button>
                </div>
                
                <!-- Expense Lines Details -->
                <div 
                  v-if="expandedExpenses.includes(item.id)" 
                  class="expense-lines-container"
                >
                  <div class="expense-lines-header">
                    <h4>Expense Breakdown</h4>
                  </div>
                  
                  <div v-if="!item.lignes || item.lignes.length === 0" class="no-expense-lines">
                    <i class="fas fa-info-circle"></i>
                    <span>No expense lines available</span>
                  </div>
                  
                  <div v-else class="expense-lines-list">
                    <div 
                      v-for="(ligne, index) in item.lignes" 
                      :key="index" 
                      class="expense-line-item"
                    >
                      <div class="expense-line-main">
                        <div class="expense-line-info">
                          <div class="expense-line-description">
                            {{ ligne.description || 'No description' }}
                          </div>
                          <div class="expense-line-meta">
                            <span v-if="ligne.date " class="expense-line-date">
                              <i class="fas fa-calendar"></i>
                              {{ formatDate2(ligne.date)  }}
                            </span>
                            
                            <span v-if="ligne.projetNom || item.projetNom" class="expense-line-project">
                              <i class="fas fa-folder"></i>
                              {{ ligne.projetNom || item.projetNom }}
                            </span>
                            <span v-if="ligne.tarifKmId" class="expense-line-rate">
                              <i class="fas fa-road"></i>
                              {{ ligne.distanceKm  }} km
                            </span>
                          </div>
                        </div>
                        <div class="expense-line-amount">
                          ${{ formatCurrency(ligne.montant || 0) }}
                        </div>
                      </div>
                      
                      <!-- Additional expense details -->
                      <div class="expense-line-extras">
                        <!-- Justificatif/Receipt indicator -->
                        <div v-if="ligne.justificatifPath || ligne.recu || ligne.document" class="expense-line-attachments">
                          <i class="fas fa-paperclip"></i>
                          <span v-if="ligne.justificatifPath">
                            Justificatif: {{ getFileName(ligne.justificatifPath) }}
                          </span>
                          <span v-else>Receipt attached</span>
                        </div>
                        
                        <!-- Distance for travel expenses -->
                        <div v-if="ligne.distance" class="expense-line-distance">
                          <i class="fas fa-route"></i>
                          <span>Distance: {{ ligne.distance }} km</span>
                        </div>
                        
                        <!-- Additional notes -->
                        <div v-if="ligne.notes || ligne.commentaire" class="expense-line-notes">
                          <i class="fas fa-sticky-note"></i>
                          <span>{{ ligne.notes || ligne.commentaire }}</span>
                        </div>
                      </div>
                    </div>
                    
                    <!-- Total Summary -->
                    <div class="expense-lines-total">
                      <div class="total-label">Total Amount:</div>
                      <div class="total-amount">${{ formatCurrency(item.totalMontant || 0) }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="approval-actions">
              <button 
                class="approve-btn" 
                @click="approveItem(item)"
                :disabled="processingItems[item.id]"
              >
                <i v-if="processingItems[item.id]" class="fas fa-spinner fa-spin"></i>
                <i v-else class="fas fa-check"></i>
                {{ processingItems[item.id] ? 'Processing...' : 'Approve' }}
              </button>
              <button 
                class="reject-btn" 
                @click="showRejectModal(item)"
                :disabled="processingItems[item.id]"
              >
                <i class="fas fa-times"></i>
                Reject
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Team Overview -->
      <div class="panel team-overview-panel">
        <h3 class="panel-title">
          <i class="fas fa-users"></i>
          Team Overview
        </h3>
        <div class="team-stats">
          <div class="stat-card">
            <div class="stat-number">{{ teamStats.totalMembers }}</div>
            <div class="stat-label">Team Members</div>
          </div>
          <div class="stat-card">
            <div class="stat-number">{{ teamStats.onLeave }}</div>
            <div class="stat-label">On Leave</div>
          </div>
          <div class="stat-card">
            <div class="stat-number">{{ teamStats.pendingRequests }}</div>
            <div class="stat-label">Pending</div>
          </div>
        </div>
        
        <div v-if="loadingTeam" class="team-loading">
          <div class="loading-spinner small"></div>
          <p>Loading team members...</p>
        </div>
        
        <div v-else class="team-members">
          <div v-for="member in teamMembers" :key="member.id" class="member-card">
            <div class="member-info">
              <div class="member-avatar">
                {{ getInitials(member.name) }}
              </div>
              <div>
                <div class="member-name">{{ member.name }}</div>
                <div class="member-role">{{ member.role }}</div>
                <div v-if="member.vacationInfo" class="member-vacation-info">
                  <span class="vacation-dates">{{ member.vacationInfo }}</span>
                </div>
              </div>
            </div>
            <div class="member-status" :class="member.status">
              <i :class="getStatusIcon(member.status)"></i>
              {{ formatMemberStatus(member.status) }}
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Actions -->
      <div class="panel recent-actions-panel">
        <h3 class="panel-title">
          <i class="fas fa-history"></i>
          Recent Actions (may be added in the future)
        </h3>
        <div class="actions-timeline">
          <div v-for="action in recentActions" :key="action.id" class="timeline-item">
            <div class="timeline-marker" :class="action.type">
              <i :class="getActionIcon(action.type)"></i>
            </div>
            <div class="timeline-content">
              <div class="action-title">{{ action.title }}</div>
              <div class="action-details">{{ action.details }}</div>
              <div class="action-time">{{ formatDateTime(action.timestamp) }}</div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Reject Modal -->
    <div v-if="showRejectConfirm" class="modal-overlay" @click="closeRejectModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Reject Request</h2>
          <button class="close-btn" @click="closeRejectModal">
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <p>Are you sure you want to reject this request from <strong>{{ selectedItem?.employeeName }}</strong>?</p>
          <div class="form-group">
            <label for="rejectReason">Reason for rejection:</label>
            <textarea 
              id="rejectReason" 
              v-model="rejectReason" 
              rows="3" 
              placeholder="Please provide a reason..."
            ></textarea>
          </div>
          <div class="modal-actions">
            <button type="button" class="cancel-btn" @click="closeRejectModal">Cancel</button>
            <button 
              type="button" 
              class="reject-confirm-btn" 
              @click="confirmReject"
              :disabled="rejectingItem"
            >
              <i v-if="rejectingItem" class="fas fa-spinner fa-spin"></i>
              {{ rejectingItem ? 'Rejecting...' : 'Reject Request' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Success/Error Notifications -->
    <div v-if="notification" class="notification" :class="notification.type">
      <i :class="notification.type === 'success' ? 'fas fa-check-circle' : 'fas fa-exclamation-circle'"></i>
      {{ notification.message }}
      <button @click="clearNotification" class="notification-close">
        <i class="fas fa-times"></i>
      </button>
    </div>
  </div>
</template>

<script>
import { vacationService } from '../../services/vacationService'
import { expenseService } from '../../services/expenseReportService'
import { userService } from '../../services/userService'

export default {
  name: 'TeamManagement',
  props: {
    user: Object
  },
  data() {
    return {
      loading: true,
      loadingTeam: false,
      error: null,
      showRejectConfirm: false,
      selectedItem: null,
      rejectReason: '',
      rejectingItem: false,
      processingItems: {},
      notification: null,
      expandedExpenses: [],
      
      pendingApprovals: [],
      pendingVacationRequests: [],
      pendingExpenseReports: [],
      allVacationRequests: [], // Store all vacation requests for team analysis
      teamStats: {
        totalMembers: 0,
        onLeave: 0,
        pendingRequests: 0
      },
      teamMembers: [],
      recentActions: []
    }
  },
  
  async mounted() {
    await this.loadData()
  },
  
  methods: {
    async loadData() {
      this.loading = true
      this.error = null
      
      try {
        const [vacationResult, expenseResult] = await Promise.all([
          this.loadPendingVacationRequests(),
          this.loadPendingExpenseReports()
        ])
        
        if (!vacationResult.success && !expenseResult.success) {
          this.error = 'Failed to load data. Please try again.'
        } else {
          this.combinePendingRequests()
          await this.updateTeamStats()
        }
      } catch (error) {
        console.error('Error loading data:', error)
        this.error = 'An unexpected error occurred. Please try again.'
      } finally {
        this.loading = false
      }
    },
    
    async loadPendingVacationRequests() {
      try {
        const result = await vacationService.getTeamVacationRequests()
        
        if (result.success && result.data) {
          const filtered = result.data.filter(request => 
            request.statut?.toLowerCase() === 'enattente'
          )
          
          // Store all vacation requests for team analysis
          this.allVacationRequests = result.data || []
          
          const processedRequests = await Promise.all(filtered.map(async (request) => {
            let employeeName = 'Unknown Employee'
            
            if (request.employeNom) {
              employeeName = request.employeNom
            } else if (request.prenom && request.nom) {
              employeeName = `${request.prenom} ${request.nom}`
            } else if (request.employeId || request.userId) {
              const userId = request.employeId || request.userId
              const userName = await userService.getUserDisplayName(userId)
              if (userName && userName !== `Employee #${userId}`) {
                employeeName = userName
              }
            }
            
            return {
              ...request,
              category: 'vacation',
              type: this.formatVacationType(request.type),
              employeeName: employeeName,
              userId: request.employeId || request.userId
            }
          }))
          
          this.pendingVacationRequests = processedRequests
          return { success: true }
        } else {
          this.pendingVacationRequests = []
          this.allVacationRequests = []
          return { success: false, message: result.message }
        }
      } catch (error) {
        console.error('Error loading vacation requests:', error)
        this.pendingVacationRequests = []
        this.allVacationRequests = []
        return { success: false, message: 'Error loading vacation requests' }
      }
    },

    async loadPendingExpenseReports() {
      try {
        const result = await expenseService.getManagerExpenseReports()
        
        if (result.success && result.data) {
          const filtered = result.data.filter(report => 
            report.statut?.toLowerCase() === 'enattente'
          )
          
          const processedReports = await Promise.all(filtered.map(async (report) => {
            let employeeName = 'Unknown Employee'
            
            if (report.employeNom) {
              employeeName = report.employeNom
            } else if (report.prenom && report.nom) {
              employeeName = `${report.prenom} ${report.nom}`
            } else if (report.employeId || report.userId) {
              const userId = report.employeId || report.userId
              const userName = await userService.getUserDisplayName(userId)
              if (userName && userName !== `Employee #${userId}`) {
                employeeName = userName
              }
            }
            
            return {
              ...report,
              category: 'expense',
              type: 'Expense Report',
              employeeName: employeeName,
              totalMontant: this.calculateExpenseTotal(report.lignes),
              userId: report.employeId || report.userId
            }
          }))
          
          this.pendingExpenseReports = processedReports
          return { success: true }
        } else {
          this.pendingExpenseReports = []
          return { success: false, message: result.message }
        }
      } catch (error) {
        console.error('Error loading expense reports:', error)
        this.pendingExpenseReports = []
        return { success: false, message: 'Error loading expense reports' }
      }
    },
    
    combinePendingRequests() {
      this.pendingApprovals = [
        ...(this.pendingVacationRequests || []),
        ...(this.pendingExpenseReports || [])
      ].sort((a, b) => new Date(b.dateSoumission || b.dateDebut) - new Date(a.dateSoumission || a.dateDebut))
    },
    
    async updateTeamStats() {
      this.loadingTeam = true
      
      try {
        // Get all unique user IDs from requests and vacation data
        const allUserIds = new Set()
        
        // Add users from pending approvals
        for (const request of this.pendingApprovals) {
          const userId = request.userId || request.employeId
          if (userId) {
            allUserIds.add(userId)
          }
        }
        
        // Add users from all vacation requests
        for (const request of this.allVacationRequests) {
          const userId = request.userId || request.employeId
          if (userId) {
            allUserIds.add(userId)
          }
        }
        
        // If we don't have enough users from requests, try to get all users
        if (allUserIds.size < 3) {
          try {
            const managersResult = await userService.getAvailableManagers()
            if (managersResult.success && managersResult.data) {
              managersResult.data.forEach(manager => allUserIds.add(manager.id))
            }
          } catch (error) {
            console.warn('Could not load additional users:', error)
          }
        }
        
        const userIdsArray = Array.from(allUserIds)
        
        // Fetch user details for all team members
        const teamMembersMap = new Map()
        const membersOnLeave = new Set()
        const currentDate = new Date()
        
        for (const userId of userIdsArray) {
          try {
            const userResult = await userService.getUserById(userId)
            if (userResult.success && userResult.data) {
              const userData = userResult.data
              const displayName = userService.formatUserDisplayName(userData)
              const role = userData.Poste || userData.poste || userData.Role || userData.role || 'Employee'
              
              // Check if user is currently on leave
              const userVacations = this.allVacationRequests.filter(req => {
                const reqUserId = req.userId || req.employeId
                return reqUserId === userId && req.statut?.toLowerCase() === 'approuve'
              })
              
              let vacationStatus = 'available'
              let vacationInfo = null
              
              for (const vacation of userVacations) {
                const startDate = new Date(vacation.dateDebut)
                const endDate = new Date(vacation.dateFin)
                
                // Check if current date is within vacation period
                if (currentDate >= startDate && currentDate <= endDate) {
                  vacationStatus = 'on-leave'
                  membersOnLeave.add(userId)
                  vacationInfo = `Until ${this.formatDate(vacation.dateFin)}`
                  break
                }
              }
              
              teamMembersMap.set(userId, {
                id: userId,
                name: displayName,
                role: role,
                status: vacationStatus,
                vacationInfo: vacationInfo
              })
            }
          } catch (error) {
            console.warn(`Failed to load user ${userId}:`, error)
          }
        }
        
        this.teamMembers = Array.from(teamMembersMap.values()).sort((a, b) => {
          // Sort by status (on-leave first, then available) and then by name
          if (a.status !== b.status) {
            return a.status === 'on-leave' ? -1 : 1
          }
          return a.name.localeCompare(b.name)
        })
        
        // Update team statistics
        this.teamStats = {
          totalMembers: this.teamMembers.length,
          onLeave: membersOnLeave.size,
          pendingRequests: this.pendingApprovals.length
        }

        
      } catch (error) {
        console.error('Error updating team stats:', error)
        this.teamStats = {
          totalMembers: 0,
          onLeave: 0,
          pendingRequests: this.pendingApprovals.length
        }
      } finally {
        this.loadingTeam = false
      }
    },
    
    formatMemberStatus(status) {
      switch(status) {
        case 'available': return 'Available'
        case 'on-leave': return 'On Leave'
        case 'busy': return 'Busy'
        default: return 'Available'
      }
    },
    
    formatDate(dateString) {
      if (!dateString) return ''
      try {
        return new Date(dateString).toLocaleDateString('en-US', {
          month: 'short',
          day: 'numeric'
        })
      } catch (error) {
        return dateString
      }
    },
    
    toggleExpenseDetails(itemId) {
      const index = this.expandedExpenses.indexOf(itemId)
      if (index > -1) {
        this.expandedExpenses.splice(index, 1)
      } else {
        this.expandedExpenses.push(itemId)
      }
    },
    
    formatCurrency(amount) {
      return new Intl.NumberFormat('en-US', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
      }).format(amount || 0)
    },
    
    getFileName(path) {
      if (!path) return ''
      return path.split('/').pop() || path.split('\\').pop() || path
    },
    
    getInitials(name) {
      if (!name || name === 'Unknown Employee') return '??'
      return name
        .split(' ')
        .map(part => part.charAt(0).toUpperCase())
        .join('')
        .substring(0, 2)
    },
    
    formatDate2(dateTime) {
      return dateTime.split('T')[0]
    },
    
    formatDateTime(date) {
      if (!date) return ''
      return new Date(date).toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    },
   
    formatVacationType(type) {
      const typeMap = {
        'CongeAnnuel': 'Annual Leave',
        'Maladie': 'Sick Leave',
        'Maternite': 'Maternity Leave',
        'Paternite': 'Paternity Leave',
        'DecesProche': 'Bereavement Leave'
      }
      return typeMap[type] || type
    },
    
    calculateDuration(startDate, endDate) {
      if (!startDate || !endDate) return 0
      const start = new Date(startDate)
      const end = new Date(endDate)
      const diffTime = Math.abs(end - start)
      return Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1
    },
    
    calculateExpenseTotal(lignes) {
      if (!Array.isArray(lignes)) return 0
      return lignes.reduce((total, ligne) => total + (ligne.montant || 0), 0)
    },
    
    getCategoryIcon(category) {
      switch(category) {
        case 'vacation': return 'fas fa-calendar-alt'
        case 'expense': return 'fas fa-receipt'
        default: return 'fas fa-circle'
      }
    },
    
    getStatusIcon(status) {
      switch(status) {
        case 'available': return 'fas fa-circle'
        case 'on-leave': return 'fas fa-plane'
        case 'busy': return 'fas fa-clock'
        default: return 'fas fa-circle'
      }
    },
    
    getActionIcon(type) {
      switch(type) {
        case 'approved': return 'fas fa-check'
        case 'rejected': return 'fas fa-times'
        case 'pending': return 'fas fa-clock'
        default: return 'fas fa-circle'
      }
    },
    
    async approveItem(item) {
      this.processingItems = {
        ...this.processingItems,
        [item.id]: true
      }
      
      try {
        let result
        
        if (item.category === 'vacation') {
          result = await vacationService.updateVacationRequestStatus(
            item.id, 
            'Approuve', 
            'Approved by manager'
          )
        } else if (item.category === 'expense') {
          result = await expenseService.updateExpenseReportStatus({
            id: item.id,
            userId: item.userId || item.employeId,
            statut: 'Approuvee'
          })
        }
        
        if (result && result.success) {
          const index = this.pendingApprovals.findIndex(p => p.id === item.id)
          if (index !== -1) {
            this.pendingApprovals.splice(index, 1)
          }
          
          this.addRecentAction({
            type: 'rejected',
            title: `${this.selectedItem.category === 'vacation' ? 'Vacation request' : 'Expense report'} rejected`,
            details: `${this.selectedItem.employeeName}'s ${this.selectedItem.type.toLowerCase()} was rejected: ${this.rejectReason}`,
            timestamp: new Date()
          })
          
          this.updateTeamStats()
          this.showNotification('Request rejected successfully', 'success')
          this.closeRejectModal()
        } else {
          this.showNotification(result?.message || 'Failed to reject request', 'error')
        }
      } catch (error) {
        console.error('Error rejecting item:', error)
        this.showNotification('An error occurred while rejecting the request', 'error')
      } finally {
        this.rejectingItem = false
      }
    },
    
    addRecentAction(action) {
      this.recentActions.unshift({
        id: Date.now() + Math.random(),
        ...action
      })
      
      if (this.recentActions.length > 10) {
        this.recentActions = this.recentActions.slice(0, 10)
      }
    },
    
    showNotification(message, type = 'info') {
      this.notification = { message, type }
      
      setTimeout(() => {
        this.clearNotification()
      }, 5000)
    },
    
    clearNotification() {
      this.notification = null
    }
  }
}
</script>

<style scoped>
.team-management {
  max-width: 1400px;
  margin: 0 auto;
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

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  text-align: center;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(246, 201, 0, 0.1);
  border-left: 4px solid #F6C900;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

.loading-spinner.small {
  width: 24px;
  height: 24px;
  border-width: 3px;
  margin-bottom: 0.5rem;
}

.team-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 2rem;
  color: rgba(29, 29, 29, 0.6);
  font-size: 0.875rem;
}

.error-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  text-align: center;
}

.error-message {
  background: rgba(248, 113, 113, 0.1);
  border: 1px solid rgba(248, 113, 113, 0.2);
  color: #DC2626;
  padding: 1rem 1.5rem;
  border-radius: 12px;
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.retry-btn {
  background: #F6C900;
  color: #1D1D1D;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.retry-btn:hover {
  background: #E6B800;
  transform: translateY(-1px);
}

.no-requests {
  text-align: center;
  padding: 3rem 2rem;
  color: rgba(29, 29, 29, 0.6);
}

.no-requests i {
  font-size: 3rem;
  color: #4ADE80;
  margin-bottom: 1rem;
  display: block;
}

.management-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-template-rows: auto auto;
  gap: 2rem;
}

.pending-panel {
  grid-column: 1 / 2;
  grid-row: 1 / 3;
}

.team-overview-panel {
  grid-column: 2 / 3;
  grid-row: 1 / 2;
}

.recent-actions-panel {
  grid-column: 2 / 3;
  grid-row: 2 / 3;
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
  gap: 0.5rem;
}

.panel-title i {
  color: #F6BF00;
}

.approval-card {
  background: rgba(246, 201, 0, 0.03);
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 16px;
  padding: 1.5rem;
  margin-bottom: 1rem;
  transition: all 0.3s ease;
}

.approval-card:hover {
  background: rgba(246, 201, 0, 0.08);
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(29, 29, 29, 0.1);
}

.approval-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.employee-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.employee-avatar, .member-avatar {
  width: 50px;
  height: 50px;
  border-radius: 12px;
  background: linear-gradient(135deg, #F6C900, #E6B800);
  color: #1D1D1D;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 1.1rem;
  text-transform: uppercase;
}

.member-avatar {
  width: 40px;
  height: 40px;
  border-radius: 8px;
  font-size: 0.9rem;
}

.employee-name {
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 0.25rem;
}

.request-type {
  color: rgba(29, 29, 29, 0.6);
  font-size: 0.875rem;
}

.request-badge {
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 0.875rem;
  font-weight: 600;
  text-transform: capitalize;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.request-badge.vacation {
  background: rgba(59, 130, 246, 0.1);
  color: #2563EB;
}

.request-badge.expense {
  background: rgba(34, 197, 94, 0.1);
  color: #16A34A;
}

.approval-content {
  margin-bottom: 1.5rem;
}

.vacation-details .date-range {
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 0.25rem;
}

.vacation-details .duration {
  color: #F6BF00;
  font-weight: 500;
  margin-bottom: 0.5rem;
}

.vacation-details .reason {
  color: rgba(29, 29, 29, 0.7);
  line-height: 1.5;
}

.expense-details {
  space-y: 1rem;
}

.expense-header {
  margin-bottom: 1rem;
}

.expense-details .expense-amount {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1D1D1D;
  margin-bottom: 0.25rem;
}

.expense-details .expense-project {
  color: #F6BF00;
  font-weight: 500;
  margin-bottom: 0.5rem;
}

.expense-summary {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.expense-count {
  color: rgba(29, 29, 29, 0.7);
  font-size: 0.875rem;
}

.toggle-details-btn {
  background: rgba(246, 201, 0, 0.1);
  color: #F6BF00;
  border: 1px solid rgba(246, 201, 0, 0.2);
  padding: 0.5rem 1rem;
  border-radius: 8px;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.toggle-details-btn:hover {
  background: rgba(246, 201, 0, 0.2);
  transform: translateY(-1px);
}

.toggle-details-btn.active {
  background: rgba(246, 201, 0, 0.15);
  color: #E6B800;
}

.expense-lines-container {
  background: rgba(246, 201, 0, 0.02);
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 12px;
  padding: 1.5rem;
  margin-top: 1rem;
  animation: slideDown 0.3s ease;
}

.expense-lines-header {
  margin-bottom: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid rgba(246, 201, 0, 0.1);
}

.expense-lines-header h4 {
  font-size: 1rem;
  font-weight: 600;
  color: #1D1D1D;
  margin: 0;
}

.no-expense-lines {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 2rem;
  color: rgba(29, 29, 29, 0.5);
  font-style: italic;
}

.expense-lines-list {
  space-y: 1rem;
}

.expense-line-item {
  background: #FFFFFF;
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 8px;
  padding: 1rem;
  margin-bottom: 1rem;
  transition: all 0.3s ease;
}

.expense-line-item:hover {
  background: rgba(246, 201, 0, 0.02);
  box-shadow: 0 2px 8px rgba(29, 29, 29, 0.05);
}

.expense-line-main {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.75rem;
}

.expense-line-info {
  flex: 1;
}

.expense-line-description {
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 0.5rem;
  line-height: 1.4;
}

.expense-line-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
}

.expense-line-date,
.expense-line-category,
.expense-line-project,
.expense-line-rate {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.875rem;
  color: rgba(29, 29, 29, 0.6);
}

.expense-line-date i,
.expense-line-category i,
.expense-line-project i,
.expense-line-rate i {
  font-size: 0.75rem;
  color: #F6BF00;
}

.expense-line-amount {
  font-size: 1.25rem;
  font-weight: 700;
  color: #1D1D1D;
  text-align: right;
  margin-left: 1rem;
}

.expense-line-extras {
  margin-top: 0.75rem;
  padding-top: 0.75rem;
  border-top: 1px solid rgba(246, 201, 0, 0.05);
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.expense-line-attachments,
.expense-line-notes,
.expense-line-distance {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: rgba(29, 29, 29, 0.6);
}

.expense-line-attachments i,
.expense-line-notes i,
.expense-line-distance i {
  color: #F6BF00;
  font-size: 0.75rem;
}

.expense-lines-total {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 2px solid rgba(246, 201, 0, 0.2);
  background: rgba(246, 201, 0, 0.05);
  border-radius: 8px;
  padding: 1rem;
}

.total-label {
  font-size: 1.1rem;
  font-weight: 600;
  color: #1D1D1D;
}

.total-amount {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1D1D1D;
}

.approval-actions {
  display: flex;
  gap: 1rem;
}

.approve-btn, .reject-btn {
  flex: 1;
  padding: 0.75rem 1rem;
  border: none;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.approve-btn:disabled, .reject-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.approve-btn {
  background: rgba(74, 222, 128, 0.1);
  color: #16A34A;
}

.approve-btn:hover:not(:disabled) {
  background: rgba(74, 222, 128, 0.2);
  transform: translateY(-1px);
}

.reject-btn {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
}

.reject-btn:hover:not(:disabled) {
  background: rgba(248, 113, 113, 0.2);
  transform: translateY(-1px);
}

.team-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: rgba(246, 201, 0, 0.05);
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 12px;
  padding: 1.25rem;
  text-align: center;
}

.stat-number {
  font-size: 2rem;
  font-weight: 700;
  color: #1D1D1D;
  line-height: 1;
  margin-bottom: 0.5rem;
}

.stat-label {
  color: rgba(29, 29, 29, 0.6);
  font-size: 0.875rem;
  font-weight: 500;
}

.team-members {
  space-y: 0.75rem;
}

.member-card {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: rgba(246, 201, 0, 0.03);
  border: 1px solid rgba(246, 201, 0, 0.1);
  border-radius: 12px;
  padding: 1rem;
  margin-bottom: 0.75rem;
  transition: all 0.3s ease;
}

.member-card:hover {
  background: rgba(246, 201, 0, 0.08);
}

.member-info {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex: 1;
}

.member-name {
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 0.25rem;
}

.member-role {
  color: rgba(29, 29, 29, 0.6);
  font-size: 0.875rem;
  margin-bottom: 0.25rem;
}

.member-vacation-info {
  font-size: 0.75rem;
  color: #EA580C;
  font-weight: 500;
}

.vacation-dates {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.vacation-dates::before {
  content: '✈️';
  font-size: 0.7rem;
}

.member-status {
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: capitalize;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  white-space: nowrap;
}

.member-status.available {
  background: rgba(74, 222, 128, 0.1);
  color: #16A34A;
}

.member-status.on-leave {
  background: rgba(251, 146, 60, 0.1);
  color: #EA580C;
}

.member-status.busy {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
}

.actions-timeline {
  space-y: 1rem;
}

.timeline-item {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  margin-bottom: 1rem;
}

.timeline-marker {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.875rem;
  flex-shrink: 0;
}

.timeline-marker.approved {
  background: rgba(74, 222, 128, 0.1);
  color: #16A34A;
}

.timeline-marker.rejected {
  background: rgba(248, 113, 113, 0.1);
  color: #DC2626;
}

.timeline-marker.pending {
  background: rgba(251, 146, 60, 0.1);
  color: #EA580C;
}

.timeline-content {
  flex: 1;
}

.action-title {
  font-weight: 600;
  color: #1D1D1D;
  margin-bottom: 0.25rem;
}

.action-details {
  color: rgba(29, 29, 29, 0.7);
  font-size: 0.875rem;
  line-height: 1.4;
  margin-bottom: 0.25rem;
}

.action-time {
  color: rgba(29, 29, 29, 0.5);
  font-size: 0.75rem;
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
}

.close-btn:hover {
  color: #1D1D1D;
}

.modal-body {
  padding: 2rem;
}

.modal-body p {
  margin-bottom: 1.5rem;
  color: rgba(29, 29, 29, 0.8);
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

.form-group textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid rgba(246, 201, 0, 0.2);
  border-radius: 8px;
  font-size: 1rem;
  resize: vertical;
  transition: border-color 0.3s ease;
}

.form-group textarea:focus {
  outline: none;
  border-color: #F6C900;
  box-shadow: 0 0 0 3px rgba(246, 201, 0, 0.1);
}

.modal-actions {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
}

.cancel-btn, .reject-confirm-btn {
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.cancel-btn {
  background: rgba(29, 29, 29, 0.1);
  color: #1D1D1D;
  border: 1px solid rgba(29, 29, 29, 0.2);
}

.reject-confirm-btn {
  background: rgba(248, 113, 113, 0.9);
  color: white;
  border: none;
}

.reject-confirm-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.cancel-btn:hover {
  background: rgba(29, 29, 29, 0.15);
}

.reject-confirm-btn:hover:not(:disabled) {
  background: rgba(248, 113, 113, 1);
  transform: translateY(-1px);
}

.notification {
  position: fixed;
  top: 2rem;
  right: 2rem;
  padding: 1rem 1.5rem;
  border-radius: 12px;
  color: white;
  font-weight: 500;
  z-index: 1001;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  animation: slide-in-right 0.3s ease;
  max-width: 400px;
}

.notification.success {
  background: linear-gradient(135deg, #10B981, #059669);
}

.notification.error {
  background: linear-gradient(135deg, #EF4444, #DC2626);
}

.notification-close {
  background: none;
  border: none;
  color: white;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 4px;
  margin-left: auto;
}

.notification-close:hover {
  background: rgba(255, 255, 255, 0.2);
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
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

@keyframes slide-in-right {
  from {
    opacity: 0;
    transform: translateX(100%);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 1024px) {
  .management-grid {
    grid-template-columns: 1fr;
    grid-template-rows: auto;
  }
  
  .pending-panel,
  .team-overview-panel,
  .recent-actions-panel {
    grid-column: 1;
    grid-row: auto;
  }
}

@media (max-width: 768px) {
  .team-stats {
    grid-template-columns: 1fr;
  }
  
  .approval-actions {
    flex-direction: column;
  }
  
  .modal-actions {
    flex-direction: column;
  }
  
  .notification {
    top: 1rem;
    right: 1rem;
    left: 1rem;
    max-width: none;
  }
  
  .expense-line-main {
    flex-direction: column;
    gap: 0.75rem;
  }
  
  .expense-line-amount {
    text-align: left;
    margin-left: 0;
  }
  
  .expense-summary {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
  
  .member-info {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
  
  .member-card {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }
  
  .member-status {
    align-self: flex-end;
  }
}
</style>