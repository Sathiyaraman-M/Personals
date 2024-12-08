using Expensive.Infrastructure.Abstractions.Utilities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Expensive.Users.Utilities;

public class PasswordHasher : IPasswordHasher
{
    private readonly RandomNumberGenerator _randomNumberGenerator = RandomNumberGenerator.Create();

    public string HashPassword(string password)
    {
        var hashBytes = HashPasswordInternal(password, _randomNumberGenerator,
            prf: KeyDerivationPrf.HMACSHA512,
            iterCount: 100_000,
            saltSize: 128 / 8,
            numBytesRequested: 256 / 8);
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        var decodedHashedPassword = Convert.FromBase64String(hashedPassword);
        if (decodedHashedPassword.Length == 0)
        {
            return false;
        }

        try
        {
            // Read header information
            var prf = (KeyDerivationPrf)ReadNetworkByteOrder(decodedHashedPassword, 1);
            var iterationCount = (int)ReadNetworkByteOrder(decodedHashedPassword, 5);
            var saltLength = (int)ReadNetworkByteOrder(decodedHashedPassword, 9);

            // Read the salt: must be >= 128 bits
            if (saltLength < 128 / 8)
            {
                return false;
            }

            var salt = new byte[saltLength];
            Buffer.BlockCopy(decodedHashedPassword, 13, salt, 0, salt.Length);

            // Read the subKey (the rest of the payload): must be >= 128 bits
            var subKeyLength = decodedHashedPassword.Length - 13 - salt.Length;
            if (subKeyLength < 128 / 8)
            {
                return false;
            }

            var expectedSubKey = new byte[subKeyLength];
            Buffer.BlockCopy(decodedHashedPassword, 13 + salt.Length, expectedSubKey, 0, expectedSubKey.Length);

            // Hash the incoming password and verify it
            var actualSubKey = KeyDerivation.Pbkdf2(providedPassword, salt, prf, iterationCount, subKeyLength);
            return CryptographicOperations.FixedTimeEquals(actualSubKey, expectedSubKey);
        }
        catch
        {
            // This should never occur except in the case of a malformed payload, where
            // we might go off the end of the array. Regardless, a malformed payload
            // implies verification failed.
            return false;
        }
    }

    private static byte[] HashPasswordInternal(string password, RandomNumberGenerator rng, KeyDerivationPrf prf,
        int iterCount, int saltSize, int numBytesRequested)
    {
        var salt = new byte[saltSize];
        rng.GetBytes(salt);
        var subKey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

        var outputBytes = new byte[13 + salt.Length + subKey.Length];
        outputBytes[0] = 0x01;
        WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
        WriteNetworkByteOrder(outputBytes, 5, (uint)iterCount);
        WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
        Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
        Buffer.BlockCopy(subKey, 0, outputBytes, 13 + saltSize, subKey.Length);
        return outputBytes;
    }

    private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
    {
        return ((uint)buffer[offset + 0] << 24)
               | ((uint)buffer[offset + 1] << 16)
               | ((uint)buffer[offset + 2] << 8)
               | buffer[offset + 3];
    }

    private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
    {
        buffer[offset + 0] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)(value >> 0);
    }
}