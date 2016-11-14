namespace AdMaiora.AppKit.Data
{
    using System;

    using Android.Content;
    using SQLitePCL;

    public class DataStoragePlatformAndroid : IDataStoragePlatform
    {
        public ISQLite3Provider GetSQLiteProvider()
        {
            return new SQLite3Provider_e_sqlite3();
        }
    }
}