// Diagnostic script to check cache configuration
// Add this to a controller or service to test cache functionality

public async Task<string> DiagnoseCacheConfiguration()
{
    var results = new List<string>();
    
    try
    {
        // Test if SettingManager is available
        if (_settingManager == null)
        {
            results.Add("❌ SettingManager is null");
            return string.Join("\n", results);
        }
        
        results.Add("✅ SettingManager is available");
        
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
    
    return string.Join("\n", results);
} 