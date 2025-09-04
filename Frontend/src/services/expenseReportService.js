// Fixed expenseReportService.js - handles 400 status for "no data found" scenarios

class ExpenseReportService {
  constructor() {
    this.apiBaseUrl = 'http://localhost:5032/api'
    this.noteDefraisUrl = `${this.apiBaseUrl}/NoteDeFrais`
    this.ligneNotefraisUrl = `${this.apiBaseUrl}/LigneNoteFrais`
    this.projetUrl = `${this.apiBaseUrl}/Projet`
    this.tarifKmUrl = `${this.apiBaseUrl}/TarifKm`
  }

  // Helper method to safely parse JSON response
  async safeJsonParse(response) {
    try {
      const text = await response.text()
      console.log('Raw response text:', text)
      
      if (!text || text.trim() === '') {
        return {
          success: response.ok,
          message: response.ok ? 'Opération réussie' : `Erreur ${response.status}: ${response.statusText}`,
          data: response.ok ? [] : null
        }
      }
      
      // Try to parse as JSON first
      try {
        const parsed = JSON.parse(text)
        console.log('Parsed JSON response:', parsed)
        
        // Handle different response structures
        if (parsed.success !== undefined) {
          return parsed
        }
        
        if (parsed.data !== undefined) {
          return {
            success: response.ok,
            message: parsed.message || (response.ok ? 'Opération réussie' : 'Erreur'),
            data: parsed.data
          }
        }
        
        // If response is direct data
        return {
          success: response.ok,
          message: response.ok ? 'Opération réussie' : 'Erreur',
          data: parsed
        }
      } catch (jsonError) {
        // Handle plain text responses
        console.log('Response is plain text:', text)
        
        // Check for "no data found" type messages - treat as successful empty result
        // Handle both 404 AND 400 status for "no data found" scenarios
        if (text.includes('Aucune note de frais trouvée') || 
            text.includes('No expense reports found') ||
            text.includes('aucune note de frais trouvée') ||
            text.includes('pour l\'utilisateur avec Id=') ||
            response.status === 404 || 
            (response.status === 400 && text.toLowerCase().includes('aucune'))) {
          return {
            success: true,
            message: 'No expense reports found yet',
            data: []
          }
        }
        
        // For other plain text responses
        return {
          success: response.ok,
          message: response.ok ? text : `Server error: ${text}`,
          data: response.ok ? [] : null
        }
      }
    } catch (error) {
      console.error('Failed to process response:', error)
      return {
        success: false,
        message: response.status === 401 ? 'Session expirée. Veuillez vous reconnecter.' : 'Erreur de traitement de la réponse',
        data: null
      }
    }
  }

  // Check if user is authenticated
  isAuthenticated() {
    return !!(localStorage.getItem('userRole') || sessionStorage.getItem('userRole'))
  }

  // Handle authentication errors
  handleAuthError() {
    localStorage.removeItem('userRole')
    sessionStorage.removeItem('userRole')
    
    window.dispatchEvent(new CustomEvent('auth-expired'))
    
    return {
      success: false,
      message: 'Session expirée. Veuillez vous reconnecter.'
    }
  }

  // Make authenticated request using HttpOnly cookies
  async makeRequest(url, options = {}) {
    console.log('🌐 Making request to:', url)
    console.log('🔧 Request options:', options.method || 'GET')
    
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

    console.log('📋 Request will include cookies via credentials: include')

    try {
      const response = await fetch(url, mergedOptions)
      console.log('📡 Response status:', response.status)
      
      if (response.status === 401) {
        console.log('🚫 401 Unauthorized - handling auth error')
        return { response, result: this.handleAuthError() }
      }

      const result = await this.safeJsonParse(response)
      return { response, result }
    } catch (error) {
      console.error('❌ Network error:', error)
      return {
        response: null,
        result: {
          success: false,
          message: 'Erreur réseau. Vérifiez votre connexion.'
        }
      }
    }
  }

  // Get user's expense reports - improved handling
  async getUserExpenseReports() {
    console.log('🔍 Fetching user expense reports...')
    
    const { response, result } = await this.makeRequest(`${this.noteDefraisUrl}/user`, {
      method: 'GET'
    })

    if (!response) return result

    // Handle success cases (including 200 with empty data)
    if (response.ok || (response.status === 400 && result.success)) {
      console.log('✅ Successfully handled expense reports request')
      return {
        success: true,
        data: result.data || [],
        message: result.message || 'Notes de frais récupérées avec succès'
      }
    } 
    
    // Handle authentication errors
    if (response.status === 401) {
      return result
    }
    
    // Handle other errors
    console.log('❌ Failed to fetch expense reports:', response.status, result.message)
    return {
      success: false,
      message: result.message || `Erreur ${response.status}: Impossible de récupérer les notes de frais`
    }
  }

