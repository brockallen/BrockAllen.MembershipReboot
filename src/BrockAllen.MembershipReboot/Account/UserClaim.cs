/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot
{
    public class UserClaim
    {
        public UserClaim()
        {
        }
        
        public UserClaim(string type, string value)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentNullException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");

            this.Type = type;
            this.Value = value;
        }

        [StringLength(150)]
        [Required]
        public virtual string Type { get; protected internal set; }
        
        [StringLength(150)]
        [Required]
        public virtual string Value { get; protected internal set; }
    }

    public static class UserClaimCollectionExtensions
    {
        public static UserClaimCollection ToCollection(this IEnumerable<UserClaim> claims)
        {
            return new UserClaimCollection(claims);
        }
    }

    public class UserClaimCollection : System.Collections.Generic.HashSet<UserClaim>
    {
        public static readonly UserClaimCollection Empty = new UserClaimCollection();

        public static implicit operator UserClaimCollection(UserClaim[] claims)
        {
            return new UserClaimCollection(claims);
        }
        
        public static implicit operator UserClaimCollection(Claim[] claims)
        {
            return new UserClaimCollection(claims);
        }

        public UserClaimCollection()
        {
        }

        public UserClaimCollection(System.Collections.Generic.IEnumerable<UserClaim> claims)
        {
            if (claims != null)
            {
                foreach (var claim in claims)
                {
                    this.Add(claim);
                }
            }
        }
        public UserClaimCollection(System.Collections.Generic.IEnumerable<Claim> claims)
        {
            if (claims != null)
            {
                foreach (var claim in claims)
                {
                    this.Add(claim.Type, claim.Value);
                }
            }
        }

        public void Add(string type, string value)
        {
            this.Add(new UserClaim(type, value));
        }
    }
}
