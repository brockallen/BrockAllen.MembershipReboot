using BrockAllen.MembershipReboot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace RolesAdmin.Controllers
{
    public class HomeController : Controller
    {
        GroupService groupSvc;
        public HomeController(GroupService groupSvc)
        {
            this.groupSvc = groupSvc;
        }

        public ActionResult Index()
        {
            var groups = groupSvc.GetAll();
            return View("Index", groups);
        }

        [HttpPost]
        public ActionResult Add(string name)
        {
            try
            {
                groupSvc.Create(name);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            var groups = groupSvc.GetAll();
            return View("Index", groups);
        }

        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            try
            {
                groupSvc.Delete(id);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            var groups = groupSvc.GetAll();
            return View("Index", groups);
        }

        [HttpPost]
        public ActionResult ChangeName(Guid id, string name)
        {
            try
            {
                groupSvc.ChangeName(id, name);
                return RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            var groups = groupSvc.GetAll();
            return View("Index", groups);
        }
    }
}
