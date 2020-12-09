using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Phonebook.Api.Tests.Unit.TestFramework;
using Phonebook.Domain.Model.Entities;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Phonebook.Api.Tests.Unit.Endpoints
{
    public class GetUserPhonebookTests
    {
        private readonly IHost _host;
        private readonly HttpClient _httpClient;
        private readonly MockServices _mockServices;
        private readonly string _requestUri;
        private readonly string _httpClientBaseAddress;

        public GetUserPhonebookTests()
        {
            _host = TestSetup.CreateHost();
            _httpClient = _host.GetTestClient();
            _mockServices = _host.Services.GetRequiredService<MockServices>();
            _httpClientBaseAddress = _httpClient.BaseAddress?.ToString() 
                ?? throw new NullReferenceException(nameof(_httpClient.BaseAddress));
            _requestUri = Path.Combine(_httpClientBaseAddress, "phonebook");
        }

        [Fact]
        public async Task GivenUserIsNotAuthenticated_WhenGetAllIsRequested_ThenUnauthorizedIsReturned()
        {
            // Act
            var response = await _httpClient.SendAsync(
                TestSetup.CreateHttpRequestMessage(_requestUri));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.EnsureCorsAllowOriginHeader(_httpClientBaseAddress);
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo(string.Empty);

            _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenOriginNotInAllowedOriginsList_WhenGetAllIsRequested_ThenStatusIsOkButCorsHeaderIsNotReturned()
        {
            // Arrange
            const string disallowedOrigin = "https://disallowedorigin.com";
            var randomUserId = Guid.NewGuid();

            _mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(randomUserId))
                .Returns(Task.FromResult<UserPhonebook?>(new UserPhonebook(randomUserId)));

            // Act
            var response = await _httpClient.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    _requestUri,
                    randomUserId,
                    null,
                    disallowedOrigin));

            // Assert
            response.EnsureSuccessStatusCode();
            response.EnsureCorsAllowOriginHeader((string?)null);
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("[]");

            _mockServices.MockPhonebookDbContext.Verify(x => x.GetUserPhonebook(randomUserId), Times.Once);
            _mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
            _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenUserPhonebookDoesNotExist_WhenGetAllIsRequested_ThenBadRequestIsReturned()
        {
            // Arrange
            var randomUserId = Guid.NewGuid();

            _mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(randomUserId))
                .Returns(Task.FromResult<UserPhonebook?>(null));

            // Act
            var response = await _httpClient.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    _requestUri, 
                    randomUserId));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            await response.EnsureBadRequestContent("Bad Request");
            response.EnsureCorsAllowOriginHeader(_httpClientBaseAddress);

            _mockServices.MockPhonebookDbContext.Verify(x => x.GetUserPhonebook(randomUserId), Times.Once);
            _mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
            _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenUserHasPhonebookButNoContacts_WhenGetAllIsRequested_ThenAnEmptyCollectionIsReturned()
        {
            // Arrange
            var randomUserId = Guid.NewGuid();

            _mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(randomUserId))
                .Returns(Task.FromResult<UserPhonebook?>(new UserPhonebook(randomUserId)));

            // Act
            var response = await _httpClient.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    _requestUri,
                    randomUserId));

            // Assert
            response.EnsureSuccessStatusCode();
            response.EnsureCorsAllowOriginHeader(_httpClientBaseAddress);
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("[]");

            _mockServices.MockPhonebookDbContext.Verify(x => x.GetUserPhonebook(randomUserId), Times.Once);
            _mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
            _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(1000)]
        public async Task GivenUserHasPhonebookWithContacts_WhenGetAllIsRequested_ThenContactsAreReturned(int noOfContacts)
        {
            // Arrange
            var randomUserId = Guid.NewGuid();

            var userPhonebook = new UserPhonebook(randomUserId);
            for (int i = 0; i < noOfContacts; i++)
            {
                userPhonebook.Contacts.Add(new Contact(TestSetup.GetRandomString(20), TestSetup.GetRandomPhoneNumber())
                    .WithIdSetToRandomInteger());
            }

            _mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(randomUserId))
                .Returns(Task.FromResult<UserPhonebook?>(userPhonebook));

            // Act
            var response = await _httpClient.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    _requestUri,
                    randomUserId));

            // Assert
            response.EnsureSuccessStatusCode();
            response.EnsureCorsAllowOriginHeader(_httpClientBaseAddress);
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo(
                JsonSerializer.Serialize(userPhonebook.Contacts.Select(x =>
                    new { id = x.Id, fullName = x.ContactName, phoneNumber = x.ContactPhoneNumber.Value })
            ));

            _mockServices.MockPhonebookDbContext.Verify(x => x.GetUserPhonebook(randomUserId), Times.Once);
            _mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
           _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GivenMultipleUserPhonebooksWithContacts_WhenGetAllIsRequestedMultipleTimes_ThenContactsAreReturnedCorrectly()
        {
            // Arrange
            var multiplier = 50;

            for (int i = 0; i < multiplier; i++)
            {
                var randomUserId = Guid.NewGuid();
                var contact1 = new Contact(TestSetup.GetRandomString(20), TestSetup.GetRandomPhoneNumber())
                    .WithIdSetToRandomInteger();
                var contact2 = new Contact(TestSetup.GetRandomString(20), TestSetup.GetRandomPhoneNumber())
                    .WithIdSetToRandomInteger();

                var userPhonebook = new UserPhonebook(randomUserId) { Contacts = { contact1, contact2 } };

                _mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(randomUserId))
                    .Returns(Task.FromResult<UserPhonebook?>(userPhonebook));

                // Act
                var response = await _httpClient.SendAsync(
                    TestSetup.CreateHttpRequestMessage(
                        _requestUri,
                        randomUserId));

                // Assert
                response.EnsureSuccessStatusCode();
                response.EnsureCorsAllowOriginHeader(_httpClientBaseAddress);
                (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo(
                    JsonSerializer.Serialize(new[] {
                        new 
                        { 
                            id = contact1.Id,
                            fullName = contact1.ContactName,
                            phoneNumber = contact1.ContactPhoneNumber.Value
                        },
                        new 
                        { 
                            id = contact2.Id,
                            fullName = contact2.ContactName,
                            phoneNumber = contact2.ContactPhoneNumber.Value 
                        }
                    }));

                _mockServices.MockPhonebookDbContext.Verify(x => x.GetUserPhonebook(randomUserId), Times.Once);
                _mockServices.MockPhonebookDbContext.EnsureDisposeCalled(() => Times.Exactly(i + 1));
                _mockServices.MockPhonebookDbContext.VerifyNoOtherCalls();
            }
        }
    }
}
