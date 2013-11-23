/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot
{
    public class LinkedAccount
    {
        internal protected LinkedAccount()
        {
            this.Claims = new HashSet<LinkedAccountClaim>();
        }

        [Key]
        [Column(Order = 1)]
        public virtual Guid UserAccountID { get; internal set; }
        [Key]
        [Column(Order=2)]
        [StringLength(30)]
        public virtual string ProviderName { get; internal set; }
        [Key]
        [Column(Order = 3)]
        [StringLength(100)]
        public virtual string ProviderAccountID { get; internal set; }

        public virtual DateTime LastLogin { get; internal set; }

        [Required]
        [ForeignKey("UserAccountID")]
        public virtual UserAccount User { get; internal set; }
        
        public virtual ICollection<LinkedAccountClaim> Claims { get; internal set; }

        public virtual bool HasClaim(string type)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            return this.Claims.Any(x => x.Type == type);
        }

        public virtual bool HasClaim(string type, string value)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            return this.Claims.Any(x => x.Type == type && x.Value == value);
        }

        public virtual IEnumerable<string> GetClaimValues(string type)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in this.Claims
                where claim.Type == type
                select claim.Value;
            return query.ToArray();
        }

        public virtual string GetClaimValue(string type)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var query =
                from claim in this.Claims
                where claim.Type == type
                select claim.Value;
            return query.SingleOrDefault();
        }
        
        public virtual void AddClaim(string type, string value)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            if (!this.HasClaim(type, value))
            {
                this.Claims.Add(
                    new LinkedAccountClaim
                    {
                        Type = type,
                        Value = value
                    });
            }
        }

        public virtual void RemoveClaim(string type)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");

            var claimsToRemove =
                from claim in this.Claims
                where claim.Type == type
                select claim;
            foreach (var claim in claimsToRemove.ToArray())
            {
                this.Claims.Remove(claim);
            }
        }

        public virtual void RemoveClaim(string type, string value)
        {
            if (String.IsNullOrWhiteSpace(type)) throw new ArgumentException("type");
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException("value");

            var claimsToRemove =
                from claim in this.Claims
                where claim.Type == type && claim.Value == value
                select claim;
            foreach (var claim in claimsToRemove.ToArray())
            {
                this.Claims.Remove(claim);
            }
        }

        public virtual void UpdateClaims(IEnumerable<Claim> claims)
        {
            claims = claims ?? Enumerable.Empty<Claim>();
            
            this.Claims.Clear();
            
            foreach (var c in claims)
            {
                this.Claims.Add(
                    new LinkedAccountClaim
                    {
                        Type = c.Type,
                        Value = c.Value
                    });
            }
        }
    }
}
