// See https://aka.ms/new-console-template for more information
using Microsoft.CodeAnalysis;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

Console.WriteLine("Hello, World!");

MSBuildLocator.RegisterDefaults();

// Load the solution
string solutionPath = @"---solution path";

using var workspace = MSBuildWorkspace.Create();
workspace.WorkspaceFailed += (sender, e) =>
{
    Console.WriteLine($"Workspace failed: {e.Diagnostic.Message}");
};

var solution = await workspace.OpenSolutionAsync(solutionPath);

// Iterate through all projects in the solution
foreach (var project in solution.Projects)
{
    Console.WriteLine($"Compiling project: {project.Name}");

    // Compile the project
    var compilation = await project.GetCompilationAsync();

    if (compilation == null)
        throw new Exception();

    var outputPath = Path.Combine(project.OutputFilePath ?? string.Empty);

    // Emit the compiled assembly
    var result = compilation.Emit(outputPath);

    if (result == null)
        throw new Exception();

    if (result.Success)
    {
        Console.WriteLine($"Compilation successful: {outputPath}");

        var usedReferences = compilation.GetUsedAssemblyReferences();
        var allReferences = compilation.ReferencedAssemblyNames;

        var usedReferenceNames = usedReferences.Select(m => m.Display?.ToLower()).ToList();

        var allReferenceNames = allReferences.Select(m => m.Name.ToLower()).ToList();

        var missingItems = allReferenceNames
            .Where(super => !usedReferenceNames.Any(sub => super.EndsWith(sub ?? string.Empty) || ($"{super}.dll").EndsWith(sub ?? "")))

            .ToList();
    }
    else
    {
        Console.WriteLine($"Compilation failed for project: {project.Name}");
        foreach (var diagnostic in result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
        {
            Console.WriteLine(diagnostic.GetMessage());
        }
    }
}

Console.ReadLine();
