using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration.Pnp;
using Windows.Networking.Connectivity;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace AuthenticatorPro
{
    public static class FeedbackHelper
    {
        #region Application Information Methods

        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        public static async Task<string> GetOsVersionAsync()
        {
            string userAgent = await GetUserAgent();

            string result = string.Empty;

            //Parse user agent
            int startIndex = userAgent.ToLower().IndexOf("windows");
            if (startIndex > 0)
            {
                int endIndex = userAgent.IndexOf(";", startIndex);

                if (endIndex > startIndex)
                    result = userAgent.Substring(startIndex, endIndex - startIndex);
            }

            return result;
        }

        private static Task<string> GetUserAgent()
        {
            var tcs = new TaskCompletionSource<string>();

            WebView webView = new WebView();

            string htmlFragment =
              @"<html>
                    <head>
                        <script type='text/javascript'>
                            function GetUserAgent() 
                            {
                                return navigator.userAgent;
                            }
                        </script>
                    </head>
                </html>";

            webView.NavigationCompleted += async (sender, e) =>
            {
                try
                {
                    //Invoke the javascript when the html load is complete
                    string result = await webView.InvokeScriptAsync("GetUserAgent", null);

                    //Set the task result
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            };

            //Load Html
            webView.NavigateToString(htmlFragment);

            return tcs.Task;
        }

        public static string GetMachineName()
        {
            var list = NetworkInformation.GetHostNames().ToArray();
            string name = null;
            if (list.Length > 0)
            {
                for (int i = 0; i < list.Length; i++)
                {
                    var entry = list[i];
                    if (entry.Type == Windows.Networking.HostNameType.DomainName)
                    {
                        string s = entry.CanonicalName;
                        if (!string.IsNullOrEmpty(s))
                        {
                            // Domain-joined. Requires at least a one-
                            // character name.
                            int j = s.IndexOf('.');

                            if (j > 0)
                            {
                                name = s.Substring(0, j);
                                break;
                            }
                            else
                            {
                                // Typical home machine.
                                name = s;
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                // TODO: Localize?
                name = "Unknown Windows 8";
            }

            return name;
        }

        #endregion

        #region System Information Methods

        const string ItemNameKey = "System.ItemNameDisplay";
        const string ModelNameKey = "System.Devices.ModelName";
        const string ManufacturerKey = "System.Devices.Manufacturer";
        const string DeviceClassKey = "{A45C254E-DF1C-4EFD-8020-67D146A850E0},10";
        const string PrimaryCategoryKey = "{78C34FC8-104A-4ACA-9EA4-524D52996E57},97";
        const string DeviceDriverVersionKey = "{A8B865DD-2E3D-4094-AD97-E593A70C75D6},3";
        const string RootContainer = "{00000000-0000-0000-FFFF-FFFFFFFFFFFF}";
        const string RootQuery = "System.Devices.ContainerId:=\"" + RootContainer + "\"";
        const string HalDeviceClass = "4d36e966-e325-11ce-bfc1-08002be10318";

        public static async Task<ProcessorArchitecture> GetProcessorArchitectureAsync()
        {
            var halDevice = await GetHalDevice(ItemNameKey);
            if (halDevice != null && halDevice.Properties[ItemNameKey] != null)
            {
                var halName = halDevice.Properties[ItemNameKey].ToString();
                if (halName.Contains("x64")) return ProcessorArchitecture.X64;
                if (halName.Contains("ARM")) return ProcessorArchitecture.Arm;
                return ProcessorArchitecture.X86;
            }
            return ProcessorArchitecture.Unknown;
        }

        public static Task<string> GetDeviceManufacturerAsync()
        {
            return GetRootDeviceInfoAsync(ManufacturerKey);
        }

        public static Task<string> GetDeviceModelAsync()
        {
            return GetRootDeviceInfoAsync(ModelNameKey);
        }

        public static Task<string> GetDeviceCategoryAsync()
        {
            return GetRootDeviceInfoAsync(PrimaryCategoryKey);
        }

        public static async Task<string> GetWindowsVersionAsync()
        {
            // There is no good place to get this.
            // The HAL driver version number should work unless you're using a custom HAL... 
            var hal = await GetHalDevice(DeviceDriverVersionKey);
            if (hal == null || !hal.Properties.ContainsKey(DeviceDriverVersionKey))
                return null;

            var versionParts = hal.Properties[DeviceDriverVersionKey].ToString().Split('.');
            return string.Join(".", versionParts.Take(2).ToArray());
        }

        private static async Task<string> GetRootDeviceInfoAsync(string propertyKey)
        {
            var pnp = await PnpObject.CreateFromIdAsync(PnpObjectType.DeviceContainer,
                        RootContainer, new[] { propertyKey });
            return (string)pnp.Properties[propertyKey];
        }

        private static async Task<PnpObject> GetHalDevice(params string[] properties)
        {
            var actualProperties = properties.Concat(new[] { DeviceClassKey });
            var rootDevices = await PnpObject.FindAllAsync(PnpObjectType.Device,
                actualProperties, RootQuery);

            foreach (var rootDevice in rootDevices.Where(d => d.Properties != null && d.Properties.Any()))
            {
                var lastProperty = rootDevice.Properties.Last();
                if (lastProperty.Value != null)
                    if (lastProperty.Value.ToString().Equals(HalDeviceClass))
                        return rootDevice;
            }
            return null;
        }

        #endregion

        public static void Feedback()
        {
            Feedback("[Your feedback here]", false);
        }

        public static async void Feedback(string contents, bool error)
        {
            string deviceName = await FeedbackHelper.GetDeviceModelAsync();
            string deviceManufacturer = await FeedbackHelper.GetDeviceManufacturerAsync();
            ProcessorArchitecture processorArchitecture = await FeedbackHelper.GetProcessorArchitectureAsync();
            string platformVersion = await FeedbackHelper.GetOsVersionAsync();
            string version = FeedbackHelper.GetAppVersion();

            string to = App.FeedbackEmailAddress;
            string subject = error ? "Authenticator Pro Error Report" : "Authenticator Pro Feedback";
            string body = string.Format(contents + "\n\n---------------------------------\nDevice Name: {0}\nDevice Manufacturer: {1}\nProcessor Architecture: {2}\nPlatform Version: {3}\nApplication Version: {4}\n---------------------------------\n\nNote: This e-mail exchange is governed by {5}’s privacy policy. You can find more details on the About page in the application.",
                                        deviceName,
                                        deviceManufacturer,
                                        processorArchitecture,
                                        platformVersion,
                                        version,
                                        "Authenticator Pro");

            body = WebUtility.UrlEncode(body);
            body = body.Replace("+", "%20");

            var mailto = new Uri("mailto:?to=" + to + "&subject=" + subject + "&body=" + body);
            await Windows.System.Launcher.LaunchUriAsync(mailto);
        }
    }
}