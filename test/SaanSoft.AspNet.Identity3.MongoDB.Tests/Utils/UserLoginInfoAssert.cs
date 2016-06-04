using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public static class UserLoginInfoAssert
	{
		public static void Equal(UserLoginInfo expected, UserLoginInfo actual)
		{
			Assert.True((expected == null && actual == null) || (expected != null && actual != null));

			Assert.Equal(expected.LoginProvider, actual.LoginProvider);
			Assert.Equal(expected.ProviderKey, actual.ProviderKey);
			Assert.Equal(expected.ProviderDisplayName, actual.ProviderDisplayName);
		}

		public static void Equal(IEnumerable<UserLoginInfo> expected, IEnumerable<UserLoginInfo> actual)
		{
			Assert.True((expected == null && actual == null) || (expected != null && actual != null));
			Assert.Equal(expected.Count(), actual.Count());

			foreach (var e in expected)
			{
				Assert.True(actual.SingleOrDefault(a => a.LoginProvider == e.LoginProvider && a.ProviderKey == e.ProviderKey && a.ProviderDisplayName == e.ProviderDisplayName) != null);
			}
		}
	}
}
