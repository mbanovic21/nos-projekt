using DigitalSignatureApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalSignatureApp.Domain.Interfaces
{
    public interface IKey
    {
        string KeyPath { get; set; }
        string KeyValue { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime? ExpiresAt { get; set; }
        KeyType KeyType { get; set; }
    }
}
