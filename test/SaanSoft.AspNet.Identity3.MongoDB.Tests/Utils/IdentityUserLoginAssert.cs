using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public static class IdentityUserLoginAssert
	{
		public static void Equal(IdentityUserLogin expected, IdentityUserLogin actual)
		{
			Assert.True((expected == null && actual == null) || (expected != null && actual != null));

			Assert.Equal(expected.LoginProvider, actual.LoginProvider);
			Assert.Equal(expected.ProviderKey, actual.ProviderKey);
			Assert.Equal(expected.ProviderDisplayName, actual.ProviderDisplayName);
		}

		public static void Equal(IEnumerable<IdentityUserLogin> expected, IEnumerable<IdentityUserLogin> actual)
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
