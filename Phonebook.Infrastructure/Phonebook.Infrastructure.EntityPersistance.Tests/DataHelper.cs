using Dapper;
using Microsoft.Extensions.Configuration;
using Phonebook.Domain.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

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

        internal static UserPhonebookData? GetUserPhonebook(int userPhonebookId)
        {
            using var connection = new SqlConnection(GetConnectionString());
            var userPhonebook = connection.Query(
                "SELECT * FROM UserPhonebook WHERE Id = @Id",
                new { Id = userPhonebookId })
                .Select(row => new UserPhonebookData() { Id = (int)row.Id, OwnerUserId = (Guid)row.OwnerUserId })
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
                .Select(row => new ContactData((string)row.ContactName, new PhoneNumber((string)row.ContactPhoneNumber))
                {
                    Id = (int)row.Id,
                }).ToList();
        }

        internal static void DeleteUserPhonebook(int userPhonebookId)
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var transactionScope = new TransactionScope();

            connection.Execute(
            "DELETE FROM Contact WHERE UserPhonebookId = @UserPhonebookId",
            new { UserPhonebookId = userPhonebookId });

            connection.Execute(
                "DELETE FROM UserPhonebook WHERE Id = @Id",
                new { Id = userPhonebookId });

            transactionScope.Complete();
        }

        internal static void SaveUserPhonebook(UserPhonebookData userPhonebook)
        {
            using var connection = new SqlConnection(GetConnectionString());
            using var transactionScope = new TransactionScope();

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

            transactionScope.Complete();

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

        public Guid OwnerUserId { get; set; }

        public IEnumerable<ContactData> Contacts { get; set; }
    }

    internal class ContactData
    {
        internal ContactData(string contactName, PhoneNumber phoneNumber)
        {
            ContactName = contactName;
            ContactPhoneNumber = phoneNumber;
        }

        public int Id { get; set; }

        public string ContactName { get; set; }

        public PhoneNumber ContactPhoneNumber { get; set; }
    }
}