﻿namespace Craftsman.Removers
{
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;

    public static class IRepositoryRemover
    {
        public static void Remove(string solutionDirectory, string filename, string entityName)
        {
            var classPath = ClassPathHelper.IRepositoryClassPath(solutionDirectory, filename, entityName);

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            File.Delete(classPath.FullClassPath);
            Directory.Delete(classPath.ClassDirectory);
        }
    }
}
