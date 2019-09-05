using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.SysPlugins.Data.Internal
{
    public class InternalDataImp : DataLoaderClassicBase<InternalDataSynchronizeContextScopedImp> //IDataLoaderPlugin
    {
        #region Constants and decarations
    private static readonly Regex _reg =
            new Regex(@"(@([a-zA-Z_][a-zA-Z0-9_\$]*))(>([a-zA-Z_][a-zA-Z0-9_\$]*))", RegexOptions.Compiled);
        #endregion Constants and decarations

        #region Configuration reading and peparation
        /*  Considerations:
         *      We have to read the configuration in a single turn and consume it from the internal configuration container
         *      in order to avoid direct hard coded dependency on the system configuration structure - most notably dependency on its
         *      structure and interpreatation.
         *  Solution:
         *      We have a nested class in which the configuration values used by this loader are collected (sometimes with some preprocessing)
         *  Lifecycle:
         *      The configuration (part of it at least) has a lifecycle equal to the node execution which is shorter than the life of the dataloader (at least potentially),
         *      so the configuration has to be reread on each node execution and thus is not persisted in the loader's instance, but passed to the main overridable methods.
         * 
         */
        protected class Configuration
        {
            public string Statement { get; internal set; }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execContext"></param>
        /// <returns></returns>
        protected Configuration ReadConfiguration(IDataLoaderContext execContext)
        {
            var cfg = new Configuration();

            if (execContext.Action == ACTION_READ)
            {
                cfg.Statement = execContext.CurrentNode.Read.Select.HasStatement() ? execContext.CurrentNode.Read.Select.Query : null;
            }
            else if (execContext.Action == ACTION_WRITE) // TODO: Does this even make sense ?
            {
                switch (execContext.Operation)
                {
                    case OPERATION_INSERT:
                        if (execContext.CurrentNode.Write.Insert.HasStatement())
                        {
                            cfg.Statement = execContext.CurrentNode.Write.Insert.Query;
                        }
                        break;
                    case OPERATION_UPDATE:
                        if (execContext.CurrentNode.Write.Update.HasStatement())
                        {
                            cfg.Statement = execContext.CurrentNode.Write.Update.Query;
                        }
                        break;
                    case OPERATION_DELETE:
                        if (execContext.CurrentNode.Write.Delete.HasStatement())
                        {
                            cfg.Statement = execContext.CurrentNode.Write.Delete.Query;
                        }
                        break;
                }
            }
            return cfg; ///////////////////////////////
        }
        #endregion


        protected override List<Dictionary<string, object>> Read(IDataLoaderReadContext execContext)
        {
            List<Dictionary<string, object>> returnResult = null;
            Configuration cfg = ReadConfiguration(execContext);
            if (!string.IsNullOrEmpty(cfg.Statement)) {
                returnResult = new List<Dictionary<string, object>>() { ProcessStatement(cfg.Statement, execContext) };
            }
            return returnResult;
        }
        protected override object Write(IDataLoaderWriteContext execContext)
        {
            //throw new NotImplementedException();
            return null;
        }
        //public Task<object> ExecuteAsync(IDataLoaderContext execContext)
        //{
        //    object returnResult = null;

        //    Configuration cfg = ReadConfiguration(execContext);
        //    if (!string.IsNullOrEmpty(cfg.Statement)) {
        //        returnResult = ProcessStatement(cfg.Statement, execContext);
        //    }

        //    return Task.FromResult(returnResult);
        //}

        private Dictionary<string, object> ProcessStatement(string statement, IDataLoaderContext execContext)
        {
            Dictionary<string, object> results = new Dictionary<string, object>();
            
            Match m = _reg.Match(statement);
            while (m.Success)
            {
                if (m.Groups[2].Success)
                {
                    string paramname = m.Groups[2].Value;
                    string jsonkey = 
                            (m.Groups[4].Success) 
                                ? m.Groups[4].Value 
                                : m.Groups[2].Value; // if no alias specifyed we assume the paramname.                    

                    var v = execContext.Evaluate(paramname);
                    if ((int)v.ValueType >= 0)
                    {
                        var resolverValue = ResolverValueDataTypeMap(v.ValueDataType, v.Value);
                        switch (v.ValueType)
                        {
                            case EResolverValueType.ValueType:
                                bool resultIsCollection = (v.ValueDataType & EValueDataType.Collection) != 0;
                                if (results.Any())
                                {
                                    if (resultIsCollection)  throw new Exception("Only the first expression is permited to return a collection");
                                    ReCodeResolverValue(resolverValue, ref results, jsonkey);
                                }
                                else
                                {
                                    if (resultIsCollection)
                                        ReCodeResolverValue(resolverValue, ref results, jsonkey);
                                    else
                                        results.Add(jsonkey, resolverValue);
                                }
                                break;
                            default:
                                break; // We shouldn't fall into this case ?
                        }
                    }
                }
                else
                {
                    throw new Exception("While parsing query we received match with invalid second group (which should be impossible)");
                }
                m = m.NextMatch();
            }

            return results;
        }

        private void ReCodeResolverValue(object resolverValue, ref Dictionary<string, object> results, string dictKey = null)
        {
            // TODO: We should combine results more granularly !
            if (results.Any())
            {
                results.Add(dictKey, resolverValue);
            }
            else
            {
                results = ReCodeResolverValue(resolverValue, dictKey);
            }
        }

        private Dictionary<string, object> ReCodeResolverValue(object resolverValue, string dictKey = null)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (resolverValue is IDictionary)
            { // single item
                result = ReDictionary(resolverValue);
                // TODO: Should we do something when the item is not converted?
            } 
            else
            {
                // TODO: Do something meaningfull with the enumerable please!
                result.Add(dictKey, resolverValue);
            }

            return result;
        }

        private static Dictionary<string, object> ReDictionary(object resolverValue)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            foreach (DictionaryEntry de in (resolverValue as IDictionary))
            {
                if (de.Key == null || !(de.Key is string))
                {
                    throw new Exception("Resolver returned data containing key(s) which are null or not a string.");
                }
                dict.Add(de.Key as string, de.Value);
            }
            return dict;
        }

        private static object ResolverValueDataTypeMap(EValueDataType valueType, object resolverValue)
        {
            // TODO: handle all cases...
            object o = resolverValue;
            switch (valueType)
            {
                case EValueDataType.Boolean:
                    o = Convert.ToBoolean(resolverValue);
                    break;
                case EValueDataType.Int:
                    o = int.Parse(resolverValue.ToString());
                    break;
                case EValueDataType.Byte:
                    o = byte.Parse(resolverValue.ToString());
                    break;
                case EValueDataType.DateTime:
                    o = DateTime.Parse(resolverValue.ToString());
                    break;
                case EValueDataType.Date:
                    o = DateTime.Parse(resolverValue.ToString()).Date;
                    break;
            }            

            return o;
        }

        //public async Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync()
        //{
        //    return await Task.FromResult(new InternalDataSynchronizeContextScopedImp());
        //}
    }
}
