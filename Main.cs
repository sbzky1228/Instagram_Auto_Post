using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Configuration - In production, use secure config
        string accessToken = Environment.GetEnvironmentVariable("INSTAGRAM_ACCESS_TOKEN");
        string igUserId = Environment.GetEnvironmentVariable("INSTAGRAM_USER_ID");

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(igUserId))
        {
            Console.WriteLine("Error: INSTAGRAM_ACCESS_TOKEN and INSTAGRAM_USER_ID environment variables must be set.");
            return;
        }

        Logger logger = new Logger();
        PostQueue queue = new PostQueue();
        InstagramPost poster = new InstagramPost(accessToken, igUserId, logger);

        // Check if we should add new posts from folders
        var checker = new CheckTarget();
        if (checker.HasRecipes())
        {
            Console.WriteLine($"📦 Found {checker.FolderCount} folders with recipes.");
            foreach (var recipe in checker.Recipes)
            {
                string caption = File.ReadAllText(recipe.TextPath);
                var post = new PostQueueItem
                {
                    Caption = caption,
                    SourceImageUrl = recipe.ImagePath, // Assuming it's a public URL; in reality, upload to S3
                    ScheduledAtJst = DateTime.UtcNow.AddHours(9) // Immediate for demo
                };
                queue.AddPost(post);
                Console.WriteLine($"Added post: {post.PostId}");
            }
        }

        // Process queued posts
        var queuedPosts = queue.GetQueuedPosts();
        if (queuedPosts.Count == 0)
        {
            Console.WriteLine("No queued posts to process.");
            return;
        }

        // Check publishing limit
        if (!await poster.CheckPublishingLimit())
        {
            Console.WriteLine("Publishing limit exceeded. Stopping.");
            return;
        }

        foreach (var post in queuedPosts)
        {
            await ProcessPost(post, poster, queue, logger);
        }
    }

    static async Task ProcessPost(PostQueueItem post, InstagramPost poster, PostQueue queue, Logger logger)
    {
        try
        {
            post.Status = PostStatus.CONTAINER_CREATED;
            string containerId = await poster.CreateMediaContainer(post.SourceImageUrl, post.Caption);
            post.IgContainerId = containerId;
            queue.UpdatePost(post);

            post.Status = PostStatus.PUBLISHING;
            string mediaId = await poster.PublishMedia(containerId);
            post.IgMediaId = mediaId;
            post.Status = PostStatus.PUBLISHED;
            queue.UpdatePost(post);

            logger.Log($"Post {post.PostId} published successfully.");
        }
        catch (Exception ex)
        {
            post.Attempts++;
            post.LastErrorMessage = ex.Message;
            if (post.Attempts >= 5) // Max retries
            {
                post.Status = PostStatus.FAILED;
            }
            else
            {
                post.Status = PostStatus.RETRY;
                // Implement exponential backoff here
            }
            queue.UpdatePost(post);
            logger.Log($"Post {post.PostId} failed: {ex.Message}");
        }
    }
}
