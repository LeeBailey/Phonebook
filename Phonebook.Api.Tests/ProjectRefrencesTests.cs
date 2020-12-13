using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Xunit;

namespace Phonebook.Api.Tests
{
    public class ProjectRefrencesTests
    {
        private const string Api_Tests = @"Phonebook.Api.Tests\Phonebook.Api.Tests.csproj";
        private const string Api = @"Phonebook.Api\Phonebook.Api.csproj";
        private const string Domain_ApplicationServices = 
            @"Phonebook.Domain\Phonebook.Domain.ApplicationServices\Phonebook.Domain.ApplicationServices.csproj";
        private const string Domain_Infrastructure_Abstractions =
            @"Phonebook.Domain\Phonebook.Domain.Infrastructure.Abstractions\Phonebook.Domain.Infrastructure.Abstractions.csproj";
        private const string Domain_Model =
            @"Phonebook.Domain\Phonebook.Domain.Model\Phonebook.Domain.Model.csproj";
        private const string Infrastructure_EntityPersistance = 
            @"Phonebook.Infrastructure\Phonebook.Infrastructure.EntityPersistance\Phonebook.Infrastructure.EntityPersistance.csproj";
        private const string Infrastructure_EntityPersistance_Tests =
            @"Phonebook.Infrastructure\Phonebook.Infrastructure.EntityPersistance.Tests\Phonebook.Infrastructure.EntityPersistance.Tests.csproj";

        [Fact]
        public void PhonebookDomainApplicationServicesProject_OnlyRefrerencesAllowedProjects()
        {
            var actualReferences = GetProjectReferences(Domain_ApplicationServices);

            Assert.Equal(new string[] {
                Domain_Infrastructure_Abstractions,
                Domain_Model
            }, actualReferences);
        }
        
        [Fact]
        public void PhonebookDomainInfrastructureAbstractionsProject_OnlyRefrerencesAllowedProjects()
        {
            var actualReferences = GetProjectReferences(Domain_Infrastructure_Abstractions);

            Assert.Equal(new string[] {
                Domain_Model
            }, actualReferences);
        }

        [Fact]
        public void PhonebookDomainModelProject_OnlyRefrerencesAllowedProjects()
        {
            var actualReferences = GetProjectReferences(Domain_Model);

            Assert.Equal(Array.Empty<string>(), actualReferences);
        }

        [Fact]
        public void PhonebookInfrastructureEntityPersistanceProject_OnlyRefrerencesAllowedProjects()
        {
            var actualReferences = GetProjectReferences(Infrastructure_EntityPersistance);

            Assert.Equal(new string[] {
                Domain_Infrastructure_Abstractions,
            }, actualReferences);
        }

        [Fact]
        public void PhonebookApiProject_OnlyRefrerencesAllowedProjects()
        {
            var actualReferences = GetProjectReferences(Api);

            Assert.Equal(new string[] {
                Domain_ApplicationServices,
                Infrastructure_EntityPersistance
            }, actualReferences);
        }

        [Fact]
        public void PhonebookApiTestsProject_OnlyRefrerencesAllowedProjects()
        {
            var actualReferences = GetProjectReferences(Api_Tests);

            Assert.Equal(new string[] {
                Api,
            }, actualReferences);
        }

        [Fact]
        public void PhonebookInfrastructureEntityPersistanceTestsProject_OnlyRefrerencesAllowedProjects()
        {
            var actualReferences = GetProjectReferences(Infrastructure_EntityPersistance_Tests);

            Assert.Equal(new string[] {
                Infrastructure_EntityPersistance,
            }, actualReferences);
        }

        private static IEnumerable<string> GetProjectReferences(string solutionRelativeProjectFilePath)
        {
            var solutionDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                !.Split("\\Phonebook.Api.Tests").First();

            var projectFilePath = Path.Combine(solutionDirectory, solutionRelativeProjectFilePath);

            var references = XDocument.Load(projectFilePath)
                ?.Element("Project")
                ?.Elements("ItemGroup")
                ?.Elements("ProjectReference")
                ?.Attributes("Include")
                ?.Select(x => 
                    Path.GetFullPath(Path.GetDirectoryName(projectFilePath) + "\\" + x.Value)
                        .Replace(solutionDirectory, string.Empty).TrimStart('\\'));

            return references ?? throw new XmlException("Unable to retrieve references for project");
        }
    }
}
