using Dapper;
using Microsoft.Extensions.Configuration;
using Phonebook.Domain.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Phonebook.Infrastructure.EntityPersistance.Tests.Integration
{
    internal static class DataHelper
    {
        private static readonly Random _random = new Random();

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
                    ContactName = (string)row.ContactName,
                    ContactPhoneNumber = new PhoneNumber((string)row.ContactPhoneNumber)
                }).ToList();
        }

        internal static void DeleteUserPhonebook(int userPhonebookId)
        {
            using var connection = new SqlConnection(GetConnectionString());

            connection.Execute(
                "DELETE FROM Contact WHERE UserPhonebookId = @UserPhonebookId",
                new { UserPhonebookId = userPhonebookId });

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

            foreach (var contact in userPhonebook.Contacts)
            {
                contact.Id = connection.Query<int>(
                    @"INSERT INTO Contact (UserPhonebookId, ContactName, ContactPhoneNumber) 
                        VALUES (@UserPhonebookId, @ContactName, @ContactPhoneNumber)
                    SELECT CAST(SCOPE_IDENTITY() as int)",
                    new { 
                        UserPhonebookId = userPhonebook.Id,
                        contact.ContactName,
                        ContactPhoneNumber = contact.ContactPhoneNumber.ToString()
                }).Single();
            }
        }

        internal static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        internal static int GetRandomInt()
        {
            return _random.Next();
        }

        internal static PhoneNumber GetRandomPhoneNumber()
        {
            const string chars = "0123456789 ";

            return new PhoneNumber(
                new string(Enumerable.Repeat(chars, 10)
                    .Select(s => s[_random.Next(s.Length)]).ToArray()));
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

        public string ContactName { get; set; }

        public PhoneNumber ContactPhoneNumber { get; set; }
    }
}