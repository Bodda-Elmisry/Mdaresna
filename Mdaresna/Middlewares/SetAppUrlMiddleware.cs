using Mdaresna.Doamin.Helpers;

namespace Mdaresna.Middlewares
{
    public class SetAppUrlMiddleware
    {
        private readonly RequestDelegate next;

        public SetAppUrlMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            SettingsHelper.SetAppUrl($"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}");

            // Call the next middleware in the pipeline
            await next(context);
        }
    }
}
