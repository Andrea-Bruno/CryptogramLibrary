using System.Collections.Generic;
namespace CryptogramLibrary
{
  public static class Functions
  {
    public delegate void AlertMessage(string text);
    public static AlertMessage Alert = (text) => { };

    public delegate bool ShareTextMessage(string text);
    public static ShareTextMessage ShareText = (text) => true;

    /// <summary>
    /// This method initializes the network.
    /// You can join the network as a node, and contribute to decentralization, or hook yourself to the network as an external user.
    /// To create a node, set the MyAddress parameter with your web address.If MyAddress is not set then you are an external user.
    /// </summary>
    /// <param name="entryPoints">The list of permanent access points nodes, to access the network. If null then the entry points will be those set in the NetworkManager.Setup</param>
    /// <param name="networkName">The name of the infrastructure. For tests we recommend using "testnet"</param>
    public static void Initialize(Dictionary<string, string> entryPoints = null, string networkName = "testnet")
    {
      BlockchainManager.NetworkInitializer.HookToNetwork(entryPoints, networkName);
    }
  }
}


