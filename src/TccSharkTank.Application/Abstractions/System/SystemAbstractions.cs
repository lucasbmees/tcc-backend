namespace TccSharkTank.Application.Abstractions.System;

public interface IClock
{
    DateTime UtcNow { get; }
}

public interface IFileStorage
{
    Task<string> SavePdfAsync(Stream content, string fileName, CancellationToken cancellationToken);
}

