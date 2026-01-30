namespace Shared.Kernel.Enums;

public enum GlobalUserStatus
{
    PendingAccountVerification = 0,
    PendingMobile = 1,
    PendingEmail = 2,
    PendingAdminApproval = 3,
    Active = 4,
    Suspended = 5,
    Deleted = 6,
    ProfileIncomplete = 7
}
