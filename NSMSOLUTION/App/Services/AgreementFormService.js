"use strict";

app.factory("AgreementFormService", [
    "$http",
    "$q",
    function ($http, $q) {
        /**
         * Fetches HTML template and prints the Internet Service Form / Agreement.
         * 
         * @param {Object} profile - Customer profile object
         * @param {Object} connection - Selected connection object
         * @param {Object} billInfo - Billing information matching connection
         */
        function printAgreement(profile, connection, billInfo) {
            var p = profile || {};
            var conn = connection || {};
            var bill = billInfo || {};

            // Determine connection type (GPON / FTTH / Other)
            var svcName = (conn.ServiceName || conn.PackageName || "").toUpperCase();
            var isGPON = svcName.indexOf("GPON") !== -1;
            var isFTTH = svcName.indexOf("FTTH") !== -1;

            // Permanent address parts
            var permAddr = p.CustomerAddress || "";
            var permCommune = p.CustomerCommune || "";
            var permDistrict = p.CustomerDistrict || "";
            var permProv = p.CustomerProvinceCity || "";

            // Billing address
            var billAddr = bill.BillToAddress || "";
            var billCommune = bill.BillToCommune || "";
            var billDistrict = bill.BillToDistrict || "";
            var billProv = bill.BillToProvinceCity || "";

            // Install address
            var instAddr = bill.InstallAddress || "";
            var instCommune = bill.InstallCommune || "";
            var instDistrict = bill.InstallDistrict || "";
            var instProv = bill.InstallProvinceCity || "";

            var lat = bill.GoogleMap_Latitude || "";
            var lng = bill.GoogleMap_Longitude || "";
            var mapLatLng = (lat && lng) ? lat + ", " + lng : (bill.GoogleMap_FullAddress || "");

            var packageName = conn.PackageName ? (conn.PackageName + (conn.Bandwidth ? " - " + conn.Bandwidth + " Mbps" : "") + (conn.UM ? " - " + conn.UM : "")) : "";
            var deposit = conn.DepositFee ? "$" + parseFloat(conn.DepositFee).toFixed(2) : "";
            var monthlyCharge = (conn.GrandTotal || conn.MonthlyCharge) ? "$" + parseFloat(conn.GrandTotal || conn.MonthlyCharge).toFixed(2) : "";
            var logoUrl = window.location.origin + "/Content/NSM-ISPLogo.png";

            var templateUrl = window.location.origin + "/Templates/AgreementForm.html";

            return $http.get(templateUrl).then(function (response) {
                var html = response.data;

                // Replace placeholders
                html = html
                    .replace(/{{logoUrl}}/g, logoUrl)
                    .replace(/{{isGPON}}/g, isGPON ? "✓" : "")
                    .replace(/{{isFTTH}}/g, isFTTH ? "✓" : "")
                    .replace(/{{customerName}}/g, (p.CustomerName || p.CustomerNameLatin || "").toUpperCase())
                    .replace(/{{gender}}/g, p.CustomerGender || "")
                    .replace(/{{companyContactName}}/g, bill.CompanyContactName || "")
                    .replace(/{{companyContactPosition}}/g, bill.CompanyContactPosition || "")
                    .replace(/{{identification}}/g, p.CustomerIdentification || "")
                    .replace(/{{vatNumber}}/g, bill.VATNumber || "")
                    .replace(/{{permAddr}}/g, permAddr)
                    .replace(/{{permCommune}}/g, permCommune)
                    .replace(/{{permDistrict}}/g, permDistrict)
                    .replace(/{{permProv}}/g, permProv)
                    .replace(/{{billAddr}}/g, billAddr)
                    .replace(/{{billCommune}}/g, billCommune)
                    .replace(/{{billDistrict}}/g, billDistrict)
                    .replace(/{{billProv}}/g, billProv)
                    .replace(/{{customerMobile}}/g, p.CustomerMobile || "")
                    .replace(/{{companyPhone}}/g, bill.CompanyMobile || bill.CompanyDeskPhone || "")
                    .replace(/{{companyContactMobile}}/g, bill.CompanyContactMobileNumber || "")
                    .replace(/{{email}}/g, p.CustomerEmail || bill.CompanyEmail || "")
                    .replace(/{{packageName}}/g, packageName)
                    .replace(/{{mapLatLng}}/g, mapLatLng)
                    .replace(/{{deposit}}/g, deposit)
                    .replace(/{{monthlyCharge}}/g, monthlyCharge)
                    .replace(/{{instAddr}}/g, instAddr)
                    .replace(/{{instCommune}}/g, instCommune)
                    .replace(/{{instDistrict}}/g, instDistrict)
                    .replace(/{{instProv}}/g, instProv);

                var win = window.open("", "_blank");
                if (!win) {
                    if (typeof DialogSweetAlert !== "undefined")
                        DialogSweetAlert("nopermission", "Please allow popups for this website.");
                    else alert("Please allow popups for this website.");
                    return;
                }
                win.document.write(html);
                win.document.close();
            }).catch(function (err) {
                console.error("Failed to load AgreementForm template", err);
            });
        }

        return {
            printAgreement: printAgreement
        };
    }
]);
