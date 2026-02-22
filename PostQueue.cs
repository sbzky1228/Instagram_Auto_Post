using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public enum PostStatus
{
    QUEUED,
    UPLOADED,
    CONTAINER_CREATED,
    PUBLISHING,
    PUBLISHED,
    FAILED,
    RETRY
}

public class PostQueueItem
{
    public string? PostId { get; set; }
    public string Type { get; set; } = "IMAGE";
    public string? Caption { get; set; }
    public string? LocalImagePath { get; set; }
    public string? SourceImageUrl { get; set; }
    public DateTime ScheduledAtJst { get; set; }
    public PostStatus Status { get; set; } = PostStatus.QUEUED;
    public string? MediaPublicUrl { get; set; }
    public string? IgContainerId { get; set; }
    public string? IgMediaId { get; set; }
    public int Attempts { get; set; } = 0;
    public string? LastErrorCode { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class PostQueue
{
    private readonly string queueFilePath = "post_queue.json";
    private List<PostQueueItem> queue = new List<PostQueueItem>();

    public PostQueue()
    {
        LoadQueue();
    }

    private void LoadQueue()
    {
        if (File.Exists(queueFilePath))
        {
            string json = File.ReadAllText(queueFilePath);
            queue = JsonSerializer.Deserialize<List<PostQueueItem>>(json) ?? new List<PostQueueItem>();
        }
    }

    private void SaveQueue()
    {
        string json = JsonSerializer.Serialize(queue, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(queueFilePath, json);
    }

    public void AddPost(PostQueueItem item)
    {
        item.PostId = Guid.NewGuid().ToString();
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        queue.Add(item);
        SaveQueue();
    }

    public List<PostQueueItem> GetQueuedPosts()
    {
        DateTime nowJst = DateTime.UtcNow.AddHours(9); // JST
        return queue.FindAll(p => p.Status == PostStatus.QUEUED && p.ScheduledAtJst <= nowJst);
    }

    public void UpdatePost(PostQueueItem item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        SaveQueue();
    }

    public PostQueueItem? GetPostById(string postId)
    {
        return queue.Find(p => p.PostId == postId);
    }
}