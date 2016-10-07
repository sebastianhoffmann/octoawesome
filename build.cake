/// <summary>
///     Just a simple build script.
/// </summary>

// *********************
//      ARGUMENTS
// *********************
var Target = Argument("target", "default");
var Configuration = Argument("configuration", "release");

// *********************
//      VARIABLES
// *********************
var Solution = File("OctoAwesome/OctoAwesome.sln");

var BuildVerbosity = Verbosity.Minimal;

// *********************
//      TASKS
// *********************

/// <summary>
///     Task to build the solution. Using MSBuild on Windows and MDToolBuild on OSX / Linux
/// </summary>
Task("build")
    .Does(() =>
    {
        DotNetBuild(Solution, cfg =>
        {
            cfg.Configuration = Configuration;
            cfg.Verbosity = BuildVerbosity;
        });
    });

/// <summary>
///     Task to clean all obj and bin directories as well as the ./output folder.
///     Commonly called right before build.
/// </summary>
Task("clean")
    .Does(() =>
    {
        CleanDirectories("./output");
        CleanDirectories("./bin");
        CleanDirectories(string.Format("./src/**/obj/{0}", Configuration));
    });

/// <summary>
///     The default task with a predefined flow.
/// </summary>
Task("default")
    .IsDependentOn("clean")
    .IsDependentOn("restore")
    .IsDependentOn("build");

/// <summary>
///     Task to rebuild. Nothing else than a clean followed by build.
/// </summary>
Task("rebuild")
    .IsDependentOn("clean")
    .IsDependentOn("build");

/// <summary>
///     Task to restore NuGet packages on solution level for all containing projects.
/// </summary>
Task("restore")
    .Does(() => NuGetRestore(Solution));

// Execution
RunTarget(Target);
