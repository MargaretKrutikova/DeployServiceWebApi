﻿using System;
using System.IO;
using System.Text;
using DeploymentSettings;
using DeploymentSettings.Json;
using DeployServiceWebApi.Exceptions;
using DeployServiceWebApi.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace DeployServiceWebApi
{
    public static class ApplicationBuilderExtensions
    {
	    public static void UseDeploymentSettingsDataInitializer(this IApplicationBuilder app)
	    {
		    var dataStoreService = app.ApplicationServices.GetRequiredService<IDeploymentSettingsDataStore>();

		    var configOptions = app.ApplicationServices.GetRequiredService<IOptions<ConfigurationOptions>>();
		    var settingsPath = configOptions.Value.DeploySettingsPath;

		    var settingsJson = JsonConvert.DeserializeObject<GlobalDeploymentSettingsJson>(
			    File.ReadAllText(settingsPath));

		    dataStoreService.InitializeData(settingsJson);
	    }

	    public static IApplicationBuilder UseExceptionHandlingMiddleware(
		    this IApplicationBuilder app)
	    {
		    return app.UseMiddleware<ExceptionHandlingMiddleware>();
	    }

		public static IApplicationBuilder UseJwtBearerAuthenticationWithCustomJwtValidation(
		    this IApplicationBuilder app)
		{
			var jwtOptions = app.ApplicationServices.GetRequiredService<IOptions<JwtOptions>>();

			// secretKey contains a secret passphrase only your server knows
			var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.Value.SignatureKey));

			// information that is used to validate the token
			var tokenValidationParameters = new TokenValidationParameters
			{
				// The signing key must match!
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = signingKey,
			
				// Validate the JWT Issuer (iss) claim
				ValidateIssuer = true,
				ValidIssuer = jwtOptions.Value.Issuer,
			
				// Validate the JWT Audience (aud) claim
				ValidateAudience = true,
				ValidAudience = jwtOptions.Value.Audience,
			
				// Validate the token expiry
				ValidateLifetime = true,
			
				// If you want to allow a certain amount of clock drift, set that here:
				ClockSkew = TimeSpan.Zero
			};
			
			return app.UseJwtBearerAuthentication(new JwtBearerOptions
			{
				AutomaticAuthenticate = true,
				AutomaticChallenge = true,
				TokenValidationParameters = tokenValidationParameters
			});
		}
	}
}