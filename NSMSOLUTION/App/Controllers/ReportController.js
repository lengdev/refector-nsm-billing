(function () {
  "use strict";

  app.controller("ReportController", [
    "$scope",
    "ReportSidebar",
    "ReportDateFeature",
    "ReportComboFeature",
    "ReportFiltersFeature",
    "ReportNavigationFeature",
    "ReportFavoritesFeature",
    "ReportPagingFeature",
    "ReportSortingFeature",
    "ReportDataFeature",
    "ReportGroupingFeature",
    "ReportLayoutFeature",
    "ReportCellsFeature",
    "ReportExportFeature",
    "ReportHeaderGroupsFeature",
    function (
      $scope,
      Sidebar,
      DateFeature,
      ComboFeature,
      FiltersFeature,
      NavigationFeature,
      FavoritesFeature,
      PagingFeature,
      SortingFeature,
      DataFeature,
      GroupingFeature,
      LayoutFeature,
      CellsFeature,
      ExportFeature,
      HeaderGroupsFeature,
    ) {
      const ctx = {};

      FiltersFeature($scope, ctx);
      DateFeature($scope, ctx);
      ComboFeature($scope, ctx);
      NavigationFeature($scope, ctx);
      FavoritesFeature($scope, ctx);
      PagingFeature($scope, ctx);
      SortingFeature($scope, ctx);
      DataFeature($scope, ctx);
      GroupingFeature($scope, ctx);
      LayoutFeature($scope, ctx);
      CellsFeature($scope, ctx);
      ExportFeature($scope, ctx);
      HeaderGroupsFeature($scope, ctx);

      Sidebar.init($scope);
    },
  ]);
})();
