using Logic.Abstractions;

using Models.BusinessModels;

using System.Collections.Concurrent;

namespace Logic;

public sealed class UserConfirmationCache :
    IUserConfirmationCache
{
    public void AddDataForConfirmation(UserConfirmationData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        _data[data.Email] = data;
    }

    public void RemoveData(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        _data.TryRemove(email, out _);
    }

    public bool TryGetValue(string email, out UserConfirmationData data)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);

        return _data.TryGetValue(email, out data!);
    }

    public void RemoveNotActual()
    {
        var notActualEmails = 
            _data.Where(x => !x.Value.IsActual()).Select(x => x.Key).ToList();

        foreach (var email in notActualEmails)
        {
            RemoveData(email);
        }
    }

    private ConcurrentDictionary<string, UserConfirmationData> _data = new();
}
