using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Npgsql;

using Contracts.Http;

namespace Api.Controllers;

public class BaseController : ControllerBase
{
    protected async Task<IActionResult> SafeExecute(Func<Task<IActionResult>> action, CancellationToken cansellationToken)
    {
        try
        {
            return await action();
        }
        catch (InvalidOperationException ioe) when (ioe.InnerException is NpgsqlException)
        {
            var response = new ErrorResponse
            {
                Code = ErrorCode.DbFailureError,
                Message = "DB failure"
            };

            return ToActionResult(response);
        }
        catch (Exception)
        {
            var response = new ErrorResponse
            {
                Code = ErrorCode.InternalServerError,
                Message = "Unhandled error"
            };

            return ToActionResult(response);
        }
    }

    protected IActionResult ToActionResult(ErrorResponse errorResponse)
    {
        return StatusCode((int)errorResponse.Code / 100, errorResponse);
    }
}