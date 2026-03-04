using System.Text;

var client = new HttpClient();
client.DefaultRequestHeaders.Add("App-Id", "00000000-0000-0000-0000-000000000012");
client.DefaultRequestHeaders.Add("Tenant-Id", "00000000-0000-0000-0000-000000000000");

var payload = """
{
   "id": "00000000-0000-0000-0000-000000000012",
    "name": "Wissler App Updated via Script",
    "description": "Dating and Social Application",
    "baseUrl": "WisslerApp",
    "externalAuthProvidersJson": "[\"google\", \"apple\", \"facebook\"]",
    "privacyPolicy": "# Wissler App Privacy Policy\nThis is the privacy policy for the Wissler mobile application.",
    "termsAndConditions": "# Wissler App Terms and Conditions\nBy using Wissler App, you agree to these terms.",
    "isActive": true,
    "verificationType": 3,
    "requiresAdminApproval": true,
    "sysConfig": {
        "theme": "light",
        "collapsedmenu": true
    },
    "notifications": {
        "email": true,
        "push": true,
        "sms": false
    }
}
""";

var content = new StringContent(payload, Encoding.UTF8, "application/json");
var response = await client.PutAsync("http://localhost:7032/apps/api/Apps/00000000-0000-0000-0000-000000000012", content);

Console.WriteLine($"Status: {response.StatusCode}");
if (!response.IsSuccessStatusCode) 
{
    Console.WriteLine($"Error: {await response.Content.ReadAsStringAsync()}");
}
