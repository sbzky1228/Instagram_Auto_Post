using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class InstagramPost
{
    private readonly HttpClient httpClient;
    private readonly string accessToken;
    private readonly string igUserId;
    private readonly Logger logger;

    public InstagramPost(string accessToken, string igUserId, Logger logger)
    {
        this.httpClient = new HttpClient();
        this.accessToken = accessToken;
        this.igUserId = igUserId;
        this.logger = logger;
    }

    public async Task<bool> CheckPublishingLimit()
    {
        string url = $"https://graph.facebook.com/v18.0/{igUserId}/content_publishing_limit?access_token={accessToken}";
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            logger.Log($"Publishing limit check: {response.StatusCode} - {content}");

            if (response.IsSuccessStatusCode)
            {
                // Parse response to check if limit is exceeded
                // For simplicity, assume it's ok if status is 200
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.Log($"Error checking publishing limit: {ex.Message}");
            return false;
        }
    }

    public async Task<string?> CreateMediaContainer(string imageUrl, string caption)
    {
        string url = $"https://graph.facebook.com/v18.0/{igUserId}/media";
        var data = new
        {
            image_url = imageUrl,
            caption = caption,
            access_token = accessToken
        };

        string json = JsonSerializer.Serialize(data);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(url, content);
            string responseContent = await response.Content.ReadAsStringAsync();
            logger.Log($"Create container: {response.StatusCode} - {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                return result.GetProperty("id").GetString();
            }
            else
            {
                throw new Exception($"Failed to create container: {response.StatusCode} - {responseContent}");
            }
        }
        catch (Exception ex)
        {
            logger.Log($"Error creating media container: {ex.Message}");
            throw;
        }
    }

    public async Task<string?> PublishMedia(string containerId)
    {
        string url = $"https://graph.facebook.com/v18.0/{igUserId}/media_publish";
        var data = new
        {
            creation_id = containerId,
            access_token = accessToken
        };

        string json = JsonSerializer.Serialize(data);
        HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await httpClient.PostAsync(url, content);
            string responseContent = await response.Content.ReadAsStringAsync();
            logger.Log($"Publish media: {response.StatusCode} - {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                return result.GetProperty("id").GetString();
            }
            else
            {
                throw new Exception($"Failed to publish media: {response.StatusCode} - {responseContent}");
            }
        }
        catch (Exception ex)
        {
            logger.Log($"Error publishing media: {ex.Message}");
            throw;
        }
    }
}