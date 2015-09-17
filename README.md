# AspNet.Identity3.MongoDB

<!-- [![Build status](https://ci.appveyor.com/api/projects/status/yopbw2mrf8ppqfkp/branch/master?svg=true)](https://ci.appveyor.com/project/saan800/aspnet-identity3-mongodb/branch/master) -->

An implementation for MongoDB.Driver (>= v2.0.0) with ASP.NET 5 Identity (>= v3) framework at <https://github.com/aspnet/Identity>


## Default details
AspNet.Identity3.MongoDB follows the EntityFramework naming defaults where possible, but they can be overridden 
when instantiating the RoleStore and UserStore.

The default Mongo details are:

* Database Name: AspNetIdentity
* User Collection Name: AspNetUsers
* Role Collection Name: AspNetRoles

## Sample MVC website
Under the code "samples" folder, have updated the standard Asp.Net MVC 6 sample site to use AspNet.Identity3.MongoDB instead of the EntityFramework.

Code changes are in:

- config.json
- Startup.cs
- AccountViewModels.cs
- AccountController.cs


## Still TODO

* create nuget package
* help documentation for setup and overriding
* get mongoDB and tests running on AppVeyor
* Finish all tests
* When queryable is implemented in mongodb driver (read Warning below), update UserStore and RoleStore implementations
* dnxcore50 (read Frameworks below)

From <http://stackoverflow.com/questions/27553952/when-is-asp-net-5-vnext-scheduled-for-release>

* update to beta8 (end of Sept 2015)
* update to RC (late autumn 2015)
* update to release version


## Frameworks
Currently only available for dnx451

Keeping an eye on these issues for mongoDB to be available in dnxcore50
- <https://jira.mongodb.org/browse/CSHARP-1177>
- <https://github.com/mongodb/mongo-csharp-driver/pull/210>




# WARNING
RoleStore.Roles and UserStore.Users are functions to return a IQueryable of roles/users.
However MongoDb have not yet implemented any AsQueryable functionality in MongoDB.Driver yet.
- <https://jira.mongodb.org/browse/CSHARP-935>
- <http://stackoverflow.com/questions/29124995/is-asqueryable-method-departed-in-new-mongodb-c-sharp-driver-2-0rc>

At the moment I have implemented the IQueryable from a ToList() the entire role/user collections. **This will not perform well.**

I highly recommend you don't use the two Queryable functions unless you are very, very sure it will always be a small collection.

I'm keeping an eye on <https://jira.mongodb.org/browse/CSHARP-935> and will update the implementation when possible and remove this warning.
