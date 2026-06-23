using System.Text;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;

// We shoulde use ( Konscious.Security.Cryptography.Argon2 ) NuGet for this scope.

// *********************************************************** Explain This Class ***********************************************************
// **                                                                                                                                      **
// **     This class create for : Hashing passwords and virify exact that passwords.                                                       **
// **                                                                                                                                      **
// **     Public Methodes :                                                                                                                **
// **                        => HashingPassword : Get string of password from user and make salt (16 byte) and hash (32 byte).             **
// **                           Combine them and convert to Base64 (64 character string) data type and return this string.                 **
// **                                                                                                                                      **
// **                        => VerifyPassword : Get two string input 1. sring password from user that we should check.                    **
// **                                                                 2. That Base64 password we hash before.                              **
// **                                            First convert Base64 password to byte.                                                    **
// **                                            After that make salt and hash from user password exactly like stored password.            **
// **                                            At the end use private helper method for compare every single byte from both of them.     **
// **                                            If they are same return true and if they are different return false.                      **
// **                                                                                                                                      **
// **                                                                                                                                      **
// **     Private Methodes :                                                                                                               **
// **                        => CompareByteArrays : Get three string input 1. byte of orginal password. (stored password)                  **
// **                                                                      2. byte of computed password. (string password from user)       **
// **                                                                      3. offset. (length of salt)                                     **
// **                                               Check all arrys and if thay are same return true to VerifyPassword method.             **
// **                                                                   if thay are different return false to VerifyPassword method.       **
// **                                                                                                                                      **
// **                                                                                                                                      **
// ****************************************************************************************************************************************** 

namespace MyUtilities
{
    public static class HashPassword
    {
        // This security setting is normal to strong. (if you want make more secure setting you can increase values)

        private const int SaltSize = 16;            // Size of hash string => (32 byte = 256 bit)  -  Salt use for prevention (Rainbow Table) attacks.
        private const int HashSize = 32;            // Size of hash string => (32 byte = 256 bit)
        private const int Iterations = 4;           // Number of repeat for Argon2id. (more value = high security, less value = low security)
        private const int MemorySize = 1024 * 64;   // Memory usage need for hashing. we use (64 MB) for this method.
        private const int Parallelism = 8;          // Number of thraed in proccess.

        /// <summary>
        /// Generates a secure Argon2id hash for the given password.
        /// </summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>A Base64 encoded string representing the salt and the computed hash.</returns>
        /// <remarks>
        /// The generated string contains both the salt and the hash, necessary for verification.
        /// Uses Argon2id with specified parameters for strong security.
        /// </remarks>
        public static string HashingPassword (string password)
        {
            // Create 16 byte arry and fill it randomly for salt.
            byte[] saltBytes = new byte[SaltSize];
            RandomNumberGenerator.Fill(saltBytes);

            // Password convert to UTF8 byte.
            // Create argon2 with Argon2id type and fill it with our random salt and other value.
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = saltBytes,                   // Our created salt.
                DegreeOfParallelism = Parallelism,  // Ram usage.
                Iterations = Iterations,            // Repeat number.
                MemorySize = MemorySize             // Thread number.
            };

            // Create 32 byte arry for hash with Argon2id.
            byte[] hashBytes = argon2.GetBytes(HashSize);

            // Combine salt with hash.
            var hashWithSalt = new byte[saltBytes.Length + hashBytes.Length];
            Buffer.BlockCopy(saltBytes, 0, hashWithSalt, 0, saltBytes.Length);
            Buffer.BlockCopy(hashBytes, 0, hashWithSalt, saltBytes.Length, hashBytes.Length);
            // Output ==> [ salt (16 byte) | hash (32 byte) ]

            // Return (salt + hash) to string. (64 character string)
            return Convert.ToBase64String(hashWithSalt);
        }

        /// <summary>
        /// Verifies if the provided plain-text password matches the stored Argon2id hash.
        /// </summary>
        /// <param name="password">The plain-text password to verify.</param>
        /// <param name="storedHash">The Base64 encoded string containing the salt and the previously generated hash.</param>
        /// <returns>True if the password matches the hash, false otherwise.</returns>
        /// <remarks>
        /// Handles potential formatting errors in storedHash and ensures minimum length.
        /// </remarks>
        public static bool VerifyPassword(string password, string storedHash)
        {
            // Check if our storedHash is null or empty return false. (verification valid input)
            if (string.IsNullOrEmpty(storedHash))
                return false;

            // Created empty arry for get password that stored with HashingPassword method.
            byte[] hashWithSalt;
            try
            {
                // Convert Base64 string (64 character string) that we create with HashingPassword to byte.
                hashWithSalt = Convert.FromBase64String(storedHash);
            }
            catch (FormatException)
            {
                // If format was not Base64 return false.
                // Because we stored password to Base64.
                return false;
            }

            // Check if Length of our stored password is less than (48) return false;
            // Because we stored 16 byte salt and 32 byte hash.
            if (hashWithSalt.Length < SaltSize + HashSize)
                return false;


            // Extract salt from our stored password.
            // Salt is first 16 byte of stored password.
            byte[] saltBytes = new byte[SaltSize];
            Buffer.BlockCopy(hashWithSalt, 0, saltBytes, 0, saltBytes.Length);

            // Hash exactly passworld like we do befor for compare.
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = saltBytes,
                DegreeOfParallelism = Parallelism,
                Iterations = Iterations,
                MemorySize = MemorySize
            };
            byte[] computedHash = argon2.GetBytes(HashSize);

            // We send data to private helper method to compare between stored password and input passwored.
            return CompareByteArrays(hashWithSalt, computedHash, saltBytes.Length);
        }


        //-------------------------------------------------------------------------- Private Methodes --------------------------------------------------------------------------//


        /// <summary>
        /// Compares two byte arrays in a way that is resistant to timing attacks.
        /// </summary>
        /// <param name="originalHash">The combined salt and hash bytes.</param>
        /// <param name="computedHash">The newly computed hash bytes.</param>
        /// <param name="offset">The offset in originalHash where the salt ends and the hash begins. ( offset is saltBytes.Length )</param>
        /// <returns>True if the computed hash matches the relevant part of the original hash, false otherwise.</returns>
        private static bool CompareByteArrays(byte[] originalHash, byte[] computedHash, int offset)
        {
            // Get Lenght of input password.
            int hashLength = computedHash.Length;

            // Create boolian data for return to main method (VerifyPassword).
            // The default return is true.
            bool isMatch = true;

            // Chech every single byte of both stored password and new input password.
            // If even 1 byte is different return false.
            for (int i = 0; i < hashLength; i++)
            {
                if (originalHash[i + offset] != computedHash[i])
                    isMatch = false;
            }

            // After check all byte return {
            //                               True => if they both same.
            //                               False => if they have different.
            //                             }
            // Return resault to VerifyPassword Method.
            return isMatch;
        }
    }
}
