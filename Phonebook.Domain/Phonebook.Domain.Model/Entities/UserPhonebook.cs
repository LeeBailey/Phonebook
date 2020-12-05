using System;
using System.Collections.Generic;

namespace Phonebook.Domain.Model.Entities
{
    public class UserPhonebook
    {
        protected UserPhonebook()
        {
            Contacts = new List<Contact>();
        }

        public UserPhonebook(Guid ownerUserId) : this()
        {
            OwnerUserId = ownerUserId;
        }

        public int Id { get; protected set; }

        public Guid OwnerUserId { get; protected set; }

        public ICollection<Contact> Contacts { get; protected set; }
    }
}
