using System;

namespace CryptogramLibrary
{
  internal class Storage
  {
    public static string SaveObject(object obj, string key)
    {
      key = KeyToNameFile(key);
      string extension = null;
      extension = ".xml";

      var subDir = obj.GetType().FullName;
      if (subDir.Contains("Version="))
      {
        subDir = obj.GetType().Namespace + "+" + obj.GetType().Name;
      }

      if (key.Length > 255)
      {
        throw new ArgumentException("File name too long", "");
      }
      else
      {
        Serialize(obj, subDir + "." + key + extension);
      }

      return key;
    }

    public static object LoadObject(Type type, string key)
    {
      object obj = null;
      key = KeyToNameFile(key);
      string extension = null;
      extension = ".xml";
      var subDir = type.FullName;
      if (subDir.Contains("Version="))
      {
        subDir = type.Namespace + "+" + type.Name;
      }
      try
      {
        obj = Deserialize(subDir + "." + key + extension, type);
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.Print(ex.Message);
        System.Diagnostics.Debugger.Break();
      }
      return obj;
    }

    public static void DeleteObject(Type type, string key)
    {
      key = KeyToNameFile(key);
      string extension = null;
      extension = ".xml";
      var subDir = type.FullName;
      if (subDir.Contains("Version="))
      {
        subDir = type.Namespace + "+" + type.Name;
      }
      var nameFile = subDir + "." + key + extension;

    }

    private static string KeyToNameFile(string text, string hexMark = "%")
    {
      if (string.IsNullOrEmpty(text)) return null;
      string functionReturnValue = null;
      foreach (var chr in text.ToCharArray())
      {
        if ("*?/\\|<>'\"".IndexOf(chr) != -1)
          functionReturnValue += "-";
        else
          functionReturnValue += chr;
      }
      return functionReturnValue;
    }

    public static void Serialize(object obj, string nameFile)
    {
      var keyLock = nameFile.ToLower();
      try
      {
        var nTry = 0;
        var nTryError = 0;
        do
        {

          nTry = nTryError;
          var stream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(nameFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
          try
          {
            var xml = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            xml.Serialize(stream, obj);
          }
          catch (Exception ex)
          {
            System.Diagnostics.Debug.Print(ex.Message);
            System.Diagnostics.Debugger.Break();
            nTryError += 1;
            System.Threading.Tasks.Task.Delay(500);
          }
          finally
          {
            stream?.Dispose();
          }
        } while (!(nTry == nTryError || nTryError > (5000 / 500)));
      }
      catch (Exception ex)
      {
        throw ex;
      }

    }

    public static object Deserialize(string nameFile, Type type = null)
    {
      if (!System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().FileExists(nameFile)) return null;
      object obj = null;
      var keyLock = nameFile.ToLower();
      var er = default(Exception);
      try
      {
        var nTry = 0;
        var nTryError = 0;
        do
        {
          nTry = nTryError;
          System.IO.Stream stream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(nameFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Inheritable);
          try
          {
            var xml = new System.Xml.Serialization.XmlSerializer(type);

            obj = xml.Deserialize(stream);
            er = null;
          }
          catch (Exception ex)
          {
            er = ex;
            nTryError += 1;
            System.Threading.Tasks.Task.Delay(500);
          }
          finally
          {
            stream?.Dispose();
          }
        } while (!(nTry == nTryError || nTryError > (5000 / 500)));
      }
      catch (Exception ex)
      {
        throw ex;
      }
      return obj;
    }
  }

}
