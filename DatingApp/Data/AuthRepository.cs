using System;
using System.Threading.Tasks;
using DatingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public DataContext Context { get; }
        public AuthRepository(DataContext context)
        {
            _context = context;
            

        }
        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Name == username);
            if(user == null)
                return null ;

            if(!VerifyPassword(password, user.PasswordHash ,user.PasswordSalt))
            return null;

           
            return user;
        }

        private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
        {
             using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for(int i =0 ; i < computedHash.Length;i++)
                {
                    if(computedHash[i] == passwordHash[i])
                    {
                        return true ;
                    }
                }
            }
            return false;
        }

        public async Task<User> Registration(User user, string password)
        {
            byte[] PasswordHash, PasswordSalt;
            CreatePasswordHash(password, out PasswordHash, out PasswordSalt);

            user.PasswordHash = PasswordHash;
            user.PasswordSalt = PasswordSalt;

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;


        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string username)
        {
           if(await _context.Users.AnyAsync(x => x.Name == username))
           {
               return true ;
           }
           return false;
        }
    }
}