using System.Linq;
using static NSMSOLUTION.Controllers.ActivationController;

namespace NSMSOLUTION.Services
{
    public static class ActivationValidator
    {
        // returns null when valid, otherwise the error message
        public static string Validate(SaveChangeActivateRequest req)
        {
            if (req == null) return "Invalid request.";
            if (req.ActivateCustomerData == null || !req.ActivateCustomerData.Any())
                return "Activation data is required.";
            if (req.DeviceInformation == null || !req.DeviceInformation.Any())
                return "Device information is required.";

            var a = req.ActivateCustomerData.First();

            if (a.RouterID <= 0) return "Router is required.";

            if (a.ConnectionType == "PPPoE" &&
                (string.IsNullOrWhiteSpace(a.PPPoEUserName) || string.IsNullOrWhiteSpace(a.PPPoEPassword)))
                return "PPPoE user name and password are required.";

            if (a.ConnectionType == "Static" &&
                (string.IsNullOrWhiteSpace(a.IPAddress) || string.IsNullOrWhiteSpace(a.VLanName)))
                return "VLAN and IP address are required for a static connection.";

            return null;
        }
    }
}