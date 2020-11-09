using Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Phonebook.Domain.ApplicationServices.Queries
{
    public class GetPhonebookContactsQuery
    {
        private readonly IPhonebookDbContextFactory _phonebookDbContextFactory;

        public GetPhonebookContactsQuery(IPhonebookDbContextFactory phonebookDbContextFactory)
        {
            _phonebookDbContextFactory = phonebookDbContextFactory;
        }

        public async Task<IEnumerable<PhonebookContactDto>> Execute(int ownerUserId)
        {
            using var phonebookDbContext = _phonebookDbContextFactory.Create();

            var userPhonebook = await phonebookDbContext.GetUserPhonebook(ownerUserId);

            if (userPhonebook is null)
            {
                throw new UserPhonebookNotFoundException(ownerUserId);
            }

            return userPhonebook.Contacts
                .Select(x => new PhonebookContactDto(x.Id, x.ContactName, x.ContactPhoneNumber));
        }
    }
}
