using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Framework.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using Xunit;

namespace AspNet.Identity3.MongoDB.Tests
{
	public class IdentityDatabaseContextTests : IDisposable
	{
		private readonly IdentityDatabaseContext _databaseContext;
		
		public IdentityDatabaseContextTests(string collectionPrefix)
		{
			RegisterClassMap<IdentityUser, IdentityRole, string>.Init();

			collectionPrefix = $"{typeof(IdentityDatabaseContextTests).Name}_{collectionPrefix}";

			var configuration = new ConfigurationBuilder(".\\").AddJsonFile("config.json").Build();

			_databaseContext = new IdentityDatabaseContext
			{
				ConnectionString = configuration["Data:ConnectionString"],
				DatabaseName = "Testing",
				CreateCollectionIndexes = false
			};
			_databaseContext.UsersCollectionName = collectionPrefix + "_" + _databaseContext.UsersCollectionName;
			_databaseContext.RolesCollectionName = collectionPrefix + "_" + _databaseContext.RolesCollectionName;
		}

		public void Dispose()
		{
			_databaseContext.DeleteUserCollection();
			_databaseContext.DeleteRoleCollection();
		}

		public class EnsureUserCollectionCreatedMethod : IdentityDatabaseContextTests
		{
			public EnsureUserCollectionCreatedMethod() : base(typeof(EnsureUserCollectionCreatedMethod).Name) { }

			[Fact]
			public async Task When_collection_does_not_exists_then_should_create_collection()
			{
				// act
				_databaseContext.EnsureUserCollectionCreated();

				// assert
				Assert.True(await DoesCollectionExist(_databaseContext.Database, _databaseContext.UsersCollectionName));
			}

			[Fact]
			public async Task When_collection_already_exists_then_should_not_error()
			{
				// arrange
				await _databaseContext.Users.InsertOneAsync(new IdentityUser("When_collection_already_exists_then_should_not_error"));

				// act
				_databaseContext.EnsureUserCollectionCreated();

				// assert
				Assert.True(await DoesCollectionExist(_databaseContext.Database, _databaseContext.UsersCollectionName));
			}

			[Fact]
			public async Task When_collection_already_has_index_should_not_error()
			{
				// arrange
				await _databaseContext.Database.CreateCollectionAsync(_databaseContext.UsersCollectionName);
				// ensure NormalizedUserName index exists
				var index = Builders<IdentityUser>.IndexKeys.Ascending(x => x.NormalizedUserName);
				_databaseContext.Users.Indexes.CreateOneAsync(index, _databaseContext.CreateIndexOptions).Wait();
				Assert.True(await DoesIndexExist(_databaseContext.Users, "NormalizedUserName_1"));

				// act
				_databaseContext.EnsureUserCollectionCreated();

				// assert
				Thread.Sleep(100);
				Assert.True(await DoesIndexExist(_databaseContext.Users, "NormalizedUserName_1"));
			}

			[Theory]
			[InlineData("NormalizedUserName_1")]
			[InlineData("NormalizedEmail_1")]
			[InlineData("Logins_LoginProvider_1")]
			[InlineData("Roles_NormalizedName_1")]
			[InlineData("Claims_ClaimType_1_Roles_Claims_ClaimType_1")]
			public async Task Should_create_index(string expectedIndexName)
			{
				// act
				_databaseContext.EnsureUserCollectionCreated();

				// assert
				Thread.Sleep(100);
				Assert.True(await DoesIndexExist(_databaseContext.Users, expectedIndexName));
			}
		}

		public class EnsureRoleCollectionCreatedMethod : IdentityDatabaseContextTests
		{
			public EnsureRoleCollectionCreatedMethod() : base(typeof(EnsureRoleCollectionCreatedMethod).Name) { }

			[Fact]
			public async Task When_collection_does_not_exists_then_should_create_collection()
			{
				// act
				_databaseContext.EnsureRoleCollectionCreated();

				// assert
				Assert.True(await DoesCollectionExist(_databaseContext.Database, _databaseContext.RolesCollectionName));
			}

