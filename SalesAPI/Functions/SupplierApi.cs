using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SalesAPI.Interfaces;
using SalesAPI.Model;
using System.Net;

namespace SalesAPI.Functions
{
    public class SupplierApi
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly ILogger<SupplierApi> _logger;

        public SupplierApi(ISupplierRepository supplierRepository, ILogger<SupplierApi> logger)
        {
            _supplierRepository = supplierRepository;
            _logger = logger;
        }

        [Function("GetAllSuppliers")]
        [OpenApiOperation(operationId: "GetAllSuppliers", tags: new[] { "Supplier" })]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<Supplier>), Description = "List of suppliers")]
        public async Task<HttpResponseData> GetAllSuppliers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "suppliers")] HttpRequestData req)
        {
            var supplier = await _supplierRepository.GetAllAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(supplier);
            return response;
        }

        [Function("GetSupplierById")]
        [OpenApiOperation(operationId: "GetSupplierById", tags: new[] { "Supplier" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Supplier ID")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Supplier), Description = "Supplier details")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Supplier not found")]
        public async Task<HttpResponseData> GetSupplierById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "suppliers/{id:int}")] HttpRequestData req, int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(supplier);
            return response;
        }

        [Function("CreateSupplier")]
        [OpenApiOperation(operationId: "CreateSupplier", tags: new[] { "Supplier" })]
        [OpenApiRequestBody("application/json", typeof(SupplierRequestPayload), Description = "Supplier data")]
        [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(Supplier), Description = "Supplier created")]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid request payload")]
        public async Task<HttpResponseData> CreateSupplier(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "suppliers")] HttpRequestData req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var supplierPayload = JsonConvert.DeserializeObject<SupplierRequestPayload>(requestBody);

            if (supplierPayload == null || string.IsNullOrWhiteSpace(supplierPayload.SupplierName))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var supplier = new Supplier { SupplierName = supplierPayload.SupplierName };
            await _supplierRepository.AddAsync(supplier);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(supplier);
            return response;
        }

        [Function("UpdateSupplier")]
        [OpenApiOperation(operationId: "UpdateSupplier", tags: new[] { "Supplier" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Supplier ID")]
        [OpenApiRequestBody("application/json", typeof(SupplierRequestPayload), Description = "Updated supplier data")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Supplier), Description = "Supplier updated")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Supplier not found")]
        [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid request payload")]
        public async Task<HttpResponseData> UpdateSupplier(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "suppliers/{id:int}")] HttpRequestData req, int id)
        {
            var existingSupplier = await _supplierRepository.GetByIdAsync(id);
            if (existingSupplier == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var supplierPayload = JsonConvert.DeserializeObject<SupplierRequestPayload>(requestBody);

            if (supplierPayload == null || string.IsNullOrWhiteSpace(supplierPayload.SupplierName))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            existingSupplier.SupplierName = supplierPayload.SupplierName;
            await _supplierRepository.UpdateAsync(existingSupplier);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(existingSupplier);
            return response;
        }

        [Function("DeleteSupplier")]
        [OpenApiOperation(operationId: "DeleteSupplier", tags: new[] { "Supplier" })]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Supplier ID")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Supplier deleted")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Supplier not found")]
        public async Task<HttpResponseData> DeleteSupplier(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "suppliers/{id:int}")] HttpRequestData req, int id)
        {
            var existingSupplier = await _supplierRepository.GetByIdAsync(id);
            if (existingSupplier == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            await _supplierRepository.DeleteAsync(id);

            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
    }
}
