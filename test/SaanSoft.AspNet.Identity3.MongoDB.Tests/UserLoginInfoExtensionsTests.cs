using Microsoft.AspNet.Identity;
using Xunit;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public class UserLoginInfoExtensionsTests
	{
		private readonly UserLoginInfo _login1;
		private readonly UserLoginInfo _login1SameType;

		public UserLoginInfoExtensionsTests()
		{
			_login1 = new UserLoginInfo("LoginProvider1", "some key", "some value");
			_login1SameType = new UserLoginInfo(_login1.LoginProvider, _login1.ProviderKey + " different", _login1.ProviderDisplayName);
		}

		public class AreEqualsMethod : UserLoginInfoExtensionsTests
		{
			[Fact]
			public void When_logins_provider_and_key_match_returns_true()
			{
				// arrange
				var c = new UserLoginInfo(_login1.LoginProvider, _login1.ProviderKey, "");


				// assert
				Assert.True(_login1.AreEqual(c));
			}

			[Fact]
			public void When_logins_provider_and_key_match_but_different_casing_returns_true()
			{
				// arrange
				var c = new UserLoginInfo(_login1.LoginProvider.ToUpper(), _login1.ProviderKey.ToUpper(), "");


				// assert
				Assert.True(_login1.AreEqual(c));
			}

			[Fact]
			public void When_logins_dont_match_returns_false()
			{
				// assert
				Assert.False(_login1.AreEqual(_login1SameType));
			}

			[Fact]
			public void When_logins_is_null_returns_false()
			{
				// assert
				Assert.False(_login1.AreEqual((UserLoginInfo)null));
			}

			[Fact]
			public void When_both_logins_is_null_returns_true()
			{
				// arrange
				UserLoginInfo login1 = null;
				UserLoginInfo login2 = null;

				// assert
				Assert.True(login1.AreEqual(login2));
			}
		}
	}
}
