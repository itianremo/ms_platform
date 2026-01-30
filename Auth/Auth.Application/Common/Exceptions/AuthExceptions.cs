using Auth.Domain.Entities;

namespace Auth.Application.Common.Exceptions;

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
