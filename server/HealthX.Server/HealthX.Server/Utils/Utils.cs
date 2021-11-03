using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HealthX.Server {

    public static class Utils { 

        public static bool IsPasswordValid (string password, string sha1) {

            string password_sha1;

            try {
                byte[] pwd = Encoding.UTF8.GetBytes(password);
                password_sha1 = HexStringFromBytes(SHA1.Create().ComputeHash(pwd));
            }
            catch (Exception) {
                Program.Log(LoggingStatus.WARNING, "Password Utils", "Could not compute SHA1 of password");
                return false;
            }

            return password_sha1 == sha1;

        }

        public static string HexStringFromBytes(byte[] bytes) {
            var sb = new StringBuilder();
            foreach (byte b in bytes) {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

    }

}
