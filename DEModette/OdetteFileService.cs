﻿using System;
using System.Collections.Generic;
using TecWare.DE.Server;
using TecWare.DE.Stuff;

namespace TecWare.DE.Odette
{
	#region -- enum OdetteFileFormat ----------------------------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary></summary>
	public enum OdetteFileFormat
	{
		/// <summary>Record file with a fixed record size.</summary>
		Fixed,
		/// <summary>Record file with a variable record size.</summary>
		Variable,
		/// <summary>Binary file.</summary>
		Unstructured,
		/// <summary>Text file (is the responsiblilty of the file server to handle the charsets).</summary>
		Text
	} // enum OdetteFileFormat

	#endregion

	#region -- enum OdetteOutFileState --------------------------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary></summary>
	public enum OdetteOutFileState
	{
		/// <summary>Not used.</summary>
		New,
		/// <summary>File needs to send.</summary>
		Sent,
		/// <summary>Waiting for the End to End response.</summary>
		WaitEndToEnd,
		/// <summary>Communication finished.</summary>
		ReceivedEndToEnd,
		/// <summary>Not used.</summary>
		Finished
	} // enum OdetteOutFileState

	#endregion

	#region -- enum OdetteOutFileState --------------------------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary></summary>
	public enum OdetteInFileState
	{
		/// <summary>New incoming file.</summary>
		Pending,
		/// <summary>File is complete received.</summary>
		Received,
		/// <summary>File is successfully processed.</summary>
		PendingEndToEnd,
		/// <summary>EndToEnd sent successful.</summary>
		Finished
	} // enum OdetteInFileState

	#endregion

	#region -- interface IOdetteFile ----------------------------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary>Basic file description</summary>
	public interface IOdetteFile
	{
		/// <summary>File name</summary>
		string VirtualFileName { get; }
		/// <summary>Time stamp of the file</summary>
		DateTime FileStamp { get; }
		/// <summary>Source of the file.</summary>
		string Originator { get; }
	} // interface IOdetteFile

	#endregion

	#region -- class OdetteFileMutable --------------------------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary></summary>
	public sealed class OdetteFileMutable : IOdetteFile
	{
		private readonly string virtualFileName;
		private readonly DateTime fileStamp;
		private readonly string originator;

		public OdetteFileMutable(string virtualFileName, DateTime fileStamp, string originator)
		{
			this.virtualFileName = virtualFileName;
			this.fileStamp = fileStamp;
			this.originator = originator;
		} // ctor

		public override string ToString()
			=> FormatFileName(this, "");

		public string VirtualFileName => virtualFileName;
		public DateTime FileStamp => fileStamp;
		public string Originator => originator;


		public static string FormatFileName(IOdetteFile file, string userData)
			=> file.Originator + "/" + file.VirtualFileName + "[userData=" + userData + "]";
	} // class OdetteFileMutable

	#endregion

	#region -- interface IOdetteFileDescription -----------------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary>Description of the file format (optional).</summary>
	public interface IOdetteFileDescription : IOdetteFile
	{
		/// <summary>Format of the file.</summary>
		OdetteFileFormat Format { get; }
		/// <summary>If the format is fixed, this value holds the record size.</summary>
		int MaximumRecordSize { get; }
		/// <summary>Estimated file size  (in 1kb).</summary>
		long FileSize { get; }
		/// <summary>Estimated file size (unpacked, in 1kb).</summary>
		long FileSizeUnpacked { get; }
		/// <summary>Description</summary>
		string Description { get; }
	} // interface IOdetteFileDescription

	#endregion

	#region -- interface IOdetteFileWriter ----------------------------------------------

	public interface IOdetteFileWriter : IDisposable
	{
		/// <summary>Writes data to the target file.</summary>
		/// <param name="buf"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="isEoR">marks a end of the current record or the complete file.</param>
		void Write(byte[] buf, int offset, int count, bool isEoR);
		/// <summary>File is received successful.</summary>
		/// <returns></returns>
		void CommitFile(long recordCount, long unitCount);

		/// <summary>File name. The description is not used.</summary>
		IOdetteFile Name { get; }

