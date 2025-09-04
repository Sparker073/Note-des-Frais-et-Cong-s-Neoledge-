// routes/index.js
import { createRouter, createWebHistory } from 'vue-router'
import authService from '../services/authService.js'

// Import all views
import LoginView from '../views/AuthViews/LoginView.vue'
import SignupView from '../views/AuthViews/SignupView.vue'
import AdminDashboard from '../views/AdminViews/Dashboard.vue'

// Import Employee Layout and Views
import EmployeeLayout from '../views/EmployeViews/EmployeeLayout.vue'
import Dashboard from '../views/EmployeViews/Dashboard.vue'
import VacationRequests from '../views/EmployeViews/VacationRequests.vue'
import ExpenseReports from '../views/EmployeViews/ExpenseReports.vue'
import TeamManagement from '../views/EmployeViews/TeamManagement.vue'
import Profile from '../views/EmployeViews/Profile.vue'

// Route definitions
export const routes = {
  // Auth routes
  LOGIN: 'login',
  SIGNUP: 'signup',
  
  // Dashboard routes
  ADMIN_DASHBOARD: 'admin-dashboard',
  EMPLOYEE_DASHBOARD: 'employee-dashboard',
  EMPLOYEE_VACATION: 'employee-vacation',
  EMPLOYEE_EXPENSES: 'employee-expenses',
  EMPLOYEE_TEAM: 'employee-team',
  EMPLOYEE_PROFILE: 'employee-profile'
}

// Vue Router routes configuration
const routerRoutes = [
  {
    path: '/',
    redirect: '/login'
  },
  {
    path: '/login',
    name: routes.LOGIN,
    component: LoginView,
    meta: { requiresGuest: true }
  },
  {
    path: '/signup',
    name: routes.SIGNUP,
    component: SignupView,
    meta: { requiresGuest: true }
  },
  {
    path: '/admin-dashboard',
    name: routes.ADMIN_DASHBOARD,
    component: AdminDashboard,
    meta: { requiresAuth: true, requiredRole: 'admin' }
  },
  {
    path: '/employee',
    component: EmployeeLayout,
    meta: { requiresAuth: true, requiredRole: 'employe' },
    children: [
      {
        path: '',
        redirect: '/employee/dashboard'
      },
      {
        path: 'dashboard',
        name: routes.EMPLOYEE_DASHBOARD,
        component: Dashboard
      },
      {
        path: 'vacation',
        name: routes.EMPLOYEE_VACATION,
        component: VacationRequests
      },
      {
        path: 'expenses',
        name: routes.EMPLOYEE_EXPENSES,
        component: ExpenseReports
      },
      {
        path: 'team',
        name: routes.EMPLOYEE_TEAM,
        component: TeamManagement
      },
      {
        path: 'profile',
        name: routes.EMPLOYEE_PROFILE,
        component: Profile
      }
    ]
  }
]

// Create Vue Router instance
const vueRouter = createRouter({
  history: createWebHistory(),
  routes: routerRoutes
})

// Navigation guards
vueRouter.beforeEach(async (to, from, next) => {
  try {
    const authResult = await authService.checkAuth()
    const isAuthenticated = authResult.authenticated
    const userRole = authResult.role?.toLowerCase()

    // Handle guest-only routes (login, signup)
    if (to.meta.requiresGuest && isAuthenticated) {
      if (userRole === 'admin') {
        return next('/admin-dashboard')
      } else if (userRole === 'employe') {
        return next('/employee/dashboard')
      }
    }

    // Handle protected routes
    if (to.meta.requiresAuth) {
      if (!isAuthenticated) {
        return next('/login')
      }

      if (to.meta.requiredRole && userRole !== to.meta.requiredRole) {
        console.error(`Access denied. Required: ${to.meta.requiredRole}, User: ${userRole}`)
        if (userRole === 'admin') {
          return next('/admin-dashboard')
        } else if (userRole === 'employe') {
          return next('/employee/dashboard')
        } else {
          return next('/login')
        }
      }
    }

    next()
  } catch (error) {
    console.error('Navigation guard error:', error)
    next('/login')
  }
})

// Router class with secure cookie authentication (updated for nested routes)
class SecureRouter {
  constructor() {
    this.vueRouter = vueRouter
    this.isLoading = false
  }

