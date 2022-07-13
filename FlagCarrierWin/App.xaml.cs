using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FlagCarrierWin
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();

            if (e.Args.Length > 0)
            {
                if (!e.Args[0].StartsWith("esa-flagcarrier:"))
                {
                    MessageBox.Show("Invalid Invocation: " + e.Args[0]);
                    Shutdown();
                    return;
                }

                Uri uri = new Uri(e.Args[0]);

                if (uri.Host.ToLower() == "write" && !string.IsNullOrWhiteSpace(uri.Query))
                {
                    var dict = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    window.SetWriteQuery(dict);
                }
                else
                {
                    MessageBox.Show("Invalid action");
                    Shutdown();
                    return;
                }
            }

            window.Show();
        }
    }
}
