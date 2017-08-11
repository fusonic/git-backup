using System;
using System.Text;

namespace Fusonic.GitBackup.Services
{
    internal static class TokenGenerator
    {
        public static string GenerateBase64Token(string username, string password)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
        }
    }
}