namespace AdMaiora.AppKit.Data
{
    using System;

    public interface IDataStoragePlatform
    {
        SQLitePCL.ISQLite3Provider GetSQLiteProvider();
    }
}