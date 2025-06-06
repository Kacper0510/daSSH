using daSSH.Data;
using daSSH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace daSSH.Controllers;

[Authorize(AuthenticationSchemes = "Bearer")]
public class ApiController(DatabaseContext db) : ControllerExt(db) {
    [ActionName("User")]
    [HttpGet]
    public async Task<IActionResult> UserRoute() {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        return Json(new {
            user!.UserID,
            user!.DiscordID,
            user!.Username,
            user!.Avatar,
            user!.CreatedOn,
            user!.PublicKey,
        });
    }

    [HttpGet]
    public async Task<IActionResult> Instances() {
        var user = await _db.Users
            .Include(u => u.Instances)
            .Include(u => u.SharedInstances)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        var instances = user!.Instances
            .Select(i => new {
                i.InstanceID,
                i.Name,
            });
        var sharedInstances = user!.SharedInstances
            .Select(i => new {
                i.InstanceID,
                i.Name,
            });
        return Json(new {
            instances,
            sharedInstances,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Instances([FromBody] InstanceApiModel model) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserID == UserID());

        var instance = new Instance {
            Name = model.Name,
            Owner = user!,
        };
        _db.Instances.Add(instance);
        if (model.Port != null) {
            var portForward = new PortForward {
                Port = (ushort) model.Port,
                Public = model.PublicPort ?? false,
                Instance = instance
            };
            _db.Forwards.Add(portForward);
        }
        await _db.SaveChangesAsync();

        return Json(new { instance.InstanceID });
    }
}
