// services/vacationService.js - Complete updated version with proper balance handling
class VacationService {
  constructor() {
    this.apiBaseUrl = 'http://localhost:5032/api/DemandeConge'
  }

  // Helper method to safely parse JSON response
  async safeJsonParse(response) {
    const text = await response.text()
    if (!text || text.trim() === '') {
      return {
        success: response.ok,
        message: response.ok ? 'Opération réussie' : `Erreur ${response.status}: ${response.statusText}`,
        data: null
      }
    }
    
    try {
      return JSON.parse(text)
    } catch (error) {
      return {
        success: false,
        message: response.status === 401 ? 'Session expirée. Veuillez vous reconnecter.' : 'Réponse invalide du serveur',
        data: null
      }
    }
  }

  // Check if user is authenticated (check if we have stored role/user data)
  isAuthenticated() {
    return !!(localStorage.getItem('userRole') || sessionStorage.getItem('userRole'))
  }

  // Handle authentication errors
  handleAuthError() {
    // Clear any stored auth data (but not cookies - those are HttpOnly)
    localStorage.removeItem('userRole')
    sessionStorage.removeItem('userRole')
    
    // Emit custom event for auth expiration
    window.dispatchEvent(new CustomEvent('auth-expired'))
    
    return {
      success: false,
      message: 'Session expirée. Veuillez vous reconnecter.'
    }
  }

  // Make authenticated request using HttpOnly cookies
  async makeRequest(url, options = {}) {
     
    const defaultOptions = {
      headers: {
        'Content-Type': 'application/json'
      },
      credentials: 'include' // This sends HttpOnly cookies automatically
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
      // Handle 401 specifically
      if (response.status === 401) {
        return { response, result: this.handleAuthError() }
      }

      const result = await this.safeJsonParse(response)
      return { response, result }
    } catch (error) {
      return {
        response: null,
        result: {
          success: false,
          message: 'Erreur réseau. Vérifiez votre connexion.'
        }
      }
    }
  }

  // Get vacation balance for current user - FIXED VERSION
  async getVacationBalance(year = new Date().getFullYear()) {
    const { response, result } = await this.makeRequest(`${this.apiBaseUrl}/solde/${year}`, {
      method: 'GET'
    })

    if (!response) return result
    
    if (response.ok && result.success) {
      // Process and validate the balance data
      const balanceData = this.processBalanceData(result.data, year)
      
      return {
        success: true,
        data: balanceData,
        message: result.message || 'Solde récupéré avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`
      }
    }
  }

  // Process balance data to ensure correct values
  processBalanceData(rawData, year) {
    if (!rawData) {
      return {
        joursTotal: 0,
        joursUtilises: 0,
        joursRestants: 0,
        annee: year
      }
    }

    // Handle different possible API response structures
    let balanceData = {}

    // If the data is directly the balance object
    if (typeof rawData === 'object') {
      balanceData = {
        joursTotal: this.parseNumber(rawData.joursTotal || rawData.JoursTotal || rawData.total || 25),
        joursUtilises: this.parseNumber(rawData.joursUtilises || rawData.JoursUtilises || rawData.utilises || 0),
        joursRestants: 0, // Will be calculated
        annee: rawData.annee || rawData.Annee || year
      }
    } else if (typeof rawData === 'number') {
      // If API returns just the remaining days as a number
      balanceData = {
        joursTotal: 25, // Default annual leave days
        joursUtilises: 0,
        joursRestants: this.parseNumber(rawData),
        annee: year
      }
    } else {
      // Fallback
      balanceData = {
        joursTotal: 25,
        joursUtilises: 0,
        joursRestants: 25,
        annee: year
      }
    }

    // Calculate remaining days if not provided or if calculation seems wrong
    if (balanceData.joursRestants === 0 || !balanceData.joursRestants) {
      balanceData.joursRestants = Math.max(0, balanceData.joursTotal - balanceData.joursUtilises)
    }

    // Validate the numbers make sense
    if (balanceData.joursUtilises > balanceData.joursTotal) {
      // If used days exceed total, adjust remaining to negative (showing debt)
      balanceData.joursRestants = balanceData.joursTotal - balanceData.joursUtilises
    }

    // Ensure all values are proper numbers
    balanceData.joursTotal = Math.max(0, balanceData.joursTotal)
    balanceData.joursUtilises = Math.max(0, balanceData.joursUtilises)
    
    return balanceData
  }

