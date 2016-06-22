namespace AdMaiora.AppKit.Extensions
{
    using System;
    using System.Reflection;
    using System.Collections.Generic;

    public class FriendlyStringAttribute : Attribute
    {
        public FriendlyStringAttribute(string friendlyText)
        {
            this.FriendlyText = friendlyText;
        }

        public string FriendlyText
        {
            get;
            private set;
        }
    }

    public class FriendlyLocalizedStringAttribute : Attribute
    {
        public FriendlyLocalizedStringAttribute(string friendlyKey)
        {
            this.FriendlyKey = friendlyKey;
        }

        public string FriendlyKey
        {
            get;
            private set;
        }
    }

    public static class EnumExtension
    {
        public static string ToFriendlyString(this Enum value)
        {
            FieldInfo field = value.GetType().GetRuntimeField(value.ToString());
            var attribs = new List<object>(field.GetCustomAttributes(typeof(FriendlyStringAttribute), true));
            if (attribs.Count > 0)
                return ((FriendlyStringAttribute)attribs[0]).FriendlyText;

            return value.ToString();
        }

        public static string ToFriendlyLocalizedString(this Enum value, object localizer)
        {
            FieldInfo field = value.GetType().GetRuntimeField(value.ToString());
            var attribs = new List<object>(field.GetCustomAttributes(typeof(FriendlyLocalizedStringAttribute), true));
            if (attribs.Count > 0)
            {
                string key = ((FriendlyLocalizedStringAttribute)attribs[0]).FriendlyKey;
                foreach (var p in localizer.GetType().GetRuntimeProperties())
                {
                    if(p.GetIndexParameters().Length > 0)
                        return (string)p.GetValue(localizer, new[] { key });                    
                }
            }

            return value.ToString();
        }
    }
}
