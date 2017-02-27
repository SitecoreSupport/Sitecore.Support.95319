using ComponentArt.Web.UI;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Security;
using Sitecore.Controls;
using Sitecore.Data.Comparers;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions;
using Sitecore.Web.UI.Grids;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XamlSharp.Ajax;
using Sitecore.Web.UI.XamlSharp.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Sitecore.Support.Shell.Applications.WebEdit.Dialogs.LockedItems
{
    public class LockedItemsPage : ModalDialogPage
    {
        protected Grid Items;

        protected override void ExecuteAjaxCommand(AjaxCommandEventArgs e)
        {
            Assert.ArgumentNotNull((object)e, "e");
            if (e.Name == "lockeditems:refresh")
            {
                SheerResponse.Eval("setTimeout(function(){Items.callback()}, 500)");
                //SheerResponse.Eval("Items.callback()"); 
            }
            else
                base.ExecuteAjaxCommand(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.CanRunApplication("/sitecore/content/Applications/Content Editor/Ribbons/Chunks/Locks/My Items");
            Assert.ArgumentNotNull((object)e, "e");
            base.OnLoad(e);
            if (XamlControl.AjaxScriptManager.IsEvent)
                return;
            ComponentArtGridHandler<Item>.Manage(this.Items, (IGridSource<Item>)new GridSource<Item>((IEnumerable<Item>)(this.SearchItems() ?? new Item[0])), true);
            this.Items.LocalizeGrid();
        }

        private Item[] SearchItems()
        {
            string userName = Context.User.Identity.Name;
            using (IProviderSearchContext searchContext = ContentSearchManager.GetIndex((IIndexable)new SitecoreIndexableItem(Client.ContentDatabase.GetRootItem())).CreateSearchContext(SearchSecurityOptions.EnableSecurityCheck))
            {
                List<SearchResultItem> list = searchContext.GetQueryable<SearchResultItem>().Where<SearchResultItem>((Expression<Func<SearchResultItem, bool>>)(x => x["lockedby"].Equals(userName))).ToList<SearchResultItem>();
                if (list.Count == 0)
                    return (Item[])null;
                return list.Select<SearchResultItem, Item>((Func<SearchResultItem, Item>)(item => item.GetItem())).Distinct<Item>((IEqualityComparer<Item>)new ItemIdComparer()).ToArray<Item>();
            }
        }
    }
}