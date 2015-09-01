using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MongoDB.Driver;

namespace AspNet.Identity3.MongoDB
{
	public class RoleStore<TUser, TRole> : RoleStore<TUser, TRole, string>
		where TUser : IdentityUser<string>
		where TRole : IdentityRole<string>
	{
		public RoleStore(IdentityDatabaseContext<TUser, TRole, string> databaseContext) : base(databaseContext, null) { }

		public RoleStore(IdentityDatabaseContext<TUser, TRole, string> databaseContext, IdentityErrorDescriber describer) : base(databaseContext, describer) { }
	}

	public class RoleStore<TUser, TRole, TKey> :
		IQueryableRoleStore<TRole>,
		IRoleClaimStore<TRole>
		where TUser : IdentityUser<TKey>
		where TRole : IdentityRole<TKey>
		where TKey : IEquatable<TKey>
	{

		public RoleStore(IdentityDatabaseContext<TUser, TRole, TKey> databaseContext) : this(databaseContext, null) { }

		public RoleStore(IdentityDatabaseContext<TUser, TRole, TKey> databaseContext, IdentityErrorDescriber describer)
		{
			if (databaseContext == null) throw new ArgumentNullException(nameof(databaseContext));

			DatabaseContext = databaseContext;
			ErrorDescriber = describer ?? new IdentityErrorDescriber();
		}


		protected IdentityDatabaseContext<TUser, TRole, TKey> DatabaseContext { get; set; }

		/// <summary>
		/// Used to generate public API error messages
		/// </summary>
		protected IdentityErrorDescriber ErrorDescriber { get; set; }

		#region IRoleStore<TRole> (base interface for both IQueryableRoleStore<TRole> and IRoleClaimStore<TRole>)

		/// <summary>
		/// Creates a new role in a store as an asynchronous operation.
		/// </summary>
		/// <param name="role">The role to create in the store.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
		public virtual async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (role == null) throw new ArgumentNullException(nameof(role));
			if (await RoleDetailsAlreadyExists(role, cancellationToken)) return IdentityResult.Failed(ErrorDescriber.DuplicateRoleName(role.ToString()));

			try
			{
				await DatabaseContext.Roles.InsertOneAsync(role, cancellationToken);
			}
			catch(MongoWriteException)
			{
				return IdentityResult.Failed(ErrorDescriber.DuplicateRoleName(role.ToString()));
			}

			return IdentityResult.Success;
		}

		/// <summary>
		/// Updates a role in a store as an asynchronous operation.
		/// </summary>
		/// <param name="role">The role to update in the store.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
		public virtual async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (role == null) throw new ArgumentNullException(nameof(role));
			if (await RoleDetailsAlreadyExists(role, cancellationToken)) return IdentityResult.Failed(ErrorDescriber.DuplicateRoleName(role.ToString()));
			
			var filter = Builders<TRole>.Filter.Eq(x => x.Id, role.Id);
			var updateOptions = new UpdateOptions { IsUpsert = true};
			await DatabaseContext.Roles.ReplaceOneAsync(filter, role, updateOptions, cancellationToken);

