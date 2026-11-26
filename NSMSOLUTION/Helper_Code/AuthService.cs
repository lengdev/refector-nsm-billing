using Microsoft.AspNet.Identity;
using NSMSOLUTION.Data;
using NSMSOLUTION.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using static NSMSOLUTION.Models.UserDTOs;

namespace NSMSOLUTION.Helper_Code
{
    public static class AuthService
    {
        private static readonly PasswordHasher _passwordHasher = new PasswordHasher();

        /// <summary>
        /// Register new user with Username + PhoneNumber + Password
        /// </summary>
        public static async Task<Result<AspNetUser>> RegisterAsync(RegisterDto dto)
        {
            using (var db = new AppDbContext())
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.Username))
                        return Result<AspNetUser>.Fail("Username is required");

                    if (string.IsNullOrWhiteSpace(dto.Password))
                        return Result<AspNetUser>.Fail("Password is required");

                    if (db.Users.Any(u => u.PhoneNumber == dto.PhoneNumber))
                        return Result<AspNetUser>.Fail("Phone number already registered");

                    var user = new AspNetUser
                    {
                        Id = Guid.NewGuid(),
                        CustomerID = dto.CustomerID,
                        UserName = dto.Username.Trim(),
                        PhoneNumber = dto.PhoneNumber?.Trim(),
                        Email = $"{Guid.NewGuid()}@nsm.local",
                        PasswordHash = _passwordHasher.HashPassword(dto.Password),
                        CreatedAt = DateTime.UtcNow
                    };

                    db.Users.Add(user);
                    await db.SaveChangesAsync();

                    return Result<AspNetUser>.Ok(user);
                }
                catch (DbEntityValidationException ex)
                {
                    var errors = ex.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
                    var msg = string.Join("; ", errors);
                    return Result<AspNetUser>.Fail("Validation failed: " + msg);
                }
                catch (Exception ex)
                {
                    return Result<AspNetUser>.Fail("Registration failed: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Delete a user by Id
        /// </summary>
        public static async Task DeleteUserAsync(Guid id)
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                    throw new Exception("User not found");

                db.Users.Remove(user);
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Get all users
        /// </summary>
        public static List<AspNetUser> GetAllUsers()
        {
            using (var db = new AppDbContext())
            {
                return db.Users.ToList();
            }
        }

        // Optional: Uncomment and convert your other methods to static as needed below.

        /*
        public static async Task<string> LoginAsync(LoginDto dto)
        {
            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.UserName == dto.UserName);
                if (user == null)
                    throw new Exception("User not found");

                var result = _passwordHasher.VerifyHashedPassword(user.PasswordHash, dto.Password);
                if (result == PasswordVerificationResult.Failed)
                    throw new Exception("Invalid password");

                string key = ConfigurationManager.AppSettings["JwtKey"];
                string issuer = ConfigurationManager.AppSettings["JwtIssuer"];
                string audience = ConfigurationManager.AppSettings["JwtAudience"];
                double expireHours = double.Parse(ConfigurationManager.AppSettings["JwtExpireHours"]);

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? "")
                    }),
                    Expires = DateTime.UtcNow.AddHours(expireHours),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
        }
        */
    }
}
