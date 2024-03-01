using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using Newtonsoft.Json;
using SoapDemo.Models;

namespace SoapDemo
{
	/// <summary>
	/// Summary description for SoapDemo
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	public class SoapDemo : System.Web.Services.WebService
	{
		[WebMethod]
		public string HelloWorld()
		{
			return "Hello World";
		}

		[WebMethod]
		public ResponseModel<string> login(string email, string password)
		{
			ResponseModel<string> response = new ResponseModel<string>();

			if (email != null)
			{
				using (SqlConnection conn = new SqlConnection(@"Data Source=ALA-IT-48131\SQLEXPRESS;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
				{
					SqlCommand cmd = new SqlCommand("sp_loginUser", conn)
					{
						CommandType = CommandType.StoredProcedure
					};
					cmd.Parameters.Add("@email", SqlDbType.NVarChar, 50).Value = email;
					cmd.Parameters.Add("@password", SqlDbType.NVarChar, 50).Value = password;

					SqlDataAdapter da = new SqlDataAdapter(cmd);
					DataTable dt = new DataTable();
					da.Fill(dt);

					if (dt.Rows.Count > 0)
					{
						response.Data = JsonConvert.SerializeObject(dt);
						response.resultCode = 200;
					}
					else
					{
						response.message = "User Not Found!";
						response.resultCode = 500;
					}
				}
			}
			return response;
		}
	}
}