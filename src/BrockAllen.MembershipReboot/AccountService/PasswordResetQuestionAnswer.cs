/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class PasswordResetQuestionAnswer
    {
        public Guid QuestionID { get; set; }
        public string Answer { get; set; }
    }
}
