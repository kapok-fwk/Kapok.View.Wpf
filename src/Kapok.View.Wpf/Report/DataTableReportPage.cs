﻿using Kapok.Report;
using Kapok.Report.Model;
using Res = Kapok.View.Wpf.Report.Resources.DataTableReportViewModel;

namespace Kapok.View.Wpf.Report;

public abstract class DataTableReportPage<TReportProcessor, TReportModel> : ReportPage<TReportProcessor, TReportModel>
    where TReportProcessor : DataTableReportProcessor<TReportModel>
    where TReportModel : DataTableReport
{
    protected DataTableReportPage(TReportModel model, TReportProcessor processor, IServiceProvider serviceProvider)
        : base(model, processor, serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(processor);
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            (baseStream) => Processor.ProcessToStream("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", baseStream)
#pragma warning restore CS8602 // Dereference of a possibly null reference.
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            (baseStream) => Processor.ProcessToStream("text/csv", baseStream)
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        );
        DialogResult = true;
    }

    protected virtual bool CanSaveAsCsvFile()
    {
        return true;
    }
}