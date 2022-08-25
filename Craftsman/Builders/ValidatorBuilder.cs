﻿namespace Craftsman.Builders
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class ValidatorBuilder
    {
        public static void CreateValidators(string solutionDirectory, string srcDirectory, string projectBaseName, Entity entity)
        {
            BuildValidatorClass(solutionDirectory, srcDirectory, projectBaseName, entity, Validator.Manipulation);
            BuildValidatorClass(solutionDirectory, srcDirectory, projectBaseName, entity, Validator.Creation);
            BuildValidatorClass(solutionDirectory, srcDirectory, projectBaseName, entity, Validator.Update);
        }
        
        public static void CreateRolePermissionValidators(string solutionDirectory, string srcDirectory, string projectBaseName, Entity entity, IFileSystem fileSystem)
        {
            BuildValidatorClass(solutionDirectory, srcDirectory, projectBaseName, entity, Validator.Creation);
            BuildValidatorClass(solutionDirectory, srcDirectory, projectBaseName, entity, Validator.Update);
            
            var manipulationClassPath = ClassPathHelper.ValidationClassPath(srcDirectory, $"{Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}.cs", entity.Plural, projectBaseName);
            var manipulationFileText = GetRolePermissionManipulationValidatorFileText(solutionDirectory, srcDirectory, projectBaseName, manipulationClassPath.ClassNamespace, entity);
            Utilities.CreateFile(manipulationClassPath, manipulationFileText, fileSystem);
        }

        private static void BuildValidatorClass(string solutionDirectory, string srcDirectory, string projectBaseName, Entity entity, Validator validator)
        {
            var classPath = ClassPathHelper.ValidationClassPath(srcDirectory, $"{Utilities.ValidatorNameGenerator(entity.Name, validator)}.cs", entity.Plural, projectBaseName);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using FileStream fs = File.Create(classPath.FullClassPath);
            var data = validator switch
            {
                Validator.Creation => GetCreationValidatorFileText(solutionDirectory, classPath.ClassNamespace, entity, projectBaseName),
                Validator.Update => GetUpdateValidatorFileText(solutionDirectory, classPath.ClassNamespace, entity, projectBaseName),
                Validator.Manipulation => GetManipulationValidatorFileText(srcDirectory, classPath.ClassNamespace, entity, projectBaseName),
                _ => throw new Exception("Unrecognized validator exception.")
            };

            fs.Write(Encoding.UTF8.GetBytes(data));
        }

        public static string GetCreationValidatorFileText(string solutionDirectory, string classNamespace, Entity entity, string projectBaseName)
        {
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using FluentValidation;

public class {Utilities.ValidatorNameGenerator(entity.Name, Validator.Creation)}: {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<{Utilities.GetDtoName(entity.Name, Dto.Creation)}>
{{
    public {Utilities.ValidatorNameGenerator(entity.Name, Validator.Creation)}()
    {{
        // add fluent validation rules that should only be run on creation operations here
        //https://fluentvalidation.net/
    }}
}}";
        }

        public static string GetUpdateValidatorFileText(string solutionDirectory, string classNamespace, Entity entity, string projectBaseName)
        {
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using FluentValidation;

public class {Utilities.ValidatorNameGenerator(entity.Name, Validator.Update)}: {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<{Utilities.GetDtoName(entity.Name, Dto.Update)}>
{{
    public {Utilities.ValidatorNameGenerator(entity.Name, Validator.Update)}()
    {{
        // add fluent validation rules that should only be run on update operations here
        //https://fluentvalidation.net/
    }}
}}";
        }
        
        public static string GetManipulationValidatorFileText(string solutionDirectory, string classNamespace, Entity entity, string projectBaseName)
        {
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using FluentValidation;

public class {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<T> : AbstractValidator<T> where T : {Utilities.GetDtoName(entity.Name, Dto.Manipulation)}
{{
    public {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}()
    {{
        // add fluent validation rules that should be shared between creation and update operations here
        //https://fluentvalidation.net/
    }}

    // want to do some kind of db check to see if something is unique? try something like this with the `MustAsync` prop
    // source: https://github.com/jasontaylordev/CleanArchitecture/blob/413fb3a68a0467359967789e347507d7e84c48d4/src/Application/TodoLists/Commands/CreateTodoList/CreateTodoListCommandValidator.cs
    // public async Task<bool> BeUniqueTitle(string title, CancellationToken cancellationToken)
    // {{
    //     return await _context.TodoLists
    //         .AllAsync(l => l.Title != title, cancellationToken);
    // }}
}}";
        }
        
        public static string GetRolePermissionManipulationValidatorFileText(string solutionDirectory, string srcDirectory, string projectBaseName, string classNamespace, Entity entity)
        {
            var dtoClassPath = ClassPathHelper.DtoClassPath(solutionDirectory, "", entity.Name, projectBaseName);
            var permissionsClassPath = ClassPathHelper.PolicyDomainClassPath(srcDirectory, "", projectBaseName);
            var rolesClassPath = ClassPathHelper.SharedKernelDomainClassPath(solutionDirectory, "");
            
            return @$"namespace {classNamespace};

using {dtoClassPath.ClassNamespace};
using {permissionsClassPath.ClassNamespace};
using {rolesClassPath.ClassNamespace};
using FluentValidation;

public class {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}<T> : AbstractValidator<T> where T : {Utilities.GetDtoName(entity.Name, Dto.Manipulation)}
{{
    public {Utilities.ValidatorNameGenerator(entity.Name, Validator.Manipulation)}()
    {{
        RuleFor(rp => rp.Permission)
            .Must(BeAnExistingPermission)
            .WithMessage(""Please use a valid role."");
        RuleFor(rp => rp.Role)
            .Must(BeAnExistingRole)
            .WithMessage(""Please use a valid role."");
    }}
    
    private static bool BeAnExistingPermission(string permission)
    {{
        return Permissions.List().Contains(permission);
    }}

    private static bool BeAnExistingRole(string role)
    {{
        return Roles.List().Contains(role);
    }}
}}";
        }
    }
}