# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
~~and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).~~

> Semantic versioning will come once I hit v1, but at this point each `0.x` release may have breaking changes as I iron out the kinks of the framework. 

## [Unreleased]

### Added

* Added BaseEntity that all entities will inherit from.
  * TODO: docs
* Added built in features to the `add:feature` command
* New `AddListByFk` option for the `add:feature`  command and `Feature` property of an entity.
  * TODO: docs
* New `craftsman example` or `craftsman new:example` command to create an example project with a prompted workflow to select. `Basic`, `WithAuth`, `AuthServer`, `WithBus`
* Added `.DS_Store` and `.env` to gitignore
* Added Consumer test
* Added `provider` to test fixture when adding a bus
* Added a mock `IPublishEndpoint` service to `TestFixture` when using MassTransit
  * update docs that mediatr handler tests aren't broken when pubilshing anymore
* New policies added to swagger on `add:entity` scaffolding
* New `add:authserver` command as well as an `AuthServer` option when creating a domain
  * TODO DOCS
    * only supports scaffolding for one scope (for now) -- they can still be added manually if you need multiple!
    * No consent support (yet)

### Updated

* Moved Policies to Feature
  * TODO: docs
  
* There is no more primary key property. A Guid with a name `Id` will be inherited by all entities.

* Docker utilities for integration test refactored to use Fluent Docker wherever possible for better readability. Some enhancements were made as well (e.g. better container/volume naming, proper volume mounting).

* Cleaned up test names

* Added `Secret` back to Environment options

