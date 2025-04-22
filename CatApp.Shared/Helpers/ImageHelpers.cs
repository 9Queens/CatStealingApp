using System.Security.Cryptography;

namespace CatApp.Shared.Helpers
{
    /// <summary>
    /// For extracking the hash out the images (to be in position to check against already stored images for uniqueness)
    /// </summary>
    public static class ImageHelpers
    {
        public static string ComputeSHA256(byte[] imageBytes)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(imageBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
