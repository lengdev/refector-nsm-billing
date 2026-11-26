using System.IO;
using System.Web.Mvc;

namespace NSMSOLUTION.Helper_Code
{
    public static class ViewRenderHelper
    {
        public static string RenderViewToString(
            ControllerContext controllerContext,
            string viewName,
            object model)
        {
            var viewData =
                new ViewDataDictionary(model);

            var tempData =
                controllerContext.Controller.TempData;

            using (var sw = new StringWriter())
            {
                var viewResult =
                    ViewEngines.Engines.FindView(
                        controllerContext,
                        viewName,
                        null);

                if (viewResult.View == null)
                {
                    throw new FileNotFoundException(
                        "View not found: " + viewName);
                }

                var viewContext =
                    new ViewContext(
                        controllerContext,
                        viewResult.View,
                        viewData,
                        tempData,
                        sw);

                viewResult.View.Render(
                    viewContext,
                    sw);

                viewResult.ViewEngine.ReleaseView(
                    controllerContext,
                    viewResult.View);

                return sw.ToString();
            }
        }
    }
}