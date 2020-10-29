using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Xunit;

namespace Phonebook.Api.Tests
{
    public class ProjectRefrencesTests
    {
        [Fact]
        public void PhonebookDomainApplicationServicesProject_OnlyRefrerencesAllowedProjects()
        {
            var solutionRelativeProjectFilePath = 
                @"Phonebook.Domain\Phonebook.Domain.ApplicationServices\Phonebook.Domain.ApplicationServices.csproj";

            var actualReferences = GetProjectReferences(solutionRelativeProjectFilePath);

            Assert.Equal(new string[] {
                @"..\Phonebook.Domain.Infrastructure.Abstractions\Phonebook.Domain.Infrastructure.Abstractions.csproj",
                @"..\Phonebook.Domain.Model\Phonebook.Domain.Model.csproj"
            }, actualReferences);
        }
        
        [Fact]
        public void PhonebookDomainInfrastructureAbstractionsProject_OnlyRefrerencesAllowedProjects()
        {
            var solutionRelativeProjectFilePath = 
                @"Phonebook.Domain\Phonebook.Domain.Infrastructure.Abstractions\Phonebook.Domain.Infrastructure.Abstractions.csproj";

            var actualReferences = GetProjectReferences(solutionRelativeProjectFilePath);

            Assert.Equal(new string[] {
                @"..\Phonebook.Domain.Model\Phonebook.Domain.Model.csproj"
            }, actualReferences);
        }

        [Fact]
        public void PhonebookDomainModelProject_OnlyRefrerencesAllowedProjects()
        {
            var solutionRelativeProjectFilePath =
                @"Phonebook.Domain\Phonebook.Domain.Model\Phonebook.Domain.Model.csproj";

            var actualReferences = GetProjectReferences(solutionRelativeProjectFilePath);

            Assert.Equal(Array.Empty<string>(), actualReferences);
        }

        [Fact]
        public void PhonebookInfrastructureEntityPersistanceProject_OnlyRefrerencesAllowedProjects()
        {
            var solutionRelativeProjectFilePath =
                @"Phonebook.Infrastructure\Phonebook.Infrastructure.EntityPersistance\Phonebook.Infrastructure.EntityPersistance.csproj";

            var actualReferences = GetProjectReferences(solutionRelativeProjectFilePath);

            Assert.Equal(new string[] {
                @"..\..\Phonebook.Domain\Phonebook.Domain.Infrastructure.Abstractions\Phonebook.Domain.Infrastructure.Abstractions.csproj",
            }, actualReferences);
        }

        [Fact]
        public void PhonebookApiProject_OnlyRefrerencesAllowedProjects()
        {
            var solutionRelativeProjectFilePath = @"Phonebook.Api\Phonebook.Api.csproj";

            var actualReferences = GetProjectReferences(solutionRelativeProjectFilePath);

            Assert.Equal(new string[] {
                @"..\Phonebook.Domain\Phonebook.Domain.ApplicationServices\Phonebook.Domain.ApplicationServices.csproj",
                @"..\Phonebook.Infrastructure\Phonebook.Infrastructure.EntityPersistance\Phonebook.Infrastructure.EntityPersistance.csproj"
            }, actualReferences);
        }

        [Fact]
        public void PhonebookApiTestsUnitProject_OnlyRefrerencesAllowedProjects()
        {
            var solutionRelativeProjectFilePath = @"Phonebook.Api.Tests.Unit\Phonebook.Api.Tests.Unit.csproj";

            var actualReferences = GetProjectReferences(solutionRelativeProjectFilePath);

            Assert.Equal(new string[] {
                @"..\Phonebook.Api\Phonebook.Api.csproj",
            }, actualReferences);
        }

        private static IEnumerable<string> GetProjectReferences(string solutionRelativeProjectFilePath)
        {
            var solutionDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                .Split("\\Phonebook.Api.Tests.Unit").First();

            var projectDefinition = XDocument.Load(Path.Combine(solutionDirectory, solutionRelativeProjectFilePath));

            return projectDefinition
                .Element("Project")
                .Elements("ItemGroup")
                .Elements("ProjectReference")
                .Attributes("Include")
                .Select(x => x.Value);
        }
    }
}
