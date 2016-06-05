using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Xunit;
using Moq;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public class UserStoreInMemoryTests
	{
		private readonly UserStore<IdentityUser, IdentityRole> _userStore;
		protected Mock<IdentityDatabaseContext> MockDatabaseContext;

		public UserStoreInMemoryTests()
		{
			MockDatabaseContext = new Mock<IdentityDatabaseContext>();
			_userStore = new UserStore<IdentityUser, IdentityRole>(MockDatabaseContext.Object);
		}

		public class Misc : UserStoreInMemoryTests
		{
			[Fact]
			public void Constructors_throw_ArgumentNullException_with_null()
			{
				Assert.Throws<ArgumentNullException>("databaseContext", () => new UserStore<IdentityUser, IdentityRole>((IdentityDatabaseContext)null));
				Assert.Throws<ArgumentNullException>("databaseContext", () => new UserStore<IdentityUser, IdentityRole>((IdentityDatabaseContext)null, null, new IdentityErrorDescriber()));
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
				
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.AddLoginAsync(null, null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.RemoveLoginAsync(null, null, null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.GetLoginsAsync(null));
				await Assert.ThrowsAsync<ObjectDisposedException>(async () => await _userStore.FindByLoginAsync(null, null));


				// TODO
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


				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.AddLoginAsync(null, new UserLoginInfo("", "", "")));
				await Assert.ThrowsAsync<ArgumentNullException>("login", async () => await _userStore.AddLoginAsync(new IdentityUser("Bob"),  null));
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.RemoveLoginAsync(null, "provider", "key"));
				await Assert.ThrowsAsync<ArgumentNullException>("user", async () => await _userStore.GetLoginsAsync(null));



				// TODO:
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
			public async Task Returns_NormalizedUserName_from_user(string normalizedUserName)
			{
				// arrange
				var user = new IdentityUser(normalizedUserName);

				// act
				var result = await _userStore.GetNormalizedUserNameAsync(user);

				// assert
				if (string.IsNullOrWhiteSpace(normalizedUserName))
				{
					Assert.Equal(normalizedUserName, result);
				}
				else
				{
					Assert.Equal(normalizedUserName.ToLower(), result);
				}
			}
		}

		public class SetNormalizedUserNameAsyncMethod : UserStoreInMemoryTests
		{
			[Theory]
			[InlineData(null)]
			[InlineData("")]
			[InlineData("Sets_UserName_to_supplied_value")]
			public async Task Sets_UserName_to_supplied_value(string normalizedUserName)
			{
				// arrange
				var user = new IdentityUser();

				// act
				await _userStore.SetNormalizedUserNameAsync(user, normalizedUserName);

				// assert
				if (string.IsNullOrWhiteSpace(normalizedUserName))
				{
					Assert.Equal(normalizedUserName, user.NormalizedUserName);
				}
				else
				{
					Assert.Equal(normalizedUserName.ToLower(), user.NormalizedUserName);
				}
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

		public class RemoveLoginAsyncMethod : UserStoreInMemoryTests
		{
			[Fact]
			public async Task Removing_login_details_that_dont_exist_should_not_update_user_or_database()
			{
				// arrange
				var user = new IdentityUser();
				var login = new UserLoginInfo("a provider", "key", "john smith");
				user.Logins.Add(login);


				// act
				await _userStore.RemoveLoginAsync(user, login.LoginProvider + "different", login.ProviderKey + "different");

				// assert
				Assert.Equal(1, user.Logins.Count);
				Assert.Equal(login, user.Logins.First());

				// check no db access
				MockDatabaseContext.Verify(x => x.UserCollection, Times.Never);
			}

			[Fact]
			public async Task Removing_provider_that_exists_but_key_that_does_not_exist_should_not_update_user_or_database()
			{
				// arrange
				var user = new IdentityUser();
				var login = new UserLoginInfo("a provider", "key", "john smith");
				user.Logins.Add(login);


				// act
				await _userStore.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey + "different");

				// assert
				Assert.Equal(1, user.Logins.Count);
				Assert.Equal(login, user.Logins.First());

				// check no db access
				MockDatabaseContext.Verify(x => x.UserCollection, Times.Never);
			}

			[Fact]
			public async Task Removing_provider_that_does_not_exists_but_key_that_does_exist_should_not_update_user_or_database()
			{
				// arrange
				var user = new IdentityUser();
				var login = new UserLoginInfo("a provider", "key", "john smith");
				user.Logins.Add(login);


				// act
				await _userStore.RemoveLoginAsync(user, login.LoginProvider + "different", login.ProviderKey);

				// assert
				Assert.Equal(1, user.Logins.Count);
				Assert.Equal(login, user.Logins.First());

				// check no db access
				MockDatabaseContext.Verify(x => x.UserCollection, Times.Never);
			}
		}


		public class GetLoginsAsyncMethod : UserStoreInMemoryTests
		{
			[Fact]
			public async Task If_user_has_no_login_details_should_return_empty_list()
			{
				// arrange
				var user = new IdentityUser();

				// act
				var result = await _userStore.GetLoginsAsync(user);

				// assert
				Assert.NotNull(result);
				Assert.Empty(result);
			}


			[Fact]
			public async Task If_user_has_login_details_should_return_all_in_list()
			{
				// arrange
				var user = new IdentityUser();
				var login1 = new UserLoginInfo("a provider", "key", "john smith");
				var login2 = new UserLoginInfo("a different provider", "key2", "john smith");
				user.Logins.Add(login1);
				user.Logins.Add(login2);

				// act
				var result = await _userStore.GetLoginsAsync(user);

				// assert
				Assert.NotNull(result);
				Assert.Equal(2, result.Count);
			}
		}
	}
}
