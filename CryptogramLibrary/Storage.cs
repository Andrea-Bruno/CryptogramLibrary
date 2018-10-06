using System;
using System.Collections.Generic;
using System.Text;

namespace CryptogramLibrary
{
  internal class Storage
  {
    public static string SaveObject(object Obj, string Key)
    {
      Key = KeyToNameFile(Key);
      string Extension = null;
      Extension = ".xml";

      string SubDir = Obj.GetType().FullName;
      if (SubDir.Contains("Version="))
      {
        SubDir = Obj.GetType().Namespace + "+" + Obj.GetType().Name;
      }

      if (Key.Length > 255)
      {
        throw new System.ArgumentException("File name too long", "");
      }
      else
      {
        Serialize(Obj, SubDir + "." + Key + Extension);
      }

      return Key;
    }

    public static object LoadObject(Type Type, string Key)
    {
      object Obj = null;
      Key = KeyToNameFile(Key);
      string Extension = null;
      Extension = ".xml";
      string SubDir = Type.FullName;
      if (SubDir.Contains("Version="))
      {
        SubDir = Type.Namespace + "+" + Type.Name;
      }
      try
      {
        Obj = Deserialize(SubDir + "." + Key + Extension, Type);
      }
      catch (Exception ex)
      {
      }
      return Obj;
    }

    public static void DeleteObject(Type Type, string Key)
    {
      Key = KeyToNameFile(Key);
      string Extension = null;
      Extension = ".xml";
      string SubDir = Type.FullName;
      if (SubDir.Contains("Version="))
      {
        SubDir = Type.Namespace + "+" + Type.Name;
      }
      string NameFile = SubDir + "." + Key + Extension;

    }

    private static string KeyToNameFile(string Text, string HexMark = "%")
    {
      string functionReturnValue = null;
      if (!string.IsNullOrEmpty(Text))
      {
        foreach (char Chr in Text.ToCharArray())
        {
          if ("*?/\\|<>'\"".IndexOf(Chr) != -1)
            functionReturnValue += "-";
          else
            functionReturnValue += Chr;
        }
      }
      return functionReturnValue;
    }

    public static void Serialize(object Obj, string NameFile)
    {
      string KeyLock = NameFile.ToLower();
      Exception Er = null;
      try
      {
        int NTry = 0;
        int NTryError = 0;
        do
        {

          NTry = NTryError;
          var Stream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(NameFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
          try
          {
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(Obj.GetType());
            xml.Serialize(Stream, Obj);
          }
          catch (Exception ex)
          {
            NTryError += 1;
            System.Threading.Tasks.Task.Delay(500);
          }
          finally
          {
            if (Stream != null)
            {
              Stream.Dispose();
            }
          }
        } while (!(NTry == NTryError || NTryError > (5000 / 500)));
      }
      catch (Exception ex)
      {
        throw ex;
      }

    }

    public static object Deserialize(string NameFile, Type Type = null)
    {
      object Obj = null;
      if (System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication().FileExists(NameFile)) ;
      {
        string KeyLock = NameFile.ToLower();
        Exception Er = default(Exception);
        try
        {
          int NTry = 0;
          int NTryError = 0;
          do
          {
            NTry = NTryError;
            System.IO.Stream Stream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(NameFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Inheritable);
            try
            {
              System.Xml.Serialization.XmlSerializer XML = new System.Xml.Serialization.XmlSerializer(Type);

              Obj = XML.Deserialize(Stream);
              Er = null;
            }
            catch (Exception ex)
            {
              Er = ex;
              NTryError += 1;
              System.Threading.Tasks.Task.Delay(500);
            }
            finally
            {
              if (Stream != null)
              {
                Stream.Dispose();
              }
            }
          } while (!(NTry == NTryError || NTryError > (5000 / 500)));
        }
        catch (Exception ex)
        {
          throw ex;
        }
      }
      return Obj;
    }
  }

}
