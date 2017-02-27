using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Data.Items;

namespace Sitecore.Support.ContentSearch.ComputedFields
{
    public class LockedBy : AbstractComputedIndexField
    {
        public override object ComputeFieldValue(IIndexable indexable)
        {
            Item item = indexable as SitecoreIndexableItem;
            if (item == null || !item.Locking.IsLocked())
            {
                return null;
            }
            return item.Locking.GetOwner();
        }
    }
}