using System;

namespace SysAudit.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class OrderAttribute : Attribute
    {
        public int Order { get; }

        public OrderAttribute(int order = 0)
        {
            this.Order = order;
        }
    }
}