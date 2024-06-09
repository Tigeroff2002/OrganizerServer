using Contracts.Request;
using Models.BusinessModels;
using Org.BouncyCastle.Asn1.BC;

namespace Logic.Abstractions;

public interface IUserConfirmationCache
{
    public void AddDataForConfirmation(UserConfirmationData data);

    public void RemoveData(string email);

    public bool TryGetValue(string email, out UserConfirmationData data);

    public void RemoveNotActual();
}
