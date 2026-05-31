using System.Drawing;
using System.Text;
using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class GraphicsCaptureOptions
{
    public readonly Size Size;
    public readonly ResizeMode ResizeMode;
    
    public readonly ScaleMode ScaleMode;
    
    public readonly bool PremultipliedAlpha;

    public readonly int? CropLeft;
    public readonly int? CropTop;
    public readonly int? CropRight;
    public readonly int? CropBottom;

    public readonly bool CaptureCursor;
    public readonly bool CaptureBorder;
    public readonly bool DisplayBorder;
    
    public readonly GfxCaptureOutputFormat OutputFormat;

    public GraphicsCaptureOptions(Size size = default,
        ResizeMode resizeMode = ResizeMode.Crop,
        ScaleMode scaleMode = ScaleMode.Bilinear,
        bool premultipliedAlpha = false,
        int? cropLeft = null,
        int? cropTop = null,
        int? cropRight = null,
        int? cropBottom = null,
        bool captureCursor = true,
        bool captureBorder = false,
        bool displayBorder = false,
        GfxCaptureOutputFormat outputFormat = GfxCaptureOutputFormat.BGRA_8bit)
    {
        Size = size;
        ResizeMode = resizeMode;
        ScaleMode = scaleMode;
        
        PremultipliedAlpha = premultipliedAlpha;
        
        CropLeft = cropLeft;
        CropTop = cropTop;
        CropRight = cropRight;
        CropBottom = cropBottom;
        
        CaptureCursor = captureCursor;
        CaptureBorder = captureBorder;
        DisplayBorder = displayBorder;
        
        OutputFormat = outputFormat;
    }

    internal void BuildOptions(StringBuilder str)
    {
        str.Append($":width={Size.Width}:height={Size.Height}");

        switch (ScaleMode)
        {
            case ScaleMode.Point: str.Append(":scale_mode=point"); break;
            case ScaleMode.Bilinear: str.Append(":scale_mode=bilinear"); break;
            case ScaleMode.Bicubic: str.Append(":scale_mode=bicubic"); break;
        }
            
        switch (ResizeMode)
        {
            case ResizeMode.Crop: str.Append(":resize_mode=crop"); break;
            case ResizeMode.Scale: str.Append(":resize_mode=scale"); break;
            case ResizeMode.ScaleAspect: str.Append(":resize_mode=scale_aspect"); break;
        }
            
        if(PremultipliedAlpha)
            str.Append(":premultiplied=1");
            
        if(CropLeft != null)
            str.Append($":crop_left={CropLeft.Value}");
        if(CropTop != null)
            str.Append($":crop_top={CropTop.Value}");
        if(CropRight != null)
            str.Append($":crop_right={CropRight.Value}");
        if(CropBottom != null)
            str.Append($":crop_bottom={CropBottom.Value}");

        if (!CaptureCursor)
            str.Append(":capture_cursor=0");
        if(CaptureBorder)
            str.Append(":capture_border=1");
        if(DisplayBorder)
            str.Append(":display_border=1");
            
        switch (OutputFormat)
        {
            case GfxCaptureOutputFormat.BGRA_8bit: str.Append(":output_format=8bit"); break;
            case GfxCaptureOutputFormat.X2BGR10_10bit: str.Append(":output_format=10bit"); break;
            case GfxCaptureOutputFormat.RGBAF16_16bit: str.Append(":output_format=16bit"); break;
        }
    }
}

/// <summary>
///     Represents gfxcapture filter
/// </summary>
public class GraphicsCaptureArgument : IVideoFilterArgument
{
    public readonly int? MonitorIndex;
    public readonly ulong? MonitorHandle;
    
    public readonly string? WindowTitle;
    public readonly string? WindowExe;
    public readonly ulong? WindowHandle;

    public readonly GraphicsCaptureOptions Options;
    
    public GraphicsCaptureArgument(GraphicsCaptureOptions options,
        int? monitorIndex = null,
        ulong? monitorHandle = null,
        string? windowTitle = null,
        string? windowExe = null,
        ulong? windowHandle = null)
    {
        if(options == null)
            throw new ArgumentNullException(nameof(options));
        
        MonitorIndex = monitorIndex;
        MonitorHandle = monitorHandle;
        WindowTitle = windowTitle;
        WindowExe = windowExe;
        WindowHandle = windowHandle;

        Options = options;
    }

    public string Key { get; } = "gfxcapture";

    public string Value
    {
        get
        {
            var str = new StringBuilder();

            if (MonitorIndex != null)
                str.Append($"monitor_idx={MonitorIndex.Value}");
            else if(MonitorHandle != null)
                str.Append($"hmonitor={MonitorHandle.Value}");
            else if(WindowTitle != null)
                str.Append($"window_title={WindowTitle}");
            else if(WindowExe != null)
                str.Append($"window_exe={WindowExe}");
            else if (WindowHandle != null)
                str.Append($"window_handle={WindowHandle}");
            else
                throw new ArgumentException("No capture source specified");
            
            Options.BuildOptions(str);

            return str.ToString();
        }
    }
}
