app.service("NotificationService", [
  "$http",
  function ($http) {
    var nearlyExpiredPromise = null;
    var CACHE_KEY = "nearlyExpiredCustomers";

    this.getNearlyExpiredCustomers = function (forceRefresh) {
      // Return cached data unless force refresh is requested.
      if (!forceRefresh) {
        if (nearlyExpiredPromise) {
          return nearlyExpiredPromise;
        }

        var cachedData = localStorage.getItem(CACHE_KEY);

        if (cachedData) {
          try {
            return (nearlyExpiredPromise = Promise.resolve(
              JSON.parse(cachedData),
            ));
          } catch (e) {
            localStorage.removeItem(CACHE_KEY);
          }
        }
      }

      var apiBaseUrl = "api/Notification/GetCustomerNearlyExpire";

      nearlyExpiredPromise = $http
        .get(apiBaseUrl, { ignoreLoadingBar: true })
        .then(function (response) {
          var data = response.data;

          // Save the successful response to localStorage.
          localStorage.setItem(CACHE_KEY, JSON.stringify(data));

          return data;
        })
        .catch(function (error) {
          // Allow retry if the request fails.
          nearlyExpiredPromise = null;
          return Promise.reject(error);
        });

      return nearlyExpiredPromise;
    };

    // Clear the cache when you need fresh data next time.
    this.clearNearlyExpiredCache = function () {
      localStorage.removeItem(CACHE_KEY);
      nearlyExpiredPromise = null;
    };
  },
]);
