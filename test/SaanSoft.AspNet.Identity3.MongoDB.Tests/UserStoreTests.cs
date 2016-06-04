using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using Xunit;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public class UserStoreTests : IDisposable
	{
		#region Contructor, Dispose and helper properties

		private readonly DatabaseFixture _databaseFixture;
		private readonly IMongoCollection<IdentityUser> _userCollection;
		private readonly IMongoCollection<IdentityRole> _roleCollection;
		private readonly IdentityDatabaseContext _databaseContext;

		private readonly UserStore<IdentityUser, IdentityRole> _userStore;
		private readonly IdentityErrorDescriber _errorDescriber;

		private readonly Claim _claim1;
		private readonly Claim _claim2;
		private readonly Claim _claim3;

		private readonly Claim _claim1SameType;

		private readonly IdentityClaim _identityClaim1;
		private readonly IdentityClaim _identityClaim2;
		private readonly IdentityClaim _identityClaim3;

		private readonly IdentityClaim _identityClaim1SameType;

		public UserStoreTests(string collectionPrefix)
		{
			collectionPrefix = $"{typeof(UserStoreTests).Name}_{collectionPrefix}";

			_databaseFixture = new DatabaseFixture(collectionPrefix);
			_userCollection = _databaseFixture.GetCollection<IdentityUser>();
			_roleCollection = _databaseFixture.GetCollection<IdentityRole>();
			_databaseContext = new IdentityDatabaseContext { UserCollection = _userCollection, RoleCollection = _roleCollection };

			_errorDescriber = new IdentityErrorDescriber();
			_userStore = new UserStore<IdentityUser, IdentityRole>(_databaseContext, null, _errorDescriber);


			_claim1 = new Claim("ClaimType1", "some value");
			_claim2 = new Claim("ClaimType2", "some other value");
			_claim3 = new Claim("other type", "some other value");

			_claim1SameType = new Claim(_claim1.Type, _claim1.Value + " different");

			_identityClaim1 = new IdentityClaim(_claim1);
			_identityClaim2 = new IdentityClaim(_claim2);
			_identityClaim3 = new IdentityClaim(_claim3);

			_identityClaim1SameType = new IdentityClaim(_claim1SameType);
		}

		public void Dispose()
		{
			_databaseFixture.Dispose();
		}

		#endregion
		

		public class IUserStoreTests : UserStoreTests
		{
			public IUserStoreTests(string collectionPrefix) : base(collectionPrefix) { }

			public class CreateAsyncMethod : IUserStoreTests
			{
				public CreateAsyncMethod() : base(typeof(CreateAsyncMethod).Name) { }

				[Fact]
				public async Task Create_user_returns_Success()
				{
					// arrange
					var user = new IdentityUser("Create_user_returns_Success");

					// act
					var result = await _userStore.CreateAsync(user);

					// assert
					IdentityResultAssert.IsSuccess(result);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityUserAssert.Equal(user, userFromDb);
				}


				[Fact]
				public async Task Creating_same_user_twice_returns_DuplicateUserName_error()
				{
					// arrange
					var user = new IdentityUser("Creating_same_user_twice_returns_DuplicateUserName_error");

					// act
					var result1 = await _userStore.CreateAsync(user);

					user.UserName = "a different name, but same Id";
					var result2 = await _userStore.CreateAsync(user);

					// assert
					IdentityResultAssert.IsSuccess(result1);

					var expectedError = IdentityResult.Failed(_errorDescriber.DuplicateUserName(user.ToString()));
					IdentityResultAssert.IsFailure(result2, expectedError.Errors.FirstOrDefault());
				}

				[Fact]
				public async Task Creating_two_different_users_but_same_UserName_returns_DuplicateUserName_error()
				{
					// arrange
					var user1 = new IdentityUser("Creating_two_different_users_but_same_UserName_returns_DuplicateUserName_error");
					var user2 = new IdentityUser(user1.UserName);

					// act
					var result1 = await _userStore.CreateAsync(user1);
					var result2 = await _userStore.CreateAsync(user2);

					// assert
					IdentityResultAssert.IsSuccess(result1);

					var expectedError = IdentityResult.Failed(_errorDescriber.DuplicateUserName(user2.ToString()));
					IdentityResultAssert.IsFailure(result2, expectedError.Errors.FirstOrDefault());
				}

				[Fact]
				public async Task Create_a_user_without_NormalizedUserName_and_NormalizedEmail_sets_to_UserName_and_Email()
				{
					// arrange
					var user = new IdentityUser
					{
						UserName = "FarlyFoo",
						Email = "farly@foo.com"
					};

					// act
					var result = await _userStore.CreateAsync(user);

					// assert
					IdentityResultAssert.IsSuccess(result);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.Equal(user.UserName.ToLower(), await _userStore.GetNormalizedUserNameAsync(userFromDb));
					Assert.Equal(user.Email.ToLower(), await _userStore.GetNormalizedEmailAsync(userFromDb));
				}
			}

			public class UpdateAsyncMethod : IUserStoreTests
			{
				public UpdateAsyncMethod() : base(typeof(UpdateAsyncMethod).Name) { }

				[Fact]
				public async Task Update_user_returns_Success()
				{
					// arrange
					var user = new IdentityUser("Update_user_returns_Success");
					user.Claims.Add(_identityClaim1);

					// initial user creation
					await _userStore.CreateAsync(user);
					user.UserName = user.UserName + " different";
					user.Claims.Add(_identityClaim2);

					// act
					var result = await _userStore.UpdateAsync(user);

					// assert
					IdentityResultAssert.IsSuccess(result);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityUserAssert.Equal(user, userFromDb);
				}

				[Fact]
				public async Task Update_user_that_does_not_already_exists_inserts_new_record_and_returns_Success()
				{
					// arrange
					var user = new IdentityUser("Update_user_that_does_not_already_exists_inserts_new_record_and_returns_Success");

					// act
					var result = await _userStore.UpdateAsync(user);

					// assert
					IdentityResultAssert.IsSuccess(result);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityUserAssert.Equal(user, userFromDb);
				}

				[Fact]
				public async Task Can_update_user_multiple_times()
				{
					// arrange
					var user = new IdentityUser("Can_update_user_multiple_times");
					await _userStore.CreateAsync(user);

					// act
					user.Claims.Add(_identityClaim1);
					var result1 = await _userStore.UpdateAsync(user);

					user.UserName = user.UserName + " different";
					var result2 = await _userStore.UpdateAsync(user);

					// assert
					IdentityResultAssert.IsSuccess(result1);
					IdentityResultAssert.IsSuccess(result2);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityUserAssert.Equal(user, userFromDb);
				}

				[Fact]
				public async Task Updating_user_name_to_existing_name_returns_DuplicateUserName_error()
				{
					// arrange
					var user1 = new IdentityUser("Updating_user_name_to_existing_name_returns_DuplicateUserName_error");
					var user2 = new IdentityUser("Updating_user_name_to_existing_name_returns_DuplicateUserName_error different");

					await _userStore.CreateAsync(user1);
					await _userStore.CreateAsync(user2);

					// act
					user2.UserName = user1.UserName;
					var result3 = await _userStore.UpdateAsync(user2);

					// assert
					var expectedError = IdentityResult.Failed(_errorDescriber.DuplicateUserName(user2.ToString()));
					IdentityResultAssert.IsFailure(result3, expectedError.Errors.FirstOrDefault());
				}

				[Fact]
				public async Task Update_a_user_without_NormalizedUserName_and_NormalizedEmail_sets_to_UserName_and_Email()
				{
					// arrange
					var user = new IdentityUser
					{
						UserName = "FarlyFoo",
						Email = "farly@foo.com"
					};
					await _userCollection.InsertOneAsync(user);

					// act
					await _userStore.UpdateAsync(user);

					// assert
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.Equal(user.UserName.ToLower(), await _userStore.GetNormalizedUserNameAsync(userFromDb));
					Assert.Equal(user.Email.ToLower(), await _userStore.GetNormalizedEmailAsync(userFromDb));
				}
			}

			public class DeleteAsyncMethod : IUserStoreTests
			{
				public DeleteAsyncMethod() : base(typeof(DeleteAsyncMethod).Name) { }

				[Fact]
				public async Task Delete_user_returns_Success()
				{
					// arrange
					var user = new IdentityUser("Delete_user_returns_Success");
					await _userStore.CreateAsync(user);


					// act
					var result = await _userStore.DeleteAsync(user);


					// assert
					IdentityResultAssert.IsSuccess(result);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.Null(userFromDb);
				}


				[Fact]
				public async Task Delete_user_that_does_not_exist_returns_Success()
				{
					// arrange
					var user = new IdentityUser("Delete_user_that_does_not_exist_returns_Success");


					// act
					var result = await _userStore.DeleteAsync(user);


					// assert
					IdentityResultAssert.IsSuccess(result);

					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.Null(userFromDb);
				}
			}

			public class FindByIdAsyncMethod : IUserStoreTests
			{
				public FindByIdAsyncMethod() : base(typeof(FindByIdAsyncMethod).Name) { }

				[Fact]
				public async Task Unknown_userId_returns_null()
				{
					// arrange
					var userId = "unknown userId";

					// act
					var result = await _userStore.FindByIdAsync(userId);

					// assert
					Assert.Null(result);
				}

				[Fact]
				public async Task Known_userId_returns_IdentityUser()
				{
					// arrange
					var user = new IdentityUser("Known_userId_returns_IdentityUser");
					await _userStore.CreateAsync(user);

					// act
					var result = await _userStore.FindByIdAsync(user.Id);

					// assert
					IdentityUserAssert.Equal(user, result);
				}
			}

			public class FindByNameAsyncMethod : IUserStoreTests
			{
				public FindByNameAsyncMethod() : base(typeof(FindByNameAsyncMethod).Name) { }

				[Fact]
				public async Task Unknown_normalizedUserName_returns_null()
				{
					// arrange
					var name = "unknown normalised name";

					// act
					var result = await _userStore.FindByNameAsync(name);

					// assert
					Assert.Null(result);
				}

				[Fact]
				public async Task Known_normalizedUserName_returns_IdentityUser()
				{
					// arrange
					var user = new IdentityUser("Known_normalizedUserName_returns_IdentityUser");
					await _userStore.CreateAsync(user);

					// act
					var result = await _userStore.FindByNameAsync(user.UserName);

					// assert
					IdentityUserAssert.Equal(user, result);
				}

				[Fact]
				public async Task Case_insensitive_userName_returns_IdentityUser()
				{
					// arrange
					var user = new IdentityUser("Case_insensitive_normalizedUserName_returns_IdentityUser");
					await _userStore.CreateAsync(user);

					// act
					var result = await _userStore.FindByNameAsync(user.UserName.ToUpper());

					// assert
					IdentityUserAssert.Equal(user, result);
				}
			}

		}

		public class IUserLoginStoreTests : UserStoreTests
		{
			public IUserLoginStoreTests(string collectionPrefix) : base(collectionPrefix) { }

			public class AddLoginAsyncMethod : IUserLoginStoreTests
			{
				public AddLoginAsyncMethod() : base(typeof(AddLoginAsyncMethod).Name)
				{
				}

				[Fact]
				public async Task Adding_new_login_details_updates_database_user_record()
				{
					// arrange
					var user = new IdentityUser("Adding_new_login_details_updates_database_user_record");
					await _userStore.CreateAsync(user);

					var login = new UserLoginInfo("a login provider", "key", "display name");

					// act
					await _userStore.AddLoginAsync(user, login);

					// assert

					// check user logins from memory object
					Assert.NotNull(user.Logins);
					Assert.Equal(1, user.Logins.Count);
					Assert.Equal(login.LoginProvider, user.Logins.First().LoginProvider);
					Assert.Equal(login.ProviderKey, user.Logins.First().ProviderKey);
					Assert.Equal(login.ProviderDisplayName, user.Logins.First().ProviderDisplayName);

					// check user logins from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.NotNull(userFromDb.Logins);
					Assert.Equal(1, userFromDb.Logins.Count);
					Assert.Equal(login.LoginProvider, userFromDb.Logins.First().LoginProvider);
					Assert.Equal(login.ProviderKey, userFromDb.Logins.First().ProviderKey);
					Assert.Equal(login.ProviderDisplayName, userFromDb.Logins.First().ProviderDisplayName);
				}
				
				[Fact]
				public async Task Updating_login_details_for_a_provider_same_key_replaces_details_and_updates_database_user_record()
				{
					// arrange
					var user = new IdentityUser("Updating_login_details_for_a_provider_same_key_replaces_details_and_updates_database_user_record");
					var currentLogin = new UserLoginInfo("a login provider","key", "display name");
					user.Logins.Add(currentLogin);
					await _userStore.CreateAsync(user);

					var newLogin = new UserLoginInfo(currentLogin.LoginProvider, currentLogin.ProviderKey, " other display name");

					// act
					await _userStore.AddLoginAsync(user, newLogin);

					// assert

					// check user logins from memory object
					Assert.NotNull(user.Logins);
					Assert.Equal(1, user.Logins.Count);
					Assert.Equal(newLogin.LoginProvider, user.Logins.First().LoginProvider);
					Assert.Equal(newLogin.ProviderKey, user.Logins.First().ProviderKey);
					Assert.Equal(newLogin.ProviderDisplayName, user.Logins.First().ProviderDisplayName);

					// check user logins from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.NotNull(userFromDb.Logins);
					Assert.Equal(1, userFromDb.Logins.Count);
					Assert.Equal(newLogin.LoginProvider, userFromDb.Logins.First().LoginProvider);
					Assert.Equal(newLogin.ProviderKey, userFromDb.Logins.First().ProviderKey);
					Assert.Equal(newLogin.ProviderDisplayName, userFromDb.Logins.First().ProviderDisplayName);
				}



				[Fact]
				public async Task Updating_login_details_for_a_provider__different_key_adds_login_details_and_updates_database_user_record()
				{
					// arrange
					var user = new IdentityUser("Updating_login_details_for_a_provider__different_key_replaces_details_and_updates_database_user_record");

					var currentLogin = new UserLoginInfo("a login provider", "key", "display name");
					user.Logins.Add(currentLogin);
					await _userStore.CreateAsync(user);

					var newLogin = new UserLoginInfo(currentLogin.LoginProvider, currentLogin.ProviderKey + " different", " other display name");

					// act
					await _userStore.AddLoginAsync(user, newLogin);

					// assert

					// check user logins from memory object
					Assert.NotNull(user.Logins);
					Assert.Equal(2, user.Logins.Count);

					// check user logins from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.NotNull(userFromDb.Logins);
					Assert.Equal(2, userFromDb.Logins.Count);
				}
			}

			public class RemoveLoginAsyncMethod : IUserLoginStoreTests
			{
				public RemoveLoginAsyncMethod() : base(typeof(RemoveLoginAsyncMethod).Name)
				{
				}

				[Fact]
				public async Task Removing_existing_login_should_update_database_record()
				{
					// arrange
					var user = new IdentityUser("Removing_existing_login_should_update_database_record");
					
					var login = new UserLoginInfo("a login provider", "key", "display name");
					user.Logins.Add(login);
					await _userStore.CreateAsync(user);


					// act
					await _userStore.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);

					// assert

					// check user logins from memory object
					Assert.NotNull(user.Logins);
					Assert.Empty(user.Logins);

					// check user logins from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.NotNull(userFromDb.Logins);
					Assert.Empty(userFromDb.Logins);
				}



				[Fact]
				public async Task Removing_existing_login_with_different_case_values_should_update_database_record()
				{
					// arrange
					var user = new IdentityUser("Removing_existing_login_with_different_case_values_should_update_database_record");


					var login = new UserLoginInfo("a login provider", "key", "display name");
					user.Logins.Add(login);
					await _userStore.CreateAsync(user);


					// act
					await _userStore.RemoveLoginAsync(user, login.LoginProvider.ToUpper(), login.ProviderKey.ToUpper());

					// assert

					// check user logins from memory object
					Assert.NotNull(user.Logins);
					Assert.Empty(user.Logins);

					// check user logins from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.NotNull(userFromDb.Logins);
					Assert.Empty(userFromDb.Logins);
				}

			}

			public class FindByLoginAsyncMethod : IUserLoginStoreTests
			{
				public FindByLoginAsyncMethod() : base(typeof (FindByLoginAsyncMethod).Name)
				{
				}

				[Fact]
				public async Task When_login_provider_key_does_not_match_user_in_database_should_return_null()
				{
					// arrange
					var user = new IdentityUser("When_login_provider_key_does_not_match_user_in_database_should_return_null");
					var login = new UserLoginInfo("a login provider", "key", "display name");
					user.Logins.Add(login);
					await _userStore.CreateAsync(user);


					// act
					var result = await _userStore.FindByLoginAsync(login.LoginProvider, login.ProviderKey + " different");

					// assert
					Assert.Null(result);
				}

				[Fact]
				public async Task When_login_provider_does_not_match_user_in_database_should_return_null()
				{
					// arrange
					var user = new IdentityUser("When_login_provider_does_not_match_user_in_database_should_return_null");
					var login = new UserLoginInfo("a login provider", "key", "display name");
					user.Logins.Add(login);
					await _userStore.CreateAsync(user);


					// act
					var result = await _userStore.FindByLoginAsync(login.LoginProvider + " different", login.ProviderKey);

					// assert
					Assert.Null(result);
				}

				[Fact]
				public async Task When_login_provider_details_match_a_user_in_database_should_return_user()
				{
					// arrange
					var user = new IdentityUser("When_login_provider_details_match_a_user_in_database_should_return_user");
					var login = new UserLoginInfo("a login provider", "key", "display name");
					user.Logins.Add(login);
					await _userStore.CreateAsync(user);


					// act
					var result = await _userStore.FindByLoginAsync(login.LoginProvider, login.ProviderKey);

					// assert
					Assert.NotNull(result);
					IdentityUserAssert.Equal(user, result);
				}

				[Fact]
				public async Task When_login_provider_details_with_different_casing_match_a_user_in_database_should_return_user()
				{
					// arrange
					var user = new IdentityUser("When_login_provider_details_with_different_casing_match_a_user_in_database_should_return_user");
					var login = new UserLoginInfo("a login provider", "key", "display name");
					user.Logins.Add(login);
					await _userStore.CreateAsync(user);


					// act
					var result = await _userStore.FindByLoginAsync(login.LoginProvider.ToUpper(), login.ProviderKey.ToUpper());

					// assert
					Assert.NotNull(result);
					IdentityUserAssert.Equal(user, result);
				}
			}
		}

		public class IUserClaimStoreTests : UserStoreTests
		{
			public IUserClaimStoreTests(string collectionPrefix) : base(collectionPrefix)
			{
			}

			public class AddClaimsAsyncMethod : IUserClaimStoreTests
			{
				public AddClaimsAsyncMethod() : base(typeof (AddClaimsAsyncMethod).Name)
				{
				}

				[Fact]
				public async Task Adding_null_claims_to_user_does_not_update_database_user_record()
				{
					// arrange

					var user = new IdentityUser("Adding_new_claim_to_user_updates_database_user_record");
					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, null);

					// assert

					// check user claims from memory
					Assert.NotNull(user.Claims);
					Assert.Empty(user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.NotNull(userFromDb.Claims);
					Assert.Empty(userFromDb.Claims);
				}

				[Fact]
				public async Task Adding_empty_claims_to_user_does_not_update_database_user_record()
				{
					// arrange

					var user = new IdentityUser("Adding_new_claim_to_user_updates_database_user_record");
					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, new List<Claim>());

					// assert

					// check user claims from memory
					Assert.NotNull(user.Claims);
					Assert.Empty(user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.NotNull(userFromDb.Claims);
					Assert.Empty(userFromDb.Claims);
				}

				[Fact]
				public async Task Adding_new_claims_to_user_updates_database_user_record()
				{
					// arrange

					var user = new IdentityUser("Adding_new_claim_to_user_updates_database_user_record");
					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, new List<Claim> {_claim1, _claim2});

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1, _identityClaim2}, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1, _identityClaim2}, userFromDb.Claims);
				}

				[Fact]
				public async Task Adding_new_claim_to_user_with_null_claims_updates_database_user_record()
				{
					// arrange

					var user = new IdentityUser("Adding_new_claim_to_user_with_null_claims_updates_database_user_record");
					user.Claims = null;

					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, new List<Claim> {_claim1, _claim2});

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1, _identityClaim2}, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1, _identityClaim2}, userFromDb.Claims);
				}

				[Fact]
				public async Task Adding_existing_claim_to_user_does_not_update_database_user_record()
				{
					// arrange
					var user = new IdentityUser("Adding_existing_claim_to_user_does_not_update_database_user_record");
					user.Claims.Add(_identityClaim1);
					user.Claims.Add(_identityClaim2);
					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, new List<Claim> {_claim1});

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1, _identityClaim2}, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1, _identityClaim2}, userFromDb.Claims);
				}

				[Fact]
				public async Task Adding_new_claims_to_user_with_some_claims_updates_database_user_record()
				{
					// arrange

					var user = new IdentityUser("Adding_new_claims_to_user_with_some_claims_updates_database_user_record");
					user.Claims.Add(_identityClaim1);
					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, new List<Claim> {_claim1, _claim2});

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1, _identityClaim2}, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1, _identityClaim2}, userFromDb.Claims);
				}

				[Fact]
				public async Task Adding_multiple_claims_with_same_ClaimType_adds_multiple_claims_to_database()
				{
					// arrange
					var user = new IdentityUser("Adding_multiple_claims_with_same_ClaimType_adds_multiple_claims_to_database");
					await _userStore.CreateAsync(user);

					// act
					await _userStore.AddClaimsAsync(user, new List<Claim> {_claim1});
					await _userStore.AddClaimsAsync(user, new List<Claim> {_claim1SameType});

					// assert
					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1, _identityClaim1SameType}, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1, _identityClaim1SameType}, userFromDb.Claims);
				}
			}

			public class RemoveClaimsAsyncMethod : IUserClaimStoreTests
			{
				public RemoveClaimsAsyncMethod() : base(typeof (RemoveClaimsAsyncMethod).Name)
				{
				}

				[Fact]
				public async Task Removing_unknown_claim_does_not_change_database_user_record()
				{
					// arrange
					var user = new IdentityUser("Removing_unknown_claim_does_not_change_database_user_record");
					user.Claims.Add(_identityClaim1);

					await _userStore.CreateAsync(user);

					// act
					await _userStore.RemoveClaimsAsync(user, new List<Claim> {_claim3});

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1}, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1}, userFromDb.Claims);
				}

				[Fact]
				public async Task Removing_claim_from_null_user_claims_does_not_change_database_user_record()
				{
					// arrange
					var user = new IdentityUser("Removing_unknown_claim_and_user_claims_is_null_does_not_change_database_user_record");
					user.Claims = null;

					await _userStore.CreateAsync(user);

					// act
					await _userStore.RemoveClaimsAsync(user, new List<Claim> {_claim3});

					// assert

					// check user claims from memory
					Assert.Equal(0, user.Claims.Count);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.Equal(0, userFromDb.Claims.Count);
				}

				[Fact]
				public async Task Remove_existing_claim_updates_database_user_record()
				{
					// arrange
					var user = new IdentityUser("Remove_existing_claim_updates_database_user_record");
					user.Claims.Add(_identityClaim1);
					user.Claims.Add(_identityClaim2);
					user.Claims.Add(_identityClaim3);
					await _userStore.CreateAsync(user);

					// act
					await _userStore.RemoveClaimsAsync(user, new List<Claim> { _claim2 });

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1, _identityClaim3}, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1, _identityClaim3 }, userFromDb.Claims);
				}

				[Fact]
				public async Task Remove_multiple_existing_claims_updates_database_user_record()
				{
					// arrange
					var user = new IdentityUser("Remove_multiple_existing_claims_updates_database_user_record");
					user.Claims.Add(_identityClaim1);
					user.Claims.Add(_identityClaim2);
					user.Claims.Add(_identityClaim3);
					await _userStore.CreateAsync(user);

					// act
					await _userStore.RemoveClaimsAsync(user, new List<Claim> { _claim1, _claim3 });

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim2 }, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim2 }, userFromDb.Claims);
				}

				[Fact]
				public async Task User_has_multiple_claims_with_same_ClaimType_removing_only_removes_claim_with_same_value()
				{
					// arrange
					var user =
						new IdentityUser("User_has_multiple_claims_with_same_ClaimType_removing_only_removes_claim_with_same_value");
					user.Claims.Add(_identityClaim1);
					user.Claims.Add(_identityClaim1SameType);
					await _userStore.CreateAsync(user);

					// act
					await _userStore.RemoveClaimsAsync(user, new List<Claim> {_claim1});

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1SameType}, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> {_identityClaim1SameType}, userFromDb.Claims);
				}
			}

			public class ReplaceClaimesAsyncMethodd : IUserClaimStoreTests
			{
				public ReplaceClaimesAsyncMethodd() : base(typeof (ReplaceClaimesAsyncMethodd).Name)
				{
				}

				[Fact]
				public async Task No_claims_on_user_does_not_update_database()
				{
					// arrange
					var user = new IdentityUser("No_claims_on_user_does_not_update_database");
					await _userStore.CreateAsync(user);

					// act
					await _userStore.ReplaceClaimAsync(user, _claim1, _claim2);

					// assert

					// check user claims from memory
					Assert.Equal(0, user.Claims.Count());

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					Assert.Equal(0, userFromDb.Claims.Count());
				}

				[Fact]
				public async Task No_matching_claims_does_not_update_database()
				{
					// arrange
					var user = new IdentityUser("No_matching_claims_does_not_update_database");
					user.Claims.Add(_identityClaim1);

					await _userStore.CreateAsync(user);

					// act
					await _userStore.ReplaceClaimAsync(user, _claim3, _claim2);

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1 }, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim1 }, userFromDb.Claims);
				}
				
				[Fact]
				public async Task Matching_claims_are_replaced_and_updated_database()
				{
					// arrange
					var user = new IdentityUser("Matching_claims_are_replaced_and_updated_database");
					user.Claims.Add(_identityClaim1);
					user.Claims.Add(_identityClaim2);

					await _userStore.CreateAsync(user);

					// act
					await _userStore.ReplaceClaimAsync(user, _claim1, _claim3);

					// assert

					// check user claims from memory
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim3, _identityClaim2 }, user.Claims);

					// check user claims from DB
					var userFromDb = await _userCollection.Find(x => x.Id == user.Id).SingleOrDefaultAsync();
					IdentityClaimAssert.Equal(new List<IdentityClaim> { _identityClaim3, _identityClaim2 }, userFromDb.Claims);
				}
			}

			public class GetUsersForClaimAsyncMethod : IUserClaimStoreTests
			{
				public GetUsersForClaimAsyncMethod() : base(typeof (GetUsersForClaimAsyncMethod).Name)
				{
				}

				[Fact]
				public async Task No_users_for_claim_returns_empty_list()
				{
					// arrange
					var user1 = new IdentityUser("No_users_for_claim_returns_empty_list-1");
					user1.Claims.Add(_identityClaim1);
					user1.Claims.Add(_identityClaim2);
					await _userStore.CreateAsync(user1);
					
					var user2 = new IdentityUser("No_users_for_claim_returns_empty_list-2");
					user2.Claims.Add(_identityClaim1);
					await _userStore.CreateAsync(user2);


					// act
					var result = await _userStore.GetUsersForClaimAsync(_claim3);


					// assert
					Assert.NotNull(result);
					Assert.Equal(0, result.Count);
				}

				[Fact]
				public async Task One_user_for_claim_returns_list_with_matching_user()
				{
					// arrange
					var user1 = new IdentityUser("One_user_for_claim_returns_list_with_matching_user-1");
					user1.Claims.Add(_identityClaim1);
					user1.Claims.Add(_identityClaim2);
					await _userStore.CreateAsync(user1);

					var user2 = new IdentityUser("One_user_for_claim_returns_list_with_matching_user-2");
					user2.Claims.Add(_identityClaim1SameType);
					user2.Claims.Add(_identityClaim2);
					await _userStore.CreateAsync(user2);


					// act
					var result = await _userStore.GetUsersForClaimAsync(_claim1);


					// assert
					Assert.Equal(1, result.Count);
					IdentityUserAssert.Equal(user1, result.Single());
				}

				[Fact]
				public async Task Multiple_users_for_claim_returns_list_with_matching_users()
				{
					// arrange
					var user1 = new IdentityUser("Multiple_users_for_claim_returns_list_with_matching_users-1");
					user1.Claims.Add(_identityClaim1);
					user1.Claims.Add(_identityClaim2);
					await _userStore.CreateAsync(user1);

					var user2 = new IdentityUser("Multiple_users_for_claim_returns_list_with_matching_users-2");
					user2.Claims.Add(_identityClaim1);
					await _userStore.CreateAsync(user2);

					var user3 = new IdentityUser("Multiple_users_for_claim_returns_list_with_matching_users-3");
					user3.Claims.Add(_identityClaim2);
					await _userStore.CreateAsync(user3);


					// act
					var result = await _userStore.GetUsersForClaimAsync(_claim1);


					// assert
					Assert.Equal(2, result.Count);
					IdentityUserAssert.Equal(new List<IdentityUser> {user1, user2}, result);
				}
				
				[Fact]
				public async Task Users_for_role_with_claim_returns_list_with_matching_users()
				{
					// arrange
					var role1 = new IdentityRole("Role 1");
					role1.Claims.Add(_identityClaim1);
					role1.Claims.Add(_identityClaim2);

					var role2 = new IdentityRole("Role 2");
					role2.Claims.Add(_identityClaim1);

					var role3 = new IdentityRole("Role 3");
					role3.Claims.Add(_identityClaim2);

					var user1 = new IdentityUser("Users_for_role_with_claim_returns_list_with_matching_users-1");
					user1.Roles.Add(role1);
					await _userStore.CreateAsync(user1);

					var user2 = new IdentityUser("Users_for_role_with_claim_returns_list_with_matching_users-2");
					user2.Roles.Add(role2);
					await _userStore.CreateAsync(user2);

					var user3 = new IdentityUser("Users_for_role_with_claim_returns_list_with_matching_users-3");
					user3.Roles.Add(role3);
					await _userStore.CreateAsync(user3);


					// act
					var result = await _userStore.GetUsersForClaimAsync(_claim1);

					// assert
					Assert.Equal(2, result.Count);
					IdentityUserAssert.Equal(new List<IdentityUser> { user1, user2 }, result);
				}
			}
		}
	}
}