		/// <summary>Total length in bytes.</summary>
		long TotalLength { get; }
		/// <summary>Number of records, only for fixed or variable files. Should be zero in other cases.</summary>
		long RecordCount { get; }
	} // interface IOdetteInFile 

	#endregion

	#region -- interface IOdetteFileReader ----------------------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary>Reads a file from the file service.</summary>
	public interface IOdetteFileReader : IDisposable
	{
		/// <summary>Read a chunk of bytes</summary>
		/// <param name="buf">Buffer to fill</param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="isEoR">marks a end of the current record or the complete file.</param>
		/// <returns>Bytes copied in the buffer.</returns>
		int Read(byte[] buf, int offset, int count, out bool isEoR);

		/// <summary>Transmission failed.</summary>
		/// <param name="answerReason"></param>
		/// <param name="reasonText"></param>
		/// <param name="retryFlag"></param>
		void SetTransmissionError(OdetteAnswerReason answerReason, string reasonText, bool retryFlag);
		/// <summary>File transmitted successful</summary>
		void SetTransmissionState();

		/// <summary>Description or name of the file. Is the description missing, it will be sent as binary file.</summary>
		IOdetteFile Name { get; }
		/// <summary>UserData for the send operation.</summary>
		string UserData { get; }

		/// <summary>Total length in bytes.</summary>
		long TotalLength { get; }
		/// <summary>Number of records, only for fixed or variable files. Should be zero in other cases.</summary>
		long RecordCount { get; }
	} // interface IOdetteFileReader 

	#endregion

	#region -- interface IOdetteFilePosition --------------------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary>Extention for the IOdetteFileReader or IOdetteFileWrite to change
	/// the current read or write position.</summary>
	public interface IOdetteFilePosition
	{
		/// <summary>Reposition for the file (1k steps or records).</summary>
		/// <param name="position"></param>
		long Seek(long position);
	} // interface IOdetteFilePosition

	#endregion

	#region -- interface IOdetteFileEndToEndDescription ---------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary>Description of a end to end message.</summary>
	public interface IOdetteFileEndToEndDescription
	{
		/// <summary>Dateiname</summary>
		IOdetteFile Name { get; }

		/// <summary>Reason code, 0 for a positive end to end, != zero for a negative</summary>
		int ReasonCode { get; }
		/// <summary>Text of the negative end to end.</summary>
		string ReasonText { get; }
		/// <summary>8 byte user data.</summary>
		string UserData { get; }
	} // interface IOdetteFileEndToEndDescription

	#endregion

	#region -- interface IOdetteFileEndToEnd --------------------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary>Implements a end to end return of the file service.</summary>
	public interface IOdetteFileEndToEnd : IOdetteFileEndToEndDescription
	{
		/// <summary>Marks the end to message as sent.</summary>
		void Commit();
	} // interface IOdetteFileEndToEnd

	#endregion

	#region -- interface IOdetteFileService ---------------------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary>Implements a file service session for oftp.</summary>
	public interface IOdetteFileService : IDisposable
	{
		/// <summary>Creates a new file in the service.</summary>
		/// <param name="file">Description or the file name. If the description is missing, the file will be handled as a normal binary file.</param>
		/// <returns></returns>
		IOdetteFileWriter CreateInFile(IOdetteFile file, string userData);
		/// <summary>List of end to end messages, that need to send.</summary>
		/// <returns></returns>
		IEnumerable<IOdetteFileEndToEnd> GetEndToEnd();

		/// <summary>Files for send.</summary>
		/// <returns></returns>
		IEnumerable<Func<IOdetteFileReader>> GetOutFiles();

		/// <summary>End to end received for a file.</summary>
		/// <param name="description"></param>
		bool UpdateOutFileState(IOdetteFileEndToEndDescription description);

		/// <summary>Id of the destination.</summary>
		string DestinationId { get; }
		/// <summary>Sort order</summary>
		int Priority { get; }

		/// <summary>Can the service receive files.</summary>
		bool SupportsInFiles { get; }
		/// <summary>Can the service send files.</summary>
		bool SupportsOutFiles { get; }
	} // interface IOdetteFileService

	#endregion

