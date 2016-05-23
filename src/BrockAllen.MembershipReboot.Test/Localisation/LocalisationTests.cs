/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrockAllen.MembershipReboot.Test.GroupService
{
    [TestClass]
    public class ClaimsExtensionsTests
    {
        [TestMethod]
        public void GetValue_NullClaims_ReturnsNull()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-GB");
            string english = Resources.ValidationMessages.AccountClosed;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("da");
            string danish = Resources.ValidationMessages.AccountClosed;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("de");
            string german = Resources.ValidationMessages.AccountClosed;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("nl-nl");
            string dutch = Resources.ValidationMessages.AccountClosed;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("no");
            string norwegian = Resources.ValidationMessages.AccountClosed;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-ru");
            string russian = Resources.ValidationMessages.AccountClosed;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("sv");
            string swedish = Resources.ValidationMessages.AccountClosed;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-cn");
            string chinese = Resources.ValidationMessages.AccountClosed;


            Assert.AreEqual(english, "Account closed.");
            Assert.AreEqual(danish, "Konto lukket.");
            Assert.AreEqual(german, "Konto geschlossen.");
            Assert.AreEqual(dutch, "Account gesloten.");
            Assert.AreEqual(norwegian, "Kontoen stengt.");
            Assert.AreEqual(russian, "Счет закрыт.");
            Assert.AreEqual(swedish, "Kontot är avslutat.");
            Assert.AreEqual(chinese, "帐户已关闭。");



        }
    }
}
