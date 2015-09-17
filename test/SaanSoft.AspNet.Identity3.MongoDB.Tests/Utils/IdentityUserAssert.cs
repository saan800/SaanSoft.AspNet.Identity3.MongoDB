using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public static class IdentityUserAssert
	{
		public static void Equal(IdentityUser expected, IdentityUser actual)
		{
			Assert.True((expected == null && actual == null) || (expected != null && actual != null));

			Assert.Equal(expected.Id, actual.Id);
			Assert.Equal(expected.UserName, actual.UserName);
			Assert.Equal(expected.NormalizedUserName, actual.NormalizedUserName);
			Assert.Equal(expected.Email, actual.Email);
			Assert.Equal(expected.NormalizedEmail, actual.NormalizedEmail);
			Assert.Equal(expected.EmailConfirmed, actual.EmailConfirmed);
			Assert.Equal(expected.PasswordHash, actual.PasswordHash);
			Assert.Equal(expected.SecurityStamp, actual.SecurityStamp);
			Assert.Equal(expected.PhoneNumber, actual.PhoneNumber);
			Assert.Equal(expected.PhoneNumberConfirmed, actual.PhoneNumberConfirmed);
			Assert.Equal(expected.TwoFactorEnabled, actual.TwoFactorEnabled);
			Assert.Equal(expected.LockoutEnd, actual.LockoutEnd);
			Assert.Equal(expected.LockoutEnabled, actual.LockoutEnabled);
			Assert.Equal(expected.AccessFailedCount, actual.AccessFailedCount);

			IdentityRoleAssert.Equal(expected.Roles.Cast<IdentityRole>(), actual.Roles.Cast<IdentityRole>());
			IdentityClaimAssert.Equal(expected.Claims, actual.Claims);
			IdentityClaimAssert.Equal(expected.AllClaims, actual.AllClaims);
			IdentityUserLoginAssert.Equal(expected.Logins, actual.Logins);
		}
		
		public static void Equal(IEnumerable<IdentityUser> expected, IEnumerable<IdentityUser> actual)
		{
			Assert.True((expected == null && actual == null) || (expected != null && actual != null));
			Assert.Equal(expected.Count(), actual.Count());

			foreach (var e in expected)
			{
				var act = actual.SingleOrDefault(a => a.Id == e.Id);
				IdentityUserAssert.Equal(e, act);
			}
		}
	}
}
