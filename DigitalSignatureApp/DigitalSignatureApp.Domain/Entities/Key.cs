using DigitalSignatureApp.Domain.Enums;
using DigitalSignatureApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Domain.Entities
{
    public abstract class Key : IKey
    {
        public int Id { get; set; }
        public string KeyPath { get; set; } = string.Empty;
        public string KeyValue { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public KeyType KeyType { get; set; }

        public Key() { }

        public Key(string path, string value)
        {
            Id = Guid.NewGuid().GetHashCode();
            KeyPath = path;
            KeyValue = value;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
