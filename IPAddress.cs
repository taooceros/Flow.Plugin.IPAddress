using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Flow.Launcher.Plugin;
using System.Linq;

namespace Flow.Plugin.IPAddress
{
    public class Program : IAsyncPlugin, IResultUpdated
    {
        internal PluginInitContext Context { get; private set; }
        public const string icon = "ipaddress.png";

        public event ResultUpdatedEventHandler ResultsUpdated;

        public Task InitAsync(PluginInitContext context)
        {
            Context = context;
            return Task.CompletedTask;
        }

        public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
        {
            var results = new List<Result>();

            var hostname = Dns.GetHostName();

            // Get the Local IP Address

            foreach (var ipAddress in (await Dns.GetHostEntryAsync(hostname)).AddressList.OrderBy(ip => ip.AddressFamily))
            {
                var ip = ipAddress.ToString();
                results.Add(Result(ip, "Local IP Address ", icon, Action(ip)));

            }

            ResultsUpdated?.Invoke(this, new ResultUpdatedEventArgs
            {
                Query = query,
                Results = results
            });

            // Get the External IP Address
            var externalIp = (await Context.API.HttpGetStringAsync("https://api.ip.sb/ip", token)).Trim();


            results.Insert(0, Result(externalIp, "External IP Address ", icon, Action(externalIp)));

            return results;
        }

        // relative path to your plugin directory
        private static Result Result(String title, String subtitle, String icon, Func<ActionContext, bool> action)
        {
            return new Result()
            {
                Title = title,
                SubTitle = subtitle,
                IcoPath = icon,
                Action = action
            };
        }

        // The Action method is called after the user selects the item
        private static Func<ActionContext, bool> Action(String text)
        {
            return e =>
            {
                CopyToClipboard(text);

                // return false to tell Flow don't hide query window, otherwise Wox will hide it automatically
                return false;
            };
        }

        public static void CopyToClipboard(String text)
        {
            Clipboard.SetDataObject(text);
        }
    }
}