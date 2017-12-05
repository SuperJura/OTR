using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OTR
{
    public static class MAC
    {
        public static bool SignFile(byte[] key, String sourceFile, String destFile)
        {
            if (!File.Exists(sourceFile))
            {
                return false;
            }

            // Initialize the keyed hash object.
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {

                using (FileStream inStream = new FileStream(sourceFile, FileMode.Open))
                {
                    using (FileStream outStream = new FileStream(destFile, FileMode.Create))
                    {
                        // Compute the hash of the input file.
                        byte[] hashValue = hmac.ComputeHash(inStream);
                        // Reset inStream to the beginning of the file.
                        inStream.Position = 0;
                        // Write the computed hash value to the output file.
                        outStream.Write(hashValue, 0, hashValue.Length);
                        // Copy the contents of the sourceFile to the destFile.
                        int bytesRead;
                        // read 1K at a time
                        byte[] buffer = new byte[1024];
                        do
                        {
                            // Read from the wrapping CryptoStream.
                            bytesRead = inStream.Read(buffer, 0, 1024);
                            outStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead > 0);
                    }
                }
            }

            return false;
        }

        public static byte[] Sign(byte[] key, String input)
        {
            
            // Initialize the keyed hash object.
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                // Compute the hash of the input file.
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
        }

        public static bool Verify(byte[] key, byte[] computeHash)
        {
            // Initialize the keyed hash object.
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                // Create an array to hold the keyed hash value read from the file.
                byte[] StoredHash = new byte[hmac.HashSize / 8];
                Array.Copy(computeHash, StoredHash, StoredHash.Length);               
                for (int i = 0; i < StoredHash.Length; i++)
                {
                    if (computeHash[i] != StoredHash[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        public static bool VerifyFile(byte[] key, String sourceFile)
        {
            bool err = false;
            if (!File.Exists(sourceFile))
            {
                return false;
            }
            // Initialize the keyed hash object. 
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                // Create an array to hold the keyed hash value read from the file.
                byte[] storedHash = new byte[hmac.HashSize / 8];
                // Create a FileStream for the source file.
                using (FileStream inStream = new FileStream(sourceFile, FileMode.Open))
                {
                    // Read in the storedHash.
                    inStream.Read(storedHash, 0, storedHash.Length);
                    // Compute the hash of the remaining contents of the file.
                    // The stream is properly positioned at the beginning of the content, 
                    // immediately after the stored hash value.
                    byte[] computedHash = hmac.ComputeHash(inStream);
                    // compare the computed hash with the stored value

                    for (int i = 0; i < storedHash.Length; i++)
                    {
                        if (computedHash[i] != storedHash[i])
                        {
                            err = true;
                        }
                    }
                }
            }
            if (err)
            {
                return false;
            }
            else
            {
                return true;
            }

        } //end VerifyFile
    }
}
