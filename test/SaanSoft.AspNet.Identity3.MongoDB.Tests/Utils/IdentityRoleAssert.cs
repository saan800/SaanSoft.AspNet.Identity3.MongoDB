using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public static class IdentityRoleAssert
	{
		public static void Equal(IdentityRole expected, IdentityRole actual)
		{
			Assert.True((expected == null && actual == null) || (expected != null && actual != null));

			Assert.Equal(expected.Id, actual.Id);
			Assert.Equal(expected.Name, actual.Name);
			Assert.Equal(expected.NormalizedName, actual.NormalizedName);

			IdentityClaimAssert.Equal(expected.Claims, actual.Claims);
		}

		public static void Equal(IEnumerable<IdentityRole> expected, IEnumerable<IdentityRole> actual)
		{
			Assert.True((expected == null && actual == null) || (expected != null && actual != null));
			Assert.Equal(expected.Count(), actual.Count());

			foreach (var e in expected)
			{
				var act = actual.SingleOrDefault(a => a.Id == e.Id);
				IdentityRoleAssert.Equal(e, act);
			}
		}
	}
}