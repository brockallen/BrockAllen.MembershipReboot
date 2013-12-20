/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public class PasswordResetQuestionAnswer
    {
        public Guid QuestionID { get; set; }
        public string Answer { get; set; }
    }
}
