using System;
using System.Diagnostics;
using Godot;
using System.IO;
using System.IO.Compression;
using Environment = System.Environment;

public partial class DownloadPanel : Panel
{
    public delegate void ModChangedEvent();
    public event ModChangedEvent ModInstalled;
    public event ModChangedEvent ModUninstalled;
    
    [Export] public TextureButton InstallButton;
    [Export] public TextureButton BackButton;
    [Export] public TextureButton BackupButton;
    [Export] public TextureButton UninstallButton;
    [Export] public Label InstallLabel;
    [Export] public Label ElapsedTimeLabel;
    [Export] public Label CurrentStageLabel;
    
    private const string legendsDllFile = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Legends of Kingdom Rush\\legends_Data\\Managed\\Ironhide.Legends.dll";
    private const string backupDirectory = "user://Backups/";
    
    private const string masterGithub = "https://github.com/DippityDapper/lokr-modding/zipball/master";
    
    private HttpRequest http = null;
    
    private bool downloadCompleted = true;
    private float elapsedTime = 0;
    
    public override void _Ready()
    {
        base._Ready();
        InstallButton.Pressed += () =>
        {
            OnInstallMod();
            SoundManager.Instance.PlaySound("button_pressed");
        };
        BackupButton.Pressed += () =>
        {
            OpenBackupsFolder();
            SoundManager.Instance.PlaySound("button_pressed");
        };
        BackButton.Pressed += () =>
        {
            Main.Instance.GoToMainPanel();
            SoundManager.Instance.PlaySound("back_pressed");
        };
        UninstallButton.Pressed += () =>
        {
            OnUninstallMod(true);
            SoundManager.Instance.PlaySound("button_pressed");
        };
        
        ElapsedTimeLabel.Text = "";
        CurrentStageLabel.Text = "";
        InstallLabel.Text = Main.modInstalled ? "Reinstall" : "Install";
        UninstallButton.Disabled = !Main.modInstalled;
        
        DirAccess backupsDir = DirAccess.Open("user://");

        if (!backupsDir.DirExists("user://backups"))
            backupsDir.MakeDir("backups");
    }
    
    public override void _Process(double delta)
    {
        if (!downloadCompleted)
        {
            elapsedTime += (float)delta;
            ElapsedTimeLabel.Text = elapsedTime.ToString("0.00");
        }
        else
        {
            elapsedTime = 0;
            ElapsedTimeLabel.Text = "";
        }
        
    }

