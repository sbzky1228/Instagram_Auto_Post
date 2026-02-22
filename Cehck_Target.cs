using System;
using System.Collections.Generic;
using System.IO;

public class RecipeInfo
{
    public string? ImagePath { get; set; }
    public string? TextPath { get; set; }
}

public class CheckTarget
{
    public string? BaseFolderPath { get; private set; }
    public List<string> FolderNames { get; private set; }
    public int FolderCount { get; private set; }
    public List<RecipeInfo> Recipes { get; private set; }

    public CheckTarget()
    {
        BaseFolderPath = Environment.GetEnvironmentVariable("base_folder_path");
        FolderNames = new List<string>();
        Recipes = new List<RecipeInfo>();

        if (string.IsNullOrEmpty(BaseFolderPath) || !Directory.Exists(BaseFolderPath))
        {
            Console.WriteLine("⚠️ 対象フォルダが存在しません");
            return;
        }

        Load();
    }

    private void Load()
    {
        var folders = Directory.GetDirectories(BaseFolderPath);

        foreach (var folder in folders)
        {
            FolderNames.Add(Path.GetFileName(folder));
        }

        FolderCount = FolderNames.Count;

        if (FolderCount == 0)
        {
            Console.WriteLine("⚠️ 対象フォルダが存在しません");
            return;
        }

        Recipes = GetRecipeFilePaths(folders);
    }

    private List<RecipeInfo> GetRecipeFilePaths(string[] folders)
    {
        var recipeList = new List<RecipeInfo>();

        foreach (var folder in folders)
        {
            var jpgFiles = Directory.GetFiles(folder, "*.jpg");
            var txtFiles = Directory.GetFiles(folder, "*.txt");

            if (jpgFiles.Length > 0 && txtFiles.Length > 0)
            {
                recipeList.Add(new RecipeInfo
                {
                    ImagePath = jpgFiles[0],
                    TextPath = txtFiles[0]
                });
            }
            else
            {
                Console.WriteLine($"⚠️ {Path.GetFileName(folder)} に .jpg または .txt が見つかりませんでした");
            }
        }

        return recipeList;
    }

    public bool HasRecipes()
    {
        return FolderCount > 0;
    }
}
