namespace ClippyDo.Tests.Architecture;

internal static class SourceTree
{
    /// <summary>
    /// Returns the absolute path to the repository root by walking up until the solution file is found.
    /// Assumes the tests run from bin/{cfg}/{tfm}.
    /// </summary>
    public static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (dir.GetFiles("*.sln").Any())
                return dir.FullName;
            dir = dir.Parent!;
        }
        throw new DirectoryNotFoundException("Could not locate solution root (*.sln not found).");
    }

    public static string ProjectPath(string projectFolderName)
        => Path.Combine(RepoRoot(), projectFolderName);

    public static string[] CsFiles(string projectFolderName)
    {
        var root = ProjectPath(projectFolderName);
        return Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
            // ignore generated/test binaries folders if any slipped in
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                        !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToArray();
    }

    public static string Read(string path) => File.ReadAllText(path);
}