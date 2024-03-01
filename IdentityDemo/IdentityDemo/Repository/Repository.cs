using System;
using System.Data;
using System.Data.SqlClient;
using System.ServiceModel;
using System.Threading.Tasks;
using Dapper;
using IdentityDemo.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SoapDemo;

namespace IdentityDemo.Repository
{
	public class Repository : IRepository
	{
		private readonly IConfiguration _configuration;
		public readonly string serviceUrl = "https://localhost:44399/SoapDemo.asmx";
		public readonly EndpointAddress endpointAddress;
		public readonly BasicHttpBinding basicHttpBinding;

		public Repository(IConfiguration configuration)
		{
			_configuration = configuration;

			endpointAddress = new EndpointAddress(serviceUrl);

			basicHttpBinding =
				new BasicHttpBinding(endpointAddress.Uri.Scheme.ToLower() == "http" ?
							BasicHttpSecurityMode.None : BasicHttpSecurityMode.Transport)
				{
					OpenTimeout = TimeSpan.MaxValue,
					CloseTimeout = TimeSpan.MaxValue,
					ReceiveTimeout = TimeSpan.MaxValue,
					SendTimeout = TimeSpan.MaxValue
				};
		}

		public async Task<SoapDemoSoapClient> GetInstanceAsync()
			=> await Task.Run(() => new SoapDemoSoapClient(basicHttpBinding, endpointAddress));

		public async Task<Response<IdentityModel>> LoginAsync(LoginViewModel loginView)
		{
			Response<IdentityModel> response = new();
			IdentityModel userModel = new();

			try
			{
				var client = await GetInstanceAsync();
				var result = await client.loginAsync(loginView.Email, loginView.Password);

				DataTable dt = new();
				dt = JsonConvert.DeserializeObject<DataTable>(result.Body.loginResult.Data);

				IdentityModel user = new()
				{
					ID = int.Parse(dt.Rows[0]["ID"].ToString()),
					Email = dt.Rows[0]["Email"].ToString(),
					Role = dt.Rows[0]["Role"].ToString(),
					Reg_Date = dt.Rows[0]["Reg_Date"].ToString()
				};

				response.Data = user;
				response.message = (result.Body.loginResult.resultCode == 500) ? "Login failed.Please check Username and / or password" : "data found";
				response.code = result.Body.loginResult.resultCode;
			}
			catch (Exception ex)
			{
				response.code = 500;
				response.message = ex.Message;
			}

			//var sp_params = new DynamicParameters();
			//sp_params.Add("email",loginView.Email,DbType.String);
			//sp_params.Add("password", loginView.Password, DbType.String);

			//try
			//{
			//    using IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("default"));

			//    response.Data = await dbConnection.QueryFirstOrDefaultAsync<IdentityModel>("sp_loginUser", sp_params, commandType: CommandType.StoredProcedure);
			//    response.message = (response.Data is null) ? "Login failed.Please check Username and / or password" : "data found";
			//    response.code = (response.Data is null) ? 500 : 200;
			//}
			//catch (Exception ex)
			//{
			//    response.code = 500;
			//    response.message = ex.Message;
			//}

			return response;
		}

		public async Task<Response<string>> RegisterAsync(RegisterViewModel registerView)
		{
			Response<string> response = new();
			var sp_params = new DynamicParameters();
			sp_params.Add("email", registerView.Email, DbType.String);
			sp_params.Add("password", registerView.Password, DbType.String);
			sp_params.Add("role", registerView.Role, DbType.String);
			sp_params.Add("retVal", DbType.String, direction: ParameterDirection.Output);

			using (IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("default")))
			{
				if (dbConnection.State == ConnectionState.Closed) dbConnection.Open();
				using var transaction = dbConnection.BeginTransaction();
				try
				{
					await dbConnection.QueryAsync<string>("sp_registerUser", sp_params, commandType: CommandType.StoredProcedure, transaction: transaction);
					response.code = sp_params.Get<int>("retVal"); //get output parameter value
					transaction.Commit();
					response.message = (response.code == 200) ? "Successfully Registered" : "Unable to register user";
				}
				catch (Exception ex)
				{
					transaction.Rollback();

					response.Data = ex.Message;
					response.message = "An error encountered during saving!";
					response.code = 500;
				}
			};
			return response;
		}
	}
}