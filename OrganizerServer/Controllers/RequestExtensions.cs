using Org.BouncyCastle.Asn1.Ocsp;

namespace ToDoCalendarServer.Controllers;

public static class RequestExtensions
{
    public static async Task<string> ReadRequestBodyAsync(Stream body)
    {
        using var reader = new StreamReader(body);

        return await reader.ReadToEndAsync();
    }
}
