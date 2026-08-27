using System;
using System.Collections.Generic;
using System.Text;

namespace ChromaDB.Library
{
    public record ChromaDBDatabase(Guid Id, string Name, string Tenant)
    {
        public Guid Id { get; internal set; } = Id;
        public string Name { get; internal set; } = Name;
        public string Tenant { get; internal set; } = Tenant;
    }
}
