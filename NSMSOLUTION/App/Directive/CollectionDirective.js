/*
 * Add Collection: shared helpers and directives.
 *   clFmt            number / date helpers
 *   <cl-combo>       searchable combo, used for EVERY dropdown on the screen
 *   cl-money         two-decimal money input bound to a number
 *   cl-indeterminate sets the indeterminate state of a checkbox
 *   cl-open-picker   button that opens the native date picker next to it
 *   cl-focus-on      focuses an input when the named event is broadcast
 *
 * Uses the module your app already defines. Replace 'collectionApp' with your module name.
 */
(function () {
  "use strict";

  app
    .factory("clFmt", function () {
      function r2(n) {
        return Math.round((n + Number.EPSILON) * 100) / 100;
      }
      function pad2(n) {
        return (n < 10 ? "0" : "") + n;
      }
      function fmt(n) {
        return (n || 0).toLocaleString("en-US", {
          minimumFractionDigits: 2,
          maximumFractionDigits: 2,
        });
      }
      function parse(s) {
        var n = parseFloat(String(s == null ? "" : s).replace(/,/g, ""));
        return isNaN(n) ? 0 : Math.max(0, r2(n));
      }
      function toIso(d) {
        return (
          d.getFullYear() +
          "-" +
          pad2(d.getMonth() + 1) +
          "-" +
          pad2(d.getDate())
        );
      }
      function isoToDate(iso) {
        var p = iso.split("-");
        return new Date(+p[0], +p[1] - 1, +p[2]);
      }
      function dmy(iso) {
        var p = iso.split("-");
        return p[2] + "-" + p[1] + "-" + p[0];
      }
      function parseDmy(s) {
        var m = /^(\d{1,2})[-\/.](\d{1,2})[-\/.](\d{4})$/.exec(
          String(s || "").trim(),
        );
        if (!m) {
          return null;
        }
        var d = +m[1],
          mo = +m[2],
          y = +m[3],
          dt = new Date(y, mo - 1, d);
        if (
          dt.getFullYear() !== y ||
          dt.getMonth() !== mo - 1 ||
          dt.getDate() !== d
        ) {
          return null;
        }
        return y + "-" + pad2(mo) + "-" + pad2(d);
      }
      return {
        r2: r2,
        fmt: fmt,
        parse: parse,
        toIso: toIso,
        isoToDate: isoToDate,
        dmy: dmy,
        parseDmy: parseDmy,
        todayIso: function () {
          return toIso(new Date());
        },
      };
    })

    /* ---------------------------------------------------------------- <cl-combo>
       <cl-combo ng-model="form.x" items="list" input-id="cl-x" placeholder="..." focus-on="event-name"
                 on-search="fn(text)" on-add="fn(text)" add-label="Add customer"></cl-combo>
       items: [{ value, label, sub?, right?, find? }]. ng-model holds the chosen item's value. ng-change works as usual.
       on-add (optional): shows a pinned "add new" row at the top of the list; it gets the text the user typed ('' if none). */
    .directive("clCombo", [
      "$timeout",
      function ($timeout) {
        return {
          restrict: "E",
          require: "ngModel",
          scope: {
            items: "=",
            placeholder: "@",
            inputId: "@",
            ariaLabel: "@",
            onSearch: "&",
            onAdd: "&",
            addLabel: "@",
            loading: "=?",
          },
          template: `
            <div class="cl-ctl cl-combo" ng-class="{'cl-open': s.open}">
              <input type="text" id="{{inputId}}" role="combobox" autocomplete="off" aria-autocomplete="list"
                aria-controls="{{inputId}}-list" aria-expanded="{{s.open}}"
                ng-attr-aria-label="{{ariaLabel || undefined}}"
                placeholder="{{placeholder}}" ng-model="s.text" ng-change="onType()"
                ng-focus="onFocus()" ng-click="openList()" ng-keydown="onKey($event)" ng-blur="onBlur()">
              <i class="ti ti-chevron-down cl-ico cl-chev" aria-hidden="true"></i>
              <ul id="{{inputId}}-list" role="listbox" aria-hidden="{{!s.open}}"
                ng-class="{'cl-show': s.open, 'cl-up': s.up, 'cl-wide': s.view[0].level !== undefined}"
                ng-mousedown="$event.preventDefault()">
                <li class="cl-addnew" ng-if="hasAdd" role="button" ng-mousedown="addNew()">
                  <i class="ti ti-plus cl-ico" aria-hidden="true"></i>
                  <span>{{addLabel || 'Add new'}}</span>
                </li>
                <!-- Combo options -->
                <li ng-repeat="it in s.view track by $index"
                    role="option"
                    ng-class="{
                      'cl-on': $index === s.idx,
                      'cl-sel': it.value === model,
                      'cl-row3': it.level !== undefined,
                      'cl-top': it.level === 0
                    }"
                    aria-selected="{{it.value === model}}"
                    ng-mousedown="choose(it)"
                    ng-mouseenter="s.idx = $index">

                  <!-- Standard combo row -->
                  <span ng-if="it.level === undefined">
                    <b class="cl-t">{{it.label}}</b>
                    <small ng-if="it.sub">{{it.sub}}</small>
                  </span>

                  <span class="cl-bal"
                        ng-if="it.level === undefined && it.right">
                    {{it.right}}
                  </span>

                  <!-- Account combo row -->
                  <span class="cl-c1"
                        ng-if="it.level !== undefined"
                        ng-style="{'padding-left': (it.level * 16) + 'px'}">
                    {{it.label}}
                  </span>

                  <span class="cl-c2" ng-if="it.level !== undefined">
                    {{it.type}}
                  </span>

                </li>
                <li class="cl-none" ng-if="!s.view.length">{{loading ? 'Searching…' : 'No match'}}</li>
              </ul>
            </div>
          `,
          link: function (scope, el, attrs, ngModel) {
            var s = (scope.s = {
              open: false,
              text: "",
              view: [],
              idx: -1,
              up: false,
              typed: false, // true once the user has typed in this session (so the add-dialog prefill is real input)
            });
            var input = el[0].querySelector("input");
            var ul = el[0].querySelector("ul");
            var suppress = false;
            var remote = !!attrs.onSearch;
            scope.hasAdd = !!attrs.onAdd;

            function find(v) {
              var a = scope.items || [];
              for (var i = 0; i < a.length; i++) {
                if (a[i].value === v) {
                  return a[i];
                }
              }
              return null;
            }
            function labelOf(v) {
              var it = find(v);
              return it ? it.label : "";
            }
            function refresh(q) {
              q = (q || "").toLowerCase().trim();
              s.view = (scope.items || []).filter(function (it) {
                return (
                  !q ||
                  (it.find || it.label + " " + (it.sub || ""))
                    .toLowerCase()
                    .indexOf(q) > -1
                );
              });
              var cur = -1;
              for (var i = 0; i < s.view.length; i++) {
                if (s.view[i].value === scope.model) {
                  cur = i;
                }
              }
              s.idx = s.view.length ? Math.max(0, cur) : -1;
              scrollToActive();
            }
            function scrollToActive() {
              $timeout(function () {
                // options only: the pinned "add new" row is not part of keyboard navigation
                var li = ul.querySelectorAll('li[role="option"]')[s.idx];
                if (li && li.scrollIntoView) {
                  li.scrollIntoView({ block: "nearest" });
                }
              });
            }
            function decidePlacement() {
              var c = el[0].getBoundingClientRect();
              var h = Math.min(
                240,
                s.view.length * 30 + (scope.hasAdd ? 40 : 10),
              ); // estimated list height (CSS max-height is 240)
              s.up =
                c.bottom + 6 + h > window.innerHeight - 8 &&
                c.top > window.innerHeight - c.bottom;
            }
            function close() {
              s.open = false;
            }

            ngModel.$render = function () {
              scope.model = ngModel.$viewValue;
              if (!s.open) {
                s.text = labelOf(scope.model);
              }
            };
            scope.$watchCollection("items", function () {
              if (s.open) {
                refresh(s.text);
              } else {
                s.text = labelOf(scope.model);
              }
            });

            scope.openList = function () {
              if (s.open) {
                return;
              }
              s.typed = false;
              refresh("");
              decidePlacement();
              s.open = true;
            };
            scope.onFocus = function () {
              if (!suppress) {
                input.select();
                scope.openList();
              }
            };
            scope.onType = function () {
              s.typed = true;
              if (!s.open) {
                refresh("");
                decidePlacement();
                s.open = true;
              }
              refresh(s.text);
              if (remote) {
                scope.onSearch({ text: s.text });
              }
            };
            scope.choose = function (it) {
              ngModel.$setViewValue(it.value);
              scope.model = it.value;
              s.text = it.label;
              close();
            };
            scope.addNew = function () {
              var t = s.typed ? s.text : "";
              close();
              s.text = labelOf(scope.model);
              scope.onAdd({ text: t });
            };
            scope.onBlur = function () {
              $timeout(function () {
                if (document.activeElement === input) {
                  return;
                }
                close();
                s.text = labelOf(scope.model);
                if (remote) {
                  scope.onSearch({ text: "" });
                }
              }, 120);
            };
            scope.onKey = function (e) {
              var n = s.view.length;
              switch (e.key) {
                case "ArrowDown":
                  e.preventDefault();
                  if (!s.open) {
                    scope.openList();
                  } else if (n) {
                    s.idx = (s.idx + 1) % n;
                    scrollToActive();
                  }
                  break;
                case "ArrowUp":
                  e.preventDefault();
                  if (!s.open) {
                    scope.openList();
                  } else if (n) {
                    s.idx = (s.idx - 1 + n) % n;
                    scrollToActive();
                  }
                  break;
                case "Home":
                  if (s.open) {
                    e.preventDefault();
                    s.idx = 0;
                    scrollToActive();
                  }
                  break;
                case "End":
                  if (s.open) {
                    e.preventDefault();
                    s.idx = n - 1;
                    scrollToActive();
                  }
                  break;
                case "Enter":
                  if (s.open && s.view[s.idx]) {
                    e.preventDefault();
                    scope.choose(s.view[s.idx]);
                  }
                  break;
                case "Escape":
                  if (s.open) {
                    e.stopPropagation();
                    close();
                    s.text = labelOf(scope.model);
                  }
                  break;
              }
            };

            if (attrs.focusOn) {
              scope.$on(attrs.focusOn, function () {
                $timeout(function () {
                  suppress = true;
                  input.focus();
                  suppress = false;
                });
              });
            }
          },
        };
      },
    ])

    /* ---------------------------------------------------------------- cl-money */
    .directive("clMoney", [
      "clFmt",
      function (F) {
        return {
          restrict: "A",
          require: "ngModel",
          link: function (scope, el, attrs, ctrl) {
            ctrl.$parsers.push(function (v) {
              return F.parse(v);
            });
            ctrl.$formatters.push(function (v) {
              return F.fmt(v);
            });
            el.on("focus", function () {
              this.select();
            });
            el.on("blur", function () {
              ctrl.$viewValue = F.fmt(ctrl.$modelValue);
              ctrl.$render();
            });
          },
        };
      },
    ])

    /* ---------------------------------------------------------------- misc */
    .directive("clIndeterminate", function () {
      return {
        restrict: "A",
        link: function (scope, el, attrs) {
          scope.$watch(attrs.clIndeterminate, function (v) {
            el[0].indeterminate = !!v;
          });
        },
      };
    })

    .directive("clOpenPicker", function () {
      return {
        restrict: "A",
        link: function (scope, el) {
          el.on("click", function () {
            var inp = el[0].parentNode.querySelector('input[type="date"]');
            if (!inp) {
              return;
            }
            try {
              inp.showPicker();
            } catch (e) {
              inp.focus();
            }
          });
        },
      };
    })

    .directive("clFocusOn", [
      "$timeout",
      function ($timeout) {
        return {
          restrict: "A",
          link: function (scope, el, attrs) {
            scope.$on(attrs.clFocusOn, function () {
              $timeout(function () {
                el[0].focus();
                el[0].select();
              });
            });
          },
        };
      },
    ])
    .directive("clSlips", [
      "$timeout",
      function ($timeout) {
        return {
          restrict: "A",
          link: function (scope, el) {
            var input = el.find("input")[0];

            scope.slipDrag = false;

            input.addEventListener("change", function () {
              scope.$apply(function () {
                scope.addSlips(input.files);
                input.value = "";
              });
            });

            el.on("dragover", function (e) {
              e.preventDefault();
              $timeout(function () {
                scope.slipDrag = true;
              });
            });

            el.on("dragleave", function () {
              $timeout(function () {
                scope.slipDrag = false;
              });
            });

            el.on("drop", function (e) {
              e.preventDefault();
              var dt = e.originalEvent
                ? e.originalEvent.dataTransfer
                : e.dataTransfer;
              scope.$apply(function () {
                scope.slipDrag = false;
                scope.addSlips(dt && dt.files);
              });
            });
          },
        };
      },
    ]);
})();
