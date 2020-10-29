using System.Collections.Generic;

namespace Phonebook.Domain.Model.ValueObjects
{
    public class PhoneNumber : ValueObject
    {
        public string Value { get; private set; }

        public PhoneNumber(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            return new object[] { Value };
        }
    }
}
