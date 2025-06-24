using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Reflection.Emit;
using VlaDO.DTOs;
using VlaDO.Helpers;
using VlaDO.Models;
using VlaDO.Repositories;
using VlaDO.Repositories.Documents;

namespace VlaDO.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _uow;
    private readonly IPermissionService _perm;
    private readonly IDocumentRepository _docRepo;
    private readonly IActivityLogger _logger;

    private IGenericRepository<Document> Docs => _uow.Documents;
    private IGenericRepository<Room> Rooms => _uow.Rooms;

    public DocumentService(IUnitOfWork uow, IPermissionService perm, IActivityLogger logger)
    {
        _uow = uow;
        _perm = perm;
        _docRepo = uow.DocumentRepository;
        _logger = logger;
    }

    public async Task<Guid> UploadAsync(Guid roomId, Guid userId, string name, byte[] data)
    {
        await _perm.CheckRoomAccessAsync(userId, roomId, AccessLevel.Edit);

        var doc = new Document
        {
            Name = name,
            Data = data,
            RoomId = roomId,
            CreatedBy = userId,
            Hash = Sha256(data),
            ForkPath = await DocumentVersionHelper.SafeGenerateInitialForkPathAsync(Docs, userId, roomId)
        };

        await Docs.AddAsync(doc);
        await _uow.CommitAsync();
        await _logger.LogAsync(ActivityType.CreatedDocument, authorId: userId, subjectId: doc.Id, meta: new { doc.Name, RoomId = roomId });
        return doc.Id;
    }

    public Task<IEnumerable<Document>> ListAsync(Guid roomId)
        => Docs.FindAsync(d => d.RoomId == roomId);

    public Task<Document?> GetAsync(Guid id) => Docs.GetByIdAsync(id);

    public async Task UploadManyAsync(Guid roomId, Guid userId, IEnumerable<IFormFile> files)
    {
        foreach (var f in files)
        {
            using var ms = new MemoryStream();
            await f.CopyToAsync(ms);
            await UploadAsync(roomId, userId, f.FileName, ms.ToArray());
        }
    }

    public async Task<Guid> UpdateAsync(Guid roomId, Guid docId, Guid userId, IFormFile newFile, string? note = null)
    {
        var parent = await Docs.GetByIdAsync(docId)
                     ?? throw new KeyNotFoundException("Документ не найден");

        await EnsureRoomAndAccess(roomId, userId, AccessLevel.Edit);

        using var ms = new MemoryStream();
        await newFile.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var (nextVersion, nextForkPath) = await DocumentVersionHelper.GenerateNextVersionAsync(Docs, parent);

        var newDoc = new Document
        {
            Name = newFile.FileName,
            Data = bytes,
            Note = note,
            RoomId = roomId,
            CreatedBy = userId,
            Version = nextVersion,
            ForkPath = nextForkPath,
            ParentDocId = parent.Id,
            PrevHash = parent.Hash,
            Hash = Sha256(bytes)
        };

        await Docs.AddAsync(newDoc);
        await _uow.CommitAsync();

        await _logger.LogAsync(
            ActivityType.UpdatedDocument,
            authorId: userId,
            subjectId: newDoc.Id,
            meta: new { newDoc.Name, newDoc.Version, ForkPath = newDoc.ForkPath }
        );

        return newDoc.Id;
    }

    public async Task<(byte[] bytes, string fileName, string ctype)> DownloadAsync(Guid docId, Guid userId)
    {
        var d = await Docs.GetByIdAsync(docId) ?? throw new KeyNotFoundException();
        await EnsureRoomAndAccess(d.RoomId!.Value, userId, AccessLevel.Read);
        return (d.Data!, d.Name, "application/octet-stream");
    }

    public async Task<(byte[] zip, string fileName)> DownloadManyAsync(IEnumerable<Guid> ids, Guid userId)
    {
        var docs = await Docs.FindAsync(d => ids.Contains(d.Id));

        foreach (var d in docs)
            await EnsureRoomAndAccess(d.RoomId!.Value, userId, AccessLevel.Read);

        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var d in docs)
            {
                var entry = zip.CreateEntry(d.Name);
                await using var es = entry.Open();
                es.Write(d.Data!, 0, d.Data!.Length);
            }
        }
        return (ms.ToArray(), "documents.zip");
    }

    private static string Sha256(byte[] bytes) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));

    private async Task EnsureRoomAndAccess(Guid roomId, Guid userId, AccessLevel level)
    {
        if (!await Rooms.ExistsAsync(roomId))
            throw new KeyNotFoundException("Комната не найдена");
        if (!await _perm.CheckRoomAccessAsync(userId, roomId, level))
            throw new UnauthorizedAccessException("Недостаточно прав");
    }

    public async Task<IEnumerable<DocumentInfoDto>> ListAsync(Guid roomId, Guid userId)
    {
        await EnsureRoomAndAccess(roomId, userId, AccessLevel.Read);

        var docs = await Docs.FindAsync(d => d.RoomId == roomId);

        return docs.Select(d => new DocumentInfoDto(
            d.Id,
            d.Name,
            d.Version,
            d.ParentDocId,
            d.Hash,
            d.PrevHash,
            d.CreatedOn,
            d.Note,
            d.ForkPath
        ));
    }

    public async Task DeleteAsync(Guid docId, Guid userId)
    {
        var doc = await Docs.GetByIdAsync(docId)
                  ?? throw new KeyNotFoundException();

        if (doc.RoomId.HasValue)
        {
            await EnsureRoomAndAccess(doc.RoomId.Value, userId, AccessLevel.Full);
        }
        else
        {
            if (doc.CreatedBy != userId)
                throw new UnauthorizedAccessException("Недостаточно прав");
        }

        await _logger.LogAsync(
            ActivityType.DeletedDocument,
            authorId: userId,
            subjectId: docId,
            meta: new { doc.Name }
        );

        var tokens = await _uow.Tokens.FindAsync(t => t.DocumentId == docId);
        foreach (var tok in tokens)
            await _uow.Tokens.DeleteAsync(tok.Id);

        await Docs.DeleteAsync(docId);
    }

    public async Task<IEnumerable<Document>> GetByRoomAndUserAsyncExcludeCreator(Guid userId)
    {
        return await _docRepo.GetByRoomAndUserAsyncExcludeCreator(userId);
    }
    public async Task<Guid> RenameAsync(Guid docId, Guid userId, string newName)
    {
        var doc = await Docs.GetByIdAsync(docId)
              ?? throw new KeyNotFoundException("Документ не найден");

        if (doc.RoomId.HasValue)
        {
            await EnsureRoomAndAccess(doc.RoomId.Value, userId, AccessLevel.Edit);
        }
        else
        {
            if (doc.CreatedBy != userId)
                throw new UnauthorizedAccessException("Недостаточно прав");
        }

        var oldName = doc.Name;
        doc.Name = newName.Trim();
        await _uow.CommitAsync();

        await _logger.LogAsync(
            ActivityType.RenamedDocument,
            authorId: userId,
            subjectId: docId,
            meta: new { OldName = oldName, NewName = doc.Name }
        );

        return docId;
    }
}
