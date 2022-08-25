﻿namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System.IO.Abstractions;
    using System.Text;

    public class WebApiServiceExtensionsBuilder
    {
        public static void CreateApiVersioningServiceExtension(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(solutionDirectory, $"ApiVersioningServiceExtension.cs", projectBaseName);
            var fileText = GetApiVersioningServiceExtensionText(classPath.ClassNamespace);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static void CreateMassTransitServiceExtension(string solutionDirectory, string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"{Utilities.GetMassTransitRegistrationName()}.cs", projectBaseName);
            var fileText = GetMassTransitServiceExtensionText(classPath.ClassNamespace, solutionDirectory, srcDirectory, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static void CreateWebApiServiceExtension(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"WebApiServiceExtension.cs", projectBaseName);
            var fileText = GetWebApiServiceExtensionText(classPath.ClassNamespace, srcDirectory, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static void CreateCorsServiceExtension(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.WebApiServiceExtensionsClassPath(srcDirectory, $"CorsServiceExtension.cs", projectBaseName);
            var fileText = GetCorsServiceExtensionText(classPath.ClassNamespace, srcDirectory, projectBaseName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }

        public static string GetApiVersioningServiceExtensionText(string classNamespace)
        {
            return @$"namespace {classNamespace};

using AutoMapper;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;

public static class ApiVersioningServiceExtension
{{
    public static void AddApiVersioningExtension(this IServiceCollection services)
    {{
        services.AddApiVersioning(config =>
        {{
            // Default API Version
            config.DefaultApiVersion = new ApiVersion(1, 0);
            // use default version when version is not specified
            config.AssumeDefaultVersionWhenUnspecified = true;
            // Advertise the API versions supported for the particular endpoint
            config.ReportApiVersions = true;
        }});
    }}
}}";
        }

        public static string GetCorsServiceExtensionText(string classNamespace, string srcDirectory, string projectBaseName)
        {
            var classPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, $"ApiVersioningServiceExtension.cs", projectBaseName);
            return @$"namespace {classNamespace};

using {classPath.ClassNamespace};
using AutoMapper;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

public static class CorsServiceExtension
{{
    public static void AddCorsService(this IServiceCollection services, string policyName, IWebHostEnvironment env)
    {{
        if (env.IsDevelopment() || env.IsEnvironment(LocalConfig.IntegrationTestingEnvName) ||
            env.IsEnvironment(LocalConfig.FunctionalTestingEnvName))
        {{
            services.AddCors(options =>
            {{
                options.AddPolicy(policyName, builder => 
                    builder.SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(""X-Pagination""));
            }});
        }}
        else
        {{
            //TODO update origins here with env vars or secret
            //services.AddCors(options =>
            //{{
            //    options.AddPolicy(policyName, builder =>
            //        builder.WithOrigins(origins)
            //        .AllowAnyMethod()
            //        .AllowAnyHeader()
            //        .WithExposedHeaders(""X-Pagination""));
            //}});
        }}
    }}
}}";
        }

        public static string GetWebApiServiceExtensionText(string classNamespace, string srcDirectory, string projectBaseName)
        {
            var servicesClassPath = ClassPathHelper.WebApiServicesClassPath(srcDirectory, "", projectBaseName);
            var middlewareClassPath = ClassPathHelper.WebApiMiddlewareClassPath(srcDirectory, $"", projectBaseName);
            
            return @$"namespace {classNamespace};

using {servicesClassPath.ClassNamespace};
using {middlewareClassPath.ClassNamespace};
using AutoMapper;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Sieve.Services;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

public static class WebApiServiceExtension
{{
    public static void AddWebApiServices(this IServiceCollection services)
    {{
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        services.AddMediatR(typeof(Startup));
        services.AddScoped<SieveProcessor>();
        services.AddMvc(options => options.Filters.Add<ErrorHandlerFilterAttribute>())
            .AddFluentValidation(cfg => {{ cfg.AutomaticValidationEnabled = false; }});
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
    }}
}}";
        }

        public static string GetMassTransitServiceExtensionText(string classNamespace, string solutionDirectory, string srcDirectory, string projectBaseName)
        {
            var utilsClassPath = ClassPathHelper.WebApiResourcesClassPath(srcDirectory, "", projectBaseName);
            
            var messagesClassPath = ClassPathHelper.MessagesClassPath(solutionDirectory, "");
            
            return @$"namespace {classNamespace};

using {utilsClassPath.ClassNamespace};
using MassTransit;
using {messagesClassPath.ClassNamespace};
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System.Reflection;

public static class MassTransitServiceExtension
{{
    public static void AddMassTransitServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {{
        if (!env.IsEnvironment(LocalConfig.IntegrationTestingEnvName) 
            && !env.IsEnvironment(LocalConfig.FunctionalTestingEnvName) 
            && !env.IsDevelopment())
        {{
            services.AddMassTransit(mt =>
            {{
                mt.AddConsumers(Assembly.GetExecutingAssembly());
                mt.UsingRabbitMq((context, cfg) =>
                {{
                    cfg.Host(Environment.GetEnvironmentVariable(""RMQ_HOST""), Environment.GetEnvironmentVariable(""RMQ_VIRTUAL_HOST""), h =>
                    {{
                        h.Username(Environment.GetEnvironmentVariable(""RMQ_USERNAME""));
                        h.Password(Environment.GetEnvironmentVariable(""AUTH_PASSWORD""));
                    }});

                    // Producers -- Do Not Delete This Comment

                    // Consumers -- Do Not Delete This Comment
                }});
            }});
            services.AddMassTransitHostedService();
        }}
    }}
}}";
        }
    }
}