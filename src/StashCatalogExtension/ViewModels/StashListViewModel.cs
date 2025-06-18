using StashCatalogExtension.Models;
using StashCatalogExtension.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace StashCatalogExtension.ViewModels
{
    /// <summary>
    /// ViewModel for the stash list
    /// </summary>
    public class StashListViewModel : INotifyPropertyChanged
    {
        private readonly GitStashService _gitStashService;
        private readonly GitRepositoryService _gitRepositoryService;
        private readonly TraceSource _logger;
        private ObservableCollection<StashItem> _stashes = [];
        private bool _isLoading;
        private string? _errorMessage;
        private string? _repositoryPath;
        private string? _currentBranch;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the collection of stash items
        /// </summary>
        public ObservableCollection<StashItem> Stashes
        {
            get => _stashes;
            private set
            {
                _stashes = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the data is being loaded
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the error message to display
        /// </summary>
        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the current repository path
        /// </summary>
        public string? RepositoryPath
        {
            get => _repositoryPath;
            private set
            {
                _repositoryPath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the current branch name
        /// </summary>
        public string? CurrentBranch
        {
            get => _currentBranch;
            private set
            {
                _currentBranch = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StashListViewModel"/> class.
        /// </summary>
        public StashListViewModel(GitStashService gitStashService, GitRepositoryService gitRepositoryService, TraceSource logger)
        {
            _gitStashService = gitStashService;
            _gitRepositoryService = gitRepositoryService;
            _logger = logger;
            Stashes = [];
        }

        /// <summary>
        /// Refreshes the list of stashes
        /// </summary>
        public async Task RefreshStashesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                // Get the current repository path
                RepositoryPath = await _gitRepositoryService.GetCurrentRepositoryPathAsync();
                
                if (string.IsNullOrEmpty(RepositoryPath))
                {
                    ErrorMessage = "No Git repository found. Please open a Git repository.";
                    Stashes.Clear();
                    return;
                }

                // Get the current branch name
                CurrentBranch = await _gitRepositoryService.GetCurrentBranchAsync(RepositoryPath);

                // Get the list of stashes
                var stashes = await _gitStashService.GetStashesAsync(RepositoryPath);
                
                // Update the observable collection
                Stashes.Clear();
                foreach (var stash in stashes)
                {
                    Stashes.Add(stash);
                }

                // If there are no stashes, show a message
                if (Stashes.Count == 0)
                {
                    ErrorMessage = "No stashes found in this repository.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error refreshing stashes: {ex.Message}";
                _logger.TraceInformation($"Error in RefreshStashesAsync: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Called when a property value changes
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}