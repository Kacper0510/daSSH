using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace daSSH.Controllers;

[Authorize(AuthenticationSchemes = "Cookies")]
public class ManageController : Controller {
    public IActionResult Instances() {
        return View();
    }

    public IActionResult NewKeyPair() {
        // TODO show the key
        return RedirectToAction("Instances", "Manage");
    }
}
