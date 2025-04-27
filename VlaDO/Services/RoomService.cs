using VlaDO.Models;
using VlaDO.Repositories;

namespace VlaDO.Services;

public class RoomService
{
    private readonly IGenericRepository<Room> _roomRepo;

    public RoomService(IGenericRepository<Room> roomRepo) => _roomRepo = roomRepo;

    public async Task<Guid> CreateAsync(Guid ownerId, string? title)
    {
        var room = new Room { OwnerId = ownerId, Title = title };
        await _roomRepo.AddAsync(room);
        return room.Id;
    }

    public Task<IEnumerable<Room>> ListAsync(Guid ownerId) =>
        _roomRepo.FindAsync(r => r.OwnerId == ownerId);

    public Task DeleteAsync(Guid roomId, Guid ownerId)
        => _roomRepo.DeleteAsync(roomId); // доп-проверка хозяина при желании
}