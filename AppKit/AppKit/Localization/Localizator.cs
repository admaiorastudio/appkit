namespace AdMaiora.AppKit.Localization
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Reflection;

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

        #region Public Methods

        public string GetValue(string key)
        {
            string value = key;
            _dictionary.TryGetValue(key, out value);

            return value ?? key;
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

            _culture = string.IsNullOrWhiteSpace(culture) ? _localizatorPlatform.GetDeviceCulture() : culture;

            Type type = null;

            foreach (var a in _localizatorPlatform.GetAssemblies())
            {
                foreach (var t in a.DefinedTypes)
                {
                    if (t.BaseType == typeof(LocalizationDictionary))
                    {
                        var pa = (LocalizationDictionaryAttribute)t
                            .GetCustomAttributes(typeof(LocalizationDictionaryAttribute), true)
                            .FirstOrDefault();

                        if (pa.TargetCulture == _culture || (type == null && pa.IsDefault))
                            type = t.AsType();
                    }
                }
            }

            if (type == null)
            {
                throw new Exception("Missing LocalizationDictionary Attribute");
            }

            _dictionary = (LocalizationDictionary)Activator.CreateInstance(type);
        }

        #endregion

        #region Public Static Methods

        public static string GetLanguageCode(string culture)
        {
            if (String.IsNullOrWhiteSpace(culture))
                return null;

            var c = culture.Split('-');
            return c[c.Length - 1];
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

        public string FormatCurrency(decimal value, string culture = null)
        {
            return value.ToString("C2", new CultureInfo(culture ?? _culture));
        }

        public string FormatDate(DateTime value, string culture = null)
        {
            return value.ToString("d", new CultureInfo(culture ?? _culture));
        }

        public string FormatTime(DateTime value, string culture = null)
        {
            return value.ToString("t", new CultureInfo(culture ?? _culture));
        }

        #endregion
    }
}