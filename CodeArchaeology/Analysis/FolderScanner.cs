namespace CodeArchaeology.Analysis;

public static class FolderScanner
{
    public static IReadOnlyList<string> GetCsFiles(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return Array.Empty<string>();

        return Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);
    }
}
