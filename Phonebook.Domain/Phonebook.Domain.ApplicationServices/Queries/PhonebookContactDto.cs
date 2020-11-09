using Phonebook.Domain.Model.ValueObjects;

namespace Phonebook.Domain.ApplicationServices.Queries
{
    public class PhonebookContactDto
    {
        public PhonebookContactDto(
            int id,
            string contactName,
            PhoneNumber contactPhoneNumber)
        {
            Id = id;
            ContactName = contactName;
            ContactPhoneNumber = contactPhoneNumber;
        }

        public int Id { get; protected set; }

        public string ContactName { get; private set; }

        public PhoneNumber ContactPhoneNumber { get; private set; }
    }
}