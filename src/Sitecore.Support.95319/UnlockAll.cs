using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Security;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Jobs;
using Sitecore.Shell.Applications.Dialogs.ProgressBoxes;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Continuations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Sitecore.Support.Shell.Applications.WebEdit.Commands
{
    [Serializable]
    public class UnlockAll : WebEditCommand, ISupportsContinuation
    {
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            ContinuationManager.Current.Start(this, "Run");
        }

        protected static void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            List<Item> list = new List<Item>();
            Item[] source = SearchItems();
            if ((source == null) || !source.Any<Item>())
            {
                SheerResponse.Alert("You have no locked items.", new string[0]);
            }
            else if (args.IsPostBack)
            {
                if (args.Result == "yes")
                {
                    foreach (Item item in source)
                    {
                        foreach (Item item2 in item.Versions.GetVersions(true))
                        {
                            if (string.Compare(item2.Locking.GetOwner(), Context.User.Name, StringComparison.InvariantCultureIgnoreCase) == 0 && !item2.IsFallback)
                            {
                                list.Add(item2);
                            }
                        }
                    }
                    ProgressBox.Execute("UnlockAll", "Unlocking items", "Network/16x16/lock.png", new ProgressBoxMethod(UnlockAll.UnlockAllItems), "lockeditems:refresh", Context.User, new object[] { list });
                }
            }
            else
            {
                if (source.Count<Item>() == 1)
                {
                    SheerResponse.Confirm("Are you sure you want to unlock this item?");
                }
                else
                {
                    SheerResponse.Confirm(Translate.Text("Are you sure you want to unlock these {0} items?", new object[] { source.Count<Item>() }));
                }
                args.WaitForPostBack();
            }
        }

        private static void UnlockAllItems(params object[] parameters)
        {
            Assert.ArgumentNotNull(parameters, "parameters");
            List<Item> list = parameters[0] as List<Item>;
            if (list != null)
            {
                Job job = Context.Job;
                if (job != null)
                {
                    job.Status.Total = list.Count;
                }
                foreach (Item item in list)
                {
                    if (job != null)
                    {
                        job.Status.Messages.Add(Translate.Text("Unlocking {0}", new object[] { item.Paths.ContentPath }));
                    }
                    item.Locking.Unlock();
                    if (job != null)
                    {
                        JobStatus status = job.Status;
                        status.Processed += 1L;
                    }
                }
            }
        }

        private static Item[] SearchItems()
        {
            string userName = Context.User.Identity.Name;
            using (IProviderSearchContext searchContext = ContentSearchManager.GetIndex((IIndexable)new SitecoreIndexableItem(Client.ContentDatabase.GetRootItem())).CreateSearchContext(SearchSecurityOptions.EnableSecurityCheck))
            {
                List<SearchResultItem> list = searchContext.GetQueryable<SearchResultItem>().Where<SearchResultItem>((Expression<Func<SearchResultItem, bool>>)(x => x["lockedby"].Equals(userName))).ToList<SearchResultItem>();
                if (list.Count == 0)
                    return (Item[])null;
                return list.Select<SearchResultItem, Item>((Func<SearchResultItem, Item>)(item => item.GetItem())).Distinct<Item>((IEqualityComparer<Item>)new Sitecore.Data.Comparers.ItemIdComparer()).ToArray<Item>();
            }
        }
    }
}