using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Phonebook.Api.Tests.Unit.TestFramework;
using Phonebook.Domain.Model.Entities;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Phonebook.Api.Tests.Unit.Endpoints
{
    public class GetUserPhonebookTests
    {
        [Fact]
        public async Task GivenUserIsNotAuthenticated_WhenGetAllIsRequested_ThenUnauthorizedIsReturned()
        {
            // Arrange
            var host = TestSetup.CreateHost();
            var client = host.GetTestClient();
            var mockServices = host.Services.GetRequiredService<MockServices>();

            // Act
            var response = await client.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(client.BaseAddress.ToString(), "phonebook")));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            response.EnsureCorsAllowOriginHeader(client.BaseAddress);
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo(string.Empty);

            mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Never);
            mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Never);
        }

        [Fact]
        public async Task GivenOriginNotInAllowedOriginsList_WhenGetAllIsRequested_ThenStatusIsOkButCorsHeaderIsNotReturned()
        {
            // Arrange
            const string disallowedOrigin = "https://disallowedorigin.com";
            var randomUserId = TestSetup.GetRandomInt();

            var host = TestSetup.CreateHost();
            var client = host.GetTestClient();

            var mockServices = host.Services.GetRequiredService<MockServices>();

            mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(randomUserId))
                .Returns(Task.FromResult(new UserPhonebook(randomUserId)));

            // Act
            var response = await client.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(client.BaseAddress.ToString(), "phonebook"),
                    randomUserId,
                    null,
                    disallowedOrigin));

            // Assert
            response.EnsureSuccessStatusCode();
            response.EnsureCorsAllowOriginHeader((string)null);
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("[]");

            mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Never);
            mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
        }

        [Fact]
        public async Task GivenUserPhonebookDoesNotExist_WhenGetAllIsRequested_ThenBadRequestIsReturned()
        {
            // Arrange
            var randomUserId = TestSetup.GetRandomInt();
            var host = TestSetup.CreateHost();
            var client = host.GetTestClient();

            var mockServices = host.Services.GetRequiredService<MockServices>();
            mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(randomUserId))
                .Returns(Task.FromResult<UserPhonebook>(null));

            // Act
            var response = await client.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(client.BaseAddress.ToString(), "phonebook"), 
                    randomUserId));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.EnsureCorsAllowOriginHeader(client.BaseAddress);

            var jsonContent = JsonSerializer.Deserialize<JsonElement>
                (await response.Content.ReadAsStringAsync());
            jsonContent.GetProperty("status").GetInt32().Should().Be(400);
            jsonContent.GetProperty("title").GetString().Should().Be("Bad Request");
            jsonContent.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
            jsonContent.GetProperty("traceId").GetString().Should().NotBeNullOrEmpty();

            mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Never);
            mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
        }

        [Fact]
        public async Task GivenUserHasPhonebookButNoContacts_WhenGetAllIsRequested_ThenAnEmptyCollectionIsReturned()
        {
            // Arrange
            var randomUserId = TestSetup.GetRandomInt();

            var host = TestSetup.CreateHost();
            var client = host.GetTestClient();

            var mockServices = host.Services.GetRequiredService<MockServices>();

            mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(randomUserId))
                .Returns(Task.FromResult(new UserPhonebook(randomUserId)));

            // Act
            var response = await client.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(client.BaseAddress.ToString(), "phonebook"),
                    randomUserId));

            // Assert
            response.EnsureSuccessStatusCode();
            response.EnsureCorsAllowOriginHeader(client.BaseAddress);
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("[]");

            mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Never);
            mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
        }

        [Fact]
        public async Task GivenUserHasPhonebookWithContacts_WhenGetAllIsRequested_ThenContactsAreReturned()
        {
            // Arrange
            var randomUserId = TestSetup.GetRandomInt();
            var host = TestSetup.CreateHost();
            var client = host.GetTestClient();

            var mockServices = host.Services.GetRequiredService<MockServices>();
            var contact1 = new Contact(TestSetup.GetRandomString(20), TestSetup.GetRandomPhoneNumber())
                .WithIdSetToRandomInteger();
            var contact2 = new Contact(TestSetup.GetRandomString(20), TestSetup.GetRandomPhoneNumber())
                .WithIdSetToRandomInteger();

            var userPhonebook = new UserPhonebook(randomUserId) { Contacts = { contact1, contact2 } };

            mockServices.MockPhonebookDbContext.Setup(x => x.GetUserPhonebook(randomUserId))
                .Returns(Task.FromResult(userPhonebook));

            // Act
            var response = await client.SendAsync(
                TestSetup.CreateHttpRequestMessage(
                    Path.Combine(client.BaseAddress.ToString(), "phonebook"),
                    randomUserId));

            // Assert
            response.EnsureSuccessStatusCode();
            response.EnsureCorsAllowOriginHeader(client.BaseAddress);
            (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo(
                JsonSerializer.Serialize(new[] {
                    new { id = contact1.Id, fullName = contact1.ContactName, phoneNumber = contact1.ContactPhoneNumber.Value },
                    new { id = contact2.Id, fullName = contact2.ContactName, phoneNumber = contact2.ContactPhoneNumber.Value }}));

            mockServices.MockPhonebookDbContext.EnsureSaveChangesCalled(Times.Never);
            mockServices.MockPhonebookDbContext.EnsureDisposeCalled(Times.Once);
        }
    }
}
