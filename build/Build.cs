using System;
using System.Diagnostics;
using System.IO;
using BunnyLand.ResourceGenerator;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;


[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    readonly string BunnyLandDesktopGLPath = "BunnyLand.DesktopGL";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository] readonly GitRepository GitRepository;

    [Solution] readonly Solution Solution;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() => {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() => {
            DotNetRestore(_ => _
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() => {
            DotNetBuild(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target StartTwo => _ => _
        .DependsOn(Compile)
        .Executes(() => {
            Process.Start(DotNetPath, "run --no-build --project " + BunnyLandProjectRoot);
            Process.Start(DotNetPath, "run --no-build --project " + BunnyLandProjectRoot);
        });

    Target GenerateResources => _ => _
        .Executes(() => {
            var contentPath = BunnyLandProjectRoot / "Content";
            var resourcesPath = BunnyLandProjectRoot / "Resources";

            void Generate(Action<string, TextWriter> write)
            {
                var className = write.Method.Name.Replace("Write", "");
                var destinationFile = resourcesPath / $"{className}.generated.cs";
                using var textWriter = File.CreateText(destinationFile);
                write(contentPath, textWriter);
            }

            Generate(Generator.WriteTextures);
            Generate(Generator.WriteSoundEffects);
            Generate(Generator.WriteSpriteFonts);
            Generate(Generator.WriteSongs);
        });

    Target Watch => _ => _
        .Executes(() => {
            DotNet("watch run", BunnyLandProjectRoot);
        });

    AbsolutePath BunnyLandProjectRoot => Solution.GetProject(BunnyLandDesktopGLPath)?.Directory;

    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);
}
