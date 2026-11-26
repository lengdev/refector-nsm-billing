using System;
using System.Collections.Generic;

namespace NSMSOLUTION.Library
{
    public class Library
    {
        public class MyCompanyInfo
        {
            public string RSLT_CD { get; set; }
            public MyCompanyData DATA { get; set; }
            public string RSLT_MSG { get; set; }
        }

        public class MyCompanyData
        {
            public int ROW_CNT { get; set; }
            public List<MyCompanyRecord> REC { get; set; }
            public int PAGE_NO { get; set; }
            public int TOTAL_ROW { get; set; }
        }

        public class MyCompanyRecord
        {
            public string TYPE_KH { get; set; }
            public string COM_TIN { get; set; }
            public string NAME_EN { get; set; }
            public DateTime REGISTERED_AT { get; set; }
            public string IP_ADDRESS { get; set; }
            public string ID { get; set; }
            public string TAXBRANCH_EN { get; set; }
            public DateTime LAST_LOGIN { get; set; }
            public string TYPE_EN { get; set; }
            public string TYPE { get; set; }
            public string NAME_KH { get; set; }
            public string TAXBRANCH_KH { get; set; }
        }
    }
}