  // Navigate to a specific route
  navigate(routeName) {
    const routeMap = {
      [routes.LOGIN]: '/login',
      [routes.SIGNUP]: '/signup',
      [routes.ADMIN_DASHBOARD]: '/admin-dashboard',
      [routes.EMPLOYEE_DASHBOARD]: '/employee/dashboard',
      [routes.EMPLOYEE_VACATION]: '/employee/vacation',
      [routes.EMPLOYEE_EXPENSES]: '/employee/expenses',
      [routes.EMPLOYEE_TEAM]: '/employee/team',
      [routes.EMPLOYEE_PROFILE]: '/employee/profile'
    }
    
    const path = routeMap[routeName]
    if (path) {
      this.vueRouter.push(path)
      console.log(`Navigated to: ${routeName}`)
    } else {
      console.error(`Route not found: ${routeName}`)
    }
  }

  // Navigation methods
  goToLogin() {
    this.vueRouter.push('/login')
  }

  goToSignup() {
    this.vueRouter.push('/signup')
  }

  goToAdminDashboard() {
    this.vueRouter.push('/admin-dashboard')
  }

  goToEmployeeDashboard() {
    this.vueRouter.push('/employee/dashboard')
  }

  // Navigate to employee sections
  goToEmployeeSection(section) {
    const sectionMap = {
      'dashboard': '/employee/dashboard',
      'vacation': '/employee/vacation',
      'expenses': '/employee/expenses',
      'team': '/employee/team',
      'profile': '/employee/profile'
    }
    
    const path = sectionMap[section]
    if (path) {
      this.vueRouter.push(path)
    } else {
      console.error(`Unknown employee section: ${section}`)
    }
  }

  // Role-based routing
  routeByRole(role) {
    switch (role?.toLowerCase()) {
      case 'admin':
        this.goToAdminDashboard()
        break
      case 'employe':
      case 'employee':
        this.goToEmployeeDashboard()
        break
      default:
        console.error(`Unknown role: ${role}`)
        this.goToLogin()
    }
  }

  // Secure authentication check and route initialization
  async initializeRoute() {
    this.isLoading = true
    
    try {
      // Check if user is authenticated via secure cookie
      const authResult = await authService.checkAuth()
      
      if (authResult.authenticated && authResult.role) {
        // User is authenticated, route to appropriate dashboard
        this.routeByRole(authResult.role)
      } else {
        // User not authenticated, go to login
        this.goToLogin()
      }
    } catch (error) {
      console.error('Route initialization error:', error)
      this.goToLogin()
    } finally {
      this.isLoading = false
    }
  }

  // Secure logout
  async logout() {
    this.isLoading = true
    
    try {
      await authService.logout()
    } catch (error) {
      console.error('Logout error:', error)
    } finally {
      this.goToLogin()
      this.isLoading = false
    }
  }

  // Handle login success
  handleLoginSuccess(userData) {
    if (userData?.role) {
      this.routeByRole(userData.role)
    } else {
      console.error('No role provided in login response')
      this.goToLogin()
    }
  }

  // Guard method to check if user has required role
  async requireRole(requiredRole) {
    const authResult = await authService.checkAuth()
    
    if (!authResult.authenticated) {
      this.goToLogin()
      return false
    }
    
    if (authResult.role?.toLowerCase() !== requiredRole?.toLowerCase()) {
      console.error(`Access denied. Required role: ${requiredRole}, User role: ${authResult.role}`)
      // Route to appropriate dashboard based on user's actual role
      this.routeByRole(authResult.role)
      return false
    }
    
    return true
  }

  // Admin guard
  async requireAdmin() {
    return await this.requireRole('admin')
  }

  // Employee guard
  async requireEmployee() {
    return await this.requireRole('employe')
  }
}

// Create and export router instance
export const router = new SecureRouter()

// Export Vue Router instance
export const vueRouterInstance = vueRouter

// Export individual navigation functions for convenience
export const {
  navigate,
  goToLogin,
  goToSignup,
  goToAdminDashboard,
  goToEmployeeDashboard,
  goToEmployeeSection,
  routeByRole,
  initializeRoute,
  logout,
  handleLoginSuccess,
  requireRole,
  requireAdmin,
  requireEmployee
} = router