  // Get manager's expense reports - for managers to see their team's reports
  async getManagerExpenseReports() {
    console.log('🔍 Fetching manager expense reports...')
    
    const { response, result } = await this.makeRequest(`${this.noteDefraisUrl}/manager`, {
      method: 'GET'
    })

    if (!response) return result

    // Handle success cases (including 200 with empty data)
    if (response.ok || (response.status === 400 && result.success)) {
      console.log('✅ Successfully handled manager expense reports request')
      return {
        success: true,
        data: result.data || [],
        message: result.message || 'Notes de frais de l\'équipe récupérées avec succès'
      }
    } 
    
    // Handle authentication errors
    if (response.status === 401) {
      return result
    }
    
    // Handle permission errors
    if (response.status === 403) {
      console.log('❌ Access denied - not a manager')
      return {
        success: false,
        message: 'Accès refusé. Vous devez être manager pour accéder à ces données.'
      }
    }
    
    // Handle other errors
    console.log('❌ Failed to fetch manager expense reports:', response.status, result.message)
    return {
      success: false,
      message: result.message || `Erreur ${response.status}: Impossible de récupérer les notes de frais de l'équipe`
    }
  }

  // Get all projects (handle 403 gracefully)
  async getAllProjects() {
    const { response, result } = await this.makeRequest(this.projetUrl, {
      method: 'GET'
    })

    if (!response) return result
    
    // Handle 403 as "no access" rather than error
    if (response.status === 403) {
      console.log('ℹ️ Projects not accessible - 403 Forbidden')
      return {
        success: true,
        data: [],
        message: 'Projets non accessibles'
      }
    }
    
    if (response.ok && result.success !== false) {
      return {
        success: true,
        data: result.data || [],
        message: result.message || 'Projets récupérés avec succès'
      }
    } else {
      return {
        success: true, // Don't fail the whole component
        data: [],
        message: 'Projets non disponibles'
      }
    }
  }

  // Get all km rates (handle 403 gracefully)
  async getAllKmRates() {
    const { response, result } = await this.makeRequest(this.tarifKmUrl, {
      method: 'GET'
    })

    if (!response) return result
    
    // Handle 403 as "no access" rather than error
    if (response.status === 403) {
      console.log('ℹ️ KM rates not accessible - 403 Forbidden')
      return {
        success: true,
        data: [],
        message: 'Tarifs km non accessibles'
      }
    }
    
    if (response.ok && result.success !== false) {
      return {
        success: true,
        data: result.data || [],
        message: result.message || 'Tarifs km récupérés avec succès'
      }
    } else {
      return {
        success: true, // Don't fail the whole component
        data: [],
        message: 'Tarifs km non disponibles'
      }
    }
  }

  // Create expense report
  async createExpenseReport(reportData) {
    console.log('Creating expense report:', reportData)
    
    const payload = {
      ProjetId: reportData.projetId || null,
      DateSoumission: reportData.dateSoumission || new Date().toISOString(),
      Statut: reportData.statut || 'EnAttente'
    }
    
    console.log('API Payload:', payload)
    
    const { response, result } = await this.makeRequest(this.noteDefraisUrl, {
      method: 'POST',
      body: JSON.stringify(payload)
    })

    if (!response) return result
    
    if (response.ok && result.success !== false) {
      return {
        success: true,
        data: result.data,
        message: result.message || 'Note de frais créée avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`,
        errors: result.errors || []
      }
    }
  }

  // Create expense line
  async createExpenseLine(lineData) {
    console.log('Creating expense line:', lineData)
    
    const payload = {
      NoteDeFraisId: lineData.noteDeFraisId,
      Date: this.formatDateForApi(lineData.date),
      Description: lineData.description,
      Montant: parseFloat(lineData.montant),
      JustificatifPath: lineData.justificatifPath || null,
      TarifKmId: lineData.tarifKmId || null,
      DistanceKm: lineData.distanceKm ? parseInt(lineData.distanceKm) : null
    }
    
    console.log('API Payload:', payload)
    
    const { response, result } = await this.makeRequest(this.ligneNotefraisUrl, {
      method: 'POST',
      body: JSON.stringify(payload)
    })

    if (!response) return result
    
    if (response.ok && result.success !== false) {
      return {
        success: true,
        data: result.data,
        message: result.message || 'Ligne de frais créée avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`,
        errors: result.errors || []
      }
    }
  }

