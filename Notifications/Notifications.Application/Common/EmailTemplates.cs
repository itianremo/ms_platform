using System.Text;

namespace Notifications.Application.Common;

public static class EmailTemplates
{
    private const string BaseTemplate = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{Subject}</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 20px auto; background: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background-color: #2563eb; color: #ffffff; padding: 20px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: 600; }}
        .content {{ padding: 30px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #2563eb; color: #ffffff; text-decoration: none; border-radius: 4px; font-weight: bold; margin-top: 20px; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #e9ecef; }}
        .otp-code {{ font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #2563eb; margin: 20px 0; text-align: center; background: #f0f7ff; padding: 15px; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{Title}</h1>
        </div>
        <div class='content'>
            {Body}
        </div>
        <div class='footer'>
            <p>&copy; {Year} MS Platform. All rights reserved.</p>
            <p>This is an automated message, please do not reply.</p>
        </div>
    </div>
</body>
</html>";

    public static string GetOtpEmail(string otpCode)
    {
        var body = $@"
            <p>Hello,</p>
            <p>You requested a verification code for your account. Please use the following code to complete your request:</p>
            <div class='otp-code'>{otpCode}</div>
            <p>This code will expire in 10 minutes.</p>
            <p>If you did not request this code, please ignore this email.</p>";

        return BaseTemplate
            .Replace("{Subject}", "Your Verification Code")
            .Replace("{Title}", "Verification Code")
            .Replace("{Body}", body)
            .Replace("{Year}", DateTime.Now.Year.ToString());
    }

    public static string GetWelcomeEmail(string name, string? initialPassword = null)
    {
        var passwordSection = !string.IsNullOrWhiteSpace(initialPassword) 
            ? $"<p>Your account has been created by an administrator. You can log in using the following password:</p><div class='otp-code' style='font-size: 24px;'>{initialPassword}</div><p>Please change this password immediately after your first login.</p>"
            : "<p>Your account has been successfully created. You can now log in and explore our services.</p>";

        var body = $@"
            <p>Hi {name},</p>
            <p>Welcome to MS Platform! We're excited to have you on board.</p>
            {passwordSection}
            <a href='#' class='button'>Go to Dashboard</a>";

        return BaseTemplate
            .Replace("{Subject}", "Welcome to MS Platform")
            .Replace("{Title}", "Welcome aboard!")
            .Replace("{Body}", body)
            .Replace("{Year}", DateTime.Now.Year.ToString());
    }
}
