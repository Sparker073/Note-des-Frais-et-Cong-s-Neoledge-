using MonBackend.Models;
using MonBackend.DTOs;
using MonBackend.Repositories.Interfaces;
using MonBackend.Services.Interfaces;

namespace MonBackend.Services;

public class DemandeCongeService : IDemandeCongeService
{
    private readonly IDemandeCongeRepository _demandeRepository;
    private readonly IJourFerieRepository _jourFerieRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;
    private const int SOLDE_CONGES_ANNUEL = 30; // 30 jours par an

    public DemandeCongeService(
        IDemandeCongeRepository demandeRepository,
        IJourFerieRepository jourFerieRepository,
        IUserService userService,IUserRepository userRepository)
    {
        _demandeRepository = demandeRepository;
        _jourFerieRepository = jourFerieRepository;
        _userService = userService;
        _userRepository = userRepository;
    }

    public async Task<List<DemandeCongeResponseDto>> GetAllDemandesAsync()
    {
        var demandes = await _demandeRepository.GetAllAsync();
        
        var responseList = new List<DemandeCongeResponseDto>();
        foreach(var demande in demandes)
        {
            var dto = await MapToResponseDto(demande);
            responseList.Add(dto);
        }
        return responseList;

    }

    public async Task<DemandeCongeResponseDto?> GetDemandeByIdAsync(int id)
    {
        var demande = await _demandeRepository.GetByIdAsync(id);
        if (demande == null)
            return null;
        return await MapToResponseDto(demande);
    }

    public async Task<List<DemandeCongeResponseDto>> GetDemandesByUserIdAsync(int userId)
    {
        
        var demandes = await _demandeRepository.GetByUserIdAsync(userId);
        //parcourir la liste des demandes et la transformer en ResponceDto un par un puis les retourner
        var responseList = new List<DemandeCongeResponseDto>();
        foreach(var demande in demandes)
        {
            var dto = await MapToResponseDto(demande);
            responseList.Add(dto);  
        }
        return responseList;
    }
    
    public async Task<(DemandeCongeResponseDto demande, List<string> joursFeries)> CreateDemandeAsync(int userId, CreateDemandeCongeDto createDto)
    {
        
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if  (existingUser == null)
            throw new InvalidOperationException("Utilisateur Introuvable !");
        if (existingUser.ManagerId == null)
            throw new InvalidOperationException("Vous pouvez creer une demande sans un manager !");
        // Validation des dates
        if (createDto.DateDebut < DateTime.Today)
            throw new InvalidOperationException("La date de début ne peut pas être dans le passé");

        if (createDto.DateFin < createDto.DateDebut)
            throw new InvalidOperationException("La date de fin ne peut pas être antérieure à la date de début");

        // Vérification des jours fériés
        var joursFeries = await GetHolidaysInRange(createDto.DateDebut, createDto.DateFin);

        // Vérification des conflits
        if (await _demandeRepository.HasConflictAsync(userId, createDto.DateDebut, createDto.DateFin))
            throw new InvalidOperationException("Il y a un conflit avec une demande de congé existante");

        // Vérification du solde
        await ValidateSoldeConges(userId, createDto.DateDebut, createDto.DateFin);

        var demande = new DemandeCongé
        {
            UserId = userId,
            DateDebut = createDto.DateDebut,
            DateFin = createDto.DateFin,
            Type = createDto.Type,
            Commentaire = createDto.Commentaire,
            Statut = StatutDemande.EnAttente,
            DateDemande = DateTime.Now
        };

        var createdDemande = await _demandeRepository.CreateAsync(demande);
        var responce = await MapToResponseDto(createdDemande);
        return (responce , joursFeries);
    }
    
    public async Task<List<DemandeCongeResponseDto>> GetDemandesByManagerIdAsync(int managerId)
    {
       
        var demandes = await _demandeRepository.GetByManagerIdAsync(managerId);        
        var responseList = new List<DemandeCongeResponseDto>();
        foreach(var demande in demandes)
        {
            var dto = await MapToResponseDto(demande);
            responseList.Add(dto);
        }
        return responseList;
    }

    public async Task<List<DemandeCongeResponseDto>> GetDemandesByStatutAsync(StatutDemande statut)
    {
        var demandes = await _demandeRepository.GetByStatutAsync(statut);
        
        //verifier la disponibiltés des demandes 
        if (demandes == null || !demandes.Any())
            return null;

        var responseList = new List<DemandeCongeResponseDto>();

        foreach (var demande in demandes)
        {
            var dto = await MapToResponseDto(demande);
            responseList.Add(dto);
        }

        return responseList;
    }



