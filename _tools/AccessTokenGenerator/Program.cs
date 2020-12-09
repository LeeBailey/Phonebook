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
            var userId = Guid.Parse(ReadLine());

            Console.WriteLine("Enter the token signing key");
            var signingKey = ReadLine();

            Console.WriteLine("Enter the token issuer");
            var issuer = ReadLine();

            Console.WriteLine("Enter the token audience");
            var audience = ReadLine();

            Console.WriteLine(GenerateToken(userId, signingKey, issuer, audience));
        }

        private static string ReadLine()
        {
            return Console.ReadLine() ?? string.Empty;
        }

        public static string GenerateToken(Guid userId, string signingKey, string issuer, string audience)
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
