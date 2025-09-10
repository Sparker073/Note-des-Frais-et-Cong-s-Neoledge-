// Enhanced userService.js with full API methods including profile update
class UserService {
  constructor() {
    this.currentUser = null
    this.listeners = []
    this.apiBaseUrl = 'http://localhost:5032/api'
    this.userCache = new Map() // Cache to avoid repeated API calls
  }

  // Make authenticated request
  async makeRequest(url, options = {}) {
    const defaultOptions = {
      headers: {
        'Content-Type': 'application/json'
      },
      credentials: 'include' // Include HttpOnly cookies
    }
    
    const mergedOptions = {
      ...defaultOptions,
      ...options,
      headers: {
        ...defaultOptions.headers,
        ...options.headers
      }
    }

    try {
      const response = await fetch(url, mergedOptions)
      
      if (response.status === 401) {
        return {
          success: false,
          message: 'Session expired. Please login again.',
          status: 401
        }
      }

      const result = await this.safeJsonParse(response)
      return {
        success: response.ok,
        data: result,
        message: response.ok ? 'Success' : (result?.message || 'Request failed'),
        status: response.status
      }
    } catch (error) {
      return {
        success: false,
        message: 'Network error. Check your connection.'
      }
    }
  }

  // Helper to safely parse JSON
  async safeJsonParse(response) {
    try {
      const text = await response.text()
      if (!text || text.trim() === '') {
        return null
      }
      return JSON.parse(text)
    } catch (error) {
      return null
    }
  }

