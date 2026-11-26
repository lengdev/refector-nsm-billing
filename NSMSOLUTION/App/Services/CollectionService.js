(function () {
  "use strict";

  app.factory("CollectionService", [
    "$http",
    "$q",
    function ($http, $q) {
      var BASE = "/CollectionApi/";
      var prefix = "RV-";
      var nextFromServer = null;

      function unwrap(res) {
        return res.data && res.data.data;
      }
      function fail(err) {
        var msg =
          err && err.data && err.data.message
            ? err.data.message
            : "Request failed. Please try again.";
        var status = err && err.status;
        var url = err && err.config && err.config.url;
        if (window.console) {
          console.error("[CollectionService]", status, url, err && err.data);
        }
        if (!(err && err.data && err.data.message)) {
          msg += " (HTTP " + status + " " + url + ")";
        }
        return $q.reject(new Error(msg));
      }

      return {
        getLookups: function () {
          return $http
            .get(BASE + "Lookups", { ignoreLoadingBar: true })
            .then(function (res) {
              var d = unwrap(res);
              nextFromServer = d.nextNumber;
              return d;
            }, fail);
        },

        getCustomers: function (search, take) {
          return $http
            .get(BASE + "Customers", {
              params: {
                search: search || "",
                take: take || 30,
              },
              ignoreLoadingBar: true,
            })
            .then(unwrap, fail);
        },

        getOpenInvoices: function (customerId, dateIso, collectionId) {
          return $http
            .get(BASE + "OpenInvoices", {
              params: {
                customerId: customerId,
                date: dateIso || null,
                collectionId: collectionId || null,
              },
              ignoreLoadingBar: true,
            })
            .then(unwrap, fail);
        },

        getCredits: function (customerId, dateIso, collectionId) {
          return $http
            .get(BASE + "GetCredits", {
              params: {
                customerId: customerId,
                date: dateIso || null,
                collectionId: collectionId || null,
              },
              ignoreLoadingBar: true,
            })
            .then(unwrap, fail);
        },

        getList: function () {
          return $http
            .get(BASE + "List", { ignoreLoadingBar: true })
            .then(unwrap, fail);
        },

        getDetails: function (id) {
          return $http
            .get(BASE + "Details", {
              params: { id: id },
              ignoreLoadingBar: true,
            })
            .then(unwrap, fail);
        },

        addCustomer: function (c) {
          return $http.post(BASE + "AddCustomer", c).then(unwrap, fail);
        },

        formatNumber: function (seq) {
          if (nextFromServer) {
            var n = parseInt(nextFromServer.slice(prefix.length), 10) || 1;
            return prefix + String(n + seq - 1).padStart(6, "0");
          }
          return prefix + String(seq).padStart(6, "0");
        },

        getNextNumber: function (classId) {
          return $http
            .get("/CollectionApi/NextNumber", {
              params: { classId: classId },
            })
            .then(function (res) {
              if (!res.data || !res.data.success) {
                throw new Error(
                  (res.data && res.data.message) ||
                    "Could not get collection number",
                );
              }

              return res.data.data.nextNumber;
            });
        },

        save: function (rec, slips) {
          var body = new FormData();
          body.append("data", angular.toJson(rec.req));
          (slips || []).forEach(function (s) {
            body.append("slips", s.file, s.name);
          });

          return $http
            .post(BASE + "Save", body, {
              transformRequest: angular.identity,
              headers: { "Content-Type": undefined },
            })
            .then(function (res) {
              var d = unwrap(res);
              var saved = angular.extend({}, rec, {
                id: d.id,
                no: d.number,
                unapplied: d.unapplied,
                notified: d.notified,
                notifyMessage: d.notifyMessage,
              });
              delete saved.req;
              return saved;
            }, fail);
        },

        update: function (id, rec, slips) {
          var body = new FormData();

          body.append("id", id);
          body.append("data", angular.toJson(rec.req));

          (slips || []).forEach(function (s) {
            body.append("slips", s.file, s.name);
          });

          return $http
            .post(BASE + "Update", body, {
              transformRequest: angular.identity,
              headers: { "Content-Type": undefined },
            })
            .then(function (res) {
              var d = unwrap(res);

              var saved = angular.extend({}, rec, {
                id: d.id,
                no: d.number,
                unapplied: d.unapplied,
                notified: d.notified,
                notifyMessage: d.notifyMessage,
              });

              delete saved.req;
              return saved;
            }, fail);
        },

        rollbackCollection: function (id) {
          return $http.post(BASE + "Rollback", { id: id }).then(unwrap, fail);
        },
      };
    },
  ]);
})();
