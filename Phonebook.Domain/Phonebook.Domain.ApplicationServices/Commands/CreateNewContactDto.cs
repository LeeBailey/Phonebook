using Phonebook.Domain.Model.ValueObjects;

namespace Phonebook.Domain.ApplicationServices.Commands
{
    public class CreateNewContactDto
    {
        public CreateNewContactDto(
            int ownerUserId,
            string contactFullName,
            PhoneNumber contactPhoneNumber)
        {
            OwnerUserId = ownerUserId;
            ContactFullName = contactFullName;
            ContactPhoneNumber = contactPhoneNumber;
        }

        public int OwnerUserId { get; set; }
        
        public string ContactFullName { get; protected set; }

        public PhoneNumber ContactPhoneNumber { get; protected set; }
    }
}