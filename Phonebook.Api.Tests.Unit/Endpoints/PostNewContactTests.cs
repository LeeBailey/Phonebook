using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Phonebook.Api.Tests.Unit.TestFramework;
using Phonebook.Domain.Model.Entities;
using Phonebook.Domain.Model.ValueObjects;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Phonebook.Api.Tests.Unit.Endpoints
{
    public class PostNewContactTests
    {
        [Fact]
        public async Task GivenUserIsNotAuthenticated_WhenNewContactIsPosted_ThenUnauthorizedIsReturned()
        {
            // Arrange
            var userId = TestSetup.GetRandomInt();
            var userPhonebookId = TestSetup.GetRandomInt();
            var host = TestSetup.CreateHost();
            var client = host.GetTestClient();

            var userPhonebook = new UserPhonebook(userId)
                .WithPrivatePropertSet(nameof(UserPhonebook.Id), userPhonebookId);

            var mockServices = host.Services.GetRequiredService<MockServices>();
            var mockUserPhonebooksDbSet = new List<UserPhonebook> { userPhonebook }.AsMockDbSet();
            mockServices.MockPhonebookDbContext.Setup(x => x.UserPhonebooks)
                .Returns(mockUserPhonebooksDbSet.Object);

            var postData = new Dictionary<string, string>
            {
                { "contactFullName", TestSetup.GetRandomString(20) },
                { "contactPhoneNumber", TestSetup.GetRandomPhoneNumber().ToString() }
            };

            // Act
            var response = await client.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(client.BaseAddress.ToString(), "phonebook/contacts"),
                    null,
                    postData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.EnsureCorsAllowOriginHeader(client.BaseAddress);
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo(string.Empty);

            mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Never);
            mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Never);
        }

        [Fact]
        public async Task GivenOriginNotInAllowedOriginsList_WhenNewContactIsPosted_ThenStatusIsOkButCorsHeaderIsNotReturned()
        {
            // Arrange
            const string disallowedOrigin = "https://disallowedorigin.com";
            var userId = TestSetup.GetRandomInt();
            var newContactName = TestSetup.GetRandomPhoneNumber().ToString();
            var newContactPhoneNumber = TestSetup.GetRandomPhoneNumber().ToString();
            var host = TestSetup.CreateHost();
            var client = host.GetTestClient();

            var userPhonebook = new UserPhonebook(userId).WithIdSetToRandomInteger();

            var mockServices = host.Services.GetRequiredService<MockServices>();
            var mockUserPhonebooksDbSet = new List<UserPhonebook> { userPhonebook }.AsMockDbSet();
            mockServices.MockPhonebookDbContext.Setup(x => x.UserPhonebooks)
                .Returns(mockUserPhonebooksDbSet.Object);

            var postData = new Dictionary<string, string>
            {
                { "contactFullName", newContactName },
                { "contactPhoneNumber", newContactPhoneNumber }
            };

            // Act
            var response = await client.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(client.BaseAddress.ToString(), "phonebook/contacts"),
                    userPhonebook.Id,
                    postData,
                    disallowedOrigin));

            // Assert
            response.EnsureSuccessStatusCode();
            response.EnsureCorsAllowOriginHeader((string)null);

            var updatedContacts = mockServices.MockPhonebookDbContext.Object.UserPhonebooks
                .Single(x => x.Id == userPhonebook.Id).Contacts;
            updatedContacts.Should().BeEquivalentTo(new List<Contact>
            {
                new Contact(newContactName, new PhoneNumber(newContactPhoneNumber))
            });

            mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Once);
            mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
        }

        [Fact]
        public async Task GivenUserPhonebookDoesntExist_WhenNewContactIsPosted_ThenBadRequestIsReturned()
        {
            // Arrange
            var userId = TestSetup.GetRandomInt();
            var host = TestSetup.CreateHost();
            var client = host.GetTestClient();

            var mockServices = host.Services.GetRequiredService<MockServices>();

            var mockUserPhonebooksDbSet = new List<UserPhonebook>().AsMockDbSet();
            mockServices.MockPhonebookDbContext.Setup(x => x.UserPhonebooks)
                .Returns(mockUserPhonebooksDbSet.Object);

            var postData = new Dictionary<string, string>
            {
                { "contactFullName", TestSetup.GetRandomString(20) },
                { "contactPhoneNumber", TestSetup.GetRandomPhoneNumber().ToString() }
            };

            // Act
            var response = await client.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(client.BaseAddress.ToString(), "phonebook/contacts"),
                    userId,
                    postData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.EnsureCorsAllowOriginHeader(client.BaseAddress);

            mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Never);
            mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
        }

        public class InvalidPostParameters : IEnumerable<object[]>
        {
            private readonly List<object[]> _data = new List<object[]>
            {
                new object[] { string.Empty, string.Empty },
                new object[] { null, null },
                new object[] { null, TestSetup.GetRandomPhoneNumber().ToString() },
                new object[] { string.Empty, TestSetup.GetRandomPhoneNumber().ToString() },
                new object[] { TestSetup.GetRandomString(20), null },
                new object[] { TestSetup.GetRandomString(20), string.Empty },
                // phone number too long:
                new object[] { TestSetup.GetRandomString(20), TestSetup.GetRandomString(33) },
                // name too long:
                new object[] { TestSetup.GetRandomString(129), TestSetup.GetRandomString(15) },
            };

            public IEnumerator<object[]> GetEnumerator()
                { return _data.GetEnumerator(); }

            IEnumerator IEnumerable.GetEnumerator()
                { return GetEnumerator(); }
        }

        [Theory]
        [ClassData(typeof(InvalidPostParameters))]
        public async Task GivenUserPhonebookExistsButParamtersAreNotValid_WhenNewContactIsPosted_ThenBadRequestIsReturned(
            string contactFullName, string contactPhoneNumber)
        {
            // Arrange
            var userId = TestSetup.GetRandomInt();
            var host = TestSetup.CreateHost();
            var client = host.GetTestClient();

            var mockServices = host.Services.GetRequiredService<MockServices>();

            var userPhonebook = new UserPhonebook(userId).WithIdSetToRandomInteger();

            var mockUserPhonebooksDbSet = new List<UserPhonebook> { userPhonebook }.AsMockDbSet();
            mockServices.MockPhonebookDbContext.Setup(x => x.UserPhonebooks)
                .Returns(mockUserPhonebooksDbSet.Object);

            var postData = new Dictionary<string, string>
            {
                { "contactFullName", contactFullName },
                { "contactPhoneNumber", contactPhoneNumber }
            };

            // Act
            var response = await client.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(client.BaseAddress.ToString(), "phonebook/contacts"),
                    userPhonebook.Id,
                    postData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.EnsureCorsAllowOriginHeader(client.BaseAddress);

            var jsonContent = JsonSerializer.Deserialize<JsonElement>
                (await response.Content.ReadAsStringAsync());
            jsonContent.GetProperty("status").GetInt32().Should().Be(400);
            jsonContent.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
            jsonContent.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
            jsonContent.GetProperty("traceId").GetString().Should().NotBeNullOrEmpty();

            mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Never);
            mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Never);
        }

        [Fact]
        public async Task GivenUserPhonebookExistsAndParamtersAreValid_WhenNewContactIsPosted_ThenContactIsCreatedAndOkIsReturned()
        {
            // Arrange
            var userId = TestSetup.GetRandomInt();
            var newContactName = TestSetup.GetRandomPhoneNumber().ToString();
            var newContactPhoneNumber = TestSetup.GetRandomPhoneNumber().ToString();
            var host = TestSetup.CreateHost();
            var client = host.GetTestClient();

            var userPhonebook = new UserPhonebook(userId).WithIdSetToRandomInteger();

            var mockServices = host.Services.GetRequiredService<MockServices>();
            var mockUserPhonebooksDbSet = new List<UserPhonebook> { userPhonebook }.AsMockDbSet();
            mockServices.MockPhonebookDbContext.Setup(x => x.UserPhonebooks)
                .Returns(mockUserPhonebooksDbSet.Object);

            var postData = new Dictionary<string, string>
            {
                { "contactFullName", newContactName },
                { "contactPhoneNumber", newContactPhoneNumber }
            };

            // Act
            var response = await client.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(client.BaseAddress.ToString(), "phonebook/contacts"),
                    userPhonebook.Id,
                    postData));

            // Assert
            response.EnsureSuccessStatusCode();
            response.EnsureCorsAllowOriginHeader(client.BaseAddress);

            var updatedContacts = mockServices.MockPhonebookDbContext.Object.UserPhonebooks
                .Single(x => x.Id == userPhonebook.Id).Contacts;
            updatedContacts.Should().BeEquivalentTo(new List<Contact>
            {
                new Contact(newContactName, new PhoneNumber(newContactPhoneNumber))
            });

            mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Once);
            mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
        }

        [Fact]
        public async Task GivenPhonebookWithExistingContacts_WhenNewContactIsPosted_ThenContactIsCreatedAndOkIsReturned()
        {
            // Arrange
            var userId = TestSetup.GetRandomInt();
            var newContactName = TestSetup.GetRandomString(20);
            var newContactPhoneNumber = TestSetup.GetRandomPhoneNumber().ToString();
            var host = TestSetup.CreateHost();
            var client = host.GetTestClient();

            var userPhonebook = new UserPhonebook(userId).WithIdSetToRandomInteger();

            var mockServices = host.Services.GetRequiredService<MockServices>();
            var mockUserPhonebooksDbSet = new List<UserPhonebook> { userPhonebook }.AsMockDbSet();
            mockServices.MockPhonebookDbContext.Setup(x => x.UserPhonebooks)
                .Returns(mockUserPhonebooksDbSet.Object);

            var postData = new Dictionary<string, string>
            {
                { "contactFullName", newContactName },
                { "contactPhoneNumber", newContactPhoneNumber }
            };

            // Act
            var response = await client.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(client.BaseAddress.ToString(), "phonebook/contacts"), 
                    userPhonebook.Id, 
                    postData));

            // Assert
            response.EnsureSuccessStatusCode();
            response.EnsureCorsAllowOriginHeader(client.BaseAddress);

            var updatedContacts = mockServices.MockPhonebookDbContext.Object.UserPhonebooks
                .Single(x => x.Id == userPhonebook.Id).Contacts;
            updatedContacts.Should().BeEquivalentTo(new List<Contact>
            {
                new Contact(newContactName, new PhoneNumber(newContactPhoneNumber))
            });

            mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Once);
            mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
        }
    }
}
