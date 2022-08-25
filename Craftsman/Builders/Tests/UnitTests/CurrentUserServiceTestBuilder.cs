﻿namespace Craftsman.Builders.Tests.UnitTests
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System.IO;
    using System.IO.Abstractions;
    using System.Text;

    public class CurrentUserServiceTestBuilder
    {
        public static void CreateTests(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.UnitTestWrapperTestsClassPath(solutionDirectory, $"CurrentUserServiceTests.cs", projectBaseName);
            var fileText = WriteTestFileText(solutionDirectory, classPath, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        private static string WriteTestFileText(string solutionDirectory, ClassPath classPath, string projectBaseName)
        {
            var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(solutionDirectory, "", projectBaseName);

            return @$"namespace {classPath.ClassNamespace};

using {servicesClassPath.ClassNamespace};
using System.Security.Claims;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NUnit.Framework;

public class {Path.GetFileNameWithoutExtension(classPath.FullClassPath)}
{{
    [Test]
    public void returns_user_in_context_if_present()
    {{
        var name = new Faker().Person.UserName;

        var id = new ClaimsIdentity();
        id.AddClaim(new Claim(ClaimTypes.NameIdentifier, name));

        var context = new DefaultHttpContext().HttpContext;
        context.User = new ClaimsPrincipal(id);

        var sub = Substitute.For<IHttpContextAccessor>();
        sub.HttpContext.Returns(context);
        
        var currentUserService = new CurrentUserService(sub);

        currentUserService.UserId.Should().Be(name);
    }}
    
    [Test]
    public void returns_null_if_user_is_not_present()
    {{
        var context = new DefaultHttpContext().HttpContext;
        var sub = Substitute.For<IHttpContextAccessor>();
        sub.HttpContext.Returns(context);
        
        var currentUserService = new CurrentUserService(sub);

        currentUserService.UserId.Should().BeNullOrEmpty();
    }}
}}";
        }
    }
}