			[Fact]
			public async Task When_collection_already_exists_then_should_not_error()
			{
				// arrange
				await _databaseContext.Roles.InsertOneAsync(new IdentityRole("When_collection_already_exists_then_should_not_error"));

				// act
				_databaseContext.EnsureRoleCollectionCreated();

				// assert
				Assert.True(await DoesCollectionExist(_databaseContext.Database, _databaseContext.RolesCollectionName));
			}

			[Fact]
			public async Task When_collection_already_has_index_should_not_error()
			{
				// arrange
				await _databaseContext.Database.CreateCollectionAsync(_databaseContext.RolesCollectionName);
				// ensure NormalizedName index exists
				var index = Builders<IdentityRole>.IndexKeys.Ascending(x => x.NormalizedName);
				await _databaseContext.Roles.Indexes.CreateOneAsync(index, _databaseContext.CreateIndexOptions);
				Assert.True(await DoesIndexExist(_databaseContext.Roles, "NormalizedName_1"));

				// act
				_databaseContext.EnsureRoleCollectionCreated();

				// assert
				Thread.Sleep(100);
				Assert.True(await DoesIndexExist(_databaseContext.Roles, "NormalizedName_1"));
			}


			[Theory]
			[InlineData("NormalizedName_1")]
			public async Task Should_create_index(string expectedIndex)
			{
				// act
				_databaseContext.EnsureRoleCollectionCreated();

				// assert
				Thread.Sleep(100);
				Assert.True(await DoesIndexExist(_databaseContext.Roles, expectedIndex));
			}
		}

		public class DeleteUserCollectionMethod : IdentityDatabaseContextTests
		{
			public DeleteUserCollectionMethod() : base(typeof(DeleteUserCollectionMethod).Name) { }

			[Fact]
			public void When_collection_does_not_exist_then_should_not_error()
			{
				// act
				_databaseContext.DeleteUserCollection();
			}

			[Fact]
			public async Task When_collection_exists_then_should_drop_collection()
			{
				// arrange
				var userCollection = _databaseContext.Users;
				// add user to collection to instantiate it
				await userCollection.InsertOneAsync(new IdentityUser("When_collection_exists_then_should_drop_collection"));
				Assert.True(await DoesCollectionExist(_databaseContext.Database, _databaseContext.UsersCollectionName));
				
				// act
				_databaseContext.DeleteUserCollection();

				// assert
				Assert.False(await DoesCollectionExist(_databaseContext.Database, _databaseContext.UsersCollectionName));
			}
		}
		
		public class DeleteRoleCollectionMethod : IdentityDatabaseContextTests
		{
			public DeleteRoleCollectionMethod() : base(typeof(DeleteRoleCollectionMethod).Name) { }

			[Fact]
			public void When_collection_does_not_exist_then_should_not_error()
			{
				// act
				_databaseContext.DeleteRoleCollection();
			}

			[Fact]
			public async Task When_collection_exists_then_should_drop_collection()
			{
				// arrange
				var roleCollection = _databaseContext.Roles;
				// add role to collection to instantiate it
				await roleCollection.InsertOneAsync(new IdentityRole("When_collection_exists_then_should_drop_collection"));
				Assert.True(await DoesCollectionExist(_databaseContext.Database, _databaseContext.RolesCollectionName));

				// act
				_databaseContext.DeleteRoleCollection();

				// assert
				Assert.False(await DoesCollectionExist(_databaseContext.Database, _databaseContext.RolesCollectionName));
			}
		}
		
		private static async Task<bool> DoesCollectionExist(IMongoDatabase database, string collectionName)
		{
			var foundCollection = false;
			var cursor = await database.ListCollectionsAsync();

			await cursor.ForEachAsync(c =>
			{
				var name = c["name"].ToString();
				if (name.Equals(collectionName))
				{
					foundCollection = true;
				}
			});

			return foundCollection;
		}

		private static async Task<bool> DoesIndexExist<T>(IMongoCollection<T> collection, string indexName)
		{
			var foundIndex = false;
			var cursor = await collection.Indexes.ListAsync();

			await cursor.ForEachAsync(c =>
			{
				var name = c["name"].ToString();
				if (name.Equals(indexName))
				{
					foundIndex = true;
				}
			});
			
			return foundIndex;
		}
	}
}
