using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using daSSH.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using daSSH.Models;

namespace daSSH.Controllers;

[Authorize(AuthenticationSchemes = "Cookies")]
public class ManageController(DatabaseContext db) : ControllerExt(db) {
    public async Task<IActionResult> Instances() {
        var user = await _db.Users
            .Include(u => u.Instances)
            .Include(u => u.SharedInstances)
            .ThenInclude(i => i.Owner)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        return View(user);
    }

    public IActionResult NewKeyPair() {
        return View(model: User.FindFirstValue("daSSH-private-key"));
    }

    public IActionResult NewToken() {
        return View(model: User.FindFirstValue("daSSH-token"));
    }

    public async Task<IActionResult> Account() {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount() {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Index", "Default");
    }

    public async Task<IActionResult> RegenerateKeyPair() {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        var privKey = await user.GenerateNewKeyPair();
        await _db.SaveChangesAsync();

        await ReissueCookie(
            user.UserID,
            user.Username,
            user.Avatar,
            new Claim("daSSH-private-key", privKey)
        );

        return RedirectToAction("NewKeyPair");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteToken() {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        user.APIToken = null;
        await _db.SaveChangesAsync();

        return RedirectToAction("Account");
    }

    public async Task<IActionResult> GenerateToken() {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        user.GenerateNewToken();
        await _db.SaveChangesAsync();

        await ReissueCookie(
            user.UserID,
            user.Username,
            user.Avatar,
            new Claim("daSSH-token", user.APIToken!)
        );

        return RedirectToAction("NewToken");
    }

    [HttpPost]
    public async Task<IActionResult> CreateInstance(string name, bool forward, ushort port, bool publicPort) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }

        var instance = new Instance {
            Name = name,
            Owner = user,
        };
        _db.Instances.Add(instance);
        if (forward) {
            var portForward = new PortForward {
                Port = port,
                Public = publicPort,
                Instance = instance
            };
            _db.Forwards.Add(portForward);
        }
        await _db.SaveChangesAsync();

        return RedirectToAction("Instance", new { id = instance.InstanceID });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteInstance(int id) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var user = await _db.Users
            .Include(u => u.Instances)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        if (!user.Instances.Any(i => i.InstanceID == id)) {
            return BadRequest("You do not own this instance.");
        }

        var instance = await _db.Instances
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InstanceID == id);
        if (instance == null) {
            return NotFound();
        }

        _db.Instances.Remove(instance);
        await _db.SaveChangesAsync();
        return RedirectToAction("Instances");
    }

    public async Task<IActionResult> Instance(int id) {
        if (!ModelState.IsValid) {
            return NotFound(ModelState);
        }

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }

        var instance = await _db.Instances
            .Include(i => i.PortForward)
            .Include(i => i.Owner)
            .Include(i => i.SharedUsers)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InstanceID == id);
        if (instance == null) {
            return NotFound();
        }

        if (instance.Owner.UserID != user.UserID && !instance.SharedUsers.Any(u => u.UserID == user.UserID)) {
            return BadRequest("You do not have access to this instance.");
        }
        var isShared = instance.Owner.UserID != user.UserID;

        return View((instance, isShared));
    }

    [HttpPost]
    public async Task<IActionResult> EditInstance(int id, string name, bool forward, ushort port, bool publicPort) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var user = await _db.Users
            .Include(u => u.Instances)
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        if (!user.Instances.Any(i => i.InstanceID == id)) {
            return BadRequest("You do not own this instance.");
        }

        var instance = await _db.Instances
            .Include(i => i.PortForward)
            .FirstOrDefaultAsync(i => i.InstanceID == id);
        if (instance == null) {
            return NotFound();
        }

        instance.Name = name;
        if (forward) {
            if (instance.PortForward == null) {
                var portForward = new PortForward {
                    Port = port,
                    Public = publicPort,
                    Instance = instance
                };
                _db.Forwards.Add(portForward);
            } else {
                instance.PortForward.Port = port;
                instance.PortForward.Public = publicPort;
            }
        } else if (instance.PortForward != null) {
            _db.Forwards.Remove(instance.PortForward);
        }
        await _db.SaveChangesAsync();

        return RedirectToAction("Instance", new { id });
    }

    [HttpPost]
    public async Task<IActionResult> AddShare(int instance, string username) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var user = await _db.Users
            .Include(u => u.Instances)
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        if (!user.Instances.Any(i => i.InstanceID == instance)) {
            return BadRequest("You do not own this instance.");
        }

        var targetUser = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username);
        if (targetUser == null) {
            return RedirectToAction("Instance", new { id = instance });
        }  // TODO handle case where targetUser is the same as user

        var targetInstance = await _db.Instances
            .Include(i => i.SharedUsers)
            .FirstOrDefaultAsync(i => i.InstanceID == instance);
        if (targetInstance == null) {
            return NotFound();
        }

        if (!targetInstance.SharedUsers.Any(u => u.UserID == targetUser.UserID)) {
            targetInstance.SharedUsers.Add(targetUser);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Instance", new { id = instance });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveShare(int instance, string username) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var user = await _db.Users
            .Include(u => u.Instances)
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        if (!user.Instances.Any(i => i.InstanceID == instance)) {
            return BadRequest("You do not own this instance.");
        }

        var targetUser = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == username);
        if (targetUser == null) {
            return RedirectToAction("Instance", new { id = instance });
        }

        var targetInstance = await _db.Instances
            .Include(i => i.SharedUsers)
            .FirstOrDefaultAsync(i => i.InstanceID == instance);
        if (targetInstance == null) {
            return NotFound();
        }

        if (targetInstance.SharedUsers.Any(u => u.UserID == targetUser.UserID)) {
            targetInstance.SharedUsers.Remove(targetUser);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Instance", new { id = instance });
    }

    [HttpPost]
    public async Task<IActionResult> LeaveInstance(int id) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var user = await _db.Users
            .Include(u => u.SharedInstances)
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        var instance = await _db.Instances
            .FirstOrDefaultAsync(i => i.InstanceID == id);
        if (instance == null) {
            return NotFound();
        }
        if (!user.SharedInstances.Any(i => i.InstanceID == id)) {
            return BadRequest("You do not have access to this instance.");
        }

        instance.SharedUsers.Remove(user);
        await _db.SaveChangesAsync();
        return RedirectToAction("Instances");
    }

    public IActionResult Files(int id) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        return View(id);
    }
}
