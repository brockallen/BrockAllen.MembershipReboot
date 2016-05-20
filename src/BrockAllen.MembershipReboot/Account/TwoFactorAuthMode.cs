/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */
namespace BrockAllen.MembershipReboot
{
  public enum TwoFactorAuthMode
  {
    None = 0,
    Mobile = 1,
    Certificate = 2,
    TimeBasedToken = 3,
    Voice = 4,
    Email = 5
  }
}
