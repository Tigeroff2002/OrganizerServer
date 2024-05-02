namespace Models.StorageModels;

public sealed class UserDeviceMap
{
    public int UserId { get; set; }

    public User User { get; set; }

    public string FirebaseToken { get; set; }

    public DateTimeOffset TokenSetMoment { get; set; }

    public bool IsActive { get; set; } = true;
}
