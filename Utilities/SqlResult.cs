using System.Collections.Generic;
using System.Data;
using tik4net;

namespace Utilities
{
    public class SqlResult
    {
        public IEnumerable<ITikReSentence> MikrotikExecuteList { get; set; }
        public SqlResultParameters Parameters { get; set; }

        public DataTable DataTable { get; set; }
        public DataSet Data { get; set; } = new DataSet();

        public string OutPutFile { get; set; } = "";
        public string keepIDENTITYValue { get; set; } = "";
        public string OutPutUrlFile { get; set; } = "";
        public string OutPutUrlImg { get; set; } = "";

        public string Message { get; set; } = "";
        public IEnumerable<ITikReSentence> DataRowRouter { get; set; }

        public ResultType Result { get; set; } = ResultType.Failed;

        public Dictionary<string, string> DictionaryList { get; set; }
        public List<KeyValuePair<string, string>> ListKeyValuePair { get; set; }

        public string Value { get; set; } = "";
        public string Caption { get; set; } = "";

        public long ChatID { get; set; }
        public int TopicID { get; set; }

        public SqlResult()
        {
            Parameters = new SqlResultParameters
            {
                ProfileLimitation = new List<ITikReSentence>(),
                UserGroups = new List<ITikReSentence>()
            };
        }
    }

    public class SqlResultParameters
    {
        public IEnumerable<ITikReSentence> ProfileLimitation { get; set; }
        public IEnumerable<ITikReSentence> UserGroups { get; set; }
        public object UserProfileInfo { get; set; }
        public object UserProfileLimitationInfo { get; set; }
    }
}
