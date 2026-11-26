// Customer category: report list, filters, grouping and column presets.
(function () {
  "use strict";

  app.run([
    "$rootScope",
    "ReportRegistry",
    "ReportColumns",
    "ReportGrouping",
    function ($rootScope, Registry, Columns, Grouping) {
      const { text, num, cols } = Columns;

      const profileLink = "#!/CustomerProfile?ID={customerID}";

      const customerName = text("Customer Name", {
        key: "customerName",
        link: profileLink,
      });
      const customerNameLatin = text("Customer Name Latin", {
        key: "customerNameLatin",
        link: profileLink,
      });

      Grouping.register("Customer By Block", {
        by: ["block"],
        label: "Block",
        countLabel: "customers",
        emptyLabel: "No Block",
      });

      Grouping.register("Customer Feedback Analysis", {
        by: ["topic"],
        label: "Topic",
        countLabel: "feedbacks",
        emptyLabel: "No Feedback",
      });

      // Opens "Customer Feedback" filtered by the clicked topic/answer.
      const feedbackDrill = (complaintOnly) => (row) => {
        const filters = {};
        if (row.topic === "Internet Speed") filters.internetSpeed = row.rating;
        else if (row.topic === "Customer Services")
          filters.customerServices = row.rating;
        if (complaintOnly) filters.complaint = "Complaint";

        $rootScope.$broadcast("report:drill", {
          report: "Customer Feedback",
          filters,
        });
      };

      const connectionDrill = (status) => (row) => {
        const filters = {};
        if (row.type && row.type !== "(No type)")
          filters.connectionType = row.type;
        if (status) filters.serviceStatus = status;

        $rootScope.$broadcast("report:drill", {
          report: "Customer By Connection",
          filters,
        });
      };

      const blockDrill = (reportName) => (row) => {
        const filters = {};
        if (row.block && row.block !== "(No block)") filters.block = row.block;

        $rootScope.$broadcast("report:drill", { report: reportName, filters });
      };

      Registry.add({
        order: 10,
        category: "Customer & Connection",
        groups: [
          {
            name: "Customer Report",
            monthPicker: true,
            datePreset: "all",
            filters: ["dateRange", "customer", "block", "class"],
            reports: [
              "Customer List",
              "Customer By Block Summary",
              "Customer By Block",
              "Connection Summary",
              "Customer By Connection",
              "Customer Feedback",
              "Customer Feedback Analysis",
            ],
          },
        ],
        reportFilters: {
          "Customer By Connection": {
            remove: ["class"],
            add: ["serviceStatus", "serviceId", "connectionType"],
          },
          "Connection Summary": {
            remove: ["customer", "block", "class", "dateRange"],
          },
          "Customer By Block Summary": {
            remove: ["customer", "class"],
          },
          "Customer Feedback": {
            remove: ["block", "class"],
            add: ["internetSpeed", "customerServices", "complaint"],
          },
          "Customer Feedback Analysis": {
            remove: ["customer", "block", "class"],
          },
        },
        columns: [
          {
            reports: ["Customer List"],
            strict: true,
            columns: cols(
              text("CID", { key: "customerCode" }),
              customerName,
              customerNameLatin,
              text("Register Date", { key: "registerDate" }),
              text("Phone", { key: "phone" }),
              text("Class", { key: "class" }),
              text("Block", { key: "block" }),
              text("Address", { key: "fullAddress" }),
              text("Status", { key: "status" }),
            ),
          },
          {
            reports: ["Customer By Block Summary"],
            strict: true,
            columns: cols(
              text("Block", { key: "block" }),
              num("Customers", {
                key: "customers",
                icon: "ti-arrow-right",
                click: blockDrill("Customer By Block"),
              }),
              num("Connections", {
                key: "connections",
                icon: "ti-arrow-right",
                click: blockDrill("Customer By Connection"),
              }),
              num("Active Connections", {
                key: "activeConnections",
                icon: "ti-arrow-right",
                className: "rp-good",
                click: (row) => {
                  const filters = { serviceStatus: "Active" };
                  if (row.block && row.block !== "(No block)")
                    filters.block = row.block;
                  connectionDrill(filters);
                },
              }),
            ),
          },
          {
            reports: ["Customer By Block"],
            strict: true,
            columns: cols(
              text("CID", { key: "customerCode", hidden: true }),
              customerName,
              text("Register Date", { key: "registerDate" }),
              text("Phone", { key: "phone" }),
              text("Class", { key: "class" }),
              text("SID", { key: "sid" }),
              text("Type", { key: "connectionType" }),
              text("Status", {
                key: "status",
                className: (v) => (v === "Active" ? "rp-good" : "rp-bad"),
              }),
            ),
          },
          {
            reports: ["Customer By Connection"],
            strict: true,
            columns: cols(
              text("CID", { key: "customerCode" }),
              customerName,
              text("Block", { key: "block" }),
              text("Class", { key: "class" }),
              text("SID", { key: "sid" }),
              text("Connection ID", { key: "connectionID" }),
              text("Type", { key: "type" }),
              text("VLAN", { key: "vlan", parent: "Access Type" }),
              text("PPPoE", { key: "pppoe", parent: "Access Type" }),
              text("Status", {
                key: "status",
                className: (v) => (v === "Active" ? "rp-good" : "rp-bad"),
              }),
              text("Start Date", { key: "startDate" }),
              text("End Date", { key: "endDate" }),
              num("Days Left", {
                key: "daysLeft",
                total: false,
                format: (v) =>
                  v < 0
                    ? "Expired"
                    : `${v} ${Number(v) === 1 ? "Day" : "Days"}`,
                className: (v) => (v < 0 ? "rp-bad" : v <= 7 ? "rp-warn" : ""),
              }),
              text("Remark", { key: "remark" }),
            ),
          },
          {
            reports: ["Connection Summary"],
            strict: true,
            columns: cols(
              text("Type", { key: "type" }),
              num("Active", {
                key: "active",
                icon: "ti-arrow-right",
                className: "rp-good",
                click: connectionDrill("Active"),
              }),
              num("Locked", {
                key: "locked",
                icon: "ti-arrow-right",
                click: connectionDrill("Locked"),
              }),
              num("Terminate", {
                key: "terminate",
                icon: "ti-arrow-right",
                className: "rp-bad",
                click: connectionDrill("Terminate"),
              }),
              num("Inactive", {
                key: "inactive",
                icon: "ti-arrow-right",
                click: connectionDrill("Inactive"),
              }),
              num("Total", {
                key: "total",
                icon: "ti-arrow-right",
                click: connectionDrill(null),
              }),
            ),
          },
          {
            reports: ["Customer Feedback"],
            strict: true,
            columns: cols(
              customerName,
              text("Phone", { key: "customerPhone" }),
              text("Internet Speed", { key: "internetSpeed" }),
              text("Pricing", { key: "pricing" }),
              text("Customer Services", { key: "customerServices" }),
              text("Feedback", { key: "customerComment" }),
              text("Complaint", { key: "complaint" }),
              text("Contact By", { key: "contactBy" }),
              text("Received by", { key: "createBy" }),
              text("Received Date", { key: "createDate" }),
            ),
          },
          {
            reports: ["Customer Feedback Analysis"],
            strict: true,
            columns: cols(
              text("Answer", { key: "rating" }),
              num("Feedbacks", {
                key: "feedbacks",
                total: false,
                icon: "ti-arrow-right",
                click: feedbackDrill(false),
              }),
              num("% of All Feedback", {
                key: "sharePct",
                total: false,
                decimals: 2,
                suffix: "%",
              }),
              num("With Complaint", {
                key: "complaints",
                total: false,
                icon: "ti-arrow-right",
                click: feedbackDrill(true),
              }),
              num("% With Complaint", {
                key: "complaintPct",
                total: false,
                decimals: 2,
                suffix: "%",
              }),
            ),
          },
        ],
      });
    },
  ]);
})();
