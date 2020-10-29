using Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance;
using PhoneBook.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Phonebook.Api.Tests.Unit.TestFramework
{
    public class TestServerStartup : Startup
    {
        public TestServerStartup(IConfiguration configuration)
            : base(configuration)
        { }

        protected override void ConfigureInfrastructureServices(IServiceCollection services)
        {
            var mockPhonebookDbContext = new Mock<IPhonebookDbContext>();
            var mockPhonebookDbContextFactory = new Mock<IPhonebookDbContextFactory>();
            mockPhonebookDbContextFactory.Setup(x => x.Create()).Returns(mockPhonebookDbContext.Object);

            services.AddSingleton(x => mockPhonebookDbContextFactory.Object);
            services.AddScoped(x => new MockServices { MockPhonebookDbContext = mockPhonebookDbContext });
        }
    }
}
