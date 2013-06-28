using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using WorldsBestBars.Services;

namespace WorldsBestBars.Web.Admin
{
    public class AutofacConfig
    {
        #region Public Methods

        /// <summary>
        /// Configures this instance.
        /// </summary>
        public static void Configure()
        {
            var builder = new ContainerBuilder();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            builder.RegisterTypes(GetEnumerableOfType<BaseService>().ToArray());

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        #endregion

        #region Private Methods

        static IEnumerable<Type> GetEnumerableOfType<T>() where T : class
        {
            var result = new List<Type>();
            
            foreach (var type in Assembly.GetAssembly(typeof(T)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                result.Add(type);
            }

            return result;
        }

        #endregion
    }
}