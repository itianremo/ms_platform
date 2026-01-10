namespace Auth.Application.Common.Exceptions;

public class UserSoftDeletedException : Exception
{
    public UserSoftDeletedException(string message) : base(message)
    {
    }
}
