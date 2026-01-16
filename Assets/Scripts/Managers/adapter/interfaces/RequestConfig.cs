using System.Collections.Generic;

namespace Managers.Adapter.Interfaces
{
	public class RequestConfig
	{
		public Dictionary<string, string> Headers { get; set; }
		public Dictionary<string, object> Params { get; set; }
		public int? Timeout { get; set; }
	}
}

