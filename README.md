# Acando.AspNet.NodeServices.MVC
--------------------------------

Adds MVC base controller and NodeResult for use in conjunction with [Acando.AspNet.NodeServices](https://github.com/AcandoCxC/acando-nodeservices/) 

Configuration
------------------------

Default Entry path for the file that node will execute can be set in an appsetting.

* `NodeServices.Mvc.NodeServerEntryFilePath` - defaults to `./react.server`
  * `<add key="NodeServices.Mvc.NodeServerEntryFilePath" value="./server" />`
 
CssDivider key that will be used in partial (or fallback) rendering (will output the CSS to `ViewBag.NodeCss`)
* `NodeServices.Mvc.CssDivider"` - defaults to `|css|`
  * `<add key="NodeServices.Mvc.CssDivider" value="|myCss|" />`
  
App Client Name that will be used in partial (or fallback) rendering (will output to `ViewBag.ClientAppName`)
* `NodeServices.Mvc.ClientAppName"` - defaults to `client`
  * `<add key="NodeServices.Mvc.ClientAppName" value="|myClient|" />`

View that will be used in partial (or fallback) rendering (place in Views/Shared)
* `NodeServices.Mvc.ViewName"` - defaults to `_NodeResult`
  * `<add key="NodeServices.Mvc.ViewName" value="_React" />`

Usage
-----------------------

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
        public HomeController(INodeServices nodeServices)  : base(nodeServices)
        {
        }

        public async Task<ActionResult> Index()
        {
            return await NodeResultAsync(<json data as string or objects here>);
        }
    }
  ```
  * Example with custom entry file
  ```
    public class HomeController : NodeBaseController
    {
        public HomeController(INodeServices nodeServices)  : base(nodeServices)
        {
        }

        public async Task<ActionResult> Index()
        {
            return await NodeResultEntryFileAsync("./server", <json data as string or objects here>);
        }
    }
  ```
  
 `NodeResult` accepts objects that will be serialized to Json or already serialized Json as strings (or a combination thereof).

