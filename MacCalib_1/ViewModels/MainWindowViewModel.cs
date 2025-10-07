using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Xml;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.Reflection;

namespace MacCalib_1.ViewModels;

    public class Param
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public Param(string name, string value) 
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Call this whenever something changes that affects whether the command can execute.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }


     public partial class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public ObservableCollection<Param> ParamsList { get; } = new ObservableCollection<Param>();
        public ICommand AddParamCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SaveJsonAndExitCommand { get; }

        private string _windowTitle; 
        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
        private string _paramName = string.Empty;
        public string ParamName
        {
            get { return _paramName; }
            set
            {
                _paramName = value;
                OnPropertyChanged(nameof(ParamName));
            }
        }

        private string _paramValue = string.Empty;

        public string ParamValue
        {
            get { return _paramValue; }
            set
            {
                _paramValue = value;
                OnPropertyChanged(nameof(ParamValue));
            }
        }

        private string _errorStr = string.Empty;

        public string ErrorStr
        {
            get { return _errorStr; }
            set
            {
                _errorStr = value;
                OnPropertyChanged(nameof(ErrorStr));
            }
        }

        private string _selectedResult = string.Empty;

        public string SelectedResult
        {
            get => _selectedResult;
            set
            {
                if (_selectedResult != value)
                {
                    _selectedResult = value;
                    OnPropertyChanged(nameof(SelectedResult));
                }
            }
        }

    public MainWindowViewModel()
    {
        AddParamCommand = new RelayCommand(AddParamToList);
        CancelCommand = new RelayCommand(OnCancel);
        SaveJsonAndExitCommand = new RelayCommand(OnSaveJsonAndExit);

        // Get the currently executing assembly 
        var assembly = Assembly.GetExecutingAssembly();

        // Get file version info 
        var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        string? fileVersion = fvi?.FileVersion;

        // Get the Description attribute 
        var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()? .Description ?? string.Empty;

        WindowTitle = description + " - " + (fileVersion == null ? string.Empty : fileVersion);
    }

    private void OnCancel(object parameter)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();   // ✅ this closes the app
        }
    }

    private void OnSaveJsonAndExit(object parameter)
    {
        //save json in ../Output folder

        ParamsList.Add(new Param("Result", SelectedResult));
        ParamsList.Add(new Param("Error", ErrorStr));

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in ParamsList)
        {
            dict[p.Name] = p.Value;
        }

        string json = JsonConvert.SerializeObject(dict, Newtonsoft.Json.Formatting.Indented);

        Console.WriteLine(json);

        string outputFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"../Output"));
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        File.WriteAllText(Path.Combine(outputFolder, "output.json"), json);

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();   // ✅ this closes the app
        }
    }
        private void AddParamToList(object parameter)
        {
            ParamsList.Add(new Param(ParamName, ParamValue));
            ParamName = string.Empty;
            ParamValue = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
