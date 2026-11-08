
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "PropertyOwner")]
    public class PropertyOwnerController : Controller
    {
        public IActionResult Dashboard()
        {
            return Content("Welcome to Property Owner Dashboard!");
        }
    }
}
