using Ccf.Ck.Models.Settings;
using Ccf.Ck.NodePlugins.Base;
using Ccf.Ck.NodePlugins.BindKraftIntro.Models;
using Ccf.Ck.NodePlugins.BindKraftIntro.SqlProvider;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using Ccf.Ck.Models.NodeRequest;

namespace Ccf.Ck.NodePlugins.BindKraftIntro
{
    public class BindKraftIntroMainImp : NodePluginBase<BindKraftIntroContext>
    {
        protected override void ExecuteRead(INodePluginReadContext pluginReadContext)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            var roleList = pluginReadContext.ProcessingContext.InputModel.SecurityModel.Roles;

            string operationName = pluginReadContext.Evaluate("operation").Value?.ToString()?.Replace("'", string.Empty).ToLower() ?? string.Empty;
            IIntroContentProvider contentProvider = GetContentProvider(pluginReadContext);
            switch (operationName)
            {
                case "nav":
                    {
                        result.Add("intro", contentProvider.LoadMenu(pluginReadContext));
                        break;
                    }
                case "view":
                    {
                        result.Add("intro", contentProvider.LoadIntroItem(pluginReadContext));
                        break;
                    }
                case "admin":
                    {
                        result.Add("intro", contentProvider.LoadDeletedIntroItem(pluginReadContext));
                        break;
                    }
                default:
                    {
                        throw new Exception("Operation not supported or operation parameter not supplied");
                    }
            }

            pluginReadContext.Results.Add(result);
        }

        private IIntroContentProvider GetContentProvider(INodePluginContext pluginContext)
        {
            KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings = pluginContext.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
            string moduleRoot = Path.Combine(kraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolder(pluginContext.ProcessingContext.InputModel.Module), pluginContext.ProcessingContext.InputModel.Module);
            string connectionString = pluginContext.OwnContextScoped.CustomSettings["ConnectionString"].Replace("@moduleroot@", moduleRoot);
            return new BindKraftIntroDbManager(connectionString);
        }

        protected override void ExecuteWrite(INodePluginWriteContext pw)
        {
            string state = pw.Row["state"].ToString();

            IIntroContentProvider contentProvider = GetContentProvider(pw);

            if (pw.NodeKey.Equals("harddeletefiles"))
            {
                contentProvider.HardDeleteAll();
                return;
            }

            var exampleData = pw.Row["example"] as Dictionary<string, object>;
            string exampleName = exampleData["Id"].ToString();
            IntroItem item = ConstructIntroItem(exampleData);

            switch (state)
            {
                case "1":
                    {
                        contentProvider.CreateIntroItem("ForReview", item);
                        break;
                    }
                case "2":
                    {
                        if (pw.NodeKey.Equals("approve"))
                        {
                            string sectionId = pw.Row["sectionId"].ToString();
                            contentProvider.ApproveExample(sectionId, item);
                        }
                        else
                        {
                            contentProvider.UpdateIntroItem(exampleName, item);
                        }
                        
                        break;
                    }
                case "3":
                    {
                        contentProvider.DeleteIntroItem(exampleName, item);
                        break;
                    }
                default:
                    {
                        throw new Exception("Operation not supported or operation parameter not supplied");
                    }
            }           

            if (pw.Row.ContainsKey("example"))
            {
                //var exampleData = pw.Row["example"] as Dictionary<string, object>;
                //var exampleSources = exampleData["Sources"] as Dictionary<string, object>;
                //exampleName = exampleData["Id"].ToString();

                //var entries = exampleSources["Entries"] as List<object>;

                //item = new IntroItem
                //{
                //    Id = exampleData["Id"].ToString(),
                //    Caption = exampleData["Caption"].ToString(),
                //    Description = exampleData["Description"].ToString(),
                //    Sources = new Sources(),
                //    LaunchSpec = null
                //};
                //for (int i = 0; i < entries.Count; i++)
                //{
                //    var entrie = entries[i] as Dictionary<string, object>;
                //    item.Sources.Entries.Add(new SourceEntry
                //    {
                //        Content = entrie["Content"].ToString(),
                //        Type = (ESourceType)int.Parse(entrie["Type"].ToString()),
                //        EntryName = entrie["EntryName"].ToString()
                //    });
                //}
            }

            

            
            //if (!pw.NodeKey.Equals("harddeletefiles"))
            //{
            //    var exampleData = pw.Row["example"] as Dictionary<string, object>;
            //    var exampleSources = exampleData["Sources"] as Dictionary<string, object>;
            //    string exampleName = exampleData["Id"].ToString();
                
            //    var entries = exampleSources["Entries"] as List<object>;
                
            //    IntroItem item = new IntroItem
            //    {
            //        Id = exampleData["Id"].ToString(),
            //        Caption = exampleData["Caption"].ToString(),
            //        Description = exampleData["Description"].ToString(),
            //        Sources = new Sources(),
            //        LaunchSpec = null
            //    };
            //    for (int i = 0; i < entries.Count; i++)
            //    {
            //        var entrie = entries[i] as Dictionary<string, object>;
            //        item.Sources.Entries.Add(new SourceEntry
            //        {
            //            Content = entrie["Content"].ToString(),
            //            Type = (ESourceType)int.Parse(entrie["Type"].ToString()),
            //            EntryName = entrie["EntryName"].ToString()
            //        });
            //    }

                
            //}
            //else
            //{
            //    contentProvider.HardDeleteAll();
            //}
            
        }

        private IntroItem ConstructIntroItem(Dictionary<string, object> exampleData)
        {
            var exampleSources = exampleData["Sources"] as Dictionary<string, object>;            

            var entries = exampleSources["Entries"] as List<object>;

            IntroItem item = new IntroItem
            {
                Id = exampleData["Id"].ToString(),
                Caption = exampleData["Caption"].ToString(),
                Description = exampleData["Description"].ToString(),
                Sources = new Sources(),
                LaunchSpec = null,
                OrderIdx = int.Parse(exampleData["OrderIdx"].ToString()),
                Author = exampleData["Author"].ToString()
            };
            for (int i = 0; i < entries.Count; i++)
            {
                var entrie = entries[i] as Dictionary<string, object>;
                item.Sources.Entries.Add(new SourceEntry
                {
                    Content = entrie["Content"].ToString(),
                    Type = (ESourceType)int.Parse(entrie["Type"].ToString()),
                    EntryName = entrie["EntryName"].ToString()
                });
            }

            return item;
        }
    }
}

