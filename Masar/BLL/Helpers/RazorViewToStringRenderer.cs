using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BLL.Helpers
{
    public class RazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync(ControllerContext controllerContext, string viewName, object model)
        {
            // 1. Use the REAL context passed from the controller. 
            // This ensures RouteData (Controller="Instructor") is preserved.

            using var sw = new StringWriter();

            // 2. Try FindView (Standard Search: /Views/Instructor/ & /Views/Shared/)
            var viewResult = _viewEngine.FindView(controllerContext, viewName, false);

            // 3. Fallback: If FindView fails, try GetView (Allows full paths like "~/Views/Specific/File.cshtml")
            if (viewResult.View == null)
            {
                viewResult = _viewEngine.GetView(null, viewName, false);
            }

            if (viewResult.View == null)
            {
                throw new FileNotFoundException($"View '{viewName}' not found. Searched locations: {string.Join(", ", viewResult.SearchedLocations ?? new string[] { })}");
            }

            // 4. Create the View Dictionary
            var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            // 5. Create the Context
            var viewContext = new ViewContext(
                controllerContext,
                viewResult.View,
                viewDictionary,
                new TempDataDictionary(controllerContext.HttpContext, _tempDataProvider),
                sw,
                new HtmlHelperOptions()
            );

            // 6. Render
            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}