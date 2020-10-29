using Dapper;
using Microsoft.Extensions.Configuration;
using Phonebook.Domain.Model.ValueObjects;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Phonebook.Infrastructure.EntityPersistance.Tests.Integration
{
    internal static class DataHelper
    {
        internal static string GetConnectionString()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("configuration.json")
                .Build();

            return config.GetConnectionString("PhonebookDbConnection");
        }

        internal static UserPhonebookData GetUserPhonebook(int userPhonebookId)
        {
            using var connection = new SqlConnection(GetConnectionString());
            var userPhonebook = connection.Query(
                "SELECT * FROM UserPhonebook WHERE Id = @Id",
                new { Id = userPhonebookId })
                .Select(row => new UserPhonebookData() { Id = (int)row.Id, OwnerUserId = (int)row.OwnerUserId })
                .FirstOrDefault();

            if (userPhonebook is null)
            {
                return null;
            }

            userPhonebook.Contacts = GetContactData(userPhonebook.Id);

            return userPhonebook;
        }

        internal static IEnumerable<ContactData> GetContactData(int userPhonebookId)
        {
            using var connection = new SqlConnection(GetConnectionString());
            return connection.Query(
                "SELECT * FROM Contact WHERE UserPhonebookId = @Id",
                new { Id = userPhonebookId })
                .Select(row => new ContactData()
                {
                    Id = (int)row.Id,
                    UserPhonebookId = (int)row.UserPhonebookId,
                    ContactName = (string)row.ContactName,
                    ContactPhoneNumber = new PhoneNumber((string)row.ContactPhoneNumber)
                }).ToList();
        }

        internal static void DeletePhonebookContacts(int userPhonebookId)
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Execute(
                "DELETE FROM Contact WHERE UserPhonebookId = @Id",
                new { Id = userPhonebookId });
        }

        internal static void DeletePhonebook(int userPhonebookId)
        {
            using var connection = new SqlConnection(GetConnectionString());
            connection.Execute(
                "DELETE FROM UserPhonebook WHERE Id = @Id",
                new { Id = userPhonebookId });
        }

        internal static void SaveUserPhonebook(UserPhonebookData userPhonebook)
        {
            using var connection = new SqlConnection(GetConnectionString());
            userPhonebook.Id = connection.Query<int>(
                @"INSERT INTO UserPhonebook (OwnerUserId) VALUES (@OwnerUserId)
                SELECT CAST(SCOPE_IDENTITY() as int)",
                userPhonebook).Single();
        }
    }

    internal class UserPhonebookData
    {
        public UserPhonebookData()
        {
            Contacts = new List<ContactData>();
        }

        public int Id { get; set; }

        public int OwnerUserId { get; set; }

        public IEnumerable<ContactData> Contacts { get; set; }
    }

    internal class ContactData
    {
        public int Id { get; set; }

        public int UserPhonebookId { get; set; }

        public string ContactName { get; set; }

        public PhoneNumber ContactPhoneNumber { get; set; }
    }
}