    private void OnInstallMod()
    {
        downloadCompleted = false;
        
        InstallButton.Disabled = true;
        BackButton.Disabled = true;
        UninstallButton.Disabled = true;
        
        DirAccess dir = DirAccess.Open("user://");
        if (!dir.DirExists("temp"))
            dir.MakeDir("temp");
        
        if (!dir.DirExists("bin"))
            dir.MakeDir("bin");
        
        DirAccess myGamesDir = DirAccess.Open(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        if (!myGamesDir.DirExists("My Games"))
            myGamesDir.MakeDir("My Games");

        if (Main.modInstalled)
            OnUninstallMod(false);
        
        Debug.Print("Installing...");
        CurrentStageLabel.Text = "Installing...";
        
        http = new HttpRequest();
        Main.Instance.AddChild(http);
        http.SetDownloadFile("user://temp/LOKR.zip");
        Error requestError = http.Request(masterGithub);
        
        if (requestError != Error.Ok)
            Debug.Print("Install request failed. Error status " + requestError);
        
        http.RequestCompleted += (result, code, headers, body) =>
        {
            if (result != (long)HttpRequest.Result.Success)
                Debug.Print("Install request received but failed. Error status " + (HttpRequest.Result)result);
            
            http.QueueFree();
            http = null;

            if (result == (long)HttpRequest.Result.Success)
            {
                Debug.Print("Mod Fetched...");
                CurrentStageLabel.Text = "Mod Fetched...";
                OnLokrZipDownloaded();
            }
            else
            {
                InstallButton.Disabled = false;
                BackButton.Disabled = false;
                UninstallButton.Disabled = false;
        
                Debug.Print("Finished Installing!");
                CurrentStageLabel.Text = "Error Installing.\nTell Dippity everything broke if this persists.";
                InstallLabel.Text = "Install";
                downloadCompleted = true;
            }
        };
    }

    private void OnLokrZipDownloaded()
    {
        Debug.Print("Lokr zip downloaded");
        string path = ProjectSettings.GlobalizePath("user://temp/LOKR.zip");
        string extractedPath = ProjectSettings.GlobalizePath("user://temp/LOKR");

        if (Directory.Exists(extractedPath))
        {
            try
            {
                Directory.Delete(extractedPath, true);
            }
            catch (Exception e)
            {
                Debug.Print("Could not delete temp LOKR folder.");
                Debug.Print(e.Message + "\n" + e.StackTrace);
            }
        }
        
        Debug.Print("Extracting Mod...");
        CurrentStageLabel.Text = "Extracting Mod...";
        try
        {
            ZipFile.ExtractToDirectory(path, extractedPath);
            Debug.Print("Mod Extracted...");
            CurrentStageLabel.Text = "Mod Extracted...";
        }
        catch (Exception e)
        {
            Debug.Print("Could not extract temp LOKR to user data.");
            Debug.Print(e.Message + "\n" + e.StackTrace);
        }
        
        string lokrExtractedFolder = "";
        try
        {
            lokrExtractedFolder = Directory.GetDirectories(extractedPath)[0];
        }
        catch (Exception e)
        {
            Debug.Print("Could not get directories in temp LOKR folder.");
            Debug.Print(e.Message + "\n" + e.StackTrace);
        }
        
        DirAccess dir = DirAccess.Open(lokrExtractedFolder);

        Error dllMoveError = dir.Rename(lokrExtractedFolder + "\\" + "Ironhide.Legends.dll", legendsDllFile);
        Error ogDllMoveError = dir.Rename(lokrExtractedFolder + "\\" + "Ironhide.Legends.OG.dll", ProjectSettings.GlobalizePath("user://bin") + "\\" + "Ironhide.Legends.OG.dll");
        Error LOKRFolderMoveError = dir.Rename(lokrExtractedFolder, GetModFolderPath());
        
        if (dllMoveError != Error.Ok)
            Debug.Print("Failed to move modded dll to " + legendsDllFile + " : Error status " + dllMoveError);
        if (ogDllMoveError != Error.Ok)
            Debug.Print("Failed to move original dll to bin." + "Error status " + ogDllMoveError);
        if (LOKRFolderMoveError != Error.Ok)
            Debug.Print("Failed to move LOKR folder to My Games. Error status " + LOKRFolderMoveError);
        
        try
        {
            Directory.Delete(ProjectSettings.GlobalizePath("user://temp"), true);
        }
        catch (Exception e)
        {
            Debug.Print("Could not delete temp folder.");
            Debug.Print(e.Message + "\n" + e.StackTrace);
        }
        
        InstallButton.Disabled = false;
        BackButton.Disabled = false;
        UninstallButton.Disabled = false;
        
        Debug.Print("Finished Installing!");
        CurrentStageLabel.Text = "Finished Installing!";
        InstallLabel.Text = "Update";
        downloadCompleted = true;
        ModInstalled?.Invoke();
    }

    private void OnUninstallMod(bool uninstalling)
    {
        if (uninstalling)
        {
            InstallButton.Disabled = true;
            BackButton.Disabled = true;
            UninstallButton.Disabled = true;
        }
        
        DirAccess dir = DirAccess.Open("user://");
        if (!dir.DirExists("bin"))
            dir.MakeDir("bin");
        
        dir = DirAccess.Open("user://bin");
        if (dir.FileExists("Ironhide.Legends.OG.dll"))
        {
            Debug.Print("Uninstalling Previous Version...");
            CurrentStageLabel.Text = "Uninstalling Previous Version...";
            
            CreateBackup();
            if (File.Exists(legendsDllFile))
            {
                try
                {
                    File.Delete(legendsDllFile);
                }
                catch (Exception e)
                {
                    Debug.Print("Could not delete 'Managed' dll.");
                    Debug.Print(e.Message + "\n" + e.StackTrace);
                }
            }
            
            Error dllCopyError = dir.Copy(ProjectSettings.GlobalizePath("user://bin/Ironhide.Legends.OG.dll"), legendsDllFile);
            
            if (dllCopyError != Error.Ok)
                Debug.Print("Failed to copy original dll to Managed folder : Error status " + dllCopyError);

            if (Directory.Exists(GetModFolderPath()))
            {
                try
                {
                    Directory.Delete(GetModFolderPath(), true);
                }
                catch (Exception e)
                {
                    Debug.Print("Could not delete LOKR folder in My Games.");
                    Debug.Print(e.Message + "\n" + e.StackTrace);
                }
            }
        }
        else
        {
            dir = DirAccess.Open("user://");
            if (!dir.DirExists("temp"))
                dir.MakeDir("temp");
            
            http = new HttpRequest();
            Main.Instance.AddChild(http);
            http.SetDownloadFile("user://temp/LOKR.zip");
            
            Debug.Print("Retrieving Original dll...");
            CurrentStageLabel.Text = "Retrieving Original dll...";
            Error requestError = http.Request(masterGithub);
            
            if (requestError != Error.Ok)
                Debug.Print("Error requesting to github (uninstall) " + masterGithub + " : Error status " + requestError);
            
            http.RequestCompleted += (result, code, headers, body) =>
            {
                if (result != (long)HttpRequest.Result.Success)
                    Debug.Print("Http request failed : Error status : " + (HttpRequest.Result)result);
                http.QueueFree();
                http = null;

                if (result == (long)HttpRequest.Result.Success)
                {
                    Debug.Print("Original dll Zip Received...");
                    CurrentStageLabel.Text = "Original dll Zip Received...";
                    OnFetchOriginalFiles();
                    OnUninstallMod(uninstalling);
                }
            };
            return;
        }

        if (uninstalling)
        {
            InstallButton.Disabled = false;
            BackButton.Disabled = false;
        }
        
        Debug.Print("Mod UnInstalled!");
        CurrentStageLabel.Text = "Mod UnInstalled!";
        InstallLabel.Text = "Install";
        ModUninstalled?.Invoke();
    }

    private void OnFetchOriginalFiles()
    {
        string path = ProjectSettings.GlobalizePath("user://temp/LOKR.zip");
        string extractedPath = ProjectSettings.GlobalizePath("user://temp/LOKR");

        if (Directory.Exists(extractedPath))
        {
            try
            {
                Directory.Delete(extractedPath, true);
            }
            catch (Exception e)
            {
                Debug.Print("Could not delete temp LOKR folder in user data.");
                Debug.Print(e.Message + "\n" + e.StackTrace);
            }
        }
        
        Debug.Print("Unzipping Original dll Zip...");
        CurrentStageLabel.Text = "Unzipping Original dll Zip...";
        try
        {
            ZipFile.ExtractToDirectory(path, extractedPath);
            Debug.Print("Original dll UnZipped...");
            CurrentStageLabel.Text = "Original dll UnZipped...";
        }
        catch (Exception e)
        {
            Debug.Print("Could not extract LOKR zip.");
            Debug.Print(e.Message + "\n" + e.StackTrace);
        }

        string lokrExtractedFolder = "";
        try
        {
            lokrExtractedFolder = Directory.GetDirectories(extractedPath)[0];
        }
        catch (Exception e)
        {
            Debug.Print("Failed to get directories within LOKR temp user folder.");
            Debug.Print(e.Message + "\n" + e.StackTrace);
        }
        
        DirAccess dir = DirAccess.Open(lokrExtractedFolder);
        Error ogTempDllMoveError = dir.Rename(lokrExtractedFolder + "\\" + "Ironhide.Legends.OG.dll", ProjectSettings.GlobalizePath("user://bin") + "\\" + "Ironhide.Legends.OG.dll");
        
        if (ogTempDllMoveError != Error.Ok)
            Debug.Print("Failed to move Original dll to bin in user data. Error status " + ogTempDllMoveError);
        
        try
        {
            Directory.Delete(ProjectSettings.GlobalizePath("user://temp"), true);
        }
        catch (Exception e)
        {
            Debug.Print("Could not delete temp folder in user data.");
            Debug.Print(e.Message + "\n" + e.StackTrace);
        }
    }

    private void CreateBackup()
    {
        DirAccess backupsDir = DirAccess.Open("user://");
        if (!backupsDir.DirExists("backups"))
            backupsDir.MakeDir("backups");
        
        backupsDir = DirAccess.Open("user://backups");
        if (Directory.Exists(GetModFolderPath()))
        {
            Debug.Print("Creating Backup...");
            CurrentStageLabel.Text = "Creating Backup...";

            string[] dirs = {""};
            try
            {
                dirs = backupsDir.GetDirectories();
            }
            catch (Exception e)
            {
                Debug.Print("Could not get directories within backups folder.");
                Debug.Print(e.Message + "\n" + e.StackTrace);
            }
            int dirLength = dirs.Length;
            if (dirLength > 0 && !dirs[0].Equals(""))
            {
                if (dirLength >= 10)
                {
                    string oldBackupPath = ProjectSettings.GlobalizePath("user://backups") + "\\" + dirs[0];
                    try
                    {
                        Directory.Delete(oldBackupPath, true);
                    }
                    catch (Exception e)
                    {
                        Debug.Print("Could not delete backup: " + oldBackupPath + "\n");
                        Debug.Print(e.Message + "\n" + e.StackTrace);
                    }
                }
            }
            else
            {
                Debug.Print("Failed to delete oldest backup.");
            }


            string nextBackupPath = "user://backups/backup_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            Error lokrBackupError = backupsDir.Rename(GetModFolderPath(), nextBackupPath);
            Error dllBackupError = backupsDir.Rename(legendsDllFile, nextBackupPath + "\\Ironhide.Legends.dll");

            if (lokrBackupError != Error.Ok)
                Debug.Print("LOKR folder backup could not be move to: " + nextBackupPath + " : Error status " + lokrBackupError);
            if (dllBackupError != Error.Ok)
                Debug.Print("Dll file could not be moved to: " + nextBackupPath + "\\Ironhide.Legends.dll" + " : Error status " + dllBackupError);

            if (lokrBackupError == Error.Ok && dllBackupError == Error.Ok)
            {
                Debug.Print("Backup Created...");
                CurrentStageLabel.Text = "Backup Created...";
            }
        }
        else
        {
            Debug.Print("Mod folder path does not exist! No backup will be created.");
        }
    }
    
    private void OpenBackupsFolder()
    {
        OS.ShellOpen(ProjectSettings.GlobalizePath(backupDirectory));
    }

    public static string GetModFolderPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "LOKR");
    }
}
