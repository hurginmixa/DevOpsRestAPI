using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CommonCode
{
    public static class WITools
    {
        public static string AddSlash(string path)
        {
            if (string.IsNullOrEmpty(path) || path.EndsWith("\\"))
            {
                return path;
            }

            return path + '\\';
        }

        public static string WrapEmptyString(string src)
        {
            return !string.IsNullOrEmpty(src) ? src : "Empty string";
        }
    }

    public struct IniFileValue
    {
        public IniFileValue(string section, string key, string value)
        {
            Inited = true;
            Section = section;
            Key = key;
            Value = value;
        }

        public readonly bool Inited;
        public readonly string Section;
        public readonly string Key;
        public readonly string Value;
    }

    public static class IniFileHelper
    {
        #region Dll's Imports

        [DllImport("kernel32.dll")]
        public static extern void OutputDebugString(string lpOutputString);

        [DllImport("kernel32", SetLastError = false)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault,
                                                          StringBuilder lpReturnedString, uint nSize, string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString,
                                                             string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern int GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, int nSize, string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, int nSize,
                                                           string lpFileName);

        [DllImport("kernel32.dll")]
        public static extern bool WritePrivateProfileSection(string lpAppName, string sectionText, string lpFileName);

        #endregion

        public static int GetIni(string sectionName, string variableName, string fileName, int defalutValue)
        {
            CheckIni(sectionName, variableName, fileName, ref defalutValue);

            return defalutValue;
        }

        public static int GetIni(string sectionName, string variableName, string fileName, Func<int> defalutValue)
        {
            int value = 0;
            if (CheckIni(sectionName, variableName, fileName, ref value))
            {
                return value;
            }

            return defalutValue();
        }

        public static bool CheckIni(string sectionName, string variableName, string fileName, ref int value)
        {
            try
            {
                string valueString = value.ToString();
                if (!CheckIni(sectionName, variableName, fileName, ref valueString))
                {
                    return false;
                }

                if (!int.TryParse(valueString, out value))
                {
                    throw new Exception("The '" + WITools.WrapEmptyString(valueString) + "' is not valid int value");
                }

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]{2}", fileName, sectionName, variableName), e);
            }
        }

        public static double GetIni(string sectionName, string variableName, string fileName, double defalutValue)
        {
            CheckIni(sectionName, variableName, fileName, ref defalutValue);

            return defalutValue;
        }

        public static bool CheckIni(string sectionName, string variableName, string fileName, ref double value)
        {
            try
            {
                string valueString = value.ToString();
                if (!CheckIni(sectionName, variableName, fileName, ref valueString))
                {
                    return false;
                }

                if (!double.TryParse(valueString, out value))
                {
                    throw new Exception(string.Format("The '{0}' is not valid double value",
                        WITools.WrapEmptyString(valueString)));
                }

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]{2}", fileName, sectionName, variableName), e);
            }
        }

        public static bool GetIni(string sectionName, string variableName, string fileName, bool defalutValue)
        {
            CheckIni(sectionName, variableName, fileName, ref defalutValue);

            return defalutValue;
        }

        public static bool GetIni(string sectionName, string variableName, string fileName, Func<bool> defalutValue)
        {
            bool value = false;
            if (CheckIni(sectionName, variableName, fileName, ref value))
            {
                return value;
            }

            return defalutValue();
        }

        public static bool CheckIni(string sectionName, string variableName, string fileName, ref bool value)
        {
            try
            {
                string valueString = value ? "1" : "0";
                if (!CheckIni(sectionName, variableName, fileName, ref valueString))
                {
                    return false;
                }

                value = String2Bool(valueString);

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]{2}", fileName, sectionName, variableName), e);
            }
        }

        public static bool String2Bool(string str)
        {
            if (str.Length == 1)
            {
                return str != "0";
            }

            bool value;
            if (!bool.TryParse(str.Trim(), out value))
            {
                throw new Exception("The '" + WITools.WrapEmptyString(str) + "' is not valid bool value");
            }

            return value;
        }

        public static void PutIni<T>(string sectionName, string variableName, string fileName, T value)
        {
            try
            {
                PutIni(sectionName, variableName, fileName, value.ToString());
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]{2}", fileName, sectionName, variableName), e);
            }
        }

        public static void PutIni(string sectionName, string variableName, string fileName, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    WritePrivateProfileString(sectionName, variableName, string.Empty, fileName);
                    return;
                }

                string valueToCheck = value;
                bool isExist = CheckIni(sectionName: sectionName, variableName: variableName, fileName: fileName, value: ref valueToCheck);

                if (!isExist || (value != valueToCheck))
                {
                    WritePrivateProfileString(sectionName, variableName, value, fileName);
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]{2}", fileName, sectionName, variableName), e);
            }
        }

        public static void PutIni(string sectionName, string variableName, string fileName, bool value)
        {
            try
            {
                PutIni(sectionName, variableName, fileName, value ? "1" : "0");
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]{2}", fileName, sectionName, variableName), e);
            }
        }

        public static string GetIni(string sectionName, string variableName, string fileName, string defaultValue)
        {
            try
            {
                CheckIni(sectionName, variableName, fileName, ref defaultValue);
                return defaultValue;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]{2}", fileName, sectionName, variableName), e);
            }
        }

        public static T GetEnumIni<T>(string sectionName, string variableName, string fileName, T defalutValue)
            where T : struct
        {
            try
            {
                string defaultValueStr = defalutValue.ToString();
                CheckIni(sectionName, variableName, fileName, ref defaultValueStr);

                T res;
                return Enum.TryParse(value: defaultValueStr, ignoreCase: true, result: out res) ? res : defalutValue;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]{2}", fileName, sectionName, variableName), e);
            }
        }

        public static void Deleteini(string sectionName, string variableName, string fileName)
        {
            try
            {
                WritePrivateProfileString(sectionName, variableName, null, fileName);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]{2}", fileName, sectionName, variableName), e);
            }
        }

        public static void DeleteSection(string sectionName, string fileName)
        {
            try
            {
                WritePrivateProfileSection(sectionName, null, fileName);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]", fileName, sectionName), e);
            }
        }

        public static List<string> GetSectionNamesList(string fileName)
        {
            try
            {
                byte[] buffer = new byte[1024 * 1024];
                int lenSectionNames;

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    IntPtr buffPtr = handle.AddrOfPinnedObject();
                    lenSectionNames = GetPrivateProfileSectionNames(buffPtr, buffer.Length, fileName);
                }
                finally
                {
                    handle.Free();
                }

                return ParseZerrosStrings(buffer, lenSectionNames);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0} file", fileName), e);
            }
        }

        public static List<string> GetPrivateProfileSection(string sectionName, string fileName)
        {
            try
            {
                byte[] buffer = new byte[1024 * 1024];
                int bytesReturned;

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    IntPtr buffPtr = handle.AddrOfPinnedObject();
                    bytesReturned = GetPrivateProfileSection(sectionName, buffPtr, buffer.Length, fileName);
                }
                finally
                {
                    handle.Free();
                }

                return ParseZerrosStrings(buffer, bytesReturned);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]", fileName, sectionName), e);
            }
        }

        public static List<IniFileValue> GetIniFileValues(string fileName)
        {
            List<IniFileValue> retValue = new List<IniFileValue>();

            List<string> sectionNamesList = GetSectionNamesList(fileName);
            foreach (string sectionName in sectionNamesList)
            {
                retValue.AddRange(GetIniFileSectionValues(fileName, sectionName));
            }

            return retValue;
        }

        public static List<IniFileValue> GetIniFileSectionValues(string fileName, string sectionName)
        {
            List<IniFileValue> retValue = new List<IniFileValue>();

            foreach (string param in GetPrivateProfileSectionParams(sectionName, fileName))
            {
                string value = GetIni(sectionName, param, fileName, string.Empty);
                IniFileValue iniFileValue = new IniFileValue(sectionName, param, value);

                retValue.Add(iniFileValue);
            }

            return retValue;
        }

        private delegate string Parsing(string src);

        private static List<string> GetPrivateProfileSection(string sectionName, string fileName, Parsing parsing)
        {
            List<string> rawRows = GetPrivateProfileSection(sectionName, fileName);
            List<string> retValue = new List<string>();

            foreach (string row in rawRows)
            {
                string item = parsing(row);
                if (!string.IsNullOrEmpty(item))
                {
                    retValue.Add(item);
                }
            }

            return retValue;
        }

        public static List<string> GetPrivateProfileSectionParams(string sectionName, string fileName)
        {
            Parsing parsing = delegate (string row)
                                  {
                                      TokenList tokens = ParseString.Parse(row, '=');
                                      return tokens[0];
                                  };

            return GetPrivateProfileSection(sectionName, fileName, parsing);
        }

        public static List<string> GetPrivateProfileSectionValues(string sectionName, string fileName)
        {
            Parsing parsing = delegate (string row)
                                  {
                                      TokenList tokens = ParseString.Parse(row, '=');
                                      return tokens.Length > 1 ? tokens[1] : string.Empty;
                                  };

            return GetPrivateProfileSection(sectionName, fileName, parsing);
        }

        public static void PutIniFileValues(List<IniFileValue> iniFileValues, string fileName)
        {
            try
            {
                iniFileValues.ForEach(iniFileValue => PutIni(iniFileValue.Section, iniFileValue.Key, fileName, iniFileValue.Value));
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]", fileName, iniFileValues), e);
            }

        }

        private static List<string> ParseZerrosStrings(byte[] buffer, int len)
        {
            List<string> retValue = new List<string>();

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < len; ++i)
            {
                if (buffer[i] == 0)
                {
                    if (sb.Length > 0)
                    {
                        retValue.Add(sb.ToString());
                        sb.Length = 0;
                    }
                }
                else
                {
                    sb.Append((char)buffer[i]);
                }
            }

            return retValue;
        }

        public static bool CheckIni(string sectionName, string variableName, string fileName, ref string value)
        {
            return IniFileOperations.CheckIni(sectionName, variableName, fileName, ref value);
        }

        public static string CheckIni2(string sectionName, string variableName, string fileName, string defaultValue)
        {
            string value = string.Empty;
            if (!CheckIni(sectionName, variableName, fileName, ref value))
            {
                PutIni(sectionName, variableName, fileName, defaultValue);
                CheckIni(sectionName, variableName, fileName, ref value);
            }
            return value;
        }
        public static int CheckIni2(string sectionName, string variableName, string fileName, int defaultValue)
        {
            int value = 0;
            if (!CheckIni(sectionName, variableName, fileName, ref value))
            {
                PutIni(sectionName, variableName, fileName, defaultValue);
                CheckIni(sectionName, variableName, fileName, ref value);
            }
            return value;
        }
        public static double CheckIni2(string sectionName, string variableName, string fileName, double defaultValue)
        {
            double value = 0.0;
            if (!CheckIni(sectionName, variableName, fileName, ref value))
            {
                PutIni(sectionName, variableName, fileName, defaultValue);
                CheckIni(sectionName, variableName, fileName, ref value);
            }
            return value;
        }
        public static bool CheckIni2(string sectionName, string variableName, string fileName, bool defaultValue)
        {
            bool value = false;
            if (!CheckIni(sectionName, variableName, fileName, ref value))
            {
                PutIni(sectionName, variableName, fileName, defaultValue);
                CheckIni(sectionName, variableName, fileName, ref value);
            }
            return value;
        }
    }
}