namespace NhibernateSample.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web.Mvc;

    using BrockAllen.MembershipReboot;
    using BrockAllen.MembershipReboot.Nh;

    using NHibernate;

    public class GroupIndexViewModel
    {
        public IEnumerable<GroupViewModel> Groups { get; set; }

        public IEnumerable<SelectListItem> GroupsAsList
        {
            get
            {
                return this.Groups.Select(x => new SelectListItem { Text = x.Name, Value = x.ID.ToString() });
            }
        }
    }

    public class GroupViewModel
    {
        public Guid ID { get; set; }

        public string Name { get; set; }

        public IEnumerable<GroupViewModel> Children { get; set; }

        public IEnumerable<string> Descendants { get; set; }
    }

    public class GroupController : Controller
    {
        private readonly GroupService<NhGroup> groupSvc;
        private readonly IGroupQuery query;
        private readonly ISession session;

        public GroupController(GroupService<NhGroup> groupSvc, IGroupQuery query, ISession session)
        {
            this.groupSvc = groupSvc;
            this.query = query;
            this.session = session;
        }

        public ActionResult Index(string filter = null)
        {
            var list = new List<GroupViewModel>();
            using (var tx = this.session.BeginTransaction())
            {
                foreach (var result in this.query.Query(groupSvc.DefaultTenant, filter))
                {
                    var item = this.groupSvc.Get(result.ID);

                    var kids = new List<GroupViewModel>();
                    foreach (var child in item.Children)
                    {
                        var childGrp = this.groupSvc.Get(child.ChildGroupID);
                        kids.Add(new GroupViewModel { ID = child.ChildGroupID, Name = childGrp.Name });
                    }

                    var descendants = this.groupSvc.GetDescendants(item).Select(x => x.Name).ToArray();
                    var gvm = new GroupViewModel
                                  {
                                      ID = item.ID,
                                      Name = item.Name,
                                      Children = kids,
                                      Descendants = descendants
                                  };
                    list.Add(gvm);
                }

                tx.Commit();
            }

            var vm = new GroupIndexViewModel { Groups = list };
            return this.View("Index", vm);
        }

        [HttpPost]
        public ActionResult Add(string name)
        {
            try
            {
                using (var tx = this.session.BeginTransaction())
                {
                    this.groupSvc.Create(name);
                    tx.Commit();
                }

                return this.RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                this.ModelState.AddModelError(string.Empty, ex.Message);
            }

            return this.Index();
        }

        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            try
            {
                using (var tx = this.session.BeginTransaction())
                {
                    this.groupSvc.Delete(id);
                    tx.Commit();
                }

                return this.RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                this.ModelState.AddModelError(string.Empty, ex.Message);
            }

            return this.Index();
        }

        [HttpPost]
        public ActionResult ChangeName(Guid id, string name)
        {
            try
            {
                using (var tx = this.session.BeginTransaction())
                {
                    this.groupSvc.ChangeName(id, name);
                    tx.Commit();
                }

                return this.RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                this.ModelState.AddModelError(string.Empty, ex.Message);
            }

            return this.Index();
        }

        [HttpPost]
        public ActionResult AddChild(Guid id, Guid child)
        {
            try
            {
                using (var tx = this.session.BeginTransaction())
                {
                    this.groupSvc.AddChildGroup(id, child);
                    tx.Commit();
                }

                return this.RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                this.ModelState.AddModelError(string.Empty, ex.Message);
            }

            return this.Index();
        }

        [HttpPost]
        public ActionResult RemoveChild(Guid id, Guid child)
        {
            try
            {
                using (var tx = this.session.BeginTransaction())
                {
                    this.groupSvc.RemoveChildGroup(id, child);
                    tx.Commit();
                }

                return this.RedirectToAction("Index");
            }
            catch (ValidationException ex)
            {
                this.ModelState.AddModelError(string.Empty, ex.Message);
            }

            return this.Index();
        }
    }
}