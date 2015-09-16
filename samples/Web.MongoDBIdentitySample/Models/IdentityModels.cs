using AspNet.Identity3.MongoDB;

namespace Web.MongoDBIdentitySample.Models
{
	// Add profile data for application users by adding properties to the ApplicationUser class
	public class ApplicationUser : IdentityUser
	{
	}

	public class ApplicationDbContext : IdentityDatabaseContext<ApplicationUser, IdentityRole, string>
	{
	}
}
