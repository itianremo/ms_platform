using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Shared.Kernel; // For VerificationType if needed
using Auth.Domain.Enums;

namespace Auth.Domain.Services;

public class UserLoginPolicy
{
    public record LoginResult(bool SuppressRoles);

    public LoginResult Evaluate(User user, Guid? appId, List<AppRequirement> requirements)
    {
        // 1. Check Global Status
        if (user.Status == GlobalUserStatus.SoftDeleted)
            throw new Auth.Domain.Exceptions.UserSoftDeletedException("Account is soft-deleted. Reactivation required.");

        if (user.Status == GlobalUserStatus.Banned)
            throw new global::Auth.Domain.Exceptions.AccountBannedException();

        // 2. Brute Force Lockout Check
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            throw new global::Auth.Domain.Exceptions.AccountLockedException(user.LockoutEnd.Value);
        }

        // 3. Dynamic Requirements Check & Status Sync
        bool identityIncomplete = false;
        GlobalUserStatus? pendingStatus = null;

        foreach (var req in requirements)
        {
            // A. Identity Verification
            bool isIdentityReady = true;
            if ((req.VerificationType == VerificationType.Email || req.VerificationType == VerificationType.Both) && !user.IsEmailVerified)
            {
                isIdentityReady = false;
                pendingStatus = GlobalUserStatus.PendingEmailVerification;
            }
            if ((req.VerificationType == VerificationType.Phone || req.VerificationType == VerificationType.Both) && !user.IsPhoneVerified)
            {
                isIdentityReady = false;
                pendingStatus = GlobalUserStatus.PendingMobileVerification;
            }

            if (!isIdentityReady) identityIncomplete = true;

            // B. App-Specific Blocking
            if (appId.HasValue && req.AppId == appId.Value)
            {
                var currentMembershipStatus = (AppUserStatus)req.MembershipStatus;
                
                // If Identity is ready, check Admin Approval logic if needed. 
                // But simplified: Just check if blocked.
                if (currentMembershipStatus == AppUserStatus.PendingApproval)
                {
                    // Check if Admin Approval is actually required for this App?
                    // The Requirement object has `RequiresAdminApproval`.
                    if (req.RequiresAdminApproval)
                    {
                         throw new global::Auth.Domain.Exceptions.RequiresAdminApprovalException();
                    }
                    // If not required, we nominally proceed (and maybe auto-activate in a separate process, 
                    // but here we just ensure we don't throw if not required).
                    // However, if the status IS PendingApproval, and approval is NOT required, 
                    // it implies the user hasn't been transitioned to Active yet. 
                    // Original logic auto-upgraded inside the loop. 
                    // We should keep side-effects separate if possible or return "Mutations" needed.
                    // For now, to keep it simple, we assume status is authoritative. 
                    // If Status is Pending and Req says NeedsApproval -> Throw.
                }

                if (currentMembershipStatus == AppUserStatus.Banned)
                {
                     throw new global::Auth.Domain.Exceptions.AccountBannedException();
                }
            }
        }

        // Global Verification Enforcement
        if (identityIncomplete)
        {
             // If incomplete, we throw. 
             // Note: The original logic UPDATED the status. 
             // Ideally the Policy returns the "NewStatus" and the Handler updates it.
             // But if we throw here, we can't update?
             // Actually, we should probably Update status if it's outdated, SAVE, and THEN Throw.
             // But a Policy shouldn't save.
             // So: If status mismatch, throw "RequiresVerification" immediately?
             throw new global::Auth.Domain.Exceptions.RequiresVerificationException(pendingStatus ?? GlobalUserStatus.PendingEmailVerification, user.Phone);
        }

        // 4. Subscription Expiry Check
        // Handled via external profile API fetch inside the CommandHandler now.
        bool suppressRoles = false;

        return new LoginResult(suppressRoles);
    }
}
