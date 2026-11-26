// Drag & drop reordering for the column / header customization lists.
(function () {
  "use strict";

  app.directive("rpSort", [
    "$parse",
    function ($parse) {
      return {
        restrict: "A",
        link: function (scope, element, attrs) {
          const root = element[0];
          const onMove = $parse(attrs.rpSort);
          let from = -1;

          const rowOf = (target) =>
            target && target.closest ? target.closest(".rp-cust-row") : null;

          const clearMarks = () =>
            root
              .querySelectorAll(".rp-dragging, .rp-drop-over")
              .forEach((n) =>
                n.classList.remove("rp-dragging", "rp-drop-over"),
              );

          const finish = () => {
            from = -1;
            clearMarks();
          };

          root.addEventListener("dragstart", function (e) {
            const row = rowOf(e.target);
            if (!row || !e.target.classList.contains("rp-grip")) return;
            from = Number(row.dataset.index);
            row.classList.add("rp-dragging");
            e.dataTransfer.effectAllowed = "move";
            e.dataTransfer.setData("text/plain", String(from));
            if (e.dataTransfer.setDragImage)
              e.dataTransfer.setDragImage(row, 16, 16);
          });

          root.addEventListener("dragover", function (e) {
            const row = rowOf(e.target);
            if (from < 0 || !row) return;
            e.preventDefault();
            e.dataTransfer.dropEffect = "move";
            root
              .querySelectorAll(".rp-drop-over")
              .forEach((n) => n.classList.remove("rp-drop-over"));
            row.classList.add("rp-drop-over");
          });

          root.addEventListener("drop", function (e) {
            const row = rowOf(e.target);
            if (from < 0 || !row) return;
            e.preventDefault();
            const start = from;
            const to = Number(row.dataset.index);
            finish();
            if (to !== start) {
              scope.$apply(function () {
                onMove(scope, { from: start, to: to });
              });
            }
          });

          root.addEventListener("dragend", finish);
        },
      };
    },
  ]);

  app.directive("rpTruncTitle", function () {
    return {
      link: function (scope, el) {
        const table = el[0];
        function onOver(e) {
          const td = e.target.closest("td");
          if (!td || !table.contains(td)) return;
          td.title =
            td.scrollWidth > td.clientWidth
              ? td.textContent.replace(/\s+/g, " ").trim()
              : "";
        }
        table.addEventListener("mouseover", onOver);
        scope.$on("$destroy", () =>
          table.removeEventListener("mouseover", onOver),
        );
      },
    };
  });
})();
