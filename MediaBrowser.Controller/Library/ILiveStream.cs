#nullable disable

#pragma warning disable CA1711, CS1591

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.LiveTv;

namespace MediaBrowser.Controller.Library
{
    public interface ILiveStream
    {
        int ConsumerCount { get; set; }

        string OriginalStreamId { get; set; }

        string TunerHostId { get; }

        bool EnableStreamSharing { get; }

        MediaSourceInfo MediaSource { get; set; }

        TunerHostInfo TunerHost { get; set; }

        string UniqueId { get; }

        Task Open(CancellationToken openCancellationToken);

        Task Close();

        Stream GetStream();
    }
}
