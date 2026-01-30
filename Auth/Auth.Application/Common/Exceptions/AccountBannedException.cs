namespace Auth.Application.Common.Exceptions;

public class AccountBannedException : Exception
{
    public AccountBannedException() 
        : base("Account is banned.")
    {
    }
}
