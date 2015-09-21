using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MongoDB.Driver;
using Xunit;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public class RoleStoreTests : IDisposable
	{
		private readonly DatabaseFixture _databaseFixture;
		private readonly IMongoCollection<IdentityUser> _userCollection;
		private readonly IMongoCollection<IdentityRole> _roleCollection;
		private readonly IdentityDatabaseContext _databaseContext;

		private readonly UserStore<IdentityUser, IdentityRole> _userStore;
		private readonly RoleStore<IdentityUser, IdentityRole> _roleStore;
		private readonly IdentityErrorDescriber _errorDescriber;

		public RoleStoreTests(string collectionPrefix)
		{
			collectionPrefix = $"{typeof(RoleStoreTests).Name}_{collectionPrefix}";

			_databaseFixture = new DatabaseFixture(collectionPrefix);
			_userCollection = _databaseFixture.GetCollection<IdentityUser>();
			_roleCollection = _databaseFixture.GetCollection<IdentityRole>();
			_databaseContext = new IdentityDatabaseContext { UserCollection = _userCollection, RoleCollection = _roleCollection };

			_userStore = new UserStore<IdentityUser, IdentityRole>(_databaseContext);
			_roleStore = new RoleStore<IdentityUser, IdentityRole>(_databaseContext);
			_errorDescriber = new IdentityErrorDescriber();
		}

		public void Dispose()
		{
			_databaseFixture.Dispose();
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

			[Fact]
			public async Task Creating_two_different_roles_with_same_Name_different_casing_returns_DuplicateRoleName_error()
			{
				// arrange
				var role1 = new IdentityRole("Creating_two_different_roles_with_same_Name_different_casing_returns_DuplicateRoleName_error");
				var role2 = new IdentityRole(role1.Name.ToUpper());

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
				IdentityResultAssert.IsFailure(result3, expectedError.Errors.FirstOrDefault());
			}
			
			[Fact]
			public async Task When_User_has_Role_and_Role_is_updated_should_update_User_Role_as_well()
			{
				// arrange
				var identityClaim1 = new IdentityClaim { ClaimType = "ClaimType1", ClaimValue = "val" };
				var identityClaim2 = new IdentityClaim { ClaimType = "ClaimType2", ClaimValue = "some val" };
				var role1 = new IdentityRole { Name = "Role1", Claims = new List<IdentityClaim> { identityClaim1, identityClaim2 } };
				var role2 = new IdentityRole { Name = "Role2", Claims = new List<IdentityClaim> { identityClaim1 } };

				await _roleStore.CreateAsync(role1);
				await _roleStore.CreateAsync(role2);

				var user1 = new IdentityUser { UserName = "User1", Roles = new List<IdentityRole<string>> { role1, role2 } };
				var user2 = new IdentityUser { UserName = "User2", Roles = new List<IdentityRole<string>> { role1 } };
				var user3 = new IdentityUser { UserName = "User3", Roles = new List<IdentityRole<string>> { role2 } };

				await _userStore.CreateAsync(user1);
				await _userStore.CreateAsync(user2);
				await _userStore.CreateAsync(user3);

				// act
				role1.Name = "Bob";
				await _roleStore.UpdateAsync(role1);

				// assert
				var user1FromDb = await _userCollection.Find(x => x.Id == user1.Id).SingleOrDefaultAsync();
				var user2FromDb = await _userCollection.Find(x => x.Id == user2.Id).SingleOrDefaultAsync();
				var user3FromDb = await _userCollection.Find(x => x.Id == user3.Id).SingleOrDefaultAsync();

				// USER 1
				Assert.Equal(user1.Roles.Count, user1FromDb.Roles.Count);
				var user1FromDbRole1 = user1FromDb.Roles.Single(x => x.Id == role1.Id);
				Assert.Equal(role1.Name, user1FromDbRole1.Name);
				IdentityClaimAssert.Equal(role1.Claims, user1FromDbRole1.Claims);

				var user1FromDbRole2 = user1FromDb.Roles.Single(x => x.Id == role2.Id);
				IdentityRoleAssert.Equal(role2, (IdentityRole)user1FromDbRole2);


				// USER 2
				Assert.Equal(user2.Roles.Count, user2FromDb.Roles.Count);
				var user2FromDbRole1 = user2FromDb.Roles.Single();
				Assert.Equal(role1.Name, user2FromDbRole1.Name);
				IdentityClaimAssert.Equal(role1.Claims, user2FromDbRole1.Claims);


				// USER 3
				Assert.Equal(user3.Roles.Count, user3FromDb.Roles.Count);
				var user3FromDbRole2 = user3FromDb.Roles.Single();
				IdentityRoleAssert.Equal(role2, (IdentityRole)user3FromDbRole2);
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

			[Fact]
			public async Task When_User_has_Role_and_Role_is_deleted_should_remove_User_Role_as_well()
			{
				// arrange
				var identityClaim1 = new IdentityClaim { ClaimType = "ClaimType1", ClaimValue = "val" };
				var role1 = new IdentityRole { Name = "Role1", Claims = new List<IdentityClaim> { identityClaim1 } };
				var role2 = new IdentityRole { Name = "Role2", Claims = new List<IdentityClaim> { identityClaim1 } };

				await _roleStore.CreateAsync(role1);
				await _roleStore.CreateAsync(role2);

				var user1 = new IdentityUser { UserName = "User1", Roles = new List<IdentityRole<string>> { role1, role2 } };
				var user2 = new IdentityUser { UserName = "User2", Roles = new List<IdentityRole<string>> { role1 } };
				var user3 = new IdentityUser { UserName = "User3", Roles = new List<IdentityRole<string>> { role2 } };

				await _userStore.CreateAsync(user1);
				await _userStore.CreateAsync(user2);
				await _userStore.CreateAsync(user3);

				// act
				await _roleStore.DeleteAsync(role1);

				// assert
				var user1FromDb = await _userCollection.Find(x => x.Id == user1.Id).SingleOrDefaultAsync();
				var user2FromDb = await _userCollection.Find(x => x.Id == user2.Id).SingleOrDefaultAsync();
				var user3FromDb = await _userCollection.Find(x => x.Id == user3.Id).SingleOrDefaultAsync();

				// USER 1
				Assert.Equal(1, user1FromDb.Roles.Count);
				var user1FromDbRole2 = user1FromDb.Roles.Single(x => x.Id == role2.Id);
				IdentityRoleAssert.Equal(role2, (IdentityRole)user1FromDbRole2);


				// USER 2
				Assert.Equal(0, user2FromDb.Roles.Count);


				// USER 3
				Assert.Equal(1, user3FromDb.Roles.Count);
				var user3FromDbRole2 = user3FromDb.Roles.Single();
				IdentityRoleAssert.Equal(role2, (IdentityRole)user3FromDbRole2);
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
				await _roleStore.CreateAsync(role);

				// act
				var result = await _roleStore.FindByNameAsync(role.Name);

				// assert
				IdentityRoleAssert.Equal(role, result);
			}

			[Fact]
			public async Task Case_insensitive_normalizedRoleName_returns_IdentityRole()
			{
				// arrange
				var role = new IdentityRole("Case_insensitive_normalizedRoleName_returns_IdentityRole");
				await _roleStore.CreateAsync(role);

				// act
				var result = await _roleStore.FindByNameAsync(role.Name.ToUpper());

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
				var identityClaim = new IdentityClaim(claim);
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
				var identityClaim = new IdentityClaim(claim);
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
				var identityClaim = new IdentityClaim(claim);
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
				var identityClaim1 = new IdentityClaim(claim1);
				var identityClaim2 = new IdentityClaim(claim2);

				// check role claims from memory
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim1, identityClaim2 }, role.Claims);

				// check role claims from DB
				var roleFromDb = await _roleCollection.Find(x => x.Id == role.Id).SingleOrDefaultAsync();
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim1, identityClaim2 }, roleFromDb.Claims);
			}
			
			[Fact]
			public async Task When_User_has_Role_and_add_claim_to_Role_should_add_cliam_from_User_Role_as_well()
			{
				// arrange
				var identityClaim1 = new IdentityClaim { ClaimType = "ClaimType1", ClaimValue = "val" };
				var identityClaim2 = new IdentityClaim { ClaimType = "ClaimType2", ClaimValue = "some val" };
				var role1 = new IdentityRole { Name = "Role1", Claims = new List<IdentityClaim> { identityClaim1 } };
				var role2 = new IdentityRole { Name = "Role2", Claims = new List<IdentityClaim> { identityClaim1 } };

				await _roleStore.CreateAsync(role1);
				await _roleStore.CreateAsync(role2);

				var user1 = new IdentityUser { UserName = "User1", Roles = new List<IdentityRole<string>> { role1, role2 } };
				var user2 = new IdentityUser { UserName = "User2", Roles = new List<IdentityRole<string>> { role1 } };
				var user3 = new IdentityUser { UserName = "User3", Roles = new List<IdentityRole<string>> { role2 } };

				await _userStore.CreateAsync(user1);
				await _userStore.CreateAsync(user2);
				await _userStore.CreateAsync(user3);

				// act
				await _roleStore.AddClaimAsync(role1, new Claim(identityClaim2.ClaimType, identityClaim2.ClaimValue));

				// assert
				var user1FromDb = await _userCollection.Find(x => x.Id == user1.Id).SingleOrDefaultAsync();
				var user2FromDb = await _userCollection.Find(x => x.Id == user2.Id).SingleOrDefaultAsync();
				var user3FromDb = await _userCollection.Find(x => x.Id == user3.Id).SingleOrDefaultAsync();

				// USER 1
				Assert.Equal(user1.Roles.Count, user1FromDb.Roles.Count);
				var user1FromDbRole1 = user1FromDb.Roles.Single(x => x.Id == role1.Id);
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim1, identityClaim2 }, user1FromDbRole1.Claims);

				// we were specifically removing the identityClaim1 from role1 - so role2 should still have identityClaim1 in role2
				var user1FromDbRole2 = user1FromDb.Roles.Single(x => x.Id == role2.Id);
				IdentityClaimAssert.Equal(role2.Claims, user1FromDbRole2.Claims);


				// USER 2
				Assert.Equal(user2.Roles.Count, user2FromDb.Roles.Count);
				var user2FromDbRole1 = user2FromDb.Roles.Single();
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim1, identityClaim2 }, user2FromDbRole1.Claims);


				// USER 3
				Assert.Equal(user3.Roles.Count, user3FromDb.Roles.Count);
				// we were specifically removing the identityClaim1 from role1 - so role2 should still have identityClaim1 in role2
				var user3FromDbRole2 = user3FromDb.Roles.Single();
				IdentityClaimAssert.Equal(role2.Claims, user3FromDbRole2.Claims);
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
				var identityClaim1 = new IdentityClaim(claim1);
				var identityClaim2 = new IdentityClaim(claim2);

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
			public async Task Remove_existing_claim_different_casing_updates_database_role_record()
			{
				// arrange
				var claim1 = new Claim("ClaimType", "some value");
				var claim2 = new Claim("ClaimType2", "some other value");
				var identityClaim1 = new IdentityClaim { ClaimType = claim1.Type.ToUpper(), ClaimValue = claim1.Value.ToUpper() };
				var identityClaim2 = new IdentityClaim(claim2);

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
				var identityClaim1 = new IdentityClaim(claim1);
				var identityClaim2 = new IdentityClaim(claim2);

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

			[Fact]
			public async Task When_User_has_Role_with_claims_and_claim_removed_from_Role_should_remove_cliam_from_User_Role_as_well()
			{
				// arrange
				var identityClaim1 = new IdentityClaim { ClaimType = "ClaimType1", ClaimValue = "val" };
				var identityClaim2 = new IdentityClaim { ClaimType = "ClaimType2", ClaimValue = "some val" };
				var role1 = new IdentityRole { Name = "Role1", Claims = new List<IdentityClaim> { identityClaim1, identityClaim2 } };
				var role2 = new IdentityRole { Name = "Role2", Claims = new List<IdentityClaim> { identityClaim1 } };

				await _roleStore.CreateAsync(role1);
				await _roleStore.CreateAsync(role2);

				var user1 = new IdentityUser { UserName = "User1", Roles = new List<IdentityRole<string>> { role1, role2 } };
				var user2 = new IdentityUser { UserName = "User2", Roles = new List<IdentityRole<string>> { role1 } };
				var user3 = new IdentityUser { UserName = "User3", Roles = new List<IdentityRole<string>> { role2 } };

				await _userStore.CreateAsync(user1);
				await _userStore.CreateAsync(user2);
				await _userStore.CreateAsync(user3);

				// act
				await _roleStore.RemoveClaimAsync(role1, new Claim(identityClaim1.ClaimType, identityClaim1.ClaimValue));

				// assert
				var user1FromDb = await _userCollection.Find(x => x.Id == user1.Id).SingleOrDefaultAsync();
				var user2FromDb = await _userCollection.Find(x => x.Id == user2.Id).SingleOrDefaultAsync();
				var user3FromDb = await _userCollection.Find(x => x.Id == user3.Id).SingleOrDefaultAsync();

				// USER 1
				Assert.Equal(user1.Roles.Count, user1FromDb.Roles.Count);
				var user1FromDbRole1 = user1FromDb.Roles.Single(x => x.Id == role1.Id);
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim2 }, user1FromDbRole1.Claims);

				// we were specifically removing the identityClaim1 from role1 - so role2 should still have identityClaim1 in role2
				var user1FromDbRole2 = user1FromDb.Roles.Single(x => x.Id == role2.Id);
				IdentityClaimAssert.Equal(role2.Claims, user1FromDbRole2.Claims);


				// USER 2
				Assert.Equal(user2.Roles.Count, user2FromDb.Roles.Count);
				var user2FromDbRole1 = user2FromDb.Roles.Single();
				IdentityClaimAssert.Equal(new List<IdentityClaim> { identityClaim2 }, user2FromDbRole1.Claims);


				// USER 3
				Assert.Equal(user3.Roles.Count, user3FromDb.Roles.Count);
				// we were specifically removing the identityClaim1 from role1 - so role2 should still have identityClaim1 in role2
				var user3FromDbRole2 = user3FromDb.Roles.Single();
				IdentityClaimAssert.Equal(role2.Claims, user3FromDbRole2.Claims);
			}
		}
		
	}
}
