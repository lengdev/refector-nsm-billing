// Categories that are listed in the sidebar but have no reports yet.
// When you add reports, move the category into its own <Name>ReportService.js.
(function () {
  "use strict";

  app.run([
    "ReportRegistry",
    function (Registry) {
      Registry.add({ order: 50, category: "Network & Router" });
      Registry.add({ order: 60, category: "Service & Package" });
    },
  ]);
})();
