namespace CodeArchaeology.Analysis;

public static class FolderScanner
{
    // 빌드 산출물 폴더 — 분석 대상에서 제외
    private static readonly HashSet<string> ExcludedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "obj"
    };

    public static IReadOnlyList<string> GetCsFiles(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return Array.Empty<string>();

        return Directory
            .GetFiles(folderPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !IsExcluded(f))
            .ToList();
    }

    private static bool IsExcluded(string filePath)
    {
        return filePath
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => ExcludedFolders.Contains(segment));
    }
}
