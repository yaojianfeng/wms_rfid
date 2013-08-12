using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Security.Principal;
using System.Diagnostics;
using Authority.Controllers;
using THOK.Common;
using SignalR;
using THOK.Wms.SignalR;
using THOK.Wms.SignalR.Connection;
using System.IO.Compression;
using THOK.Security;
using Wms.Security;
using THOK.Common.Ef.Infrastructure;
using THOK.Authority.Bll.Interfaces;
using Quartz;
using Quartz.Impl;
using System.Configuration;
using System.Collections.Specialized;
using Wms.Quartz;
namespace Wms
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static IControllerFactory controllerFactory;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new SystemEventLogAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapConnection<AutomotiveSystemsConnection>("automotiveSystems", "task/automotiveSystems/{*operation}");
            routes.MapConnection<AllotStockInConnection>("allotStockIn", "allotStockIn/{*operation}");
            routes.MapConnection<AllotStockOutConnection>("allotStockOut", "allotStockOut/{*operation}");
            routes.MapConnection<DispatchSortWorkConnection>("allotSortWork", "allotSortWork/{*operation}");
            routes.MapRoute(
                "Default", // 路由名称
                "{controller}/{action}/{SystemID}", // 带有参数的 URL
                new { controller = "Home", action = "Index", SystemID = UrlParameter.Optional } // 参数默认值
            );            
        }

        public static void RegisterIocUnityControllerFactory()
        {
            //Set for Controller Factory
            controllerFactory = new UnityControllerFactory();
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);
            GlobalHost.DependencyResolver = new UnityConnectionDependencyResolver();
        }

        void Application_Start()
        {
            RegisterIocUnityControllerFactory();
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            QuartzStart();
        }

        void Application_End()
        {
            QuartzShutdown();
        }

        void Application_Error()
        {
            ResetContext();
            ServiceFactory EventLogFactory = new ServiceFactory();
            Exception exception = Server.GetLastError();
            if (exception != null)
            {
                Response.Clear();
                HttpException httpException = new HttpException(exception.Message, exception);
                var cxt = new HttpContextWrapper(Context);

                RouteData routeData = new RouteData();
                routeData.Values.Add("controller", "Home");

               string ModuleNam ="/"+ Context.Request.RequestContext.RouteData.Values["controller"].ToString()+"/";
                string FunctionName = Context.Request.RequestContext.RouteData.Values["action"].ToString();
                string ExceptionalType = exception.Message;
                string ExceptionalDescription = exception.ToString();
                string State = "1";

                if (httpException != null)
                {
                    switch (httpException.GetHttpCode())
                    {
                        case 404:
                            if (Context.Request.RequestContext.RouteData.Values["action"].ToString() == "Index")
                            {
                                routeData.Values.Add("action", "PageNotFound");
                                routeData.Values.Add("PageNotFoundLog", exception.Message);
                            }
                            else
                            {
                                routeData.Values.Add("action", "AjaxPageNotFound");
                                routeData.Values.Add("AjaxPageNotFoundLog", exception.Message);
                            }
                            break;
                        case 500:
                            if (Context.Request.RequestContext.RouteData.Values["action"].ToString() == "Index")
                            {
                                routeData.Values.Add("action", "ServerError");
                                routeData.Values.Add("ServerErrorLog", exception.Message);
                            }
                            else
                            {
                                routeData.Values.Add("action", "AjaxServerError");
                                routeData.Values.Add("AjaxServerErrorLog", exception.Message);
                            }
                            Trace.TraceError("Server Error occured and caught in Global.asax - {0}", exception.ToString());
                            break;
                        default:
                            if (Context.Request.RequestContext.RouteData.Values["action"].ToString() == "Index")
                            {
                                routeData.Values.Add("action", "Error");
                                routeData.Values.Add("ErrorLog", exception.Message);
                                routeData.Values.Add("errorCode", httpException.GetHttpCode());
                            }
                            else
                            {
                                routeData.Values.Add("action", "AjaxError");
                                routeData.Values.Add("AjaxErrorLog", exception.Message);
                                routeData.Values.Add("errorCode", httpException.GetHttpCode());
                            }
                            Trace.TraceError("Error occured and caught in Global.asax - {0}", exception.ToString());
                            break;
                    }
                }
                EventLogFactory.GetService<IExceptionalLogService>().CreateExceptionLog(ModuleNam, FunctionName, ExceptionalType, ExceptionalDescription, State);
                Server.ClearError();
                Response.TrySkipIisCustomErrors = true;
                IController errorController = new HomeController();
                errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
            }
        }

        void Session_Start()
        {

        }

        void Session_End()
        {
            if (Session["userName"] != null)
            {
                ServiceFactory UserFactory = new ServiceFactory();
                UserFactory.GetService<IUserService>().DeleteUserIp(Session["userName"].ToString());
                UserFactory.GetService<ILoginLogService>().UpdateLoginLog(Session["userName"].ToString(), DateTime.Now.ToString());
            }
        }        

        void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            //bool enableGzip = this.Request.Headers["Accept-Encoding"] != null ?
            //    this.Request.Headers["Accept-Encoding"].Contains("gzip") : false;
            //if (enableGzip)
            //{
            //    this.Response.Filter = new GZipStream(this.Response.Filter, CompressionMode.Compress);
            //    this.Response.AppendHeader("Content-Encoding", "gzip");
            //}

            if (Context.User == null)
            {
                var oldTicket = ExtractTicketFromCookie(Context, FormsAuthentication.FormsCookieName);
                if (oldTicket != null && !oldTicket.Expired)
                {
                    var ticket = oldTicket;
                    if (FormsAuthentication.SlidingExpiration)
                    {
                        ticket = FormsAuthentication.RenewTicketIfOld(oldTicket);
                        if (ticket == null)
                        {
                            return;
                        }
                    }
                    string[] roles = new string[] { "Administrator" };
                    Context.User = new GenericPrincipal(new FormsIdentity(ticket), roles);
                    if (ticket != oldTicket)
                    {
                        string cookieValue = FormsAuthentication.Encrypt(ticket);
                        var cookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName] ?? new HttpCookie(FormsAuthentication.FormsCookieName, cookieValue) { Path = ticket.CookiePath };
                        if (ticket.IsPersistent)
                        {
                            cookie.Expires = ticket.Expiration;
                        }
                        cookie.Value = cookieValue;
                        cookie.Secure = FormsAuthentication.RequireSSL;
                        cookie.HttpOnly = true;
                        if (FormsAuthentication.CookieDomain != null)
                        {
                            cookie.Domain = FormsAuthentication.CookieDomain;
                        }
                        Context.Response.Cookies.Remove(cookie.Name);
                        Context.Response.Cookies.Add(cookie);
                    }
                }
            }
        }

        private static FormsAuthenticationTicket ExtractTicketFromCookie(HttpContext context, string name)
        {
            FormsAuthenticationTicket ticket = null;
            string encryptedTicket = null;

            var cookie = context.Request.Cookies[name];
            if (cookie != null)
            {
                encryptedTicket = cookie.Value;
            }

            if (!string.IsNullOrEmpty(encryptedTicket))
            {
                try
                {
                    ticket = FormsAuthentication.Decrypt(encryptedTicket);
                }
                catch
                {
                    context.Request.Cookies.Remove(name);
                }

                if (ticket != null && !ticket.Expired)
                {
                    return ticket;
                }

                context.Request.Cookies.Remove(name);
            }

            return null;
        }

        private static void ResetContext()
        {
            ContextManager.SetRepositoryContext(null, @"THOK.Wms.Repository.AuthorizeContext,THOK.Wms.Repository.dll");
        }

        private IScheduler sched { get; set; }
        private void QuartzStart()
        {
            NameValueCollection properties = new NameValueCollection();
            properties["quartz.scheduler.instanceName"] = "ConsoleScheduler";
            properties["quartz.scheduler.instanceId"] = "instance_one";
            properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
            properties["quartz.threadPool.threadCount"] = "10";
            properties["quartz.threadPool.threadPriority"] = "Normal";

            ISchedulerFactory sf = new StdSchedulerFactory(properties);
            IScheduler sched = sf.GetScheduler();

            IJobDetail job = JobBuilder.Create<JobDoDailyBalance>()
                .WithIdentity("HelloJob", "HelloJobGroup")
                .WithDescription("This is my first job")
                .RequestRecovery()
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("HelloJob", "HelloJobGroup")
                .WithDescription("This is my first trigger")
                //.WithCronSchedule("0/10 * * * * ?")//每10秒执行；
                .WithCronSchedule("0 0 0/1 1/1 * ? *")//每小时执行；
                //.WithCronSchedule("0 0 4 1/1 * ? *")//每天4点执行；
                .StartNow()
                .Build();

            if (!sched.CheckExists(job.Key))
            {
                sched.ScheduleJob(job, trigger);
            }

            if (!sched.IsStarted)
            {
                sched.Start();
            }
        }
        private void QuartzShutdown()
        {
            if (sched != null && sched.IsStarted)
            {
                sched.Shutdown();
            }
        }
    }
}