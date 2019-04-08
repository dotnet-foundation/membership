using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Extension class enabling adding the CookieBasedTokenCache implementation service
    /// </summary>
    public static class InMemoryTokenCacheExtension
    {
        /// <summary>
        /// Add the token acquisition service.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>the service collection</returns>
        public static IServiceCollection AddInMemoryTokenCache(this IServiceCollection services)
        {
            // Token acquisition service
            services.AddSingleton<ITokenCacheProvider, InMemoryTokenCacheProvider>();
            return services;
        }
    }

    /// <summary>
    /// Provides an implementation of <see cref="ITokenCacheProvider"/> for a cookie based token cache implementation
    /// </summary>
    class InMemoryTokenCacheProvider : ITokenCacheProvider
    {
        InMemoryTokenCacheHelper helper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cache"></param>
        public InMemoryTokenCacheProvider(IMemoryCache cache)
        {
            _memoryCache = cache;
        }

        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// Get an MSAL.NET Token cache from the HttpContext, and possibly the AuthenticationProperties and Cookies sign-in scheme
        /// </summary>
        /// <param name="httpContext">HttpContext</param>
        /// <param name="authenticationProperties">Authentication properties</param>
        /// <param name="signInScheme">Sign-in scheme</param>
        /// <returns>A token cache to use in the application</returns>

        public TokenCache GetCache(HttpContext httpContext, ClaimsPrincipal claimsPrincipal, AuthenticationProperties authenticationProperties, string signInScheme)
        {
            string userId = claimsPrincipal.GetMsalAccountId();
            helper = new InMemoryTokenCacheHelper(userId, httpContext, _memoryCache);
            return helper.GetMsalCacheInstance();
        }
    }

    public class InMemoryTokenCacheHelper
    {
        private readonly string _cacheId;
        private readonly IMemoryCache _memoryCache;

        private readonly TokenCache _cache = new TokenCache();

        public InMemoryTokenCacheHelper(string userId, HttpContext httpContext, IMemoryCache aspnetInMemoryCache)
        {
            // not object, we want the SUB
            _cacheId = userId + "_TokenCache";
            _memoryCache = aspnetInMemoryCache;
            Load();
        }

        public TokenCache GetMsalCacheInstance()
        {
            _cache.SetBeforeAccess(BeforeAccessNotification);
            _cache.SetAfterAccess(AfterAccessNotification);
            Load();
            return _cache;
        }

        public void Load()
        {
            if (_memoryCache.TryGetValue(_cacheId, out byte[] blob))
            {
                _cache.Deserialize(blob);
            }
        }

        public void Persist()
        {
            // Optimistically set HasStateChanged to false. We need to do it early to avoid losing changes made by a concurrent thread.
            _cache.HasStateChanged = false;

            // Reflect changes in the persistent store
            byte[] blob = _cache.Serialize();
            _memoryCache.Set(_cacheId, blob);
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (_cache.HasStateChanged)
            {
                Persist();
            }
        }
    }
}
