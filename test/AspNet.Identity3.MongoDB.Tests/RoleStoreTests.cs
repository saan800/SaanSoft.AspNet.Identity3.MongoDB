using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MongoDB.Driver;
using Xunit;

namespace AspNet.Identity3.MongoDB.Tests
{
	public class RoleStoreTests : IDisposable
	{
		private readonly DatabaseFixture _databaseFixture;
		private readonly IMongoCollection<IdentityRole> _roleCollection;
		private readonly string _collectionPrefix;

		private readonly RoleStore<IdentityRole> _roleStore;
		private readonly IdentityErrorDescriber _errorDescriber;

		public RoleStoreTests(string collectionPrefix)
		{
			collectionPrefix = $"{typeof(RoleStoreTests).Name}_{collectionPrefix}";

			_databaseFixture = new DatabaseFixture(collectionPrefix);
			_roleCollection = _databaseFixture.GetCollection<IdentityRole>();
			_collectionPrefix = collectionPrefix;

			_roleStore = new RoleStore<IdentityRole>(_roleCollection);
			_errorDescriber = new IdentityErrorDescriber();
		}

		public void Dispose()
		{
			_databaseFixture.Dispose();
		}

		public class Constructors : RoleStoreTests
		{
			public Constructors() : base(typeof(Constructors).Name) { }

			[Fact]
			public void Can_inisialise_from_connectionString()
			{
				// arrange
				string collectionName = "TestColName";
				var roleStore = new RoleStoreHelper(_databaseFixture.ConnectionString, _databaseFixture.DatabaseName, collectionName);

				// assert
				Assert.Equal(_databaseFixture.DatabaseName, roleStore.DatabaseName);
				Assert.Equal(collectionName, roleStore.CollectionName);

				Assert.NotNull(roleStore.MongoClient);
				Assert.NotNull(roleStore.MongoDatabase);
				Assert.NotNull(roleStore.MongoCollection);

				Assert.Equal(_databaseFixture.DatabaseName, roleStore.MongoDatabase.DatabaseNamespace.DatabaseName);
				Assert.Equal(_databaseFixture.DatabaseName, roleStore.MongoCollection.CollectionNamespace.DatabaseNamespace.DatabaseName);
				Assert.Equal(collectionName, roleStore.MongoCollection.CollectionNamespace.CollectionName);
			}

			[Fact]
			public void Can_inisialise_from_connectionString_with_default_db_and_collection_names()
			{
				// arrange
				var roleStore = new RoleStoreHelper(_databaseFixture.ConnectionString);

				// assert
				string defaultDatabaseName = "AspNetIdentity";
				string defaultCollectionName = "AspNetRoles";
				Assert.Equal(defaultDatabaseName, roleStore.DatabaseName);
				Assert.Equal(defaultCollectionName, roleStore.CollectionName);

				Assert.Equal(defaultDatabaseName, roleStore.MongoDatabase.DatabaseNamespace.DatabaseName);
				Assert.Equal(defaultDatabaseName, roleStore.MongoCollection.CollectionNamespace.DatabaseNamespace.DatabaseName);
				Assert.Equal(defaultCollectionName, roleStore.MongoCollection.CollectionNamespace.CollectionName);
			}

			[Fact]
			public void Can_inisialise_from_MongoClient()
			{
				// arrange
				string collectionName = "TestColName";
				var roleStore = new RoleStoreHelper(_databaseFixture.GetMongoClient(), _databaseFixture.DatabaseName, collectionName);

				// assert
				Assert.Equal(_databaseFixture.DatabaseName, roleStore.DatabaseName);
				Assert.Equal(collectionName, roleStore.CollectionName);

				Assert.NotNull(roleStore.MongoClient);
				Assert.NotNull(roleStore.MongoDatabase);
				Assert.NotNull(roleStore.MongoCollection);

				Assert.Equal(_databaseFixture.DatabaseName, roleStore.MongoDatabase.DatabaseNamespace.DatabaseName);
				Assert.Equal(_databaseFixture.DatabaseName, roleStore.MongoCollection.CollectionNamespace.DatabaseNamespace.DatabaseName);
				Assert.Equal(collectionName, roleStore.MongoCollection.CollectionNamespace.CollectionName);
			}

