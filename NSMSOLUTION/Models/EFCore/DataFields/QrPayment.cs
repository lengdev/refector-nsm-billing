namespace NSMSOLUTION.Models.EFCore.DataFields
{
    public class QrPayment
    {
        public int QrID { get; set; }
        public string CompanyID { get; set; }
        public string QrName { get; set; }
        public string QrImagePath { get; set; }
        public bool Status { get; set; }

    }
}