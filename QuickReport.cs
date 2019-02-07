using System;
using System.Collections.Generic;
using UnitsNet;

namespace BDInfo {
    public class StreamInfo {
        public long? ActiveBitRate;
        public int? AngleIndex;
        public string AspectRatio;
        public string AudioMode;
        public bool? BaseView;
        public int? BitDepth;
        public long? BitRate;
        public int? ChannelCount;
        public string ChannelDescription;
        public string ChannelLayout;
        public string CodecAltName;
        public string CodecName;
        public string CodecShortName;
        public StreamInfo CoreStream;
        public string Description;
        public int? DialNorm;
        public string EncodingProfile;
        public object ExtendedData;
        public string FrameRate;
        public int? FrameRateDenominator;
        public int? FrameRateEnumerator;
        public int? Height;
        public bool? IsHidden;
        public bool? IsInterlaced;
        public bool? IsVBR;
        public string LanguageCode;
        public string LanguageName;
        public int? LFE;
        public ushort? PID;
        public ulong? PacketCount;
        public double? PacketSeconds;
        public ulong? PacketSize;
        public ulong? PayloadBytes;
        public int? SampleRate;
        public int? Width;

        public static StreamInfo FromStream(TSStream stream) {
            var r = new StreamInfo() {
                Description = stream.Description,
                PID = stream.PID,
                BitRate = stream.BitRate,
                ActiveBitRate = stream.ActiveBitRate,
                IsVBR = stream.IsVBR,
                LanguageName = stream.LanguageName,
                IsHidden = stream.IsHidden,
                PayloadBytes = stream.PayloadBytes,
                PacketCount = stream.PacketCount,
                PacketSeconds = stream.PacketSeconds,
                AngleIndex = stream.AngleIndex,
                BaseView = stream.BaseView,
                CodecName = stream.CodecName,
                CodecAltName = stream.CodecAltName,
                CodecShortName = stream.CodecShortName
            };
            if (stream.IsVideoStream) {
                r.Width = ((TSVideoStream) stream).Width;
                r.Height = ((TSVideoStream) stream).Height;
                r.IsInterlaced = ((TSVideoStream) stream).IsInterlaced;
                r.FrameRateEnumerator = ((TSVideoStream) stream).FrameRateEnumerator;
                r.FrameRateDenominator = ((TSVideoStream) stream).FrameRateDenominator;
                switch (((TSVideoStream) stream).AspectRatio) {
                    case TSAspectRatio.ASPECT_4_3:
                        r.AspectRatio = "4:3";
                        break;
                    case TSAspectRatio.ASPECT_16_9:
                        r.AspectRatio = "16:9";
                        break;
                }
                r.EncodingProfile = ((TSVideoStream) stream).EncodingProfile;
                r.ExtendedData = ((TSVideoStream) stream).ExtendedData;
            } else if (stream.IsAudioStream) {
                r.SampleRate = ((TSAudioStream) stream).SampleRate;
                r.ChannelCount = ((TSAudioStream) stream).ChannelCount;
                r.BitDepth = ((TSAudioStream) stream).BitDepth;
                r.LFE = ((TSAudioStream) stream).LFE;
                r.DialNorm = ((TSAudioStream) stream).DialNorm;
                switch (((TSAudioStream) stream).AudioMode) {
                    case TSAudioMode.Unknown:
                        r.AudioMode = "Unknown";
                        break;
                    case TSAudioMode.DualMono:
                        r.AudioMode = "DualMono";
                        break;
                    case TSAudioMode.Stereo:
                        r.AudioMode = "Stereo";
                        break;
                    case TSAudioMode.Surround:
                        r.AudioMode = "Surround";
                        break;
                    case TSAudioMode.Extended:
                        r.AudioMode = "Extended";
                        break;
                }
                if (((TSAudioStream) stream).CoreStream != null) {
                    r.CoreStream = StreamInfo.FromStream(((TSAudioStream) stream).CoreStream);
                }
                switch (((TSAudioStream) stream).ChannelLayout) {
                    case TSChannelLayout.Unknown:
                        r.ChannelLayout = "Unknown";
                        break;
                    case TSChannelLayout.CHANNELLAYOUT_MONO:
                        r.ChannelLayout = "Mono";
                        break;
                    case TSChannelLayout.CHANNELLAYOUT_STEREO:
                        r.ChannelLayout = "Stereo";
                        break;
                    case TSChannelLayout.CHANNELLAYOUT_MULTI:
                        r.ChannelLayout = "Multi";
                        break;
                    case TSChannelLayout.CHANNELLAYOUT_COMBO:
                        r.ChannelLayout = "Combo";
                        break;
                }
                r.ChannelDescription = ((TSAudioStream) stream).ChannelDescription;
            }

            return r;
        }
    }
    public class StreamReport {
        public string Type;
        public string CodecName;
        public int AngleIndex;
        public long Bitrate;
        public long ActiveBitrate;
        public string Description;
        public bool IsHidden;
        public string LanguageName;
        public StreamInfo StreamInfo;

