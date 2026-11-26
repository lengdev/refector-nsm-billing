/**
 * Fixed Table Header Plugin for jQuery
 *
 * @version 1.0.0
 * @license MIT
 * @author Kiyofumi Torigoe (kiyotd, torigoedesign)
 * @copyright 2021 Kiyofumi Torigoe
 */
(function ($) {

  const defaultTarget = "th";

  const defaultCss = {
    position: "sticky",
      top: 63,
      "white-space": "nowrap",
      "text-align": "center",
      background: "rgb(248, 246, 246)",
      //border: "1px solid rgba(0,0,0,0.2)"
    //color: "#fff"
  };

  $.fn.fixedTableHeader = function (target = null, css = null) {

    target = target ?? defaultTarget;
    css = css ?? defaultCss;

    const resultCss = Object.assign({}, defaultCss, css)
    // const resultCss = { ...defaultCss, ...css }

    $(target, this).css(resultCss);
    return this;

  }
})(jQuery);
