namespace NSMSOLUTION.Models
{
    public class InsertedInvoice
    {
        public string InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceAmount { get; set; }
        public string TID { get; set; }
        public string Name { get; set; }
        public string VAT { get; set; }
        public string VATPrice { get; set; }
        public string Descript { get; set; }
    }

    public class InvoiceModel
    {
        public int TRANSACTION { get; set; }
        public string SheetName { get; set; }
        public string TYPE { get; set; }
        public string SessionId { get; set; }
        public string COM_ID { get; set; }
        public string TAX_BRANCH { get; set; }
        public string IS_BRANCH { get; set; }
        public string COMPANY_BRANCH { get; set; }
        public string MONTH { get; set; }
        public string YEAR { get; set; }
        public string INV_DATE { get; set; }
        public string INV_NO { get; set; }
        public string TOTAL_AMT { get; set; }
        public string Name { get; set; }
        public string VAT { get; set; }
        public string VATPrice { get; set; }
        public string INV_REMARK { get; set; }
        public string TID { get; set; }
        public string COM_TYPE { get; set; }
        public InsertedInvoice InsertedInvoice { get; set; }
    }

    public class ResultItem
    {
        public string MyComID { get; set; }
        public string ComID { get; set; }
        public string INV_NO { get; set; }
        public string Status { get; set; }
    }

    public class ResultInsertedInvoice
    {
        public string Message { get; set; }
        public string Result { get; set; } // Change this to a string to handle the inner JSON array
    }

    public class AddNonTaxablePerson
    {
        public string TIN { get; set; }
        public string NAME_KH { get; set; }
        public string NAME_EN { get; set; }
        public string PHONE { get; set; }
        public string sessionId { get; set; }
    }
    public class ResponseJson
    {
        public string Message { get; set; }
        public string Result { get; set; } // Keep this as a string
    }

    public class ResponseResult
    {
        public string RSLT_CD { get; set; }
        public string RSLT_MSG { get; set; }
    }
}