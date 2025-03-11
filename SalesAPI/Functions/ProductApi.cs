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

public class ProductApi
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductApi> _logger;

    public ProductApi(IProductRepository repository, ILogger<ProductApi> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [Function("GetProducts")]
    [OpenApiOperation(operationId: "GetProducts", tags: new[] { "Product" })]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<Product>), Description = "List of products")]
    public async Task<HttpResponseData> GetProducts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequestData req)
    {
        _logger.LogStart<ProductApi>();
        try
        {
            var product = await _repository.GetAllAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);

            _logger.LogSuccess().LogFinish<ProductApi>();
            await response.WriteAsJsonAsync(product);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogFailed(ex, "Error retrieving products");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("GetProductById")]
    [OpenApiOperation(operationId: "GetProductById", tags: new[] { "Product" })]
    [OpenApiParameter("id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Product ID")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Product), Description = "Product details")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Product not found")]
    public async Task<HttpResponseData> GetProductById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products/{id}")] HttpRequestData req, int id)
    {
        try
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            _logger.LogSuccess().LogFinish<ProductApi>();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(product);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogFailed(ex, "Error retrieving product by ID");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("CreateProduct")]
    [OpenApiOperation(operationId: "CreateProduct", tags: new[] { "Product" })]
    [OpenApiRequestBody("application/json", typeof(ProductRequestPayload), Description = "Product data")]
    [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(Product), Description = "Product created")]
    public async Task<HttpResponseData> CreateProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products")] HttpRequestData req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var product = JsonConvert.DeserializeObject<Product>(requestBody);

            if (product == null || string.IsNullOrWhiteSpace(product.ProductName) || product.ProductPrice <= 0 || product.ProductStock < 0)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            await _repository.AddAsync(product);

            _logger.LogSuccess().LogFinish<ProductApi>();
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(product);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogFailed(ex, "Error creating product");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("UpdateProduct")]
    [OpenApiOperation(operationId: "UpdateProduct", tags: new[] { "Product" })]
    [OpenApiRequestBody("application/json", typeof(ProductRequestPayload), Description = "Updated product data")]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Product ID")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Product updated")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Product not found")]
    public async Task<HttpResponseData> UpdateProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "products/{id}")] HttpRequestData req, int id)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var product = JsonConvert.DeserializeObject<Product>(requestBody);

            if (product == null || string.IsNullOrWhiteSpace(product.ProductName) || product.ProductPrice <= 0 || product.ProductStock < 0)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var existingProduct = await _repository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            _logger.LogSuccess().LogFinish<ProductApi>();
            product.ProductId = id;
            await _repository.UpdateAsync(product);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogFailed(ex, "Error updating product");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("DeleteProduct")]
    [OpenApiOperation(operationId: "DeleteProduct", tags: new[] { "Product" })]
    [OpenApiParameter("id", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "Product ID")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Product deleted")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Product not found")]
    public async Task<HttpResponseData> DeleteProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "products/{id}")] HttpRequestData req, int id)
    {
        _logger.LogStart<ProductApi>();
        try
        {
            var existingProduct = await _repository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            await _repository.DeleteAsync(id);
            _logger.LogSuccess().LogFinish<ProductApi>();
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogFailed(ex, "Error deleting product");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }

    [Function("GetLowStockProducts")]
    [OpenApiOperation(operationId: "GetLowStockProducts", tags: new[] { "Product" })]
    [OpenApiParameter(name: "threshold", In = ParameterLocation.Query, Required = true, Type = typeof(int), Description = "Stock threshold")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<LowStockProductDto>), Description = "List of low stock products with supplier info")]
    public async Task<HttpResponseData> GetLowStockProducts(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products/low-stock")] HttpRequestData req)
    {
        _logger.LogStart<ProductApi>();
        try
        {
            var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            if (!int.TryParse(queryParams["threshold"], out int threshold))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var product = await _repository.GetLowStockProductsAsync(threshold);

            _logger.LogSuccess().LogFinish<ProductApi>();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(product);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogFailed(ex, "Error retrieving low stock products");
            return req.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}