"use strict";

//Get base color
setTimeout(function () {
  const baseBGColor = $("aside nav ul > li.nav-item > .nav-link.active")
    .first()
    .css("background-color");
  const baseColor = $("aside nav ul > li.nav-item > .nav-link.active")
    .first()
    .css("color");
  localStorage.setItem("BaseSystemBGColor", baseBGColor);
  localStorage.setItem("BaseSystemColor", baseColor);
  $(":root").css({
    "--base-bg-color": baseBGColor,
    "--base-color": baseColor,
  });
}, 3000);

// Format date dynamically
const convertFormatDate = (dateString, format) => {
  let date;

  // Check if the date is in "DD/MM/YYYY" format and convert it
  if (/^\d{2}\/\d{2}\/\d{4}$/.test(dateString)) {
    const [day, month, year] = dateString.split("/").map(Number);
    date = new Date(year, month - 1, day);
  } else {
    date = new Date(dateString);
  }
  if (isNaN(date)) return dateString; // Return original if invalid

  const monthsShort = [
    "Jan",
    "Feb",
    "Mar",
    "Apr",
    "May",
    "Jun",
    "Jul",
    "Aug",
    "Sep",
    "Oct",
    "Nov",
    "Dec",
  ];
  const monthsFull = [
    "January",
    "February",
    "March",
    "April",
    "May",
    "June",
    "July",
    "August",
    "September",
    "October",
    "November",
    "December",
  ];

  const day = String(date.getDate()).padStart(2, "0");
  const monthShort = monthsShort[date.getMonth()];
  const monthFull = monthsFull[date.getMonth()];
  const monthNumeric = String(date.getMonth() + 1).padStart(2, "0");
  const year = date.getFullYear();
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  const seconds = String(date.getSeconds()).padStart(2, "0");

  return format
    .replace("dd", day)
    .replace("MMM", monthShort)
    .replace("MMMM", monthFull)
    .replace("MM", monthNumeric)
    .replace("yyyy", year)
    .replace("HH", hours)
    .replace("mm", minutes)
    .replace("ss", seconds);
};

// Random barcode generator (digitLen: number of digits, e.g, digitLeng = 10 (0123456789))
const generateBarcode = (digitLen) => {
  let rnd = new Date().getTime();
  for (let i = 0; i <= 10; i++) {
    rnd += parseInt(rnd) * parseInt(Math.random() * 100 + i);
  }
  let result = rnd.toString();
  result = result.substring(0, digitLen).replace(".", "9");
  return result;
};

// Convert column index to Excel letter (A, B, C, ..., Z, AA, AB, etc.)
const getExcelColumnLetter = (index) => {
  let letter = "";
  while (index >= 0) {
    letter = String.fromCharCode((index % 26) + 65) + letter;
    index = Math.floor(index / 26) - 1;
  }
  return letter;
};

// Get company info and date time
const getExcelTitleExport = () => {
  try {
    let companyInfo = [];
    const res = LoadData(`UserCompany/GetSystemCompanyInfo`, function (res) {
      companyInfo = JSON.parse(JSON.parse(res).data);
    });

    if (!companyInfo.length) {
      throw new Error("No company information found.");
    }

    const name = companyInfo[0].CompanyName;
    const nameKh = companyInfo[0].CompanyNameKH;
    const symbol = "~~~~~~~~~~~~~";

    const t = localStorage.getItem("custom-excel-title");
    const reportLinks = $(".breadcrumb").find("li:nth-child(3) span").text();

    const reportTitle =
      t && t.trim() ? t : reportLinks.split(/(?=[A-Z])/).join(" ");

    if (t && t.trim()) {
      localStorage.removeItem("custom-excel-title");
    }

    const title = `${name}\r\n${symbol}\r\n${reportTitle}`;

    const printDate = `Print Date: ${convertFormatDate(
      new Date(),
      "dd-MMM-yyyy HH:mm:ss",
    )}`;

    let dateRange = $("form[name='Frm_Search'] input:first").val();
    if (dateRange) {
      dateRange = dateRange
        .split("-")
        .map((date) => convertFormatDate(date.trim(), "dd-MMM-yyyy"));
    }

    return {
      title: { nameKh: nameKh, subTitle: title },
      printDate: printDate,
      dateRange: dateRange || [],
    };
  } catch (error) {
    console.error("Error fetching company info:", error);
    return {
      title: { nameKh: "Unknown", subTitle: "Unknown" },
      printDate: `Print Date: ${convertFormatDate(
        new Date(),
        "dd-MMM-yyyy HH:mm:ss",
      )}`,
      dateRange: [],
    };
  }
};

