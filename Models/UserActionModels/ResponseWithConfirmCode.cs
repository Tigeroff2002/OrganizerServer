using Models.BusinessModels;

namespace Models.UserActionModels;

public class ResponseWithConfirmCode : Response
{
    public string Code { get; set; }
}
