using Media.API.DTO;
using Media.Core.Repositories;
using Media.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Media.API.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/media")
            .WithTags("Media");

        g.MapPost("/", UploadAsync)
            .DisableAntiforgery() 
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<MediaUploadResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        g.MapGet("/{id:guid}", DownloadAsync)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        g.MapGet("/{id:guid}/info", GetInfoAsync)
            .Produces<MediaInfoResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        g.MapDelete("/{id:guid}", DeleteAsync)
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task<IResult> UploadAsync(
        HttpRequest request,
        IMediaRepository repo,
        CancellationToken ct)
    {
        if (!request.HasFormContentType)
        {
            return Results.BadRequest(new { error = "Expected multipart/form-data" });
        }

        var form = await request.ReadFormAsync(ct);

        var file = form.Files.GetFile("file");

        if (file is null)
        {
            file = form.Files.FirstOrDefault();
        }

        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "Empty file" });
        }

        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);

        var entity = new MediaFile
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType,
            Length = file.Length,
            Data = ms.ToArray(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await repo.AddAsync(entity, ct);

        var dto = new MediaUploadResponse(entity.Id, entity.FileName, entity.ContentType, entity.Length);

        return Results.Created($"/media/{entity.Id}", dto);
    }

    private static async Task<IResult> DownloadAsync(
        [FromRoute] Guid id,
        IMediaRepository repo,
        CancellationToken ct)
    {
        var entity = await repo.GetAsync(id, ct);
        if (entity is null)
            return Results.NotFound();

        return Results.File(entity.Data, entity.ContentType, entity.FileName);
    }

    private static async Task<IResult> GetInfoAsync(
        [FromRoute] Guid id,
        IMediaRepository repo,
        CancellationToken ct)
    {
        var entity = await repo.GetAsync(id, ct);
        if (entity is null)
            return Results.NotFound();

        var dto = new MediaInfoResponse(
            entity.Id,
            entity.FileName,
            entity.ContentType,
            entity.Length,
            entity.CreatedAt
        );

        return Results.Ok(dto);
    }

    private static async Task<IResult> DeleteAsync(
        [FromRoute] Guid id,
        IMediaRepository repo,
        CancellationToken ct)
    {
        await repo.DeleteAsync(id, ct);
        return Results.NoContent();
    }
}