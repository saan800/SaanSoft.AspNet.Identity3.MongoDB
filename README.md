# SaanSoft.AspNet.Identity3.MongoDB

<!-- [![Build status](https://ci.appveyor.com/api/projects/status/8xpmflre615aa0s6/branch/master?svg=true)](https://ci.appveyor.com/project/saan800/saansoft-aspnet-identity3-mongodb/branch/master) -->


An implementation for MongoDB.Driver (>= v2.1.1) with ASP.NET 5 Identity (>= v3) framework at <https://github.com/aspnet/Identity>

Nuget package at: <https://www.nuget.org/packages/SaanSoft.AspNet.Identity3.MongoDB>


## Default details
SaanSoft.AspNet.Identity3.MongoDB follows the EntityFramework naming defaults where possible, but they can be overridden 
when instantiating the RoleStore and UserStore.

The default Mongo details are:

* Database Name: AspNetIdentity
* User Collection Name: AspNetUsers
* Role Collection Name: AspNetRoles


## Sample MVC website
Under the code "samples" folder, have updated the standard Asp.Net MVC 6 sample site to use SaanSoft.AspNet.Identity3.MongoDB instead of the EntityFramework.

Code changes are in:

- config.json
- Startup.cs
- AccountViewModels.cs
- AccountController.cs


## Still TODO

* get mongoDB and tests running on AppVeyor
* help documentation for setup and overriding
  * though do have a sample MVC website that you can check out in the mean time 
* Finish all tests
* add logging to UserStore, RoleStore, IdentityDatabaseContext
    * LogDebug: of the generated mongo queries where possible
        * <http://stackoverflow.com/questions/30306193/get-generated-script-in-mongodb-c-sharp-driver>
    * LogError: add try / catch / Log around all insert/update/delete functionality to the database
* dnxcore50 (read Frameworks below)

From <http://stackoverflow.com/questions/27553952/when-is-asp-net-5-vnext-scheduled-for-release>

* update to RC (late autumn 2015)
* update to release version (Q1 2016)


## Frameworks
Currently only available for dnx451 and net451

Keeping an eye on these issues for mongoDB to be available in dnxcore50

* <https://jira.mongodb.org/browse/CSHARP-1177>
* <https://github.com/mongodb/mongo-csharp-driver/pull/210>
