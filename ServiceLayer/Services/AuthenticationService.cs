using BusinessLayer.Entities;
using DataLayer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class AuthenticationService
    {
            private readonly UserContext _userContext;
            public AuthenticationService(UserContext userContext)
            {
                _userContext = userContext;
            }
            private string HashPassword(string password)
            {
                using (MD5 md5 = MD5.Create())
                {
                    var hashedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                }
            }

            private bool VerifyPassword(string password, string hashedPassword)
            {
                var hashedInput = HashPassword(password);
                return hashedInput.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
            }

            public async Task<User> Authenticate(string email, string password)
            {
                try
                {

                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    {
                        return null;
                    }
                    var user = await _userContext.GetByEmail(email);
                    if (user == null)
                    {
                        return null;
                    }
                    if (VerifyPassword(password, user.Password))
                    {
                        return user;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    throw new Exception("Authentication failed", ex);
                }
            }
    }
}
