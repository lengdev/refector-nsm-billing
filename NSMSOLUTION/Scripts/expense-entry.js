(function ($) {
    "use strict";

    var itemIndex = 0;
    var additionalIndex = 0;

    function money(value) {
        value = parseFloat(value) || 0;
        return value.toLocaleString("en-US", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function num(value) {
        value = parseFloat(value);
        return isNaN(value) ? 0 : value;
    }

    function exchangeRate() {
        var rate = num($("#ExchangeRate").val());
        return rate > 0 ? rate : 1;
    }

    function taxRateFromSelect($select) {
        var value = $select.val();
        if (value === "1") return 0.10;
        if (value === "2") return 0.15;
        return 0;
    }

    function calculateItem($row) {
        var qty = num($row.find(".line-qty").val());
        var price = num($row.find(".line-price").val());
        var discount = num($row.find(".line-discount").val());
        var discountType = $row.find(".line-discount-type").val();
        var taxRate = taxRateFromSelect($row.find(".line-tax"));

        var gross = qty * price;

        if (discountType === "PERCENT") {
            discount = gross * discount / 100;
        }

        var net = Math.max(0, gross - discount);
        var tax = net * taxRate;
        var total = net + tax;

        $row.find(".line-total-usd").val(money(total));
        $row.find(".line-total-khr").val(money(total * exchangeRate()));

        return total;
    }

    function calculateAdditional($row) {
        var amount = num($row.find(".additional-amount").val());
        $row.find(".additional-khr").val(money(amount * exchangeRate()));
        return amount;
    }

    function calculateAll() {
        var itemTotal = 0;
        var additionalTotal = 0;

        $("#itemTable tbody .item-row").each(function () {
            itemTotal += calculateItem($(this));
        });

        $("#additionalTable tbody .additional-row").each(function () {
            additionalTotal += calculateAdditional($(this));
        });

        var base = itemTotal + additionalTotal;

        var discount = num($("#HeaderDiscount").val());
        var discountType = $("#HeaderDiscountType").val();

        if (discountType === "PERCENT") {
            discount = base * discount / 100;
        }

        var tax = num($("#HeaderTax").val());
        var taxType = $("#HeaderTaxType").val();

        if (taxType === "PERCENT") {
            tax = (base - discount) * tax / 100;
        }

        var grand = Math.max(0, base - discount + tax);

        $("#summaryItemTotal").text(money(itemTotal + additionalTotal));
        $("#summaryItemKhr").text(money((itemTotal + additionalTotal) * exchangeRate()));
        $("#summaryGrandUsd").text(money(grand));
        $("#summaryGrandKhr").text(money(grand * exchangeRate()));

        if (num($("#PaymentAmount").val()) === 0) {
            $("#PaymentAmount").val(grand.toFixed(2));
        }
    }

    function renumberItems() {
        $("#itemTable tbody .item-row").each(function (index) {
            $(this).find(".row-number").text(index + 1);
        });
    }

    function renumberAdditional() {
        $("#additionalTable tbody .additional-row").each(function (index) {
            $(this).find(".row-number").text(index + 1);
        });
    }

    function addItem() {
        var html = $("#itemRowTemplate").html();
        var $row = $(html);

        $row.attr("data-index", itemIndex++);
        $("#itemTable tbody").append($row);

        renumberItems();
        calculateAll();

        $row.find(".line-account").focus();
    }

    function addAdditional() {
        var html = $("#additionalRowTemplate").html();
        var $row = $(html);

        $row.attr("data-index", additionalIndex++);
        $("#additionalTable tbody").append($row);

        renumberAdditional();
        calculateAll();

        $row.find(".additional-account").focus();
    }

    function clearItems() {
        $("#itemTable tbody").empty();
        addItem();
    }

    function clearAdditional() {
        $("#additionalTable tbody").empty();
        addAdditional();
    }

    function resetForm() {
        $("#expenseForm")[0].reset();
        $("#Date").val(new Date().toISOString().substring(0, 10));
        $("#ExchangeRate").val("1.0000");
        $("#HeaderDiscount").val("0");
        $("#HeaderTax").val("0");
        $("#PaymentAmount").val("0");

        clearItems();
        clearAdditional();
        calculateAll();
    }

    function collectItems() {
        var rows = [];

        $("#itemTable tbody .item-row").each(function () {
            var $r = $(this);

            rows.push({
                AccountId: parseInt($r.find(".line-account").val() || "0"),
                ItemService: $r.find(".line-service").val() || "",
                Uom: $r.find(".line-uom").val() || "",
                Qty: num($r.find(".line-qty").val()),
                UnitPrice: num($r.find(".line-price").val()),
                Discount: num($r.find(".line-discount").val()),
                DiscountType: $r.find(".line-discount-type").val(),
                TaxId: parseInt($r.find(".line-tax").val() || "0")
            });
        });

        return rows;
    }

    function collectAdditional() {
        var rows = [];

        $("#additionalTable tbody .additional-row").each(function () {
            var $r = $(this);

            rows.push({
                AccountId: parseInt($r.find(".additional-account").val() || "0"),
                Description: $r.find(".additional-description").val() || "",
                Amount: num($r.find(".additional-amount").val())
            });
        });

        return rows;
    }

    function collectModel() {
        return {
            ClassId: parseInt($("#ClassId").val() || "0"),
            PaymentMethodId: parseInt($("#PaymentMethodId").val() || "0"),
            PaymentAccountId: parseInt($("#PaymentAccountId").val() || "0"),
            PayeeId: parseInt($("#PayeeId").val() || "0"),
            Date: $("#Date").val(),
            Terms: $("#Terms").val(),
            PaidOnDate: $("#PaidOnDate").val() || null,
            CheckedBy: $("#CheckedBy").val(),
            VerifiedBy: $("#VerifiedBy").val(),
            PostedBy: $("#PostedBy").val(),
            PoRfAdvNo: $("#PoRfAdvNo").val(),
            BillInvoiceNo: $("#BillInvoiceNo").val(),
            ExchangeRate: num($("#ExchangeRate").val()),
            Description: $("#Description").val(),
            HeaderDiscount: num($("#HeaderDiscount").val()),
            HeaderTax: num($("#HeaderTax").val()),
            Items: collectItems(),
            AdditionalExpenses: collectAdditional()
        };
    }

    function validateModel(model) {
        if (!model.ClassId) return "Please select Class Name.";
        if (!model.PaymentMethodId) return "Please select Payment Method.";
        if (!model.PaymentAccountId) return "Please select Payment Account.";
        if (!model.PayeeId) return "Please select Payee.";

        var meaningfulItems = model.Items.filter(function (x) {
            return x.AccountId || x.ItemService || x.Qty !== 0 || x.UnitPrice !== 0;
        });

        var meaningfulAdditional = model.AdditionalExpenses.filter(function (x) {
            return x.AccountId || x.Description || x.Amount !== 0;
        });

        if (meaningfulItems.length === 0 && meaningfulAdditional.length === 0) {
            return "Please add at least one expense line.";
        }

        for (var i = 0; i < meaningfulItems.length; i++) {
            if (!meaningfulItems[i].AccountId) {
                return "Please select an account for item row " + (i + 1) + ".";
            }
        }

        for (var j = 0; j < meaningfulAdditional.length; j++) {
            if (!meaningfulAdditional[j].AccountId) {
                return "Please select an account for additional expense row " + (j + 1) + ".";
            }
        }

        return null;
    }

    function saveExpense(saveAndNew) {
        var model = collectModel();
        var error = validateModel(model);

        if (error) {
            Swal.fire({
                icon: "warning",
                title: "Check Expense",
                text: error
            });
            return;
        }

        var token = $('input[name="__RequestVerificationToken"]').val();

        Swal.fire({
            title: "Saving...",
            text: "Please wait",
            allowOutsideClick: false,
            didOpen: function () {
                Swal.showLoading();
            }
        });

        $.ajax({
            url: $("#expenseForm").attr("action"),
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(model),
            headers: {
                "RequestVerificationToken": token
            }
        })
        .done(function (response) {
            if (!response || !response.success) {
                Swal.fire({
                    icon: "error",
                    title: "Save Failed",
                    text: response && response.message
                        ? response.message
                        : "Unable to save expense."
                });
                return;
            }

            Swal.fire({
                icon: "success",
                title: "Saved",
                text: response.message || "Expense saved successfully.",
                timer: 1300,
                showConfirmButton: false
            }).then(function () {
                if (saveAndNew) {
                    resetForm();
                } else {
                    window.location.href = '@Url.Action("Index", "ExpenseTransactions")';
                }
            });
        })
        .fail(function (xhr) {
            var message = "Unexpected server error.";

            if (xhr.responseJSON && xhr.responseJSON.message) {
                message = xhr.responseJSON.message;
            }

            Swal.fire({
                icon: "error",
                title: "Server Error",
                text: message
            });
        });
    }

    function loadInitialRows() {
        addItem();
        addItem();
        addAdditional();
    }

    $(document).on("input change", "#ExchangeRate, #HeaderDiscount, #HeaderDiscountType, #HeaderTax, #HeaderTaxType, .line-qty, .line-price, .line-discount, .line-discount-type, .line-tax, .additional-amount", function () {
        calculateAll();
    });

    $(document).on("click", ".remove-row", function () {
        $(this).closest("tr").remove();

        if ($("#itemTable tbody .item-row").length === 0) {
            addItem();
        }

        renumberItems();
        calculateAll();
    });

    $(document).on("click", ".copy-row", function () {
        var $original = $(this).closest("tr");
        var $copy = $original.clone();

        $copy.attr("data-index", itemIndex++);
        $copy.find(".line-total-usd").val("0.00");
        $copy.find(".line-total-khr").val("0.00");

        $("#itemTable tbody").append($copy);

        renumberItems();
        calculateAll();
    });

    $(document).on("click", ".remove-additional", function () {
        $(this).closest("tr").remove();

        if ($("#additionalTable tbody .additional-row").length === 0) {
            addAdditional();
        }

        renumberAdditional();
        calculateAll();
    });

    $(document).on("click", ".copy-additional", function () {
        var $original = $(this).closest("tr");
        var $copy = $original.clone();

        $copy.attr("data-index", additionalIndex++);
        $("#additionalTable tbody").append($copy);

        renumberAdditional();
        calculateAll();
    });

    $("#btnAddItem").on("click", addItem);
    $("#btnAddAdditional").on("click", addAdditional);
    $("#btnClearItems").on("click", clearItems);
    $("#btnClearAdditional").on("click", clearAdditional);

    $("#btnReset").on("click", function () {
        Swal.fire({
            icon: "question",
            title: "Reset Expense?",
            text: "All entered data will be cleared.",
            showCancelButton: true,
            confirmButtonText: "Yes, reset",
            cancelButtonText: "Cancel"
        }).then(function (result) {
            if (result.isConfirmed) {
                resetForm();
            }
        });
    });

    $("#btnSave").on("click", function () {
        saveExpense(false);
    });

    $("#btnSaveNew").on("click", function () {
        saveExpense(true);
    });

    $("#btnClose").on("click", function () {
        if (window.history.length > 1) {
            window.history.back();
        } else {
            window.close();
        }
    });

    $(".tab").on("click", function () {
        var tab = $(this).data("tab");

        $(".tab").removeClass("active");
        $(this).addClass("active");

        $(".tab-content").removeClass("active");
        $("#tab-" + tab).addClass("active");
    });

    $("#AttachmentFiles").on("change", function () {
        var files = this.files;
        var $list = $("#attachmentList");

        $list.empty();

        for (var i = 0; i < files.length; i++) {
            var file = files[i];

            var $chip = $("<div/>", {
                "class": "attachment-chip"
            }).html(
                '<i class="bi bi-paperclip"></i> ' +
                $("<div>").text(file.name).html()
            );

            $list.append($chip);
        }
    });

    $(function () {
        loadInitialRows();
        calculateAll();
    });

})(jQuery);
