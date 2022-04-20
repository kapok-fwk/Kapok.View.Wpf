using System.Collections.ObjectModel;
using System.IO;
using Kapok.Report;
using Res = Kapok.View.Wpf.Report.Resources.DataTableReportViewModel;

namespace Kapok.View.Wpf.Report;

public abstract class ReportPage<TReportProcessor, TReportModel> : DialogPage
    where TReportProcessor : ReportProcessor<TReportModel>
    where TReportModel : Kapok.Report.Model.Report
{
    protected ReportPage(TReportModel model, TReportProcessor? processor, IViewDomain? viewDomain)
        : base(viewDomain)
    {
        if (processor != null)
        {
            Processor = processor;
            if (processor.ReportModel == null || processor.ReportModel != model)
                processor.ReportModel = model;
        }
        ReportModel = model;

        ReportParameters = new Collection<ReportParameterViewModel>();
        LoadReportParameters();

        // UI
        Title = ReportModel.Caption?.LanguageOrDefault(ViewDomain.Culture) ?? ReportModel.Name;
    }

    private void LoadReportParameters()
    {
        ReportParameters.Clear();

        if (ReportModel.Parameters != null)
        {
            foreach (var parameter in ReportModel.Parameters)
            {
                if ((parameter.DefaultIterativeValues?.Count ?? 0) > 0)
                {
                    ReportParameters.Add(new ReportParameterViewModel(parameter)
                    {
                        Value = parameter.DefaultValue,
                        // TODO/NOTE: iterative values currently cannot be edited in view
                        HasIterativeValues = true
                    });
                }
                else
                {
                    ReportParameters.Add(new ReportParameterViewModel(parameter)
                    {
                        Value = parameter.DefaultValue
                    });
                }
            }
        }
    }

    protected TReportModel ReportModel { get; }

    protected TReportProcessor Processor { get; }

    public Collection<ReportParameterViewModel> ReportParameters { get; }

    [MenuItem]
    public IAction CancelAction { get; set; }

    public event EventHandler ProcessingDone;

    protected void OnProcessingDone(object sender, EventArgs e)
    {
        ProcessingDone?.Invoke(sender, e);
    }

    protected void SaveAsReportExecution(string saveDialogTitle, string saveFileDialogFilter, Action<Stream> processToStreamProcedure)
    {
        string? fileName = ViewDomain.OpenSaveFileDialog(saveDialogTitle, saveFileDialogFilter);
        if (fileName == null)
        {
            return;
        }

        var iterationParameter = ReportParameters.FirstOrDefault(t => t.HasIterativeValues);

        try
        {
            if (iterationParameter != null)
            {
                foreach (string iterationValue in iterationParameter.ReportParameter.DefaultIterativeValues.Select(d => d?.ToString() ?? string.Empty))
                {
                    // prepare processor
                    Processor.ParameterValues = ReportParameters.Where(p => !p.HasIterativeValues).ToDictionary(p => p.ReportParameter.Name, p => p.Value);
                    Processor.ParameterValues.Add(iterationParameter.ReportParameter.Name, iterationValue);

                    var newFileName =
                        Path.Combine(
                            Path.GetDirectoryName(fileName) ?? string.Empty,
                            Path.GetFileNameWithoutExtension(fileName) + " " + iterationValue + Path.GetExtension(fileName)
                        );

                    // execute processor
                    using (var sw = new StreamWriter(newFileName))
                    {
                        processToStreamProcedure(sw.BaseStream);
                    }
                }
            }
            else
            {
                // prepare processor
                Processor.ParameterValues = ReportParameters.ToDictionary(p => p.ReportParameter.Name, p => p.Value);

                // execute processor
                using (var sw = new StreamWriter(fileName))
                {
                    processToStreamProcedure(sw.BaseStream);
                }
            }

            SendNotificationOnExportSuccessful();
        }
        catch (IOException ex)
        {
            SendNotificationOnIOException(ex);
        }

        OnProcessingDone(this, new EventArgs());
        Close();
    }

    protected void SendNotificationOnExportSuccessful()
    {
        ViewDomain.ShowInfoMessage(
            Res.SendNotificationOnExportSuccessful_Message,
            Res.SendNotificationOnExportSuccessful_Caption,
            this);
    }

    // ReSharper disable once InconsistentNaming
    protected void SendNotificationOnIOException(IOException exception)
    {
        ViewDomain.ShowErrorMessage(
            Res.SendNotificationOnIOException_Message,
            Res.SendNotificationOnIOException_Caption,
            this, exception);
    }
}