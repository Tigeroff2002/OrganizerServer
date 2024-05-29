namespace Models.BusinessModels;

public enum RegistrationCase
{
    SuchUserExisted,

    ConfirmationFailed,

    ConfirmationAwaited,

    ConfirmationExpired,

    CodeNotEqualsConfirmation,

    ConfirmationSucceeded
}
