using Shared.Kernel;

namespace Payments.Domain.Entities
{
    public class Bank : Entity
    {
        public string Name { get; private set; }
        public string CountryCode { get; private set; }
        public string? SwiftCode { get; private set; }

        public Bank(Guid id, string name, string countryCode, string? swiftCode = null)
        {
            Id = id;
            Name = name;
            CountryCode = countryCode;
            SwiftCode = swiftCode;
        }

        // EF Core
        protected Bank() { }
    }
}
