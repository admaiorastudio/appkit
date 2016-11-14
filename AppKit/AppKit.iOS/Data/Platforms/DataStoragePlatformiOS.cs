namespace AdMaiora.AppKit.Data
{
    using System;

    using Foundation;
    using SQLitePCL;

    public class DataStoragePlatformiOS : IDataStoragePlatform
    {
        public ISQLite3Provider GetSQLiteProvider()
        {
            return new SQLite3Provider_sqlite3();
        }
    }
}