//Immediately Invoked Function Expression (IIFE)
const LoadCustomExpireDateAutoComplete = (el, wid) => {
  let debounceTimer;

  $(el)
    .autocomplete({
      minLength: 0,
      source: function (request, response) {
        const $row = $(el).closest("tr");
        const itemId = $row.find('input[name="ItemID"]').val();
        const warehouseId = wid;
        if (!itemId || !warehouseId) return;

        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
          fetchExpireDates(itemId, warehouseId, request.term, response);
        }, 100);
      },
      select: function (event, ui) {
        const $row = $(el).closest("tr");
        $row.find('input[name="ExpireDate"]').val(ui.item.value);
        $row.find('input[name="QOH"]').val(ui.item.qty).trigger("change");
        return false;
      },
    })
    .on("click", function () {
      $(this).autocomplete("search", $(this).val());
    })
    .autocomplete("instance")._renderItem = function (ul, item) {
    return $("<li>")
      .append(
        `<div>${item.label} <span class="text-red">(qoh: ${item.qty})</span></div>`,
      )
      .appendTo(ul);
  };
};

const fetchExpireDates = async (itemId, warehouseId, searchTerm, callback) => {
  try {
    const response = await $.ajax({
      url: `${ServerRoot}InventoryExpirationDate/getStockExpireDate`,
      type: "GET",
      headers: { Authorization: `Bearer ${access_Token}` },
      data: {
        ItemID: itemId,
        LocationID: warehouseId,
        SearchTerm: searchTerm,
      },
    });

    const items = $.map(JSON.parse(response.data), (s) => ({
      label: s.StockExpireDate,
      value: s.StockExpireDate,
      qty: s.QOH,
    }));

    callback(items);
  } catch (error) {
    console.error("Error fetching expiration dates:", error);
    callback([]);
  }
};

const applyRequireToExpireDate = (wid) => {
  $('input[name="ExpireDate"]').each(function () {
    const expireDateInput = $(this);
    let tr = expireDateInput.closest("tr");

    // Check if ItemStatus is "NonOtherCharge" for DN
    const itemStatus = tr.find('input[name="ItemStatus"]').val();
    if (itemStatus && itemStatus !== "NonOtherCharge") return;

    const itemId = tr.find('input[name="ItemID"]').val();
    const warehouseId = wid;
    if (itemId && warehouseId) {
      $.ajax({
        url: ServerRoot + "InventoryExpirationDate/getStockExpireDate",
        type: "GET",
        headers: { Authorization: "Bearer " + access_Token },
        data: {
          ItemID: itemId,
          LocationID: warehouseId,
          SearchTerm: "",
        },
        success: function (data) {
          const res = JSON.parse(data.data);
          if (res.length > 0) {
            expireDateInput.parent().addClass("validate-input");
            expireDateInput
              .parent()
              .attr("data-validate", "Expire Date is required");
          }
        },
      });
    }
  });
};

//reusable class for total calculation
class TotalCalculator {
  constructor({
    qty = 0,
    unitPrice = 0,
    unitCost = 0,
    discountUSD = 0,
    discountKHR = 0,
    discountPercent = 0,
    vatRate = 0,
    deposit = 0,
    exchangeRate = 4100,
  } = {}) {
    this.qty = qty;
    this.unitPrice = unitPrice;
    this.unitCost = unitCost;
    this.discountUSD = discountUSD;
    this.discountKHR = discountKHR;
    this.discountPercent = discountPercent;
    this.vatRate = vatRate / 100;
    this.deposit = deposit;
    this.exchangeRate = exchangeRate;
  }

  calculate() {
    // Subtotal
    const subtotal = this.qty * this.unitPrice;

    // Discounts
    const discountFromPercent = (this.discountPercent / 100) * subtotal;
    const totalDiscount =
      discountFromPercent +
      this.discountUSD +
      this.discountKHR / this.exchangeRate;

    // After discount
    const afterDiscount = subtotal - totalDiscount;

    // Total
    const total = afterDiscount;

    // VAT
    const amtVAT = afterDiscount * this.vatRate;

    // Grand total
    const grandTotal = total + amtVAT;

    // Balances
    const balanceUSD = grandTotal - this.deposit;
    const balanceKHR = balanceUSD * this.exchangeRate;

    return {
      subtotal: Number(subtotal.toFixed(4)),
      totalDiscount: Number(totalDiscount.toFixed(4)),
      amtVAT: Number(amtVAT.toFixed(4)),
      total: Number(total.toFixed(4)),
      grandTotal: Number(grandTotal.toFixed(4)),
      balanceUSD: Number(balanceUSD.toFixed(4)),
      balanceKHR: Math.round(balanceKHR),
    };
  }
}