  // Helper to safely parse numbers
  parseNumber(value) {
    if (value === null || value === undefined || value === '') return 0
    
    // If it's already a number
    if (typeof value === 'number') {
      return isNaN(value) ? 0 : value
    }
    
    // If it's a string, try to parse it
    if (typeof value === 'string') {
      const parsed = parseFloat(value.replace(',', '.')) // Handle French decimal notation
      return isNaN(parsed) ? 0 : parsed
    }
    
    return 0
  }

  // Calculate vacation balance from user's requests (backup method)
  calculateBalanceFromRequests(requests, totalDays = 25) {
    if (!Array.isArray(requests)) return { joursTotal: totalDays, joursUtilises: 0, joursRestants: totalDays }

    const currentYear = new Date().getFullYear()
    
    // Filter approved requests for current year
    const approvedRequests = requests.filter(request => 
      (request.statut === 'Approuve' ) && 
      new Date(request.dateDebut).getFullYear() === currentYear
    )

    // Calculate total used days
    const joursUtilises = approvedRequests.reduce((total, request) => {
      const duration = this.calculateDuration(request.dateDebut, request.dateFin)
      return total + duration
    }, 0)

    const joursRestants = Math.max(0, totalDays - joursUtilises)

    return {
      joursTotal: totalDays,
      joursUtilises,
      joursRestants,
      annee: currentYear
    }
  }

  // Get all vacation requests for current user
  async getUserVacationRequests() {
    const { response, result } = await this.makeRequest(`${this.apiBaseUrl}/user`, {
      method: 'GET'
    })

    if (!response) return result

    if (response.ok && result.success) {
      return {
        success: true,
        data: result.data || [],
        message: result.message || 'Demandes récupérées avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`
      }
    }
  }

  // Create a new vacation request
  async createVacationRequest(requestData) {
        const payload = {
      DateDebut: this.formatDateForApi(requestData.dateDebut),
      DateFin: this.formatDateForApi(requestData.dateFin),
      Type: requestData.type,
      Commentaire: requestData.commentaire || null
    }
    
    
    const { response, result } = await this.makeRequest(`${this.apiBaseUrl}/user`, {
      method: 'POST',
      body: JSON.stringify(payload)
    })

    if (!response) return result

    
    if (response.ok && result.success !== false) {
      return {
        success: true,
        data: result.data,
        message: result.message || 'Demande créée avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`,
        errors: result.errors || []
      }
    }
  }

