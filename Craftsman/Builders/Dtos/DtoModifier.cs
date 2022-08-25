﻿namespace Craftsman.Builders.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Enums;
    using Helpers;
    using Models;

    public class DtoModifier
    {
        public static void AddPropertiesToDtos(string solutionDirectory, string entityName, List<EntityProperty> props, string projectBaseName)
        {
            UpdateDtoFile(solutionDirectory, entityName, props, Dto.Read, projectBaseName);
            UpdateDtoFile(solutionDirectory, entityName, props, Dto.Manipulation, projectBaseName);
        }

        private static void UpdateDtoFile(string solutionDirectory, string entityName, List<EntityProperty> props, Dto dto, string projectBaseName)
        {
            var dtoFileName = $"{Utilities.GetDtoName(entityName, dto)}.cs";
            var classPath = ClassPathHelper.DtoClassPath(solutionDirectory, dtoFileName, entityName, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";
            using (var input = File.OpenText(classPath.FullClassPath))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;

                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if (line.Contains($"add-on property marker"))
                        {
                            newText += @$"{Environment.NewLine}{Environment.NewLine}{DtoFileTextGenerator.DtoPropBuilder(props, dto)}";
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);
        }
    }
}
