using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using daSSH.Models;
using Microsoft.AspNetCore.Authorization;

namespace daSSH.Controllers;

[Authorize(AuthenticationSchemes = "Cookies")]
public class ManageController : Controller {
    public IActionResult Instances() {
        return View();
    }
}
