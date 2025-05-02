using System.Net;
using System;
using DataStore.Abstraction.Exceptions;

namespace AzureCosmosDBToSqlServerProject.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
           try

            {
                await _next(context);
            }

            catch (DatabaseExceptions dbEx)

            {

                _logger.LogError($"Database Exception: {dbEx.Message}");

                await HandleDatabaseExceptionAsync(context, dbEx);

            }

            catch (NullExceptions nullEx)

            {

                _logger.LogError($"Null Exception: {nullEx.Message}");

                await HandleNullExceptionAsync(context, nullEx);
            }

           

            catch (OperationFailureException opEx)

            {

                _logger.LogError($"Operation Failure Exception: {opEx.Message}");

                await HandleOpeartionFailureAsync(context, opEx);

            }

            catch (CosmosDBExceptions ex)
            {
                _logger.LogError($"Cosmos DB Exception: {ex.Message}");
                await HandleCosmosDBExceptionAsync(context, ex);
            }

            catch (Exception ex)

            {

                _logger.LogError($"General Exception: {ex.Message}");

                await HandleGeneralExceptionAsync(context, ex);

            }

        }

        private static Task HandleDatabaseExceptionAsync(HttpContext context, DatabaseExceptions dbEx)

        {

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(new

            {

                StatusCode = context.Response.StatusCode,

                Error = "Database Error",

                Message = dbEx.Message

            }.ToString());

        }

        private static Task HandleNullExceptionAsync(HttpContext context, NullExceptions nullEx)

        {

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            return context.Response.WriteAsync(new

            {

                StatusCode = context.Response.StatusCode,

                Error = "Null Value Error",

                Message = nullEx.Message
            }.ToString());
        }

    

        private static Task HandleOpeartionFailureAsync(HttpContext context, OperationFailureException opEx)

        {

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(new

            {

                StatusCode = context.Response.StatusCode,

                Error = "Operation Failure Error",

                Message = opEx.Message

            }.ToString());

        }

        public static Task HandleCosmosDBExceptionAsync(HttpContext context, CosmosDBExceptions cosmosEx)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsync(new
            {
                StatusCode = context.Response.StatusCode,
                Error = "Cosmos DB Error",
                Message = cosmosEx.Message
            }.ToString());
        }

        private static Task HandleGeneralExceptionAsync(HttpContext context, Exception ex)

        {

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            return context.Response.WriteAsync(new

            {

                StatusCode = context.Response.StatusCode,

                Error = "An unexpected error occurred",

                Message = "Please try again later."

            }.ToString());

        }



    }
}
