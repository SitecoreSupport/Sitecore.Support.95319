using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Jobs;
using Sitecore.Shell.Applications.Dialogs.ProgressBoxes;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Grids;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Continuations;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Sitecore.Support.Shell.Applications.WebEdit.Commands
{
    [Serializable]
    public class Unlock : WebEditCommand, ISupportsContinuation
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            string selectedValue = GridUtil.GetSelectedValue("Items");
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("items", selectedValue);
            ClientPipelineArgs args = new ClientPipelineArgs(parameters);
            ContinuationManager.Current.Start(this, "Run", args);
        }

        protected static void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            ListString str = new ListString(args.Parameters["items"]);
            if (str.Count == 0)
            {
                SheerResponse.Alert("Select an item first.", new string[0]);
            }
            else
            {
                List<Item> list = new List<Item>();
                foreach (string str2 in str)
                {
                    Item item = Client.ContentDatabase.GetItem(str2);
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
                List<Item> list2 = new List<Item>();
                foreach (Item item2 in list)
                {
                    foreach (Item item3 in item2.Versions.GetVersions(true))
                    {
                        if (item3.Locking.GetOwner().Equals(Context.User.Name, StringComparison.InvariantCultureIgnoreCase) && !item3.IsFallback)
                        {
                            list2.Add(item3);
                        }
                    }
                }
                ProgressBox.Execute("Unlock", "Unlocking items", "Network/16x16/lock.png", new ProgressBoxMethod(Unlock.UnlockItems), "lockeditems:refresh", Context.User, new object[] { list2 });
            }
        }

        private static void UnlockItems(params object[] parameters)
        {
            Assert.ArgumentNotNull(parameters, "parameters");
            List<Item> list = parameters[0] as List<Item>;
            if (list != null)
            {
                Job.Total = list.Count;
                foreach (Item item in list)
                {
                    Job.AddMessage(Translate.Text("Unlocking {0}", new object[] { item.Paths.ContentPath }), new object[0]);
                    item.Locking.Unlock();
                    Job.Processed += 1L;
                }
            }
        }
    }
}