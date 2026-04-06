using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NVOAMASIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapsController(IConfiguration configuration, ILogger<MapsController> logger) : ControllerBase
    {
        [HttpGet("distance")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDrivingDistance([FromQuery] string origin, [FromQuery] string destination)
        {
            if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
                return BadRequest(new { message = "origin/destination is required" });

            try
            {
                // 1) Geocode origin and destination with Nominatim (OpenStreetMap)
                using var http = new HttpClient();
                http.DefaultRequestHeaders.UserAgent.ParseAdd("LMS-GS/1.0 (contact: admin@yourdomain)");

                async Task<(double lat, double lon)?> GeocodeAsync(string address)
                {
                    // Try Nominatim first
                    var geoUrl = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
                    var r = await http.GetAsync(geoUrl);
                    var b = await r.Content.ReadAsStringAsync();
                    if (r.IsSuccessStatusCode)
                    {
                        using var j = JsonDocument.Parse(b);
                        if (j.RootElement.ValueKind == JsonValueKind.Array && j.RootElement.GetArrayLength() > 0)
                        {
                            var first = j.RootElement[0];
                            var latStr = first.GetProperty("lat").GetString();
                            var lonStr = first.GetProperty("lon").GetString();
                            if (double.TryParse(latStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat)
                                && double.TryParse(lonStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon))
                            {
                                return (lat, lon);
                            }
                        }
                    }
                    else
                    {
                        logger.LogWarning("Nominatim failed: {Status} {Body}", r.StatusCode, b);
                    }

                    // Fallback: Photon (komoot)
                    var photonUrl = $"https://photon.komoot.io/api/?q={Uri.EscapeDataString(address)}&limit=1";
                    var r2 = await http.GetAsync(photonUrl);
                    var b2 = await r2.Content.ReadAsStringAsync();
                    if (r2.IsSuccessStatusCode)
                    {
                        using var j2 = JsonDocument.Parse(b2);
                        if (j2.RootElement.TryGetProperty("features", out var features)
                            && features.ValueKind == JsonValueKind.Array
                            && features.GetArrayLength() > 0)
                        {
                            var geom = features[0].GetProperty("geometry");
                            var coords = geom.GetProperty("coordinates");
                            if (coords.ValueKind == JsonValueKind.Array && coords.GetArrayLength() >= 2)
                            {
                                var lon = coords[0].GetDouble();
                                var lat = coords[1].GetDouble();
                                return (lat, lon);
                            }
                        }
                    }
                    else
                    {
                        logger.LogWarning("Photon failed: {Status} {Body}", r2.StatusCode, b2);
                    }

                    return null;
                }

                var orig = await GeocodeAsync(origin);
                var dest = await GeocodeAsync(destination);
                if (orig == null || dest == null)
                    return BadRequest(new { message = "Không tìm thấy tọa độ cho địa chỉ. Hãy thử nhập địa chỉ chuẩn hơn (số nhà, đường, quận, thành phố)." });

                // 2) Route via OSRM (public demo server)
                var osrmUrl = $"https://router.project-osrm.org/route/v1/driving/{orig.Value.lon.ToString(System.Globalization.CultureInfo.InvariantCulture)},{orig.Value.lat.ToString(System.Globalization.CultureInfo.InvariantCulture)};{dest.Value.lon.ToString(System.Globalization.CultureInfo.InvariantCulture)},{dest.Value.lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}?overview=false";
                var routeResp = await http.GetAsync(osrmUrl);
                var routeJson = await routeResp.Content.ReadAsStringAsync();
                if (!routeResp.IsSuccessStatusCode)
                {
                    logger.LogWarning("OSRM failed: {Status} {Body}", routeResp.StatusCode, routeJson);
                    return StatusCode((int)routeResp.StatusCode, new { message = "OSRM error" });
                }
                using var rdoc = JsonDocument.Parse(routeJson);
                var code = rdoc.RootElement.TryGetProperty("code", out var codeEl) ? codeEl.GetString() : null;
                if (!string.Equals(code, "Ok", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Không tìm thấy tuyến đường phù hợp từ OSRM", code });
                var routes = rdoc.RootElement.GetProperty("routes");
                if (routes.GetArrayLength() == 0)
                    return BadRequest(new { message = "No routes returned" });
                var distanceMeters = routes[0].GetProperty("distance").GetDouble();
                var km = distanceMeters / 1000.0;
                return Ok(new { kilometers = km });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error computing driving distance");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("distance-from-coordinates")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDrivingDistanceFromCoordinates(
            [FromQuery] double latitude, 
            [FromQuery] double longitude, 
            [FromQuery] string destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
                return BadRequest(new { message = "destination is required" });

            // Validate latitude range: -90 to 90
            if (latitude < -90 || latitude > 90)
                return BadRequest(new { message = "latitude must be between -90 and 90" });

            // Validate longitude range: -180 to 180
            if (longitude < -180 || longitude > 180)
                return BadRequest(new { message = "longitude must be between -180 and 180" });

            try
            {
                using var http = new HttpClient();
                http.DefaultRequestHeaders.UserAgent.ParseAdd("LMS-GS/1.0 (contact: admin@yourdomain)");

                async Task<(double lat, double lon)?> GeocodeAsync(string address)
                {
                    // Try Nominatim first
                    var geoUrl = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
                    var r = await http.GetAsync(geoUrl);
                    var b = await r.Content.ReadAsStringAsync();
                    if (r.IsSuccessStatusCode)
                    {
                        using var j = JsonDocument.Parse(b);
                        if (j.RootElement.ValueKind == JsonValueKind.Array && j.RootElement.GetArrayLength() > 0)
                        {
                            var first = j.RootElement[0];
                            var latStr = first.GetProperty("lat").GetString();
                            var lonStr = first.GetProperty("lon").GetString();
                            if (double.TryParse(latStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat)
                                && double.TryParse(lonStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon))
                            {
                                return (lat, lon);
                            }
                        }
                    }
                    else
                    {
                        logger.LogWarning("Nominatim failed: {Status} {Body}", r.StatusCode, b);
                    }

                    // Fallback: Photon (komoot)
                    var photonUrl = $"https://photon.komoot.io/api/?q={Uri.EscapeDataString(address)}&limit=1";
                    var r2 = await http.GetAsync(photonUrl);
                    var b2 = await r2.Content.ReadAsStringAsync();
                    if (r2.IsSuccessStatusCode)
                    {
                        using var j2 = JsonDocument.Parse(b2);
                        if (j2.RootElement.TryGetProperty("features", out var features)
                            && features.ValueKind == JsonValueKind.Array
                            && features.GetArrayLength() > 0)
                        {
                            var geom = features[0].GetProperty("geometry");
                            var coords = geom.GetProperty("coordinates");
                            if (coords.ValueKind == JsonValueKind.Array && coords.GetArrayLength() >= 2)
                            {
                                var lon = coords[0].GetDouble();
                                var lat = coords[1].GetDouble();
                                return (lat, lon);
                            }
                        }
                    }
                    else
                    {
                        logger.LogWarning("Photon failed: {Status} {Body}", r2.StatusCode, b2);
                    }

                    return null;
                }

                // Geocode destination address
                var dest = await GeocodeAsync(destination);
                if (dest == null)
                    return BadRequest(new { message = "Không tìm thấy tọa độ cho địa chỉ điểm đến. Hãy thử nhập địa chỉ chuẩn hơn (số nhà, đường, quận, thành phố)." });

                // Route via OSRM (public demo server)
                // Origin coordinates: longitude,latitude (already provided, no geocoding needed)
                var osrmUrl = $"https://router.project-osrm.org/route/v1/driving/{longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)};{dest.Value.lon.ToString(System.Globalization.CultureInfo.InvariantCulture)},{dest.Value.lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}?overview=false";
                var routeResp = await http.GetAsync(osrmUrl);
                var routeJson = await routeResp.Content.ReadAsStringAsync();
                if (!routeResp.IsSuccessStatusCode)
                {
                    logger.LogWarning("OSRM failed: {Status} {Body}", routeResp.StatusCode, routeJson);
                    return StatusCode((int)routeResp.StatusCode, new { message = "OSRM error" });
                }
                using var rdoc = JsonDocument.Parse(routeJson);
                var code = rdoc.RootElement.TryGetProperty("code", out var codeEl) ? codeEl.GetString() : null;
                if (!string.Equals(code, "Ok", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Không tìm thấy tuyến đường phù hợp từ OSRM", code });
                var routes = rdoc.RootElement.GetProperty("routes");
                if (routes.GetArrayLength() == 0)
                    return BadRequest(new { message = "No routes returned" });
                var distanceMeters = routes[0].GetProperty("distance").GetDouble();
                var km = distanceMeters / 1000.0;
                return Ok(new { kilometers = km });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error computing driving distance from coordinates");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}


