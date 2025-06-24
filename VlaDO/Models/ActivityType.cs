namespace VlaDO.Models
{
    public enum ActivityType
    {
        CreatedDocument,
        UpdatedDocument,
        DeletedDocument,
        ArchivedDocument,
        RenamedDocument,

        IssuedToken,
        UpdatedToken,
        RevokedToken,

        CreatedRoom,
        InvitedToRoom,
        AcceptedRoom,
        DeclinedRoom,
        UpdatedRoomAccess,
        DeletedRoom,

        InvitedToContacts,
        AcceptedContact,
        DeclinedContact
    }

}
