using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NVOAMASIS.Services
{
    public class WebSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WebSearchService> _logger;

        public WebSearchService(HttpClient httpClient, ILogger<WebSearchService> logger)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
            _logger = logger;
        }

        /// <summary>
        /// Main search method - tries DuckDuckGo API first, falls back to HTML scraping
        /// </summary>
        public async Task<string> SearchAsync(string query, int maxResults = 5)
        {
            try
            {
                // Try DuckDuckGo Instant Answer API first
                var instantResult = await SearchDuckDuckGoAsync(query);
                if (!string.IsNullOrWhiteSpace(instantResult) && instantResult.Length > 50)
                {
                    return instantResult;
                }

                // Fallback to HTML scraping
                var htmlResults = await SearchDuckDuckGoHtmlAsync(query, maxResults);
                if (!string.IsNullOrWhiteSpace(htmlResults))
                {
                    return htmlResults;
                }

                return "Không tìm thấy kết quả phù hợp từ internet.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Web search failed for query: {Query}", query);
                return $"Lỗi khi tìm kiếm: {ex.Message}";
            }
        }

        /// <summary>
        /// DuckDuckGo Instant Answer API (free, no key needed)
        /// </summary>
        private async Task<string> SearchDuckDuckGoAsync(string query)
        {
            try
            {
                var url = $"https://api.duckduckgo.com/?q={Uri.EscapeDataString(query)}&format=json&no_html=1&skip_disambig=1";
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "NVOCC-LMS-Bot/1.0");

                var response = await _httpClient.GetStringAsync(url);
                var ddg = JsonSerializer.Deserialize<DdgResponse>(response);

                if (ddg == null) return "";

                var sb = new System.Text.StringBuilder();

                // Abstract (main answer)
                if (!string.IsNullOrWhiteSpace(ddg.Abstract))
                {
                    sb.AppendLine($"**{ddg.Heading}**");
                    sb.AppendLine(ddg.Abstract);
                    if (!string.IsNullOrWhiteSpace(ddg.AbstractURL))
                        sb.AppendLine($"Nguồn: {ddg.AbstractURL}");
                    sb.AppendLine();
                }

                // Answer
                if (!string.IsNullOrWhiteSpace(ddg.Answer))
                {
                    sb.AppendLine($"**Trả lời:** {ddg.Answer}");
                    sb.AppendLine();
                }

                // Infobox
                if (ddg.Infobox?.content != null && ddg.Infobox.content.Count > 0)
                {
                    sb.AppendLine("**Thông tin chi tiết:**");
                    foreach (var item in ddg.Infobox.content.Take(8))
                    {
                        if (!string.IsNullOrWhiteSpace(item.label) && !string.IsNullOrWhiteSpace(item.value))
                            sb.AppendLine($"- {item.label}: {item.value}");
                    }
                    sb.AppendLine();
                }

                // Related topics
                if (ddg.RelatedTopics != null && ddg.RelatedTopics.Count > 0)
                {
                    sb.AppendLine("**Chủ đề liên quan:**");
                    foreach (var topic in ddg.RelatedTopics.Take(5))
                    {
                        if (!string.IsNullOrWhiteSpace(topic.Text))
                            sb.AppendLine($"- {topic.Text}");
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DuckDuckGo API failed");
                return "";
            }
        }

        /// <summary>
        /// Scrape DuckDuckGo HTML results as fallback
        /// </summary>
        private async Task<string> SearchDuckDuckGoHtmlAsync(string query, int max = 5)
        {
            try
            {
                var url = $"https://html.duckduckgo.com/html/?q={Uri.EscapeDataString(query)}";
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                var html = await _httpClient.GetStringAsync(url);
                var results = ParseDdgHtmlResults(html, max);

                if (results.Count == 0) return "";

                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"**Kết quả tìm kiếm cho: \"{query}\"**\n");

                for (int i = 0; i < results.Count; i++)
                {
                    sb.AppendLine($"**{i + 1}. {results[i].Title}**");
                    sb.AppendLine($"   {results[i].Snippet}");
                    if (!string.IsNullOrWhiteSpace(results[i].Url))
                        sb.AppendLine($"   🔗 {results[i].Url}");
                    sb.AppendLine();
                }

                // Try to fetch content from top result for more detail
                if (results.Count > 0 && !string.IsNullOrWhiteSpace(results[0].Url))
                {
                    var pageContent = await FetchPageContentAsync(results[0].Url, 1500);
                    if (!string.IsNullOrWhiteSpace(pageContent))
                    {
                        sb.AppendLine("**Nội dung chi tiết từ kết quả đầu tiên:**");
                        sb.AppendLine(pageContent);
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DuckDuckGo HTML search failed");
                return "";
            }
        }

        /// <summary>
        /// Fetch and extract text from a web page
        /// </summary>
        private async Task<string> FetchPageContentAsync(string url, int maxChars = 1500)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return "";

                var html = await response.Content.ReadAsStringAsync();
                var text = ExtractTextFromHtml(html);

                return text.Length > maxChars ? text[..maxChars] + "..." : text;
            }
            catch
            {
                return "";
            }
        }

        private List<SearchResult> ParseDdgHtmlResults(string html, int max)
        {
            var results = new List<SearchResult>();
            try
            {
                // Match result blocks
                var resultPattern = new Regex(
                    @"<a[^>]*class=""result__a""[^>]*href=""([^""]*?)""[^>]*>(.*?)</a>.*?<a[^>]*class=""result__snippet""[^>]*>(.*?)</a>",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);

                var matches = resultPattern.Matches(html);
                foreach (Match m in matches)
                {
                    if (results.Count >= max) break;

                    var rawUrl = m.Groups[1].Value;
                    var title = StripHtml(m.Groups[2].Value);
                    var snippet = StripHtml(m.Groups[3].Value);

                    if (string.IsNullOrWhiteSpace(title)) continue;

                    var actualUrl = ExtractDdgUrl(rawUrl);

                    results.Add(new SearchResult
                    {
                        Title = title.Trim(),
                        Snippet = snippet.Trim(),
                        Url = actualUrl
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse DDG HTML");
            }
            return results;
        }

        private string ExtractDdgUrl(string ddgUrl)
        {
            try
            {
                if (ddgUrl.Contains("uddg="))
                {
                    var match = Regex.Match(ddgUrl, @"uddg=([^&]+)");
                    if (match.Success)
                        return Uri.UnescapeDataString(match.Groups[1].Value);
                }
                return ddgUrl;
            }
            catch
            {
                return ddgUrl;
            }
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            return Regex.Replace(html, "<[^>]+>", "").Trim();
        }

        private string ExtractTextFromHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";

            // Remove scripts, styles, nav, header, footer
            html = Regex.Replace(html, @"<(script|style|nav|header|footer|aside)[^>]*>.*?</\1>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            // Remove all tags
            var text = Regex.Replace(html, "<[^>]+>", " ");
            // Clean whitespace
            text = Regex.Replace(text, @"\s+", " ");
            // Decode HTML entities
            text = WebUtility.HtmlDecode(text);
            return text.Trim();
        }

        // Models
        public class SearchResult
        {
            public string Title { get; set; } = "";
            public string Snippet { get; set; } = "";
            public string Url { get; set; } = "";
        }

        public class DdgResponse
        {
            public string? Abstract { get; set; }
            public string? AbstractURL { get; set; }
            public string? Heading { get; set; }
            public string? Answer { get; set; }
            public string? AnswerType { get; set; }
            public DdgInfobox? Infobox { get; set; }
            public List<DdgTopic>? RelatedTopics { get; set; }
        }

        public class DdgTopic
        {
            public string? Text { get; set; }
            public string? FirstURL { get; set; }
        }

        public class DdgInfobox
        {
            public List<DdgInfoboxItem>? content { get; set; }
        }

        public class DdgInfoboxItem
        {
            public string? label { get; set; }
            public string? value { get; set; }
        }
    }
}
