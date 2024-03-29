﻿using System.Configuration;
using System.Data.SqlClient;

namespace WorldsBestBars.Services
{
    public abstract class BaseService
    {
        #region Constants

        static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["worldsbestbars_db"].ConnectionString;

        #endregion

        #region Public Properties

        public static IServiceResolver ServiceResolver
        {
            protected get { return _serviceResolver = (_serviceResolver ?? new DefaultServiceResolver()); }
            set { _serviceResolver = value; }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Gets a new database connection.
        /// </summary>
        /// <returns>A new database connection.</returns>
        protected static SqlConnection GetConnection()
        {
            var result = new SqlConnection(ConnectionString);

            result.Open();

            return result;
        }


        /// <summary>
        /// Provides a wrapper around the specified service resolvers GetService method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected static T GetService<T>() where T : new()
        {
            return ServiceResolver.GetService<T>();
        }

        #endregion

        #region Private Fields

        static IServiceResolver _serviceResolver;

        #endregion
    }
}
