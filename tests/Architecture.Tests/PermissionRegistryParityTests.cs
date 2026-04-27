using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace OpenPsa.Architecture.Tests;

/// <summary>
/// Guards parity between permissions registered by modules (IModule.RegisterPermissions)
/// and permissions actually enforced on endpoints via RequirePermission("...").
///
/// A drift in either direction is a real bug:
///  - Registered but never required: dead permission key — UI exposes a toggle that does nothing.
///  - Required but never registered: undiscoverable by the role editor; effectively unassignable.
/// </summary>
public class PermissionRegistryParityTests
{
    private static readonly string[] CrudVerbs = ["list", "view", "create", "update", "delete"];

    [Fact]
    public void Registered_and_required_permissions_should_match()
    {
        var modulesDir = LocateModulesDir();
        var sources = Directory.EnumerateFiles(modulesDir, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Select(File.ReadAllText)
            .ToList();
        var allText = string.Join("\n", sources);

        var registered = new HashSet<string>(StringComparer.Ordinal);

        foreach (Match m in Regex.Matches(allText, @"RegisterPermission\(\s*""([^""]+)"""))
            registered.Add(m.Groups[1].Value);

        foreach (Match m in Regex.Matches(allText,
            @"RegisterCrudPermissions\(\s*""([^""]+)""\s*,\s*""[^""]+""\s*,\s*""[^""]+""(?:\s*,\s*([^)]+))?\)"))
        {
            var prefix = m.Groups[1].Value;
            var verbsArg = m.Groups[2].Success ? m.Groups[2].Value : null;
            foreach (var verb in ResolveVerbs(verbsArg))
                registered.Add($"{prefix}.{verb}");
        }

        var required = new HashSet<string>(
            Regex.Matches(allText, @"RequirePermission\(\s*""([^""]+)""")
                 .Select(m => m.Groups[1].Value),
            StringComparer.Ordinal);

        var requiredButNotRegistered = required.Except(registered).OrderBy(x => x).ToList();
        var registeredButNotRequired = registered.Except(required).OrderBy(x => x).ToList();

        requiredButNotRegistered.Should().BeEmpty(
            "every permission used by an endpoint must be registered so admins can grant it. " +
            "Missing: " + string.Join(", ", requiredButNotRegistered));

        registeredButNotRequired.Should().BeEmpty(
            "every registered permission should gate at least one endpoint. " +
            "Either implement the endpoint or stop registering: " +
            string.Join(", ", registeredButNotRequired));
    }

    private static IEnumerable<string> ResolveVerbs(string? verbsArg)
    {
        if (string.IsNullOrWhiteSpace(verbsArg)) return CrudVerbs;
        // Heuristic: parse "CrudVerbs.X | CrudVerbs.Y" or "CrudVerbs.All & ~CrudVerbs.Z".
        var tokens = Regex.Matches(verbsArg, @"CrudVerbs\.(\w+)")
            .Select(m => m.Groups[1].Value)
            .ToList();
        var hasNot = verbsArg.Contains('~');
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (tokens.Contains("All", StringComparer.OrdinalIgnoreCase)) foreach (var v in CrudVerbs) set.Add(v);
        foreach (var t in tokens)
        {
            if (t.Equals("All", StringComparison.OrdinalIgnoreCase)) continue;
            var verb = t.ToLowerInvariant();
            if (hasNot && set.Contains(verb)) set.Remove(verb);
            else if (!hasNot) set.Add(verb);
        }
        return set;
    }

    private static string LocateModulesDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "Modules");
            if (Directory.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate src/Modules from " + AppContext.BaseDirectory);
    }
}
