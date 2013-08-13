using BrockAllen.MembershipReboot;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RolesAdmin.Controllers
{
    public class HomeController : Controller
    {
        IGroupRepository groupRepository;
        public HomeController(IGroupRepository groupRepository)
        {
            this.groupRepository = groupRepository;
        }

        public ActionResult Index()
        {
            var roles = groupRepository.GetAll().ToArray();
            return View(roles);
        }

        [HttpPost]
        public ActionResult Add(string name)
        {
            var role = new Group(name);
            groupRepository.Add(role);
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            var grp = groupRepository.Get(id);
            if (grp != null)
            {
                groupRepository.Remove(grp);
            }
            return RedirectToAction("Index");
        }
    }
}
