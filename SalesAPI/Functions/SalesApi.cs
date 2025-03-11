using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SalesAPI.Extensions;
using SalesAPI.Interfaces;
using SalesAPI.Model;
using System.Globalization;
using System.Net;

namespace SalesAPI.Functions
{
    public class SalesApi
    {
        private readonly ISalesRepository _salesRepository;
        private readonly ILogger<SalesApi> _logger;

        public SalesApi(ISalesRepository salesRepository, ILogger<SalesApi> logger)
        {
            _salesRepository = salesRepository;
            _logger = logger;
        }

        [Function("GetSales")]
        [OpenApiOperation(operationId: "GetSales", tags: new[] { "Sales" })]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<Sales>), Description = "List of sales")]
        public async Task<HttpResponseData> GetSales(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sales")] HttpRequestData req)
        {
            _logger.LogStart<SalesApi>();
            try
            {
                var sales = await _salesRepository.GetAllAsync();
                var response = req.CreateResponse(HttpStatusCode.OK);

                _logger.LogSuccess().LogFinish<SalesApi>();
                await response.WriteAsJsonAsync(sales);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogFailed(ex, "Error in GetSales method");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("CreateSale")]
        [OpenApiOperation(operationId: "CreateSale", tags: new[] { "Sales" })]
        [OpenApiRequestBody("application/json", typeof(CreateSaleDto))]
        [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(Sales), Description = "Sale Created")]
        public async Task<HttpResponseData> CreateSale(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "sales")] HttpRequestData req)
        {
            _logger.LogStart<SalesApi>();
            try
            {
                var requestBody = await req.ReadFromJsonAsync<CreateSaleDto>();
                if (requestBody == null || requestBody.CustomerId <= 0 || requestBody.ProductId <= 0 || requestBody.ProductQty <= 0)
                {
                    return req.CreateResponse(HttpStatusCode.BadRequest);
                }

                var sale = await _salesRepository.AddSaleAsync(requestBody);

                _logger.LogSuccess().LogFinish<SalesApi>();
                var response = req.CreateResponse(HttpStatusCode.Created);
                await response.WriteAsJsonAsync(sale);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogFailed(ex, "Error in CreateSale method");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        [Function("GetDailySalesReport")]
        [OpenApiOperation(operationId: "GetDailySalesReport", tags: new[] { "Sales" })]
        [OpenApiParameter(name: "date", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Date for the sales report (format: yyyy-MM-dd)")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<DailySalesReportDto>), Description = "Daily Sales Report")]
        public async Task<HttpResponseData> GetDailySalesReport(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sales/daily-report")] HttpRequestData req)
        {
            _logger.LogStart<SalesApi>();

            try
            {
                var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                string dateStr = query["date"];

                // Validasi parameter tanggal
                if (string.IsNullOrWhiteSpace(dateStr) || !DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Invalid date format. Please use yyyy-MM-dd.");
                    return badRequestResponse;
                }

                var dailySalesReport = await _salesRepository.GetDailySalesReportAsync(date);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(dailySalesReport);

                _logger.LogSuccess().LogFinish<SalesApi>();
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogFailed(ex, "Error in GetDailySalesReport method");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while retrieving the sales report.");
                return errorResponse;
            }
        }

    }
}
