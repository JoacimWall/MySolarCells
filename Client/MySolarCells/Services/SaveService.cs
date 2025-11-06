#nullable disable
#if IOS || MACCATALYST
using Foundation;
using QuickLook;
using UIKit;
#endif
#if ANDROID

using Android.Content;
using Android.OS;
using Java.IO;
#endif
namespace MySolarCells.Services;
public interface ISaveAndView
{
    Task<bool> SaveAndView(string filename, string contentType, MemoryStream stream);
}
public class SaveService : ISaveAndView
{
    //Method to save document as a file and view the saved document.

#if IOS
    public async Task<bool> SaveAndView(string filename, string contentType, MemoryStream stream)
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        string filePath = Path.Combine(path, filename);
        try
        {
            FileStream fileStream = File.Open(filePath, FileMode.Create);
            stream.Position = 0;
            stream.CopyTo(fileStream);
            fileStream.Flush();
            fileStream.Close();
            // await Launcher.OpenAsync(new OpenFileRequest("TEst" , new ReadOnlyFile(filePath)));

        }
        catch (Exception)
        {
            return await Task.FromResult(false);
        }


#pragma warning disable CA1422
        if (UIApplication.SharedApplication.KeyWindow != null)
#pragma warning restore CA1422
        {
#pragma warning disable CA1422
            UIViewController currentController = UIApplication.SharedApplication.KeyWindow.RootViewController;
#pragma warning restore CA1422

            while (currentController!.PresentedViewController != null)
                currentController = currentController.PresentedViewController;

            QLPreviewController qlPreview = new();
            QLPreviewItem item = new QLPreviewItemBundle(filename, filePath);
            qlPreview.DataSource = new PreviewControllerDS(item);
            currentController.PresentViewController(qlPreview, true, null);
        }

        return await Task.FromResult(true);
    }
#elif MACCATALYST
public async Task<bool> SaveAndView(string filename, string contentType, MemoryStream stream)
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string filePath = Path.Combine(path, filename);
        stream.Position = 0;
        //Saves the document
        using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.ReadWrite);
        stream.CopyTo(fileStream);
        fileStream.Flush();
        fileStream.Dispose();
#pragma warning disable CA1416
        //Launch the file
#pragma warning disable CA1422
        UIViewController currentController = UIApplication.SharedApplication.KeyWindow.RootViewController;
#pragma warning restore CA1422
#pragma warning restore CA1416
        while (currentController!.PresentedViewController != null)
            currentController = currentController.PresentedViewController;
        UIView currentView = currentController.View;

        QLPreviewController qlPreview = new();
        QLPreviewItem item = new QLPreviewItemBundle(filename, filePath);
        qlPreview.DataSource = new PreviewControllerDS(item);
        currentController.PresentViewController((UIViewController)qlPreview, true, null);

        return await Task.FromResult(true);
 }
#else
    public async Task<bool> SaveAndView(string filename, string contentType, MemoryStream stream)
    {
        string exception = string.Empty;
         string root = null;

      if (Android.OS.Environment.IsExternalStorageEmulated)
      {
        root = Android.App.Application.Context!.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads)!.AbsolutePath;
      }
      else
        root = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

      Java.IO.File myDir = new(root + "/Syncfusion");
      myDir.Mkdir();
      Java.IO.File file = new(myDir, filename);

      if (file.Exists())
      {
        file.Delete();
      }

      try
      {
        FileOutputStream outs = new(file);
        outs.Write(stream.ToArray());

        outs.Flush();
        outs.Close();
      }
      catch (Exception e)
      {
        exception = e.ToString();
      }
      if (file.Exists())
      {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
        {
          var fileUri = AndroidX.Core.Content.FileProvider.GetUriForFile(Android.App.Application.Context, Android.App.Application.Context.PackageName + ".provider", file);
          var intent = new Intent(Intent.ActionView);
          intent.SetData(fileUri);
          intent.AddFlags(ActivityFlags.NewTask);
          intent.AddFlags(ActivityFlags.GrantReadUriPermission);
          Android.App.Application.Context.StartActivity(intent);
        }
        else
        {
          var fileUri = Android.Net.Uri.Parse(file.AbsolutePath);
          var intent = new Intent(Intent.ActionView);
          intent.SetDataAndType(fileUri, contentType);
          intent = Intent.CreateChooser(intent, "Open File");
          intent!.AddFlags(ActivityFlags.NewTask);
          Android.App.Application.Context.StartActivity(intent);
        }
      }
        return await Task.FromResult(true);
    }
#endif

}
#if IOS || MACCATALYST
public class QlPreviewItemFileSystem : QLPreviewItem
{
    readonly string _fileName, _filePath;

    public QlPreviewItemFileSystem(string fileName, string filePath)
    {
        _fileName = fileName;
        _filePath = filePath;
    }

    public override string PreviewItemTitle => _fileName;

    public override NSUrl PreviewItemUrl => NSUrl.FromFilename(_filePath);
}
public class QLPreviewItemBundle : QLPreviewItem
{
    readonly string _fileName, _filePath;
    public QLPreviewItemBundle(string fileName, string filePath)
    {
        _fileName = fileName;
        _filePath = filePath;
    }

    public override string PreviewItemTitle => _fileName;

    public override NSUrl PreviewItemUrl
    {
        get
        {
            var documents = NSBundle.MainBundle.BundlePath;
            var lib = Path.Combine(documents, _filePath);
            var url = NSUrl.FromFilename(lib);
            return url;
        }
    }
}

public class PreviewControllerDS : QLPreviewControllerDataSource
{
    private readonly QLPreviewItem _item;

    public PreviewControllerDS(QLPreviewItem item)
    {
        _item = item;
    }

    public override nint PreviewItemCount(QLPreviewController controller)
    {
        return (nint)1;
    }

    public override IQLPreviewItem GetPreviewItem(QLPreviewController controller, nint index)
    {
        return _item;
    }
}
#endif

