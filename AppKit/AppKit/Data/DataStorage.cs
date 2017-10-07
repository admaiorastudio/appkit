namespace AdMaiora.AppKit.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;

    using SQLite;

    using AdMaiora.AppKit.IO;

    public enum OrderByDirection
    {
        Asc,
        Desc,
    }

    public class DataStorage
    {
        #region Constants and Fields

        private IDataStoragePlatform _dataStoragePlatform;

        private FileUri _storageUri;

        private bool _isTransaction;
        private SQLiteConnection _sqlConnection;

        private string _lock = "DatabaseLock";

        private static bool NeedsInit = true;

        #endregion

        #region Constructors

        public DataStorage(IDataStoragePlatform dataStoragePlatform, FileUri storageUri)
            : this(dataStoragePlatform, storageUri, false)
        {
        }

        private DataStorage(IDataStoragePlatform dataStoragePlatform, FileUri storageUri, bool isTransaction)
        {
            _dataStoragePlatform = dataStoragePlatform;

            _storageUri = storageUri;
            _isTransaction = isTransaction; 
            
            if(DataStorage.NeedsInit)
            {
                DataStorage.NeedsInit = false;
                SQLitePCL.Batteries.Init();
                SQLitePCL.raw.SetProvider(_dataStoragePlatform.GetSQLiteProvider());
            }           
        }

        #endregion

        #region Properties

        public FileUri StorageUri
        {
            get
            {
                if (_storageUri == null)
                    throw new InvalidOperationException("You must set a valid storage file uri.");

                return _storageUri;
            }
            set
            {
                _storageUri = value;
            }
        }

        #endregion

        #region Public Methods

        public void InsertAll<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, new()
        {
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                c.InsertAll(entities, true);
            });
        }

        public void Insert<TEntity>(TEntity entity) where TEntity : class, new()
        {
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                c.Insert(entity);
            });
        }

        public TEntity GetById<TEntity>(int objectId) where TEntity : class, new()
        {
            TEntity entity = null;
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                entity = c.Get<TEntity>(objectId);
            });

            return entity;
        }

        public List<TEntity> GetAll<TEntity>() where TEntity : class, new()
        {
            List<TEntity> entities = null;
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                entities = c.Table<TEntity>().ToList();
            });

            return entities.Count > 0 ? entities : null;
        }

        public List<TEntity> GetAll<TEntity>(int pageIndex, int pageSize, out int totalItems) where TEntity : class, new()
        {
            int count = 0;
            List<TEntity> entities = null;
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                count = c.Table<TEntity>().Count();
                entities = c.Table<TEntity>().Skip(pageIndex * pageSize).Take(pageSize).ToList();
            });

            totalItems = count;
            return entities.Count > 0 ? entities : null;
        }

        public List<TEntity> FindAll<TEntity>(Expression<Func<TEntity, bool>> selector) where TEntity : class, new()
        {
            List<TEntity> entities = null;
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                entities = new List<TEntity>(c.Table<TEntity>().Where(selector));
            });

            return entities.Count > 0 ? entities : null;
        }

        public List<TEntity> FindAll<TEntity>(Expression<Func<TEntity, bool>> selector, Expression<Func<TEntity, object>> orderer, OrderByDirection od) where TEntity : class, new()
        {
            List<TEntity> entities = null;
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                var query = c.Table<TEntity>().Where(selector);
                if (od == OrderByDirection.Asc)
                    query = query.OrderBy(orderer);
                else
                    query = query.OrderByDescending(orderer);
                entities = new List<TEntity>(query);
            });

            return entities.Count > 0 ? entities : null;
        }

        public List<TEntity> FindAll<TEntity>(Func<TEntity, bool> selector, int pageIndex, int pageSize, out int totalItems) where TEntity : class, new()
        {
            int count = 0;
            List<TEntity> entities = null;
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                count = c.Table<TEntity>().Where(selector).Count();
                entities = new List<TEntity>(c.Table<TEntity>().Where(selector).Skip(pageIndex * pageSize).Take(pageSize));
            });

            totalItems = count;
            return entities.Count > 0 ? entities : null;
        }

        public List<TEntity> FindAll<TEntity>(Expression<Func<TEntity, bool>> selector, Expression<Func<TEntity, object>> orderer, OrderByDirection od, int pageIndex, int pageSize, out int totalItems) where TEntity : class, new()
        {
            int count = 0;
            List<TEntity> entities = null;
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                count = c.Table<TEntity>().Where(selector).Count();
                var query = c.Table<TEntity>().Where(selector);
                if (od == OrderByDirection.Asc)
                    query = query.OrderBy(orderer);
                else
                    query = query.OrderByDescending(orderer);
                entities = new List<TEntity>(query.Skip(pageIndex * pageSize).Take(pageSize));
            });

            totalItems = count;
            return entities.Count > 0 ? entities : null;
        }

        public List<TEntity> FindByQuery<TEntity>(string query, params object[] args) where TEntity : class, new()
        {
            List<TEntity> entities = null;
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                entities = c.Query<TEntity>(query, args);
            });

            return entities.Count > 0 ? entities : null;
        }

        public List<TEntity> FindByQuery<TEntity>(string query, object[] args, int pageIndex, int pageSize, out int totalItems) where TEntity : class, new()
        {
            int count = 0;
            List<TEntity> entities = null;
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));

                count = c.ExecuteScalar<int>(query.Replace("*", "COUNT()"), args);
                args = (new List<object>(args)).Concat(new object[] { pageSize, pageIndex * pageSize }).ToArray();
                entities = c.Query<TEntity>(String.Concat(query, " LIMIT ? OFFSET ?"), args);
            });

            totalItems = count;
            return entities.Count > 0 ? entities : null;
        }

        public TEntity FindSingle<TEntity>(Expression<Func<TEntity, bool>> selector) where TEntity : class, new()
        {
            TEntity entity = null;
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                entity = c.Find<TEntity>(selector);
            });

            return entity;
        }

        public void UpdateAll<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, new()
        {
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                c.UpdateAll(entities, true);
            });
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class, new()
        {
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                c.Update(entity, typeof(TEntity));
            });
        }

        public void DeleteAll<TEntity>() where TEntity : class, new()
        {
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                c.DeleteAll<TEntity>();
            });
        }

        public void DeleteAll<TEntity>(Func<TEntity, bool> selector) where TEntity : class, new()
        {
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                var entities = c.Table<TEntity>().Where(selector).ToList();
                if (entities != null
                    && entities.Count > 0)
                {
                    foreach (TEntity entity in entities.Where(selector))
                        Delete<TEntity>(entity);
                }
            });
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class, new()
        {
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                c.Delete(entity);
            });
        }

        public void RunInTransaction(CancellationToken ct, Action<DataStorage> action)
        {
            lock (_lock)
            {
                DataStorage transaction = new DataStorage(_dataStoragePlatform, _storageUri, true);

                if (action != null)
                {
                    if (!ct.IsCancellationRequested)
                    {
                        transaction.BeginTransaction();

                        try
                        {
                            action(transaction);

                            if (ct.IsCancellationRequested)
                                throw new InvalidOperationException("Transaction abroted. Rolling back.");

                            transaction.CommitTransaction();
                        }
                        catch (Exception ex)
                        {
                            transaction.RollbackTransaction();

                            throw ex;
                        }
                    }
                }
            }
        }

        public void RunInTransaction(Action<DataStorage> action)
        {
            RunInTransaction(CancellationToken.None, action);
        }

        public SQLiteCommand CreateCommand(string sqlCommand, params object[] args)
        {
            return GetSqlConnection().CreateCommand(sqlCommand, args);
        }

        public void ExecuteNonQueryCommand(SQLiteCommand cmd)
        {
            InSqlContext(c =>
            {
                cmd.ExecuteNonQuery();
            });
        }

        public TValue ExecuteScalarCommand<TValue>(SQLiteCommand cmd)
        {
            TValue value = default(TValue);
            InSqlContext(c =>
            {
                value = cmd.ExecuteScalar<TValue>();
            });

            return value;
        }

        public bool TableExists<TEntity>()
        {
            bool exists = false;
            InSqlContext(c =>
            {
                var cmd = c.CreateCommand("SELECT name FROM sqlite_master WHERE type='table' AND name=?", typeof(TEntity).Name);
                exists = cmd.ExecuteScalar<string>() != null;
            });

            return exists;
        }

        #endregion

        #region Methods

        private void InSqlContext(Action<SQLiteConnection> action)
        {
            if (!_isTransaction)
            {
                lock (_lock)
                {
                    if (action != null)
                        action(GetSqlConnection());
                }
            }
            else
            {
                if (action != null)
                    action(GetSqlConnection());
            }
        }

        private SQLiteConnection GetSqlConnection()
        {
            if (_sqlConnection == null)
                _sqlConnection = new SQLiteConnection(this.StorageUri.AbsolutePath, false);

            return _sqlConnection;
        }

        private void BeginTransaction()
        {
            var connection = GetSqlConnection();
            if (connection.IsInTransaction)
                throw new InvalidOperationException("Transaction overlap. Did you miss to call rollback?");

            if (_isTransaction)
                connection.BeginTransaction();
        }

        private void CommitTransaction()
        {
            var connection = GetSqlConnection();
            if (_isTransaction)
                connection.Commit();
        }

        private void RollbackTransaction()
        {
            var connection = GetSqlConnection();
            if (_isTransaction)
                connection.Rollback();
        }

        private void EnsureTable(SQLiteConnection connection, Type type)
        {
            string tableName = type.Name;
            var existingCols = connection.GetTableInfo(tableName);
            if (existingCols.Count == 0)
            {
                connection.CreateTable(type);
            }
            else
            {
                // Do we need to update the schema? 
                // This method will automatically check table schema against entity fields
                MigrateTable(connection, type);
            }
        }

        private void MigrateTable(SQLiteConnection connection, Type ty)
        {
            var map = connection.GetMapping(ty);
            var existingCols = connection.GetTableInfo(map.TableName);

            var toBeAdded = new List<TableMapping.Column>();

            foreach (var p in map.Columns)
            {
                var found = false;
                foreach (var c in existingCols)
                {
                    found = (string.Compare(p.Name, c.Name, StringComparison.OrdinalIgnoreCase) == 0);
                    if (found)
                    {
                        break;
                    }
                }
                if (!found)
                {
                    toBeAdded.Add(p);
                }
            }

            foreach (var p in toBeAdded)
            {
                var addCol = "alter table \"" + map.TableName + "\" add column " +
                             Orm.SqlDecl(p, connection.StoreDateTimeAsTicks);

                connection.Execute(addCol);
            }
        }

        #endregion
    }
}