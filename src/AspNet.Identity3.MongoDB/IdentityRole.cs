using System;
using System.Collections.Generic;

namespace AspNet.Identity3.MongoDB
{
	/// <summary>
	/// Represents a Role entity
	/// </summary>
	public class IdentityRole : IdentityRole<string>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public IdentityRole() : base()
		{
			Id = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="roleName"></param>
		public IdentityRole(string roleName) : this()
		{
			Name = roleName;
		}
	}

	/// <summary>
	/// Represents a Role entity
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	public class IdentityRole<TKey> where TKey : IEquatable<TKey>
	{
		public IdentityRole() { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="roleName"></param>
		public IdentityRole(string roleName) : this()
		{
			Name = roleName;
		}

		/// <summary>
		/// Role id
		/// </summary>
		public virtual TKey Id { get; set; }

		/// <summary>
		/// Role name
		/// </summary>
		public virtual string Name { get; set; }
		
		public virtual string NormalizedName { get; set; }

		/// <summary>
		/// Navigation property for claims in the role
		/// </summary>
		public virtual IList<IdentityClaim> Claims
		{
			get { return _claims; }
			set { _claims = value ?? new List<IdentityClaim>(); }
		}
		private IList<IdentityClaim> _claims = new List<IdentityClaim>();

		/// <summary>
		/// Returns a friendly name
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name;
		}



		// TODO: Equals override?? and test
	}
}
