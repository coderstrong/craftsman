﻿namespace Craftsman.Helpers
{
    using System;
    using Craftsman.Builders;
    using Craftsman.Builders.Dtos;
    using Craftsman.Builders.Features;
    using Craftsman.Builders.Tests.Fakes;
    using Craftsman.Builders.Tests.FunctionalTests;
    using Craftsman.Builders.Tests.IntegrationTests;
    using Craftsman.Enums;
    using Craftsman.Models;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using Builders.Endpoints;
    using Builders.Tests.Utilities;
    using static Helpers.ConsoleWriter;

    public class EntityScaffolding
    {
        public static void ScaffoldEntities(string srcDirectory,
            string testDirectory,
            string projectBaseName,
            List<Entity> entities,
            string dbContextName,
            bool addSwaggerComments,
            IFileSystem fileSystem)
        {
            foreach (var entity in entities)
            {
                // not worrying about DTOs, profiles, validators, fakers - they are all added by default
                EntityBuilder.CreateEntity(srcDirectory, entity, projectBaseName, fileSystem);
                DtoBuilder.CreateDtos(srcDirectory, entity, projectBaseName);
                ValidatorBuilder.CreateValidators(srcDirectory, projectBaseName, entity);
                ProfileBuilder.CreateProfile(srcDirectory, entity, projectBaseName);
                ApiRouteModifier.AddRoutes(testDirectory, entity, projectBaseName); // api routes always added to testing by default. too much of a pain to scaffold
                
                if(entity.Features.Count > 0)
                    ControllerBuilder.CreateController(srcDirectory, entity.Name, entity.Plural, projectBaseName);
                
                // TODO refactor to factory?
                foreach (var feature in entity.Features)
                {
                    AddFeatureToProject(srcDirectory, testDirectory, projectBaseName, dbContextName, addSwaggerComments, feature.Policies, feature, entity, fileSystem);
                }

                // Shared Tests
                FakesBuilder.CreateFakes(testDirectory, projectBaseName, entity);
            }
        }

        public static void AddFeatureToProject(string srcDirectory, string testDirectory, string projectBaseName,
            string dbContextName, bool addSwaggerComments, List<Policy> policies, Feature feature, Entity entity,
            IFileSystem fileSystem)
        {
            var controllerClassPath = ClassPathHelper.ControllerClassPath(srcDirectory, $"{Utilities.GetControllerName(entity.Plural)}.cs", projectBaseName);
            if (!File.Exists(controllerClassPath.FullClassPath))
                ControllerBuilder.CreateController(srcDirectory, entity.Name, entity.Plural, projectBaseName);
            
            if (feature.Type == FeatureType.AddRecord.Name)
            {
                CommandAddRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                AddCommandTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                CreateEntityTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.AddRecord, entity, addSwaggerComments, policies, 
                    feature, projectBaseName);
            }

            if (feature.Type == FeatureType.GetRecord.Name)
            {
                QueryGetRecordBuilder.CreateQuery(srcDirectory, entity, dbContextName, projectBaseName);
                GetRecordQueryTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                GetEntityRecordTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.GetRecord, entity, addSwaggerComments, policies, 
                    feature, projectBaseName);
            }

            if (feature.Type == FeatureType.GetList.Name)
            {
                QueryGetListBuilder.CreateQuery(srcDirectory, entity, dbContextName, projectBaseName);
                GetListQueryTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                GetEntityListTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.GetList, entity, addSwaggerComments, policies, 
                    feature, projectBaseName);
            }
            
            if (feature.Type == FeatureType.DeleteRecord.Name)
            {
                CommandDeleteRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                DeleteCommandTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                DeleteEntityTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.DeleteRecord, entity, addSwaggerComments, policies, 
                    feature, projectBaseName);
            }
            
            if (feature.Type == FeatureType.UpdateRecord.Name)
            {
                CommandUpdateRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                PutCommandTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                PutEntityTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.UpdateRecord, entity, addSwaggerComments, policies, 
                    feature, projectBaseName);
            }
            
            if (feature.Type == FeatureType.PatchRecord.Name)
            {
                CommandPatchRecordBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName);
                PatchCommandTestBuilder.CreateTests(testDirectory, entity, projectBaseName);
                PatchEntityTestBuilder.CreateTests(testDirectory, entity, policies, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.PatchRecord, entity, addSwaggerComments, policies, 
                    feature, projectBaseName);
            }
            
            if (feature.Type == FeatureType.AddListByFk.Name)
            {
                CommandAddListBuilder.CreateCommand(srcDirectory, entity, dbContextName, projectBaseName, feature, fileSystem);
                AddListCommandTestBuilder.CreateTests(testDirectory, entity, feature, projectBaseName, fileSystem);
                AddListTestBuilder.CreateTests(testDirectory, entity, policies, feature, projectBaseName, fileSystem);
                ControllerModifier.AddEndpoint(srcDirectory, FeatureType.AddListByFk, entity, addSwaggerComments, policies, 
                    feature, projectBaseName);
            }

            if (feature.Type == FeatureType.AdHoc.Name)
            {
                EmptyFeatureBuilder.CreateCommand(srcDirectory, dbContextName, projectBaseName, feature);
                // TODO ad hoc feature endpoint
                // TODO empty failing test to promote test writing?
            }
        }
    }
}
