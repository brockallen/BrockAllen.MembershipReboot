/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrockAllen.MembershipReboot
{
    public interface IUserCertificate
    {
        string Thumbprint { get; set; }
        string Subject { get; set; }
    }
}
