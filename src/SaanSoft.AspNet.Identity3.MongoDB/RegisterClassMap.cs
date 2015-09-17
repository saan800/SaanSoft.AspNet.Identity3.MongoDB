using System;
using MongoDB.Bson.Serialization.Conventions;

namespace SaanSoft.AspNet.Identity3.MongoDB
{
	public class RegisterClassMap<TUser, TRole, TKey>
		where TUser : IdentityUser<TKey>
		where TRole : IdentityRole<TKey>
		where TKey : IEquatable<TKey>
	{

		private static RegisterClassMap<TUser, TRole, TKey> _thisClassMap;
		public static void Init()
		{
			if (_thisClassMap == null)
			{
				_thisClassMap = new RegisterClassMap<TUser, TRole, TKey>();
			}
			_thisClassMap.Configure();
		}


		public virtual void Configure()
		{
			RegisterConventions();

			RegisterRoleClassMap();
			RegisterUserClassMap();

		}
		
		public virtual void RegisterConventions()
		{
			var conv = new ConventionPack();
			conv.Add(new IgnoreIfDefaultConvention(true));
			conv.Add(new IgnoreExtraElementsConvention(true));

			// apply these conventions to SaanSoft.AspNet.Identity3.MongoDB and items that inherit it
			ConventionRegistry.Register("SaanSoft.AspNet.Identity3.MongoDB.Conventions", conv, t => t.Namespace.StartsWith(typeof(IdentityRole<TKey>).Namespace) || (t.BaseType != null && t.BaseType.Namespace.StartsWith(typeof(IdentityRole<TKey>).Namespace)));
		}

		public virtual void RegisterRoleClassMap() { }
		public virtual void RegisterUserClassMap() { }
	}
}
