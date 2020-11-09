using Phonebook.Domain.Model.ValueObjects;

namespace Phonebook.Domain.Model.Entities
{
    public class Contact
    {
        protected Contact() { }

        public Contact(string contactName, PhoneNumber contactPhoneNumber)
        {
            ContactName = contactName;
            ContactPhoneNumber = contactPhoneNumber;
        }

        public int Id { get; protected set; }

        public string ContactName { get; protected set; }

        public PhoneNumber ContactPhoneNumber { get; protected set; } 
    }
}
