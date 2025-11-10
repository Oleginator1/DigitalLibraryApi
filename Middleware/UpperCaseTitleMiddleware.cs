using DigitalLibraryApi.Repositories;
using System.Text.Json;
using DigitalLibraryApi.Models;

namespace DigitalLibraryApi.Middleware
{
    public class UpperCaseTitleMiddleware
    {
        private readonly RequestDelegate _next;

        public UpperCaseTitleMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Only process /books (public) endpoints
            if (path == null || !path.StartsWith("/books"))
            {
                await _next(context);
                return;
            }

            // Capture response body
            var originalBodyStream = context.Response.Body;
            using var newBodyStream = new MemoryStream();
            context.Response.Body = newBodyStream;

            await _next(context);

            // If not JSON or no content, just copy back
            if (context.Response.ContentType == null ||
                !context.Response.ContentType.Contains("application/json") ||
                context.Response.StatusCode != StatusCodes.Status200OK)
            {
                newBodyStream.Seek(0, SeekOrigin.Begin);
                await newBodyStream.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
                return;
            }

            // Read the response
            newBodyStream.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(newBodyStream).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(bodyText))
            {
                context.Response.Body = originalBodyStream;
                return;
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            try
            {
                if (bodyText.TrimStart().StartsWith("["))
                {
                    var books = JsonSerializer.Deserialize<List<Book>>(bodyText, jsonOptions);
                    if (books != null)
                    {
                        foreach (var book in books)
                        {
                            if (!string.IsNullOrEmpty(book.Title))
                                book.Title = book.Title.ToUpper();
                        }
                        bodyText = JsonSerializer.Serialize(books, jsonOptions);
                    }
                }
                else
                {
                    var book = JsonSerializer.Deserialize<Book>(bodyText, jsonOptions);
                    if (book != null && !string.IsNullOrEmpty(book.Title))
                    {
                        book.Title = book.Title.ToUpper();
                        bodyText = JsonSerializer.Serialize(book, jsonOptions);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Uppercase Middleware] Error: {ex.Message}");
            }

            // Write modified or original response back
            var modifiedBytes = System.Text.Encoding.UTF8.GetBytes(bodyText);
            context.Response.Body = originalBodyStream;
            await context.Response.Body.WriteAsync(modifiedBytes, 0, modifiedBytes.Length);
        }
    }

    public static class UpperCaseTitleMiddlewareExtensions
    {
        public static IApplicationBuilder UseUpperCaseTitleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UpperCaseTitleMiddleware>();
        }
    }
}
