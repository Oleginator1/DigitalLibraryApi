using CsvHelper;
using System.Globalization;

namespace DigitalLibraryApi.Middleware
{
    public static class CsvFileValidator
    {
        private const long MaxFileSize = 2 * 1024 * 1024; // 2MB

        public static (bool isValid, string? errorMessage) Validate(IFormFile file)
        {
            // 1️⃣ File existence
            if (file == null || file.Length == 0)
                return (false, "No file uploaded or file is empty.");

            // 2️⃣ Extension validation
            if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
                return (false, "Invalid file extension. Only .csv files are allowed.");

            // 3️⃣ MIME type validation
            if (file.ContentType != "text/csv" && file.ContentType != "application/vnd.ms-excel")
                return (false, "Invalid file type. Only CSV files are supported.");

            // 4️⃣ Size validation
            if (file.Length > MaxFileSize)
                return (false, "File size exceeds the 2MB limit.");

            // 5️⃣ Header validation
            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                var headers = csv.HeaderRecord;

                var requiredHeaders = new[] { "Title", "Author", "ISBN", "Year", "Description", "CategoryId" };

                // Check that all required headers exist
                var missingHeaders = requiredHeaders.Except(headers, StringComparer.OrdinalIgnoreCase).ToList();
                if (missingHeaders.Any())
                    return (false, $"Missing required columns: {string.Join(", ", missingHeaders)}");

                // Optional: Check for extra columns if you want strict schema enforcement
                // var extraHeaders = headers.Except(requiredHeaders, StringComparer.OrdinalIgnoreCase).ToList();
                // if (extraHeaders.Any())
                //     return (false, $"Unexpected columns present: {string.Join(", ", extraHeaders)}");
            }

            return (true, null); // ✅ All good
        }
    }
}
