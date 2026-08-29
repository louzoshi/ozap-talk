using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace SopaTalk.ArchitectureTests;

/// <summary>
/// Enforces the rules in docs/arquitetura.md that a code review would otherwise have to
/// catch by eye. If one of these fails, the fix is the code, not the test.
/// </summary>
public class ModuleBoundaryTests
{
    private static readonly string[] Modules = ["Channels", "Inbox", "Crm", "Chatbot", "AiAgent"];

    private static Assembly Load(string name) => Assembly.Load($"SopaTalk.{name}");

    [Fact]
    public void Domain_does_not_depend_on_EntityFrameworkCore()
    {
        foreach (var module in Modules)
        {
            var result = Types.InAssembly(Load($"{module}.Domain"))
                .ShouldNot()
                .HaveDependencyOn("Microsoft.EntityFrameworkCore")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{module}.Domain must stay free of EF Core: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Fact]
    public void Domain_does_not_depend_on_AspNetCore()
    {
        foreach (var module in Modules)
        {
            var result = Types.InAssembly(Load($"{module}.Domain"))
                .ShouldNot()
                .HaveDependencyOn("Microsoft.AspNetCore")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{module}.Domain must not know about HTTP: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Fact]
    public void A_module_does_not_reference_another_modules_internals()
    {
        foreach (var module in Modules)
        {
            var otherModuleNamespaces = Modules
                .Where(m => m != module)
                .SelectMany(m => new[] { $"SopaTalk.{m}.Domain", $"SopaTalk.{m}.Infrastructure" })
                .ToArray();

            foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Api" })
            {
                var result = Types.InAssembly(Load($"{module}.{layer}"))
                    .ShouldNot()
                    .HaveDependencyOnAny(otherModuleNamespaces)
                    .GetResult();

                result.IsSuccessful.Should().BeTrue(
                    $"SopaTalk.{module}.{layer} reached into another module: {string.Join(", ", result.FailingTypeNames ?? [])}");
            }
        }
    }
}
