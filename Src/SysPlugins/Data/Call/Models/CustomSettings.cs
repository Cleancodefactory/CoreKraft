using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Linq;

namespace Ccf.Ck.SysPlugins.Data.Call.Models
{
    /// <summary>
    /// Extracts the custom settings for call data loader
    /// </summary>
    internal class CustomSettings
    {
        private const string SETTING_MODULEKEY = "ModuleKey";
        private const string SETTING_NODESETKEY = "NodesetKey";
        private const string SETTING_NODEPATHKEY = "NodepathKey";
        private const string OPERATION = "Operation";

        private const string REQUIRED_KEYVALUE_ERROR = "Please configure the loader's custom settings because the setting: {0} is required!";
        private const string REQUIRED_VALUE_ERROR = "Please configure the parameter {0}, because it is required.";

        internal CustomSettings(IDataLoaderContext execContext, bool isWriteOperation)
        {
            if (execContext.OwnContextScoped.CustomSettings.TryGetValue(OPERATION, out string operation))
            {
                OperationKey = operation;
            }

            ModuleKey = execContext.OwnContextScoped.CustomSettings[SETTING_MODULEKEY];
            NodesetKey = execContext.OwnContextScoped.CustomSettings[SETTING_NODESETKEY];
            NodepathKey = execContext.OwnContextScoped.CustomSettings[SETTING_NODEPATHKEY];

            ValidateStringIsNotEmpty(ModuleKey, string.Format(REQUIRED_KEYVALUE_ERROR, SETTING_MODULEKEY));
            ValidateStringIsNotEmpty(NodesetKey, string.Format(REQUIRED_KEYVALUE_ERROR, SETTING_NODESETKEY));
            ValidateStringIsNotEmpty(NodepathKey, string.Format(REQUIRED_KEYVALUE_ERROR, SETTING_NODEPATHKEY));

            if (!string.IsNullOrWhiteSpace(OperationKey))
            {
                string operationValueAsString = FindValue(OperationKey, execContext);

                if (string.Equals(operationValueAsString, "write", StringComparison.OrdinalIgnoreCase))
                {
                    OperationValue = true;
                }
                else if (string.Equals(operationValueAsString, "read", StringComparison.OrdinalIgnoreCase))
                {
                    OperationValue = false;
                }
            }
            else
            {
                OperationValue = isWriteOperation;
            }

            ModuleValue = FindValue(ModuleKey, execContext);
            NodesetValue = FindValue(NodesetKey, execContext);
            NodepathValue = FindValue(NodepathKey, execContext);

            ValidateStringIsNotEmpty(ModuleValue, string.Format(REQUIRED_VALUE_ERROR, ModuleKey));
            ValidateStringIsNotEmpty(NodesetValue, string.Format(REQUIRED_VALUE_ERROR, NodesetKey));
            ValidateStringIsNotEmpty(NodepathValue, string.Format(REQUIRED_VALUE_ERROR, NodepathValue));
        }

        internal string ModuleKey { get; private set; }

        internal string NodesetKey { get; private set; }

        internal string NodepathKey { get; private set; }

        internal string OperationKey { get; private set; }

        internal string ModuleValue { get; private set; }

        internal string NodesetValue { get; private set; }

        internal string NodepathValue { get; private set; }

        internal bool OperationValue { get; private set; }

        private string FindValue(string name, IDataLoaderContext execContext)
        {
            ParameterResolverValue parameterReolverValue = execContext.Evaluate(name);

            return parameterReolverValue.Value.ToString();
        }

        private void ValidateStringIsNotEmpty(string text, string message)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException(message);
            }
        }
    }
}