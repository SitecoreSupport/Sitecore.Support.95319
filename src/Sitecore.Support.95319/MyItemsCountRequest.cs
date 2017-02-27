using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.ExperienceEditor.Speak.Server.Contexts;
using Sitecore.ExperienceEditor.Speak.Server.Requests;
using Sitecore.ExperienceEditor.Speak.Server.Responses;
using System;
using System.Reflection;

namespace Sitecore.Support.ExperienceEditor.Speak.Ribbon.Requests.MyItems
{
    public class MyItemsCountRequest : PipelineProcessorRequest<ItemContext>
    {
        public override PipelineProcessorResponseValue ProcessRequest()
        {
            Assert.IsNotNull(base.RequestContext.Database, "Could not get context.Database for requestArgs:{0}", new object[]
            {
                base.Args.Data
            });
            Database database = Factory.GetDatabase(base.RequestContext.Database);
            Sitecore.Context.Items["sc_ContentDatabase"] = database;
            Assert.IsNotNull(database, "Could not get database, with name:{0}", new object[]
            {
                base.RequestContext.Database
            });
            int num = 0;
            if (Settings.WebEdit.ShowNumberOfLockedItemsOnButton)
            {
                num = ((Type.GetType("Sitecore.Search.XpathQuerySearcher,Sitecore.Kernel", true, true).InvokeMember("SelectItems", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null, new object[]
                {
                    "search://*[@__lock='%\"" + Sitecore.Context.User.Name + "\"%']",
                    database
                }) as Item[]) ?? new Item[0]).Length;
            }
            return new PipelineProcessorResponseValue
            {
                Value = num
            };
        }
    }
}