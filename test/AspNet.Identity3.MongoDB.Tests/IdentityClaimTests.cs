using Xunit;

namespace AspNet.Identity3.MongoDB.Tests
{
	public class IdentityClaimTests
	{
		private readonly IdentityClaim _identityClaim1;
		private readonly IdentityClaim _identityClaim1SameType;

		public IdentityClaimTests()
		{
			_identityClaim1 = new IdentityClaim { ClaimType = "ClaimType1", ClaimValue = "some value" };
			_identityClaim1SameType = new IdentityClaim { ClaimType = _identityClaim1.ClaimType, ClaimValue = _identityClaim1.ClaimValue + " different"};
		}

		public class EqualsObjMethod : IdentityClaimTests
		{
			[Fact]
			public void When_objects_match_returns_true()
			{
				// arrange
				var obj = (object)new IdentityClaim { ClaimType = _identityClaim1.ClaimType, ClaimValue = _identityClaim1.ClaimValue }; ;
				

				// assert
				Assert.True(_identityClaim1.Equals(obj));
			}

			[Fact]
			public void When_objects_dont_match_returns_false()
			{
				// arrange
				var obj = (object)new IdentityClaim { ClaimType = _identityClaim1SameType.ClaimType, ClaimValue = _identityClaim1SameType.ClaimValue }; ;


				// assert
				Assert.False(_identityClaim1.Equals(obj));
			}

			[Fact]
			public void When_object_is_null_returns_false()
			{
				// assert
				Assert.False(_identityClaim1.Equals((object)null));
			}

			[Fact]
			public void When_object_is_different_type_returns_false()
			{
				// arrange
				var obj = (object)"hello world";

				// assert
				Assert.False(_identityClaim1.Equals(obj));
			}
		}

		public class EqualsMethod : IdentityClaimTests
		{
			[Fact]
			public void When_claims_match_returns_true()
			{
				// arrange
				var c = new IdentityClaim { ClaimType = _identityClaim1.ClaimType, ClaimValue = _identityClaim1.ClaimValue }; ;


				// assert
				Assert.True(_identityClaim1.Equals(c));
			}

			[Fact]
			public void When_claims_dont_match_returns_false()
			{
				// arrange
				var c = new IdentityClaim { ClaimType = _identityClaim1SameType.ClaimType, ClaimValue = _identityClaim1SameType.ClaimValue }; ;


				// assert
				Assert.False(_identityClaim1.Equals(c));
			}

			[Fact]
			public void When_claim_is_null_returns_false()
			{
				// assert
				Assert.False(_identityClaim1.Equals((IdentityClaim)null));
			}
		}
	}
}
