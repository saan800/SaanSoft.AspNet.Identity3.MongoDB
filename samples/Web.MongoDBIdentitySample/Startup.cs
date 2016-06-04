using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using SaanSoft.AspNet.Identity3.MongoDB;
using Web.MongoDBIdentitySample.Models;
using Web.MongoDBIdentitySample.Services;

namespace Web.MongoDBIdentitySample
{
	public class Startup {
		public Startup(IHostingEnvironment env) {
			// Set up configuration sources.
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.WebRootPath)
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();

			if(env.IsDevelopment()) {
				// For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
				builder.AddUserSecrets();
			}

			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; set; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {
			// Registers MongoDB conventions for ignoring default and blank fields
			// NOTE: if you have registered default conventions elsewhere, probably don't need to do this
			RegisterClassMap<ApplicationUser, IdentityRole, string>.Init();

			// Add Mongo Identity services to the services container.
			// registers all of the interfaces it implements to the services as "Scoped"
			services.AddIdentity<ApplicationUser, IdentityRole>()
					.AddMongoDBIdentityStores<ApplicationDbContext, ApplicationUser, IdentityRole, string>(options => {
						options.ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];        // No default, must be configured if using (eg "mongodb://localhost:27017" in this project's appsettings.json)
																													// Only required if need to create [Client] from the connection string
																													// If you are providing the [Client] or [Database] or [UserCollection] and [RoleCollection] options instead, don't need to supply [ConnectionString]
																													// options.Client = [IMongoClient];									// Defaults to: uses either Client attached to [Database] (if supplied), otherwise it creates a new client using [ConnectionString]
																													// options.DatabaseName = [string];									// Defaults to: "AspNetIdentity"
																													// options.Database = [IMongoDatabase];								// Defaults to: Creating Database using [DatabaseName] and [Client]

						// options.UserCollectionName = [string];							// Defaults to: "AspNetUsers"
						// options.RoleCollectionName = [string];							// Defaults to: "AspNetRoles"
						// options.UserCollection = [IMongoCollection<TUser>];				// Defaults to: Creating user collection in [Database] using [UserCollectionName] and [CollectionSettings]
						// options.RoleCollection = [IMongoCollection<TRole>];				// Defaults to: Creating user collection in [Database] using [RoleCollectionName] and [CollectionSettings]
						// options.CollectionSettings = [MongoCollectionSettings];			// Defaults to: { WriteConcern = WriteConcern.WMajority } => Used when creating default [UserCollection] and [RoleCollection]

						// options.EnsureCollectionIndexes = [bool];						// Defaults to: false => Used to ensure the User and Role collections have been created in MongoDB and indexes assigned. Only runs on first calls to user and role collections.
						// options.CreateCollectionOptions = [CreateCollectionOptions];		// Defaults to: { AutoIndexId = true } => Used when [EnsureCollectionIndexes] is true and User or Role collections need to be created.
						// options.CreateIndexOptions = [CreateIndexOptions];				// Defaults to: { Background = true, Sparse = true } => Used when [EnsureCollectionIndexes] is true and any indexes need to be created.
					})
					.AddDefaultTokenProviders();

			// Add MVC services to the services container.
			services.AddMvc();

			// Add application services.
			services.AddTransient<IEmailSender, AuthMessageSender>();
			services.AddTransient<ISmsSender, AuthMessageSender>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			if(env.IsDevelopment()) {
				//app.UseBrowserLink();
				app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions { SourceCodeLineCount = 20 });
			}
			else {
				app.UseExceptionHandler("/Home/Error");
			}

			//app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

			app.UseStaticFiles();

			app.UseIdentity();

			// To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715

			app.UseMvc(routes => {
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}

		// Entry point for the application.
		public static void Main(string[] args) {
			var host = new WebHostBuilder()
						.UseKestrel()
						.UseContentRoot(Directory.GetCurrentDirectory())
						.UseConfiguration(new ConfigurationBuilder()
							.AddJsonFile("hosting.json", optional: true)
							.AddEnvironmentVariables(prefix: "ASPNETCORE_")
							.AddCommandLine(args)
							.Build()
						)
						.UseIISIntegration()
						.UseStartup<Startup>()
						.Build();

			host.Run();
		}
	}
}
