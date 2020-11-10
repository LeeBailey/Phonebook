using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AccessTokenGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter a userId:");
            var userId = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter the token signing key");
            var signingKey = Console.ReadLine();

            Console.WriteLine("Enter the token issuer");
            var issuer = Console.ReadLine();

            Console.WriteLine("Enter the token audience key");
            var audience = Console.ReadLine();

            Console.WriteLine(GenerateToken(userId, signingKey, issuer, audience));
        }

        public static string GenerateToken(int userId, string signingKey, string issuer, string audience)
        {
            var claims = new List<Claim>() {
                new Claim("UserId", userId.ToString()),
            };

            var secretKey = new SymmetricSecurityKey(
               Encoding.UTF8.GetBytes(signingKey));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha512);

            var tokenOptions = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: signinCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }
    }
}
