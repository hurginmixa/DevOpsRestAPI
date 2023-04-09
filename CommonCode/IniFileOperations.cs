using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CommonCode
{
    public class IniFileOperations
    {
        #region Dll's Imports

        [DllImport("kernel32.dll")]
        public static extern void OutputDebugString(string lpOutputString);

        [DllImport("kernel32", SetLastError = false)]
        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault,
                                                          StringBuilder lpReturnedString, uint nSize, string lpFileName);

        [DllImport("kernel32.dll")]
        public static extern uint GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString,
                                                             string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, int nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern bool WritePrivateProfileSection(string lpAppName, string lpString, string lpFileName);

        #endregion

        private readonly string myFileName;

        public IniFileOperations(string fileName)
        {
            myFileName = fileName;
        }

        public string FileName
        {
            get { return myFileName; }
        }

        //------------------------------------------------------------------------

        public string GetIni(string sectionName, string varName, string defaultValue)
        {
            CheckIni(sectionName, varName, ref defaultValue);
            return defaultValue;
        }

        public string GetIni(string sectionName, string varName, string defaultValue, out bool wasFound)
        {
            wasFound = CheckIni(sectionName, varName, ref defaultValue);
            return defaultValue;
        }

        public void SetIni(string sectionName, string varName, string value)
        {
            SetIni(sectionName, varName, myFileName, value);    
        }

        /// <summary>
        /// Updating value parameter on value from file if one exists in file. If value do not exist in file so default value will be saved
        /// </summary>
        /// <param name="sectionName">Section Name</param>
        /// <param name="name">Parameter name</param>
        /// <param name="value">default Value or retrun value</param>
        public void InitParam(string sectionName, string name, ref string value)
        {
            if (!CheckIni(sectionName, name, ref value))
            {
                SetIni(sectionName, name, value);
            }
        }

        public bool CheckIni(string sectionName, string varName, ref string value)
        {
            return CheckIni(sectionName, varName, myFileName, ref value);
        }

        //------------------------------------------------------------------------

        public bool GetIni(string sectionName, string varName, bool defaultValue)
        {
            CheckIni(sectionName, varName, ref defaultValue);
            return defaultValue;
        }

        public void SetIni(string sectionName, string varName, bool value)
        {
            SetIni(sectionName, varName, (value ? 1 : 0));
        }

        /// <summary>
        /// Updating value parameter on value from file if one exists in file. If value do not exist in file so default value will be saved
        /// </summary>
        /// <param name="sectionName">Section Name</param>
        /// <param name="name">Parameter name</param>
        /// <param name="value">default Value or retrun value</param>
        public void InitParam(string sectionName, string name, ref bool value)
        {
            if (!CheckIni(sectionName, name, ref value))
            {
                SetIni(sectionName, name, value);
            }
        }

        public bool CheckIni(string sectionName, string varName, ref bool value)
        {
            int v = value ? 1 : 0;
            bool wasFound = CheckIni(sectionName, varName, ref v);

            value = v != 0;
            return wasFound;
        }

        //------------------------------------------------------------------------

        public int GetIni(string sectionName, string varName, int defaultValue)
        {
            CheckIni(sectionName, varName, ref defaultValue);
            return defaultValue;
        }

        public int GetIni(string sectionName, string varName, int defaultValue, out bool wasFound)
        {
            wasFound = CheckIni(sectionName, varName, ref defaultValue);
            return defaultValue;
        }

        public void SetIni(string sectionName, string varName, int value)
        {
            SetIni(sectionName, varName, value.ToString());
        }

        /// <summary>
        /// Updating value parameter on value from file if one exists in file. If value do not exist in file so default value will be saved
        /// </summary>
        /// <param name="sectionName">Section Name</param>
        /// <param name="name">Parameter name</param>
        /// <param name="value">default Value or retrun value</param>
        public void InitParam(string sectionName, string name, ref int value)
        {
            if (!CheckIni(sectionName, name, ref value))
            {
                SetIni(sectionName, name, value);
            }
        }

        public bool CheckIni(string sectionName, string varName, ref int value)
        {
            string s = value.ToString();
            bool wasFound = CheckIni(sectionName, varName, ref s);

            if (!wasFound)
            {
                return false;
            }

            int v;
            value = int.TryParse(s, out v) ? v : value;
            return true;
        }

        //------------------------------------------------------------------------

        public T GetIni<T>(string sectionName, string varName, T defaultValue) where T : struct
        {
            CheckIni(sectionName, varName, ref defaultValue);
            return defaultValue;
        }

        public T GetIni<T>(string sectionName, string varName, T defaultValue, out bool wasFound) where T : struct
        {
            wasFound = CheckIni(sectionName, varName, ref defaultValue);
            return defaultValue;
        }

        public void SetIni<T>(string sectionName, string varName, T value)  where T : struct
        {
            SetIni(sectionName, varName, value.ToString());
        }

        /// <summary>
        /// Updating value parameter on value from file if one exists in file. If value do not exist in file so default value will be saved
        /// </summary>
        /// <param name="sectionName">Section Name</param>
        /// <param name="name">Parameter name</param>
        /// <param name="value">default Value or retrun value</param>
        public void InitParam<T>(string sectionName, string name, ref T value) where T : struct
        {
            if (!CheckIni(sectionName, name, ref value))
            {
                SetIni(sectionName, name, value);
            }
        }

        public bool CheckIni<T>(string sectionName, string varName, ref T value) where T : struct
        {
            string s = value.ToString();
            bool wasFound = CheckIni(sectionName, varName, ref s);

            if (!wasFound)
            {
                return false;
            }

            T v;
            value = Enum.TryParse<T>(s, out v) ? v : value;
            return true;
        }

        //------------------------------------------------------------------------

        public double GetIni(string sectionName, string varName, double defaultValue)
        {
            CheckIni(sectionName, varName, ref defaultValue);
            return defaultValue;
        }

        public void SetIni(string sectionName, string varName, double value)
        {
            SetIni(sectionName, varName, value.ToString("0.000"));
        }

        public void InitParam(string sectionName, string name, ref double value)
        {
            if (!CheckIni(sectionName, name, ref value))
            {
                SetIni(sectionName, name, value);
            }
        }

        public bool CheckIni(string sectionName, string varName, ref double value)
        {
            string s = value.ToString();
            bool wasFound = CheckIni(sectionName, varName, ref s);

            if (!wasFound)
            {
                return false;
            }

            double v;
            value = double.TryParse(s, out v) ? v : value;
            return true;
        }

        //------------------------------------------------------------------------

        public bool IsExists(string sectionName, string varName)
        {
            string v = string.Empty;
            return CheckIni(sectionName, varName, ref v);
        }

        //------------------------------------------------------------------------

        public string[] GetSection(string sectionName)
        {
            return GetSection(sectionName, myFileName);
        }

        public void DeleteSection(string sectionName)
        {
            SetSection(sectionName, string.Empty, myFileName);
        }

        public void SetSection(string sectionName, string[] values)
        {
            SetSection(sectionName, values, myFileName);
        }

        public void SetSection(string sectionName, string value)
        {
            SetSection(sectionName, value, myFileName);
        }

        public string [] GetSectionNames()
        {
            return GetSectionNames(myFileName);
        }

        //------------------------------------------------------------------------

        public List<IniFileValue> GetIniFileValues(string fileName)
        {
            return IniFileHelper.GetIniFileValues(fileName);
        }

        public void SetIniFileValues(List<IniFileValue> iniFileValues, string fileName)
        {
            IniFileHelper.PutIniFileValues(iniFileValues, fileName);
        }

        //------------------------------------------------------------------------
        public static bool CheckIni2(string sectionName, string variableName, string fileName, ref string value, string defaultValue)
        {
            if(!CheckIni(sectionName, variableName, fileName, ref value))
            {
                SetIni(sectionName, variableName, fileName, defaultValue);
                return CheckIni(sectionName, variableName, fileName, ref value);
            }
            return true;
        }

        public static bool CheckIni(string sectionName, string variableName, string fileName, ref string value)
        {
            return CheckIni(sectionName: sectionName, variableName: variableName, fileName: fileName, value: ref value, size: 1024 * 5);
        }

        public static bool CheckIni(string sectionName, string variableName, string fileName, ref string value, uint size)
        {
            try
            {
                StringBuilder sbld = new StringBuilder((int)size);
                string newValue;
                try
                {
                    GetPrivateProfileString(sectionName, variableName, string.Empty, sbld, (uint)sbld.Capacity, fileName);

                    newValue = sbld.ToString();
                }
                finally
                {
                    sbld.Clear();
                }

                if (string.IsNullOrEmpty(newValue))
                {
                    return false;
                }

                int semiCollonIndex = newValue.IndexOf(';');
                value = (semiCollonIndex >= 0 ? newValue.Substring(0, semiCollonIndex) : newValue).Trim(' ');
                value = value.TrimEnd('\t');
                value = value.TrimEnd(' ');

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]{2}", fileName, sectionName, variableName), e);
            }
        }

        public static void SetIni(string sectionName, string varName, string fileName, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    WritePrivateProfileString(sectionName, varName, value, fileName);
                    return;
                }

                string valueToCheck = value;
                bool isExist = CheckIni(sectionName: sectionName, variableName: varName, fileName: fileName, value: ref valueToCheck);

                if (!isExist || ( value != valueToCheck))
                {
                    WritePrivateProfileString(sectionName, varName, value, fileName);
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Exception at {0}:[{1}]{2}", fileName, sectionName, varName), e);
            }
        }

        public static string [] GetSectionNames(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return new string[0];
            }

            string[] sectionNames = new string[0];

            try
            {
                IntPtr pReturnedString = IntPtr.Zero;
                try
                {
                    const int maxBuffer = 32767;
                    pReturnedString = Marshal.AllocCoTaskMem(maxBuffer);

                    int bytesReturned = GetPrivateProfileSectionNames(pReturnedString, maxBuffer, fileName);
                    if ((bytesReturned != maxBuffer - 2) && (bytesReturned != 0))
                    {
                        //bytesReturned -1 to remove trailing \0
                        // NOTE: Calling Marshal.PtrToStringAuto(pReturnedString) will result in only the first pair being returned

                        string returnedString = Marshal.PtrToStringAuto(pReturnedString, bytesReturned - 1);
                        sectionNames = returnedString.Split('\0');
                    }
                }
                finally
                {
                    if (pReturnedString != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pReturnedString);
                    }
                }
            }
            catch
            {
                sectionNames = new string[0];
            }

            return sectionNames;
        }

        public static string[] GetSection(string sectionName, string fileName)
        {
            string[] section = new string[0];
            if (!File.Exists(fileName))
            {
                return new string[0];
            }

            IntPtr pReturnedString = IntPtr.Zero;

            try
            {
                try
                {
                    const int maxBuffer = 32767;
                    pReturnedString = Marshal.AllocCoTaskMem(maxBuffer);

                    int bytesReturned = GetPrivateProfileSection(sectionName, pReturnedString, maxBuffer, fileName);
                    if ((bytesReturned != maxBuffer - 2) && (bytesReturned != 0))
                    {
                        //bytesReturned -1 to remove trailing \0
                        // NOTE: Calling Marshal.PtrToStringAuto(pReturnedString) will result in only the first pair being returned

                        string returnedString = Marshal.PtrToStringAuto(pReturnedString, bytesReturned - 1);
                        section = returnedString.Split('\0');
                    }
                }
                finally
                {
                    if (pReturnedString != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(pReturnedString);
                    }
                }
            }
            catch
            {
                section = new string[0];
            }

            return section;
        }

        public static void SetSection(string sectionName, string[] valueArr, string fileName)
        {
            if (valueArr == null)
            {
                SetSection(sectionName, string.Empty, fileName);
                return;
            }

            SetSection(sectionName, string.Join(Environment.NewLine, valueArr), fileName);
        }

        public static void SetSection(string sectionName, string value, string fileName)
        {
            if (string.IsNullOrEmpty(value))
            {
                WritePrivateProfileSection(sectionName, null, fileName);
                return;
            }

            WritePrivateProfileSection(sectionName, value, fileName);
        }
    }
}