  // Update user profile - matches your UpdateUserDto
  async updateUserProfile(userId, updateData) {
    
    // Validate required fields before sending
    if (!updateData.nom || !updateData.email) {
      return {
        success: false,
        message: 'Name and email are required fields'
      }
    }

    // Prepare the data to match your UpdateUserDto exactly
    const payload = {
      nom: updateData.nom,
      email: updateData.email,
      role: updateData.role || 'Employe',
      poste: updateData.poste || '',
      managerId: updateData.managerId || null
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
        message: result.message || 'Failed to update profile'
      }
    }
  }

  // Refresh current user data after update
  async refreshCurrentUserData() {
    const userId = this.getUserId()
    if (userId) {
      const result = await this.getUserById(userId)
      if (result.success) {
        this.setCurrentUser(result.data)
      }
    }
  }

  // Fetch user by ID from API
  async getUserById(userId) {
    // Check cache first
    if (this.userCache.has(userId)) {
      return {
        success: true,
        data: this.userCache.get(userId)
      }
    }

    
    const result = await this.makeRequest(`${this.apiBaseUrl}/User/${userId}`)
    
    if (result.success && result.data) {
      // Extract user from API response wrapper
      const userData = result.data.data || result.data
      this.userCache.set(userId, userData)
      return {
        success: true,
        data: userData
      }
    } else {
      return {
        success: false,
        message: result.message || 'Failed to fetch user',
        data: null
      }
    }
  }

  // Get all users who can be managers (you might want to create a specific endpoint for this)
  async getAvailableManagers() {
    
    // Option 1: Get all users and filter (if you have a get all users endpoint)
    const result = await this.makeRequest(`${this.apiBaseUrl}/User`)
    
    if (result.success && result.data) {
      // Extract users from API response
      const users = result.data.data || result.data
      
      // Filter out current user and format for dropdown
      const currentUserId = this.getUserId()
      const managers = users
        .filter(user => {
          const userId = user.Id || user.id
          return userId !== currentUserId // Don't allow self as manager
        })
        .map(user => ({
          id: user.Id || user.id,
          displayName: this.formatUserDisplayName(user),
          role: user.Role || user.role || 'Employee'
        }))
        
      return {
        success: true,
        data: managers
      }
    } else {
      return {
        success: false,
        message: result.message || 'Failed to load managers',
        data: []
      }
    }
  }

  // Get multiple users by IDs (batch fetch)
  async getUsersByIds(userIds) {
    const uniqueIds = [...new Set(userIds)]
    const users = {}
    
    // Check cache first
    const uncachedIds = uniqueIds.filter(id => !this.userCache.has(id))
    
    // Fetch uncached users
    for (const userId of uncachedIds) {
      const result = await this.getUserById(userId)
      if (result.success) {
        users[userId] = result.data
      }
    }
    
    // Add cached users
    uniqueIds.forEach(id => {
      if (this.userCache.has(id)) {
        users[id] = this.userCache.get(id)
      }
    })
    
    return users
  }

  // Get user display name by ID
  async getUserDisplayName(userId) {
    if (!userId) return 'Unknown Employee'
    
    const result = await this.getUserById(userId)
    if (result.success && result.data) {
      const user = result.data
      return this.formatUserDisplayName(user)
    }
    
    return `Employee #${userId}`
  }

  // Format user display name from user object
  formatUserDisplayName(user) {
    if (!user) return 'Unknown Employee'
    
    // Try different name field combinations
    if (user.prenom && user.nom) {
      return `${user.prenom} ${user.nom}`
    }
    if (user.Prenom && user.Nom) {
      return `${user.Prenom} ${user.Nom}`
    }
    if (user.nom) {
      return user.nom
    }
    if (user.Nom) {
      return user.Nom
    }
    if (user.name) {
      return user.name
    }
    if (user.email || user.Email) {
      return user.email || user.Email
    }
    
    return 'Unknown Employee'
  }

  // Clear user cache
  clearCache() {
    this.userCache.clear()
  }

  // Set current user data
  setCurrentUser(userData) {
    this.currentUser = userData
    this.notifyListeners()
  }

  // Get current user data
  getCurrentUser() {
    return this.currentUser
  }

  // Clear user data
  clearUserData() {
    this.currentUser = null
    this.clearCache()
    this.notifyListeners()
  }

  // Get specific user property
  getUserProperty(property) {
    return this.currentUser?.[property] || null
  }

  // Get user ID
  getUserId() {
    return this.currentUser?.Id || this.currentUser?.id || null
  }

  // Get user name
  getUserName() {
    return this.currentUser?.Nom || this.currentUser?.nom || this.currentUser?.name || null
  }

  // Get user email
  getUserEmail() {
    return this.currentUser?.Email || this.currentUser?.email || null
  }

  // Get user position/role
  getUserPoste() {
    return this.currentUser?.Poste || this.currentUser?.poste || null
  }

  // Get user role
  getUserRole() {
    return this.currentUser?.Role || this.currentUser?.role || 'Employe'
  }

  // Get manager ID
  getManagerId() {
    return this.currentUser?.ManagerId || this.currentUser?.managerId || null
  }

  // Get manager display name
  async getManagerName() {
    const managerId = this.getManagerId()
    if (!managerId) return 'No Manager Assigned'

    return await this.getUserDisplayName(managerId)
  }

  // Check if user data exists
  hasUserData() {
    return this.currentUser !== null
  }

  // Subscribe to user data changes (for reactive components)
  subscribe(callback) {
    this.listeners.push(callback)
    
    // Return unsubscribe function
    return () => {
      this.listeners = this.listeners.filter(listener => listener !== callback)
    }
  }

  // Notify all listeners of user data changes
  notifyListeners() {
    this.listeners.forEach(callback => callback(this.currentUser))
  }

  // Update specific user property locally
  updateUserProperty(property, value) {
    if (this.currentUser) {
      this.currentUser[property] = value
      this.notifyListeners()
    }
  }

  // Utility method to format user display name
  getDisplayName() {
    const name = this.getUserName()
    const email = this.getUserEmail()
    return name || email || 'Unknown User'
  }

  // Get user initials for avatars
  getUserInitials() {
    const name = this.getUserName()
    if (name) {
      return name
        .split(' ')
        .map(word => word.charAt(0).toUpperCase())
        .join('')
        .substring(0, 2)
    }
    
    const email = this.getUserEmail()
    if (email) {
      return email.charAt(0).toUpperCase()
    }
    
    return 'U'
  }

  // Parse name into first and last names
  parseUserName(fullName) {
    if (!fullName) return { firstName: '', lastName: '' }
    
    const parts = fullName.trim().split(' ')
    return {
      firstName: parts[0] || '',
      lastName: parts.slice(1).join(' ') || ''
    }
  }

  // Combine first and last names
  combineUserName(firstName, lastName) {
    return `${firstName || ''} ${lastName || ''}`.trim()
  }

  // Validation helpers
  validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(email)
  }

  validateUpdateData(data) {
    const errors = []
    
    if (!data.nom || data.nom.trim() === '') {
      errors.push('Name is required')
    }
    
    if (!data.email || data.email.trim() === '') {
      errors.push('Email is required')
    } else if (!this.validateEmail(data.email)) {
      errors.push('Invalid email format')
    }
    
    return {
      isValid: errors.length === 0,
      errors: errors
    }
  }

}

// Export singleton instance
export const userService = new UserService()
export default userService