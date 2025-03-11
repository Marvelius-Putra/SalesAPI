using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SalesAPI.Extensions;
using SalesAPI.Interfaces;
using SalesAPI.Model;
using System.Net;

public class CustomerApi
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerApi> _logger;

    public CustomerApi(ICustomerRepository customerRepository, ILogger<CustomerApi> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    [Function("GetAllCustomers")]
    [OpenApiOperation(operationId: "GetAllCustomers", tags: new[] { "Customer" })]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<Customer>), Description = "List of customers")]
    public async Task<HttpResponseData> GetAllCustomers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers")] HttpRequestData req)
    {
        _logger.LogStart<CustomerApi>();
        try
        {
            var customer = await _customerRepository.GetAllAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(customer);
            _logger.LogSuccess().LogFinish<CustomerApi>();
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogFailed(ex, "Error fetching customers");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("GetCustomerById")]
    [OpenApiOperation(operationId: "GetCustomerById", tags: new[] { "Customer" })]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Customer ID")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Customer), Description = "Customer details")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Customer not found")]
    public async Task<HttpResponseData> GetCustomerById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "customers/{id}")] HttpRequestData req, int id)
    {
        _logger.LogStart<CustomerApi>();
        try
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer is null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);

            _logger.LogSuccess().LogFinish<CustomerApi>();
            await response.WriteAsJsonAsync(customer);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogFailed(ex, $"Error fetching customer with ID {id}");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("CreateCustomer")]
    [OpenApiOperation(operationId: "CreateCustomer", tags: new[] { "Customer" })]
    [OpenApiRequestBody("application/json", typeof(CustomerRequestPayload), Description = "Customer data")]
    [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(Customer), Description = "Customer created")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid request payload")]
    public async Task<HttpResponseData> CreateCustomer(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers")] HttpRequestData req)
    {
        _logger.LogStart<CustomerApi>();
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<CustomerRequestPayload>(requestBody);
            if (request == null || string.IsNullOrWhiteSpace(request.CustomerName))
                return req.CreateResponse(HttpStatusCode.BadRequest);

            var customer = new Customer { CustomerName = request.CustomerName };
            await _customerRepository.AddAsync(customer);

            var response = req.CreateResponse(HttpStatusCode.Created);

            _logger.LogSuccess().LogFinish<CustomerApi>();
            await response.WriteAsJsonAsync(customer);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogFailed(ex, "Error creating customer");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("UpdateCustomer")]
    [OpenApiOperation(operationId: "UpdateCustomer", tags: new[] { "Customer" })]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Customer ID")]
    [OpenApiRequestBody("application/json", typeof(CustomerRequestPayload), Description = "Updated customer data")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Customer), Description = "Customer updated")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Customer not found")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Invalid request payload")]
    public async Task<HttpResponseData> UpdateCustomer(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "customers/{id}")] HttpRequestData req, int id)
    {
        _logger.LogStart<CustomerApi>();
        try
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return req.CreateResponse(HttpStatusCode.NotFound);

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<CustomerRequestPayload>(requestBody);
            if (request == null || string.IsNullOrWhiteSpace(request.CustomerName))
                return req.CreateResponse(HttpStatusCode.BadRequest);

            customer.CustomerName = request.CustomerName;
            await _customerRepository.UpdateAsync(customer);

            var response = req.CreateResponse(HttpStatusCode.OK);

            _logger.LogSuccess().LogFinish<CustomerApi>();
            await response.WriteAsJsonAsync(customer);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogFailed(ex, "Error updating customer");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("DeleteCustomer")]
    [OpenApiOperation(operationId: "DeleteCustomer", tags: new[] { "Customer" })]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Customer ID")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Customer deleted")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Customer not found")]
    public async Task<HttpResponseData> DeleteCustomer(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "customers/{id}")] HttpRequestData req, int id)
    {
        _logger.LogStart<CustomerApi>();
        try
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return req.CreateResponse(HttpStatusCode.NotFound);
            
            await _customerRepository.DeleteAsync(id);

            _logger.LogSuccess().LogFinish<CustomerApi>();
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogFailed(ex, "Error deleting customer");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
