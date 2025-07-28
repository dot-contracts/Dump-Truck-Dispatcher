using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using DispatcherWeb.Caching;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherWeb.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheDiagnosticController : DispatcherWebControllerBase
    {
        private readonly ISettingManager _settingManager;
        private readonly ListCacheCollection _listCaches;

        public CacheDiagnosticController(
            ISettingManager settingManager,
            ListCacheCollection listCaches)
        {
            _settingManager = settingManager;
            _listCaches = listCaches;
        }

        [HttpGet("test")]
        public async Task<ActionResult<string>> TestCacheConfiguration()
        {
            var results = new List<string>();
            
            try
            {
                results.Add("=== Cache Configuration Diagnostic ===");
                
                // Test if SettingManager is available
                if (_settingManager == null)
                {
                    results.Add("❌ SettingManager is null");
                    return Ok(string.Join("\n", results));
                }
                
                results.Add("✅ SettingManager is available");
                
                // Test global cache setting
                try
                {
                    var globalCacheEnabled = await _settingManager.GetSettingValueAsync<bool>("App.ListCaches.GlobalCacheEnabled");
                    results.Add($"✅ Global cache enabled: {globalCacheEnabled}");
                }
                catch (Exception ex)
                {
                    results.Add($"❌ Global cache setting error: {ex.Message}");
                }
                
                // Test a few key cache settings
                var cacheNames = new[] { "OrderListCache", "DriverListCache", "TruckListCache", "ItemListCache" };
                
                foreach (var cacheName in cacheNames)
                {
                    try
                    {
                        var backendEnabled = await _settingManager.GetSettingValueAsync<bool>(
                            $"App.ListCaches.{cacheName}.Backend.IsEnabled");
                        var frontendEnabled = await _settingManager.GetSettingValueAsync<bool>(
                            $"App.ListCaches.{cacheName}.Frontend.IsEnabled");
                        
                        results.Add($"✅ {cacheName}: Backend={backendEnabled}, Frontend={frontendEnabled}");
                    }
                    catch (Exception ex)
                    {
                        results.Add($"❌ {cacheName}: Error - {ex.Message}");
                    }
                }
                
                // Test ListCacheCollection
                if (_listCaches != null)
                {
                    results.Add("✅ ListCacheCollection is available");
                    
                    // Test a few cache instances
                    try
                    {
                        var orderCacheEnabled = await _listCaches.Order.IsEnabled();
                        results.Add($"✅ OrderListCache.IsEnabled() = {orderCacheEnabled}");
                    }
                    catch (Exception ex)
                    {
                        results.Add($"❌ OrderListCache.IsEnabled() failed: {ex.Message}");
                    }
                }
                else
                {
                    results.Add("❌ ListCacheCollection is null");
                }
            }
            catch (Exception ex)
            {
                results.Add($"❌ Diagnostic failed: {ex.Message}");
            }
            
            return Ok(string.Join("\n", results));
        }

        [HttpGet("enable")]
        public async Task<ActionResult<string>> EnableGlobalCache()
        {
            try
            {
                await _settingManager.ChangeSettingForApplicationAsync("App.ListCaches.GlobalCacheEnabled", "true");
                return Ok("Global cache enabled successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to enable global cache: {ex.Message}");
            }
        }

        [HttpGet("disable")]
        public async Task<ActionResult<string>> DisableGlobalCache()
        {
            try
            {
                await _settingManager.ChangeSettingForApplicationAsync("App.ListCaches.GlobalCacheEnabled", "false");
                return Ok("Global cache disabled successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to disable global cache: {ex.Message}");
            }
        }
    }
} 