using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkini.Application.DTOs;
using Parkini.Entities;
using Parkini.Infrastructure.Data;
using Parkini.Infrastructure.Notifications;
using Parkini.Infrastructure.Security;

namespace Parkini.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationService _notificationService;

    public NotificationsController(ApplicationDbContext context, NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    // GET /api/notifications?entrepriseId=&abonnementId=&type=&page=&pageSize=
    [HasPermission(Permissions.NotificationsRead)]
    [HttpGet]
    public async Task<ActionResult<NotificationListeResponseDto>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? entrepriseId = null,
        [FromQuery] int? abonnementId = null,
        [FromQuery] string? type = null)
    {
        var query = _context.Notifications
            .Include(n => n.Entreprise)
            .AsQueryable();

        if (entrepriseId.HasValue)
            query = query.Where(n => n.EntrepriseId == entrepriseId.Value);

        if (abonnementId.HasValue)
            query = query.Where(n => n.AbonnementId == abonnementId.Value);

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<TypeNotification>(type, ignoreCase: true, out var typeParsed))
            query = query.Where(n => n.Type == typeParsed);

        query = query.OrderByDescending(n => n.DateCreation);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                Canal = n.Canal.ToString(),
                Statut = n.Statut.ToString(),
                Destinataire = n.Destinataire,
                Sujet = n.Sujet,
                Message = n.Message,
                EntrepriseId = n.EntrepriseId,
                EntrepriseNom = n.Entreprise != null ? n.Entreprise.Nom : null,
                AbonnementId = n.AbonnementId,
                ParkingId = n.ParkingId,
                AccesId = n.AccesId,
                PaiementId = n.PaiementId,
                DateCreation = n.DateCreation,
                DateEnvoi = n.DateEnvoi,
                ErreurDetail = n.ErreurDetail
            })
            .ToListAsync();

        return Ok(new NotificationListeResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    // POST /api/notifications/5/renvoyer
    [HasPermission(Permissions.NotificationsWrite)]
    [HttpPost("{id:int}/renvoyer")]
    public async Task<ActionResult<NotificationDto>> Renvoyer(int id)
    {
        var notif = await _context.Notifications.FindAsync(id);
        if (notif is null)
            return NotFound();

        var nouvelle = await _notificationService.CreerEtEnvoyerAsync(
            notif.Type,
            notif.Destinataire,
            notif.Sujet,
            notif.Message,
            entrepriseId: notif.EntrepriseId,
            abonnementId: notif.AbonnementId,
            parkingId: notif.ParkingId,
            accesId: notif.AccesId,
            paiementId: notif.PaiementId,
            canal: notif.Canal);

        return Ok(new NotificationDto
        {
            Id = nouvelle.Id,
            Type = nouvelle.Type.ToString(),
            Canal = nouvelle.Canal.ToString(),
            Statut = nouvelle.Statut.ToString(),
            Destinataire = nouvelle.Destinataire,
            Sujet = nouvelle.Sujet,
            Message = nouvelle.Message,
            EntrepriseId = nouvelle.EntrepriseId,
            AbonnementId = nouvelle.AbonnementId,
            ParkingId = nouvelle.ParkingId,
            AccesId = nouvelle.AccesId,
            PaiementId = nouvelle.PaiementId,
            DateCreation = nouvelle.DateCreation,
            DateEnvoi = nouvelle.DateEnvoi,
            ErreurDetail = nouvelle.ErreurDetail
        });
    }
}