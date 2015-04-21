using WorldWind.Renderable;
using System;
using System.Globalization;

namespace WorldWind
{
	/// <summary>
	/// Formats urls for images stored in NLT-style
	/// </summary>
	public class NltImageStore : ImageStore
	{
		#region Private Members

		string m_dataSetName;
		string m_serverUri;

		#endregion

		public override bool IsDownloadableLayer
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:WorldWind.ImageTileService"/> class.
		/// </summary>
		/// <param name="dataSetName"></param>
		/// <param name="serverUri"></param>
		public NltImageStore(
			string dataSetName,
			string serverUri)
		{
			m_serverUri = serverUri;
			m_dataSetName = dataSetName;
		}

		protected override string GetDownloadUrl(QuadTile qt)
		{
			return string.Format(CultureInfo.InvariantCulture, 
				"{0}?T={1}&L={2}&X={3}&Y={4}", m_serverUri, 
				m_dataSetName, qt.Level, qt.Col, qt.Row);
		}
	}
}
