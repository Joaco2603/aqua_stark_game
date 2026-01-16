using System.Threading.Tasks;

namespace Managers.Adapter.Interfaces
{
	public interface IHttpClient
	{
		Task<object> GetAsync(string url, RequestConfig config = null);
		Task<object> PostAsync(string url, object data = null, RequestConfig config = null);
		Task<object> PutAsync(string url, object data = null, RequestConfig config = null);
		Task<object> DeleteAsync(string url, RequestConfig config = null);
	}
}

