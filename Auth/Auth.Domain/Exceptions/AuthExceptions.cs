using Auth.Domain.Entities;

namespace Auth.Domain.Exceptions;

public class AccountLockedException : Exception
{
    public DateTime LockoutEnd { get; }

    public AccountLockedException(DateTime lockoutEnd) 
        : base($"Account is locked. Try again after {lockoutEnd:g}")
    {
        LockoutEnd = lockoutEnd;
    }
}

public class AccountBannedException : Exception
{
    public AccountBannedException() 
        : base("Account is banned.")
    {
    }
}

public class UserSoftDeletedException : Exception
{
    public UserSoftDeletedException(string message) : base(message)
    {
    }
}

public class RequiresVerificationException : Exception
{
    public GlobalUserStatus RequiredStatus { get; }
    public string? Phone { get; }

    public RequiresVerificationException(GlobalUserStatus status, string? phone = null) 
        : base($"Account requires verification: {status}")
    {
        RequiredStatus = status;
        Phone = phone;
    }
}

public class RequiresProfileCompletionException : Exception
{
    public RequiresProfileCompletionException() 
        : base("Account requires profile completion.")
    {
    }
}

public class RequiresAdminApprovalException : Exception
{
    public RequiresAdminApprovalException() 
        : base("Account pending admin approval.")
    {
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
