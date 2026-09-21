namespace IdentityAndSerilog.Logging;

public static class SecurityEventIds
{
    public const int LoginSucceeded = 4001;
    public const int LoginFailed = 4002;

    public const int RegisterFailed = 5010;

    public const int RegisterSucceeded = 5011;
    public const int Logout = 4003;

    public const int UserRegistered = 4010;
    public const int PasswordChanged = 4011;

    public const int UnauthorizedAccess = 4020;
    public const int NotFound = 4022;
    public const int ForbiddenAccess = 4021;

    public const int InvalidToken = 4030;
    public const int ExpiredToken = 4031;

    public const int AccountLockedOut = 4040;

    public const int UnknownUserLoginAttempt = 4041;
}