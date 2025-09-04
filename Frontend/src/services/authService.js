// services/authService.js
import { userService } from './UserService.js'

class AuthService {
  constructor() {
    this.apiBaseUrl = 'http://localhost:5032/api/auth'
  }

  // Login method - Updated for HttpOnly cookie authentication
  async login(credentials) {
    try {
      const response = await fetch(`${this.apiBaseUrl}/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include', // This allows HttpOnly cookies to be sent/received
        body: JSON.stringify({
          Email: credentials.email,
          motDePasse: credentials.password
        })
      })

      const data = await response.json()
      console.log('🔍 Full backend response:', data)
      
      if (response.ok) {
        // Since the token is in HttpOnly cookie, we don't store it manually
        // But we store the role and user data for frontend use
        const storage = credentials.rememberMe ? localStorage : sessionStorage
        
        // Store user role for frontend logic
        const userRole = data.role || data.Role || data.userRole || data.UserRole
        if (userRole) {
          storage.setItem('userRole', userRole)
          console.log('✅ User role stored:', userRole)
        }

        // Store user data in UserService
        const userData = data.user || data.User || data.userData || data.UserData
        if (userData) {
          userService.setCurrentUser(userData)
          console.log('✅ User data stored in UserService')
        }
        
        console.log('✅ Login successful - HttpOnly cookie set by server')
        
        return {
          success: true,
          data: data,
          role: userRole,
          user: userData,
          message: data.message || 'Login successful'
        }
      } else {
        console.error('❌ Login failed with status:', response.status)
        return {
          success: false,
          message: data.message || 'Login failed'
        }
      }
    } catch (error) {
      console.error('Login error:', error)
      return {
        success: false,
        message: 'Network error. Please check your connection and try again.'
      }
    }
  }

  // Signup method - Updated for HttpOnly cookie authentication
  async signup(userData) {
    try {
      console.log('Sending signup data:', userData)
      
      const response = await fetch(`${this.apiBaseUrl}/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include', // This allows HttpOnly cookies to be sent/received
        body: JSON.stringify({
          Nom: userData.Nom,
          Email: userData.Email,
          motDePasse: userData.motDePasse,
          Poste: userData.Poste,
          ManagerId: userData.ManagerId
        })
      })

      const data = await response.json()
      console.log('Server response:', data)
      
      if (response.ok) {
        // Store user role and data (token is in HttpOnly cookie)
        const userRole = data.role || data.Role || 'employe'
        sessionStorage.setItem('userRole', userRole)

        // Store user data in UserService
        const user = data.user || data.User
        if (user) {
          userService.setCurrentUser(user)
        }
        
        console.log('✅ Registration successful - HttpOnly cookie set by server')
        
        return {
          success: true,
          data: data,
          role: userRole,
          user: user,
          message: data.message || 'Registration successful'
        }
      } else {
        console.error('Server error response:', response.status, data)
        return {
          success: false,
          message: data.message || 'Registration failed'
        }
      }
    } catch (error) {
      console.error('Signup error:', error)
      return {
        success: false,
        message: 'Network error. Please check your connection and try again.'
      }
    }
  }

  // Logout method - Updated for HttpOnly cookie authentication
  async logout() {
    try {
      await fetch(`${this.apiBaseUrl}/logout`, {
        method: 'POST',
        credentials: 'include' // This ensures the HttpOnly cookie is sent for logout
      })
      console.log('✅ Logout request sent - server will clear HttpOnly cookie')
    } catch (error) {
      console.error('Logout error:', error)
    } finally {
      // Clear frontend stored data (but not the cookie - server handles that)
      this.clearAllAuthData()
    }
  }

  // Check if user is authenticated - Updated to verify with server
  async checkAuth() {
    try {
      const response = await fetch(`${this.apiBaseUrl}/verify`, {
        method: 'GET',
        credentials: 'include' // This sends the HttpOnly cookie for verification
      })
      
      if (response.ok) {
        const data = await response.json()
        const userRole = data.role || data.Role
        
        // Sync user data with UserService
        const userData = data.user || data.User
        if (userData) {
          userService.setCurrentUser(userData)
        }
        
        // Update stored role if needed
        if (userRole && !this.getStoredRole()) {
          sessionStorage.setItem('userRole', userRole)
        }
        
        console.log('✅ Authentication verified via HttpOnly cookie')
        
        return {
          authenticated: true,
          role: userRole,
          user: userData
        }
      } else {
        // Clear user data if not authenticated
        this.clearAllAuthData()
        console.log('❌ Authentication failed - clearing stored data')
        return { authenticated: false }
      }
    } catch (error) {
      console.error('Auth check error:', error)
      this.clearAllAuthData()
      return { authenticated: false }
    }
  }

  // Get stored role (non-sensitive data stored in frontend)
  getStoredRole() {
    return localStorage.getItem('userRole') || sessionStorage.getItem('userRole')
  }

  // Clear all authentication data (frontend only - server handles cookie clearing)
  clearAllAuthData() {
    // Clear localStorage and sessionStorage
    this.clearLocalAuthData()
    // Clear UserService data
    userService.clearUserData()
    console.log('✅ Frontend auth data cleared')
  }

  // Clear local auth data
  clearLocalAuthData() {
    localStorage.removeItem('userRole')
    sessionStorage.removeItem('userRole')
  }

  // Check if user has required role
  hasRole(requiredRole) {
    const userRole = this.getStoredRole()
    return userRole?.toLowerCase() === requiredRole?.toLowerCase()
  }

  // Get current user from UserService (convenience method)
  getCurrentUser() {
    return userService.getCurrentUser()
  }

  // Check if user is authenticated based on stored data
  isAuthenticated() {
    return !!this.getStoredRole() && userService.hasUserData()
  }

  // Note: No need for getAuthHeaders() since authentication is handled via HttpOnly cookies
  // and credentials: 'include' in fetch requests
}

// Export singleton instance
export const authService = new AuthService()
export default authService