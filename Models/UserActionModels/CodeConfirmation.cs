namespace Models.BusinessModels;

public sealed class CodeConfirmation
{
    public string Code { get; }

    public CodeConfirmation(string code)
    {
        Code = code;
    }
}
