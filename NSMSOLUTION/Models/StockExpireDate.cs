using System;

namespace NSMSOLUTION.Models
{
    public class StockExpireDate
    {
        public int ItemID { get; set; }
        public int StockInID { get; set; }
        public DateTime? ExpireDate { get; set; }
        public DateTime ReceiveDate { get; set; }
        public int QTY { get; set; }
        public int UomID { get; set; }
        public string UM { get; set; }
        public int LocationID { get; set; }
        public string Barcode { get; set; }
    }
}