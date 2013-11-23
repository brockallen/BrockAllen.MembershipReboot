/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    class DynamicDictionary : System.Dynamic.DynamicObject
    {
        public Dictionary<string, object> Values { get; set; }
        public DynamicDictionary()
        {
            Values = new Dictionary<string, object>();
        }

        public DynamicDictionary(object values)
            : this()
        {
            if (values != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    object obj2 = descriptor.GetValue(values);
                    Values.Add(descriptor.Name, obj2);
                }
            }
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            Values.TryGetValue(binder.Name, out result);
            return true;
        }

        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
        {
            Values[binder.Name] = value;
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Values.Keys;
        }
    }
}
