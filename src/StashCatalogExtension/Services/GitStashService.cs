using StashCatalogExtension.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace StashCatalogExtension.Services
{
    /// <summary>
    /// Service for interacting with Git stash commands
    /// </summary>
    public class GitStashService
    {
        private static readonly Regex StashRegex = new(@"stash@\{(\d+)\}:\s*(.*?)(?:\s*on\s+([\w\d/-]+))?(?::\s*(.*))?$", RegexOptions.Compiled);
        private readonly TraceSource _logger;

        public GitStashService(TraceSource logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets a list of all stashes in the current repository
        /// </summary>
        /// <param name="gitPath">Path to the Git repository</param>
        /// <returns>A list of StashItem objects</returns>
        public async Task<IList<StashItem>> GetStashesAsync(string gitPath)
        {
            string output = await ExecuteGitCommandAsync(gitPath, "stash list");
            var stashList = new List<StashItem>();
            
            using (StringReader reader = new StringReader(output))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var stashItem = ParseStashLine(line);
                    if (stashItem != null)
                    {
                        // Get timestamp for the stash
                        DateTime timestamp = await GetStashDateAsync(gitPath, stashItem.Index);
                        stashItem.CreatedAt = timestamp;
                        
                        // Load custom name if it exists (would be stored in a metadata file)
                        stashItem.CustomName = await GetStashCustomNameAsync(gitPath, stashItem.Index);
                        
                        stashList.Add(stashItem);
                    }
                }
            }
            
            return stashList;
        }
        
        /// <summary>
        /// Parses a single stash line from 'git stash list' output
        /// </summary>
        /// <param name="line">Line from git stash list output</param>
        /// <returns>StashItem object or null if parsing failed</returns>
        private StashItem? ParseStashLine(string line)
        {
            var match = StashRegex.Match(line);
            if (!match.Success) 
                return null;
            
            int index = int.Parse(match.Groups[1].Value);
            string branchName = match.Groups[3].Success ? match.Groups[3].Value : string.Empty;
            string message = match.Groups[4].Success ? match.Groups[4].Value : match.Groups[2].Value;
            
            // Extract WIP message (the default stash message starts with "WIP on")
            bool isWIP = message.StartsWith("WIP on", StringComparison.OrdinalIgnoreCase);
            
            return new StashItem
            {
                Index = index,
                Name = $"stash@{{{index}}}",
                Message = message,
                BranchName = branchName,
                IsWIP = isWIP
            };
        }
        
        /// <summary>
        /// Gets the creation date of a stash by index
        /// </summary>
        /// <param name="repoPath">Path to the repository</param>
        /// <param name="stashIndex">Index of the stash</param>
        /// <returns>DateTime representing when the stash was created</returns>
        private async Task<DateTime> GetStashDateAsync(string repoPath, int stashIndex)
        {
            try
            {
                // Run git show to get the commit date from the stash
                string output = await ExecuteGitCommandAsync(repoPath, $"show -g --format=%at stash@{{{stashIndex}}}");
                if (string.IsNullOrWhiteSpace(output))
                    return DateTime.Now; // Fallback

                // Parse the Unix timestamp (first line of the output)
                if (long.TryParse(output.Split('\n').FirstOrDefault()?.Trim(), out long unixTime))
                {
                    // Convert Unix timestamp to DateTime
                    return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
                }
            }
            catch (Exception ex)
            {
                _logger.TraceInformation($"Error getting stash date: {ex.Message}");
            }

            return DateTime.Now; // Fallback
        }

        /// <summary>
        /// Gets the custom name for a stash, if it exists
        /// </summary>
        /// <param name="repoPath">Path to the repository</param>
        /// <param name="stashIndex">Index of the stash</param>
        /// <returns>Custom name or empty string if not found</returns>
        private async Task<string> GetStashCustomNameAsync(string repoPath, int stashIndex)
        {
            try
            {
                // This would read from a custom metadata file in the .git directory
                string metadataPath = Path.Combine(repoPath, ".git", "info", "stash-catalog.json");
                
                // For now we're returning empty strings
                // In a full implementation, this would load JSON data from the metadata file
                // and look up the custom name by stash index
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.TraceInformation($"Error loading stash metadata: {ex.Message}");
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Executes a git command and returns the output
        /// </summary>
        /// <param name="repoPath">Path to the git repository</param>
        /// <param name="arguments">Git command arguments</param>
        /// <returns>Command output as string</returns>
        private async Task<string> ExecuteGitCommandAsync(string repoPath, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = repoPath
                }
            };
            
            try
            {
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                return output;
            }
            catch (Exception ex)
            {
                _logger.TraceInformation($"Git command error: {ex.Message}");
                return string.Empty;
            }
        }
    }
}