			[Fact]
			public void Can_inisialise_from_MongoDatabase()
			{
				// arrange
				string collectionName = "TestColName";
				var roleStore = new RoleStoreHelper(_databaseFixture.GetMongoDatabase(), collectionName);

				// assert
				Assert.Equal(_databaseFixture.DatabaseName, roleStore.DatabaseName);
				Assert.Equal(collectionName, roleStore.CollectionName);

				Assert.NotNull(roleStore.MongoClient);
				Assert.NotNull(roleStore.MongoDatabase);
				Assert.NotNull(roleStore.MongoCollection);

				Assert.Equal(_databaseFixture.DatabaseName, roleStore.MongoDatabase.DatabaseNamespace.DatabaseName);
				Assert.Equal(_databaseFixture.DatabaseName, roleStore.MongoCollection.CollectionNamespace.DatabaseNamespace.DatabaseName);
				Assert.Equal(collectionName, roleStore.MongoCollection.CollectionNamespace.CollectionName);
			}

			[Fact]
			public void Can_inisialise_from_MongoCollection()
			{
				// arrange
				string collectionName = _roleCollection.CollectionNamespace.CollectionName;
				var roleStore = new RoleStoreHelper(_roleCollection);

				// assert
				Assert.Equal(_databaseFixture.DatabaseName, roleStore.DatabaseName);
				Assert.Equal(collectionName, roleStore.CollectionName);

				Assert.NotNull(roleStore.MongoClient);
				Assert.NotNull(roleStore.MongoDatabase);
				Assert.NotNull(roleStore.MongoCollection);

				Assert.Equal(_databaseFixture.DatabaseName, roleStore.MongoDatabase.DatabaseNamespace.DatabaseName);
				Assert.Equal(_databaseFixture.DatabaseName, roleStore.MongoCollection.CollectionNamespace.DatabaseNamespace.DatabaseName);
				Assert.Equal(collectionName, roleStore.MongoCollection.CollectionNamespace.CollectionName);
			}

		}

		public class CreateAsyncMethod : RoleStoreTests
		{
			public CreateAsyncMethod() : base(typeof(CreateAsyncMethod).Name) { }

			[Fact]
			public async Task Create_role_returns_Success()
			{
				// arrange
				var claim1 = new IdentityClaim { ClaimType = "ClaimType1", ClaimValue = "some value" };
				var claim2 = new IdentityClaim { ClaimType = "ClaimType2", ClaimValue = "some other value" };
				var role = new IdentityRole("Create_role_returns_Success");
				role.Claims.Add(claim1);
				role.Claims.Add(claim2);

				// act
				var result = await _roleStore.CreateAsync(role);

				// assert
				IdentityResultAssert.IsSuccess(result);

				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				IdentityRoleAssert.Equal(role, roleFromDb);
			}


			[Fact]
			public async Task Creating_same_role_twice_returns_DuplicateRoleName_error()
			{
				// arrange
				var role = new IdentityRole("Creating_same_role_twice_returns_DuplicateRoleName_error");

				// act
				var result1 = await _roleStore.CreateAsync(role);

				role.Name = "a different name, but same Id";
				var result2 = await _roleStore.CreateAsync(role);

				// assert
				IdentityResultAssert.IsSuccess(result1);

				var expectedError = IdentityResult.Failed(_errorDescriber.DuplicateRoleName(role.ToString()));
				IdentityResultAssert.IsFailure(result2, expectedError.Errors.FirstOrDefault());
			}

			[Fact]
			public async Task Creating_two_different_roles_but_same_Name_returns_DuplicateRoleName_error()
			{
				// arrange
				var role1 = new IdentityRole("Creating_two_different_roles_but_same_Name_returns_DuplicateRoleName_error");
				var role2 = new IdentityRole(role1.Name);

				// act
				var result1 = await _roleStore.CreateAsync(role1);
				var result2 = await _roleStore.CreateAsync(role2);

				// assert
				IdentityResultAssert.IsSuccess(result1);

				var expectedError = IdentityResult.Failed(_errorDescriber.DuplicateRoleName(role2.ToString()));
				IdentityResultAssert.IsFailure(result2, expectedError.Errors.FirstOrDefault());
			}
		}

		public class UpdateAsyncMethod : RoleStoreTests
		{
			public UpdateAsyncMethod() : base(typeof(UpdateAsyncMethod).Name) { }

