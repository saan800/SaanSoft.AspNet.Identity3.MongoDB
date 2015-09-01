using Microsoft.AspNet.Identity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace AspNet.Identity3.MongoDB
{
	public class UserStore<TUser, TRole> 
		: UserStore<TUser, TRole, string>
		where TUser : IdentityUser<string>
		where TRole : IdentityRole<string>
	{
		public UserStore(string connectionString, string databaseName = null, string collectionName = null, MongoCollectionSettings collectionSettings = null, IdentityErrorDescriber describer = null) : 
			base(connectionString, databaseName, collectionName, collectionSettings, describer) { }

		public UserStore(IMongoClient client, string databaseName = null, string collectionName = null, MongoCollectionSettings collectionSettings = null, IdentityErrorDescriber describer = null) : 
			base(client, databaseName, collectionName, collectionSettings, describer) { }

		public UserStore(IMongoDatabase database, string collectionName = null, MongoCollectionSettings collectionSettings = null, IdentityErrorDescriber describer = null) : 
			base(database, collectionName, collectionSettings, describer) { }

		public UserStore(IMongoCollection<TUser> collection, IdentityErrorDescriber describer = null) : 
			base(collection, describer) { }
	}

	public class UserStore<TUser, TRole, TKey> :
		IUserLoginStore<TUser>,
		IUserRoleStore<TUser>,
		IUserClaimStore<TUser>,
		IUserPasswordStore<TUser>,
		IUserSecurityStampStore<TUser>,
		IUserEmailStore<TUser>,
		IUserLockoutStore<TUser>,
		IUserPhoneNumberStore<TUser>,
		IQueryableUserStore<TUser>,
		IUserTwoFactorStore<TUser>
		where TUser : IdentityUser<TKey>
		where TRole : IdentityRole<TKey>
		where TKey : IEquatable<TKey>
	{
		#region Constructor and MongoDB Connections

		protected string _databaseName;
		protected string _collectionName;
		protected IMongoClient _client;
		protected IMongoDatabase _database;
		protected IMongoCollection<TUser> _collection;

		public UserStore(string connectionString, string databaseName = null, string collectionName = null, MongoCollectionSettings collectionSettings = null, IdentityErrorDescriber describer = null)
		{
			if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));

			SetProperties(databaseName, collectionName, describer);
			SetDbConnection(connectionString, collectionSettings);
		}

		public UserStore(IMongoClient client, string databaseName = null, string collectionName = null, MongoCollectionSettings collectionSettings = null, IdentityErrorDescriber describer = null)
		{
			if (client == null) throw new ArgumentNullException(nameof(client));

			SetProperties(databaseName, collectionName, describer);
			SetDbConnection(client, collectionSettings);
		}

		public UserStore(IMongoDatabase database, string collectionName = null, MongoCollectionSettings collectionSettings = null, IdentityErrorDescriber describer = null)
		{
			if (database == null) throw new ArgumentNullException(nameof(database));

			SetProperties(database.DatabaseNamespace.DatabaseName, collectionName, describer);
			SetDbConnection(database, collectionSettings);
		}

		public UserStore(IMongoCollection<TUser> collection, IdentityErrorDescriber describer = null)
		{
			if (collection == null) throw new ArgumentNullException(nameof(collection));

			_collection = collection;
			_database = collection.Database;
			_client = _database.Client;

			SetProperties(_database.DatabaseNamespace.DatabaseName, _collection.CollectionNamespace.CollectionName, describer);
		}

		protected void SetProperties(string databaseName, string collectionName, IdentityErrorDescriber describer)
		{
			_databaseName = string.IsNullOrWhiteSpace(databaseName) ? DefaultSettings.DatabaseName : databaseName;
			_collectionName = string.IsNullOrWhiteSpace(collectionName) ? DefaultSettings.UserCollectionName : collectionName;
			ErrorDescriber = describer ?? new IdentityErrorDescriber();
		}

		/// <summary>
		/// IMPORTANT: ensure _databaseName and _collectionName are set (if needed) before calling this
		/// </summary>
		/// <param name="connectionString"></param>
		protected void SetDbConnection(string connectionString, MongoCollectionSettings collectionSettings)
		{
			SetDbConnection(new MongoClient(connectionString), collectionSettings);
		}

		/// <summary>
		/// IMPORTANT: ensure _databaseName and _collectionName are set (if needed) before calling this
		/// </summary>
		/// <param name="client"></param>
		protected void SetDbConnection(IMongoClient client, MongoCollectionSettings collectionSettings)
		{
			SetDbConnection(client.GetDatabase(_databaseName), collectionSettings);
		}

		/// <summary>
		/// IMPORTANT: ensure _collectionName is set (if needed) before calling this
		/// </summary>
		/// <param name="database"></param>
		protected void SetDbConnection(IMongoDatabase database, MongoCollectionSettings collectionSettings)
		{
			_database = database;
			_client = _database.Client;
			collectionSettings = collectionSettings ?? DefaultSettings.CollectionSettings();
			_collection = _database.GetCollection<TUser>(_collectionName, collectionSettings);
		}


		#endregion

		/// <summary>
		/// Used to generate public API error messages
		/// </summary>
		public virtual IdentityErrorDescriber ErrorDescriber { get; set; }

		#region IUserStore<TUser> (base inteface for the other interfaces)

		/// <summary>
		/// Gets the user identifier for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose identifier should be retrieved.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the identifier for the specified <paramref name="user"/>.</returns>
		public virtual Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));

			return Task.FromResult(ConvertIdToString(user.Id));
		}

		/// <summary>
		/// Gets the user name for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose name should be retrieved.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the name for the specified <paramref name="user"/>.</returns>
		public virtual Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));
			
			return Task.FromResult(user.UserName);
		}

		/// <summary>
		/// Sets the given <paramref name="userName" /> for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose name should be set.</param>
		/// <param name="userName">The user name to set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));

			user.UserName = userName;
			return Task.FromResult(0);
		}

		/// <summary>
		/// Gets the normalized user name for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose normalized name should be retrieved.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the normalized user name for the specified <paramref name="user"/>.</returns>
		public virtual Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));

			return Task.FromResult(user.NormalizedUserName);
		}

		/// <summary>
		/// Sets the given normalized name for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose name should be set.</param>
		/// <param name="normalizedName">The normalized name to set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));

			user.NormalizedUserName = normalizedName;
			return Task.FromResult(0);
		}

		/// <summary>
		/// Creates the specified <paramref name="user"/> in the user store, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user to create.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the creation operation.</returns>
		public virtual async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (await UserDetailsAlreadyExists(user, cancellationToken)) return IdentityResult.Failed(ErrorDescriber.DuplicateUserName(user.ToString()));

			try
			{
				await _collection.InsertOneAsync(user, cancellationToken);
			}
			catch (MongoWriteException)
			{
				return IdentityResult.Failed(ErrorDescriber.DuplicateUserName(user.ToString()));
			}

			return IdentityResult.Success;
		}

		/// <summary>
		/// Updates the specified <paramref name="user"/> in the user store, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user to update.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the update operation.</returns>
		public virtual async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (await UserDetailsAlreadyExists(user, cancellationToken)) return IdentityResult.Failed(ErrorDescriber.DuplicateUserName(user.ToString()));

			var filter = Builders<TUser>.Filter.Eq(x => x.Id, user.Id);
			var updateOptions = new UpdateOptions { IsUpsert = true };
			await _collection.ReplaceOneAsync(filter, user, updateOptions, cancellationToken);

			return IdentityResult.Success;
		}

		/// <summary>
		/// Deletes the specified <paramref name="user"/> from the user store, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user to delete.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the update operation.</returns>
		public virtual async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));

			var filter = Builders<TUser>.Filter.Eq(x => x.Id, user.Id);
			await _collection.DeleteOneAsync(filter, cancellationToken);

			return IdentityResult.Success;
		}

		/// <summary>
		/// Finds and returns a user, if any, who has the specified <paramref name="userId"/>.
		/// </summary>
		/// <param name="userId">The user ID to search for.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="userID"/> if it exists.
		/// </returns>
		public virtual Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			TKey id = ConvertIdFromString(userId);
			if (id == null) return Task.FromResult((TUser)null);

			var filter = Builders<TUser>.Filter.Eq(x => x.Id, id);
			var options = new FindOptions { AllowPartialResults = false };

			return _collection.Find(filter, options).SingleOrDefaultAsync(cancellationToken);
		}

		/// <summary>
		/// Finds and returns a user, if any, who has the specified normalized user name.
		/// </summary>
		/// <param name="normalizedUserName">The normalized user name to search for.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="userID"/> if it exists.
		/// </returns>
		public virtual Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();

			if (string.IsNullOrWhiteSpace(normalizedUserName)) return Task.FromResult((TUser)null);

			var filter = Builders<TUser>.Filter.Eq(x => x.NormalizedUserName, normalizedUserName);
			var options = new FindOptions { AllowPartialResults = false };

			return _collection.Find(filter, options).SingleOrDefaultAsync(cancellationToken);
		}

		#endregion

		#region IUserLoginStore<TUser>

		/// <summary>
		/// Adds an external <see cref="UserLoginInfo"/> to the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user to add the login to.</param>
		/// <param name="login">The external <see cref="UserLoginInfo"/> to add to the specified <paramref name="user"/>.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (login == null) throw new ArgumentNullException(nameof(login));

			if (user.Logins == null) user.Logins = new List<IdentityUserLogin>();

			var iul = new IdentityUserLogin
			{
				ProviderKey = login.ProviderKey,
				LoginProvider = login.LoginProvider,
				ProviderDisplayName = login.ProviderDisplayName
			};
			user.Logins.Add(iul);

			// update in database
			var update = Builders<TUser>.Update.Push(x => x.Logins, iul);
			await DoUserDetailsUpdate(user.Id, update, null, cancellationToken);
		}

		/// <summary>
		/// Attempts to remove the provided login information from the specified <paramref name="user"/>, as an asynchronous operation.
		/// and returns a flag indicating whether the removal succeed or not.
		/// </summary>
		/// <param name="user">The user to remove the login information from.</param>
		/// <param name="loginProvider">The login provide whose information should be removed.</param>
		/// <param name="providerKey">The key given by the external login provider for the specified user.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that contains a flag the result of the asynchronous removing operation. The flag will be true if
		/// the login information was existed and removed, otherwise false.
		/// </returns>
		public virtual Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Retrieves the associated logins for the specified <param ref="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose associated logins to retrieve.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> for the asynchronous operation, containing a list of <see cref="UserLoginInfo"/> for the specified <paramref name="user"/>, if any.
		/// </returns>
		public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Retrieves the user associated with the specified login provider and login provider key, as an asynchronous operation..
		/// </summary>
		/// <param name="loginProvider">The login provider who provided the <paramref name="providerKey"/>.</param>
		/// <param name="providerKey">The key provided by the <paramref name="loginProvider"/> to identify a user.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> for the asynchronous operation, containing the user, if any which matched the specified login provider and key.
		/// </returns>
		public virtual Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IUserRoleStore<TUser>

		/// <summary>
		/// Add a the specified <paramref name="user"/> to the named role, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user to add to the named role.</param>
		/// <param name="roleName">The name of the role to add the user to.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Add a the specified <paramref name="user"/> from the named role, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user to remove the named role from.</param>
		/// <param name="roleName">The name of the role to remove.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a list of role names the specified <paramref name="user"/> belongs to, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose role names to retrieve.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing a list of role names.</returns>
		public virtual Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a flag indicating whether the specified <paramref name="user"/> is a member of the give named role, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose role membership should be checked.</param>
		/// <param name="roleName">The name of the role to be checked.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified <see cref="user"/> is
		/// a member of the named role.
		/// </returns>
		public virtual Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a list of Users who are members of the named role.
		/// </summary>
		/// <param name="roleName">The name of the role whose membership should be returned.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that represents the asynchronous operation, containing a list of users who are in the named role.
		/// </returns>
		public virtual Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IUserClaimStore<TUser>

		/// <summary>
		/// Gets a list of all <see cref="Claim"/>s to be belonging to the specified <paramref name="user"/> as an asynchronous operation.
		/// </summary>
		/// <param name="user">The role whose claims to retrieve.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> that represents the result of the asynchronous query, a list of <see cref="Claim"/>s.
		/// </returns>
		public virtual Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));
			EnsureClaimsNotNull(user);
			EnsureRolesNotNull(user);

			IList<Claim> result = user.AllClaims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
			return Task.FromResult(result);
		}

		/// <summary>
		/// Add claims to a user as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user to add the claim to.</param>
		/// <param name="claims">The collection of <see cref="Claim"/>s to add.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public virtual async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));
			EnsureClaimsNotNull(user);
			if (claims == null || !claims.Any()) return;

			// claim and value already exist - just return
			var newClaimsList = new List<IdentityClaim>();
			foreach (var c in 
					from claim in claims 
					where !user.Claims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value) 
					select new IdentityClaim { ClaimType = claim.Type, ClaimValue = claim.Value })
			{
				newClaimsList.Add(c);
				user.Claims.Add(c);
			}

			// if no new claims - nothing else to do
			if (!newClaimsList.Any()) return;
			
			// update user claims in the database
			var update = Builders<TUser>.Update.PushEach(x => x.Claims, newClaimsList);
			await DoUserDetailsUpdate(user.Id, update, null, cancellationToken);
		}

		/// <summary>
		/// Replaces the given <paramref name="claim"/> on the specified <paramref name="user"/> with the <paramref name="newClaim"/>
		/// </summary>
		/// <param name="user">The user to replace the claim on.</param>
		/// <param name="claim">The claim to replace.</param>
		/// <param name="newClaim">The new claim to replace the existing <paramref name="claim"/> with.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public virtual async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));
			EnsureClaimsNotNull(user);
			if (claim == null) throw new ArgumentNullException(nameof(claim));
			if (newClaim == null) throw new ArgumentNullException(nameof(newClaim));
			
			if (user.Claims == null) user.Claims = new List<IdentityClaim>();


			var matchedClaims = user.Claims.Where(uc=> uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type);
			if (matchedClaims.Any())
			{
				foreach (var matchedClaim in matchedClaims)
				{
					matchedClaim.ClaimValue = newClaim.Value;
					matchedClaim.ClaimType = newClaim.Type;
				}


				var update = Builders<TUser>.Update.Set(x => x.Claims, user.Claims);
				await DoUserDetailsUpdate(user.Id, update, null, cancellationToken);
			}
		}

		/// <summary>
		/// Removes the specified <paramref name="claims"/> from the given <paramref name="user"/>.
		/// </summary>
		/// <param name="user">The user to remove the specified <paramref name="claims"/> from.</param>
		/// <param name="claims">A collection of <see cref="Claim"/>s to remove.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public virtual async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (user == null) throw new ArgumentNullException(nameof(user));
			EnsureClaimsNotNull(user);
			if (!user.Claims.Any()) return;
			if (claims == null || !claims.Any()) return;
			
			var existingClaimsList = new List<IdentityClaim>();
			foreach (var c in 
					from claim in claims 
					where user.Claims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value) 
					select user.Claims.Single(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value))
			{
				existingClaimsList.Add(c);
				user.Claims.Remove(c);
			}

			if (!existingClaimsList.Any()) return;

			// update user claims in the database
			var update = Builders<TUser>.Update.PullAll(x => x.Claims, existingClaimsList);
			await DoUserDetailsUpdate(user.Id, update, null, cancellationToken);
		}

		/// <summary>
		/// Returns a list of users who contain the specified <see cref="Claim"/>.
		/// </summary>
		/// <param name="claim">The claim to look for.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> that represents the result of the asynchronous query, a list of <typeparamref name="TUser"/> who
		/// contain the specified claim.
		/// </returns>
		public virtual Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (claim == null) throw new ArgumentNullException(nameof(claim));

			throw new NotImplementedException();
		}

		#endregion

		#region IUserPasswordStore<TUser>

		/// <summary>
		/// Sets the password hash for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose password hash to set.</param>
		/// <param name="passwordHash">The password hash to set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the password hash for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose password hash to retrieve.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, returning the password hash for the specified <paramref name="user"/>.</returns>
		public virtual Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a flag indicating whether the specified <paramref name="user"/> has a password, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user to return a flag for, indicating whether they have a password or not.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that represents the asynchronous operation, returning true if the specified <paramref name="user"/> has a password
		/// otherwise false.
		/// </returns>
		public virtual Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IUserSecurityStampStore<TUser>

		/// <summary>
		/// Sets the provided security <paramref name="stamp"/> for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose security stamp should be set.</param>
		/// <param name="stamp">The security stamp to set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get the security stamp for the specified <paramref name="user" />, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose security stamp should be set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the security stamp for the specified <paramref name="user"/>.</returns>
		public virtual Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IUserEmailStore<TUser>

		/// <summary>
		/// Sets the <paramref name="email"/> address for a <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose email should be set.</param>
		/// <param name="email">The email to set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public virtual Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the email address for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose email should be returned.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object containing the results of the asynchronous operation, the email address for the specified <paramref name="user"/>.</returns>
		public virtual Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a flag indicating whether the email address for the specified <paramref name="user"/> has been verified, true if the email address is verified otherwise
		/// false, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose email confirmation status should be returned.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The task object containing the results of the asynchronous operation, a flag indicating whether the email address for the specified <paramref name="user"/>
		/// has been confirmed or not.
		/// </returns>
		public virtual Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sets the flag indicating whether the specified <paramref name="user"/>'s email address has been confirmed or not, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose email confirmation status should be set.</param>
		/// <param name="confirmed">A flag indicating if the email address has been confirmed, true if the address is confirmed otherwise false.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public virtual Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the user, if any, associated with the specified, normalized email address, as an asynchronous operation.
		/// </summary>
		/// <param name="normalizedEmail">The normalized email address to return the user for.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The task object containing the results of the asynchronous lookup operation, the user if any associated with the specified normalized email address.
		/// </returns>
		public virtual Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the normalized email for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose email address to retrieve.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The task object containing the results of the asynchronous lookup operation, the normalized email address if any associated with the specified user.
		/// </returns>
		public virtual Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sets the normalized email for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose email address to set.</param>
		/// <param name="normalizedEmail">The normalized email to set for the specified <paramref name="user"/>.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public virtual Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IUserLockoutStore<TUser>

		/// <summary>
		/// Gets the last <see cref="DateTimeOffset"/> a user's last lockout expired, if any, as an asynchronous operation.
		/// Any time in the past should be indicates a user is not locked out.
		/// </summary>
		/// <param name="user">The user whose lockout date should be retrieved.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> that represents the result of the asynchronous query, a <see cref="DateTimeOffset"/> containing the last time
		/// a user's lockout expired, if any.
		/// </returns>
		public virtual Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Locks out a user until the specified end date has passed, as an asynchronous operation. Setting a end date in the past immediately unlocks a user.
		/// </summary>
		/// <param name="user">The user whose lockout date should be set.</param>
		/// <param name="lockoutEnd">The <see cref="DateTimeOffset"/> after which the <paramref name="user"/>'s lockout should end.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Records that a failed access has occurred, incrementing the failed access count, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose cancellation count should be incremented.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the incremented failed access count.</returns>
		public virtual Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Resets a user's failed access count, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose failed access count should be reset.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		/// <remarks>This is typically called after the account is successfully accessed.</remarks>
		public virtual Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Retrieves the current failed access count for the specified <paramref name="user"/>, as an asynchronous operation..
		/// </summary>
		/// <param name="user">The user whose failed access count should be retrieved.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the failed access count.</returns>
		public virtual Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Retrieves a flag indicating whether user lockout can enabled for the specified user, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose ability to be locked out should be returned.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that represents the asynchronous operation, true if a user can be locked out, otherwise false.
		/// </returns>
		public virtual Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Set the flag indicating if the specified <paramref name="user"/> can be locked out, as an asynchronous operation..
		/// </summary>
		/// <param name="user">The user whose ability to be locked out should be set.</param>
		/// <param name="enabled">A flag indicating if lock out can be enabled for the specified <paramref name="user"/>.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IUserPhoneNumberStore<TUser>

		/// <summary>
		/// Sets the telephone number for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose telephone number should be set.</param>
		/// <param name="phoneNumber">The telephone number to set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the telephone number, if any, for the specified <paramref name="user"/>, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose telephone number should be retrieved.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user's telephone number, if any.</returns>
		public virtual Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a flag indicating whether the specified <paramref name="user"/>'s telephone number has been confirmed, as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user to return a flag for, indicating whether their telephone number is confirmed.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that represents the asynchronous operation, returning true if the specified <paramref name="user"/> has a confirmed
		/// telephone number otherwise false.
		/// </returns>
		public virtual Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sets a flag indicating if the specified <paramref name="user"/>'s phone number has been confirmed, as an asynchronous operation..
		/// </summary>
		/// <param name="user">The user whose telephone number confirmation status should be set.</param>
		/// <param name="confirmed">A flag indicating whether the user's telephone number has been confirmed.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IUserTwoFactorStore<TUser>

		/// <summary>
		/// Sets a flag indicating whether the specified <paramref name="user "/>has two factor authentication enabled or not,
		/// as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose two factor authentication enabled status should be set.</param>
		/// <param name="enabled">A flag indicating whether the specified <paramref name="user"/> has two factor authentication enabled.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a flag indicating whether the specified <paramref name="user "/>has two factor authentication enabled or not,
		/// as an asynchronous operation.
		/// </summary>
		/// <param name="user">The user whose two factor authentication enabled status should be set.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// The <see cref="Task"/> that represents the asynchronous operation, containing a flag indicating whether the specified 
		/// <paramref name="user "/>has two factor authentication enabled or not.
		/// </returns>
		public virtual Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IQueryableUserStore<TUser>

		/// <summary>
		/// WARNING: awaiting the mongoDB csharp driver to implement AsQueryable https://jira.mongodb.org/browse/CSHARP-935. In the mean time using ToList of the repos (http://stackoverflow.com/questions/29124995/is-asqueryable-method-departed-in-new-mongodb-c-sharp-driver-2-0rc).
		/// Returns an <see cref="IQueryable{T}"/> collection of users.
		/// </summary>
		/// <value>An <see cref="IQueryable{T}"/> collection of users.</value>
		public virtual IQueryable<TUser> Users
		{
			get
			{
				// TODO: This is really rubbish
				//		awaiting the mongoDB csharp driver to implement AsQueryable
				//		https://jira.mongodb.org/browse/CSHARP-935
				//		Temporary list solution from http://stackoverflow.com/questions/29124995/is-asqueryable-method-departed-in-new-mongodb-c-sharp-driver-2-0rc
				ThrowIfDisposed();
				var filter = Builders<TUser>.Filter.Ne(x => x.Id, default(TKey));
				var list = _collection.Find(filter).ToListAsync().Result;

				return list.AsQueryable();
			}
		}

		#endregion

		#region IDisposable

		private bool _disposed = false; // To detect redundant calls


		public virtual void Dispose()
		{
			_disposed = true;
		}

		/// <summary>
		/// Throws if disposed.
		/// </summary>
		/// <exception cref="System.ObjectDisposedException"></exception>
		protected virtual void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		#endregion

		#region PROTECTED HELPER METHODS

		/// <summary>
		/// User userNames are distinct, and should never have two users with the same name
		/// </summary>
		/// <remarks>
		/// Can override to have different "distinct user details" implementation if necessary.
		/// </remarks>
		/// <param name="user"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected virtual async Task<bool> UserDetailsAlreadyExists(TUser user, CancellationToken cancellationToken = default(CancellationToken))
		{
			// if the result does exist, make sure its not for the same user object (ie same userName, but different Ids)
			var fBuilder = Builders<TUser>.Filter;
			var filter = fBuilder.Ne(x => x.Id, user.Id) & fBuilder.Eq(x => x.UserName, user.UserName);

			var result = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
			return result != null;
		}

		protected virtual TKey ConvertIdFromString(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return default(TKey);
			}
			return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(id);
		}

		protected virtual string ConvertIdToString(TKey id)
		{
			if (id == null || id.Equals(default(TKey)))
			{
				return null;
			}
			return id.ToString();
		}

		/// <summary>
		/// update sub-set of user details in database
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="update"></param>
		/// <param name="options"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected virtual Task<UpdateResult> DoUserDetailsUpdate(TKey userId, UpdateDefinition<TUser> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var filter = Builders<TUser>.Filter.Eq(x => x.Id, userId);
			return _collection.UpdateOneAsync(filter, update, options, cancellationToken);
		}

		protected virtual void EnsureClaimsNotNull(TUser user)
		{
			if (user.Claims == null) user.Claims = new List<IdentityClaim>();
		}

		protected virtual void EnsureRolesNotNull(TUser user)
		{
			if (user.Roles == null) user.Roles = new List<IdentityRole<TKey>>();
		}

		#endregion
	}
}
