namespace GameFrameX.Core.Tests.System;

internal static class DirectoryInfoExtensions
{
    public static string Combine(this DirectoryInfo directory, string relativePath)
    {
        return Path.Combine(directory.FullName, relativePath);
    }
}