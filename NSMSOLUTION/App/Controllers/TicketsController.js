"use strict";
app.controller("TicketsController", [
    "$rootScope",
    "$scope",
    "Form",
    "$location",
    function ($rootScope, $scope, Form, $location) {
        $rootScope.title = "ISP Support Tickets";
        $scope.IsLoaded = false;

        // ============================================================
        // STATIC DATA — replace with API/service calls later
        // ============================================================
        var STATUSES = [
            "OPEN",
            "ASSIGNED",
            "IN_PROGRESS",
            "WAITING_CUSTOMER",
            "SCHEDULED",
            "RESOLVED",
            "CLOSED",
            "REOPENED",
            "CANCELLED",
        ];

        var CATEGORIES = [
            "Connectivity",
            "Performance",
            "Billing",
            "Hardware",
            "Installation",
            "Other",
        ];
        var PRIORITIES = ["High", "Medium", "Low"];

        var STAFF = [
            { id: 1, name: "Dara Sok", role: "Network Technician" },
            { id: 2, name: "Sophea Chan", role: "Billing Support" },
            { id: 3, name: "Vuthy Meas", role: "Field Technician" },
        ];

        var TICKETS = [
            {
                id: "TCK-1001",
                subject: "No internet connection since morning",
                customerName: "Chan Dara",
                customerPhone: "012 345 678",
                category: "Connectivity",
                priority: "High",
                status: "OPEN",
                assignedTo: null,
                createdAt: "2026-08-10T08:15:00",
                updatedAt: "2026-08-10T08:15:00",
                description:
                    "Customer reports total loss of connection since 7AM. Router lights show no signal.",
                comments: [],
            },
            {
                id: "TCK-1002",
                subject: "Slow speed during evening hours",
                customerName: "Sreymom Ly",
                customerPhone: "092 111 222",
                category: "Performance",
                priority: "Medium",
                status: "ASSIGNED",
                assignedTo: 1,
                createdAt: "2026-08-09T14:30:00",
                updatedAt: "2026-08-10T09:00:00",
                description: "Speeds drop below 5mbps between 7-11PM daily.",
                comments: [
                    {
                        author: "Dara Sok",
                        text: "Scheduled a line check for tomorrow.",
                        at: "2026-08-10T09:00:00",
                    },
                ],
            },
            {
                id: "TCK-1003",
                subject: "Billing discrepancy on last invoice",
                customerName: "Pisach Heng",
                customerPhone: "070 555 999",
                category: "Billing",
                priority: "Low",
                status: "IN_PROGRESS",
                assignedTo: 2,
                createdAt: "2026-08-08T10:00:00",
                updatedAt: "2026-08-09T16:20:00",
                description:
                    "Customer was charged twice for the installation fee.",
                comments: [],
            },
            {
                id: "TCK-1004",
                subject: "Router replacement requested",
                customerName: "Sokha Prum",
                customerPhone: "015 888 222",
                category: "Hardware",
                priority: "Medium",
                status: "SCHEDULED",
                assignedTo: 3,
                createdAt: "2026-08-07T09:45:00",
                updatedAt: "2026-08-09T11:10:00",
                description:
                    "Old router overheating. Technician visit scheduled for Aug 13.",
                comments: [],
            },
            {
                id: "TCK-1005",
                subject: "Intermittent disconnects",
                customerName: "Rithy Vann",
                customerPhone: "011 222 333",
                category: "Connectivity",
                priority: "High",
                status: "WAITING_CUSTOMER",
                assignedTo: 1,
                createdAt: "2026-08-06T13:00:00",
                updatedAt: "2026-08-08T10:30:00",
                description:
                    "Awaiting customer confirmation of available time slot for on-site check.",
                comments: [],
            },
            {
                id: "TCK-1006",
                subject: "Installation completed, confirming closure",
                customerName: "Bopha Sok",
                customerPhone: "096 777 444",
                category: "Installation",
                priority: "Low",
                status: "RESOLVED",
                assignedTo: 3,
                createdAt: "2026-08-04T08:00:00",
                updatedAt: "2026-08-05T15:00:00",
                description: "New line installed and tested successfully.",
                comments: [],
            },
            {
                id: "TCK-1007",
                subject: "Monthly plan upgrade",
                customerName: "Chenda Kim",
                customerPhone: "089 456 123",
                category: "Billing",
                priority: "Low",
                status: "CLOSED",
                assignedTo: 2,
                createdAt: "2026-08-01T10:00:00",
                updatedAt: "2026-08-02T09:00:00",
                description:
                    "Upgraded from 30mbps to 100mbps plan, confirmed by customer.",
                comments: [],
            },
            {
                id: "TCK-1008",
                subject: "Connection dropped again after fix",
                customerName: "Chan Dara",
                customerPhone: "012 345 678",
                category: "Connectivity",
                priority: "High",
                status: "REOPENED",
                assignedTo: 1,
                createdAt: "2026-08-05T07:30:00",
                updatedAt: "2026-08-10T07:45:00",
                description:
                    "Same customer as TCK-1001, issue returned two days after being marked resolved.",
                comments: [],
            },
            {
                id: "TCK-1009",
                subject: "Duplicate ticket - customer cancelled request",
                customerName: "Vanna Ouk",
                customerPhone: "017 999 000",
                category: "Other",
                priority: "Low",
                status: "CANCELLED",
                assignedTo: null,
                createdAt: "2026-08-03T12:00:00",
                updatedAt: "2026-08-03T12:30:00",
                description:
                    "Customer opened this by mistake; original ticket TCK-1002 covers the issue.",
                comments: [],
            },
        ];

        // Add synthetic tickets for pagination testing and larger dataset.
        var EXTRA_CUSTOMERS = [
            "Sophea Chan",
            "Mony Neang",
            "Srey Leak",
            "Dara Sok",
            "Vuthy Meas",
            "Rithy Vann",
            "Chenda Kim",
            "Pisach Heng",
            "Sokha Prum",
            "Vanna Ouk",
        ];
        var EXTRA_SUBJECTS = [
            "Intermittent disconnect after power outage",
            "Unable to access account portal",
            "Router blinking orange after restart",
            "New installation appointment confirmation",
            "Invoice total seems incorrect",
            "Customer requests upgrade to fiber",
            "Signal dropping during rain",
            "Payment method needs update",
            "Ticket reopened for recurring issue",
            "Request to close resolved ticket",
        ];
        var EXTRA_DESCRIPTIONS = [
            "Customer reports that the internet connection drops intermittently when the router restarts.",
            "Client cannot log in to the service portal with correct credentials.",
            "Router LED shows an orange warning after a recent firmware update.",
            "Customer asks to confirm the installation visit date and time.",
            "Invoice amount includes fees that customer does not recognize.",
            "Subscriber wants to upgrade to higher bandwidth plan.",
            "Service drops only during stormy weather and needs investigation.",
            "Payment card failed and needs a new card on file.",
            "Issue reoccurred after being marked resolved and needs review.",
            "Customer would like this ticket closed after work is verified.",
        ];
        var extraStatus = [
            "OPEN",
            "ASSIGNED",
            "IN_PROGRESS",
            "WAITING_CUSTOMER",
            "SCHEDULED",
            "RESOLVED",
            "CLOSED",
            "REOPENED",
            "CANCELLED",
        ];
        var extraPriority = ["High", "Medium", "Low"];
        var extraCategories = [
            "Connectivity",
            "Performance",
            "Billing",
            "Hardware",
            "Installation",
            "Other",
        ];
        var now = new Date();
        for (var i = TICKETS.length + 1; i <= 100; i++) {
            var idx = i - TICKETS.length - 1;
            var customerIndex = idx % EXTRA_CUSTOMERS.length;
            var subjectIndex = idx % EXTRA_SUBJECTS.length;
            var descriptionIndex = idx % EXTRA_DESCRIPTIONS.length;
            var statusIndex = idx % extraStatus.length;
            var priorityIndex = idx % extraPriority.length;
            var categoryIndex = idx % extraCategories.length;
            var created = new Date(
                now.getTime() - i * 3600 * 1000,
            ).toISOString();
            var updated = new Date(
                now.getTime() - (i - 1) * 1800 * 1000,
            ).toISOString();

            TICKETS.push({
                id: "TCK-" + (1000 + i),
                subject: EXTRA_SUBJECTS[subjectIndex],
                customerName: EXTRA_CUSTOMERS[customerIndex],
                customerPhone: "077 " + (100000 + i).toString().slice(-6),
                category: extraCategories[categoryIndex],
                priority: extraPriority[priorityIndex],
                status: extraStatus[statusIndex],
                assignedTo:
                    statusIndex === 0 || statusIndex === 8
                        ? null
                        : (statusIndex % STAFF.length) + 1,
                createdAt: created,
                updatedAt: updated,
                description: EXTRA_DESCRIPTIONS[descriptionIndex],
                comments: [],
            });
        }

        // ============================================================
        // END STATIC DATA
        // ============================================================

        $scope.Statuses = STATUSES;
        $scope.Categories = CATEGORIES;
        $scope.Priorities = PRIORITIES;
        $scope.Staff = STAFF;
        $scope.Tickets = TICKETS;
        $scope.FilteredTickets = [];
        $scope.SelectedTicket = null;
        $scope.NewComment = { text: "" };
        $scope.ShowNewForm = false;
        $scope.ShowAllComments = false;

        $scope.SearchText = "";
        $scope.StatusFilter = "ALL";
        $scope.PriorityFilter = "ALL";

        $scope.NewTicket = {
            subject: "",
            customerName: "",
            customerPhone: "",
            category: "Connectivity",
            priority: "Medium",
            description: "",
        };

        $scope.ShowFlowGuide = false;
        $scope.PageSizes = [10, 25, 50, 100];
        $scope.PageSize = 10;
        $scope.CurrentPage = 1;
        $scope.TotalPages = 1;
        $scope.PagedTickets = [];

        $scope.ToggleFlowGuide = function () {
            $scope.ShowFlowGuide = !$scope.ShowFlowGuide;
        };

        function GetPageNumbers() {
            var pages = [];
            for (var i = 1; i <= $scope.TotalPages; i++) {
                pages.push(i);
            }
            return pages;
        }
        $scope.GetPageNumbers = GetPageNumbers;

        function UpdatePagedTickets() {
            $scope.TotalPages = Math.max(
                1,
                Math.ceil($scope.FilteredTickets.length / $scope.PageSize),
            );
            if ($scope.CurrentPage > $scope.TotalPages) {
                $scope.CurrentPage = $scope.TotalPages;
            }
            if ($scope.CurrentPage < 1) {
                $scope.CurrentPage = 1;
            }
            var start = ($scope.CurrentPage - 1) * $scope.PageSize;
            $scope.PagedTickets = $scope.FilteredTickets.slice(
                start,
                start + $scope.PageSize,
            );
        }
        $scope.UpdatePagedTickets = UpdatePagedTickets;

        function ChangePage(page) {
            if (page < 1 || page > $scope.TotalPages) {
                return;
            }
            $scope.CurrentPage = page;
            UpdatePagedTickets();
        }
        $scope.ChangePage = ChangePage;

        function ChangePageSize() {
            $scope.CurrentPage = 1;
            UpdatePagedTickets();
        }
        $scope.ChangePageSize = ChangePageSize;

        function ExportTickets() {
            var rows = $scope.FilteredTickets.map(function (t) {
                return [
                    t.id,
                    t.subject,
                    t.customerName,
                    t.customerPhone,
                    t.category,
                    t.priority,
                    t.status,
                    StaffName(t.assignedTo),
                    t.updatedAt,
                ];
            });
            var csv = [
                [
                    "Ticket ID",
                    "Subject",
                    "Customer",
                    "Phone",
                    "Category",
                    "Priority",
                    "Status",
                    "Assigned To",
                    "Updated",
                ],
            ]
                .concat(rows)
                .map(function (row) {
                    return row
                        .map(function (field) {
                            var value = field == null ? "" : String(field);
                            return '"' + value.replace(/"/g, '""') + '"';
                        })
                        .join(",");
                })
                .join("\r\n");

            var blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
            var link = document.createElement("a");
            link.href = window.URL.createObjectURL(blob);
            link.download = "tickets_export.csv";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.URL.revokeObjectURL(link.href);
        }
        $scope.ExportTickets = ExportTickets;

        // ---------- Helpers ----------
        function StaffName(staffId) {
            var match = STAFF.filter(function (s) {
                return s.id === staffId;
            })[0];
            return match ? match.name : "Unassigned";
        }
        $scope.StaffName = StaffName;

        function AddActivityLog(ticket, text) {
            if (!ticket || !text) {
                return;
            }
            ticket.comments.unshift({
                author: "System",
                text: text,
                at: new Date().toISOString(),
            });
        }

        function StatusClass(status) {
            return (status || "").toLowerCase().replace(/_/g, "-");
        }
        $scope.StatusClass = StatusClass;

        function ApplyFilters() {
            var text = ($scope.SearchText || "").toLowerCase();

            $scope.FilteredTickets = $scope.Tickets.filter(function (t) {
                var matchesStatus =
                    $scope.StatusFilter === "ALL" ||
                    t.status === $scope.StatusFilter;
                var matchesPriority =
                    $scope.PriorityFilter === "ALL" ||
                    t.priority === $scope.PriorityFilter;
                var matchesText =
                    !text ||
                    t.subject.toLowerCase().indexOf(text) > -1 ||
                    t.customerName.toLowerCase().indexOf(text) > -1 ||
                    t.id.toLowerCase().indexOf(text) > -1;

                return matchesStatus && matchesPriority && matchesText;
            }).sort(function (a, b) {
                return new Date(b.updatedAt) - new Date(a.updatedAt);
            });

            $scope.CurrentPage = 1;
            UpdatePagedTickets();
        }
        $scope.ApplyFilters = ApplyFilters;

        // ---------- Counts for summary badges ----------
        function RecalcCounts() {
            var counts = {};
            STATUSES.forEach(function (s) {
                counts[s] = 0;
            });
            $scope.Tickets.forEach(function (t) {
                counts[t.status] = (counts[t.status] || 0) + 1;
            });
            $scope.StatusCounts = counts;
        }
        $scope.RecalcCounts = RecalcCounts;

        // ---------- Actions ----------
        $scope.RefreshTickets = function () {
            $scope.IsLoaded = false;
            // Simulated load delay — swap for a real API call later
            setTimeout(function () {
                ApplyFilters();
                RecalcCounts();
                UpdatePagedTickets();
                $scope.IsLoaded = true;
                $scope.$applyAsync();
            }, 300);
        };

        $scope.SelectTicket = function (ticket) {
            $scope.SelectedTicket = ticket;
            $scope.NewComment.text = "";
            $scope.ShowAllComments = false;
        };

        $scope.CloseDetail = function () {
            $scope.SelectedTicket = null;
        };

        $scope.AssignTicket = function (ticket, staffId, previousAssignee) {
            var oldAssignee =
                previousAssignee != null ? previousAssignee : ticket.assignedTo;
            var previousStatus = ticket.status;
            var newAssignee = staffId ? parseInt(staffId, 10) : null;
            ticket.assignedTo = newAssignee;
            if (ticket.status === "OPEN" && ticket.assignedTo) {
                ticket.status = "ASSIGNED";
            }
            ticket.updatedAt = new Date().toISOString();

            if (oldAssignee !== newAssignee) {
                var previousName = StaffName(oldAssignee);
                var newName = StaffName(newAssignee);
                var activity = newAssignee
                    ? "Assigned to " + newName + "."
                    : "Unassigned from " + previousName + ".";
                AddActivityLog(ticket, activity);
                if (window.toastr) {
                    toastr.success(activity, "Ticket updated");
                }
            }

            if (previousStatus !== ticket.status) {
                var statusMessage =
                    "Status changed from " +
                    previousStatus +
                    " to " +
                    ticket.status +
                    ".";
                AddActivityLog(ticket, statusMessage);
                if (window.toastr) {
                    toastr.info(statusMessage, "Ticket status");
                }
            }

            ApplyFilters();
            RecalcCounts();
            UpdatePagedTickets();
        };

        $scope.ChangeStatus = function (ticket, previousStatus) {
            var newStatus = ticket.status;
            ticket.updatedAt = new Date().toISOString();

            if (previousStatus !== newStatus) {
                var statusMessage =
                    "Status changed from " +
                    previousStatus +
                    " to " +
                    newStatus +
                    ".";
                AddActivityLog(ticket, statusMessage);
                if (window.toastr) {
                    toastr.info(statusMessage, "Ticket status");
                }
            }

            ApplyFilters();
            RecalcCounts();
            UpdatePagedTickets();
        };

        $scope.AddComment = function () {
            if (
                !$scope.SelectedTicket ||
                !$scope.NewComment.text ||
                !$scope.NewComment.text.trim()
            ) {
                return;
            }
            var commentText = $scope.NewComment.text.trim();
            $scope.SelectedTicket.comments.unshift({
                author: "You",
                text: commentText,
                at: new Date().toISOString(),
            });
            $scope.SelectedTicket.updatedAt = new Date().toISOString();
            $scope.NewComment.text = "";
            ApplyFilters();
            UpdatePagedTickets();
        };

        $scope.ToggleNewForm = function () {
            $scope.ShowNewForm = !$scope.ShowNewForm;
        };

        $scope.ToggleComments = function () {
            $scope.ShowAllComments = !$scope.ShowAllComments;
        };

        $scope.CreateTicket = function () {
            if (!$scope.NewTicket.subject || !$scope.NewTicket.customerName) {
                return;
            }
            var nextNum = 1000 + $scope.Tickets.length + 1;
            var now = new Date().toISOString();

            $scope.Tickets.unshift({
                id: "TCK-" + nextNum,
                subject: $scope.NewTicket.subject,
                customerName: $scope.NewTicket.customerName,
                customerPhone: $scope.NewTicket.customerPhone,
                category: $scope.NewTicket.category,
                priority: $scope.NewTicket.priority,
                status: "OPEN",
                assignedTo: null,
                createdAt: now,
                updatedAt: now,
                description: $scope.NewTicket.description,
                comments: [],
            });

            $scope.NewTicket = {
                subject: "",
                customerName: "",
                customerPhone: "",
                category: "Connectivity",
                priority: "Medium",
                description: "",
            };
            $scope.ShowNewForm = false;

            ApplyFilters();
            RecalcCounts();
        };

        // ---------- Init ----------
        ApplyFilters();
        RecalcCounts();
        $scope.IsLoaded = true;
    },
]);