  // Update vacation request
  async updateVacationRequest(id, requestData) {
    const payload = {
      DateDebut: this.formatDateForApi(requestData.dateDebut),
      DateFin: this.formatDateForApi(requestData.dateFin),
      Type: requestData.type,
      Commentaire: requestData.commentaire || null
    }
    
    const { response, result } = await this.makeRequest(`${this.apiBaseUrl}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    })

    if (!response) return result
    
    if (response.ok && result.success !== false) {
      return {
        success: true,
        data: result.data,
        message: result.message || 'Demande mise à jour avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`,
        errors: result.errors || []
      }
    }
  }

  // Delete vacation request
  async deleteVacationRequest(id) {
    const { response, result } = await this.makeRequest(`${this.apiBaseUrl}/${id}`, {
      method: 'DELETE'
    })

    if (!response) return result
    
    if (response.ok) {
      return {
        success: true,
        message: result.message || 'Demande supprimée avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`
      }
    }
  }

  // Get vacation requests for manager (team members' requests)
  async getTeamVacationRequests() {
    const { response, result } = await this.makeRequest(`${this.apiBaseUrl}/manager`, {
      method: 'GET'
    })

    if (!response) return result
    
    if (response.ok && result.success) {
      return {
        success: true,
        data: result.data || [],
        message: result.message || 'Demandes de l\'équipe récupérées avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`
      }
    }
  }

  // Update vacation request status (for managers)
  async updateVacationRequestStatus(id, status, commentaire = null) {
    const payload = {
      Statut: status,
      CommentaireManager: commentaire
    }
    
    const { response, result } = await this.makeRequest(`${this.apiBaseUrl}/${id}/statut`, {
      method: 'PATCH',
      body: JSON.stringify(payload)
    })

    if (!response) return result
    
    if (response.ok && result.success !== false) {
      return {
        success: true,
        data: result.data,
        message: result.message || 'Statut mis à jour avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`,
        errors: result.errors || []
      }
    }
  }

  // Get requests by status (for admins)
  async getVacationRequestsByStatus(status) {
    const { response, result } = await this.makeRequest(`${this.apiBaseUrl}/statut/${status}`, {
      method: 'GET'
    })

    if (!response) return result
    
    if (response.ok && result.success) {
      return {
        success: true,
        data: result.data || [],
        message: result.message || 'Demandes récupérées avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`
      }
    }
  }

  // Get all requests (for admins)
  async getAllVacationRequests() {
    const { response, result } = await this.makeRequest(`${this.apiBaseUrl}`, {
      method: 'GET'
    })

    if (!response) return result
    
    if (response.ok && result.success) {
      return {
        success: true,
        data: result.data || [],
        message: result.message || 'Toutes les demandes récupérées avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`
      }
    }
  }

  // Utility methods
  formatVacationType(type) {
    const typeMap = {
      'CongeAnnuel': 'Congé Annuel',
      'Maladie': 'Maladie',
      'Maternite': 'Maternité',
      'Paternite': 'Paternité',
      'DecesProche': 'Décès Proche'
    }
    return typeMap[type] || type
  }

  formatVacationStatus(status) {
    const statusMap = {
      'EnAttente': 'En Attente',
      'Approuve': 'Approuvé',
      'Refuse': 'Refusé'
    }
    return statusMap[status] || status
  }

  getStatusColor(status) {
    const colorMap = {
      'EnAttente': 'orange',
      'Approuve': 'green',
      'Refuse': 'red'
    }
    return colorMap[status] || 'gray'
  }

  getTypeIcon(type) {
    const iconMap = {
      'CongeAnnuel': 'fas fa-calendar-alt',
      'Maladie': 'fas fa-plus-circle',
      'Maternite': 'fas fa-baby',
      'Paternite': 'fas fa-male',
      'DecesProche': 'fas fa-heart'
    }
    return iconMap[type] || 'fas fa-calendar'
  }

  // Calculate duration between dates
  calculateDuration(startDate, endDate) {
    const start = new Date(startDate)
    const end = new Date(endDate)
    const diffTime = Math.abs(end - start)
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1 // +1 to include both start and end dates
    return diffDays
  }

  // Validate vacation request dates
  validateVacationDates(startDate, endDate) {
    const start = new Date(startDate)
    const end = new Date(endDate)
    const today = new Date()
    
    const errors = []

    if (start < today.setHours(0, 0, 0, 0)) {
      errors.push('La date de début ne peut pas être dans le passé')
    }

    if (end < start) {
      errors.push('La date de fin doit être après la date de début')
    }

    if (this.calculateDuration(startDate, endDate) > 365) {
      errors.push('La durée de congé ne peut pas dépasser 365 jours')
    }

    return errors
  }

  // Format date for display
  formatDate(dateString) {
    if (!dateString) return ''
    return new Date(dateString).toLocaleDateString('fr-FR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    })
  }

  // Format date for API (ISO format)
  formatDateForApi(date) {
    if (!date) return null
    
    if (typeof date === 'string') {
      // If it's already in YYYY-MM-DD format, convert to ISO
      if (date.match(/^\d{4}-\d{2}-\d{2}$/)) {
        return new Date(date + 'T00:00:00.000Z').toISOString()
      }
      return new Date(date).toISOString()
    }
    return date.toISOString()
  }
}

// Export singleton instance
export const vacationService = new VacationService()
export default vacationService