using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using WorldsBestBars.Services.Models;
using Dapper;

namespace WorldsBestBars.Services
{
    public class DocumentService : BaseService
    {
        #region Public Methods

        /// <summary>
        /// Gets the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public Document Get(Guid id)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"select 
    D.Id, 
    D.ParentId,
    D.RedirectId,
    D.Name,
    D.Synopsis,
    D.Content,
    D.UrlKey,
    D.Url,
    D.IsActive,
    D.Created,
    D.Modified
from 
    Document D
where 
    D.Id = @id";

                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var result = Document.FromReader(reader);

                            if (result != null)
                            {
                                result.Images = GetImages(result.Id);
                                reader.Close();

                                result.Categories = GetCategories(connection, result.Id);
                            }

                            return result;
                        }

                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the drafts.
        /// </summary>
        /// <param name="total">The total.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        public IEnumerable<Document> GetDrafts(out int total, int skip, int take)
        {
            return GetWithStatus(false, out total, skip, take);
        }

        /// <summary>
        /// Gets the active.
        /// </summary>
        /// <param name="total">The total.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        public IEnumerable<Document> GetActive(out int total, int skip, int take)
        {
            return GetWithStatus(true, out total, skip, take);
        }

        /// <summary>
        /// Activates the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        public void Activate(Guid id, bool isActive)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "update Document set IsActive = @isActive where Id = @id";

                    command.Parameters.AddWithValue("@isActive", isActive);
                    command.Parameters.AddWithValue("@id", id);

                    command.ExecuteNonQuery();

                    ServiceResolver.GetService<TryInvalidateCache>().Execute(id, "document");
                }
            }
        }

        /// <summary>
        /// Deletes the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        public void Delete(Guid id)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"delete from DocumentCategory where DocumentId = @id;
delete from Document where Id = @id";

                    command.Parameters.AddWithValue("@id", id);

                    command.ExecuteNonQuery();

                    ServiceResolver.GetService<TryInvalidateCache>().Execute(id, "document");
                }
            }
        }

        /// <summary>
        /// Creates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Guid Create(UpdateDocument model)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select @id = newid();insert into Document (Id, Name, UrlKey, Synopsis, Content, Created, Modified, IsActive) values ( @id, @name, @urlKey, @synopsis, @content, getdate(), getdate(), 0)";

                    command.Parameters.AddWithValue("@name", model.Name);
                    command.Parameters.AddWithValue("@urlKey", model.UrlKey);
                    command.Parameters.AddWithValue("@synopsis", model.Synopsis);
                    command.Parameters.AddWithValue("@content", model.Content);
                    command.Parameters.Add(new SqlParameter("@id", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output });

                    command.ExecuteNonQuery();

                    var id = (Guid)command.Parameters["@id"].Value;

                    ServiceResolver.GetService<TryInvalidateCache>().Execute(id, "document");

                    return id;
                }
            }
        }

        /// <summary>
        /// Updates the specified model.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="model">The model.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Update(Guid id, UpdateDocument model)
        {
            // TODO: move media if key changes
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "update Document set Name = @name, UrlKey = @urlKey, ParentId = @parent, Synopsis = @synopsis, Content = @content, Modified = getdate() where Id = @id";

                    command.Parameters.AddWithValue("@name", model.Name);
                    command.Parameters.AddWithValue("@parent", model.ParentId.HasValue ? (object)model.ParentId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@urlKey", model.UrlKey);
                    command.Parameters.AddWithValue("@synopsis", model.Synopsis);
                    command.Parameters.AddWithValue("@content", model.Content);
                    command.Parameters.AddWithValue("@id", id);

                    command.ExecuteNonQuery();
                }
            }

            ServiceResolver.GetService<TryInvalidateCache>().Execute(id, "document");
        }

        public string GetMediaPathPhysical(Guid id)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select @path = Url from UrlMap where id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.Add(new SqlParameter("@path", SqlDbType.VarChar, 64) { Direction = ParameterDirection.Output });

                    command.ExecuteNonQuery();

                    var path = command.Parameters["@path"].Value == DBNull.Value ? null : (string)command.Parameters["@path"].Value;

                    if (path != null)
                    {
                        return System.IO.Path.Combine(ConfigurationManager.AppSettings["path:media:physical"], path);
                    }

                    return null;
                }
            }
        }

        public string GetMediaPathRelative(Guid id)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select @path = Url from UrlMap where id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.Add(new SqlParameter("@path", SqlDbType.VarChar, 64) { Direction = ParameterDirection.Output });

                    command.ExecuteNonQuery();

                    var path = command.Parameters["@path"].Value == DBNull.Value ? null : (string)command.Parameters["@path"].Value;
                    
                    if (path != null)
                    {
                        return string.Format("{0}/{1}/{2}", ConfigurationManager.AppSettings["path:root:relative"], "content/media", path);
                    }

                    return null;
                }
            }
        }

        #endregion

        #region Private Methods

        public IEnumerable<Document> GetWithStatus(bool isActive, out int total, int skip, int take)
        {
            using (var connection = GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select @count = count(*) from Document where IsActive = @isActive";
                    command.Parameters.AddWithValue("@isActive", isActive);
                    command.Parameters.Add(new SqlParameter("@count", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    command.ExecuteNonQuery();
                    total = (int)command.Parameters["@count"].Value;
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"with cte as ( 
select 
    D.Id, 
    D.ParentId,
    D.RedirectId,
    D.Name,
    D.Synopsis,
    D.Content,
    D.UrlKey,
    D.Url,
    D.IsActive,
    D.Created,
    D.Modified,
    row_number() over (order by D.Created desc) as __RN
from 
    Document D
where 
    D.IsActive = @isActive )

select
    Id, 
    ParentId,
    RedirectId,
    Name,
    Synopsis,
    Content,
    UrlKey,
    Url,
    IsActive,
    Created,
    Modified
from 
    cte 
where
    __RN between @skip and (@skip + @take)";

                    command.Parameters.AddWithValue("@skip", skip);
                    command.Parameters.AddWithValue("@take", take);
                    command.Parameters.AddWithValue("@isActive", isActive);

                    using (var reader = command.ExecuteReader())
                    {
                        var result = new List<Document>();
                        while (reader.Read())
                        {
                            result.Add(Document.FromReader(reader));
                        }

                        return result.ToArray();
                    }
                }
            }
        }

        IEnumerable<string> GetImages(Guid id)
        {
            var path = GetMediaPathPhysical(id);
            var pathRelative = GetMediaPathRelative(id);
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path).Select(f => pathRelative + "/" + Path.GetFileName(f));
            }

            return new string[0];
        }

        IEnumerable<NamedEntity> GetCategories(IDbConnection connection, Guid id)
        {
            const string Sql = "select C.[Id], C.[Name] from [DocumentCategory] DC inner join [Category] C on (DC.CategoryId = C.[Id]) where DC.[DocumentId] = @id";

            return connection.Query<NamedEntity>(Sql, new { id = id });
        }

        #endregion
    }
}
