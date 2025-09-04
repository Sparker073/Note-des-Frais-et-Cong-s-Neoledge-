<template>
  <nav class="sidebar" :class="{ collapsed: isCollapsed }">
    <!-- Toggle Button inside sidebar -->
    <button 
      @click="$emit('toggle')" 
      class="sidebar-toggle-internal"
      aria-label="Toggle sidebar"
    >
      <i class="fas fa-bars" v-if="isCollapsed"></i>
      <i class="fas fa-times" v-else></i>
    </button>
    
    <div class="sidebar-header">
      <div class="logo" v-show="!isCollapsed">
        <div class="neo-logo">
          <!-- N Block -->
          <div class="neo-block neo-n">N</div>
          <!-- E Block -->
          <div class="neo-block neo-e">E</div>
          <!-- O Block -->
          <div class="neo-block neo-o">O</div>
          <!-- Ledge Text -->
          <span class="ledge-text">Ledge</span>
        </div>
      </div>
      
      <!-- Collapsed logo -->
      <div class="logo-collapsed" v-show="isCollapsed">
        <div class="neo-logo-collapsed">
          <div class="neo-circle">
            <div class="neo-circle-half"></div>
          </div>
        </div>
      </div>
      
      <div class="user-profile" v-show="!isCollapsed">
        <div class="avatar-wrapper">
          <div class="avatar text-avatar" :style="avatarStyle">
            {{ userInitials }}
          </div>
          <div class="status-indicator"></div>
        </div>
        <div class="user-info">
          <h3>{{ displayName }}</h3>
          <p>{{ userRole }}</p>
        </div>
      </div>
      
      <!-- Collapsed user profile -->
      <div class="user-profile-collapsed" v-show="isCollapsed">
        <div class="avatar-wrapper">
          <div class="avatar text-avatar" :style="avatarStyle">
            {{ userInitials }}
          </div>
          <div class="status-indicator"></div>
        </div>
      </div>
    </div>

    <div class="nav-menu">
      <div 
        v-for="item in menuItems" 
        :key="item.id"
        class="nav-item"
        :class="{ active: isCurrentRoute(item.id) }"
        @click="navigateToSection(item.id)"
        :title="isCollapsed ? item.label : ''"
      >
        <div class="nav-icon">
          <i :class="item.icon"></i>
        </div>
        <span class="nav-label" v-show="!isCollapsed">{{ item.label }}</span>
        <div class="nav-indicator"></div>
      </div>
    </div>

    <div class="sidebar-footer">
      <div class="quick-stats" v-show="!isCollapsed">
        <div class="stat-item" :class="{ loading: isLoadingStats }">
          <div class="stat-number">
            <span v-if="!isLoadingStats">{{ pendingCount }}</span>
            <div v-else class="stat-spinner"></div>
          </div>
          <div class="stat-label">Pending</div>
        </div>
        <div class="stat-divider"></div>
        <div class="stat-item" :class="{ loading: isLoadingStats }">
          <div class="stat-number">
            <span v-if="!isLoadingStats">{{ approvedCount }}</span>
            <div v-else class="stat-spinner"></div>
          </div>
          <div class="stat-label">Approved</div>
        </div>
      </div>
      
      <!-- Collapsed stats -->
      <div class="quick-stats-collapsed" v-show="isCollapsed">
        <div class="stat-dot" :title="`${pendingCount} Pending`" :class="{ loading: isLoadingStats }">
          <span v-if="!isLoadingStats">{{ pendingCount }}</span>
          <div v-else class="stat-spinner-small"></div>
        </div>
        <div class="stat-dot" :title="`${approvedCount} Approved`" :class="{ loading: isLoadingStats }">
          <span v-if="!isLoadingStats">{{ approvedCount }}</span>
          <div v-else class="stat-spinner-small"></div>
        </div>
      </div>
      
      <!-- Logout Button -->
      <div class="logout-section">
        <button 
          @click="handleLogout" 
          class="logout-button"
          :class="{ collapsed: isCollapsed }"
          :disabled="isLoggingOut"
          :title="isCollapsed ? (isLoggingOut ? 'Logging out...' : 'Sign Out') : ''"
        >
          <div class="logout-icon">
            <i v-if="!isLoggingOut" class="fas fa-sign-out-alt"></i>
            <div v-else class="logout-spinner"></div>
          </div>
          <span class="logout-text" v-show="!isCollapsed">
            {{ isLoggingOut ? 'Logging out...' : 'Sign Out' }}
          </span>
        </button>
      </div>
    </div>
  </nav>