	#region -- interface IOdetteFileServiceFactory --------------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary></summary>
	public interface IOdetteFileServiceFactory
	{
		/// <summary>Fits this fileservice to the given destination.</summary>
		/// <param name="destinationId">Destination, asked for.</param>
		/// <param name="password">Password of the destination.</param>
		/// <returns>File session or <c>null</c>, if the service is not responsible for the destination.</returns>
		/// <exception cref="OdetteFileServiceException">Will result in end session, with Service is currently not available.</exception>
		IOdetteFileService CreateFileService(string destinationId, string password);
	} // interface IOdetteFileServiceFactory

	#endregion

	#region -- class OdetteFileService --------------------------------------------------

	///////////////////////////////////////////////////////////////////////////////
	/// <summary>Implements a file service that host all collected file service
	/// sessions for the current destination.</summary>
	internal sealed class OdetteFileService : IDisposable
	{
		private readonly IServiceProvider sp;
		private readonly string destinationId;
		private readonly IOdetteFileService[] services;

		#region -- Ctor/Dtor --------------------------------------------------------------

		public OdetteFileService(IServiceProvider sp, string destinationId, string password)
		{
			this.sp = sp;
			this.destinationId = destinationId;

			// Search for the file providers
			var item = sp.GetService<DEConfigItem>(false);
			if (item != null)
			{
				var collected = new List<IOdetteFileService>();

				item.WalkChildren<IOdetteFileServiceFactory>(
					f =>
					{
						var t = f.CreateFileService(destinationId, password);
						if (t != null)
							collected.Add(t);
					},
					recursive: true
				);

				services = collected.ToArray();
			}
		} // ctor

		public void Dispose()
		{
			if (services != null)
				Array.ForEach(services, s => s.Dispose());
		} // proc Dispose

		#endregion

		#region -- File service proxy implementation --------------------------------------

		/// <summary>Create/Overrides/Resumes a new odette file, to receive.</summary>
		/// <param name="fileDescription"></param>
		/// <param name="userData"></param>
		/// <returns></returns>
		public IOdetteFileWriter CreateInFile(IOdetteFile file, string userData)
		{
			foreach (var s in services)
			{
				var inFile = s.CreateInFile(file, userData);
				if (inFile != null)
					return inFile;
			}
			return null;
		} // func CreateInFile

		/// <summary>Get the files that can be committed.</summary>
		/// <returns></returns>
		public IEnumerable<IOdetteFileEndToEnd> GetEndToEnd()
		{
			foreach (var s in services)
				foreach (var i in s.GetEndToEnd())
					yield return i;
		} // func GetEndToEnd

		/// <summary>Files for send.</summary>
		/// <returns></returns>
		public IEnumerable<Func<IOdetteFileReader>> GetOutFiles()
		{
			foreach (var s in services)
				foreach (var i in s.GetOutFiles())
					yield return i;
		} // func GetOutFiles

		/// <summary>End to end received for a file.</summary>
		/// <param name="file"></param>
		/// <param name="description"></param>
		public void UpdateOutFileState(IOdetteFileEndToEndDescription description)
		{
			foreach (var s in services)
			{
				if (s.UpdateOutFileState(description))
					return;
			}
			
			throw new OdetteFileServiceException(OdetteAnswerReason.InvalidFilename, String.Format("E2E failed for {0}.", OdetteFileMutable.FormatFileName(description.Name, description.UserData)), false);
		} // proc UpdateOutFileState

		/// <summary>File service destination id</summary>
		public string DestinationId => destinationId;

		/// <summary>Can one file service create a file.</summary>
		public bool SupportsInFiles
		{
			get
			{
				var r = false;
				for (var i = 0; i < services.Length; i++)
					r |= services[i].SupportsInFiles;
				return r;
			}
		} // prop SupportsInFiles

		/// <summary>Can one file service send a file.</summary>
		public bool SupportsOutFiles
		{
			get
			{
				var r = false;
				for (var i = 0; i < services.Length; i++)
					r |= services[i].SupportsOutFiles;
				return r;
			}
		} // prop SupportsInFiles

		#endregion
	} // class OdetteFileService

	#endregion
}