			return IdentityResult.Success;
		}

		/// <summary>
		/// Deletes a role from the store as an asynchronous operation.
		/// </summary>
		/// <param name="role">The role to delete from the store.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> that represents the <see cref="IdentityResult"/> of the asynchronous query.</returns>
		public virtual async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (role == null) throw new ArgumentNullException(nameof(role));

			var filter = Builders<TRole>.Filter.Eq(x => x.Id, role.Id);
			await DatabaseContext.Roles.DeleteOneAsync(filter, cancellationToken);

			return IdentityResult.Success;
		}

		/// <summary>
		/// Gets the ID for a role from the store as an asynchronous operation.
		/// </summary>
		/// <param name="role">The role whose ID should be returned.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> that contains the ID of the role.</returns>
		public virtual Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (role == null) throw new ArgumentNullException(nameof(role));
			
			return Task.FromResult(ConvertIdToString(role.Id));
		}

		/// <summary>
		/// Gets the name of a role from the store as an asynchronous operation.
		/// </summary>
		/// <param name="role">The role whose name should be returned.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> that contains the name of the role.</returns>
		public virtual Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (role == null) throw new ArgumentNullException(nameof(role));
			
			return Task.FromResult(role.Name);
		}

		/// <summary>
		/// Get a role's normalized name as an asynchronous operation.
		/// </summary>
		/// <param name="role">The role whose normalized name should be retrieved.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> that contains the name of the role.</returns>
		public virtual Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (role == null) throw new ArgumentNullException(nameof(role));

			return Task.FromResult(role.NormalizedName);
		}

		/// <summary>
		/// Finds the role who has the specified ID as an asynchronous operation.
		/// </summary>
		/// <param name="roleId">The role ID to look for.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> that result of the look up.</returns>
		public virtual Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			TKey id = ConvertIdFromString(roleId);
			if (id == null) return Task.FromResult((TRole)null);

			var filter = Builders<TRole>.Filter.Eq(x => x.Id, id);
			var options = new FindOptions { AllowPartialResults = false };

			return DatabaseContext.Roles.Find(filter, options).SingleOrDefaultAsync(cancellationToken);
		}

		/// <summary>
		/// Finds the role who has the specified normalized name as an asynchronous operation.
		/// </summary>
		/// <param name="normalizedRoleName">The normalized role name to look for.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>A <see cref="Task{TResult}"/> that result of the look up.</returns>
		public virtual Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();

			if (string.IsNullOrWhiteSpace(normalizedRoleName)) return Task.FromResult((TRole)null);

			var filter = Builders<TRole>.Filter.Eq(x => x.NormalizedName, normalizedRoleName);
			var options = new FindOptions { AllowPartialResults = false };

			return DatabaseContext.Roles.Find(filter, options).SingleOrDefaultAsync(cancellationToken);
		}

		/// <summary>
		/// Sets the name of a role in the store as an asynchronous operation.
		/// </summary>
		/// <param name="role">The role whose name should be set.</param>
		/// <param name="roleName">The name of the role.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (role == null) throw new ArgumentNullException(nameof(role));

			role.Name = roleName;
			return Task.FromResult(0);
		}

		/// <summary>
		/// Set a role's normalized name as an asynchronous operation.
		/// </summary>
		/// <param name="role">The role whose normalized name should be set.</param>
		/// <param name="normalizedName">The normalized name to set</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
		public virtual Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (role == null) throw new ArgumentNullException(nameof(role));

			role.NormalizedName = normalizedName;
			return Task.FromResult(0);
		}

		#endregion
		
		#region IRoleClaimStore<TRole>

		/// <summary>
		///  Gets a list of <see cref="Claim"/>s to be belonging to the specified <paramref name="role"/> as an asynchronous operation.
		/// </summary>
		/// <param name="role">The role whose claims to retrieve.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> that represents the result of the asynchronous query, a list of <see cref="Claim"/>s.
		/// </returns>
		public virtual Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ThrowIfDisposed();
			if (role == null) throw new ArgumentNullException(nameof(role));

			IList<Claim> result = role.Claims == null ? new List<Claim>() : role.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
			return Task.FromResult(result);
		}

		/// <summary>
		/// Add a new claim to a role as an asynchronous operation.
		/// </summary>
		/// <param name="role">The role to add a claim to.</param>
		/// <param name="claim">The <see cref="Claim"/> to add.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public virtual async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (role == null) throw new ArgumentNullException(nameof(role));
			if (claim == null) throw new ArgumentNullException(nameof(claim));
			
			if (role.Claims == null) role.Claims = new List<IdentityClaim>();
			
			// claim and value already exist - just return
			if (role.Claims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value)) return;
			
			// new claim for the role
			var c = new IdentityClaim {ClaimType = claim.Type, ClaimValue = claim.Value};
			role.Claims.Add(c);

			// update role claims in the database
			var update = Builders<TRole>.Update.Push(x => x.Claims, c);
			await DoRoleDetailsUpdate(role.Id, update, null, cancellationToken);
		}

		/// <summary>
		/// Remove a claim from a role as an asynchronous operation.
		/// </summary>
		/// <param name="role">The role to remove the claim from.</param>
		/// <param name="claim">The <see cref="Claim"/> to remove.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be cancelled.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public virtual async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
		{
			ThrowIfDisposed();
			if (role == null) throw new ArgumentNullException(nameof(role));
			if (claim == null) throw new ArgumentNullException(nameof(claim));

			if (role.Claims == null)
			{
				role.Claims = new List<IdentityClaim>();
				return;
			}

			if (role.Claims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value))
			{
				var c = role.Claims.Single(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
				role.Claims.Remove(c);

				var update = Builders<TRole>.Update.Pull(x => x.Claims, c);
				await DoRoleDetailsUpdate(role.Id, update, null, cancellationToken);
			}
		}
		
		#endregion

		#region IQueryableRoleStore<TRole>

		/// <summary>
		/// WARNING: awaiting the mongoDB csharp driver to implement AsQueryable https://jira.mongodb.org/browse/CSHARP-935. In the mean time using ToList of the repos (http://stackoverflow.com/questions/29124995/is-asqueryable-method-departed-in-new-mongodb-c-sharp-driver-2-0rc).
		/// Returns an <see cref="IQueryable{T}"/> collection of roles.
		/// </summary>
		/// <value>An <see cref="IQueryable{T}"/> collection of roles.</value>
		public virtual IQueryable<TRole> Roles
		{
			get
			{
				// TODO: This is really rubbish
				//		awaiting the mongoDB csharp driver to implement AsQueryable
				//		https://jira.mongodb.org/browse/CSHARP-935
				//		Temporary list solution from http://stackoverflow.com/questions/29124995/is-asqueryable-method-departed-in-new-mongodb-c-sharp-driver-2-0rc
				ThrowIfDisposed();
				var filter = Builders<TRole>.Filter.Ne(x => x.Id, default(TKey));
				var list = DatabaseContext.Roles.Find(filter).ToListAsync().Result;

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
		/// Role names are distinct, and should never have two roles with the same name
		/// </summary>
		/// <remarks>
		/// Can override to have different "distinct role details" implementation if necessary.
		/// </remarks>
		/// <param name="role"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected virtual async Task<bool> RoleDetailsAlreadyExists(TRole role, CancellationToken cancellationToken = default(CancellationToken))
		{
			// if the result does exist, make sure its not for the same role object (ie same name, but different Ids)
			var fBuilder = Builders<TRole>.Filter;
			var filter = fBuilder.Ne(x => x.Id, role.Id) & fBuilder.Eq(x => x.Name, role.Name);

			var result = await DatabaseContext.Roles.Find(filter).FirstOrDefaultAsync(cancellationToken);
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
		/// update sub-set of role details in database
		/// </summary>
		/// <param name="roleId"></param>
		/// <param name="update"></param>
		/// <param name="options"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected virtual async Task<UpdateResult> DoRoleDetailsUpdate(TKey roleId, UpdateDefinition<TRole> update, UpdateOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			var filter = Builders<TRole>.Filter.Eq(x => x.Id, roleId);
			return await DatabaseContext.Roles.UpdateOneAsync(filter, update, options, cancellationToken);
		}

		#endregion
	}
}