			[Fact]
			public async Task Update_role_returns_Success()
			{
				// arrange
				var claim1 = new IdentityClaim { ClaimType = "ClaimType1", ClaimValue = "some value" };
				var claim2 = new IdentityClaim { ClaimType = "ClaimType2", ClaimValue = "some other value" };
				var role = new IdentityRole("Update_role_returns_Success");
				role.Claims.Add(claim1);

				// initial role creation
				await _roleStore.CreateAsync(role);
				role.Name = role.Name + " different";
				role.Claims.Add(claim2);


				// act
				var result = await _roleStore.UpdateAsync(role);

				// assert
				IdentityResultAssert.IsSuccess(result);

				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				IdentityRoleAssert.Equal(role, roleFromDb);
			}


			[Fact]
			public async Task Update_role_that_does_not_already_exists_inserts_and_returns_Success()
			{
				// arrange
				var role = new IdentityRole("Update_role_that_does_not_already_exists_inserts_and_returns_Success");


				// act
				var result = await _roleStore.UpdateAsync(role);

				// assert
				IdentityResultAssert.IsSuccess(result);

				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				IdentityRoleAssert.Equal(role, roleFromDb);
			}


			[Fact]
			public async Task Can_update_role_multiple_times()
			{
				// arrange
				var role = new IdentityRole("Can_update_role_multiple_times");
				await _roleStore.CreateAsync(role);

				// act
				role.Claims.Add(new IdentityClaim { ClaimType = "ClaimType1", ClaimValue = "claim value" });
				var result1 = await _roleStore.UpdateAsync(role);

				role.Name = role.Name + " different";
				var result2 = await _roleStore.UpdateAsync(role);

				// assert
				IdentityResultAssert.IsSuccess(result1);
				IdentityResultAssert.IsSuccess(result2);

				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				IdentityRoleAssert.Equal(role, roleFromDb);
			}

			[Fact]
			public async Task Updating_role_name_to_existing_name_returns_DuplicateRoleName_error()
			{
				// arrange
				var role1 = new IdentityRole("Updating_role_name_to_existing_name_returns_DuplicateRoleName_error");
				var role2 = new IdentityRole("Updating_role_name_to_existing_name_returns_DuplicateRoleName_error different");

				await _roleStore.CreateAsync(role1);
				await _roleStore.CreateAsync(role2);

				// act
				role2.Name = role1.Name;
				var result3= await _roleStore.UpdateAsync(role2);

				// assert
				var expectedError = IdentityResult.Failed(_errorDescriber.DuplicateRoleName(role2.ToString()));
				IdentityResultAssert.IsFailure(result3,expectedError.Errors.FirstOrDefault());
			}
		}

		public class DeleteAsyncMethod : RoleStoreTests
		{
			public DeleteAsyncMethod() : base(typeof(DeleteAsyncMethod).Name) { }

			[Fact]
			public async Task Delete_role_returns_Success()
			{
				// arrange
				var role = new IdentityRole("Delete_role_returns_Success");
				await _roleStore.CreateAsync(role);
				

				// act
				var result = await _roleStore.DeleteAsync(role);


				// assert
				IdentityResultAssert.IsSuccess(result);

				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				Assert.Null(roleFromDb);
			}


			[Fact]
			public async Task Delete_role_that_does_not_exist_returns_Success()
			{
				// arrange
				var role = new IdentityRole("Delete_role_that_does_not_exist_returns_Success");


				// act
				var result = await _roleStore.DeleteAsync(role);


				// assert
				IdentityResultAssert.IsSuccess(result);

				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				Assert.Null(roleFromDb);
			}
		}

		public class FindByIdAsyncMethod : RoleStoreTests
		{
			public FindByIdAsyncMethod() : base(typeof(FindByIdAsyncMethod).Name) { }

			[Fact]
			public async Task Unknown_roleId_returns_null()
			{
				// arrange
				var roleId = "unknown roleId";

				// act
				var result = await _roleStore.FindByIdAsync(roleId);

				// assert
				Assert.Null(result);
			}

			[Fact]
			public async Task Known_roleId_returns_IdentityRole()
			{
				// arrange
				var role = new IdentityRole("Known_roleId_returns_IdentityRole");
				await _roleStore.CreateAsync(role);

				// act
				var result = await _roleStore.FindByIdAsync(role.Id);

				// assert
				IdentityRoleAssert.Equal(role, result);
			}
		}