        public override string ToString() {
            if (Type == "Video") {
                return string.Format("Video: {0} / {1:0} kbps / {2}\n", CodecName,
                    BitRate.FromBitsPerSecond((double) Bitrate).KilobitsPerSecond, Description);
            } else if (Type == "Audio") {
                return string.Format("Audio: {0} / {1} / {2}\n", LanguageName, CodecName, Description);
            } else if (Type == "Subtitle") {
                return string.Format("Subtitle: {0} / {1:0.00} kbps\n", LanguageName,
                    BitRate.FromBitsPerSecond((double) Bitrate).KilobitsPerSecond);
            }

            return "";
        }
    }

    public class PlaylistReport {
        public string Name;
        public TimeSpan Length;
        public ulong Size;
        public ulong Bitrate;
        public List<StreamReport> VideoStreams;
        public List<StreamReport> AudioStreams;
        public List<StreamReport> GraphicsStreams;

        public override string ToString() {
            // Playlist: 00120.MPLS
            // Size: 1,297,950,720 bytes
            // Length: 0:05:31.000
            // Total Bitrate: 31.37 Mbps
            var summary = "";
            summary += string.Format("Playlist: {0}\n", Name);
            summary += string.Format("Size: {0:0.00} GB\n", Information.FromBytes((double) Size).Gigabytes);
            summary += string.Format("Length: {0}\n", Length);
            summary += string.Format("Total Bitrate: {0:0.00} Mbps\n",
                BitRate.FromBitsPerSecond((double) Bitrate).MegabitsPerSecond);

            foreach (var stream in VideoStreams) {
                summary += stream.ToString();
            }

            foreach (var stream in AudioStreams) {
                summary += stream.ToString();
            }

            foreach (var stream in GraphicsStreams) {
                summary += stream.ToString();
            }

            return summary;
        }
    }

    public class QuickReportData {
        public string DiscTitle;
        public string VolumeLabel;
        public ulong Size;
        public string Protection;
        public List<PlaylistReport> Playlists;

        public override string ToString() {
            // Disc Title: Doctor Who - Complete Series 11 Boxset - Disc 2
            // Disc Label: DOCTOR_WHO_S11_D2
            // Disc Size: 44,811,681,983 bytes
            // Protection: AACS
            var summary = "";
            summary += string.Format("Disc Title: {0}\n", DiscTitle);
            summary += string.Format("Disc Label: {0}\n", VolumeLabel);
            summary += string.Format("Disc Size: {0:0.00} GB\n", Information.FromBytes((double) Size).Gigabytes);
            summary += string.Format("Protection: {0}\n", Protection);

            foreach (var playlist in Playlists) {
                summary += playlist.ToString();
            }

            return summary;
        }
    }

    public class QuickReport {
        public QuickReportData Generate(
            BDROM BDROM,
            List<TSPlaylistFile> playlists,
            ScanBDROMResult scanResult,
            String savePath = null
        ) {
            var summary = new QuickReportData();
            if (!string.IsNullOrEmpty(BDROM.DiscTitle)) {
                summary.DiscTitle = BDROM.DiscTitle;
            }

            summary.VolumeLabel = BDROM.VolumeLabel;
            summary.Size = BDROM.Size;
            summary.Protection = (BDROM.IsBDPlus ? "BD+" : BDROM.IsUHD ? "AACS2" : "AACS");
            summary.Playlists = new List<PlaylistReport>();

            foreach (var playlist in playlists) {
                var playlistSummary = new PlaylistReport();
                playlistSummary.VideoStreams = new List<StreamReport>();
                playlistSummary.AudioStreams = new List<StreamReport>();
                playlistSummary.GraphicsStreams = new List<StreamReport>();
                playlistSummary.Name = playlist.Name;
                playlistSummary.Size = playlist.TotalSize;
                playlistSummary.Length = new TimeSpan((long) (playlist.TotalLength * 10000000));
                playlistSummary.Bitrate = playlist.TotalBitRate;

                foreach (var stream in playlist.SortedStreams) {
                    var streamSummary = new StreamReport();

                    streamSummary.CodecName = stream.CodecName;
                    if (stream.AngleIndex > 0) {
                        streamSummary.AngleIndex = stream.AngleIndex;
                        streamSummary.ActiveBitrate = stream.ActiveBitRate;
                    }

                    streamSummary.Bitrate = stream.BitRate;
                    streamSummary.Description = stream.Description;
                    streamSummary.LanguageName = stream.LanguageName;
                    streamSummary.IsHidden = stream.IsHidden;

                    if (stream.IsVideoStream) {
                        streamSummary.Type = "Video";
                        playlistSummary.VideoStreams.Add(streamSummary);
                    } else if (stream.IsAudioStream) {
                        streamSummary.Type = "Audio";
                        playlistSummary.AudioStreams.Add(streamSummary);
                    } else if (stream.IsGraphicsStream) {
                        streamSummary.Type = "Subtitle";
                        playlistSummary.GraphicsStreams.Add(streamSummary);
                    }

                    streamSummary.StreamInfo = StreamInfo.FromStream(stream);
                }

                summary.Playlists.Add(playlistSummary);
            }

            return summary;
        }
    }
}