    public async Task<(DemandeCongeResponseDto demande, List<string> joursFeries)> UpdateDemandeAsync(int id, UpdateDemandeCongeDto updateDto)
    {
        var demande = await _demandeRepository.GetByIdAsync(id);

        // Seules les demandes en attente peuvent être modifiées
        if (demande.Statut != StatutDemande.EnAttente)
            throw new InvalidOperationException("Seules les demandes en attente peuvent être modifiées");

        bool datesChanged = false;
        DateTime newDateDebut = demande.DateDebut;
        DateTime newDateFin = demande.DateFin;

        if (updateDto.DateDebut.HasValue)
        {
            if (updateDto.DateDebut.Value < DateTime.Today)
                throw new InvalidOperationException("La date de début ne peut pas être dans le passé");
            
            newDateDebut = updateDto.DateDebut.Value;
            datesChanged = true;
        }

        if (updateDto.DateFin.HasValue)
        {
            newDateFin = updateDto.DateFin.Value;
            datesChanged = true;
        }

        if (newDateFin < newDateDebut)
            throw new InvalidOperationException("La date de fin ne peut pas être antérieure à la date de début");

        List<string> joursFeries = new();

        if (datesChanged)
        {
            // Vérification des jours fériés
            joursFeries = await GetHolidaysInRange(newDateDebut, newDateFin);

            // Vérification des conflits
            if (await _demandeRepository.HasConflictAsync(demande.UserId, newDateDebut, newDateFin, id))
                throw new InvalidOperationException("Il y a un conflit avec une demande de congé existante");

            // Vérification du solde
            await ValidateSoldeConges(demande.UserId, newDateDebut, newDateFin);
        }

        demande.DateDebut = newDateDebut;
        demande.DateFin = newDateFin;
        if (updateDto.Type.HasValue) demande.Type = updateDto.Type.Value;
        if (updateDto.Commentaire != null) demande.Commentaire = updateDto.Commentaire;

        var updatedDemande = await _demandeRepository.UpdateAsync(demande);
        
        var responce = await MapToResponseDto(updatedDemande);
        return (responce,joursFeries); 
    }

    public async Task<DemandeCongeResponseDto?> UpdateStatutDemandeAsync(int id, int managerId, UpdateStatutDemandeDto updateDto)
    {
        var demande = await _demandeRepository.GetByIdAsync(id);

        
        if (demande.Statut != StatutDemande.EnAttente)
            throw new InvalidOperationException("Cette demande a déjà été traitée");

        demande.Statut = updateDto.Statut;
        demande.CommentaireManager = updateDto.CommentaireManager;
        // Vous pouvez ajouter CommentaireManager si nécessaire dans le modèle

        var updatedDemande = await _demandeRepository.UpdateAsync(demande);
        if (updatedDemande == null)
            return null;
        return await MapToResponseDto(updatedDemande); 
    }

    public async Task<bool> DeleteDemandeAsync(int id)
    {
        var demande = await _demandeRepository.GetByIdAsync(id);
        if (demande == null)
            throw new InvalidOperationException("Demnade Introuvable avec cet ID !");
        // Seules les demandes en attente peuvent être supprimées
        if (demande.Statut != StatutDemande.EnAttente)
            throw new InvalidOperationException("Seules les demandes en attente peuvent être supprimées");

        return await _demandeRepository.DeleteAsync(id);
    }

    public async Task<int> GetSoldeCongesAsync(int userId, int year)
    {
        var joursUtilises = await _demandeRepository.GetNombreJoursCongesByUserAndYearAsync(userId, year);
        return SOLDE_CONGES_ANNUEL - joursUtilises;
    }

    //il faut pas interrompre le code dans lexception de cette methode 
    private async Task<List<string>> GetHolidaysInRange(DateTime dateDebut, DateTime dateFin)
    {
        var joursFeries = await _jourFerieRepository.GetByDateRangeAsync(dateDebut, dateFin);
        return joursFeries.Select(j => j.Date.ToString("dd/MM/yyyy")).ToList();
    }

    private async Task ValidateSoldeConges(int userId, DateTime dateDebut, DateTime dateFin)
    {
        int nombreJours = await CalculerNombreJoursOuvrables(dateDebut, dateFin);
        int soldeActuel = await GetSoldeCongesAsync(userId, dateDebut.Year);
        
        if (nombreJours > soldeActuel)
            throw new InvalidOperationException($"Solde insuffisant. Jours demandés: {nombreJours}, Solde disponible: {soldeActuel}");
    }

    private async Task<int> CalculerNombreJoursOuvrables(DateTime dateDebut, DateTime dateFin)
    {
        int jours = 0;

        for (DateTime date = dateDebut.Date; date <= dateFin.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Sunday)
            {
                jours++;
            }
        }
        //la date des jours feriés est consideré toujours comme un seul jour

        int nbJoursFeries = await _jourFerieRepository.CountJoursFeriesAsync(dateDebut, dateFin);

        return jours - nbJoursFeries;
    }

    private async Task<DemandeCongeResponseDto> MapToResponseDto(DemandeCongé demande)
    {
        return new DemandeCongeResponseDto
        {
            Id = demande.Id,
            UserId = demande.UserId,
            NomEmploye = demande.Employe.Nom,
            EmailEmploye = demande.Employe.Email,
            DateDebut = demande.DateDebut,
            DateFin = demande.DateFin,
            Type = demande.Type,
            Statut = demande.Statut,
            DateDemande = demande.DateDemande,
            Commentaire = demande.Commentaire,
            NombreJours = await CalculerNombreJoursOuvrables(demande.DateDebut, demande.DateFin)
        };
    }
}