		public class FindByNameAsyncMethod : RoleStoreTests
		{
			public FindByNameAsyncMethod() : base(typeof(FindByNameAsyncMethod).Name) { }

			[Fact]
			public async Task Unknown_normalizedRoleName_returns_null()
			{
				// arrange
				var name = "unknown normalised name";

				// act
				var result = await _roleStore.FindByNameAsync(name);

				// assert
				Assert.Null(result);
			}

			[Fact]
			public async Task Known_normalizedRoleName_returns_IdentityRole()
			{
				// arrange
				var role = new IdentityRole("Known_normalizedRoleName_returns_IdentityRole");
				role.NormalizedName = role.Name;
				await _roleStore.CreateAsync(role);

				// act
				var result = await _roleStore.FindByNameAsync(role.NormalizedName);

				// assert
				IdentityRoleAssert.Equal(role, result);
			}
		}

		public class AddClaimAsyncMethod : RoleStoreTests
		{
			public AddClaimAsyncMethod() : base(typeof(AddClaimAsyncMethod).Name) { }

			[Fact]
			public async Task Adding_new_claim_to_role_updates_database_role_record()
			{
				// arrange
				var claim = new Claim("ClaimType", "some value");

				var role = new IdentityRole("Adding_new_claim_to_role_updates_database_role_record");
				await _roleStore.CreateAsync(role);
				
				// act
				await _roleStore.AddClaimAsync(role, claim);

				// assert

				// check role claims from memory
				var identityClaim = new IdentityClaim { ClaimType = claim.Type, ClaimValue = claim.Value};
				IdentityClaimAssert.Equal(new List<IdentityClaim> {identityClaim}, role.Claims);

				// check role claims from DB
				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim }, roleFromDb.Claims);
			}

