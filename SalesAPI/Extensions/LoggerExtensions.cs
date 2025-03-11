using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SalesAPI.Extensions
{
    public static class LoggerExtensions
    {
        /// <summary>
        /// Logs the start.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_logger">The logger.</param>
        /// <param name="caller">The caller.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns></returns>
        public static ILogger LogStart<T>(this ILogger _logger, [CallerMemberName] string caller = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            var className = typeof(T).Name;
            _logger.LogInformation($"Start {className}::{caller}");
            return _logger;
        }

        /// <summary>
        /// Logs the parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_logger">The logger.</param>
        /// <param name="expr">The expr.</param>
        /// <returns></returns>
        public static ILogger LogParam<T>(this ILogger _logger, Expression<Func<T>> expr)
        {
            var memberExpr = expr.Body as MemberExpression;

            if (memberExpr == null) return _logger;

            var varName = string.Empty;

            try
            {
                varName = expr.Body.ToString().Split(')')[1].Substring(1);
            }
            catch
            {
                varName = memberExpr.Member.Name;
            }

            var varData = expr.Compile()();

            _logger.LogInformation($">> {varName}: {JsonConvert.SerializeObject(varData, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}");
            return _logger;
        }

        /// <summary>
        /// Logs the success.
        /// </summary>
        /// <param name="_logger">The logger.</param>
        /// <param name="caller">The caller.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns></returns>
        public static ILogger LogSuccess(this ILogger _logger, [CallerMemberName] string caller = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            _logger.LogInformation($">> {caller}: Success");
            return _logger;
        }

        /// <summary>
        /// Logs the failed.
        /// </summary>
        /// <param name="_logger">The logger.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="caller">The caller.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns></returns>
        public static void LogFailed(this ILogger _logger, string errorMessage, [CallerMemberName] string caller = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            _logger.LogInformation($">> {caller}: Failed with error: {errorMessage}");
        }

        /// <summary>
        /// Logs the failed.
        /// </summary>
        /// <param name="_logger">The logger.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="caller">The caller.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns></returns>
        public static ILogger LogFailed(this ILogger _logger, Exception ex, [CallerMemberName] string caller = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            _logger.LogInformation($">> {caller}: Failed with error: {ex}");
            return _logger;
        }

        /// <summary>
        /// Logs the finish.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_logger">The logger.</param>
        /// <param name="caller">The caller.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns></returns>
        public static ILogger LogFinish<T>(this ILogger _logger, [CallerMemberName] string caller = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            var className = typeof(T).Name;
            _logger.LogInformation($"Finish {className}::{caller}");
            return _logger;
        }
    }
}
