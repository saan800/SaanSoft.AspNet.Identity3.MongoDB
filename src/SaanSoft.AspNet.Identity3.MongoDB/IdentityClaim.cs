using System;
using System.Security.Claims;

namespace SaanSoft.AspNet.Identity3.MongoDB
{
	/// <summary>
	/// EntityType that represents one specific claim
	/// </summary>
	public class IdentityClaim : IEquatable<IdentityClaim>
	{
		public IdentityClaim()
		{
		}

		public IdentityClaim(string type, string value)
		{
			this.ClaimType = type;
			this.ClaimValue = value;
		}

		public IdentityClaim(Claim claim)
		{
			if (claim == null) return;

			this.ClaimType = claim.Type;
			this.ClaimValue = claim.Value;
		}

		/// <summary>
		/// Claim type
		/// </summary>
		public virtual string ClaimType { get; set; }

		/// <summary>
		/// Claim value
		/// </summary>
		public virtual string ClaimValue { get; set; }
		

		#region IEquatable<IdentityClaim> (Equals, GetHashCode(), ==, !=)
		
		public override bool Equals(object obj)
		{
			if (!(obj is IdentityClaim)) return false;

			var thisObj = (IdentityClaim)obj;
			return this.Equals(thisObj);
		}

		public virtual bool Equals(IdentityClaim obj)
		{
			if (obj == null) return false;

			return this.ClaimType.Equals(obj.ClaimType, StringComparison.OrdinalIgnoreCase) &&
				   this.ClaimValue.Equals(obj.ClaimValue, StringComparison.OrdinalIgnoreCase);
		}

		public virtual bool Equals(Claim obj)
		{
			if (obj == null) return false;

			return this.Equals(new IdentityClaim(obj));
		}

		public static bool operator ==(IdentityClaim left, IdentityClaim right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(IdentityClaim left, IdentityClaim right)
		{
			return !Equals(left, right);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (StringComparer.OrdinalIgnoreCase.GetHashCode(ClaimType) * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(ClaimValue);
			}
		}

		#endregion
	}
}
