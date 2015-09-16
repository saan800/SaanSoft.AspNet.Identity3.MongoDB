using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Facebook;
using Microsoft.AspNet.Authentication.Google;
using Microsoft.AspNet.Authentication.MicrosoftAccount;
using Microsoft.AspNet.Authentication.Twitter;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Runtime;
using AspNet.Identity3.MongoDB;
using Web.MongoDBIdentitySample.Models;
using Web.MongoDBIdentitySample.Services;

namespace Web.MongoDBIdentitySample
{
	public class Startup
	{
		public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
		{
			// Setup configuration sources.

			var builder = new ConfigurationBuilder(appEnv.ApplicationBasePath)
				.AddJsonFile("config.json")
				.AddJsonFile($"config.{env.EnvironmentName}.json", optional: true);

			if (env.IsDevelopment())
			{
				// This reads the configuration keys from the secret store.
				// For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
				builder.AddUserSecrets();
			}
			builder.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfiguration Configuration { get; set; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Register identity mapping configuration for MongoDB
			RegisterClassMap<ApplicationUser, IdentityRole, string>.Init();

			// Add Mongo Identity services to the services container.
			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddMongoDBIdentityStores<ApplicationDbContext, ApplicationUser, IdentityRole, string>(options =>
				{
					options.ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];        // No default, must be configured if using (eg "mongodb://localhost:27017")
					// options.Client = [IMongoClient];									Defaults to: either Client attached to [Database] or creates a new client using [ConnectionString]
					// options.DatabaseName = [string];									Defaults to: "AspNetIdentity"
					// options.Database = [IMongoDatabase];								Defaults to: Creating Database using [DatabaseName] and [Client]

					// options.UserCollectionName = [string];							Defaults to: "AspNetUsers"
					// options.RoleCollectionName = [string];							Defaults to: "AspNetRoles"
					// options.UserCollection = [IMongoCollection<TUser>];				Defaults to: Creating user collection in [Database] using [UserCollectionName]
					// options.RoleCollection = [IMongoCollection<TRole>];				Defaults to: Creating user collection in [Database] using [RoleCollectionName]
					// options.CollectionSettings = [MongoCollectionSettings];			Defaults to: { WriteConcern = WriteConcern.WMajority } => Used when creating default [UserCollection] and [RoleCollection]
					
					// options.EnsureCollectionIndexes = [bool];						Defaults to: false => Used to ensure the User and Role collections have been created in MongoDB and indexes assigned. Only runs on first call to user and/or role collection.
					// options.CreateCollectionOptions = [CreateCollectionOptions];		Defaults to: { AutoIndexId = true } => Used when [EnsureCollectionIndexes] is true and any collections need to be created.
					// options.CreateIndexOptions = [CreateIndexOptions];				Defaults to: { Background = true, Sparse = true } => Used when [EnsureCollectionIndexes] is true and any indexes need to be created.
				})
				.AddDefaultTokenProviders();
			

			// Configure the options for the authentication middleware.
			// You can add options for Google, Twitter and other middleware as shown below.
			// For more information see http://go.microsoft.com/fwlink/?LinkID=532715
			services.Configure<FacebookAuthenticationOptions>(options =>
			{
				options.AppId = Configuration["Authentication:Facebook:AppId"];
				options.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
			});

			services.Configure<MicrosoftAccountAuthenticationOptions>(options =>
			{
				options.ClientId = Configuration["Authentication:MicrosoftAccount:ClientId"];
				options.ClientSecret = Configuration["Authentication:MicrosoftAccount:ClientSecret"];
			});

			// Add MVC services to the services container.
			services.AddMvc();

			// Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
			// You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
			// services.AddWebApiConventions();

			// Register application services.
			services.AddTransient<IEmailSender, AuthMessageSender>();
			services.AddTransient<ISmsSender, AuthMessageSender>();
		}

		// Configure is called after ConfigureServices is called.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.MinimumLevel = LogLevel.Information;
			loggerFactory.AddConsole();

			// Configure the HTTP request pipeline.

			// Add the following to the request pipeline only in development environment.
			if (env.IsDevelopment())
			{
				app.UseBrowserLink();
				app.UseErrorPage(new ErrorPageOptions { SourceCodeLineCount = 20});
			}
			else
			{
				// Add Error handling middleware which catches all application specific errors and
				// sends the request to the following path or controller action.
				app.UseErrorHandler("/Home/Error");
			}

			// Add static files to the request pipeline.
			app.UseStaticFiles();

			// Add cookie-based authentication to the request pipeline.
			app.UseIdentity();

			// Add authentication middleware to the request pipeline. You can configure options such as Id and Secret in the ConfigureServices method.
			// For more information see http://go.microsoft.com/fwlink/?LinkID=532715
			// app.UseFacebookAuthentication();
			// app.UseGoogleAuthentication();
			// app.UseMicrosoftAccountAuthentication();
			// app.UseTwitterAuthentication();

			// Add MVC to the request pipeline.
			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");

				// Uncomment the following line to add a route for porting Web API 2 controllers.
				// routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
			});
		}
	}
}
