using System;
using System.Collections.Generic;
using System.Text;

namespace CryptogramLibrary
{
  public static class Functions
  {
    public delegate void AlertMessage(string Text);
    public static AlertMessage Alert = (Text) => { };

    public delegate bool ShareTextMessage(string Text);
    public static ShareTextMessage ShareText = (Text) => { return true; };

    /// <summary>
    /// This method initializes the network.
    /// You can join the network as a node, and contribute to decentralization, or hook yourself to the network as an external user.
    /// To create a node, set the MyAddress parameter with your web address.If MyAddress is not set then you are an external user.
    /// </summary>
    /// <param name="MyAddress">Your web address. If you do not want to create the node, omit this parameter</param>
    /// <param name="EntryPoints">The list of permanent access points nodes, to access the network. If null then the entry points will be those set in the NetworkManager.Setup</param>
    /// <param name="NetworkName">The name of the infrastructure. For tests we recommend using "testnet"</param>
    public static bool Initialize(string MyAddress = null, Dictionary<string, string> EntryPoints = null, string NetworkName = "testnet")
    {
      return BlockchainManager.HookToNetwork.Initialize(MyAddress, EntryPoints, NetworkName);
    }
  }
}


