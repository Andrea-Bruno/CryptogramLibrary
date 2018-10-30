using System;
using System.Collections.Generic;
using System.Text;

namespace CryptogramLibrary.Resources
{
  public static class Dictionary
  {
    static Dictionary()
    {
      var Lng = System.Globalization.CultureInfo.CurrentCulture.ToString().Substring(0, 2);
      switch (Lng)
      {
        case "it":
          Name = "Nome contatto";
          PublicKey = "Chiave pubblica";
          NewContact = "Nuovo contatto";
          Contacts = "Contatti";
          About = "Info";
          Send = "Invia";
          Save = "Salva";
          Alert = "Avviso";
          Ok = "Ok";
          InvalidKey = "Chiave non valida!";
          Add = "Aggiungi";
          Remove = "Rimuovi";
          Info = "Basato su tecnologia blockchain e decentralizzazione, le vostre comunicazioni sono sicure";
          OpenSource = "Open Source";
          EditPrivateKey = "Edita chiave privata";
          TooManyParticipants = "Sono stati aggiunti troppi partecipanti ";
          ExceededBlockSizeLimit = "Superato limite dimensione blocco";
          Exit = "Esci";
          Edit = "Modifica";
          Share = "Condividi";
          Search = "Search";
          strictlyConfidentialMessage = "Messaggio strettamente confidenziale";
          break;
        default:
          Name = "Contact name";
          PublicKey = "Public key";
          NewContact = "New contact";
          Contacts = "Contacts";
          About = "About";
          Send = "Send";
          Save = "Save";
          Alert = "Alert";
          Ok = "Ok";
          InvalidKey = "Key not valid!";
          Add = "Add";
          Remove = "Remove";
          Info = "Based on blockchain technology and decentralization, your communications are secure";
          OpenSource = "Open Source";
          EditPrivateKey = "Edit private key";
          TooManyParticipants = "Too many participants have been added";
          ExceededBlockSizeLimit = "Exceeded block size limit";
          Exit = "Exit";
          Edit = "Edit";
          Share = "Share";
          Search = "Seach";
          strictlyConfidentialMessage = "Strictly confidential message";
          break;
      }
    }
    public static string Name;
    public static string PublicKey;
    public static string NewContact;
    public static string Contacts;
    public static string About;
    public static string Send; //Send message
    public static string Save;
    public static string Alert;
    public static string Ok;
    public static string InvalidKey;
    public static string Add;
    public static string Remove;
    public static string Info;
    public static string OpenSource;
    public static string EditPrivateKey;
    public static string TooManyParticipants;
    public static string ExceededBlockSizeLimit;
    public static string Exit;
    public static string Edit;
    public static string Share;
    public static string Search;
    public static string strictlyConfidentialMessage;
  }

}
