# Acando.AspNet.NodeServices.MVC
--------------------------------

Adds MVC base controller and NodeResult for use in conjunction with [Acando.AspNet.NodeServices](https://github.com/AcandoCxC/acando-nodeservices/) 

Configuration
------------------------

Default Entry path for the file that node will execute can be set in an appsetting.

* `DefaultNodeServerEntryFilePath` - defaults to `./react.server`
  * `<add key="DefaultNodeServerEntryFilePath" value="./server" />
 
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