			[Fact]
			public async Task Adding_new_claim_to_role_with_null_claims_updates_database_role_record()
			{
				// arrange
				var claim = new Claim("ClaimType", "some value");

				var role = new IdentityRole("Adding_new_claim_to_role_with_null_claims_updates_database_role_record");
				role.Claims = null;

				await _roleStore.CreateAsync(role);

				// act
				await _roleStore.AddClaimAsync(role, claim);

				// assert

				// check role claims from memory
				var identityClaim = new IdentityClaim { ClaimType = claim.Type, ClaimValue = claim.Value };
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim }, role.Claims);

				// check role claims from DB
				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim }, roleFromDb.Claims);
			}

			[Fact]
			public async Task Adding_existing_claim_to_role_does_not_update_database_role_record()
			{
				// arrange
				var claim = new Claim("ClaimType", "some value");

				var role = new IdentityRole("Adding_existing_claim_to_role_does_not_update_database_role_record");
				var identityClaim = new IdentityClaim { ClaimType = claim.Type, ClaimValue = claim.Value };
				role.Claims.Add(identityClaim);
				await _roleStore.CreateAsync(role);

				// act
				await _roleStore.AddClaimAsync(role, claim);

				// assert

				// check role claims from memory
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim }, role.Claims);

				// check role claims from DB
				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim }, roleFromDb.Claims);
			}

			[Fact]
			public async Task Adding_multiple_claims_with_same_ClaimType_adds_multiple_claims_to_database()
			{
				// arrange
				var claim1 = new Claim("ClaimType", "some value");
				var claim2 = new Claim(claim1.Type, "some other value");

				var role = new IdentityRole("Adding_multiple_claims_with_same_ClaimType_adds_multiple_claims_to_database");
				await _roleStore.CreateAsync(role);

				// act
				await _roleStore.AddClaimAsync(role, claim1);
				await _roleStore.AddClaimAsync(role, claim2);

				// assert
				var identityClaim1 = new IdentityClaim { ClaimType = claim1.Type, ClaimValue = claim1.Value };
				var identityClaim2 = new IdentityClaim { ClaimType = claim2.Type, ClaimValue = claim2.Value };

				// check role claims from memory
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim1, identityClaim2 }, role.Claims);

				// check role claims from DB
				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim1, identityClaim2 }, roleFromDb.Claims);
			}
		}

		public class RemoveClaimAsyncMethod : RoleStoreTests
		{
			public RemoveClaimAsyncMethod() : base(typeof(RemoveClaimAsyncMethod).Name) { }

			[Fact]
			public async Task Removing_unknown_claim_does_not_change_database_role_record()
			{
				// arrange
				var identityClaim = new IdentityClaim { ClaimType = "claim type", ClaimValue = "some value" };
				var role = new IdentityRole("Removing_unknown_claim_does_not_change_database_role_record");
				role.Claims.Add(identityClaim);

				await _roleStore.CreateAsync(role);

				// act
				var claim = new Claim("other type", "some other value");
				await _roleStore.RemoveClaimAsync(role, claim);

				// assert

				// check role claims from memory
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim }, role.Claims);

				// check role claims from DB
				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim }, roleFromDb.Claims);
			}

			[Fact]
			public async Task Removing_claim_from_null_role_claims_does_not_change_database_role_record()
			{
				// arrange
				var role = new IdentityRole("Removing_unknown_claim_and_role_claims_is_null_does_not_change_database_role_record");
				role.Claims = null;

				await _roleStore.CreateAsync(role);

				// act
				var claim = new Claim("other type", "some other value");
				await _roleStore.RemoveClaimAsync(role, claim);

				// assert

				// check role claims from memory
				Assert.Equal(0, role.Claims.Count);

				// check role claims from DB
				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				Assert.Equal(0, roleFromDb.Claims.Count);
			}

			[Fact]
			public async Task Remove_existing_claim_updates_database_role_record()
			{
				// arrange
				var claim1 = new Claim("ClaimType", "some value");
				var claim2 = new Claim("ClaimType2", "some other value");
				var identityClaim1 = new IdentityClaim { ClaimType = claim1.Type, ClaimValue = claim1.Value };
				var identityClaim2 = new IdentityClaim { ClaimType = claim2.Type, ClaimValue = claim2.Value };

				var role = new IdentityRole("Remove_existing_claim_updates_database_role_record");
				role.Claims.Add(identityClaim1);
				role.Claims.Add(identityClaim2);
				await _roleStore.CreateAsync(role);

				// act
				await _roleStore.RemoveClaimAsync(role, claim1);

				// assert

				// check role claims from memory
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim2 }, role.Claims);

				// check role claims from DB
				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim2 }, roleFromDb.Claims);
			}

			[Fact]
			public async Task Role_has_multiple_claims_with_same_ClaimType_removing_only_removes_cliam_with_same_value()
			{
				// arrange
				var claim1 = new Claim("ClaimType", "some value");
				var claim2 = new Claim(claim1.Type, "some other value");
				var identityClaim1 = new IdentityClaim { ClaimType = claim1.Type, ClaimValue = claim1.Value };
				var identityClaim2 = new IdentityClaim { ClaimType = claim2.Type, ClaimValue = claim2.Value };

				var role = new IdentityRole("Role_has_multiple_claims_with_same_ClaimType_removing_only_removes_cliam_with_same_value");
				role.Claims.Add(identityClaim1);
				role.Claims.Add(identityClaim2);
				await _roleStore.CreateAsync(role);

				// act
				await _roleStore.RemoveClaimAsync(role, claim1);

				// assert

				// check role claims from memory
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim2 }, role.Claims);

				// check role claims from DB
				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim2 }, roleFromDb.Claims);
			}
		}

		#region HELPER CLASS

		class RoleStoreHelper : RoleStore<IdentityRole>
		{
			public RoleStoreHelper(string connectionString, string databaseName = null, string collectionName = null) : base(connectionString, databaseName, collectionName) { }

			public RoleStoreHelper(IMongoClient client, string databaseName = null, string collectionName = null) : base(client, databaseName, collectionName) { }

			public RoleStoreHelper(IMongoDatabase database, string collectionName = null) : base(database, collectionName) { }

			public RoleStoreHelper(IMongoCollection<IdentityRole> collection) : base(collection) { }


			#region helper methods to get protected fields

			public string DatabaseName => base._databaseName;
			public string CollectionName => base._collectionName;
			public IMongoClient MongoClient => base._client;
			public IMongoDatabase MongoDatabase => base._database;
			public IMongoCollection<IdentityRole> MongoCollection => base._collection;

			#endregion
		}

		#endregion
	}
}