document.addEventListener("click", function (e) {
  return;
  const btn = e.target.closest(".btn");
  if (!btn) return;

  const rect = btn.getBoundingClientRect();
  const ripple = document.createElement("span");
  ripple.className = "ripple-el";

  const size = Math.max(rect.width, rect.height) * 2.2;
  ripple.style.width = ripple.style.height = size + "px";

  const x = e.clientX - rect.left - size / 2;
  const y = e.clientY - rect.top - size / 2;

  ripple.style.left = x + "px";
  ripple.style.top = y + "px";

  btn.appendChild(ripple);

  ripple.addEventListener("animationend", () => ripple.remove(), {
    once: true,
  });
});

function destroyKendoWidgets($container) {
  try {
    $container.find("[data-role]").each(function () {
      const widget =
        $(this).data("kendoGrid") ||
        $(this).data("kendoDropDownList") ||
        $(this).data("kendoDatePicker") ||
        $(this).data("kendoComboBox") ||
        $(this).data("kendoNumericTextBox");

      if (widget && widget.destroy) widget.destroy();
    });

    $container.empty();
  } catch (e) {
    console.warn("Kendo destroy error:", e);
  }
}

function EditCustomer(id, done) {
  if (!id) return;

  // Ensure App exists and reset previous data
  window.App = window.App || {};
  window.App.CustomerInfo = null;

  LoadData(
    `AddCustomer/GetCustomerBilling?CustomerID=${id}`,
    function (response) {
      const data = JSON.parse(response.data);

      if (!data || Object.keys(data).length === 0) {
        DialogSweetAlert("error", "Customer Not Found");
        return;
      }

      window.App.CustomerInfo = data;

      if (typeof done === "function") {
        done(data);
      }
    },
    true,
    "json",
  );
}

function EditCustomerWithRefNumber(
  id,
  refNum,
  formType,
  isSelectedRef = false,
  callBackFn,
  isRenew = false,
  isChangePlan = false,
) {
  if (!id) return;

  window.App = window.App || {};
  window.App.CustomerInfo = null;

  !isSelectedRef &&
    ShowDialogBox(
      "Edit",
      "Auto",
      600,
      `AddCustomer/CreateCustomerV2?FormType=${formType}&IsRenew=${isRenew}&IsChangePlan=${isChangePlan}`,
      id,
    );
  LoadData(
    `AddCustomer/GetCustomerBilling?CustomerID=${id}&InvoiceNo=${refNum}&IsRenew=${isRenew}&IsChangePlan=${isChangePlan}`,
    function (response) {
      const data = JSON.parse(response.data);

      if (!data || Object.keys(data).length === 0) {
        DialogSweetAlert("error", "Customer Not Found");
        return;
      }
      window.App.CustomerInfo = data;

      if (typeof callBackFn === "function") {
        callBackFn(data);
      }
    },
    true,
    "json",
  );
}

function ToInvoiceDialog(customerId, refNum) {
  const formType = "Invoice";
  ShowDialogBox(
    `Quotation To Invoice Ref.: <span class="text-danger">${refNum}</span>`,
    "Auto",
    600,
    `AddCustomer/CreateCustomerV2?FormType=${formType}&IsToInvoice=true`,
    customerId,
    "",
    "",
    "",
    "",
    function () {
      $("#ReferenceNo")
        .off("typeahead:select")
        .on("typeahead:select", function () {
          const refNo = $(this).val();
          const custId = customerId;
          $("#CustomerName_CustomerID").val(custId);
          EditCustomerWithRefNumber(custId, refNo, formType, true, function () {
            const customerInfo = window.App.CustomerInfo;
            const tables = [
              "Table",
              "Table1",
              "Table2",
              "Table3",
              "Table4",
              "Table5",
              "Table6",
              "Table7",
              "Table8",
            ];

            tables.forEach((tableName) => {
              const data = customerInfo?.[tableName];
              if (!data?.length) return;
              $.each(data, function (index, val) {
                if ("InvoiceNo" in val) delete val.InvoiceNo;
                if ("InvoiceDate" in val) delete val.InvoiceDate;
                if ("PaymentDueDate" in val) delete val.PaymentDueDate;
              });
            });
          });
        })
        .val(refNum)
        .typeahead("val", refNum)
        .trigger("typeahead:select"); // fire after binding
    },
  );
}

