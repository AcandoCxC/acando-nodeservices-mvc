# Acando.AspNet.NodeServices.MVC

---

Adds MVC base controller and NodeResult for use in conjunction with [Acando.AspNet.NodeServices](https://github.com/AcandoCxC/acando-nodeservices/)

## Configuration

---

Default Entry path for the file that node will execute can be set in an appsetting.

* `NodeServices.Mvc.NodeServerEntryFilePath` - defaults to `./react.server`
  * `<add key="NodeServices.Mvc.NodeServerEntryFilePath" value="./server" />`

App Client Name that will be used in partial (or fallback) rendering (will output to `ViewBag.ClientAppName`)

* `NodeServices.Mvc.ClientAppName"` - defaults to `client`
  * `<add key="NodeServices.Mvc.ClientAppName" value="|myClient|" />`

View that will be used in partial (or fallback) rendering (place in Views/Shared)

* `NodeServices.Mvc.ViewName"` - defaults to `_NodeResult`
  * `<add key="NodeServices.Mvc.ViewName" value="_React" />`

Disable fallback rendering, outputting the error instead.
* `NodeServices.Mvc.DisableFallbackRendering"` - defaults to `false`
  * `<add key="NodeServices.Mvc.DisableFallbackRendering" value="true" />`

## Usage

---

* Inject NodeServices into your controller
    * Example with structuremap.mvc (in `DefaultRegistry.cs`):
    ```
    For<INodeServices>().Use(
                () => NodeServicesFactory.CreateNodeServices(
                new NodeServicesOptions(HttpContext.Current.Server.MapPath("/")))
                ).LifecycleIs<SingletonLifecycle>();
    ```

* Inherit NodeBaseController on your controller. Supply INodeServices to base.
  * Example with default entry file
  ```
    public class HomeController : NodeBaseController
    {
        private GlobalData _globalData;
        
        public HomeController(INodeServices nodeServices)  : base(nodeServices)
        {
          _globalData = new GlobalData() {
            Routes = YourRouteBuilder(),
            WhateverElseIsNeeded = SomeBuilder()
          }
        }

        public async Task<ActionResult> Index()
        {
            var viewModel = BuildYourViewModel();

            // Full node Rendering, result written directly to Response
            return await NodeResultAsync(_globalData, viewModel);

            // Partial node Rendering, result rendered with Shared/_NodeResult.cshtml by default
            return await PartialNodeResultAsync(_globalData, viewModel);
        }
    }
  ```
  * Example with custom entry file
  ```
    public class HomeController : NodeBaseController
    {
        private GlobalData _globalData;
        
        public HomeController(INodeServices nodeServices)  : base(nodeServices)
        {
          _globalData = new GlobalData() {
            Routes = YourRouteBuilder(),
            WhateverElseIsNeeded = SomeBuilder()
          }
        }

        public async Task<ActionResult> Index()
        {
            // Full node Rendering, result written directly to Response
            return await NodeResultEntryFileAsync("./server", _globalData, viewModel);

            // Partial node Rendering, result rendered with Shared/_NodeResult.cshtml by default
            return await PartialNodeResultEntryFileAsync(_globalData, viewModel);
        }
    }
  ```
  
  ## Outputs

  ---
  
  Partial and Fallback rendering will be output to `NodeServices.Mvc.ViewName` (default _NodeResults.cshtml) with the Model `Acando.AspNet.NodeServices.Mvc.NodeResultModel`
  
  ViewBag will be set with:

  * `ViewBag.NodeCss` - a string of the css output delivered from Node.
    * Output this to your _Layout.cshtml as `Html.Raw()` and wrap it in `<style>-tags`.
  * `ViewBag.NodeChunks` - a string of script chunks delivered from node.
    * Output this with Html.Raw at the end of your _Layout.cshtml
  * `ViewBag.ClientAppName` - the string set in `NodeServices.Mvc.ViewName` 
    * If this is present then you know that NodeServices has loaded on your page and you should add a link to your main.js

