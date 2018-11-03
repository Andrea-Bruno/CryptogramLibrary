using BlockchainManager;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace CryptogramLibrary
{
  public static class Messaging
  {
    public enum DataType
    {
      Text,
      Image,
      Audio,
    }

    private const int MaxParticipants = 10;
    private static string _blockChainName;
    private static Blockchain _blockchain;

    /// <summary>
    /// This function starts the messaging chat room
    /// </summary>
    /// <param name="publicKeys">A string containing all the participants' public keys</param>
    public static void CreateChatRoom(string publicKeys)
    {
      new System.Threading.Thread(() =>
      {
        var myPublicKey = GetMyPublicKey();
        if (!publicKeys.Contains(myPublicKey))
          publicKeys += myPublicKey;
        publicKeys = publicKeys.Replace("==", "== ");
        var keys = publicKeys.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        CreateChatRoom(new List<string>(keys));
      }).Start();
    }

    private static List<string> _participants; //List in base64 format
    private static int _runningCreateChatRoom;
    public static void CreateChatRoom(List<string> participants)
    {
      _runningCreateChatRoom += 1;
      if (_runningCreateChatRoom == 1)
      {
        new System.Threading.Thread(() =>
        {
          if (participants.Count > MaxParticipants)
          {
            Functions.Alert(Resources.Dictionary.TooManyParticipants);
            _runningCreateChatRoom = 0;
            return;
          }
          foreach (var memberKey in participants)
          {
            if (ValidateKey(memberKey)) continue;
            Functions.Alert(Resources.Dictionary.InvalidKey);
            _runningCreateChatRoom = 0;
            return;
          }
          //Messaging.Container = Container;
          participants.Sort();
          _participants = participants;
          var ptsStr = string.Join(" ", _participants.ToArray());
          System.Security.Cryptography.HashAlgorithm hashType = new System.Security.Cryptography.SHA256Managed();
          var hashBytes = hashType.ComputeHash(Encoding.GetEncoding("utf-16LE").GetBytes(ptsStr));
          _blockChainName = Convert.ToBase64String(hashBytes);
          _blockchain = new Blockchain("cryptogram", _blockChainName, Blockchain.BlockchainType.Binary, Blockchain.BlockSynchronization.SendToTheNetworkBuffer, false, 8192);
          var blockchainLen = ReadBlockchain();
          new System.Threading.Thread(() =>
          {
            _blockchain.RequestAnyNewBlocks();
            ReadBlockchain(blockchainLen);
            _runningCreateChatRoom = 0;
          }).Start();
        }).Start();
      }
    }

    private static long ReadBlockchain(long fromPosition = 0)
    {
      _blockchain.ReadBlocks(fromPosition, ExecuteBlock);
      return _blockchain.Length();
    }
    private static void ExecuteBlock(Blockchain.Block block)
    {
      if (block == null || !block.IsValid()) return;
      var dateAndTime = block.Timestamp;
      var blockData = block.DataByteArray;
      var version = blockData[0];
      var type = (DataType)blockData[1];
      var password = DecryptPassword(blockData, out var encryptedDataPosition);
      var len = blockData.Length - encryptedDataPosition;
      var encryptedData = new byte[len];
      Buffer.BlockCopy(blockData, encryptedDataPosition, encryptedData, 0, len);
      var dataElement = Cryptography.Decrypt(encryptedData, password);
      var dataLen = dataElement.Length - 128;
      var data = new byte[dataLen];
      Buffer.BlockCopy(dataElement, 0, data, 0, dataLen);
      System.Security.Cryptography.HashAlgorithm hashType = new System.Security.Cryptography.SHA256Managed();
      var hashData = hashType.ComputeHash(data);
      var signatureOfData = new byte[128];
      Buffer.BlockCopy(dataElement, dataLen, signatureOfData, 0, 128);
      // Find the author
      string author = null;
      foreach (var participant in _participants)
      {
        var rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
        rsa.ImportCspBlob(Convert.FromBase64String(participant));
        if (!rsa.VerifyHash(hashData, System.Security.Cryptography.CryptoConfig.MapNameToOID("SHA256"),
          signatureOfData)) continue;
        author = participant;
        break;
      }
      //var SignatureOfData = GetMyRSA().SignHash(HashData, System.Security.Cryptography.CryptoConfig.MapNameToOID("SHA256"));
      //var Signatures = Block.GetAllBodySignature();
      //var Author = _Participants.Find(x => Signatures.ContainsKey(x));
      if (author == null)
        System.Diagnostics.Debug.WriteLine("Block written by an impostor");
      else
      {
        var isMy = author == GetMyPublicKey();
        ViewMessage(dateAndTime, type, data, isMy);
      }
    }
    public delegate void ViewMessageUi(DateTime timestamp, DataType type, Byte[] data, bool isMyMessage);
    public static ViewMessageUi ViewMessage = (timeSpan, type, data, isMyMessage) => { };

    private static byte[] GeneratePassword()
    {
      return Guid.NewGuid().ToByteArray();
    }

    //private static byte[] pw;
    private static IEnumerable<byte> EncryptPasswordForParticipants(byte[] password)
    {
      //========================RESULT================================
      //[len ePass1] + [ePass1] + [len ePass2] + [ePass2] + ... + [0] 
      //==============================================================
      var result = new byte[0];
      foreach (var publicKey in _participants)
      {
        var rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
        rsa.ImportCspBlob(Convert.FromBase64String(publicKey));
        var encryptedPassword = rsa.Encrypt(password, true);

        //test
        //if (PublicKey == Functions.GetMyPublicKey())
        //{
        //  pw = EncryptedPassword;
        //  var PW = Functions.GetMyRSA().Decrypt(EncryptedPassword, true);
        //}

        var lanPass = (byte)encryptedPassword.Length;
        var len = new[] { lanPass };
        result = result.Concat(len).Concat(encryptedPassword).ToArray();
      }
      result = result.Concat(new byte[] { 0 }).ToArray();
      return result;
    }
    private static byte[] DecryptPassword(byte[] data, out int encryptedDataPosition)
    {
      //START ==== Obtain all password encrypted ====
      var encryptedPasswords = new List<byte[]>();
      var p = 2;
      int len = data[p];
      do
      {
        p += 1;
        var encryptedPassword = new byte[len];
        Buffer.BlockCopy(data, p, encryptedPassword, 0, len);
        encryptedPasswords.Add(encryptedPassword);
        p += len;
        len = data[p];
      } while (len != 0);
      //END  ==== Obtain all password encrypted ====
      encryptedDataPosition = p + 1;

      var myId = _participants.IndexOf(GetMyPublicKey());
      if (myId == -1)
      {
        //Im not in this chat
        return null;
      }
      var ePassword = encryptedPasswords[myId];
      var rsa = GetMyRsa();

      return rsa.Decrypt(ePassword, true);
    }

    private static void SendData(DataType type, byte[] data)
    {
      new System.Threading.Thread(() =>
      {
        try
        {
          // Specifications of the cryptogram data format (Version 0):
          // First byte (version): Indicates the version of the technical specification, if this parameter changes everything in the data package it can follow other specifications
          // Second byte: Indicates the type of data that contains this block: Text, Image, Audio (in the future also new implementations).
          // Global Password: Variable length data that contains the password for each participant in the chat room. The length of this data depends on the number of participants. For this purpose, see the EncryptPasswordForParticipants function. The protocol includes more than 2 participants in a chat room.
          // Encrypted data: This is the real data (message, photo, audio, etc.), encrypted according to an algorithm contained in the Cryptography.Encrypt class. The encryption is made with an xor between the original data and a random data generated with a repetitive hash that starting from the password.
          const byte version = 0;
          byte[] blockchainData = { version, (byte)type };
          var password = GeneratePassword();
          var globalPassword = EncryptPasswordForParticipants(password);
          System.Security.Cryptography.HashAlgorithm hashType = new System.Security.Cryptography.SHA256Managed();
          var hashData = hashType.ComputeHash(data);
          var signatureOfData = GetMyRsa().SignHash(hashData, System.Security.Cryptography.CryptoConfig.MapNameToOID("SHA256"));
          var encryptedData = Cryptography.Encrypt(data.Concat(signatureOfData).ToArray(), password);
          blockchainData = blockchainData.Concat(globalPassword).Concat(encryptedData).ToArray();
          _blockchain.RequestAnyNewBlocks();
          if (blockchainData.Length * 2 + 4096 <= _blockchain.MaxBlockLength)
          {
            var newBlock = new Blockchain.Block(_blockchain, blockchainData);
            var blockPosition = _blockchain.Length();
            if (!newBlock.IsValid()) System.Diagnostics.Debugger.Break();
            _blockchain.SyncBlockToNetwork(newBlock, blockPosition);
            ViewMessage(newBlock.Timestamp, type, data, true);
          }
          else
            Functions.Alert(Resources.Dictionary.ExceededBlockSizeLimit);
        }
        catch (Exception ex)
        {
          Functions.Alert(ex.Message);
        }
        _sending = 0;
      }).Start();
    }

    private static int _sending;
    public static void SendText(string text)
    {
      _sending += 1;
      if (_sending != 1) return;
      text = text?.Trim(" ".ToCharArray());
      if (!string.IsNullOrEmpty(text))
        SendData(DataType.Text, Encoding.Unicode.GetBytes(text));
    }

    public static void SendPicture(object image)
    {
      _sending += 1;
      if (_sending == 1)
      {
      }
    }

    public static void SendAudio(object audio)
    {
      _sending += 1;
      if (_sending == 1)
      {
      }
    }

    /// <summary>
    /// Return a RSA of current user
    /// </summary>
    /// <returns></returns>
    public static System.Security.Cryptography.RSACryptoServiceProvider GetMyRsa()
    {
      var rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
      rsa.ImportCspBlob(Convert.FromBase64String(MyPrivateKey));
      return rsa;
    }

    /// <summary>
    /// Return the public key of current user in base64 format
    /// </summary>
    /// <returns></returns>
    public static string GetMyPublicKey()
    {
      return Convert.ToBase64String(GetMyRsa().ExportCspBlob(false));
    }

    private static string _myPrivateKey;
    /// <summary>
    /// Return the private key stored in the device,if not present, it generates one
    /// </summary>
    /// <returns></returns>
    public static string MyPrivateKey
    {
      get
      {
        if (string.IsNullOrEmpty(_myPrivateKey))
          _myPrivateKey = (string)Storage.LoadObject(typeof(string), "MyPrivateKey");
        if (string.IsNullOrEmpty(_myPrivateKey))
        {
          MyPrivateKey = Convert.ToBase64String(new System.Security.Cryptography.RSACryptoServiceProvider().ExportCspBlob(true)); //Save
        }
        return _myPrivateKey;
      }
      set
      {
        if (_myPrivateKey == value) return;
        try
        {
          var rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
          rsa.ImportCspBlob(Convert.FromBase64String(value));
          _myPrivateKey = value;
          Storage.SaveObject(_myPrivateKey, "MyPrivateKey");
        }
        catch (Exception)
        {
          Functions.Alert(Resources.Dictionary.InvalidKey);
          throw;
        }

      }
    }

    public class Contact : ICloneable
    {
      private static string FirstUpper(string text)
      {
        var value = "";
        if (string.IsNullOrEmpty(text)) return value;
        var last = false;
        foreach (var c in text)
        {
          if (char.IsLetter(c))
          {
            if (!last)
              value += char.ToUpper(c);
            else
              value += c;
            last = true;
          }
          else
          {
            last = false;
            value += c;
          }
        }
        return value;
      }
      private string _name;
      public string Name
      {
        get => _name;
        set => _name = FirstUpper(value);
      }
      private string _publicKey;
      public string PublicKey
      {
        get => _publicKey;
        set
        {
          _publicKey = "";
          if (value == null) return;
          foreach (var c in value.ToCharArray())
          {
            //Clear Base64 string
            if (char.IsLetterOrDigit(c) || @"+=/".Contains(c))
              _publicKey += c;
          }
        }
      }
      public void Save()
      {
        AddContact(this);
      }

      public object Clone()
      {
        return MemberwiseClone();
      }
    }

    private static readonly List<Contact> Contacts = InitContacts();
    private static List<Contact> InitContacts()
    {
      var list = (List<Contact>)Storage.LoadObject(typeof(List<Contact>), "Contacts") ?? new List<Contact>();
#if DEBUG
      if (list.Count == 0)
        list.Add(new Contact() { Name = "Pippo", PublicKey = Convert.ToBase64String(new System.Security.Cryptography.RSACryptoServiceProvider().ExportCspBlob(false)) });
#endif
      return list;
    }

    public static Contact[] GetContacts()
    {
      lock (Contacts)
        return Contacts.ToArray();
    }
    public static bool AddContact(Contact contact)
    {
      lock (Contacts)
      {
        if (Contacts.Contains(contact))
          Contacts.Remove(contact);
        var duplicate = Contacts.Find(x => x.PublicKey == contact.PublicKey);
        if (duplicate != null)
          Contacts.Remove(duplicate);
        Contacts.Add(contact);
        var sorted = Contacts.OrderBy(o => o.Name).ToList();
        Contacts.Clear();
        Contacts.AddRange(sorted);
      }
      Storage.SaveObject(Contacts, "Contacts");
      if (ValidateKey(contact.PublicKey)) return true;
      Functions.Alert(Resources.Dictionary.InvalidKey);
      return false;
    }

    public static void RemoveContact(Contact contact)
    {
      lock (Contacts)
      {
        if (!Contacts.Contains(contact)) return;
        Contacts.Remove(contact);
        Storage.SaveObject(Contacts, "Contacts");
      }
    }
    public static void RemoveContact(string key)
    {
      var contact = Contacts.Find(x => x.PublicKey == key);
      if (contact != null)
        RemoveContact(contact);
    }
    private static bool ValidateKey(string key)
    {
      try
      {
        var rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
        rsa.ImportCspBlob(Convert.FromBase64String(key));
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

  }

}
