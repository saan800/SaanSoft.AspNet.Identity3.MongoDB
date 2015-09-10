using Xunit;

namespace AspNet.Identity3.MongoDB.Tests
{
	public class IdentityUserLoginTests
	{
		private readonly IdentityUserLogin _login1;
		private readonly IdentityUserLogin _login1SameType;

		public IdentityUserLoginTests()
		{
			_login1 = new IdentityUserLogin { LoginProvider = "LoginProvider1", ProviderKey = "some key"};
			_login1SameType = new IdentityUserLogin { LoginProvider = _login1.LoginProvider, ProviderKey = _login1.ProviderKey + " different" };
		}

		public class EqualsObjMethod : IdentityUserLoginTests
		{
			[Fact]
			public void When_objects_match_returns_true()
			{
				// arrange
				var obj = (object)new IdentityUserLogin { LoginProvider = _login1.LoginProvider, ProviderKey = _login1.ProviderKey }; ;


				// assert
				Assert.True(_login1.Equals(obj));
			}

			[Fact]
			public void When_objects_dont_match_returns_false()
			{
				// arrange
				var obj = (object)new IdentityUserLogin { LoginProvider = _login1SameType.LoginProvider, ProviderKey = _login1SameType.ProviderKey }; ;


				// assert
				Assert.False(_login1.Equals(obj));
			}

			[Fact]
			public void When_object_is_null_returns_false()
			{
				// assert
				Assert.False(_login1.Equals((object)null));
			}

			[Fact]
			public void When_object_is_different_type_returns_false()
			{
				// arrange
				var obj = (object)"hello world";

				// assert
				Assert.False(_login1.Equals(obj));
			}
		}

		public class EqualsMethod : IdentityUserLoginTests
		{
			[Fact]
			public void When_logins_match_returns_true()
			{
				// arrange
				var c = new IdentityUserLogin { LoginProvider = _login1.LoginProvider, ProviderKey = _login1.ProviderKey }; ;


				// assert
				Assert.True(_login1.Equals(c));
			}

			[Fact]
			public void When_logins_dont_match_returns_false()
			{
				// arrange
				var c = new IdentityUserLogin { LoginProvider = _login1SameType.LoginProvider, ProviderKey = _login1SameType.ProviderKey }; ;


				// assert
				Assert.False(_login1.Equals(c));
			}

			[Fact]
			public void When_logins_is_null_returns_false()
			{
				// assert
				Assert.False(_login1.Equals((IdentityUserLogin)null));
			}
		}
	}
}
