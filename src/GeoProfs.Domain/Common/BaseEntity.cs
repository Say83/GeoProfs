using System;

namespace GeoProfs.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; }
    }
}
