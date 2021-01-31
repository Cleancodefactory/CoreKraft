﻿using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.Settings.Modules;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Recorders.Store;
using Ccf.Ck.Utilities.NodeSetService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ccf.Ck.Web.Middleware.Tools
{
    internal class SignalDelegate
    {
        private const string HOSTINGSERVICESTARTTYPE = "hostingservice";
        private const string ONSYSTEMSTARTUPTYPE = "onsystemstartup";
        private const string ONSYSTEMSHUTDOWNTYPE = "onsystemshutdown";
        private const string INCODESTARTTYPE = "from code";

        internal static RequestDelegate ExecutionDelegate(IApplicationBuilder app, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            INodeSetService nodeSetService = app.ApplicationServices.GetService<INodeSetService>();
            KraftModuleCollection modulesCollection = app.ApplicationServices.GetService<KraftModuleCollection>();
            SignalsResponse signalsResponse = GenerateSignalResponse(kraftGlobalConfigurationSettings, modulesCollection, nodeSetService);
            RequestDelegate requestDelegate = async httpContext =>
            {
                const string contentType = "application/json";
                int statusCode = 200;
                string message = string.Empty;
                httpContext.Response.StatusCode = statusCode;
                httpContext.Response.ContentType = contentType;
                message = JsonSerializer.Serialize<SignalsResponse>(signalsResponse);
                await httpContext.Response.WriteAsync(message);
            };
            return requestDelegate;
        }

        private static SignalsResponse GenerateSignalResponse(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, KraftModuleCollection modulesCollection, INodeSetService nodeSetService)
        {
            SignalsResponse signalsResponse = new SignalsResponse();
            signalsResponse.HostingServiceSettings = kraftGlobalConfigurationSettings.GeneralSettings.HostingServiceSettings;
            signalsResponse.SignalSettings = kraftGlobalConfigurationSettings.GeneralSettings.SignalSettings;

            List<SignalWithType> signalsWithTypes = new List<SignalWithType>();
            foreach (HostingServiceSetting hostingServiceSetting in signalsResponse.HostingServiceSettings)
            {
                //Collect signals
                foreach (string signal in hostingServiceSetting.Signals)
                {
                    SignalWithType signalWithType = new SignalWithType();
                    signalWithType.SignalType = HOSTINGSERVICESTARTTYPE;
                    signalWithType.SignalName = signal;
                    signalWithType.Interval = hostingServiceSetting.IntervalInMinutes;
                    signalsWithTypes.Add(signalWithType);
                }
            }

            //Collect signals
            foreach (string signal in signalsResponse.SignalSettings.OnSystemStartup)
            {
                SignalWithType signalWithType = new SignalWithType();
                signalWithType.SignalType = ONSYSTEMSTARTUPTYPE;
                signalWithType.SignalName = signal;
                signalsWithTypes.Add(signalWithType);
            }

            //Collect signals
            foreach (string signal in signalsResponse.SignalSettings.OnSystemShutdown)
            {
                SignalWithType signalWithType = new SignalWithType();
                signalWithType.SignalType = ONSYSTEMSHUTDOWNTYPE;
                signalWithType.SignalName = signal;
                signalsWithTypes.Add(signalWithType);
            }

            //Find all signals in the nodesets
            signalsResponse.ModuleSignals = new List<ModuleSignal>();
            foreach (KraftModule module in modulesCollection.GetSortedModules())
            {
                foreach (KraftModuleSignal kraftModuleSignal in module.KraftModuleRootConf.Signals ?? new List<KraftModuleSignal>())
                {
                    ModuleSignal moduleSignal = new ModuleSignal();
                    moduleSignal.ModuleName = module.Key;
                    moduleSignal.NodeKey = kraftModuleSignal.Key;
                    moduleSignal.NodePath = kraftModuleSignal.NodePath;
                    moduleSignal.NodeSet = kraftModuleSignal.NodeSet;
                    moduleSignal.Maintenance = kraftModuleSignal.Maintenance;
                    moduleSignal.Details = GenerateDetails(module.Key, kraftModuleSignal, nodeSetService);
                    moduleSignal.Url = $"/{kraftGlobalConfigurationSettings.GeneralSettings.KraftUrlSegment}/{moduleSignal.Details.OperationReadWrite()}/signal/{module.Key}/{kraftModuleSignal.Key}?sysrequestcontent=ffff";
                    moduleSignal.ExecuteWhen = CalcExecuteWhen(signalsWithTypes, kraftModuleSignal.Key);
                    signalsResponse.ModuleSignals.Add(moduleSignal);
                }
            }

            //(www)myserver.com/node/<read/write>/signal/board/nodekey?sysrequestcontent=ffff

            return signalsResponse;
        }

        private static ModuleSignalDetails GenerateDetails(string moduleKey, KraftModuleSignal kraftModuleSignal, INodeSetService nodeSetService)
        {
            ModuleSignalDetails moduleSignalDetails = new ModuleSignalDetails();
            LoadedNodeSet nodeSet = nodeSetService.LoadNodeSet(moduleKey, kraftModuleSignal.NodeSet, kraftModuleSignal.NodePath);
            if (nodeSet.StartNode.Read != null)
            {
                moduleSignalDetails.Read = nodeSet.StartNode.Read;
                moduleSignalDetails.Read.Parameters = nodeSet.StartNode.Parameters;
                //moduleSignalDetails.InitRead();
            }
            if (nodeSet.StartNode.Write != null)
            {
                moduleSignalDetails.Write = nodeSet.StartNode.Write;
                moduleSignalDetails.Write.Parameters = nodeSet.StartNode.Parameters;
                //moduleSignalDetails.InitWrite();
            }
            return moduleSignalDetails;
        }

        private static string CalcExecuteWhen(List<SignalWithType> signalsWithTypes, string signalName)
        {
            foreach (SignalWithType signalWithType in signalsWithTypes)
            {
                if (signalWithType.SignalName.Equals(signalName, StringComparison.OrdinalIgnoreCase))
                {
                    return signalWithType.SignalType;
                }
            }
            return INCODESTARTTYPE;
        }

        private struct SignalWithType
        {
            public string SignalType { get; set; }
            public string SignalName { get; set; }
            public int Interval { get; set; }
        }
    }

    public class SignalsResponse
    {
        public List<HostingServiceSetting> HostingServiceSettings { get; set; }
        public SignalSettings SignalSettings { get; set; }
        public List<ModuleSignal> ModuleSignals { get; set; }
    }

    public class ModuleSignal
    {
        public string ModuleName { get; set; }
        public string NodeKey { get; set; }
        public string NodeSet { get; set; }
        public string NodePath { get; set; }
        public bool Maintenance { get; set; }
        public string ExecuteWhen { get; set; }
        public string Url { get; set; }
        public ModuleSignalDetails Details { get; set; }

    }

    public class ModuleSignalDetails
    {
        OperationBase _Read;
        public OperationBase Read { get; set; }
        public OperationBase Write { get; set; }
        public bool IsInconsistent { get; set; }
        public string OperationReadWrite()
        {
            string urlSegment = null;
            if (Read != null)
            {
                urlSegment = "read";
            }
            if (Write != null)
            {
                if (string.IsNullOrEmpty(urlSegment))
                {
                    urlSegment = "write";
                }
                else
                {
                    urlSegment = "<read/write>";
                    IsInconsistent = true;
                }
            }

            return urlSegment;
        }

        //internal void InitRead()
        //{
        //    throw new NotImplementedException();
        //}

        //internal void InitWrite()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
