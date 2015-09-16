using System;
using System.Collections.Generic;
using System.Linq;

namespace AspNet.Identity3.MongoDB
{

	public class IdentityUser : IdentityUser<string>
	{
		public IdentityUser() : base()
		{
			Id = Guid.NewGuid().ToString();
		}

		public IdentityUser(string userName) : this()
		{
			UserName = userName;
		}
	}

	public class IdentityUser<TKey> : IdentityUser<IdentityRole<TKey>, TKey>
		where TKey : IEquatable<TKey>
	{
		public IdentityUser() : base()
		{
		}

		public IdentityUser(string userName) : base(userName)
		{
		}
	}

	public class IdentityUser<TRole, TKey> 
		where TRole : IdentityRole<TKey>
		where TKey : IEquatable<TKey>
	{


		public IdentityUser() { }

		public IdentityUser(string userName) : this()
		{
			UserName = userName;
		}

		public virtual TKey Id { get; set; }
		public virtual string UserName { get; set; }
		
		/// /// <summary>
		/// NOTE: should not be used except when extending AspNet.Identity3.MongoDB. 
		/// Value will be overridden by RoleStore.
		/// Used to store the username that is formatted in a case insensitive way so can do searches on it
		/// </summary>
		public virtual string NormalizedUserName { get; set; }

		/// <summary>
		/// Email
		/// </summary>
		public virtual string Email { get; set; }
		
		/// /// <summary>
		/// NOTE: should not be used except when extending AspNet.Identity3.MongoDB. 
		/// Value will be overridden by RoleStore.
		/// Used to store the email that is formatted in a case insensitive way so can do searches on it
		/// </summary>
		public virtual string NormalizedEmail { get; set; }

		/// <summary>
		/// True if the email is confirmed, default is false
		/// </summary>
		public virtual bool EmailConfirmed { get; set; }

		/// <summary>
		/// The salted/hashed form of the user password
		/// </summary>
		public virtual string PasswordHash { get; set; }

		/// <summary>
		/// A random value that should change whenever a user's credentials change (ie, password changed, login removed)
		/// </summary>
		public virtual string SecurityStamp { get; set; }

		/// <summary>
		/// PhoneNumber for the user
		/// </summary>
		public virtual string PhoneNumber { get; set; }

		/// <summary>
		/// True if the phone number is confirmed, default is false
		/// </summary>
		public virtual bool PhoneNumberConfirmed { get; set; }

		/// <summary>
		/// Is two factor enabled for the user
		/// </summary>
		public virtual bool TwoFactorEnabled { get; set; }

		/// <summary>
		/// DateTime in UTC when lockout ends, any time in the past is considered not locked out.
		/// </summary>
		public virtual DateTimeOffset? LockoutEnd { get; set; }

		/// <summary>
		/// Is lockout enabled for this user
		/// </summary>
		public virtual bool LockoutEnabled { get; set; }

		/// <summary>
		/// Used to record failures for the purposes of lockout
		/// </summary>
		public virtual int AccessFailedCount { get; set; }

		/// <summary>
		/// Navigation property for users in the role
		/// </summary>
		public virtual IList<TRole> Roles
		{
			get { return _roles; }
			set { _roles = value ?? new List<TRole>(); }
		} 
		private IList<TRole> _roles = new List<TRole>();

		/// <summary>
		/// Navigation property for users claims
		/// </summary>
		public virtual IList<IdentityClaim> Claims
		{
			get { return _claims; }
			set { _claims = value ?? new List<IdentityClaim>(); }
		}
		private IList<IdentityClaim> _claims = new List<IdentityClaim>();

		/// <summary>
		/// Get a list of all user's claims combined with claims from role
		/// </summary>
		public virtual IList<IdentityClaim> AllClaims
		{ 
			get
			{
				// as Claims and Roles are virtual and could be overridden with an implementation that allows nulls
				//	- make sure they aren't null just in case
				var clms = Claims ?? new List<IdentityClaim>();
				var rls = Roles ??  new List<TRole>();

				return clms.Concat(rls.Where(r => r.Claims != null).SelectMany(r => r.Claims)).Distinct().ToList();
			}
		}

		/// <summary>
		/// Navigation property for users logins
		/// </summary>
		public virtual IList<IdentityUserLogin> Logins
		{
			get { return _logins; }
			set { _logins = value ?? new List<IdentityUserLogin>(); }
		}
		private IList<IdentityUserLogin> _logins = new List<IdentityUserLogin>();

		/// <summary>
		/// Returns a friendly name
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return UserName;
		}


		#region IEquatable<IdentityUserLogin> (Equals, GetHashCode(), ==, !=)

		public override bool Equals(object obj)
		{
			if (!(obj is IdentityUser<TRole, TKey>)) return false;

			var thisObj = (IdentityUser<TRole, TKey>)obj;
			return this.Equals(thisObj);
		}

		public virtual bool Equals(IdentityUser<TRole, TKey> obj)
		{
			if (obj == null) return false;

			return this.Id.Equals(obj.Id);
		}

		public static bool operator ==(IdentityUser<TRole, TKey> left, IdentityUser<TRole, TKey> right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(IdentityUser<TRole, TKey> left, IdentityUser<TRole, TKey> right)
		{
			return !Equals(left, right);
		}

		public override int GetHashCode()
		{
			unchecked
			{

				return StringComparer.OrdinalIgnoreCase.GetHashCode(this.Id);
			}
		}

		#endregion
	}
}
