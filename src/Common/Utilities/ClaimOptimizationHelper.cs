

using System.IO.Compression;
using System.Text;

namespace Common.Utilities;

public class ClaimOptimizationHelper
{
    // Compress a list of roles using GZip compression
    public static string CompressRoles(List<string> roles)
    {
        string rolesString = string.Join(",", roles);
        byte[] originalBytes = Encoding.UTF8.GetBytes(rolesString);

        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                gzipStream.Write(originalBytes, 0, originalBytes.Length);
            }

            byte[] compressedBytes = memoryStream.ToArray();
            return Convert.ToBase64String(compressedBytes);
        }
    }

    // Decompress roles that were previously compressed
    public static List<string> DecompressRoles(string compressedRoles)
    {
        byte[] compressedBytes = Convert.FromBase64String(compressedRoles);
        List<string> roles = new List<string>();

        using (MemoryStream memoryStream = new MemoryStream(compressedBytes))
        {
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                using (StreamReader reader = new StreamReader(gzipStream))
                {
                    string rolesString = reader.ReadToEnd();
                    roles.AddRange(rolesString.Split(','));
                }
            }
        }

        return roles;
    }
}