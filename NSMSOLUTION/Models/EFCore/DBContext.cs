using NSMSOLUTION.Models.EFCore.DataFields;
using NSMSOLUTION.Models.Expenses;
using System.Data.Entity;

namespace NSMSOLUTION.Models.EFCore
{
    public class DBContext : DbContext
    {
        public DBContext() : base("name=AuthContextNSM") { Database.SetInitializer<DBContext>(null); }
        public DbSet<PurchaseOrder> TblPurchaseOrder { get; set; }
        public DbSet<PurchaseOrderDetail> TblPurchaseOrderDetail { get; set; }
        public DbSet<ReportStatus> TblReportStatuses { get; set; }
        public DbSet<RequestForm> TblRequestForm { get; set; }
        public DbSet<Sale> TblSales { get; set; }
        public DbSet<TelegramGroup> TblTelegramGroupNotification { get; set; }
        public DbSet<UserLogin> TblUserLogin { get; set; }
        public DbSet<QrPayment> TblQrPayment { get; set; }
        public DbSet<Customer> TblCustomer { get; set; }
        public DbSet<CustomerCompany> TblCustomerCompany { get; set; }
        public DbSet<BillInfo> TblCustomerBillInformation { get; set; }
        public DbSet<CustomerInfra> TblCustomerInfra { get; set; }
        public DbSet<Commission> TblCommission { get; set; }
        public DbSet<CustomerBalance> TblCustomerBalance { get; set; }
        public DbSet<CustomerInvoice> TblCustomerInvoice { get; set; }
        public DbSet<CustomerBillingInvoice> TblCustomerBillingInvoice { get; set; }
        public DbSet<CustomerPayment> TblCustomerPayment { get; set; }
        public DbSet<AttachmentFile> TblCustomerAttachmentFile { get; set; }
        public DbSet<BankSlip> TblCustomerBankSlip { get; set; }
        public DbSet<CustomerBlock> TblCustomerBlock { get; set; }
        public DbSet<SupportOpenTicket> TblSupportOpenTicket { get; set; }
        public DbSet<CustomerAccount> TblCustomerAccount { get; set; }
        public DbSet<CustomerDeposit> TblCustomerDeposit { get; set; }
        public DbSet<ActivateCustomer> TblActivateCustomer { get; set; }
        public DbSet<HistoryCustomerActivate> TblHistoryCustomerActivate { get; set; }
        public DbSet<CustomerInvoiceHistory> TblCustomerInvoiceHistory { get; set; }
        public DbSet<ISPLink> TblIspLink { get; set; }
        public DbSet<InfraProvider> TblInfraProvider { get; set; }
        public DbSet<AlertMessage> TblAlertMessages { get; set; }
        public DbSet<ItemOtherCharge> TblOtherCharge { get; set; }
        public DbSet<OtherChargeImage> TblOtherChargeImage { get; set; }
        public DbSet<Service> TblService { get; set; }
        public DbSet<OtherInformationCategory> TblOtherInformationCategory { get; set; }
        public DbSet<OtherInformation> TblOtherInformation { get; set; }
        public DbSet<OtherInformationType> TblOtherInformationType { get; set; }
        public DbSet<OtherIChargeImage> TblOtherIChargeImage { get; set; }
        public DbSet<DataFields.ChartOfAccount> TblChartOfAccount { get; set; }
        public DbSet<Lead> TblEmployee { get; set; }
        public DbSet<Vendor> TblVendor { get; set; }
        public DbSet<Warehouse> TblWarehouse { get; set; }
        public DbSet<Class> TblClass { get; set; }
        public DbSet<Company> TblCompany { get; set; }
        public DbSet<Payment> TblPayment { get; set; }
        public DbSet<PaymentHistory> TblPaymentHistory { get; set; }
        public DbSet<Feedback> TblCustomerFeedback { get; set; }
        public DbSet<PackageDetail> TblPackageDetail { get; set; }
        public DbSet<Item> TblItem { get; set; }
        public DbSet<StockInDetail> TblStockInDetail { get; set; }
        public DbSet<StockOut> TblStockOut { get; set; }
        public DbSet<ItemVariant> TblItemVariant { get; set; }
        public DbSet<ItemSerialDetail> TblItemSerialDetail { get; set; }
        public DbSet<Uom> TblUom { get; set; }
        public DbSet<StockIn> TblStockIn { get; set; }
        public DbSet<ItemGroup> TblItemGroup { get; set; }
        public DbSet<ItemCategory> TblItemCategory { get; set; }
        public DbSet<Category> TblCategory { get; set; }
        public DbSet<Collection> TblCollection { get; set; }
        public DbSet<CollectionDetail> TblCollectionDetail { get; set; }
        public DbSet<CollectionCharge> TblCollectionCharge { get; set; }
        public DbSet<PaymentMethod> TblPaymentMethod { get; set; }
        public DbSet<TblExpense> TblExpense { get; set; }
        public DbSet<TblExpenseDetail> TblExpenseDetail { get; set; }
        public DbSet<AccountJournalEntries> TblAccountJournalEntries { get; set; }
        public DbSet<AccountTransaction> TblAccountTransaction { get; set; }
        public DbSet<TblAccountOpeningBalance> TblAccountOpeningBalances { get; set; }
        public DbSet<AccountCategories> TblAccountCategories { get; set; }
        public DbSet<CustomerDeviceInfo> TblCustomerDeviceInfo { get; set; }
        public DbSet<CustomerDeviceInfoDetail> TblCustomerDeviceInfoDetail { get; set; }
        public DbSet<Movement> TblActivateCustomerMovement { get; set; }
        public DbSet<Device> TblDeviceName { get; set; }
        public DbSet<Splitter> TblDeviceSplitterName { get; set; }
        public DbSet<UserWarehouse> TblUserWarehouse { get; set; }
        public DbSet<Router> TblRouter { get; set; }
        public DbSet<UserRole> TblUserRole { get; set; }
        public DbSet<AccountType> TblAccountType { get; set; }
        public DbSet<CollectionHistory> TblCollectionHistory { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>()
                .ToTable("TblCompany")
                .HasKey(e => e.CompanyID);

            modelBuilder.Entity<PaymentHistory>()
                .ToTable("TblPaymentHistory")
                .HasKey(e => e.PaymentHistoryID);

            modelBuilder.Entity<Category>()
                .ToTable("TblStockName")
                .HasKey(e => e.StockNameID);

            modelBuilder.Entity<ItemCategory>()
                .ToTable("TblItemCategory")
                .HasKey(e => e.CategoryID);

            modelBuilder.Entity<ItemGroup>()
                .ToTable("TblItemGroup")
                .HasKey(e => e.ItemGroupID);

            modelBuilder.Entity<StockIn>()
                .ToTable("TblStockIn")
                .HasKey(e => e.StockInID);

            modelBuilder.Entity<Uom>()
                .ToTable("TblUom")
                .HasKey(e => e.UomID);

            modelBuilder.Entity<ItemSerialDetail>()
                .ToTable("TblItemSerialDetail")
                .HasKey(e => e.ItemSerialDetailID);

            modelBuilder.Entity<ItemVariant>()
                .ToTable("TblItemVariant")
                .HasKey(e => e.ItemVariantID);

            modelBuilder.Entity<StockOut>()
                .ToTable("TblStockOut")
                .HasKey(e => e.StockOutID);

            modelBuilder.Entity<StockInDetail>()
                .ToTable("TblStockInDetail")
                .HasKey(e => e.StockInDetailID);

            modelBuilder.Entity<Item>()
                .ToTable("TblItem")
                .HasKey(e => e.ItemID);

            modelBuilder.Entity<HistoryCustomerActivate>()
                .ToTable("TblHistoryActivateCustomer")
                .HasKey(e => e.HistoryCustomerActivateID);

            modelBuilder.Entity<PackageDetail>()
                .ToTable("TblPackageDetail")
                .HasKey(e => e.PackageDetailID);

            modelBuilder.Entity<Feedback>()
                .ToTable("TblCustomerFeedback")
                .HasKey(e => e.FeedbackID);

            modelBuilder.Entity<Payment>()
                .ToTable("TblPayment")
                .HasKey(e => e.PaymentID);

            modelBuilder.Entity<Class>()
                .ToTable("TblClass")
                .HasKey(e => e.ClassID);

            modelBuilder.Entity<Vendor>()
                .ToTable("TblVendor")
                .HasKey(e => e.VendorID);

            modelBuilder.Entity<Lead>()
                .ToTable("TblLeads")
                .HasKey(e => e.LeadID);

            modelBuilder.Entity<DataFields.ChartOfAccount>()
                .ToTable("TblChartOfAccount")
                .HasKey(e => e.ChartOfAccountID);

            modelBuilder.Entity<RequestForm>()
                .ToTable("TblRequestForm")
                .HasKey(e => e.RequestFormID);

            modelBuilder.Entity<Sale>()
                .ToTable("TblSales")
                .HasKey(e => e.SalesID);

            modelBuilder.Entity<PurchaseOrder>()
                .ToTable("TblPurchaseOrder")
                .HasKey(e => e.PurchaseOrderID);

            modelBuilder.Entity<PurchaseOrderDetail>()
                .ToTable("TblPurchaseOrderDetail")
                .HasKey(e => e.PurchaseOrderDetailID);

            modelBuilder.Entity<ReportStatus>()
                .ToTable("TblStatus")
                .HasKey(e => e.StatusID);

            modelBuilder.Entity<TelegramGroup>()
                .ToTable("TblTelegramGroupNotification")
                .HasKey(e => e.TID);

            modelBuilder.Entity<UserLogin>()
                .ToTable("TblUserLogin")
                .HasKey(e => e.UserLoginID);

            modelBuilder.Entity<QrPayment>()
                .ToTable("TblQrPayment")
                .HasKey(e => e.QrID);

            modelBuilder.Entity<Customer>()
                .ToTable("TblCustomer")
                .HasKey(e => e.CustomerID);

            modelBuilder.Entity<BillInfo>()
                .ToTable("TblCustomerBillInformation")
                .HasKey(e => e.CustomerBillInformationID);

            modelBuilder.Entity<CustomerInfra>()
                .ToTable("TblCustomerInfra")
                .HasKey(e => e.CustomerInfraID);

            modelBuilder.Entity<CustomerCompany>()
                .ToTable("TblCustomerCompany")
                .HasKey(e => e.CustomerCompanyID);

            modelBuilder.Entity<Commission>()
                .ToTable("TblCommission")
                .HasKey(e => e.CommissionID);

            modelBuilder.Entity<CustomerBalance>()
                .ToTable("TblCustomerBalance")
                .HasKey(e => e.CustomerBalanceID);

            modelBuilder.Entity<CustomerInvoice>()
                .ToTable("TblCustomerInvoice")
                .HasKey(e => e.CustomerInvoiceID);

            modelBuilder.Entity<CustomerBillingInvoice>()
                .ToTable("TblCustomerBillingInvoice")
                .HasKey(e => e.BillingInvoiceID);

            modelBuilder.Entity<CustomerPayment>()
                .ToTable("TblCustomerPayment")
                .HasKey(e => e.CustomerPaymentID);

            modelBuilder.Entity<AttachmentFile>()
                .ToTable("TblCustomerAttachmentFile")
                .HasKey(e => e.AttachmentFileID);

            modelBuilder.Entity<BankSlip>()
                .ToTable("TblCustomerBankSlip")
                .HasKey(e => e.ID);

            modelBuilder.Entity<CustomerBlock>()
                .ToTable("TblCustomerType")
                .HasKey(e => e.CustomerTypeID);

            modelBuilder.Entity<SupportOpenTicket>()
                .ToTable("TblSupportOpenTicket")
                .HasKey(e => e.OpenTicketID);

            modelBuilder.Entity<CustomerAccount>()
                .ToTable("TblCustomerAccount")
                .HasKey(e => e.CustomerAccountID);

            modelBuilder.Entity<CustomerDeposit>()
                .ToTable("TblCustomerDeposit")
                .HasKey(e => e.DepositFeeID);

            modelBuilder.Entity<ActivateCustomer>()
                .ToTable("TblActivateCustomer")
                .HasKey(e => e.ActivateCustomerID);

            modelBuilder.Entity<CustomerInvoiceHistory>()
                .ToTable("TblCustomerInvoiceHistory")
                .HasKey(e => e.CustomerInvoiceID);

            modelBuilder.Entity<ISPLink>()
                .ToTable("TblIspLink")
                .HasKey(e => e.IspLinkID);

            modelBuilder.Entity<InfraProvider>()
                .ToTable("TblInfraProvider")
                .HasKey(e => e.InfraProviderID);

            modelBuilder.Entity<AlertMessage>()
                .ToTable("TblAlertMessages")
                .HasKey(e => e.AlertID);

            modelBuilder.Entity<ItemOtherCharge>()
                .ToTable("TblOtherCharge")
                .HasKey(e => e.OtherChargeID);

            modelBuilder.Entity<OtherChargeImage>()
                .ToTable("TblOtherChargeImage")
                .HasKey(e => e.OtherChargeImageID);

            modelBuilder.Entity<OtherInformationCategory>()
                .ToTable("TblOtherInformationCategory")
                .HasKey(e => e.OtherInformationCategoryID)
                .Property(e => e.OtherInformationCategoryID).HasColumnName("CategoryID");


            modelBuilder.Entity<OtherInformation>()
                .ToTable("TblOtherInformation")
                .HasKey(e => e.OtherInformationID);

            modelBuilder.Entity<OtherInformation>()
                .Property(e => e.OtherInformationCategoryID).HasColumnName("CategoryID");

            modelBuilder.Entity<OtherIChargeImage>()
                .ToTable("TblOtherIChargeImage")
                .HasKey(e => e.OtherIChargeImageID);

            // EF's default conventions and other configurations are applied.
            base.OnModelCreating(modelBuilder);
        }
    }
}
