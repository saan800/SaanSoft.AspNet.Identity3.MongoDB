# SaanSoft.AspNet.Identity3.MongoDB

<!-- [![Build status](https://ci.appveyor.com/api/projects/status/yopbw2mrf8ppqfkp/branch/master?svg=true)](https://ci.appveyor.com/project/saan800/aspnet-identity3-mongodb/branch/master) -->

An implementation for MongoDB.Driver (>= v2.1.0) with ASP.NET 5 Identity (>= v3) framework at <https://github.com/aspnet/Identity>

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
* dnxcore50 (read Frameworks below)
* update the minimum version of MongoDB.Driver to 2.1.0 when its ready (currently using 2.1.0-rc0)

From <http://stackoverflow.com/questions/27553952/when-is-asp-net-5-vnext-scheduled-for-release>

* update to beta8 (end of Sept 2015)
* update to RC (late autumn 2015)
* update to release version


## Frameworks
Currently only available for dnx451

Keeping an eye on these issues for mongoDB to be available in dnxcore50

* <https://jira.mongodb.org/browse/CSHARP-1177>
* <https://github.com/mongodb/mongo-csharp-driver/pull/210>
