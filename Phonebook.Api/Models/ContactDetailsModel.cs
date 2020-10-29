namespace Phonebook.Api.Models
{
    public class ContactDetailsModel
    {
        public ContactDetailsModel(
            int id,
            string fullName,
            string phoneNumber)
        {
            Id = id;
            FullName = fullName;
            PhoneNumber = phoneNumber;
        }

        public int Id { get; protected set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }
    }
}
