using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.SysPlugins.Data.Db.ADO
{
    /// <summary>
    /// The trivial derivative of the ADOProtoBase - default base class for most providers.
    /// Only providers that need specific creation of connections or management of transactions will derive from ADOProtoBase instead.
    /// </summary>
    /// <typeparam name="XConnection"></typeparam>
    public abstract class ADOBase<XConnection> : ADOProtoBase<XConnection,ADOSynchronizeContextScopedDefault<XConnection>> // IDataLoaderPlugin
        where XConnection : DbConnection, new()
    {

        #region Construction and feature configuration
        public ADOBase():base() { }
        #endregion
    }

}
