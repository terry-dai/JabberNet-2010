﻿/* --------------------------------------------------------------------------
 * Copyrights
 *
 * Portions created by or assigned to Sébastien Gissinger
 *
 * License
 *
 * Jabber-Net is licensed under the LGPL.
 * See LICENSE.txt for details.
 * --------------------------------------------------------------------------*/
using System;
using System.Net;
using System.Net.Sockets;

namespace Jabber.Stun
{
    /// <summary>
    /// TODO: Documentation Class
    /// </summary>
    public class StunClient
    {
        #region CONSTANTS
        /// <summary>
        /// TODO: Documentation Constant
        /// </summary>
        public const Int32 DEFAULT_STUN_PORT = 3478;
        /// <summary>
        /// TODO: Documentation Constant
        /// </summary>
        public const Int32 DEFAULT_STUNS_PORT = 5489;
        /// <summary>
        /// TODO: Documentation Constant
        /// </summary>
        private const Int32 BUFFER = 8192;
        #endregion

        #region PROPERTIES
        /// <summary>
        /// TODO: Documentation Property
        /// </summary>
        public IPEndPoint StunServerEP { get; private set; }
        /// <summary>
        /// TODO: Documentation Property
        /// </summary>
        public IPEndPoint StunningEP { get; private set; }
        /// <summary>
        /// TODO: Documentation Property
        /// </summary>
        public Socket Socket { get; private set; }
        /// <summary>
        /// TODO: Documentation Property
        /// </summary>
        public ProtocolType ProtocolType { get; private set; }
        /// <summary>
        /// TODO: Documentation Property
        /// </summary>
        public SocketType SocketType { get; private set; }
        #endregion


        #region CONSTRUCTORS & FINALIZERS
        /// <summary>
        /// TODO: Documentation Constructor
        /// </summary>
        public StunClient()
        { }

        /// <summary>
        /// Sample usage of this StunClient
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            StunMessage msg = new StunMessage(StunMethodType.Binding, StunMethodClass.Request, StunUtilities.NewTransactionId);
            StunAttribute attr1 = new StunAttribute(StunAttributeType.Realm, "Hello World !");
            StunAttribute attr2 = new StunAttribute(StunAttributeType.Username, "Bob");

            msg.SetAttribute(attr1);
            msg.SetAttribute(attr2);

            byte[] octets = msg;
            StunMessage msgCopy = octets;

            StunClient cli = new StunClient();

            cli.Connect("66.228.45.110", ProtocolType.Udp);
            StunMessage op1 = cli.SendMessage(msgCopy);
            cli.Close();

            msgCopy.ClearAttributes();

            cli.Connect("132.177.123.13", ProtocolType.Udp);
            StunMessage op2 = cli.SendMessage(msgCopy);
            cli.Close();
        }
        #endregion

        #region METHODS
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stunServerIp"></param>
        /// <param name="stunServerPort"></param>
        /// <param name="protocolType"></param>
        public void Connect(String stunServerIp, Int32 stunServerPort, ProtocolType protocolType)
        {
            this.Connect(new IPEndPoint(IPAddress.Parse(stunServerIp), stunServerPort), protocolType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stunServerIp"></param>
        /// <param name="protocolType"></param>
        public void Connect(String stunServerIp, ProtocolType protocolType)
        {
            this.Connect(new IPEndPoint(IPAddress.Parse(stunServerIp), StunClient.DEFAULT_STUN_PORT), protocolType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stunServerEP"></param>
        /// <param name="protocolType"></param>
        public void Connect(IPEndPoint stunServerEP, ProtocolType protocolType)
        {
            if (this.Socket != null)
                this.Close();

            this.StunServerEP = stunServerEP;
            this.ProtocolType = protocolType;

            if (this.ProtocolType == ProtocolType.Tcp)
                this.SocketType = SocketType.Stream;
            else
                this.SocketType = SocketType.Dgram;

            this.Socket = new Socket(stunServerEP.AddressFamily, this.SocketType, this.ProtocolType);
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            if (this.StunningEP != null)
                this.Socket.Bind(this.StunningEP);

            this.Socket.Connect(stunServerEP);

            if (this.StunningEP == null)
                this.StunningEP = (IPEndPoint)this.Socket.LocalEndPoint;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            this.Socket.Shutdown(SocketShutdown.Both);

            this.Socket.Close();

            this.Socket = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            if (this.Socket != null)
                this.Close();

            this.StunServerEP = null;
            this.ProtocolType = ProtocolType.Unknown;
            this.SocketType = SocketType.Unknown;
            this.StunningEP = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public StunMessage SendMessage(StunMessage msg)
        {
            return this.SendMessage(msg.GetBytes(), msg.MethodClass != StunMethodClass.Indication);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="needResponse"></param>
        /// <returns></returns>
        public StunMessage SendMessage(byte[] msg, Boolean needResponse)
        {
            this.Socket.Send(msg, 0, msg.Length, SocketFlags.None);

            if (needResponse)
            {
                byte[] result = new byte[StunClient.BUFFER];

                this.Socket.Receive(result, 0, StunClient.BUFFER, SocketFlags.None);

                return result;
            }
            return null;
        }
        #endregion
    }
}