function RenewDialog(customerId, refNum) {
  const formType = "Invoice";
  ShowDialogBox(
    `Renew Invoice Ref.: <span class="text-danger">${refNum}</span>`,
    "Auto",
    600,
    `AddCustomer/CreateCustomerV2?FormType=${formType}&IsRenew=true`,
    customerId,
    "",
    "",
    "",
    "",
    function () {
      $("#ReferenceNo")
        .off("typeahead:select")
        .on("typeahead:select", function () {
          const refNo = $(this).val();
          const custId = customerId;
          $("#CustomerName_CustomerID").val(custId);
          EditCustomerWithRefNumber(
            custId,
            refNo,
            formType,
            true,
            function () {
              const customerInfo = window.App.CustomerInfo;
              const tables = [
                "Table",
                "Table1",
                "Table2",
                "Table3",
                "Table4",
                "Table5",
                "Table6",
                "Table7",
                "Table8",
              ];

              tables.forEach((tableName) => {
                const data = customerInfo?.[tableName];
                if (!data?.length) return;
                $.each(data, function (index, val) {
                  if ("ReferenceNo" in val) delete val.ReferenceNo;
                  if ("InvoiceNo" in val) delete val.InvoiceNo;
                  if ("InvoiceDate" in val) delete val.InvoiceDate;
                  if ("PaymentDueDate" in val) delete val.PaymentDueDate;
                  if ("StartDate" in val) delete val.StartDate;
                  if ("EndDate" in val) delete val.EndDate;
                });
              });
            },
            true,
          );
        })
        .val(refNum)
        .prop("readonly", true)
        .typeahead("val", refNum)
        .trigger("typeahead:select"); // fire after binding
    },
  );
}

function ChangePlanDialog(customerId, refNum) {
  const formType = "Invoice";
  ShowDialogBox(
    `Change Plan`,
    "Auto",
    600,
    `AddCustomer/CreateCustomerV2?FormType=${formType}&IsChangePlan=true`,
    customerId,
    "",
    "",
    "",
    "",
    function () {
      $("#ReferenceNo")
        .off("typeahead:select")
        .on("typeahead:select", function () {
          const refNo = $(this).val();
          const custId = customerId;
          $("#CustomerName_CustomerID").val(custId);
          EditCustomerWithRefNumber(
            custId,
            refNo,
            formType,
            true,
            function () {
              const customerInfo = window.App.CustomerInfo;
              const tables = [
                "Table",
                "Table1",
                "Table2",
                "Table3",
                "Table4",
                "Table5",
                "Table6",
                "Table7",
                "Table8",
              ];

              tables.forEach((tableName) => {
                const data = customerInfo?.[tableName];
                if (!data?.length) return;
                $.each(data, function (index, val) {
                  if ("ReferenceNo" in val) delete val.ReferenceNo;
                  if ("InvoiceNo" in val) delete val.InvoiceNo;
                  if ("InvoiceDate" in val) delete val.InvoiceDate;
                  if ("PaymentDueDate" in val) delete val.PaymentDueDate;
                  //if ('StartDate' in val) delete val.StartDate;
                  //if ('EndDate' in val) delete val.EndDate;
                });
              });
            },
            false,
            true,
          );
        })
        .val(refNum)
        .prop("readonly", true)
        .typeahead("val", refNum)
        .trigger("typeahead:select"); // fire after binding
    },
  );
}

document.addEventListener("click", function (e) {
  const tab = e.target.closest(".tabs-showcase .tab-item");
  if (!tab) return; // Not a tab click

  const tabs = document.querySelectorAll(".tabs-showcase .tab-item");
  const panes = document.querySelectorAll(".tabs-showcase .tab-pane");

  const index = tab.getAttribute("data-tab-index");

  // Remove active from all tabs
  tabs.forEach((t) => t.classList.remove("active"));

  // Remove active from all panes
  panes.forEach((p) => p.classList.remove("active"));

  // Activate current tab
  tab.classList.add("active");

  // Activate corresponding pane
  if (panes[index]) {
    panes[index].classList.add("active");
  }
});

function fixLightboxImages() {
  $('[data-toggle="lightbox"]').each(function () {
    var $el = $(this);
    var href = $el.attr("href");
    if (!href || $el.data("checked")) return;

    var img = new Image();
    img.onload = function () {
      $el.data("checked", true);
    };
    img.onerror = function () {
      $el.attr("href", "/Content/images/placeholder.jpg").data("checked", true);
    };
    img.src = href;
  });
}

