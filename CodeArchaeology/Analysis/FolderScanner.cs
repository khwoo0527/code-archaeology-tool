namespace CodeArchaeology.Analysis;

/// <summary>
/// C# 소스 파일 수집기 — <see cref="IFolderScanner"/> 구현체.
/// 재귀적으로 폴더를 탐색하며 bin / obj 빌드 산출물 폴더를 자동 제외한다.
/// </summary>
public class FolderScanner : IFolderScanner
{
    // 빌드 산출물 폴더 — 분석 대상에서 제외
    private static readonly HashSet<string> ExcludedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin", "obj"
    };

    /// <inheritdoc/>
    public IReadOnlyList<string> GetCsFiles(string folderPath)
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
