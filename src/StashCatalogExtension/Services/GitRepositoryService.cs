using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.ProjectSystem.Query;
using System.Diagnostics;

namespace StashCatalogExtension.Services
{
    /// <summary>
    /// Service for detecting and interacting with Git repositories in Visual Studio
    /// </summary>
    public class GitRepositoryService
    {
        private readonly TraceSource _logger;
        private readonly VisualStudioExtensibility _extensibility;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitRepositoryService"/> class.
        /// </summary>
        /// <param name="logger">Trace source for logging</param>
        /// <param name="extensibility">Extension API accessor</param>
        public GitRepositoryService(TraceSource logger, VisualStudioExtensibility extensibility)
        {
            _logger = logger;
            _extensibility = extensibility;
        }

        /// <summary>
        /// Gets the path to the current Git repository
        /// </summary>
        /// <returns>Path to the Git repository, or null if not found</returns>
        public async Task<string?> GetCurrentRepositoryPathAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var solutionPathQuery = await _extensibility.Workspaces().QuerySolutionAsync(
                    queryFunc: q => q.With(p => p.Path),
                    cancellationToken: cancellationToken
                );
                var solutionPath = solutionPathQuery.FirstOrDefault()?.Path;
                if (string.IsNullOrEmpty(solutionPath))
                {
                    _logger.TraceInformation("No active document or solution found");
                    return null;
                }

                // If this is a file path, get its directory
                if (File.Exists(solutionPath))
                {
                    solutionPath = Path.GetDirectoryName(solutionPath);
                }

                // Walk up directories to find .git folder
                string? currentPath = solutionPath;
                while (!string.IsNullOrEmpty(currentPath))
                {
                    string potentialGitDir = Path.Combine(currentPath, ".git");
                    if (Directory.Exists(potentialGitDir))
                    {
                        _logger.TraceInformation($"Found Git repository at {currentPath}");
                        return currentPath;
                    }

                    // Move up to the parent directory
                    DirectoryInfo? parentDir = Directory.GetParent(currentPath);
                    currentPath = parentDir?.FullName;
                }

                _logger.TraceInformation("No Git repository found for the current context");
                return null;
            }
            catch (Exception ex)
            {
                _logger.TraceInformation($"Error detecting Git repository: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the name of the current branch
        /// </summary>
        /// <param name="repoPath">Path to the Git repository</param>
        /// <returns>Name of the current branch, or null if not found</returns>
        public async Task<string?> GetCurrentBranchAsync(string repoPath)
        {
            try
            {
                if (string.IsNullOrEmpty(repoPath) || !Directory.Exists(repoPath))
                {
                    return null;
                }

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "symbolic-ref --short HEAD",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = repoPath
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                // The output contains the branch name with a newline at the end
                string branch = output.Trim();
                if (!string.IsNullOrEmpty(branch))
                {
                    return branch;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.TraceInformation($"Error getting current branch: {ex.Message}");
                return null;
            }
        }
    }
}