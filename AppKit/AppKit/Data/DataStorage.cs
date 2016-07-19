namespace AdMaiora.AppKit.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
   
    using SQLite.Net;
    using SQLite.Net.Interop;

    using AdMaiora.AppKit.IO;

    public class DataStorage
    {
        #region Constants and Fields

        private ISQLitePlatform _sqlitePlatform;

        private FileUri _storageUri;

        private bool _isTransaction;
        private SQLiteConnection _sqlConnection;

        private string _lock = "DatabaseLock";

        #endregion

        #region Constructors

        public DataStorage(ISQLitePlatform sqlitePlatform, FileUri storageUri)
            : this(sqlitePlatform, storageUri, false)
        {
        }

        private DataStorage(ISQLitePlatform sqlitePlatform, FileUri storageUri, bool isTransaction)
        {
            _sqlitePlatform = sqlitePlatform;

            _storageUri = storageUri;
            _isTransaction = isTransaction;
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

        public List<TEntity> FindAll<TEntity>() where TEntity : class, new()
        {
            List<TEntity> entities = null;
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                entities = c.Table<TEntity>().ToList();
            });

            return entities.Count > 0 ? entities : null;
        }

        public List<TEntity> FindAll<TEntity>(Func<TEntity, bool> selector) where TEntity : class, new()
        {
            List<TEntity> entities = null;
            InSqlContext(c =>
            {
                EnsureTable(c, typeof(TEntity));
                entities = new List<TEntity>(c.Table<TEntity>().Where(selector));
            });

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
                var entities = FindAll<TEntity>();
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
                DataStorage transaction = new DataStorage(_sqlitePlatform, _storageUri, true);

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
                connection.MigrateTable(type);
            }
        }

        private SQLiteConnection GetSqlConnection()
        {
            if (_sqlConnection == null)
                _sqlConnection = new SQLiteConnection(_sqlitePlatform, this.StorageUri.AbsolutePath, false);

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

        #endregion
    }
}