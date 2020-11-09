using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Phonebook.Domain.ApplicationServices.Commands;
using Phonebook.Domain.ApplicationServices.Queries;
using Phonebook.Domain.Infrastructure.Abstractions.EntityPersistance;
using Phonebook.Infrastructure.EntityPersistance;
using System.Linq;
using System.Text;

namespace PhoneBook.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly string AllowSpecificOriginsPolicy = "AllowSpecificOriginsPolicy";

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: AllowSpecificOriginsPolicy,
                    builder =>
                    {
                        builder.WithOrigins(
                            _configuration.GetSection("CorsAllowedOrigins")
                                .GetChildren().Select(x => x.Value).ToArray());
                    });
            });

            services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);
            services.AddControllers();

            services.AddSingleton<GetPhonebookContactsQuery>();
            services.AddSingleton<CreateNewContactCommand>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.IncludeErrorDetails = true;
                options.RequireHttpsMetadata = false;
                options.Audience = _configuration.GetValue<string>("Authorization:JwtTokenAudience");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration.GetValue<string>("Authorization:JwtTokenIssuer"),
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Authorization:JwtTokenSigningKey"))),
                };
            });

            ConfigureInfrastructureServices(services);
        }

        protected virtual void ConfigureInfrastructureServices(IServiceCollection services)
        {
            services.AddSingleton<IPhonebookDbContextFactory>(
                x => new PhonebookDbContextFactory(_configuration.GetConnectionString("PhonebookDbConnection")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(AllowSpecificOriginsPolicy);

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
