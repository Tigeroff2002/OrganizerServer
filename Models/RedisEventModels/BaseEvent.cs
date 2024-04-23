using System.Numerics;

namespace Models.RedisEventModels;

public abstract record class BaseEvent
{
    public string Id { get; }

    public bool IsCommited { get; set; }

    public BaseEvent(string id, bool isCommited)
    {
        Id = id;
        IsCommited = isCommited;
    }
}