namespace Ccf.Ck.Models.ContextBasket
{
    public static class ModelConstants
    {
        /*
        #region Client/Server Params
        /// <summary>
        ///   These flags control which parts of the XML response are included in the server response. 
        ///   Most are a matter of out of band communication and may or may not be included during varius calls (if the rest of the infrastructure can do it) 
        ///   The client may require some parts to be send and in that case the server should include these parts. 
        ///   To achieve that the client sets those flags in a predefined query string parameter (see STUFFREQUEST_QUERYPARAM_NAME).
        /// </summary>
        public const int STUFFRESULT_VIEWS = 0x0001;
        public const int STUFFRESULT_RESOURCES = 0x0002;
        public const int STUFFRESULT_LOOKUPS = 0x0004;
        public const int STUFFRESULT_RULES = 0x0008;
        public const int STUFFRESULT_SCRIPTS = 0x0010;
        public const int STUFFRESULT_DATA = 0x0020; // See the comment below !!!
        public const int STUFFRESULT_METADATA = 0x0040; // See the comment below !!!
        // Special note about STUFFRESULT_DATA: this flag concerns tha main payload and is an exception. It is not recommended to use this flag unless the design
        // includes a dedicated action in the controller which must return the supporting parts (such as resource or lookups), but the data is missing or optional.
        public const int STUFFRESULT_RVIEWS = 0x0080; // Read-only aspect views
        public const int STUFFRESULT_ALL = 0xFFFF;
        public const string STUFFREQUEST_QUERYPARAM_NAME = "ngRequestContent";
        public const string CURRENT_SESSION_ID = "sessionid";
        #endregion Client/Server Params
        */
        #region EDataState
        /// <summary>
        /// Track state that is provided by the client-side framework
        /// </summary>
        public static string _STATE_PROPERTY_NAME = "state";
        public static string STATE_PROPERTY_NAME { get { return _STATE_PROPERTY_NAME; }  }
        public const string STATE_PROPERTY_UNCHANGED = "0";
        public const string STATE_PROPERTY_INSERT = "1";
        public const string STATE_PROPERTY_UPDATE = "2";
        public const string STATE_PROPERTY_DELETE = "3";
        public static readonly string[] STATE_VALID_VALUES = { STATE_PROPERTY_UNCHANGED, STATE_PROPERTY_INSERT, STATE_PROPERTY_UPDATE, STATE_PROPERTY_DELETE };
        #endregion

        public const string ACTION_READ = "read";
        public const string ACTION_WRITE = "write";

        public const string OPERATION_SELECT = "select";
        public const string OPERATION_INSERT = "insert";
        public const string OPERATION_DELETE = "delete";
        public const string OPERATION_UPDATE = "update";
        public const string OPERATION_PREPARE = "prepare";
        public const string OPERATION_UNCHANGED = "unchanged";

        #region Model
        //public const string EMPTY_STATEMENT = "@@empty";
        //public const string PAGING_STARTPAGE = "startrowindex";
        //public const string PAGING_TOTALPAGES = "numrows";
        //public const string PACKAGE_STATEMENT_TYPE_INSERT = "insert";
        //public const string PACKAGE_STATEMENT_TYPE_UPDATE = "update";
        //public const string PACKAGE_STATEMENT_TYPE_DELETE = "delete";

        //public const string PLUGIN_PHASE_BEFORE_SQL = "BeforeSql";
        //public const string PLUGIN_PHASE_AFTER_SQL = "AfterSql";
        //public const string PLUGIN_PHASE_AFTER_CHILDREN = "AfterChildren";

        //public const string TABLE_NAME = "tablename";
        //public const string PAGING_FIELDTOSORT = "fieldtosort";
        //public const string PAGING_SORTORDER = "sortdirection";
        //public const string VALID_KEYFILED_TABLE = "VALID_TABLENAME";
        //public const string KEYFILED_TABLE = "__TABLENAME__";
        //public const string KEYFILED_ORDER = "__ORDERBYCOLUMNNAME__";
        //public const string KEYFILED_ORDER_ASC = "ASC";
        //public const string KEYFILED_ORDER_DESC = "DESC";
        //public const string KEYFILED_LIKE_LEFT = "_left";
        //public const string KEYFILED_LIKE_RIGHT = "_right";
        //public const string KEYFILED_OBJECTNAME_FROM_PARENT = "__objectname__from_parent";
        #endregion Model

        public const string START_FOLDER_PATH_JSON_DATA = "BasePath";
    }
}
