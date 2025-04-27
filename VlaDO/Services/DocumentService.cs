using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services;

public class DocumentService
{
    private readonly IGenericRepository<Document> _docRepo;
    private readonly IGenericRepository<Room> _roomRepo;

    public DocumentService(
        IGenericRepository<Document> docRepo,
        IGenericRepository<Room> roomRepo)
    {
        _docRepo = docRepo;
        _roomRepo = roomRepo;
    }

    public async Task<Guid> UploadAsync(Guid roomId, Guid userId, string name, byte[] data)
    {
        if (!(await _roomRepo.ExistsAsync(roomId)))
            throw new InvalidOperationException("Комната не найдена");

        var doc = new Document
        {
            Name = name,
            Data = data,
            RoomId = roomId,
            CreatedBy = userId,
            Hash = Convert.ToHexString(
                          System.Security.Cryptography.SHA256.HashData(data))
        };
        await _docRepo.AddAsync(doc);
        return doc.Id;
    }

    public Task<IEnumerable<Document>> ListAsync(Guid roomId)
        => _docRepo.FindAsync(d => d.RoomId == roomId);

    public Task<Document?> GetAsync(Guid id) => _docRepo.GetByIdAsync(id);

    public Task DeleteAsync(Guid id) => _docRepo.DeleteAsync(id);
}