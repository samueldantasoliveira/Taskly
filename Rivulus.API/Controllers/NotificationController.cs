using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivulus.Application;
namespace Rivulus.Controllers;
[Authorize, ApiController, Route("api/notification")]
public class NotificationController(UserNotificationService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Get(CancellationToken ct) => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? Ok(await service.GetAsync(id, ct)) : Unauthorized();
    [HttpPost("{id}/read")] public async Task<IActionResult> Read(Guid id, CancellationToken ct) { if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized(); return await service.MarkReadAsync(id, userId, ct) ? NoContent() : NotFound(); }
}
