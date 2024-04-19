namespace Models.StorageModels;

public class DirectChat
{
    public int Id { get; set; }

    public string Caption { get; set; }

    public DateTimeOffset CreateTime { get; set; }

    public int User1Id { get; set; }

    public virtual User User1 { get; set; }

    public int User2Id { get; set; }

    public virtual User User2 { get; set; }

    public virtual List<DirectMessage> DirectMessages { get; set; }
}
