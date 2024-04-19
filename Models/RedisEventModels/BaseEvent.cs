namespace Models.RedisEventModels;

public abstract record class BaseEvent(int Id, bool IsCommited);