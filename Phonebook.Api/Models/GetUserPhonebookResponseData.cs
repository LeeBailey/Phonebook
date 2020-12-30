using System.Collections.Generic;

namespace Phonebook.Api.Models
{
    public class GetUserPhonebookResponseData
    {
        public IEnumerable<Result>? Results { get; set; }

        public GetUserPhonebookResponseData() { }

        public GetUserPhonebookResponseData(IEnumerable<Result> results)
        {
            Results = results;
        }

        public class Result
        {
            public Result(
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
}
