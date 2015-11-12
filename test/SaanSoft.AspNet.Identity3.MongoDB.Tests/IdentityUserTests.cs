using System;
using Microsoft.AspNet.Identity;
using Xunit;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public class IdentityUserTests
	{
		#region Common/Shared

		protected IdentityUser User;

		protected IdentityRole Role1;
		protected IdentityRole Role2;

		protected IdentityClaim Claim1;
		protected IdentityClaim Claim2;
		protected IdentityClaim Claim3;
		protected IdentityClaim Claim4;
		protected IdentityClaim Claim1Alt;

		protected UserLoginInfo Login1;

		public IdentityUserTests()
		{
			User = new IdentityUser("John Doe");

			Role1 = new IdentityRole { Name = "Role1" };
			Role2 = new IdentityRole { Name = "Role2" };

			Claim1 = new IdentityClaim { ClaimType = "Claim1", ClaimValue = "Some value" };
			Claim2 = new IdentityClaim { ClaimType = "Claim2", ClaimValue = "Some other value" };
			Claim3 = new IdentityClaim { ClaimType = "Claim3", ClaimValue = "Yet another value" };
			Claim4 = new IdentityClaim { ClaimType = "Claim4", ClaimValue = "Many many claims" };

			Claim1Alt = new IdentityClaim { ClaimType = "Claim1", ClaimValue = "Some alternate value" };

			Login1 = new UserLoginInfo("SomeProvider", "AKey", "Bob");

		}

		#endregion

		public class RolesProperty : IdentityUserTests
		{
			[Fact]
			public void When_try_to_set_roles_to_null_is_actually_set_to_empty_list()
			{
				// arrange
				User.Roles.Add(Role1);

				// act
				User.Roles = null;

				// assert
				Assert.NotNull(User.Roles);
				Assert.Empty(User.Roles);
			}
		}

		public class ClaimsProperty : IdentityUserTests
		{
			[Fact]
			public void When_try_to_set_claims_to_null_is_actually_set_to_empty_list()
			{
				// arrange
				User.Claims.Add(Claim1);

				// act
				User.Claims = null;

				// assert
				Assert.NotNull(User.Claims);
				Assert.Empty(User.Claims);
			}
		}

		public class LoginsProperty : IdentityUserTests
		{
			[Fact]
			public void When_try_to_set_logins_to_null_is_actually_set_to_empty_list()
			{
				// arrange
				User.Logins.Add(Login1);

				// act
				User.Logins = null;

				// assert
				Assert.NotNull(User.Logins);
				Assert.Empty(User.Logins);
			}
		}

		public class AllClaimsMethod : IdentityUserTests
		{
			[Fact]
			public void Returns_empty_list_when_initialised()
			{
				// assert
				Assert.NotNull(User.AllClaims);
				Assert.Empty(User.AllClaims);
			}

			[Fact]
			public void Returns_all_from_users_claims()
			{
				// arrange
				User.Claims.Add(Claim1);
				User.Claims.Add(Claim2);

				// assert
				Assert.Contains(Claim1, User.AllClaims);
				Assert.Contains(Claim2, User.AllClaims);
				Assert.Equal(2, User.AllClaims.Count);
				
			}

			[Fact]
			public void Returns_distinct_list_of_all_claims_from_roles()
			{
				// arrange
				Role1.Claims.Add(Claim1);
				Role1.Claims.Add(Claim2);
				Role1.Claims.Add(Claim4);

				Role2.Claims.Add(Claim1Alt);
				Role2.Claims.Add(Claim2);
				Role2.Claims.Add(Claim4);

				User.Roles.Add(Role1);
				User.Roles.Add(Role2);

				// assert
				Assert.Contains(Claim1, User.AllClaims);
				Assert.Contains(Claim1Alt, User.AllClaims);
				Assert.Contains(Claim2, User.AllClaims);
				Assert.Contains(Claim4, User.AllClaims);
				Assert.DoesNotContain(Claim3, User.AllClaims);

				Assert.Equal(4, User.AllClaims.Count);
			}

			[Fact]
			public void Returns_distinct_list_of_all_from_user_claims_and_roles_claims()
			{
				// arrange
				Role1.Claims.Add(Claim1);
				Role1.Claims.Add(Claim2);
				Role1.Claims.Add(Claim4);
				
				Role2.Claims.Add(Claim2);
				Role2.Claims.Add(Claim4);

				User.Roles.Add(Role1);
				User.Roles.Add(Role2);
				User.Claims.Add(Claim1);
				User.Claims.Add(Claim1Alt);

				// assert
				Assert.Contains(Claim1, User.AllClaims);
				Assert.Contains(Claim1Alt, User.AllClaims);
				Assert.Contains(Claim2, User.AllClaims);
				Assert.Contains(Claim4, User.AllClaims);
				Assert.DoesNotContain(Claim3, User.AllClaims);

				Assert.Equal(4, User.AllClaims.Count);
			}
		}
		
		public class EqualsObjMethod : IdentityUserTests
		{
			[Fact]
			public void When_objects_match_returns_true()
			{
				// arrange
				var obj = (object)new IdentityUser { Id = User.Id };


				// assert
				Assert.True(User.Equals(obj));
			}

			[Fact]
			public void When_objects_dont_match_returns_false()
			{
				// arrange
				var obj = (object)new IdentityUser { Id = Guid.NewGuid().ToString() };


				// assert
				Assert.False(User.Equals(obj));
			}

			[Fact]
			public void When_object_is_null_returns_false()
			{
				// assert
				Assert.False(User.Equals((object)null));
			}

			[Fact]
			public void When_object_is_different_type_returns_false()
			{
				// arrange
				var obj = (object)"hello world";

				// assert
				Assert.False(User.Equals(obj));
			}
		}

		public class EqualsMethod : IdentityUserTests
		{
			[Fact]
			public void When_users_match_returns_true()
			{
				// arrange
				var c = new IdentityUser { Id = User.Id };


				// assert
				Assert.True(User.Equals(c));
			}

			[Fact]
			public void When_users_dont_match_returns_false()
			{
				// arrange
				var c = new IdentityUser{ Id = Guid.NewGuid().ToString() };


				// assert
				Assert.False(User.Equals(c));
			}

			[Fact]
			public void When_user_is_null_returns_false()
			{
				// assert
				Assert.False(User.Equals((IdentityUser)null));
			}
		}
	}
}
