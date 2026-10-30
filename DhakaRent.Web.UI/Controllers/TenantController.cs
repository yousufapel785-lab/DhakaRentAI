
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DhakaRent.Web.UI.Controllers
{
    [Authorize(Roles = "Tenant")]
    public class TenantController : Controller
    {
        public IActionResult Dashboard()
        {
            return Content("Welcome to Tenant Dashboard!");
        }
    }
}
