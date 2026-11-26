using System;

public class AttachmentFile
{
    public int AttachmentFileID { get; set; }
    public int CustomerID { get; set; }
    public string InvoiceNo { get; set; }
    public string DestinationFolder { get; set; }
    public string AttachmentFileName { get; set; }
    public DateTime? AddDate { get; set; }
    public string UploadBy { get; set; }
    public string TypeFor { get; set; }
    public int SystemID { get; set; }
    public int WarehouseID { get; set; }
    public string DomainID { get; set; }
}