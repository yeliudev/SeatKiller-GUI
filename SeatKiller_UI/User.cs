using System;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Collections;
using System.Text;
using System.Security.Cryptography;

namespace SeatKiller_UI
{
    public static class User
    {
        public static RegistryKey key = Registry.ClassesRoot;
        public static ArrayList users = new ArrayList();

        public static bool CreateSubKey()
        {
            try
            {
                RegistryKey sk = key.CreateSubKey(@"SeatKiller_UI");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                return false;
            }
        }

        public static bool SetValue(string username, string password)
        {
            try
            {
                RegistryKey SK_Key = key.OpenSubKey(@"SeatKiller_UI", true);
                SK_Key.SetValue(Encrypt(username), Encrypt(password));
                SK_Key.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                return false;
            }
        }

        public static bool GetKey()
        {
            try
            {
                users.Clear();
                RegistryKey SK_Key = key.OpenSubKey(@"SeatKiller_UI");
                foreach (string encrypted_username in SK_Key.GetValueNames())
                {
                    users.Add(Decrypt(encrypted_username));
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                return false;
            }
        }

        public static string GetValue(string username)
        {
            RegistryKey SK_Key = key.OpenSubKey(@"SeatKiller_UI");
            if (username == "")
            {
                return "";
            }
            else
            {
                return Decrypt(SK_Key.GetValue(Encrypt(username)).ToString());
            }
        }

        public static bool DeleteValue()
        {
            try
            {
                RegistryKey SK_Key = key.OpenSubKey(@"SeatKiller_UI", true);
                foreach (string username in SK_Key.GetValueNames())
                {
                    SK_Key.DeleteValue(username);
                }
                users.Clear();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                return false;
            }
        }

        public static string Encrypt(string encryptStr, string key = "goolhanrrylikeschocolateverymuch")
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(encryptStr);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string decryptStr, string key = "goolhanrrylikeschocolateverymuch")
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] toEncryptArray = Convert.FromBase64String(decryptStr);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }
    }
}
