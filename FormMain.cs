//============================================================================
// BDInfo - Blu-ray Video and Audio Analysis Tool
// Copyright © 2010 Cinema Squid
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace BDInfo {
    public class PlaylistInfo {
        public int Index;
        public int Group;
        public string Name;
        public TimeSpan Length;
        public ulong Size;

        public override string ToString() {
            String length = string.Format(
                "{0:D2}:{1:D2}:{2:D2}",
                Length.Hours,
                Length.Minutes,
                Length.Seconds);
            return String.Format("{0,-4:G}{1,-7}{2,-15}{3,-10}{4,-16}", Index.ToString(), (Group + 1).ToString(), Name, length, Size.ToString("N0"));
        }
    }

    public class ListPlaylistInfo : List<PlaylistInfo> {
        public override string ToString() {
            var report = "";
            report += String.Format("{0,-4}{1,-7}{2,-15}{3,-10}{4,-16}{5,-16}\n", "#", "Group", "Playlist File", "Length", "Estimated Bytes", "Measured Bytes");
            foreach (var i in this) {
                report += i.ToString() + "\n";
            }
            return report;
        }
    }

    public class FormMain {
        private BDROM BDROM = null;
        ScanBDROMResult ScanResult = new ScanBDROMResult();

        #region BDROM Initialization Worker

        public void InitBDROMWork(
            object sender,
            DoWorkEventArgs e
        ) {
            try {
                BDROM = new BDROM((string) e.Argument);
                // BDROM.StreamClipFileScanError += new BDROM.OnStreamClipFileScanError(BDROM_StreamClipFileScanError);
                // BDROM.StreamFileScanError += new BDROM.OnStreamFileScanError(BDROM_StreamFileScanError);
                // BDROM.PlaylistFileScanError += new BDROM.OnPlaylistFileScanError(BDROM_PlaylistFileScanError);
                BDROM.Scan();
                e.Result = null;
            }
            catch (Exception ex) {
                e.Result = ex;
            }
        }

        private void InitBDROMProgress(
            object sender,
            ProgressChangedEventArgs e
        ) { }

        #endregion

        #region File/Stream Lists

        List<TSPlaylistFile> selectedPlaylists = new List<TSPlaylistFile>();

        public void GenerateReportCLI(String savePath, Boolean quick = false) {
            if (ScanResult.ScanException != null) {
                System.Console.WriteLine(string.Format("{0}", ScanResult.ScanException.Message));
            } else {
                if (ScanResult.FileExceptions.Count > 0) {
                    System.Console.Error.WriteLine("Scan completed with errors (see report).");
                } else {
                    System.Console.Error.WriteLine("Scan completed successfully.");
                }

                try {
                    if (quick) {
                        var report = new QuickReport();
                        report.Generate(BDROM, selectedPlaylists, ScanResult, savePath);
                    } else {
                        var report = new FormReport();
                        report.Generate(BDROM, selectedPlaylists, ScanResult, savePath);
                    }
                }
                catch (Exception ex) {
                    System.Console.WriteLine(string.Format("{0}", (ex.Message)));
                }
            }
        }

        public QuickReportData GenerateReport() {
            if (ScanResult.ScanException != null) {
                throw new Exception(ScanResult.ScanException.Message);
            }

            if (ScanResult.FileExceptions.Count > 0) {
                System.Console.Error.WriteLine("Scan completed with errors (see report).");
            } else {
                System.Console.Error.WriteLine("Scan completed successfully.");
            }

            var report = new QuickReport();
            return report.Generate(BDROM, selectedPlaylists, ScanResult);
        }

        /* XXX: returns -1 on 'q' input */
        private static int getIntIndex(int min, int max) {
            String response;
            int resp = -1;
            do {
                while (Console.KeyAvailable)
                    Console.ReadKey();

                Console.Write("Select (q when finished): ");
                response = Console.ReadLine();
                if (response == "q")
                    return -1;

                try {
                    resp = int.Parse(response);
                }
                catch (Exception) {
                    Console.WriteLine("Invalid Input!");
                }

                if (resp > max || resp < min) {
                    System.Console.WriteLine("Invalid Selection!");
                }
            } while (resp > max || resp < min);

            System.Console.WriteLine();

            return resp;
        }

        public void LoadPlaylists(List<String> inputPlaylists) {
            selectedPlaylists = new List<TSPlaylistFile>();
            foreach (String playlistName in inputPlaylists) {
                String Name = playlistName.ToUpper();
                if (BDROM.PlaylistFiles.ContainsKey(Name)) {
                    if (!selectedPlaylists.Contains(BDROM.PlaylistFiles[Name])) {
                        selectedPlaylists.Add(BDROM.PlaylistFiles[Name]);
                    }
                }
            }

            // throw error if no playlist is found
            if (selectedPlaylists.Count == 0) {
                throw new Exception("No matching playlists found on BD");
            }
        }


        public ListPlaylistInfo LoadPlaylists(bool wholeDisc = false) {
            selectedPlaylists = new List<TSPlaylistFile>();

            if (BDROM == null) {
                return new ListPlaylistInfo();
            }

            bool hasHiddenTracks = false;

            //Dictionary<string, int> playlistGroup = new Dictionary<string, int>();
            List<List<TSPlaylistFile>> groups = new List<List<TSPlaylistFile>>();

            TSPlaylistFile[] sortedPlaylistFiles = new TSPlaylistFile[BDROM.PlaylistFiles.Count];
            BDROM.PlaylistFiles.Values.CopyTo(sortedPlaylistFiles, 0);
            Array.Sort(sortedPlaylistFiles, ComparePlaylistFiles);

            foreach (TSPlaylistFile playlist1 in sortedPlaylistFiles) {
                if (!playlist1.IsValid) continue;

                int matchingGroupIndex = 0;
                for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++) {
                    List<TSPlaylistFile> group = groups[groupIndex];
                    foreach (TSPlaylistFile playlist2 in group) {
                        if (!playlist2.IsValid) continue;

                        foreach (TSStreamClip clip1 in playlist1.StreamClips) {
                            foreach (TSStreamClip clip2 in playlist2.StreamClips) {
                                if (clip1.Name == clip2.Name) {
                                    matchingGroupIndex = groupIndex + 1;
                                    break;
                                }
                            }

                            if (matchingGroupIndex > 0) break;
                        }

                        if (matchingGroupIndex > 0) break;
                    }

                    if (matchingGroupIndex > 0) break;
                }

                if (matchingGroupIndex > 0) {
                    groups[matchingGroupIndex - 1].Add(playlist1);
                } else {
                    groups.Add(new List<TSPlaylistFile> {playlist1});
                    //matchingGroupIndex = groups.Count;
                }

                //playlistGroup[playlist1.Name] = matchingGroupIndex;
            }

            int playlistIdx = 1;
            Dictionary<int, TSPlaylistFile> playlistDict = new Dictionary<int, TSPlaylistFile>();

            var playlistInfos = new ListPlaylistInfo();
            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++) {
                List<TSPlaylistFile> group = groups[groupIndex];
                group.Sort(ComparePlaylistFiles);

                foreach (TSPlaylistFile playlist in group)
                    //in BDROM.PlaylistFiles.Values)
                {
                    if (!playlist.IsValid) continue;

                    playlistDict[playlistIdx] = playlist;
                    if (wholeDisc)
                        selectedPlaylists.Add(playlist);

                    if (playlist.HasHiddenTracks) {
                        hasHiddenTracks = true;
                    }

                    String groupString = (groupIndex + 1).ToString();

                    TimeSpan playlistLengthSpan = new TimeSpan((long) (playlist.TotalLength * 10000000));
                    

                    String fileSize;
                    if (BDInfoSettings.EnableSSIF &&
                        playlist.InterleavedFileSize > 0) {
                        fileSize = playlist.InterleavedFileSize.ToString("N0");
                    } else if (playlist.FileSize > 0) {
                        fileSize = playlist.FileSize.ToString("N0");
                    } else {
                        fileSize = "-";
                    }

                    String fileSize2;
                    if (playlist.TotalAngleSize > 0) {
                        fileSize2 = (playlist.TotalAngleSize).ToString("N0");
                    } else {
                        fileSize2 = "-";
                    }

                    // System.Console.WriteLine(String.Format("{0,-4:G}{1,-7}{2,-15}{3,-10}{4,-16}{5,-16}", playlistIdx.ToString(), groupString, playlist.Name, length, fileSize, fileSize2));
                    var info = new PlaylistInfo() {
                        Index = playlistIdx,
                        Group = groupIndex,
                        Name = playlist.Name,
                        Length = playlistLengthSpan,
                        Size = BDInfoSettings.EnableSSIF && playlist.InterleavedFileSize > 0 ? playlist.InterleavedFileSize : playlist.FileSize > 0 ? playlist.FileSize : 0,
                    };
                    playlistInfos.Add(info);
                    playlistIdx++;
                }
            }

            if (hasHiddenTracks) {
                System.Console.WriteLine(
                    "(*) Some playlists on this disc have hidden tracks. These tracks are marked with an asterisk.");
            }

            return playlistInfos;
        }

        #endregion

        #region Scan BDROM

        private BackgroundWorker ScanBDROMWorker = null;

        private class ScanBDROMState {
            public long TotalBytes = 0;
            public long FinishedBytes = 0;
            public DateTime TimeStarted = DateTime.Now;
            public TSStreamFile StreamFile = null;

            public Dictionary<string, List<TSPlaylistFile>> PlaylistMap =
                new Dictionary<string, List<TSPlaylistFile>>();

            public Exception Exception = null;
        }

        public void ScanBDROMWork(
            object sender,
            DoWorkEventArgs e
        ) {
            ScanResult = new ScanBDROMResult {ScanException = new Exception("Scan is still running.")};

            List<TSStreamFile> streamFiles = new List<TSStreamFile>();
            List<string> streamNames;
            System.Console.Error.WriteLine("Preparing to analyze the following:");
            // Adapted from ScanBDROM()
            foreach (TSPlaylistFile playlist in selectedPlaylists) {
                System.Console.Error.Write("{0} --> ", playlist.Name);
                streamNames = new List<string>();
                foreach (TSStreamClip clip in playlist.StreamClips) {
                    if (!streamFiles.Contains(clip.StreamFile)) {
                        streamNames.Add(clip.StreamFile.Name);
                        streamFiles.Add(clip.StreamFile);
                    }
                }

                Console.Error.WriteLine(String.Join(" + ", streamNames));
            }

            System.Threading.Timer timer = null;
            try {
                ScanBDROMState scanState = new ScanBDROMState();
                foreach (TSStreamFile streamFile in streamFiles) {
                    if (BDInfoSettings.EnableSSIF &&
                        streamFile.InterleavedFile != null) {
                        if (streamFile.InterleavedFile.FileInfo != null)
                            scanState.TotalBytes += streamFile.InterleavedFile.FileInfo.Length;
                        else
                            scanState.TotalBytes += streamFile.InterleavedFile.DFileInfo.Length;
                    } else {
                        if (streamFile.FileInfo != null)
                            scanState.TotalBytes += streamFile.FileInfo.Length;
                        else
                            scanState.TotalBytes += streamFile.DFileInfo.Length;
                    }

                    if (!scanState.PlaylistMap.ContainsKey(streamFile.Name)) {
                        scanState.PlaylistMap[streamFile.Name] = new List<TSPlaylistFile>();
                    }

                    foreach (TSPlaylistFile playlist
                        in BDROM.PlaylistFiles.Values) {
                        playlist.ClearBitrates();

                        foreach (TSStreamClip clip in playlist.StreamClips) {
                            if (clip.Name == streamFile.Name) {
                                if (!scanState.PlaylistMap[streamFile.Name].Contains(playlist)) {
                                    scanState.PlaylistMap[streamFile.Name].Add(playlist);
                                }
                            }
                        }
                    }
                }

                timer = new System.Threading.Timer(
                    ScanBDROMProgress, scanState, 1000, 1000);
                System.Console.Error.WriteLine("\n{0,16}{1,-15}{2,-13}{3}", "", "File", "Elapsed", "Remaining");

                foreach (TSStreamFile streamFile in streamFiles) {
                    scanState.StreamFile = streamFile;

                    Thread thread = new Thread(ScanBDROMThread);
                    thread.Start(scanState);
                    while (thread.IsAlive) {
                        Thread.Sleep(250);
                    }

                    if (streamFile.FileInfo != null)
                        scanState.FinishedBytes += streamFile.FileInfo.Length;
                    else
                        scanState.FinishedBytes += streamFile.DFileInfo.Length;
                    if (scanState.Exception != null) {
                        ScanResult.FileExceptions[streamFile.Name] = scanState.Exception;
                    }
                }

                ScanResult.ScanException = null;
            }
            catch (Exception ex) {
                ScanResult.ScanException = ex;
            }
            finally {
                System.Console.Error.WriteLine();
                timer?.Dispose();
            }
        }

        private void ScanBDROMThread(
            object parameter
        ) {
            ScanBDROMState scanState = (ScanBDROMState) parameter;
            try {
                TSStreamFile streamFile = scanState.StreamFile;
                List<TSPlaylistFile> playlists = scanState.PlaylistMap[streamFile.Name];
                streamFile.Scan(playlists, true);
            }
            catch (Exception ex) {
                scanState.Exception = ex;
            }
        }

        private void ScanBDROMEvent(
            object state
        ) {
            try {
                if (ScanBDROMWorker.IsBusy &&
                    !ScanBDROMWorker.CancellationPending) {
                    ScanBDROMWorker.ReportProgress(0, state);
                }
            }
            catch { }
        }

        private void ScanBDROMProgress(
            object state
        ) {
            ScanBDROMState scanState = (ScanBDROMState) state;

            try {
                long finishedBytes = scanState.FinishedBytes;
                if (scanState.StreamFile != null) {
                    finishedBytes += scanState.StreamFile.Size;
                }

                double progress = ((double) finishedBytes / scanState.TotalBytes);
                int progressValue = (int) Math.Round(progress * 100);
                if (progressValue < 0) progressValue = 0;
                if (progressValue > 100) progressValue = 100;
//                progressBarScan.Value = progressValue;

                TimeSpan elapsedTime = DateTime.Now.Subtract(scanState.TimeStarted);
                TimeSpan remainingTime;
                if (progress > 0 && progress < 1) {
                    remainingTime = new TimeSpan(
                        (long) ((double) elapsedTime.Ticks / progress) - elapsedTime.Ticks);
                } else {
                    remainingTime = new TimeSpan(0);
                }

                string elapsedTimeString = string.Format(CultureInfo.InvariantCulture,
                    "{0:D2}:{1:D2}:{2:D2}",
                    elapsedTime.Hours,
                    elapsedTime.Minutes,
                    elapsedTime.Seconds);

                string remainingTimeString = string.Format(CultureInfo.InvariantCulture,
                    "{0:D2}:{1:D2}:{2:D2}",
                    remainingTime.Hours,
                    remainingTime.Minutes,
                    remainingTime.Seconds);

                if (scanState.StreamFile != null) {
                    System.Console.Error.Write("Scanning {0,3:d}% - {1,10} {2,12}  |  {3}\r", progressValue,
                        scanState.StreamFile.DisplayName, elapsedTimeString, remainingTimeString);
                } else {
                    System.Console.Error.Write("Scanning {0,3}% - \t{1,10}  |  {2}...\r", progressValue,
                        elapsedTimeString, remainingTimeString);
                }

//                UpdatePlaylistBitrates();
            }
            catch { }
        }

        #endregion

        #region Report Generation

        private void GenerateReportProgress(
            object sender,
            ProgressChangedEventArgs e
        ) { }

        #endregion

        public static int ComparePlaylistFiles(
            TSPlaylistFile x,
            TSPlaylistFile y
        ) {
            if (x == null && y == null) {
                return 0;
            } else if (x == null && y != null) {
                return 1;
            } else if (x != null && y == null) {
                return -1;
            } else {
                if (x.TotalLength > y.TotalLength) {
                    return -1;
                } else if (y.TotalLength > x.TotalLength) {
                    return 1;
                } else {
                    return x.Name.CompareTo(y.Name);
                }
            }
        }
    }

    public class ScanBDROMResult {
        public Exception ScanException = new Exception("Scan has not been run.");
        public Dictionary<string, Exception> FileExceptions = new Dictionary<string, Exception>();
    }
}