using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SaanSoft.AspNet.Identity3.MongoDB
{
	public interface IIdentityDatabaseContext<TUser, TRole, TKey>
		where TRole : IdentityRole<TKey>
		where TUser : IdentityUser<TKey>
		where TKey : IEquatable<TKey>
	{
		IMongoCollection<TUser> UserCollection { get; set; }
		IMongoCollection<TRole> RoleCollection { get; set; }
	}

	public class IdentityDatabaseContext : IdentityDatabaseContext<IdentityUser, IdentityRole, string>
	{
	}

	public class IdentityDatabaseContext<TUser, TRole, TKey> : IIdentityDatabaseContext<TUser, TRole, TKey>
		where TRole : IdentityRole<TKey>
		where TUser : IdentityUser<TKey>
		where TKey : IEquatable<TKey>
	{
		public string ConnectionString { get; set; }
		public string DatabaseName { get; set; } = "AspNetIdentity";
		public string UserCollectionName { get; set; } = "AspNetUsers";
		public string RoleCollectionName { get; set; } = "AspNetRoles";
		
		/// <summary>
		/// When the <see cref="UserCollection"/> and <see cref="RoleCollection"/> collections are initially fetched - ensure the indexes are created (default: false)
		/// </summary>
		public bool EnsureCollectionIndexes { get; set; } = false;

		public MongoCollectionSettings CollectionSettings { get; set; } = new MongoCollectionSettings { WriteConcern = WriteConcern.WMajority };
		public CreateCollectionOptions CreateCollectionOptions { get; set; } = new CreateCollectionOptions { AutoIndexId = true };
		public CreateIndexOptions CreateIndexOptions { get; set; } = new CreateIndexOptions { Background = true, Sparse = true };


		public virtual IMongoClient Client
		{
			get
			{
				if (_client == null)
				{
					if (_database != null)
					{
						_client = _database.Client;
					}

					if (string.IsNullOrWhiteSpace(ConnectionString))
					{
						throw new NullReferenceException($"The parameter '{nameof(ConnectionString)}' in '{typeof (IdentityDatabaseContext<TUser, TRole, TKey>).FullName}' is null and must be set before calling '{nameof(Client)}'. This is usually configured as part of Startup.cs");
					}
					_client = new MongoClient(ConnectionString);
				}
				return _client;
			}
			set { _client = value; }
		}
		private IMongoClient _client;

		public virtual IMongoDatabase Database
		{
			get
			{
				if (_database == null)
				{
					if (string.IsNullOrWhiteSpace(DatabaseName))
					{
						throw new NullReferenceException($"The parameter '{nameof(DatabaseName)}' in '{typeof(IdentityDatabaseContext<TUser, TRole, TKey>).FullName}' is null and must be set before calling '{nameof(Database)}'. This is usually configured as part of Startup.cs");
					}
					_database = Client.GetDatabase(DatabaseName);
				}
				return _database;
			}
			set { _database = value; }
		}
		private IMongoDatabase _database;


		/// <summary>
		/// Initiates the user collection. If <see cref="EnsureCollectionIndexes"/> == true, will call <see cref="EnsureUserIndexesCreated"/>
		/// </summary>
		public virtual IMongoCollection<TUser> UserCollection
		{
			get
			{
				if (_userCollection == null)
				{
					if (string.IsNullOrWhiteSpace(UserCollectionName))
					{
						throw new NullReferenceException($"The parameter '{nameof(UserCollectionName)}' in '{typeof (IdentityDatabaseContext<TUser, TRole, TKey>).FullName}' is null and must be set before calling '{nameof(UserCollection)}'. This is usually configured as part of Startup.cs");
					}
					if (EnsureCollectionIndexes)
					{
						EnsureUserIndexesCreated();
					}

					_userCollection = Database.GetCollection<TUser>(UserCollectionName, CollectionSettings);
				}
				return _userCollection;
			}
			set { _userCollection = value; }
		}
		private IMongoCollection<TUser> _userCollection;


		/// <summary>
		/// Initiates the role collection. If <see cref="EnsureCollectionIndexes"/> == true, will call <see cref="EnsureRoleIndexesCreated"/>
		/// </summary>
		public virtual IMongoCollection<TRole> RoleCollection
		{
			get
			{
				if (_roleCollection == null)
				{
					if (string.IsNullOrWhiteSpace(RoleCollectionName))
					{
						throw new NullReferenceException($"The parameter '{nameof(RoleCollectionName)}' in '{typeof(IdentityDatabaseContext<TUser, TRole, TKey>).FullName}' is null and must be set before calling '{nameof(RoleCollection)}'. This is usually configured as part of Startup.cs");
					}
					if (EnsureCollectionIndexes)
					{
						EnsureRoleIndexesCreated();
					}
					_roleCollection = Database.GetCollection<TRole>(RoleCollectionName, CollectionSettings);
				}
				return _roleCollection;
			}
			set { _roleCollection = value; }
		}
		private IMongoCollection<TRole> _roleCollection;
		
		/// <summary>
		/// Ensures the user collection is instantiated, and has the standard indexes applied. Called when <see cref="UserCollection"/> is called if <see cref="EnsureCollectionIndexes"/> == true.
		/// </summary>
		/// <remarks>
		/// Indexes Created:
		/// Users: NormalizedUserName, NormalizedEmail, Logins.LoginProvider, Roles.NormalizedName, Claims.ClaimType + Roles.Claims.ClaimType
		/// </remarks>
		public virtual void EnsureUserIndexesCreated()
		{
			// only check on app startup
			if (_doneUserIndexes) return;
			_doneUserIndexes = true;

			// ensure collection exists
			if (!CollectionExists(UserCollectionName))
			{
				Database.CreateCollectionAsync(UserCollectionName, CreateCollectionOptions).Wait();
			}

			// ensure NormalizedUserName index exists
			var normalizedNameIndex = Builders<TUser>.IndexKeys.Ascending(x => x.NormalizedUserName);
			UserCollection.Indexes.CreateOneAsync(normalizedNameIndex, CreateIndexOptions);

			// ensure NormalizedEmail index exists
			var normalizedEmailIndex = Builders<TUser>.IndexKeys.Ascending(x => x.NormalizedEmail);
			UserCollection.Indexes.CreateOneAsync(normalizedEmailIndex, CreateIndexOptions);
			
			// ensure Roles.NormalizedName index exists
			var roleNameIndex = Builders<TUser>.IndexKeys.Ascending("Roles_NormalizedName");
			UserCollection.Indexes.CreateOneAsync(roleNameIndex, CreateIndexOptions);

			// ensure LoginProvider index exists
			var loginProviderIndex = Builders<TUser>.IndexKeys.Ascending("Logins_LoginProvider");
			UserCollection.Indexes.CreateOneAsync(loginProviderIndex, CreateIndexOptions);

			// ensure claims index exists
			var claimsProviderIndex = Builders<TUser>.IndexKeys.Ascending("Claims_ClaimType").Ascending("Roles_Claims_ClaimType");
			UserCollection.Indexes.CreateOneAsync(claimsProviderIndex, CreateIndexOptions);
		}

		/// <summary>
		/// Singleton property. Only want to check collection indexes are done once, not on every call
		/// </summary>
		protected static bool _doneUserIndexes = false;
		protected static bool _doneRoleIndexes = false;

		/// <summary>
		/// Ensures the role collection is instantiated, and has the standard indexes applied. Called when <see cref="RoleCollection"/> is called if <see cref="EnsureCollectionIndexes"/> == true.
		/// </summary>
		/// <remarks>
		/// Indexes Created:
		/// Roles: NormalizedName
		/// </remarks>
		public virtual void EnsureRoleIndexesCreated()
		{
			// only check on app startup
			if (_doneRoleIndexes) return;
			_doneRoleIndexes = true;

			// ensure collection exists
			if (!CollectionExists(RoleCollectionName))
			{
				Database.CreateCollectionAsync(RoleCollectionName, CreateCollectionOptions).Wait();
			}

			// ensure NormalizedName index exists
			var normalizedNameIndex = Builders<TRole>.IndexKeys.Ascending(x => x.NormalizedName);
			RoleCollection.Indexes.CreateOneAsync(normalizedNameIndex, CreateIndexOptions);
		}

		/// <summary>
		/// WARNING: Permanently deletes user collection, including all indexes and data.
		/// </summary>
		public virtual void DeleteUserCollection()
		{
			Database.DropCollectionAsync(UserCollectionName).Wait();
			_doneUserIndexes = false;
		}

		/// <summary>
		/// WARNING: Permanently deletes role collection, including all indexes and data.
		/// </summary>
		public virtual void DeleteRoleCollection()
		{
			Database.DropCollectionAsync(RoleCollectionName).Wait();
			_doneRoleIndexes = false;
		}
		
		/// <summary>
		/// check if the collection already exists
		/// </summary>
		/// <param name="collectionName"></param>
		/// <returns></returns>
		protected virtual bool CollectionExists(string collectionName)
		{
			var filter = new BsonDocument("name", collectionName);
			var cursorTask = Database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
			cursorTask.Wait();

			var cursor = cursorTask.Result.ToListAsync();
			cursor.Wait();

			return cursor.Result.Any();
		}
	}
}