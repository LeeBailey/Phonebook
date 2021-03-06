﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Phonebook.Domain.Model.ValueObjects;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Phonebook.Api.Tests.TestFramework
{
    internal static class TestSetup
    {
        private static readonly Random _random = new Random();
        private static readonly string _authJwtTokenSigningKey = "*G-KaPdSgVkYp3s5v8y/B?E(H+MbQeThWmZq4t7w!z$C&F)J@NcRfUjXn2r5u8x/";
        private static readonly string _authJwtTokenAudience = "youraudience.com";
        private static readonly string _authJwtTokenIssuer = "yourissuer.com";

        public static IHost CreateHost()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> 
                    { 
                        { "Authorization:JwtTokenSigningKey", _authJwtTokenSigningKey },
                        { "Authorization:JwtTokenAudience", _authJwtTokenAudience },
                        { "Authorization:JwtTokenIssuer", _authJwtTokenIssuer },
                        { "CorsAllowedOrigins:1", "http://localhost" },
                        { "CorsAllowedOrigins:2", "http://localhost:3000" }
                    })
                .Build();

            var hostBuilder = new HostBuilder()
                .ConfigureWebHostDefaults(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.UseStartup<TestServerStartup>().UseConfiguration(config);
                });

            return hostBuilder.Start();
        }

        public static string GenerateToken(string? userId)
        {
            var claims = new List<Claim>();

            if (userId is not null)
            {
                claims.Add(new Claim("UserId", userId));
            }

            var secretKey = new SymmetricSecurityKey(
               Encoding.UTF8.GetBytes(_authJwtTokenSigningKey));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha512);

            var tokenOptions = new JwtSecurityToken(
                issuer: _authJwtTokenIssuer,
                audience: _authJwtTokenAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: signinCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        public static HttpRequestMessage CreateHttpRequestMessage(
            string requestUri,
            IDictionary<string, string>? postData = null,
            string? originHeaderValue = null)
        {
            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(requestUri)
            };

            if (postData is not null)
            {
                httpRequest.Method = HttpMethod.Post;

                httpRequest.Content = new StringContent(
                    JsonSerializer.Serialize(postData),
                    Encoding.UTF8,
                    "application/json");
            }

            httpRequest.Headers.Add("Origin", originHeaderValue ?? "http://localhost");

            return httpRequest;
        }

        public static HttpRequestMessage CreateHttpRequestMessage(
        string requestUri,
        Guid userId,
        IDictionary<string, string>? postData = null,
        string? originHeaderValue = null)
        {
            var httpRequest = CreateHttpRequestMessage(requestUri, GenerateToken(userId.ToString()!), postData , originHeaderValue);

            return httpRequest;
        }

        public static HttpRequestMessage CreateHttpRequestMessage(
            string requestUri,
            string? authorizationHeaderValue,
            IDictionary<string, string>? postData = null,
            string? originHeaderValue = null)
        {
            var httpRequest = CreateHttpRequestMessage(requestUri, postData, originHeaderValue);

            if (authorizationHeaderValue is not null)
            {
                httpRequest.Headers.Add("Authorization", $"Bearer {authorizationHeaderValue}");
            }

            return httpRequest;
        }

        public static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public static int GetRandomInt()
        {
            return _random.Next();
        }

        public static PhoneNumber GetRandomPhoneNumber()
        {
            const string chars = "0123456789 ";

            return new PhoneNumber(
                new string(Enumerable.Repeat(chars, 10)
                    .Select(s => s[_random.Next(s.Length)]).ToArray()));
        }
    }
}

