using System;
using System.Net;
using System.Threading.Tasks;
using CF.CustomerMngt.Application.Dtos;
using CF.CustomerMngt.Application.Facades.Interfaces;
using CF.CustomerMngt.Domain.Exceptions;
using CorrelationId;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace CF.Api.Controllers
{
    [Route("api/v1/customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly string _correlationId;
        private readonly ICustomerFacade _customerFacade;

        public CustomerController(ICorrelationContextAccessor correlationContext, ILogger<CustomerController> logger,
            ICustomerFacade customerFacade)
        {
            _logger = logger;
            _correlationId = correlationContext.CorrelationContext.CorrelationId;
            _customerFacade = customerFacade;
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, "Customer successfully returned.", typeof(PaginationDto<CustomerResponseDto>))]
        public async Task<ActionResult<PaginationDto<CustomerResponseDto>>> Get([FromQuery] CustomerFilterDto customerFilterDto)
        {
            try
            {
                var result = await _customerFacade.GetListByFilter(customerFilterDto);
                return result;
            }
            catch (ValidationException e)
            {
                _logger.LogError($"Exception Details: {e.Message}, {e.InnerException}, {e.StackTrace}. CorrelationId: {_correlationId}");
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{id}")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Customer not found.")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Customer successfully returned.")]
        public async Task<ActionResult<CustomerResponseDto>> Get(long id)
        {
            try
            {
                if (id <= 0) return BadRequest("Invalid Id.");

                var filter = new CustomerFilterDto {Id = id};
                var result = await _customerFacade.GetByFilter(filter);

                if (result == null) return NotFound();

                return result;
            }
            catch (ValidationException e)
            {
                _logger.LogError($"Exception Details: {e.Message}, {e.InnerException}, {e.StackTrace}. CorrelationId: {_correlationId}");
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid id.")]
        [SwaggerResponse((int)HttpStatusCode.Created, "Customer has been created successfully.")]
        public async Task<IActionResult> Post([FromBody] CustomerRequestDto customerRequestDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var id = await _customerFacade.Create(customerRequestDto);

                return CreatedAtAction(nameof(Get), new {id}, new {id});
            }
            catch (ValidationException e)
            {
                _logger.LogError($"Exception Details: {e.Message}, {e.InnerException}, {e.StackTrace}. CorrelationId: {_correlationId}");
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{id}")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Customer not found")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "Customer has been updated successfully.")]
        public async Task<IActionResult> Put(long id, [FromBody] CustomerRequestDto customerRequestDto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                if (id <= 0) return BadRequest("Invalid id.");

                await _customerFacade.Update(id, customerRequestDto);
                return NoContent();
            }
            catch (EntityNotFoundException e)
            {
                _logger.LogError($"Exception Details: {e.Message}, {e.InnerException}, {e.StackTrace}. CorrelationId: {_correlationId}");
                return NotFound();
            }
            catch (ValidationException e)
            {
                _logger.LogError($"Exception Details: {e.Message}, {e.InnerException}, {e.StackTrace}. CorrelationId: {_correlationId}");
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid id.")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Customer not found.")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "Customer has been deleted successfully.")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                if (id <= 0) return BadRequest("Invalid id.");

                await _customerFacade.Delete(id);
                
                return NoContent();
            }
            catch (EntityNotFoundException e)
            {
                _logger.LogError($"Exception Details: {e.Message}, {e.InnerException}, {e.StackTrace}. CorrelationId: {_correlationId}");
                return NotFound();
            }
            catch (ValidationException e)
            {
                _logger.LogError($"Exception Details: {e.Message}, {e.InnerException}, {e.StackTrace}. CorrelationId: {_correlationId}");
                return BadRequest(e.Message);
            }
        }
    }
}
