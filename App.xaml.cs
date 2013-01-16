using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace Authenticator
{
    public partial class App : Application
    {
        public AccountList Database { get; set; }
        public bool DataCorruptionException { get; set; }
        
        public PhoneApplicationFrame RootFrame { get; private set; }
        
        public App()
        {
            UnhandledException += Application_UnhandledException;

            InitializeComponent();

            InitializePhoneApplication();

            this.Database = new AccountList();
            StreamReader sr = null;

            try
            {
                IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
                if (iso.FileExists("AccountArchive.xml"))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(AccountList));
                    using (sr = new StreamReader(iso.OpenFile("AccountArchive.xml", FileMode.Open)))
                    {
                        this.Database = (AccountList)xs.Deserialize(sr);
                    }
                }
            }
            catch (Exception ex)
            {
                DataCorruptionException = true;

                if (sr != null)
                {
                    sr.Close();
                }

                IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
                if (iso.FileExists("AccountArchive.xml"))
                {
                    iso.DeleteFile("AccountArchive.xml");
                }
            }
        }

        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            
        }

        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            PhoneApplicationService.Current.State["AccountDB"] = this.Database;
        }

        public void Application_Closing(object sender, ClosingEventArgs e)
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var xs = new XmlSerializer(typeof(AccountList));
                var stream = iso.OpenFile("AccountArchive.xml", FileMode.Create);
                using (var sw = new StreamWriter(stream))
                {
                    xs.Serialize(sw, this.Database);
                    sw.Flush();
                }
            }
        }

        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}