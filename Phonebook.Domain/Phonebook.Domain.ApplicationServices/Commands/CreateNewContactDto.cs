using Phonebook.Domain.Model.ValueObjects;
using System;

namespace Phonebook.Domain.ApplicationServices.Commands
{
    public class CreateNewContactDto
    {
        public CreateNewContactDto(
            Guid ownerUserId,
            string contactFullName,
            PhoneNumber contactPhoneNumber)
        {
            OwnerUserId = ownerUserId;
            ContactFullName = contactFullName;
            ContactPhoneNumber = contactPhoneNumber;
        }

        public Guid OwnerUserId { get; set; }
        
        public string ContactFullName { get; protected set; }

        public PhoneNumber ContactPhoneNumber { get; protected set; }
    }
}