$(document).on("click", '[data-toggle="lightbox"]', function (event) {
  event.preventDefault();
  var $el = $(this);
  var href = $el.attr("href");

  function updateOverlay(lightboxInstance) {
    $(".ekko-lightbox-overlay-caption").remove();

    // this.element() returns the <a> tag currently being displayed,
    // correct both on first open AND after next/prev navigation
    var $current = lightboxInstance.element();
    var currentTitle =
      $current.attr("data-title-overlay") || $current.data("title-overlay");

    console.log();

    if (currentTitle) {
      $(".ekko-lightbox-container").append(
        '<div class="ekko-lightbox-overlay-caption">' +
          "<span>" +
          currentTitle +
          "</span>" +
          "</div>",
      );
    }
  }

  function openLightbox() {
    $el.ekkoLightbox({
      wrapping: false,
      showArrows: false,
      onShown: function () {
        updateOverlay(this);
      },
    });
  }

  var img = new Image();
  img.onload = openLightbox;
  img.onerror = function () {
    $el.attr("href", "/Content/images/placeholder.jpg");
    openLightbox();
  };
  img.src = href;
});

$(document).on({
  "shown.lte.pushmenu": () => {
    $("#menuIcon").attr("class", "ti ti-chevrons-left");
    $(".brand-image")[0].style.removeProperty("width");
    $(".brand-image").css({
      height: "",
      "object-fit": "",
    });
  },
  "collapsed.lte.pushmenu": () => {
    $("#menuIcon").attr("class", "ti ti-chevrons-right");
    $(".brand-image")[0].style.setProperty("width", "34px", "important");
    $(".brand-image").css({
      height: "34px",
      "object-fit": "contain",
    });
  },
});
$(document).on("keydown", function (e) {
  // Check for Ctrl + B (or Cmd + B for Mac)
  if ((e.ctrlKey || e.metaKey) && e.which === 66) {
    e.preventDefault(); // Stop browser default behavior

    // Trigger the AdminLTE pushMenu toggle programmatically
    // Note: '[data-widget="pushmenu"]' is the standard AdminLTE selector
    $('[data-widget="pushmenu"]').PushMenu("toggle");
  }
});

$(document).ready(function () {
  // Apply saved theme on load
  var savedTheme = localStorage.getItem("darkTheme") || "midnight";
  document.body.setAttribute("data-theme", savedTheme);
  $('.theme-option[data-theme="' + savedTheme + '"]').addClass("active");

  // Handle theme selection
  $(".theme-option").on("click", function () {
    var theme = $(this).data("theme");
    document.body.setAttribute("data-theme", theme);
    localStorage.setItem("darkTheme", theme);

    $(".theme-option").removeClass("active");
    $(this).addClass("active");
  });

  function applyTheme(theme) {
    const $body = $("body");
    const $navbar = $(".main-header");
    const $sidebar = $(".main-sidebar");
    const $icon = $("#darkModeIcon");

    const isDarkMode = theme === "dark";
    $body.toggleClass("dark-mode", isDarkMode);
    $navbar
      .toggleClass("navbar-dark", isDarkMode)
      .toggleClass("navbar-white", !isDarkMode);
    if ($icon.length) {
      $icon
        .removeClass("ti-sun ti-moon")
        .addClass(isDarkMode ? "ti-sun" : "ti-moon");
    }
    localStorage.setItem("theme", theme);
  }
  function toggleDarkMode() {
    const theme = $("body").hasClass("dark-mode") ? "light" : "dark";
    if (theme === "dark") $(".theme-picker").removeClass("d-none");
    else $(".theme-picker").addClass("d-none");
    applyTheme(theme);
  }
});

function ShowPOInvoice(VendorID, POID, PONumber) {
  var URL =
    "Profile/InvoiceTMP?VendorID=" + VendorID + "&POID=" + POID + "&Type=PO";
  ShowDialogBox("Preview PO No.: " + PONumber, "Auto", "500", URL);
}

function showSOForm(salesId, soNumber, saleType) {
  var URL = "Profile/InvoiceSaleOrderTMP?ID=" + salesId + "&Type=" + saleType;
  ShowDialogBox(
    "Preview Sales Order Form No.: " + soNumber,
    "Auto",
    "500",
    URL,
  );
}

(function () {
  function injectArrow(el) {
    if (el.querySelector("i.ti")) return;
    el.innerHTML = '<i class="ti ti-chevron-down"></i>';
  }
  document.querySelectorAll(".select2-selection__arrow").forEach(injectArrow);

  var observer = new MutationObserver(function (mutations) {
    mutations.forEach(function (m) {
      m.addedNodes.forEach(function (node) {
        if (node.nodeType !== 1) return;
        if (node.matches && node.matches(".select2-selection__arrow"))
          injectArrow(node);
        node.querySelectorAll &&
          node
            .querySelectorAll(".select2-selection__arrow")
            .forEach(injectArrow);
      });
    });
  });
  observer.observe(document.body, { childList: true, subtree: true });
})();
