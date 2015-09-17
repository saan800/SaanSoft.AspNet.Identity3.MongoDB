using System;
using Xunit;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public class IdentityRoleTests
	{
		#region Common/Shared

		protected IdentityRole Role;

		protected IdentityClaim Claim1;
		protected IdentityClaim Claim2;
		protected IdentityClaim Claim3;
		protected IdentityClaim Claim4;
		protected IdentityClaim Claim1Alt;

		public IdentityRoleTests()
		{
			Role = new IdentityRole("role");

			Claim1 = new IdentityClaim { ClaimType = "Claim1", ClaimValue = "Some value" };
			Claim2 = new IdentityClaim { ClaimType = "Claim2", ClaimValue = "Some other value" };
			Claim3 = new IdentityClaim { ClaimType = "Claim3", ClaimValue = "Yet another value" };
			Claim4 = new IdentityClaim { ClaimType = "Claim4", ClaimValue = "Many many claims" };

			Claim1Alt = new IdentityClaim { ClaimType = "Claim1", ClaimValue = "Some alternate value" };

		}

		#endregion

		public class ClaimsProperty : IdentityRoleTests
		{
			[Fact]
			public void When_try_to_set_claims_to_null_is_actually_set_to_empty_list()
			{
				// arrange
				Role.Claims.Add(Claim1);

				// act
				Role.Claims = null;

				// assert
				Assert.NotNull(Role.Claims);
				Assert.Empty(Role.Claims);
			}
		}
		

		public class EqualsObjMethod : IdentityRoleTests
		{
			[Fact]
			public void When_objects_match_returns_true()
			{
				// arrange
				var obj = (object)new IdentityRole { Id = Role.Id };


				// assert
				Assert.True(Role.Equals(obj));
			}

			[Fact]
			public void When_objects_dont_match_returns_false()
			{
				// arrange
				var obj = (object)new IdentityRole { Id = Guid.NewGuid().ToString() };


				// assert
				Assert.False(Role.Equals(obj));
			}

			[Fact]
			public void When_object_is_null_returns_false()
			{
				// assert
				Assert.False(Role.Equals((object)null));
			}

			[Fact]
			public void When_object_is_different_type_returns_false()
			{
				// arrange
				var obj = (object)"hello world";

				// assert
				Assert.False(Role.Equals(obj));
			}
		}

		public class EqualsMethod : IdentityRoleTests
		{
			[Fact]
			public void When_roles_match_returns_true()
			{
				// arrange
				var c = new IdentityRole {Id = Role.Id};


				// assert
				Assert.True(Role.Equals(c));
			}

			[Fact]
			public void When_roles_dont_match_returns_false()
			{
				// arrange
				var c = new IdentityRole {Id = Guid.NewGuid().ToString()};


				// assert
				Assert.False(Role.Equals(c));
			}

			[Fact]
			public void When_role_is_null_returns_false()
			{
				// assert
				Assert.False(Role.Equals((IdentityRole)null));
			}
		}
	}
}
