using Kapok.Report;
using Kapok.Report.Model;
using Res = Kapok.View.Wpf.Report.Resources.MultipleDataTableReportViewModel;
using DataTableReportRes = Kapok.View.Wpf.Report.Resources.DataTableReportViewModel;

namespace Kapok.View.Wpf.Report;

[Obsolete]
public sealed class MultipleDataTableReportPage : ReportPage<MultipleDataTableReportProcessor, MultipleDataTableReport>
{
    static MultipleDataTableReportPage()
    {
        WpfViewDomain.RegisterPageWpfWindowConstructor<MultipleDataTableReportPage>(() => new MultipleDataTableReportWindow());
    }

    public MultipleDataTableReportPage(MultipleDataTableReport model, IViewDomain viewDomain, MultipleDataTableReportFormatter? formatter = null)
        : base(model, new MultipleDataTableReportProcessor(), viewDomain)
    {
        if (formatter != null)
            Processor.ReportFormatter = formatter;

        // UI
        SaveAsExcelFileAction = new UIAction("SaveAsExcelFile", SaveAsExcelFile, CanSaveAsExcelFile);
        SaveAsXmlFileAction = new UIAction("SaveAsXmlFile", XmlExport, CanXmlExport);
    }

    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public IAction SaveAsExcelFileAction { get; }
        
    // ReSharper disable once SuspiciousTypeConversion.Global
    public bool IsXmlExportImplemented => Processor is IXmlReportProcessor;
        
    public IAction SaveAsXmlFileAction { get; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore MemberCanBePrivate.Global

    private void SaveAsExcelFile()
    {
        SaveAsReportExecution(
            DataTableReportRes.SaveAsExcelFile_Title,
            DataTableReportRes.SaveAsExcelFile_SaveFileDialogFilter,
            (baseStream) => Processor.ProcessToExcelStream(baseStream)
        );
        DialogResult = true;
    }

    private bool CanSaveAsExcelFile()
    {
        return true;
    }

    private void XmlExport()
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (!(Processor is IXmlReportProcessor))
            throw new NotSupportedException($"The processor {Processor.GetType().FullName} does not implement the interface IXmlReportProcessor. Method {nameof(XmlExport)} requires that.");

        SaveAsReportExecution(
            "XML Export", // TODO translation missing
            Res.SaveAsXmlFile_SaveFileDialogFilter,
            // ReSharper disable once SuspiciousTypeConversion.Global
            baseStream => ((IXmlReportProcessor) Processor).ProcessToXml(baseStream)
        );
        DialogResult = true;
    }

    private bool CanXmlExport()
    {
        return IsXmlExportImplemented;
    }
}