  // Update expense line
  async updateExpenseLine(lineData) {
    const payload = {
      Id: lineData.id,
      NoteDeFraisId: lineData.noteDeFraisId,
      Date: this.formatDateForApi(lineData.date),
      Description: lineData.description,
      Montant: parseFloat(lineData.montant),
      JustificatifPath: lineData.justificatifPath || null,
      TarifKmId: lineData.tarifKmId || null,
      DistanceKm: lineData.distanceKm ? parseInt(lineData.distanceKm) : null
    }
    
    const { response, result } = await this.makeRequest(this.ligneNotefraisUrl, {
      method: 'PUT',
      body: JSON.stringify(payload)
    })

    if (!response) return result
    
    if (response.ok && result.success !== false) {
      return {
        success: true,
        data: result.data,
        message: result.message || 'Ligne de frais mise à jour avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`,
        errors: result.errors || []
      }
    }
  }

  // Update expense report
  async updateExpenseReport(id, reportData) {
    const payload = {
      ProjetId: reportData.projetId || null,
      DateSoumission: reportData.dateSoumission || new Date().toISOString(),
      Statut: reportData.statut || 'EnAttente'
    }
    
    const { response, result } = await this.makeRequest(`${this.noteDefraisUrl}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    })

    if (!response) return result
    
    if (response.ok && result.success !== false) {
      return {
        success: true,
        data: result.data,
        message: result.message || 'Note de frais mise à jour avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`,
        errors: result.errors || []
      }
    }
  }

  // Update expense report status (Manager/Admin only)
  async updateExpenseReportStatus(statusData) {
    console.log('Updating expense report status:', statusData)
    
    const payload = {
    id: statusData.id,
    userId: statusData.userId,
    statut: statusData.statut,
    commentaireMananger: statusData.commentaireMananger || null // Change this field name
  }
      
    console.log('Status update payload:', payload)
    
    const { response, result } = await this.makeRequest(`${this.noteDefraisUrl}/status`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    })

    if (!response) return result
    
    if (response.ok && result.success !== false) {
      return {
        success: true,
        data: result.data,
        message: result.message || 'Statut de la note de frais mis à jour avec succès'
      }
    } else {
      // Handle specific error cases based on status codes
      let errorMessage = result.message || `Erreur ${response.status}: ${response.statusText}`
      
      if (response.status === 400) {
        errorMessage = result.message || 'Données invalides pour la mise à jour du statut'
      } else if (response.status === 404) {
        errorMessage = 'Note de frais introuvable'
      } else if (response.status === 409) {
        errorMessage = 'Conflit lors de la mise à jour du statut'
      } else if (response.status === 403) {
        errorMessage = 'Vous n\'avez pas l\'autorisation de modifier ce statut'
      }
      
      return {
        success: false,
        message: errorMessage,
        errors: result.errors || []
      }
    }
  }

  // Delete expense report
  async deleteExpenseReport(id) {
    const { response, result } = await this.makeRequest(`${this.noteDefraisUrl}/${id}`, {
      method: 'DELETE'
    })

    if (!response) return result
    
    if (response.ok) {
      return {
        success: true,
        message: result.message || 'Note de frais supprimée avec succès'
      }
    } else {
      return {
        success: false,
        message: result.message || `Erreur ${response.status}: ${response.statusText}`
      }
    }
  }

  // Utility methods
  formatExpenseStatus(status) {
    const statusMap = {
      'EnAttente': 'En Attente',
      'Approuvee': 'Approuvée',
      'Refusee': 'Refusée'
    }
    return statusMap[status] || status
  }

  getStatusIcon(status) {
    const iconMap = {
      'EnAttente': 'fas fa-clock',
      'Approuvee': 'fas fa-check-circle',
      'Refusee': 'fas fa-times-circle'
    }
    return iconMap[status] || 'fas fa-question-circle'
  }

  getStatusColor(status) {
    const colorMap = {
      'EnAttente': 'warning',
      'Approuvee': 'success',
      'Refusee': 'danger'
    }
    return colorMap[status] || 'secondary'
  }

  calculateExpenseTotal(lignes) {
    if (!Array.isArray(lignes)) return 0
    return lignes.reduce((total, ligne) => total + (ligne.montant || 0), 0)
  }

  calculateTravelExpense(distanceKm, tarifParKm) {
    if (!distanceKm || !tarifParKm) return 0
    return parseFloat(distanceKm) * parseFloat(tarifParKm)
  }

  // Format date for API (ISO format)
  formatDateForApi(date) {
    if (!date) return null
    
    if (typeof date === 'string') {
      if (date.match(/^\d{4}-\d{2}-\d{2}$/)) {
        return new Date(date + 'T00:00:00.000Z').toISOString()
      }
      return new Date(date).toISOString()
    }   
    return date.toISOString()
  }
}

// Export singleton instance
export const expenseService = new ExpenseReportService()
export default expenseService