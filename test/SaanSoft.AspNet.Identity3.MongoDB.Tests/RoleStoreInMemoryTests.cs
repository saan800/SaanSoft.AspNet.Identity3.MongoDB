using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public class RoleStoreInMemoryTests
	{
		private readonly RoleStore<IdentityUser, IdentityRole> _roleStore;

		public RoleStoreInMemoryTests()
		{
			var databaseContext = new IdentityDatabaseContext();
			_roleStore = new RoleStore<IdentityUser, IdentityRole>(databaseContext);
		}

		public class Misc : RoleStoreInMemoryTests
		{
			[Fact]
			public void Constructors_throw_ArgumentNullException_with_null()
			{
				Assert.Throws<ArgumentNullException>("databaseContext", () => new RoleStore<IdentityUser, IdentityRole>((IdentityDatabaseContext)null));
				Assert.Throws<ArgumentNullException>("databaseContext", () => new RoleStore<IdentityUser, IdentityRole>((IdentityDatabaseContext)null, null, new IdentityErrorDescriber()));
			}

			[Fact]
			public async Task Methods_throw_ObjectDisposedException_when_disposed()
			{
				_roleStore.Dispose();
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _roleStore.FindByIdAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _roleStore.FindByNameAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _roleStore.GetRoleIdAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _roleStore.GetRoleNameAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _roleStore.SetRoleNameAsync(null, null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _roleStore.CreateAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _roleStore.UpdateAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _roleStore.DeleteAsync(null));
				
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _roleStore.GetClaimsAsync(null));
				
			}

			[Fact]
			public async Task Methods_throw_ArgumentNullException_when_null_arguments()
			{
				await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await _roleStore.GetRoleIdAsync(null));
				await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await _roleStore.GetRoleNameAsync(null));
				await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await _roleStore.SetRoleNameAsync(null, null));
				await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await _roleStore.CreateAsync(null));
				await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await _roleStore.UpdateAsync(null));
				await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await _roleStore.DeleteAsync(null));

				await Assert.ThrowsAsync<ArgumentNullException>("role", async () => await _roleStore.GetClaimsAsync(null));
			}
		}

		public class GetRoleIdAsyncMethod : RoleStoreInMemoryTests
		{
			[Fact]
			public async Task Returns_Id_from_Role_as_string()
			{
				// arrange
				var role = new IdentityRole("Returns_Id_from_Role_as_string");

				// act
				var result = await _roleStore.GetRoleIdAsync(role);

				// assert
				Assert.Equal(role.Id, result);
			}

			[Fact]
			public async Task Null_id_on_role_returns_Null()
			{
				// arrange
				var role = new IdentityRole("Null_id_on_role_returns_Null");
				role.Id = null;

				// act
				var result = await _roleStore.GetRoleIdAsync(role);

				// assert
				Assert.Null(result);
			}
		}

		public class GetRoleNameAsyncMethod : RoleStoreInMemoryTests
		{
			[Theory]
			[InlineData(null)]
			[InlineData("")]
			[InlineData("Returns_name_from_Role")]
			public async Task Returns_name_from_Role(string roleName)
			{
				// arrange
				var role = new IdentityRole(roleName);

				// act
				var result = await _roleStore.GetRoleNameAsync(role);

				// assert
				Assert.Equal(role.Name, result);
				Assert.Equal(roleName, result);
			}
		}

		public class GetNormalizedRoleNameAsyncMethod : RoleStoreInMemoryTests
		{
			[Theory]
			[InlineData(null)]
			[InlineData("")]
			[InlineData("Returns_normalized_name_from_Role")]
			public async Task Returns_normalized_name_from_Role(string roleName)
			{
				// arrange
				var role = new IdentityRole(roleName);

				// act
				var result = await _roleStore.GetNormalizedRoleNameAsync(role);

				// assert
				if (string.IsNullOrWhiteSpace(roleName))
				{
					Assert.Equal(role.Name, result);
				}
				else
				{	
					Assert.Equal(roleName.ToLower(), result);
				}
			}
		}

		public class SetNormalizedRoleNameAsyncMethod : RoleStoreInMemoryTests
		{
			[Theory]
			[InlineData(null)]
			[InlineData("")]
			[InlineData("normalised name value")]
			public async Task Sets_role_normalisedName_to_supplied_value(string normalisedName)
			{
				// arrange
				var role = new IdentityRole("Sets_role_normalisedName_to_supplied_value");

				// act
				await _roleStore.SetNormalizedRoleNameAsync(role, normalisedName); 

				// assert
				Assert.Equal(normalisedName, role.NormalizedName);
			}
		}

		public class FindByIdAsyncMethod : RoleStoreInMemoryTests
		{
			[Theory]
			[InlineData(null)]
			[InlineData("")]
			public async Task Find_with_blank_roleId_returns_null(string roleId)
			{
				// act
				var result = await _roleStore.FindByIdAsync(roleId);

				// assert
				Assert.Null(result);
			}
		}

		public class FindByNameAsyncMethod : RoleStoreInMemoryTests
		{
			[Theory]
			[InlineData(null)]
			[InlineData("")]
			public async Task Find_with_blank_normalisedName_returns_null(string normalisedName)
			{
				// act
				var result = await _roleStore.FindByNameAsync(normalisedName);

				// assert
				Assert.Null(result);
			}
		}

		public class SetRoleNameAsyncMethod : RoleStoreInMemoryTests
		{
			[Theory]
			[InlineData(null)]
			[InlineData("")]
			[InlineData("Sets_role_name_to_provided_value")]
			public async Task Sets_role_name_to_supplied_value(string roleName)
			{
				// arrange
				var role = new IdentityRole();

				// act
				await _roleStore.SetRoleNameAsync(role, roleName);

				// assert
				Assert.Equal(roleName, role.Name);
			}
		}

		public class GetClaimsAsyncMethod : RoleStoreInMemoryTests
		{
			[Fact]
			public async Task Returns_empty_list_when_claims_on_role_not_set()
			{
				// arrange
				var role = new IdentityRole();

				// act
				var result = await _roleStore.GetClaimsAsync(role);

				// assert
				Assert.Empty(result);
			}


			[Fact]
			public async Task Returns_empty_list_when_claims_on_role_is_null()
			{
				// arrange
				var role = new IdentityRole { Claims = null };

				// act
				var result = await _roleStore.GetClaimsAsync(role);

				// assert
				Assert.Empty(result);
			}

			[Fact]
			public async Task Returns_list_of_claims_from_role()
			{
				// arrange
				var role = new IdentityRole();
				var claim1 = new IdentityClaim { ClaimType = "ClaimType1", ClaimValue = "some value" };
				var claim2 = new IdentityClaim { ClaimType = "ClaimType2", ClaimValue = "some other value" };
				role.Claims.Add(claim1);
				role.Claims.Add(claim2);


				// act
				var result = await _roleStore.GetClaimsAsync(role);

				// assert
				Assert.Equal(role.Claims.Count, result.Count);
				Assert.True(result.Single(c => c.Type == claim1.ClaimType && c.Value == claim1.ClaimValue) != null);
				Assert.True(result.Single(c => c.Type == claim2.ClaimType && c.Value == claim2.ClaimValue) != null);

			}
		}
	}
}