</template>

<script>
import { computed, ref, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { authService } from '../services/authService'
import { userService } from '../services/userService'
import { expenseService } from '../services/expenseReportService'
import { vacationService } from '../services/vacationService'

export default {
  name: 'Sidebar',
  props: {
    isCollapsed: {
      type: Boolean,
      default: false
    }
  },
  emits: ['navigate', 'toggle'],
  setup(props, { emit }) {
    const route = useRoute()
    const router = useRouter()
    const isLoggingOut = ref(false)
    const currentUser = ref(null)
    const isLoadingStats = ref(false)
    let unsubscribe = null
    
    const menuItems = [
      { id: 'dashboard', label: 'Dashboard', icon: 'fas fa-th-large' },
      { id: 'vacation', label: 'Vacation Requests', icon: 'fas fa-calendar-alt' },
      { id: 'expenses', label: 'Expense Reports', icon: 'fas fa-receipt' },
      { id: 'team', label: 'Team Management', icon: 'fas fa-users' },
      { id: 'profile', label: 'Profile', icon: 'fas fa-user-circle' }
    ]

    // Stats state
    const pendingCount = ref(0)
    const approvedCount = ref(0)
    const statsRefreshInterval = ref(null)

    // User data computed properties
    const displayName = computed(() => {
      const name = userService.getDisplayName()
      return name !== 'Unknown User' ? name : 'User'
    })

    const userInitials = computed(() => {
      return userService.getUserInitials()
    })

    const userRole = computed(() => {
      const role = authService.getStoredRole()
      if (!role) return 'User'
      
      // Format role for display
      return role.charAt(0).toUpperCase() + role.slice(1).toLowerCase()
    })

    // Generate avatar background color based on user name
    const avatarStyle = computed(() => {
      const name = userService.getUserName() || userService.getUserEmail() || 'User'
      const colors = [
        '#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4', '#FFEAA7',
        '#DDA0DD', '#98D8C8', '#F7DC6F', '#BB8FCE', '#85C1E9'
      ]
      
      // Simple hash function to consistently assign colors
      let hash = 0
      for (let i = 0; i < name.length; i++) {
        hash = name.charCodeAt(i) + ((hash << 5) - hash)
      }
      const colorIndex = Math.abs(hash) % colors.length
      
      return {
        backgroundColor: colors[colorIndex],
        color: 'white'
      }
    })

    const isCurrentRoute = (sectionId) => {
      const currentSection = route.path.split('/').pop()
      return currentSection === sectionId
    }

    const navigateToSection = (sectionId) => {
      emit('navigate', sectionId)
    }

    const handleLogout = async () => {
      if (isLoggingOut.value) return
      
      try {
        isLoggingOut.value = true
        
        // Clear the refresh interval
        if (statsRefreshInterval.value) {
          clearInterval(statsRefreshInterval.value)
          statsRefreshInterval.value = null
        }
        
        await authService.logout()
        router.push('/login')
        console.log('Logged out successfully')
      } catch (error) {
        console.error('Logout failed:', error)
        alert('Logout failed. Please try again.')
      } finally {
        isLoggingOut.value = false
      }
    }

    // Load user statistics from both services
// CORRECTED loadUserStats function for Sidebar.vue
const loadUserStats = async () => {
  if (isLoadingStats.value) return // Prevent concurrent loading
  
  console.log('📊 Loading user statistics...')
  isLoadingStats.value = true
  
  try {
    const currentRole = authService.getStoredRole()
    
    // Fetch data from both services simultaneously
    const [expenseResult, vacationResult] = await Promise.all([
      fetchExpenseStats(currentRole),
      fetchVacationStats(currentRole)
    ])

    // Count pending and approved items from both services
    let pendingTotal = 0
    let approvedTotal = 0

    // Process expense reports
    if (expenseResult.success && Array.isArray(expenseResult.data)) {
      // Debug: Log all expense statuses to understand the format
      console.log('💰 All expense statuses:', expenseResult.data.map(item => ({
        id: item.id,
        statut: item.statut,
        statusType: typeof item.statut
      })))

      // Handle the actual status values from your expense API
      const expensePending = expenseResult.data.filter(item => {
        const status = item.statut
        if (status === null || status === undefined) return false
        
        // Convert to string for comparison and normalize
        const statusStr = String(status).toLowerCase().trim()
        
        return statusStr === 'enattente' || 
               statusStr === 'en_attente' || 
               statusStr === 'en attente' ||
               statusStr === 'pending' ||
               statusStr === '0' ||
               status === 0
      }).length

      const expenseApproved = expenseResult.data.filter(item => {
        const status = item.statut
        if (status === null || status === undefined) return false
        
        // Convert to string for comparison and normalize
        const statusStr = String(status).toLowerCase().trim()
        
        return statusStr === 'approuvee' || 
               statusStr === 'approuve' ||
               statusStr === 'approved' ||
               statusStr === '1' ||
               status === 1
      }).length

      pendingTotal += expensePending
      approvedTotal += expenseApproved

      console.log('💰 Expense stats:', { 
        pending: expensePending, 
        approved: expenseApproved,
        totalItems: expenseResult.data.length,
        rawData: expenseResult.data.map(item => ({ id: item.id, statut: item.statut }))
      })
    } else {
      console.log('💰 No expense data or error:', expenseResult?.message || 'No data')
    }

    // Process vacation requests
    if (vacationResult.success && Array.isArray(vacationResult.data)) {
      // Debug: Log all vacation statuses to understand the format
      console.log('🏖️ All vacation statuses:', vacationResult.data.map(item => ({
        id: item.id,
        statut: item.statut,
        statusType: typeof item.statut
      })))

      // Handle the actual status values from your vacation API
      const vacationPending = vacationResult.data.filter(item => {
        const status = item.statut
        if (status === null || status === undefined) return false
        
        // Convert to string for comparison and normalize
        const statusStr = String(status).toLowerCase().trim()
        
        return statusStr === 'enattente' ||
               statusStr === 'en_attente' || 
               statusStr === 'en attente' ||
               statusStr === 'pending' ||
               statusStr === '0' ||
               status === 0
      }).length

      const vacationApproved = vacationResult.data.filter(item => {
        const status = item.statut
        if (status === null || status === undefined) return false
        
        // Convert to string for comparison and normalize
        const statusStr = String(status).toLowerCase().trim()
        
        return statusStr === 'approuve' || 
               statusStr === 'approuvee' ||
               statusStr === 'approved' ||
               statusStr === '1' ||
               status === 1
      }).length

      pendingTotal += vacationPending
      approvedTotal += vacationApproved

      console.log('🏖️ Vacation stats:', { 
        pending: vacationPending, 
        approved: vacationApproved,
        totalItems: vacationResult.data.length,
        rawData: vacationResult.data.map(item => ({ id: item.id, statut: item.statut }))
      })
    } else {
      console.log('🏖️ No vacation data or error:', vacationResult?.message || 'No data')
    }

    // Update the reactive values
    pendingCount.value = pendingTotal
    approvedCount.value = approvedTotal

    console.log('✅ Total combined stats:', { 
      pending: pendingTotal, 
      approved: approvedTotal,
      breakdown: {
        expenses: expenseResult.success ? expenseResult.data?.length || 0 : 0,
        vacations: vacationResult.success ? vacationResult.data?.length || 0 : 0
      }
    })

  } catch (error) {
    console.error('❌ Error loading user stats:', error)
    // Don't reset counts on error, keep previous values
  } finally {
    isLoadingStats.value = false
  }
}
    // Fetch expense statistics based on user role
    const fetchExpenseStats = async (role) => {
      try {
        if (role === 'manager' || role === 'admin') {
          // Managers and admins see team expense reports
          return await expenseService.getManagerExpenseReports()
        } else {
          // Regular users see their own expense reports
          return await expenseService.getUserExpenseReports()
        }
      } catch (error) {
        console.error('Error fetching expense stats:', error)
        return { success: false, message: error.message, data: [] }
      }
    }

    // Fetch vacation statistics based on user role
    const fetchVacationStats = async (role) => {
      try {
        if (role === 'manager') {
          // Managers see team vacation requests
          return await vacationService.getTeamVacationRequests()
        } else if (role === 'admin') {
          // Admins see all vacation requests
          return await vacationService.getAllVacationRequests()
        } else {
          // Regular users see their own vacation requests
          return await vacationService.getUserVacationRequests()
        }
      } catch (error) {
        console.error('Error fetching vacation stats:', error)
        return { success: false, message: error.message, data: [] }
      }
    }

    const initializeUserData = () => {
      currentUser.value = userService.getCurrentUser()
      loadUserStats()
    }

    // Set up automatic refresh of stats every 5 minutes
    const setupStatsRefresh = () => {
      if (statsRefreshInterval.value) {
        clearInterval(statsRefreshInterval.value)
      }
      
      statsRefreshInterval.value = setInterval(() => {
        console.log('🔄 Auto-refreshing stats...')
        loadUserStats()
      }, 5 * 60 * 1000) // 5 minutes
    }

    // Watch for route changes to refresh stats when navigating between sections
    watch(() => route.path, (newPath) => {
      // Refresh stats when user navigates to expense or vacation sections
      if (newPath.includes('/expenses') || newPath.includes('/vacation')) {
        console.log('🔄 Route changed to data section, refreshing stats...')
        setTimeout(loadUserStats, 1000) // Small delay to let the component load
      }
    })

    onMounted(() => {
      // Initialize user data
      initializeUserData()
      
      // Subscribe to user data changes
      unsubscribe = userService.subscribe((userData) => {
        currentUser.value = userData
        if (userData) {
          loadUserStats()
        }
      })

      // Check if user data is not loaded, try to get it from auth check
      if (!currentUser.value) {
        authService.checkAuth().then((result) => {
          if (result.authenticated && result.user) {
            initializeUserData()
          }
        })
      }

      // Set up automatic refresh
      setupStatsRefresh()

      // Listen for auth expiration events
      window.addEventListener('auth-expired', () => {
        console.log('🚫 Auth expired, clearing stats refresh')
        if (statsRefreshInterval.value) {
          clearInterval(statsRefreshInterval.value)
          statsRefreshInterval.value = null
        }
        pendingCount.value = 0
        approvedCount.value = 0
      })
    })

    onUnmounted(() => {
      if (unsubscribe) {
        unsubscribe()
      }
      
      if (statsRefreshInterval.value) {
        clearInterval(statsRefreshInterval.value)
      }
    })

    return {
      menuItems,
      currentUser,
      displayName,
      userInitials,
      userRole,
      avatarStyle,
      pendingCount,
      approvedCount,
      isLoadingStats,
      isCurrentRoute,
      navigateToSection,
      handleLogout,
      isLoggingOut,
      loadUserStats // Expose for manual refresh if needed
    }
  }
}
</script>

<style scoped>
.sidebar {
  width: 280px;
  background: linear-gradient(180deg, #F6C900 0%, #F6BF00 100%);
  padding: 2rem 0;
  display: flex;
  flex-direction: column;
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  z-index: 1000;
}

.sidebar.collapsed {
  width: 80px;
  padding: 1rem 0;
}

.sidebar::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><defs><pattern id="grain" patternUnits="userSpaceOnUse" width="100" height="100"><circle cx="25" cy="25" r="1" fill="white" opacity="0.1"/><circle cx="75" cy="75" r="1" fill="white" opacity="0.1"/><circle cx="50" cy="10" r="0.5" fill="white" opacity="0.05"/></pattern></defs><rect width="100" height="100" fill="url(%23grain)"/></svg>');
  pointer-events: none;
}

.sidebar-header {
  padding: 0 2rem 2rem;
  position: relative;
  z-index: 2;
}

.sidebar.collapsed .sidebar-header {
  padding: 0 1rem 1rem;
}

/* Internal Toggle Button */
.sidebar-toggle-internal {
  position: absolute;
  top: 1rem;
  right: 1rem;
  z-index: 10;
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s ease;
}

.sidebar-toggle-internal:hover {
  background: rgba(255, 255, 255, 0.3);
  transform: scale(1.05);
}

.sidebar-toggle-internal i {
  color: #1D1D1D;
  font-size: 0.9rem;
}

.sidebar.collapsed .sidebar-toggle-internal {
  right: 0.75rem;
}

/* NEO Logo Styles */
.logo {
  display: flex;
  align-items: center;
  margin-bottom: 2rem;
}

.logo-collapsed {
  display: flex;
  justify-content: center;
  margin-bottom: 1.5rem;
}

.neo-logo {
  display: flex;
  align-items: center;
  gap: 2px;
}

.neo-block {
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 28px;
  color: #F6BF00;
}

.neo-n {
  width: 32px;
  background: white;
}

.neo-e {
  width: 24px;
  background: white;
}

.neo-o {
  width: 32px;
  background: #F6BF00;
  color: white;
}

.ledge-text {
  margin-left: 8px;
  font-size: 28px;
  font-weight: normal;
  color: #1D1D1D;
}

/* Collapsed Logo */
.neo-logo-collapsed {
  display: flex;
  justify-content: center;
  align-items: center;
}

.neo-circle {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: white;
  position: relative;
  overflow: hidden;
}

.neo-circle-half {
  position: absolute;
  top: 0;
  right: 0;
  width: 50%;
  height: 100%;
  background: #F6BF00;
  border-radius: 0 50% 50% 0;
}

.user-profile {
  display: flex;
  align-items: center;
  background: rgba(255, 255, 255, 0.15);
  backdrop-filter: blur(10px);
  padding: 1rem;
  border-radius: 16px;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.user-profile-collapsed {
  display: flex;
  justify-content: center;
  background: rgba(255, 255, 255, 0.15);
  backdrop-filter: blur(10px);
  padding: 0.75rem;
  border-radius: 16px;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.avatar-wrapper {
  position: relative;
  margin-right: 1rem;
}

.user-profile-collapsed .avatar-wrapper {
  margin-right: 0;
}

.avatar {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 1.2rem;
  text-transform: uppercase;
  overflow: hidden;
}

.sidebar.collapsed .avatar {
  width: 36px;
  height: 36px;
  font-size: 1rem;
}

.text-avatar {
  font-family: 'Arial', sans-serif;
  letter-spacing: 0.5px;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
}

.status-indicator {
  position: absolute;
  bottom: -2px;
  right: -2px;
  width: 14px;
  height: 14px;
  background: #4ADE80;
  border-radius: 50%;
  border: 2px solid #F6C900;
}

.sidebar.collapsed .status-indicator {
  width: 10px;
  height: 10px;
  bottom: -1px;
  right: -1px;
}

.user-info h3 {
  color: #1D1D1D;
  font-size: 1rem;
  font-weight: 600;
  margin-bottom: 0.25rem;
}

.user-info p {
  color: rgba(29, 29, 29, 0.7);
  font-size: 0.875rem;
}

.nav-menu {
  flex: 1;
  padding: 1rem 0;
}

.nav-item {
  display: flex;
  align-items: center;
  padding: 1rem 2rem;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  margin: 0.25rem 1rem;
  border-radius: 12px;
}

.sidebar.collapsed .nav-item {
  padding: 1rem;
  margin: 0.25rem 0.5rem;
  justify-content: center;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.15);
  transform: translateX(4px);
}

.sidebar.collapsed .nav-item:hover {
  transform: translateX(0) scale(1.1);
}

.nav-item.active {
  background: rgba(255, 255, 255, 0.25);
  transform: translateX(8px);
}

.sidebar.collapsed .nav-item.active {
  transform: translateX(0) scale(1.1);
  background: rgba(255, 255, 255, 0.3);
}

.nav-item.active .nav-indicator {
  opacity: 1;
  transform: scaleY(1);
}

.nav-icon {
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 1rem;
}

.sidebar.collapsed .nav-icon {
  margin-right: 0;
}

.nav-icon i {
  color: #1D1D1D;
  font-size: 1.1rem;
}

.nav-label {
  color: #1D1D1D;
  font-weight: 500;
  flex: 1;
}

.nav-indicator {
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%) scaleY(0);
  width: 4px;
  height: 24px;
  background: #1D1D1D;
  border-radius: 0 4px 4px 0;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  opacity: 0;
}

.sidebar-footer {
  padding: 1rem 2rem;
  position: relative;
  z-index: 2;
}

.sidebar.collapsed .sidebar-footer {
  padding: 1rem 0.5rem;
}

.quick-stats {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: rgba(255, 255, 255, 0.15);
  backdrop-filter: blur(10px);
  padding: 1rem;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.2);
  margin-bottom: 1rem;
}

