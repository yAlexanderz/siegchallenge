using NetArchTest.Rules;
using NUnit.Framework;
using FluentAssertions;

namespace FiscalDocuments.ArchitectureTests;

[TestFixture]
public class ArchitectureTests
{
    private const string DomainNamespace = "FiscalDocuments.Domain";
    private const string ApplicationNamespace = "FiscalDocuments.Application";
    private const string InfrastructureNamespace = "FiscalDocuments.Infrastructure";
    private const string ApiNamespace = "FiscalDocuments.API";

    [Test]
    public void Domain_ShouldNotHaveDependencyOnOtherLayers()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(FiscalDocuments.Domain.Entities.FiscalDocument).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("Domain layer should not depend on other layers");
    }

    [Test]
    public void Application_ShouldNotHaveDependencyOnInfrastructureOrApi()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(FiscalDocuments.Application.Commands.UploadDocumentCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("Application layer should not depend on Infrastructure or API layers");
    }

    [Test]
    public void Infrastructure_ShouldNotHaveDependencyOnApi()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(FiscalDocuments.Infrastructure.Persistence.FiscalDocumentsDbContext).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("Infrastructure layer should not depend on API layer");
    }

    [Test]
    public void Controllers_ShouldHaveSuffixController()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(FiscalDocuments.API.Controllers.FiscalDocumentsController).Assembly)
            .That()
            .ResideInNamespace("FiscalDocuments.API.Controllers")
            .Should()
            .HaveNameEndingWith("Controller")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("All controllers should end with 'Controller'");
    }

    [Test]
    public void Handlers_ShouldHaveSuffixHandler()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(FiscalDocuments.Application.Handlers.UploadDocumentCommandHandler).Assembly)
            .That()
            .ResideInNamespace("FiscalDocuments.Application.Handlers")
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("All handlers should end with 'Handler'");
    }

    [Test]
    public void Repositories_ShouldImplementIRepository()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(FiscalDocuments.Infrastructure.Repositories.FiscalDocumentRepository).Assembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .ImplementInterface(typeof(FiscalDocuments.Domain.Interfaces.IFiscalDocumentRepository))
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("All repositories should implement IRepository interface");
    }

    [Test]
    public void Entities_ShouldBeInDomainEntitiesNamespace()
    {
        // Arrange & Act
        var result = Types.InAssembly(typeof(FiscalDocuments.Domain.Entities.FiscalDocument).Assembly)
            .That()
            .Inherit(typeof(FiscalDocuments.Domain.Common.BaseEntity))
            .Should()
            .ResideInNamespace("FiscalDocuments.Domain.Entities")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("All entities should be in Domain.Entities namespace");
    }
}