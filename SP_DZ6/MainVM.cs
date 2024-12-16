using Microsoft.Win32;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SP_DZ6
{
    public class MainVM : BindableBase
    {

        private List<CopiedFile>? copiedFiles;
        private List<Task> tasks = new();
        private CancellationTokenSource cancel;
        private string sourceDirectory = "";
        private string destinationDirectory = "";

        private long totalBytesCopied = 0;
        private long totalBytesToCopy = 0;
        private readonly object lockObj = new();

        private const int FileCount = 4;

        private bool copyCanStart = true;

        public string SourceDirectory
        {
            get => sourceDirectory;
            set
            {
                SetProperty(ref sourceDirectory, value);
            }
        }

        public string DestinationDirectory
        {
            get => destinationDirectory;
            set
            {
                SetProperty(ref destinationDirectory, value);
            }
        }

        public bool CopyCanStart
        {
            get
            {
                return copyCanStart;
            }
            set
            {
                SetProperty(ref copyCanStart, value);
                RaisePropertyChanged(nameof(CancelButtonEnabled));
            }
        }

        public bool CancelButtonEnabled 
        {
            get => !CopyCanStart;
        }

        public double TotalProgress
        {
            get
            {
                if (totalBytesToCopy == 0)
                {
                    return 0;
                }
                return (double)totalBytesCopied / totalBytesToCopy;
            }
        }

        public MainVM()
        {
            StartCopyCommand = new DelegateCommand(StartCopy);
            ChoiseSourceFolderCommand = new DelegateCommand(ChoiseSourceFolder);
            ChoiseDestinationFolderCommand = new DelegateCommand(ChoiseDestinationFolder);
            CancelCommand = new DelegateCommand(Cancel);
            copiedFiles = [];
            //copiedFiles.Add(new CopiedFile("", 0));
        }

        #region "File Names"
        public string FileName1
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 1)
                {
                    return "";
                }
                return copiedFiles[0].FileName;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 1)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(FileName1));
                copiedFiles[0].FileName = value;
            }
        }
        public string FileName2
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 2)
                {
                    return "";
                }
                return copiedFiles[1].FileName;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 2)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(FileName2));
                copiedFiles[1].FileName = value;
            }
        }
        public string FileName3
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 3)
                {
                    return "";
                }
                return copiedFiles[2].FileName;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 3)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(FileName3));
                copiedFiles[2].FileName = value;
            }
        }
        public string FileName4
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 4)
                {
                    return "";
                }
                return copiedFiles[3].FileName;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 4)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(FileName4));
                copiedFiles[3].FileName = value;
            }
        }
        #endregion

        #region "Progresses"

        public double Progress1
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 1)
                {
                    return 0;
                }
                return copiedFiles[0].Progress;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 1)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(Progress1));
                copiedFiles[0].Progress = value;
            }
        }
        public double Progress2
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 2)
                {
                    return 0;
                }
                return copiedFiles[1].Progress;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 2)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(Progress2));
                copiedFiles[1].Progress = value;
            }
        }
        public double Progress3
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 3)
                {
                    return 0;
                }
                return copiedFiles[2].Progress;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 3)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(Progress3));
                copiedFiles[2].Progress = value;
            }
        }
        public double Progress4
        {
            get
            {
                if (copiedFiles is null || copiedFiles.Count < 4)
                {
                    return 0;
                }
                return copiedFiles[3].Progress;
            }
            set
            {
                if (copiedFiles is null || copiedFiles.Count < 4)
                {
                    throw new IndexOutOfRangeException();
                }
                RaisePropertyChanged(nameof(Progress4));
                copiedFiles[3].Progress = value;
            }
        }

        #endregion

        #region "Commands and Functions"

        public ICommand CancelCommand { get; }
        private void Cancel() 
        {
            cancel.Cancel();
        }

        public ICommand ChoiseSourceFolderCommand { get; }
        private void ChoiseSourceFolder()
        {
            OpenFolderDialog dialog = new();
            if (dialog.ShowDialog() == true)
            {
                SourceDirectory = dialog.FolderName;
            }
        }

        public ICommand ChoiseDestinationFolderCommand { get; }
        private void ChoiseDestinationFolder()
        {
            OpenFolderDialog dialog = new();
            if (dialog.ShowDialog() == true)
            {
                DestinationDirectory = dialog.FolderName;
            }
        }

        public ICommand StartCopyCommand { get; }
        private void StartCopy()
        {
            cancel = new();
            if (SourceDirectory == "" || DestinationDirectory == "" || copiedFiles is null || !CopyCanStart)
            {
                return;
            }
            totalBytesCopied = 0;
            totalBytesToCopy = 0;
            CopyCanStart = false;

            List<string> filesToCopy = Directory.GetFiles(SourceDirectory).ToList();
            for (int i = 0; i < filesToCopy.Count; i++)
            {
                copiedFiles!.Add(new CopiedFile(filesToCopy[i], 0));
                totalBytesToCopy += new FileInfo(filesToCopy[i]).Length;
            }
            RaisePropertyChanged(nameof(FileName1));
            RaisePropertyChanged(nameof(FileName2));
            RaisePropertyChanged(nameof(FileName3));
            RaisePropertyChanged(nameof(FileName4));
            RaisePropertyChanged(nameof(TotalProgress));
            for (int i = 0; i < FileCount; i++)
            {
                int index = i;
                tasks.Add(Task.Run(() => CopyFile(filesToCopy[index], Path.Combine(DestinationDirectory, Path.GetFileName(filesToCopy[index])), index, cancel.Token), cancel.Token));
            }
            Task copiedAllFiles;
            try
            {
                copiedAllFiles = Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            copiedAllFiles.ContinueWith(allCompleted =>
            {
                CopyCanStart = true;
                MessageBox.Show("Копирование завершено");
            });
        }

        private void CopyFile(string sourceFilePath, string destinationFilePath, int index, CancellationToken token)
        {
            using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
            using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                long fileSize = sourceStream.Length;
                long bytesCopied = 0;

                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    Thread.Sleep(100);
                    destinationStream.Write(buffer, 0, bytesRead);
                    bytesCopied += bytesRead;                                      

                    lock (lockObj)
                    {
                        totalBytesCopied += bytesRead;
                        RaisePropertyChanged(nameof(TotalProgress));
                    }
                    double fileProgress = (double)bytesCopied / fileSize;
                    if (token.IsCancellationRequested)
                    {
                        fileProgress = 0;
                        destinationStream.Close();
                        File.Delete(destinationFilePath);
                        totalBytesCopied = 0;
                        RaisePropertyChanged(nameof(TotalProgress));
                        UpdateProccessesByIndex(index, fileProgress);
                        return;
                    }
                    UpdateProccessesByIndex(index, fileProgress);
                    throw new UnauthorizedAccessException();
                }
            }
        }
        #endregion

        private void UpdateProccessesByIndex(int index, double fileProgress) 
        {
            switch (index)
            {
                case 0:
                    Progress1 = fileProgress;
                    break;
                case 1:
                    Progress2 = fileProgress;
                    break;
                case 2:
                    Progress3 = fileProgress;
                    break;
                case 3:
                    Progress4 = fileProgress;
                    break;
            }
        }
    }
}
