﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Bluscream {
    internal static class Extensions {
        #region DateTime
        public static bool ExpiredSince(this DateTime dateTime, int minutes) {
            return (dateTime - DateTime.Now).TotalMinutes < minutes;
        }
        public static TimeSpan StripMilliseconds(this TimeSpan time) {
            return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
        }
        #endregion
        #region FileInfo
        public static DirectoryInfo Combine(this DirectoryInfo dir, params string[] paths) {
            string final = dir.FullName;
            foreach (string path in paths) {
                final = Path.Combine(final, path);
            }
            return new DirectoryInfo(final);
        }
        public static FileInfo Combine(this FileInfo file, params string[] paths) {
            string final = file.DirectoryName;
            foreach (string path in paths) {
                final = Path.Combine(final, path);
            }
            return new FileInfo(final);
        }
        public static string FileNameWithoutExtension(this FileInfo file) {
            return Path.GetFileNameWithoutExtension(file.Name);
        }
        /*public static string Extension(this FileInfo file) {
            return Path.GetExtension(file.Name);
        }*/
        public static void AppendLine(this FileInfo file, string line) {
            try {
                if (!file.Exists) {
                    _ = file.Create();
                }

                File.AppendAllLines(file.FullName, new string[] { line });
            } catch { }
        }

        public static string ReadAllText(this FileInfo file) {
            return File.ReadAllText(file.FullName);
        }

        public static List<string> ReadAllLines(this FileInfo file) {
            return File.ReadAllLines(file.FullName).ToList();
        }
        #endregion
        #region UI
        #endregion
        #region Object
        public static object ToJson(this object obj, bool indented = true) {
            return JsonConvert.SerializeObject(obj, indented ? Formatting.Indented : Formatting.None, new JsonConverter[] { new StringEnumConverter() });
        }
        #endregion
        #region String
        public static bool Contains(this string source, string toCheck, StringComparison comp) {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
        public static bool IsNullOrEmpty(this string source) {
            return string.IsNullOrEmpty(source);
        }
        public static string[] Split(this string source, string split, int count = -1, StringSplitOptions options = StringSplitOptions.None) {
            return count != -1 ? source.Split(new string[] { split }, count, options) : source.Split(new string[] { split }, options);
        }
        public static string Remove(this string Source, string Replace) {
            return Source.Replace(Replace, string.Empty);
        }
        public static string ReplaceLastOccurrence(this string Source, string Find, string Replace) {
            int place = Source.LastIndexOf(Find);
            if (place == -1) {
                return Source;
            }

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }
        public static string Ext(this string text, string extension) {
            return text + "." + extension;
        }
        public static string Quote(this string text) {
            return SurroundWith(text, "\"");
        }
        public static string Enclose(this string text) {
            return SurroundWith(text, "(", ")");
        }
        public static string Brackets(this string text) {
            return SurroundWith(text, "[", "]");
        }
        public static string SurroundWith(this string text, string surrounds) {
            return surrounds + text + surrounds;
        }
        public static string SurroundWith(this string text, string starts, string ends) {
            return starts + text + ends;
        }
        #endregion
        #region Dict
        public static void AddSafe(this IDictionary<string, string> dictionary, string key, string value) {
            if (!dictionary.ContainsKey(key)) {
                dictionary.Add(key, value);
            }
        }
        #endregion
        #region List
        public static string ToQueryString(this NameValueCollection nvc) {
            if (nvc == null) {
                return string.Empty;
            }

            StringBuilder sb = new();

            foreach (string key in nvc.Keys) {
                if (string.IsNullOrWhiteSpace(key)) {
                    continue;
                }

                string[] values = nvc.GetValues(key);
                if (values == null) {
                    continue;
                }

                foreach (string value in values) {
                    _ = sb.Append(sb.Length == 0 ? "?" : "&");
                    _ = sb.AppendFormat("{0}={1}", key, value);
                }
            }

            return sb.ToString();
        }
        public static bool GetBool(this NameValueCollection collection, string key, bool defaultValue = false) {
            if (!collection.AllKeys.Contains(key, StringComparer.OrdinalIgnoreCase)) {
                return false;
            }

            string[] trueValues = new string[] { true.ToString(), "yes", "1" };
            if (trueValues.Contains(collection[key], StringComparer.OrdinalIgnoreCase)) {
                return true;
            }

            string[] falseValues = new string[] { false.ToString(), "no", "0" };
            return falseValues.Contains(collection[key], StringComparer.OrdinalIgnoreCase) || defaultValue;
        }
        public static string GetString(this NameValueCollection collection, string key) {
            return !collection.AllKeys.Contains(key) ? collection[key] : null;
        }
        public static T PopFirst<T>(this IEnumerable<T> list) {
            return list.ToList().PopAt(0);
        }

        public static T PopLast<T>(this IEnumerable<T> list) {
            return list.ToList().PopAt(list.Count() - 1);
        }

        public static T PopAt<T>(this List<T> list, int index) {
            T r = list.ElementAt<T>(index);
            list.RemoveAt(index);
            return r;
        }
        #endregion
        #region Uri
        private static readonly Regex QueryRegex = new(@"[?&](\w[\w.]*)=([^?&]+)");
        public static IReadOnlyDictionary<string, string> ParseQueryString(this Uri uri) {
            Match match = QueryRegex.Match(uri.PathAndQuery);
            Dictionary<string, string> paramaters = new();
            while (match.Success) {
                paramaters.Add(match.Groups[1].Value, match.Groups[2].Value);
                match = match.NextMatch();
            }
            return paramaters;
        }
        public static Dictionary<string, string> ParseQueryString(this string queryString) {
            NameValueCollection nvc = HttpUtility.ParseQueryString(queryString);
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }
        #endregion
        #region Enum
        public static string GetDescription(this Enum value) {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null) {
                FieldInfo field = type.GetField(name);
                if (field != null) {
                    if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr) {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
        public static T GetValueFromDescription<T>(string description, bool returnDefault = false) {
            Type type = typeof(T);
            if (!type.IsEnum) {
                throw new InvalidOperationException();
            }

            foreach (FieldInfo field in type.GetFields()) {
                if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute) {
                    if (attribute.Description == description) {
                        return (T)field.GetValue(null);
                    }
                } else {
                    if (field.Name == description) {
                        return (T)field.GetValue(null);
                    }
                }
            }
            return returnDefault ? default : throw new ArgumentException("Not found.", "description");
        }
        #endregion
        #region Task
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout) {
            using CancellationTokenSource timeoutCancellationTokenSource = new();
            Task completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task) {
                timeoutCancellationTokenSource.Cancel();
                return await task;  // Very important in order to propagate exceptions
            } else {
                return default;
            }
        }
        #endregion
    }
}