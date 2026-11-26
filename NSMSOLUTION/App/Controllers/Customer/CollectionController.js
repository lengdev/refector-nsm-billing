(function () {
  "use strict";

  var openingEdit = false;

  app.controller("CollectionCtrl", [
    "$scope",
    "$timeout",
    "$q",
    "CollectionService",
    "clFmt",
    function ($scope, $timeout, $q, Api, F) {
      var chargeSeq = 0;
      var customersById = {};
      var toastTimer;

      function emptyErrors() {
        return {
          customer: "",
          date: "",
          method: "",
          deposit: "",
          classId: "",
          files: "",
        };
      }

      $scope.modal = { open: false };
      $scope.ui = {
        tab: "pay",
        collapsed: false,
        confirm: false,
        printAfter: false,
        opening: false,
        saving: false,
        notify: true,
      };
      $scope.preview = { open: false, rec: null };
      $scope.toast = { show: false, bad: false, msg: "" };
      $scope.lookup = {
        customers: [],
        methods: [],
        deposits: [],
        classes: [],
        currencies: [],
        chargeTypes: [],
      };
      $scope.errors = emptyErrors();
      $scope.saved = [];
      $scope.listLoading = true; // used by the list page (CollectionListCtrl)
      $scope.customer = null;
      $scope.rows = [];
      $scope.loadingRows = false;
      $scope.charges = [];
      $scope.totals = {};
      $scope.form = {};
      $scope.newCust = {
        open: false,
        saving: false,
        name: "",
        code: "",
        phone: "",
        error: "",
      };
      $scope.editing = { active: false, id: null, number: null };

      // ============================================================
      // helpers
      // ============================================================
      function first(list) {
        return list && list.length ? list[0].value : null;
      }
      function initials(name) {
        var w = String(name || "")
          .replace(/[^\w\s]/g, " ")
          .trim()
          .split(/\s+/);
        return ((w[0] || "").charAt(0) + (w[1] || "").charAt(0)).toUpperCase();
      }
      function ageDays(iso) {
        return Math.max(
          0,
          Math.round((F.isoToDate(F.todayIso()) - F.isoToDate(iso)) / 86400000),
        );
      }
      function num(v) {
        v = Number(v);
        return isFinite(v) ? v : 0;
      }
      function norm(s) {
        return String(s == null ? "" : s)
          .trim()
          .toLowerCase();
      }
      function labelOf(list, v) {
        var it = (list || []).filter(function (x) {
          return String(x.value) === String(v);
        })[0];
        return it ? it.label : v || "—";
      }
      // what is still owed on a row after its discount
      function due(r) {
        return F.r2(r.open - r.discount);
      }
      function sum(list, fn) {
        return F.r2(
          list.reduce(function (a, x) {
            return a + fn(x);
          }, 0),
        );
      }
      function toast(msg, bad) {
        $scope.toast = { show: true, bad: !!bad, msg: msg };
        $timeout.cancel(toastTimer);
        toastTimer = $timeout(
          function () {
            $scope.toast.show = false;
          },
          bad ? 5000 : 3000,
        );
      }

      $scope.sym = function () {
        var c = ($scope.lookup.currencies || []).filter(function (x) {
          return x.value === $scope.form.currency;
        })[0];
        return c ? c.symbol : "$";
      };
      $scope.ageClass = function (d) {
        return d > 60 ? "cl-age-hi" : d > 30 ? "cl-age-mid" : "cl-age-lo";
      };

      // ============================================================
      // payment slips
      // ============================================================
      var MAX_SLIPS = 6;
      var MAX_SLIP_BYTES = 5 * 1024 * 1024;
      var SLIP_TYPES = ["image/jpeg", "image/png", "image/webp"];
      var slipSeq = 0;

      $scope.slips = [];
      $scope.slipView = { open: false, file: null, index: 0 };
      $scope.slipMax = MAX_SLIPS;

      function slipSize(n) {
        return n >= 1048576
          ? (n / 1048576).toFixed(1) + " MB"
          : Math.max(1, Math.round(n / 1024)) + " KB";
      }
      $scope.addSlips = function (files) {
        $scope.errors.files = "";
        Array.prototype.forEach.call(files || [], function (file) {
          if (SLIP_TYPES.indexOf(file.type) < 0) {
            $scope.errors.files =
              file.name + " is not a JPG, PNG or WEBP image.";
            return;
          }
          if (file.size > MAX_SLIP_BYTES) {
            $scope.errors.files = file.name + " is larger than 5 MB.";
            return;
          }
          if ($scope.slips.length >= MAX_SLIPS) {
            $scope.errors.files =
              "You can attach up to " + MAX_SLIPS + " slips.";
            return;
          }
          $scope.slips.push({
            id: ++slipSeq,
            file: file,
            name: file.name,
            sizeLabel: slipSize(file.size),
            url: URL.createObjectURL(file),
          });
        });
      };
      $scope.removeSlip = function (slip) {
        var i = $scope.slips.indexOf(slip);
        if (i < 0) return;
        URL.revokeObjectURL(slip.url);
        $scope.slips.splice(i, 1);
        $scope.errors.files = "";
      };
      $scope.viewSlip = function (slip) {
        var i = $scope.slips.indexOf(slip);
        if (i < 0) return;
        $scope.slipView = { open: true, file: slip, index: i };
      };
      $scope.closeSlip = function () {
        $scope.slipView.open = false;
      };
      $scope.stepSlip = function (d) {
        var n = $scope.slips.length;
        if (!n) return;
        var i = ($scope.slipView.index + d + n) % n;
        $scope.slipView = { open: true, file: $scope.slips[i], index: i };
      };
      $scope.clearSlips = function () {
        $scope.slipView.open = false;
        $scope.slips.forEach(function (s) {
          URL.revokeObjectURL(s.url);
        });
        $scope.slips.length = 0;
        $scope.errors.files = "";
      };

      // ============================================================
      // receipt number
      // ============================================================
      var numberReq = 0;

      function loadNumber() {
        var req = ++numberReq;
        var classId = $scope.form.classId;

        if (!classId) {
          $scope.form.number = "";
          return;
        }

        Api.getNextNumber(classId)
          .then(function (n) {
            // an edited receipt keeps its own number
            if (req === numberReq && !$scope.editing.active) {
              $scope.form.number = n;
            }
          })
          .catch(function (e) {
            if (req === numberReq) {
              toast(
                (e && e.message) || "Could not load collection number",
                true,
              );
            }
          });
      }

      $scope.onClass = function () {
        $scope.errors.classId = "";
        applyClass();
        loadNumber();
      };

      // ============================================================
      // customers
      // ============================================================
      var customerReq = 0;
      var invoicesReq = 0;
      var searchTimer;
      var skipAutoDeposit = false; // set when a new receipt is opened on purpose, without loading a deposit
      var lastSearch = "";

      function customerItem(c) {
        return {
          value: c.id,
          label: c.name,
          sub: c.code || c.id,
          find: [c.name, c.latin, c.code, c.id].join(" "),
          right: c.count
            ? $scope.sym() + F.fmt(c.balance)
            : c.credit > 0
              ? "Credit " + F.fmt(c.credit)
              : "",
        };
      }

      $scope.customersLoading = false;

      function loadCustomers(search) {
        var req = ++customerReq;
        lastSearch = search || "";
        $scope.customersLoading = true;
        return Api.getCustomers(lastSearch)
          .then(function (list) {
            if (req !== customerReq) return;
            list.forEach(function (c) {
              c.initials = initials(c.name);
              customersById[c.id] = c;
            });
            var items = list.map(customerItem);
            var sel = $scope.customer;
            if (
              sel &&
              !list.some(function (c) {
                return c.id === sel.id;
              })
            ) {
              items.unshift(customerItem(customersById[sel.id] || sel));
            }
            $scope.lookup.customers = items;
          })
          .finally(function () {
            if (req === customerReq) $scope.customersLoading = false;
          });
      }

      $scope.searchCustomers = function (text) {
        text = (text || "").trim();
        $timeout.cancel(searchTimer);
        $scope.customersLoading = text !== lastSearch;
        searchTimer = $timeout(function () {
          if (text === lastSearch) return;
          loadCustomers(text).catch(function (e) {
            toast(e.message, true);
          });
        }, 250);
      };

      $scope.openAddCustomer = function (text) {
        $scope.newCust = {
          open: true,
          saving: false,
          name: (text || "").trim(), // prefill with what the user typed
          code: "",
          phone: "",
          error: "",
        };
        $scope.$broadcast("cl-focus-newcust");
      };

      $scope.saveCustomer = function () {
        var n = $scope.newCust;
        if (n.saving) return;
        if (!n.name.trim()) {
          n.error = "Enter the customer name";
          return;
        }
        n.saving = true;
        n.error = "";
        Api.addCustomer({
          name: n.name.trim(),
          code: n.code.trim(),
          phone: n.phone.trim(),
        })
          .then(function (c) {
            c.initials = initials(c.name);
            c.count = c.count || 0;
            c.balance = c.balance || 0;
            c.credit = c.credit || 0;
            customersById[c.id] = c;
            $scope.lookup.customers.unshift(customerItem(c));
            $scope.form.customerId = c.id;
            $scope.onCustomer(); // sets customer, loads invoices and credits
            n.open = false;
            toast(c.name + " added");
          })
          .catch(function (e) {
            n.error = e.message;
          })
          .finally(function () {
            n.saving = false;
          });
      };

      $scope.onCustomer = function () {
        $scope.customer = customersById[$scope.form.customerId] || null;
        $scope.errors.customer = "";

        // a deposit receipt that was auto-loaded belongs to the previous customer: drop it
        var ed = $scope.editing;
        if (
          ed.active &&
          ed.auto &&
          String($scope.form.customerId) !== String(ed.customerId)
        ) {
          $scope.editing = { active: false, id: null, number: null };
          $scope.charges = [];
          loadNumber();
        }

        loadInvoices();

        if (skipAutoDeposit) {
          skipAutoDeposit = false;
          return;
        }

        // customer has invoices and an RV that still holds a deposit: update that RV
        var c = $scope.customer;
        if ($scope.editing.active || !c || !(c.count > 0)) return;

        var cid = c.id;
        Api.getCredits(cid, null)
          .then(function (list) {
            if (!list || !list.length) return;
            if (
              $scope.editing.active ||
              !$scope.customer ||
              $scope.customer.id !== cid
            )
              return;
            $scope.openEdit({ id: list[0].id }, true); // oldest deposit first
          })
          .catch(function (e) {
            toast(
              (e && e.message) || "Could not load the customer's deposit",
              true,
            );
          });
      };

      // ============================================================
      // payment method
      // ============================================================
      $scope.onMethod = function () {
        $scope.errors.method = "";
        $scope.errors.deposit = "";
      };
      $scope.onCurrency = function () {
        recalc();
      };

      // ============================================================
      // saved list + lookups
      // ============================================================
      function loadSaved() {
        return Api.getList().then(function (list) {
          // old receipts may use the hidden Customer Credit method, so label from the full list
          var all = $scope.lookup.allMethods || $scope.lookup.methods;
          $scope.saved = list.map(function (r) {
            r.method = labelOf(all, r.method);
            r.deposit = labelOf($scope.lookup.deposits, r.deposit);
            return r;
          });
        });
      }

      Api.getLookups()
        .then(function (l) {
          angular.extend($scope.lookup, l);
          reset();
          return loadSaved();
        })
        .catch(function (e) {
          toast(e.message, true);
        })
        .finally(function () {
          $scope.listLoading = false;
        });

      // used by the list page: Refresh button and after a rollback
      $scope.reloadSaved = function () {
        $scope.listLoading = true;
        return loadSaved()
          .catch(function (e) {
            toast(e.message, true);
          })
          .finally(function () {
            $scope.listLoading = false;
          });
      };
      $scope.notify = toast;
      $scope.showReceipt = openPreview; // used by the list: click a receipt number

      function reset() {
        var iso = F.todayIso();
        invoicesReq++;
        $scope.loadingRows = false;
        $scope.customer = null;
        $scope.rows = [];
        $scope.charges = [];
        $scope.ui.tab = "pay";
        $scope.ui.notify = true;
        $scope.errors = emptyErrors();
        $scope.clearSlips();
        $scope.form = {
          customerId: null,
          dateIso: iso,
          dateText: F.dmy(iso),
          dateObj: F.isoToDate(iso),
          amount: 0,
          method: null, // required, no default
          deposit: null,
          classId: null,
          currency: first($scope.lookup.currencies),
          memo: "",
          number: "",
        };
        recalc();
      }

      // ============================================================
      // open invoices
      // ============================================================
      function toRow(i) {
        var date = String(i.date || "").slice(0, 10);
        var type = i.type || "";
        return angular.extend({}, i, {
          date: date,
          total: num(i.total),
          open: num(i.open),
          kind:
            i.kind ||
            (/opening/i.test(type) ? "ob" : /debit/i.test(type) ? "dn" : "inv"),
          discount: 0,
          payment: 0, // paid on this invoice
          checked: false,
          age: ageDays(date),
        });
      }

      function matchesClass(r) {
        var id = $scope.form.classId;
        if (!id) return true;
        if (r.classId != null && r.classId !== "") {
          return String(r.classId) === String(id);
        }
        return norm(r.className) === norm(labelOf($scope.lookup.classes, id));
      }

      // Class chosen: its invoices are selected in full and all others are disabled.
      // No class: every row is enabled and the selection is left alone.
      function applyClass() {
        var on = !!$scope.form.classId;
        var selectable = 0;
        $scope.rows.forEach(function (r) {
          r.disabled = on && !matchesClass(r);
          if (r.disabled) {
            r.checked = false;
            r.payment = 0;
            r.discount = 0;
          } else {
            selectable++;
            if (on) {
              r.checked = true;
              r.payment = due(r);
            }
          }
        });
        if (on && selectable) {
          syncAmount();
        } else {
          recalc(); // keeps a typed amount, so it can still become a deposit
        }
      }

      function loadInvoices() {
        var c = $scope.customer;
        var req = ++invoicesReq;

        $scope.rows = [];
        $scope.form.amount = 0;

        if (!c) {
          $scope.loadingRows = false;
          recalc();
          return;
        }

        $scope.loadingRows = true;
        recalc();

        Api.getOpenInvoices(
          c.id,
          $scope.form.dateIso,
          $scope.editing.active ? $scope.editing.id : null,
        )
          .then(function (list) {
            if (req !== invoicesReq) return;
            $scope.rows = (list || []).map(toRow);
            applyClass();
          })
          .catch(function (e) {
            if (req === invoicesReq) {
              $scope.rows = [];
              recalc();
              toast(e.message, true);
            }
          })
          .finally(function () {
            $scope.loadingRows = false;
          });
      }

      // ============================================================
      // totals + allocation
      // ============================================================
      function recalc() {
        var rows = $scope.rows,
          f = $scope.form;
        var pay = sum(rows, function (r) {
          return r.checked ? r.payment : 0;
        });
        var disc = sum(rows, function (r) {
          return r.checked ? r.discount : 0;
        });
        var chg = sum($scope.charges, function (c) {
          return c.amount;
        });
        $scope.totals = {
          pay: pay, // paid on the invoices
          credit: 0,
          applied: pay, // what the invoices receive in total
          disc: disc,
          chg: chg,
          sub: F.r2(pay + disc),
          grand: F.r2(pay + chg), // money the customer pays now
          checked: rows.filter(function (r) {
            return r.checked;
          }).length,
          grandTotal: sum(rows, function (r) {
            return r.total;
          }),
          open: sum(rows, function (r) {
            return r.open;
          }),
          excess: F.r2((f.amount || 0) - pay), // above the invoices stays as deposit
        };
        $scope.totals.pct = $scope.totals.open
          ? Math.min(
              100,
              Math.round(($scope.totals.applied / $scope.totals.open) * 100),
            )
          : 0;
      }
      $scope.recalc = recalc;

      // Typed Amount Paid: pays the selected invoices, oldest first.
      function allocate(cash) {
        var rem = cash;
        $scope.rows.forEach(function (r) {
          if (r.disabled) {
            r.payment = 0;
            r.checked = false;
            return;
          }
          var p = F.r2(Math.max(0, Math.min(rem, due(r))));
          r.payment = p;
          r.checked = p > 0;
          rem = F.r2(rem - p);
        });
      }

      function enabledRows() {
        return $scope.rows.filter(function (r) {
          return !r.disabled;
        });
      }

      // after any change to the selection: Amount Paid = what the invoices need,
      // but never less than the deposit already on the receipt (the rest stays as deposit)
      function syncAmount() {
        var pay = sum($scope.rows, function (r) {
          return r.checked ? r.payment : 0;
        });
        var floor = $scope.editing.active ? num($scope.editing.deposit) : 0;
        $scope.form.amount = Math.max(pay, floor);
        recalc();
      }

      // Receipt that holds a deposit: an invoice takes only what is still left on it.
      // null = no limit (normal receipt).
      function depositLeft(except) {
        var dep = $scope.editing.active ? num($scope.editing.deposit) : 0;
        if (dep <= 0.004) return null;
        var used = sum($scope.rows, function (r) {
          return r !== except && r.checked ? r.payment : 0;
        });
        return Math.max(0, F.r2(dep - used));
      }
      function defaultPay(r) {
        var cap = depositLeft(r);
        return cap === null ? due(r) : F.r2(Math.min(due(r), cap));
      }

      $scope.selectable = function () {
        return enabledRows().length;
      };
      $scope.allChecked = function () {
        var e = enabledRows();
        return (
          e.length > 0 &&
          e.every(function (r) {
            return r.checked;
          })
        );
      };
      $scope.someChecked = function () {
        return (
          enabledRows().some(function (r) {
            return r.checked;
          }) && !$scope.allChecked()
        );
      };
      $scope.toggleAll = function () {
        var on = !$scope.allChecked();
        $scope.rows.forEach(function (r) {
          if (r.disabled) return;
          r.checked = false;
          r.payment = 0;
        });
        // oldest first, each invoice takes what the deposit still covers
        $scope.rows.forEach(function (r) {
          if (r.disabled || !on) return;
          r.payment = defaultPay(r);
          r.checked = r.payment > 0;
        });
        syncAmount();
      };
      $scope.classNote = function () {
        if (!$scope.form.classId || !$scope.rows.length) return "";
        var off = $scope.rows.filter(function (r) {
          return r.disabled;
        }).length;
        if (off === $scope.rows.length) {
          return "No open invoice in this class. Any amount paid will be saved as customer credit.";
        }
        return off
          ? off +
              " invoice" +
              (off > 1 ? "s" : "") +
              " of other classes disabled."
          : "";
      };

      $scope.onAmount = function () {
        allocate($scope.form.amount || 0);
        recalc();
      };
      $scope.onRowCheck = function (r) {
        r.payment = r.checked ? defaultPay(r) : 0;
        if (r.checked && r.payment <= 0) {
          r.checked = false;
          toast(
            "This deposit is fully used. Collect the rest on a new receipt.",
            true,
          );
        }
        syncAmount();
      };
      $scope.onPayment = function (r) {
        r.payment = Math.max(0, Math.min(num(r.payment), due(r)));
        r.checked = r.payment > 0;
        syncAmount();
      };
      $scope.onDiscount = function (r) {
        r.discount = Math.min(r.discount, r.open);
        r.payment = r.checked ? defaultPay(r) : 0;
        syncAmount();
      };

      $scope.excessMsg = function () {
        var t = $scope.totals;
        var dep = $scope.editing.active ? num($scope.editing.deposit) : 0;

        // a loaded deposit is shown by the "Applied credit" link instead
        if (dep > 0.004) return "";

        var ex = t.excess;
        if (!(ex > 0.004)) return "";
        return $scope.rows.length
          ? $scope.sym() +
              F.fmt(ex) +
              " is more than the selected open balances. The extra stays as deposit."
          : "No open invoice. " +
              $scope.sym() +
              F.fmt(ex) +
              " will be saved as a deposit.";
      };

      // "Applied credit" link: only when the loaded receipt holds a deposit
      $scope.hasDeposit = function () {
        return $scope.editing.active && num($scope.editing.deposit) > 0.004;
      };
      $scope.creditLabel = function () {
        return $scope.totals.pay > 0.004 ? "Applied credit" : "Deposit credit";
      };
      $scope.creditNote = function () {
        var t = $scope.totals,
          dep = num($scope.editing.deposit);
        var more = F.r2(Math.max(0, t.pay - dep));
        var left = F.r2(Math.max(0, ($scope.form.amount || 0) - t.pay));
        if (more > 0.004)
          return "collect " + $scope.sym() + F.fmt(more) + " more";
        if (left > 0.004)
          return $scope.sym() + F.fmt(left) + " stays as deposit";
        return "";
      };

      // click the link: show the deposit receipt and its history
      var viewingDeposit = false;
      $scope.viewDeposit = function () {
        if (viewingDeposit || !$scope.editing.id) return;
        viewingDeposit = true;
        Api.getDetails($scope.editing.id)
          .then(openPreview)
          .catch(function (e) {
            toast((e && e.message) || "Could not load the deposit", true);
          })
          .finally(function () {
            viewingDeposit = false;
          });
      };

      // Customer with no open invoice at all: the receipt is a deposit (real RV number, nothing applied).
      $scope.noInvoices = function () {
        return (
          !$scope.editing.active &&
          !!$scope.customer &&
          !$scope.loadingRows &&
          $scope.rows.length === 0
        );
      };
      $scope.isDeposit = function () {
        return (
          $scope.noInvoices() &&
          num($scope.form.amount) > 0 &&
          !$scope.charges.some(function (c) {
            return c.amount > 0;
          })
        );
      };

      // ============================================================
      // other charges
      // ============================================================
      $scope.addCharge = function () {
        $scope.charges.push({
          id: ++chargeSeq,
          type: first($scope.lookup.chargeTypes),
          note: "",
          amount: 0,
        });
        recalc();
      };
      $scope.removeCharge = function (i) {
        $scope.charges.splice(i, 1);
        recalc();
      };
      $scope.onCharge = function () {
        recalc();
      };
      $scope.setTab = function (t) {
        $scope.ui.tab = t;
        $scope.ui.collapsed = false;
      };

      // ============================================================
      // date
      // ============================================================
      function applyDate(iso) {
        var f = $scope.form;
        f.dateObj = F.isoToDate(iso);
        if (f.dateIso !== iso) {
          f.dateIso = iso;
          if ($scope.customer) loadInvoices(); // open balance and age depend on the date
        }
        $scope.errors.date = "";
      }
      $scope.onDateTyped = function () {
        var iso = F.parseDmy($scope.form.dateText);
        if (iso) applyDate(iso);
      };
      $scope.onDateBlur = function () {
        var f = $scope.form,
          iso = F.parseDmy(f.dateText);
        if (iso) {
          f.dateText = F.dmy(iso);
          applyDate(iso);
        } else if (f.dateText) {
          $scope.errors.date = "Enter a valid date as dd-mm-yyyy";
        }
      };
      $scope.onPickDate = function () {
        var f = $scope.form;
        if (!f.dateObj) return;
        var iso = F.toIso(f.dateObj);
        f.dateText = F.dmy(iso);
        applyDate(iso);
      };

      // ============================================================
      // edit a posted receipt
      // ============================================================
      $scope.openEdit = function (row, fromCustomer) {
        if (!row || !row.id) return;
        if (openingEdit || $scope.ui.opening || $scope.ui.saving) return;

        openingEdit = true;
        $scope.ui.opening = true;

        function abort(e) {
          $scope.modal.open = false;
          $scope.editing = { active: false, id: null, number: null };
          reset();
          toast((e && e.message) || "Could not load collection", true);
        }

        Api.getDetails(row.id)
          .then(function (rec) {
            if (!rec || !rec.customerId) {
              throw new Error("Collection details came back incomplete");
            }
            if (rec.status && rec.status !== "Posted") {
              throw new Error("Only posted collections can be edited");
            }
            if (String(rec.methodId) === String($scope.lookup.creditMethodId)) {
              throw new Error(
                "This receipt used the old Customer Credit method. Roll it back and create a new one.",
              );
            }

            var iso = String(rec.date || "").slice(0, 10);

            reset();

            $scope.editing = {
              active: true,
              id: rec.id,
              number: rec.no,
              deposit: num(rec.credit) > 0.004 ? num(rec.amountPaid) : 0,
              auto: !!fromCustomer, // loaded automatically after picking a customer
              customerId: rec.customerId,
            };

            // combo values are strings, so convert
            var f = $scope.form;
            f.customerId = rec.customerId;
            f.classId = rec.classId != null ? String(rec.classId) : null;
            f.method = String(rec.methodId);
            f.deposit = rec.depositId != null ? String(rec.depositId) : null;
            f.currency = rec.currency;
            f.memo = rec.memo || "";
            f.number = rec.no;
            f.dateIso = iso;
            f.dateText = F.dmy(iso);
            f.dateObj = F.isoToDate(iso);

            $scope.modal.open = true;
            $scope.loadingRows = true;

            // search by code or id, because the display name has a prefix
            return loadCustomers(rec.code || String(rec.customerId))
              .then(function () {
                var c = customersById[rec.customerId];
                if (!c) {
                  c = {
                    id: rec.customerId,
                    name: rec.customer || String(rec.customerId),
                    code: rec.code || "",
                    initials: initials(rec.customer),
                    count: 0,
                    balance: 0,
                    credit: 0,
                  };
                  customersById[c.id] = c;
                }

                var inList = $scope.lookup.customers.some(function (x) {
                  return String(x.value) === String(c.id);
                });
                if (!inList) $scope.lookup.customers.unshift(customerItem(c));

                $scope.customer = c;

                // open invoices, with this receipt's own allocations added back
                return Api.getOpenInvoices(rec.customerId, iso, rec.id);
              })
              .then(function (list) {
                $scope.rows = (list || []).map(toRow);

                // the class can't change on edit, so disable other-class invoices
                // without touching the restored selection
                var on = !!f.classId;
                $scope.rows.forEach(function (r) {
                  r.disabled = on && !matchesClass(r);
                });

                // the server stores the TOTAL paid on each invoice (credit + cash)
                (rec.lines || []).forEach(function (line) {
                  var r = $scope.rows.find(function (x) {
                    return String(x.id) === String(line.invoiceId);
                  });
                  if (!r) return;
                  r.checked = true;
                  r.discount = num(line.disc);
                  r.payment = num(line.pay);
                });

                $scope.charges = (rec.charges || []).map(function (ch) {
                  return {
                    id: ++chargeSeq,
                    type: ch.type,
                    note: ch.note || "",
                    amount: num(ch.amount),
                  };
                });

                f.amount = num(rec.amountPaid);
                recalc();

                if (fromCustomer && num(rec.credit) > 0.004) {
                  toast(
                    rec.no +
                      " loaded · deposit " +
                      $scope.sym() +
                      F.fmt(rec.credit),
                  );
                }
              });
          })
          .catch(abort)
          .finally(function () {
            $scope.loadingRows = false;
            $scope.ui.opening = false;
            openingEdit = false;
          });
      };

      // ============================================================
      // validation
      // ============================================================
      // Checks every required field, shows all errors at once and focuses the first one
      // in screen order: customer, date, method, deposit, class.
      function validate() {
        var f = $scope.form,
          e = ($scope.errors = emptyErrors()),
          focus = null;

        function bad(field, msg, focusEvent) {
          e[field] = msg;
          if (!focus) focus = focusEvent;
        }

        if (!$scope.customer) {
          bad(
            "customer",
            "Select a customer to collect from",
            "cl-focus-customer",
          );
        }
        if (!F.parseDmy(f.dateText)) {
          bad("date", "Enter a valid date as dd-mm-yyyy", "cl-focus-date");
        }
        if (!f.method) {
          bad("method", "Select a payment method", "cl-focus-method");
        }
        if (f.method && !f.deposit) {
          bad(
            "deposit",
            "Choose the account this money goes into",
            "cl-focus-deposit",
          );
        }
        if (!f.classId) {
          bad("classId", "Select a class", "cl-focus-class");
        }

        if (focus) {
          toast("Complete the required fields", true);
          $scope.$broadcast(focus);
          return false;
        }

        if ($scope.totals.applied + $scope.totals.chg <= 0 && !(f.amount > 0)) {
          toast("Select invoices to collect or enter an amount paid", true);
          return false;
        }
        return true;
      }

      // ============================================================
      // save / preview
      // ============================================================
      function buildRequest() {
        var f = $scope.form;
        return {
          customerId: f.customerId,
          dateIso: f.dateIso,
          number: f.number,
          methodId: f.method,
          depositId: Number(f.deposit) || 0,
          classId: f.classId ? Number(f.classId) : null,
          currency: f.currency,
          memo: f.memo,
          notifySlip: $scope.ui.notify !== false,
          amountPaid: F.r2(f.amount || 0),
          lines: $scope.rows
            .filter(function (r) {
              return r.checked && r.payment > 0;
            })
            .map(function (r) {
              return {
                invoiceId: r.id,
                invoiceNo: r.no,
                discount: r.discount || 0,
                payment: F.r2(r.payment),
              };
            }),
          charges: $scope.charges
            .filter(function (c) {
              return c.amount > 0;
            })
            .map(function (c) {
              return { type: c.type, note: c.note, amount: c.amount };
            }),
        };
      }

      function buildRecord() {
        var f = $scope.form,
          t = $scope.totals,
          L = $scope.lookup;
        return {
          no: f.number,
          date: f.dateIso,
          customer: $scope.customer ? $scope.customer.name : "—",
          method: labelOf(L.methods, f.method),
          deposit: labelOf(L.deposits, f.deposit),
          memo: f.memo,
          sym: $scope.sym(),
          pay: t.applied,
          disc: t.disc,
          chg: t.chg,
          grand: F.r2(t.applied + t.chg),
          credit: t.excess > 0 ? t.excess : 0,
          creditUsed: t.credit,
          lines: $scope.rows
            .filter(function (r) {
              return r.checked;
            })
            .map(function (r) {
              return {
                no: r.no,
                type: r.type,
                date: r.date,
                open: r.open,
                disc: r.discount,
                pay: F.r2(r.payment),
              };
            }),
          charges: $scope.charges
            .filter(function (c) {
              return c.amount > 0;
            })
            .map(function (c) {
              return { type: c.type, note: c.note, amount: c.amount };
            }),
        };
      }

      function openPreview(rec) {
        $scope.preview.rec = rec;
        $scope.preview.open = true;
      }

      $scope.previewDraft = function () {
        if (!$scope.customer) {
          toast("Select a customer to preview the receipt", true);
          return;
        }
        openPreview(buildRecord());
      };

      function afterSave(saved, mode, skipped, asDeposit) {
        var isEdit = mode === "edit";
        var msg =
          saved.no +
          (isEdit ? " updated" : asDeposit ? " saved as deposit" : " saved");
        var warn = false;
        if (asDeposit) saved.type = "Deposit";

        if (saved.unapplied > 0 && !asDeposit) {
          msg +=
            " · " + saved.sym + F.fmt(saved.unapplied) + " kept as deposit";
        }

        // Telegram notification
        if (saved.notified) {
          msg += " · sent to Telegram";
        } else if (skipped) {
          msg += " · Telegram alert skipped";
        } else if (saved.notified === false && saved.notifyMessage) {
          msg += " · Telegram not sent: " + saved.notifyMessage;
          warn = true;
        }

        // device check done by the server after payment (SID -> activate customer)
        var services = saved.services || [];
        var failed = services.filter(function (s) {
          return s.mode === "Failed";
        });
        var reenabled = services.filter(function (s) {
          return s.mode === "Activated";
        }).length;

        if (failed.length) {
          msg +=
            " · " +
            failed
              .map(function (s) {
                return s.message;
              })
              .join(" ");
          warn = true;
        } else if (reenabled) {
          msg +=
            " · " +
            reenabled +
            " service" +
            (reenabled > 1 ? "s" : "") +
            " re-enabled";
        }

        try {
          // update the existing row or add the new one
          var index = $scope.saved.findIndex(function (x) {
            return String(x.id) === String(saved.id);
          });
          if (index >= 0) {
            $scope.saved[index] = saved;
          } else {
            $scope.saved.unshift(saved);
          }

          // credit balances changed (used or created), so reload them
          loadCustomers("").catch(function () {});

          if (mode === "new") {
            reset();
            $scope.$broadcast("cl-focus-customer");
          } else {
            closeModal();
          }

          if ($scope.ui.printAfter) openPreview(saved);
        } catch (e) {
          // The server already saved. Never report that as a failed save.
          if (window.console)
            console.error("[Collection] After-save UI error:", e);
        }

        toast(msg, warn);
      }

      // A deposit RV was just used. If invoices are still open, load the customer's next deposit RV
      // (oldest first). When none is left, open a new receipt for the remaining cash.
      function chainNextDeposit(customerId, savedId) {
        loadCustomers(String(customerId))
          .then(function () {
            var c = customersById[customerId];
            if (!c || !(c.count > 0)) return; // everything is paid

            return Api.getCredits(customerId, null).then(function (list) {
              var next = (list || []).filter(function (x) {
                return String(x.id) !== String(savedId);
              })[0];

              if (next) {
                $scope.openEdit({ id: next.id }, true);
                return;
              }

              skipAutoDeposit = true;
              $scope.openAdd();
              $scope.form.customerId = customerId;
              $scope.onCustomer();
              toast(
                "No deposit left. Collect the remaining " +
                  $scope.sym() +
                  F.fmt(c.balance) +
                  " on a new receipt.",
              );
            });
          })
          .catch(function (e) {
            toast((e && e.message) || "Could not load the next deposit", true);
          });
      }

      $scope.save = function (mode) {
        if ($scope.ui.saving || !validate()) return;

        var rec = buildRecord();
        rec.req = buildRequest();
        var slips = $scope.slips.slice();
        var skipped = !$scope.ui.notify;
        var wasEdit = $scope.editing.active;
        var asDeposit = !wasEdit && $scope.isDeposit();
        var depositBefore = wasEdit ? num($scope.editing.deposit) : 0;
        var custId = $scope.form.customerId;
        var editId = $scope.editing.id;

        $scope.ui.saving = true;

        // Api.save can throw before it returns a promise; without this the overlay would stay on.
        var request;
        try {
          request = wasEdit
            ? Api.update(editId, rec, slips)
            : Api.save(rec, slips);
        } catch (e) {
          $scope.ui.saving = false;
          toast((e && e.message) || "Could not send the request", true);
          return;
        }

        // success and failure are separate callbacks, so an error inside afterSave
        // can never be shown as a failed save
        request
          .then(
            function (saved) {
              afterSave(saved, wasEdit ? "edit" : mode, skipped, asDeposit);
              if (depositBefore > 0.004) chainNextDeposit(custId, editId);
            },
            function (e) {
              toast((e && e.message) || "Save failed", true);
            },
          )
          .finally(function () {
            $scope.ui.saving = false;
          });
      };

      // ============================================================
      // modal open / close
      // ============================================================
      $scope.openAdd = function () {
        resetEditing();
        reset();
        $scope.modal.open = true;
        $timeout.cancel(searchTimer);
        loadCustomers("").catch(function (e) {
          toast(e.message, true);
        });
        $scope.$broadcast("cl-focus-customer");
      };
      function resetEditing() {
        $scope.editing = { active: false, id: null, number: null };
      }
      function closeModal() {
        $scope.modal.open = false;
        $scope.ui.confirm = false;
        $scope.preview.open = false;
        $scope.clearSlips();
        resetEditing();
      }
      $scope.closeModal = closeModal;

      function isDirty() {
        var f = $scope.form;
        return !!(
          f.customerId ||
          f.method ||
          f.classId ||
          f.deposit ||
          $scope.slips.length ||
          $scope.charges.length ||
          (f.memo || "").trim() ||
          f.amount > 0
        );
      }
      $scope.attemptClose = function () {
        if (isDirty()) {
          $scope.ui.confirm = true;
        } else {
          closeModal();
        }
      };

      $scope.onKeydown = function (e) {
        if ($scope.slipView.open) {
          if (e.key === "Escape") {
            $scope.closeSlip();
          } else if (e.key === "ArrowLeft") {
            $scope.stepSlip(-1);
          } else if (e.key === "ArrowRight") {
            $scope.stepSlip(1);
          }
          return;
        }
        if (e.key !== "Escape") return;
        if ($scope.newCust.open) {
          $scope.newCust.open = false;
        } else if ($scope.preview.open) {
          $scope.preview.open = false;
        } else if ($scope.ui.confirm) {
          $scope.ui.confirm = false;
        } else {
          $scope.attemptClose();
        }
      };
      $scope.onScrimClick = function (e) {
        if (e.target === e.currentTarget) $scope.preview.open = false;
      };
    },
  ]);

  // ================================================================
  // List page: search, filters, sort, paging, export, rollback
  // ================================================================
  app.controller("CollectionListCtrl", [
    "$scope",
    "CollectionService",
    "clFmt",
    function ($scope, Api, F) {
      $scope.presets = [
        { key: "today", label: "Today" },
        { key: "week", label: "This week" },
        { key: "month", label: "This month" },
        { key: "all", label: "All" },
        { key: "custom", label: "Custom" },
      ];
      $scope.sizes = [10, 25, 50, 100];

      var list = ($scope.list = {
        q: "",
        preset: "all",
        fromObj: null,
        toObj: null,
        method: null,
        sort: { key: "date", dir: "asc" },
        size: 10,
        page: 1,
        pages: 1,
        items: [],
        methods: [],
        count: 0,
        from: 0,
        to: 0,
        total: 0,
        avg: 0,
        pageSum: 0,
        top: null,
        sym: "$",
        rangeLabel: "All time",
      });

      $scope.rollbackDlg = { open: false, busy: false, row: null, error: "" };

      function notify(msg, bad) {
        ($scope.notify || angular.noop)(msg, bad);
      }
      function pad(n) {
        return n < 10 ? "0" + n : String(n);
      }
      function isoOf(d) {
        return (
          d.getFullYear() + "-" + pad(d.getMonth() + 1) + "-" + pad(d.getDate())
        );
      }

      function rangeFor(preset) {
        var now = new Date();
        var today = isoOf(now);
        if (preset === "today") return { from: today, to: today };
        if (preset === "week") {
          var mon = new Date(now);
          mon.setDate(now.getDate() - ((now.getDay() + 6) % 7));
          return { from: isoOf(mon), to: today };
        }
        if (preset === "month") {
          return {
            from: isoOf(new Date(now.getFullYear(), now.getMonth(), 1)),
            to: today,
          };
        }
        if (preset === "custom") {
          return {
            from: list.fromObj ? isoOf(list.fromObj) : null,
            to: list.toObj ? isoOf(list.toObj) : null,
          };
        }
        return { from: null, to: null };
      }

      function label(rg) {
        if (list.preset === "all") return "All time";
        if (list.preset === "today") return "Today";
        if (list.preset === "week") return "This week";
        if (list.preset === "month") return "This month";
        if (rg.from && rg.to) return F.dmy(rg.from) + " to " + F.dmy(rg.to);
        if (rg.from) return "From " + F.dmy(rg.from);
        if (rg.to) return "Up to " + F.dmy(rg.to);
        return "All time";
      }

      // Money received on a receipt = applied to invoices/charges (grand)
      // plus whatever was kept as customer credit (credit).
      function amt(r) {
        return F.r2((Number(r.grand) || 0) + (Number(r.credit) || 0));
      }

      function val(r, key) {
        if (key === "amount") return amt(r);
        if (key === "date") return String(r.date || "").slice(0, 10);
        return String(r[key] == null ? "" : r[key]).toLowerCase();
      }
      function compare(a, b) {
        var k = list.sort.key;
        var x = val(a, k);
        var y = val(b, k);
        var c =
          typeof x === "number" ? x - y : String(x).localeCompare(String(y));
        if (c === 0) c = String(a.no).localeCompare(String(b.no));
        return list.sort.dir === "asc" ? c : -c;
      }

      function refresh() {
        var all = $scope.saved || [];

        var seen = {};
        list.methods = all
          .map(function (r) {
            return r.method;
          })
          .filter(function (m) {
            if (!m || seen[m]) return false;
            seen[m] = true;
            return true;
          })
          .sort();
        if (list.method && !seen[list.method]) list.method = null;

        var rg = rangeFor(list.preset);
        var q = (list.q || "").trim().toLowerCase();

        var rows = all
          .filter(function (r) {
            var d = String(r.date || "").slice(0, 10);
            if (rg.from && d < rg.from) return false;
            if (rg.to && d > rg.to) return false;
            if (list.method && r.method !== list.method) return false;
            if (q) {
              var hay = [
                r.no,
                r.customer,
                r.method,
                r.deposit,
                r.grand,
                $scope.typeLabel(r),
              ]
                .join(" ")
                .toLowerCase();
              if (hay.indexOf(q) < 0) return false;
            }
            return true;
          })
          .sort(compare);

        var byMethod = {};
        var total = 0;
        rows.forEach(function (r) {
          var g = amt(r);
          total += g;
          var m =
            byMethod[r.method] ||
            (byMethod[r.method] = { label: r.method, sum: 0, count: 0 });
          m.sum += g;
          m.count++;
        });
        var top = null;
        Object.keys(byMethod).forEach(function (k) {
          if (!top || byMethod[k].sum > top.sum) top = byMethod[k];
        });
        if (top) top.sum = F.r2(top.sum);

        list.count = rows.length;
        list.total = F.r2(total);
        list.avg = rows.length ? F.r2(total / rows.length) : 0;
        list.top = top;
        list.sym = all.length ? all[0].sym || "$" : "$";
        list.rangeLabel = label(rg);

        list.pages = Math.max(1, Math.ceil(rows.length / list.size));
        if (list.page > list.pages) list.page = list.pages;
        if (list.page < 1) list.page = 1;

        var start = (list.page - 1) * list.size;
        list.items = rows.slice(start, start + list.size);
        list.from = rows.length ? start + 1 : 0;
        list.to = Math.min(rows.length, start + list.size);
        list.pageSum = F.r2(
          list.items.reduce(function (a, r) {
            return a + amt(r);
          }, 0),
        );
        list.filtered = rows;
      }

      function filterKey() {
        return [
          list.q,
          list.preset,
          list.fromObj ? +list.fromObj : "",
          list.toObj ? +list.toObj : "",
          list.method,
          list.sort.key,
          list.sort.dir,
          list.size,
        ].join("|");
      }

      $scope.$watch(filterKey, function (n, o) {
        if (n !== o) list.page = 1;
        refresh();
      });
      $scope.$watch("list.page", refresh);
      $scope.$watchCollection("saved", refresh);

      $scope.setPreset = function (key) {
        list.preset = key;
        if (key !== "custom") {
          list.fromObj = null;
          list.toObj = null;
        }
      };
      $scope.onDate = function () {
        list.preset = "custom";
      };
      $scope.hasFilters = function () {
        return !!(
          (list.q || "").trim() ||
          list.preset !== "all" ||
          list.method
        );
      };
      $scope.clearFilters = function () {
        list.q = "";
        list.method = null;
        $scope.setPreset("all");
      };

      $scope.sortBy = function (key) {
        var same = list.sort.key === key;
        list.sort = {
          key: key,
          dir: same
            ? list.sort.dir === "asc"
              ? "desc"
              : "asc"
            : key === "date" || key === "amount"
              ? "desc"
              : "asc",
        };
      };
      $scope.sortClass = function (key) {
        return list.sort.key === key
          ? {
              "cx-sorted": true,
              "cx-asc": list.sort.dir === "asc",
              "cx-desc": list.sort.dir === "desc",
            }
          : {};
      };
      $scope.ariaSort = function (key) {
        return list.sort.key === key
          ? list.sort.dir === "asc"
            ? "ascending"
            : "descending"
          : "none";
      };

      $scope.go = function (n) {
        list.page = Math.min(Math.max(1, n), list.pages);
      };

      // receipts saved with the old Customer Credit method (list page only)
      $scope.isCredit = function (r) {
        return r.method === "Customer Credit";
      };
      $scope.amt = amt;
      // Type column: "Deposit" while nothing is applied yet, otherwise "Receipt"
      $scope.isDepositRow = function (r) {
        return r.type === "Deposit";
      };
      $scope.typeLabel = function (r) {
        return r.type === "Deposit" ? "Deposit" : "Receipt";
      };
      $scope.hasCredit = function (r) {
        return (Number(r.credit) || 0) > 0.004;
      };
      // short note under the amount when part or all of a receipt became credit
      $scope.creditNote = function (r) {
        if (r.type === "Deposit") return "Deposit · not used yet";
        if (!$scope.hasCredit(r)) return "";
        return (Number(r.grand) || 0) <= 0.004
          ? "Advance payment: saved as credit"
          : r.sym + F.fmt(r.credit) + " kept as deposit";
      };
      $scope.reload = function () {
        return ($scope.reloadSaved || angular.noop)();
      };

      // ---------- export / print ----------
      function csv(v) {
        v = v == null ? "" : String(v);
        return /[",\r\n]/.test(v) ? '"' + v.replace(/"/g, '""') + '"' : v;
      }
      $scope.exportCsv = function () {
        var rows = list.filtered || [];
        if (!rows.length) return;
        var lines = [
          [
            "Number",
            "Date",
            "Customer",
            "Method",
            "Deposit To",
            "Amount",
            "Kept as deposit",
            "Type",
          ].join(","),
        ];
        rows.forEach(function (r) {
          lines.push(
            [
              r.no,
              String(r.date || "").slice(0, 10),
              r.customer,
              r.method,
              r.deposit === "—" ? "" : r.deposit,
              amt(r).toFixed(2),
              (Number(r.credit) || 0).toFixed(2),
              $scope.typeLabel(r),
            ]
              .map(csv)
              .join(","),
          );
        });
        var blob = new Blob(["\uFEFF" + lines.join("\r\n")], {
          type: "text/csv;charset=utf-8",
        });
        var a = document.createElement("a");
        a.href = URL.createObjectURL(blob);
        a.download = "collections-" + F.todayIso() + ".csv";
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        setTimeout(function () {
          URL.revokeObjectURL(a.href);
        }, 1000);
      };
      $scope.printList = function () {
        // print every filtered row, not only the current page
        var prev = list.size;
        list.size = Math.max(prev, list.count);
        $scope.$applyAsync(function () {
          window.print();
          list.size = prev;
        });
      };

      // ---------- copy number ----------
      $scope.copyNo = function (r) {
        function done() {
          $scope.$applyAsync(function () {
            notify(r.no + " copied");
          });
        }
        if (navigator.clipboard && navigator.clipboard.writeText) {
          navigator.clipboard.writeText(r.no).then(done);
          return;
        }
        var t = document.createElement("textarea");
        t.value = r.no;
        document.body.appendChild(t);
        t.select();
        try {
          document.execCommand("copy");
        } catch (e) {}
        document.body.removeChild(t);
        done();
      };

      // ---------- view a receipt ----------
      // Loads the full receipt (lines, charges, credit used) and opens the preview in modal.html.
      $scope.viewRow = function (r) {
        if (list.viewing || !r.id) return;
        list.viewing = r.no;
        Api.getDetails(r.id)
          .then(function (rec) {
            ($scope.showReceipt || angular.noop)(rec);
          })
          .catch(function (e) {
            notify(e.message, true);
          })
          .finally(function () {
            list.viewing = null;
          });
      };

      // ---------- rollback ----------
      $scope.askRollback = function (r) {
        $scope.rollbackDlg = { open: true, busy: false, row: r, error: "" };
      };

      $scope.confirmRollback = function () {
        var d = $scope.rollbackDlg;
        if (d.busy || !d.row) return;

        d.busy = true;
        d.error = "";

        Api.rollbackCollection(d.row.id)
          .then(function () {
            var i = ($scope.saved || []).indexOf(d.row);
            if (i >= 0) $scope.saved.splice(i, 1);

            d.open = false;
            notify(d.row.no + " rolled back");
            $scope.reload();
          })
          .catch(function (e) {
            d.error = e.message || "Could not rollback collection";
          })
          .finally(function () {
            d.busy = false;
          });
      };
    },
  ]);
})();
