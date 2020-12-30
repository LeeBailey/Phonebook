using Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance;
using Phonebook.Domain.Model.ValueObjects;
using System;
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

        public async Task<Response> Execute(Guid ownerUserId)
        {
            using var phonebookDbContext = _phonebookDbContextFactory.Create();

            var userPhonebook = await phonebookDbContext.GetUserPhonebook(ownerUserId);

            if (userPhonebook is null)
            {
                throw new UserPhonebookNotFoundException(ownerUserId);
            }

            return new Response(userPhonebook.Contacts
                .Select(x => new Result(x.Id, x.ContactName, x.ContactPhoneNumber)));
        }

        public class Response
        {
            public readonly IEnumerable<Result> Results;

            public Response(IEnumerable<Result> results)
            {
                Results = results;
            }
        }

        public class Result
        {
            public Result(
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
}