.quick-stats-collapsed {
  display: flex;
  justify-content: space-around;
  align-items: center;
  margin-bottom: 1rem;
  padding: 0.5rem;
}

.stat-dot {
  width: 24px;
  height: 24px;
  background: rgba(255, 255, 255, 0.25);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
  font-weight: 600;
  color: #1D1D1D;
  border: 1px solid rgba(255, 255, 255, 0.3);
}

.stat-item {
  text-align: center;
  flex: 1;
  position: relative;
}

.stat-item.loading {
  opacity: 0.7;
}

.stat-dot.loading {
  opacity: 0.7;
}

.stat-number {
  color: #1D1D1D;
  font-size: 1.5rem;
  font-weight: 700;
  line-height: 1;
  margin-bottom: 0.25rem;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 2rem;
}

.stat-label {
  color: rgba(29, 29, 29, 0.7);
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.stat-divider {
  width: 1px;
  height: 30px;
  background: rgba(255, 255, 255, 0.3);
  margin: 0 1rem;
}

/* Loading spinners */
.stat-spinner {
  width: 20px;
  height: 20px;
  border: 2px solid rgba(29, 29, 29, 0.2);
  border-radius: 50%;
  border-top-color: #1D1D1D;
  animation: spin 1s ease-in-out infinite;
}

.stat-spinner-small {
  width: 12px;
  height: 12px;
  border: 1px solid rgba(29, 29, 29, 0.2);
  border-radius: 50%;
  border-top-color: #1D1D1D;
  animation: spin 1s ease-in-out infinite;
}

/* Logout Button Styles */
.logout-section {
  margin-top: 1rem;
}

.logout-button {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0.875rem 1.5rem;
  background: rgba(220, 38, 38, 0.9);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(220, 38, 38, 0.3);
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  font-family: inherit;
  position: relative;
  overflow: hidden;
}

.logout-button.collapsed {
  padding: 0.75rem;
  border-radius: 50%;
  width: 48px;
  height: 48px;
}

.logout-button:hover:not(:disabled) {
  background: rgba(220, 38, 38, 1);
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(220, 38, 38, 0.3);
}

.logout-button.collapsed:hover:not(:disabled) {
  transform: translateY(0) scale(1.1);
}

.logout-button:active:not(:disabled) {
  transform: translateY(0);
  box-shadow: 0 4px 15px rgba(220, 38, 38, 0.2);
}

.logout-button:disabled {
  cursor: not-allowed;
  opacity: 0.7;
}

.logout-button::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
  transition: left 0.5s;
}

.logout-button:hover:not(:disabled)::before {
  left: 100%;
}

.logout-icon {
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 0.75rem;
}

.logout-button.collapsed .logout-icon {
  margin-right: 0;
}

.logout-icon i {
  color: white;
  font-size: 1rem;
}
</style>
