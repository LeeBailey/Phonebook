using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Phonebook.Api.Tests.Unit.TestFramework;
using Phonebook.Domain.Model.Entities;
using Phonebook.Domain.Model.ValueObjects;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Phonebook.Api.Tests.Unit.Endpoints
{
    public class PostNewContactTests
    {
        private readonly IHost _host;
        private readonly HttpClient _httpClient;
        private readonly MockServices _mockServices;

        public PostNewContactTests()
        {
            _host = TestSetup.CreateHost();
            _httpClient = _host.GetTestClient();
            _mockServices = _host.Services.GetRequiredService<MockServices>();
        }

        [Fact]
        public async Task GivenUserIsNotAuthenticated_WhenNewContactIsPosted_ThenUnauthorizedIsReturned()
        {
            // Arrange
            var userPhonebook = new UserPhonebook(TestSetup.GetRandomInt()).WithIdSetToRandomInteger();

            _mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(userPhonebook.OwnerUserId))
                .Returns(Task.FromResult(userPhonebook));

            var postData = new Dictionary<string, string>
            {
                { "contactFullName", TestSetup.GetRandomString(20) },
                { "contactPhoneNumber", TestSetup.GetRandomPhoneNumber().ToString() }
            };

            // Act
            var response = await _httpClient.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(_httpClient.BaseAddress.ToString(), "phonebook/contacts"),
                    null,
                    postData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.EnsureCorsAllowOriginHeader(_httpClient.BaseAddress);
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo(string.Empty);

            _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenOriginNotInAllowedOriginsList_WhenNewContactIsPosted_ThenStatusIsOkButCorsHeaderIsNotReturned()
        {
            // Arrange
            const string disallowedOrigin = "https://disallowedorigin.com";
            var newContactName = TestSetup.GetRandomString(10);
            var newContactPhoneNumber = TestSetup.GetRandomPhoneNumber().ToString();

            var userPhonebook = new UserPhonebook(TestSetup.GetRandomInt()).WithIdSetToRandomInteger();

            _mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(userPhonebook.OwnerUserId))
                .Returns(Task.FromResult(userPhonebook));

            _mockServices.MockPhonebookDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    // Assert
                    userPhonebook.Contacts.Should().BeEquivalentTo(new List<Contact>
                    {
                        new Contact(newContactName, new PhoneNumber(newContactPhoneNumber))
                    });
                });

            var postData = new Dictionary<string, string>
            {
                { "contactFullName", newContactName },
                { "contactPhoneNumber", newContactPhoneNumber }
            };

            // Act
            var response = await _httpClient.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(_httpClient.BaseAddress.ToString(), "phonebook/contacts"),
                    userPhonebook.OwnerUserId,
                    postData,
                    disallowedOrigin));

            var content = response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            response.EnsureCorsAllowOriginHeader((string)null);

            _mockServices.MockPhonebookDbContext.Verify(x => x.GetUserPhonebook(userPhonebook.OwnerUserId), Times.Once);
            _mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Once);
            _mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
            _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenUserPhonebookDoesntExist_WhenNewContactIsPosted_ThenBadRequestIsReturned()
        {
            // Arrange
            var userId = TestSetup.GetRandomInt();

            _mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(userId))
                .Returns(Task.FromResult<UserPhonebook>(null));

            var postData = new Dictionary<string, string>
            {
                { "contactFullName", TestSetup.GetRandomString(20) },
                { "contactPhoneNumber", TestSetup.GetRandomPhoneNumber().ToString() }
            };

            // Act
            var response = await _httpClient.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(_httpClient.BaseAddress.ToString(), "phonebook/contacts"),
                    userId,
                    postData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.EnsureCorsAllowOriginHeader(_httpClient.BaseAddress);

            _mockServices.MockPhonebookDbContext.Verify(x => x.GetUserPhonebook(userId), Times.Once);
            _mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
            _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
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
            var userPhonebook = new UserPhonebook(TestSetup.GetRandomInt()).WithIdSetToRandomInteger();

            _mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(userPhonebook.OwnerUserId))
                .Returns(Task.FromResult(userPhonebook));

            var postData = new Dictionary<string, string>
            {
                { "contactFullName", contactFullName },
                { "contactPhoneNumber", contactPhoneNumber }
            };

            // Act
            var response = await _httpClient.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(_httpClient.BaseAddress.ToString(), "phonebook/contacts"),
                    userPhonebook.OwnerUserId,
                    postData));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            await response.EnsureBadRequestContent("One or more validation errors occurred.");
            response.EnsureCorsAllowOriginHeader(_httpClient.BaseAddress);

            _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenUserPhonebookExistsAndParamtersAreValid_WhenNewContactIsPosted_ThenContactIsCreatedAndOkIsReturned()
        {
            // Arrange
            var newContactName = TestSetup.GetRandomString(10);
            var newContactPhoneNumber = TestSetup.GetRandomPhoneNumber().ToString();

            var userPhonebook = new UserPhonebook(TestSetup.GetRandomInt()).WithIdSetToRandomInteger();

            _mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(userPhonebook.OwnerUserId))
                .Returns(Task.FromResult(userPhonebook));

            _mockServices.MockPhonebookDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                    // Assert
                userPhonebook.Contacts.Should().BeEquivalentTo(new []
                {
                    new Contact(newContactName, new PhoneNumber(newContactPhoneNumber))
                });
            });

            var postData = new Dictionary<string, string>
            {
                { "contactFullName", newContactName },
                { "contactPhoneNumber", newContactPhoneNumber }
            };

            // Act
            var response = await _httpClient.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(_httpClient.BaseAddress.ToString(), "phonebook/contacts"),
                    userPhonebook.OwnerUserId,
                    postData));

            // Assert
            response.EnsureSuccessStatusCode();
            response.EnsureCorsAllowOriginHeader(_httpClient.BaseAddress);

            _mockServices.MockPhonebookDbContext.Verify(x => x.GetUserPhonebook(userPhonebook.OwnerUserId), Times.Once);
            _mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Once);
            _mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
            _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenPhonebookWithExistingContacts_WhenNewContactIsPosted_ThenExistingContactsAreNotAffected()
        {
            // Arrange
            var existingContact = new Contact(TestSetup.GetRandomString(20), TestSetup.GetRandomPhoneNumber());
            var newContactName = TestSetup.GetRandomString(20);
            var newContactPhoneNumber = TestSetup.GetRandomPhoneNumber().ToString();

            var userPhonebook = new UserPhonebook(TestSetup.GetRandomInt()).WithIdSetToRandomInteger();
            userPhonebook.Contacts.Add(existingContact);

            _mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(userPhonebook.OwnerUserId))
                .Returns(Task.FromResult(userPhonebook));

            _mockServices.MockPhonebookDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                    // Assert
                userPhonebook.Contacts.Should().BeEquivalentTo(new List<Contact>
                {
                    existingContact,
                    new Contact(newContactName, new PhoneNumber(newContactPhoneNumber))
                });
            });

            var postData = new Dictionary<string, string>
            {
                { "contactFullName", newContactName },
                { "contactPhoneNumber", newContactPhoneNumber }
            };

            // Act
            var response = await _httpClient.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(_httpClient.BaseAddress.ToString(), "phonebook/contacts"),
                    userPhonebook.OwnerUserId,
                    postData));

            // Assert
            response.EnsureSuccessStatusCode();
            response.EnsureCorsAllowOriginHeader(_httpClient.BaseAddress);

            _mockServices.MockPhonebookDbContext.Verify(x => x.GetUserPhonebook(userPhonebook.OwnerUserId), Times.Once);
            _mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Once);
            _mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
            _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenUserPhonebookExistsAndParamtersAreValid_WhenNewContactIsPostedMultipleTimes_ThenContactIsCreatedAndOkIsReturned()
        {
            // Arrange
            var userPhonebooks = new List<UserPhonebook>();
            var newContactName = TestSetup.GetRandomString(10);
            var newContactPhoneNumber = TestSetup.GetRandomPhoneNumber().ToString();

            for (int i = 0; i < 100; i++)
            {
                var userPhonebook = new UserPhonebook(TestSetup.GetRandomInt()).WithIdSetToRandomInteger();
                userPhonebooks.Add(userPhonebook);
                _mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(userPhonebook.OwnerUserId))
                    .Returns(Task.FromResult(userPhonebook));

                _mockServices.MockPhonebookDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    // Assert
                    userPhonebooks[i].Contacts.Should().BeEquivalentTo(new[]
                    {
                        new Contact(newContactName, new PhoneNumber(newContactPhoneNumber))
                    });
                });

                var postData = new Dictionary<string, string>
                {
                    { "contactFullName", newContactName },
                    { "contactPhoneNumber", newContactPhoneNumber }
                };

                // Act
                var response = await _httpClient.SendAsync(
                    TestSetup.CreateHttpRequestMessage(
                        Path.Combine(_httpClient.BaseAddress.ToString(), "phonebook/contacts"),
                        userPhonebooks[i].OwnerUserId,
                        postData));

                // Assert
                response.EnsureSuccessStatusCode();
                response.EnsureCorsAllowOriginHeader(_httpClient.BaseAddress);

                _mockServices.MockPhonebookDbContext.Verify(x => x.GetUserPhonebook(userPhonebooks[i].OwnerUserId), Times.Once);
                _mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(() => Times.Exactly(i + 1));
                _mockServices.MockPhonebookDbContext.EnsureDisposeCalled(() => Times.Exactly(i + 1));
                _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
            }
        }
    }
}
