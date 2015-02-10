/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrockAllen.MembershipReboot.Test.GroupService
{
    [TestClass]
    public class GroupServiceTests
    {
        TestGroupService subject;
        FakeGroupRepository repository;

        [TestInitialize]
        public void Init()
        {
            repository = new FakeGroupRepository();
            subject = new TestGroupService("test", repository);
        }

        [TestMethod]
        public void CreateGroup_CreatesGroupInRepository()
        {
            // Arrange
            var root = subject.Create("root");
            var groupA = subject.Create("childA");
            var groupB = subject.Create("childB");
            subject.AddChildGroup(root.ID, groupA.ID);
            subject.AddChildGroup(root.ID, groupB.ID);

            // Assert
            root = subject.Get(root.ID);

            Assert.AreEqual(1, root.Children.Count(s => s.ChildGroupID == groupA.ID));
            Assert.AreEqual(1, root.Children.Count(s => s.ChildGroupID == groupB.ID));
        }

        [TestMethod]
        public void DeleteGroup_DeletesGroup()
        {
            // Arrange
            var root = subject.Create("root");
            var groupA = subject.Create("childA");
            var groupB = subject.Create("childB");
            subject.AddChildGroup(root.ID, groupA.ID);
            subject.AddChildGroup(root.ID, groupB.ID);

            // Act
            subject.Delete(groupA.ID);

            // Assert
            root = subject.Get(root.ID);
            Assert.AreEqual(0, root.Children.Count(s => s.ChildGroupID == groupA.ID));
            Assert.AreEqual(1, root.Children.Count(s => s.ChildGroupID == groupB.ID));

            groupA = subject.Get(groupA.ID);
            Assert.IsNull(groupA);

            groupB = subject.Get(groupB.ID);
            Assert.IsNotNull(groupB);
        }
    }
}
