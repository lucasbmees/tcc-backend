using Microsoft.Extensions.Configuration;
using TccSharkTank.Application.Abstractions.System;

namespace TccSharkTank.Infrastructure.System;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public sealed class LocalFileStorage : IFileStorage
{
    private readonly IConfiguration _configuration;

    public LocalFileStorage(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> SavePdfAsync(Stream content, string fileName, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension != ".pdf")
        {
            throw new InvalidOperationException("Apenas arquivos .pdf são permitidos.");
        }

        var root = _configuration.GetSection("Storage")["Root"] ?? "storage";
        Directory.CreateDirectory(root);

        var safeName = $"{Guid.NewGuid():N}.pdf";
        var fullPath = Path.Combine(root, safeName);

        await using var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs, cancellationToken);

        return fullPath.Replace('\\', '/');
    }
}

