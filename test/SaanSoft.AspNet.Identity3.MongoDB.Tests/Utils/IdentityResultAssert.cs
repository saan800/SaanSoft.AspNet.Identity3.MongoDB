using System.Linq;
using Microsoft.AspNet.Identity;
using Xunit;

namespace SaanSoft.AspNet.Identity3.MongoDB.Tests
{
	public static class IdentityResultAssert
	{
		public static void IsSuccess(IdentityResult result)
		{
			Assert.NotNull(result);
			Assert.True(result.Succeeded);
		}

		public static void IsFailure(IdentityResult result)
		{
			Assert.NotNull(result);
			Assert.False(result.Succeeded);
		}

		public static void IsFailure(IdentityResult result, string error)
		{
			Assert.NotNull(result);
			Assert.False(result.Succeeded);
			Assert.Equal(error, result.Errors.First().Description);
		}

		public static void IsFailure(IdentityResult result, IdentityError error)
		{
			Assert.NotNull(result);
			Assert.False(result.Succeeded);
			Assert.Equal(error.Description, result.Errors.First().Description);
			Assert.Equal(error.Code, result.Errors.First().Code);
		}
	}
}