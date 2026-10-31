using AIEngineConnectivity.Services;
using System.Security.Cryptography;
using System.Text;

namespace AIEngineGateway.EngineInfrastructure
{
    public sealed class EngineRecoveryKeyService : IEngineRecoveryKeyService
    {
        private const string Prefix = "AIE1-";
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        private const int KeyBytes = 32;
        private const int ChecksumBytes = 2;

        public string GenerateDisplayKey()
        {
            var raw = RandomNumberGenerator.GetBytes(KeyBytes);
            try
            {
                var checksum = SHA256.HashData(raw)[..ChecksumBytes];
                var text = Base32Encode([.. raw, .. checksum]);
                var grouped = string.Join("-", Enumerable.Range(0, (text.Length + 4) / 5)
                    .Select(i => text.Substring(i * 5, Math.Min(5, text.Length - i * 5))));
                return Prefix + grouped;
            }
            finally { CryptographicOperations.ZeroMemory(raw); }
        }

        public bool TryParse(string input, out byte[] raw)
        {
            raw = null;
            if (string.IsNullOrWhiteSpace(input)) return false;

            try
            {
                var clean = new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();

                if (!clean.StartsWith(Prefix, StringComparison.Ordinal)) return false;

                clean = clean[Prefix.Length..].Replace("-", "");
                var bytes = Base32Decode(clean);
                if (bytes.Length != KeyBytes + ChecksumBytes) return false;

                var candidate = bytes[..KeyBytes];
                var expected = SHA256.HashData(candidate)[..ChecksumBytes];
                if (!CryptographicOperations.FixedTimeEquals(expected, bytes.AsSpan(KeyBytes, ChecksumBytes)))
                    return false;

                raw = candidate;
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public string ComputeVerifier(byte[] raw, Guid id)
            => Convert.ToBase64String(HMACSHA256.HashData(raw, id.ToByteArray()));

        public bool Matches(string storedBase64, byte[] raw, Guid id)
        {
            try
            {
                var stored = Convert.FromBase64String(storedBase64);
                var computed = HMACSHA256.HashData(raw, id.ToByteArray());
                return CryptographicOperations.FixedTimeEquals(stored, computed);
            }
            catch (FormatException) { return false; }
        }

        private static string Base32Encode(byte[] data)
        {
            var sb = new StringBuilder(); int buffer = 0, bits = 0;
            foreach (var b in data)
            {
                buffer = (buffer << 8) | b; bits += 8;
                while (bits >= 5) { sb.Append(Alphabet[(buffer >> (bits - 5)) & 31]); bits -= 5; }
                buffer &= (1 << bits) - 1;
            }
            if (bits > 0) sb.Append(Alphabet[(buffer << (5 - bits)) & 31]);
            return sb.ToString();
        }

        private static byte[] Base32Decode(string s)
        {
            var output = new List<byte>(); int buffer = 0, bits = 0;
            foreach (var c in s)
            {
                int v = Alphabet.IndexOf(c);
                if (v < 0) throw new FormatException();
                buffer = (buffer << 5) | v; bits += 5;
                if (bits >= 8) { output.Add((byte)((buffer >> (bits - 8)) & 0xFF)); bits -= 8; }
                buffer &= (1 << bits) - 1;
            }
            return output.ToArray();
        }
    }
}