* Updated FK support to better API

  * TODO: docs -- `ForeignEntityPlural` defaults to `s` suffix if not provided

    ```yaml
        Properties:
          - Name: EventId
            Type: Guid
            ColumnName: event_id
            ForeignEntityName: Event
            ForeignEntityPlural: Events
    ```
    
    Can not have fk in manipulation DTO. will need to add manually and adjust feature accordingly (currently assumes it's from query param)
    
    > TODO: add `RelationshipToParentEntity` to allow for appropriate prop to be added to entity (`Many` adds a list with a prop using the plural, Single adds a prop with a singular type and name)

### Fixed

- Removed the broken patch validation from command
- No more `409` produced response annotation on POST
- Descending sort tests now actually test desc instead of mirroring asc
- Removed error handler comments in controller
- Empty controller no longer added when no features present (fixes #40)
- Messages project will be properly referenced when using a bus
- Swagger policies won't get duplicates

## [0.11.2] - TBD

### Fixed

- Better variable name on delete integ test

## [0.11.1] - 2021-08-23

### Fixed

- Removed extra parentheses on endpoint names

## [0.11.0] - 2021-08-22

### Added

- Added an `add:feature` command (also works with `new:feature`)
- Non-nullable guids will now have a default of `Guid.NewGuid()` unless otherwise specified
- Added XML docs to release in csproj
- Added a new `features` option to entities to allow for granular feature control. The accepted values are `AdHoc`, `GetRecord`, `GetList`, `DeleteRecord`, `UpdateRecord`, `PatchRecord`, `AddRecord`, and `CreateRecord` (same as `AddRecord` but available as an alias in case you can't remember!)

### Changed

- Instead of 3 BC projects (Core, Infra, Api) there will now be one. This helps with colocation and does force premature optimization for something like a model library and a heavily separated infra project.
  - Features dir changed to Domain with a features directory inside of it
  - Entities live on their respective entity folder in the domain with their feature
  - Removed the `webapi` suffix on the api project
  - Core directories moved to web api
  - `Contexts` dir changed to `Databases`
- Removed save successful checks on add and update commands
- Added better naming to PUT command variables
- Guid PKs will no longer be added to the creation DTO
- POST commands will no longer have a conflict check since you can't add a PK anymore
- A conflict integration test will not be added anymore
- `NoKeyGenerated` commands are no longer in dbcontext
- Controller url all lowercase
- Swagger comments on by default
- 409 no longer shown on swagegr comments for POST
- No more save successful check on delete command
- Seeders have a sub `DummyData` directory
- Sieve service registration moved to webapi service class
- `SolutionName` to `ProjectName`

### Fixed

- Seeder indentation in startup fixed
- PUT commands will no longer throw 500 when entity is not modified (#31)
- Route indentation fixed
- Removed annoying comments from features
- Fixed test name for basic gets in functional tests
- Seeders in startup will newline when there are multiple entities
- Unicode now onlyenforced on windows (for better emoji support)
- Fixed `isRequired` property
- Command prop for `Update` command has proper casing

### Removed

- Removed the `add:property` cli command

## [0.10.0] - 2021-05-31

### Added

- Added [SpectreConsole](https://spectreconsole.net/) for a better CLI experience
- Added `add:bus` command
- Added `add:consumer` and `add:producer` commands for direct, topic, and fanout messages
- Added Bus, Producers, and Consumers props to BC template
- Added Messages to the domain template
- Added `add:message` command
- Added conflict test for add command when using a guid
- Added tests for command and query exceptions
  - Didn't do them in the controller as that is not the dependency. Can test the exceptions causing the correct httpstatus code in the exception separately

### Changed

* Changed the `new:domain` output to a single solution with directories for each bounded context for easier management
* Changed the seeder regions in `StartupDevelopment.cs` to comments
* Changed the Logger settings in `Program.cs`
* Updated `add:entity` and `add:prop` to now be called from the BC directory
* Updated `ProducesResponseType` in controllers to generic `Response` type where applicable
* Updated App Registrations to separate files
* Updated Service Registrations to separate files
* Updated entity name and entity prop names first letter to always be capitalized
* Better namespacing for features in controllers using static classes for features
* Updated functional test to pass without conflict
* Updated nuget packages
* Updated `Program.cs` to async
* Changed migrations to happen after all bounded contexts are added
* Removed the custom fluent validation boilderplate from the add, update, patch, and ad hoc commands

### Removed

- Removed verbosity option from commands due to simplified spectre console
- Removed legacy comment for include statement marker
- Removed BC readme and updated sln readme

### Fixed

- Fixed double error messages
- Fixed incorrect help message for `new:domain` command (#24)
- Fixed help text on `list` command
- Fixed controllers to inherit from `ControllerBase` instead of `Controller`. fixes (#26)
- Fixed extra space in the class in the dto classes when not abstract and trailing new line
- If using a guid for a PK, it will be added to the creation dto (not manipulation or update) -- fixes #28
  - Guid PKs will have a default value of `Guid.NewGuid()` in their creation dto
- PK already exists guard will be added for GUIDs and will be performed when adding a new entity and throw a 409 conflict via a new conflict exception if a record already exists with that guid. -- fixes #29
- Fixed issue where POST would throw 500 when primary key != EntityNameId (e.g. PK of ReportId would break for an entity of ReportRequest) - fixes #30
- Fixed default value for strings on entities to use quotes
- Fixed missing exception handling on `add:bc` command

## [0.9.3] - 2021-04-10

### Fixed

- Fixed autogen identity

## [0.9.2] - 2021-04-10

### Fixed

- Lingering dbcontext and dbname mix up

## [0.9.1] - 2021-04-10

### Fixed

- Db context will now be used instead of name in api scaffolding
- Test utilities in functional tests will now be added when not using auth

## [0.9.0] - 2021-04-10

### Added

- Added a new vertical slice architecture
  - Projects have been consolidated and will now have a prefix of the solution name before each project type. For example, the api project with a solution name of `ordering` is `ordering.webapi`
- Added a `new:domain` command to create a ddd based domain with various bounded contexts inside of it. this is recommended for long term maintainability
- Added the `add:bc` command which will add a new bounded context to your ddd project
- Testing completely rebuilt from the ground up. Now has unit, integration, and functional tests. Integration and Functional Tests can spin up their own docker db on their own to run against a real database.
- Moved 'addGit' property from the api template to the domain template
- Added a `version` or `-v` command to get the craftsman version
- Added an initial db migration to run automatically on project creation
- Added verbosity option to new domain and add bc
- Added a version checker to make sure you are alerted if out of date
- Added an `add:prop` alias
- Added explicit add entity template with auth policies available to add
- Added a production app settings by default

### Changed

- Changed the startup marker for dynamic services to a comment instead of a region
- Readme will now be generated in the domain directory
- Updated environment to have production as a reserved word instead of startup to be consistent with dotnet process
  - Will use startup and appsettings.production
  - Normal appsettings will be empty, but have all the config keys required to make migrations and builds possible
- Updated the default Cors policy name
- Consolidated launchsettings to have the same setup for all environments as it is just a setting for the IDE and not used for the release package

### Removed

- Removed `micro` command to consolidate and reduce complexity. if you still want to build a microservice, you can build a domain and deploy each bounded context as a microservice
  - Gateways were removed and may be added back with better integration in a future release
- Removed the `new:api` command to focus on the DDD driven style
- Removed `ClientSecret` to promote code+PKCE flow

### Fixed

- Existing auth policies will now be skipped for registration when adding a new entity
- Fixed documented response codes for delete, put, and patch from 201 > 204
- Foreign keys will no longer be automatically included in features or DTOs for better performance (#2)

## [0.8.2] - 2021-02-25

### Fixed

- Fixed issue where xml comments would throw an error on non windows machines (#16)

## [0.8.1] - 2021-02-22

### Fixed

- fixed bug when creating an api with auth settings

## [0.8.0] - 2021-02-22

### Added

- New `add:micro` command that scaffolds a new microservice template as well as an ocelot gateway
- New `port ` property on the `new:api` template to let you customize and api or microservice port on localhost
- Added `https` default on local
- Added additional startup middleware
  - `UseHsts` for non dev environments
  - `UseHttpsRedirection` with notes on even more secure options
- New `AuthorizationSettings` object and authorization based properties on the environments for the `new:api` and `new:micro` commands
- Added new `GetEntity` and `DeleteEntity` integration tests with and without auth
- Added 401/403 response types to swagger comments when using auth
- Added auth to swagger setup
  - note that secret is currently stored in appsettings
- Auth added to integration tests when required

### Fixed

- The `CurrentStartIndex` calculation in the `PagedList` class was broken and now has a new calculation.
- Added null conditional operator (`?.`) to certain tests before `.Data` to make them fail more gracefully
  - Get{entity.Plural}_ReturnsSuccessCodeAndResourceWithAccurateFields()
  - Put{entity.Name}ReturnsBodyAndFieldsWereSuccessfullyUpdated
- Cleaned up `WebApplicationFactory` to remove deprecated services.
- Removed `[Collection(""Sequential"")]` from repo tests

### Clean up

- Internal tests now passing
- refactored out template drilling
- removed old auth debt from earlier alpha

## [0.7.0] - 2021-01-12

### Added

- Removed the dependency on the foundation api template!

### Fixed

- Fixed `UseEnvironment` in WebAppFactory to use `Development`
- Fixed integration tests to use the new `Response` wrapper
- Updated pagination tests to have proper keys due to default sort order possibly breaking these tests

## [0.6.1] - 2021-01-06

### Fixed

- XML comment info is now properly added to `WebApi.csproj` and the Swagger config
- Extra line will no longer be added when no swagger contact url is provided
- Repository now sets default sort order for proper sql compatibility in lists (issue #9)

## [0.6.0] - 2020-12-22

### Added

- Added table name and schema properties to entity
- Added column name attribute to entity properties
- Added Serilog by default in all new projects. This includes Console and Seq logging by default in `Development`. For non-Development environments, you'll need to add whatever logging you're interested in to their respective app-settings projects. There are just too many options to create a whole API on top of Serilog.
- Updated swagger implementation from nswag
- Added Consumes and Produces headers to the controller endpoints

- Added an option to manage additional swagger settings to your API endpoints. This will be turned off by default for now as dealing the with xml docs path is potentially burdensome, but will add a lot of valuable details for users consuming your API. If you are looking to add additional XML details, this is highly recommended. 
- Added a custom Response Wrapper to the GET and POST endpoints

### Fixed

- Fixed launch settings to have null environment  variable for Startup (Production). If you'd like to change this, be sure to update the appsetting lookup in `Program.cs`
- Fixed POST endpoint that was lacking a `[FromBody]` marker
- `BasePaginationParameters` will now have `MaxPageSizee` and `DefaultPageSize` set as `internal` properties so they don't show up in swagger. These can be overridden in the distinct entity classes like so: `internal override int MaxPageSize { get; } = 30;`
- Fixed controllers to be able to handle a name and plural with the same value (e.g. Buffalo)

## [0.5.0] - 2020-12-14

### Added

- Added `add:entities` alias for the `add:entity` command
- Can now add Guid or other non-integer primary key

### Fixed

- Fixed bug where postgres library was getting added every time

## [0.4.2] - 2020-12-10

### Fixed

- Seeder was not getting added to `StartupDevelopment` when using `add:entity` command

## [0.4.1] - 2020-12-10

### Fixed

- Async method in controller POST wasn't awaited

## [0.4.0] - 2020-12-06

### Added

- Default `Startup.cs` class can now be configured using the reserved `Startup` keyword

### Fixed

- Fixed `craftsman add:property -h` help text
- The appsettings connection string will now escape backslash
- Foreign key using statement will now be dynamic on DTOs

## [0.3.1] - 2020-12-02

### Added

- Added `new:webapi` alias that acts the same as `new:api`

### Fixed

- Fixed `craftsman add:property -h` to point to the correct help page

## [0.3.0] - 2020-11-14

### Added

- Updated the API to run on NET 5.0
- Pagination metadata enhancements on PagedList that is returned in  GET list endpoint will now include more metadata for the current page  including the current size as well as the start and end indices. I also  removed the Next and Previous Page URI links to reduce complexity.
- Updated all controller calls to be asynchronous, including the get list
- Saves updated to be asynchronous
- One major capability I want to add into this is a good basis for auth  generation. I've started to build this out, but it could (and very  likely will) change drastically. With that said, I left it in as an  alpha feature in case anyone is interested in trying it.

### Fixed

- Add Entity bug in repository fixed
- Fixed some builder options when not using auth
