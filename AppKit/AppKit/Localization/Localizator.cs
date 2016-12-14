namespace AdMaiora.AppKit.Localization
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Reflection;
    using System.Xml.Linq;
    using System.IO;

    using AdMaiora.AppKit.IO;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class LocalizationDictionaryAttribute : Attribute
    {
        public LocalizationDictionaryAttribute(string targetCulture, bool isDefault = false)
        {
            this.TargetCulture = targetCulture;
            this.IsDefault = isDefault;
        }

        public string TargetCulture
        {
            get;
        }

        public bool IsDefault
        {
            get;
        }
    }

    public abstract class LocalizationDictionary
    {
        #region Constants and Fields

        private Dictionary<string, string> _dictionary;

        #endregion

        #region Indexers

        public string this[string key]
        {
            get
            {
                return GetValue(key);
            }
        }

        #endregion

        #region Constructors

        public LocalizationDictionary()
        {
            _dictionary = new Dictionary<string, string>();
        }

        #endregion

        #region Properties

        internal Dictionary<string, string> Entries
        {
            get
            {
                return _dictionary;
            }
        }

        #endregion

        #region Public Methods

        public string GetValue(string key)
        {
            string value = key;
            _dictionary.TryGetValue(key, out value);

            return value ?? String.Format("[{0}]", key);
        }

        public LocalizationDictionary AddValue(string key, string value)
        {
            if (_dictionary.ContainsKey(key))
                throw new InvalidOperationException(String.Format("Dictionary already contains key '{0}'", key));

            _dictionary.Add(key, value);
            return this;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in _dictionary)
                sb.AppendLine(String.Format("{0}|{1}", kvp.Key, kvp.Value));

            return sb.ToString();
        }        

        #endregion
    }

    public class Localizator
    {
        #region Inner Localization Classes

        public static class Cultures
        {
            public const string ItalianIT = "it-IT";
            public const string EnglishUS = "en-US";
            public const string FrenchFR = "fr-FR";
            public const string GermanDE = "de-DE";
            public const string SpanishES = "es-ES";
        }

        #endregion

        #region Constants and Fields

        private string _culture;

        private CultureInfo _language;
        private CultureInfo _region;

        private LocalizationDictionary _dictionary;

        private ILocalizatorPlatform _localizatorPlatform;

        #endregion

        #region Indexers

        public string this[string key]
        {
            get
            {
                return GetString(key);
            }
        }

        #endregion

        #region Properties

        public string Culture
        {
            get
            {
                return _culture;
            }
        }

        public CultureInfo LanguageCulture
        {
            get
            {
                return _language;
            }
        }

        public CultureInfo RegionalCulture
        {
            get
            {
                return _region;
            }
        }

        public LocalizationDictionary Dictionary
        {
            get
            {
                return _dictionary;
            }
        }

        #endregion

        #region Constructors

        public Localizator(ILocalizatorPlatform localizatorPlatform, string culture = null)
        {
            _localizatorPlatform = localizatorPlatform;
            _culture = string.IsNullOrWhiteSpace(culture) ? _localizatorPlatform.GetDeviceUICulture() : culture;

            try
            {                
                _language = new CultureInfo(_culture);
                _region = new CultureInfo(_culture);
            }
            catch
            {
                // Something wrong here, usually it could be that the SO 
                // return a combined value for (country-region) like it-US 
                // that means country language IT and region formats US
                // In iOS user can specify customized country-region combination                

                // We need to get the "nearest" culture info for language and region

                CultureInfo[] allCultures = _localizatorPlatform.GetInstalledCultures();

                _language = allCultures.FirstOrDefault(ci => ci.TwoLetterISOLanguageName == _culture.Substring(0, 2));
                if (_language == null)
                    _language = CultureInfo.CurrentCulture;

                _region = allCultures.FirstOrDefault(ci => ci.Name.EndsWith(_culture.Substring(3, 2)));
                if (_region == null)
                    _region = CultureInfo.CurrentUICulture;
            }

            // Fetch all available localized dictionaries by culture
            Type defaultLocalizedDictionaryType = null;
            Dictionary<string, Type> localizedDictionaries = new Dictionary<string, Type>();            
            foreach (var a in _localizatorPlatform.GetAssemblies())
            {
                foreach (var t in a.DefinedTypes)
                {
                    if (t.BaseType == typeof(LocalizationDictionary))
                    {
                        var pa = (LocalizationDictionaryAttribute)t
                            .GetCustomAttributes(typeof(LocalizationDictionaryAttribute), true)
                            .FirstOrDefault();

                        if (pa != null)
                        {
                            localizedDictionaries.Add(pa.TargetCulture, t.AsType());

                            if (pa.IsDefault)
                                defaultLocalizedDictionaryType = t.AsType();
                        }
                    }
                }
            }
            
            // Try get localized dictionary type by designated culture
            Type localizedDictionaryType = null;
            localizedDictionaries.TryGetValue(_culture, out localizedDictionaryType);
            if (localizedDictionaryType == null)
            {
                // Try get localized dictionary type by nearest culture
                foreach(var kvp in localizedDictionaries)
                {
                    string targetCulture = kvp.Key;
                    if(targetCulture.StartsWith(_culture.Substring(0, 2)))
                    {
                        localizedDictionaryType = kvp.Value;
                        break;
                    }
                }                
            }

            _dictionary = 
                (LocalizationDictionary)Activator.CreateInstance(localizedDictionaryType ?? defaultLocalizedDictionaryType);
        }

        #endregion


        #region Public Methods

        public string GetString(string key)
        {
            return _dictionary[key];
        }

        public string GetString(string key, params object[] args)
        {
            return String.Format(GetString(key), args);
        }

        public string FormatCurrency(decimal value)
        {
            return value.ToString("C2", _region);
        }

        public string FormatDate(DateTime value, string format)
        {
            return value.ToString(format, _region);
        }

        public string FormatShortDate(DateTime value)
        {
            return value.ToString("d", _region);
        }

        public string FormatShortDateAndTime(DateTime value)
        {
            return value.ToString("g", _region);
        }

        public string FormatLongDate(DateTime value)
        {
            return value.ToString("D", _region);
        }

        public string FormatLongDateAndTime(DateTime value)
        {
            return value.ToString("f", _region);
        }

        public string FormatTime(DateTime value)
        {
            return value.ToString("t", _region);
        }

        public void GenerateXmlDictionaries(FolderUri outputUri)
        {
            var fs = _localizatorPlatform.GetFileSystem();
            if (!fs.FolderExists(outputUri))
                fs.CreateFolder(outputUri);

            foreach (var a in _localizatorPlatform.GetAssemblies())
            {
                foreach (var t in a.DefinedTypes)
                {
                    if (t.BaseType == typeof(LocalizationDictionary))
                    {
                        var pa = (LocalizationDictionaryAttribute)t
                            .GetCustomAttributes(typeof(LocalizationDictionaryAttribute), true)
                            .FirstOrDefault();

                        if (pa != null)
                        {
                            var dictionary =
                                (LocalizationDictionary)Activator.CreateInstance(t.AsType());

                            XDocument xdoc = new XDocument(
                                new XDeclaration("1.0", "UTF-8", "yes"),
                                new XElement("dictionary",
                                    new XAttribute("targetCulture", pa.TargetCulture),
                                    new XElement("entries",
                                        dictionary.Entries.Select(
                                            kvp =>
                                            {
                                                return new XElement("entry",
                                                    new XElement("key", kvp.Key),
                                                    new XElement("value", kvp.Value));
                                            })
                                            .ToArray())));

                            string dictionaryPath = String.Concat(Path.Combine(outputUri.Uri, pa.TargetCulture), ".xml");
                            FileUri dictionaryUri = _localizatorPlatform.GetFileSystem().CreateFileUri(dictionaryPath);
                            if (fs.FileExists(dictionaryUri))
                                fs.DeleteFile(dictionaryUri);

                            using (Stream s = fs.OpenFile(dictionaryUri, UniversalFileMode.CreateNew, UniversalFileAccess.Write))
                                xdoc.Save(s);
                        }
                    }
                }
            }

        }

        #endregion
    }
}