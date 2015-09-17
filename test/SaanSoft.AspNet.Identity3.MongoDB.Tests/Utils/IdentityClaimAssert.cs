using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public static class IdentityClaimAssert
	{
		public static void Equal(IdentityClaim expected, IdentityClaim actual)
		{
			Assert.True((expected == null && actual == null) || (expected != null && actual != null));

			Assert.Equal(expected.ClaimType, actual.ClaimType);
			Assert.Equal(expected.ClaimValue, actual.ClaimValue);
		}

		public static void Equal(IEnumerable<IdentityClaim> expected, IEnumerable<IdentityClaim> actual)
		{
			Assert.True((expected == null && actual == null) || (expected != null && actual != null));
			Assert.Equal(expected.Count(), actual.Count());

			foreach(var e in expected)
			{
				Assert.True(actual.SingleOrDefault(a => a.ClaimType == e.ClaimType && a.ClaimValue == e.ClaimValue) != null);
			}
		}
	}
}
