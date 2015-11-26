﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TecWare.DE.Server;
using TecWare.DE.Stuff;

namespace TecWare.DE.Odette.Network
{
	///////////////////////////////////////////////////////////////////////////////
	/// <summary></summary>
	internal sealed	class OdetteListenerTcpItem :  DEConfigItem
	{
		private IServerTcp serverTcp;

		private IListenerTcp currentListener = null;

		#region -- Ctor/Dtor --------------------------------------------------------------

		public OdetteListenerTcpItem(IServiceProvider sp, string name)
			: base(sp, name)
		{
    } // ctor

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				Procs.FreeAndNil(ref currentListener);

			base.Dispose(disposing);
		} // proc Dispose

		protected override void OnBeginReadConfiguration(IDEConfigLoading config)
		{
			base.OnBeginReadConfiguration(config);

			// is there the tcp listener
			serverTcp = this.GetService<IServerTcp>(true);
		} // proc OnBeginReadConfiguration

		protected override void OnEndReadConfiguration(IDEConfigLoading config)
		{
			base.OnEndReadConfiguration(config);

			var listenerAddress = Config.GetAttribute("address", "0.0.0.0");
			var listenerPort = Config.GetAttribute("port", 3305);
			var useSsl = Config.GetAttribute("ssl", false);

			Log.Info($"Register Listener (port={listenerPort}, addr={listenerAddress.ToString()}, ssl={useSsl})");
			var endPoint = new IPEndPoint(IPAddress.Parse(listenerAddress), 3305);

			// start the listener
			serverTcp.RegisterListener(endPoint,
				useSsl ?
					new Action<Stream>(CreateSslHandler) :
					new Action<Stream>(CreateHandler)
			);
		} // proc OnEndReadConfiguration

		#endregion

		private void CreateHandler(Stream socket)
		{
			var protocol = this.GetService<OdetteFileTransferProtocolItem>(true);

			// start the protocol
			Task.Run(() => protocol.StartProtocolAsync(new OdetteNetworkStream(socket, "tcp:" + serverTcp.GetStreamInfo(socket), Config.GetAttribute("userData", String.Empty)), false));
		} // proc CreateHandler

		private void CreateSslHandler(Stream socket)
		{
			throw new NotImplementedException();
		} // proc CreateSslHandler
	} // class OdetteListenerTcpItem
}