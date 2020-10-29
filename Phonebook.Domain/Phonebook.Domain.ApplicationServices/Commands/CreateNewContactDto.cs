using Phonebook.Domain.Model.ValueObjects;

namespace Phonebook.Domain.ApplicationServices.Commands
{
    public class CreateNewContactDto
    {
        public CreateNewContactDto(
            int userPhonebookId,
            string contactFullName,
            PhoneNumber contactPhoneNumber)
        {
            UserPhonebookId = userPhonebookId;
            ContactFullName = contactFullName;
            ContactPhoneNumber = contactPhoneNumber;
        }

        public int UserPhonebookId { get; set; }
        
        public string ContactFullName { get; set; }

        public PhoneNumber ContactPhoneNumber { get; set; }
    }
}