using System.Collections.Immutable;

namespace Kapok.View.Wpf;

// NODE: Decided not to use the nuget package MimeTypeMap anymore since the maintenance is not so good:
//       NuGet package is outdated over several years now and multiple pull requests on github are not
//       dealt with.

/// <summary>
/// This is an internal helper class to map mime types to their extension.
///
/// It implements the same interface as <a href="https://github.com/samuelneff/MimeTypeMap">MimeTypeMap</a>.
/// </summary>
internal static class MimeTypeMap
{
    static MimeTypeMap()
    {
        MimeTypeToExtensionMap = ImmutableDictionary.CreateRange(new[]
        {
            // ReSharper disable StringLiteralTypo
            KeyValuePair.Create("image/bmp", ".bmp"),
            KeyValuePair.Create("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx"),
            KeyValuePair.Create("text/html", ".html"),
            KeyValuePair.Create("image/jpeg", ".jpg"),
            KeyValuePair.Create("application/json", ".json"),
            KeyValuePair.Create("image/emf", ".emf"),
            KeyValuePair.Create("message/rfc822", ".mhtml"),
            KeyValuePair.Create("application/pdf", ".pdf"),
            KeyValuePair.Create("image/png", ".png"),
            KeyValuePair.Create("application/vnd.openxmlformats-officedocument.presentationml.presentation", ".pptx"),
            KeyValuePair.Create("application/rtf", ".rtf"),
            KeyValuePair.Create("image/svg+xml", ".svg"),
            KeyValuePair.Create("text/plain", ".txt"),
            KeyValuePair.Create("image/tiff", ".tiff"),
            KeyValuePair.Create("application/xhtml+xml", ".xhtml"),
            KeyValuePair.Create("application/vnd.ms-excel", ".xls"),
            KeyValuePair.Create("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx"),
            KeyValuePair.Create("application/xml", ".xml"),
            KeyValuePair.Create("application/vnd.ms-xpsdocument", ".xps"),
            KeyValuePair.Create("text/csv", ".csv"),
            // ReSharper restore StringLiteralTypo
        });
    }
    
    private static readonly IReadOnlyDictionary<string, string> MimeTypeToExtensionMap;

    /// <summary>
    /// Gets the extension from the provided MINE type.
    /// </summary>
    /// <param name="mimeType">Type of the MIME.</param>
    /// <param name="throwErrorIfNotFound">
    /// If set to <c>true</c>, throws exception <exception cref="KeyNotFoundException" /> if extension's not found.
    /// </param>
    /// <returns>
    /// The extension with dot, e.g. <c>".txt"</c> or <c>string.Empty</c> if <see cref="mimeType"/> was not found and
    /// <see cref="throwErrorIfNotFound"/> was set to <c>false</c>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If <see cref="mimeType"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// If <see cref="mimeType"/> starts with <c>'.'</c>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// If extension for <see cref="mimeType"/> is not found ad <see cref="throwErrorIfNotFound"/> is set to
    /// <c>true</c>.
    /// </exception>
    public static string GetExtension(string mimeType, bool throwErrorIfNotFound = true)
    {
        if (mimeType == null) throw new ArgumentNullException(nameof(mimeType));
        
        if (mimeType.StartsWith('.'))
        {
            throw new ArgumentException("Requested mime type is not valid: " + mimeType);
        }
        
        if (MimeTypeToExtensionMap.TryGetValue(mimeType, out string? extension))
        {
            return extension;
        }
        
        if (throwErrorIfNotFound)
        {
            throw new KeyNotFoundException("Requested mime type is not registered: " + mimeType);
        }
        
        return string.Empty;
    }
}