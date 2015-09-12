using System;

namespace AspNet.Identity3.MongoDB
{
	/// <summary>
	/// Entity type for a user's login (i.e. facebook, google, email/password)
	/// </summary>
	public class IdentityUserLogin : IEquatable<IdentityUserLogin>
	{
		/// <summary>
		/// The login provider for the login (i.e. facebook, google)
		/// </summary>
		public virtual string LoginProvider { get; set; }

		/// <summary>
		/// Key representing the login for the provider
		/// </summary>
		public virtual string ProviderKey { get; set; }

		/// <summary>
		/// Display name for the login
		/// </summary>
		public virtual string ProviderDisplayName { get; set; }
		
		#region IEquatable<IdentityUserLogin> (Equals, GetHashCode(), ==, !=)
		
		public override bool Equals(object obj)
		{
			if (!(obj is IdentityUserLogin)) return false;

			var thisObj = (IdentityUserLogin)obj;
			return this.Equals(thisObj);
		}

		public virtual bool Equals(IdentityUserLogin obj)
		{
			if (obj == null) return false;

			return this.LoginProvider.Equals(obj.LoginProvider, StringComparison.OrdinalIgnoreCase) &&
				   this.ProviderKey.Equals(obj.ProviderKey, StringComparison.OrdinalIgnoreCase);
		}

		public static bool operator ==(IdentityUserLogin left, IdentityUserLogin right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(IdentityUserLogin left, IdentityUserLogin right)
		{
			return !Equals(left, right);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (StringComparer.OrdinalIgnoreCase.GetHashCode(LoginProvider) * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(ProviderKey);
			}
		}

		#endregion
	}
}