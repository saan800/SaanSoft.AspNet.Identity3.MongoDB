using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaanSoft.AspNet.Identity3.MongoDB;
using Web.MongoDBIdentitySample.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Web.MongoDBIdentitySample.Controllers
{
	public class DiTestController : Controller
	{
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IIdentityDatabaseContext<ApplicationUser, IdentityRole, string> _identityDatabaseContext;
		private readonly IUserStore<ApplicationUser> _userStore;
		private readonly IUserLoginStore<ApplicationUser> _userLoginStore;
		private readonly IUserRoleStore<ApplicationUser> _userRoleStore;
		private readonly IUserClaimStore<ApplicationUser> _userClaimStore;
		private readonly IUserPasswordStore<ApplicationUser> _userPasswordStore;
		private readonly IUserSecurityStampStore<ApplicationUser> _userSecurityStampStore;
		private readonly IUserEmailStore<ApplicationUser> _userEmailStore;
		private readonly IUserLockoutStore<ApplicationUser> _userLockoutStore;
		private readonly IUserPhoneNumberStore<ApplicationUser> _userPhoneNumberStore;
		private readonly IUserTwoFactorStore<ApplicationUser> _userTwoFactorStore;
		private readonly IQueryableUserStore<ApplicationUser> _queryableUserStore;
		private readonly IRoleStore<IdentityRole> _roleStore;
		private readonly IRoleClaimStore<IdentityRole> _roleClaimStore;
		private readonly IQueryableRoleStore<IdentityRole> _queryableRoleStore;

		public DiTestController(
			// the Microsoft.AspNetCore.Identity User and Role Manager classes
			RoleManager<IdentityRole> roleManager,
			UserManager<ApplicationUser> userManager,
			
			IIdentityDatabaseContext<ApplicationUser, IdentityRole, string> identityDatabaseContext,

			// if want to use with SOLID and Interface Segregation Principle, then can just use the specific interface that need

			// these interfaces are all implemented by UserStore
			IUserStore<ApplicationUser> userStore,
			IUserLoginStore<ApplicationUser> userLoginStore,
			IUserRoleStore<ApplicationUser> userRoleStore,
			IUserClaimStore<ApplicationUser> userClaimStore,
			IUserPasswordStore<ApplicationUser> userPasswordStore,
			IUserSecurityStampStore<ApplicationUser> userSecurityStampStore,
			IUserEmailStore<ApplicationUser> userEmailStore,
			IUserLockoutStore<ApplicationUser>  userLockoutStore,
			IUserPhoneNumberStore<ApplicationUser> userPhoneNumberStore,
			IUserTwoFactorStore<ApplicationUser> userTwoFactorStore,
			IQueryableUserStore<ApplicationUser> queryableUserStore,

			// these interfaces are all implemented by RoleStore
			IRoleStore<IdentityRole> roleStore,
			IRoleClaimStore<IdentityRole> roleClaimStore,
			IQueryableRoleStore<IdentityRole> queryableRoleStore
		)
		{
			_roleManager = roleManager;
			_userManager = userManager;

			_identityDatabaseContext = identityDatabaseContext;
			_userStore = userStore;
			_userLoginStore = userLoginStore;
			_userRoleStore = userRoleStore;
			_userClaimStore = userClaimStore;
			_userPasswordStore = userPasswordStore;
			_userSecurityStampStore = userSecurityStampStore;
			_userEmailStore = userEmailStore;
			_userLockoutStore = userLockoutStore;
			_userPhoneNumberStore = userPhoneNumberStore;
			_userTwoFactorStore = userTwoFactorStore;
			_queryableUserStore = queryableUserStore;

			_roleStore = roleStore;
			_roleClaimStore = roleClaimStore;
			_queryableRoleStore = queryableRoleStore;
		}

		// GET: /<controller>/
		public IActionResult Index()
		{
			return View();
		}
	}
}
