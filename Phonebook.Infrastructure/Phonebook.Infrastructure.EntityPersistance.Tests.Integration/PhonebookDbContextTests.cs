using Phonebook.Domain.Model.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Phonebook.Domain.Model.ValueObjects;
using System;

namespace Phonebook.Infrastructure.EntityPersistance.Tests.Integration
{
    public class PhonebookDbContextTests
    {
        [Fact]
        public async Task GivenPhonebookDoesNotExistsInDatabase_WhenPhonebooksDbSetQueried_ThenNullShouldBeReturned()
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(DataHelper.GetConnectionString());

            using var context = new PhonebookDbContext(optionsBuilder.Options);

            // Act
            var result = await context.UserPhonebooks.Where(x => x.OwnerUserId == 123).SingleOrDefaultAsync();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GivenPhonebookExistsInDatabase_WhenPhonebooksDbSetQueried_ThenThePhonebookShouldBeReturned()
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(DataHelper.GetConnectionString());

            using var context = new PhonebookDbContext(optionsBuilder.Options);

            var existingPhonebook = new UserPhonebookData() { OwnerUserId = 123 };

            DataHelper.SaveUserPhonebook(existingPhonebook);

            try
            {
                // Act
                var result = await context.UserPhonebooks.Where(x => x.OwnerUserId == 123).SingleAsync();

                // Assert
                result.Should().BeEquivalentTo(existingPhonebook);
            }
            finally
            {
                DataHelper.DeletePhonebook(existingPhonebook.Id);
            }
        }

        [Fact]
        public async Task GivenValidUserPhonebookWithNoContacts_WhenAddedToDbSetAndSaved_ThenTheUserPhonebookIsSaved()
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(DataHelper.GetConnectionString());

            using var context = new PhonebookDbContext(optionsBuilder.Options);

            var userPhonebook = new UserPhonebook(456);

            try
            {
                // Act
                await context.UserPhonebooks.AddAsync(userPhonebook);
                await context.SaveChangesAsync();

                // Assert
                userPhonebook.Id.Should().NotBe(0);
                DataHelper.GetUserPhonebook(userPhonebook.Id).Should().BeEquivalentTo(userPhonebook);
            }
            finally
            {
                DataHelper.DeletePhonebook(userPhonebook.Id);
            }
        }

        [Fact]
        public async Task GivenValidUserPhonebookWithContacts_WhenAddedToDbSetAndSaved_ThenTheUserPhonebookIsSaved()
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(DataHelper.GetConnectionString());

            using var context = new PhonebookDbContext(optionsBuilder.Options);

            var userPhonebook = new UserPhonebook(456);
            userPhonebook.Contacts.Add(new Contact("Michael", new PhoneNumber("01234545678")));

            try
            {
                // Act
                await context.UserPhonebooks.AddAsync(userPhonebook);
                await context.SaveChangesAsync();

                // Assert
                userPhonebook.Id.Should().NotBe(0);
                DataHelper.GetUserPhonebook(userPhonebook.Id).Should().BeEquivalentTo(userPhonebook);
            }
            finally
            {
                DataHelper.DeletePhonebookContacts(userPhonebook.Id);
                DataHelper.DeletePhonebook(userPhonebook.Id);
            }
        }

        [Theory]
        [InlineData("Steve Jones", "012345678901234556677890124235233")] // phone number is too long
        [InlineData("Steve Jones Steve Jones Steve Jones Steve Jones Steve Jones " +
            "Steve Jones Steve Jones Steve Jones Steve Jones Steve Jones Steve Jones", "012345678901233")] // name is too long
        public async Task GivenPropertiesExceeedColumnLength_WhenSavingChanges_ThenExceptionIsThrown(
            string contactFullName, string contactPhoneNumber)
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder<PhonebookDbContext>();
            optionsBuilder.UseSqlServer(DataHelper.GetConnectionString());

            using var context = new PhonebookDbContext(optionsBuilder.Options);

            var userPhonebook = new UserPhonebook(456);
            userPhonebook.Contacts.Add(new Contact(contactFullName, new PhoneNumber(contactPhoneNumber)));

            // Act
            await context.UserPhonebooks.AddAsync(userPhonebook);

            // Assert
            context.Invoking(async x => await x.SaveChangesAsync())
                .Should().Throw<DbUpdateException>()
                .WithInnerException<Exception>()
                .WithMessage("String or binary data would be truncated in table*");

            userPhonebook.Id.Should().Be(0);
            DataHelper.GetUserPhonebook(userPhonebook.Id).Should().BeNull();
        }
    }
}
