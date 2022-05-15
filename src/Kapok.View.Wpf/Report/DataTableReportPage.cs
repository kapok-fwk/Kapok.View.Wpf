using Kapok.Report;
using Kapok.Report.Model;
using Res = Kapok.View.Wpf.Report.Resources.DataTableReportViewModel;

namespace Kapok.View.Wpf.Report;

public abstract class DataTableReportPage<TReportProcessor, TReportModel> : ReportPage<TReportProcessor, TReportModel>
    where TReportProcessor : DataTableReportProcessor<TReportModel>
    where TReportModel : DataTableReport
{
    protected DataTableReportPage(TReportModel model, TReportProcessor processor, IViewDomain viewDomain)
        : base(model, processor, viewDomain)
    {
        SaveAsExcelFileAction = new UIAction("SaveAsExcelFile", SaveAsExcelFile, CanSaveAsExcelFile);
        SaveAsCsvFileAction = new UIAction("SaveAsCsvFile", SaveAsCsvFile, CanSaveAsCsvFile);
    }

    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public IAction SaveAsExcelFileAction { get; }
    public IAction SaveAsCsvFileAction { get; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore MemberCanBePrivate.Global

    protected virtual void SaveAsExcelFile()
    {
        SaveAsReportExecution(
            Res.SaveAsExcelFile_Title,
            Res.SaveAsExcelFile_SaveFileDialogFilter,
            (baseStream) => Processor.ProcessToExcelStream(baseStream)
        );
        DialogResult = true;
    }

    protected virtual bool CanSaveAsExcelFile()
    {
        return true;
    }

    protected virtual void SaveAsCsvFile()
    {
        SaveAsReportExecution(
            Res.SaveAsCsvFile_Title,
            Res.SaveAsCsvFile_SaveFileDialogFilter,
            (baseStream) => Processor.ProcessToCsvStream(baseStream)
        );
        DialogResult = true;
    }

    protected virtual bool CanSaveAsCsvFile()
    {
        return true;
    }
}