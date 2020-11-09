using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Phonebook.Domain.Model.Entities;
using Phonebook.Domain.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Phonebook.Infrastructure.EntityPersistance.Tests.Integration
{
    public class PhonebookDbContextTests
    {
        [Fact]
        public async Task GivenPhonebookDoesNotExist_WhenGetUserPhonebookIsCalled_ThenNullIsReturned()
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(DataHelper.GetConnectionString());

            using var context = new PhonebookDbContext(optionsBuilder.Options);

            // Act
            var result = await context.GetUserPhonebook(DataHelper.GetRandomInt());

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GivenPhonebookWithNoContactsExists_WhenGetUserPhonebookIsCalled_ThenThePhonebookIsReturned()
        {
            // Arrange
            var ownerUserId = DataHelper.GetRandomInt();
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(DataHelper.GetConnectionString());

            using var context = new PhonebookDbContext(optionsBuilder.Options);

            var existingPhonebook = new UserPhonebookData { OwnerUserId = ownerUserId };

            try
            {
                DataHelper.SaveUserPhonebook(existingPhonebook);

                // Act
                var result = await context.GetUserPhonebook(ownerUserId);

                // Assert
                result.Should().BeEquivalentTo(existingPhonebook);
            }
            finally
            {
                DataHelper.DeleteUserPhonebook(existingPhonebook.Id);
            }
        }

        [Fact]
        public async Task GivenPhonebookWithContactsExists_WhenGetUserPhonebookIsCalled_ThenThePhonebookAndContactsAreReturned()
        {
            // Arrange
            var ownerUserId = DataHelper.GetRandomInt();
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(DataHelper.GetConnectionString());

            using var context = new PhonebookDbContext(optionsBuilder.Options);

            var existingPhonebook = new UserPhonebookData
            {
                OwnerUserId = ownerUserId,
                Contacts = new List<ContactData> 
                {
                    new ContactData { ContactName = DataHelper.GetRandomString(15), ContactPhoneNumber = DataHelper.GetRandomPhoneNumber() },
                    new ContactData { ContactName = DataHelper.GetRandomString(15), ContactPhoneNumber = DataHelper.GetRandomPhoneNumber() },
                    new ContactData { ContactName = DataHelper.GetRandomString(15), ContactPhoneNumber = DataHelper.GetRandomPhoneNumber() }
                }
            };

            try
            {
                DataHelper.SaveUserPhonebook(existingPhonebook);

                // Act
                var result = await context.GetUserPhonebook(ownerUserId);

                // Assert
                result.Should().BeEquivalentTo(existingPhonebook);
            }
            finally
            {
                DataHelper.DeleteUserPhonebook(existingPhonebook.Id);
            }
        }

        [Fact]
        public async Task GivenPhonebooksForDifferentUsersExist_WhenGetUserPhonebookIsCalled_ThenOnlyTheCorrectContactsAreReturned()
        {
            // Arrange
            var ownerUserId = DataHelper.GetRandomInt();
            var differentUserId = DataHelper.GetRandomInt();
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(DataHelper.GetConnectionString());

            using var context = new PhonebookDbContext(optionsBuilder.Options);

            var existingPhonebook = new UserPhonebookData
            {
                OwnerUserId = ownerUserId,
                Contacts = new List<ContactData>
                {
                    new ContactData { ContactName = DataHelper.GetRandomString(15), ContactPhoneNumber = DataHelper.GetRandomPhoneNumber() },
                    new ContactData { ContactName = DataHelper.GetRandomString(15), ContactPhoneNumber = DataHelper.GetRandomPhoneNumber() },
                    new ContactData { ContactName = DataHelper.GetRandomString(15), ContactPhoneNumber = DataHelper.GetRandomPhoneNumber() }
                }
            };

            var differentUserPhonebook = new UserPhonebookData
            {
                OwnerUserId = differentUserId,
                Contacts = new List<ContactData>
                {
                    new ContactData { ContactName = DataHelper.GetRandomString(15), ContactPhoneNumber = DataHelper.GetRandomPhoneNumber() },
                    new ContactData { ContactName = DataHelper.GetRandomString(15), ContactPhoneNumber = DataHelper.GetRandomPhoneNumber() },
                    new ContactData { ContactName = DataHelper.GetRandomString(15), ContactPhoneNumber = DataHelper.GetRandomPhoneNumber() }
                }
            };

            try
            {
                DataHelper.SaveUserPhonebook(existingPhonebook);
                DataHelper.SaveUserPhonebook(differentUserPhonebook);

                // Act
                var result = await context.GetUserPhonebook(ownerUserId);

                // Assert
                result.Should().BeEquivalentTo(existingPhonebook);
            }
            finally
            {
                DataHelper.DeleteUserPhonebook(existingPhonebook.Id);
                DataHelper.DeleteUserPhonebook(differentUserPhonebook.Id);
            }
        }

        [Theory]
        [InlineData(20, 33)] // phone number is too long
        [InlineData(131, 20)] // name is too long
        public async Task GivenPropertiesExceeedColumnLength_WhenSavingChanges_ThenExceptionIsThrown(
           int contactFullNameLength, int contactPhoneNumberLength)
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(DataHelper.GetConnectionString());

            var existingPhonebook = new UserPhonebookData
            {
                OwnerUserId = DataHelper.GetRandomInt()
            };

            try
            {
                DataHelper.SaveUserPhonebook(existingPhonebook);

                // Act
                using var context = new PhonebookDbContext(optionsBuilder.Options);
                var userPhonebook = await context.GetUserPhonebook(existingPhonebook.OwnerUserId);
                userPhonebook.Contacts.Add(new Contact(
                    DataHelper.GetRandomString(contactFullNameLength),
                    new PhoneNumber(DataHelper.GetRandomString(contactPhoneNumberLength))));

                context.Invoking(async x => await x.SaveChangesAsync())
                    .Should().Throw<DbUpdateException>()
                    .WithInnerException<Exception>()
                    .WithMessage("String or binary data would be truncated in table*");

                // Assert
                DataHelper.GetUserPhonebook(existingPhonebook.Id).Should().BeEquivalentTo(existingPhonebook);
            }
            finally
            {
                DataHelper.DeleteUserPhonebook(existingPhonebook.Id);
            }
        }

        [Fact]
        public async Task GivenPhonebookWithNoContactsExists_WhenNewContactIsAddedAndSaveChangesIsCalled_ThenNewContactIsSaved()
        {
            // Arrange
            var ownerUserId = DataHelper.GetRandomInt();
            var newContactName = DataHelper.GetRandomString(15);
            var newContactPhoneNumber = DataHelper.GetRandomPhoneNumber();
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(DataHelper.GetConnectionString());

            var existingPhonebook = new UserPhonebookData
            {
                OwnerUserId = ownerUserId
            };

            try
            {
                DataHelper.SaveUserPhonebook(existingPhonebook);

                // Act
                using var context = new PhonebookDbContext(optionsBuilder.Options);
                var userPhonebook = await context.GetUserPhonebook(existingPhonebook.OwnerUserId);
                userPhonebook.Contacts.Add(new Contact(newContactName, newContactPhoneNumber));
                await context.SaveChangesAsync();

                // Assert
                var actual = DataHelper.GetUserPhonebook(userPhonebook.Id);

                actual.Id.Should().Be(existingPhonebook.Id);
                actual.OwnerUserId.Should().Be(existingPhonebook.OwnerUserId);
                actual.Contacts.Single().Should().BeEquivalentTo(
                    new ContactData { ContactName = newContactName, ContactPhoneNumber = newContactPhoneNumber },
                    options => options.Excluding(x => x.Id));
                actual.Contacts.Single().Id.Should().BeGreaterThan(0);
            }
            finally
            {
                DataHelper.DeleteUserPhonebook(existingPhonebook.Id);
            }
        }

        [Fact]
        public async Task GivenPhonebookWithContactsExists_WhenNewContactIsAddedAndSaveChangesIsCalled_ThenNewContactIsSaved()
        {
            // Arrange
            var ownerUserId = DataHelper.GetRandomInt();
            var newContactName = DataHelper.GetRandomString(15);
            var newContactPhoneNumber = DataHelper.GetRandomPhoneNumber();
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(DataHelper.GetConnectionString());

            var existingPhonebook = new UserPhonebookData
            {
                OwnerUserId = ownerUserId,
                Contacts = new List<ContactData>
                {
                    new ContactData { 
                        ContactName = DataHelper.GetRandomString(20), ContactPhoneNumber = DataHelper.GetRandomPhoneNumber() }
                }
            };

            try
            {
                DataHelper.SaveUserPhonebook(existingPhonebook);

                // Act
                using var context = new PhonebookDbContext(optionsBuilder.Options);
                var userPhonebook = await context.GetUserPhonebook(existingPhonebook.OwnerUserId);
                userPhonebook.Contacts.Add(new Contact(newContactName, newContactPhoneNumber));
                await context.SaveChangesAsync();

                // Assert
                var actual = DataHelper.GetUserPhonebook(userPhonebook.Id);

                actual.Id.Should().Be(existingPhonebook.Id);
                actual.OwnerUserId.Should().Be(existingPhonebook.OwnerUserId);
                actual.Contacts.First().Should().BeEquivalentTo(existingPhonebook.Contacts.Single());
                actual.Contacts.Last().Should().BeEquivalentTo(
                    new ContactData { ContactName = newContactName, ContactPhoneNumber = newContactPhoneNumber },
                    options => options.Excluding(x => x.Id));
                actual.Contacts.Last().Id.Should().BeGreaterThan(0);
            }
            finally
            {
                DataHelper.DeleteUserPhonebook(existingPhonebook.Id);
            }
        }
    }
}
