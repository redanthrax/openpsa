using System.Linq;
using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace OpenPsa.Architecture.Tests;

/// <summary>
/// Architecture invariants for the modular monolith.
/// Each module owns its own data + endpoints; modules must not depend on each other directly —
/// cross-module communication goes through Contracts or IntegrationEvents.
/// </summary>
public class ModuleBoundaryTests
{
    private static readonly string[] ModuleAssemblyNames =
    [
        "OpenPsa.Modules.Authentication",
        "OpenPsa.Modules.Tickets",
        "OpenPsa.Modules.Clients",
        "OpenPsa.Modules.Contacts",
        "OpenPsa.Modules.Projects",
        "OpenPsa.Modules.TimeEntries",
        "OpenPsa.Modules.Invoicing",
        "OpenPsa.Modules.Notes",
        "OpenPsa.Modules.Security",
        "OpenPsa.Modules.Settings",
        "OpenPsa.Modules.Dashboard",
        "OpenPsa.Modules.Agreements",
    ];

    [Fact]
    public void Modules_should_not_reference_other_modules_directly()
    {
        foreach (var moduleName in ModuleAssemblyNames)
        {
            var asm = TryLoad(moduleName);
            if (asm is null) continue;

            var otherModulePrefixes = ModuleAssemblyNames
                .Where(n => n != moduleName)
                .ToArray();

            var result = Types.InAssembly(asm)
                .ShouldNot()
                .HaveDependencyOnAny(otherModulePrefixes)
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{moduleName} should not depend on other modules. Offenders: " +
                string.Join(", ", result.FailingTypeNames ?? []));
        }
    }

    [Fact]
    public void Modules_should_not_reference_Api_or_Web()
    {
        foreach (var moduleName in ModuleAssemblyNames)
        {
            var asm = TryLoad(moduleName);
            if (asm is null) continue;

            var result = Types.InAssembly(asm)
                .ShouldNot()
                .HaveDependencyOnAny("OpenPsa.Api", "OpenPsa.Web")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"{moduleName} should not depend on Api or Web. Offenders: " +
                string.Join(", ", result.FailingTypeNames ?? []));
        }
    }

    [Fact]
    public void Endpoints_should_be_sealed()
    {
        foreach (var moduleName in ModuleAssemblyNames)
        {
            var asm = TryLoad(moduleName);
            if (asm is null) continue;

            var result = Types.InAssembly(asm)
                .That()
                .HaveNameEndingWith("Endpoint")
                .And().AreClasses()
                .And().AreNotAbstract()
                .Should()
                .BeSealed()
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"Endpoint classes in {moduleName} should be sealed. Offenders: " +
                string.Join(", ", result.FailingTypeNames ?? []));
        }
    }

    private static Assembly? TryLoad(string name)
    {
        try { return Assembly.Load(name); }
        catch { return null; }
    }
}
