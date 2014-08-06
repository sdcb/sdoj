using System;
using System.Configuration;
using StructureMap;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SdojWeb.Infrastructure.IoC;
using SdojWeb.Infrastructure.Tasks;
using SdojWeb.Infrastructure.ModelMetadata;

namespace SdojWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public IContainer Container
        {
            get
            {
                return (IContainer)HttpContext.Current.Items["_Container"];
            }
            set
            {
                HttpContext.Current.Items["_Container"] = value;
            }
        }

        public static IContainer StaticContainer { get; set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutoMapperConfig.Execute();

            StaticContainer = new Container();
            DependencyResolver.SetResolver(new StructureMapDependencyResolver(
                () => Container ?? StaticContainer));

            StaticContainer.Configure(cfg =>
            {
                cfg.AddRegistry(new MvcRegistry());
                cfg.AddRegistry(new StandardRegistry());
                cfg.AddRegistry(new ControllerRegistry());
                cfg.AddRegistry(new TaskRegistry());
                cfg.AddRegistry(new ModelMetadataRegistry());
            });

            using (var container = StaticContainer.GetNestedContainer())
            {
                foreach (var task in container.GetAllInstances<IRunAtInit>())
                {
                    task.Execute();
                }

                foreach (var task in container.GetAllInstances<IRunAtStartup>())
                {
                    task.Execute();
                } 
            }
        }

        public void Application_BeginRequest()
        {
            Container = StaticContainer.GetNestedContainer();

            foreach (var task in Container.GetAllInstances<IRunOnEachRequest>())
            {
                task.Execute();
            }
        }

        public void Application_EndRequest()
        {
            try
            {
                foreach (var task in Container.GetAllInstances<IRunAfterEachRequest>())
                {
                    task.Execute();
                }
            }
            finally
            {
                Container.Dispose();
                Container = null;
            }
        }

        public void Application_Error()
        {
            foreach (var task in Container.GetAllInstances<IRunOnError>())
            {
                task.Execute();
            }
        }

        public static string RootNamespace { get { return typeof(MvcApplication).Namespace; } }
    }

    public static class AppSettings
    {
        public static int DefaultPageSize
        {
            get
            {
                var settingString = ConfigurationManager.AppSettings["DefaultPageSize"];
                return int.Parse(settingString);
            }
        }

        public static bool EnableSolutionDbScan
        {
            get
            {
                var settingsString = ConfigurationManager.AppSettings["EnableSolutionDbScan"];
                return bool.Parse(settingsString);
            }
        }
    }
}
