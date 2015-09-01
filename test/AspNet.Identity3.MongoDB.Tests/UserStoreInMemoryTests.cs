using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Xunit;

namespace AspNet.Identity3.MongoDB.Tests
{
	public class UserStoreInMemoryTests
	{
		private readonly UserStore<IdentityUser, IdentityRole> _userStore;

		public UserStoreInMemoryTests()
		{
			var databaseContext = new IdentityDatabaseContext();
			// "mongodb://localhost:27017"
			_userStore = new UserStore<IdentityUser, IdentityRole>(databaseContext);
		}

		public class Misc : UserStoreInMemoryTests
		{
			[Fact]
			public void Constructors_throw_ArgumentNullException_with_null()
			{
				Assert.Throws<ArgumentNullException>("databaseContext", () => new UserStore<IdentityUser, IdentityRole>((IdentityDatabaseContext)null));
				Assert.Throws<ArgumentNullException>("databaseContext", () => new UserStore<IdentityUser, IdentityRole>((IdentityDatabaseContext)null, new IdentityErrorDescriber()));
			}

			[Fact]
			public async Task Methods_throw_ObjectDisposedException_when_disposed()
			{
				_userStore.Dispose();
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.GetUserIdAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.GetUserNameAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.SetUserNameAsync(null, null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.GetNormalizedUserNameAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.SetNormalizedUserNameAsync(null, null));

				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.FindByIdAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.FindByNameAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.CreateAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.UpdateAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.DeleteAsync(null));

				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.GetClaimsAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.AddClaimsAsync(null, null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.ReplaceClaimAsync(null, null, null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.RemoveClaimsAsync(null, null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.GetUsersForClaimAsync(null));

				// TODO:
				// AddLoginAsync

			}

			[Fact]
			public async Task Methods_throw_ArgumentNullException_when_null_arguments()
			{
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.GetUserIdAsync(null));
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.GetUserNameAsync(null));
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.SetUserNameAsync(null, "user name"));
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.GetNormalizedUserNameAsync(null));
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.SetNormalizedUserNameAsync(null, "normalised user name"));
				
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.CreateAsync(null));
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.UpdateAsync(null));
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.DeleteAsync(null));

				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.GetClaimsAsync(null));
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.AddClaimsAsync(null, new List<Claim>()));
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.ReplaceClaimAsync(null, new Claim("type", "value"), new Claim("new type", "new value")));
				await Assert.ThrowsAsync<ArgumentNullException>("claim", async () => await _userStore.ReplaceClaimAsync(new IdentityUser("Bob"), null, new Claim("new type", "new value")));
				await Assert.ThrowsAsync<ArgumentNullException>("newClaim", async () => await _userStore.ReplaceClaimAsync(new IdentityUser("Bob"), new Claim("type", "value"), null));
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.RemoveClaimsAsync(null, new List<Claim>()));
				await Assert.ThrowsAsync<ArgumentNullException>("claim", async () => await _userStore.GetUsersForClaimAsync(null));



				// TODO:
				// AddLoginAsync - check user and login
			}
		}

		public class GetUserIdAsyncMethod : UserStoreInMemoryTests
		{
			[Fact]
			public async Task Returns_Id_from_User_as_string()
			{
				// arrange
				var user = new IdentityUser("Returns_Id_from_User_as_string");

				// act
				var result = await _userStore.GetUserIdAsync(user);

				// assert
				Assert.Equal(user.Id, result);
			}

			[Fact]
			public async Task Null_id_on_user_returns_Null()
			{
				// arrange
				var user = new IdentityUser("Null_id_on_user_returns_Null");
				user.Id = null;

				// act
				var result = await _userStore.GetUserIdAsync(user);

				// assert
				Assert.Null(result);
			}
		}

		public class GetUserNameAsyncMethod : UserStoreInMemoryTests
		{
			[Theory]
			[InlineData(null)]
			[InlineData("")]
			[InlineData("Returns_UserName_from_user")]
			public async Task Returns_UserName_from_user(string userName)
			{
				// arrange
				var user = new IdentityUser(userName);

				// act
				var result = await _userStore.GetUserNameAsync(user);

				// assert
				Assert.Equal(user.UserName, result);
				Assert.Equal(userName, result);
			}
		}

		public class SetUserNameAsyncMethod : UserStoreInMemoryTests
		{
			[Theory]
			[InlineData(null)]
			[InlineData("")]
			[InlineData("Sets_UserName_to_supplied_value")]
			public async Task Sets_UserName_to_supplied_value(string userName)
			{
				// arrange
				var user = new IdentityUser();

				// act
				await _userStore.SetUserNameAsync(user, userName);

				// assert
				Assert.Equal(userName, user.UserName);
			}
		}
		
		public class GetNormalizedUserNameAsyncMethod : UserStoreInMemoryTests
		{
			[Theory]
			[InlineData(null)]
			[InlineData("")]
			[InlineData("Returns_NormalizedUserName_from_user")]
			public async Task Returns_NormalizedUserName_from_user(string normalizedUuserName)
			{
				// arrange
				var user = new IdentityUser();
				user.NormalizedUserName = normalizedUuserName;

				// act
				var result = await _userStore.GetNormalizedUserNameAsync(user);

				// assert
				Assert.Equal(user.NormalizedUserName, result);
				Assert.Equal(normalizedUuserName, result);
			}
		}

		public class SetNormalizedUserNameAsyncMethod : UserStoreInMemoryTests
		{
			[Theory]
			[InlineData(null)]
			[InlineData("")]
			[InlineData("Sets_UserName_to_supplied_value")]
			public async Task Sets_UserName_to_supplied_value(string normalizedUuserName)
			{
				// arrange
				var user = new IdentityUser();

				// act
				await _userStore.SetNormalizedUserNameAsync(user, normalizedUuserName);

				// assert
				Assert.Equal(normalizedUuserName, user.NormalizedUserName);
			}
		}

		public class GetClaimsAsyncMethod : UserStoreInMemoryTests
		{
			[Fact]
			public async Task Returns_empty_list_when_claims_on_user_not_set()
			{
				// arrange
				var user = new IdentityUser();

				// act
				var result = await _userStore.GetClaimsAsync(user);

				// assert
				Assert.Empty(result);
			}


			[Fact]
			public async Task Returns_empty_list_when_claims_on_user_is_null()
			{
				// arrange
				var user = new IdentityUser { Claims = null };

				// act
				var result = await _userStore.GetClaimsAsync(user);

				// assert
				Assert.Empty(result);
			}

			[Fact]
			public async Task Returns_list_of_claims_from_user()
			{
				// arrange
				var user = new IdentityUser();
				var claim1 = new IdentityClaim { ClaimType = "ClaimType1", ClaimValue = "some value" };
				var claim2 = new IdentityClaim { ClaimType = "ClaimType2", ClaimValue = "some other value" };
				user.Claims.Add(claim1);
				user.Claims.Add(claim2);
				
				// act
				var result = await _userStore.GetClaimsAsync(user);

				// assert
				Assert.Equal(user.Claims.Count, result.Count);
				Assert.True(result.Single(c => c.Type == claim1.ClaimType && c.Value == claim1.ClaimValue) != null);
				Assert.True(result.Single(c => c.Type == claim2.ClaimType && c.Value == claim2.ClaimValue) != null);

			}
			
			[Fact]
			public async Task Returns_all_claims_from_users_claims_and_roles()
			{
				// arrange
				var user = new IdentityUser();
				var claim1 = new IdentityClaim { ClaimType = "ClaimType1", ClaimValue = "some value" };
				var claim2 = new IdentityClaim { ClaimType = "ClaimType2", ClaimValue = "some other value" };
				var claim3 = new IdentityClaim { ClaimType = "ClaimType3", ClaimValue = "yet another value" };
				user.Claims.Add(claim1);
				user.Claims.Add(claim2);

				var role = new IdentityRole();
				role.Claims.Add(claim1);
				role.Claims.Add(claim3);
				user.Roles.Add(role);
				
				// act
				var result = await _userStore.GetClaimsAsync(user);

				// assert
				Assert.Equal(3, result.Count);
				Assert.True(result.Single(c => c.Type == claim1.ClaimType && c.Value == claim1.ClaimValue) != null);
				Assert.True(result.Single(c => c.Type == claim2.ClaimType && c.Value == claim2.ClaimValue) != null);
				Assert.True(result.Single(c => c.Type == claim3.ClaimType && c.Value == claim3.ClaimValue) != null);

			}
		}
	}
}
